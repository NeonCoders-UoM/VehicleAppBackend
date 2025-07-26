using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Vpassbackend.Data;
using Vpassbackend.Models;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace Vpassbackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PayHereController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private const string PayHereSandboxUrl = "https://sandbox.payhere.lk/pay/checkout";
        private const string MerchantId = "1230582"; // TODO: Move to config
        private const string MerchantSecret = "MTA4NzE3ODU2ODQwNTA4MTE1OTQzOTQxMDE0MzcyMjAyNTg2MDgy"; // TODO: Move to config
        private const string NotifyUrl = "https://d0aaf8a77ee4.ngrok-free.app/api/payhere/notify"; // TODO: Replace with your ngrok URL
        private const string ReturnUrl = "http://localhost:8080/payment-success"; // TODO: Replace with your frontend return URL
        private const string CancelUrl = "http://localhost:8080/payment-cancel"; // TODO: Replace with your frontend cancel URL

        public PayHereController(ApplicationDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        // DTO for session creation
        public class CreateSessionRequest
        {
            public int VehicleId { get; set; }
            public string UserEmail { get; set; }
            public string UserName { get; set; }
        }

        [HttpPost("create-session")]
        public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest request)
        {
            // Fetch the vehicle entity from the database
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == request.VehicleId);
            if (vehicle == null)
            {
                return NotFound(new { message = "Vehicle not found" });
            }

            // Create or get Invoice for this vehicle/payment
            var invoice = new Invoice
            {
                VehicleId = request.VehicleId,
                Vehicle = vehicle, // set required navigation property
                TotalCost = 500,
                InvoiceDate = DateTime.UtcNow
            };
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            // Create orderId for PayHere (must be unique)
            var orderId = $"invoice_{invoice.InvoiceId}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

            // Set return_url to service history page with vehicleId and order_id
            var returnUrl = $"http://localhost:8080/service-history?vehicleId={request.VehicleId}&order_id={orderId}";

            // Prepare PayHere payment object
            var amount = "500.00";
            var currency = "LKR";
            var hash = GeneratePayHereHash(MerchantId, orderId, amount, currency, MerchantSecret);

            var paymentData = new
            {
                merchant_id = MerchantId,
                return_url = returnUrl,
                cancel_url = CancelUrl,
                notify_url = NotifyUrl,
                order_id = orderId,
                items = "Service History PDF",
                amount = amount,
                currency = currency,
                first_name = request.UserName,
                last_name = "",
                email = request.UserEmail,
                phone = "0771234567",
                address = "Colombo",
                city = "Colombo",
                country = "Sri Lanka",
                hash = hash
            };

            // Store PaymentLog as pending
            var paymentLog = new PaymentLog
            {
                InvoiceId = invoice.InvoiceId,
                PaymentDate = null,
                Status = "Pending"
            };
            _context.PaymentLogs.Add(paymentLog);
            await _context.SaveChangesAsync();

            // Return PayHere payment URL and orderId to frontend
            // (PayHere expects a POST form, so frontend will need to POST to PayHere with these fields)
            return Ok(new
            {
                payHereUrl = PayHereSandboxUrl,
                paymentFields = paymentData,
                orderId = orderId
            });
        }

        // Add hash generation method for PayHere
        private string GeneratePayHereHash(string merchantId, string orderId, string amount, string currency, string merchantSecret)
        {
            // Step 1: MD5 of merchant secret, upper case
            string merchantSecretMd5 = GetMd5Hash(merchantSecret).ToUpper();

            // Step 2: Concatenate merchant_id + order_id + amount + currency + merchantSecretMd5
            string raw = merchantId + orderId + amount + currency + merchantSecretMd5;

            // Step 3: MD5 of the concatenated string, upper case
            string hash = GetMd5Hash(raw).ToUpper();

            return hash;
        }

        private string GetMd5Hash(string input)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        // PayHere notify_url endpoint
        [HttpPost("notify")]
        public async Task<IActionResult> Notify()
        {
            // PayHere sends form-urlencoded data
            var form = await Request.ReadFormAsync();
            var orderId = form["order_id"].ToString();
            var statusCode = form["status_code"].ToString();
            var paymentStatus = statusCode == "2" ? "Paid" : "Failed";
            var paidAt = DateTime.UtcNow;

            // Find invoice by orderId (parse invoiceId from orderId string)
            int invoiceId = 0;
            if (orderId.StartsWith("invoice_") && int.TryParse(orderId.Split('_')[1], out invoiceId))
            {
                var paymentLog = await _context.PaymentLogs
                    .Include(p => p.Invoice)
                    .Where(p => p.InvoiceId == invoiceId)
                    .OrderByDescending(p => p.LogId)
                    .FirstOrDefaultAsync();
                if (paymentLog != null)
                {
                    paymentLog.Status = paymentStatus;
                    paymentLog.PaymentDate = paidAt;
                    await _context.SaveChangesAsync();
                }
            }
            return Ok();
        }

        // Check payment status by orderId
        [HttpGet("payment-status")]
        public async Task<IActionResult> GetPaymentStatus([FromQuery] string orderId)
        {
            int invoiceId = 0;
            if (orderId.StartsWith("invoice_") && int.TryParse(orderId.Split('_')[1], out invoiceId))
            {
                var paymentLog = await _context.PaymentLogs
                    .Include(p => p.Invoice)
                    .Where(p => p.InvoiceId == invoiceId)
                    .OrderByDescending(p => p.LogId)
                    .FirstOrDefaultAsync();
                if (paymentLog != null)
                {
                    return Ok(new { status = paymentLog.Status });
                }
            }
            return NotFound(new { status = "Unknown" });
        }
    }
} 