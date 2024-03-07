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
