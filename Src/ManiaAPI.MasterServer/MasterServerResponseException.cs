namespace ManiaAPI.MasterServer;

/// <summary>
/// The master server returned an error, or the response from it or its scores files couldn't be read,
/// like XML without the expected elements or a file without a timestamp.
/// </summary>
[Serializable]
public class MasterServerResponseException : Exception
{
    /// <summary>
    /// Raw error code returned by the master server, or <see langword="null"/> if the response couldn't be read.
    /// </summary>
    public int? Value { get; }

    /// <summary>
    /// Error code returned by the master server, or <see langword="null"/> if the response couldn't be read.
    /// It can hold a value that is not defined in <see cref="MasterServerError"/>, use <see cref="Value"/> for the raw code.
    /// </summary>
    public MasterServerError? Error => (MasterServerError?)Value;

    /// <summary>
    /// Error message returned by the master server, or <see langword="null"/> if the response couldn't be read.
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    /// Time it took to parse the error response.
    /// </summary>
    public TimeSpan XmlParseTime { get; }

    /// <summary>
    /// Details of the request that returned the error, or <see langword="null"/> if the response couldn't be read.
    /// </summary>
    public MasterServerResponseDetails? Details { get; }

    /// <summary>
    /// Creates the exception for an error returned by the master server.
    /// </summary>
    public MasterServerResponseException(int value, string message, TimeSpan xmlParseTime, MasterServerResponseDetails details) : base($"Error {value}: {message}")
    {
        Value = value;
        ErrorMessage = message;
        XmlParseTime = xmlParseTime;
        Details = details;
    }

    /// <summary>
    /// Creates the exception for a response that couldn't be read.
    /// </summary>
    public MasterServerResponseException(string? message) : base(message) { }

    /// <summary>
    /// Creates the exception for a response that couldn't be read.
    /// </summary>
    public MasterServerResponseException(string? message, Exception? innerException) : base(message, innerException) { }
}
