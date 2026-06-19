using HarmonyLib;
using MCI.Embedded.ReactorCoroutines;
using System.Linq;

namespace MCI.Patches;

[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
public static class CountdownPatch
{
    public static void Prefix(GameStartManager __instance) {
        __instance.countDownTimer = 0;
        if (Coroutines._ourCoroutineStore.Any(x => x.Value != null)) __instance.countDownTimer = 1;
    }
}
