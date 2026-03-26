using FinVentoryAPI.DTOs.OpeningItemBalanceDTOs;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OpeningItemBalanceController : ControllerBase
    {
        private IOpeningItemBalanceService _service;

        public OpeningItemBalanceController(IOpeningItemBalanceService openingItemBalanceService)
        {
            _service = openingItemBalanceService;
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] OpeningBalanceItemDto dto)
        {
            try
            {
                var result = await _service.SaveAsync(dto);
                return Ok(new
                {
                    success = true,
                    message = "Opening balance saved successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var data = await _service.GetAsync();

            return Ok(new
            {
                success = true,
                data = data
            });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete()
        {
            var result = await _service.DeleteAsync();

            if (!result)
            {
                return NotFound(new
                {
                    success = false,
                    message = "No opening balance found to delete"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Opening balance deleted successfully"
            });
        }
    }
}
