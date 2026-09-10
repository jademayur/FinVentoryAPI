using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class JobWorkIssueMain : BaseEntity
    {
        [Key]
        public int JobWorkIssueId { get; set; }
        public int CompanyId { get; set; }
        public int FinancialYearId { get; set; }

        [MaxLength(30)]
        public string IssueNo { get; set; } = string.Empty;

        public DateTime IssueDate { get; set; }
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

        public ICollection<JobWorkIssueDetail> Details { get; set; } = new List<JobWorkIssueDetail>();
    }
}
