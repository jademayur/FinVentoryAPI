using FinVentoryAPI.DTOs.CompanyConfigDTOs;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CompanyConfigController : ControllerBase
    {
        private readonly ICompanyConfigService _service;
        private readonly Common _common;

        public CompanyConfigController(ICompanyConfigService service, Common common)
        {
            _service = service;
            _common = common;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var companyId = _common.GetCompanyId();
            var result = await _service.GetAllAsync(companyId);
            return Ok(result);
        }

        [HttpGet("key/{key}")]
        public async Task<IActionResult> GetByKey(string key)
        {
            var companyId = _common.GetCompanyId();
            var result = await _service.GetByKeyAsync(companyId, key);
            if (result == null)
                return NotFound(new { message = $"Config '{key}' not found." });
            return Ok(result);
        }

        [HttpGet("value/{key}")]
        public async Task<IActionResult> GetValue(string key)
        {
            var companyId = _common.GetCompanyId();
            var value = await _service.GetValueAsync(companyId, key);
            return Ok(new { key, value });
        }

        [HttpPost]
        public async Task<IActionResult> Upsert([FromBody] UpsertCompanyConfigDto dto)
        {
            var companyId = _common.GetCompanyId();
            await _service.UpsertAsync(companyId, dto);
            return Ok(new { message = "Config saved." });
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> BulkUpsert([FromBody] BulkUpsertCompanyConfigDto dto)
        {
            var companyId = _common.GetCompanyId();
            await _service.BulkUpsertAsync(companyId, dto);
            return Ok(new { message = $"{dto.Configs.Count} configs saved." });
        }

        [HttpDelete("{configId}")]
        public async Task<IActionResult> Delete(int configId)
        {
            var companyId = _common.GetCompanyId();
            await _service.DeleteAsync(companyId, configId);
            return Ok(new { message = "Config deleted." });
        }
    }
}
