using Echo.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Messages.Queries;

public record GetChatMessagesQuery(Guid ChatId) : IRequest<List<MessageDto>>;

public class GetChatMessagesQueryHandler : IRequestHandler<GetChatMessagesQuery, List<MessageDto>>
{
    private readonly IEchoDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetChatMessagesQueryHandler(IEchoDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<MessageDto>> Handle(GetChatMessagesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var isMember = await _context.ChatMembers
            .AnyAsync(cm => cm.ChatId == request.ChatId && cm.UserId == userId, cancellationToken);

        if (!isMember)
        {
            throw new UnauthorizedAccessException("Нет доступа к этому чату!");
        }

        var messages = await _context.Messages
            .Where(m => m.ChatId == request.ChatId)
            .OrderBy(m => m.SentAt)
            .Select(m => new MessageDto(
                m.Id,
                m.SenderId,
                m.Sender.Username,
                m.Content,
                m.SentAt))
            .ToListAsync(cancellationToken);

        return messages;
    }
}