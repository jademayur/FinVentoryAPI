using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class ItemBatch : BaseEntity
    {
        public int BatchId { get; set; }
        public int CompanyId { get; set; }
        public int FinYearId { get; set; }
        public int ItemId { get; set; }

        public string BatchNo { get; set; } = string.Empty;      // e.g. "BATCH-2025-001"
        public DateTime? ManufactureDate { get; set; }
        public DateTime? ExpiryDate { get; set; }

        /// <summary>Total qty received into this batch across all GRNs.</summary>
        public decimal ReceivedQty { get; set; }

        /// <summary>Qty already consumed / sold. Updated on every sales/consumption posting.</summary>
        public decimal UsedQty { get; set; }

        /// <summary>ReceivedQty - UsedQty — kept denormalised for fast queries.</summary>
        public decimal AvailableQty { get; set; }

        public string? Remarks { get; set; }

        // ── Navigation ──────────────────────────────────────
        [ForeignKey(nameof(ItemId))]
        public Item? Item { get; set; }

        public ICollection<SalesInvoiceDetailBatch>? SalesInvoiceDetailBatches { get; set; }
    }
}
