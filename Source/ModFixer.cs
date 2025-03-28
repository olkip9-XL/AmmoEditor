
using Verse;

namespace AmmoEditor
{
    [StaticConstructorOnStartup]
    public static class ModFixer
    {
        static ModFixer()
        {
            Mod_AmmoEditor.settings.PostLoad();
        }
    }
}
