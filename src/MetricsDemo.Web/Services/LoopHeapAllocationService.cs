namespace MetricsDemo.Web.Services;

/// <summary>
/// Fixed moderate buffer in loop (~1.5 KB) — mid-tier for allocation-in-loop metrics vs huge slabs.
/// </summary>
public sealed class LoopHeapAllocationService
{
    public const int FixedBlockSize = 1536;

    public (int Rounds, long ByteSum) Run(int rounds)
    {
        var r = Math.Clamp(rounds, 1, 64);
        long sum = 0;

        for (var i = 0; i < r; i++)
        {
            var block = new byte[FixedBlockSize];
            block[0] = (byte)i;
            block[^1] = (byte)(i ^ 0x5A);
            sum += block[0] + block[^1];
        }

        return (r, sum);
    }
}
