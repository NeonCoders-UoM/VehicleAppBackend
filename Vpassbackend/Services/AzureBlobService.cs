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
                // Preserve original extension from fileName
                var extension = Path.GetExtension(fileName);
                if (string.IsNullOrEmpty(extension))
                {
                    extension = ".pdf"; // fallback if no extension found
                }
                fileName = $"{documentType.ToLower()}{extension}";
            }

            string blobName = path + fileName;
            var blobClient = _containerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(fileStream, overwrite: true);
            return blobClient.Uri.ToString();
        }


        public async Task<bool> DeleteFileAsync(string fileUrl)
        {
            try
            {
                Console.WriteLine($"Requested to delete: {fileUrl}");

                Uri uri = new Uri(fileUrl);
                string blobName = uri.AbsolutePath.TrimStart('/').Replace(_containerClient.Name + "/", "");
                Console.WriteLine("Corrected blob name: " + blobName);

                Console.WriteLine($"Extracted blob name: {blobName}");

                var blobClient = _containerClient.GetBlobClient(blobName);
                var response = await blobClient.DeleteIfExistsAsync();
                Console.WriteLine($"Delete response: {response.Value}");

                return response.Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting blob: {ex.Message}");
                return false;
            }
        }


        public async Task<Stream> DownloadFileAsync(string fileUrl)
        {
            Uri uri = new Uri(fileUrl);

            // Remove "/vdocuments/" or any container name from path
            string blobName = uri.AbsolutePath.TrimStart('/').Replace($"{_containerClient.Name}/", "");

            Console.WriteLine($"Corrected blob name: {blobName}");

            var blobClient = _containerClient.GetBlobClient(blobName);

            if (await blobClient.ExistsAsync())
            {
                var response = await blobClient.DownloadAsync();
                return response.Value.Content;
            }

            throw new FileNotFoundException("File not found in blob storage");
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
