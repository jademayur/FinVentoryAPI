namespace FinVentoryAPI.DTOs.AccountGroupDTOs
{
    public class PagedRequestDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public string? Search { get; set; }

        public string? SortBy { get; set; } = "GroupName";
        public string? SortDirection { get; set; } = "asc";

        public int? GroupTypeId { get; set; }
        public int? BalanceToId { get; set; }
        public bool? IsActive { get; set; }
    }
}
