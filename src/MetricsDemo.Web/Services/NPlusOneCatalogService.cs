namespace MetricsDemo.Web.Services;

/// <summary>
/// Mid-weight N+1: line totals are pre-aggregated (good), shipping fees still scan the full shipping list per order (one mild N+1 pattern).
/// </summary>
public sealed class NPlusOneCatalogService
{
    private readonly IReadOnlyList<OrderWithLines> _orders;
    private readonly IReadOnlyList<ShippingRecord> _shipping;
    private readonly Dictionary<int, decimal> _linesTotalByOrderId;

    public NPlusOneCatalogService()
    {
        _orders = BuildSeedData();
        _shipping = BuildShippingRows();
        _linesTotalByOrderId = BuildLineTotals(_orders);
    }

    public IReadOnlyList<OrderRollupDto> GetOrderTotalsNaive(string customerId)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            return Array.Empty<OrderRollupDto>();

        var normalized = customerId.Trim();
        var headers = _orders
            .Where(o => string.Equals(o.CustomerId, normalized, StringComparison.OrdinalIgnoreCase))
            .Select(o => o.OrderId)
            .ToList();

        var rollups = new List<OrderRollupDto>(headers.Count);
        foreach (var orderId in headers)
        {
            var linesTotal = _linesTotalByOrderId.TryGetValue(orderId, out var lt) ? lt : 0m;

            decimal shipFee = 0;
            foreach (var ship in _shipping)
            {
                if (ship.OrderId == orderId)
                    shipFee += ship.Fee;
            }

            rollups.Add(new OrderRollupDto(orderId, linesTotal, shipFee));
        }

        return rollups;
    }

    private static Dictionary<int, decimal> BuildLineTotals(IReadOnlyList<OrderWithLines> orders)
    {
        var map = new Dictionary<int, decimal>();
        foreach (var row in orders)
        {
            foreach (var line in row.Lines)
                map[line.OrderId] = map.GetValueOrDefault(line.OrderId) + line.Amount;
        }

        return map;
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

    private static IReadOnlyList<ShippingRecord> BuildShippingRows()
    {
        return new List<ShippingRecord>
        {
            new(1, 4.5m),
            new(2, 3m),
            new(3, 12m),
        };
    }

    private sealed record OrderWithLines(int OrderId, string CustomerId, IReadOnlyList<LineRecord> Lines);

    private sealed record LineRecord(int OrderId, decimal Amount);

    private sealed record ShippingRecord(int OrderId, decimal Fee);
}

public sealed record OrderRollupDto(int OrderId, decimal LinesTotal, decimal ShippingFee);
