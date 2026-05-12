namespace MetricsDemo.Web.Services;

/// <summary>
/// Intentional deep nested loops for Big-O / nested-loop depth tooling (four levels).
/// </summary>
public sealed class NestedLoopDepthService
{
    /// <summary>
    /// O(n^4) style kernel; dim kept small for demo API safety.
    /// </summary>
    public long CountDeepMatches(int dim, int threshold)
    {
        if (dim <= 0)
            return 0;

        var safe = Math.Clamp(dim, 1, 12);
        var matrix = BuildMatrix(safe);
        long count = 0;

        for (var a = 0; a < safe; a++)
        {
            for (var b = 0; b < safe; b++)
            {
                for (var c = 0; c < safe; c++)
                {
                    for (var d = 0; d < safe; d++)
                    {
                        var value = matrix[a, b] + matrix[c, d] + (a ^ b ^ c ^ d);
                        if (value >= threshold)
                            count++;
                    }
                }
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
