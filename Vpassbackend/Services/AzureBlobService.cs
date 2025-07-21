using Azure.Storage;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

namespace Vpassbackend.Services
{
    public class AzureBlobService
    {
        private readonly BlobContainerClient _containerClient;
        public StorageSharedKeyCredential Credential { get; }

        public AzureBlobService(IConfiguration configuration)
        {
            var connectionString = configuration["AzureBlobStorage:ConnectionString"];
            var containerName = configuration["AzureBlobStorage:ContainerName"];

            // Parse AccountName and AccountKey from the connection string
            var accountName = ParseConnectionStringValue(connectionString, "AccountName");
            var accountKey = ParseConnectionStringValue(connectionString, "AccountKey");

            if (accountName == null || accountKey == null)
            {
                throw new InvalidOperationException("AccountName or AccountKey missing in connection string.");
            }

            Credential = new StorageSharedKeyCredential(accountName, accountKey);

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

            // Sanitize filename to replace spaces and special characters
            fileName = Regex.Replace(fileName, @"[^a-zA-Z0-9._-]", "_");

            if (documentType.ToLower() == "warrantydocument")
            {
                var timestamp = DateTime.UtcNow.Ticks;
                fileName = $"warranty_{timestamp}_{fileName}";
            }
            else
            {
                var extension = Path.GetExtension(fileName);
                if (string.IsNullOrEmpty(extension))
                {
                    extension = ".pdf";
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

                var uri = new Uri(fileUrl);
                string fullPath = uri.AbsolutePath.TrimStart('/');

                // Ensure exact blob path by removing container name prefix
                if (fullPath.StartsWith(_containerClient.Name + "/"))
                {
                    fullPath = fullPath.Substring(_containerClient.Name.Length + 1); // +1 for '/'
                }

                string blobName = Uri.UnescapeDataString(fullPath); // Correct for encoded characters

                Console.WriteLine("Corrected blob name: " + blobName);

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
            // Decode the URL to handle encoded characters like %20
            string decodedPath = Uri.UnescapeDataString(uri.AbsolutePath);
            string blobName = decodedPath.TrimStart('/').Replace($"{_containerClient.Name}/", "");

            Console.WriteLine($"Corrected blob name: {blobName}");

            var blobClient = _containerClient.GetBlobClient(blobName);

            if (await blobClient.ExistsAsync())
            {
                var response = await blobClient.DownloadAsync();
                return response.Value.Content;
            }

            throw new FileNotFoundException($"File not found in blob storage: {blobName}");
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

        private string? ParseConnectionStringValue(string connectionString, string key)
        {
            var regex = new Regex($"{key}=([^;]+)", RegexOptions.IgnoreCase);
            var match = regex.Match(connectionString);
            return match.Success ? match.Groups[1].Value : null;
        }
    }
}
