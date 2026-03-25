namespace StayFit.Domain.Exceptions;

/// <summary>
/// Базовий клас для всіх винятків домену StayFit.
/// </summary>
public abstract class StayFitException : Exception
{
    protected StayFitException(string message) : base(message)
    {
    }

    protected StayFitException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
