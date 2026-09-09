using FinVentoryAPI.DTOs.DocumentTypeDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IDocumentTypeService
    {
        Task<DocumentTypeResponseDto> CreateAsync(CreateDocumentTypeDto dto);
        Task<List<DocumentTypeResponseDto>> GetAllAsync();
        Task<DocumentTypeResponseDto?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(int id, UpdateDocumentTypeDto dto);
        Task<bool> DeleteAsync(int id);
        Task<PagedResponseDto<DocumentTypeResponseDto>> GetPagedAsync(PagedRequestDto request);
    }
}
