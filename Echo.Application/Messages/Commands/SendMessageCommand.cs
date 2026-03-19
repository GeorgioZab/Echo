using Echo.Application.Interfaces;
using Echo.Application.Messages.Queries;
using Echo.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Messages.Commands;

// Изменили команду: добавили Content и ImageUrl, сделали их nullable
public record SendMessageCommand(Guid ChatId, string? Content, string? ImageUrl) : IRequest<Guid>;

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

        // 1. Проверка прав доступа: состоит ли пользователь в чате
        var isMember = await _context.ChatMembers
            .AnyAsync(cm => cm.ChatId == request.ChatId && cm.UserId == userId, cancellationToken);

        if (!isMember)
        {
            throw new UnauthorizedAccessException("Вы не состоите в этом чате!");
        }

        // 2. Логика машинного обучения
        bool isToxic = false;

        // Если текст есть, проверяем его через ML-сервис
        if (!string.IsNullOrWhiteSpace(request.Content))
        {
            isToxic = _moderationService.IsToxic(request.Content);
        }

        // 3. Создание сущности сообщения
        var message = new Message
        {
            Id = Guid.NewGuid(),
            ChatId = request.ChatId,
            SenderId = userId,
            Content = request.Content ?? "",
            ImageUrl = request.ImageUrl,
            SentAt = DateTime.UtcNow,
            IsFlaggedByML = isToxic
        };

        _context.Messages.Add(message);

        // 4. Создание алерта для админа
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

        // 5. Сохранение в базу
        await _context.SaveChangesAsync(cancellationToken);

        // 6. Получаем данные отправителя для SignalR
        var sender = await _context.Users
            .FirstAsync(u => u.Id == userId, cancellationToken);

        // 7. Формируем DTO для отправки клиентам в реальном времени
        var messageDto = new MessageDto(
            message.Id,
            userId,
            sender.Username,
            message.Content,
            message.SentAt,
            message.ChatId,
            message.ImageUrl
        );

        // 8. Отправляем уведомление всем участникам чата
        await _notificationService.NotifyNewMessage(request.ChatId, messageDto);

        return message.Id;
    }
}