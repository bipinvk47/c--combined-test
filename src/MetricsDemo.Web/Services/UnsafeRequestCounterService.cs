namespace MetricsDemo.Web.Services;

/// <summary>
/// Narrow unsynchronized mutation hotspot for concurrency rule packs (not used for monetary state).
/// </summary>
public sealed class UnsafeRequestCounterService
{
    private int _hits;

    public int Bump()
    {
        _hits++;
        return _hits;
    }

    public int Read() => _hits;
}
