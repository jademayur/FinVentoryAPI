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
    }
}
