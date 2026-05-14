namespace StayFit.Web.Models;

public sealed class CardViewModel
{
    public string Title { get; set; } = string.Empty;

    public string? Subtitle { get; set; }

    public string? IconClass { get; set; }

    public string ColorClass { get; set; } = "primary";

    public string CardClass { get; set; } = string.Empty;

    public string BodyHtml { get; set; } = string.Empty;

    public string? FooterHtml { get; set; }
}

public sealed class ProgressBarViewModel
{
    public string Label { get; set; } = string.Empty;

    public string? ValueText { get; set; }

    public decimal Value { get; set; }

    public decimal Max { get; set; } = 100m;

    public string ColorClass { get; set; } = "bg-primary";

    public string TrackClass { get; set; } = string.Empty;

    public string ProgressClass { get; set; } = "progress-sm";

    public string BarClass { get; set; } = string.Empty;

    public string? BarText { get; set; }

    public bool ShowBarText { get; set; }

    public bool ShowPercentText { get; set; } = true;

    public string PercentTextClass { get; set; } = "text-muted d-block mt-1 text-end";

    public string ContainerClass { get; set; } = "mb-3";
}

public sealed class ModalConfirmViewModel
{
    public string ModalId { get; set; } = "confirmModal";

    public string Title { get; set; } = "Підтвердження";

    public string Message { get; set; } = string.Empty;

    public string ConfirmButtonText { get; set; } = "Підтвердити";

    public string CancelButtonText { get; set; } = "Скасувати";

    public string ConfirmButtonClass { get; set; } = "btn-danger";
}

public sealed class SearchFilterViewModel
{
    public string Controller { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public string Method { get; set; } = "get";

    public string SearchName { get; set; } = "searchTerm";

    public string? SearchValue { get; set; }

    public string Placeholder { get; set; } = string.Empty;

    public string ButtonText { get; set; } = "Шукати";

    public string FormClass { get; set; } = "row g-3 mb-4 mt-2";

    public string SearchColClass { get; set; } = "col-md-5";

    public string ButtonColClass { get; set; } = "col-md-2";

    public string? AdditionalHtml { get; set; }
}