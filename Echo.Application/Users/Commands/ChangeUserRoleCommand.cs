using Echo.Application.Interfaces;
using Echo.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Users.Commands;

public record ChangeUserRoleCommand(Guid UserId, Role NewRole) : IRequest<bool>;

public class ChangeUserRoleCommandHandler : IRequestHandler<ChangeUserRoleCommand, bool>
{
    private readonly IEchoDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public ChangeUserRoleCommandHandler(IEchoDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(ChangeUserRoleCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == _currentUserService.UserId)
        {
            throw new Exception("Вы не можете изменить роль самому себе!");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null) throw new Exception("Пользователь не найден");

        user.Role = request.NewRole;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}