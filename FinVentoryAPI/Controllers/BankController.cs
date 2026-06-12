using FinVentoryAPI.DTOs.BankDTOs;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BankController : ControllerBase
    {
        private readonly IBankMasterService _service;

        public BankController(IBankMasterService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateBankMasterDto dto)
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

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateBankMasterDto dto)
        {
            try
            {
                var updated = await _service.UpdateAsync(id, dto);

                if (!updated)
                    return NotFound(new { message = "Bank master not found." });

                return Ok(new { message = "Bank master updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);

            if (result == null)
                return NotFound(new { message = "Bank master not found." });

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, [FromQuery] int userId)
        {
            var deleted = await _service.DeleteAsync(id);

            if (!deleted)
                return NotFound(new { message = "Bank master not fond." });

            return Ok(new { message = "Bank master deleted successfully." });
        }
    }
}
