namespace MetricsDemo.Web.Services;

/// <summary>
/// Report token helpers — shared implementation via <see cref="LegacyTokenParser"/>.
/// </summary>
public static class LegacyReportParser
{
    public static IReadOnlyList<string> ExtractTokens(string raw) => LegacyTokenParser.ExtractTokens(raw);

    public static bool IsNumericToken(string token) => LegacyTokenParser.IsNumericToken(token);
}
