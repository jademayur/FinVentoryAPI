using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class StockTransferMain : BaseEntity
    {
        [Key]
        public int TransferId { get; set; }
        public int CompanyId { get; set; }
        public int FinancialYearId { get; set; }

        [MaxLength(30)]
        public string TransferNo { get; set; } = string.Empty;

        public DateTime TransferDate { get; set; }

        public int FromWarehouseId { get; set; }
        public int ToWarehouseId { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Draft";

        public int LocationId { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }

        [ForeignKey(nameof(FromWarehouseId))]
        public Warehouse? FromWarehouse { get; set; }

        [ForeignKey(nameof(ToWarehouseId))]
        public Warehouse? ToWarehouse { get; set; }

        [ForeignKey(nameof(LocationId))]
        public Location? Location { get; set; }

        public ICollection<StockTransferDetail> Details { get; set; } = new List<StockTransferDetail>();
    }
}
