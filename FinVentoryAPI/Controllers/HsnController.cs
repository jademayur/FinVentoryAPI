using FinVentoryAPI.DTOs.HsnDTOs;
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
    public class HsnController : ControllerBase
    {
        private readonly IHsnService _service;
        public HsnController(IHsnService hsnService)
        {
            _service = hsnService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateHsnDto dto)
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
        public async Task<IActionResult> Update(int id, UpdateHsnDto dto)
        {
            try
            {
                var updated = await _service.UpdateAsync(id, dto);

                if (!updated)
                    return NotFound(new { message = "HSN not found." });

                return Ok(new { message = "HSN updated successfully." });
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
                return NotFound(new { message = "HSN not found." });

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);

            if (!deleted)
                return NotFound(new { message = "HSN not found." });

            return Ok(new { message = "HSN deleted successfully." });
        }

        [HttpPost("list")]
        public async Task<IActionResult> GetPaged([FromBody] PagedRequestDto request)
        {
            var result = await _service.GetPagedAsync(request);
            return Ok(result);
        }
    }
}
