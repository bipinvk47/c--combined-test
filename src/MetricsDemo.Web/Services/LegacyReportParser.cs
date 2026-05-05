namespace MetricsDemo.Web.Services;

/// <summary>
/// Near-duplicate of <see cref="LegacyInvoiceParser"/> for token-wise duplication detection.
/// </summary>
public static class LegacyReportParser
{
    public static IReadOnlyList<string> ExtractTokens(string raw)
    {
        var tokens = new List<string>();
        if (string.IsNullOrWhiteSpace(raw))
            return tokens;

        var buffer = string.Empty;
        foreach (var ch in raw)
        {
            if (char.IsWhiteSpace(ch) || ch == '|' || ch == ';')
            {
                if (buffer.Length > 0)
                {
                    tokens.Add(buffer);
                    buffer = string.Empty;
                }
            }
            else
            {
                buffer += ch;
            }
        }

        if (buffer.Length > 0)
            tokens.Add(buffer);

        return tokens;
    }

    public static bool IsNumericToken(string token)
    {
        if (string.IsNullOrEmpty(token))
            return false;

        foreach (var ch in token)
        {
            if (!char.IsDigit(ch) && ch != '.' && ch != '-')
                return false;
        }

        return true;
    }
}
