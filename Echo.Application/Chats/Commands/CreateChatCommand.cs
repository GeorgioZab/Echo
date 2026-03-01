using Echo.Application.Interfaces;
using Echo.Domain;
using Echo.Domain.Enums;
using MediatR;

namespace Echo.Application.Chats.Commands;

// Что получаем с фронта: Название чата и флаг IsGroup
public record CreateChatCommand(string? Title, bool IsGroup) : IRequest<Guid>;

public class CreateChatCommandHandler : IRequestHandler<CreateChatCommand, Guid>
{
    private readonly IEchoDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateChatCommandHandler(IEchoDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateChatCommand request, CancellationToken cancellationToken)
    {
        // 1. Узнаем, кто отправляет запрос
        var currentUserId = _currentUserService.UserId;

        if (currentUserId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Пользователь не авторизован!");
        }

        // 2. Создаем Чат
        var chat = new Chat
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            IsGroup = request.IsGroup
        };

        // 3. Создаем связь: добавляем создателя в этот чат
        var member = new ChatMember
        {
            ChatId = chat.Id,
            UserId = currentUserId,
            Role = MemberRole.Admin // Создатель группы автоматом становится ее админом
        };

        // 4. Сохраняем в БД
        _context.Chats.Add(chat);
        _context.ChatMembers.Add(member);
        await _context.SaveChangesAsync(cancellationToken);

        return chat.Id;
    }
}