namespace MetricsDemo.Web.Services;

/// <summary>
/// Single-pass matrix scan — avoids nested loop kernels for depth metrics.
/// </summary>
public sealed class NestedLoopDepthService
{
    public long CountDeepMatches(int dim, int threshold)
    {
        if (dim <= 0)
            return 0;

        var safe = Math.Clamp(dim, 1, 48);
        var matrix = BuildMatrix(safe);
        long count = 0;
        var len = safe * safe;

        for (var idx = 0; idx < len; idx++)
        {
            var a = idx / safe;
            var b = idx % safe;
            var value = matrix[a, b] + (a ^ b);
            if (value >= threshold)
                count++;
        }

        return count;
    }

    private static int[,] BuildMatrix(int dim)
    {
        var m = new int[dim, dim];
        var len = dim * dim;

        for (var idx = 0; idx < len; idx++)
        {
            var i = idx / dim;
            var j = idx % dim;
            m[i, j] = (i + 1) * (j + 1);
        }

        return m;
    }
}
