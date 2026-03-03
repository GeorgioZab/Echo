using Echo.Application.Messages.Queries;

namespace Echo.Application.Interfaces;

public interface IMessageNotificationService
{
    Task NotifyNewMessage(Guid chatId, MessageDto message);
}