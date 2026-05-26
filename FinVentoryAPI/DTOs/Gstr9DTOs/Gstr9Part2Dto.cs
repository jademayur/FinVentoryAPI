namespace FinVentoryAPI.DTOs.Gstr9DTOs
{
    public class Gstr9Part2Dto
    {
        // Table 4 — Outward supplies
        /// <summary>4A — Supplies made to registered persons (B2B)</summary>
        public SupplyAmountDto B2BSupplies { get; set; } = new();

        /// <summary>4B — Supplies made to unregistered persons (B2C)</summary>
        public SupplyAmountDto B2CSupplies { get; set; } = new();

        /// <summary>4C — Zero rated supplies (exports)</summary>
        public SupplyAmountDto ZeroRated { get; set; } = new();

        /// <summary>4D — Deemed exports</summary>
        public SupplyAmountDto DeemedExports { get; set; } = new();

        /// <summary>4E — Exempt supplies</summary>
        public decimal ExemptSupplies { get; set; }

        /// <summary>4F — Nil rated supplies</summary>
        public decimal NilRatedSupplies { get; set; }

        /// <summary>4G — Non-GST supplies</summary>
        public decimal NonGstSupplies { get; set; }

        // Table 5 — Inward supplies (purchase side)
        /// <summary>5A — Total inward supplies</summary>
        public SupplyAmountDto InwardSupplies { get; set; } = new();

        /// <summary>5B — Inward supplies attracting reverse charge (from reg. supplier)</summary>
        public SupplyAmountDto ReverseChargeReg { get; set; } = new();

        /// <summary>5C — Inward supplies attracting reverse charge (from unreg. supplier)</summary>
        public SupplyAmountDto ReverseChargeUnreg { get; set; } = new();
    }
}
