namespace FinVentoryAPI.DTOs.AccountGroupDTOs
{
    public class PagedResponseDto<T>
    {
        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public List<T> Data { get; set; } = new();
    }
}
