using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.ProductionReceiptDTOs
{
    public class CreateProductionReceiptDetailDto
    {
        [Required]
        public int ItemId { get; set; }

        [Required]
        [Range(0.0001, double.MaxValue)]
        public decimal Qty { get; set; }

        public string? Remarks { get; set; }

        public List<CreateProductionReceiptDetailBatchDto>? Batches { get; set; }
        public List<CreateProductionReceiptDetailSerialDto>? Serials { get; set; }
    }

    public class CreateProductionReceiptDetailBatchDto
    {
        [Required]
        public int ItemBatchId { get; set; }

        [Required]
        [Range(0.0001, double.MaxValue)]
        public decimal Qty { get; set; }
    }

    public class CreateProductionReceiptDetailSerialDto
    {
        [Required]
        public int ItemSerialId { get; set; }
    }

    public class CreateProductionReceiptMainDto
    {
        [Required]
        public DateTime ReceiptDate { get; set; }

        public int? ProductionOrderId { get; set; }

        [Required]
        public int WarehouseId { get; set; }

        [Required]
        public int LocationId { get; set; }

        public string? Remarks { get; set; }

        [Required]
        public List<CreateProductionReceiptDetailDto> Details { get; set; } = new();
    }

    public class UpdateProductionReceiptMainDto : CreateProductionReceiptMainDto { }

    public class ProductionReceiptDetailResponseDto
    {
        public int ProductionReceiptDetailId { get; set; }
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public string? ItemCode { get; set; }
        public decimal Qty { get; set; }
        public string? Remarks { get; set; }
        public List<ProductionReceiptDetailBatchResponseDto> Batches { get; set; } = new();
        public List<ProductionReceiptDetailSerialResponseDto> Serials { get; set; } = new();
    }

    public class ProductionReceiptDetailBatchResponseDto
    {
        public int Id { get; set; }
        public int ItemBatchId { get; set; }
        public string? BatchNo { get; set; }
        public decimal Qty { get; set; }
    }

    public class ProductionReceiptDetailSerialResponseDto
    {
        public int Id { get; set; }
        public int ItemSerialId { get; set; }
        public string? SerialNo { get; set; }
    }

    public class ProductionReceiptResponseDto
    {
        public int ProductionReceiptId { get; set; }
        public int CompanyId { get; set; }
        public int FinancialYearId { get; set; }
        public string ReceiptNo { get; set; } = string.Empty;
        public DateTime ReceiptDate { get; set; }
        public int? ProductionOrderId { get; set; }
        public string? ProductionOrderNo { get; set; }
        public int WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public string Status { get; set; } = string.Empty;
        public int LocationId { get; set; }
        public string? LocationName { get; set; }
        public string? Remarks { get; set; }
        public List<ProductionReceiptDetailResponseDto> Details { get; set; } = new();
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    public class ProductionReceiptListDto
    {
        public int ProductionReceiptId { get; set; }
        public string ReceiptNo { get; set; } = string.Empty;
        public int FinancialYearId { get; set; }
        public DateTime ReceiptDate { get; set; }
        public string? WarehouseName { get; set; }
        public string? ProductionOrderNo { get; set; }
        public string Status { get; set; } = string.Empty;
        public int DetailCount { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
