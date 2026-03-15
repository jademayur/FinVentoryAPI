using FinVentoryAPI.DTOs.AccountDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.TaxTDOs;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TaxController : ControllerBase
    {
        private readonly ITaxService _service;

        public TaxController(ITaxService taxService)
        {
            _service = taxService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTaxDto dto)
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
        public async Task<IActionResult> Update(int id, UpdateTaxDto dto)
        {
            try
            {
                var updated = await _service.UpdateAsync(id, dto);

                if (!updated)
                    return NotFound(new { message = "Tax not found." });

                return Ok(new { message = "Tax updated successfully." });
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
                return NotFound(new { message = "Tax not found." });

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);

            if (!deleted)
                return NotFound(new { message = "Tax not found." });

            return Ok(new { message = "Tax deleted successfully." });
        }

        [HttpPost("list")]
        public async Task<IActionResult> GetPaged([FromBody] PagedRequestDto request)
        {
            var result = await _service.GetPagedAsync(request);
            return Ok(result);
        }

    }
}
