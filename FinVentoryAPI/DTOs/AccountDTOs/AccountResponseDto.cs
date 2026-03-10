namespace FinVentoryAPI.DTOs.AccountDTOs
{
    public class AccountResponseDto
    {
        public int AccountId { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public int AccountGroupId { get; set; }
        public string AccountGroupName { get; set; } = string.Empty;
        public string? AccountCode { get; set; }
        public int AccountTypeId { get; set; }
        public string AccountTypeName { get; set; } = string.Empty;
        public int? BookTypeId { get; set; }
        public string? BookTypeName { get; set; }
        public int? BookSubTypeId { get; set; }
        public string? BookSubTypeName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
