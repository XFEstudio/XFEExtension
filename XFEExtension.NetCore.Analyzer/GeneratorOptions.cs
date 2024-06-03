using Microsoft.CodeAnalysis;

namespace XFEExtension.NetCore.Analyzer
{
    public class GeneratorOptions
    {
        public static bool AutoProfile { get; set; } = true;
        public static bool AutoPath { get; set; } = true;
        public static bool AutoImplement { get; set; } = true;
        public static bool TodoList { get; set; } = true;
        public static int TodoListWarningLevel { get; set; } = 2;
        public static void GetOptions(GeneratorExecutionContext context)
        {
            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.AutoProfile", out var autoProfile))
                AutoProfile = autoProfile.ToLower() == "true";
            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.AutoPath", out var autoPath))
                AutoPath = autoPath.ToLower() == "true";
            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.AutoImplement", out var autoImplement))
                AutoImplement = autoImplement.ToLower() == "true";
            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.TodoList", out var todoList))
                TodoList = todoList.ToLower() == "true";
            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.TodoListWarningLevel", out var todoListWarningLevel))
                if (int.TryParse(todoListWarningLevel, out var warningLevel))
                    TodoListWarningLevel = warningLevel;
        }
    }
}
