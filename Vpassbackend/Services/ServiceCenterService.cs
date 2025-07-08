using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Vpassbackend.Data;
using Vpassbackend.DTOs;
using Vpassbackend.Models;

namespace Vpassbackend.Services
{
    public interface IServiceCenterService
    {
        Task<IEnumerable<ServiceCenterDto>> GetAllServiceCentersAsync();
        Task<ServiceCenterDto?> GetServiceCenterByIdAsync(int id);
        Task<ServiceCenterDto> CreateServiceCenterAsync(ServiceCenterCreateDto serviceCenterCreateDto);
        Task<ServiceCenterDto?> UpdateServiceCenterAsync(int id, ServiceCenterUpdateDto serviceCenterUpdateDto);
        Task<bool> DeleteServiceCenterAsync(int id);
    }

    public class ServiceCenterService : IServiceCenterService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ServiceCenterService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ServiceCenterDto>> GetAllServiceCentersAsync()
        {
            var serviceCenters = await _context.ServiceCenters.ToListAsync();
            return _mapper.Map<IEnumerable<ServiceCenterDto>>(serviceCenters);
        }

        public async Task<ServiceCenterDto?> GetServiceCenterByIdAsync(int id)
        {
            var serviceCenter = await _context.ServiceCenters.FindAsync(id);
            if (serviceCenter == null)
            {
                return null;
            }

            return _mapper.Map<ServiceCenterDto>(serviceCenter);
        }

        public async Task<ServiceCenterDto> CreateServiceCenterAsync(ServiceCenterCreateDto serviceCenterCreateDto)
        {
            var serviceCenter = _mapper.Map<ServiceCenter>(serviceCenterCreateDto);
            _context.ServiceCenters.Add(serviceCenter);
            await _context.SaveChangesAsync();

            return _mapper.Map<ServiceCenterDto>(serviceCenter);
        }

        public async Task<ServiceCenterDto?> UpdateServiceCenterAsync(int id, ServiceCenterUpdateDto serviceCenterUpdateDto)
        {
            var serviceCenter = await _context.ServiceCenters.FindAsync(id);
            if (serviceCenter == null)
            {
                return null;
            }

            _mapper.Map(serviceCenterUpdateDto, serviceCenter);
            await _context.SaveChangesAsync();

            return _mapper.Map<ServiceCenterDto>(serviceCenter);
        }

        public async Task<bool> DeleteServiceCenterAsync(int id)
        {
            var serviceCenter = await _context.ServiceCenters.FindAsync(id);
            if (serviceCenter == null)
            {
                return false;
            }

            _context.ServiceCenters.Remove(serviceCenter);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
