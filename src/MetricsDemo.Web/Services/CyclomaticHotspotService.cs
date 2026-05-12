namespace MetricsDemo.Web.Services;

/// <summary>
/// Kept intentionally small (few predicates) so cyclomatic scores sit lower than order/N+1 demos.
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
        if (loadPct > 92 && !failoverReady)
            return "high";

        if (errorRateBp > 250)
            return "high";

        if (string.IsNullOrWhiteSpace(region) || string.IsNullOrWhiteSpace(channel))
            return "medium";

        if (paymentRail.Equals("wire", StringComparison.OrdinalIgnoreCase) && loadPct > 75)
            return "medium";

        return "low";
    }
}
