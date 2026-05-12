using MetricsDemo.Web.Models;
using MetricsDemo.Web.Services;
using MetricsDemo.Web.Services.Baseline;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<OrderProcessingService>();
builder.Services.AddSingleton<UserValidationService>();
builder.Services.AddSingleton<InventoryPricingService>();
builder.Services.AddSingleton<ShippingQuoteService>();
builder.Services.AddSingleton<NPlusOneCatalogService>();
builder.Services.AddSingleton<NestedLoopDepthService>();
builder.Services.AddSingleton<LoopHeapAllocationService>();
builder.Services.AddSingleton<ConcurrentBalanceService>();
builder.Services.AddSingleton<CyclomaticHotspotService>();
builder.Services.AddSingleton<BaselinePlainChecksumRunner>();
builder.Services.AddSingleton<BaselineCatalogLookup>();
builder.Services.AddSingleton<BaselineMovingAverage>();
builder.Services.AddSingleton<BaselineStats>();
builder.Services.AddSingleton<BaselinePlainFacade>();

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

app.MapGet("/api/metrics/nplus-one", (string customerId, NPlusOneCatalogService svc) =>
    Results.Json(new { customerId, totals = svc.GetOrderTotalsNaive(customerId ?? string.Empty) }));

app.MapGet("/api/metrics/nested-loops", (int? dim, int? threshold, NestedLoopDepthService svc) =>
{
    var d = dim.GetValueOrDefault(8);
    var t = threshold.GetValueOrDefault(12);
    var matches = svc.CountDeepMatches(d, t);
    return Results.Json(new { tier = "depth2", dim = d, threshold = t, matches });
});

app.MapGet("/api/metrics/loop-alloc", (int? rounds, LoopHeapAllocationService svc) =>
{
    var r = rounds.GetValueOrDefault(4);
    var result = svc.Run(r);
    return Results.Json(new
    {
        result.Rounds,
        blockSize = LoopHeapAllocationService.FixedBlockSize,
        checksum = result.ByteSum,
    });
});

app.MapPost("/api/metrics/balance", (BalanceDeltaDto body, ConcurrentBalanceService svc) =>
{
    svc.RecordDelta(body.Delta);
    var snap = svc.Snapshot();
    return Results.Json(new { appliedDelta = body.Delta, balance = snap.Balance, mutations = snap.Mutations });
});

app.MapGet("/api/metrics/risk-class", (
        string region,
        string channel,
        int loadPct,
        bool failoverReady,
        int errorRateBp,
        string paymentRail,
        CyclomaticHotspotService svc) =>
    Results.Json(new
    {
        bucket = svc.ClassifyOperationalRisk(region, channel, loadPct, failoverReady, errorRateBp, paymentRail),
        region,
        channel,
        loadPct,
        failoverReady,
        errorRateBp,
        paymentRail,
    }));

app.MapGet("/api/baseline/checksum", (int? seed, BaselinePlainChecksumRunner runner) =>
    Results.Json(new { seed = seed ?? 1, checksum = runner.Run(seed ?? 1) }));

app.MapGet("/api/baseline/demo", (int? seed, BaselinePlainFacade facade) =>
    Results.Json(new { seed = seed ?? 1, text = facade.Demo(seed ?? 1) }));

app.Run();
