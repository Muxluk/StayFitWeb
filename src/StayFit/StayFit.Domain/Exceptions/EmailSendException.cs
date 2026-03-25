namespace StayFit.Domain.Exceptions;

/// <summary>
/// Виняток, що викидається, коли не вдалося відправити email.
/// </summary>
public sealed class EmailSendException : StayFitException
{
    public EmailSendException(string email, Exception innerException)
        : base($"Не вдалося відправити email на адресу '{email}'.", innerException)
    {
        Email = email;
    }

    public string Email { get; }
}
