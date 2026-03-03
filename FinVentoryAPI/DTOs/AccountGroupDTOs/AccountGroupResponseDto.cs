namespace FinVentoryAPI.DTOs.AccountGroupDTOs
{
    public class AccountGroupResponseDto
    {
        public int AccountGroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public int? ParentGroupId { get; set; }
        public string ParentGroupName { get; set; }
        = string.Empty;
        public int GroupTypeId { get; set; }
        public string GroupTypeName { get; set; }= string.Empty;

        public int BalanceToId { get; set; }
        public string BalanceToName { get; set; } = string.Empty;

        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate {  get; set; }

    }
}
