using System.Collections.Generic;
using UnityEngine;

namespace TheOtherRoles{
    static class TORMapOptions {
        // Set values
        public static int maxNumberOfMeetings = 10;
        public static bool blockSkippingInEmergencyMeetings = false;
        public static bool noVoteIsSelfVote = false;
        public static bool hidePlayerNames = false;
        public static bool allowParallelMedBayScans = false;
        public static bool enableHorseMode = false;
        public static bool shieldFirstKill = false;
        public static bool cleanedBodyShowAsMissing = true;
        public static CustomGamemodes gameMode = CustomGamemodes.Classic;

        // Updating values
        public static int meetingsCount = 0;
        public static List<SurvCamera> camerasToAdd = new();
        public static List<Vent> ventsToSeal = new();
        public static Dictionary<byte, PoolablePlayer> playerIcons = new();
        public static string firstKillName;
        public static PlayerControl firstKillPlayer;

        public static void clearAndReloadMapOptions() {
            meetingsCount = 0;
            camerasToAdd = new List<SurvCamera>();
            ventsToSeal = new List<Vent>();
            playerIcons = new Dictionary<byte, PoolablePlayer>();

            maxNumberOfMeetings = Mathf.RoundToInt(CustomOptionHolder.maxNumberOfMeetings.getSelection());
            blockSkippingInEmergencyMeetings = CustomOptionHolder.blockSkippingInEmergencyMeetings.getBool();
            noVoteIsSelfVote = CustomOptionHolder.noVoteIsSelfVote.getBool();
            hidePlayerNames = CustomOptionHolder.hidePlayerNames.getBool();
            allowParallelMedBayScans = CustomOptionHolder.allowParallelMedBayScans.getBool();
            shieldFirstKill = CustomOptionHolder.shieldFirstKill.getBool();
            cleanedBodyShowAsMissing = CustomOptionHolder.cleanedBodyShowAsMissing.getBool();
            firstKillPlayer = null;
        }

        public static void resetPoolables() {
            foreach (PoolablePlayer p in playerIcons.Values) {
                if (p != null && p.gameObject != null) p.gameObject.SetActive(false);
            }
        }
    }
}
