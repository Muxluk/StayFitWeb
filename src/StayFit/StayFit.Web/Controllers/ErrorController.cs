using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StayFit.Web.Models;

namespace StayFit.Web.Controllers;

[Route("Error")]
public class ErrorController : Controller
{
    [Route("{statusCode:int}")]
    public IActionResult HandleStatusCode(int statusCode)
    {
        var model = BuildModel(statusCode);
        Response.StatusCode = statusCode;
        return View("HttpError", model);
    }

    private static HttpErrorViewModel BuildModel(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status403Forbidden => new HttpErrorViewModel
            {
                StatusCode = statusCode,
                Title = "Доступ заборонено",
                Message = "У вас немає прав для перегляду цієї сторінки."
            },
            StatusCodes.Status404NotFound => new HttpErrorViewModel
            {
                StatusCode = statusCode,
                Title = "Сторінку не знайдено",
                Message = "Схоже, такої сторінки не існує або її було переміщено."
            },
            StatusCodes.Status500InternalServerError => new HttpErrorViewModel
            {
                StatusCode = statusCode,
                Title = "Внутрішня помилка сервера",
                Message = "Щось пішло не так. Спробуйте оновити сторінку трохи пізніше."
            },
            _ => new HttpErrorViewModel
            {
                StatusCode = statusCode,
                Title = "Помилка",
                Message = "Під час обробки запиту сталася помилка."
            }
        };
    }
}
