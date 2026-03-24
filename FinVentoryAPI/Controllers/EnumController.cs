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

        [HttpGet("item-types")]
        public IActionResult GetItemType() 
        {
            return Ok(EnumHelper.GetEnumList<ItemType>());
        }

        [HttpGet("item-categories")]
        public IActionResult GetItemCategory()
        {
            return Ok(EnumHelper.GetEnumList<ItemCategory>());
        }

        [HttpGet("item-manage-by")]
        public IActionResult GetItemManageBy()
        {
            return Ok(EnumHelper.GetEnumList<ItemManageBy>());
        }

        [HttpGet("costing-methods")]
        public IActionResult GetCostingMethod() 
        { 
            return Ok(EnumHelper.GetEnumList<CostingMethod>());
        }

        [HttpGet("base-units")]
        public IActionResult GetBaseUnit()
        {
            return Ok(EnumHelper.GetEnumList<BaseUnit>());
        }

        // ── Alternate Units filtered by selected Base Unit ─────────
        // GET /api/Enum/alternate-units?baseUnitId=1
        [HttpGet("alternate-units")]
        public IActionResult GetAlternativeUnit([FromQuery] int baseUnitId)
        {
            if (!Enum.IsDefined(typeof(BaseUnit), baseUnitId))
                return BadRequest("Invalid base unit.");

            var baseUnit = (BaseUnit)baseUnitId;

            // Get only the AlternateUnit values that have a valid factor for this base unit
            var validAlternates = EnumHelper.Factors
                .Where(kvp => kvp.Key.Item1 == baseUnit)
                .Select(kvp => new
                {
                    id = Convert.ToInt32(kvp.Key.Item2),
                    name = EnumHelper.GetDisplayName(kvp.Key.Item2),
                    factor = kvp.Value          // auto-fill conversion factor on frontend
                })
                .ToList();

            return Ok(validAlternates);
        }

        // ── Get conversion factor for a specific pair ──────────────
        // GET /api/Enum/conversion-factor?baseUnitId=1&altUnitId=2
        [HttpGet("conversion-factor")]
        public IActionResult GetConversionFactor([FromQuery] int baseUnitId, [FromQuery] int altUnitId)
        {
            if (!Enum.IsDefined(typeof(BaseUnit), baseUnitId)) return BadRequest("Invalid base unit.");
            if (!Enum.IsDefined(typeof(AlternateUnit), altUnitId)) return BadRequest("Invalid alternate unit.");

            var baseUnit = (BaseUnit)baseUnitId;
            var altUnit = (AlternateUnit)altUnitId;
            var factor = EnumHelper.GetFactor(baseUnit, altUnit);

            if (factor == null)
                return NotFound(new { message = "No conversion factor defined for this pair." });

            return Ok(new { factor });
        }

        [HttpGet("business-partner-types")]
        public IActionResult GetBusinessPartnerType()
        {
            return Ok(EnumHelper.GetEnumList<BusinessPartnerType>());
        }

        [HttpGet("address-types")]
        public IActionResult GetAddressType()
        {
            return Ok(EnumHelper.GetEnumList<AddressType>());
        }

        [HttpGet("gst-types")]
        public IActionResult GetGstType()
        {
            return Ok(EnumHelper.GetEnumList<GSTType>());
        }

        [HttpGet("BalanceType")]
        public IActionResult GetBalanceType()
        {
            return Ok(EnumHelper.GetEnumList<BalanceType>());
        }

    }
}
