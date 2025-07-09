using System;
using System.Collections.Generic;

namespace Vpassbackend.DTOs
{
    public class AppointmentBookingDTO
    {
        // Vehicle information
        public int VehicleId { get; set; }

        // Customer information
        public int CustomerId { get; set; }

        // Selected date for appointment
        public DateTime AppointmentDate { get; set; }

        // Selected services
        public List<int> ServiceIds { get; set; } = new List<int>();

        // Selected service center
        public int ServiceCenterId { get; set; }

        // Customer's current location for nearby service center recommendations
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // Optional description
        public string? Description { get; set; }
    }
}
