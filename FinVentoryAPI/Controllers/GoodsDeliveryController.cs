using FinVentoryAPI.DTOs.GoodsDeliveryDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoodsDeliveryController : ControllerBase
    {
        private readonly IGoodsDeliveryService _service;

        public GoodsDeliveryController(IGoodsDeliveryService service)
        {
            _service = service;
        }

        // ────────────────────────────────────────────────────
        // PICKER / PREFILL  (call before opening the form)
        // ────────────────────────────────────────────────────

        /// <summary>
        /// Step 1 – select customer, then call this to see eligible orders.
        /// Returns Confirmed orders that still have pending delivery qty.
        /// </summary>
        [HttpGet("orders-for-customer/{businessPartnerId:int}")]
        public async Task<IActionResult> GetOrdersForCustomer(int businessPartnerId)
        {
            var result = await _service.GetOrdersForCustomerAsync(businessPartnerId);
            return Ok(result);
        }

        /// <summary>
        /// Step 2 – user ticks one or more orders, then call this.
        /// Returns merged prefill data (header from first order, lines from all orders
        /// with orderedQty / deliveredQty / pendingQty populated).
        /// </summary>
        [HttpPost("prefill")]
        public async Task<IActionResult> GetDeliveryPrefill([FromBody] List<int> orderIds)
        {
            var result = await _service.GetDeliveryPrefillAsync(orderIds);
            return Ok(result);
        }

        // ────────────────────────────────────────────────────
        // CRUD
        // ────────────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateGoodsDeliveryMainDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.DeliveryId }, result);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateGoodsDeliveryMainDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            return Ok(result);
        }

        // ────────────────────────────────────────────────────
        // STATUS TRANSITIONS
        // ────────────────────────────────────────────────────

        [HttpPatch("{id:int}/confirm")]
        public async Task<IActionResult> Confirm(int id)
        {
            var result = await _service.ConfirmAsync(id);
            return Ok(result);
        }

        [HttpPatch("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var result = await _service.CancelAsync(id);
            return Ok(result);
        }

        // ────────────────────────────────────────────────────
        // QUERIES
        // ────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost("paged")]
        public async Task<IActionResult> GetPaged([FromBody] PagedRequestDto request)
        {
            var result = await _service.GetPagedAsync(request);
            return Ok(result);
        }

        [HttpGet("by-customer/{businessPartnerId}")]
        public async Task<IActionResult> GetDeliveriesForCustomer(int businessPartnerId)
        {
            var result = await _service.GetDeliveriesForCustomerAsync(businessPartnerId);
            return Ok(result);
        }
    }
}
