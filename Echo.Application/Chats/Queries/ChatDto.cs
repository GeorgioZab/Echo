namespace Echo.Application.Chats.Queries;

public record ChatDto
(
    Guid Id,
    string? Title,
    bool IsGroup,
    string? AvatarUrl
);