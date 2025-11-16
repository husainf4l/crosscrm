using crm_backend.Modules.Customer.DTOs;

namespace crm_backend.Modules.Customer.Services;

public interface IFileUploadService
{
    Task<UploadResultDto> UploadFileAsync(UploadFileDto uploadDto, int uploadedByUserId);
    Task<bool> DeleteFileAsync(int fileId, int userId);
    Task<FileAttachmentDto?> GetFileAsync(int fileId, int userId);
    Task<IEnumerable<FileAttachmentDto>> GetFilesByCustomerAsync(int customerId, int userId);
}

public interface IS3Service
{
    Task<string> UploadFileAsync(string fileName, byte[] fileContent, string contentType);
    Task<bool> DeleteFileAsync(string s3Key);
    Task<string> GeneratePresignedUrlAsync(string s3Key, int expirationMinutes = 60);
}
