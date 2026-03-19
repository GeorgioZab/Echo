using Echo.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Users.Queries;

public record UserProfileDto(string Username, string? AvatarUrl, string? Bio, string Role);

public record GetProfileQuery(Guid UserId) : IRequest<UserProfileDto?>;

public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, UserProfileDto?>
{
    private readonly IEchoDbContext _context;

    public GetProfileQueryHandler(IEchoDbContext context)
    {
        _context = context;
    }

    public async Task<UserProfileDto?> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Where(u => u.Id == request.UserId)
            .Select(u => new UserProfileDto(
                u.Username,
                u.AvatarUrl,
                u.Bio,
                u.Role.ToString()))
            .FirstOrDefaultAsync(cancellationToken);

        return user;
    }
}