using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.JobWorkIssueDTOs;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobWorkIssueController : ControllerBase
    {
        private readonly IJobWorkIssueService _service;
        public JobWorkIssueController(IJobWorkIssueService service) { _service = service; }

        [HttpGet] public async Task<IActionResult> GetAll() { try { var r = await _service.GetPagedAsync(new PagedRequestDto { PageNumber = 1, PageSize = 1000 }); return Ok(r.Data); } catch (Exception ex) { return BadRequest(new { message = ex.Message }); } }
        [HttpGet("{id}")] public async Task<IActionResult> GetById(int id) { try { var r = await _service.GetByIdAsync(id); if (r == null) return NotFound(new { message = "Not found." }); return Ok(r); } catch (Exception ex) { return BadRequest(new { message = ex.Message }); } }
        [HttpPost] public async Task<IActionResult> Create([FromBody] CreateJobWorkIssueMainDto dto) { try { return Ok(await _service.CreateAsync(dto)); } catch (Exception ex) { return BadRequest(new { message = ex.Message }); } }
        [HttpPut("{id}")] public async Task<IActionResult> Update(int id, [FromBody] UpdateJobWorkIssueMainDto dto) { try { var r = await _service.UpdateAsync(id, dto); if (r == null) return NotFound(new { message = "Not found or not Draft." }); return Ok(r); } catch (Exception ex) { return BadRequest(new { message = ex.Message }); } }
        [HttpDelete("{id}")] public async Task<IActionResult> Delete(int id) { try { return Ok(new { deleted = await _service.DeleteAsync(id) }); } catch (Exception ex) { return BadRequest(new { message = ex.Message }); } }
        [HttpPost("paged")] public async Task<IActionResult> GetPaged([FromBody] PagedRequestDto request) { try { return Ok(await _service.GetPagedAsync(request)); } catch (Exception ex) { return BadRequest(new { message = ex.Message }); } }
        [HttpPatch("{id}/confirm")] public async Task<IActionResult> Confirm(int id) { try { return Ok(await _service.ConfirmAsync(id)); } catch (Exception ex) { return BadRequest(new { message = ex.Message }); } }
        [HttpPatch("{id}/cancel")] public async Task<IActionResult> Cancel(int id) { try { return Ok(await _service.CancelAsync(id)); } catch (Exception ex) { return BadRequest(new { message = ex.Message }); } }
    }
}
