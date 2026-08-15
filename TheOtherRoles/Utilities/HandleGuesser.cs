using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheOtherRoles.CustomGameModes;
using TheOtherRoles.Roles;
using UnityEngine;

namespace TheOtherRoles.Utilities {
    public static class HandleGuesser {
        private static Sprite targetSprite;
        public static bool isGuesserGm = false;
        public static bool hasMultipleShotsPerMeeting = false;
        public static bool killsThroughShield = true;
        public static bool evilGuesserCanGuessSpy = true;
        public static bool guesserCantGuessSnitch = false;
        public static bool guesserCantGuessFortuneTeller = true;
        public static int tasksToUnlock = Mathf.RoundToInt(CustomOptionHolder.guesserGamemodeCrewGuesserNumberOfTasks.getFloat());
        public static List<RoleInfo> guessableRoleFilter => [.. RoleInfo.allRoleInfos.Where(x => !x.isModifier)];
        public static List<RoleInfo> guesserGMRoleFilter => [.. RoleInfo.allRoleInfos.Where(x => !x.isModifier && x.roleId is not RoleId.NiceGuesser and not RoleId.EvilGuesser)];
        public static List<RoleInfo> guessableRoles = [];

        public static Sprite getTargetSprite() {
            if (targetSprite) return targetSprite;
            targetSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.TargetIcon.png", 150f);
            return targetSprite;
        }

        public static bool isGuesser(byte playerId) {
            if (isGuesserGm) return GuesserGM.isGuesser(playerId);
            return Guesser.isGuesser(playerId);
        }

        public static int remainingShots(byte playerId, bool shoot = false) {
            if (isGuesserGm) return GuesserGM.remainingShots(playerId, shoot);
            return Guesser.remainingShots(playerId, shoot);
        }

        public static void clearAndReload() {
            Guesser.clearAndReload();
            GuesserGM.clearAndReload();
            isGuesserGm = TORMapOptions.gameMode == CustomGamemodes.Guesser;
            if (isGuesserGm) {
                guesserCantGuessSnitch = CustomOptionHolder.guesserGamemodeCantGuessSnitchIfTaksDone.getBool();
                guesserCantGuessFortuneTeller = CustomOptionHolder.guesserGamemodeCantGuessFortuneTeller.getBool();
                hasMultipleShotsPerMeeting = CustomOptionHolder.guesserGamemodeHasMultipleShotsPerMeeting.getBool();
                killsThroughShield = CustomOptionHolder.guesserGamemodeKillsThroughShield.getBool();
                evilGuesserCanGuessSpy = CustomOptionHolder.guesserGamemodeEvilCanKillSpy.getBool();
                tasksToUnlock = Mathf.RoundToInt(CustomOptionHolder.guesserGamemodeCrewGuesserNumberOfTasks.getFloat());
                guessableRoles = CustomOptionHolder.guesserGamemodeGuessableRoles.filterSelection;
            } else {
                guesserCantGuessSnitch = CustomOptionHolder.guesserCantGuessSnitchIfTaksDone.getBool();
                guesserCantGuessFortuneTeller = CustomOptionHolder.guesserCantGuessFortuneTeller.getBool();
                hasMultipleShotsPerMeeting = CustomOptionHolder.guesserHasMultipleShotsPerMeeting.getBool();
                killsThroughShield = CustomOptionHolder.guesserKillsThroughShield.getBool();
                evilGuesserCanGuessSpy = CustomOptionHolder.guesserEvilCanKillSpy.getBool();
                guessableRoles = CustomOptionHolder.guesserGuessableRoles.filterSelection;
            }

        }
    }
}
