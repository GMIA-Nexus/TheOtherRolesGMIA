using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Reactor.Utilities;
using TheOtherRoles.Modules;
using TheOtherRoles.Objects;
using TheOtherRoles.Utilities;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    [TORRPCHolder]
    public class SecurityGuard : RoleBase<SecurityGuard> {
        public static Color color = new Color32(195, 178, 95, byte.MaxValue);

        public SecurityGuard()
        {
            RoleId = roleId = RoleId.SecurityGuard;
        }

        static public IEnumerable<HelpSprite> GetHelpSprites()
        {
            yield return new(getCloseVentButtonSprite(), "securityGuardVentHint");
            yield return new(getPlaceCameraButtonSprite(), "securityGuardCamHint");
            yield return new(getFlushSprite(), "securityGuardFlushHint");
        }

        static public IEnumerable<DocumentReplacement> GetReplacementPart()
        {
            yield return new("%VENT%", ventPrice.ToString());
            yield return new("%CAM%", camPrice.ToString());
        }

        public static float cooldown = 30f;
        public static int remainingScrews = 7;
        public static int totalScrews = 7;
        public static int ventPrice { get { return Mathf.RoundToInt(CustomOptionHolder.securityGuardVentPrice.getFloat()); } }
        public static int camPrice { get { return Mathf.RoundToInt(CustomOptionHolder.securityGuardCamPrice.getFloat()); } }
        public static int placedCameras = 0;
        public static float duration = 10f;
        public static int maxCharges = 5;
        public static int rechargeTasksNumber = 3;
        public static int rechargedTasks = 3;
        public static int charges = 1;
        public static float flushCooldown = 30f;
        public static bool cantMove = true;
        public static Vent ventTarget = null;
        public static Minigame minigame = null;

        private static Sprite flushButtonSprite;
        public static Sprite getFlushSprite()
        {
            if (flushButtonSprite) return flushButtonSprite;
            flushButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.FlushButton.png", 115f);
            return flushButtonSprite;
        }

        public static AchievementToken<(bool vent, bool camera)> acTokenCommon = null;
        public static AchievementToken<int> acTokenChallenge = null;

        public static RemoteProcess RpcFlush = new("SecurityGuardFlush", (isByMe) =>
        {
            if (PlayerControl.LocalPlayer.inVent)
            {
                PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(Vent.currentVent.Id);
                PlayerControl.LocalPlayer.MyPhysics.ExitAllVents();

                Helpers.showFlash(color, message: ModTranslation.getString("securityGuardFlushNotification"), textColor: Color.white, textSprite: RoleHelpers.GetRoleIcon(RoleId.SecurityGuard).GetSprite());
            }

            if (isByMe) Coroutines.Start(SeeVenter());
        });

        static IEnumerator SeeVenter()
        {
            var playersInVent = PlayerControl.AllPlayerControls.ToArray().Where(x => x.inVent);
            if (!playersInVent.Any()) yield break;
            var arrows = new List<Arrow>();
            foreach (var p in playersInVent)
            {
                var arrow = new Arrow(color);
                arrow.Update(p.transform.position);
                arrows.Add(arrow);
            }
            yield return new WaitForSeconds(1f);

            foreach (Arrow arrow in arrows)
                if (arrow?.arrow != null)
                    UnityEngine.Object.Destroy(arrow.arrow);

            arrows = [];
        }

        public override void PostInit()
        {
            if (PlayerControl.LocalPlayer != player) return;
            acTokenCommon ??= new("securityGuard.common1", (false, false), (val, _) => val.vent && val.camera);
            acTokenChallenge ??= new("securityGuard.challenge", 0, (val, _) => val >= 5);
        }

        public override void FixedUpdate()
        {
            updateCharges();
            setVentTarget();
        }

        private void updateCharges()
        {
            if (PlayerControl.LocalPlayer != player || player.Data.IsDead) return;
            var (playerCompleted, _) = TasksHandler.taskInfo(player.Data);
            if (playerCompleted == rechargedTasks)
            {
                rechargedTasks += rechargeTasksNumber;
                if (maxCharges > charges) charges++;
            }
        }

        private void setVentTarget()
        {
            if (player != PlayerControl.LocalPlayer || MapUtilities.CachedShipStatus == null || MapUtilities.CachedShipStatus.AllVents == null) return;

            Vent target = null;
            Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
            float closestDistance = float.MaxValue;
            for (int i = 0; i < MapUtilities.CachedShipStatus.AllVents.Length; i++)
            {
                Vent vent = MapUtilities.CachedShipStatus.AllVents[i];
                if (vent.gameObject.name.StartsWith("JackInTheBoxVent_") || vent.gameObject.name.StartsWith("SealedVent_") || vent.gameObject.name.StartsWith("FutureSealedVent_")) continue;
                if (SubmergedCompatibility.IsSubmerged && vent.Id == 9) continue; // cannot seal submergeds exit only vent!
                float distance = Vector2.Distance(vent.transform.position, truePosition);
                if (distance <= vent.UsableDistance && distance < closestDistance)
                {
                    closestDistance = distance;
                    target = vent;
                }
            }
            ventTarget = target;
        }

        private static Sprite closeVentButtonSprite;
        public static Sprite getCloseVentButtonSprite() {
            if (closeVentButtonSprite) return closeVentButtonSprite;
            closeVentButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.CloseVentButton.png", 115f);
            return closeVentButtonSprite;
        }

        private static Sprite placeCameraButtonSprite;
        public static Sprite getPlaceCameraButtonSprite() {
            if (placeCameraButtonSprite) return placeCameraButtonSprite;
            placeCameraButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.PlaceCameraButton.png", 115f);
            return placeCameraButtonSprite;
        }

        private static Sprite animatedVentSealedSprite;
        private static float lastPPU;
        public static Sprite getAnimatedVentSealedSprite() {
            float ppu = 185f;
            if (SubmergedCompatibility.IsSubmerged) ppu = 120f;
            if (lastPPU != ppu) {
                animatedVentSealedSprite = null;
                lastPPU = ppu;
            }
            if (animatedVentSealedSprite) return animatedVentSealedSprite;
            animatedVentSealedSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.AnimatedVentSealed.png", ppu);
            return animatedVentSealedSprite;
        }

        private static Sprite staticVentSealedSprite;
        public static Sprite getStaticVentSealedSprite() {
            if (staticVentSealedSprite) return staticVentSealedSprite;
            staticVentSealedSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.StaticVentSealed.png", 160f);
            return staticVentSealedSprite;
        }

        private static Sprite fungleVentSealedSprite;
        public static Sprite getFungleVentSealedSprite()
        {
            if (fungleVentSealedSprite) return fungleVentSealedSprite;
            fungleVentSealedSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.FungleVentSealed.png", 160f);
            return fungleVentSealedSprite;
        }

        private static Sprite submergedCentralUpperVentSealedSprite;
        public static Sprite getSubmergedCentralUpperSealedSprite() {
            if (submergedCentralUpperVentSealedSprite) return submergedCentralUpperVentSealedSprite;
            submergedCentralUpperVentSealedSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.CentralUpperBlocked.png", 145f);
            return submergedCentralUpperVentSealedSprite;
        }

        private static Sprite submergedCentralLowerVentSealedSprite;
        public static Sprite getSubmergedCentralLowerSealedSprite() {
            if (submergedCentralLowerVentSealedSprite) return submergedCentralLowerVentSealedSprite;
            submergedCentralLowerVentSealedSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.CentralLowerBlocked.png", 145f);
            return submergedCentralLowerVentSealedSprite;
        }

        private static Sprite camSprite;
        public static Sprite getCamSprite() {
            if (camSprite) return camSprite;
            camSprite = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.CamsButton].Image;
            return camSprite;
        }

        private static Sprite logSprite;
        public static Sprite getLogSprite() {
            if (logSprite) return logSprite;
            logSprite = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.DoorLogsButton].Image;
            return logSprite;
        }

        public static void clearAndReload() {
            ventTarget = null;
            minigame = null;
            duration = CustomOptionHolder.securityGuardCamDuration.getFloat();
            maxCharges = Mathf.RoundToInt(CustomOptionHolder.securityGuardCamMaxCharges.getFloat());
            rechargeTasksNumber = Mathf.RoundToInt(CustomOptionHolder.securityGuardCamRechargeTasksNumber.getFloat());
            rechargedTasks = Mathf.RoundToInt(CustomOptionHolder.securityGuardCamRechargeTasksNumber.getFloat());
            charges = Mathf.RoundToInt(CustomOptionHolder.securityGuardCamMaxCharges.getFloat()) /2;
            placedCameras = 0;
            cooldown = CustomOptionHolder.securityGuardCooldown.getFloat();
            totalScrews = remainingScrews = Mathf.RoundToInt(CustomOptionHolder.securityGuardTotalScrews.getFloat());
            cantMove = CustomOptionHolder.securityGuardNoMove.getBool();
            flushCooldown = CustomOptionHolder.securityGuardFlushCooldown.getFloat();
            acTokenChallenge = null;
            acTokenCommon = null;
            players = [];
        }
    }
}
