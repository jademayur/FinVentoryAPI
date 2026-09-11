using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class JobWorkReceiptMain : BaseEntity
    {
        [Key]
        public int JobWorkReceiptId { get; set; }
        public int CompanyId { get; set; }
        public int FinancialYearId { get; set; }

        [MaxLength(30)]
        public string ReceiptNo { get; set; } = string.Empty;

        public DateTime ReceiptDate { get; set; }
        public int? BusinessPartnerId { get; set; }
        public int WarehouseId { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Draft";

        public int LocationId { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }

        [ForeignKey(nameof(WarehouseId))]
        public Warehouse? Warehouse { get; set; }

        [ForeignKey(nameof(LocationId))]
        public Location? Location { get; set; }

        [ForeignKey(nameof(BusinessPartnerId))]
        public BusinessPartner? BusinessPartner { get; set; }

        public ICollection<JobWorkReceiptDetail> Details { get; set; } = new List<JobWorkReceiptDetail>();
    }
}
