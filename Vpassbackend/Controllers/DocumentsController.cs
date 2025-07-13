using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Vpassbackend.Models;
using Vpassbackend.Services;
using Vpassbackend.Data;  


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

            if (docTypeEnum == DocumentType.WarrantyDocument && vehicleId.HasValue)
            {
                var existingDocs = _context.Documents
                    .Where(d => d.DocumentType == DocumentType.WarrantyDocument && d.VehicleId == vehicleId)
                    .ToList();

                long totalSize = existingDocs.Sum(d => d.FileSize);
                long incomingFileSize = file.Length;

                if (totalSize + incomingFileSize > 10 * 1024 * 1024) // 10 MB limit
                {
                    return BadRequest("Warranty document storage limit (10MB) exceeded for this vehicle.");
                }
            }

            using var stream = file.OpenReadStream();
            var url = await _blobService.UploadFileAsync(stream, file.FileName, customerId, vehicleId, docTypeEnum.ToString());

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
                ContentType = file.ContentType  // Store the MIME/content type here
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
        public async Task<IActionResult> Download([FromQuery] string fileUrl)
        {
            // Find document metadata by fileUrl in the database
            var document = _context.Documents.FirstOrDefault(d => d.FileUrl == fileUrl);
            if (document == null)
                return NotFound("Document metadata not found");

            try
            {
                var stream = await _blobService.DownloadFileAsync(fileUrl);
                var contentType = string.IsNullOrEmpty(document.ContentType) ? "application/octet-stream" : document.ContentType;
                var fileName = document.FileName ?? Path.GetFileName(new Uri(fileUrl).LocalPath);

                // Return file stream with correct content type and filename for download
                return File(stream, contentType, fileName);
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
        [HttpGet("preview-url")]
        public async Task<IActionResult> GetPreviewUrl([FromQuery] string fileUrl)
        {
            var document = _context.Documents.FirstOrDefault(d => d.FileUrl == fileUrl);
            if (document == null)
                return NotFound("Document not found");

            try
            {
                var sasUrl = await _blobService.GeneratePreviewUrlAsync(fileUrl);
                return Ok(new { previewUrl = sasUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to generate preview URL: {ex.Message}");
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
                // Attempt to delete from Azure Blob
                bool deletedFromCloud = await _blobService.DeleteFileAsync(document.FileUrl);

                if (!deletedFromCloud)
                {
                    return StatusCode(500, "File not found or not deleted in Azure Storage");
                }

                // Delete from database if successfully deleted from cloud
                _context.Documents.Remove(document);
                await _context.SaveChangesAsync();

                return Ok("Document deleted successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while deleting the document: {ex.Message}");
            }
        }

    }
}
