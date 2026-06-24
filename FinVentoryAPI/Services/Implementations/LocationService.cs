using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.LocationDTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinVentoryAPI.Services.Implementations
{
    public class LocationService : ILocationService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;

        public LocationService(AppDbContext context, Common common)
        {
            _context = context;
            _common = common;
        }

        public async Task<LocationResponseDto> CreateAsync(CreateLocationDTO dto)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();

            if (await _context.Locations.AnyAsync(x =>
                    x.CompanyId == companyId &&
                    x.LocationName == dto.LocationName &&
                    x.IsActive))
                throw new Exception("Location already exists.");

            var location = new Location
            {
                CompanyId = companyId,        // ✅ from JWT
                LocationName = dto.LocationName,
                LocationCode = dto.LocationCode,
                AddressLine1 = dto.AddressLine1,
                AddressLine2 = dto.AddressLine2,
                City = dto.City,
                State = dto.State,
                Country = dto.Country,
                Pincode = dto.Pincode,
                IsHeadOffice = dto.IsHeadOffice,
                CreatedBy = userId,           // ✅ from JWT
                CreatedDate = DateTime.UtcNow,
            };

            _context.Locations.Add(location);
            await _context.SaveChangesAsync();

            return MapToResponse(location);
        }

        public async Task<bool> UpdateAsync(int id, UpdateLocationDTO dto)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();

            var location = await _context.Locations
                .FirstOrDefaultAsync(x =>
                    x.LocationId == id &&
                    x.CompanyId == companyId &&  // ✅ scoped to company
                    x.IsActive);

            if (location == null)
                return false;

            var duplicate = await _context.Locations
                .AnyAsync(x =>
                    x.CompanyId == companyId &&
                    x.LocationName.ToLower() == dto.LocationName.ToLower() &&
                    x.LocationId != id &&
                    x.IsActive);

            if (duplicate)
                throw new Exception("Location already exists.");

            location.LocationName = dto.LocationName;
            location.LocationCode = dto.LocationCode;
            location.AddressLine1 = dto.AddressLine1;
            location.AddressLine2 = dto.AddressLine2;
            location.City = dto.City;
            location.State = dto.State;
            location.Country = dto.Country;
            location.Pincode = dto.Pincode;
            location.IsHeadOffice = dto.IsHeadOffice;
            location.UpdatedBy = userId;       // ✅ from JWT
            location.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<LocationResponseDto>> GetAllAsync()
        {
            var companyId = _common.GetCompanyId();  // ✅ filter by JWT company

            return await _context.Locations
                .Where(x => x.IsActive && x.CompanyId == companyId)
                .Select(x => new LocationResponseDto
                {
                    LocationId = x.LocationId,
                    CompanyId = x.CompanyId,
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
                })
                .ToListAsync();
        }

        public async Task<LocationResponseDto?> GetByIdAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var location = await _context.Locations
                .Include(x => x.Company)
                .FirstOrDefaultAsync(x =>
                    x.LocationId == id &&
                    x.CompanyId == companyId &&  // ✅ scoped to company
                    x.IsActive);

            if (location == null)
                return null;

            return MapToResponse(location);
        }

        public async Task<bool> DeleteAsync(int id, int userId)
        {
            var companyId = _common.GetCompanyId();

            var location = await _context.Locations
                .FirstOrDefaultAsync(x =>
                    x.LocationId == id &&
                    x.CompanyId == companyId &&  // ✅ scoped to company
                    x.IsActive);

            if (location == null)
                return false;

            location.IsActive = false;
            location.UpdatedBy = _common.GetUserId();  // ✅ from JWT
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
                CreatedByName = _context.Users
                    .Where(u => u.UserId == location.CreatedBy)
                    .Select(u => u.FullName)
                    .FirstOrDefault() ?? "Unknown",
                CreatedDate = location.CreatedDate,
                UpdatedByName = location.UpdatedBy.HasValue
                    ? _context.Users
                        .Where(u => u.UserId == location.UpdatedBy.Value)
                        .Select(u => u.FullName)
                        .FirstOrDefault() ?? "Unknown"
                    : null,
                UpdatedDate = location.UpdatedDate
            };
        }
    }
}