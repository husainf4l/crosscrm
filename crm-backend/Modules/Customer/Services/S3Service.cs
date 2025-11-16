using Amazon.S3;
using Amazon.S3.Model;

namespace crm_backend.Modules.Customer.Services;

public class S3Service : IS3Service
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _region;

    public S3Service()
    {
        var accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
        var secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
        _bucketName = Environment.GetEnvironmentVariable("AWS_BUCKET_NAME") ?? "4wk-garage-media";
        _region = Environment.GetEnvironmentVariable("AWS_REGION") ?? "me-central-1";

        var config = new AmazonS3Config
        {
            RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(_region)
        };

        _s3Client = new AmazonS3Client(accessKey, secretKey, config);
    }

    public async Task<string> UploadFileAsync(string fileName, byte[] fileContent, string contentType)
    {
        try
        {
            // Generate unique S3 key
            var s3Key = $"crm-attachments/{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid()}_{fileName}";

            using var stream = new MemoryStream(fileContent);

            var putRequest = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = s3Key,
                InputStream = stream,
                ContentType = contentType,
                CannedACL = S3CannedACL.Private // Files are private, accessed via pre-signed URLs
            };

            await _s3Client.PutObjectAsync(putRequest);

            return s3Key;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to upload file to S3: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteFileAsync(string s3Key)
    {
        try
        {
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = s3Key
            };

            var response = await _s3Client.DeleteObjectAsync(deleteRequest);
            return response.HttpStatusCode == System.Net.HttpStatusCode.NoContent;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to delete file from S3: {ex.Message}", ex);
        }
    }

    public async Task<string> GeneratePresignedUrlAsync(string s3Key, int expirationMinutes = 60)
    {
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = s3Key,
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                Protocol = Protocol.HTTPS
            };

            return _s3Client.GetPreSignedURL(request);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to generate pre-signed URL: {ex.Message}", ex);
        }
    }
}
