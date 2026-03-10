using FinVentoryAPI.Enums;

namespace FinVentoryAPI.Entities
{
    public class Account : BaseEntity
    {
        public int AccountId { get; set; }
        public int CompanyId { get; set; }
        public string? AccountCode { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public int AccountGroupId { get; set; }
        public AccountType AccountType { get; set; } 
        public BookType? BookType { get; set; }
        public BookSubType? BookSubType { get; set; } 


    }
}
