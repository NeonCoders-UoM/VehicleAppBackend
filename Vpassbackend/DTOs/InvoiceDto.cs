namespace Vpassbackend.DTOs
{
    public class InvoiceDto
    {
        public int InvoiceId { get; set; }
        public int VehicleId { get; set; }
        public decimal? TotalCost { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public VehicleDto? Vehicle { get; set; }
        public List<PaymentLogDto>? PaymentLogs { get; set; }
    }

    public class InvoiceCreateDto
    {
        public int VehicleId { get; set; }
        public decimal TotalCost { get; set; }
        public DateTime InvoiceDate { get; set; }
    }

    public class InvoiceUpdateDto
    {
        public decimal? TotalCost { get; set; }
        public DateTime? InvoiceDate { get; set; }
    }
}
