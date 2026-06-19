using HarmonyLib;

namespace MCI.Patches;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGameHost))]
public static class OnGameStart
{
    public static void Prefix(AmongUsClient __instance)
    {
        if (!MCIPlugin.Enabled)
            return;

        foreach (var p in __instance.allClients)
        {
            p.IsReady = true;
            p.Character.gameObject.GetComponent<DummyBehaviour>().enabled = false;
        }
    }
}
