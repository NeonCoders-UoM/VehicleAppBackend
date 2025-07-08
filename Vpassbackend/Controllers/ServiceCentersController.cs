using Microsoft.AspNetCore.Mvc;
using Vpassbackend.DTOs;
using Vpassbackend.Services;

namespace Vpassbackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceCentersController : ControllerBase
    {
        private readonly IServiceCenterService _serviceCenterService;

        public ServiceCentersController(IServiceCenterService serviceCenterService)
        {
            _serviceCenterService = serviceCenterService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceCenterDto>>> GetServiceCenters()
        {
            var serviceCenters = await _serviceCenterService.GetAllServiceCentersAsync();
            return Ok(serviceCenters);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceCenterDto>> GetServiceCenter(int id)
        {
            var serviceCenter = await _serviceCenterService.GetServiceCenterByIdAsync(id);
            if (serviceCenter == null)
            {
                return NotFound();
            }

            return Ok(serviceCenter);
        }

        [HttpPost]
        public async Task<ActionResult<ServiceCenterDto>> CreateServiceCenter(ServiceCenterCreateDto serviceCenterCreateDto)
        {
            var serviceCenter = await _serviceCenterService.CreateServiceCenterAsync(serviceCenterCreateDto);
            return CreatedAtAction(nameof(GetServiceCenter), new { id = serviceCenter.Station_id }, serviceCenter);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ServiceCenterDto>> UpdateServiceCenter(int id, ServiceCenterUpdateDto serviceCenterUpdateDto)
        {
            var serviceCenter = await _serviceCenterService.UpdateServiceCenterAsync(id, serviceCenterUpdateDto);
            if (serviceCenter == null)
            {
                return NotFound();
            }

            return Ok(serviceCenter);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteServiceCenter(int id)
        {
            var result = await _serviceCenterService.DeleteServiceCenterAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
