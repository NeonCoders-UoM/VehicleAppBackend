using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Vpassbackend.Data;
using Vpassbackend.DTOs;
using Vpassbackend.Models;

namespace Vpassbackend.Services
{
    public interface IDocumentService
    {
        Task<IEnumerable<DocumentDto>> GetAllDocumentsAsync();
        Task<DocumentDto?> GetDocumentByIdAsync(int id);
        Task<IEnumerable<DocumentDto>> GetDocumentsByVehicleIdAsync(int vehicleId);
        Task<DocumentDto> CreateDocumentAsync(DocumentCreateDto documentCreateDto);
        Task<DocumentDto?> UpdateDocumentAsync(int id, DocumentUpdateDto documentUpdateDto);
        Task<bool> DeleteDocumentAsync(int id);
    }

    public class DocumentService : IDocumentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public DocumentService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<DocumentDto>> GetAllDocumentsAsync()
        {
            var documents = await _context.Documents
                .Include(d => d.Vehicle)
                .ToListAsync();
            return _mapper.Map<IEnumerable<DocumentDto>>(documents);
        }

        public async Task<DocumentDto?> GetDocumentByIdAsync(int id)
        {
            var document = await _context.Documents
                .Include(d => d.Vehicle)
                .FirstOrDefaultAsync(d => d.DocumentId == id);

            if (document == null)
            {
                return null;
            }

            return _mapper.Map<DocumentDto>(document);
        }

        public async Task<IEnumerable<DocumentDto>> GetDocumentsByVehicleIdAsync(int vehicleId)
        {
            var documents = await _context.Documents
                .Where(d => d.VehicleId == vehicleId)
                .Include(d => d.Vehicle)
                .ToListAsync();

            return _mapper.Map<IEnumerable<DocumentDto>>(documents);
        }

        public async Task<DocumentDto> CreateDocumentAsync(DocumentCreateDto documentCreateDto)
        {
            var document = _mapper.Map<Document>(documentCreateDto);

            // Set upload date to current UTC time
            document.UploadDate = DateTime.UtcNow;

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            return _mapper.Map<DocumentDto>(document);
        }

        public async Task<DocumentDto?> UpdateDocumentAsync(int id, DocumentUpdateDto documentUpdateDto)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return null;
            }

            _mapper.Map(documentUpdateDto, document);
            await _context.SaveChangesAsync();

            return _mapper.Map<DocumentDto>(document);
        }

        public async Task<bool> DeleteDocumentAsync(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return false;
            }

            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
