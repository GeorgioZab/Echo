using Echo.Application.Chats.Commands;
using Echo.Application.Chats.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IMediator _mediator;

    public ChatController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyChats()
    {
        // Отправляем пустой запрос GetMyChatsQuery
        var chats = await _mediator.Send(new GetMyChatsQuery());
        return Ok(chats);
    }

    [HttpPost]
    public async Task<IActionResult> CreateChat([FromBody] CreateChatCommand command)
    {
        var chatId = await _mediator.Send(command);
        return Ok(new { ChatId = chatId, Message = "Чат успешно создан!" });
    }
}