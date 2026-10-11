using System.Collections.Generic;
using TheOtherRoles.Modules;
using TheOtherRoles.Objects;
using TheOtherRoles.Utilities;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    [TORRPCHolder]
    public class MimicA : RoleBase<MimicA>
    {
        public static Color color = Palette.ImpostorRed;

        public static bool isMorph = false;

        public MimicA()
        {
            RoleId = roleId = RoleId.MimicA;
            isMorph = false;
        }

        public static RemoteProcess<(byte mimicAId, byte mimicBId)> Morph = new("MimicMorph", (message, _) =>
        {
            PlayerControl mimicA = Helpers.playerById(message.mimicAId);
            PlayerControl mimicB = Helpers.playerById(message.mimicBId);
            mimicA.GetOutfitInfo().AddOutfit(new("MimicAssistant", 40, true, mimicB.GetOutfitInfo().DefaultOutfit));
            isMorph = true;
        });

        public static RemoteProcess<byte> ResetMorph = RemotePrimitiveProcess.OfByte("MimicResetMorph", (message, _) =>
        {
            PlayerControl mimicA = Helpers.playerById(message);
            mimicA.GetOutfitInfo().RemoveOutfit("MimicAssistant");
            isMorph = false;
        });

        public static bool isAlive()
        {
            foreach (var p in allPlayers)
                if (!(p.Data.IsDead || p.Data.Disconnected))
                    return true;
            return false;
        }

        public static void resetMorph()
        {
            if (!hasAlivePlayers) return;
            isMorph = false;
            foreach (var mimicA in allPlayers)
                mimicA.setDefaultLook();
        }

        public override void PostInit()
        {
            if (PlayerControl.LocalPlayer != player) return;
            acTokenCommon ??= new("mimicA.challenge", 0, (val, _) => val >= 4);
        }

        public override void FixedUpdate()
        {
            if (player == PlayerControl.LocalPlayer) arrowUpdate();
        }

        public override void OnDeath(PlayerControl killer = null)
        {
            resetMorph();
            if (MimicK.ifOneDiesBothDie)
            {
                foreach (var partner in MimicK.allPlayers)
                {
                    if (partner != null && !partner.Data.IsDead)
                    {
                        if (killer != null)
                            partner.MurderPlayer(partner, MurderResultFlags.Succeeded);
                        else
                        {
                            partner.Exiled();
                            if (PlayerControl.LocalPlayer == partner && Helpers.ShowKillAnimation)
                                FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(partner.Data, partner.Data);
                        }
                        GameHistory.overrideDeathReasonAndKiller(partner, DeadPlayer.CustomDeathReason.Suicide);
                    }
                }
            }
        }

        public static AchievementToken<int> acTokenCommon;

        public static List<Arrow> arrows = [];
        public static float updateTimer = 0f;
        public static float arrowUpdateInterval = 0.5f;
        static void arrowUpdate()
        {
            if (PlayerControl.LocalPlayer.Data.IsDead) {
                foreach (Arrow arrow in arrows)
                {
                    if (arrow != null && arrow.arrow != null)
                    {
                        arrow.arrow.SetActive(false);
                        Object.Destroy(arrow.arrow);
                    }
                }
                return;
            }
            // 前フレームからの経過時間をマイナスする
            updateTimer -= Time.fixedDeltaTime;

            // 1秒経過したらArrowを更新
            if (updateTimer <= 0.0f)
            {

                // 前回のArrowをすべて破棄する
                foreach (Arrow arrow in arrows)
                {
                    if (arrow != null && arrow.arrow != null)
                    {
                        arrow.arrow.SetActive(false);
                        Object.Destroy(arrow.arrow);
                    }
                }

                // Arrows一覧
                arrows = [];

                // インポスターの位置を示すArrowsを描画
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (p.Data.IsDead) continue;
                    Arrow arrow;
                    if (p.isRole(RoleId.MimicK))
                    {
                        arrow = new Arrow(Palette.ImpostorRed);
                        arrow.arrow.SetActive(true);
                        arrow.Update(p.transform.position);
                        arrows.Add(arrow);
                    }
                }

                // タイマーに時間をセット
                updateTimer = arrowUpdateInterval;
            }
        }

        public override void ResetRole(bool isShifted)
        {
            resetMorph();
            if (arrows != null)
                foreach (Arrow arrow in arrows)
                    if (arrow?.arrow != null)
                        Object.Destroy(arrow.arrow);
            arrows = [];
        }

        public override void OnMeetingStart()
        {
            resetMorph();
            if (PlayerControl.LocalPlayer == player) acTokenCommon.Value = 0;
        }

        public static void clearAndReload()
        {
            isMorph = false;
            acTokenCommon = null;
            players = [];
        }
    }
}
