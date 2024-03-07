using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace XFE各类拓展.NetCore.Analyzer.Generator
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
                var classDeclarations = GetClassDeclarations(root);
                var usingDirectives = root.DescendantNodes().OfType<UsingDirectiveSyntax>().ToArray();
                var fileScopedNamespaceDeclarationSyntax = GetFileScopedNamespaceDeclaration(root);
                foreach (var classDeclaration in classDeclarations)
                {
                    var fieldDeclarationSyntaxes = GetFieldDeclarations(classDeclaration);
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
                        GetProfilePropertyAttributeList(fieldDeclarationSyntax).ForEach(attribute =>
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
                        var getExpressionStatements = new List<StatementSyntax>();
                        if (fieldDeclarationSyntax.AttributeLists.Any(IsProfilePropertyAddGetAttribute))
                        {
                            GetProfilePropertyAddGetAttributeList(fieldDeclarationSyntax).ForEach(attribute =>
                            {
                                if (attribute.ArgumentList is null)
                                {
                                    return;
                                }
                                var argument = attribute.ArgumentList.Arguments.First();
                                if (argument.Expression is LiteralExpressionSyntax literalExpressionSyntax)
                                {
                                    getExpressionStatements.Add(SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression(literalExpressionSyntax.Token.ValueText)));
                                }
                            });
                        }
                        else
                        {
                            getExpressionStatements.Add(SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName(fieldName)));
                        }
                        var setExpressionStatements = new List<StatementSyntax>();
                        if (fieldDeclarationSyntax.AttributeLists.Any(IsProfilePropertyAddSetAttribute))
                        {
                            GetProfilePropertyAddSetAttributeList(fieldDeclarationSyntax).ForEach(attribute =>
                            {
                                if (attribute.ArgumentList is null)
                                {
                                    return;
                                }
                                var argument = attribute.ArgumentList.Arguments.First();
                                if (argument.Expression is LiteralExpressionSyntax literalExpressionSyntax)
                                {
                                    setExpressionStatements.Add(SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression(literalExpressionSyntax.Token.ValueText)));
                                }
                            });
                        }
                        else
                        {
                            setExpressionStatements.Add(SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression($"{fieldName} = value")));
                        }
                        setExpressionStatements.Add(SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression($"global::XFE各类拓展.NetCore.ProfileExtension.XFEProfile.SaveProfile(typeof({className}))")));
                        var property = SyntaxFactory.PropertyDeclaration(propertyType, propertyName)
                            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                            .AddAttributeLists(attributeSyntax)
                            .WithAccessorList(SyntaxFactory.AccessorList(
                                SyntaxFactory.List(new[]
                                {
                                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                        .WithBody(SyntaxFactory.Block(getExpressionStatements)),
                                    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                        .WithBody(SyntaxFactory.Block(setExpressionStatements))
                                })))
                            .WithLeadingTrivia(fieldDeclarationSyntax.GetLeadingTrivia());
                        return property.NormalizeWhitespace();
                    });
                    var profileClassSyntaxTree = GenerateProfileClassSyntaxTree(classDeclaration, usingDirectives, properties, fileScopedNamespaceDeclarationSyntax);
                    context.AddSource($"{className}.g.cs", profileClassSyntaxTree.ToString());
                }
            }
        }

        public static bool IsProfilePropertyAttribute(AttributeListSyntax attributeList) => attributeList.Attributes.Any(attribute => attribute.Name.ToString() == "ProfileProperty");

        public static List<AttributeSyntax> GetProfilePropertyAttributeList(FieldDeclarationSyntax fieldDeclaration)
        {
            return fieldDeclaration.AttributeLists.Where(IsProfilePropertyAttribute).SelectMany(attributeList => attributeList.Attributes).ToList();
        }

        public static bool IsAutoLoadProfileAttribute(AttributeListSyntax attributeList) => attributeList.Attributes.Any(attribute => attribute.Name.ToString() == "AutoLoadProfile");

        public static List<AttributeSyntax> GetAutoLoadProfileAttribute(FieldDeclarationSyntax fieldDeclaration)
        {
            return fieldDeclaration.AttributeLists.Where(IsAutoLoadProfileAttribute).SelectMany(attributeList => attributeList.Attributes).ToList();
        }

        public static bool IsProfilePropertyAddGetAttribute(AttributeListSyntax attributeList) => attributeList.Attributes.Any(attribute => attribute.Name.ToString() == "ProfilePropertyAddGet");

        public static List<AttributeSyntax> GetProfilePropertyAddGetAttributeList(FieldDeclarationSyntax fieldDeclaration)
        {
            return fieldDeclaration.AttributeLists.Where(IsProfilePropertyAddGetAttribute).SelectMany(attributeList => attributeList.Attributes).ToList();
        }

        public static bool IsProfilePropertyAddSetAttribute(AttributeListSyntax attributeList) => attributeList.Attributes.Any(attribute => attribute.Name.ToString() == "ProfilePropertyAddSet");

        public static List<AttributeSyntax> GetProfilePropertyAddSetAttributeList(FieldDeclarationSyntax fieldDeclaration)
        {
            return fieldDeclaration.AttributeLists.Where(IsProfilePropertyAddSetAttribute).SelectMany(attributeList => attributeList.Attributes).ToList();
        }

        public static FileScopedNamespaceDeclarationSyntax GetFileScopedNamespaceDeclaration(SyntaxNode rootNode)
        {
            var namespaceResults = rootNode.DescendantNodes().OfType<FileScopedNamespaceDeclarationSyntax>();
            if (namespaceResults != null && namespaceResults.Count() > 0)
                return namespaceResults.First();
            return null;
        }

        public static IEnumerable<FieldDeclarationSyntax> GetFieldDeclarations(ClassDeclarationSyntax classDeclaration)
        {
            return classDeclaration.DescendantNodes().OfType<FieldDeclarationSyntax>()
                        .Where(fieldDeclarationSyntax => fieldDeclarationSyntax.AttributeLists.Any(IsProfilePropertyAttribute) && fieldDeclarationSyntax.Modifiers.Any(SyntaxKind.StaticKeyword));
        }

        public static IEnumerable<ClassDeclarationSyntax> GetClassDeclarations(SyntaxNode rootNode)
        {
            return rootNode.DescendantNodes().OfType<ClassDeclarationSyntax>()
                    .Where(classDeclaration => classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword));
        }

        private static SyntaxTree GenerateProfileClassSyntaxTree(ClassDeclarationSyntax classDeclaration, UsingDirectiveSyntax[] usingDirectiveSyntaxes, IEnumerable<PropertyDeclarationSyntax> propertyDeclarationSyntaxes, FileScopedNamespaceDeclarationSyntax fileScopedNamespaceDeclarationSyntax)
        {
            var className = classDeclaration.Identifier.ValueText;
            var summaryText = $@"/// <remarks>
/// <code><seealso cref=""{className}""/> 已自动实现以下属性：</code><br/>
/// <code>
";
            summaryText += string.Join("<br/>\n", propertyDeclarationSyntaxes.Select(propertyDeclarationSyntax => $"/// ○ <seealso cref=\"{propertyDeclarationSyntax.Identifier}\"/>")) + "\n/// </code><br/>\n/// <code>来自<seealso cref=\"global::XFE各类拓展.NetCore.ProfileExtension.XFEProfile\"/></code>\n/// </remarks>\n";
            var memberDeclarations = new List<MemberDeclarationSyntax>();
            memberDeclarations.AddRange(propertyDeclarationSyntaxes);
            var staticConstructorSyntax = SyntaxFactory.ConstructorDeclaration(className)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword))
                    .WithBody(SyntaxFactory.Block(
                        SyntaxFactory.ParseStatement($"global::XFE各类拓展.NetCore.ProfileExtension.XFEProfile.LoadProfiles(typeof({className}));")));
            if (classDeclaration.AttributeLists.Any(IsAutoLoadProfileAttribute))
            {
                var autoLoadProfileAttribute = classDeclaration.AttributeLists.First(attributeList => IsAutoLoadProfileAttribute(attributeList)).Attributes.First();
                if (autoLoadProfileAttribute.ArgumentList != null)
                {
                    var argument = autoLoadProfileAttribute.ArgumentList.Arguments.First();
                    if (argument.Expression is LiteralExpressionSyntax literalExpressionSyntax && literalExpressionSyntax.Token.ValueText == "true")
                    {
                        memberDeclarations.Add(staticConstructorSyntax);
                    }
                }
                else
                {
                    memberDeclarations.Add(staticConstructorSyntax);
                }
            }
            else
            {
                memberDeclarations.Add(staticConstructorSyntax);
            }
            var profileClass = SyntaxFactory.ClassDeclaration(className)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword))
                .AddMembers(memberDeclarations.ToArray())
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
