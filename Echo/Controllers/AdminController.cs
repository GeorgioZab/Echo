using Echo.Application.Admin.Commands;
using Echo.Application.Admin.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Api.Controllers;

[Authorize (Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator) => _mediator = mediator;

    [HttpGet("alerts")]
    public async Task<IActionResult> GetAlerts()
    {
        var alerts = await _mediator.Send(new GetAlertsQuery());
        return Ok(alerts);
    }

    [HttpPost("resolve")]
    public async Task<IActionResult> ResolveAlert([FromBody] ResolveAlertCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result) return NotFound("Алерт не найден");
        return Ok(new { Message = "Алерт обработан" });
    }
}