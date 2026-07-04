using FinVentoryAPI.DTOs.GRNDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GRNController : ControllerBase
    {
        private readonly IGRNService _service;

        public GRNController(IGRNService service)
        {
            _service = service;
        }

        // ── CRUD ─────────────────────────────────────────────────────────────

        /// <summary>Creates a new GRN (status = Draft).</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateGRNMainDto dto)
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

        /// <summary>Updates a Draft GRN.</summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateGRNMainDto dto)
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

        /// <summary>Soft-deletes a Draft GRN.</summary>
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

        // ── Workflow ─────────────────────────────────────────────────────────

        /// <summary>Confirms a Draft GRN (Draft → Confirmed).</summary>
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

        /// <summary>Cancels a Draft or Confirmed GRN.</summary>
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

        // ── Queries ───────────────────────────────────────────────────────────

        /// <summary>Returns all GRNs for the current company (use for small datasets).</summary>
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

        /// <summary>Returns a single GRN by ID.</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _service.GetByIdAsync(id);
                if (result == null) return NotFound(new { success = false, message = "GRN not found." });
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>Returns a paginated, filtered, sorted list of GRNs.</summary>
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

        // ── Form helpers ─────────────────────────────────────────────────────

        /// <summary>
        /// Returns confirmed Purchase Orders for a supplier that still have
        /// pending (un-received) quantity on at least one line.
        /// Used to populate the PO picker before creating a GRN.
        /// </summary>
        [HttpGet("purchase-orders/{businessPartnerId:int}")]
        public async Task<IActionResult> GetPurchaseOrdersForSupplier(int businessPartnerId)
        {
            try
            {
                var result = await _service.GetPurchaseOrdersForSupplierAsync(businessPartnerId);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Given selected PO IDs, returns pre-filled header + line data
        /// ready to bind into the GRN entry form.
        /// </summary>
        [HttpPost("prefill")]
        public async Task<IActionResult> GetPrefill([FromBody] List<int> purchaseOrderIds)
        {
            try
            {
                var result = await _service.GetGRNPrefillAsync(purchaseOrderIds);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
