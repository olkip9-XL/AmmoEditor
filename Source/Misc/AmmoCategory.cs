using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AmmoEditor.Misc
{
    public enum AmmoCategorySign
    {
        CategoryDef,
        HandGrenade,
        Uncategorized,
    }

    public class AmmoCategory
    {
        public ThingCategoryDef thingCategoryDef { get; private set; }
        public AmmoCategorySign categorySign { get; private set; }

        public string Name
        {
            get
            {
                switch (categorySign)
                {
                    case AmmoCategorySign.HandGrenade:
                        return "HandGrenade";
                    case AmmoCategorySign.Uncategorized:
                        return "Uncategorized";
                    case AmmoCategorySign.CategoryDef:
                        return this.thingCategoryDef?.defName ?? string.Empty;
                    default:
                        return string.Empty;
                }

            }
        }

        public string Label
        {
            get
            {
                switch (categorySign)
                {
                    case AmmoCategorySign.HandGrenade:
                        return "AE_HandGrenade".Translate();
                    case AmmoCategorySign.Uncategorized:
                        return "AE_Uncategorized".Translate();
                    case AmmoCategorySign.CategoryDef:
                        return this.thingCategoryDef?.label ?? string.Empty;
                    default:
                        return string.Empty;
                }
            }
        }

        public AmmoCategory() { }

        public AmmoCategory(ThingCategoryDef thingCategoryDef)
        {
            this.thingCategoryDef = thingCategoryDef;
            this.categorySign = AmmoCategorySign.CategoryDef;
        }

        public AmmoCategory(AmmoCategorySign sign = AmmoCategorySign.Uncategorized)
        {
            this.thingCategoryDef = null;
            this.categorySign = sign;
        }

    }
}
