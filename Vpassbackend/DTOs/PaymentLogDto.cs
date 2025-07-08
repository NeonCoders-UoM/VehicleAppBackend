namespace Vpassbackend.DTOs
{
    public class PaymentLogDto
    {
        public int LogId { get; set; }
        public int InvoiceId { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? Status { get; set; }
    }

    public class PaymentLogCreateDto
    {
        public int InvoiceId { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; }
    }

    public class PaymentLogUpdateDto
    {
        public DateTime? PaymentDate { get; set; }
        public string? Status { get; set; }
    }
}
