using FinVentoryAPI.DTOs.AccountGroupDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountGroupController : ControllerBase
    {
        private readonly IAccountGroupService _service;

        public AccountGroupController(IAccountGroupService service)
        {
            _service = service;
        }

        // ========================================
        // CREATE
        // ========================================
        [HttpPost]
        public async Task<IActionResult> Create(CreateAccountGroupDto dto)
        {
            try
            {
                var result = await _service.CreateAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ========================================
        // UPDATE
        // ========================================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateAccountGroupDto dto)
        {
            try
            {
                var updated = await _service.UpdateAsync(id, dto);

                if (!updated)
                    return NotFound(new { message = "Account group not found." });

                return Ok(new { message = "Account group updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ========================================
        // GET ALL
        // ========================================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        // ========================================
        // GET BY ID
        // ========================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);

            if (result == null)
                return NotFound(new { message = "Account group not found." });

            return Ok(result);
        }

        // ========================================
        // DELETE (Soft Delete)
        // ========================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);

            if (!deleted)
                return NotFound(new { message = "Account group not found." });

            return Ok(new { message = "Account group deleted successfully." });
        }

        [HttpPost("list")]
        public async Task<IActionResult> GetPaged([FromBody] PagedRequestDto request)
        {       
            var result = await _service.GetPagedAsync(request);
            return Ok(result);
        }
    }
}
