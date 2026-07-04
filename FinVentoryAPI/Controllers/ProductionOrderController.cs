using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.ProductionDTOs;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductionOrderController : ControllerBase
    {
        private readonly IProductionOrderService _service;

        public ProductionOrderController(IProductionOrderService service)
        {
            _service = service;
        }

        // ─────────────────────────────────────────────────
        // POST api/productionorder
        // ─────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductionOrderDto dto)
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

        // ─────────────────────────────────────────────────
        // PUT api/productionorder/{id}
        // ─────────────────────────────────────────────────
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductionOrderDto dto)
        {
            try
            {
                var result = await _service.UpdateAsync(id, dto);
                if (!result) return NotFound(new { message = "Production order not found." });
                return Ok(new { message = "Updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────
        // DELETE api/productionorder/{id}
        // ─────────────────────────────────────────────────
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _service.DeleteAsync(id);
                if (!result) return NotFound(new { message = "Production order not found." });
                return Ok(new { message = "Deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────
        // GET api/productionorder/{id}
        // ─────────────────────────────────────────────────
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound(new { message = "Production order not found." });
            return Ok(result);
        }

        // ─────────────────────────────────────────────────
        // POST api/productionorder/paged
        // ─────────────────────────────────────────────────
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
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────
        // POST api/productionorder/{id}/inprogress
        // ─────────────────────────────────────────────────
        [HttpPost("{id}/inprogress")]
        public async Task<IActionResult> SetInProgress(int id)
        {
            try
            {
                var result = await _service.SetInProgressAsync(id);
                if (!result) return NotFound(new { message = "Production order not found." });
                return Ok(new { message = "Order moved to In Progress." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────
        // POST api/productionorder/{id}/complete
        // ─────────────────────────────────────────────────
        [HttpPost("{id}/complete")]
        public async Task<IActionResult> Complete(int id, [FromBody] CompleteProductionOrderDto dto)
        {
            try
            {
                var result = await _service.CompleteAsync(id, dto);
                if (!result) return NotFound(new { message = "Production order not found." });
                return Ok(new { message = "Order completed. Stock ledger updated." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────
        // POST api/productionorder/{id}/cancel
        // ─────────────────────────────────────────────────
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var result = await _service.CancelAsync(id);
                if (!result) return NotFound(new { message = "Production order not found." });
                return Ok(new { message = "Order cancelled. Stock reversed if was completed." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
