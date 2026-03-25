namespace StayFit.Domain.Exceptions;

/// <summary>
/// Виняток, що викидається, коли запитуваний ресурс не знайдено.
/// </summary>
public sealed class NotFoundException : StayFitException
{
    public NotFoundException(string resourceName, object key)
        : base($"{resourceName} з ключем '{key}' не знайдено.")
    {
        ResourceName = resourceName;
        Key = key;
    }

    public string ResourceName { get; }
    public object Key { get; }
}
