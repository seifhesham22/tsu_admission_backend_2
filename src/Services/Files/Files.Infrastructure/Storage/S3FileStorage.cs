using Amazon.S3.Model;
using Amazon.S3;
using Files.Application.Persistence.Contracts;
using Files.Application.Storage.Contracts;
using Microsoft.Extensions.Options;

namespace Files.Infrastructure.Storage;

public sealed class S3FileStorage : IFileStorage
{
    private readonly IAmazonS3 _s3;
    private readonly S3Options _options;

    public S3FileStorage(IAmazonS3 s3, IOptions<S3Options> options)
    {
        _s3 = s3;
        _options = options.Value;
    }

    public async Task UploadAsync(
        string key,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        await _s3.PutObjectAsync(
            new PutObjectRequest
            {
                BucketName = _options.BucketName,
                Key = key,
                InputStream = content,
                ContentType = contentType,
                AutoCloseStream = false
            },
            cancellationToken);
    }

    public async Task<Stream> DownloadAsync(string key, CancellationToken cancellationToken = default)
    {
        var response = await _s3.GetObjectAsync(_options.BucketName, key, cancellationToken);
        return response.ResponseStream;
    }

    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        await _s3.DeleteObjectAsync(
            new DeleteObjectRequest
            {
                BucketName = _options.BucketName,
                Key = key
            },
            cancellationToken);
    }
}
