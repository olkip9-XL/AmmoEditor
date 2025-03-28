using AmmoEditor.Misc;
using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;

using UnityEngine;

using Verse;

namespace AmmoEditor
{

    public class ModSetting_AmmoEditor : ModSettings
    {

        public List<AmmoModifier> ammoModifiers = new List<AmmoModifier>();

        public Dictionary<AmmoCategory, List<AmmoSetAE>> ammoSetDictionary = new Dictionary<AmmoCategory, List<AmmoSetAE>>();

        public List<AmmoCategory> ammoCategories = new List<AmmoCategory>();
        public void PostLoad()
        {
            List<ThingCategoryDef> categoryDefs = DefDatabase<ThingCategoryDef>.AllDefs.Where(x =>
                    x.parent == DefDatabase<ThingCategoryDef>.GetNamed("Ammo")).ToList();

            List<ThingDef> processedProjectileDefs = new List<ThingDef>();

            //init categories
            foreach (ThingCategoryDef item in categoryDefs)
            {
                AmmoCategory temp = new AmmoCategory(item);

                ammoSetDictionary[temp] = new List<AmmoSetAE>();
                ammoCategories.Add(temp);
            }

            AmmoCategory ammoCategory1 = new AmmoCategory(AmmoCategorySign.HandGrenade);
            ammoSetDictionary[ammoCategory1] = new List<AmmoSetAE>();
            ammoCategories.Add(ammoCategory1);

            AmmoCategory ammoCategory2 = new AmmoCategory(AmmoCategorySign.Uncategorized);
            ammoSetDictionary[ammoCategory2] = new List<AmmoSetAE>();
            ammoCategories.Add(ammoCategory2);
            //ammoset
            foreach (var item in DefDatabase<AmmoSetDef>.AllDefs)
            {
                AmmoSetAE ammoSet = new AmmoSetAE(item);

                ThingCategoryDef category = categoryDefs.Find(x => item.ammoTypes[0].ammo.IsWithinCategory(x));

                AmmoCategory targetCategory = ammoCategories.Find(x => x.thingCategoryDef == category);

                ammoSetDictionary[targetCategory].Add(new AmmoSetAE(item));

                foreach (var item2 in item.ammoTypes)
                {
                    processedProjectileDefs.Add(item2.projectile);
                }
            }

            //Hand grenade
            AmmoCategory categoryHandGrenade = ammoCategories.Find(x => x.categorySign == AmmoCategorySign.HandGrenade);
            foreach (var item in DefDatabase<ThingDef>.AllDefs.Where(x => x.IsWithinCategory(DefDatabase<ThingCategoryDef>.GetNamed("Grenades"))))
            {
                ThingDef projectile = item.Verbs.FirstOrDefault().defaultProjectile;
                ammoSetDictionary[categoryHandGrenade].Add(new AmmoSetAE(projectile, item));
                processedProjectileDefs.Add(projectile);
            }

            //uncategoried
            AmmoCategory categoryUncategoried = ammoCategories.Find(x => x.categorySign == AmmoCategorySign.Uncategorized);
            foreach (var item in DefDatabase<ThingDef>.AllDefs.Where(x => x.projectile as ProjectilePropertiesCE != null).Except(processedProjectileDefs))
            {
                ammoSetDictionary[categoryUncategoried].Add(new AmmoSetAE(item));
            }

            foreach (var item in this.ammoModifiers)
            {
                item.InitOriginData();
            }
            this.ApplyAll();
        }

        public void ApplyAll()
        {
            string str = "";
            string missingModStr = "Missing mod:\n";
            int count = 0;

            for (int i = 0; i < ammoModifiers.Count; i++)
            {
                if (ammoModifiers[i].CanDelete())
                {
                    ammoModifiers.RemoveAt(i);
                }
                else if (ammoModifiers[i].IsMissingMod())
                {
                    missingModStr += $"{ammoModifiers[i].defName} source: {ammoModifiers[i].sourceModName}\n";
                    continue;
                }
                else
                {
                    ammoModifiers[i].Apply();
                    str += ammoModifiers[i].ToString();
                    count++;
                }
            }

            str = $"[AmmoEditor] Modified {count} items\n\n" + str;

            if (missingModStr != "Missing mod:\n")
            {
                str += missingModStr;
            }

            Log.Message(str);
        }

        public void Apply(ThingDef projectileDef)
        {
            AmmoModifier ammoModifier = ammoModifiers.Find(x => x.defName == projectileDef.defName);
            if (ammoModifier != null)
            {
                ammoModifier.Apply();
            }
        }

        public void ResetAll()
        {
            ammoModifiers.ForEach(AmmoModifier => AmmoModifier.Reset());

            ammoModifiers.Clear();
        }

        public void Reset(ThingDef projectileDef)
        {
            AmmoModifier ammoModifier = ammoModifiers.Find(x => x.defName == projectileDef.defName);
            if (ammoModifier != null)
            {
                ammoModifier.Reset();
                this.ammoModifiers.Remove(ammoModifier);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<AmmoModifier>(ref this.ammoModifiers, "ammoModifiers", LookMode.Deep, Array.Empty<object>());
            if (Scribe.mode == LoadSaveMode.LoadingVars && ammoModifiers == null)
            {
                ammoModifiers = new List<AmmoModifier>();
            }
        }

        public void AddModification(ThingDef def, ModifyKey key, object value, ModifyType type = ModifyType.Modify, int index = -1)
        {
            AmmoModifier ammoModifier = ammoModifiers.Find(x => x.defName == def.defName);
            if (ammoModifier == null)
            {
                ammoModifier = new AmmoModifier(def);
                ammoModifiers.Add(ammoModifier);
            }

            ammoModifier.AddModify(key, value, type, index);
        }

        public void LogInfo()
        {
            string str = "";
            foreach (AmmoModifier ammoModifier in ammoModifiers)
            {
                str += ammoModifier.ToString() + "\n";
            }
            Log.Warning(str);
        }

        public void ExportFile()
        {
            string filePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\AmmoEditorConfig.xml";
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            int count = 0;

            using (XmlWriter writer = XmlWriter.Create(filePath, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("AmmoModifiers");

                foreach (AmmoModifier ammoModifier in ammoModifiers)
                {
                    if (ammoModifier.CanDelete())
                    {
                        continue;
                    }

                    ammoModifier.WriteXml(writer);

                    count++;
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            Messages.Message("AE_ExportMsg".Translate(count, filePath), MessageTypeDefOf.NeutralEvent);
        }

        public void ImportFile()
        {
            this.ResetAll();

            XmlDocument xmlDocument = new XmlDocument();
            string filePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\AmmoEditorConfig.xml";
            xmlDocument.Load(filePath);

            int count = 0;

            XmlNodeList ammoModifiers = xmlDocument.SelectNodes("AmmoModifiers/li");
            foreach (XmlNode ammoModifierNode in ammoModifiers)
            {
                this.ammoModifiers.Add(new AmmoModifier(ammoModifierNode));

                count++;
            }

            this.ApplyAll();

            Messages.Message("AE_ImportMsg".Translate(count, filePath), MessageTypeDefOf.NeutralEvent);
        }
    }

    public class Mod_AmmoEditor : Mod
    {
        public static ModSetting_AmmoEditor settings { get; private set; }

        internal static AmmoSetAE curAmmoSet;

        Rect_AmmoInfo rect_AmmoInfo;
        Rect_Category rect_Category;

        public Mod_AmmoEditor(ModContentPack content) : base(content)
        {
            settings = GetSettings<ModSetting_AmmoEditor>();

            rect_AmmoInfo = new Rect_AmmoInfo();
            //rect_AmmoInfo.curAmmoSet = this.curAmmoSet;

            rect_Category = new Rect_Category();
            //rect_Category.curAmmoSet = this.curAmmoSet;
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Rect leftRect = inRect.LeftPart(0.3f);
            Rect rightRect = inRect.RightPart(0.7f);

            leftRect.height -= 30f;
            rightRect.height -= 30f;
            rightRect.width -= 20f;
            rightRect.x += 20f;

            DoLeftContent(leftRect);
            DoRightContent(rightRect);

            base.DoSettingsWindowContents(inRect);
        }

        private void DoLeftContent(Rect rect)
        {
            rect_Category.DoDialog(rect);
        }

        private void DoRightContent(Rect rect)
        {
            float pannelHeight = 24f;
            Rect buttonPannelRect = new Rect(rect.x, rect.y + rect.height - pannelHeight, rect.width, pannelHeight);
            Rect infoRect = new Rect(rect.x, rect.y, rect.width, rect.height - pannelHeight - 6f);

            Rect rect1 = new Rect(buttonPannelRect.x, buttonPannelRect.y, 100f, buttonPannelRect.height);
            if (Widgets.ButtonText(rect1, "AE_ResetAll".Translate()))
            {
                settings.ResetAll();
            }

            Rect rect2 = new Rect(buttonPannelRect.x + buttonPannelRect.width - 100f, buttonPannelRect.y, 100f, buttonPannelRect.height);
            if (Widgets.ButtonText(rect2, "AE_Export".Translate()))
            {
                settings.ExportFile();
            }

            Rect rect3 = new Rect(buttonPannelRect.x + buttonPannelRect.width - 200f - 6f, buttonPannelRect.y, 100f, buttonPannelRect.height);
            if (Widgets.ButtonText(rect3, "AE_Import".Translate()))
            {
                settings.ImportFile();
            }

            this.rect_AmmoInfo.DoDialog(infoRect);
        }

        public override string SettingsCategory()
        {
            return "CE Ammo Editor";
        }
    }
}

