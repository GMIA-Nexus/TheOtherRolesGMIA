using System.Collections.Generic;
using TheOtherRoles.Modules;
using TheOtherRoles.Utilities;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    [TORRPCHolder]
    public class TimeMaster : RoleBase<TimeMaster> {
        public static Color color = new Color32(112, 142, 239, byte.MaxValue);

        public static float rewindTime { get { return CustomOptionHolder.timeMasterRewindTime.getFloat(); } }
        public static float shieldDuration { get { return CustomOptionHolder.timeMasterShieldDuration.getFloat(); } }
        public static float cooldown = 30f;

        public bool shieldActive = false;
        public static bool isRewinding = false;
        public static bool reviveDuringReweind = false;
        public static bool rewindIndependently { get { return CustomOptionHolder.timeMasterCanRewindIndependently.getBool(); } }
        public int numRewinds = 0;

        private static Sprite rewindSprite;
        public static Sprite getRewindSprite()
        {
            if (rewindSprite) return rewindSprite;
            rewindSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.RewindButton.png", 115f);
            return rewindSprite;
        }

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite() {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.TimeShieldButton.png", 115f);
            return buttonSprite;
        }

        static public IEnumerable<DocumentReplacement> GetReplacementPart()
        {
            yield return new("%SEC%", shieldDuration.ToString());
            yield return new("%TIME%", rewindTime.ToString());
            yield return new("%OPT%", rewindIndependently ? ModTranslation.getString("timeMasterOPTHint") : "");
        }

        public TimeMaster()
        {
            RoleId = roleId = RoleId.TimeMaster;
            shieldActive = false;
            numRewinds = Mathf.RoundToInt(CustomOptionHolder.timeMasterNumberOfRewind.getFloat());
        }

        public static RemoteProcess<byte> UseShield = RemotePrimitiveProcess.OfByte("TimeMasterShield", (message, _) =>
        {
            var timeMaster = getRole(Helpers.playerById(message));
            timeMaster.shieldActive = true;
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(shieldDuration, new System.Action<float>((p) => {
                if (p == 1f) timeMaster.shieldActive = false;
            })));
        });

        public static void clearAndReload() {
            isRewinding = false;
            cooldown = CustomOptionHolder.timeMasterCooldown.getFloat();
            reviveDuringReweind = CustomOptionHolder.timeMasterReviveDuringRewind.getBool();
            players = [];
        }
    }
}
