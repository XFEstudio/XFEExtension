using Microsoft.CodeAnalysis;

namespace XFEExtension.NetCore.Analyzer
{
    public class GeneratorOptions
    {
        public static bool AutoProfile { get; set; } = true;
        public static bool AutoPath { get; set; } = true;
        public static bool AutoImplement { get; set; } = true;
        public static bool EnableTodoList { get; set; } = true;
        public static string TodoListWarningLevel { get; set; } = "Warning";
        public static void GetOptions(GeneratorExecutionContext context)
        {
            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.AutoProfile", out var autoProfile))
                AutoProfile = autoProfile.ToLower() == "true";
            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.AutoPath", out var autoPath))
                AutoPath = autoPath.ToLower() == "true";
            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.AutoImplement", out var autoImplement))
                AutoImplement = autoImplement.ToLower() == "true";
            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.EnableTodoList", out var enableTodoList))
                EnableTodoList = enableTodoList.ToLower() == "true";
            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.TodoListWarningLevel", out var todoListWarningLevel))
                TodoListWarningLevel = todoListWarningLevel;
        }
    }
}
