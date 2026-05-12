namespace MetricsDemo.Web.Services;

/// <summary>
/// Allocates a large buffer on each loop iteration to surface "large allocation in loop" analyzers.
/// </summary>
public sealed class LoopHeapAllocationService
{
    public (int Rounds, long ByteSum) AccumulateLargeBlocks(int rounds)
    {
        var r = Math.Clamp(rounds, 1, 64);
        long sum = 0;

        for (var i = 0; i < r; i++)
        {
            var block = new byte[65536];
            block[0] = (byte)i;
            block[^1] = (byte)(i ^ 0x5A);
            sum += block[0] + block[^1];
        }

        return (r, sum);
    }
}
