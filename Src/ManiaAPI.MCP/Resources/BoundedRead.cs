namespace ManiaAPI.MCP.Resources;

public static class BoundedRead
{
    public static async Task<byte[]> AllAsync(Stream stream, int limit, CancellationToken cancellationToken)
    {
        using var buffer = new MemoryStream();
        var block = new byte[81920];

        while (true)
        {
            var count = await stream.ReadAsync(block, cancellationToken);

            if (count == 0)
            {
                return buffer.ToArray();
            }

            if (buffer.Length + count > limit)
            {
                throw new ArgumentException("The resource exceeds MaxResourceBytes.");
            }

            await buffer.WriteAsync(block.AsMemory(0, count), cancellationToken);
        }
    }
}
