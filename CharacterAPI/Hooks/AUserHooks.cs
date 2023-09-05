using CharacterAPI.Saving;
using Reptile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SocialPlatforms.Impl;

namespace CharacterAPI.Hooks
{
    public class AUserHooks
    {
        public static void InitHooks()
        {
            On.Reptile.AUser.CreateUserSaveDataLoaders += AUser_CreateUserSaveDataLoaders;
        }

        // TODO: rewrite, its a shitshow
        private static void AUser_CreateUserSaveDataLoaders(On.Reptile.AUser.orig_CreateUserSaveDataLoaders orig, Reptile.AUser self)
        {
            int num = self.DetermineLoaderCount() + 1;
            self.userSaveDataLoaders = new IUserSaveDataLoader[num];
            self.userSaveDataLoaders[0] = new UserSettingsSaveDataLoader(self.saveManager, self.storage, self.errorHandler, self.uIManager, self, self.localizer);
            self.userSaveDataLoaders[1] = new UserSaveSlotSaveDataLoader(self.saveManager, self.storage, self.errorHandler, self.uIManager, self.localizer);
            self.userSaveDataLoaders[2] = new UserSaveSlotSettingsDataLoader(self.saveManager, self.storage, self.errorHandler, self.uIManager, self.localizer);
            self.userSaveDataLoaders[3] = new UserPlayerStatsLoader(self.saveManager, self.storage, self.errorHandler, self.uIManager, self.localizer);
            self.userSaveDataLoaders[4] = new ModdedCharactersProgressDataLoader(self.saveManager, self.storage, self.errorHandler, self.uIManager, self.localizer, CoreHooks.moddedSaveManager);
            if (Core.Instance.Platform.HasAchievementsSupport)
            {
                self.userSaveDataLoaders[5] = new UserAchievementsCacheLoader(self.saveManager, self.storage, self.errorHandler, self.uIManager, self.localizer);
            }
        }
    }
}
