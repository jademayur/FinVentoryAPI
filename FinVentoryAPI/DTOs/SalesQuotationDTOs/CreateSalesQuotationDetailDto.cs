using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.SalesQuotationDTOs
{
    public class CreateSalesQuotationDetailDto
    {
        [Required(ErrorMessage = "Item is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Item.")]
        public int ItemId { get; set; }

        public string? PriceType { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        public decimal Qty { get; set; }

        [Required(ErrorMessage = "Rate is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Rate must be 0 or greater.")]
        public decimal Rate { get; set; }

        [Range(0, 100, ErrorMessage = "Discount Rate must be between 0 and 100.")]
        public decimal DiscountRate { get; set; } = 0;

        [Range(0, 100, ErrorMessage = "Additional Discount Rate must be between 0 and 100.")]
        public decimal AddisDiscountRate { get; set; } = 0;

        public bool IsTaxIncluded { get; set; } = false;
    }
}
