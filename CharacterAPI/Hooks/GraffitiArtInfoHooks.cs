using Reptile;
using System;

namespace CharacterAPI.Hooks
{
    public class GraffitiArtInfoHooks
    {
        public static void InitHooks()
        {
            On.Reptile.GraffitiArtInfo.FindByCharacter += GraffitiArtInfo_FindByCharacter;
        }

        private static Reptile.GraffitiArt GraffitiArtInfo_FindByCharacter(On.Reptile.GraffitiArtInfo.orig_FindByCharacter orig, Reptile.GraffitiArtInfo self, Reptile.Characters character)
        {
            if (Enum.IsDefined(typeof(Characters), character))
            {
                return orig(self, character);
            }

            var moddedCharacter = CharacterAPI.GetModdedCharacter(character);
            if (moddedCharacter.usePersonalGrafitti)
            {
                GraffitiArt art = moddedCharacter.personalGrafitti;
                // should probably move this to initialization, but I am not sure how to load game's objects without Adressables
                CharacterAPI.AttemtToFixShaderGraffiti(GetGraffitiLoader(), art.graffitiMaterial);

                return art;
            }
            else
            {
                return orig(self, Characters.metalHead);
            }
        }

        private static GraffitiLoader? GetGraffitiLoader()
        {
            foreach (var assetLoader in Reptile.Core.Instance.assets.assetLoaders)
            {
                if (assetLoader is Reptile.GraffitiLoader)
                {
                    return assetLoader as Reptile.GraffitiLoader;
                }
            }

            return null;
        }
    }
}
