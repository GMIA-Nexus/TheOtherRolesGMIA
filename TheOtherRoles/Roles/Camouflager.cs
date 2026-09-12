using System.Collections.Generic;
using System.Linq;
using TheOtherRoles.Modules;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    [TORRPCHolder]
    public class Camouflager : RoleBase<Camouflager> {
        public static Color color = Palette.ImpostorRed;
    
        public static float cooldown = 30f;
        public static float duration = 10f;
        public static float camouflageTimer = 0f;
        public static AchievementToken<(int kills, bool cleared)> acTokenChallenge = null;

        public override AudioClip IntroSound { get => MetaContext.VanillaAsset.ShapeshifterIntroSound; }

        static public IEnumerable<HelpSprite> GetHelpSprites()
        {
            yield return new(getButtonSprite(), "camouflagerCamoHint");
        }

        public Camouflager()
        {
            RoleId = roleId = RoleId.Camouflager;
        }

        public static RemoteProcess ActivateCamo = new("CamouflagerCamouflage", (_) =>
        {
            if (!exists) return;

            camouflageTimer = duration;
            if (Helpers.MushroomSabotageActive()) return; // Dont overwrite the fungle "camo"
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                player.setLook("", 6, "", "", "", "");
        });

        public override void PostInit()
        {
            if (PlayerControl.LocalPlayer != player) return;
            acTokenChallenge ??= new("camouflager.challenge", (0, false), (val, _) => val.cleared);
        }

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite() {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.CamoButton.png", 115f);
            return buttonSprite;
        }

        public override void OnMeetingStart()
        {
            resetCamouflage();
        }

        public override void OnDeath(PlayerControl killer = null)
        {
            if (killer != null && PlayerControl.LocalPlayer == player && camouflageTimer > 0f)
                _ = new StaticAchievementToken("camouflager.another1");
        }

        public static void resetCamouflage() {
            camouflageTimer = 0f;
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (Assassin.players.Any(x => x.player == p && x.isInvisble))
                    continue;
                p.setDefaultLook();
            }
            if (isRole(PlayerControl.LocalPlayer))
                acTokenChallenge.Value.kills = 0;
        }

        public override void ResetRole(bool isShifted)
        {
            resetCamouflage();
        }

        public static void clearAndReload() {
            camouflageTimer = 0f;
            cooldown = CustomOptionHolder.camouflagerCooldown.getFloat();
            duration = CustomOptionHolder.camouflagerDuration.getFloat();
            acTokenChallenge = null;
            players = [];
        }
    }
}
