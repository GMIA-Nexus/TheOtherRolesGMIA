using System;
using System.Collections.Generic;
using TheOtherRoles.MetaContext;
using TheOtherRoles.Modules;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    [TORRPCHolder]
    public class Swapper : RoleBase<Swapper> {
        public static Color color = new Color32(134, 55, 86, byte.MaxValue);
        private static Sprite spriteCheck;
        public static bool canCallEmergency = false;
        public static bool canOnlySwapOthers { get { return CustomOptionHolder.swapperCanOnlySwapOthers.getBool(); } }
        public static int charges;
        public static float rechargeTasksNumber;
        public static float rechargedTasks;
        static public IEnumerable<HelpSprite> GetHelpSprites() => [new(getCheckSprite(), "swapperCheckHint")];
        static public IEnumerable<DocumentReplacement> GetReplacementPart() => [new("%SWAP%", ModTranslation.getString(canOnlySwapOthers ? "swapperCheckCantSwapSelfHint" : "swapperCheckSwapSelfHint"))];

        public Swapper()
        {
            RoleId = roleId = RoleId.Swapper;
        }

        public static RemoteProcess<(byte playerId1, byte playerId2)> ConfirmSwap = new("SwapperSwap", (message, _) =>
        {
            if (MeetingHud.Instance)
            {
                playerId1 = message.playerId1;
                playerId2 = message.playerId2;
            }
        });
 
        public static byte playerId1 = byte.MaxValue;
        public static byte playerId2 = byte.MaxValue;

        public static AchievementToken<(byte swapped1, byte swapped2, bool cleared)> acTokenChallenge = null;
        public static AchievementToken<(byte swapped1, byte swapped2, bool cleared)> evilSwapperAcTokenChallenge;

        public override void OnMeetingStart()
        {
            if (PlayerControl.LocalPlayer == player)
                if (!PlayerControl.LocalPlayer.Data.Role.IsImpostor)
                {
                    acTokenChallenge.Value.swapped1 = byte.MaxValue;
                    acTokenChallenge.Value.swapped2 = byte.MaxValue;
                }
                else
                {
                    evilSwapperAcTokenChallenge.Value.swapped1 = byte.MaxValue;
                    evilSwapperAcTokenChallenge.Value.swapped2 = byte.MaxValue;
                }
        }

        public override void PostInit()
        {
            if (PlayerControl.LocalPlayer != player) return;
            acTokenChallenge ??= new("niceSwapper.challenge", (byte.MaxValue, byte.MaxValue, false), (val, _) => val.cleared);
            evilSwapperAcTokenChallenge ??= new("evilSwapper.challenge", (byte.MaxValue, byte.MaxValue, false), (val, _) => val.cleared);
        }

        public override void FixedUpdate()
        {
            if (PlayerControl.LocalPlayer != player || PlayerControl.LocalPlayer.Data.IsDead) return;
            var (playerCompleted, _) = TasksHandler.taskInfo(PlayerControl.LocalPlayer.Data);
            if (playerCompleted == rechargedTasks)
            {
                rechargedTasks += rechargeTasksNumber;
                charges++;
                _ = new StaticAchievementToken("niceSwapper.common1");
            }
        }

        public override void OnMeetingEnd(PlayerControl exiled = null)
        {
            if (PlayerControl.LocalPlayer == player && exiled != null)
                if (!PlayerControl.LocalPlayer.Data.Role.IsImpostor)
                    acTokenChallenge.Value.cleared |= (acTokenChallenge.Value.swapped1 == exiled.PlayerId || acTokenChallenge.Value.swapped2 == exiled.PlayerId)
                        && ExileController.Instance.initData.networkedPlayer.Object.Data.Role.IsImpostor;
                else
                {
                    bool swapped = evilSwapperAcTokenChallenge.Value.swapped1 == exiled.PlayerId || evilSwapperAcTokenChallenge.Value.swapped2 == exiled.PlayerId;
                    evilSwapperAcTokenChallenge.Value.cleared |= swapped && !ExileController.Instance.initData.networkedPlayer.Object.Data.Role.IsImpostor && (Helpers.playerById(evilSwapperAcTokenChallenge.Value.swapped1).Data.Role.IsImpostor || Helpers.playerById(
                        evilSwapperAcTokenChallenge.Value.swapped2).Data.Role.IsImpostor);
                    if (swapped && ExileController.Instance.initData.networkedPlayer.Object.isRole(RoleId.Jester))
                        _ = new StaticAchievementToken("evilSwapper.another1");
                }
        }

        public static Sprite getCheckSprite() {
            if (spriteCheck) return spriteCheck;
            spriteCheck = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.SwapperCheck.png", 150f);
            return spriteCheck;
        }

        public static void clearAndReload() {
            playerId1 = byte.MaxValue;
            playerId2 = byte.MaxValue;
            canCallEmergency = CustomOptionHolder.swapperCanCallEmergency.getBool();
            charges = Mathf.RoundToInt(CustomOptionHolder.swapperSwapsNumber.getFloat());
            rechargeTasksNumber = Mathf.RoundToInt(CustomOptionHolder.swapperRechargeTasksNumber.getFloat());
            rechargedTasks = Mathf.RoundToInt(CustomOptionHolder.swapperRechargeTasksNumber.getFloat());
            acTokenChallenge = null;
            evilSwapperAcTokenChallenge = null;
            players = [];
        }
    }
}
