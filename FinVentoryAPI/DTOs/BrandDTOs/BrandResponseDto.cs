namespace FinVentoryAPI.DTOs.BrandDTOs
{
    public class BrandResponseDto
    {
        public int BrandId { get; set; }
        public string BrandName { get; set;} = string.Empty;
        public bool IsActive { get; set; }
    }
}
