namespace FinVentoryAPI.DTOs.AccountDTOs
{
    public class ChartOfAccountDTO
    {
        public class ChartOfAccountNodeDto
        {
            public int Id { get; set; }

            public string Name { get; set; } = string.Empty;

            // Group or Account
            public string NodeType { get; set; } = string.Empty;

            public string? AccountCode { get; set; }

            public List<ChartOfAccountNodeDto> Children { get; set; } = new();
        }
    }
}
