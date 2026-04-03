using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.Enums
{
    public enum GroupType
    {
        Asset = 1,
        Liability = 2,
        Income = 3,
        Expense = 4

    }

    public enum BalanceTo
    {
        Trading = 1,
        [Display(Name = "Profit And Loss")]
        ProfitAndLoss = 2,
        [Display(Name = "Balance Sheet")]
        BalanceSheet = 3
    }

    public enum BookType
    {
        [Display(Name = "NO BOOK")]
        NONE = 0,
        [Display(Name = "CASH BOOK")]
        CASH = 1,
        [Display(Name = "BANK BOOK")]
        BANK = 2,
        [Display(Name = "SALES BOOK")]
        SALE = 3,
        [Display(Name = "PURCHASE BOOK")]
        PRCH = 4,
        [Display(Name = "J.V BOOK")]
        JV = 5
    }

    public enum AccountType
    {
        General = 1,
        Head = 2
    }

    public enum BookSubType
    {
        None = 0,
        Goods = 1,
        Services = 2
    }

    public enum ItemType
    {
        Goods = 1,
        Service = 2
    }

    public enum ItemCategory
    {
        RawMaterial = 1,
        SemiFinished = 2,
        FinishedGoods = 3,
        Trading = 4
    }

    public enum ItemManageBy
    {
        Regular = 1,
        Batch = 2,
        Serial = 3
    }

    public enum CostingMethod
    {
        MovingAverage = 1,
        FIFO = 2,
        Standard = 3
    }

    public enum BaseUnit
    {
        // Weight
        [Display(Name = "Kilogram")] KG = 1,
        [Display(Name = "Gram")] GM = 2,
        [Display(Name = "Ton")] TON = 3,
        [Display(Name = "Pound")] LB = 4,
        [Display(Name = "Ounce")] OZ = 5,

        // Volume
        [Display(Name = "Liter")] LTR = 6,
        [Display(Name = "Milliliter")] ML = 7,
        [Display(Name = "Gallon")] GAL = 8,
        [Display(Name = "Fluid Ounce")] FLOZ = 9,

        // Length
        [Display(Name = "Meter")] MTR = 10,
        [Display(Name = "Centimeter")] CM = 11,
        [Display(Name = "Millimeter")] MM = 12,
        [Display(Name = "Inch")] INCH = 13,
        [Display(Name = "Foot")] FT = 14,
        [Display(Name = "Yard")] YD = 15,

        // Quantity
        [Display(Name = "Piece")] PCS = 16,
        [Display(Name = "Dozen")] DOZ = 17,
        [Display(Name = "Box")] BOX = 18,
        [Display(Name = "Pack")] PACK = 19,
        [Display(Name = "Carton")] CTN = 20,
        [Display(Name = "Bag")] BAG = 21,
        [Display(Name = "Roll")] ROLL = 22,
        [Display(Name = "Set")] SET = 23,
        [Display(Name = "Pair")] PAIR = 24,
        [Display(Name = "Bundle")] BDL = 25,
    }

    public enum AlternateUnit
    {
        // Weight
        [Display(Name = "Gram")] GM = 1,
        [Display(Name = "Milligram")] MG = 2,
        [Display(Name = "Ton")] TON = 3,
        [Display(Name = "Pound")] LB = 4,
        [Display(Name = "Ounce")] OZ = 5,
        [Display(Name = "Quintal")] QNT = 6,

        // Volume
        [Display(Name = "Milliliter")] ML = 7,
        [Display(Name = "Gallon")] GAL = 8,
        [Display(Name = "Fluid Ounce")] FLOZ = 9,
        [Display(Name = "Cup")] CUP = 10,
        [Display(Name = "Pint")] PT = 11,
        [Display(Name = "Quart")] QT = 12,

        // Length
        [Display(Name = "Centimeter")] CM = 13,
        [Display(Name = "Millimeter")] MM = 14,
        [Display(Name = "Inch")] INCH = 15,
        [Display(Name = "Foot")] FT = 16,
        [Display(Name = "Yard")] YD = 17,
        [Display(Name = "Kilometer")] KM = 18,
        [Display(Name = "Mile")] MI = 19,

        // Quantity
        [Display(Name = "Piece")] PCS = 20,
        [Display(Name = "Dozen")] DOZ = 21,
        [Display(Name = "Box")] BOX = 22,
        [Display(Name = "Pack")] PACK = 23,
        [Display(Name = "Carton")] CTN = 24,
        [Display(Name = "Bag")] BAG = 25,
        [Display(Name = "Roll")] ROLL = 26,
        [Display(Name = "Set")] SET = 27,
        [Display(Name = "Pair")] PAIR = 28,
        [Display(Name = "Bundle")] BDL = 29,
        [Display(Name = "Gross")] GRS = 30,  // 144 pcs
        [Display(Name = "Ream")] REAM = 31,  // 500 sheets


    }

    public enum BusinessPartnerType
    {
        Customer = 1,
        Supplier = 2,
        Both = 3,
        Agent = 4,
    }
    public enum AddressType
    {
        Billing = 1,
        Shipping = 2,
        Common = 3
    }
    public enum GSTType
    {
        Regular = 1,
        NoRegister = 2,
        Composite = 3
    }

    public enum BalanceType
    {
        Dr = 1,
        Cr = 2,
    }


    public enum GstState
    {
       
        JammuAndKashmir = 1,      
        HimachalPradesh = 2,
        Punjab = 3,
        Chandigarh = 4,
        Uttarakhand = 5,
        Haryana = 6,
        Delhi = 7,
        Rajasthan = 8,
        UttarPradesh = 9,
        Bihar = 10,
        Sikkim = 11,
        ArunachalPradesh = 12,
        Nagaland = 13,
        Manipur = 14,
        Mizoram = 15,
        Tripura = 16,
        Meghalaya = 17,
        Assam = 18,
        WestBengal = 19,
        Jharkhand = 20,
        Odisha = 21,
        Chhattisgarh = 22,
        MadhyaPradesh = 23,
        Gujarat = 24,
        DadraAndNagarHaveliAndDamanAndDiu = 26,
        Maharashtra = 27,
        Karnataka = 29,
        Goa = 30,
        Lakshadweep = 31,
        Kerala = 32,
        TamilNadu = 33,
        Puducherry = 34,
        AndamanAndNicobarIslands = 35,
        Telangana = 36,
        AndhraPradesh = 37,
        Ladakh = 38,
        OtherTerritory = 97,       
        OtherCountries = 99
    }


}
