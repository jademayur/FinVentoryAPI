using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.IncomingPaymentDTOs
{
    public class UpdateIncomingPaymentDto : IValidatableObject
    {

        [Required(ErrorMessage = "Payment date is required.")]
        public DateTime PaymentDate { get; set; }

        [Required(ErrorMessage = "Customer is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Customer ID must be a positive integer.")]
        public int BusinessPartnerId { get; set; }

        [Required(ErrorMessage = "Deposit account is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Deposit account ID must be a positive integer.")]
        public int DepositAccountId { get; set; }

        [Required(ErrorMessage = "Payment mode is required.")]
        [RegularExpression("^(Cash|Cheque|NEFT|RTGS|UPI|Other)$",
            ErrorMessage = "Payment mode must be one of: Cash, Cheque, NEFT, RTGS, UPI, Other.")]
        public string PaymentMode { get; set; } = "Cash";

        [StringLength(50, ErrorMessage = "Cheque number cannot exceed 50 characters.")]
        public string? ChequeNo { get; set; }

        public DateTime? ChequeDate { get; set; }

        [StringLength(100, ErrorMessage = "Bank name cannot exceed 100 characters.")]
        public string? BankName { get; set; }

        [StringLength(100, ErrorMessage = "Transaction reference cannot exceed 100 characters.")]
        public string? TransactionRef { get; set; }

        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters.")]
        public string? Remarks { get; set; }

        [Required(ErrorMessage = "Total amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total amount must be greater than zero.")]
        public decimal TotalAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "On-account amount cannot be negative.")]
        public decimal OnAccountAmount { get; set; }

        public List<IncomingPaymentAllocationDto> Allocations { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PaymentDate != default && PaymentDate.Date > DateTime.UtcNow.Date)
                yield return new ValidationResult(
                    "Payment date cannot be a future date.",
                    new[] { nameof(PaymentDate) });

            if (PaymentMode == "Cheque")
            {
                if (string.IsNullOrWhiteSpace(ChequeNo))
                    yield return new ValidationResult(
                        "Cheque number is required when payment mode is Cheque.",
                        new[] { nameof(ChequeNo) });

                if (ChequeDate == null)
                    yield return new ValidationResult(
                        "Cheque date is required when payment mode is Cheque.",
                        new[] { nameof(ChequeDate) });

                if (ChequeDate != null && ChequeDate.Value.Date > DateTime.UtcNow.Date.AddDays(90))
                    yield return new ValidationResult(
                        "Cheque date cannot be more than 90 days in the future.",
                        new[] { nameof(ChequeDate) });
            }

            var allocatedSum = Allocations.Sum(a => a.AmountApplied);
            var expected = allocatedSum + OnAccountAmount;
            if (Math.Abs(expected - TotalAmount) > 0.01m)
                yield return new ValidationResult(
                    $"Total amount ({TotalAmount:N2}) must equal " +
                    $"allocated ({allocatedSum:N2}) + on-account ({OnAccountAmount:N2}).",
                    new[] { nameof(TotalAmount) });

            if (Allocations.Any())
            {
                var dupes = Allocations
                    .GroupBy(a => a.InvoiceId)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (dupes.Any())
                    yield return new ValidationResult(
                        $"Duplicate invoice(s) in allocations: {string.Join(", ", dupes)}.",
                        new[] { nameof(Allocations) });
            }

            if (OnAccountAmount > TotalAmount)
                yield return new ValidationResult(
                    "On-account amount cannot exceed total amount received.",
                    new[] { nameof(OnAccountAmount) });
        }
    }
}
