using Echo.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Chats.Queries;

public record GetMyChatsQuery() : IRequest<List<ChatDto>>;

public class GetMyChatsQueryHandler : IRequestHandler<GetMyChatsQuery, List<ChatDto>>
{
    private readonly IEchoDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetMyChatsQueryHandler(IEchoDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<ChatDto>> Handle(GetMyChatsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var chats = await _context.ChatMembers
            .Where(cm => cm.UserId == userId)
            .Select(cm => new ChatDto(
                cm.ChatId,
                cm.Chat.IsGroup ? cm.Chat.Title : _context.ChatMembers
                    .Where(other => other.ChatId == cm.ChatId && other.UserId != userId)
                    .Select(other => other.User.Username).FirstOrDefault(),
                cm.Chat.IsGroup,
                cm.Chat.IsGroup ? cm.Chat.AvatarUrl : _context.ChatMembers
                    .Where(other => other.ChatId == cm.ChatId && other.UserId != userId)
                    .Select(other => other.User.AvatarUrl).FirstOrDefault()
            ))
            .ToListAsync(cancellationToken);

        return chats;
    }
}