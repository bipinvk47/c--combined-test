namespace MetricsDemo.Web.Services;

/// <summary>
/// Shipping quote with a smaller decision surface than a deep tier ladder.
/// </summary>
public sealed class ShippingQuoteService
{
    public decimal EstimateShipping(decimal weightKg, string zone, bool express)
    {
        if (weightKg <= 0)
            return 0;

        var baseRate = weightKg * 3.5m + (express ? 12m : 0m);
        var discount = baseRate >= 300 ? baseRate * 0.08m : (baseRate >= 120 ? baseRate * 0.04m : 0m);

        var z = zone?.Trim().ToUpperInvariant() ?? string.Empty;
        var lift = z switch
        {
            "EU" => baseRate * 0.02m,
            "APAC" => baseRate * 0.015m,
            _ => baseRate * 0.01m,
        };

        return Math.Round(baseRate - discount + lift, 2);
    }
}
