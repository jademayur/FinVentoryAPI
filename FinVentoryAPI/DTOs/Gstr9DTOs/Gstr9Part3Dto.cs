namespace FinVentoryAPI.DTOs.Gstr9DTOs
{
    public class Gstr9Part3Dto
    {
        /// <summary>6B — Import of goods</summary>
        public ITCAmountDto ImportOfGoods { get; set; } = new();

        /// <summary>6C — Import of services</summary>
        public ITCAmountDto ImportOfServices { get; set; } = new();

        /// <summary>6D — Inward supplies on reverse charge (registered)</summary>
        public ITCAmountDto ReverseChargeReg { get; set; } = new();

        /// <summary>6E — All other ITC availed (domestic purchases)</summary>
        public ITCAmountDto OtherITC { get; set; } = new();

        /// <summary>6H — ITC reversed (Rule 42 & 43)</summary>
        public ITCAmountDto ITCReversed { get; set; } = new();

        /// <summary>6J — Net ITC available = (B+C+D+E) – (H)</summary>
        public ITCAmountDto NetITCAvailable { get; set; } = new();

        /// <summary>6K — ITC reclaimed (previously reversed)</summary>
        public ITCAmountDto ITCReclaimed { get; set; } = new();
    }
}
