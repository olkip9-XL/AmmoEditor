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
    public class AmmoLinkAE
    {
        public ThingDef projectile;
        public ThingDef ammo;

        public string Label
        {
            get
            {
                if (this.ammo != null)
                    return this.ammo.label;
                else if (this.projectile != null)
                    return this.projectile.label;
                else
                    return string.Empty;
            }
        }
        public string Description
        {
            get
            {
                if (ammo != null)
                {
                    return ammo.description;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        private Texture2D textureInt;
        public Texture2D Icon
        {
            get
            {
                if (this.textureInt == null)
                {
                    if (ammo != null)
                    {
                        textureInt = ammo.graphic.MatSingle.mainTexture as Texture2D;
                    }
                    else if (projectile != null)
                    {
                        textureInt = projectile.graphic.MatSingle.mainTexture as Texture2D;
                    }
                    else
                        textureInt = Texture2D.whiteTexture;
                }
                return textureInt;
            }
        }
        public AmmoLinkAE() { }

        public AmmoLinkAE(AmmoLink ammoLinkCE)
        {
            this.projectile = ammoLinkCE.projectile;
            this.ammo = ammoLinkCE.ammo;
        }

        public AmmoLinkAE(ThingDef projectile, ThingDef ammo)
        {
            this.projectile = projectile;
            this.ammo = ammo;
        }
    }
}
