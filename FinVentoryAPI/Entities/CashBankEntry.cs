using FinVentoryAPI.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class CashBankEntry : BaseEntity
    {
        [Key]
        public int CashBankEntryId { get; set; }
        public int CompanyId { get; set; }
        public int FinYearId { get; set; }

        [Required]
        [MaxLength(20)]
        public string EntryNumber { get; set; } = null!;

        [Required]
        public DateOnly EntryDate { get; set; }

        [Required]
        public EntryType EntryType { get; set; }

        /// <summary>FK to Cash or Bank account (head account for this entry).</summary>
        [Required]
        public int HeadAccountId { get; set; }

        /// <summary>Dr for Receipt, Cr for Payment.</summary>
        [Required]
        public BalanceType AccountDrCr { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [MaxLength(50)]
        public string? PaymentMode { get; set; }

        [MaxLength(100)]
        public string? ReferenceNo { get; set; }

        public DateOnly? ReferenceDate { get; set; }

        [MaxLength(500)]
        public string? Narration { get; set; }

        [Required]
        public string Status { get; set; } = "Draft";

    

        // ── Navigation ───────────────────────────────────────────────
        [ForeignKey(nameof(HeadAccountId))]
        public Account CashBankAccount { get; set; } = null!;

        public ICollection<CashBankEntryLine> Lines { get; set; } = new List<CashBankEntryLine>();
    }
}
