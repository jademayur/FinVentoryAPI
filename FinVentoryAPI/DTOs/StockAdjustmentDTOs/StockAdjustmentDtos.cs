using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.StockAdjustmentDTOs
{
    public class CreateStockAdjustmentDetailDto
    {
        [Required]
        public int ItemId { get; set; }

        [Required]
        [Range(0.0001, double.MaxValue)]
        public decimal Qty { get; set; }

        public string? Remarks { get; set; }

        public List<CreateStockAdjustmentDetailBatchDto>? Batches { get; set; }
        public List<CreateStockAdjustmentDetailSerialDto>? Serials { get; set; }
    }

    public class CreateStockAdjustmentDetailBatchDto
    {
        [Required]
        public int ItemBatchId { get; set; }

        [Required]
        [Range(0.0001, double.MaxValue)]
        public decimal Qty { get; set; }
    }

    public class CreateStockAdjustmentDetailSerialDto
    {
        [Required]
        public int ItemSerialId { get; set; }
    }

    public class CreateStockAdjustmentMainDto
    {
        [Required]
        public DateTime AdjustmentDate { get; set; }

        [Required]
        public int WarehouseId { get; set; }

        [Required]
        public string AdjustmentType { get; set; } = string.Empty; // Increase / Decrease

        [Required]
        public string Reason { get; set; } = string.Empty;

        [Required]
        public int LocationId { get; set; }

        public string? Remarks { get; set; }

        [Required]
        public List<CreateStockAdjustmentDetailDto> Details { get; set; } = new();
    }

    public class UpdateStockAdjustmentMainDto : CreateStockAdjustmentMainDto { }

    public class StockAdjustmentDetailResponseDto
    {
        public int AdjustmentDetailId { get; set; }
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public string? ItemCode { get; set; }
        public decimal Qty { get; set; }
        public string? Remarks { get; set; }
        public List<StockAdjustmentDetailBatchResponseDto> Batches { get; set; } = new();
        public List<StockAdjustmentDetailSerialResponseDto> Serials { get; set; } = new();
    }

    public class StockAdjustmentDetailBatchResponseDto
    {
        public int Id { get; set; }
        public int ItemBatchId { get; set; }
        public string? BatchNo { get; set; }
        public decimal Qty { get; set; }
    }

    public class StockAdjustmentDetailSerialResponseDto
    {
        public int Id { get; set; }
        public int ItemSerialId { get; set; }
        public string? SerialNo { get; set; }
    }

    public class StockAdjustmentResponseDto
    {
        public int AdjustmentId { get; set; }
        public int CompanyId { get; set; }
        public int FinancialYearId { get; set; }
        public string AdjustmentNo { get; set; } = string.Empty;
        public DateTime AdjustmentDate { get; set; }
        public int WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public string AdjustmentType { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int LocationId { get; set; }
        public string? LocationName { get; set; }
        public string? Remarks { get; set; }
        public List<StockAdjustmentDetailResponseDto> Details { get; set; } = new();
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    public class StockAdjustmentListDto
    {
        public int AdjustmentId { get; set; }
        public string AdjustmentNo { get; set; } = string.Empty;
        public int FinancialYearId { get; set; }
        public DateTime AdjustmentDate { get; set; }
        public string? WarehouseName { get; set; }
        public string AdjustmentType { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int DetailCount { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
