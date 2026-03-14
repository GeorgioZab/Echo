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
            var token = await _mediator.Send(query);

            return Ok(new { Token = token, Message = "Успешный вход!" });
        }
        catch (Exception ex)
        {
            return Unauthorized(new { Error = ex.Message });
        }
    }

    [Authorize]
    [HttpGet("secret")]
    public IActionResult GetSecretData()
    {
        return Ok(new { Message = "Доступ разрешен! Ты находишься в секретной зоне." });
    }
}