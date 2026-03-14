namespace Echo.Application.Interfaces;

public interface IContentModerationService
{
    bool IsToxic(string text);
}