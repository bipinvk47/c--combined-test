namespace MetricsDemo.Web.Services;

/// <summary>
/// Reuses a single buffer across rounds — avoids heap allocation inside the loop body.
/// </summary>
public sealed class LoopHeapAllocationService
{
    public const int FixedBlockSize = 1536;

    public (int Rounds, long ByteSum) Run(int rounds)
    {
        var r = Math.Clamp(rounds, 1, 64);
        var block = new byte[FixedBlockSize];
        long sum = 0;

        for (var i = 0; i < r; i++)
        {
            block[0] = (byte)i;
            block[^1] = (byte)(i ^ 0x5A);
            sum += block[0] + block[^1];
        }

        return (r, sum);
    }
}
