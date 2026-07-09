using System;
using System.Collections.Generic;
using Hazel;
using TheOtherRoles.Modules;
using TheOtherRoles.Objects;
using TheOtherRoles.Utilities;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    [TORRPCHolder]
    public class Trapper : RoleBase<Trapper>
    {
        public Trapper()
        {
            RoleId = roleId = RoleId.Trapper;
        }

        static public IEnumerable<HelpSprite> GetHelpSprites() => [new(getTrapButtonSprite(), "trapperTrapHint")];

        public override void OnMeetingEnd(PlayerControl exiled = null)
        {
            Trap.clearAllTraps();
            meetingFlag = false;
        }

        public override void OnMeetingStart()
        {
            meetingFlag = true;
        }

        public static RemoteProcess<(byte trapId, byte trapperId, byte playerId)> ActivateTrap = new("ActivateTrap", (message, _) =>
        {
            var trapper = Helpers.playerById(message.trapperId);
            var player = Helpers.playerById(message.playerId);
            Trap.activateTrap(message.trapId, trapper, player);
        });

        public static RemoteProcess<byte> DisableTrap = RemotePrimitiveProcess.OfByte("DisableTrap", (message, _) => Trap.disableTrap(message));

        public static RemoteProcess<Vector3> PlaceTrap = RemotePrimitiveProcess.OfVector3("PlaceTrap", (message, _) => new Trap(new(message.x, message.y - 0.2f, message.z)));

        public static RemoteProcess<(byte trapId, byte trapperId, byte playerId)> TrapKill = new("TrapperKill", (message, _) =>
        {
            var trapper = Helpers.playerById(message.trapperId);
            var target = Helpers.playerById(message.playerId);
            if (PlayerControl.LocalPlayer == trapper)
            {
                new StaticAchievementToken("trapper.common1");
                new StaticAchievementToken("trapper.challenge");
            }
            Trap.trapKill(message.trapId, trapper, target);
        });

        public static RemoteProcess ClearTraps = new("ClearTraps", (_) =>
        {
            Trap.clearAllTraps();
        });

        public static RemoteProcess MeetingFlag = new("TrapperMeetingFlag", (_) =>
        {
            Trap.onMeeting();
        });

        public override void ResetRole(bool isShifted)
        {
            Trap.clearAllTraps();
        }

        public static Color color = Palette.ImpostorRed;

        public static float minDistance = 0f;
        public static float maxDistance;
        public static int numTrap;
        public static float extensionTime;
        public static float killTimer;
        public static float cooldown;
        public static float trapRange;
        public static float penaltyTime;
        public static float bonusTime;
        public static bool isTrapKill = false;
        public static bool meetingFlag;

        public static Sprite trapButtonSprite;
        public static DateTime placedTime;

        public override void FixedUpdate()
        {
            try
            {
                if (PlayerControl.LocalPlayer == player && Trap.traps.Count != 0 && !Trap.hasTrappedPlayer() && !meetingFlag)
                {
                    foreach (var p in PlayerControl.AllPlayerControls.GetFastEnumerator())
                    {
                        foreach (var trap in Trap.traps)
                        {
                            if (DateTime.UtcNow.Subtract(trap.Value.placedTime).TotalSeconds < extensionTime) continue;
                            if (trap.Value.isActive || p.Data.IsDead || p.inVent || meetingFlag) continue;
                            var p1 = p.transform.localPosition;
                            Dictionary<GameObject, byte> listActivate = [];
                            var p2 = trap.Value.trap.transform.localPosition;
                            var distance = Vector3.Distance(p1, p2);
                            if (distance < trapRange)
                            {
                                var text = Helpers.CreateAndShowNotification(string.Format(ModTranslation.getString("trapperGotTrapText"), p.Data.PlayerName), Color.white, localPos: new(0f, 1f, -20f), spr: RoleHelpers.GetRoleIcon(RoleId.Trapper).GetSprite());
                                text.AdjustNotification();

                                ActivateTrap.Invoke((trap.Key, PlayerControl.LocalPlayer.PlayerId, p.PlayerId));
                                break;
                            }
                        }
                    }
                }

                if (PlayerControl.LocalPlayer == player && Trap.hasTrappedPlayer() && !meetingFlag)
                {
                    // トラップにかかっているプレイヤーを救出する
                    foreach (var trap in Trap.traps)
                    {
                        if (trap.Value.trap == null || !trap.Value.isActive) return;
                        Vector3 p1 = trap.Value.trap.transform.position;
                        foreach (var player in PlayerControl.AllPlayerControls.GetFastEnumerator())
                        {
                            if (player.PlayerId == trap.Value.target.PlayerId || player.Data.IsDead || player.inVent || player == this.player) continue;
                            Vector3 p2 = player.transform.position;
                            float distance = Vector3.Distance(p1, p2);
                            if (distance < 0.5)
                            {
                                DisableTrap.Invoke(trap.Key);
                            }
                        }
                    }
                }
            }
            catch (NullReferenceException e)
            {
                TheOtherRolesPlugin.Logger.LogWarning(e.Message);
            }
        }

        public override void OnKill(PlayerControl target)
        {
            //　キルクールダウン設定
            if (PlayerControl.LocalPlayer == player)
            {
                if (Trap.isTrapped(target) && !isTrapKill)  // トラップにかかっている対象をキルした場合のボーナス
                {
                    player.killTimer = Mathf.Max(1f, GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown - bonusTime);
                    HudManagerStartPatch.trapperSetTrapButton.Timer = Mathf.Max(1f, cooldown - bonusTime);
                }
                else if (Trap.isTrapped(target) && isTrapKill)  // トラップキルした場合のペナルティ
                {
                    PlayerControl.LocalPlayer.killTimer = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown;
                    HudManagerStartPatch.trapperSetTrapButton.Timer = cooldown;
                    if (target.Data.Role.IsImpostor) _ = new StaticAchievementToken("trapper.another1");
                }
                else // トラップにかかっていない対象を通常キルした場合はペナルティーを受ける
                {
                    PlayerControl.LocalPlayer.killTimer = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown + penaltyTime;
                    HudManagerStartPatch.trapperSetTrapButton.Timer = cooldown + penaltyTime;
                }
                if (!isTrapKill)
                {
                    ClearTraps.Invoke();
                }
                isTrapKill = false;
            }
        }

        public static Sprite getTrapButtonSprite()
        {
            if (trapButtonSprite) return trapButtonSprite;
            trapButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.TrapperButton.png", 115f);
            return trapButtonSprite;
        }

        public static void setTrap()
        {
            var pos = PlayerControl.LocalPlayer.transform.position;
            PlaceTrap.Invoke(pos);
            placedTime = DateTime.UtcNow;
        }

        public static void clearAndReload()
        {
            numTrap = (int)CustomOptionHolder.trapperNumTrap.getFloat();
            extensionTime = CustomOptionHolder.trapperExtensionTime.getFloat();
            killTimer = CustomOptionHolder.trapperKillTimer.getFloat();
            cooldown = CustomOptionHolder.trapperCooldown.getFloat();
            trapRange = CustomOptionHolder.trapperTrapRange.getFloat();
            penaltyTime = CustomOptionHolder.trapperPenaltyTime.getFloat();
            bonusTime = CustomOptionHolder.trapperBonusTime.getFloat();
            maxDistance = CustomOptionHolder.trapperMaxDistance.getFloat();
            meetingFlag = false;
            Trap.clearAllTraps();
            players = [];
        }
    }
}
