using MetricsDemo.Web.Models;

namespace MetricsDemo.Web.Services;

/// <summary>
/// Flat order path — expression-based tier and fee resolution to keep cyclomatic low.
/// </summary>
public sealed class OrderProcessingService
{
    private static readonly Dictionary<string, decimal> PaymentFees = new(StringComparer.OrdinalIgnoreCase)
    {
        ["invoice"] = 5m,
        ["paypal"] = 2m,
    };

    public OrderResult ProcessOrder(OrderRequest order)
    {
        if (order is null)
            throw new ArgumentNullException(nameof(order));

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

        var discount = ResolveTierDiscount(order);
        if (order.Tier is null)
            notes.Add("unknown_tier");

        if (!string.IsNullOrEmpty(order.CouponCode)
            && order.CouponCode.Equals("SAVE10", StringComparison.OrdinalIgnoreCase))
            discount += order.Subtotal * 0.10m;

        var fee = ResolvePaymentFee(order.PaymentMethod);
        var total = Math.Max(0, order.Subtotal - discount + fee);
        var approved = order.Subtotal > 0 && !notes.Contains("invalid_subtotal");

        return new OrderResult(approved, total, discount, fee, notes);
    }

    private static decimal ResolveTierDiscount(OrderRequest order) =>
        order.Tier?.Trim().ToLowerInvariant() switch
        {
            "gold" => order.Subtotal * 0.15m,
            "silver" => order.Subtotal * 0.08m,
            "bronze" => order.Subtotal > 50 ? 5m : 0m,
            _ => 0m,
        };

    private static decimal ResolvePaymentFee(string? paymentMethod) =>
        paymentMethod is not null && PaymentFees.TryGetValue(paymentMethod, out var fee) ? fee : 0.5m;
}

public sealed record OrderResult(bool Approved, decimal Total, decimal DiscountApplied, decimal Fees, IReadOnlyList<string> Notes);
