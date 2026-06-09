using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.PurchaseOrderDTOs;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseOrderController : ControllerBase
    {
        private readonly IPurchaseOrderService _service;

        public PurchaseOrderController(IPurchaseOrderService service)
        {
            _service = service;
        }

        // ════════════════════════════════════════════════════
        // POST  api/purchaseorder
        // ════════════════════════════════════════════════════
        /// <summary>Creates a new Purchase Order in Draft status.</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderMainDto dto)
        {
            try
            {
                var result = await _service.CreateAsync(dto);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ════════════════════════════════════════════════════
        // PUT  api/purchaseorder/{id}
        // ════════════════════════════════════════════════════
        /// <summary>Updates a Draft Purchase Order (Draft only).</summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePurchaseOrderMainDto dto)
        {
            try
            {
                var result = await _service.UpdateAsync(id, dto);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ════════════════════════════════════════════════════
        // DELETE  api/purchaseorder/{id}
        // ════════════════════════════════════════════════════
        /// <summary>Soft-deletes a Draft Purchase Order.</summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _service.DeleteAsync(id);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ════════════════════════════════════════════════════
        // PATCH  api/purchaseorder/{id}/confirm
        // ════════════════════════════════════════════════════
        /// <summary>Confirms a Draft Purchase Order → Confirmed.</summary>
        [HttpPatch("{id:int}/confirm")]
        public async Task<IActionResult> Confirm(int id)
        {
            try
            {
                var result = await _service.ConfirmAsync(id);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ════════════════════════════════════════════════════
        // PATCH  api/purchaseorder/{id}/cancel
        // ════════════════════════════════════════════════════
        /// <summary>Cancels a Draft or Confirmed Purchase Order.</summary>
        [HttpPatch("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var result = await _service.CancelAsync(id);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ════════════════════════════════════════════════════
        // GET  api/purchaseorder
        // ════════════════════════════════════════════════════
        /// <summary>Returns all Purchase Orders for the company (no paging).</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _service.GetAllAsync();
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ════════════════════════════════════════════════════
        // GET  api/purchaseorder/{id}
        // ════════════════════════════════════════════════════
        /// <summary>Returns a single Purchase Order with full line + tax detail.</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _service.GetByIdAsync(id);
                if (result == null)
                    return NotFound(new { success = false, message = "Purchase Order not found." });

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
               
        [HttpPost("paged")]
        public async Task<IActionResult> GetPaged([FromBody] PagedRequestDto request)
        {
            try
            {
                var result = await _service.GetPagedAsync(request);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
