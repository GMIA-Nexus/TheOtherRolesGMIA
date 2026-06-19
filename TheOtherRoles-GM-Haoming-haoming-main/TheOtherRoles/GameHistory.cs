using System;
using System.Collections.Generic;
using TheOtherRoles.Patches;
using UnityEngine;

namespace TheOtherRoles
{
    public class DeadPlayer
    {
        public PlayerControl player;
        public DateTime timeOfDeath;
        public DeathReason deathReason;
        public PlayerControl killerIfExisting;

        public DeadPlayer(PlayerControl player, DateTime timeOfDeath, DeathReason deathReason, PlayerControl killerIfExisting)
        {
            this.player = player;
            this.timeOfDeath = timeOfDeath;
            this.deathReason = deathReason;
            this.killerIfExisting = killerIfExisting;
        }
    }

    static class GameHistory
    {
        public static List<Tuple<Vector3, bool>> localPlayerPositions = new();
        public static List<DeadPlayer> deadPlayers = new();
        public static Dictionary<int, FinalStatus> finalStatuses = new();

        public static void clearGameHistory()
        {
            localPlayerPositions = new List<Tuple<Vector3, bool>>();
            deadPlayers = new List<DeadPlayer>();
            finalStatuses = new Dictionary<int, FinalStatus>();
        }
    }
}
