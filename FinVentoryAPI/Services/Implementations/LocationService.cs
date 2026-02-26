using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.LocationDTOs;
using FinVentoryAPI.DTOs.UserDTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;
using System.Diagnostics.Metrics;

namespace FinVentoryAPI.Services.Implementations
{
    public class LocationService : ILocationService
    {
        private readonly AppDbContext _context;

        public LocationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<LocationResponseDto> CreateAsync(CreateLocationDTO dto, int userId)
        {
            if (await _context.Locations.AnyAsync(x => x.LocationName == dto.LocationName))
                throw new Exception("Location already exists");

            var location = new Location
            {
                LocationName = dto.LocationName,
                LocationCode = dto.LocationCode,
                AddressLine1 = dto.AddressLine1,
                AddressLine2 = dto.AddressLine2,
                City = dto.City,
                State = dto.State,
                Country = dto.Country,
                Pincode = dto.Pincode,
                IsHeadOffice = dto.IsHeadOffice,
                CompanyId = dto.CompanyId,
                CreatedBy = dto.CreatedBy,
                CreatedDate = DateTime.UtcNow,
            };

            _context.Locations.Add(location);
            await _context.SaveChangesAsync();

            return MapToResponse(location);
        }

        public async Task<bool> UpdateAsync(int id, UpdateLocationDTO dto)
        {
            var location = await _context.Locations
                .FirstOrDefaultAsync(x => x.LocationId == id && x.IsActive);

            if (location == null)
                return false;

            var duplicate = await _context.Locations
                .AnyAsync(x =>
                    x.CompanyId == location.CompanyId &&
                    x.LocationName.ToLower() == dto.LocationName.ToLower() &&
                    x.LocationId != id &&
                    x.IsActive);

            if (duplicate)
                throw new Exception("Location already exists");

            location.LocationName = dto.LocationName;
            location.LocationCode = dto.LocationCode;
            location.AddressLine1 = dto.AddressLine1;
            location.AddressLine2 = dto.AddressLine2;
            location.City = dto.City;
            location.State = dto.State;
            location.Country = dto.Country;
            location.Pincode = dto.Pincode;
            location.IsHeadOffice = dto.IsHeadOffice;
            location.UpdatedBy = dto.UpdatedBy;
            location.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<LocationResponseDto>> GetAllAsync()
        {
            return await _context.Locations
                .Where(x => x.IsActive)
                .Select(x => new LocationResponseDto
                {
                    LocationId = x.LocationId,
                    CompanyId = x.CompanyId,
                   // CompanyName = x.Company.CompanyName,
                    LocationName = x.LocationName,
                    LocationCode = x.LocationCode,
                    AddressLine1 = x.AddressLine1,
                    AddressLine2 = x.AddressLine2,
                    City = x.City,
                    State = x.State,
                    Country = x.Country,
                    Pincode = x.Pincode,
                    IsHeadOffice = x.IsHeadOffice,
                    IsActive = x.IsActive
                }).ToListAsync();
        }


        public async Task<LocationResponseDto?> GetByIdAsync(int id)
        {
            var location = await _context.Locations
                .Include(x => x.Company)
                .FirstOrDefaultAsync(x => x.LocationId == id && x.IsActive);
            if (location == null)
                return null;
            return MapToResponse(location);
        }

        public async Task<bool> DeleteAsync(int id, int userId)
        {
            var location = await _context.Locations
                .FirstOrDefaultAsync(x => x.LocationId == id && x.IsActive);
            if (location == null)
                return false;
            location.IsActive = false;
            location.UpdatedBy = userId;
            location.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        private LocationResponseDto MapToResponse(Location location)
        {
            return new LocationResponseDto
            {
                LocationId = location.LocationId,
                CompanyId = location.CompanyId,
                LocationName = location.LocationName,
                LocationCode = location.LocationCode,
                AddressLine1 = location.AddressLine1,
                AddressLine2 = location.AddressLine2,
                City = location.City,
                State = location.State,
                Country = location.Country,
                Pincode = location.Pincode,
                IsHeadOffice = location.IsHeadOffice,
                IsActive = location.IsActive,
                CreatedByName = _context.Users.Where(u => u.UserId == location.CreatedBy).Select(u => u.FullName).FirstOrDefault() ?? "Unknown",
                CreatedDate = location.CreatedDate,
                UpdatedByName = location.UpdatedBy.HasValue ? _context.Users.Where(u => u.UserId == location.UpdatedBy.Value).Select(u => u.FullName).FirstOrDefault() ?? "Unknown" : null,
                UpdatedDate = location.UpdatedDate
            };
        }
    }
}
