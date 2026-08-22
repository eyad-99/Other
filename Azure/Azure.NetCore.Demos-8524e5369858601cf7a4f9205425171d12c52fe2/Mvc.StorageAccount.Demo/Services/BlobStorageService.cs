using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Mvc.StorageAccount.Demo.Services;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName = "attendeeimages";

    public BlobStorageService(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    private async Task<BlobContainerClient> GetContainerAsync()
    {
        var container = _blobServiceClient.GetBlobContainerClient(_containerName);
        await container.CreateIfNotExistsAsync();
        return container;
    }

    public async Task<string> UploadBlob(IFormFile formFile, string imageName, string? originalBlobName = null)
    {
        var blobName = $"{imageName}{Path.GetExtension(formFile.FileName)}";
        var container = await GetContainerAsync();

        if (!string.IsNullOrEmpty(originalBlobName))
        {
            await RemoveBlob(originalBlobName);
        }

        using var memoryStream = new MemoryStream();
        await formFile.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        var blob = container.GetBlobClient(blobName);
        await blob.UploadAsync(memoryStream, overwrite: true);

        return blobName;
    }

    public async Task<string> GetBlobUrl(string imageName)
    {
        var container = await GetContainerAsync();
        var blob = container.GetBlobClient(imageName);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = blob.BlobContainerName,
            BlobName = blob.Name,
            ExpiresOn = DateTime.UtcNow.AddMinutes(2),
            Protocol = SasProtocol.Https,
            Resource = "b"
        };
        sasBuilder.SetPermissions(BlobAccountSasPermissions.Read);

        return blob.GenerateSasUri(sasBuilder).ToString();
    }

    public async Task RemoveBlob(string imageName)
    {
        var container = await GetContainerAsync();
        var blob = container.GetBlobClient(imageName);
        await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
    }
}
