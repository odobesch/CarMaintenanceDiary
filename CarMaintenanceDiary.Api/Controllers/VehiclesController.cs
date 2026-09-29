using CarMaintenanceDiary.Application.Interfaces;
using CarMaintenanceDiary.Core.Models;
using CarMaintenanceDiary.Infrastructure.Data;
using CarMaintenanceDiary.Infrastructure.Media;
using CarMaintenanceDiary.Infrastructure.Security;
using CarMaintenanceDiary.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarMaintenanceDiary.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]   
    [Authorize]
    public class VehiclesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;
        private readonly IImageProcessingService _imageProcessingService;

        public VehiclesController(ApplicationDbContext context, IUserContext userContext, IImageProcessingService imageProcessingService)
        {
            _context = context;
            _userContext = userContext;
            _imageProcessingService = imageProcessingService;
        }

        [HttpGet("getall")]
        public async Task<ActionResult<List<VehicleDto>>> GetAll()
        {
            var currentUserId = _userContext.GetCurrentUserId();
            var isAdmin = _userContext.IsInRole("Admin");

            var query = _context.Vehicles.AsNoTracking();

             if (!isAdmin)
            {
                query = query.Where(v => v.UserId == currentUserId);
            }

            var vehicles = await query
                .Select(v => new VehicleDto
                {
                    Id = v.Id,
                    Make = v.Make,
                    Model = v.Model,
                    Year = v.Year,
                    LicensePlate = v.LicensePlate.ToUpper(),
                    VIN = v.VIN,
                    UserId = v.UserId
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

            if (!_userContext.CanAccessOwnedResource(v.UserId))
                return Forbid();

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
            var currentUserId = _userContext.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

            var entity = new Vehicle
            {
                Make = dto.Make,
                Model = dto.Model,
                Year = dto.Year,
                LicensePlate = dto.LicensePlate.ToUpper(),
                VIN = dto.VIN,
                UserId = currentUserId
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

            if (!_userContext.CanAccessOwnedResource(entity.UserId))
                return Forbid();

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

            if (!_userContext.CanAccessOwnedResource(entity.UserId))
                return Forbid();

            _context.Vehicles.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{vehicleId}/photos")]
        public async Task<IActionResult> UploadPhoto(int vehicleId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var vehicle = await _context.Vehicles.FindAsync(vehicleId);
            if (vehicle == null)
                return NotFound("Vehicle not found.");

            if (!_userContext.CanAccessOwnedResource(vehicle.UserId))
                return Forbid();

            await using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var data = ms.ToArray();

            var photo = new VehiclePhoto
            {
                VehicleId = vehicleId,
                Data = data,
                ContentType = file.ContentType ?? "application/octet-stream"
            };

            _context.VehiclePhotos.Add(photo);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                photo.Id
            });
        }

        [HttpGet("photos/{photoId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPhoto(
            int photoId,
            int? w = null, int? h = null,
            string mode = "crop", int dpr = 1,
            string? format = null, int q = 82,
            float sharpen = 0.8f)
        {
            var photo = await _context.VehiclePhotos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == photoId);
            if (photo == null)
                return NotFound();

            var (data, contentType) = await _imageProcessingService.ProcessAsync(
                photo.Data,
                photo.ContentType,
                new ImageProcessingOptions { Width = w, Height = h, Mode = mode, Dpr = dpr, Format = format, Quality = q, Sharpen = sharpen });

            Response.Headers.CacheControl = "public,max-age=31536000,immutable";
            return File(data, contentType);
        }

        [HttpDelete("photos/{photoId}")]
        public async Task<IActionResult> DeletePhoto(int photoId)
        {
            var photo = await _context.VehiclePhotos.FindAsync(photoId);
            if (photo == null)
                return NotFound();

            var vehicle = await _context.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Id == photo.VehicleId);
            if (vehicle == null)
                return NotFound();

            if (!_userContext.CanAccessOwnedResource(vehicle.UserId))
                return Forbid();

            _context.VehiclePhotos.Remove(photo);
            await _context.SaveChangesAsync();
            return Ok();
        }
        
        [HttpGet("{vehicleId}/photos")]
        public async Task<IActionResult> GetPhotoIdsForVehicle(int vehicleId)
        {
            var vehicle = await _context.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Id == vehicleId);
            if (vehicle == null)
                return NotFound();

            if (!_userContext.CanAccessOwnedResource(vehicle.UserId))
                return Forbid();

            var ids = await _context.VehiclePhotos
                .Where(p => p.VehicleId == vehicleId)
                .OrderBy(p => p.Id)
                .Select(p => p.Id)
                .ToListAsync();

            return Ok(ids);
        }
    }
}
