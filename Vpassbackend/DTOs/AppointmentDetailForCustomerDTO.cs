namespace Vpassbackend.DTOs
{
    public class AppointmentDetailForCustomerDTO
    {
        public string VehicleRegistration { get; set; }
        public DateTime AppointmentDate { get; set; }
        public int ServiceCenterId { get; set; }
        public string ServiceCenterAddress { get; set; }
        public List<string> Services { get; set; }
    }
}
