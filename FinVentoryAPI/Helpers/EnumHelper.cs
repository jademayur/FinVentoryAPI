using FinVentoryAPI.Enums;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FinVentoryAPI.Helpers
{
    public static class EnumHelper
    {
        public static List<object> GetEnumList<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T))
                .Cast<T>()
                .Select(e => new
                {
                    id = Convert.ToInt32(e),
                    name = e.ToString()
                })
                .Cast<object>()
                .ToList();
        }

        public static string GetDisplayName(Enum enumValue)
        {
            return enumValue.GetType()
                .GetMember(enumValue.ToString())
                .First()
                .GetCustomAttribute<DisplayAttribute>()?
                .Name ?? enumValue.ToString();
        }

        public static readonly Dictionary<(BaseUnit, AlternateUnit), decimal> Factors = new()
        {
             // Weight
            { (BaseUnit.KG,  AlternateUnit.GM),   1000m       },
            { (BaseUnit.KG,  AlternateUnit.MG),   1000000m    },
            { (BaseUnit.KG,  AlternateUnit.LB),   2.20462m    },
            { (BaseUnit.KG,  AlternateUnit.OZ),   35.274m     },
            { (BaseUnit.TON, AlternateUnit.QNT),  10m         },
            
            // Volume
            { (BaseUnit.LTR, AlternateUnit.ML),   1000m       },
            { (BaseUnit.LTR, AlternateUnit.GAL),  0.264172m   },
            { (BaseUnit.LTR, AlternateUnit.PT),   2.11338m    },
            { (BaseUnit.LTR, AlternateUnit.QT),   1.05669m    },
            { (BaseUnit.LTR, AlternateUnit.CUP),  4.22675m    },
            { (BaseUnit.LTR, AlternateUnit.FLOZ), 33.814m     },
            
            // Length
            { (BaseUnit.MTR, AlternateUnit.CM),   100m        },
            { (BaseUnit.MTR, AlternateUnit.MM),   1000m       },
            { (BaseUnit.MTR, AlternateUnit.INCH), 39.3701m    },
            { (BaseUnit.MTR, AlternateUnit.FT),   3.28084m    },
            { (BaseUnit.MTR, AlternateUnit.YD),   1.09361m    },
            { (BaseUnit.MTR, AlternateUnit.KM),   0.001m      },
            { (BaseUnit.MTR, AlternateUnit.MI),   0.000621371m},
            
            // Quantity
            { (BaseUnit.PCS,  AlternateUnit.DOZ),  0.0833m  },
            { (BaseUnit.DOZ,  AlternateUnit.PCS),  12m      },
            { (BaseUnit.BOX,  AlternateUnit.PCS),  12m      },
            { (BaseUnit.CTN,  AlternateUnit.PCS),  24m      },
            { (BaseUnit.PACK, AlternateUnit.PCS),  10m      },  // ✅ PACK is in BaseUnit
            { (BaseUnit.BDL,  AlternateUnit.PCS),  10m      },  // ✅ BDL is in BaseUnit
        };

        public static decimal? GetFactor(BaseUnit baseUnit, AlternateUnit altUnit)
        {
            return Factors.TryGetValue((baseUnit, altUnit), out var factor) ? factor : null;
        }

        public static string GetStateName(int code) => code switch
        {
            1 => "Jammu & Kashmir",
            2 => "Himachal Pradesh",
            3 => "Punjab",
            4 => "Chandigarh",
            5 => "Uttarakhand",
            6 => "Haryana",
            7 => "Delhi",
            8 => "Rajasthan",
            9 => "Uttar Pradesh",
            10 => "Bihar",
            11 => "Sikkim",
            12 => "Arunachal Pradesh",
            13 => "Nagaland",
            14 => "Manipur",
            15 => "Mizoram",
            16 => "Tripura",
            17 => "Meghalaya",
            18 => "Assam",
            19 => "West Bengal",
            20 => "Jharkhand",
            21 => "Odisha",
            22 => "Chhattisgarh",
            23 => "Madhya Pradesh",
            24 => "Gujarat",
            26 => "Dadra & Nagar Haveli and Daman & Diu",
            27 => "Maharashtra",
            29 => "Karnataka",
            30 => "Goa",
            31 => "Lakshadweep",
            32 => "Kerala",
            33 => "Tamil Nadu",
            34 => "Puducherry",
            35 => "Andaman & Nicobar Islands",
            36 => "Telangana",
            37 => "Andhra Pradesh",
            38 => "Ladakh",
            97 => "Other Territory",
            99 => "Other Countries",
            _ => "Unknown"
        };

        public static List<object> GetAll() =>
            Enum.GetValues<GstState>()
                .Select(s => (object)new
                {
                    stateId = (int)s,
                    stateName = GetStateName((int)s),
                    stateCode = ((int)s).ToString("D2") // ✅ e.g. "24" for Gujarat
                })
                .ToList();
    }
}
