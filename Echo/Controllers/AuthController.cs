using Echo.Application.Users.Commands;
using Echo.Application.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        try
        {
            var userId = await _mediator.Send(command);
            return Ok(new { UserId = userId, Message = "Регистрация успешна!" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserQuery query)
    {
        try
        {
            // Отправляем запрос через MediatR
            var token = await _mediator.Send(query);

            // Возвращаем токен клиенту
            return Ok(new { Token = token, Message = "Успешный вход!" });
        }
        catch (Exception ex)
        {
            // Если пароль неверный или юзера нет
            return Unauthorized(new { Error = ex.Message });
        }
    }

    [Authorize]
    [HttpGet("secret")]
    public IActionResult GetSecretData()
    {
        // Этот код выполнится если пользователь передал правильный токен
        return Ok(new { Message = "Доступ разрешен! Ты находишься в секретной зоне." });
    }
}