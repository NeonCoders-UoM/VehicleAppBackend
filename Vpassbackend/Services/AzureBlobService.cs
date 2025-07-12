using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;

namespace Vpassbackend.Services
{
    public class AzureBlobService
    {
        private readonly BlobContainerClient _containerClient;

        public AzureBlobService(IConfiguration configuration)
        {
            var connectionString = configuration["AzureBlobStorage:ConnectionString"];
            var containerName = configuration["AzureBlobStorage:ContainerName"];
            var blobServiceClient = new BlobServiceClient(connectionString);
            _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, int customerId, int? vehicleId, string documentType)
        {
            string path = $"documents/customer_{customerId}/";

            if (vehicleId != null)
                path += $"vehicle_{vehicleId}/";
            else
                path += "common/";

            if (documentType.ToLower() == "warrantydocument")
            {
                var timestamp = DateTime.UtcNow.Ticks;
                fileName = $"warranty_{timestamp}_{fileName}";
            }
            else
            {
                fileName = $"{documentType.ToLower()}.pdf";
            }

            string blobName = path + fileName;
            var blobClient = _containerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(fileStream, overwrite: true);
            return blobClient.Uri.ToString();
        }

        public async Task<bool> DeleteFileAsync(string fileUrl)
        {
            var blobClient = new BlobClient(new Uri(fileUrl));
            var response = await blobClient.DeleteIfExistsAsync();
            return response.Value;
        }

        public async Task<Stream> DownloadFileAsync(string fileUrl)
        {
            var blobClient = new BlobClient(new Uri(fileUrl));
            var response = await blobClient.DownloadAsync();
            return response.Value.Content;
        }

        public async Task<List<string>> ListAllFilesAsync()
        {
            var files = new List<string>();
            await foreach (var blobItem in _containerClient.GetBlobsAsync())
            {
                files.Add(blobItem.Name);
            }
            return files;
        }
    }
}
