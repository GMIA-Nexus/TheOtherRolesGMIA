using HarmonyLib;

namespace MCI.Patches;

[HarmonyPriority(Priority.Low)]
[HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
public static class PingTracker_Update
{
    public static void Postfix(PingTracker __instance)
    {
        var position = __instance.GetComponent<AspectPosition>();
        position.Alignment = AspectPosition.EdgeAlignments.Top;
        position.DistanceFromEdge = new(0f, 0.1f, 0);
        //position.DistanceFromEdge = new(3.6f, 0.1f, 0);
        position.AdjustPosition();
        __instance.text.text += "\n<color=#ff6700FF>MCI v" + MCIPlugin.VersionString + "</color>";
        __instance.text.text += MCIPlugin.Enabled ? " <color=#00ff00FF>[Active]</color>" : " <color=#ff0000FF>[InActive]</color>";

        if (MCIPlugin.Enabled)
        {
            if (!MCIPlugin.IKnowWhatImDoing)
                __instance.text.text += MCIPlugin.Persistence ? " <color=#00ff00FF>[✓]</color>" : " <color=#ff0000FF>[X]</color>";
            else
                __instance.text.text += MCIPlugin.Persistence ? " <color=#00ff00FF>[<color=#ccaa00FF>✓</color>]</color>" : " <color=#ff0000FF>[<color=#ccaa00FF>X</color>]</color>";

            __instance.text.text += (SubmergedCompatibility.Loaded && GameOptionsManager.Instance.currentNormalGameOptions.MapId == 5) ? " <color=#00ccccFF>[Submerged]</color>" : " ";
        }

        if (UpdateChecker.NeedsUpdate)
            __instance.text.text += " - <color=#ff0000FF>UPDATE AVAILABLE</color>";

        __instance.text.text += "\n<size=2>    github.com/MyDragonBreath/AmongUs.MultiClientInstancing</size>";
    }
}
