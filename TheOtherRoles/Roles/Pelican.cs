using System.Collections.Generic;
using TheOtherRoles.Modules;
using UnityEngine;
using static TheOtherRoles.Patches.PlayerControlFixedUpdatePatch;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles;

public class Pelican : RoleBase<Pelican>
{
    public static Color32 color = new(153, 253, 153, byte.MaxValue);
    public PlayerControl currentTarget;
    public List<PlayerControl> eatenPlayers = [];
    public static float cooldown;
    public static bool canUseVents;
    public static bool hasImpVision;

    public Pelican()
    {
        roleId = RoleId = RoleId.Pelican;
        currentTarget = null;
        eatenPlayers = [];
    }

    public static RemoteProcess<(byte playerId, byte targetId)> RpcSwallow = new("PelicanKill", (message, _) =>
    {
        var player = Helpers.playerById(message.playerId);
        var target = Helpers.playerById(message.targetId);
        if (player == null || target == null) return;
        TORGameManager.Instance?.GameStatistics.RecordEvent(new(GameStatistics.EventVariation.Kill, message.playerId, 1 << message.targetId) { RelatedTag = EventDetail.Swallowed });
        var pelican = getRole(player);
        pelican.eatenPlayers.Add(target);
        target.Die(DeathReason.Exile, false);
        if (target == PlayerControl.LocalPlayer)
        {
            HudManager.Instance.PlayerCam.SetTargetWithLight(player);
            PlayerControl.LocalPlayer.moveable = false;
        }
    });

    public override void ResetRole(bool isShifted)
    {
        DiePelican();
    }

    public override void FixedUpdate()
    {
        if (PlayerControl.LocalPlayer != player || PlayerControl.LocalPlayer.Data.IsDead) return;
        var untargetablePlayers = new List<PlayerControl>();
        if (Mini.mini != null && !Mini.isGrownUp()) untargetablePlayers.Add(Mini.mini);
        currentTarget = setTarget(untargetablePlayers: untargetablePlayers);
        setPlayerOutline(currentTarget, Palette.ImpostorRed);
    }

    public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason)
    {
        DiePelican(true);
    }

    public override void OnDeath(PlayerControl killer = null)
    {
        if (PlayerControl.LocalPlayer == player && killer != null && eatenPlayers?.Count > 0)
            _ = new StaticAchievementToken("pelican.another1");
        DiePelican(killer != null);
    }

    public void DiePelican(bool reviveEaten = false)
    {
        foreach (var p in eatenPlayers)
        {
            if (reviveEaten) p.Revive();
            if (p == PlayerControl.LocalPlayer)
            {
                HudManager.Instance.PlayerCam.SetTargetWithLight(PlayerControl.LocalPlayer);
                PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(player.transform.position);
                PlayerControl.LocalPlayer.moveable = true;
            }
        }
        eatenPlayers = [];
    }

    public override void OnMeetingStart()
    {
        if (player.Data.IsDead) return;
        foreach (var p in eatenPlayers)
        {
            p.Exiled();
            GameHistory.overrideDeathReasonAndKiller(p, DeadPlayer.CustomDeathReason.Swallowed, player);
            if (PlayerControl.LocalPlayer == p)
            {
                HudManager.Instance.ShadowQuad?.gameObject?.SetActive(false);
                PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(player.GetTruePosition());
                HudManager.Instance.PlayerCam.SetTargetWithLight(PlayerControl.LocalPlayer);
                PlayerControl.LocalPlayer.moveable = true;
            }
        }
        eatenPlayers = [];
    }

    public static void clearAndReload()
    {
        cooldown = CustomOptionHolder.pelicanCooldown.getFloat();
        canUseVents = CustomOptionHolder.pelicanCanUseVents.getBool();
        hasImpVision = CustomOptionHolder.pelicanHasImpVision.getBool();
        players = [];
    }
}
