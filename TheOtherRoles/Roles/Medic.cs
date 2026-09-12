using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using TheOtherRoles.Modules;
using UnityEngine;
using static TheOtherRoles.Patches.PlayerControlFixedUpdatePatch;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    [TORRPCHolder]
    public class Medic : RoleBase<Medic> {
        public PlayerControl shielded;
        public PlayerControl futureShielded;

        public static Color color = new Color32(126, 251, 194, byte.MaxValue);
        public bool usedShield;

        public static int showShielded = 0;
        public static bool showAttemptToShielded = false;
        public static bool showAttemptToMedic = false;
        public static bool setShieldAfterMeeting = false;
        public static bool showShieldAfterMeeting = false;
        public static bool canUseVitals = true;
        public static bool seesDeathReasonOnVitals = true;
        public bool meetingAfterShielding = false;

        public Medic()
        {
            RoleId = roleId = RoleId.Medic;
            shielded = null;
            futureShielded = null;
            currentTarget = null;
            usedShield = false;
            meetingAfterShielding = false;
            acTokenChallenge = null;
        }

        public override AudioClip IntroSound { get => MetaContext.VanillaAsset.ScientistIntroSound; }

        public static RemoteProcess<(byte shieldedId, byte medicId)> Shield = new("MedicSetShielded", (message, _) =>
        {
            var medic = getRole(Helpers.playerById(message.medicId));
            medic.usedShield = true;
            medic.shielded = Helpers.playerById(message.shieldedId);
            medic.futureShielded = null;
        });

        public static RemoteProcess<(byte playerId, byte medicId)> FutureShield = new("MedicSetFutureShielded", (message, _) =>
        {
            var medic = getRole(Helpers.playerById(message.medicId));
            if (medic == null) return;
            medic.futureShielded = Helpers.playerById(message.playerId);
            medic.usedShield = true;
        });

        public static RemoteProcess<(byte killerId, byte medicId)> GainAchievement = new("UnlockMedicAch", (message, _) =>
        {
            if (PlayerControl.LocalPlayer.PlayerId == message.medicId)
                local.acTokenChallenge.Value.killerId = message.killerId;
        });

        public static bool IsShielded(PlayerControl player) => players.Any(x => x.player != null && !x.player.Data.Disconnected && !x.player.Data.IsDead && x.shielded == player && player?.Data.IsDead == false);
        public static List<Medic> GetMedic(PlayerControl shielded) => players.Where(x => x.shielded == shielded || x.futureShielded == shielded).ToList();

        public override void OnMeetingEnd(PlayerControl exiled = null)
        {
            if (PlayerControl.LocalPlayer == player)
            {
                if (exiled != null)
                    acTokenChallenge.Value.cleared |= acTokenChallenge.Value.killerId == exiled.PlayerId;
                acTokenChallenge.Value.killerId = byte.MaxValue;
            }
        }

        public override void FixedUpdate()
        {
            if (player != PlayerControl.LocalPlayer) return;
            currentTarget = setTarget();
            if (!usedShield) setPlayerOutline(currentTarget, shieldedColor);
        }

        public static Color shieldedColor = new Color32(0, 221, 255, byte.MaxValue);
        public PlayerControl currentTarget;
        public AchievementToken<(byte killerId, bool cleared)> acTokenChallenge = null;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite() {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.ShieldButton.png", 115f);
            return buttonSprite;
        }

        internal static VitalsMinigame OpenSpecialVitalsMinigame()
        {
            VitalsMinigame vitalsMinigame = null;
            foreach (RoleBehaviour role in RoleManager.Instance.AllRoles)
            {
                if (role.Role == RoleTypes.Scientist)
                {
                    vitalsMinigame = UnityEngine.Object.Instantiate(role.gameObject.GetComponent<ScientistRole>().VitalsPrefab, Camera.main.transform, false);
                    break;
                }
            }
            if (vitalsMinigame == null) return null!;
            vitalsMinigame.transform.SetParent(Camera.main.transform, false);
            vitalsMinigame.transform.localPosition = new Vector3(0.0f, 0.0f, -50f);
            vitalsMinigame.Begin(null);

            return vitalsMinigame;
        }

        public static string GetDeathReason(PlayerControl player)
        {
            if (player == null) return null;
            var dp = GameHistory.deadPlayers?.FirstOrDefault(x => x.player?.PlayerId == player?.PlayerId);
            if (dp == null) return null;

            return dp.deathReason.ToString();
        }

        public override void PostInit()
        {
            if (PlayerControl.LocalPlayer != player) return;
            acTokenChallenge ??= new("medic.challenge", (byte.MaxValue, false), (val, _) => val.cleared);
        }

        public static bool shieldVisible(PlayerControl target)
        {
            bool hasVisibleShield = false;

            bool isMorphedMorphling = Morphling.players.Any(x => x.player == target && x.morphTarget != null && x.morphTimer > 0f);
            bool isMimicKShield = target.isRole(RoleId.MimicK) && MimicK.victim != null;
            bool isMimicAMorph = target.isRole(RoleId.MimicA) && MimicA.isMorph;
            if (IsShielded(target) && !isMorphedMorphling && !isMimicKShield && !isMimicAMorph || isMorphedMorphling && IsShielded(Morphling.getRole(target).morphTarget) || isMimicAMorph && MimicK.allPlayers.Any(x => IsShielded(x)))
                foreach (var medic in GetMedic(target)) {
                    if (medic == null || medic.player == null) return false;
                    hasVisibleShield = showShielded == 0 || Helpers.shouldShowGhostInfo() // Everyone or Ghost info
                        || showShielded == 1 && (IsShielded(PlayerControl.LocalPlayer) || PlayerControl.LocalPlayer == medic.player) // Shielded + Medic
                        || showShielded == 2 && PlayerControl.LocalPlayer == medic.player; // Medic only
                                                                                             // Make shield invisible till after the next meeting if the option is set (the medic can already see the shield)
                    hasVisibleShield = hasVisibleShield && (medic.meetingAfterShielding || !showShieldAfterMeeting || PlayerControl.LocalPlayer == medic.player || Helpers.shouldShowGhostInfo());
                }
            return hasVisibleShield;
        }

        public static void clearAndReload() {
            showShielded = CustomOptionHolder.medicShowShielded.getSelection();
            showAttemptToShielded = CustomOptionHolder.medicShowAttemptToShielded.getBool();
            showAttemptToMedic = CustomOptionHolder.medicShowAttemptToMedic.getBool();
            setShieldAfterMeeting = CustomOptionHolder.medicSetOrShowShieldAfterMeeting.getSelection() == 2;
            showShieldAfterMeeting = CustomOptionHolder.medicSetOrShowShieldAfterMeeting.getSelection() == 1;
            canUseVitals = CustomOptionHolder.medicCanUseVitals.getBool();
            seesDeathReasonOnVitals = CustomOptionHolder.medicSeesDeathReasonOnVitals.getBool();
            players = [];
        }
    }
}
