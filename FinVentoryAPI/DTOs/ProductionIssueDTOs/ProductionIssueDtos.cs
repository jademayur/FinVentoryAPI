using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.ProductionIssueDTOs
{
    public class CreateProductionIssueDetailDto
    {
        [Required]
        public int ItemId { get; set; }

        [Required]
        [Range(0.0001, double.MaxValue)]
        public decimal Qty { get; set; }

        public string? Remarks { get; set; }

        public List<CreateProductionIssueDetailBatchDto>? Batches { get; set; }
        public List<CreateProductionIssueDetailSerialDto>? Serials { get; set; }
    }

    public class CreateProductionIssueDetailBatchDto
    {
        [Required]
        public int ItemBatchId { get; set; }

        [Required]
        [Range(0.0001, double.MaxValue)]
        public decimal Qty { get; set; }
    }

    public class CreateProductionIssueDetailSerialDto
    {
        [Required]
        public int ItemSerialId { get; set; }
    }

    public class CreateProductionIssueMainDto
    {
        [Required]
        public DateTime IssueDate { get; set; }

        public int? ProductionOrderId { get; set; }

        [Required]
        public int WarehouseId { get; set; }

        [Required]
        public int LocationId { get; set; }

        public string? Remarks { get; set; }

        [Required]
        public List<CreateProductionIssueDetailDto> Details { get; set; } = new();
    }

    public class UpdateProductionIssueMainDto : CreateProductionIssueMainDto { }

    public class ProductionIssueDetailResponseDto
    {
        public int ProductionIssueDetailId { get; set; }
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public string? ItemCode { get; set; }
        public decimal Qty { get; set; }
        public string? Remarks { get; set; }
        public List<ProductionIssueDetailBatchResponseDto> Batches { get; set; } = new();
        public List<ProductionIssueDetailSerialResponseDto> Serials { get; set; } = new();
    }

    public class ProductionIssueDetailBatchResponseDto
    {
        public int Id { get; set; }
        public int ItemBatchId { get; set; }
        public string? BatchNo { get; set; }
        public decimal Qty { get; set; }
    }

    public class ProductionIssueDetailSerialResponseDto
    {
        public int Id { get; set; }
        public int ItemSerialId { get; set; }
        public string? SerialNo { get; set; }
    }

    public class ProductionIssueResponseDto
    {
        public int ProductionIssueId { get; set; }
        public int CompanyId { get; set; }
        public int FinancialYearId { get; set; }
        public string IssueNo { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public int? ProductionOrderId { get; set; }
        public string? ProductionOrderNo { get; set; }
        public int WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public string Status { get; set; } = string.Empty;
        public int LocationId { get; set; }
        public string? LocationName { get; set; }
        public string? Remarks { get; set; }
        public List<ProductionIssueDetailResponseDto> Details { get; set; } = new();
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    public class ProductionIssueListDto
    {
        public int ProductionIssueId { get; set; }
        public string IssueNo { get; set; } = string.Empty;
        public int FinancialYearId { get; set; }
        public DateTime IssueDate { get; set; }
        public string? WarehouseName { get; set; }
        public string? ProductionOrderNo { get; set; }
        public string Status { get; set; } = string.Empty;
        public int DetailCount { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
