using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XFEExtension.NetCore.Analyzer.Generator
{
    [Generator]
    public class ConstExpressionAutoGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxReceiver is FieldSyntaxReceiver receiver))
                return;

            foreach (var field in receiver.CandidateFields)
            {
                // TODO: 解析字段的初始值，执行方法并获取返回值
                // 注意：这是一个非常困难的任务，可能需要运行时环境

                // TODO: 在新的部分类中生成一个const常量，其值为方法的返回值
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new FieldSyntaxReceiver());
        }
    }

    class FieldSyntaxReceiver : ISyntaxReceiver
    {
        public List<FieldDeclarationSyntax> CandidateFields { get; } = new List<FieldDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            // TODO: 查找带有特定属性的字段
            if (syntaxNode is FieldDeclarationSyntax fieldDeclarationSyntax
                && fieldDeclarationSyntax.AttributeLists.Count > 0)
            {
                CandidateFields.Add(fieldDeclarationSyntax);
            }
        }
    }
}
