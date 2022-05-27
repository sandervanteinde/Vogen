﻿using System;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Vogen.Generators;

namespace Vogen;

internal static class WriteWorkItems
{
    private static readonly ClassGenerator _classGenerator;
    private static readonly RecordClassGenerator _recordClassGenerator;
    private static readonly RecordStructGenerator _recordStructGenerator;
    private static readonly StructGenerator _structGenerator;

    private static readonly string _generatedPreamble = @"// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a source generator named Vogen (https://github.com/SteveDunn/Vogen)
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------

// Suppress warnings about [Obsolete] member usage in generated code.
#pragma warning disable CS0618".Replace("\r\n", "\n").Replace("\n", Environment.NewLine); // normalize regardless of git checkout policy        

    static WriteWorkItems()
    {
        _classGenerator = new ClassGenerator();
        _recordClassGenerator = new RecordClassGenerator();
        _recordStructGenerator = new RecordStructGenerator();
        _structGenerator = new StructGenerator();
    }

    public static void WriteVo(VoWorkItem item, SourceProductionContext context)
    {
        // get the recorded user class
        TypeDeclarationSyntax voClass = item.TypeToAugment;

        IGenerateSourceCode generator = GetGenerator(item);

        string classAsText = _generatedPreamble + Environment.NewLine + generator.BuildClass(item, voClass);

        SourceText sourceText = SourceText.From(classAsText, Encoding.UTF8);
        
        var unsanitized = $"{item.FullNamespace}_{voClass.Identifier}.g.cs";

        string filename = SanitizeToALegalFilename(unsanitized);

        context.AddSource(filename, sourceText);

        string SanitizeToALegalFilename(string input)
        {
            return input.Replace('@', '_');
        }
    }

    private static IGenerateSourceCode GetGenerator(VoWorkItem item) =>
        item.TypeToAugment switch
        {
            ClassDeclarationSyntax => _classGenerator,
            StructDeclarationSyntax => _structGenerator,
            RecordDeclarationSyntax rds when rds.IsKind(SyntaxKind.RecordDeclaration) => _recordClassGenerator,
            RecordDeclarationSyntax rds when rds.IsKind(SyntaxKind.RecordStructDeclaration) => _recordStructGenerator,
            _ => throw new InvalidOperationException("Don't know how to get the generator!")
        };
}