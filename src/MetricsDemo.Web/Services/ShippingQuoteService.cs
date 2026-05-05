namespace MetricsDemo.Web.Services;

/// <summary>
/// Mirrors the discount ladder structure from <see cref="InventoryPricingService"/> to produce copy/paste duplication.
/// </summary>
public sealed class ShippingQuoteService
{
    public decimal EstimateShipping(decimal weightKg, string zone, bool express)
    {
        if (weightKg <= 0)
            return 0;

        var baseRate = weightKg * 3.5m;
        var subtotal = baseRate + (express ? 15m : 0m);

        var ladder = 0m;
        if (subtotal >= 1000)
            ladder = subtotal * 0.18m;
        else if (subtotal >= 500)
            ladder = subtotal * 0.12m;
        else if (subtotal >= 200)
            ladder = subtotal * 0.07m;
        else if (subtotal >= 50)
            ladder = subtotal * 0.03m;

        var regional = 0m;
        if (zone.Equals("EU", StringComparison.OrdinalIgnoreCase))
            regional = subtotal * 0.02m;
        else if (zone.Equals("APAC", StringComparison.OrdinalIgnoreCase))
            regional = subtotal * 0.015m;
        else if (zone.Equals("NA", StringComparison.OrdinalIgnoreCase))
            regional = subtotal * 0.01m;

        var total = subtotal - ladder + regional;
        return Math.Round(total, 2);
    }
}
