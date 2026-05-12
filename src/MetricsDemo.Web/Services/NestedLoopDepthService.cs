namespace MetricsDemo.Web.Services;

/// <summary>
/// Depth-2 kernel only (no deeper nesting). Intended mid-tier nested-loop metrics vs neutral baseline.
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

        for (var a = 0; a < safe; a++)
        {
            for (var b = 0; b < safe; b++)
            {
                var value = matrix[a, b] + (a ^ b);
                if (value >= threshold)
                    count++;
            }
        }

        return count;
    }

    private static int[,] BuildMatrix(int dim)
    {
        var m = new int[dim, dim];
        for (var i = 0; i < dim; i++)
        {
            for (var j = 0; j < dim; j++)
                m[i, j] = (i + 1) * (j + 1);
        }

        return m;
    }
}
