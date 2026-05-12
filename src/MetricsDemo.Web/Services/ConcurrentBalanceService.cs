namespace MetricsDemo.Web.Services;

/// <summary>
/// Thread-safe balance updates (lower race-risk score); pair with <see cref="UnsafeRequestCounterService"/>.
/// </summary>
public sealed class ConcurrentBalanceService
{
    private readonly object _gate = new();
    private decimal _runningBalance;
    private int _mutationCount;

    public void RecordDelta(decimal delta)
    {
        lock (_gate)
        {
            _runningBalance += delta;
            _mutationCount++;
        }
    }

    public (decimal Balance, int Mutations) Snapshot()
    {
        lock (_gate)
            return (_runningBalance, _mutationCount);
    }
}
