using FinVentoryAPI.DTOs.JournalEntryDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IJournalEntryService
    {
        Task<JournalEntryResponseDto> CreateAsync(CreateJournalEntryDto dto);
        Task<bool> UpdateAsync(int id, UpdateJournalEntryDto dto);
        Task<bool> DeleteAsync(int id);
        Task<JournalEntryResponseDto?> GetByIdAsync(int id);
        Task<List<JournalEntryResponseDto>> GetAllAsync();
        Task<PagedResponseDto<JournalEntryResponseDto>> GetPagedAsync(PagedRequestDto request);
    }
}
