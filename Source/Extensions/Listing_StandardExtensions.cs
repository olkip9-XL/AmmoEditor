using AmmoEditor.Extensions;
using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AmmoEditor
{
    internal static class Listing_StandardExtensions
    {
        private static bool TextField<T>(Rect rect, ref T value) where T : struct 
        {
            T originalValue = value;
            T newValue = value;
            string buffer = value.ToString();
            Widgets.TextFieldNumeric(rect, ref newValue, ref buffer, min:-10);
            if (buffer == "")
                newValue = default(T);

            if (!newValue.Equals(originalValue))
            {
                value = newValue;
                return true;
            }
            else return false;
        }
        public static void AmmoSubtitle(this Listing_Standard listing, ThingDef projectileDef, Texture2D icon, string subTitle, Action onReset)
        {
            Rect rect = listing.GetRect(30f);

            float height = rect.height;

            Rect signRect = new Rect(rect.x, rect.y, 3f, height);
            Rect iconRect = new Rect(signRect.x + 3f, rect.y, height, height);
            Rect labelRect = new Rect(rect.x + 3f + height, rect.y, rect.width - height - height - 3f, height);
            Rect buttonRect = new Rect(rect.x + rect.width - height, rect.y, height, height);

            if (projectileDef.Modified())
            {
                Widgets.DrawBoxSolidWithOutline(signRect, new Color(85f / 256f, 177f / 256f, 85f / 256f), new Color(0, 0, 0), 0);
            }
            Widgets.DrawTextureFitted(iconRect, icon, 0.7f);

            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, subTitle);
            Text.Anchor = TextAnchor.UpperLeft;

            Texture2D resetIcon = ContentFinder<Texture2D>.Get("Icons_AE/Reset");
            if (Widgets.ButtonImage(buttonRect, resetIcon, tooltip: "AE_Reset".Translate()))
            {
                onReset();
            }

            listing.Gap(listing.verticalSpacing);
        }
        public static void TextFieldNumericLine<T>(this Listing_Standard listing, string label, T value, float fieldWidth, Action<T> onChange, float indent = 0f) where T : struct
        {
            Rect rect = listing.GetRect(Text.LineHeight);
            rect.x += indent;
            rect.width -= indent;

            Rect fieldRect = new Rect(rect.x + rect.width - fieldWidth, rect.y, fieldWidth, rect.height);

            Widgets.Label(rect, label);

            if(TextField(fieldRect,ref value))
            {
                onChange(value);
            }

            listing.Gap(listing.verticalSpacing);
        }
        public static void ButtonLine(this Listing_Standard listing, string label, string buttonLabel, float buttonWidth, Action onClick, float indent = 0f)
        {
            Rect rect = listing.GetRect(Text.LineHeight);
            rect.x += indent;
            rect.width -= indent;

            Rect buttonRect = new Rect(rect.x + rect.width - buttonWidth, rect.y, buttonWidth, rect.height);

            Widgets.Label(rect, label);

            if (Widgets.ButtonText(buttonRect, buttonLabel))
            {
                onClick();
            }
            listing.Gap(listing.verticalSpacing);
        }

        public static void ButtonTextFieldLine<T>(this Listing_Standard listing, string label, string buttonLabel, float buttonWidth, Action onBtnClick, T value, float fieldWidth, Action<T> onFieldChange, float indent = 0f) where T : struct
        {
            Rect rect = listing.GetRect(Text.LineHeight);
            rect.x += indent;
            rect.width -= indent;

            Rect fieldRect = new Rect(rect.x + rect.width - fieldWidth, rect.y, fieldWidth, rect.height);
            Rect buttonRect = new Rect(fieldRect.x - 6f - buttonWidth, rect.y, buttonWidth, rect.height);

            Widgets.Label(rect, label);

            if (Widgets.ButtonText(buttonRect, buttonLabel))
            {
                onBtnClick();
            }


            if (TextField(fieldRect, ref value))
            {
                onFieldChange(value);
            }

            listing.Gap(listing.verticalSpacing);
        }
        public static void TitleWithButton(this Listing_Standard listing, string title, Action onBtnPressed, Texture2D buttonImage)
        {
            Rect rect = listing.GetRect(Text.LineHeight);
            Rect buttonRect = new Rect(rect.x + rect.width - rect.height, rect.y, rect.height, rect.height);
            Widgets.Label(rect, title);
            if (Widgets.ButtonImage(buttonRect, TexButton.Add))
            {
                onBtnPressed();
            }
            listing.Gap(listing.verticalSpacing);
        }
        public static void DamageRow(this Listing_Standard listing, ThingDef projectileDef, int index, float buttonWidth, float fieldWidth, Action onLabelClick, Action<int> onFieldChange, Action onDelete)
        {

            Rect rect = listing.GetRect(Text.LineHeight);
            rect.width -= 20f;
            rect.x += 20f;

            Rect labelRect = new Rect(rect.x, rect.y, buttonWidth, rect.height);
            Rect fieldRect = new Rect(rect.x + buttonWidth + 6f, rect.y, fieldWidth, rect.height);
            Rect deleteRect = new Rect(rect.x + rect.width - rect.height, rect.y, rect.height, rect.height);

            ProjectilePropertiesCE props = projectileDef.projectile as ProjectilePropertiesCE;

            DamageDef damageDef = index == -1 ? props.damageDef : props.secondaryDamage[index].def;
            int damageAmount = index == -1 ? (int)typeof(ProjectileProperties).GetField("damageAmountBase", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(props) : props.secondaryDamage[index].amount;

            if (Widgets.ButtonText(labelRect, damageDef == null ? "null" : damageDef.label))
            {
                onLabelClick();
            }


            if (TextField(fieldRect, ref damageAmount))
            {
                onFieldChange(damageAmount);
            }

            if (index != -1)
            {
                if (Widgets.ButtonImage(deleteRect, TexButton.Delete))
                {
                    onDelete();
                }
            }

            listing.Gap(listing.verticalSpacing);
        }

        public static void SearchBar(this Listing_Standard listing, ref string keyWords)
        {
            Rect rect = listing.GetRect(Text.LineHeight);
            Rect searchBarRect = new Rect(rect.x, rect.y, rect.width - rect.height, rect.height);
            Rect iconRect = new Rect(rect.x + rect.width - rect.height, rect.y, rect.height, rect.height);

            keyWords = Widgets.TextField(searchBarRect, keyWords);
            Widgets.ButtonImage(iconRect, TexButton.Search);

            listing.Gap(listing.verticalSpacing);
        }
        public static void FragmentsRow(this Listing_Standard listing, ThingDef projectileDef, int index, Action<int, int> onFieldChange, Action<int> onDelete)
        {
            if (!projectileDef.HasComp<CompFragments>())
            {
                return;
            }

            List<ThingDefCountClass> fragments = projectileDef.GetCompProperties<CompProperties_Fragments>().fragments;

            Rect rect = listing.GetRect(Text.LineHeight);
            rect.width -= 20f;
            rect.x += 20f;

            float labelWidth = 200f;

            Rect labelRect = new Rect(rect.x, rect.y, labelWidth, rect.height);
            Rect fieldRect = new Rect(rect.x + labelWidth + 6f, rect.y, 100f, rect.height);
            Rect deleteRect = new Rect(rect.x + rect.width - rect.height, rect.y, rect.height, rect.height);

            Widgets.Label(labelRect, fragments[index].thingDef.label);

            //x sign
            Rect xSignRect = new Rect(fieldRect.x - 10f, fieldRect.y, 10f, fieldRect.height);
            Widgets.Label(xSignRect, "x");

            int count = fragments[index].count;
            if (TextField(fieldRect, ref count))
            {
                onFieldChange(index, count);
            }

            if (Widgets.ButtonImage(deleteRect, TexButton.Delete))
            {
                onDelete(index);
            }

            listing.Gap(listing.verticalSpacing);
        }

    }
}
