namespace FinVentoryAPI.DTOs.Gstr3bDTOs
{
    public class Section4Dto
    {
        /// <summary>(A) ITC available</summary>
        public ITCRowDto ImportOfGoods { get; set; } = new();
        public ITCRowDto ImportOfServices { get; set; } = new();
        public ITCRowDto InwardReverseCharge { get; set; } = new();
        public ITCRowDto InwardITCOthers { get; set; } = new();   // (A)(5) All other ITC

        /// <summary>(B) ITC reversed</summary>
        public ITCRowDto Rule42And43 { get; set; } = new();
        public ITCRowDto OtherReversal { get; set; } = new();

        /// <summary>Net ITC available  = (A) – (B)</summary>
        public ITCRowDto NetITCAvailable { get; set; } = new();
    }
}
