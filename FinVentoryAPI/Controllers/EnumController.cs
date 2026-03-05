using FinVentoryAPI.Enums;
using FinVentoryAPI.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnumController : ControllerBase
    {
        [HttpGet("group-types")]
        public IActionResult GetGroupTypes()
        {
            return Ok(EnumHelper.GetEnumList<GroupType>());
        }

        [HttpGet("balance-to")]
        public IActionResult GetBalanceTo()
        {
            return Ok(EnumHelper.GetEnumList<BalanceTo>());
        }
    }
}
