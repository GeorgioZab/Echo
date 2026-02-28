public class AdminAlert
{
    public Guid Id { get; set; }
    public Guid MessageId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public bool IsResolved { get; set; }

    public Message Message { get; set; } = null!;
}