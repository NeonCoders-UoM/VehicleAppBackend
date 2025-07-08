using Microsoft.AspNetCore.Mvc;
using Vpassbackend.DTOs;
using Vpassbackend.Services;

namespace Vpassbackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly IWebHostEnvironment _environment;

        public DocumentsController(IDocumentService documentService, IWebHostEnvironment environment)
        {
            _documentService = documentService;
            _environment = environment;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DocumentDto>>> GetDocuments()
        {
            var documents = await _documentService.GetAllDocumentsAsync();
            return Ok(documents);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentDto>> GetDocument(int id)
        {
            var document = await _documentService.GetDocumentByIdAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            return Ok(document);
        }

        [HttpGet("vehicle/{vehicleId}")]
        public async Task<ActionResult<IEnumerable<DocumentDto>>> GetDocumentsByVehicleId(int vehicleId)
        {
            var documents = await _documentService.GetDocumentsByVehicleIdAsync(vehicleId);
            return Ok(documents);
        }

        [HttpPost]
        public async Task<ActionResult<DocumentDto>> CreateDocument([FromForm] int vehicleId, [FromForm] string? documentType,
            [FromForm] DateTime? expiryDate, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            // Create documents directory if it doesn't exist
            var documentsFolder = Path.Combine(_environment.ContentRootPath, "documents");
            if (!Directory.Exists(documentsFolder))
            {
                Directory.CreateDirectory(documentsFolder);
            }

            // Generate unique file name
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(documentsFolder, fileName);

            // Save file
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Create document record
            var documentCreateDto = new DocumentCreateDto
            {
                VehicleId = vehicleId,
                DocumentType = documentType,
                ExpiryDate = expiryDate,
                FilePath = $"/documents/{fileName}"
            };

            var document = await _documentService.CreateDocumentAsync(documentCreateDto);
            return CreatedAtAction(nameof(GetDocument), new { id = document.DocumentId }, document);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<DocumentDto>> UpdateDocument(int id, DocumentUpdateDto documentUpdateDto)
        {
            var document = await _documentService.UpdateDocumentAsync(id, documentUpdateDto);
            if (document == null)
            {
                return NotFound();
            }

            return Ok(document);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteDocument(int id)
        {
            var document = await _documentService.GetDocumentByIdAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            // Delete file if it exists
            if (!string.IsNullOrEmpty(document.FilePath))
            {
                var filePath = Path.Combine(_environment.ContentRootPath, document.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            var result = await _documentService.DeleteDocumentAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
