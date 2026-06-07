using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.SalesOrderDTOs;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesOrderController : ControllerBase
    {
        private readonly ISalesOrderService _service;

        public SalesOrderController(ISalesOrderService service)
        {
            _service = service;
        }

        // POST api/salesorder
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSalesOrderMainDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.OrderId }, result);
        }

        // PUT api/salesorder/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSalesOrderMainDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            return updated ? NoContent() : NotFound();
        }

        // DELETE api/salesorder/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }

        // GET api/salesorder/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        // GET api/salesorder
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        // POST api/salesorder/paged
        [HttpPost("paged")]
        public async Task<IActionResult> GetPaged([FromBody] PagedRequestDto request)
        {
            var result = await _service.GetPagedAsync(request);
            return Ok(result);
        }

        // GET api/salesorder/quotations-for-customer/42
        // Step 1 — user picks customer → load available quotations
        [HttpGet("quotations-for-customer/{businessPartnerId:int}")]
        public async Task<IActionResult> GetQuotationsForCustomer(int businessPartnerId)
        {
            var result = await _service.GetQuotationsForCustomerAsync(businessPartnerId);
            return Ok(result);
        }

        // GET api/salesorder/quotation-prefill/7
        // Step 2 — user picks quotation → pre-fill order form
        [HttpGet("quotation-prefill/{quotationId:int}")]
        public async Task<IActionResult> GetQuotationPrefill(int quotationId)
        {
            var result = await _service.GetQuotationPrefillAsync(quotationId);
            return Ok(result);
        }

        // PATCH api/salesorder/5/confirm
        [HttpPatch("{id:int}/confirm")]
        public async Task<IActionResult> Confirm(int id)
        {
            await _service.ConfirmAsync(id);
            return NoContent();
        }

        // PATCH api/salesorder/5/cancel
        [HttpPatch("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            await _service.CancelAsync(id);
            return NoContent();
        }
    }
}
