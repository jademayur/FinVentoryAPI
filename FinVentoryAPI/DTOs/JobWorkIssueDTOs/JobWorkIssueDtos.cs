using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.JobWorkIssueDTOs
{
    public class CreateJobWorkIssueDetailDto
    {
        [Required] public int ItemId { get; set; }
        [Required] [Range(0.0001, double.MaxValue)] public decimal Qty { get; set; }
        public string? Remarks { get; set; }
        public List<CreateJobWorkIssueDetailBatchDto>? Batches { get; set; }
        public List<CreateJobWorkIssueDetailSerialDto>? Serials { get; set; }
    }

    public class CreateJobWorkIssueDetailBatchDto
    {
        [Required] public int ItemBatchId { get; set; }
        [Required] [Range(0.0001, double.MaxValue)] public decimal Qty { get; set; }
    }

    public class CreateJobWorkIssueDetailSerialDto
    {
        [Required] public int ItemSerialId { get; set; }
    }

    public class CreateJobWorkIssueMainDto
    {
        [Required] public DateTime IssueDate { get; set; }
        public int? BusinessPartnerId { get; set; }
        [Required] public int WarehouseId { get; set; }
        [Required] public int LocationId { get; set; }
        public string? Remarks { get; set; }
        [Required] public List<CreateJobWorkIssueDetailDto> Details { get; set; } = new();
    }

    public class UpdateJobWorkIssueMainDto : CreateJobWorkIssueMainDto { }

    public class JobWorkIssueDetailResponseDto
    {
        public int JobWorkIssueDetailId { get; set; }
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public string? ItemCode { get; set; }
        public decimal Qty { get; set; }
        public string? Remarks { get; set; }
        public List<JobWorkIssueDetailBatchResponseDto> Batches { get; set; } = new();
        public List<JobWorkIssueDetailSerialResponseDto> Serials { get; set; } = new();
    }

    public class JobWorkIssueDetailBatchResponseDto { public int Id { get; set; } public int ItemBatchId { get; set; } public string? BatchNo { get; set; } public decimal Qty { get; set; } }
    public class JobWorkIssueDetailSerialResponseDto { public int Id { get; set; } public int ItemSerialId { get; set; } public string? SerialNo { get; set; } }

    public class JobWorkIssueResponseDto
    {
        public int JobWorkIssueId { get; set; }
        public int CompanyId { get; set; }
        public int FinancialYearId { get; set; }
        public string IssueNo { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public int? BusinessPartnerId { get; set; }
        public string? BusinessPartnerName { get; set; }
        public int WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public string Status { get; set; } = string.Empty;
        public int LocationId { get; set; }
        public string? LocationName { get; set; }
        public string? Remarks { get; set; }
        public List<JobWorkIssueDetailResponseDto> Details { get; set; } = new();
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    public class JobWorkIssueListDto
    {
        public int JobWorkIssueId { get; set; }
        public string IssueNo { get; set; } = string.Empty;
        public int FinancialYearId { get; set; }
        public DateTime IssueDate { get; set; }
        public string? BusinessPartnerName { get; set; }
        public string? WarehouseName { get; set; }
        public string Status { get; set; } = string.Empty;
        public int DetailCount { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
