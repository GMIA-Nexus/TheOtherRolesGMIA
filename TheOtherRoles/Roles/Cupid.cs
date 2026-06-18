using System;
using System.Collections.Generic;
using System.Linq;
using Hazel;
using TheOtherRoles.MetaContext;
using TheOtherRoles.Modules;
using TheOtherRoles.Utilities;
using UnityEngine;
using static TheOtherRoles.Patches.PlayerControlFixedUpdatePatch;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    [TORRPCHolder]
    public class Cupid : RoleBase<Cupid>
    {
        public Cupid()
        {
            RoleId = roleId = RoleId.Cupid;
            shielded = null;
            currentTarget = null;
            shieldTarget = null;
            startTime = DateTime.UtcNow;
            lovers1 = null;
            lovers2 = null;
        }

        static public IEnumerable<HelpSprite> GetHelpSprites()
        {
            yield return new(getArrowSprite(), "cupidArrowHint");
            yield return new(Medic.getButtonSprite(), "cupidShieldHint");
        }

        public static Color color = new Color32(246, 152, 150, byte.MaxValue);

        public PlayerControl lovers1;
        public PlayerControl lovers2;
        public PlayerControl shielded;
        public PlayerControl currentTarget;
        public PlayerControl shieldTarget;
        public DateTime startTime = DateTime.UtcNow;
        public static bool isShieldOn = false;
        public int timeLeft { get { return (int)Math.Ceiling(timeLimit - (DateTime.UtcNow - startTime).TotalSeconds); } }
        public static float timeLimit;

        public override void ResetRole(bool isShifted)
        {
            if (PlayerControl.LocalPlayer == player && HudManagerStartPatch.cupidTimeRemainingText != null)
                HudManagerStartPatch.cupidTimeRemainingText.enabled = false;
        }

        public override void FixedUpdate()
        {
            if (player != PlayerControl.LocalPlayer) return;
            var untargetables = new List<PlayerControl>();
            if (lovers1 != null) untargetables.Add(lovers1);
            currentTarget = setTarget(untargetablePlayers: untargetables);
            if (isShieldOn) shieldTarget = setTarget();
            if (lovers1 == null || lovers2 == null) setPlayerOutline(currentTarget, color);
            if (shielded == null && isShieldOn) setPlayerOutline(shieldTarget, color);

            if (HudManagerStartPatch.cupidTimeRemainingText != null)
                HudManagerStartPatch.cupidTimeRemainingText.enabled = false;

            if ((lovers1 == null || lovers2 == null) && !player.Data.IsDead)
            {
                if (HudManagerStartPatch.cupidTimeRemainingText != null)
                {
                    HudManagerStartPatch.cupidTimeRemainingText.text = TimeSpan.FromSeconds(timeLeft).ToString(@"mm\:ss");
                    HudManagerStartPatch.cupidTimeRemainingText.enabled = Helpers.ShowButtons;
                }
                if (timeLeft <= 0)
                {
                    Suicide.Invoke((player.PlayerId, false, false));
                }
            }
        }

        public override GUIContext ProgressContext { get => ProgressGUI.Holder(lovers1 != null && lovers2 != null ? ProgressGUI.OneLineText(ModTranslation.getString("lover") + ": " +
            (lovers1?.Data.PlayerName ?? "") + ", " + (lovers2?.Data.PlayerName ?? "")) : null); }

        public static RemoteProcess<(byte cupidId, bool isScapegoat, bool isExile)> Suicide = new("CupidSuicide", (message, isCalledByMe) =>
        {
            var cupid = Helpers.playerById(message.cupidId);
            if (cupid != null)
            {
                if (message.isExile)
                {
                    cupid.Exiled();
                    if (PlayerControl.LocalPlayer == cupid && Helpers.ShowKillAnimation)
                        FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(cupid.Data, cupid.Data);
                }
                cupid.MurderPlayer(cupid, MurderResultFlags.Succeeded);
                GameHistory.overrideDeathReasonAndKiller(cupid, message.isScapegoat ? DeadPlayer.CustomDeathReason.Scapegoat : DeadPlayer.CustomDeathReason.Suicide);
                if (PlayerControl.LocalPlayer == cupid && message.isScapegoat) _ = new StaticAchievementToken("cupid.another1");
            }
        });

        public static RemoteProcess<(byte targetId, byte playerId)> SetShielded = new("SetCupidShielded", (message, _) =>
        {
            PlayerControl player = Helpers.playerById(message.playerId);
            var cupid = getRole(player);
            if (player == null || cupid == null) return;
            cupid.shielded = Helpers.playerById(message.targetId);
        });

        public static RemoteProcess<(byte playerId1, byte playerId2, byte cupidId)> CreateLovers = new("CupidSetLovers", (message, _) =>
        {
            var p1 = Helpers.playerById(message.playerId1);
            var p2 = Helpers.playerById(message.playerId2);
            var player = Helpers.playerById(message.cupidId);
            var cupid = getRole(player);
            if (player == null || p1 == null || p2 == null || cupid == null) return;
            cupid.lovers1 = p1;
            cupid.lovers2 = p2;
            breakLovers(p1, p2);
            breakLovers(p2, p1);
            Lovers.addCouple(p1, p2);
        });

        public static bool checkShieldActive(PlayerControl target)
        {
            return players.Any(x => x.shielded == target && x.player && !x.player.Data.IsDead);
        }

        public static void killCupid(PlayerControl player, PlayerControl killer = null)
        {
            // Cupidを道連れにする
            foreach (var cupid in players)
            {
                if (cupid.player == null || cupid.player != PlayerControl.LocalPlayer || cupid.player.Data.IsDead) continue;
                if (cupid.lovers1 == player || cupid.lovers2 == player)
                {
                    if (killer != null)
                    {
                        Suicide.Invoke((cupid.player.PlayerId, false, false));
                    }
                    else
                    {
                        Suicide.Invoke((cupid.player.PlayerId, false, true));
                    }
                }
            }
        }

        public static void scapeGoat(PlayerControl target)
        {
            var cupids = players.FindAll(x => x.player && x.shielded == target && !x.player.Data.IsDead);
            cupids.ForEach(x => Suicide.Invoke((x.player.PlayerId, true, false)));
        }

        private static Sprite arrowSprite;
        public static Sprite getArrowSprite()
        {
            if (arrowSprite) return arrowSprite;
            arrowSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.CupidButton.png", 115f);
            return arrowSprite;
        }

        public static void breakLovers(PlayerControl p1, PlayerControl p2)
        {
            if (p1.isLovers())
            {
                PlayerControl otherLover = p1.getPartner();
                if (otherLover != null && otherLover != p2)
                {
                    Lovers.eraseCouple(p1);
                    if (!otherLover.Data.IsDead)
                    {
                        otherLover.MurderPlayer(otherLover, MurderResultFlags.Succeeded);
                        GameHistory.overrideDeathReasonAndKiller(otherLover, DeadPlayer.CustomDeathReason.LoveStolen);
                    }
                }
                else if (otherLover == p2)
                    Lovers.eraseCouple(p1);
            }
            else if (p1.isRole(RoleId.Akujo) && !Akujo.isHonmei(p2))
            {
                var akujo = Akujo.getRole(p1);
                var honmei = akujo.honmei;
                if (honmei != null)
                {
                    akujo.honmei = null;
                    // 本命を自殺させる
                    honmei.MurderPlayer(honmei, MurderResultFlags.Succeeded);
                    GameHistory.overrideDeathReasonAndKiller(honmei, DeadPlayer.CustomDeathReason.LoveStolen);
                }
                akujo.cupidHonmei = p2;
            }
            else if (p1.isRole(RoleId.Akujo) && Akujo.isHonmei(p2))
            {
                var akujo = Akujo.getRole(p1);
                akujo.honmei = null;
                akujo.cupidHonmei = p2;
            }
            else if (Akujo.isKeep(p1) && !p2.isRole(RoleId.Akujo))
                Akujo.players.ForEach(x => x.keeps.Remove(p1));
        }

        public static bool isCupidLovers(PlayerControl player)
        {
            return players.Any(x => x.lovers1 == player || x.lovers2 == player);
        }

        public static void createLovers()
        {
            CreateLovers.Invoke((local.lovers1.PlayerId, local.lovers2.PlayerId, local.player.PlayerId));
        }

        public static void clearAndReload()
        {
            timeLimit = CustomOptionHolder.cupidTimeLimit.getFloat() + 10f;
            isShieldOn = CustomOptionHolder.cupidShield.getBool();
            players = [];
        }
    }
}
