using System.Text;

namespace ManiaAPI.MasterServer;

/// <summary>
/// A binary reader modified to be compatible with Gbx engine binary serialization techniques.
/// </summary>
internal sealed class GbxBasedReader(Stream input, bool leaveOpen = true) : BinaryReader(input, encoding, leaveOpen)
{
    private static readonly Encoding encoding = Encoding.UTF8;

    /// <summary>
    /// Reads the next <see cref="int"/> from the current stream, casts it as <see cref="bool"/> and advances the current position of the stream by 4 bytes.
    /// </summary>
    /// <returns>A boolean.</returns>
    /// <exception cref="EndOfStreamException">The end of the stream is reached.</exception>
    /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
    /// <exception cref="IOException">An I/O error occurs.</exception>
    public override bool ReadBoolean()
    {
        return ReadInt32() != 0;
    }
    
    /// <summary>
    /// Reads the next <see cref="byte"/> from the current stream, casts it as <see cref="bool"/> and advances the current position of the stream by 1 byte.
    /// </summary>
    /// <returns>A boolean.</returns>
    /// <exception cref="EndOfStreamException">The end of the stream is reached.</exception>
    /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
    /// <exception cref="IOException">An I/O error occurs.</exception>
    public bool ReadBoolean(bool asByte)
    {
        return asByte
            ? ReadByte() != 0
            : ReadBoolean();
    }

    /// <summary>
    /// Reads a <see cref="string"/> from the current stream. The string is prefixed with the length, encoded as <see cref="int"/>.
    /// </summary>
    /// <returns>The string being read.</returns>
    /// <exception cref="EndOfStreamException">The end of the stream is reached.</exception>
    /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
    /// <exception cref="IOException">An I/O error occurs.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Length is negative.</exception>
    public override string ReadString()
    {
        return ReadString(ReadInt32());
    }

    /// <summary>
    /// Reads a <see cref="string"/> from the current stream using the <paramref name="length"/> parameter.
    /// </summary>
    /// <param name="length">Length of the bytes to read.</param>
    /// <returns>The string being read.</returns>
    /// <exception cref="EndOfStreamException">The end of the stream is reached.</exception>
    /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
    /// <exception cref="IOException">An I/O error occurs.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is negative.</exception>
    public string ReadString(int length)
    {
        return encoding.GetString(ReadBytes(length));
    }
}
