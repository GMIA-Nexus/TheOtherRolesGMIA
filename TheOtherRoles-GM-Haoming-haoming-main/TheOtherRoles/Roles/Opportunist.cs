using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace TheOtherRoles
{
    [HarmonyPatch]
    public class Opportunist : RoleBase<Opportunist>
    {
        public static Color color = new Color32(0, 255, 00, byte.MaxValue);

        public Opportunist()
        {
            RoleType = roleId = RoleType.Opportunist;
        }

        public static void Clear()
        {
            players = new List<Opportunist>();
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void OnFinishShipStatusBegin() { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
    }
}
