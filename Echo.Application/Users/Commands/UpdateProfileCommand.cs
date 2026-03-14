using Echo.Application.Interfaces;
using MediatR;

public record UpdateProfileCommand(string? AvatarUrl, string? Username) : IRequest<bool>;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, bool>
{
    private readonly IEchoDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateProfileCommandHandler(IEchoDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync(_currentUserService.UserId);
        if (user == null) return false;

        if (!string.IsNullOrEmpty(request.Username)) user.Username = request.Username;
        user.AvatarUrl = request.AvatarUrl;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}