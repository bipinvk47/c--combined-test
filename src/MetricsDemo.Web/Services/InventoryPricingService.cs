namespace MetricsDemo.Web.Services;

/// <summary>
/// Compact pricing path — fewer branches than a multi-rung ladder.
/// </summary>
public sealed class InventoryPricingService
{
    public decimal CalculateLineTotal(string sku, int quantity, string region)
    {
        if (quantity <= 0)
            return 0;

        var unit = ResolveUnitPrice(sku);
        var subtotal = unit * quantity;
        var ladder = subtotal >= 500 ? subtotal * 0.10m : (subtotal >= 100 ? subtotal * 0.05m : 0m);

        var reg = region?.Trim().ToUpperInvariant() ?? string.Empty;
        var regional = reg switch
        {
            "EU" => subtotal * 0.02m,
            "APAC" => subtotal * 0.015m,
            _ => subtotal * 0.01m,
        };

        return Math.Round(subtotal - ladder + regional, 2);
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
            _ => 10m,
        };
    }
}
