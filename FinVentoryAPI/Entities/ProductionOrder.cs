using FinVentoryAPI.Enums;

namespace FinVentoryAPI.Entities
{
    public class ProductionOrder
    {
        public int ProductionOrderId { get; set; }
        public int CompanyId { get; set; }
        public int FinancialYearId { get; set; }

        public string? OrderNo { get; set; } = null!;        // PRD-2425-0001
        public DateOnly? OrderDate { get; set; }

        public int ItemId { get; set; }                     // finished good
        public Item FinishedGood { get; set; } = null!;

        public int? BomId { get; set; }                     // nullable — ad-hoc allowed
        public BillOfMaterial? Bom { get; set; }

        public decimal PlannedQuantity { get; set; }
        public decimal? ActualQuantity { get; set; }        // filled on Complete
        public int UnitId { get; set; }

        public ProductionOrderStatus Status { get; set; } = ProductionOrderStatus.Draft;

        public string? Notes { get; set; }
        public string? RefNo { get; set; }
        public DateOnly? RefDate { get; set; }

        public DateOnly? PlannedStartDate { get; set; }
        public DateOnly? PlannedEndDate { get; set; }
        public DateOnly? ActualCompletionDate { get; set; }

        // Audit
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; } = false;
        public bool IsActive { get; set; } = true;

        public ICollection<ProductionOrderLine> Lines { get; set; } = new List<ProductionOrderLine>();

    }
}
