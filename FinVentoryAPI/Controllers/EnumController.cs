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

        [HttpGet("book-types")]
        public IActionResult GetBookType()
        {
            return Ok(EnumHelper.GetEnumList<BookType>());
        }

        [HttpGet("account-types")]
        public IActionResult GetAccountType()
        {
            return Ok(EnumHelper.GetEnumList<AccountType>());
        }

        [HttpGet("book-sub-types")]
        public IActionResult GetBookSubType()
        {
            return Ok(EnumHelper.GetEnumList<BookSubType>());
        }
    }
}
