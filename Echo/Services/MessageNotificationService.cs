using Echo.Application.Interfaces;
using Echo.Application.Messages.Queries;
using Echo.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Echo.Api.Services;

public class MessageNotificationService : IMessageNotificationService
{
    private readonly IHubContext<ChatHub> _hubContext;

    public MessageNotificationService(IHubContext<ChatHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyNewMessage(Guid chatId, MessageDto message)
    {
        // Отправляем сообщение всем, кто находится в группе с именем chatId
        await _hubContext.Clients.Group(chatId.ToString())
            .SendAsync("ReceiveMessage", message);
    }
}