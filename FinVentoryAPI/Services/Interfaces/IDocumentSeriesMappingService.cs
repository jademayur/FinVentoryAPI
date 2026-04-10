using FinVentoryAPI.DTOs.DocumentSeriesMappingDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IDocumentSeriesMappingService
    {
        Task<List<DocumentSeriesMappingResponseDto>> GetAllAsync();
        Task<DocumentSeriesMappingResponseDto?> GetByIdAsync(int id);
        Task<DocumentSeriesMappingResponseDto> CreateAsync(CreateDocumentSeriesMappingDto dto);
        Task<bool> UpdateAsync(int id, CreateDocumentSeriesMappingDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
