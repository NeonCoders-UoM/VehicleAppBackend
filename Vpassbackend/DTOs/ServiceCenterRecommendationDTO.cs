using System;
using System.Collections.Generic;
using Vpassbackend.Models;

namespace Vpassbackend.DTOs
{
    public class ServiceCenterRecommendationDTO
    {
        public int ServiceCenterId { get; set; }
        public required string Name { get; set; }
        public required string Address { get; set; }
        public double Distance { get; set; } // Distance in kilometers from customer's location
        public decimal EstimatedTotalCost { get; set; }
        public decimal AdvancePaymentAmount { get; set; } // 10% of estimated cost
        public List<ServiceDetailsDTO> AvailableServices { get; set; } = new List<ServiceDetailsDTO>();
    }

    public class ServiceDetailsDTO
    {
        public int ServiceId { get; set; }
        public required string ServiceName { get; set; }
        public required string Description { get; set; }
        public decimal Price { get; set; }
    }

    public class NearbyServiceCentersRequestDTO
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public List<int> ServiceIds { get; set; } = new List<int>();
        public DateTime AppointmentDate { get; set; }
    }
}
