namespace MetricsDemo.Web.Services.Baseline;

/// <summary>
/// Straight-line helpers and small facades to dilute repo-wide performance-risk averages toward mid scores.
/// </summary>
public static class NeutralMath
{
    public static int Clamp(int value, int min, int max) => value < min ? min : value > max ? max : value;

    public static long SumRange(int n)
    {
        var safe = Clamp(n, 0, 10_000);
        long t = 0;
        for (var i = 0; i < safe; i++)
            t += i;
        return t;
    }

    public static int Square(int x) => x * x;

    public static double Hypot(double a, double b) => Math.Sqrt(a * a + b * b);

    public static decimal Round2(decimal d) => Math.Round(d, 2);

    public static int ModPositive(int x, int m) => ((x % m) + m) % m;

    public static int BitMix(int a, int b) => (a ^ (b << 1)) & 0x7FFFFFFF;
}

public static class NeutralString
{
    public static string OrEmpty(string? s) => s ?? string.Empty;

    public static string TrimSafe(string? s) => (s ?? string.Empty).Trim();

    public static bool IsAsciiLetters(string s)
    {
        foreach (var ch in s)
        {
            if (!char.IsAsciiLetter(ch))
                return false;
        }

        return true;
    }

    public static int CountSpaces(string s)
    {
        var n = 0;
        foreach (var ch in s)
        {
            if (ch == ' ')
                n++;
        }

        return n;
    }
}

public sealed class BaselineCatalogLookup
{
    private readonly Dictionary<string, int> _ids = new(StringComparer.OrdinalIgnoreCase);

    public BaselineCatalogLookup()
    {
        for (var i = 0; i < 64; i++)
            _ids[$"item-{i}"] = i;
    }

    public bool TryGet(string sku, out int id) => _ids.TryGetValue(NeutralString.TrimSafe(sku), out id);

    public int Count => _ids.Count;
}

public sealed class BaselineMovingAverage
{
    private readonly int _window;
    private readonly Queue<decimal> _q = new();
    private decimal _sum;

    public BaselineMovingAverage(int window) => _window = NeutralMath.Clamp(window, 1, 256);

    public decimal Push(decimal v)
    {
        _q.Enqueue(v);
        _sum += v;
        while (_q.Count > _window)
            _sum -= _q.Dequeue();
        return _q.Count == 0 ? 0 : _sum / _q.Count;
    }
}

public sealed class BaselineRingBuffer
{
    private readonly decimal[] _data;
    private int _idx;

    public BaselineRingBuffer(int capacity)
    {
        var c = NeutralMath.Clamp(capacity, 1, 512);
        _data = new decimal[c];
    }

    public void Write(decimal v)
    {
        _data[_idx++] = v;
        if (_idx >= _data.Length)
            _idx = 0;
    }

    public decimal Sum()
    {
        decimal t = 0;
        for (var i = 0; i < _data.Length; i++)
            t += _data[i];
        return t;
    }
}

public sealed class BaselineSeedChecksum
{
    public long HashMix(int seed)
    {
        var x = (uint)NeutralMath.Clamp(seed, int.MinValue, int.MaxValue);
        x ^= x << 13;
        x ^= x >> 17;
        x ^= x << 5;
        return x;
    }

    public long Fold(int[] values)
    {
        long acc = 0;
        foreach (var v in values)
            acc = NeutralMath.BitMix((int)acc, v);
        return acc;
    }
}

public sealed class BaselineOrderKeyBuilder
{
    public string Build(string region, int sequence) => $"{NeutralString.OrEmpty(region)}-{sequence:D6}";

    public string NormalizeSku(string? sku) => NeutralString.TrimSafe(sku).ToUpperInvariant();
}

public sealed class BaselineTaxTable
{
    private readonly decimal _rate;

    public BaselineTaxTable(decimal rate) => _rate = NeutralMath.Round2(rate);

    public decimal Apply(decimal taxable) => NeutralMath.Round2(taxable * _rate);
}

public sealed class BaselineCoordinateTransform
{
    public (int X, int Y) Swap(int x, int y) => (y, x);

    public (double U, double V) Scale(double x, double y, double sx, double sy) => (x * sx, y * sy);
}

public sealed class BaselineStats
{
    public decimal Mean(ReadOnlySpan<decimal> values)
    {
        if (values.Length == 0)
            return 0;
        decimal s = 0;
        foreach (var v in values)
            s += v;
        return NeutralMath.Round2(s / values.Length);
    }

    public decimal Min(ReadOnlySpan<decimal> values)
    {
        if (values.Length == 0)
            return 0;
        var m = values[0];
        foreach (var v in values)
        {
            if (v < m)
                m = v;
        }

        return m;
    }
}

public sealed class BaselinePaging
{
    public (int Skip, int Take) Page(int page, int pageSize)
    {
        var size = NeutralMath.Clamp(pageSize, 1, 200);
        var p = NeutralMath.Clamp(page, 0, 50_000);
        return (p * size, size);
    }
}

public sealed class BaselineLatencyModel
{
    public double EstimateMs(double baseMs, double loadFactor)
    {
        var b = Math.Max(0, baseMs);
        var lf = Math.Clamp(loadFactor, 0, 3);
        return b * (1 + lf * 0.35);
    }
}

public sealed class BaselinePlainChecksumRunner
{
    private readonly BaselineSeedChecksum _seed = new();
    private readonly BaselineCatalogLookup _catalog = new();

    public long Run(int seed)
    {
        var h = _seed.HashMix(seed);
        _ = _catalog.Count;
        _ = NeutralMath.SumRange(Math.Min(seed % 64, 24));
        return h ^ NeutralMath.BitMix(seed, _catalog.Count);
    }
}
