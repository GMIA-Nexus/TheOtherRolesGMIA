using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheOtherRoles.Modules;
using TheOtherRoles.Utilities;
using UnityEngine;
using static TheOtherRoles.Patches.PlayerControlFixedUpdatePatch;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles;

[TORRPCHolder]
public class Jailor : RoleBase<Jailor>
{
    public static Color32 color = new(166, 166, 166, 255);

    public static float cooldown = 30f;
    public int remainingUses = 3;
    public static bool suicidesIfFalseJail = true;
    public static bool targetDiesAsWell;
    public PlayerControl currentTarget = null;
    public PlayerControl jailTarget = null;
    public AchievementToken<int> acTokenChallenge = null;

    public Jailor()
    {
        roleId = RoleId = RoleId.Jailor;
        remainingUses = Mathf.RoundToInt(CustomOptionHolder.jailorNumberOfJails.getFloat());
        currentTarget = null;
        jailTarget = null;
        acTokenChallenge = null;
    }

    public override void FixedUpdate()
    {
        if (PlayerControl.LocalPlayer != player || PlayerControl.LocalPlayer.Data.IsDead || remainingUses <= 0) return;
        currentTarget = setTarget();
        setPlayerOutline(currentTarget, color);
    }

    public override void PostInit()
    {
        if (PlayerControl.LocalPlayer != player) return;
        acTokenChallenge ??= new("jailor.challenge", 0, (val, _) => val >= 3);
    }

    private static Sprite buttonSprite;
    public static Sprite getButtonSprite()
    {
        if (buttonSprite) return buttonSprite;
        buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.JailorButton.png", 115f);
        return buttonSprite;
    }

    public static MetaContext.Image InJail = MetaContext.SpriteLoader.FromResource("TheOtherRoles.Resources.InJail.png", 225f);
    public static MetaContext.Image JailCell = MetaContext.SpriteLoader.FromResource("TheOtherRoles.Resources.JailCell.png", 105f);

    public static RemoteProcess<(byte jailorId, byte playerId)> Jail = new("JailorJail", (message, _) =>
    {
        var jailor = getRole(Helpers.playerById(message.jailorId));
        var player = Helpers.playerById(message.playerId);
        if (jailor == null || player == null) return;
        jailor.remainingUses--;
        jailor.jailTarget = player;
    });

    public static RemoteProcess<(byte jailorId, byte targetId, bool suicide)> Execute = new("JailorExecute", (message, _) =>
    {
        var jailorPlayer = Helpers.playerById(message.jailorId);
        var jailor = getRole(jailorPlayer);
        var player = Helpers.playerById(message.targetId);
        if (jailorPlayer == null || jailor == null || player == null) return;
        if (jailorPlayer.Data.IsDead || player.Data.IsDead) return;

        string msg = "";
        if (message.suicide)
        {
            if (suicidesIfFalseJail)
            {
                if (PlayerControl.LocalPlayer == jailorPlayer)
                {
                    new StaticAchievementToken("jailor.another1");
                    if (Helpers.ShowKillAnimation)
                        FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(PlayerControl.LocalPlayer.Data, PlayerControl.LocalPlayer.Data);
                }
                jailorPlayer.Exiled();
                GameHistory.overrideDeathReasonAndKiller(jailorPlayer, DeadPlayer.CustomDeathReason.Suicide);
                msg = "jailorSuicideNotification";
            }
            else {
                msg = "jailorExecuteCrewNotification";
            }
            jailor.remainingUses = 0;
        }
        else
        {
            if (PlayerControl.LocalPlayer == jailorPlayer)
            {
                new StaticAchievementToken("jailor.common1");
                jailor.acTokenChallenge.Value++;
            }
            if (player.isRole(RoleId.NekoKabocha)) NekoKabocha.getRole(player).meetingKiller = jailorPlayer;
            msg = "jailorExecuteEvilNotification";
        }

        if (!message.suicide || targetDiesAsWell)
        {
            if (PlayerControl.LocalPlayer == player && Helpers.ShowKillAnimation) FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(jailorPlayer.Data, PlayerControl.LocalPlayer.Data);
            player.Exiled();
            GameHistory.overrideDeathReasonAndKiller(player, DeadPlayer.CustomDeathReason.Jailed, jailorPlayer);
        }

        jailor.jailTarget = null;

        if (PlayerControl.LocalPlayer == jailorPlayer)
            RoleHelpers.RoleMessage(RoleId.Jailor, msg);
    });

    public static bool isJailed(byte playerId)
    {
        var player = Helpers.playerById(playerId);
        if (player == null || player.Data?.IsDead == true) return false;
        return players.Any(x => x.player && !x.player.Data.IsDead && x.jailTarget && x.jailTarget.PlayerId == playerId);
    }

    public override void OnMeetingEnd(PlayerControl exiled = null)
    {
        jailTarget = null;
    }

    public static void clearAndReload()
    {
        cooldown = CustomOptionHolder.jailorCooldown.getFloat();
        suicidesIfFalseJail = CustomOptionHolder.jailorSuicidesIfFalseJail.getBool();
        targetDiesAsWell = CustomOptionHolder.jailorTargetDiesIfFalseJail.getBool();
        players = [];
    }
}
