﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace XFE各类拓展.NetCore.Analyzer
{
    [Generator]
    public class ImplementAutoGenerator : ISourceGenerator
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
                    .Where(classDeclaration => classDeclaration.AttributeLists.Any(IsCreateImplAttribute));
                var usingDirectives = root.DescendantNodes().OfType<UsingDirectiveSyntax>().ToArray();
                foreach (var classDeclaration in classDeclarations)
                {
                    var className = classDeclaration.Identifier.ValueText;
                    // 向类中添加一个空的Test方法
                    var method = SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), "Test")
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .WithBody(SyntaxFactory.Block())
                        .NormalizeWhitespace();
                    // 将方法添加到原来的类中
                    var newClass = classDeclaration.AddMembers(method);
                    var implementationSyntaxTree = GenerateImplementationSyntaxTree(classDeclaration, usingDirectives);
                    context.AddSource($"{className}Impl.g.cs", implementationSyntaxTree.ToString());
                }
            }
        }

        private static bool IsCreateImplAttribute(AttributeListSyntax attributeList) => attributeList.Attributes.Any(attribute => attribute.Name.ToString() == "CreateImpl");

        private static SyntaxTree GenerateImplementationSyntaxTree(ClassDeclarationSyntax classDeclaration, UsingDirectiveSyntax[] usingDirectiveSyntaxes)
        {
            var className = classDeclaration.Identifier.ValueText;
            ClassDeclarationSyntax implementationClass;
            if (classDeclaration.ParameterList is null)
            {
                // 添加summary注释
                implementationClass = SyntaxFactory.ClassDeclaration($"{className}Impl")
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.InternalKeyword))
                    .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(className)))
                    .AddMembers(classDeclaration.Members.OfType<ConstructorDeclarationSyntax>().Select(constructor =>
                    {
                        return constructor.WithIdentifier(SyntaxFactory.Identifier($"{className}Impl"))
                                          .WithInitializer(SyntaxFactory.ConstructorInitializer(SyntaxKind.BaseConstructorInitializer, SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(constructor.ParameterList.Parameters.Select(parameter =>
                                SyntaxFactory.Argument(SyntaxFactory.IdentifierName(parameter.Identifier)))))));
                    }).ToArray())
                    .WithLeadingTrivia(SyntaxFactory.ParseLeadingTrivia($@"/// <summary>
/// <seealso cref=""{className}Impl""/> 是根据 <seealso cref=""{className}""/> 自动生成的实现类
/// </summary>
"))
                    .NormalizeWhitespace();
            }
            else
            {
                implementationClass = SyntaxFactory.ClassDeclaration($"{className}Impl")
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.InternalKeyword))
                    .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(className)))
                    .AddMembers(SyntaxFactory.ConstructorDeclaration($"{className}Impl")
                                             .AddModifiers(SyntaxFactory.Token(SyntaxKind.InternalKeyword))
                                             .WithBody(SyntaxFactory.Block())
                                             .WithParameterList(classDeclaration.ParameterList)
                                             .WithInitializer(SyntaxFactory.ConstructorInitializer(SyntaxKind.BaseConstructorInitializer, SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(classDeclaration.ParameterList.Parameters.Select(parameter => SyntaxFactory.Argument(SyntaxFactory.IdentifierName(parameter.Identifier))))))))
                    .WithLeadingTrivia(SyntaxFactory.ParseLeadingTrivia($@"/// <summary>
/// <seealso cref=""{className}Impl""/> 是根据 <seealso cref=""{className}""/> 自动生成的实现类
/// </summary>
"))
                    .NormalizeWhitespace();
            }
            var namespaceDeclaration = classDeclaration.FirstAncestorOrSelf<NamespaceDeclarationSyntax>();
            MemberDeclarationSyntax memberDeclaration;
            if (namespaceDeclaration is null)
                memberDeclaration = implementationClass;
            else
                memberDeclaration = SyntaxFactory.NamespaceDeclaration(namespaceDeclaration.Name)
                    .AddMembers(implementationClass);
            var implementationCompilationUnit = SyntaxFactory.CompilationUnit()
                .AddUsings(usingDirectiveSyntaxes)
                .AddMembers(memberDeclaration)
                .NormalizeWhitespace();
            return SyntaxFactory.SyntaxTree(implementationCompilationUnit);
        }
    }
}
