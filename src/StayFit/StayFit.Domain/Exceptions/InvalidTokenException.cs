namespace StayFit.Domain.Exceptions;

/// <summary>
/// Виняток, що викидається, коли формат токена невалідний.
/// </summary>
public sealed class InvalidTokenException : StayFitException
{
    public InvalidTokenException(string message = "Невірний або прострочений токен.")
        : base(message)
    {
    }

    public InvalidTokenException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
