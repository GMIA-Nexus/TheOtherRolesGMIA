using System.Collections;
using System.Collections.Generic;
using Hazel;
using TheOtherRoles.MetaContext;
using TheOtherRoles.Modules;
using TheOtherRoles.Objects;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles;

[TORRPCHolder]
public class Detective : RoleBase<Detective>
{
    public static Color color = new Color32(45, 106, 165, byte.MaxValue);

    public static float footprintIntervall = 1f;
    public static float footprintDuration = 1f;
    public static bool anonymousFootprints = false;
    public static float reportNameDuration = 0f;
    public static float reportColorDuration = 20f;
    public float timer = 6.2f;
    public static float inspectCooldown = 15f;
    public static float inspectDuration { get { return CustomOptionHolder.detectiveInspectDuration.getFloat(); } }
    public static SpriteLoader detectiveIcon = SpriteLoader.FromResource("TheOtherRoles.Resources.DetectiveArrow.png", 100f);
    static public SpriteLoader hintSprite = SpriteLoader.FromResource("TheOtherRoles.Resources.DetectiveArrowHint.png", 100f);

    static public IEnumerable<HelpSprite> GetHelpSprites()
    {
        yield return new(getButtonSprite(), "detectiveTrackHint");
        yield return new(hintSprite, "detectiveIconHint");
    }

    static public IEnumerable<DocumentReplacement> GetReplacementPart()
    {
        yield return new("%DUR%", inspectDuration.ToString());
    }

    public override AudioClip IntroSound { get => VanillaAsset.DetectiveIntroSound; }

    public Detective()
    {
        RoleId = roleId = RoleId.Detective;
        acTokenCommon = null;
        acTokenChallenge = null;
        timer = 6.2f;
    }

    public AchievementToken<bool> acTokenCommon = null;
    public AchievementToken<(bool reported, byte votedFor, byte killerId, bool cleared)> acTokenChallenge = null;

    public static RemoteProcess<(byte votedFor, byte playerId)> GainAchievement = new("UnlockDetectiveAch", (message, isCalledByMe) =>
    {
        if (PlayerControl.LocalPlayer.PlayerId == message.playerId)
        {
            local.acTokenChallenge.Value.votedFor = message.votedFor;
        }
    });

    private static Sprite buttonSprite;
    public static Sprite getButtonSprite()
    {
        if (buttonSprite) return buttonSprite;
        buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.DetectiveButton.png", 115f);
        return buttonSprite;
    }

    public override void FixedUpdate()
    {
        if (player != PlayerControl.LocalPlayer) return;

        timer -= Time.fixedDeltaTime;
        if (timer <= 0f)
        {
            timer = footprintIntervall;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player != null && player != PlayerControl.LocalPlayer && !player.Data.IsDead && !player.inVent && !(Ninja.isStealthed(player) || Sprinter.isSprinting(player)
                    || (player.isRole(RoleId.Fox) && Fox.stealthed) || (player.isRole(RoleId.Kataomoi) && Kataomoi.isStalking())))
                {
                    FootprintHolder.Instance.MakeFootprint(player);
                }
            }
        }
    }

    public override void PostInit()
    {
        if (PlayerControl.LocalPlayer != player) return;
        acTokenChallenge ??= new("detective.challenge", (false, byte.MaxValue, byte.MaxValue, false), (val, _) => val.cleared);
        acTokenCommon ??= new("detective.common1", false, (val, _) => val);
    }

    public override void OnMeetingEnd(PlayerControl exiled = null)
    {
        if (!PlayerControl.LocalPlayer.Data.IsDead && PlayerControl.LocalPlayer == player)
        {
            if (exiled != null)
                acTokenChallenge.Value.cleared |= acTokenChallenge.Value.reported && acTokenChallenge.Value.votedFor == exiled.PlayerId;
            acTokenChallenge.Value.reported = false;
            acTokenChallenge.Value.killerId = byte.MaxValue;
        }
    }

    public static void clearAndReload()
    {
        anonymousFootprints = CustomOptionHolder.detectiveAnonymousFootprints.getBool();
        footprintIntervall = CustomOptionHolder.detectiveFootprintIntervall.getFloat();
        footprintDuration = CustomOptionHolder.detectiveFootprintDuration.getFloat();
        reportNameDuration = CustomOptionHolder.detectiveReportNameDuration.getFloat();
        reportColorDuration = CustomOptionHolder.detectiveReportColorDuration.getFloat();
        inspectCooldown = CustomOptionHolder.detectiveInspectCooldown.getFloat();
        players = [];
    }
}
