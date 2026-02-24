using FinVentoryAPI.DTOs.UserCompany;
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
            => Ok(await _service.GetAllAsync());

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId)
            => Ok(await _service.GetByUserAsync(userId));

        [HttpPost]
        public async Task<IActionResult> Create(UserCompanyCreateDto dto)
            => Ok(await _service.CreateAsync(dto));

        [HttpPut]
        public async Task<IActionResult> Update(UserCompanyUpdateDto dto)
            => Ok(await _service.UpdateAsync(dto));

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
            => Ok(await _service.DeleteAsync(id));
    }
}
