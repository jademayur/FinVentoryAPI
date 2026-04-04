using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.SeriesDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IDocumentSeriesService
    {
        Task<IEnumerable<SeriesResponseDto>> GetAllAsync();
        Task<SeriesResponseDto?> GetByIdAsync(int seriesId);
        Task<SeriesResponseDto> CreateAsync(CreateSeriesDto dto);
        Task<SeriesResponseDto?> UpdateAsync(int seriesId, UpdateSeriesDto dto);
        Task<bool> DeleteAsync(int seriesId);
        Task<SeriesResponseDto?> GetDefaultSeriesAsync(string documentType);
        Task<bool> SetAsDefaultAsync(int seriesId);
        Task<string> GenerateNextNumberAsync(int seriesId);
        Task<PagedResponseDto<SeriesResponseDto>> GetPagedAsync(PagedRequestDto request);
    }
}
