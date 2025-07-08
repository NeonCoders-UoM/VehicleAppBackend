using AutoMapper;
using Vpassbackend.DTOs;
using Vpassbackend.Models;

namespace Vpassbackend.Data
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Customer mappings
            CreateMap<Customer, CustomerDto>();
            CreateMap<CustomerCreateDto, Customer>();
            CreateMap<CustomerUpdateDto, Customer>();

            // Vehicle mappings
            CreateMap<Vehicle, VehicleDto>();
            CreateMap<VehicleCreateDto, Vehicle>();
            CreateMap<VehicleUpdateDto, Vehicle>();

            // ServiceCenter mappings
            CreateMap<ServiceCenter, ServiceCenterDto>();
            CreateMap<ServiceCenterCreateDto, ServiceCenter>();
            CreateMap<ServiceCenterUpdateDto, ServiceCenter>();

            // Service mappings
            CreateMap<Service, ServiceDto>();
            CreateMap<ServiceCreateDto, Service>();
            CreateMap<ServiceUpdateDto, Service>();

            // ServiceCenterCheckInPoint mappings
            CreateMap<ServiceCenterCheckInPoint, ServiceCenterCheckInPointDto>();
            CreateMap<ServiceCenterCheckInPointCreateDto, ServiceCenterCheckInPoint>();
            CreateMap<ServiceCenterCheckInPointUpdateDto, ServiceCenterCheckInPoint>();

            // Appointment mappings
            CreateMap<Appointment, AppointmentDto>();
            CreateMap<AppointmentCreateDto, Appointment>();
            CreateMap<AppointmentUpdateDto, Appointment>();

            // Invoice mappings
            CreateMap<Invoice, InvoiceDto>();
            CreateMap<InvoiceCreateDto, Invoice>();
            CreateMap<InvoiceUpdateDto, Invoice>();

            // PaymentLog mappings
            CreateMap<PaymentLog, PaymentLogDto>();
            CreateMap<PaymentLogCreateDto, PaymentLog>();
            CreateMap<PaymentLogUpdateDto, PaymentLog>();

            // BorderPoint mappings
            CreateMap<BorderPoint, BorderPointDto>();
            CreateMap<BorderPointCreateDto, BorderPoint>();
            CreateMap<BorderPointUpdateDto, BorderPoint>();

            // Document mappings
            CreateMap<Document, DocumentDto>();
            CreateMap<DocumentCreateDto, Document>();
            CreateMap<DocumentUpdateDto, Document>();
        }
    }
}
