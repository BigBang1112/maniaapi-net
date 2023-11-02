using System.Runtime.InteropServices;
using System.Text;

namespace ManiaAPI.XmlRpc;

/// <summary>
/// A binary reader modified to be compatible with Gbx engine binary serialization techniques.
/// </summary>
sealed class GbxBasedReader(Stream input, bool leaveOpen = true) : BinaryReader(input, encoding, leaveOpen)
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

    /// <summary>
    /// Reads bytes into a stack-allocated area (decided by <paramref name="length"/>, where <paramref name="lengthInBytes"/> determines the format), then casts the data into <typeparamref name="T"/> structs by using <see cref="MemoryMarshal.Cast{TFrom, TTo}(Span{TFrom})"/>, resulting in more optimized read of array for value types.
    /// </summary>
    /// <typeparam name="T">A struct type.</typeparam>
    /// <param name="length">Length of the array.</param>
    /// <param name="lengthInBytes">If to take length as the size of the byte array and not the <typeparamref name="T"/> array.</param>
    /// <returns>An array of <typeparamref name="T"/>.</returns>
    /// <exception cref="EndOfStreamException">The end of the stream is reached.</exception>
    /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
    /// <exception cref="IOException">An I/O error occurs.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Length is negative.</exception>
    public T[] ReadArray<T>(int length, bool lengthInBytes = false) where T : struct
    {
        if (length < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "Length is negative.");
        }

        var l = length * (lengthInBytes ? 1 : Marshal.SizeOf<T>());

#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
        Span<byte> bytes = stackalloc byte[l];
        Read(bytes);
#else
        var bytes = ReadBytes(l);
#endif

        return MemoryMarshal.Cast<byte, T>(bytes).ToArray();
    }

    /// <summary>
    /// First reads an <see cref="int"/> representing the length, then reads bytes into a stack-allocated area (decided by the length, where <paramref name="lengthInBytes"/> determines the format), then casts the data into <typeparamref name="T"/> structs by using <see cref="MemoryMarshal.Cast{TFrom, TTo}(Span{TFrom})"/>, resulting in more optimized read of array for value types.
    /// </summary>
    /// <typeparam name="T">A struct type.</typeparam>
    /// <param name="lengthInBytes">If to take length as the size of the byte array and not the <typeparamref name="T"/> array.</param>
    /// <returns>An array of <typeparamref name="T"/>.</returns>
    /// <exception cref="EndOfStreamException">The end of the stream is reached.</exception>
    /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
    /// <exception cref="IOException">An I/O error occurs.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Length is negative.</exception>
    public T[] ReadArray<T>(bool lengthInBytes = false) where T : struct
    {
        return ReadArray<T>(length: ReadInt32(), lengthInBytes);
    }
    
    public T[] ReadArray<T>(int length, Func<GbxBasedReader, T> func)
    {
        var array = new T[length];

        for (var i = 0; i < length; i++)
        {
            array[i] = func(this);
        }

        return array;
    }
}
