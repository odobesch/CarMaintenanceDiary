using CarMaintenanceDiary.Application.Interfaces;
using CarMaintenanceDiary.Core.Models;
using CarMaintenanceDiary.Infrastructure.Data;
using CarMaintenanceDiary.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarMaintenanceDiary.Api.Controllers
{
    [Route("api/[controller]")]
    public class VehiclesController : ControllerBase
    {        
        private readonly ApplicationDbContext _context;

        public VehiclesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("getall")]
        public async Task<ActionResult<List<VehicleDto>>> GetAll()
        {
            var vehicles = await _context.Vehicles
                .AsNoTracking()
                .Select(v => new VehicleDto
                {
                    Id = v.Id,
                    Make = v.Make,
                    Model = v.Model,
                    Year = v.Year,
                    LicensePlate = v.LicensePlate.ToUpper(),
                    VIN = v.VIN
                })
                .ToListAsync();

            return Ok(vehicles);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<VehicleDto>> GetById(int id)
        {
            var v = await _context.Vehicles.FindAsync(id);
            if (v == null)
                return NotFound();

            return new VehicleDto
            {
                Id = v.Id,
                Make = v.Make,
                Model = v.Model,
                Year = v.Year,
                LicensePlate = v.LicensePlate.ToUpper(),
                VIN = v.VIN
            };
        }

        [HttpPost]
        public async Task<ActionResult<int>> Add([FromBody] VehicleDto dto)
        {
            var entity = new Vehicle
            {
                Make = dto.Make,
                Model = dto.Model,
                Year = dto.Year,
                LicensePlate = dto.LicensePlate.ToUpper(),
                VIN = dto.VIN
            };

            _context.Vehicles.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(entity.Id);
        }

        [HttpGet("exists/{licensePlate}")]
        public async Task<ActionResult<bool>> LicensePlateExists(string licensePlate)
        {
            var exists = await _context.Vehicles
                .AnyAsync(v => v.LicensePlate.ToUpper() == licensePlate.ToUpper());
            return Ok(exists);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] VehicleDto dto)
        {
            var entity = await _context.Vehicles.FindAsync(id);
            if (entity == null)
                return NotFound();            

            entity.Make = dto.Make;
            entity.Model = dto.Model;
            entity.Year = dto.Year;
            entity.LicensePlate = dto.LicensePlate.ToUpper();
            entity.VIN = dto.VIN;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.Vehicles.FindAsync(id);
            if (entity == null)
                return NotFound();

            _context.Vehicles.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
