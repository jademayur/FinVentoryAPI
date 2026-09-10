using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.JobWorkReceiptDTOs
{
    public class CreateJobWorkReceiptDetailDto
    {
        [Required] public int ItemId { get; set; }
        [Required] [Range(0.0001, double.MaxValue)] public decimal Qty { get; set; }
        public string? Remarks { get; set; }
        public List<CreateJobWorkReceiptDetailBatchDto>? Batches { get; set; }
        public List<CreateJobWorkReceiptDetailSerialDto>? Serials { get; set; }
    }

    public class CreateJobWorkReceiptDetailBatchDto
    {
        [Required] public int ItemBatchId { get; set; }
        [Required] [Range(0.0001, double.MaxValue)] public decimal Qty { get; set; }
    }

    public class CreateJobWorkReceiptDetailSerialDto
    {
        [Required] public int ItemSerialId { get; set; }
    }

    public class CreateJobWorkReceiptMainDto
    {
        [Required] public DateTime ReceiptDate { get; set; }
        public int? BusinessPartnerId { get; set; }
        [Required] public int WarehouseId { get; set; }
        [Required] public int LocationId { get; set; }
        public string? Remarks { get; set; }
        [Required] public List<CreateJobWorkReceiptDetailDto> Details { get; set; } = new();
    }

    public class UpdateJobWorkReceiptMainDto : CreateJobWorkReceiptMainDto { }

    public class JobWorkReceiptDetailResponseDto
    {
        public int JobWorkReceiptDetailId { get; set; }
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public string? ItemCode { get; set; }
        public decimal Qty { get; set; }
        public string? Remarks { get; set; }
        public List<JobWorkReceiptDetailBatchResponseDto> Batches { get; set; } = new();
        public List<JobWorkReceiptDetailSerialResponseDto> Serials { get; set; } = new();
    }

    public class JobWorkReceiptDetailBatchResponseDto { public int Id { get; set; } public int ItemBatchId { get; set; } public string? BatchNo { get; set; } public decimal Qty { get; set; } }
    public class JobWorkReceiptDetailSerialResponseDto { public int Id { get; set; } public int ItemSerialId { get; set; } public string? SerialNo { get; set; } }

    public class JobWorkReceiptResponseDto
    {
        public int JobWorkReceiptId { get; set; }
        public int CompanyId { get; set; }
        public int FinancialYearId { get; set; }
        public string ReceiptNo { get; set; } = string.Empty;
        public DateTime ReceiptDate { get; set; }
        public int? BusinessPartnerId { get; set; }
        public string? BusinessPartnerName { get; set; }
        public int WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public string Status { get; set; } = string.Empty;
        public int LocationId { get; set; }
        public string? LocationName { get; set; }
        public string? Remarks { get; set; }
        public List<JobWorkReceiptDetailResponseDto> Details { get; set; } = new();
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    public class JobWorkReceiptListDto
    {
        public int JobWorkReceiptId { get; set; }
        public string ReceiptNo { get; set; } = string.Empty;
        public int FinancialYearId { get; set; }
        public DateTime ReceiptDate { get; set; }
        public string? BusinessPartnerName { get; set; }
        public string? WarehouseName { get; set; }
        public string Status { get; set; } = string.Empty;
        public int DetailCount { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
