using Echo.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Users.Queries;

public record UserDto(Guid Id, string Username);

public record SearchUsersQuery(string SearchTerm) : IRequest<List<UserDto>>;

public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, List<UserDto>>
{
    private readonly IEchoDbContext _context;

    public SearchUsersQueryHandler(IEchoDbContext context) => _context = context;

    public async Task<List<UserDto>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SearchTerm)) return new List<UserDto>();

        return await _context.Users
            .Where(u => u.Username.Contains(request.SearchTerm))
            .Take(10)
            .Select(u => new UserDto(u.Id, u.Username))
            .ToListAsync(cancellationToken);
    }
}