namespace RumorsSearcher.Model;

/// <summary>
/// 表示一条由时间线说明和正文内容组成的辟谣信息。
/// </summary>
/// <param name="TimeLine">该信息对应的时间线说明。</param>
/// <param name="Content">辟谣信息的正文内容。</param>
public record class RumorEntry(string TimeLine, string Content) { }
