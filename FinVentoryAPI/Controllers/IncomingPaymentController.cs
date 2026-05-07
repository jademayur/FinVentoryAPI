using FinVentoryAPI.DTOs.IncomingPaymentDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IncomingPaymentController : ControllerBase
    {
        private readonly IIncomingPaymentService _service;

        public IncomingPaymentController(IIncomingPaymentService service)
        {
            _service = service;
        }

        // GET api/incomingpayment/pending-bills/{businessPartnerId}
        [HttpGet("pending-bills/{businessPartnerId:int}")]
        public async Task<IActionResult> GetPendingBills(int businessPartnerId)
        {
            var result = await _service.GetPendingBillsAsync(businessPartnerId);
            return Ok(result);
        }

        // POST api/incomingpayment
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateIncomingPaymentDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.PaymentId }, result);
        }

        // PUT api/incomingpayment/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateIncomingPaymentDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            if (!updated) return NotFound();
            return NoContent();
        }

        // DELETE api/incomingpayment/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        // GET api/incomingpayment/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // GET api/incomingpayment
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        // POST api/incomingpayment/paged
        [HttpPost("paged")]
        public async Task<IActionResult> GetPaged([FromBody] PagedRequestDto request)
        {
            var result = await _service.GetPagedAsync(request);
            return Ok(result);
        }
    }
}
