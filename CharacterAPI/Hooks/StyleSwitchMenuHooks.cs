using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharacterAPI.Hooks
{
    public class StyleSwitchMenuHooks
    {
        public static void InitHooks()
        {
            On.Reptile.StyleSwitchMenu.SkinButtonClicked += StyleSwitchMenu_SkinButtonClicked;
        }

        private static void StyleSwitchMenu_SkinButtonClicked(On.Reptile.StyleSwitchMenu.orig_SkinButtonClicked orig, Reptile.StyleSwitchMenu self, Reptile.MenuTimelineButton clickedButton, int skinIndex)
        {
            orig(self, clickedButton, skinIndex);

            // SAVE_MODDED_CHARACTERS
            ModdedCharacterProgress.SaveAsync();
        }
    }
}
