namespace ManiaAPI.NadeoAPI;

public class NadeoAPIResponseException : Exception
{
    public NadeoAPIResponseException(string? message) : base(message) { }
    public NadeoAPIResponseException(string? message, Exception inner) : base(message, inner) { }
}
