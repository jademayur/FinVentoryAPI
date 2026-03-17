using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.WarehouseDTOs
{
    public class CreateWarehouseDto
    {

        [Required(ErrorMessage = "Warehouse name is required.")]
        public string WarehouseName { get; set; } = string.Empty;
        public string? WarehouseCode { get; set; }
        public int? ParentWarehouseId { get; set; }
        public string? Address { get;set; }
        public string? City { get; set; }
        public string? ContactPerson { get; set; }
        public string? MobileNo { get; set; }

    }
}
