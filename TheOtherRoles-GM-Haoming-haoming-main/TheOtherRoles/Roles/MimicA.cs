using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using TheOtherRoles.Objects;
using TheOtherRoles.Patches;
using UnityEngine;
using static TheOtherRoles.GameHistory;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles
{
    [HarmonyPatch]
    public class MimicA : RoleBase<MimicA>
    {
        public static Color color = Palette.ImpostorRed;
        public static bool isMorph = false;

        public MimicA()
        {
            RoleType = roleId = RoleType.MimicA;
        }

        public override void OnMeetingStart()
        {
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(3f, new Action<float>((p) =>
            { // Delayed action
                if (p == 1f)
                {
                    MorphHandler.resetMorph(player);
                    isMorph = false;
                }
            })));

        }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate()
        {
            if (PlayerControl.LocalPlayer == player)
                arrowUpdate();
        }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null)
        {
            if (MimicK.ifOneDiesBothDie)
            {
                var partner = MimicK.players.FirstOrDefault().player;
                if (!partner.Data.IsDead)
                {
                    if (killer != null)
                    {
                        partner.MurderPlayer(partner);
                    }
                    else
                    {
                        partner.Exiled();
                    }
                    finalStatuses[partner.PlayerId] = FinalStatus.Suicide;
                }
            }

        }
        public override void OnFinishShipStatusBegin() { }

        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static CustomButton morphButton;
        public static CustomButton adminButton;
        public static Sprite adminButtonSprite;
        public static Sprite morphButtonSprite;
        public static Sprite getMorphButtonSprite()
        {
            if (morphButtonSprite) return morphButtonSprite;
            morphButtonSprite = ModTranslation.getImage("MorphButton.png", 115f);
            return morphButtonSprite;
        }
        public static Sprite getAdminButtonSprite()
        {
            if (adminButtonSprite) return adminButtonSprite;
            byte mapId = GameOptionsManager.Instance.currentNormalGameOptions.MapId;
            UseButtonSettings button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.PolusAdminButton]; // Polus
            if (mapId is 0 or 3) button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.AdminMapButton]; // Skeld || Dleks
            else if (mapId == 1) button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.MIRAAdminButton]; // Mira HQ
            else if (mapId == 4) button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.AirshipAdminButton]; // Airship
            adminButtonSprite = button.Image;
            return adminButtonSprite;
        }
        public static void MakeButtons(HudManager hm)
        {
            morphButton = new CustomButton(
                () =>
                {
                    if (!isMorph)
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.mimicMorph, Hazel.SendOption.Reliable, -1);
                        writer.Write(PlayerControl.LocalPlayer.PlayerId);
                        writer.Write(MimicK.allPlayers.FirstOrDefault().PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.mimicMorph(PlayerControl.LocalPlayer.PlayerId, MimicK.allPlayers.FirstOrDefault().PlayerId);
                    }
                    else
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.mimicResetMorph, Hazel.SendOption.Reliable, -1);
                        writer.Write(PlayerControl.LocalPlayer.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.mimicResetMorph(PlayerControl.LocalPlayer.PlayerId);
                    }

                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleType.MimicA) && PlayerControl.LocalPlayer.isAlive() && MimicK.isAlive(); },
                () => { return PlayerControl.LocalPlayer.CanMove && !Helpers.MushroomSabotageActive(); },
                () =>
                {
                },
                getMorphButtonSprite(),
                CustomButton.ButtonPositions.upperRowLeft,
                hm,
                hm.UseButton,
                KeyCode.Q,
                false
            )
            {
                buttonText = ""
            };

            adminButton = new CustomButton(
                 () =>
                 {
                     PlayerControl.LocalPlayer.NetTransform.Halt();
                     HudManager __instance = FastDestroyableSingleton<HudManager>.Instance;
                     __instance.InitMap();
                     MapBehaviour.Instance.ShowCountOverlay(allowedToMove: true, showLivePlayerPosition: true, includeDeadBodies: true);
                     if (PlayerControl.LocalPlayer.AmOwner)
                     {
                         PlayerControl.LocalPlayer.MyPhysics.inputHandler.enabled = true;
                         ConsoleJoystick.SetMode_Task();
                     }
                 },
                 () =>
                 {
                     return PlayerControl.LocalPlayer.isRole(RoleType.MimicA) &&
                       PlayerControl.LocalPlayer.isAlive() &&
                       MimicK.isAlive();
                 },
                 () => { return PlayerControl.LocalPlayer.CanMove; },
                 () => { },
                 EvilHacker.getButtonSprite(),
                 CustomButton.ButtonPositions.upperRowCenter,
                 hm,
                 hm.KillButton,
                 KeyCode.F,
                 false)
            {
                buttonText = FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Admin)
            };
            adminButton.MaxTimer = adminButton.Timer = 0;
        }
        public static void SetButtonCooldowns()
        {
            morphButton.MaxTimer = 0f;
            adminButton.MaxTimer = 0f;
        }

        public static void Clear()
        {
            players = new List<MimicA>();
            isMorph = false;
        }
        public static bool isAlive()
        {
            foreach (var p in players)
            {
                if (!(p.player.Data.IsDead || p.player.Data.Disconnected))
                    return true;
            }
            return false;
        }

        public static List<Arrow> arrows = new();
        public static float updateTimer = 0f;
        public static float arrowUpdateInterval = 0.5f;
        static void arrowUpdate()
        {

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
                        UnityEngine.Object.Destroy(arrow.arrow);
                    }
                }

                // Arrows一覧
                arrows = new List<Arrow>();

                // インポスターの位置を示すArrowsを描画
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (p.Data.IsDead) continue;
                    Arrow arrow;
                    if (p.isRole(RoleType.MimicK))
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
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
        public static class MurderPlayerPatch
        {
            public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
            {
                PlayerControl player = PlayerControl.LocalPlayer;
                if (__instance.isRole(RoleType.MimicK) && __instance != player && player.isRole(RoleType.MimicA) && player.isAlive())
                {
                    Helpers.showFlash(new Color(42f / 255f, 187f / 255f, 245f / 255f));
                }
            }
        }
    }
}
