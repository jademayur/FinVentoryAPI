using FinVentoryAPI.Entities;
using FinVentoryAPI.Services.Implementations;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuGroupController : ControllerBase
    {
        private readonly IMenuGroupService _menuGroupService;
        public MenuGroupController(IMenuGroupService menuGroupService)
        {
            _menuGroupService = menuGroupService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var MenuGroup = await _menuGroupService.GetAllAsync();
            return Ok(MenuGroup);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var MenuGroup = await _menuGroupService.GetByIdAsync(id);

            if (MenuGroup == null)
                return NotFound(new { message = "Menu Group not found" });

            return Ok(MenuGroup);
        }
    }
}
