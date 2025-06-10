using CarMaintenanceDiary.Application.Interfaces;
using CarMaintenanceDiary.Core.Models;
using CarMaintenanceDiary.Infrastructure.Data;
using CarMaintenanceDiary.Shared.DTOs.Fuel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarMaintenanceDiary.Api.Controllers
{
    [Route("api/[controller]")]
    public class FuelController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FuelController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{vehicleId}")]
        public async Task<ActionResult<List<FuelRecordDto>>> GetFuelRecords(int vehicleId)
        {
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
                    FullTank = r.FullTank
                })
                .ToListAsync();

            return Ok(records);
        }

        [HttpGet("record/{recordId}")]
        public async Task<ActionResult<FuelRecordDto>> GetFuelRecordById(int recordId)
        {
            var record = await _context.FuelEntries
                .Where(r => r.Id == recordId)
                .Select(r => new FuelRecordDto
                {
                    Id = r.Id,
                    VehicleId = r.VehicleId,
                    Date = r.Date,
                    Odometer = r.Odometer,
                    Liters = r.Liters,
                    PricePerLiter = r.PricePerLiter,
                    FuelStation = r.FuelStation,
                    FullTank = r.FullTank
                })
                .FirstOrDefaultAsync();

            if (record == null)
                return NotFound();

            return Ok(record);
        }

        [HttpPost("{vehicleId}")]
        public async Task<IActionResult> AddFuelRecord(int vehicleId, [FromBody] FuelRecordDto dto)
        {
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

            _context.FuelEntries.Remove(record);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("summary/monthly/{vehicleId}")]
        public async Task<IActionResult> GetMonthlySummary(int vehicleId)
        {
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
    }
}
