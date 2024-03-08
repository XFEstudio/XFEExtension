using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace XFEExtension.NetCore.Analyzer.Generator
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
                            SyntaxFactory.Attribute(SyntaxFactory.ParseName("global::XFEExtension.NetCore.ProfileExtension.ProfileProperty"))));
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
                        #region Trivia头
                        var triviaText = $@"/// <remarks>
/// <seealso cref=""{propertyName}""/> 是根据 <seealso cref=""{fieldName}""/> 自动生成的属性<br/><br/>
/// <code><seealso langword=""get""/>方法已生成以下代码:";
                        #endregion
                        var getExpressionStatements = new List<StatementSyntax>();
                        var getIndex = 0;
                        var setIndex = 0;
                        if (fieldDeclarationSyntax.AttributeLists.Any(IsProfilePropertyAddGetAttribute))
                        {
                            GetProfilePropertyAddGetAttributeList(fieldDeclarationSyntax).ForEach(attribute =>
                            {
                                if (attribute.ArgumentList is null)
                                {
                                    return;
                                }
                                var argument = attribute.ArgumentList.Arguments.First();
                                var funcText = string.Empty;
                                if (argument.Expression is LiteralExpressionSyntax literalExpressionSyntax)
                                    funcText = literalExpressionSyntax.Token.ValueText;
                                if (argument.Expression is InterpolatedStringExpressionSyntax interpolatedStringExpressionSyntax)
                                    funcText = interpolatedStringExpressionSyntax.Contents.ToString();
                                if (argument.Expression is InvocationExpressionSyntax invocationExpressionSyntax)
                                    funcText = invocationExpressionSyntax.GetText().ToString();
                                getExpressionStatements.Add(SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression(funcText)));
                                if (getIndex == 0)
                                {
                                    #region Get方法首个注释
                                    triviaText += $"\t○ {funcText.Replace("\n", "<br/>").Replace("return", "<seealso langword=\"return\"/>").Replace(fieldName, $"<seealso langword=\"{fieldName}\"/>")};<br/>";
                                    #endregion
                                }
                                else
                                {
                                    #region Get剩余方法注释
                                    triviaText += $"\n///\t\t\t\t○ {funcText.Replace("\n", "<br/>").Replace("return", "<seealso langword=\"return\"/>").Replace(fieldName, $"<seealso langword=\"{fieldName}\"/>")};<br/>";
                                    #endregion
                                }
                                getIndex++;
                            });
                        }
                        else
                        {
                            getExpressionStatements.Add(SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName(fieldName)));
                            #region Get方默认注释
                            triviaText += $@"	○ <seealso langword=""return""/> <seealso langword=""{fieldName}""/>;";
                            #endregion
                            getIndex++;
                        }
                        #region Get方法尾及Set方法头注释
                        triviaText += @"
/// </code>
/// <br/>
/// <code><seealso langword=""set""/>方法已生成以下代码:";
                        #endregion
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
                                var funcText = string.Empty;
                                if (argument.Expression is LiteralExpressionSyntax literalExpressionSyntax)
                                    funcText = literalExpressionSyntax.Token.ValueText;
                                else if (argument.Expression is InterpolatedStringExpressionSyntax interpolatedStringExpressionSyntax)
                                    funcText = interpolatedStringExpressionSyntax.Contents.ToString();
                                else if (argument.Expression is InvocationExpressionSyntax invocationExpressionSyntax)
                                    funcText = invocationExpressionSyntax.GetText().ToString();
                                setExpressionStatements.Add(SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression(funcText)));
                                if (setIndex == 0)
                                {
                                    #region Set方法首个注释
                                    triviaText += $"\t○ {funcText.Replace("\n", "<br/>").Replace(fieldName, $"<seealso langword=\"{fieldName}\"/>").Replace("value", "<seealso langword=\"value\"/>")};<br/>";
                                    #endregion
                                }
                                else
                                {
                                    #region Set剩余方法注释
                                    triviaText += $"\n///\t\t\t\t○ {funcText.Replace("\n", "<br/>").Replace(fieldName, $"<seealso langword=\"{fieldName}\"/>").Replace("value", "<seealso langword=\"value\"/>")};<br/>";
                                    #endregion
                                }
                                setIndex++;
                            });
                        }
                        else
                        {
                            setExpressionStatements.Add(SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression($"{fieldName} = value")));
                            #region Set方法默认注释
                            triviaText += $@"	○ <seealso langword=""{fieldName}""/> = <seealso langword=""value""/>;<br/>";
                            #endregion
                            setIndex++;
                        }
                        setExpressionStatements.Add(SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression($"global::XFEExtension.NetCore.ProfileExtension.XFEProfile.SaveProfile(typeof({className}))")));
                        if (setIndex == 0)
                        {
                            #region Set方法中的保存方法在第一位情况的注释
                            triviaText += $@"	○ <seealso cref=""global::XFEExtension.NetCore.ProfileExtension.XFEProfile.SaveProfile(ProfileInfo)""/>";
                            #endregion
                        }
                        else
                        {
                            #region Set方法中的保存方法在非第一位情况的注释
                            triviaText += $"\n///\t\t\t\t○ <seealso cref=\"global::XFEExtension.NetCore.ProfileExtension.XFEProfile.SaveProfile(ProfileInfo)\"/>;";
                            #endregion
                        }
                        #region Trivia尾
                        triviaText += @"
/// </code>
/// </remarks>
";
                        #endregion
                        var leadingTrivia = fieldDeclarationSyntax.DescendantTrivia().ToList();
                        leadingTrivia.AddRange(SyntaxFactory.ParseLeadingTrivia(triviaText));
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
                            .WithLeadingTrivia(leadingTrivia);
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
            var triviaText = $@"/// <remarks>
/// <code><seealso cref=""{className}""/> 已自动实现以下属性：</code><br/>
/// <code>
";
            triviaText += string.Join("<br/>\n", propertyDeclarationSyntaxes.Select(propertyDeclarationSyntax => $"/// ○ <seealso cref=\"{propertyDeclarationSyntax.Identifier}\"/>")) + "\n/// </code><br/>\n/// <code>来自<seealso cref=\"global::XFEExtension.NetCore.ProfileExtension.XFEProfile\"/></code>\n/// </remarks>\n";
            var memberDeclarations = new List<MemberDeclarationSyntax>();
            memberDeclarations.AddRange(propertyDeclarationSyntaxes);
            var staticConstructorSyntax = SyntaxFactory.ConstructorDeclaration(className)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword))
                    .WithBody(SyntaxFactory.Block(
                        SyntaxFactory.ParseStatement($"global::XFEExtension.NetCore.ProfileExtension.XFEProfile.LoadProfiles(typeof({className}));")));
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
                .WithLeadingTrivia(SyntaxFactory.ParseLeadingTrivia(triviaText))
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
        // 定义Windows API
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EmptyClipboard();

        [DllImport("user32.dll")]
        public static extern IntPtr SetClipboardData(uint uFormat, IntPtr data);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseClipboard();

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GlobalUnlock(IntPtr hMem);
        public static bool SetClipboardText(string text)
        {
            // 打开剪切板
            if (!OpenClipboard(IntPtr.Zero))
                return false;

            // 清空剪切板
            EmptyClipboard();

            // 分配内存
            IntPtr hGlobal = GlobalAlloc(0x2000, (UIntPtr)((text.Length + 1) * 2)); // 0x2000 是GMEM_MOVEABLE

            // 锁定内存
            IntPtr pGlobal = GlobalLock(hGlobal);

            // 将文本复制到内存
            byte[] bytes = Encoding.Unicode.GetBytes(text);
            Marshal.Copy(bytes, 0, pGlobal, bytes.Length);

            // 解锁内存
            GlobalUnlock(hGlobal);

            // 将内存设置到剪切板
            SetClipboardData(13, hGlobal);

            // 关闭剪切板
            CloseClipboard();

            return true;
        }
    }
}
