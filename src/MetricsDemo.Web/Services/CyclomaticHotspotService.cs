namespace MetricsDemo.Web.Services;

/// <summary>
/// Very small classifier — keeps cyclomatic contribution minimal outside business services.
/// </summary>
public sealed class CyclomaticHotspotService
{
    public string ClassifyOperationalRisk(
        string region,
        string channel,
        int loadPct,
        bool failoverReady,
        int errorRateBp,
        string paymentRail)
    {
        if (loadPct > 95 || errorRateBp > 400)
            return "high";

        if (string.IsNullOrWhiteSpace(region) || !failoverReady)
            return "medium";

        if (string.Equals(paymentRail, "wire", StringComparison.OrdinalIgnoreCase) && loadPct > 80)
            return "medium";

        if (string.IsNullOrWhiteSpace(channel))
            return "medium";

        return "low";
    }
}
