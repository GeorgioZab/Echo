using Echo.Application.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // GET: api/User/search
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string term)
    {
        try
        {
            var users = await _mediator.Send(new SearchUsersQuery(term));
            return Ok(users);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }
}