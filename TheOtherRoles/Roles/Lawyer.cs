using Hazel;
using TheOtherRoles.CustomGameModes;
using TheOtherRoles.Modules;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    [TORRPCHolder]
    public class Lawyer : RoleBase<Lawyer> {
        public static PlayerControl target;
        public static Color color = new Color32(134, 153, 25, byte.MaxValue);
        public static Sprite targetSprite;
        public static bool targetKnows = true;
        public static bool triggerLawyerWin = false;
        public static int meetings = 0;

        public Lawyer()
        {
            RoleId = roleId = RoleId.Lawyer;
        }

        public static RemoteProcess<byte> SetTarget = RemotePrimitiveProcess.OfByte("LawyerSetTarget", (message, _) => target = Helpers.playerById(message));

        public static RemoteProcess TriggerWin = new("LawyerWin", (_) =>  triggerLawyerWin = true);

        public static RemoteProcess<byte> PromoteToPursuer = RemotePrimitiveProcess.OfByte("LawyerPromotesToPursuer", (message, _) =>
        {
            PlayerControl player = Helpers.playerById(message);
            eraseRole(player);
            player.setRole(RoleId.Pursuer);
        });

        public override void FixedUpdate()
        {
            if (player != PlayerControl.LocalPlayer) return;

            // Meeting win
            if (winsAfterMeetings && neededMeetings == meetings && target != null && !target.Data.IsDead)
            {
                winsAfterMeetings = false; // Avoid sending mutliple RPCs until the host finshes the game
                TriggerWin.Invoke();
                return;
            }

            // Promote to Pursuer
            if (target != null && target.Data.Disconnected && !player.Data.IsDead)
            {
                PromoteToPursuer.Invoke(player.PlayerId);
                return;
            }
        }

        public override void OnMeetingEnd(PlayerControl exiled = null)
        {
            if (PlayerControl.LocalPlayer == player && !player.Data.IsDead)
                meetings++;
        }

        public override void OnDeath(PlayerControl killer = null)
        {
            if (!FreePlayGM.isFreePlayGM) player.clearAllTasks();
            if (killer != null && PlayerControl.LocalPlayer == player && killer == target)
                _ = new StaticAchievementToken("lawyer.another1");
        }

        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason)
        {
            if (this.player == player) target = null;
        }

        public static bool winsAfterMeetings = false;
        public static int neededMeetings = 4;
        public static float vision = 1f;
        public static bool lawyerTargetKnows = true;
        public static bool lawyerKnowsRole = false;
        public static bool targetCanBeJester = false;
        public static bool targetWasGuessed = false;

        public static void clearTarget()
        {
            target = null;
            targetWasGuessed = false;
        }

        public static void clearAndReload() {
            clearTarget();
            triggerLawyerWin = false;
            meetings = 0;
            vision = CustomOptionHolder.lawyerVision.getFloat();
            lawyerKnowsRole = CustomOptionHolder.lawyerKnowsRole.getBool();
            lawyerTargetKnows = CustomOptionHolder.lawyerTargetKnows.getBool();
            targetCanBeJester = CustomOptionHolder.lawyerTargetCanBeJester.getBool();
            winsAfterMeetings = CustomOptionHolder.lawyerWinsAfterMeetings.getBool();
            neededMeetings = Mathf.RoundToInt(CustomOptionHolder.lawyerNeededMeetings.getFloat());
            targetKnows = CustomOptionHolder.lawyerTargetKnows.getBool();
            players = [];
        }
    }
}
