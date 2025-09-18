namespace ManiaAPI.NadeoAPI.Extensions.Gbx;

/// <summary>
/// The exception that is thrown when a map is not found with the NadeoAPI.
/// </summary>
[Serializable]
public class MapNotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MapNotFoundException"/> class.
    /// </summary>
    public MapNotFoundException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MapNotFoundException"/> class with a specified error message.
    /// </summary>
	public MapNotFoundException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MapNotFoundException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
	public MapNotFoundException(string message, Exception inner) : base(message, inner) { }
}