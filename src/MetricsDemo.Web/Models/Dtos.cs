namespace MetricsDemo.Web.Models;

public sealed record OrderRequest(
    string CustomerId,
    string Tier,
    decimal Subtotal,
    int ItemCount,
    bool IsWeekend,
    string PaymentMethod,
    string? CouponCode);

public sealed record UserProfileDto(
    string Email,
    string DisplayName,
    int Age,
    string CountryCode,
    bool AcceptsMarketing,
    string? Phone);

public sealed record BalanceDeltaDto(decimal Delta);
