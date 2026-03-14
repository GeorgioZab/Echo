using Echo.Application.Interfaces;
using Echo.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record CreatePrivateChatCommand(Guid TargetUserId) : IRequest<Guid>;

public class CreatePrivateChatCommandHandler : IRequestHandler<CreatePrivateChatCommand, Guid>
{
    private readonly IEchoDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreatePrivateChatCommandHandler(IEchoDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreatePrivateChatCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;

        var existingChatId = await _context.ChatMembers
            .Where(cm => cm.Chat.IsGroup == false)
            .GroupBy(cm => cm.ChatId)
            .Where(g => g.Any(cm => cm.UserId == currentUserId) && g.Any(cm => cm.UserId == request.TargetUserId))
            .Select(g => g.Key)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingChatId != Guid.Empty) return existingChatId;

        var chat = new Chat { Id = Guid.NewGuid(), IsGroup = false };
        _context.Chats.Add(chat);

        _context.ChatMembers.AddRange(
            new ChatMember { ChatId = chat.Id, UserId = currentUserId, Role = MemberRole.Admin },
            new ChatMember { ChatId = chat.Id, UserId = request.TargetUserId, Role = MemberRole.Member }
        );

        await _context.SaveChangesAsync(cancellationToken);
        return chat.Id;
    }
}