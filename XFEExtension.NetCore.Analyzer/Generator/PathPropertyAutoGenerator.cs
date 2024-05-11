using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace XFEExtension.NetCore.Analyzer.Generator
{
    [Generator]
    public class PathPropertyAutoGenerator : ISourceGenerator
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
                var classDeclarations = GetClassDeclarations(root);
                var fileScopedNamespaceDeclarationSyntax = GetFileScopedNamespaceDeclaration(root);
                foreach (var classDeclaration in classDeclarations)
                {
                    var fieldDeclarationSyntaxes = GetFieldDeclarations(classDeclaration);
                    if (fieldDeclarationSyntaxes is null || !fieldDeclarationSyntaxes.Any())
                    {
                        continue;
                    }
                    var className = classDeclaration.Identifier.ValueText;
                    var properties = new List<PropertyDeclarationSyntax>();
                    var enableCheckProperties = new List<PropertyDeclarationSyntax>();
                    foreach (var fieldDeclarationSyntax in fieldDeclarationSyntaxes)
                    {
                        var variableDeclaration = fieldDeclarationSyntax.Declaration.Variables.First();
                        var fieldName = variableDeclaration.Identifier.Text;
                        var propertyName = fieldName[0] == '_' ? fieldName[1].ToString().ToUpper() + fieldName.Substring(2) : fieldName[0].ToString().ToUpper() + fieldName.Substring(1);
                        var enableCheckPropertyName = $"{propertyName}EnableCheck";
                        GetAutoPathAttributeList(fieldDeclarationSyntax).ForEach(attribute =>
                        {
                            if (attribute.ArgumentList is null)
                            {
                                return;
                            }
                            var argument = attribute.ArgumentList.Arguments.First();
                            if (argument.Expression is LiteralExpressionSyntax literalExpressionSyntax)
                            {
                                propertyName = literalExpressionSyntax.Token.ValueText;
                            }
                        });
                        var propertyType = fieldDeclarationSyntax.Declaration.Type;
                        var triviaText = $@"/// <inheritdoc cref=""{fieldName}""/>
/// <remarks>
/// <seealso cref=""{propertyName}""/> 是根据 <seealso cref=""{fieldName}""/> 自动生成的路径属性<br/><br/>
/// </remarks>
";
                        var checkEnableTriviaText = $@"/// <summary>
/// 是否为 <seealso cref=""{fieldName}""/> 启用检测路径<br/><br/>
/// </summary>
";
                        var property = SyntaxFactory.PropertyDeclaration(propertyType, propertyName)
                            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                            .WithAccessorList(SyntaxFactory.AccessorList(
                                SyntaxFactory.List(new[]
                                {
                                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                        .WithBody(SyntaxFactory.Block(
                                            SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression($"global::XFEExtension.NetCore.PathExtension.XFEAutoPath.CheckPathExistAndCreate({fieldName}, Options.{enableCheckPropertyName})")),
                                            SyntaxFactory.ReturnStatement(SyntaxFactory.ParseExpression($"{fieldName}"))))
                                })))
                            .WithLeadingTrivia(SyntaxFactory.ParseLeadingTrivia(triviaText));
                        var enableCheckProperty = SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName("bool"), enableCheckPropertyName)
                            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                            .WithAccessorList(SyntaxFactory.AccessorList(
                                SyntaxFactory.List(new[]
                                {
                                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                                 .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                                })))
                            .WithLeadingTrivia(SyntaxFactory.ParseLeadingTrivia(checkEnableTriviaText))
                            .WithInitializer(SyntaxFactory.EqualsValueClause(SyntaxFactory.ParseExpression("true")))
                            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
                        properties.Add(property.NormalizeWhitespace());
                        enableCheckProperties.Add(enableCheckProperty.NormalizeWhitespace());
                    }
                    var profileClassSyntaxTree = GeneratePathClassSyntaxTree(classDeclaration, properties, enableCheckProperties, fileScopedNamespaceDeclarationSyntax);
                    context.AddSource($"{className}.g.cs", profileClassSyntaxTree.ToString());
                }
            }
        }

        public static bool IsAutoPathAttribute(AttributeListSyntax attributeList) => attributeList.Attributes.Any(attribute => attribute.Name.ToString() == "AutoPath");

        public static List<AttributeSyntax> GetAutoPathAttributeList(FieldDeclarationSyntax fieldDeclaration) => fieldDeclaration.AttributeLists.Where(IsAutoPathAttribute).SelectMany(attributeList => attributeList.Attributes).ToList();

        public static FileScopedNamespaceDeclarationSyntax GetFileScopedNamespaceDeclaration(SyntaxNode rootNode)
        {
            var namespaceResults = rootNode.DescendantNodes().OfType<FileScopedNamespaceDeclarationSyntax>();
            if (namespaceResults != null && namespaceResults.Count() > 0)
                return namespaceResults.First();
            return null;
        }

        public static IEnumerable<FieldDeclarationSyntax> GetFieldDeclarations(ClassDeclarationSyntax classDeclaration) => classDeclaration.DescendantNodes()
                                                                                                                                           .OfType<FieldDeclarationSyntax>()
                                                                                                                                           .Where(fieldDeclarationSyntax => fieldDeclarationSyntax.AttributeLists.Any(IsAutoPathAttribute) && fieldDeclarationSyntax.Modifiers.Any(SyntaxKind.StaticKeyword));

        public static IEnumerable<ClassDeclarationSyntax> GetClassDeclarations(SyntaxNode rootNode) => rootNode.DescendantNodes()
                                                                                                               .OfType<ClassDeclarationSyntax>()
                                                                                                               .Where(classDeclaration => classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword));

        private static SyntaxTree GeneratePathClassSyntaxTree(ClassDeclarationSyntax classDeclaration, List<PropertyDeclarationSyntax> propertyDeclarationSyntaxes, List<PropertyDeclarationSyntax> enableCheckDeclarationSyntaxes, FileScopedNamespaceDeclarationSyntax fileScopedNamespaceDeclarationSyntax)
        {
            var className = classDeclaration.Identifier.ValueText;
            var triviaText = $@"/// <remarks>
/// <code><seealso cref=""{className}""/> 已生成以下路径：</code><br/>
/// <code>
";
            triviaText += string.Join("<br/>\n", propertyDeclarationSyntaxes.Select(propertyDeclarationSyntax => $"/// ○ <seealso cref=\"{propertyDeclarationSyntax.Identifier}\"/>")) + "\n/// </code><br/>\n/// <code>来自<seealso cref=\"global::XFEExtension.NetCore.PathExtension\"/></code>\n/// </remarks>\n";
            var memberDeclarations = new List<MemberDeclarationSyntax>()
            {
                SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName(className), "Options")
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                .AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Attribute(SyntaxFactory.ParseName("global::XFEExtension.NetCore.ProfileExtension.ProfileInstanceAttribute")))))
                .WithAccessorList(SyntaxFactory.AccessorList(SyntaxFactory.List(
                    new[]
                    {
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                     .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                     .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                    })))
                .WithInitializer(SyntaxFactory.EqualsValueClause(SyntaxFactory.ParseExpression($"new {className}()")))
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                .WithLeadingTrivia(SyntaxFactory.ParseLeadingTrivia($@"/// <summary>
/// 配置选项<br/>
/// <seealso cref=""Options""/> 是 <seealso cref=""{className}""/> 类的配置选项
/// </summary>
"))
            };
            memberDeclarations.AddRange(propertyDeclarationSyntaxes);
            memberDeclarations.AddRange(enableCheckDeclarationSyntaxes);
            var pathClass = SyntaxFactory.ClassDeclaration(className)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword))
                .AddMembers(memberDeclarations.ToArray())
                .WithLeadingTrivia(SyntaxFactory.ParseLeadingTrivia(triviaText))
                .NormalizeWhitespace();
            MemberDeclarationSyntax memberDeclaration;
            if (fileScopedNamespaceDeclarationSyntax is null)
            {
                var namespaceDeclaration = classDeclaration.FirstAncestorOrSelf<NamespaceDeclarationSyntax>();
                if (namespaceDeclaration is null)
                    memberDeclaration = pathClass;
                else
                    memberDeclaration = SyntaxFactory.NamespaceDeclaration(namespaceDeclaration.Name)
                        .AddMembers(pathClass);
            }
            else
            {
                memberDeclaration = SyntaxFactory.FileScopedNamespaceDeclaration(fileScopedNamespaceDeclarationSyntax.Name)
                    .AddMembers(pathClass);
            }
            var profileClassCompilationUnit = SyntaxFactory.CompilationUnit()
                .AddMembers(memberDeclaration)
                .NormalizeWhitespace();
            return SyntaxFactory.SyntaxTree(profileClassCompilationUnit);
        }
    }
}
