namespace crm_backend.Modules.Customer;

public class FileAttachment
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string S3Key { get; set; } = string.Empty; // S3 object key
    public string S3Url { get; set; } = string.Empty; // Pre-signed URL or public URL

    // Relationships
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public int UploadedByUserId { get; set; }
    public crm_backend.Modules.User.User UploadedByUser { get; set; } = null!;

    // Timestamps
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Optional metadata
    public string? Description { get; set; }
    public string? Tags { get; set; } // JSON array of tags
}