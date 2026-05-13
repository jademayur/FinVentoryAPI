using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.PurchaseInvoiceDTOs;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PurchaseInvoiceController : ControllerBase
    {
        private readonly IPurchaseInvoiceService _service;

        public PurchaseInvoiceController(IPurchaseInvoiceService service)
        {
            _service = service;
        }

        // POST api/purchaseinvoice
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePurchaseInvoiceMainDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.InvoiceId }, result);
        }

        // GET api/purchaseinvoice
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        // GET api/purchaseinvoice/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // PUT api/purchaseinvoice/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePurchaseInvoiceMainDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            if (!updated) return NotFound();
            return NoContent();
        }

        // DELETE api/purchaseinvoice/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        // POST api/purchaseinvoice/paged
        [HttpPost("paged")]
        public async Task<IActionResult> GetPaged([FromBody] PagedRequestDto request)
        {
            var result = await _service.GetPagedAsync(request);
            return Ok(result);
        }
    }
}