using FinVentoryAPI.DTOs.AccountDTOs;
using FinVentoryAPI.DTOs.AccountGroupDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.Services.Implementations;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _service;
        public AccountController(IAccountService accountService)
        {
            _service = accountService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateAccountDto dto)
        {
            try
            {
                var result = await _service.CreateAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateAccountDto dto)
        {
            try
            {
                var updated = await _service.UpdateAsync(id, dto);

                if (!updated)
                    return NotFound(new { message = "Account not found." });

                return Ok(new { message = "Account updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);

            if (result == null)
                return NotFound(new { message = "Account not found." });

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);

            if (!deleted)
                return NotFound(new { message = "Account group not found." });

            return Ok(new { message = "Account group deleted successfully." });
        }

        [HttpPost("list")]
        public async Task<IActionResult> GetPaged([FromBody] PagedRequestDto request)
        {
            var result = await _service.GetPagedAsync(request);
            return Ok(result);
        }

        [HttpGet("chart-of-accounts")]
        public async Task<IActionResult> GetChartOfAccounts()
        {
            var result = await _service.GetChartOfAccountsAsync();
            return Ok(result);
        }

        [HttpGet("balancesheet")]
        public async Task<IActionResult> GetBalanceSheetAccounts()
        {
            var data = await _service.GetBalanceSheetAccountsAsync();

            return Ok(new
            {
                success = true,
                data = data
            });
        }

        [HttpGet("sales-accounts")]
        public async Task<IActionResult> GetSalesAccounts()
        {
            try
            {
                var result = await _service.GetSalesAccountsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("deposit-accounts")]
        public async Task<IActionResult> GetDepositeAsync()
        {
            try
            {
                var result = await _service.GetDepositAccountsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("cash-books")]
        public async Task<IActionResult> GetCashBooks()
        {
            var result = await _service.GetCashBooksAsync();
            return Ok(result);
        }
        
        [HttpGet("bank-books")]
        public async Task<IActionResult> GetBankBooks()
        {
            var result = await _service.GetBankBooksAsync();
            return Ok(result);
        }
    }
}
