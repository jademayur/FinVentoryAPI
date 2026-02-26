using FinVentoryAPI.DTOs.CompanyDTOs;
using FinVentoryAPI.DTOs.LocationDTOs;
using FinVentoryAPI.Services.Implementations;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata;

namespace FinVentoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly ILocationService _locationService;

        public LocationController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateLocationDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int userId = 1; // Later get from JWT claim

            var result = await _locationService.CreateAsync(dto, userId);

            return CreatedAtAction(nameof(GetById), new { id = result.CompanyId }, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var locations = await _locationService.GetAllAsync();
            return Ok(locations);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var location = await _locationService.GetByIdAsync(id);

            if (location == null)
                return NotFound(new { message = "Location not found" });

            return Ok(location);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateLocationDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int userId = 1; // Later get from JWT

            var updated = await _locationService.UpdateAsync(id, dto, userId);

            if (!updated)
                return NotFound(new { message = "Location not found" });

            return Ok(new { message = "Location updated successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            int userId = 1; // Later get from JWT

            var deleted = await _locationService.DeleteAsync(id, userId);

            if (!deleted)
                return NotFound(new { message = "Location not found" });

            return Ok(new { message = "Location deleted successfully" });
        }
    }
}
