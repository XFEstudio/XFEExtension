using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

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
                                                         GetSeverityFromInt(GeneratorOptions.TodoListWarningLevel),
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
            if (!GeneratorOptions.TodoList)
                return;
            var root = context.Tree.GetRoot(context.CancellationToken);
            var todoComments = root.DescendantTrivia().Where(trivia => trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) && trivia.ToString().Contains("TODO:"));
            foreach (var todoComment in todoComments)
            {
                var match = Regex.Match(todoComment.ToString(), @"//TODO:\s*(\d)?\s*(.*)");
                if (match.Success)
                {
                    var level = match.Groups[1].Value != "" ? int.Parse(match.Groups[1].Value) : GeneratorOptions.TodoListWarningLevel;
                    var task = match.Groups[2].Value;

                    var descriptor = new DiagnosticDescriptor(TodoCommentId,
                                                             "TODO待办事项",
                                                             "待办任务：{0}",
                                                             "XFEExtension.NetCore.Analyzer.Diagnostics",
                                                             GetSeverityFromInt(level),
                                                             true,
                                                             "TODO的待办事项.",
                                                             "https://www.xfegzs.com/codespace/diagnostics/TODO.html");
                    var diagnostic = Diagnostic.Create(descriptor, todoComment.GetLocation(), task);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private static DiagnosticSeverity GetSeverityFromInt(int level)
        {
            switch (level)
            {
                case 0:
                    return DiagnosticSeverity.Hidden;
                case 1:
                    return DiagnosticSeverity.Info;
                case 2:
                    return DiagnosticSeverity.Warning;
                case 3:
                    return DiagnosticSeverity.Error;
                default:
                    return DiagnosticSeverity.Warning;
            }
        }
    }
}
