using Echo.Application.Interfaces;
using Echo.Domain;
using Echo.Domain.Enums;
using MediatR;

namespace Echo.Application.Chats.Commands;

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
        var currentUserId = _currentUserService.UserId;

        if (currentUserId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Пользователь не авторизован!");
        }

        var chat = new Chat
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            IsGroup = request.IsGroup
        };

        var member = new ChatMember
        {
            ChatId = chat.Id,
            UserId = currentUserId,
            Role = MemberRole.Admin
        };

        _context.Chats.Add(chat);
        _context.ChatMembers.Add(member);
        await _context.SaveChangesAsync(cancellationToken);

        return chat.Id;
    }
}