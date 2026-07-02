namespace MetricsDemo.Web.Services;

/// <summary>
/// Invoice token helpers — shared implementation via <see cref="LegacyTokenParser"/>.
/// </summary>
public static class LegacyInvoiceParser
{
    public static IReadOnlyList<string> ExtractTokens(string raw) => LegacyTokenParser.ExtractTokens(raw);

    public static bool IsNumericToken(string token) => LegacyTokenParser.IsNumericToken(token);
}
