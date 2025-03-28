using CombatExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AmmoEditor.Extensions
{
    public static class ThingDefExtension
    {
        public static bool Modified(this ThingDef thingDef)
        {
            AmmoModifier modifier = Mod_AmmoEditor.settings.ammoModifiers.Find(x =>
                x.defName == thingDef.defName);

            return modifier != null && !modifier.CanDelete();
        }

        public static bool IsExplosive(this ThingDef thingDef)
        {
            if(thingDef.projectile == null) return false;

            AmmoModifier modifier = Mod_AmmoEditor.settings.ammoModifiers.Find(x =>
            x.defName == thingDef.defName);

            if (modifier != null)
            {
                return modifier.IsExplosive;
            }
            else
            {
                return thingDef.projectile.explosionRadius > 0;
            }
        }
    }
}
