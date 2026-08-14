using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace XFEExtension.NetCore.Analyzer.Generator
{
    /// <summary>
    /// 查找带特性的字段声明，并为常量表达式代码生成流程提供候选字段。
    /// </summary>
    [Generator]
    public class ConstExpressionAutoGenerator : ISourceGenerator
    {
        /// <summary>
        /// 执行源生成流程并遍历语法接收器收集到的候选字段。
        /// </summary>
        /// <param name="context">当前源生成任务的执行上下文。</param>
        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxReceiver is FieldSyntaxReceiver receiver))
                return;

            foreach (var field in receiver.CandidateFields)
            {
                // TODO: 解析字段的初始值，执行方法并获取返回值

                // TODO: 在新的部分类中生成一个const常量，其值为方法的返回值
            }
        }

        /// <summary>
        /// 初始化源生成器，并注册用于收集带特性字段的语法接收器。
        /// </summary>
        /// <param name="context">用于注册语法通知的生成器初始化上下文。</param>
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new FieldSyntaxReceiver());
        }
    }

    class FieldSyntaxReceiver : ISyntaxReceiver
    {
        /// <summary>
        /// 获取扫描期间收集到的带特性字段声明。
        /// </summary>
        public List<FieldDeclarationSyntax> CandidateFields { get; } = new List<FieldDeclarationSyntax>();

        /// <summary>
        /// 检查访问到的语法节点，并收集包含特性列表的字段声明。
        /// </summary>
        /// <param name="syntaxNode">当前访问的语法节点。</param>
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is FieldDeclarationSyntax fieldDeclarationSyntax
                && fieldDeclarationSyntax.AttributeLists.Count > 0)
            {
                CandidateFields.Add(fieldDeclarationSyntax);
            }
        }
    }
}
