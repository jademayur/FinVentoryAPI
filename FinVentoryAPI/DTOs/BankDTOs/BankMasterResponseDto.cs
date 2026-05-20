namespace FinVentoryAPI.DTOs.BankDTOs
{
    public class BankMasterResponseDto
    {
        public int BankId { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public string AccountNo { get; set; } = string.Empty;
        public string SwiftCode { get; set; } = string.Empty;
        public string IFSCCode { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
