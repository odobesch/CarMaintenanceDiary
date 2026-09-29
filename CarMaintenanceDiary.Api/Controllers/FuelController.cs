using CarMaintenanceDiary.Application.Interfaces;
using CarMaintenanceDiary.Core.Models;
using CarMaintenanceDiary.Infrastructure.Data;
using CarMaintenanceDiary.Shared.DTOs.Fuel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using CarMaintenanceDiary.Infrastructure.Media;
using CarMaintenanceDiary.Infrastructure.Security;

namespace CarMaintenanceDiary.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FuelController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;
        private readonly IImageProcessingService _imageProcessingService;

        public FuelController(ApplicationDbContext context, IUserContext userContext, IImageProcessingService imageProcessingService)
        {
            _context = context;
            _userContext = userContext;
            _imageProcessingService = imageProcessingService;
        }

        [HttpGet("{vehicleId}")]
        public async Task<ActionResult<List<FuelRecordDto>>> GetFuelRecords(int vehicleId)
        {
            var vehicle = await _context.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Id == vehicleId);
            if (vehicle == null)
                return NotFound();

            if (!_userContext.CanAccessOwnedResource(vehicle.UserId))
                return Forbid();

            var records = await _context.FuelEntries
                .Where(r => r.VehicleId == vehicleId)
                .OrderBy(r => r.Date)
                .Select(r => new FuelRecordDto
                {
                    Id = r.Id,
                    VehicleId = r.VehicleId,
                    Date = r.Date,
                    Odometer = r.Odometer,
                    Liters = r.Liters,
                    PricePerLiter = r.PricePerLiter,
                    FuelStation = r.FuelStation,
                    FullTank = r.FullTank,
                    PhotoIds = r.Photos.Select(p => p.Id).ToList()
                })
                .ToListAsync();

            return Ok(records);
        }

        [HttpGet("record/{recordId}")]
        public async Task<ActionResult<FuelRecordDto>> GetFuelRecordById(int recordId)
        {
            var recordEntity = await _context.FuelEntries
                .Include(r => r.Photos)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == recordId);

            if (recordEntity == null)
                return NotFound();

            var vehicle = await _context.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Id == recordEntity.VehicleId);
            if (vehicle == null)
                return NotFound();

            if (!_userContext.CanAccessOwnedResource(vehicle.UserId))
                return Forbid();

            var record = new FuelRecordDto
            {
                Id = recordEntity.Id,
                VehicleId = recordEntity.VehicleId,
                Date = recordEntity.Date,
                Odometer = recordEntity.Odometer,
                Liters = recordEntity.Liters,
                PricePerLiter = recordEntity.PricePerLiter,
                FuelStation = recordEntity.FuelStation,
                FullTank = recordEntity.FullTank,
                PhotoIds = recordEntity.Photos.Select(p => p.Id).ToList()
            };

            return Ok(record);
        }

        [HttpPost("{vehicleId}")]
        public async Task<IActionResult> AddFuelRecord(int vehicleId, [FromBody] FuelRecordDto dto)
        {
            var vehicle = await _context.Vehicles.FindAsync(vehicleId);
            if (vehicle == null)
                return NotFound("Vehicle not found.");

            if (!_userContext.CanAccessOwnedResource(vehicle.UserId))
                return Forbid();

            var entry = new FuelRecord
            {
                VehicleId = vehicleId,
                Date = dto.Date,
                Odometer = dto.Odometer,
                Liters = dto.Liters,
                PricePerLiter = dto.PricePerLiter,
                FuelStation = dto.FuelStation,
                FullTank = dto.FullTank
            };

            _context.FuelEntries.Add(entry);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{recordId}")]
        public async Task<IActionResult> UpdateFuelRecord(int recordId, [FromBody] FuelRecordDto dto)
        {
            if (recordId != dto.Id)
                return BadRequest("Mismatched record ID.");

            var record = await _context.FuelEntries.FindAsync(recordId);
            if (record == null)
                return NotFound();

            var vehicle = await _context.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Id == record.VehicleId);
            if (vehicle == null)
                return NotFound();

            if (!_userContext.CanAccessOwnedResource(vehicle.UserId))
                return Forbid();

            record.Date = dto.Date;
            record.Odometer = dto.Odometer;
            record.Liters = dto.Liters;
            record.PricePerLiter = dto.PricePerLiter;
            record.FuelStation = dto.FuelStation;
            record.FullTank = dto.FullTank;

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{recordId}")]
        public async Task<IActionResult> DeleteFuelRecord(int recordId)
        {
            var record = await _context.FuelEntries.FindAsync(recordId);
            if (record == null)
                return NotFound();

            var vehicle = await _context.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Id == record.VehicleId);
            if (vehicle == null)
                return NotFound();

            if (!_userContext.CanAccessOwnedResource(vehicle.UserId))
                return Forbid();

            _context.FuelEntries.Remove(record);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("summary/monthly/{vehicleId}")]
        public async Task<IActionResult> GetMonthlySummary(int vehicleId)
        {
            var vehicle = await _context.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Id == vehicleId);
            if (vehicle == null)
                return NotFound();

            if (!_userContext.CanAccessOwnedResource(vehicle.UserId))
                return Forbid();

            var records = await _context.FuelEntries
                .Where(r => r.VehicleId == vehicleId)
                .OrderBy(r => r.Date)
                .ToListAsync();

            var grouped = records
                .GroupBy(r => new { r.Date.Year, r.Date.Month })
                .Select(g =>
                {                    
                    var ordered = g.OrderBy(x => x.Odometer).ToList();
                    var distance = ordered.Last().Odometer - ordered.First().Odometer;
                    var totalLiters = ordered.Sum(x => x.Liters);
                    var totalCost = ordered.Sum(x => (decimal)x.Liters * x.PricePerLiter);
                    return new MonthlyFuelSummaryDto
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        TotalLiters = totalLiters,
                        TotalCost = totalCost,
                        AverageConsumptionPer100Km = distance > 0 ? (totalLiters / distance) * 100 : 0
                    };
                })
                .ToList();

            return Ok(grouped);
        }

        [HttpGet("summary/yearly/{vehicleId}")]
        public async Task<IActionResult> GetYearlySummary(int vehicleId)
        {
            var vehicle = await _context.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Id == vehicleId);
            if (vehicle == null)
                return NotFound();

            if (!_userContext.CanAccessOwnedResource(vehicle.UserId))
                return Forbid();

            var records = await _context.FuelEntries
                .Where(r => r.VehicleId == vehicleId)
                .OrderBy(r => r.Date)
                .ToListAsync();

            var grouped = records
                .GroupBy(r => r.Date.Year)
                .Select(g =>
                {
                    var ordered = g.OrderBy(x => x.Odometer).ToList();
                    var distance = ordered.Last().Odometer - ordered.First().Odometer;
                    var totalLiters = ordered.Sum(x => x.Liters);
                    var totalCost = ordered.Sum(x => (decimal)x.Liters * x.PricePerLiter);
                    return new YearlyFuelSummaryDto
                    {
                        Year = g.Key,
                        TotalLiters = totalLiters,
                        TotalCost = totalCost,
                        AverageConsumptionPer100Km = distance > 0 ? (totalLiters / distance) * 100 : 0
                    };
                })
                .ToList();

            return Ok(grouped);
        }

        [HttpPost("{recordId}/photos")]
        public async Task<IActionResult> UploadPhoto(int recordId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var record = await _context.FuelEntries.FindAsync(recordId);
            if (record == null)
                return NotFound("Fuel record not found.");

            var vehicle = await _context.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Id == record.VehicleId);
            if (vehicle == null)
                return NotFound();

            if (!_userContext.CanAccessOwnedResource(vehicle.UserId))
                return Forbid();

            await using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var data = ms.ToArray();

            var photo = new FuelPhoto
            {
                FuelRecordId = recordId,
                Data = data,
                ContentType = file.ContentType ?? "application/octet-stream"
            };

            _context.FuelPhotos.Add(photo);
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
    int? w = null,                // width in CSS px
    int? h = null,                // height in CSS px
    string mode = "crop",         // crop | pad | max
    int dpr = 1,                  // 1 or 2 (retina)
    string? format = null,        // webp | jpeg | png (optional)
    int q = 82,                   // quality for webp/jpeg
    float sharpen = 0.8f          // subtle sharpening
)
        {
            var photo = await _context.FuelPhotos
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
            var photo = await _context.FuelPhotos.FindAsync(photoId);
            if (photo == null)
                return NotFound();

            var record = await _context.FuelEntries.AsNoTracking().FirstOrDefaultAsync(r => r.Id == photo.FuelRecordId);
            if (record == null)
                return NotFound();

            var vehicle = await _context.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Id == record.VehicleId);
            if (vehicle == null)
                return NotFound();

            if (!_userContext.CanAccessOwnedResource(vehicle.UserId))
                return Forbid();

            _context.FuelPhotos.Remove(photo);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
