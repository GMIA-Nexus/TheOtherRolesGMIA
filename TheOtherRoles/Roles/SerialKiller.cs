using System.Collections.Generic;
using TheOtherRoles.Modules;
using TheOtherRoles.Patches;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    public class SerialKiller : RoleBase<SerialKiller>
    {
        public SerialKiller()
        {
            RoleId = roleId = RoleId.SerialKiller;
            isCountDown = false;
        }

        static public IEnumerable<HelpSprite> GetHelpSprites() => [new(getButtonSprite(), "serialKillerSuicideHint")];
        static public IEnumerable<DocumentReplacement> GetReplacementPart() {
            yield return new("%RES%", resetTimer ? ModTranslation.getString("serialKillerSuicideMeetingHint") : "");
            yield return new("%UST%", CustomOptionHolder.serialKillerSuicideTimer.getFloat().ToString());
        }

        public static PlayerControl serialKiller;
        public static Color color = Palette.ImpostorRed;

        public static float killCooldown = 15f;
        public static float suicideTimer = 40f;
        public static bool resetTimer { get { return CustomOptionHolder.serialKillerResetTimer.getBool(); } }

        public bool isCountDown = false;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.SuicideButton.png", 115f);
            return buttonSprite;
        }

        public override void OnMeetingEnd(PlayerControl exiled = null)
        {
            if (PlayerControl.LocalPlayer == player)
                PlayerControl.LocalPlayer.SetKillTimerUnchecked(killCooldown);
        }

        public override void OnKill(PlayerControl target)
        {
            if (PlayerControl.LocalPlayer == player && target != player)
            {
                _ = new StaticAchievementToken("serialKiller.common1");
                player.SetKillTimerUnchecked(killCooldown);
                HudManagerStartPatch.serialKillerButton.Timer = HudManagerStartPatch.serialKillerButton.EffectDuration;
                isCountDown = true;
            }
        }

        public static void OnRevive(PlayerControl playerControl)
        {
            if (!isRole(playerControl)) return;
            var sk = getRole(playerControl);
            if (sk.isCountDown && PlayerControl.LocalPlayer == playerControl)
                HudManagerStartPatch.serialKillerButton.Timer = HudManagerStartPatch.serialKillerButton.EffectDuration;
        }

        public static void clearAndReload()
        {
            killCooldown = CustomOptionHolder.serialKillerKillCooldown.getFloat();
            suicideTimer = Mathf.Max(CustomOptionHolder.serialKillerSuicideTimer.getFloat(), killCooldown + 2.5f);
            players = [];
        }
    }
}
