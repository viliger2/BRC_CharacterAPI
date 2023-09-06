using Reptile;
using System;

namespace CharacterAPI.Hooks
{
    public class GraffitiArtInfoHooks
    {
        public static void InitHooks()
        {
            On.Reptile.GraffitiArtInfo.FindByCharacter += GraffitiArtInfo_FindByCharacter;
            On.Reptile.GraffitiArtInfo.FindByTitle += GraffitiArtInfo_FindByTitle;
        }

        private static GraffitiArt GraffitiArtInfo_FindByTitle(On.Reptile.GraffitiArtInfo.orig_FindByTitle orig, GraffitiArtInfo self, string grafTitle)
        {
            GraffitiArt origResult = orig(self, grafTitle);

            if (origResult == null && !string.IsNullOrEmpty(grafTitle))
            {
                var moddedCharacter = CharacterAPI.ModdedCharacters.Find(x => x.personalGrafitti != null && x.personalGrafitti.title.Equals(grafTitle));
                if (moddedCharacter != null)
                {
                    if (moddedCharacter.personalGrafitti != null)
                    {
                        CharacterAPI.AttemtToFixShaderGraffiti(GetGraffitiLoader(), moddedCharacter.personalGrafitti.graffitiMaterial);
                        return moddedCharacter.personalGrafitti;
                    }
                    else
                    {
                        return orig(self, Reptile.GraffitiArt.Titles.playersGraf_S0);
                    }
                }
                else // using this structure mainly because somtimes game tries to load graffiti without a name
                {
                    CharacterAPI.logger.LogWarning($"Modded character with graffiti {grafTitle} doesn't exist, replacing it with graffiti {Reptile.GraffitiArt.Titles.playersGraf_S0}.");
                    return orig(self, Reptile.GraffitiArt.Titles.playersGraf_S0);
                }
            }
            else
            {
                return origResult;
            }
        }

        private static Reptile.GraffitiArt GraffitiArtInfo_FindByCharacter(On.Reptile.GraffitiArtInfo.orig_FindByCharacter orig, Reptile.GraffitiArtInfo self, Reptile.Characters character)
        {
            if (Enum.IsDefined(typeof(Characters), character))
            {
                return orig(self, character);
            }

            var moddedCharacter = CharacterAPI.GetModdedCharacter(character);
            if (moddedCharacter != null)
            {
                if (moddedCharacter.usePersonalGrafitti)
                {
                    GraffitiArt art = moddedCharacter.personalGrafitti;
                    // should probably move this to initialization, but I am not sure how to load game's objects without Adressables
                    CharacterAPI.AttemtToFixShaderGraffiti(GetGraffitiLoader(), art.graffitiMaterial);

                    return art;
                }
                return orig(self, moddedCharacter.characterGraffitiBase);
            }

            CharacterAPI.logger.LogWarning($"GraffitiArtInfo::FindByCharacter couldn't find graffiti for character {character} because character does not exist, yet character is modded. Replacing graffiti with {Characters.metalHead}.");
            return orig(self, Characters.metalHead);
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
