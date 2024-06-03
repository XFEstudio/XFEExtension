using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace XFEExtension.NetCore.Analyzer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TodoCommentAnalyzerDiagnostics : DiagnosticAnalyzer
    {
        public const string TodoCommentId = "TODO";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                var descriptor = new DiagnosticDescriptor(TodoCommentId,
                                                         "TODO待办事项",
                                                         "待办任务：{0}",
                                                         "XFEExtension.NetCore.Analyzer.Diagnostics",
                                                         (DiagnosticSeverity)Enum.Parse(typeof(DiagnosticSeverity),GeneratorOptions.TodoListWarningLevel.ToString()),
                                                         true,
                                                         "TODO的待办事项.",
                                                         "https://www.xfegzs.com/codespace/diagnostics/TODO.html");
                return ImmutableArray.Create(descriptor);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
        }

        private void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
        {
            if (!GeneratorOptions.EnableTodoList)
                return;
            var root = context.Tree.GetRoot(context.CancellationToken);
            var todoComments = root.DescendantTrivia().Where(trivia => trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) && trivia.ToString().Contains("TODO:"));
            foreach (var todoComment in todoComments)
            {
                var descriptor = new DiagnosticDescriptor(TodoCommentId,
                                                         "TODO待办事项",
                                                         "待办任务：{0}",
                                                         "XFEExtension.NetCore.Analyzer.Diagnostics",
                                                         (DiagnosticSeverity)Enum.Parse(typeof(DiagnosticSeverity),GeneratorOptions.TodoListWarningLevel.ToString()),
                                                         true,
                                                         "TODO的待办事项.",
                                                         "https://www.xfegzs.com/codespace/diagnostics/TODO.html");
                var diagnostic = Diagnostic.Create(descriptor, todoComment.GetLocation(), todoComment.ToString().Replace("//", "").Replace("TODO:", ""));
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
