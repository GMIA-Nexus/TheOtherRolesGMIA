using HarmonyLib;

namespace MCI.Patches;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Confirm))]
public static class SameVoteAll
{
    public static void Postfix(MeetingHud __instance, ref byte suspectStateIdx)
    {
        if (!MCIPlugin.Enabled)
            return;

        foreach (var player in PlayerControl.AllPlayerControls)
            __instance.CmdCastVote(player.PlayerId, suspectStateIdx);
    }
}
