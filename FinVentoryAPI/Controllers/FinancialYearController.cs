using FinVentoryAPI.DTOs.FinancialYearDTOs;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FinancialYearController : ControllerBase
    {
        private readonly IFinancialYearService _service;

        public FinancialYearController(IFinancialYearService service)
        {
            _service = service;
        }

        [HttpGet("{companyId}")]
        public async Task<IActionResult> GetByCompany(int companyId)
        {
            return Ok(await _service.GetByCompanyAsync(companyId));
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateFinancialYearDto dto)
        {
            return Ok(await _service.CreateAsync(dto));
        }

        [HttpPut]
        public async Task<IActionResult> Update(UpdateFinancialYearDto dto)
        {
            return Ok(await _service.UpdateAsync(dto));
        }

        [HttpPut("close/{id}")]
        public async Task<IActionResult> Close(int id)
        {
            return Ok(await _service.CloseAsync(id));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return Ok(await _service.DeleteAsync(id));
        }
    }
}
