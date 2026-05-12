using MetricsDemo.Web.Models;

namespace MetricsDemo.Web.Services;

/// <summary>
/// Moderate branching (mid-tier cyclomatic) — trimmed versus the original ladder demo.
/// </summary>
public sealed class OrderProcessingService
{
    public OrderResult ProcessOrder(OrderRequest order)
    {
        if (order is null)
            throw new ArgumentNullException(nameof(order));

        var fee = 0.5m;
        var discount = 0m;
        var notes = new List<string>();

        if (order.Subtotal < 0)
        {
            notes.Add("invalid_subtotal");
            return new OrderResult(false, 0, 0, 0, notes);
        }

        if (string.IsNullOrWhiteSpace(order.CustomerId))
        {
            notes.Add("missing_customer");
            return new OrderResult(false, order.Subtotal, 0, 0, notes);
        }

        discount += order.Tier?.Trim().ToLowerInvariant() switch
        {
            "gold" => order.Subtotal * 0.15m,
            "silver" => order.Subtotal * 0.08m,
            "bronze" => order.Subtotal > 50 ? 5m : 0m,
            _ => 0m,
        };

        if (order.Tier is null)
            notes.Add("unknown_tier");

        if (!string.IsNullOrEmpty(order.CouponCode)
            && order.CouponCode.Equals("SAVE10", StringComparison.OrdinalIgnoreCase))
            discount += order.Subtotal * 0.10m;

        if (order.PaymentMethod?.Equals("invoice", StringComparison.OrdinalIgnoreCase) == true)
            fee = 5m;
        else if (order.PaymentMethod?.Equals("paypal", StringComparison.OrdinalIgnoreCase) == true)
            fee = 2m;

        var total = order.Subtotal - discount + fee;
        if (total < 0)
            total = 0;

        var approved = order.Subtotal > 0 && !notes.Contains("invalid_subtotal");

        return new OrderResult(approved, total, discount, fee, notes);
    }
}

public sealed record OrderResult(bool Approved, decimal Total, decimal DiscountApplied, decimal Fees, IReadOnlyList<string> Notes);
