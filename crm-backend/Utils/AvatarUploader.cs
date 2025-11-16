using Amazon.S3;
using Amazon.S3.Model;

namespace crm_backend.Utils;

public class AvatarUploader
{
    public static async Task<string> UploadDefaultAvatarAsync()
    {
        var accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
        var secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
        var bucketName = Environment.GetEnvironmentVariable("AWS_BUCKET_NAME") ?? "4wk-garage-media";
        var region = Environment.GetEnvironmentVariable("AWS_REGION") ?? "me-central-1";

        var config = new AmazonS3Config
        {
            RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region)
        };

        using var s3Client = new AmazonS3Client(accessKey, secretKey, config);

        try
        {
            var avatarPath = "/Users/husain/Desktop/cross/crm-backend/6596121.webp";
            var fileBytes = await File.ReadAllBytesAsync(avatarPath);
            var s3Key = "crm-assets/default-avatar.webp";

            using var stream = new MemoryStream(fileBytes);

            var putRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = s3Key,
                InputStream = stream,
                ContentType = "image/webp",
                CannedACL = S3CannedACL.PublicRead // Public so we can use it as default avatar
            };

            await s3Client.PutObjectAsync(putRequest);

            // Return the public URL
            return $"https://{bucketName}.s3.{region}.amazonaws.com/{s3Key}";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to upload default avatar: {ex.Message}");
            // Fallback to dicebear if S3 upload fails
            return "https://api.dicebear.com/7.x/initials/svg?seed=default&backgroundColor=random";
        }
    }
}
