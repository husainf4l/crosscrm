namespace crm_backend.Modules.Customer.DTOs;

public class FileAttachmentDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string S3Url { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int UploadedByUserId { get; set; }
    public string? UploadedByUserName { get; set; }
    public DateTime UploadedAt { get; set; }
    public string? Description { get; set; }
    public string? Tags { get; set; }
}

public class UploadFileDto
{
    public string FileName { get; set; } = string.Empty;
    public string Base64Content { get; set; } = string.Empty; // Base64 encoded file content
    public string ContentType { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string? Description { get; set; }
    public string? Tags { get; set; }
}

public class UploadResultDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public FileAttachmentDto? FileAttachment { get; set; }
}