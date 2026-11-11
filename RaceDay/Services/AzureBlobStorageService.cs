using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.Services;

public sealed class AzureBlobStorageService(IConfiguration configuration) : IBlobStorageService
{
    private BlobServiceClient CreateClient()
    {
        var connectionString = configuration["AzureBlobStorage:ConnectionString"];
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "Azure Blob Storage is not configured. Set AzureBlobStorage:ConnectionString using user secrets or environment variables.");

        return new BlobServiceClient(connectionString);
    }

    public async Task<string> UploadAsync(
        Stream content, string fileName, string contentType, string containerName)
    {
        var container = CreateClient().GetBlobContainerClient(containerName);
        await container.CreateIfNotExistsAsync(PublicAccessType.None);

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var blobName = $"{Guid.NewGuid():N}{extension}";
        var blob = container.GetBlobClient(blobName);
        await blob.UploadAsync(content, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        });

        return blobName;
    }

    public async Task<(Stream Content, string ContentType)?> OpenReadAsync(
        string blobName, string containerName)
    {
        var blob = CreateClient().GetBlobContainerClient(containerName).GetBlobClient(blobName);
        if (!await blob.ExistsAsync()) return null;

        var download = await blob.DownloadStreamingAsync();
        return (download.Value.Content, download.Value.Details.ContentType ?? "application/octet-stream");
    }
}
