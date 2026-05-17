using FinVentoryAPI.DTOs.CashBankEntryDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CashBankEntryController : ControllerBase
    {
        private readonly ICashBankEntryService _service;

        public CashBankEntryController(ICashBankEntryService service)
        {
            _service = service;
        }

        // ════════════════════════════════════════════════════
        // POST /api/CashBankEntry
        // CREATE
        // ════════════════════════════════════════════════════
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCashBankEntryDto dto)
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

        // ════════════════════════════════════════════════════
        // PUT /api/CashBankEntry/{id}
        // UPDATE (Draft only)
        // ════════════════════════════════════════════════════
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCashBankEntryDto dto)
        {
            try
            {
                var updated = await _service.UpdateAsync(id, dto);

                if (!updated)
                    return NotFound(new { message = "Entry not found." });

                return Ok(new { message = "Entry updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ════════════════════════════════════════════════════
        // DELETE /api/CashBankEntry/{id}
        // DELETE (Draft only — soft delete)
        // ════════════════════════════════════════════════════
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _service.DeleteAsync(id);

                if (!deleted)
                    return NotFound(new { message = "Entry not found." });

                return Ok(new { message = "Entry deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ════════════════════════════════════════════════════
        // GET /api/CashBankEntry/{id}
        // GET BY ID
        // ════════════════════════════════════════════════════
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _service.GetByIdAsync(id);

                if (result == null)
                    return NotFound(new { message = "Entry not found." });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ════════════════════════════════════════════════════
        // GET /api/CashBankEntry
        // GET ALL
        // ════════════════════════════════════════════════════
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
                return BadRequest(new { message = ex.Message });
            }
        }

        // ════════════════════════════════════════════════════
        // POST /api/CashBankEntry/list
        // GET PAGED (with search, filters, sorting)
        // ════════════════════════════════════════════════════
        [HttpPost("list")]
        public async Task<IActionResult> GetPaged([FromBody] PagedRequestDto request)
        {
            try
            {
                var result = await _service.GetPagedAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}