using Echo.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace Echo.Application.Interfaces;

public interface IEchoDbContext
{
    DbSet<User> Users { get; }
    DbSet<Chat> Chats { get; }
    DbSet<ChatMember> ChatMembers { get; }
    DbSet<Message> Messages { get; }
    DbSet<AdminAlert> AdminAlerts { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}