namespace MetricsDemo.Web.Services;

/// <summary>
/// Separate triple-nested kernel so nested-loop tooling can report a distinct tier from the O(n²) service.
/// </summary>
public sealed class ModerateNestedKernelService
{
    /// <summary>
    /// O(n³) accumulation; n is clamped for API safety.
    /// </summary>
    public long TripleLoopChecksum(int n)
    {
        var safe = Math.Clamp(n, 1, 48);
        long acc = 0;

        for (var i = 0; i < safe; i++)
        {
            for (var j = 0; j < safe; j++)
            {
                for (var k = 0; k < safe; k++)
                    acc += (i ^ j ^ k) & 0xFF;
            }
        }

        return acc;
    }
}
