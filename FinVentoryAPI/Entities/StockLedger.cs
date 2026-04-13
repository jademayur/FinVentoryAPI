using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class StockLedger:BaseEntity
    {
        [Key]
        public int LedgerId { get; set; }
        public int CompanyId { get; set; }
        public int ItemId { get; set; }
        public int? WarehouseId { get; set; }
        public DateTime Date { get; set; }
        public string? VoucherType { get; set; }   // "Purchase", "Sale", "Transfer", "Adjustment"
        public string? VoucherNo { get; set; }
        public int? BusinessPartnerId { get; set; }
        public decimal Qty { get; set; }           // positive = IN, negative = OUT
        public decimal? Rate { get; set; }
        public string? Remarks { get; set; }

        // Navigation
        [ForeignKey(nameof(ItemId))]
        public Item? Item { get; set; }

        [ForeignKey(nameof(WarehouseId))]
        public Warehouse? Warehouse { get; set; }

        [ForeignKey(nameof(BusinessPartnerId))]
        public BusinessPartner? BusinessPartner { get; set; }
    }
}
