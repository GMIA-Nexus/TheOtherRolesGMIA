using HarmonyLib;
using UnityEngine;

namespace MCI.Patches;

[HarmonyPatch(typeof(SpawnInMinigame), nameof(SpawnInMinigame.Begin))]
public static class AirshipSpawn
{
    public static void Postfix(SpawnInMinigame __instance)
    {
        if (!MCIPlugin.Enabled)
            return;

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (!player.Data.PlayerName.Contains(MCIPlugin.RobotName))
                continue;

            var rand = Random.Range(0, __instance.Locations.Count);
            player.gameObject.SetActive(true);
            player.NetTransform.RpcSnapTo(__instance.Locations[rand].Location);
        }
    }
}
