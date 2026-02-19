using System.ComponentModel.DataAnnotations;

namespace Vpassbackend.DTOs
{
    public class AcceptTransferDTO
    {
        [Required]
        public int TransferId { get; set; }
    }
}
