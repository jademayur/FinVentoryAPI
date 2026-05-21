using FinVentoryAPI.DTOs.JournalEntryDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class JournalEntryController : ControllerBase
    {
        private readonly IJournalEntryService _service;

        public JournalEntryController(IJournalEntryService service)
        {
            _service = service;
        }

        // ════════════════════════════════════════════════════
        // POST  api/journalentry
        // ════════════════════════════════════════════════════
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateJournalEntryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = result.JournalEntryId }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ════════════════════════════════════════════════════
        // PUT  api/journalentry/{id}
        // ════════════════════════════════════════════════════
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateJournalEntryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _service.UpdateAsync(id, dto);
                return result
                    ? NoContent()
                    : NotFound(new { message = $"Journal entry {id} not found." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ════════════════════════════════════════════════════
        // DELETE  api/journalentry/{id}
        // ════════════════════════════════════════════════════
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _service.DeleteAsync(id);
                return result
                    ? NoContent()
                    : NotFound(new { message = $"Journal entry {id} not found." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ════════════════════════════════════════════════════
        // GET  api/journalentry/{id}
        // ════════════════════════════════════════════════════
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return result == null
                ? NotFound(new { message = $"Journal entry {id} not found." })
                : Ok(result);
        }

        // ════════════════════════════════════════════════════
        // GET  api/journalentry
        // ════════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        // ════════════════════════════════════════════════════
        // POST  api/journalentry/paged
        // ════════════════════════════════════════════════════
        [HttpPost("paged")]
        public async Task<IActionResult> GetPaged([FromBody] PagedRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

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
