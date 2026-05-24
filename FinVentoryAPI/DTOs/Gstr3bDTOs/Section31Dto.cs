namespace FinVentoryAPI.DTOs.Gstr3bDTOs
{
    public class Section31Dto
    {
        /// <summary>(a) Outward taxable supplies (other than zero rated, nil rated and exempted)</summary>
        public SupplyRowDto TaxableSupplies { get; set; } = new();

        /// <summary>(b) Outward taxable supplies (zero rated)</summary>
        public SupplyRowDto ZeroRatedSupplies { get; set; } = new();

        /// <summary>(c) Other outward supplies (nil rated, exempted)</summary>
        public SupplyRowDto NilExemptSupplies { get; set; } = new();

        /// <summary>(d) Inward supplies (liable to reverse charge)</summary>
        public SupplyRowDto ReverseCharge { get; set; } = new();

        /// <summary>(e) Non-GST outward supplies</summary>
        public SupplyRowDto NonGstSupplies { get; set; } = new();
    }
}
