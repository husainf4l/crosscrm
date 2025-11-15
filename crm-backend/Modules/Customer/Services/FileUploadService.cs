using crm_backend.Data;
using crm_backend.Modules.Customer.DTOs;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Customer.Services;

public class FileUploadService : IFileUploadService
{
    private readonly CrmDbContext _context;
    private readonly IS3Service _s3Service;

    public FileUploadService(CrmDbContext context, IS3Service s3Service)
    {
        _context = context;
        _s3Service = s3Service;
    }

    public async Task<UploadResultDto> UploadFileAsync(UploadFileDto uploadDto, int uploadedByUserId)
    {
        try
        {
            // Validate customer exists and user has access
            var customer = await _context.Customers.FindAsync(uploadDto.CustomerId);
            if (customer == null)
            {
                return new UploadResultDto
                {
                    Success = false,
                    Message = "Customer not found"
                };
            }

            // Decode base64 content
            byte[] fileContent;
            try
            {
                // Remove data URL prefix if present (e.g., "data:image/png;base64,")
                var base64Data = uploadDto.Base64Content;
                if (base64Data.Contains(','))
                {
                    base64Data = base64Data.Split(',')[1];
                }

                fileContent = Convert.FromBase64String(base64Data);
            }
            catch (Exception)
            {
                return new UploadResultDto
                {
                    Success = false,
                    Message = "Invalid base64 content"
                };
            }

            // Validate file size (max 10MB)
            if (fileContent.Length > 10 * 1024 * 1024)
            {
                return new UploadResultDto
                {
                    Success = false,
                    Message = "File size exceeds maximum limit of 10MB"
                };
            }

            // Upload to S3
            var s3Key = await _s3Service.UploadFileAsync(uploadDto.FileName, fileContent, uploadDto.ContentType);

            // Generate pre-signed URL for access
            var s3Url = await _s3Service.GeneratePresignedUrlAsync(s3Key);

            // Save file metadata to database
            var fileAttachment = new FileAttachment
            {
                FileName = uploadDto.FileName,
                OriginalFileName = uploadDto.FileName,
                ContentType = uploadDto.ContentType,
                FileSize = fileContent.Length,
                S3Key = s3Key,
                S3Url = s3Url,
                CustomerId = uploadDto.CustomerId,
                UploadedByUserId = uploadedByUserId,
                Description = uploadDto.Description,
                Tags = uploadDto.Tags,
                UploadedAt = DateTime.UtcNow
            };

            _context.FileAttachments.Add(fileAttachment);
            await _context.SaveChangesAsync();

            var result = await GetFileAsync(fileAttachment.Id, uploadedByUserId);

            return new UploadResultDto
            {
                Success = true,
                Message = "File uploaded successfully",
                FileAttachment = result
            };
        }
        catch (Exception ex)
        {
            return new UploadResultDto
            {
                Success = false,
                Message = $"Upload failed: {ex.Message}"
            };
        }
    }

    public async Task<bool> DeleteFileAsync(int fileId, int userId)
    {
        var fileAttachment = await _context.FileAttachments.FindAsync(fileId);
        if (fileAttachment == null)
        {
            return false;
        }

        // Only allow the uploader to delete their files
        if (fileAttachment.UploadedByUserId != userId)
        {
            throw new UnauthorizedAccessException("You can only delete your own uploaded files");
        }

        // Delete from S3
        await _s3Service.DeleteFileAsync(fileAttachment.S3Key);

        // Delete from database
        _context.FileAttachments.Remove(fileAttachment);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<FileAttachmentDto?> GetFileAsync(int fileId, int userId)
    {
        var file = await _context.FileAttachments
            .Include(f => f.Customer)
            .Include(f => f.UploadedByUser)
            .FirstOrDefaultAsync(f => f.Id == fileId);

        if (file == null)
        {
            return null;
        }

        // Check if user has access to this customer's files
        var user = await _context.Users.FindAsync(userId);
        if (user == null || user.CompanyId != file.Customer.CompanyId)
        {
            return null; // No access
        }

        return MapToDto(file);
    }

    public async Task<IEnumerable<FileAttachmentDto>> GetFilesByCustomerAsync(int customerId, int userId)
    {
        // First verify user has access to this customer
        var customer = await _context.Customers.FindAsync(customerId);
        if (customer == null)
        {
            return new List<FileAttachmentDto>();
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null || user.CompanyId != customer.CompanyId)
        {
            return new List<FileAttachmentDto>();
        }

        var files = await _context.FileAttachments
            .Include(f => f.Customer)
            .Include(f => f.UploadedByUser)
            .Where(f => f.CustomerId == customerId)
            .OrderByDescending(f => f.UploadedAt)
            .ToListAsync();

        return files.Select(MapToDto);
    }

    private static FileAttachmentDto MapToDto(FileAttachment file)
    {
        return new FileAttachmentDto
        {
            Id = file.Id,
            FileName = file.FileName,
            OriginalFileName = file.OriginalFileName,
            ContentType = file.ContentType,
            FileSize = file.FileSize,
            S3Url = file.S3Url,
            CustomerId = file.CustomerId,
            CustomerName = file.Customer?.Name,
            UploadedByUserId = file.UploadedByUserId,
            UploadedByUserName = file.UploadedByUser?.Name,
            UploadedAt = file.UploadedAt,
            Description = file.Description,
            Tags = file.Tags
        };
    }
}