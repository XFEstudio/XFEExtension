using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using XFEExtension.NetCore.Analyzer.Diagnostics;
using XFEExtension.NetCore.Analyzer.Generator;

namespace XFEExtension.NetCore.Analyzer.CodeFix
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ProfileExtensionCodeFixProvider))]
    public class ProfileExtensionCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(ProfileExtensionDiagnostics.AddGetNoResultErrorId, ProfileExtensionDiagnostics.AddSetNoSetResultWarningId);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                if (diagnostic.Id == ProfileExtensionDiagnostics.AddGetNoResultErrorId)
                {
                    context.RegisterCodeFix(CodeAction.Create(title: "添加返回值方法",
                                                              createChangedDocument: c => AddReturnFuncAsync(context.Document, diagnostic.Location.SourceSpan, c),
                                                              equivalenceKey: "添加返回值"),
                                                              diagnostic: diagnostic);
                }
                else if (diagnostic.Id == ProfileExtensionDiagnostics.AddSetNoSetResultWarningId)
                {
                    context.RegisterCodeFix(CodeAction.Create(title: "添加字段的设置方法",
                                                              createChangedDocument: c => AddSetFuncAsync(context.Document, diagnostic.Location.SourceSpan, c),
                                                              equivalenceKey: "添加字段的设置方法"),
                                                              diagnostic: diagnostic);
                }
            }
            return Task.CompletedTask;
        }
        private async Task<Document> AddReturnFuncAsync(Document document, TextSpan sourceSpan, System.Threading.CancellationToken c)
        {
            var root = await document.GetSyntaxRootAsync(c);
            var fieldDeclaration = root.FindToken(sourceSpan.Start).Parent.AncestorsAndSelf().OfType<FieldDeclarationSyntax>().First();
            var fieldName = fieldDeclaration.Declaration.Variables.First().Identifier.ValueText;
            var newAttribute = SyntaxFactory.Attribute(SyntaxFactory.ParseName("ProfilePropertyAddGet")).AddArgumentListArguments(SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal($"return {fieldName}"))));
            var newRoot = root.ReplaceNode(fieldDeclaration, fieldDeclaration.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(newAttribute))));
            return document.WithSyntaxRoot(newRoot);
        }

        private async Task<Document> AddSetFuncAsync(Document document, TextSpan sourceSpan, System.Threading.CancellationToken c)
        {
            var root = await document.GetSyntaxRootAsync(c);
            var fieldDeclaration = root.FindToken(sourceSpan.Start).Parent.AncestorsAndSelf().OfType<FieldDeclarationSyntax>().First();
            var fieldName = fieldDeclaration.Declaration.Variables.First().Identifier.ValueText;
            var newAttribute = SyntaxFactory.Attribute(SyntaxFactory.ParseName("ProfilePropertyAddSet")).AddArgumentListArguments(SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal($"{fieldName} = value"))));
            var newRoot = root.ReplaceNode(fieldDeclaration, fieldDeclaration.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(newAttribute))));
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
