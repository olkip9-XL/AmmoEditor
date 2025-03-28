using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;
using Verse.Noise;

namespace AmmoEditor
{
    public abstract class AmmoEdiotr_Saveable : IExposable
    {
        public AmmoEdiotr_Saveable() { }
        public AmmoEdiotr_Saveable(XmlNode rootNode) { }
        public abstract void ExposeData();
        public abstract void WriteXml(XmlWriter writer);
    }
}
