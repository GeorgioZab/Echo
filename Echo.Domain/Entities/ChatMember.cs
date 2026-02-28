using Echo.Domain.Enums;

public class ChatMember
{
    public Guid ChatId { get; set; }
    public Guid UserId { get; set; }
    public MemberRole Role { get; set; }

    // Навигационные свойства для EF Core
    public Chat Chat { get; set; } = null!;
    public User User { get; set; } = null!;
}