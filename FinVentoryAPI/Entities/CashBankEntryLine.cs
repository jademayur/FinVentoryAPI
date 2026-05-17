using FinVentoryAPI.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class CashBankEntryLine
    {
        [Key]
        public int CashBankEntryLineId { get; set; }

        [Required]
        public int CashBankEntryId { get; set; }

        [Required]
        public int AccountId { get; set; }

        [Required]
        public BalanceType DrCr { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [MaxLength(200)]
        public string? Narration { get; set; }

        public int SortOrder { get; set; }

        // ── Navigation ───────────────────────────────────────────────
        [ForeignKey(nameof(CashBankEntryId))]
        public CashBankEntry CashBankEntry { get; set; } = null!;

        [ForeignKey(nameof(AccountId))]
        public Account Account { get; set; } = null!;
    }
}
