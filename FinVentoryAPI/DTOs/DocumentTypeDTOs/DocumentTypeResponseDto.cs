namespace FinVentoryAPI.DTOs.DocumentTypeDTOs
{
    public class DocumentTypeResponseDto
    {
        public int DocumentTypeId { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
