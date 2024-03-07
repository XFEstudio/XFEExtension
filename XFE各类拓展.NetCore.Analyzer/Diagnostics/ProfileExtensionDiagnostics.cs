using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.RegularExpressions;
using XFE各类拓展.NetCore.Analyzer.Generator;

namespace XFE各类拓展.NetCore.Analyzer.Diagnostics
{
    [Generator]
    public class ProfileExtensionDiagnostics : ISourceGenerator
    {
        public static readonly DiagnosticDescriptor AddGetNoResultError = new DiagnosticDescriptor("XFE0002",
                                                                                                   "Get方法没有返回值",
                                                                                                   "设置了自定义的Get方法但是没有返回值：'{0}'",
                                                                                                   "XFE各类拓展.NetCore.Analyzer.Diagnostics",
                                                                                                   DiagnosticSeverity.Error,
                                                                                                   true,
                                                                                                   "设置了自定义的Get方法但是没有返回值.",
                                                                                                   null);
        public static readonly DiagnosticDescriptor AddSetNoSetResultWarning = new DiagnosticDescriptor("XFW0003",
                                                                                                        "Set方法没有设置值",
                                                                                                        "设置了自定义的Set方法但是没有设置实际的字段对应的值：'{0}'",
                                                                                                        "XFE各类拓展.NetCore.Analyzer.Diagnostics",
                                                                                                        DiagnosticSeverity.Warning,
                                                                                                        true,
                                                                                                        "设置了自定义的Set方法但是没有设置实际的字段对应的值.",
                                                                                                        );
        public void Initialize(GeneratorInitializationContext context)
        {
        }
        public void Execute(GeneratorExecutionContext context)
        {
            foreach (var syntaxTree in context.Compilation.SyntaxTrees)
            {
                var root = syntaxTree.GetRoot();
                foreach (var classDeclaration in ProfilePropertyAutoGenerator.GetClassDeclarations(root))
                {
                    foreach (var fieldDeclaration in ProfilePropertyAutoGenerator.GetFieldDeclarations(classDeclaration))
                    {
                        var getAttributeHasResult = false;
                        var setAttributeSetResult = false;
                        foreach (var attributeSyntax in ProfilePropertyAutoGenerator.GetProfilePropertyAddGetAttributeList(fieldDeclaration))
                        {
                            if (attributeSyntax.ArgumentList is null)
                            {
                                continue;
                            }
                            var argument = attributeSyntax.ArgumentList.Arguments.First();
                            if (argument.Expression is LiteralExpressionSyntax literalExpressionSyntax && literalExpressionSyntax.Token.ValueText.Contains("return"))
                            {
                                getAttributeHasResult = true;
                            }
                        }
                        foreach (var attributeSyntax in ProfilePropertyAutoGenerator.GetProfilePropertyAddSetAttributeList(fieldDeclaration))
                        {
                            if (attributeSyntax.ArgumentList is null)
                            {
                                continue;
                            }
                            var argument = attributeSyntax.ArgumentList.Arguments.First();
                            if (argument.Expression is LiteralExpressionSyntax literalExpressionSyntax && Regex.IsMatch(literalExpressionSyntax.Token.ValueText, $@"{fieldDeclaration.Declaration.Variables.First().Identifier.ValueText}\s*=\s*value"))
                            {
                                setAttributeSetResult = true;
                            }
                        }
                        if (!getAttributeHasResult)
                        {
                            var diagnostic = Diagnostic.Create(AddGetNoResultError, fieldDeclaration.GetLocation(), fieldDeclaration.Declaration.Variables.First().Identifier.ValueText);
                            context.ReportDiagnostic(diagnostic);
                        }
                        if (!setAttributeSetResult)
                        {
                            var diagnostic = Diagnostic.Create(AddSetNoSetResultWarning, fieldDeclaration.GetLocation(), fieldDeclaration.Declaration.Variables.First().Identifier.ValueText);
                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                }
            }
        }
    }
}
