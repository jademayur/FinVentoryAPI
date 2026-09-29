using FinVentoryAPI.DTOs.RoleDTOs;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _service;

        public RoleController(IRoleService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
            => Ok(await _service.GetByIdAsync(id));

        [HttpPost]
        [Authorize(Roles = "applAdmin")]
        public async Task<IActionResult> Create(RoleCreateDto dto)
            => Ok(await _service.CreateAsync(dto));

        [HttpPut]
        [Authorize(Roles = "applAdmin")]
        public async Task<IActionResult> Update(RoleUpdateDto dto)
            => Ok(await _service.UpdateAsync(dto));

        [HttpDelete("{id}")]
        [Authorize(Roles = "applAdmin")]
        public async Task<IActionResult> Delete(int id)
            => Ok(await _service.DeleteAsync(id));
    }
}
