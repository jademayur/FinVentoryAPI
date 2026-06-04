using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.SalesQuotationDTOs;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesQuotationController : ControllerBase
    {
        private readonly ISalesQuotationService _service;

        public SalesQuotationController(ISalesQuotationService service)
        {
            _service = service;
        }

        // ── GET /api/SalesQuotation ───────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        // ── GET /api/SalesQuotation/paged ─────────────────────
        [HttpPost("paged")]
        public async Task<IActionResult> GetPaged([FromBody] PagedRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.GetPagedAsync(request);
            return Ok(result);
        }

        // ── GET /api/SalesQuotation/{id} ──────────────────────
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { message = $"Quotation with ID {id} not found." });

            return Ok(result);
        }

        // ── GET /api/SalesQuotation/customer/{businessPartnerId} ──
        [HttpGet("customer/{businessPartnerId:int}")]
        public async Task<IActionResult> GetByCustomer(int businessPartnerId)
        {
            var result = await _service.GetByCustomerAsync(businessPartnerId);
            return Ok(result);
        }

        // ── POST /api/SalesQuotation ──────────────────────────
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSalesQuotationMainDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = result.QuotationId }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ── PUT /api/SalesQuotation/{id} ──────────────────────
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSalesQuotationMainDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updated = await _service.UpdateAsync(id, dto);
                if (!updated)
                    return NotFound(new { message = $"Quotation with ID {id} not found." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ── DELETE /api/SalesQuotation/{id} ───────────────────
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _service.DeleteAsync(id);
                if (!deleted)
                    return NotFound(new { message = $"Quotation with ID {id} not found." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

       
        // POST api/salesquotation/{id}/copy      
        [HttpPost("{id:int}/copy")]
        public async Task<IActionResult> Copy(int id, [FromBody] CopySalesQuotationDto? dto = null)
        {
            var result = await _service.CopyAsync(id, dto);
            return CreatedAtAction(nameof(GetById), new { id = result.QuotationId }, result);
        }

        
        // POST api/salesquotation/{id}/revise
       
        [HttpPost("{id:int}/revise")]
        public async Task<IActionResult> Revise(int id, [FromBody] ReviseSalesQuotationDto dto)
        {
            var result = await _service.ReviseAsync(id, dto);
            return CreatedAtAction(nameof(GetById), new { id = result.QuotationId }, result);
        }
    }
}
