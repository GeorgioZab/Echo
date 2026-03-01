using Echo.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Chats.Queries;

// Запрос не принимает параметров, так как UserId берется из токена
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
            .Select(cm => cm.Chat)
            .Select(c => new ChatDto(
                c.Id,
                c.Title,
                c.IsGroup,
                c.AvatarUrl))
            .ToListAsync(cancellationToken);

        return chats;
    }
}