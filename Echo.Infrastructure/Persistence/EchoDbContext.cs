using Microsoft.EntityFrameworkCore;
using Echo.Application.Interfaces;

namespace Echo.Infrastructure.Persistence;

public class EchoDbContext : DbContext, IEchoDbContext
{
    public EchoDbContext(DbContextOptions<EchoDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Chat> Chats { get; set; }
    public DbSet<ChatMember> ChatMembers { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<AdminAlert> AdminAlerts { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ChatMember>()
            .HasKey(cm => new { cm.ChatId, cm.UserId });

        builder.Entity<ChatMember>()
            .HasOne(cm => cm.Chat)
            .WithMany()
            .HasForeignKey(cm => cm.ChatId);

        builder.Entity<ChatMember>()
            .HasOne(cm => cm.User)
            .WithMany()
            .HasForeignKey(cm => cm.UserId);
    }
}