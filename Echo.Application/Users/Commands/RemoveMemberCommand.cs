using Echo.Application.Interfaces;
using Echo.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Chats.Commands;

public record RemoveMemberCommand(Guid ChatId, Guid UserIdToRemove) : IRequest<bool>;

public class RemoveMemberCommandHandler : IRequestHandler<RemoveMemberCommand, bool>
{
    private readonly IEchoDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public RemoveMemberCommandHandler(IEchoDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(RemoveMemberCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;

        var requester = await _context.Users.FirstAsync(u => u.Id == currentUserId);
        var requesterMemberRecord = await _context.ChatMembers
            .FirstOrDefaultAsync(cm => cm.ChatId == request.ChatId && cm.UserId == currentUserId);

        bool isGlobalAdmin = requester.Role == Role.Admin;
        bool isChatAdmin = requesterMemberRecord?.Role == MemberRole.Admin;

        if (!isGlobalAdmin && !isChatAdmin)
        {
            throw new Exception("У вас нет прав для удаления участников из этого чата!");
        }

        var memberToRemove = await _context.ChatMembers
            .FirstOrDefaultAsync(cm => cm.ChatId == request.ChatId && cm.UserId == request.UserIdToRemove);

        if (memberToRemove == null) return true;

        if (memberToRemove.UserId == currentUserId && !isGlobalAdmin)
        {
            throw new Exception("Вы не можете удалить сами себя из собственного чата!");
        }

        _context.ChatMembers.Remove(memberToRemove);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}