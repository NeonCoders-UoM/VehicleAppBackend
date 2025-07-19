using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Vpassbackend.Models;
using Vpassbackend.Services;
using Vpassbackend.Data;
using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;

namespace Vpassbackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly AzureBlobService _blobService;
        private readonly ApplicationDbContext _context;

        public DocumentsController(AzureBlobService blobService, ApplicationDbContext context)
        {
            _blobService = blobService;
            _context = context;
        }

        [HttpGet("ping")]
        public IActionResult Ping() => Ok("API is alive");

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(
            [FromForm] IFormFile file,
            [FromForm] int customerId,
            [FromForm] int documentType,
            [FromForm] int? vehicleId,
            [FromForm] DateTime? expirationDate,
            [FromForm] string? displayName)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is required");

            if (!Enum.IsDefined(typeof(DocumentType), documentType))
                return BadRequest("Invalid document type");

            var docTypeEnum = (DocumentType)documentType;

            if (DocumentTypeHelper.HasExpiration(docTypeEnum) && expirationDate == null)
                return BadRequest("Expiration date is required for this document type");

            if (!DocumentTypeHelper.HasExpiration(docTypeEnum))
                expirationDate = null;

            // Validate customer-level constraint: only one DriversLicense per customer
            if (docTypeEnum == DocumentType.DriversLicense)
            {
                var existingDriversLicense = await _context.Documents
                    .AnyAsync(d => d.CustomerId == customerId && d.DocumentType == DocumentType.DriversLicense);
                if (existingDriversLicense)
                    return BadRequest("Customer already has a Driver's License.");
            }

            // Validate vehicle-level constraints: only one of each document type (except WarrantyDocument)
            if (vehicleId.HasValue && docTypeEnum != DocumentType.WarrantyDocument)
            {
                var existingDocument = await _context.Documents
                    .AnyAsync(d => d.VehicleId == vehicleId && d.DocumentType == docTypeEnum);
                if (existingDocument)
                    return BadRequest($"Vehicle already has a {docTypeEnum}.");
            }

            // Validate WarrantyDocument size limit
            if (docTypeEnum == DocumentType.WarrantyDocument && vehicleId.HasValue)
            {
                var existingDocs = await _context.Documents
                    .Where(d => d.DocumentType == DocumentType.WarrantyDocument && d.VehicleId == vehicleId)
                    .ToListAsync();

                long totalSize = existingDocs.Sum(d => d.FileSize);
                long incomingFileSize = file.Length;

                if (totalSize + incomingFileSize > 10 * 1024 * 1024) // 10 MB limit
                {
                    return BadRequest("Warranty document storage limit (10MB) exceeded for this vehicle.");
                }
            }

            using var stream = file.OpenReadStream();
            string url;
            try
            {
                url = await _blobService.UploadFileAsync(stream, file.FileName, customerId, vehicleId, docTypeEnum.ToString());
                // Verify blob exists
                var blobClient = new BlobClient(new Uri(url), _blobService.Credential);
                if (!await blobClient.ExistsAsync())
                {
                    Console.WriteLine($"Blob upload failed verification for {docTypeEnum}: {url}");
                    return StatusCode(500, $"Failed to verify blob upload for {docTypeEnum}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Blob upload failed for {docTypeEnum}: {ex.Message}");
                return StatusCode(500, $"Failed to upload blob: {ex.Message}");
            }

            var document = new Document
            {
                FileName = file.FileName,
                FileUrl = url,
                CustomerId = customerId,
                VehicleId = vehicleId,
                DocumentType = docTypeEnum,
                UploadedAt = DateTime.UtcNow,
                ExpirationDate = expirationDate,
                DisplayName = displayName,
                FileSize = file.Length,
                ContentType = file.ContentType
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            return Ok(document);
        }

        [HttpGet("list/{customerId}")]
        public IActionResult List(int customerId)
        {
            var documents = _context.Documents.Where(d => d.CustomerId == customerId);
            return Ok(documents);
        }

        [HttpGet("listByVehicle/{customerId}/{vehicleId}")]
        public IActionResult ListByVehicle(int customerId, int vehicleId)
        {
            var documents = _context.Documents
                .Where(d => d.CustomerId == customerId && d.VehicleId == vehicleId);
            return Ok(documents);
        }

        [HttpGet("download")]
        public async Task<IActionResult> Download([FromQuery] string fileUrl, [FromQuery] string mode = "inline")
        {
            if (mode != "inline" && mode != "attachment")
            {
                return BadRequest("Invalid mode. Use 'inline' for preview or 'attachment' for download.");
            }

            var document = _context.Documents.FirstOrDefault(d => d.FileUrl == fileUrl);
            if (document == null)
                return NotFound("Document metadata not found");

            try
            {
                var stream = await _blobService.DownloadFileAsync(fileUrl);
                var fileName = document.FileName ?? Path.GetFileName(new Uri(fileUrl).LocalPath);

                var fileExtension = Path.GetExtension(fileName).ToLower();
                var contentType = string.IsNullOrEmpty(document.ContentType) || document.ContentType == "application/octet-stream"
                    ? fileExtension switch
                    {
                        ".pdf" => "application/pdf",
                        ".jpg" => "image/jpeg",
                        ".jpeg" => "image/jpeg",
                        ".png" => "image/png",
                        ".txt" => "text/plain",
                        _ => "application/octet-stream"
                    }
                    : document.ContentType;

                Response.Headers.Add("Content-Disposition", $"{mode}; filename=\"{fileName}\"; filename*=UTF-8''{Uri.EscapeDataString(fileName)}");

                return File(stream, contentType);
            }
            catch (FileNotFoundException)
            {
                return NotFound("File not found in blob storage");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error downloading file: {ex.Message}");
            }
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> Delete([FromQuery] int documentId)
        {
            var document = await _context.Documents.FindAsync(documentId);
            if (document == null)
                return NotFound("Document not found");

            try
            {
                bool deletedFromCloud = await _blobService.DeleteFileAsync(document.FileUrl);
                if (!deletedFromCloud)
                {
                    Console.WriteLine($"Blob not found or not deleted for DocumentId={documentId}, DocumentType={document.DocumentType}: {document.FileUrl}");
                }

                _context.Documents.Remove(document);
                await _context.SaveChangesAsync();

                return Ok("Document deleted successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting document {documentId}: {ex.Message}");
                return StatusCode(500, $"An error occurred while deleting the document: {ex.Message}");
            }
        }
    }
}