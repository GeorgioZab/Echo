using Echo.Application.Interfaces;
using Echo.Domain;
using Echo.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Echo.Application.Users.Commands;

// 1. То, что мы получаем от пользователя
// Возвращаем Guid (Id нового пользователя)
public record RegisterUserCommand(string Username, string Password) : IRequest<Guid>;

// 2. Логика обработки
public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IEchoDbContext _context;

    public RegisterUserCommandHandler(IEchoDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Проверяем, есть ли уже такой юзер
        var userExists = await _context.Users.AnyAsync(u => u.Username == request.Username, cancellationToken);
        if (userExists)
        {
            throw new Exception("Пользователь с таким именем уже существует!");
        }

        // Хэшируем пароль
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // Создаем сущность
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            PasswordHash = passwordHash,
            Role = Role.User
        };

        // Сохраняем в БД
        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}