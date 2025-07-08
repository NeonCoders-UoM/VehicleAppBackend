using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Vpassbackend.Data;
using Vpassbackend.DTOs;
using Vpassbackend.Models;

namespace Vpassbackend.Services
{
    public interface IVehicleService
    {
        Task<IEnumerable<VehicleDto>> GetAllVehiclesAsync();
        Task<VehicleDto> GetVehicleByIdAsync(int id);
        Task<IEnumerable<VehicleDto>> GetVehiclesByCustomerIdAsync(int customerId);
        Task<VehicleDto> CreateVehicleAsync(VehicleCreateDto vehicleCreateDto);
        Task<VehicleDto> UpdateVehicleAsync(int id, VehicleUpdateDto vehicleUpdateDto);
        Task<bool> DeleteVehicleAsync(int id);
    }

    public class VehicleService : IVehicleService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public VehicleService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<VehicleDto>> GetAllVehiclesAsync()
        {
            var vehicles = await _context.Vehicles
                .Include(v => v.Customer)
                .ToListAsync();
            return _mapper.Map<IEnumerable<VehicleDto>>(vehicles);
        }

        public async Task<VehicleDto> GetVehicleByIdAsync(int id)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.Customer)
                .FirstOrDefaultAsync(v => v.VehicleId == id);

            if (vehicle == null)
            {
                return null;
            }

            return _mapper.Map<VehicleDto>(vehicle);
        }

        public async Task<IEnumerable<VehicleDto>> GetVehiclesByCustomerIdAsync(int customerId)
        {
            var vehicles = await _context.Vehicles
                .Where(v => v.CustomerId == customerId)
                .Include(v => v.Customer)
                .ToListAsync();

            return _mapper.Map<IEnumerable<VehicleDto>>(vehicles);
        }

        public async Task<VehicleDto> CreateVehicleAsync(VehicleCreateDto vehicleCreateDto)
        {
            var vehicle = _mapper.Map<Vehicle>(vehicleCreateDto);
            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            return _mapper.Map<VehicleDto>(vehicle);
        }

        public async Task<VehicleDto> UpdateVehicleAsync(int id, VehicleUpdateDto vehicleUpdateDto)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return null;
            }

            _mapper.Map(vehicleUpdateDto, vehicle);
            await _context.SaveChangesAsync();

            return _mapper.Map<VehicleDto>(vehicle);
        }

        public async Task<bool> DeleteVehicleAsync(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return false;
            }

            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
