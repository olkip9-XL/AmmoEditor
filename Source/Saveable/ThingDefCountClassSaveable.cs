using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

using UnityEngine;
using RimWorld;
using CombatExtended;
using System.Xml;
namespace AmmoEditor
{
    public class ThingDefCountClassSaveable : AmmoEdiotr_Saveable
    {
        public ThingDef ThingDef
        {
            get
            {
                return DefDatabase<ThingDef>.GetNamed(thingDefString, false);
            }
            set
            {
                this.thingDefString = value.defName;
            }
        }
        private string thingDefString;

        public int count;
        public ThingDefCountClassSaveable() { }

        public ThingDefCountClassSaveable(XmlNode rootNode) : base(rootNode)
        {
            foreach (XmlNode node in rootNode.ChildNodes)
            {
                switch (node.Name)
                {
                    case "def":
                        this.thingDefString = node.InnerText;
                        break;
                    case "count":
                        this.count = int.Parse(node.InnerText);
                        break;
                }
            }
        }
        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("li");
            writer.WriteElementString("def", this.thingDefString);
            writer.WriteElementString("count", this.count.ToString());
            writer.WriteEndElement();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref thingDefString, "thingDefString");
            Scribe_Values.Look(ref count, "count", 1);
        }
      
        public static implicit operator ThingDefCountClassSaveable(ThingDefCountClass thingDefCountClass)
        {
            return new ThingDefCountClassSaveable
            {
                ThingDef = thingDefCountClass.thingDef,
                count = thingDefCountClass.count,
            };
        }

        public static implicit operator ThingDefCountClass(ThingDefCountClassSaveable saveable)
        {
            if(saveable.ThingDef == null)
                return null;

            return new ThingDefCountClass
            {
                thingDef = saveable.ThingDef,
                count = saveable.count,
            };
        }

        public override string ToString()
        {
            return $"{this.thingDefString} : {this.count}";
        }
    }
}