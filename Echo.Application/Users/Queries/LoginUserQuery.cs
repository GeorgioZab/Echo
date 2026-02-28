using Echo.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Users.Queries;

// Запрос принимает логин/пароль и токен
public record LoginUserQuery(string Username, string Password) : IRequest<string>;

public class LoginUserQueryHandler : IRequestHandler<LoginUserQuery, string>
{
    private readonly IEchoDbContext _context;
    private readonly IJwtProvider _jwtProvider;

    public LoginUserQueryHandler(IEchoDbContext context, IJwtProvider jwtProvider)
    {
        _context = context;
        _jwtProvider = jwtProvider;
    }

    public async Task<string> Handle(LoginUserQuery request, CancellationToken cancellationToken)
    {
        // 1. Ищем юзера в БД
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);

        if (user == null)
        {
            throw new Exception("Пользователь не найден.");
        }

        // 2. Проверяем пароль (сравниваем введенный с хэшем из БД)
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if (!isPasswordValid)
        {
            throw new Exception("Неверный пароль.");
        }

        // 3. Генерируем токен
        string token = _jwtProvider.GenerateToken(user);

        return token;
    }
}