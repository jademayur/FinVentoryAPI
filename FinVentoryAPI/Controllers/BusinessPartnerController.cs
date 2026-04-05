using FinVentoryAPI.DTOs.BusinessPartnerDTOs;
using FinVentoryAPI.DTOs.ItemDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.Services.Implementations;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusinessPartnerController : ControllerBase
    {
        private readonly IBusinessPartnerService _service;

        public BusinessPartnerController(IBusinessPartnerService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBusinessPartnerDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _service.CreateAsync(dto);
                return Ok(result);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                // Unwrap to get the actual SQL/DB error
                var inner = dbEx.InnerException?.InnerException?.Message
                         ?? dbEx.InnerException?.Message
                         ?? dbEx.Message;
                return BadRequest(new { message = inner });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            //if (!ModelState.IsValid)
            //    return BadRequest(ModelState);

            //try
            //{
            //    var result = await _service.CreateAsync(dto);
            //    return Ok(result);
            //}
            //catch (Exception ex)
            //{
            //    // Return structured error so Angular can read err?.error?.message
            //    return BadRequest(new { message = ex.Message });
            //}
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBusinessPartnerDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _service.UpdateAsync(id, dto);
                if (!result)
                    return NotFound(new { message = "Business Partner not found." });

                return Ok(new { message = "Business Partner updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);

            if (result == null)
                return NotFound(new { message = "Business Partner found." });

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);

            if (!deleted)
                return NotFound(new { message = "Business Partner not found." });

            return Ok(new { message = "Business Partner deleted successfully." });
        }

        [HttpPost("list")]
        public async Task<IActionResult> GetPaged([FromBody] PagedRequestDto request)
        {
            var result = await _service.GetPagedAsync(request);
            return Ok(result);
        }

        [HttpGet("{businessPartnerId}/invoice-defaults")]
        public async Task<IActionResult> GetInvoiceDefaults(int businessPartnerId)
        {
            try
            {
                var result = await _service.GetInvoiceDefaultsByBPAsync(businessPartnerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ────────────────────────────────────────────────────
        // GET api/BusinessPartner/{businessPartnerId}/addresses
        // ────────────────────────────────────────────────────
        [HttpGet("{businessPartnerId}/addresses")]
        public async Task<IActionResult> GetAddresses(int businessPartnerId)
        {
            try
            {
                var result = await _service.GetAddressesByBPAsync(businessPartnerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ────────────────────────────────────────────────────
        // GET api/BusinessPartner/{businessPartnerId}/addresses/bill
        // ────────────────────────────────────────────────────
        [HttpGet("{businessPartnerId}/addresses/bill")]
        public async Task<IActionResult> GetBillAddresses(int businessPartnerId)
        {
            try
            {
                var result = await _service.GetBillAddressesByBPAsync(businessPartnerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ────────────────────────────────────────────────────
        // GET api/BusinessPartner/{businessPartnerId}/addresses/ship
        // ────────────────────────────────────────────────────
        [HttpGet("{businessPartnerId}/addresses/ship")]
        public async Task<IActionResult> GetShipAddresses(int businessPartnerId)
        {
            try
            {
                var result = await _service.GetShipAddressesByBPAsync(businessPartnerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ────────────────────────────────────────────────────
        // GET api/BusinessPartner/{businessPartnerId}/addresses/{addressId}
        // ────────────────────────────────────────────────────
        [HttpGet("{businessPartnerId}/addresses/{addressId:int}")]
        public async Task<IActionResult> GetAddressById(int businessPartnerId, int addressId)
        {
            try
            {
                var result = await _service.GetAddressByIdAsync(businessPartnerId, addressId);

                if (result == null)
                    return NotFound(new { message = "Address not found." });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ────────────────────────────────────────────────────
        // GET api/BusinessPartner/{businessPartnerId}/contacts
        // ────────────────────────────────────────────────────
        [HttpGet("{businessPartnerId}/contacts")]
        public async Task<IActionResult> GetContacts(int businessPartnerId)
        {
            try
            {
                var result = await _service.GetContactsByBPAsync(businessPartnerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}
