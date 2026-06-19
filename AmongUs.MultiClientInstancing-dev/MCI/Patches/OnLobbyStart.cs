using HarmonyLib;

namespace MCI.Patches;

[HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
public static class OnLobbyStart
{
    public static void Postfix()
    {
        MCIPlugin.Enabled = AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame;

        if (MCIPlugin.Enabled && MCIPlugin.Persistence && InstanceControl.Clients.Count != 0)
        {
            var count = InstanceControl.Clients.Count;
            InstanceControl.Clients.Clear();
            InstanceControl.PlayerClientIDs.Clear();

            for (var i = 0; i < count; i++)
                InstanceControl.CreatePlayerInstance();
        }
    }
}
