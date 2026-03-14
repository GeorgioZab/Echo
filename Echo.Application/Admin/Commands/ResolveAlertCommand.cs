using Echo.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Admin.Commands;

public record ResolveAlertCommand(Guid AlertId, bool DeleteMessage) : IRequest<bool>;

public class ResolveAlertCommandHandler : IRequestHandler<ResolveAlertCommand, bool>
{
    private readonly IEchoDbContext _context;

    public ResolveAlertCommandHandler(IEchoDbContext context) => _context = context;

    public async Task<bool> Handle(ResolveAlertCommand request, CancellationToken cancellationToken)
    {
        var alert = await _context.AdminAlerts
            .Include(a => a.Message)
            .FirstOrDefaultAsync(a => a.Id == request.AlertId, cancellationToken);

        if (alert == null) return false;

        alert.IsResolved = true;

        if (request.DeleteMessage)
        {
            alert.Message.Content = "[Сообщение удалено администратором]";
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}