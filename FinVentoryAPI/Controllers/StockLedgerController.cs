// Controllers/StockLedgerController.cs
using FinVentoryAPI.DTOs.StockLedgerDTOs;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockLedgerController : ControllerBase
    {
        private readonly IStockLedgerService _service;
        private readonly Common _common;

        public StockLedgerController(IStockLedgerService service, Common common)
        {
            _service = service;
            _common = common;
        }

        // ── GET /api/StockLedger/{itemId}
        // Returns full ledger with running balance for one item
        [HttpGet("{itemId}")]
        public async Task<IActionResult> GetByItem(
            int itemId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var result = await _service.GetLedgerByItemAsync(itemId, from, to);
            if (result == null) return NotFound(new { message = "Item not found." });
            return Ok(result);
        }

        // ── GET /api/StockLedger
        // Returns ledger summary for ALL items (optional group filter)
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int? itemGroupId)
        {
            var result = await _service.GetAllLedgersAsync(from, to, itemGroupId);
            return Ok(result);
        }

        // ── POST /api/StockLedger
        // Manual entry (opening stock, adjustments)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStockLedgerDto dto)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();

            await _service.AddEntryAsync(
                companyId: companyId,
                itemId: dto.ItemId,
                warehouseId: dto.WarehouseId,
                date: dto.Date,
                voucherType: dto.VoucherType,
                voucherNo: dto.VoucherNo,
                businessPartnerId: dto.BusinessPartnerId,
                qty: dto.Qty,
                rate: dto.Rate,
                remarks: dto.Remarks,
                createdBy: userId
            );

            return Ok(new { message = "Stock ledger entry created successfully." });
        }

        // ── DELETE /api/StockLedger/{ledgerId}
        // Soft delete a single entry
        [HttpDelete("{ledgerId}")]
        public async Task<IActionResult> Delete(int ledgerId)
        {
            var result = await _service.DeleteEntryAsync(ledgerId);
            if (!result) return NotFound(new { message = "Ledger entry not found." });
            return Ok(new { message = "Entry deleted successfully." });
        }
    }
}