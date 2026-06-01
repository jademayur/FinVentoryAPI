using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.SalesReturnDTOs;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SalesReturnController : ControllerBase
    {
        private readonly ISalesReturnService _service;

        public SalesReturnController(ISalesReturnService service)
        {
            _service = service;
        }

        // ────────────────────────────────────────────────────────────────────
        // POST  api/salesreturn
        // ────────────────────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSalesReturnMainDto dto)
        {
            try
            {
                var result = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById),
                    new { id = result.ReturnId }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ────────────────────────────────────────────────────────────────────
        // PUT  api/salesreturn/{id}
        // ────────────────────────────────────────────────────────────────────
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSalesReturnMainDto dto)
        {
            try
            {
                var result = await _service.UpdateAsync(id, dto);
                if (!result) return NotFound(new { message = "Sales return not found." });
                return Ok(new { message = "Sales return updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ────────────────────────────────────────────────────────────────────
        // DELETE  api/salesreturn/{id}
        // ────────────────────────────────────────────────────────────────────
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _service.DeleteAsync(id);
                if (!result) return NotFound(new { message = "Sales return not found." });
                return Ok(new { message = "Sales return deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ────────────────────────────────────────────────────────────────────
        // GET  api/salesreturn/{id}
        // ────────────────────────────────────────────────────────────────────
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _service.GetByIdAsync(id);
                if (result == null) return NotFound(new { message = "Sales return not found." });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ────────────────────────────────────────────────────────────────────
        // GET  api/salesreturn
        // ────────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _service.GetAllAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ────────────────────────────────────────────────────────────────────
        // POST  api/salesreturn/paged
        // ────────────────────────────────────────────────────────────────────
        [HttpPost("paged")]
        public async Task<IActionResult> GetPaged([FromBody] PagedRequestDto request)
        {
            try
            {
                var result = await _service.GetPagedAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
