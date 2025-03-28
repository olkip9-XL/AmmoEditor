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
    public class SecondaryDamageSaveable : AmmoEdiotr_Saveable
    {
        public DamageDef def
        {
            get
            {
                return DefDatabase<DamageDef>.GetNamed(damageDefString, false);
            }
            set
            {
                damageDefString = value.defName;
            }
        }
        private string damageDefString;

        public int amount;
        public string sourceModName;

        public SecondaryDamageSaveable()
        {
        }

        public SecondaryDamageSaveable(DamageDef damageDef, int amount)
        {
            this.def = damageDef;
            this.amount = amount;
        }

        public SecondaryDamageSaveable(XmlNode rootNode)
        {
            foreach (XmlNode node in rootNode.ChildNodes)
            {
                switch (node.Name)
                {
                    case "def":
                        this.damageDefString =node.InnerText;
                        break;
                    case "amount":
                        this.amount = int.Parse(node.InnerText);
                        break;
                }
            }
        }
        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("li");
            writer.WriteElementString("def", this.damageDefString);
            writer.WriteElementString("amount", this.amount.ToString());
            writer.WriteEndElement();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref damageDefString, "damageDefString");
            Scribe_Values.Look(ref amount, "amount");
        }

        public static implicit operator SecondaryDamageSaveable(SecondaryDamage secondaryDamage)
        {
            return new SecondaryDamageSaveable
            {
                def = secondaryDamage.def,
                amount = secondaryDamage.amount
            };

        }

        public static implicit operator SecondaryDamage(SecondaryDamageSaveable saveable)
        {
            if(saveable.def == null)
                return null;

            return new SecondaryDamage
            {
                def = saveable.def,
                amount = saveable.amount
            };
        }

        public override string ToString()
        {
            return $"{this.damageDefString} : {this.amount}";
        }

    }
}
