using Echo.Application.Interfaces;
using Echo.Application.Messages.Queries;
using Echo.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Messages.Commands;

public record SendMessageCommand(Guid ChatId, string Content) : IRequest<Guid>;

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, Guid>
{
    private readonly IEchoDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMessageNotificationService _notificationService;
    private readonly IContentModerationService _moderationService;

    public SendMessageCommandHandler(
        IEchoDbContext context,
        ICurrentUserService currentUserService,
        IMessageNotificationService notificationService,
        IContentModerationService moderationService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _notificationService = notificationService;
        _moderationService = moderationService;
    }

    public async Task<Guid> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        // 1. Проверка прав доступа
        var isMember = await _context.ChatMembers
            .AnyAsync(cm => cm.ChatId == request.ChatId && cm.UserId == userId, cancellationToken);

        if (!isMember)
        {
            throw new UnauthorizedAccessException("Вы не состоите в этом чате!");
        }

        bool isToxic = _moderationService.IsToxic(request.Content);

        // 2. Создание сообщения
        var message = new Message
        {
            Id = Guid.NewGuid(),
            ChatId = request.ChatId,
            SenderId = userId,
            Content = request.Content,
            SentAt = DateTime.UtcNow,
            IsFlaggedByML = isToxic // Ставим флаг на основе анализа ML
        };

        _context.Messages.Add(message);

        // 3. Создание алерта для админа (если ML пометил сообщение)
        if (isToxic)
        {
            var alert = new AdminAlert
            {
                Id = Guid.NewGuid(),
                MessageId = message.Id,
                Reason = "Подозрение на запрещенный контент",
                IsResolved = false
            };
            _context.AdminAlerts.Add(alert);
        }

        // 4. Сохранение в базу
        await _context.SaveChangesAsync(cancellationToken);

        // 5. Получаем данные отправителя
        var sender = await _context.Users
            .FirstAsync(u => u.Id == userId, cancellationToken);

        // 6. Формируем DTO для отправки
        var messageDto = new MessageDto(
            message.Id,
            userId,
            sender.Username,
            message.Content,
            message.SentAt,
            message.ChatId);

        // 7. Отправка уведомления всем участникам чата в реальном времени
        await _notificationService.NotifyNewMessage(request.ChatId, messageDto);

        return message.Id;
    }
}