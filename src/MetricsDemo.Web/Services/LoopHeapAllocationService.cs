namespace MetricsDemo.Web.Services;

/// <summary>
/// Mixed allocation sizes per profile so large-allocation-in-loop tooling can tier results.
/// </summary>
public sealed class LoopHeapAllocationService
{
    public (int Rounds, long ByteSum, string Profile, int BlockSize) Run(int rounds, string? profile)
    {
        var r = Math.Clamp(rounds, 1, 64);
        var p = (profile ?? "medium").Trim().ToLowerInvariant();
        var blockSize = p switch
        {
            "tiny" => 64,
            "small" => 1024,
            "medium" => 8192,
            "large" => 65536,
            _ => 2048,
        };

        long sum = 0;
        for (var i = 0; i < r; i++)
        {
            var block = new byte[blockSize];
            block[0] = (byte)i;
            block[^1] = (byte)(i ^ 0x5A);
            sum += block[0] + block[^1];
        }

        return (r, sum, p, blockSize);
    }
}
