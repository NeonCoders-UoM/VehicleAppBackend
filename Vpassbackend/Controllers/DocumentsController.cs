using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Vpassbackend.Models;
using Vpassbackend.Services;
using Vpassbackend.Data;
using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using Vpassbackend.DTOs;


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
        public async Task<IActionResult> Upload([FromForm] DocumentUploadRequest dto)
        {
            if (dto.File == null || dto.File.Length == 0)
                return BadRequest("File is required");

            if (!Enum.IsDefined(typeof(DocumentType), dto.DocumentType))
                return BadRequest("Invalid document type");

            var docTypeEnum = (DocumentType)dto.DocumentType;

            if (DocumentTypeHelper.HasExpiration(docTypeEnum) && dto.ExpirationDate == null)
                return BadRequest("Expiration date is required for this document type");

            if (!DocumentTypeHelper.HasExpiration(docTypeEnum))
                dto.ExpirationDate = null;

            if (docTypeEnum == DocumentType.DriversLicense)
            {
                var existingDriversLicense = await _context.Documents
                    .AnyAsync(d => d.CustomerId == dto.CustomerId && d.DocumentType == DocumentType.DriversLicense);
                if (existingDriversLicense)
                    return BadRequest("Customer already has a Driver's License.");
            }

            if (dto.VehicleId.HasValue && docTypeEnum != DocumentType.WarrantyDocument)
            {
                var existingDocument = await _context.Documents
                    .AnyAsync(d => d.VehicleId == dto.VehicleId && d.DocumentType == docTypeEnum);
                if (existingDocument)
                    return BadRequest($"Vehicle already has a {docTypeEnum}.");
            }

            if (docTypeEnum == DocumentType.WarrantyDocument && dto.VehicleId.HasValue)
            {
                var existingDocs = await _context.Documents
                    .Where(d => d.DocumentType == DocumentType.WarrantyDocument && d.VehicleId == dto.VehicleId)
                    .ToListAsync();

                long totalSize = existingDocs.Sum(d => d.FileSize);
                long incomingFileSize = dto.File.Length;

                if (totalSize + incomingFileSize > 10 * 1024 * 1024) // 10 MB
                {
                    return BadRequest("Warranty document storage limit (10MB) exceeded for this vehicle.");
                }
            }

            using var stream = dto.File.OpenReadStream();
            string url;
            try
            {
                url = await _blobService.UploadFileAsync(
                    stream, dto.File.FileName, dto.CustomerId, dto.VehicleId, docTypeEnum.ToString());

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
                FileName = dto.File.FileName,
                FileUrl = url,
                CustomerId = dto.CustomerId,
                VehicleId = dto.VehicleId,
                DocumentType = docTypeEnum,
                UploadedAt = DateTime.UtcNow,
                ExpirationDate = dto.ExpirationDate,
                DisplayName = dto.DisplayName,
                FileSize = dto.File.Length,
                ContentType = dto.File.ContentType
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