namespace Echo.Application.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }
}