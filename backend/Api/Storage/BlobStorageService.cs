using AdSPMdS.DemanioDigitale.Api.Options;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;

namespace AdSPMdS.DemanioDigitale.Api.Storage;

public class BlobStorageService
{
    private readonly BlobContainerClient _container;

    public BlobStorageService(IOptions<StorageOptions> options)
    {
        var config = options.Value;
        var serviceClient = new BlobServiceClient(config.ConnectionString);
        _container = serviceClient.GetBlobContainerClient(config.ContainerName);
    }

    public async Task<BlobItemDto> UploadAsync(
        IFormFile file,
        string? prefix,
        CancellationToken cancellationToken)
    {
        await _container.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);

        var extension = Path.GetExtension(file.FileName);
        var safePrefix = string.IsNullOrWhiteSpace(prefix)
            ? string.Empty
            : $"{prefix.Trim('/').Replace('\\', '/')}/";
        var blobName = $"{safePrefix}{Guid.NewGuid():N}{extension}";
        var client = _container.GetBlobClient(blobName);

        await using var stream = file.OpenReadStream();
        await client.UploadAsync(stream, new BlobHttpHeaders
        {
            ContentType = string.IsNullOrWhiteSpace(file.ContentType)
                ? "application/octet-stream"
                : file.ContentType
        }, cancellationToken: cancellationToken);

        var properties = await client.GetPropertiesAsync(cancellationToken: cancellationToken);

        return new BlobItemDto(
            client.Name,
            properties.Value.ContentLength,
            properties.Value.LastModified);
    }

    public async Task<IReadOnlyList<BlobItemDto>> ListAsync(
        string? prefix,
        CancellationToken cancellationToken)
    {
        var results = new List<BlobItemDto>();
        await foreach (var blob in _container.GetBlobsAsync(prefix: prefix, cancellationToken: cancellationToken))
        {
            results.Add(new BlobItemDto(
                blob.Name,
                blob.Properties.ContentLength ?? 0,
                blob.Properties.LastModified));
        }

        return results;
    }

    public async Task<BlobDownloadResult?> DownloadAsync(
        string blobName,
        CancellationToken cancellationToken)
    {
        var client = _container.GetBlobClient(blobName);
        if (!await client.ExistsAsync(cancellationToken))
        {
            return null;
        }

        var response = await client.DownloadStreamingAsync(cancellationToken: cancellationToken);
        var contentType = response.Value.Details.ContentType ?? "application/octet-stream";
        var fileName = Path.GetFileName(blobName);

        return new BlobDownloadResult(response.Value.Content, contentType, fileName);
    }
}

public record BlobItemDto(string Name, long Size, DateTimeOffset? LastModified);

public record BlobDownloadResult(Stream Stream, string ContentType, string FileName);
