using MetricsDemo.Web.Models;
using MetricsDemo.Web.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<OrderProcessingService>();
builder.Services.AddSingleton<UserValidationService>();
builder.Services.AddSingleton<InventoryPricingService>();
builder.Services.AddSingleton<ShippingQuoteService>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/health", () => Results.Ok(new { status = "ok", app = "MetricsDemo" }));

app.MapPost("/api/orders/process", (OrderRequest req, OrderProcessingService svc) =>
{
    var result = svc.ProcessOrder(req);
    return Results.Json(result);
});

app.MapPost("/api/users/validate", (UserProfileDto profile, UserValidationService svc) =>
{
    var (ok, errors) = svc.ValidateProfile(profile);
    return Results.Json(new { valid = ok, errors });
});

app.MapGet("/api/inventory/price", (string sku, int qty, string region, InventoryPricingService svc) =>
{
    var price = svc.CalculateLineTotal(sku, qty, region);
    return Results.Json(new { sku, qty, region, total = price });
});

app.MapGet("/api/shipping/quote", (decimal weightKg, string zone, bool express, ShippingQuoteService svc) =>
{
    var quote = svc.EstimateShipping(weightKg, zone, express);
    return Results.Json(new { weightKg, zone, express, quote });
});

app.Run();
