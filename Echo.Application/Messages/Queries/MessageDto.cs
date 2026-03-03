namespace Echo.Application.Messages.Queries;

public record MessageDto(
    Guid Id,
    Guid SenderId,
    string SenderName,
    string Content,
    DateTime SentAt,
    Guid ChatId);