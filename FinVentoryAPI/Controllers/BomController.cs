using FinVentoryAPI.DTOs.BomDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BomController : ControllerBase
    {
        private readonly IBomService _bomService;

        public BomController(IBomService bomService)
        {
            _bomService = bomService;
        }

        // ── POST /api/bom ─────────────────────────────────────────
        /// <summary>Create a new Bill of Material.</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBomDto dto)
        {
            try
            {
                var result = await _bomService.CreateAsync(dto);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ── PUT /api/bom/{id} ─────────────────────────────────────
        /// <summary>Update an existing BOM (header + lines replaced).</summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBomDto dto)
        {
            try
            {
                var result = await _bomService.UpdateAsync(id, dto);
                if (!result)
                    return NotFound(new { success = false, message = "BOM not found." });

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ── DELETE /api/bom/{id} ──────────────────────────────────
        /// <summary>Soft-delete a BOM.</summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _bomService.DeleteAsync(id);
            if (!result)
                return NotFound(new { success = false, message = "BOM not found." });

            return Ok(new { success = true });
        }

        // ── GET /api/bom/{id} ─────────────────────────────────────
        /// <summary>Get full BOM detail with all component lines.</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _bomService.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { success = false, message = "BOM not found." });

            return Ok(new { success = true, data = result });
        }

        // ── POST /api/bom/paged ───────────────────────────────────
        /// <summary>Paged + filtered BOM list for grids.</summary>
        [HttpPost("paged")]
        public async Task<IActionResult> GetPaged([FromBody] PagedRequestDto request)
        {
            var result = await _bomService.GetPagedAsync(request);
            return Ok(new { success = true, data = result });
        }

        // ── GET /api/bom/by-item/{itemId} ─────────────────────────
        /// <summary>All BOMs that produce a specific finished-good item.</summary>
        [HttpGet("by-item/{itemId:int}")]
        public async Task<IActionResult> GetByItemId(int itemId)
        {
            var result = await _bomService.GetByItemIdAsync(itemId);
            return Ok(new { success = true, data = result });
        }

        // ── PATCH /api/bom/{id}/set-default ──────────────────────
        /// <summary>Mark a BOM as the default for its item.</summary>
        [HttpPatch("{id:int}/set-default")]
        public async Task<IActionResult> SetDefault(int id)
        {
            var result = await _bomService.SetDefaultAsync(id);
            if (!result)
                return NotFound(new { success = false, message = "BOM not found." });

            return Ok(new { success = true });
        }
    }
}
