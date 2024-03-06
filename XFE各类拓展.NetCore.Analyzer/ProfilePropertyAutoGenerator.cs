using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace XFE各类拓展.NetCore.Analyzer
{
    [Generator]
    public class ProfilePropertyAutoGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var syntaxTrees = context.Compilation.SyntaxTrees;
            foreach (var syntaxTree in syntaxTrees)
            {
                var root = syntaxTree.GetRoot();
                var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                    .Where(classDeclaration => classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword));
                var usingDirectives = root.DescendantNodes().OfType<UsingDirectiveSyntax>().ToArray();
                FileScopedNamespaceDeclarationSyntax fileScopedNamespaceDeclarationSyntax = null;
                var namespaceResults = root.DescendantNodes().OfType<FileScopedNamespaceDeclarationSyntax>();
                if (namespaceResults != null && namespaceResults.Count() > 0)
                    fileScopedNamespaceDeclarationSyntax = namespaceResults.First();
                foreach (var classDeclaration in classDeclarations)
                {
                    var fieldDeclarationSyntaxes = classDeclaration.DescendantNodes().OfType<FieldDeclarationSyntax>()
                        .Where(fieldDeclarationSyntax => fieldDeclarationSyntax.AttributeLists.Any(IsProfilePropertyAttribute) && fieldDeclarationSyntax.Modifiers.Any(SyntaxKind.StaticKeyword));
                    if (fieldDeclarationSyntaxes is null || !fieldDeclarationSyntaxes.Any())
                    {
                        continue;
                    }
                    var className = classDeclaration.Identifier.ValueText;
                    var attributeSyntax = SyntaxFactory.AttributeList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Attribute(SyntaxFactory.ParseName("global::XFE各类拓展.NetCore.ProfileExtension.ProfileProperty"))));
                    var properties = fieldDeclarationSyntaxes.Select(fieldDeclarationSyntax =>
                    {
                        var variableDeclaration = fieldDeclarationSyntax.Declaration.Variables.First();
                        var fieldName = variableDeclaration.Identifier.Text;
                        var propertyName = fieldName[0] == '_' ? fieldName[1].ToString().ToUpper() + fieldName.Substring(2) : fieldName[0].ToString().ToUpper() + fieldName.Substring(1);
                        var propertyType = fieldDeclarationSyntax.Declaration.Type;
                        var property = SyntaxFactory.PropertyDeclaration(propertyType, propertyName)
                            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                            .WithAccessorList(SyntaxFactory.AccessorList(
                                SyntaxFactory.List(new[]
                                {
                                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                        .WithBody(SyntaxFactory.Block(SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName(fieldName)))),
                                    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                        .WithBody(
                                        SyntaxFactory.Block(
                                            SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression($"{fieldName} = value")),
                                            SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression($"_ = global::XFE各类拓展.NetCore.ProfileExtension.XFEProfile.SaveProfile(typeof({className}))"))
                                            ))
                                })));
                        return property;
                    });
                    var profileClassSyntaxTree = GenerateProfileClassSyntaxTree(classDeclaration, usingDirectives, properties, fileScopedNamespaceDeclarationSyntax);
                    context.AddSource($"{className}.g.cs", profileClassSyntaxTree.ToString());
                }
            }
        }

        private static bool IsProfilePropertyAttribute(AttributeListSyntax attributeList) => attributeList.Attributes.Any(attribute => attribute.Name.ToString() == "ProfileProperty");

        private static SyntaxTree GenerateProfileClassSyntaxTree(ClassDeclarationSyntax classDeclaration, UsingDirectiveSyntax[] usingDirectiveSyntaxes, IEnumerable<PropertyDeclarationSyntax> propertyDeclarationSyntaxes, FileScopedNamespaceDeclarationSyntax fileScopedNamespaceDeclarationSyntax)
        {
            var className = classDeclaration.Identifier.ValueText;
            var summaryText = $@"/// <remarks>
/// <code><seealso cref=""{className}""/> 已自动实现以下属性：</code><br/>
/// <code>
";
            summaryText += string.Join("<br/>\n", propertyDeclarationSyntaxes.Select(propertyDeclarationSyntax => $"/// ○ <seealso cref=\"{propertyDeclarationSyntax.Identifier}\"/>")) + "\n/// </code><br/>\n/// <code>来自<seealso cref=\"global::XFE各类拓展.NetCore.ProfileExtension.XFEProfile\"/></code>\n/// </remarks>\n";
            var profileClass = SyntaxFactory.ClassDeclaration(className)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword))
                .AddMembers(propertyDeclarationSyntaxes.ToArray())
                .WithLeadingTrivia(SyntaxFactory.ParseLeadingTrivia(summaryText))
                .NormalizeWhitespace();
            MemberDeclarationSyntax memberDeclaration;
            if (fileScopedNamespaceDeclarationSyntax is null)
            {
                var namespaceDeclaration = classDeclaration.FirstAncestorOrSelf<NamespaceDeclarationSyntax>();
                if (namespaceDeclaration is null)
                    memberDeclaration = profileClass;
                else
                    memberDeclaration = SyntaxFactory.NamespaceDeclaration(namespaceDeclaration.Name)
                        .AddMembers(profileClass);
            }
            else
            {
                memberDeclaration = SyntaxFactory.FileScopedNamespaceDeclaration(fileScopedNamespaceDeclarationSyntax.Name)
                    .AddMembers(profileClass);
            }
            var profileClassCompilationUnit = SyntaxFactory.CompilationUnit()
                .AddUsings(usingDirectiveSyntaxes)
                .AddMembers(memberDeclaration)
                .NormalizeWhitespace();
            return SyntaxFactory.SyntaxTree(profileClassCompilationUnit);
        }
    }
}
