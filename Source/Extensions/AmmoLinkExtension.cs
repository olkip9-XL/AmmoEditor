using CombatExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmmoEditor
{
    public static class AmmoLinkExtension
    {
        public static bool Modified(this AmmoLink ammoLink)
        {
            AmmoModifier modifier = Mod_AmmoEditor.settings.ammoModifiers.Find(x=>
            x.defName == ammoLink.projectile.defName);

            return modifier != null && !modifier.CanDelete();
        }

        public static bool IsExplosive(this AmmoLink ammoLink)
        {
            AmmoModifier modifier = Mod_AmmoEditor.settings.ammoModifiers.Find(x =>
            x.defName == ammoLink.projectile.defName);

            if (modifier != null)
            {
                return modifier.IsExplosive;
            }
            else
            {
                return ammoLink.projectile.projectile.explosionRadius > 0;
            }
        }
    }
}
