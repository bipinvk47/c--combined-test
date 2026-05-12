namespace MetricsDemo.Web.Services.Baseline;

public static class NeutralSerialization
{
    public static string JoinCsv(IEnumerable<string> parts)
    {
        return string.Join(',', parts);
    }

    public static string[] SplitCsv(string line)
    {
        return string.IsNullOrEmpty(line) ? Array.Empty<string>() : line.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    }
}

public sealed class BaselineHistogram
{
    private readonly int[] _buckets;

    public BaselineHistogram(int bucketCount)
    {
        var c = NeutralMath.Clamp(bucketCount, 1, 128);
        _buckets = new int[c];
    }

    public void Add(int value)
    {
        var idx = NeutralMath.ModPositive(value, _buckets.Length);
        _buckets[idx]++;
    }

    public int TotalCount()
    {
        var n = 0;
        foreach (var b in _buckets)
            n += b;
        return n;
    }
}

public sealed class BaselineMatrixSum
{
    public int Sum2D(int[,] m)
    {
        var rows = m.GetLength(0);
        var cols = m.GetLength(1);
        var t = 0;
        for (var r = 0; r < rows; r++)
        {
            for (var c = 0; c < cols; c++)
                t += m[r, c];
        }

        return t;
    }
}

public sealed class BaselinePercentiles
{
    public decimal Ratio(decimal part, decimal whole) => whole == 0 ? 0 : NeutralMath.Round2(part / whole);
}

public sealed class BaselineIdGenerator
{
    private int _next;

    public int Next() => System.Threading.Interlocked.Increment(ref _next);
}

public sealed class BaselinePlainFacade
{
    private readonly BaselineHistogram _histogram = new(16);
    private readonly BaselinePercentiles _pct = new();

    public string Demo(int seed)
    {
        _histogram.Add(seed);
        _ = _pct.Ratio(seed % 7, 10);
        return NeutralSerialization.JoinCsv(new[] { "ok", seed.ToStringInvariant(), _histogram.TotalCount().ToStringInvariant() });
    }
}

internal static class IntString
{
    public static string ToStringInvariant(this int v) => v.ToString(System.Globalization.CultureInfo.InvariantCulture);
}
