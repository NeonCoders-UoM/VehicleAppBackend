using Microsoft.AspNetCore.Mvc;
using Vpassbackend.Services;

namespace Vpassbackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceController : ControllerBase
    {
        private readonly TInvoiceOperations _invoiceService;

        public InvoiceController(TInvoiceOperations invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [HttpGet("details/{appointmentId}")]
        public async Task<IActionResult> GetInvoiceDetails(int appointmentId)
        {
            var result = await _invoiceService.GetInvoiceDetailsByAppointmentId(appointmentId);
            if (result == null)
                return NotFound("Invoice details not found");

            return Ok(result);
        }
    }
}
