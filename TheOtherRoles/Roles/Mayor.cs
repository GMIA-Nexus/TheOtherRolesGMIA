using System.Collections.Generic;
using System.Linq;
using Hazel;
using TheOtherRoles.MetaContext;
using TheOtherRoles.Modules;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles;

[TORRPCHolder]
public class Mayor : RoleBase<Mayor>
{
    public static Color color = new Color32(32, 77, 66, byte.MaxValue);
    public static Minigame emergency = null;
    public static Sprite emergencySprite = null;
    public int remoteMeetingsLeft = 1;
    public bool voteMultiple = true;
    public static int mayorChooseSingleVote { get { return CustomOptionHolder.mayorChooseSingleVote.getSelection(); } }

    public static bool meetingButton = true;
    public static int numVotes { get { return (int)CustomOptionHolder.mayorNumVotes.getFloat(); } }

    static public IEnumerable<HelpSprite> GetHelpSprites() => [
        new(getMeetingSprite(), "mayorMeetingButtonHint")
    ];

    static public IEnumerable<DocumentReplacement> GetReplacementPart()
    {
        yield return new("%NUM%", Mathf.RoundToInt(CustomOptionHolder.mayorMaxRemoteMeetings.getFloat()).ToString());
        yield return new("%VOTE%", numVotes.ToString());
        yield return new("%MVO%", mayorChooseSingleVote > 0 ? ModTranslation.getString("mayorMVOHint") : "");
    }

    public Mayor()
    {
        RoleId = roleId = RoleId.Mayor;
        remoteMeetingsLeft = Mathf.RoundToInt(CustomOptionHolder.mayorMaxRemoteMeetings.getFloat());
        acTokenChallenge = null;
        voteMultiple = true;
    }

    public static RemoteProcess<byte> MultipleVote = RemotePrimitiveProcess.OfByte("MayorVoteMultiple", (message, _) =>
    {
        var mayor = getRole(Helpers.playerById(message));
        if (mayor == null) return;
        mayor.voteMultiple = !mayor.voteMultiple;
    });

    public static RemoteProcess<(byte votedFor, byte playerId)> GainAchievement = new("UnlockMayorAch", (message, _) =>
    {
        if (PlayerControl.LocalPlayer.PlayerId == message.playerId)
        {
            if (!GameManager.Instance.LogicOptions.GetAnonymousVotes())
                new StaticAchievementToken("mayor.common1");
            if (local.acTokenChallenge != null) local.acTokenChallenge.Value.votedFor = message.votedFor;
        }
    });

    public AchievementToken<(byte votedFor, bool cleared)> acTokenChallenge = null;

    public static Sprite getMeetingSprite()
    {
        if (emergencySprite) return emergencySprite;
        emergencySprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.EmergencyButton.png", 550f);
        return emergencySprite;
    }

    public static void clearAndReload()
    {
        emergency = null;
        emergencySprite = null;
        meetingButton = CustomOptionHolder.mayorMeetingButton.getBool();
        players = [];
    }

    public override void OnMeetingEnd(PlayerControl exiled = null)
    {
        if (PlayerControl.LocalPlayer == player && exiled != null)
        {
            acTokenChallenge.Value.cleared |= acTokenChallenge.Value.votedFor == exiled.PlayerId &&
                ((Helpers.isEvil(ExileController.Instance.initData.networkedPlayer.Object) && !ExileController.Instance.initData.networkedPlayer.Object.isRole(RoleId.Jester)) || exiled.hasModifier(RoleId.Madmate)
            || exiled.hasModifier(RoleId.CreatedMadmate));
        }
    }

    public override void PostInit()
    {
        if (PlayerControl.LocalPlayer != player) return;
        acTokenChallenge ??= new("mayor.challenge", (byte.MaxValue, false), (val, _) => val.cleared);
    }
}
