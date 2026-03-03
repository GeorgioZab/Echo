using Echo.Application.Interfaces;
using Echo.Domain;
using Echo.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Chats.Commands;

public record AddMemberCommand(Guid ChatId, Guid UserId) : IRequest<bool>;

public class AddMemberCommandHandler : IRequestHandler<AddMemberCommand, bool>
{
    private readonly IEchoDbContext _context;

    public AddMemberCommandHandler(IEchoDbContext context) => _context = context;

    public async Task<bool> Handle(AddMemberCommand request, CancellationToken cancellationToken)
    {
        // Проверяем, не в чате ли он уже
        var exists = await _context.ChatMembers
            .AnyAsync(cm => cm.ChatId == request.ChatId && cm.UserId == request.UserId);

        if (exists) return true;

        var member = new ChatMember
        {
            ChatId = request.ChatId,
            UserId = request.UserId,
            Role = MemberRole.Member
        };

        _context.ChatMembers.Add(member);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}