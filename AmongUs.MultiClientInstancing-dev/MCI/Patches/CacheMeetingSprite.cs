using HarmonyLib;
using UnityEngine;

namespace MCI.Patches;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.SetForegroundForDead))]
public static class CacheMeetingSprite
{
    public static Sprite Cache;

    public static void Prefix(MeetingHud __instance) => Cache ??= __instance.Glass.sprite;
}