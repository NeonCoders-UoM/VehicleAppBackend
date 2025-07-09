using System;

namespace Vpassbackend.Models
{
    public class FuelUsage
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public double Amount { get; set; }
        public DateTime Date { get; set; }
    }
}
