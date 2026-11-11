namespace RaceDay.Services;

public interface IBlobStorageService
{
    Task<string> UploadAsync(Stream content, string fileName, string contentType, string containerName);
    Task<(Stream Content, string ContentType)?> OpenReadAsync(string blobName, string containerName);
}
