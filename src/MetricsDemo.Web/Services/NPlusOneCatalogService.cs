namespace MetricsDemo.Web.Services;

/// <summary>
/// Demonstrates classic N+1 load: outer enumeration with a per-item lookup/query inside the loop.
/// Static analyzers flag the repeated work per parent entity as N+1 risk.
/// </summary>
public sealed class NPlusOneCatalogService
{
    private readonly IReadOnlyList<OrderWithLines> _orders;

    public NPlusOneCatalogService()
    {
        _orders = BuildSeedData();
    }

    /// <summary>
    /// For each order header we scan all line items again (intentional N+1-shaped pattern).
    /// </summary>
    public IReadOnlyList<decimal> GetOrderTotalsNaive(string customerId)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            return Array.Empty<decimal>();

        var normalized = customerId.Trim();
        var headers = _orders
            .Where(o => string.Equals(o.CustomerId, normalized, StringComparison.OrdinalIgnoreCase))
            .Select(o => o.OrderId)
            .ToList();

        var totals = new List<decimal>(headers.Count);
        foreach (var orderId in headers)
        {
            decimal sum = 0;
            foreach (var row in _orders)
            {
                foreach (var line in row.Lines)
                {
                    if (line.OrderId == orderId)
                        sum += line.Amount;
                }
            }

            totals.Add(sum);
        }

        return totals;
    }

    private static IReadOnlyList<OrderWithLines> BuildSeedData()
    {
        return new List<OrderWithLines>
        {
            new(1, "c1", new List<LineRecord>
            {
                new(1, 10m), new(1, 20m),
            }),
            new(2, "c1", new List<LineRecord>
            {
                new(2, 5m),
            }),
            new(3, "c2", new List<LineRecord>
            {
                new(3, 100m), new(3, 2.5m),
            }),
        };
    }

    private sealed record OrderWithLines(int OrderId, string CustomerId, IReadOnlyList<LineRecord> Lines);

    private sealed record LineRecord(int OrderId, decimal Amount);
}
