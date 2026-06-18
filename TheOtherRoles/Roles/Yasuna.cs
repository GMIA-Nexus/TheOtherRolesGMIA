using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheOtherRoles.MetaContext;
using TheOtherRoles.Modules;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    public class NiceYasuna : RoleBase<NiceYasuna>
    {
        public NiceYasuna()
        {
            RoleId = roleId = RoleId.Yasuna;
        }

        public static AchievementToken<(byte targetId, bool cleared)> yasunaAcTokenChallenge = null;

        public override void PostInit()
        {
            if (PlayerControl.LocalPlayer == player)
                yasunaAcTokenChallenge ??= new("niceYasuna.challenge", (byte.MaxValue, false), (val, _) => val.cleared);
        }

        public static readonly Image Illustration = new TORSpriteLoader("Assets/Sprites/Illustrations/Yasuna.png");

        public override void OnMeetingEnd(PlayerControl exiled = null)
        {
            if (PlayerControl.LocalPlayer == player && exiled != null)
            {
                if (yasunaAcTokenChallenge.Value.targetId == exiled.PlayerId)
                {
                    PlayerControl exiledPlayer = Helpers.playerById(exiled.PlayerId);
                    yasunaAcTokenChallenge.Value.cleared |= Helpers.isEvil(exiledPlayer) && exiledPlayer.getPartner() && Helpers.isEvil(exiledPlayer.getPartner());
                    if (exiledPlayer.isRole(RoleId.Jester)) _ = new StaticAchievementToken("niceYasuna.another1");
                    yasunaAcTokenChallenge.Value.targetId = byte.MaxValue;
                }
            }
        }
    }

    public class EvilYasuna : RoleBase<EvilYasuna>
    {
        public EvilYasuna()
        {
            RoleId = roleId = RoleId.EvilYasuna;
        }

        public static AchievementToken<(byte targetId, bool cleared)> evilYasunaAcTokenChallenge;

        public override void OnMeetingEnd(PlayerControl exiled = null)
        {
            if (PlayerControl.LocalPlayer == player && exiled != null)
            {
                evilYasunaAcTokenChallenge.Value.cleared |= evilYasunaAcTokenChallenge.Value.targetId == exiled.PlayerId && !ExileController.Instance.initData.networkedPlayer.Object.Data.Role.IsImpostor;
                evilYasunaAcTokenChallenge.Value.targetId = byte.MaxValue;
            }
        }

        public static readonly Image Illustration = new TORSpriteLoader("Assets/Sprites/Illustrations/Yasuna.png");
        public override void PostInit()
        {
            if (PlayerControl.LocalPlayer != player) return;
            evilYasunaAcTokenChallenge ??= new("evilYasuna.another1", (byte.MaxValue, false), (val, _) => val.cleared);
        }
    }

    public static class Yasuna
    {
        public static Color color = new Color32(90, 255, 25, byte.MaxValue);
        public static byte specialVoteTargetPlayerId = byte.MaxValue;
        public static int _remainingSpecialVotes = 1;
        private static Sprite targetSprite;

        public static void clearAndReload()
        {
            _remainingSpecialVotes = Mathf.RoundToInt(CustomOptionHolder.yasunaNumberOfSpecialVotes.getFloat());
            specialVoteTargetPlayerId = byte.MaxValue;
            NiceYasuna.yasunaAcTokenChallenge = null;
            EvilYasuna.evilYasunaAcTokenChallenge = null;
            NiceYasuna.players = [];
            EvilYasuna.players = [];
        }

        public static PlayerControl YasunaPlayer
        {
            get
            {
                if (EvilYasuna.allPlayers.Count > 0) return EvilYasuna.allPlayers.FirstOrDefault();
                else if (NiceYasuna.allPlayers.Count > 0) return NiceYasuna.allPlayers.FirstOrDefault();
                return null;
            }
        }

        public static Sprite getTargetSprite(bool isImpostor)
        {
            if (targetSprite) return targetSprite;
            targetSprite = Helpers.loadSpriteFromResources(isImpostor ? "TheOtherRoles.Resources.EvilYasunaTargetIcon.png" : "TheOtherRoles.Resources.YasunaTargetIcon.png", 150f);
            return targetSprite;
        }

        public static int remainingSpecialVotes(bool isVote = false)
        {
            if (isVote)
                _remainingSpecialVotes = Mathf.Max(0, _remainingSpecialVotes - 1);
            return _remainingSpecialVotes;
        }

        public static bool isYasuna(byte playerId)
        {
            PlayerControl player = Helpers.playerById(playerId);
            return player.isRole(RoleId.Yasuna) || player.isRole(RoleId.EvilYasuna);
        }
    }
}
