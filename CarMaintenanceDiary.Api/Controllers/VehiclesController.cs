using CarMaintenanceDiary.Application.Interfaces;
using CarMaintenanceDiary.Core.Models;
using CarMaintenanceDiary.Infrastructure.Data;
using CarMaintenanceDiary.Infrastructure.Security;
using CarMaintenanceDiary.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace CarMaintenanceDiary.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]   
    [Authorize]
    public class VehiclesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;

        public VehiclesController(ApplicationDbContext context, IUserContext userContext)
        {
            _context = context;
            _userContext = userContext;
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

        [HttpPost("{vehicleId}/photos")]
        public async Task<IActionResult> UploadPhoto(int vehicleId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var vehicle = await _context.Vehicles.FindAsync(vehicleId);
            if (vehicle == null)
                return NotFound("Vehicle not found.");

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

            // Fast path: original bytes
            if (w is null || h is null)
            {
                Response.Headers.CacheControl = "public,max-age=31536000,immutable";
                return File(photo.Data, photo.ContentType ?? "application/octet-stream");
            }

            // Resize + sharpen
            using var image = SixLabors.ImageSharp.Image.Load(photo.Data);

            dpr = Math.Max(1, dpr);
            var targetSize = new SixLabors.ImageSharp.Size(
                Math.Max(1, w.Value * dpr),
                Math.Max(1, h.Value * dpr));

            var resizeMode = mode?.ToLowerInvariant() switch
            {
                "pad" => SixLabors.ImageSharp.Processing.ResizeMode.Pad,
                "max" => SixLabors.ImageSharp.Processing.ResizeMode.Max,
                _ => SixLabors.ImageSharp.Processing.ResizeMode.Crop
            };

            image.Mutate(x => x
                .Resize(new SixLabors.ImageSharp.Processing.ResizeOptions
                {
                    Size = targetSize,
                    Mode = resizeMode
                })
                .GaussianSharpen(Math.Max(0f, sharpen)));

            q = Math.Clamp(q, 1, 100);

            // Choose output by query or stored ContentType; default to webp
            var inferred = photo.ContentType?.ToLowerInvariant() switch
            {
                "image/jpeg" or "image/jpg" => "jpeg",
                "image/png" => "png",
                "image/webp" => "webp",
                _ => null
            };
            var chosen = (format ?? inferred ?? "webp").ToLowerInvariant();

            SixLabors.ImageSharp.Formats.IImageEncoder encoder = chosen switch
            {
                "jpeg" or "jpg" => new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder { Quality = q },
                "png" => new SixLabors.ImageSharp.Formats.Png.PngEncoder(),
                _ => new SixLabors.ImageSharp.Formats.Webp.WebpEncoder { Quality = q },
            };
            var contentType = chosen switch
            {
                "jpeg" or "jpg" => "image/jpeg",
                "png" => "image/png",
                _ => "image/webp",
            };

            using var ms = new MemoryStream();
            await image.SaveAsync(ms, encoder);
            Response.Headers.CacheControl = "public,max-age=31536000,immutable";
            return File(ms.ToArray(), contentType); // <- return bytes, safe
        }

        [HttpDelete("photos/{photoId}")]
        public async Task<IActionResult> DeletePhoto(int photoId)
        {
            var photo = await _context.VehiclePhotos.FindAsync(photoId);
            if (photo == null)
                return NotFound();

            _context.VehiclePhotos.Remove(photo);
            await _context.SaveChangesAsync();
            return Ok();
        }
        
        [HttpGet("{vehicleId}/photos")]
        public async Task<IActionResult> GetPhotoIdsForVehicle(int vehicleId)
        {
            var exists = await _context.Vehicles.AnyAsync(v => v.Id == vehicleId);
            if (!exists)
                return NotFound();

            var ids = await _context.VehiclePhotos
                .Where(p => p.VehicleId == vehicleId)
                .OrderBy(p => p.Id)
                .Select(p => p.Id)
                .ToListAsync();

            return Ok(ids);
        }
    }
}
