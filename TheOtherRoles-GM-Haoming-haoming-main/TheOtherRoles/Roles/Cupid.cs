using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TheOtherRoles.Objects;
using TheOtherRoles.Patches;
using static TheOtherRoles.TheOtherRoles;
using static TheOtherRoles.GameHistory;
using static TheOtherRoles.Patches.PlayerControlFixedUpdatePatch;
using System;
using Hazel;

namespace TheOtherRoles
{
    [HarmonyPatch]
    public class Cupid : RoleBase<Cupid>
    {
        public PlayerControl lovers1;
        public PlayerControl lovers2;
        public PlayerControl shielded;
        private PlayerControl currentTarget;
        private PlayerControl shieldTarget;
        private static bool isShieldOn { get { return CustomOptionHolder.cupidShield.getBool(); } }
        private static CustomButton arrowButton;
        private static CustomButton shieldButton;
        public static TMPro.TMP_Text timeLimitText;
        public static TMPro.TMP_Text numKeepsText;

        public static Color color = new Color32(246, 152, 150, byte.MaxValue);

        public int timeLeft { get { return (int)Math.Ceiling(timeLimit - (DateTime.UtcNow - local.startTime).TotalSeconds); } }
        public static float timeLimit { get { return CustomOptionHolder.cupidTimeLimit.getFloat() + 10f; } }
        public DateTime startTime = DateTime.UtcNow;
        public string timeString
        {
            get
            {
                return String.Format(ModTranslation.getString("timeRemaining"), TimeSpan.FromSeconds(local.timeLeft).ToString(@"mm\:ss"));
            }
        }

        public Cupid()
        {
            RoleType = roleId = RoleType.Cupid;
            startTime = DateTime.UtcNow;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd()
        {
        }

        public override void FixedUpdate()
        {
            if (PlayerControl.LocalPlayer == player)
            {
                shieldTarget = setTarget();
                if (timeLimitText != null) timeLimitText.enabled = false;
                currentTarget = setTarget(untargetablePlayers: new List<PlayerControl>() { local.lovers1 });
                if (local.player.isAlive() && (lovers1 == null || lovers2 == null))
                {
                    if (timeLimitText != null)
                    {
                        timeLimitText.text = timeString;
                        timeLimitText.enabled = true;
                    }
                    if (timeLeft <= 0 && (lovers1 == null || lovers2 == null) && player.isAlive())
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CupidSuicide, Hazel.SendOption.Reliable, -1);
                        writer.Write(player.PlayerId);
                        writer.Write(false);
                        writer.Write(false);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.cupidSuicide(player.PlayerId, false, false);
                    }
                }
            }
        }

        public static bool checkShieldActive(PlayerControl target)
        {
            return Cupid.players.Count(x => x.shielded == target && x.player.isAlive()) > 0;
        }

        public static void scapeGoat(PlayerControl target)
        {
            var cupids = Cupid.players.FindAll(x => x.shielded == target && x.player.isAlive());
            cupids.ForEach(x =>
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CupidSuicide, Hazel.SendOption.Reliable, -1);
                writer.Write(x.player.PlayerId);
                writer.Write(true);
                writer.Write(false);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.cupidSuicide(x.player.PlayerId, true, false);
            });

        }

        public override void OnKill(PlayerControl target) { }

        public override void OnDeath(PlayerControl killer = null)
        {
            if (PlayerControl.LocalPlayer == player)
            {
                lovers1 = null;
                lovers2 = null;
                shielded = null;
            }
        }
        public override void OnFinishShipStatusBegin() { }

        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason)
        {
        }


        public static void MakeButtons(HudManager hm)
        {
            // Arrow Button
            arrowButton = new CustomButton(
                () =>
                {
                    if (local.lovers1 == null)
                    {
                        local.lovers1 = local.currentTarget;
                    }
                    else
                    {
                        if (local.currentTarget != local.lovers1)
                        {
                            local.lovers2 = local.currentTarget;
                        }
                    }
                    if (local.lovers1 != null && local.lovers2 != null) createLovers();
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleType.Cupid) && !PlayerControl.LocalPlayer.Data.IsDead && local.lovers2 == null && local.timeLeft > 0; },
                () => { return PlayerControl.LocalPlayer.isRole(RoleType.Cupid) && !PlayerControl.LocalPlayer.Data.IsDead && local.currentTarget != null && local.lovers2 == null && local.timeLeft > 0; },
                () => { arrowButton.Timer = arrowButton.MaxTimer; },
                Cupid.getArrowSprite(),
                CustomButton.ButtonPositions.upperRowRight,
                hm,
                hm.AbilityButton,
                KeyCode.F
            );
            arrowButton.buttonText = ModTranslation.getString("cupidArrow");
            timeLimitText = GameObject.Instantiate(arrowButton.actionButton.cooldownTimerText, hm.transform);
            timeLimitText.text = "";
            timeLimitText.enableWordWrapping = false;
            timeLimitText.transform.localScale = Vector3.one * 0.45f;
            timeLimitText.transform.localPosition = arrowButton.actionButton.cooldownTimerText.transform.parent.localPosition + new Vector3(-0.1f, 0.35f, 0f);

            // Shield Button
            shieldButton = new CustomButton(
                () =>
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetCupidShield, Hazel.SendOption.Reliable, -1);
                    writer.Write(local.player.PlayerId);
                    writer.Write(local.shieldTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.setCupidShield(local.player.PlayerId, local.shieldTarget.PlayerId);
                },
                () => { return isShieldOn && PlayerControl.LocalPlayer.isRole(RoleType.Cupid) && !PlayerControl.LocalPlayer.Data.IsDead && local.shielded == null; },
                () => { return isShieldOn && PlayerControl.LocalPlayer.isRole(RoleType.Cupid) && !PlayerControl.LocalPlayer.Data.IsDead && local.shielded == null && local.shieldTarget != null; },

                () => { shieldButton.Timer = shieldButton.MaxTimer; },
                Medic.getButtonSprite(),
                CustomButton.ButtonPositions.lowerRowRight,
                hm,
                hm.AbilityButton,
                KeyCode.G
            );
            shieldButton.buttonText = ModTranslation.getString("ShieldText");
        }

        public static void SetButtonCooldowns()
        {
            arrowButton.MaxTimer = 0f;
            shieldButton.MaxTimer = 0f;
        }

        public static void createLovers()
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetCupidLovers, Hazel.SendOption.Reliable, -1);
            writer.Write(local.lovers1.PlayerId);
            writer.Write(local.lovers2.PlayerId);
            writer.Write(local.player.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.setCupidLovers(local.lovers1.PlayerId, local.lovers2.PlayerId, local.player.PlayerId);
        }


        public static void Clear()
        {
            players = new List<Cupid>();
        }
        private static Sprite arrowSprite;
        public static Sprite getArrowSprite()
        {
            if (arrowSprite) return arrowSprite;
            arrowSprite = Helpers.loadSpriteFromAssetBundle("CupidButton.png");
            return arrowSprite;
        }

        public static void breakCouple(PlayerControl p1, PlayerControl p2)
        {
            // ラヴァーズ寝取り
            if (p1.isLovers())
            {
                var couple = Lovers.couples.FirstOrDefault(x => x.lover1 == p1 || x.lover2 == p1);
                if (couple != null)
                {
                    if (couple.lover1 == p1 && couple.lover2 != p2)
                    {
                        Lovers.eraseCouple(p1);
                        couple.lover2.MurderPlayer(couple.lover2);
                        finalStatuses[couple.lover2.PlayerId] = FinalStatus.Suicide;
                    }
                    else if (couple.lover2 == p1 && couple.lover1 != p2)
                    {
                        Lovers.eraseCouple(p1);
                        couple.lover1.MurderPlayer(couple.lover1);
                        finalStatuses[couple.lover1.PlayerId] = FinalStatus.Suicide;
                    }
                    else
                    {
                        Lovers.eraseCouple(p1);
                    }
                }
            }
            // 本命寝取り
            if (Akujo.isHonmei(p1) && !p2.isRole(RoleType.Akujo))
            {
                // 悪女と本命を解消させる
                var honmei = AkujoHonmei.getModifier(p1);
                var akujo = honmei.akujo;
                akujo.honmei = null;
                AkujoHonmei.eraseModifier(honmei.player);
                // 悪女を自殺させる
                akujo.player.MurderPlayer(akujo.player);
                finalStatuses[akujo.player.PlayerId] = FinalStatus.Suicide;
            }
            // 悪女寝取り
            else if (p1.isRole(RoleType.Akujo) && !Akujo.isHonmei(p2))
            {
                // 悪女と本命を解消させる
                var akujo = Akujo.getRole(p1);
                var honmei = akujo.honmei;
                if (honmei != null)
                {
                    AkujoHonmei.eraseModifier(honmei.player);
                    akujo.honmei = null;
                    // 本命を自殺させる
                    honmei.player.MurderPlayer(honmei.player);
                    finalStatuses[honmei.player.PlayerId] = FinalStatus.Suicide;
                }
                akujo.cupidHonmei = p2;

            }
            // 悪女、本命が選択された場合
            else if (p1.isRole(RoleType.Akujo) && Akujo.isHonmei(p2))
            {
                var akujo = Akujo.getRole(p1);
                akujo.honmei = null;
                akujo.cupidHonmei = p2;
            }
            // キープ寝取り
            else if (Akujo.isKeep(p1) && !p2.isRole(RoleType.Akujo))
            {
                foreach (var akujo in Akujo.players)
                {
                    List<AkujoKeep> removeList = new();
                    akujo.keeps.ForEach(x =>
                    {
                        if (x.player == p1) removeList.Add(x);
                    });
                    removeList.ForEach(x => akujo.keeps.Remove(x));
                    AkujoKeep.eraseModifier(p1);
                }
            }
        }
        public static void killCupid(PlayerControl player, PlayerControl killer = null)
        {
            // Cupidを道連れにする
            foreach (var cupid in Cupid.players)
            {
                if (cupid.player != PlayerControl.LocalPlayer || cupid.player.isDead()) continue;
                if (cupid.lovers1 == player || cupid.lovers2 == player)
                {
                    if (killer != null)
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CupidSuicide, Hazel.SendOption.Reliable, -1);
                        writer.Write(cupid.player.PlayerId);
                        writer.Write(false);
                        writer.Write(false);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.cupidSuicide(cupid.player.PlayerId, false, false);
                    }
                    else
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CupidSuicide, Hazel.SendOption.Reliable, -1);
                        writer.Write(cupid.player.PlayerId);
                        writer.Write(false);
                        writer.Write(true);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.cupidSuicide(cupid.player.PlayerId, false, true);
                    }
                    finalStatuses[cupid.player.PlayerId] = FinalStatus.Suicide;
                }
            }
        }
        public static bool isCupidLovers(PlayerControl player)
        {
            return 0 < Cupid.players.Count(x => x.lovers1 == player || x.lovers2 == player);
        }

        public static string getIcon(PlayerControl player)
        {
            bool isLovers = 0 < Cupid.players.Count(x => x.lovers1 == player || x.lovers2 == player);
            return isLovers ? Helpers.cs(Cupid.color, " ♥") : "";
        }
    }

}
