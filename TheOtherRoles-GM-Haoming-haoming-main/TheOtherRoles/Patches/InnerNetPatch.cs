using HarmonyLib;
using Hazel.Udp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace TheOtherRoles.Patches
{
    [HarmonyPatch(typeof(UnityUdpClientConnection), nameof(UnityUdpClientConnection.ConnectAsync))]
    public static class UnityUdpClientConnectionConnectAsyncPatch
    {
        public static void Prefix(UnityUdpClientConnection __instance, Il2CppStructArray<byte> bytes)
        {
            __instance.KeepAliveInterval = 2000;
            __instance.DisconnectTimeoutMs = __instance.EndPoint.Port == 22025 ? 0 : 15000;
        }
    }
}
