using Echo.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Admin.Queries;

public record AlertDto(
    Guid AlertId,
    Guid MessageId,
    string SenderName,
    string Content,
    string Reason,
    DateTime SentAt);

public record GetAlertsQuery() : IRequest<List<AlertDto>>;

public class GetAlertsQueryHandler : IRequestHandler<GetAlertsQuery, List<AlertDto>>
{
    private readonly IEchoDbContext _context;

    public GetAlertsQueryHandler(IEchoDbContext context) => _context = context;

    public async Task<List<AlertDto>> Handle(GetAlertsQuery request, CancellationToken cancellationToken)
    {
        return await _context.AdminAlerts
            .Where(a => !a.IsResolved)
            .Include(a => a.Message)
                .ThenInclude(m => m.Sender)
            .Select(a => new AlertDto(
                a.Id,
                a.MessageId,
                a.Message.Sender.Username,
                a.Message.Content,
                a.Reason,
                a.Message.SentAt))
            .ToListAsync(cancellationToken);
    }
}