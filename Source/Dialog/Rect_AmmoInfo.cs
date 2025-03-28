using AmmoEditor.Extensions;
using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AmmoEditor
{
    internal class Rect_AmmoInfo
    {

        private Vector2 scrollPosition = Vector2.zero;
        private float curWindowHeight = 0;
        private List<ThingDef> fragmentList = new List<ThingDef>();

        //const
        private ModSetting_AmmoEditor settings => Mod_AmmoEditor.settings;
        private AmmoSetAE curAmmoSet => Mod_AmmoEditor.curAmmoSet;

        private readonly float damageButtonWidth = 150f;

        private List<DamageDef> ExsitDamage(ThingDef projectile)
        {
            List<DamageDef> damageDefs = new List<DamageDef>();

            ProjectilePropertiesCE props = projectile.projectile as ProjectilePropertiesCE;
            if (props == null)
            {
                return damageDefs;
            }

            damageDefs.Add(props.damageDef);

            foreach (SecondaryDamage secondDamage in props.secondaryDamage)
            {
                damageDefs.Add(secondDamage.def);
            }

            if (projectile.HasComp<CompExplosiveCE>())
            {
                damageDefs.Add(projectile.GetCompProperties<CompProperties_ExplosiveCE>().explosiveDamageType);
            }

            return damageDefs;
        }
        private IEnumerable<DamageDef> AvaliableDamageDef(ThingDef projectileDef, bool explosiveOnly = false)
        {
            List<DamageDef> exsitDefs = ExsitDamage(projectileDef);

            foreach (var def in DefDatabase<DamageDef>.AllDefs)
            {
                if (!exsitDefs.Contains(def) && (!explosiveOnly || def.soundExplosion != null))
                {
                    yield return def;
                }
            }
            yield break;
        }
        private void ShowDamageFloatMenu(ThingDef projectileDef, ModifyType type = ModifyType.Modify, int index = -1)
        {
            FloatMenuUtility.MakeMenu<DamageDef>(AvaliableDamageDef(projectileDef, projectileDef.IsExplosive()),
                           (DamageDef damageDef) => $"{damageDef.label} ({damageDef.defName})",
                           (DamageDef damageDef) => delegate ()
                           {
                               settings.AddModification(projectileDef, ModifyKey.DamageDef, damageDef, type, index);

                               Messages.Message(damageDef.defName, MessageTypeDefOf.SilentInput);
                           });
        }

        internal void DoDialog(Rect rect)
        {
            if (curAmmoSet == null)
            {
                return;
            }

            Rect innerRect = new Rect(rect.x, rect.y, rect.width - 30f, curWindowHeight);
            curWindowHeight = 0f;

            Widgets.BeginScrollView(rect, ref scrollPosition, innerRect, true);


            DoAmmoSet(innerRect, curAmmoSet);

            Widgets.EndScrollView();
        }
        private void DoAmmoSet(Rect rect, AmmoSetAE ammoSet)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(rect);

            Text.Font = GameFont.Medium;
            listing.Label(ammoSet.Label);
            Text.Font = GameFont.Small;

            curWindowHeight += 32f;

            foreach (var item in ammoSet.ammoLinks)
            {
                //Texture2D icon = item.ammo.graphic.MatSingle.mainTexture as Texture2D;
                DoSingleAmmo(listing, item.projectile, item.Icon, item.Label);
            }

            listing.End();
        }
        private void DoSingleAmmo(Listing_Standard listing, ThingDef projectileDef, Texture2D icon, string subTitle)
        {
            if (projectileDef == null)
                return;

            listing.GapLine();

            // subtitle
            listing.AmmoSubtitle(projectileDef, icon, subTitle, () =>
            {
                settings.Reset(projectileDef);
                Messages.Message("AE_ResetMsg".Translate(projectileDef.defName), MessageTypeDefOf.SilentInput);
            });
            curWindowHeight += 44f;

            //props
            ProjectilePropertiesCE prop = projectileDef.projectile as ProjectilePropertiesCE;
            if (projectileDef.IsExplosive())
            {
                listing.TextFieldNumericLine("AE_ExplotionRadius".Translate(), prop.explosionRadius, 100f, newValue =>
                {
                    settings.AddModification(projectileDef, ModifyKey.ExplosionRadius, newValue);
                });
                curWindowHeight += 24f;

                listing.ButtonLine("AE_GasType".Translate(), prop.postExplosionGasType.GetLabel(), 100f, () =>
                {
                    IEnumerable<GasType?> gasTypes = Enum.GetValues(typeof(GasType)).Cast<GasType?>();
                    gasTypes = gasTypes.Prepend(null);
                    FloatMenuUtility.MakeMenu<GasType?>(gasTypes,
                            (GasType? gasType) => gasType.GetLabel(),
                            (GasType? gasType) => delegate ()
                            {
                                settings.AddModification(projectileDef, ModifyKey.PostExplosionGasType, gasType);
                            }
                    );
                });
                curWindowHeight += 24f;

                listing.ButtonTextFieldLine("CE_DescDamage".Translate(), prop.damageDef.label, damageButtonWidth, () =>
                {
                    ShowDamageFloatMenu(projectileDef);

                }, (int)typeof(ProjectileProperties).GetField("damageAmountBase", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(prop), 100f, (newValue) =>
                {
                    settings.AddModification(projectileDef, ModifyKey.DamageAmount, newValue, ModifyType.Modify);
                });
                curWindowHeight += 24f;
            }
            // not explosive
            else if (projectileDef.projectile.damageDef != null)
            {
                listing.TextFieldNumericLine("CE_DescSharpPenetration".Translate(), prop.armorPenetrationSharp, 100f, newValue =>
                {
                    settings.AddModification(projectileDef, ModifyKey.ArmorPenetrationSharp, newValue);
                });
                curWindowHeight += 24f;

                listing.TextFieldNumericLine("CE_DescBluntPenetration".Translate(), prop.armorPenetrationBlunt, 100f, newValue =>
                {
                    settings.AddModification(projectileDef, ModifyKey.ArmorPenetrationBlunt, newValue);
                });
                curWindowHeight += 24f;

                listing.TitleWithButton("CE_DescDamage".Translate(), delegate ()
                {
                    ShowDamageFloatMenu(projectileDef, ModifyType.Add);
                }, TexButton.Add);
                curWindowHeight += 24f;

                for (int i = -1; i < prop.secondaryDamage.Count; i++)
                {
                    listing.DamageRow(projectileDef, i, damageButtonWidth, 100f,
                        () =>
                        {
                            int index = i;

                            ShowDamageFloatMenu(projectileDef, index: index);
                        },
                        damageAmount =>
                        {
                            settings.AddModification(projectileDef, ModifyKey.DamageAmount, damageAmount, ModifyType.Modify, i);
                        },
                        () =>
                        {
                            settings.AddModification(projectileDef, ModifyKey.DamageDef, null, ModifyType.Delete, i);
                        });
                    curWindowHeight += 24f;
                }
            }
            else
            {

            }

            // common
            listing.TextFieldNumericLine("AE_SuppressionFactor".Translate(), prop.suppressionFactor, 100f, newValue =>
            {
                settings.AddModification(projectileDef, ModifyKey.SuppressionFactor, newValue);
            });
            curWindowHeight += 24f;

            //comps
            if (projectileDef.HasComp<CompFragments>())
            {
                if (!fragmentList.Any())
                {
                    fragmentList.AddRange(new List<ThingDef>()
                        {
                            DefDatabase<ThingDef>.GetNamed("Fragment_Small"),
                            DefDatabase<ThingDef>.GetNamed("Fragment_Medium"),
                            DefDatabase<ThingDef>.GetNamed("Fragment_Large"),
                            DefDatabase<ThingDef>.GetNamed("Fragment_Bomblet"),
                        });
                }

                listing.TitleWithButton("AE_Fragments".Translate(), () =>
                {
                    List<ThingDef> exsistingFragments = projectileDef.GetCompProperties<CompProperties_Fragments>().fragments.Select(x => x.thingDef).ToList();

                    if (exsistingFragments.Count == fragmentList.Count)
                    {
                        return;
                    }

                    FloatMenuUtility.MakeMenu<ThingDef>(fragmentList.Except(exsistingFragments),
                        (ThingDef thingDef) => thingDef.label,
                        (ThingDef thingDef) => delegate ()
                        {
                            settings.AddModification(projectileDef, ModifyKey.Fragments, new ThingDefCountClass() { thingDef = thingDef, count = 0 }, ModifyType.Add);

                            Messages.Message(thingDef.defName, MessageTypeDefOf.SilentInput);
                        });

                }, TexButton.Add);
                curWindowHeight += 24f;

                List<ThingDefCountClass> fragments = projectileDef.GetCompProperties<CompProperties_Fragments>().fragments;

                for (int i = 0; i < fragments.Count; i++)
                {
                    listing.FragmentsRow(projectileDef, i, (int index, int count) =>
                    {
                        settings.AddModification(projectileDef, ModifyKey.Fragments, new ThingDefCountClass() { thingDef = fragments[index].thingDef, count = count }, ModifyType.Modify, index);
                    },
                    index =>
                    {
                        settings.AddModification(projectileDef, ModifyKey.Fragments, null, ModifyType.Delete, index);
                    });
                    curWindowHeight += 24f;
                }
            }

            if (projectileDef.HasComp<CompExplosiveCE>())
            {
                listing.Label("CE_DescSecondaryExplosion".Translate());
                curWindowHeight += 24f;

                CompProperties_ExplosiveCE compProperties_ExplosiveCE = projectileDef.GetCompProperties<CompProperties_ExplosiveCE>();

                //Damage
                listing.ButtonTextFieldLine("CE_DescDamage".Translate(),
                    compProperties_ExplosiveCE.explosiveDamageType.label, damageButtonWidth, () =>
                    {
                        FloatMenuUtility.MakeMenu<DamageDef>(AvaliableDamageDef(projectileDef, true),
                                 (DamageDef damageDef) => $"{damageDef.label} ({damageDef.defName})",
                                 (DamageDef damageDef) => delegate ()
                                 {
                                     SecondaryExplosionSaveable temp = new SecondaryExplosionSaveable(compProperties_ExplosiveCE)
                                     {
                                         DamageDef = damageDef
                                     };
                                     settings.AddModification(projectileDef, ModifyKey.SecondaryExplosion, temp);

                                     Messages.Message(damageDef.defName, MessageTypeDefOf.SilentInput);
                                 });

                    }, compProperties_ExplosiveCE.damageAmountBase, 100f, (newValue) =>
                    {
                        SecondaryExplosionSaveable temp = new SecondaryExplosionSaveable(compProperties_ExplosiveCE)
                        {
                            damageAmountBase = newValue
                        };
                        settings.AddModification(projectileDef, ModifyKey.SecondaryExplosion, temp);
                    }, indent: 20f);
                curWindowHeight += 24f;

                listing.TextFieldNumericLine("AE_ExplotionRadius".Translate(), compProperties_ExplosiveCE.explosiveRadius, 100f, (newValue) =>
                {
                    SecondaryExplosionSaveable temp = new SecondaryExplosionSaveable(compProperties_ExplosiveCE)
                    {
                        explosionRadius = newValue
                    };
                    settings.AddModification(projectileDef, ModifyKey.SecondaryExplosion, temp);
                }, indent: 20f);
                curWindowHeight += 24f;

                listing.ButtonLine("AE_GasType".Translate(), compProperties_ExplosiveCE.postExplosionGasType.GetLabel(), 100f, () =>
                {
                    IEnumerable<GasType?> gasTypes = Enum.GetValues(typeof(GasType)).Cast<GasType?>();
                    gasTypes = gasTypes.Prepend(null);
                    FloatMenuUtility.MakeMenu<GasType?>(gasTypes,
                            (GasType? gasType) => gasType.GetLabel(),
                            (GasType? gasType) => delegate ()
                            {

                                SecondaryExplosionSaveable temp = new SecondaryExplosionSaveable(compProperties_ExplosiveCE)
                                {
                                    postExplosionGasType = gasType
                                };
                                settings.AddModification(projectileDef, ModifyKey.SecondaryExplosion, temp);
                            }
                    );
                }, indent: 20f);
                curWindowHeight += 24f;
            }

            settings.Apply(projectileDef);
        }
    }
}
