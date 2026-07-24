using Il2CppSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheOtherRoles.Utilities;

namespace TheOtherRoles.Patches
{
    using FakeVitalsParam = (byte playerId, VitalsState state);
    public enum VitalsState
    {
        Disconnected,
        Alive,
        Dead,
        Missing
    }
    public record FakeVitals(FakeVitalsParam[] Players);

    public class VitalsStatePatch
    {
        static public MetaContext.Image MissBackground = MetaContext.SpriteLoader.FromResource("TheOtherRoles.Resources.VitalBgMissing.png", 100f);
        static public FakeVitals VitalsFromActuals
        {
            get
            {
                List<FakeVitalsParam> param = new();
                var deadBodies = UnityEngine.Object.FindObjectsOfType<DeadBody>();
                foreach (var p in GameData.Instance.AllPlayers.GetFastEnumerator())
                {
                    VitalsState state = VitalsState.Alive;
                    if (p.IsDead)
                        state = deadBodies.Any(d => d.ParentId == p.PlayerId) ? VitalsState.Dead : VitalsState.Disconnected;

                    if (MissingPlayers.Any(x => x.PlayerId == p.PlayerId))
                        state = VitalsState.Missing;

                    param.Add(new(p.PlayerId, state));
                }
                return new(param.ToArray());
            }
        }

        internal static List<NetworkedPlayerInfo> MissingPlayers = [];

        public static void AddMissingPlayer(NetworkedPlayerInfo player)
        {
            MissingPlayers.Add(player);
            TheOtherRolesPlugin.Logger.LogMessage($"Player {player.PlayerId} is now marked as missing.");
        }

        public static void RemoveMissingPlayer(NetworkedPlayerInfo player)
        {
            MissingPlayers.Remove(player);
            TheOtherRolesPlugin.Logger.LogMessage($"Player {player.PlayerId} is no longer marked as missing.");
        }

        public static void ClearMissingPlayers()
        {
            MissingPlayers.Clear();
        }
    }
}
