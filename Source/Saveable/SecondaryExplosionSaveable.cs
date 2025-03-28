using CombatExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;

namespace AmmoEditor
{
    public class SecondaryExplosionSaveable : AmmoEdiotr_Saveable
    {
        public DamageDef DamageDef
        {
            get
            {
                return DefDatabase<DamageDef>.GetNamed(damageDefString, false);
            }
            set
            {
                this.damageDefString = value.defName;
            }
        }
        private string damageDefString;

        public float explosionRadius;

        public float damageAmountBase;

        public GasType? postExplosionGasType;

        public SecondaryExplosionSaveable() { }

        public SecondaryExplosionSaveable(XmlNode rootNode)
        {
            foreach (XmlNode childNode in rootNode.ChildNodes)
            {
                switch (childNode.Name)
                {
                    case "damageDefString":
                        damageDefString = childNode.InnerText;
                        break;
                    case "explosionRadius":
                        explosionRadius = float.Parse(childNode.InnerText);
                        break;
                    case "damageAmountBase":
                        damageAmountBase = float.Parse(childNode.InnerText);
                        break;
                    case "postExplosionGasType":
                        if (childNode.InnerText.Count() > 0)
                            postExplosionGasType = (GasType)Enum.Parse(typeof(GasType), childNode.InnerText);
                        else
                            postExplosionGasType = null;
                        break;
                }
            }
        }

        public SecondaryExplosionSaveable(CompProperties_ExplosiveCE compProperties_ExplosiveCE)
        {
            this.damageDefString = compProperties_ExplosiveCE.explosiveDamageType.defName;
            this.explosionRadius = compProperties_ExplosiveCE.explosiveRadius;
            this.damageAmountBase = compProperties_ExplosiveCE.damageAmountBase;
            this.postExplosionGasType = compProperties_ExplosiveCE.postExplosionGasType;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref damageDefString, "damageDefString");
            Scribe_Values.Look(ref explosionRadius, "explosionRadius");
            Scribe_Values.Look(ref damageAmountBase, "damageAmountBase");
            Scribe_Values.Look(ref postExplosionGasType, "postExplosionGasType");
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteElementString("damageDefString", this.damageDefString);
            writer.WriteElementString("explosionRadius", this.explosionRadius.ToString());
            writer.WriteElementString("damageAmountBase", this.damageAmountBase.ToString());
            writer.WriteElementString("postExplosionGasType", this.postExplosionGasType.ToString());
        }

        public override string ToString()
        {
            string str = "Secondary Explosion:\n";
            str += $"  DamageDef: {this.damageDefString}\n";
            str += $"  Explosion Radius: {this.explosionRadius.ToString()}\n";
            str += $"  Damage amount: {this.damageAmountBase.ToString()}\n";
            str += $"  Post explosion gas type: {this.postExplosionGasType.ToString()}\n";

            return str;
        }
    }
}
