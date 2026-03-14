using Echo.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Chats.Queries;

public record ChatMemberDto(Guid UserId, string Username, string Role);

public record GetChatMembersQuery(Guid ChatId) : IRequest<List<ChatMemberDto>>;

public class GetChatMembersQueryHandler : IRequestHandler<GetChatMembersQuery, List<ChatMemberDto>>
{
    private readonly IEchoDbContext _context;

    public GetChatMembersQueryHandler(IEchoDbContext context) => _context = context;

    public async Task<List<ChatMemberDto>> Handle(GetChatMembersQuery request, CancellationToken cancellationToken)
    {
        return await _context.ChatMembers
            .Where(cm => cm.ChatId == request.ChatId)
            .Select(cm => new ChatMemberDto(cm.UserId, cm.User.Username, cm.Role.ToString()))
            .ToListAsync(cancellationToken);
    }
}