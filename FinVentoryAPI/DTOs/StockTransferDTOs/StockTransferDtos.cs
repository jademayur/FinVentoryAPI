using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.StockTransferDTOs
{
    public class CreateStockTransferDetailDto
    {
        [Required]
        public int ItemId { get; set; }

        [Required]
        [Range(0.0001, double.MaxValue)]
        public decimal Qty { get; set; }

        public string? Remarks { get; set; }

        public List<CreateStockTransferDetailBatchDto>? Batches { get; set; }
        public List<CreateStockTransferDetailSerialDto>? Serials { get; set; }
    }

    public class CreateStockTransferDetailBatchDto
    {
        [Required]
        public int ItemBatchId { get; set; }

        [Required]
        [Range(0.0001, double.MaxValue)]
        public decimal Qty { get; set; }
    }

    public class CreateStockTransferDetailSerialDto
    {
        [Required]
        public int ItemSerialId { get; set; }
    }

    public class CreateStockTransferMainDto
    {
        [Required]
        public DateTime TransferDate { get; set; }

        [Required]
        public int FromWarehouseId { get; set; }

        [Required]
        public int ToWarehouseId { get; set; }

        [Required]
        public int LocationId { get; set; }

        public string? Remarks { get; set; }

        [Required]
        public List<CreateStockTransferDetailDto> Details { get; set; } = new();
    }

    public class UpdateStockTransferMainDto : CreateStockTransferMainDto { }

    public class StockTransferDetailResponseDto
    {
        public int TransferDetailId { get; set; }
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public string? ItemCode { get; set; }
        public decimal Qty { get; set; }
        public string? Remarks { get; set; }
        public List<StockTransferDetailBatchResponseDto> Batches { get; set; } = new();
        public List<StockTransferDetailSerialResponseDto> Serials { get; set; } = new();
    }

    public class StockTransferDetailBatchResponseDto
    {
        public int Id { get; set; }
        public int ItemBatchId { get; set; }
        public string? BatchNo { get; set; }
        public decimal Qty { get; set; }
    }

    public class StockTransferDetailSerialResponseDto
    {
        public int Id { get; set; }
        public int ItemSerialId { get; set; }
        public string? SerialNo { get; set; }
    }

    public class StockTransferResponseDto
    {
        public int TransferId { get; set; }
        public int CompanyId { get; set; }
        public int FinancialYearId { get; set; }
        public string TransferNo { get; set; } = string.Empty;
        public DateTime TransferDate { get; set; }
        public int FromWarehouseId { get; set; }
        public string? FromWarehouseName { get; set; }
        public int ToWarehouseId { get; set; }
        public string? ToWarehouseName { get; set; }
        public string Status { get; set; } = string.Empty;
        public int LocationId { get; set; }
        public string? LocationName { get; set; }
        public string? Remarks { get; set; }
        public List<StockTransferDetailResponseDto> Details { get; set; } = new();
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    public class StockTransferListDto
    {
        public int TransferId { get; set; }
        public string TransferNo { get; set; } = string.Empty;
        public int FinancialYearId { get; set; }
        public DateTime TransferDate { get; set; }
        public string? FromWarehouseName { get; set; }
        public string? ToWarehouseName { get; set; }
        public string Status { get; set; } = string.Empty;
        public int DetailCount { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
