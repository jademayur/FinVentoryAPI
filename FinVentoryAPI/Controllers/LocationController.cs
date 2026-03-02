using FinVentoryAPI.DTOs.CompanyDTOs;
using FinVentoryAPI.DTOs.LocationDTOs;
using FinVentoryAPI.DTOs.RoleRightsDTOs;
using FinVentoryAPI.Services.Implementations;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata;

namespace FinVentoryAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly ILocationService _locationService;
        private readonly IRoleRightService _roleRightService;

        public LocationController(ILocationService locationService, IRoleRightService roleRightService)
        {
            _locationService = locationService;
            _roleRightService = roleRightService;

        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateLocationDTO dto)
        {
            var permission = await GetPermission("Add");

            if (permission == null || !permission.CanAdd)
                return Forbid("No Add Permission");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _locationService.CreateAsync(dto);

            return CreatedAtAction(nameof(GetById),
                new { id = result.LocationId }, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var permission = await GetPermission("View");

            if (permission == null || !permission.CanView)
                return Forbid("No View Permission");

            var locations = await _locationService.GetAllAsync();
            return Ok(locations);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var permission = await GetPermission("View");

            if (permission == null || !permission.CanView)
                return Forbid("No View Permission");

            var location = await _locationService.GetByIdAsync(id);

            if (location == null)
                return NotFound(new { message = "Location not found" });

            return Ok(location);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateLocationDTO dto)
        {
            var permission = await GetPermission("Edit");

            if (permission == null || !permission.CanEdit)
                return Forbid("No Edit Permission");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _locationService.UpdateAsync(id, dto);

            if (!updated)
                return NotFound(new { message = "Location not found" });

            return Ok(new { message = "Location updated successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var permission = await GetPermission("Delete");

            if (permission == null || !permission.CanDelete)
                return Forbid("No Delete Permission");

            var userId = Convert.ToInt32(User.FindFirst("UserId")?.Value);

            var deleted = await _locationService.DeleteAsync(id, userId);

            if (!deleted)
                return NotFound(new { message = "Location not found" });

            return Ok(new { message = "Location deleted successfully" });
        }
        
        private async Task<FormPermissionDto?> GetPermission(string actionType)
        {
            // 🔹 Get RoleId from JWT
            var roleIdClaim = User.FindFirst("RoleId")?.Value;
            if (roleIdClaim == null)
                return null;

            int roleId = Convert.ToInt32(roleIdClaim);

            // 🔹 Get MenuItemId from Header
            if (!Request.Headers.TryGetValue("MenuItemId", out var menuHeader))
                return null;

            int menuItemId = Convert.ToInt32(menuHeader);

            var permission = await _roleRightService
                .GetFormPermissionsAsync(menuItemId, roleId);

            return permission;
        }
    }
}
