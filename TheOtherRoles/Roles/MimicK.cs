using System.Collections.Generic;
using TheOtherRoles.Modules;
using TheOtherRoles.Objects;
using TheOtherRoles.Patches;
using TheOtherRoles.Utilities;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    public class MimicK : RoleBase<MimicK>
    {
        public static Color color = Palette.ImpostorRed;

        public static bool ifOneDiesBothDie = true;
        public static bool hasOneVote = true;
        public static bool countAsOne = true;

        public static List<Arrow> arrows = [];
        public static float updateTimer = 0f;
        public static float arrowUpdateInterval = 0.5f;
        public static float specialCooldown = 20f;

        public MimicK()
        {
            RoleId = roleId = RoleId.MimicK;
            victim = null;
        }

        public override void OnMeetingStart()
        {
            if (!hasAlivePlayers) return;
            player.setDefaultLook();
            victim = null;
        }

        public static PlayerControl victim;

        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason)
        {
            if (this.player == player)
                MimicA.resetMorph();
        }

        public override void OnMeetingEnd(PlayerControl exiled = null)
        {
            if (PlayerControl.LocalPlayer == player && MimicA.isAlive())
                PlayerControl.LocalPlayer.SetKillTimerUnchecked(specialCooldown);
        }

        public override void OnDeath(PlayerControl killer = null)
        {
            MimicA.resetMorph();
            player.setDefaultLook();
            victim = null;

            if (ifOneDiesBothDie)
            {
                foreach (var partner in MimicA.allPlayers)
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

        public override void OnKill(PlayerControl target)
        {
            if (target != player)
            {
                // Delete the dead body
                DeadBody[] array = Object.FindObjectsOfType<DeadBody>();
                for (int i = 0; i < array.Length; i++)
                    if (GameData.Instance.GetPlayerById(array[i].ParentId).PlayerId == target.PlayerId)
                        array[i].gameObject.active = false;

                // Block Mimic(Killer) from morphing if camo or mushroom is active
                if (Camouflager.camouflageTimer <= 0f && !Helpers.MushroomSabotageActive())
                    player.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId, target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId);
                victim = target;

                if (PlayerControl.LocalPlayer == player)
                {
                    _ = new StaticAchievementToken("mimicK.challenge");
                    if (MimicA.isAlive()) PlayerControl.LocalPlayer.SetKillTimerUnchecked(specialCooldown);
                }

                if (PlayerControl.LocalPlayer.isRole(RoleId.MimicA) && !PlayerControl.LocalPlayer.Data.IsDead)
                    Helpers.showFlash(new Color(42f / 255f, 187f / 255f, 245f / 255f), message: ModTranslation.getString("mimicAInfo"), textSprite: RoleHelpers.GetRoleIcon(RoleId.MimicK).GetSprite());

                VitalsStatePatch.AddMissingPlayer(target.Data);
            }
        }

        public static bool isAlive()
        {
            foreach (var p in allPlayers)
                if (!(p.Data.IsDead || p.Data.Disconnected))
                    return true;
            return false;
        }

        public override void FixedUpdate()
        {
            if (PlayerControl.LocalPlayer == player) arrowUpdate();
        }

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
                    if (p.isRole(RoleId.MimicA))
                    {
                        arrow = MimicA.isMorph ? new Arrow(Palette.White) : new Arrow(Palette.ImpostorRed);
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
            MimicA.resetMorph();
            player.setDefaultLook();
            if (arrows != null)
                foreach (Arrow arrow in arrows)
                    if (arrow?.arrow != null)
                        Object.Destroy(arrow.arrow);
            arrows = [];
        }

        public static void clearAndReload()
        {
            victim = null;
            ifOneDiesBothDie = CustomOptionHolder.mimicIfOneDiesBothDie.getBool();
            hasOneVote = CustomOptionHolder.mimicHasOneVote.getBool();
            countAsOne = CustomOptionHolder.mimicCountAsOne.getBool();
            specialCooldown = CustomOptionHolder.mimicSpecialCooldown.getFloat();
            players = [];
        }
    }
}
