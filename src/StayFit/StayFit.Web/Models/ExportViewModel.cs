using System.ComponentModel.DataAnnotations;
using StayFit.Application.Interfaces;

namespace StayFit.Web.Models;

public class ExportViewModel
{
    [Required(ErrorMessage = "Оберіть дату початку")]
    [DataType(DataType.Date)]
    [Display(Name = "Дата початку")]
    public DateTime From { get; set; } = DateTime.Today.AddDays(-7);

    [Required(ErrorMessage = "Оберіть дату кінця")]
    [DataType(DataType.Date)]
    [Display(Name = "Дата кінця")]
    public DateTime To { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Оберіть формат")]
    [Display(Name = "Формат")]
    public ExportFormat Format { get; set; } = ExportFormat.Csv;
}
