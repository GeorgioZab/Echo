using Echo.Application.Interfaces;
using Echo.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Users.Commands;

public record SetAdminRoleCommand(Guid UserId) : IRequest<bool>;

public class SetAdminRoleCommandHandler : IRequestHandler<SetAdminRoleCommand, bool>
{
    private readonly IEchoDbContext _context;

    public SetAdminRoleCommandHandler(IEchoDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(SetAdminRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            throw new Exception("Пользователь не найден");
        }

        user.Role = Role.Admin;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}