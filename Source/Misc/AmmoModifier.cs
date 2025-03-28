
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

using CombatExtended;
using System.Reflection;
using System.Xml;
using RimWorld;

namespace AmmoEditor
{
    public enum ModifyKey
    {
        DamageAmount,
        ArmorPenetrationSharp,
        ArmorPenetrationBlunt,
        DamageDef,
        ExplosionRadius,
        SuppressionFactor,
        PostExplosionGasType,
        Fragments,
        SecondaryExplosion
    }

    public enum ModifyType
    {
        Add,
        Delete,
        Modify
    }

    public class AmmoModifier : IExposable
    {
        public ThingDef def
        {
            get
            {
                return DefDatabase<ThingDef>.GetNamed(defString);
            }
            set
            {
                defString = value.defName;
            }
        }
        private string defString;
        public string defName => defString;

        public DamageDef damageDef
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

        public int damageAmountBase;
        public float armorPenetrationSharp;
        public float armorPenetrationBlunt;
        public List<SecondaryDamageSaveable> secondaryDamage = new List<SecondaryDamageSaveable>();
        public float explosionRadius;
        public float suppressionFactor;
        public GasType? postExplosionGasType;
        public List<ThingDefCountClassSaveable> fragments = new List<ThingDefCountClassSaveable>();
        public SecondaryExplosionSaveable secondaryExplosion;

        public AmmoModifier originData;
        public string sourceModName;

        public bool IsExplosive
        {
            get
            {
                if (this.originData == null)
                    return this.explosionRadius > 0;

                return this.originData.explosionRadius > 0;
            }
        }

        public AmmoModifier()
        {
        }
        public AmmoModifier(ThingDef def, bool isOrigin = false)
        {
            this.def = def;

            this.sourceModName = def.modContentPack.Name;

            ProjectilePropertiesCE props = def.projectile as ProjectilePropertiesCE;
            if (props == null)
            {
                Log.Error("ProjectilePropertiesCE is null");
                return;
            }

            this.damageAmountBase = (int)typeof(ProjectileProperties).GetField("damageAmountBase", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(props);

            this.armorPenetrationSharp = props.armorPenetrationSharp;
            this.armorPenetrationBlunt = props.armorPenetrationBlunt;
            this.damageDef = props.damageDef;
            this.explosionRadius = props.explosionRadius;
            this.suppressionFactor = props.suppressionFactor;
            this.postExplosionGasType = props.postExplosionGasType;

            this.secondaryDamage.Clear();
            this.secondaryDamage.AddRange(props.secondaryDamage.Select(x => (SecondaryDamageSaveable)x));

            this.fragments.Clear();
            if (def.HasComp<CompFragments>())
            {
                this.fragments.Clear();
                this.fragments.AddRange(def.GetCompProperties<CompProperties_Fragments>().fragments.Select(x => (ThingDefCountClassSaveable)x));
            }

            if (def.HasComp<CompExplosiveCE>())
            {
                CompProperties_ExplosiveCE prop = def.GetCompProperties<CompProperties_ExplosiveCE>();

                this.secondaryExplosion = new SecondaryExplosionSaveable()
                {
                    DamageDef = prop.explosiveDamageType,
                    damageAmountBase = prop.damageAmountBase,
                    explosionRadius = prop.explosiveRadius,
                    postExplosionGasType = prop.postExplosionGasType
                };
            }

            if (!isOrigin)
            {
                this.originData = new AmmoModifier(def, isOrigin: true);
            }
            else
            {
                this.originData = null;
            }
        }

        public AmmoModifier(XmlNode ammoModifierNode)
        {
            foreach (XmlNode node in ammoModifierNode.ChildNodes)
            {
                switch (node.Name)
                {
                    case "targetDef":
                        this.defString = node.InnerText;
                        break;
                    case "sourceModName":
                        this.sourceModName = node.InnerText;
                        break;
                    case "damageAmountBase":
                        this.damageAmountBase = int.Parse(node.InnerText);
                        break;
                    case "armorPenetrationSharp":
                        this.armorPenetrationSharp = float.Parse(node.InnerText);
                        break;
                    case "armorPenetrationBlunt":
                        this.armorPenetrationBlunt = float.Parse(node.InnerText);
                        break;
                    case "damageDef":
                        this.damageDefString = node.InnerText;
                        break;
                    case "explosionRadius":
                        this.explosionRadius = float.Parse(node.InnerText);
                        break;
                    case "secondaryDamages":
                        foreach (XmlNode secondaryDamageNode in node.ChildNodes)
                        {
                            this.secondaryDamage.Add(new SecondaryDamageSaveable(secondaryDamageNode));
                        }
                        break;
                    case "suppressionFactor":
                        this.suppressionFactor = float.Parse(node.InnerText);
                        break;
                    case "postExplosionGasType":
                        if (node.InnerText.Length > 0)
                        {
                            this.postExplosionGasType = (GasType)Enum.Parse(typeof(GasType), node.InnerText);
                        }
                        else
                        {
                            this.postExplosionGasType = null;
                        }
                        break;
                    case "fragments":
                        foreach (XmlNode fragmentNode in node.ChildNodes)
                        {
                            this.fragments.Add(new ThingDefCountClassSaveable(fragmentNode));
                        }
                        break;
                    case "secondaryExplosion":
                        this.secondaryExplosion = new SecondaryExplosionSaveable(node);
                        break;
                }
            }

            if (IsMissingMod())
            {
                originData = null;
            }
            else
            {
                originData = new AmmoModifier(def, isOrigin: true);
            }
        }

        public void Apply()
        {
            if (IsMissingMod())
            {
                Log.Warning($"Missing mod [{this.sourceModName}], skip {this.defString}");
                return;
            }

            ProjectilePropertiesCE prop = (def.projectile as ProjectilePropertiesCE);
            if (prop == null)
            {
                Log.Error("ProjectilePropertiesCE is null");
                return;
            }

            typeof(ProjectileProperties).GetField("damageAmountBase", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(prop, damageAmountBase);
            prop.armorPenetrationSharp = armorPenetrationSharp;
            prop.armorPenetrationBlunt = armorPenetrationBlunt;

            if (this.damageDef != null)
                prop.damageDef = damageDef;

            prop.secondaryDamage.Clear();
            prop.secondaryDamage.AddRange(secondaryDamage.Select(x => (SecondaryDamage)x).Where(x => x != null));

            prop.explosionRadius = explosionRadius;
            prop.suppressionFactor = suppressionFactor;
            prop.postExplosionGasType = postExplosionGasType;

            if (def.HasComp<CompFragments>())
            {
                CompProperties_Fragments compProperties_Fragments = def.GetCompProperties<CompProperties_Fragments>();
                compProperties_Fragments.fragments.Clear();
                compProperties_Fragments.fragments.AddRange(fragments.Select(x => (ThingDefCountClass)x));
            }

            if (def.HasComp<CompExplosiveCE>())
            {
                CompProperties_ExplosiveCE compProperties_ExplosiveCE = def.GetCompProperties<CompProperties_ExplosiveCE>();

                compProperties_ExplosiveCE.damageAmountBase = this.secondaryExplosion.damageAmountBase;
                if (this.secondaryExplosion.DamageDef != null)
                    compProperties_ExplosiveCE.explosiveDamageType = this.secondaryExplosion.DamageDef;
                compProperties_ExplosiveCE.explosiveRadius = this.secondaryExplosion.explosionRadius;
                compProperties_ExplosiveCE.postExplosionGasType = this.secondaryExplosion.postExplosionGasType;
            }
        }

        public void Reset()
        {
            if (IsMissingMod())
            {
                //Log.Warning($"Missing mod [{this.sourceModName}] when reset, skip {this.defString}");
                return;
            }

            if (originData == null)
            {
                Log.Error($"OriginData of {this.defString} is null when reset");
                return;
            }

            this.armorPenetrationBlunt = originData.armorPenetrationBlunt;
            this.armorPenetrationSharp = originData.armorPenetrationSharp;
            this.damageAmountBase = originData.damageAmountBase;
            this.damageDef = originData.damageDef;

            this.secondaryDamage.Clear();
            this.secondaryDamage.AddRange(originData.secondaryDamage);

            this.explosionRadius = originData.explosionRadius;
            this.suppressionFactor = originData.suppressionFactor;
            this.postExplosionGasType = originData.postExplosionGasType;

            if (def.HasComp<CompFragments>())
            {
                this.fragments.Clear();
                this.fragments.AddRange(originData.fragments);
            }

            if (def.HasComp<CompExplosiveCE>())
            {
                this.secondaryExplosion = originData.secondaryExplosion;
            }

            this.originData.Apply();
        }

        public void AddModify(ModifyKey key, object value, ModifyType type = ModifyType.Modify, int index = -1)
        {
            //varify index
            if (type != ModifyType.Add)
            {
                switch (key)
                {
                    case ModifyKey.DamageDef:
                        if (index < -1 || index >= this.secondaryDamage.Count)
                        {
                            Log.Error("Invalid index when modify DamageDef :" + index);
                            return;
                        }
                        break;
                    case ModifyKey.DamageAmount:
                        if (index < -1 || index >= this.secondaryDamage.Count)
                        {
                            Log.Error("Invalid index when modify DamageAmount :" + index);
                            return;
                        }
                        break;
                    case ModifyKey.Fragments:
                        if (index < 0 || index >= this.fragments.Count)
                        {
                            Log.Error("Invalid index when modify fragments :" + index);
                            return;
                        }
                        break;
                    default:
                        break;
                }
            }

            switch (key)
            {
                case ModifyKey.DamageDef:
                    if (type == ModifyType.Add)
                    {
                        this.secondaryDamage.Add(new SecondaryDamageSaveable()
                        {
                            def = (DamageDef)value,
                            amount = 0
                        });
                    }
                    else if (type == ModifyType.Modify)
                    {
                        if (index == -1)
                        {
                            this.damageDef = (DamageDef)value;
                        }
                        else
                        {
                            this.secondaryDamage[index].def = (DamageDef)value;

                        }
                    }
                    else if (type == ModifyType.Delete)
                    {
                        if (index == -1)
                        {
                            Log.Error("Invalid index when delete DamageDef :" + index);
                            break;
                        }
                        this.secondaryDamage.RemoveAt(index);
                    }
                    break;
                case ModifyKey.DamageAmount:
                    if (index == -1)
                    {
                        this.damageAmountBase = (int)value;
                    }
                    else
                    {
                        this.secondaryDamage[index].amount = (int)value;
                    }
                    break;
                case ModifyKey.ArmorPenetrationSharp:
                    armorPenetrationSharp = (float)value;
                    break;
                case ModifyKey.ArmorPenetrationBlunt:
                    armorPenetrationBlunt = (float)value;
                    break;
                case ModifyKey.ExplosionRadius:
                    explosionRadius = (float)value;
                    break;
                case ModifyKey.SuppressionFactor:
                    suppressionFactor = (float)value;
                    break;
                case ModifyKey.PostExplosionGasType:
                    postExplosionGasType = (GasType?)value;
                    break;
                case ModifyKey.Fragments:
                    if (type == ModifyType.Add)
                    {
                        this.fragments.Add((ThingDefCountClass)value);
                    }
                    else if (type == ModifyType.Modify)
                    {
                        this.fragments[index] = (ThingDefCountClass)value;
                    }
                    else if (type == ModifyType.Delete)
                    {
                        this.fragments.RemoveAt(index);
                    }
                    break;
                case ModifyKey.SecondaryExplosion:
                    this.secondaryExplosion = (SecondaryExplosionSaveable)value;
                    break;
            }
        }

        public bool CanDelete()
        {
            //Missing mod
            if (this.originData == null || IsMissingMod())
                return false;

            //Check Data
            if (this.def != originData.def)
                return false;

            if (this.damageAmountBase != originData.damageAmountBase)
                return false;

            if (this.armorPenetrationSharp != originData.armorPenetrationSharp)
                return false;

            if (this.armorPenetrationBlunt != originData.armorPenetrationBlunt)
                return false;

            if (this.damageDef != originData.damageDef)
                return false;

            if (!new HashSet<SecondaryDamageSaveable>(this.secondaryDamage).SetEquals(this.originData.secondaryDamage))
                return false;

            if (this.explosionRadius != originData.explosionRadius)
                return false;

            if (this.suppressionFactor != originData.suppressionFactor)
                return false;

            if (this.postExplosionGasType != originData.postExplosionGasType)
                return false;

            if (!new HashSet<ThingDefCountClassSaveable>(this.fragments).SetEquals(this.originData.fragments))
                return false;

            if (this.secondaryExplosion != originData.secondaryExplosion)
                return false;

            return true;
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("li");
            writer.WriteElementString("targetDef", this.defString);
            writer.WriteElementString("sourceModName", this.sourceModName);

            if (this.IsExplosive)
            {
                writer.WriteElementString("explosionRadius", this.explosionRadius.ToString());
                writer.WriteElementString("postExplosionGasType", this.postExplosionGasType.ToString());
            }
            else
            {
                writer.WriteElementString("armorPenetrationSharp", this.armorPenetrationSharp.ToString());
                writer.WriteElementString("armorPenetrationBlunt", this.armorPenetrationBlunt.ToString());
                writer.WriteStartElement("secondaryDamages");
                foreach (var secondaryDamage in this.secondaryDamage)
                {
                    secondaryDamage.WriteXml(writer);
                }
                writer.WriteEndElement();
            }

            writer.WriteElementString("damageAmountBase", this.damageAmountBase.ToString());
            writer.WriteElementString("damageDef", this.damageDefString);
            writer.WriteElementString("suppressionFactor", this.suppressionFactor.ToString());

            if (this.fragments.Any())
            {
                writer.WriteStartElement("fragments");
                foreach (var fragment in this.fragments)
                {
                    fragment.WriteXml(writer);
                }
                writer.WriteEndElement();
            }

            if (this.secondaryExplosion != null)
            {
                writer.WriteStartElement("secondaryExplosion");
                this.secondaryExplosion.WriteXml(writer);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref damageDefString, "damageDefString");
            Scribe_Values.Look(ref defString, "defString");
            Scribe_Values.Look(ref sourceModName, "sourceModName");

            Scribe_Values.Look(ref damageAmountBase, "damageAmountBase");
            Scribe_Values.Look(ref armorPenetrationSharp, "armorPenetrationSharp");
            Scribe_Values.Look(ref armorPenetrationBlunt, "armorPenetrationBlunt");

            Scribe_Collections.Look(ref secondaryDamage, "secondaryDamage", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.LoadingVars && secondaryDamage == null)
            {
                secondaryDamage = new List<SecondaryDamageSaveable>();
            }

            Scribe_Values.Look(ref explosionRadius, "explosionRadius");
            Scribe_Values.Look(ref suppressionFactor, "suppressionFactor");
            Scribe_Values.Look(ref postExplosionGasType, "postExplosionGasType");

            Scribe_Collections.Look(ref fragments, "fragments", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.LoadingVars && fragments == null)
            {
                fragments = new List<ThingDefCountClassSaveable>();
            }

            Scribe_Deep.Look(ref secondaryExplosion, "secondaryExplosion");
        }

        public override string ToString()
        {
            string str = "";

            str += $"Def: {defString}\n";
            str += $"source: {sourceModName}\n";

            if (this.IsExplosive)
            {
                str += $"ExplosionRadius: {explosionRadius}\n";
                str += $"PostExplosionGasType: {(postExplosionGasType == null ? "None" : postExplosionGasType.GetLabel())}\n";
            }
            else
            {
                str += $"ArmorPenetrationSharp: {armorPenetrationSharp}\n";
                str += $"ArmorPenetrationBlunt: {armorPenetrationBlunt}\n";
            }

            str += $"DamageAmountBase: {damageAmountBase}\n";
            str += $"DamageDef: {damageDefString}\n";
            str += $"SuppressionFactor: {suppressionFactor}\n";

            if (secondaryDamage.Count > 0)
            {
                str += "SecondaryDamage:\n";
                foreach (var secondary in secondaryDamage)
                {
                    str += $"  {secondary.ToString()}\n";
                }
            }

            if (def.HasComp<CompFragments>())
            {
                if (this.fragments.Count > 0)
                {
                    str += "Fragments:\n";
                    foreach (var fragment in fragments)
                    {
                        str += $"  {fragment.ToString()}\n";
                    }
                }
            }

            if (def.HasComp<CompExplosiveCE>())
            {
                str += this.secondaryExplosion.ToString();
            }

            if (originData != null)
            {
                str += "[OriginData]----------------------\n";
                str += originData.ToString();
            }

            str += "\n";

            return str;
        }
        public bool IsMissingMod()
        {
            return !ModLister.HasActiveModWithName(sourceModName);
        }

        public void InitOriginData()
        {
            if (originData == null && !IsMissingMod())
            {
                originData = new AmmoModifier(def, isOrigin: true);
            }
        }
    }
}
