namespace FinVentoryAPI.DTOs.PagedRequestDto
{
    public class PagedRequestDto
    {
        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // Search / Filter
        public string? Search { get; set; }

        // Optional filters (can be extended)
        public Dictionary<string, object>? Filters { get; set; }

        // Sorting
        public List<SortDto>? Sorts { get; set; }
    }

    public class SortDto
    {
        public string Column { get; set; } = string.Empty;  // column name
        public string Direction { get; set; } = "asc";      // "asc" or "desc"
    }
}
