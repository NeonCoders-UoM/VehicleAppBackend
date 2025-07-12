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

                if (totalSize + incomingFileSize > 10 * 1024 * 1024)
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
                FileSize = file.Length
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

        [HttpGet("download")]
        public async Task<IActionResult> Download([FromQuery] string fileUrl)
        {
            try
            {
                var stream = await _blobService.DownloadFileAsync(fileUrl);
                var fileName = Path.GetFileName(new Uri(fileUrl).LocalPath);

                // Return the file with headers for downloading
                return File(stream, "application/octet-stream", fileName);
            }
            catch (FileNotFoundException)
            {
                return NotFound("File not found");
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

            bool deleted = await _blobService.DeleteFileAsync(document.FileUrl);
            if (!deleted)
                return StatusCode(500, "Error deleting file from storage");

            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();

            return Ok("Document deleted");
        }
    }
}
