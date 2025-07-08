using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Vpassbackend.Data;
using Vpassbackend.DTOs;
using Vpassbackend.Models;

namespace Vpassbackend.Services
{
    public interface IInvoiceService
    {
        Task<IEnumerable<InvoiceDto>> GetAllInvoicesAsync();
        Task<InvoiceDto?> GetInvoiceByIdAsync(int id);
        Task<IEnumerable<InvoiceDto>> GetInvoicesByVehicleIdAsync(int vehicleId);
        Task<InvoiceDto> CreateInvoiceAsync(InvoiceCreateDto invoiceCreateDto);
        Task<InvoiceDto?> UpdateInvoiceAsync(int id, InvoiceUpdateDto invoiceUpdateDto);
        Task<bool> DeleteInvoiceAsync(int id);
    }

    public class InvoiceService : IInvoiceService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public InvoiceService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<InvoiceDto>> GetAllInvoicesAsync()
        {
            var invoices = await _context.Invoices
                .Include(i => i.Vehicle)
                .Include(i => i.PaymentLogs)
                .ToListAsync();
            return _mapper.Map<IEnumerable<InvoiceDto>>(invoices);
        }

        public async Task<InvoiceDto?> GetInvoiceByIdAsync(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Vehicle)
                .Include(i => i.PaymentLogs)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);

            if (invoice == null)
            {
                return null;
            }

            return _mapper.Map<InvoiceDto>(invoice);
        }

        public async Task<IEnumerable<InvoiceDto>> GetInvoicesByVehicleIdAsync(int vehicleId)
        {
            var invoices = await _context.Invoices
                .Where(i => i.VehicleId == vehicleId)
                .Include(i => i.Vehicle)
                .Include(i => i.PaymentLogs)
                .ToListAsync();

            return _mapper.Map<IEnumerable<InvoiceDto>>(invoices);
        }

        public async Task<InvoiceDto> CreateInvoiceAsync(InvoiceCreateDto invoiceCreateDto)
        {
            var invoice = _mapper.Map<Invoice>(invoiceCreateDto);
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            return _mapper.Map<InvoiceDto>(invoice);
        }

        public async Task<InvoiceDto?> UpdateInvoiceAsync(int id, InvoiceUpdateDto invoiceUpdateDto)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
            {
                return null;
            }

            _mapper.Map(invoiceUpdateDto, invoice);
            await _context.SaveChangesAsync();

            return _mapper.Map<InvoiceDto>(invoice);
        }

        public async Task<bool> DeleteInvoiceAsync(int id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
            {
                return false;
            }

            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
