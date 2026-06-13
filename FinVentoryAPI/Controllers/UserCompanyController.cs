using FinVentoryAPI.DTOs.UserCompany;
using FinVentoryAPI.Services.Implementations;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserCompanyController : ControllerBase
    {
        private readonly IUserCompanyService _service;

        public UserCompanyController(IUserCompanyService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserCompanyCreateDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);

            if (!result)
                return NotFound();

            return NoContent();
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UserCompanyCreateDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);

            if (!result)
                return NotFound();

            return Ok(new
            {
                message = "User access updated successfully."
            });
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> BulkCreate(UserCompanyBulkCreateDto dto)
        {
            var count = await _service.BulkCreateAsync(dto);
            return Ok(new { inserted = count });
        }

    }
}
