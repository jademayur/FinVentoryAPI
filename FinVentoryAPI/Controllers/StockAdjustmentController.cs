using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.StockAdjustmentDTOs;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StockAdjustmentController : ControllerBase
    {
        private readonly IStockAdjustmentService _service;
        public StockAdjustmentController(IStockAdjustmentService service) => _service = service;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStockAdjustmentMainDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var result = await _service.CreateAsync(dto);
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateStockAdjustmentMainDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var result = await _service.UpdateAsync(id, dto);
                if (!result) return NotFound(new { message = "Adjustment not found or not in Draft status." });
                return Ok(new { message = "Adjustment updated successfully." });
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result) return NotFound(new { message = "Adjustment not found or not in Draft status." });
            return Ok(new { message = "Adjustment deleted successfully." });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound(new { message = "Adjustment not found." });
            return Ok(result);
        }

        [HttpPost("list")]
        public async Task<IActionResult> GetPaged([FromBody] PagedRequestDto request)
        {
            var result = await _service.GetPagedAsync(request);
            return Ok(result);
        }

        [HttpPatch("{id}/confirm")]
        public async Task<IActionResult> Confirm(int id)
        {
            try
            {
                var result = await _service.ConfirmAsync(id);
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPatch("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var result = await _service.CancelAsync(id);
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }
    }
}
