using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Verse;

namespace AmmoEditor
{

   public class ModSetting_AmmoEditor : ModSettings
   {

        List<AmmoModifier> ammoModifiers = new List<AmmoModifier>();

        public static void Apply()
        {

        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref ammoModifiers, "ammoModifiers", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs && ammoModifiers == null)
            {
                ammoModifiers = new List<AmmoModifier>();

                //test
                AmmoModifier ammoModifier = new AmmoModifier();
            }
        }
    }

    public class Mod_AmmoEditor : Mod
    {
        public static ModSetting_AmmoEditor settings { get; private set; }

        public Mod_AmmoEditor(ModContentPack content) : base(content)
        {
            settings = GetSettings<ModSetting_AmmoEditor>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            const float LINE_GAP = 6f;

            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);


            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Ammo Editor";
        }
    }
}
