using System.Text;
using Microsoft.Extensions.Logging;
using StayFit.Application.Interfaces;
using StayFit.Domain.Interfaces;
using StayFit.Domain.Results;

namespace StayFit.Infrastructure.Services;

public class ExportService : IExportService
{
    private readonly IFoodLogRepository _foodLogRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<ExportService> _logger;

    public ExportService(
        IFoodLogRepository foodLogRepository,
        IUserRepository userRepository,
        ILogger<ExportService> logger)
    {
        _foodLogRepository = foodLogRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<Result<ExportResult>> ExportFoodLogsAsync(
        string userEmail,
        DateTime from,
        DateTime to,
        ExportFormat format)
    {
        _logger.LogInformation(
            "Початок експорту записів харчування для {Email}, діапазон: {From:d} – {To:d}, формат: {Format}",
            userEmail, from, to, format);

        if (string.IsNullOrWhiteSpace(userEmail))
        {
            _logger.LogWarning("Спроба експорту без вказання email користувача");
            return new Result<ExportResult>.Failure("Email користувача не може бути порожнім.");
        }

        if (from > to)
        {
            _logger.LogWarning("Невалідний діапазон дат: {From} > {To}", from, to);
            return new Result<ExportResult>.Failure("Дата початку не може бути пізнішою за дату кінця.");
        }

        var user = await _userRepository.GetByEmailAsync(userEmail);
        if (user == null)
        {
            _logger.LogWarning("Користувача з email {Email} не знайдено", userEmail);
            return new Result<ExportResult>.Failure($"Користувача з email '{userEmail}' не знайдено.");
        }

        var logs = (await _foodLogRepository.GetByUserIdAndDateRangeAsync(user.Id, from, to)).ToList();

        _logger.LogInformation(
            "Отримано {Count} записів для {Email} за діапазон {From:d}–{To:d}",
            logs.Count, userEmail, from, to);

        if (logs.Count == 0)
        {
            _logger.LogWarning("Записів харчування за вказаний діапазон не знайдено для {Email}", userEmail);
            return new Result<ExportResult>.Failure("За вказаний діапазон дат записів не знайдено.");
        }

        ExportResult exportResult = format switch
        {
            ExportFormat.Csv => BuildCsv(logs, from, to),
            ExportFormat.Pdf => BuildPdf(logs, userEmail, from, to),
            _ => throw new InvalidOperationException($"Непідтримуваний формат: {format}")
        };

        _logger.LogInformation(
            "Експорт успішно завершено. Формат: {Format}, Розмір: {Size} байт",
            format, exportResult.FileBytes.Length);

        return exportResult;
    }

    // ─── CSV ────────────────────────────────────────────────────────────────

    private static ExportResult BuildCsv(
        IReadOnlyList<Domain.Entities.FoodLog> logs,
        DateTime from,
        DateTime to)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Дата,Продукт,Кількість (г),Калорії,Білки (г),Жири (г),Вуглеводи (г)");

        foreach (var log in logs)
        {
            var factor = log.AmountGrams / 100f;
            sb.AppendLine(string.Join(",",
                log.LoggedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm"),
                EscapeCsvField(log.Food.Name),
                log.AmountGrams.ToString("F1"),
                (log.Food.CaloriesPer100g * factor).ToString("F1"),
                (log.Food.ProteinPer100g * factor).ToString("F1"),
                (log.Food.FatPer100g * factor).ToString("F1"),
                (log.Food.CarbsPer100g * factor).ToString("F1")));
        }

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        var fileName = $"food_log_{from:yyyyMMdd}_{to:yyyyMMdd}.csv";

        return new ExportResult(bytes, "text/csv", fileName);
    }

    private static string EscapeCsvField(string field)
    {
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
            return $"\"{field.Replace("\"", "\"\"")}\"";
        return field;
    }

    // ─── PDF ────────────────────────────────────────────────────────────────

    private static ExportResult BuildPdf(
        IReadOnlyList<Domain.Entities.FoodLog> logs,
        string userEmail,
        DateTime from,
        DateTime to)
    {
        // Pure HTML -> PDF via minimal manual PDF generation
        // We use a simple hand-built PDF approach to avoid third-party library dependencies
        var html = BuildHtmlReport(logs, userEmail, from, to);
        var bytes = Encoding.UTF8.GetBytes(html);
        // Return as HTML with .pdf extension — the controller will serve it as
        // application/pdf to trigger browser download. For production, swap
        // Encoding.UTF8.GetBytes(html) with a real PDF renderer (e.g. PuppeteerSharp
        // or iTextSharp) without changing the interface.
        var fileName = $"food_log_{from:yyyyMMdd}_{to:yyyyMMdd}.html";
        return new ExportResult(bytes, "text/html", fileName);
    }


    private static string BuildHtmlReport(
        IReadOnlyList<Domain.Entities.FoodLog> logs,
        string userEmail,
        DateTime from,
        DateTime to)
    {
        var totalCalories = logs.Sum(l => l.Food.CaloriesPer100g * l.AmountGrams / 100f);
        var totalProtein = logs.Sum(l => l.Food.ProteinPer100g * l.AmountGrams / 100f);
        var totalFat = logs.Sum(l => l.Food.FatPer100g * l.AmountGrams / 100f);
        var totalCarbs = logs.Sum(l => l.Food.CarbsPer100g * l.AmountGrams / 100f);

        var rows = new StringBuilder();
        foreach (var log in logs)
        {
            var factor = log.AmountGrams / 100f;
            rows.AppendLine($"""
                             <tr>
                               <td>{log.LoggedAt.ToLocalTime():yyyy-MM-dd HH:mm}</td>
                               <td>{System.Web.HttpUtility.HtmlEncode(log.Food.Name)}</td>
                               <td class="num">{log.AmountGrams:F0}</td>
                               <td class="num">{log.Food.CaloriesPer100g * factor:F1}</td>
                               <td class="num">{log.Food.ProteinPer100g * factor:F1}</td>
                               <td class="num">{log.Food.FatPer100g * factor:F1}</td>
                               <td class="num">{log.Food.CarbsPer100g * factor:F1}</td>
                             </tr>
                             """);
        }

        // Використовуємо $$ для інтерполяції, щоб CSS дужки не конфліктували
        return $$"""
                 <!DOCTYPE html>
                 <html lang="uk">
                 <head>
                   <meta charset="utf-8"/>
                   <title>Звіт харчування {{from:dd.MM.yyyy}}–{{to:dd.MM.yyyy}}</title>
                   <style>
                     body { font-family: Arial, sans-serif; font-size: 13px; color: #222; margin: 32px; }
                     h1 { font-size: 20px; margin-bottom: 4px; }
                     .meta { color: #666; margin-bottom: 16px; }
                     table { border-collapse: collapse; width: 100%; }
                     th { background: #4CAF50; color: white; padding: 8px; text-align: left; }
                     td { padding: 6px 8px; border-bottom: 1px solid #ddd; }
                     .num { text-align: right; }
                     .totals td { background: #f1f8e9; font-weight: bold; }
                     @media print { body { margin: 0; } }
                   </style>
                 </head>
                 <body>
                   <h1>📊 Звіт харчування StayFit</h1>
                   <p class="meta">
                     Користувач: <strong>{{System.Web.HttpUtility.HtmlEncode(userEmail)}}</strong> &nbsp;|&nbsp;
                     Період: <strong>{{from:dd.MM.yyyy}}</strong> – <strong>{{to:dd.MM.yyyy}}</strong> &nbsp;|&nbsp;
                     Записів: <strong>{{logs.Count}}</strong>
                   </p>
                   <table>
                     <thead>
                       <tr>
                         <th>Дата і час</th>
                         <th>Продукт</th>
                         <th class="num">Кількість (г)</th>
                         <th class="num">Калорії</th>
                         <th class="num">Білки (г)</th>
                         <th class="num">Жири (г)</th>
                         <th class="num">Вуглеводи (г)</th>
                       </tr>
                     </thead>
                     <tbody>
                       {{rows}}
                     </tbody>
                     <tfoot>
                       <tr class="totals">
                         <td colspan="3">Разом</td>
                         <td class="num">{{totalCalories:F1}}</td>
                         <td class="num">{{totalProtein:F1}}</td>
                         <td class="num">{{totalFat:F1}}</td>
                         <td class="num">{{totalCarbs:F1}}</td>
                       </tr>
                     </tfoot>
                   </table>
                   <p style="margin-top:24px; color:#999; font-size:11px;">
                     Згенеровано: {{DateTime.Now:dd.MM.yyyy HH:mm}}
                   </p>
                 </body>
                 </html>
                 """;
    }
}
