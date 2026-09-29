using FinVentoryAPI.DTOs.MenuItemDTOs;
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
    public class MenuItemController : ControllerBase
    {
        private readonly IMenuItemService _menuItemService;
        public MenuItemController(IMenuItemService menuItemService)
        {
            _menuItemService = menuItemService;
        }


        [HttpPost]
        [Authorize(Roles = "applAdmin")]
        public async Task<IActionResult> Create([FromBody] MenuItemCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _menuItemService.CreateAsync(dto);

            return CreatedAtAction(nameof(GetById), new { id = result.MenuItemId }, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _menuItemService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _menuItemService.GetByIdAsync(id);

            if (result == null)
                return NotFound(new { message = "Menu Item not found" });

            return Ok(result);
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "applAdmin")]
        public async Task<IActionResult> Update(int id, [FromBody] MenuItemUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _menuItemService.UpdateAsync(id, dto);

            if (!updated)
                return NotFound(new { message = "Menu Item not found" });

            return Ok(new { message = "Menu Item updated successfully" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "applAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            int userId = Convert.ToInt32(userIdClaim);

            var deleted = await _menuItemService.DeleteAsync(id, userId);

            if (!deleted)
                return NotFound(new { message = "Menu Item not found" });

            return Ok(new { message = "Menu Item deleted successfully" });
        }
    }
}
