
using AmmoEditor.Extensions;
using CombatExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AmmoEditor
{
    public class AmmoSetAE
    {
        public List<AmmoLinkAE> ammoLinks = new List<AmmoLinkAE>();
        public ModContentPack modContentPack;

        private AmmoSetDef ammoSetDef;

        public string Label
        {
            get
            {
                if(ammoSetDef != null)
                    return ammoSetDef.label;
                else if (ammoLinks.Any())
                    return ammoLinks[0].Label;
                else
                    return string.Empty;
            }
        }

        public string Name
        {
            get
            {
                if(ammoSetDef != null)
                {
                    return ammoSetDef.defName;
                }
                if (ammoLinks.Any())
                {
                    return ammoLinks[0].ammo?.defName ?? ammoLinks[0].projectile.defName;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public bool ContainModified
        {
            get
            {
                return ammoLinks.Any(x =>
                    x.projectile.Modified()
                );
            }   
        }
        public string Description
        {
            get
            {
                if (ammoLinks.Any())
                    return ammoLinks[0].Description;
                else
                    return string.Empty;
            }
        }
        
        public Texture2D Icon {
            get
            {
                if(ammoLinks.Any())
                    return ammoLinks[0].Icon;
                else
                    return null;
            }
        }
        
        public AmmoSetAE() { }

        public AmmoSetAE(AmmoSetDef ammoSetDef)
        {
            foreach(var ammoLink in ammoSetDef.ammoTypes)
            {
                ammoLinks.Add(new AmmoLinkAE(ammoLink));
            }
            this.modContentPack = ammoSetDef.modContentPack;
            this.ammoSetDef = ammoSetDef;
        }

        public AmmoSetAE(ThingDef projectileDef, ThingDef ammo = null)
        {
            ammoLinks.Add(new AmmoLinkAE(projectileDef, ammo));

            this.modContentPack = projectileDef.modContentPack;
        }



    }
}
