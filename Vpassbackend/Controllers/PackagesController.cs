using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vpassbackend.Data;
using Vpassbackend.DTOs;
using Vpassbackend.Models;

namespace Vpassbackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PackagesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PackagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Packages
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PackageDTO>>> GetPackages()
        {
            var packages = await _context.Packages
                .Select(p => new PackageDTO
                {
                    PackageId = p.PackageId,
                    PackageName = p.PackageName,
                    Percentage = p.Percentage,
                    Description = p.Description,
                    IsActive = p.IsActive
                })
                .ToListAsync();

            return packages;
        }

        // GET: api/Packages/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PackageDTO>> GetPackage(int id)
        {
            var package = await _context.Packages
                .Where(p => p.PackageId == id)
                .Select(p => new PackageDTO
                {
                    PackageId = p.PackageId,
                    PackageName = p.PackageName,
                    Percentage = p.Percentage,
                    Description = p.Description,
                    IsActive = p.IsActive
                })
                .FirstOrDefaultAsync();

            if (package == null)
            {
                return NotFound();
            }

            return package;
        }

        // GET: api/Packages/Active
        [HttpGet("Active")]
        public async Task<ActionResult<IEnumerable<PackageDTO>>> GetActivePackages()
        {
            var packages = await _context.Packages
                .Where(p => p.IsActive)
                .Select(p => new PackageDTO
                {
                    PackageId = p.PackageId,
                    PackageName = p.PackageName,
                    Percentage = p.Percentage,
                    Description = p.Description,
                    IsActive = p.IsActive
                })
                .ToListAsync();

            return packages;
        }

        // POST: api/Packages
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<ActionResult<PackageDTO>> CreatePackage(CreatePackageDTO createDto)
        {
            // Check if package name already exists
            bool packageExists = await _context.Packages
                .AnyAsync(p => p.PackageName.ToLower() == createDto.PackageName.ToLower());

            if (packageExists)
            {
                return BadRequest("Package with this name already exists");
            }

            var package = new Package
            {
                PackageName = createDto.PackageName,
                Percentage = createDto.Percentage,
                Description = createDto.Description,
                IsActive = createDto.IsActive
            };

            _context.Packages.Add(package);
            await _context.SaveChangesAsync();

            var resultDto = new PackageDTO
            {
                PackageId = package.PackageId,
                PackageName = package.PackageName,
                Percentage = package.Percentage,
                Description = package.Description,
                IsActive = package.IsActive
            };

            return CreatedAtAction(nameof(GetPackage), new { id = package.PackageId }, resultDto);
        }

        // PUT: api/Packages/5
        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> UpdatePackage(int id, UpdatePackageDTO updateDto)
        {
            var package = await _context.Packages.FindAsync(id);
            if (package == null)
            {
                return NotFound();
            }

            // Check if package name already exists (if name is being updated)
            if (!string.IsNullOrEmpty(updateDto.PackageName) && 
                updateDto.PackageName.ToLower() != package.PackageName.ToLower())
            {
                bool packageExists = await _context.Packages
                    .AnyAsync(p => p.PackageName.ToLower() == updateDto.PackageName.ToLower());

                if (packageExists)
                {
                    return BadRequest("Package with this name already exists");
                }
            }

            // Update properties if provided
            if (!string.IsNullOrEmpty(updateDto.PackageName))
                package.PackageName = updateDto.PackageName;

            if (updateDto.Percentage.HasValue)
                package.Percentage = updateDto.Percentage.Value;

            if (updateDto.Description != null)
                package.Description = updateDto.Description;

            if (updateDto.IsActive.HasValue)
                package.IsActive = updateDto.IsActive.Value;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PackageExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Packages/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> DeletePackage(int id)
        {
            var package = await _context.Packages.FindAsync(id);
            if (package == null)
            {
                return NotFound();
            }

            // Check if package is being used by any service center services
            bool isInUse = await _context.ServiceCenterServices
                .AnyAsync(scs => scs.PackageId == id);

            if (isInUse)
            {
                return BadRequest("Cannot delete package as it is currently being used by service centers");
            }

            _context.Packages.Remove(package);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PackageExists(int id)
        {
            return _context.Packages.Any(e => e.PackageId == id);
        }
    }
} 