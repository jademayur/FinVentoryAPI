using FinVentoryAPI.DTOs.LocationDTOs;
using FinVentoryAPI.DTOs.RoleRightsDTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Services.Implementations;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinVentoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RoleRightController : ControllerBase
    {
        private readonly IRoleRightService _roleRightService;

        public RoleRightController(IRoleRightService roleRightService)
        {
            _roleRightService = roleRightService;
        }

        [HttpPost]
        [Authorize(Roles = "applAdmin")]
        public async Task<IActionResult> Create([FromBody] RoleRightCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _roleRightService.CreateAsync(dto);

            return CreatedAtAction(nameof(GetById), new { id = result.RoleRightId }, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var List = await _roleRightService.GetAllAsync();
            return Ok(List);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var List = await _roleRightService.GetByIdAsync(id);

            if (List == null)
                return NotFound(new { message = "Role Right not found" });

            return Ok(List);
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "applAdmin")]
        public async Task<IActionResult> Update(int id, [FromBody] RoleRightUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _roleRightService.UpdateAsync(id, dto);

            if (!updated)
                return NotFound(new { message = "Role Right not found" });

            return Ok(new { message = "Role Right updated successfully" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "applAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            int userId = Convert.ToInt32(userIdClaim);

            var deleted = await _roleRightService.DeleteAsync(id, userId);

            if (!deleted)
                return NotFound(new { message = "Role Right not found" });

            return Ok(new { message = "Role Right deleted successfully" });
        }

        [HttpGet("GetMenuByRole/{roleId}")]
        public async Task<IActionResult> GetMenuByRole(int roleId)
        {
            var callerRoleId = int.Parse(User.FindFirst("RoleId")?.Value ?? "0");
            var callerRoleName = User.FindFirst(ClaimTypes.Role)?.Value;

            if (callerRoleId != roleId && callerRoleName != "applAdmin")
                return Forbid();

            var data = await _roleRightService.GetMenuByRoleAsync(roleId);
            return Ok(data);
        }

        [HttpGet("permission/{MenuItemID}")]
        public async Task<IActionResult> GetFormPermissions(int MenuItemID)
        {
            var roleIdClaim = User.FindFirst("RoleId")?.Value;

            if (roleIdClaim == null)
                return Unauthorized();

            int roleId = Convert.ToInt32(roleIdClaim);

            var permission = await _roleRightService.GetFormPermissionsAsync(MenuItemID, roleId);

            if (permission == null)
                return Forbid();

            return Ok(permission);
        }
    }
}
