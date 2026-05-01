using FinVentoryAPI.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class ItemSerial : BaseEntity
    {
        public int SerialId { get; set; }
        public int CompanyId { get; set; }
        public int? FinYearId { get; set; }

        public int ItemId { get; set; }

        public string SerialNo { get; set; } = string.Empty;   // e.g. "SN-ABC-00123"

        /// <summary>Current lifecycle status of this unit.</summary>
        public SerialStatus Status { get; set; } = SerialStatus.InStock;

        public DateTime? PurchaseDate { get; set; }
        public DateTime? WarrantyExpiry { get; set; }
        public string? Remarks { get; set; }

        // ── Navigation ──────────────────────────────────────
        [ForeignKey(nameof(ItemId))]
        public Item? Item { get; set; }

        public ICollection<SalesInvoiceDetailSerial>? SalesInvoiceDetailSerials { get; set; }
    }

   
}

