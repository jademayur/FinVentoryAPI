namespace FinVentoryAPI.DTOs.ItemGroupDTOs
{
    public class ItemGroupResponseDto
    {
        public int ItemGroupId { get; set; }      
        public string ItemGroupName { get; set; } = string.Empty;
        public int? ParentGroupId { get; set; }
        public string? ParentGroupName { get; set; }
        public string? GroupCode { get; set; }
        public bool IsActive { get; set; }

    }
}
