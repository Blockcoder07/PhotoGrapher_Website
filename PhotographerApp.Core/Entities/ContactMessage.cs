namespace PhotographerApp.Core.Entities;

public class ContactMessage : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Subject { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? EventType { get; set; }
    public DateTime? EventDate { get; set; }
    public bool IsRead { get; set; }
}
