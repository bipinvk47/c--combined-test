namespace MetricsDemo.Web.Services;

/// <summary>
/// Duplicated "ladder discount" pattern also appears in <see cref="ShippingQuoteService"/> for duplication scanners.
/// </summary>
public sealed class InventoryPricingService
{
    public decimal CalculateLineTotal(string sku, int quantity, string region)
    {
        if (quantity <= 0)
            return 0;

        var unit = ResolveUnitPrice(sku);
        var subtotal = unit * quantity;

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
        if (region.Equals("EU", StringComparison.OrdinalIgnoreCase))
            regional = subtotal * 0.02m;
        else if (region.Equals("APAC", StringComparison.OrdinalIgnoreCase))
            regional = subtotal * 0.015m;
        else if (region.Equals("NA", StringComparison.OrdinalIgnoreCase))
            regional = subtotal * 0.01m;

        var total = subtotal - ladder + regional;
        return Math.Round(total, 2);
    }

    private static decimal ResolveUnitPrice(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return 9.99m;

        return sku.ToUpperInvariant() switch
        {
            "SKU-A" => 12.5m,
            "SKU-B" => 24m,
            "SKU-C" => 7.25m,
            _ => 10m
        };
    }
}
