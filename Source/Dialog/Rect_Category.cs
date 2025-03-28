using AmmoEditor.Misc;
using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AmmoEditor
{
    internal class Rect_Category
    {
       

        //filter
        private string keyword = string.Empty;
        private AmmoCategory curCategory;
        private bool modifiedDefsOnly = false;

        private Vector2 scrollPosition = Vector2.zero;

        private ModSetting_AmmoEditor settings => Mod_AmmoEditor.settings;
        private AmmoSetAE curAmmoSet
        {
            get
            {
                return Mod_AmmoEditor.curAmmoSet;
            }
            set
            {
                Mod_AmmoEditor.curAmmoSet = value;
            }
        }

        private List<AmmoSetAE> AmmoSetDefs
        {
            get
            {
                List<AmmoSetAE> list;

                if (curCategory == null)
                {
                    list = new List<AmmoSetAE>();
                    foreach (var item in settings.ammoSetDictionary)
                    {
                        list.AddRange(item.Value);
                    }
                }
                else
                {
                    list = settings.ammoSetDictionary[curCategory];
                }

                //key words
                list = list.Where(x =>
                    x.Label.ToLower().Contains(keyword.ToLower())
                ).ToList();

                //modifiedDefsOnly
                if (modifiedDefsOnly)
                {
                    list = list.Where(x =>
                        x.ContainModified
                    ).ToList();
                }

                return list;
            }
        }

        public void DoDialog(Rect rect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(rect);

            if (listing.ButtonTextLabeled("AE_Category".Translate(), curCategory?.Label ?? "AE_All".Translate()))
            {
                List<FloatMenuOption> floatMenuList = new List<FloatMenuOption>();

                floatMenuList.Add(new FloatMenuOption("AE_All".Translate(), () =>
                {
                    curCategory = null;
                }));

                foreach (var item in settings.ammoCategories)
                {
                    floatMenuList.Add(new FloatMenuOption(item.Label, () =>
                    {
                        curCategory = item;
                    }));
                }

                if (floatMenuList.Any())
                {
                    Find.WindowStack.Add(new FloatMenu(floatMenuList));
                }
            }

            listing.CheckboxLabeled("AE_ModifiedDefsOnly".Translate(), ref modifiedDefsOnly);

            listing.Gap(6f);
            listing.SearchBar(ref keyword);

            listing.Gap(6f);
            List<AmmoSetAE> ammoSetDefs = AmmoSetDefs;
            float LINE_HEIGHT = 30f;
            float curHeight = listing.CurHeight;

            float innerRectHeight = 0f;
            innerRectHeight += LINE_HEIGHT * ammoSetDefs.Count;

            Widgets.BeginScrollView(new Rect(rect.x, curHeight, rect.width, rect.height - curHeight), ref scrollPosition, new Rect(rect.x, curHeight, rect.width - 16f, innerRectHeight), true);
            int curI = 0;

            foreach (var item in ammoSetDefs)
            {
                DrawAmmoSetItem(item);
            }

            Widgets.EndScrollView();

            listing.End();

            void DrawAmmoSetItem(AmmoSetAE item)
            {
                Rect inRect = new Rect(rect.x, curI++ * LINE_HEIGHT + curHeight, rect.width, LINE_HEIGHT);

                if (Widgets.ButtonInvisible(inRect))
                {
                    //set current AmmoSetDef
                    curAmmoSet = item;

                    Messages.Message(curAmmoSet.Name, MessageTypeDefOf.SilentInput);
                }

                //Draw the box
                Rect iconRect = new Rect(0, inRect.y, LINE_HEIGHT, LINE_HEIGHT);
                Widgets.DrawTextureFitted(iconRect, item.Icon, 0.7f);

                Rect labelRect = new Rect(0 + LINE_HEIGHT, inRect.y, inRect.width - LINE_HEIGHT, LINE_HEIGHT);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(labelRect, item.Label);
                Text.Anchor = TextAnchor.UpperLeft;

                TooltipHandler.TipRegion(inRect, "AE_ItemDes".Translate(item.Description, item.modContentPack.Name));

                if (curAmmoSet == item)
                {
                    Widgets.DrawHighlightSelected(inRect);
                }
            }

        }
    }
}
