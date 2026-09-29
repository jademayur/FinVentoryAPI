using FinVentoryAPI.DTOs.LocationDTOs;
using FinVentoryAPI.DTOs.ModuleDTOs;
using FinVentoryAPI.Services.Implementations;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ModuleController : ControllerBase
    {
        private readonly IModuleService _moduleService;
        public ModuleController(IModuleService moduleService)
        {
            _moduleService = moduleService;
        }

        [HttpPost]
        [Authorize(Roles = "applAdmin")]
        public async Task<IActionResult> Create([FromBody] ModuleCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _moduleService.CreateAsync(dto);

            return CreatedAtAction(nameof(GetById), new { id = result.ModuleId }, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var modules = await _moduleService.GetAllAsync();
            return Ok(modules);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var modules = await _moduleService.GetByIdAsync(id);

            if (modules == null)
                return NotFound(new { message = "Module not found" });

            return Ok(modules);
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "applAdmin")]
        public async Task<IActionResult> Update(int id, [FromBody] ModuleUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _moduleService.UpdateAsync(id, dto);

            if (!updated)
                return NotFound(new { message = "Module not found" });

            return Ok(new { message = "Module updated successfully" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "applAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            int userId = Convert.ToInt32(userIdClaim);

            var deleted = await _moduleService.DeleteAsync(id, userId);

            if (!deleted)
                return NotFound(new { message = "Module not found" });

            return Ok(new { message = "Module deleted successfully" });
        }
    }
}
