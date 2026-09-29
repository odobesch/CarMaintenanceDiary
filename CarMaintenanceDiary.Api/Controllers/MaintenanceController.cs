using CarMaintenanceDiary.Core.Models;
using CarMaintenanceDiary.Infrastructure.Data;
using CarMaintenanceDiary.Infrastructure.Media;
using CarMaintenanceDiary.Infrastructure.Security;
using CarMaintenanceDiary.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;

namespace CarMaintenanceDiary.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MaintenanceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;
        private readonly IImageProcessingService _imageProcessingService;
        private readonly FileExtensionContentTypeProvider _contentProvider = new();

        public MaintenanceController(ApplicationDbContext context, IUserContext userContext, IImageProcessingService imageProcessingService)
        {
            _context = context;
            _userContext = userContext;
            _imageProcessingService = imageProcessingService;
        }
        
        [HttpGet("vehicle/{vehicleId}/records")]
        public async Task<ActionResult<List<MaintenanceRecordDto>>> GetRecordsForVehicle(int vehicleId)
        {
            var vehicle = await _context.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Id == vehicleId);
            if (vehicle == null)
                return NotFound();

            if (!_userContext.CanAccessOwnedResource(vehicle.UserId))
                return Forbid();

            var records = await _context.MaintenanceRecords
                .Where(r => r.VehicleId == vehicleId)
                .OrderByDescending(r => r.Date)
                .Select(r => new MaintenanceRecordDto
                {
                    Id = r.Id,
                    VehicleId = r.VehicleId,
                    Date = r.Date,
                    Description = r.Description,
                    Cost = r.Cost,
                    Workshop = r.Workshop,
                    MaintenanceType = r.MaintenanceType,
                    // return filenames in DTO (client uses separate endpoint to get ids)
                    PhotoPaths = r.Documents.Select(d => d.FileName).ToList()
                })
                .ToListAsync();

            return Ok(records);
        }

        [HttpGet("records/{id}/documents")]
        public async Task<ActionResult<List<MaintenanceDocumentDto>>> GetDocumentIds(int id)
        {
            var record = await _context.MaintenanceRecords.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
            if (!await _context.MaintenanceRecords.AnyAsync(r => r.Id == id))
                return NotFound();

            var vehicle = await _context.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Id == record!.VehicleId);
            if (vehicle == null)
                return NotFound();

            if (!_userContext.CanAccessOwnedResource(vehicle.UserId))
                return Forbid();

            var docs = await _context.MaintenanceDocuments
                .AsNoTracking()
                .Where(p => p.MaintenanceRecordId == id)
                .Select(p => new MaintenanceDocumentDto
                {
                    Id = p.Id,
                    FileName = p.FileName,
                    ContentType = p.ContentType
                })
                .ToListAsync();

            return Ok(docs);
        }

        [HttpGet("records/{id}")]
        public async Task<ActionResult<MaintenanceRecordDto>> GetRecordById(int id)
        {
            var recordEntity = await _context.MaintenanceRecords
                .Include(x => x.Documents)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (recordEntity == null)
                return NotFound();

            var vehicle = await _context.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Id == recordEntity.VehicleId);
            if (vehicle == null)
                return NotFound();

            if (!_userContext.CanAccessOwnedResource(vehicle.UserId))
                return Forbid();

            var dto = new MaintenanceRecordDto
            {
                Id = recordEntity.Id,
                VehicleId = recordEntity.VehicleId,
                Date = recordEntity.Date,
                Description = recordEntity.Description,
                Cost = recordEntity.Cost,
                Workshop = recordEntity.Workshop,
                MaintenanceType = recordEntity.MaintenanceType,
                PhotoPaths = recordEntity.Documents.Select(d => d.FileName).ToList()
            };

            return Ok(dto);
        }
        
        [HttpPost("vehicle/{vehicleId}/records")]
        public async Task<IActionResult> AddRecord(int vehicleId, [FromBody] MaintenanceRecordDto dto)
        {
            var vehicle = await _context.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Id == vehicleId);
            if (vehicle == null)
                return NotFound("Vehicle not found.");

            if (!_userContext.CanAccessOwnedResource(vehicle.UserId))
                return Forbid();

            var record = new MaintenanceRecord
            {
                VehicleId = vehicleId,
                Date = dto.Date,
                Description = dto.Description,
                Cost = dto.Cost,
                Workshop = dto.Workshop,
                MaintenanceType = dto.MaintenanceType
            };

            _context.MaintenanceRecords.Add(record);
            await _context.SaveChangesAsync();
            return Ok(new
            {
                record.Id
            });
        }
       
        [HttpPut("records/{id}")]
        public async Task<IActionResult> UpdateRecord(int id, [FromBody] MaintenanceRecordDto dto)
        {
            if (id != dto.Id)
                return BadRequest("Mismatched id.");

            var record = await _context.MaintenanceRecords.FindAsync(id);
            if (record == null)
                return NotFound();

            var vehicle = await _context.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Id == record.VehicleId);
            if (vehicle == null)
                return NotFound();

            if (!_userContext.CanAccessOwnedResource(vehicle.UserId))
                return Forbid();

            record.Date = dto.Date;
            record.Description = dto.Description;
            record.Cost = dto.Cost;
            record.Workshop = dto.Workshop;
            record.MaintenanceType = dto.MaintenanceType;

            await _context.SaveChangesAsync();
            return Ok();
        }
        
        [HttpDelete("records/{id}")]
        public async Task<IActionResult> DeleteRecord(int id)
        {
            var record = await _context.MaintenanceRecords
                 .Include(r => r.Documents)
                 .FirstOrDefaultAsync(r => r.Id == id);
            if (record == null)
                return NotFound();

            var vehicle = await _context.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Id == record.VehicleId);
            if (vehicle == null)
                return NotFound();

            if (!_userContext.CanAccessOwnedResource(vehicle.UserId))
                return Forbid();

            _context.MaintenanceDocuments.RemoveRange(record.Documents);
            _context.MaintenanceRecords.Remove(record);
            await _context.SaveChangesAsync();
            return Ok();
        }
 
        [HttpPost("records/{id}/documents")]
        public async Task<IActionResult> UploadDocument(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var record = await _context.MaintenanceRecords.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
            if (record == null)
                return NotFound("Maintenance record not found.");

            var vehicle = await _context.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Id == record.VehicleId);
            if (vehicle == null)
                return NotFound();

            if (!_userContext.CanAccessOwnedResource(vehicle.UserId))
                return Forbid();

            await using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var data = ms.ToArray();

            var doc = new MaintenanceDocument
            {
                MaintenanceRecordId = id,
                Data = data,
                ContentType = file.ContentType ?? "application/octet-stream",
                FileName = Path.GetFileName(file.FileName)
            };

            _context.MaintenanceDocuments.Add(doc);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                doc.Id
            });
        } 
        
        [HttpGet("documents/{docId:int}")]        
        public async Task<IActionResult> GetDocument(
            int docId,
            int? w = null,
            int? h = null,
            string mode = "crop",
            int dpr =1,
            string? format = null,
            int q =82,
            float sharpen =0.8f)
        {
            var doc = await _context.MaintenanceDocuments.AsNoTracking().FirstOrDefaultAsync(p => p.Id == docId);
            if (doc == null)
                return NotFound();

            var record = await _context.MaintenanceRecords.AsNoTracking().FirstOrDefaultAsync(r => r.Id == doc.MaintenanceRecordId);
            if (record == null)
                return NotFound();

            var vehicle = await _context.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Id == record.VehicleId);
            if (vehicle == null)
                return NotFound();

            if (!_userContext.CanAccessOwnedResource(vehicle.UserId))
                return Forbid();

            var contentType = doc.ContentType ?? "application/octet-stream";

            var isImage = contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
            if (!isImage || w == null || h == null)
            {
                Response.Headers.CacheControl = "public,max-age=31536000,immutable";
                return File(doc.Data, contentType);
            }

            var (data, outContentType) = await _imageProcessingService.ProcessAsync(
                doc.Data,
                contentType,
                new ImageProcessingOptions { Width = w, Height = h, Mode = mode, Dpr = dpr, Format = format, Quality = q, Sharpen = sharpen });

            Response.Headers.CacheControl = "public,max-age=31536000,immutable";
            return File(data, outContentType);
        }
        
        [HttpGet("documents/{docId:int}/download")]
        public async Task<IActionResult> DownloadDocument(int docId)
        {
            var doc = await _context.MaintenanceDocuments.AsNoTracking().FirstOrDefaultAsync(p => p.Id == docId);
            if (doc == null)
                return NotFound();

            var record = await _context.MaintenanceRecords.AsNoTracking().FirstOrDefaultAsync(r => r.Id == doc.MaintenanceRecordId);
            if (record == null)
                return NotFound();

            var vehicle = await _context.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Id == record.VehicleId);
            if (vehicle == null)
                return NotFound();

            if (!_userContext.CanAccessOwnedResource(vehicle.UserId))
                return Forbid();

            var contentType = doc.ContentType ?? "application/octet-stream";
            var fileName = string.IsNullOrWhiteSpace(doc.FileName) ? $"document_{docId}" : doc.FileName;
            var bytes = doc.Data;

            return File(bytes, contentType, fileName);
        }
        
        [HttpDelete("documents/{docId:int}")]
        public async Task<IActionResult> DeleteDocument(int docId)
        {
            var doc = await _context.MaintenanceDocuments.FindAsync(docId);
            if (doc == null)
                return NotFound();

            var record = await _context.MaintenanceRecords.AsNoTracking().FirstOrDefaultAsync(r => r.Id == doc.MaintenanceRecordId);
            if (record == null)
                return NotFound();

            var vehicle = await _context.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Id == record.VehicleId);
            if (vehicle == null)
                return NotFound();

            if (!_userContext.CanAccessOwnedResource(vehicle.UserId))
                return Forbid();

            _context.MaintenanceDocuments.Remove(doc);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
