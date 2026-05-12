namespace MetricsDemo.Web.Services;

/// <summary>
/// Single method with many independent decision points for cyclomatic complexity tooling.
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
        var tier = 0;

        if (string.IsNullOrEmpty(region))
            tier += 1;
        else if (region.Equals("EU", StringComparison.OrdinalIgnoreCase))
            tier += 2;
        else if (region.Equals("NA", StringComparison.OrdinalIgnoreCase))
            tier += 2;
        else if (region.Equals("APAC", StringComparison.OrdinalIgnoreCase))
            tier += 3;
        else if (region.Equals("LATAM", StringComparison.OrdinalIgnoreCase))
            tier += 2;
        else
            tier += 4;

        switch (channel?.Trim().ToLowerInvariant())
        {
            case "web":
                tier += 1;
                break;
            case "mobile":
                tier += 2;
                break;
            case "pos":
                tier += 3;
                break;
            case "ivr":
                tier += 4;
                break;
            default:
                tier += 5;
                break;
        }

        if (loadPct < 0)
            tier += 5;
        else if (loadPct < 30)
            tier += 1;
        else if (loadPct < 60)
            tier += 2;
        else if (loadPct < 85)
            tier += 3;
        else if (loadPct < 95)
            tier += 4;
        else
            tier += 6;

        if (failoverReady)
            tier -= 1;
        else
            tier += 2;

        if (errorRateBp < 0)
            tier += 3;
        else if (errorRateBp < 25)
            tier += 0;
        else if (errorRateBp < 75)
            tier += 1;
        else if (errorRateBp < 150)
            tier += 2;
        else if (errorRateBp < 400)
            tier += 4;
        else
            tier += 6;

        if (string.Equals(paymentRail, "card", StringComparison.OrdinalIgnoreCase))
            tier += 1;
        else if (string.Equals(paymentRail, "ach", StringComparison.OrdinalIgnoreCase))
            tier += 2;
        else if (string.Equals(paymentRail, "sepa", StringComparison.OrdinalIgnoreCase))
            tier += 2;
        else if (string.Equals(paymentRail, "wire", StringComparison.OrdinalIgnoreCase))
            tier += 3;
        else if (string.Equals(paymentRail, "wallet", StringComparison.OrdinalIgnoreCase))
            tier += 2;
        else
            tier += 4;

        if (tier < 8)
            return "low";
        if (tier < 14)
            return "medium";
        if (tier < 22)
            return "high";
        return "critical";
    }
}
