namespace MetricsDemo.Web.Services;

/// <summary>
/// Mutable shared state updated from request handlers without locking or interlocked operations.
/// Intended to trigger thread-safety / data race heuristic rules in static analysis.
/// </summary>
public sealed class ConcurrentBalanceService
{
    private decimal _runningBalance;
    private int _mutationCount;

    public void RecordDelta(decimal delta)
    {
        _runningBalance += delta;
        _mutationCount++;
    }

    public (decimal Balance, int Mutations) Snapshot()
    {
        var b = _runningBalance;
        var n = _mutationCount;
        return (b, n);
    }
}
