namespace MetricsDemo.Web.Services;

/// <summary>
/// Strong N+1 surface: repeated full scans per parent id (multiple inner queries per loop).
/// </summary>
public sealed class NPlusOneCatalogService
{
    private readonly IReadOnlyList<OrderWithLines> _orders;
    private readonly IReadOnlyList<ShippingRecord> _shipping;

    public NPlusOneCatalogService()
    {
        _orders = BuildSeedData();
        _shipping = BuildShippingRows();
    }

    /// <summary>
    /// For each order id: full table scans for lines and again for shipping (N+1-shaped).
    /// </summary>
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
            decimal linesTotal = 0;
            foreach (var row in _orders)
            {
                foreach (var line in row.Lines)
                {
                    if (line.OrderId == orderId)
                        linesTotal += line.Amount;
                }
            }

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
