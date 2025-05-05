using CarMaintenanceDiary.Application.Interfaces;
using CarMaintenanceDiary.Core.Models;
using CarMaintenanceDiary.Infrastructure.Data;
using CarMaintenanceDiary.Shared.DTOs;
using CarMaintenanceDiary.Shared.DTOs.Fuel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarMaintenanceDiary.Application.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly ApplicationDbContext _context;

        public VehicleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<VehicleDto>> GetAllAsync()
        {
            return await _context.Vehicles
                .Select(v => new VehicleDto
                {
                    Id = v.Id,
                    Make = v.Make,
                    Model = v.Model,
                    Year = v.Year,
                    LicensePlate = v.LicensePlate.ToUpper(),
                    VIN = v.VIN
                }).ToListAsync();
        }

        public async Task<VehicleDto?> GetByIdAsync(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            return vehicle == null ? null : new VehicleDto
            {
                Id = vehicle.Id,
                Make = vehicle.Make,
                Model = vehicle.Model,
                Year = vehicle.Year,
                LicensePlate = vehicle.LicensePlate.ToUpper(),
                VIN = vehicle.VIN
            };
        }

        public async Task<int> AddAsync(VehicleDto dto)
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
            return entity.Id;
        }

        public async Task UpdateAsync(VehicleDto dto)
        {
            var entity = await _context.Vehicles.FindAsync(dto.Id);
            if (entity == null)
                return;
            
            entity.Make = dto.Make;
            entity.Model = dto.Model;
            entity.Year = dto.Year;
            entity.LicensePlate = dto.LicensePlate.ToUpper();
            entity.VIN = dto.VIN;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Vehicles.FindAsync(id);
            if (entity == null)
                return;

            _context.Vehicles.Remove(entity);
            await _context.SaveChangesAsync();
        }


        public async Task<List<FuelRecordDto>> GetFuelRecordsAsync(int vehicleId)
        {
            var fuelRecords = await _context.FuelEntries
                .Where(fr => fr.VehicleId == vehicleId)
                .OrderBy(fr => fr.Date)
                .ToListAsync();

            return fuelRecords.Select(fr => new FuelRecordDto
            {
                Id = fr.Id,
                VehicleId = fr.VehicleId,
                Date = fr.Date,
                Odometer = fr.Odometer,
                Liters = fr.Liters,
                PricePerLiter = fr.PricePerLiter,
                FuelStation = fr.FuelStation,
                FullTank = fr.FullTank
            }).ToList();
        }

        public async Task AddFuelRecordAsync(int vehicleId, FuelRecordDto fuelRecordDto)
        {
            var fuelRecord = new FuelRecord
            {
                VehicleId = vehicleId,
                Date = fuelRecordDto.Date,
                Odometer = fuelRecordDto.Odometer,
                Liters = fuelRecordDto.Liters,
                PricePerLiter = fuelRecordDto.PricePerLiter,
                FuelStation = fuelRecordDto.FuelStation,
                FullTank = fuelRecordDto.FullTank
            };

            _context.FuelEntries.Add(fuelRecord);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateFuelRecordAsync(FuelRecordDto fuelRecordDto)
        {
            var fuelRecord = await _context.FuelEntries.FindAsync(fuelRecordDto.Id);

            if (fuelRecord == null)
            {
                throw new KeyNotFoundException($"Fuel record with ID {fuelRecordDto.Id} not found.");
            }

            fuelRecord.Date = fuelRecordDto.Date;
            fuelRecord.Odometer = fuelRecordDto.Odometer;
            fuelRecord.Liters = fuelRecordDto.Liters;
            fuelRecord.PricePerLiter = fuelRecordDto.PricePerLiter;
            fuelRecord.FuelStation = fuelRecordDto.FuelStation;
            fuelRecord.FullTank = fuelRecordDto.FullTank;

            _context.FuelEntries.Update(fuelRecord);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteFuelRecordAsync(int fuelRecordId)
        {
            var fuelRecord = await _context.FuelEntries.FindAsync(fuelRecordId);

            if (fuelRecord == null)
            {
                throw new KeyNotFoundException($"Fuel record with ID {fuelRecordId} not found.");
            }

            _context.FuelEntries.Remove(fuelRecord);
            await _context.SaveChangesAsync();
        }

        public async Task<List<MonthlyFuelSummaryDto>> GetMonthlyFuelSummariesAsync(int vehicleId)
        {
            var records = await GetFuelRecordsAsync(vehicleId);
            var grouped = records
                .OrderBy(r => r.Date)
                .GroupBy(r => new { r.Date.Year, r.Date.Month })
                .ToList();

            var result = new List<MonthlyFuelSummaryDto>();

            foreach (var group in grouped)
            {
                var ordered = group.OrderBy(r => r.Odometer).ToList();

                double totalLiters = ordered.Sum(r => r.Liters);
                decimal totalCost = ordered.Sum(r => (decimal)r.Liters * r.PricePerLiter);
                double totalDistance = ordered.Last().Odometer - ordered.First().Odometer;

                result.Add(new MonthlyFuelSummaryDto
                {
                    Year = group.Key.Year,
                    Month = group.Key.Month,
                    TotalLiters = totalLiters,
                    TotalCost = totalCost,
                    AverageConsumptionPer100Km = totalDistance > 0 ? (totalLiters / totalDistance) * 100 : 0
                });
            }

            return result;
        }

        public async Task<List<YearlyFuelSummaryDto>> GetYearlyFuelSummariesAsync(int vehicleId)
        {
            var records = await GetFuelRecordsAsync(vehicleId);
            var fullTanks = records
                .OrderBy(r => r.Date)
                .ToList();

            var grouped = fullTanks
                .GroupBy(r => r.Date.Year);

            var result = new List<YearlyFuelSummaryDto>();

            foreach (var group in grouped)
            {
                var ordered = group.OrderBy(r => r.Odometer).ToList();
                double totalLiters = ordered.Sum(r => r.Liters);
                decimal totalCost = ordered.Sum(r => (decimal)r.Liters * r.PricePerLiter);
                double totalDistance = ordered.Last().Odometer - ordered.First().Odometer;

                result.Add(new YearlyFuelSummaryDto
                {
                    Year = group.Key,
                    TotalLiters = totalLiters,
                    TotalCost = totalCost,
                    AverageConsumptionPer100Km = totalDistance > 0 ? (totalLiters / totalDistance) * 100 : 0
                });
            }

            return result;
        }
    }
}
