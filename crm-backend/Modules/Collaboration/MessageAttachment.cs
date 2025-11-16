namespace crm_backend.Modules.Collaboration;

public class MessageAttachment
{
    public int Id { get; set; }

    // Relationships
    public int MessageId { get; set; }
    public Message Message { get; set; } = null!;

    // File Information
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; } // in bytes

    public int UploadedByUserId { get; set; }
    public User.User UploadedByUser { get; set; } = null!;

    // Multi-tenant
    public int CompanyId { get; set; }
    public Company.Company Company { get; set; } = null!;

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

