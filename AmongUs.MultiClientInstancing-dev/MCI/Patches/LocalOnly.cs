using HarmonyLib;
using UnityEngine;
using TMPro;

namespace MCI.Patches;

[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
public static class LocalOnly
{
    public static void Postfix()
    {
        var inf = new GameObject("Info");
        inf.transform.position = new(0, -1.75f, 0);
        var tmp = inf.AddComponent<TextMeshPro>();
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.horizontalAlignment = HorizontalAlignmentOptions.Center;
        tmp.text = "MCI only supports localhosted lobbies.";
        tmp.color = Color.red;
        tmp.fontSize = 2.25f;

        var pos = inf.AddComponent<AspectPosition>();
        pos.Alignment = AspectPosition.EdgeAlignments.RightBottom;
        pos.DistanceFromEdge = new(3.2f, 1, 200);
        pos.AdjustPosition();
    }
}
