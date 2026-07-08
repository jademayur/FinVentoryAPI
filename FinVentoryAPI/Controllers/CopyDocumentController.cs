using FinVentoryAPI.DTOs.CopyDocumentDTOs;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CopyDocumentController : ControllerBase
    {
        private readonly ICopyDocumentService _copyService;

        public CopyDocumentController(ICopyDocumentService copyService)
        {
            _copyService = copyService;
        }

        // ════════════════════════════════════════════════════
        // SALES ORDER → SALES INVOICE
        // ════════════════════════════════════════════════════

        /// <summary>
        /// GET: Returns all pending lines from a Sales Order ready to copy into an Invoice.
        /// Full pending qty used for all lines.
        /// </summary>
        [HttpGet("sales-order-to-invoice/{id:int}")]
        public async Task<IActionResult> SalesOrderToInvoice(int id)
        {
            try
            {
                var result = await _copyService.CopySalesOrderToInvoiceAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// POST: Same as GET but accepts QtyOverrides for partial-qty copy.
        /// Body: { "qtyOverrides": { "101": 3, "102": 5 } }
        /// </summary>
        [HttpPost("sales-order-to-invoice/{id:int}")]
        public async Task<IActionResult> SalesOrderToInvoicePartial(
            int id, [FromBody] CopyRequestDto request)
        {
            try
            {
                var result = await _copyService.CopySalesOrderToInvoiceAsync(id, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ════════════════════════════════════════════════════
        // SALES INVOICE → SALES RETURN
        // ════════════════════════════════════════════════════

        /// <summary>
        /// GET: Returns all pending (not yet returned) lines from a Sales Invoice.
        /// </summary>
        [HttpGet("sales-invoice-to-return/{id:int}")]
        public async Task<IActionResult> SalesInvoiceToReturn(int id)
        {
            try
            {
                var result = await _copyService.CopySalesInvoiceToReturnAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// POST: Partial-qty copy from Sales Invoice to Return.
        /// </summary>
        [HttpPost("sales-invoice-to-return/{id:int}")]
        public async Task<IActionResult> SalesInvoiceToReturnPartial(
            int id, [FromBody] CopyRequestDto request)
        {
            try
            {
                var result = await _copyService.CopySalesInvoiceToReturnAsync(id, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ════════════════════════════════════════════════════
        // QUOTATION → SALES ORDER
        // ════════════════════════════════════════════════════

        [HttpGet("quotation-to-sales-order/{id:int}")]
        public async Task<IActionResult> QuotationToSalesOrder(int id)
        {
            try
            {
                var result = await _copyService.CopyQuotationToSalesOrderAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("quotation-to-sales-order/{id:int}")]
        public async Task<IActionResult> QuotationToSalesOrderPartial(
            int id, [FromBody] CopyRequestDto request)
        {
            try
            {
                var result = await _copyService.CopyQuotationToSalesOrderAsync(id, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ════════════════════════════════════════════════════
        // PURCHASE ORDER → PURCHASE INVOICE
        // ════════════════════════════════════════════════════

        [HttpGet("purchase-order-to-invoice/{id:int}")]
        public async Task<IActionResult> PurchaseOrderToInvoice(int id)
        {
            try
            {
                var result = await _copyService.CopyPurchaseOrderToInvoiceAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("purchase-order-to-invoice/{id:int}")]
        public async Task<IActionResult> PurchaseOrderToInvoicePartial(
            int id, [FromBody] CopyRequestDto request)
        {
            try
            {
                var result = await _copyService.CopyPurchaseOrderToInvoiceAsync(id, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ════════════════════════════════════════════════════
        // PURCHASE INVOICE → PURCHASE RETURN
        // ════════════════════════════════════════════════════

        [HttpGet("purchase-invoice-to-return/{id:int}")]
        public async Task<IActionResult> PurchaseInvoiceToReturn(int id)
        {
            try
            {
                var result = await _copyService.CopyPurchaseInvoiceToReturnAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("purchase-invoice-to-return/{id:int}")]
        public async Task<IActionResult> PurchaseInvoiceToReturnPartial(
            int id, [FromBody] CopyRequestDto request)
        {
            try
            {
                var result = await _copyService.CopyPurchaseInvoiceToReturnAsync(id, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
