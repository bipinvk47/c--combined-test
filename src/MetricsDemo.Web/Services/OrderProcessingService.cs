using MetricsDemo.Web.Models;

namespace MetricsDemo.Web.Services;

/// <summary>
/// Intentionally branch-heavy for cyclomatic complexity tooling (discount ladder, tier rules, coupons).
/// </summary>
public sealed class OrderProcessingService
{
    public OrderResult ProcessOrder(OrderRequest order)
    {
        if (order is null)
            throw new ArgumentNullException(nameof(order));

        var fee = 0m;
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

        switch (order.Tier?.Trim().ToLowerInvariant())
        {
            case "gold":
                discount += order.Subtotal * 0.15m;
                if (order.ItemCount > 5)
                    discount += 5m;
                break;
            case "silver":
                discount += order.Subtotal * 0.08m;
                if (order.ItemCount > 10)
                    discount += 3m;
                break;
            case "bronze":
                if (order.Subtotal > 100)
                    discount += 10m;
                else if (order.Subtotal > 50)
                    discount += 5m;
                break;
            default:
                notes.Add("unknown_tier");
                break;
        }

        if (order.IsWeekend && order.Subtotal > 25)
            discount += 2m;

        if (!string.IsNullOrEmpty(order.CouponCode))
        {
            switch (order.CouponCode.ToUpperInvariant())
            {
                case "SAVE10":
                    discount += order.Subtotal * 0.10m;
                    break;
                case "SAVE20":
                    if (order.Subtotal >= 200)
                        discount += order.Subtotal * 0.20m;
                    else
                        notes.Add("coupon_threshold_not_met");
                    break;
                case "FREESHIP":
                    fee = 0;
                    notes.Add("free_shipping_coupon");
                    break;
                default:
                    notes.Add("unknown_coupon");
                    break;
            }
        }

        if (order.PaymentMethod?.Equals("card", StringComparison.OrdinalIgnoreCase) == true)
            fee += 1.5m;
        else if (order.PaymentMethod?.Equals("paypal", StringComparison.OrdinalIgnoreCase) == true)
            fee += 2.0m;
        else if (order.PaymentMethod?.Equals("invoice", StringComparison.OrdinalIgnoreCase) == true)
            fee += 5m;
        else
            fee += 0.5m;

        for (var i = 0; i < order.ItemCount; i++)
        {
            if (i > 0 && i % 7 == 0)
                fee += 0.25m;
        }

        if (order.Subtotal > 500 && order.ItemCount < 3)
            discount += 15m;
        else if (order.Subtotal > 500)
            discount += 10m;

        var total = order.Subtotal - discount + fee;
        if (total < 0)
            total = 0;

        var approved = order.Subtotal > 0
                       && !notes.Contains("invalid_subtotal")
                       && (order.Tier is not null || order.Subtotal < 1000);

        return new OrderResult(approved, total, discount, fee, notes);
    }
}

public sealed record OrderResult(bool Approved, decimal Total, decimal DiscountApplied, decimal Fees, IReadOnlyList<string> Notes);
