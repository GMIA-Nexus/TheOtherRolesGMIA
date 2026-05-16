using System;
using System.Collections.Generic;
using System.Linq;
using Hazel;
using TheOtherRoles.MetaContext;
using TheOtherRoles.Modules;
using UnityEngine;
using static TheOtherRoles.Patches.PlayerControlFixedUpdatePatch;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    [TORRPCHolder]
    public class Deputy : RoleBase<Deputy>
    {
        public static Color color = Sheriff.color;
        public Sheriff sheriff;

        public PlayerControl currentTarget;
        public static List<byte> handcuffedPlayers = [];
        public static int promotesToSheriff; // No: 0, Immediately: 1, After Meeting: 2
        public static bool keepsHandcuffsOnPromotion;
        public static float handcuffDuration;
        public float remainingHandcuffs;
        public static float handcuffCooldown;
        public static bool knowsSheriff;
        public static bool stopsGameEnd;
        public static Dictionary<byte, float> handcuffedKnows = [];

        public Deputy()
        {
            RoleId = roleId = RoleId.Deputy;
            sheriff = Sheriff.allPlayers == null || Sheriff.allPlayers.Count == 0 ? null :
                Sheriff.players.FirstOrDefault(x => !x.isFormerDeputy && !players.Any(deputy => deputy.sheriff != null && deputy.sheriff?.player == x.player));
            currentTarget = null;
            remainingHandcuffs = CustomOptionHolder.deputyNumberOfHandcuffs.getFloat();
        }

        public static RemoteProcess<byte> Handcuff = RemotePrimitiveProcess.OfByte("DeputyHandcuff", (message, _) => handcuffedPlayers.Add(message));

        private static Sprite buttonSprite;
        private static Sprite handcuffedSprite;

        public override GUIContext ProgressWidget { get => ProgressGUI.Holder(sheriff != null && sheriff.player ? ProgressGUI.OneLineText(ModTranslation.getString("sheriff") + ": " + sheriff.player.Data.PlayerName) : null); }

        public override void FixedUpdate()
        {
            if (PlayerControl.LocalPlayer == player)
            {
                currentTarget = setTarget();
                if (remainingHandcuffs > 0) setPlayerOutline(currentTarget, color);
                deputyCheckPromotion();
            }
        }

        public static void handcuffUpdate()
        {
            if (PlayerControl.LocalPlayer == null || !handcuffedKnows.ContainsKey(PlayerControl.LocalPlayer.PlayerId)) return;

            if (handcuffedKnows[PlayerControl.LocalPlayer.PlayerId] <= 0)
            {
                handcuffedKnows.Remove(PlayerControl.LocalPlayer.PlayerId);
                // Resets the buttons
                setHandcuffedKnows(false);

                // Ghost info
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareGhostInfo, SendOption.Reliable, -1);
                writer.Write(PlayerControl.LocalPlayer.PlayerId);
                writer.Write((byte)RPCProcedure.GhostInfoTypes.HandcuffOver);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
        }

        public void deputyCheckPromotion(bool isMeeting = false)
        {
            // If LocalPlayer is Deputy, the Sheriff is disconnected and Deputy promotion is enabled, then trigger promotion
            if (promotesToSheriff == 0 || player == null || player.Data.IsDead || (promotesToSheriff == 2 && !isMeeting)) return;
            if (sheriff == null || sheriff.player == null || sheriff?.player?.Data?.Disconnected == true || sheriff?.player?.Data.IsDead == true)
            {
                PromoteToSheriff.Invoke(player.PlayerId);
            }
        }

        public override void OnMeetingEnd(PlayerControl exiled = null)
        {
            if (PlayerControl.LocalPlayer == player) deputyCheckPromotion(true);
        }

        public static RemoteProcess<byte> PromoteToSheriff = RemotePrimitiveProcess.OfByte("DeputyPromotes", (message, _) =>
        {
            PlayerControl deputy = Helpers.playerById(message);
            var deputyRole = getRole(deputy);
            if (deputy != null && deputyRole != null)
            {  // Deputy should never be null here, but there appeared to be a race condition during testing, which was removed.
                float remainingCuffs = deputyRole.remainingHandcuffs;
                Sheriff.replaceCurrentSheriff(deputy);
                if (PlayerControl.LocalPlayer == deputy)
                {
                    new StaticAchievementToken("deputy.another1");
                }
                Sheriff curSheriff = Sheriff.getRole(deputy);
                curSheriff.isFormerDeputy = true;
                curSheriff.remainingHandcuffs = remainingCuffs;
                eraseRole(deputy);
            }
        });

        public static Sprite getButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.DeputyHandcuffButton.png", 115f);
            return buttonSprite;
        }

        public static Sprite getHandcuffedButtonSprite()
        {
            if (handcuffedSprite) return handcuffedSprite;
            handcuffedSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.DeputyHandcuffed.png", 115f);
            return handcuffedSprite;
        }

        // Can be used to enable / disable the handcuff effect on the target's buttons
        public static void setHandcuffedKnows(bool active = true, byte playerId = Byte.MaxValue)
        {
            if (playerId == Byte.MaxValue)
                playerId = PlayerControl.LocalPlayer.PlayerId;

            if (active && playerId == PlayerControl.LocalPlayer.PlayerId)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareGhostInfo, SendOption.Reliable, -1);
                writer.Write(PlayerControl.LocalPlayer.PlayerId);
                writer.Write((byte)RPCProcedure.GhostInfoTypes.HandcuffNoticed);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }

            if (active)
            {
                handcuffedKnows.Add(playerId, handcuffDuration);
                handcuffedPlayers.RemoveAll(x => x == playerId);
            }

            if (playerId == PlayerControl.LocalPlayer.PlayerId)
            {
                HudManagerStartPatch.setAllButtonsHandcuffedStatus(active);
                SoundEffectsManager.play("deputyHandcuff");
            }
        }

        public static void clearAndReload()
        {
            handcuffedPlayers = [];
            handcuffedKnows = [];
            HudManagerStartPatch.setAllButtonsHandcuffedStatus(false, true);
            promotesToSheriff = CustomOptionHolder.deputyGetsPromoted.getSelection();
            handcuffCooldown = CustomOptionHolder.deputyHandcuffCooldown.getFloat();
            keepsHandcuffsOnPromotion = CustomOptionHolder.deputyKeepsHandcuffs.getBool();
            handcuffDuration = CustomOptionHolder.deputyHandcuffDuration.getFloat();
            knowsSheriff = CustomOptionHolder.deputyKnowsSheriff.getBool();
            stopsGameEnd = CustomOptionHolder.deputyStopsGameEnd.getBool();
            players = [];
        }
    }
}
