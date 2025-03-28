
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AmmoEditor
{
    public static class GasTypeExtensions
    {
        public static string GetLabel(this GasType? gasType)
        {
            switch (gasType)
            {
                case GasType.BlindSmoke:
                    return "BlindSmoke".Translate();
                case GasType.ToxGas:
                    return "ToxGas".Translate();
                case GasType.RotStink:
                    return "RotStink".Translate();
                case GasType.DeadlifeDust:
                    return "DeadlifeDust".Translate();
                case null:
                    return "None".Translate();
                default:
                    Log.ErrorOnce("Trying to get unknown gas type label.", 172091);
                    return string.Empty;
            }
        }
    }
}
