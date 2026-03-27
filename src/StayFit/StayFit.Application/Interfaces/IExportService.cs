using StayFit.Domain.Results;

namespace StayFit.Application.Interfaces;

public enum ExportFormat
{
    Csv,
    Pdf
}

public record ExportResult(byte[] FileBytes, string ContentType, string FileName);

public interface IExportService
{
    Task<Result<ExportResult>> ExportFoodLogsAsync(
        string userEmail,
        DateTime from,
        DateTime to,
        ExportFormat format);
}
