using FinVentoryAPI.DTOs.CompanySelectionDtos;
using FinVentoryAPI.DTOs.LoginDtos;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _service;

        public AuthController(IAuthService service)
        {
            _service = service;
        }

        // Phase 1: Login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result = await _service.LoginAsync(dto);

            if (result == null)
                return Unauthorized("Invalid credentials");

            return Ok(result);
        }

        // Phase 2: Select Company
        [HttpPost("select-company")]
        public async Task<IActionResult> SelectCompany(CompanySelectionDto dto)
        {
            var token = await _service.GenerateTokenAsync(dto);

            if (token == null)
                return Unauthorized("Invalid company selection");

            return Ok(new { token });
        }
    }
}
