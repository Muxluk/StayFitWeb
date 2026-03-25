using Microsoft.AspNetCore.Mvc;
using StayFit.Domain.Exceptions;

namespace StayFit.Web.Controllers;

/// <summary>
/// Тестовий контролер для перевірки глобальної обробки винятків.
/// Видалити після тестування
/// </summary>
[ApiController]
[Route("api/test-exceptions")]
public class TestExceptionsController : ControllerBase
{
    [HttpGet("not-found")]
    public IActionResult TestNotFoundException()
    {
        throw new NotFoundException("Користувач", 123);
    }

    [HttpGet("email-send")]
    public IActionResult TestEmailSendException()
    {
        throw new EmailSendException("test@example.com", new Exception("SMTP failure"));
    }

    [HttpGet("invalid-token")]
    public IActionResult TestInvalidTokenException()
    {
        throw new InvalidTokenException("Токен прострочений");
    }

    [HttpGet("invalid-operation")]
    public IActionResult TestInvalidOperationException()
    {
        throw new InvalidOperationException("Профіль вже існує");
    }

    [HttpGet("argument-null")]
    public IActionResult TestArgumentNullException()
    {
        throw new ArgumentNullException("userId", "UserId не може бути null");
    }

    [HttpGet("key-not-found")]
    public IActionResult TestKeyNotFoundException()
    {
        throw new KeyNotFoundException("Продукт з ID 999 не знайдено");
    }

    [HttpGet("generic")]
    public IActionResult TestGenericException()
    {
        throw new Exception("Непередбачена помилка бази даних");
    }

    [HttpGet("divide-by-zero")]
    public IActionResult TestDivideByZero()
    {
        var x = 10;
        var y = 0;
        var result = x / y;
        return Ok(result);
    }
}
