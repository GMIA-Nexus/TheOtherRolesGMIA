using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using TheOtherRoles.Objects;
using TheOtherRoles.Patches;
using UnityEngine;
using static TheOtherRoles.GameHistory;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles
{
    [HarmonyPatch]
    public class Munou : ModifierBase<Munou>
    {
        public enum MunouType
        {
            Simple = 0,
            Random = 1,
        }
        public static Color color = Color.grey;
        public static bool endGameFlag = false;
        public static bool randomColorFlag = false;
        public static int probability { get { return (int)CustomOptionHolder.munouProbability.getFloat(); } }
        public static int numShufflePlayers { get { return (int)CustomOptionHolder.munouNumShufflePlayers.getFloat(); } }
        public static Dictionary<byte, byte> randomPlayers = new();
        public static string postfix
        {
            get
            {
                return ModTranslation.getString("incompetent");
            }
        }

        public static MunouType munouType { get { return (MunouType)CustomOptionHolder.munouType.getSelection(); } }
        public static List<RoleType> validRoles = new()
        {
            RoleType.Crewmate,
            RoleType.Shifter,
            RoleType.Mayor,
            RoleType.Engineer,
            RoleType.Sheriff,
            RoleType.Lighter,
            RoleType.Detective,
            RoleType.TimeMaster,
            RoleType.Medic,
            RoleType.Swapper,
            RoleType.Seer,
            RoleType.Hacker,
            RoleType.Tracker,
            RoleType.SecurityGuard,
            RoleType.Bait,
            RoleType.Medium,
            RoleType.FortuneTeller,
            RoleType.NiceGuesser,
            RoleType.Watcher,
        };
        public static List<PlayerControl> candidates
        {
            get
            {
                List<PlayerControl> crewNoRole = new();
                List<PlayerControl> validPlayers = new();

                foreach (var player in PlayerControl.AllPlayerControls.ToArray())
                {
                    var info = RoleInfo.getRoleInfoForPlayer(player);
                    if (info.Contains(RoleInfo.crewmate) && !player.hasModifier(ModifierType.Munou) && !player.isRole(RoleType.FortuneTeller))
                    {
                        crewNoRole.Add(player);
                    }
                    if (!player.hasModifier(ModifierType.Munou))
                    {
                        validPlayers.Add(player);
                    }
                }

                if (munouType == MunouType.Simple) return crewNoRole;
                else if (munouType == MunouType.Random) return validPlayers;
                return validPlayers;
            }
        }


        public Munou()
        {
            ModType = modId = ModifierType.Munou;
        }

        public override void OnMeetingStart()
        {
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(3f, new Action<float>((p) =>
            {
                if (p == 1)
                {
                    resetColors();
                }
            })));
        }
        public override void OnMeetingEnd()
        {
            if (player == PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.hasModifier(ModifierType.Munou) && PlayerControl.LocalPlayer.isAlive())
            {
                randomColors();
            }
        }

        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null)
        {
            if (player == PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.hasModifier(ModifierType.Munou)) resetColors();
        }
        public override void OnFinishShipStatusBegin() { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void MakeButtons(HudManager hm) { }
        public static void SetButtonCooldowns() { }

        public static void Clear()
        {
            players = new List<Munou>();
            randomPlayers = new Dictionary<byte, byte>();
            endGameFlag = false;
            colorPairs = new Dictionary<byte, byte>();
            resetColors();
        }

        public static Dictionary<byte, byte> colorPairs = new();
        public static void randomColors()
        {
            colorPairs = new Dictionary<byte, byte>();
            // 発生確率
            int random = rnd.Next(100);
            if (random > probability) return;

            var allPlayers = PlayerControl.AllPlayerControls.ToArray();
            List<byte> alivePlayers = new();
            List<int> tempList = new();
            foreach (var p in allPlayers)
            {
                if (p.PlayerId == PlayerControl.LocalPlayer.PlayerId) continue;
                if (p == Puppeteer.dummy) continue;
                if (p.isAlive()) alivePlayers.Add(p.PlayerId);
            }
            alivePlayers.shuffle();
            List<byte> shuffleTargets = alivePlayers.Count > numShufflePlayers ? alivePlayers.Take(numShufflePlayers).ToList() : alivePlayers;
            foreach (byte id in shuffleTargets)
            {
                if (id == PlayerControl.LocalPlayer.PlayerId) continue;
                var p = Helpers.playerById(id);
                int rnd;
                int coutner = 0;
                while (true)
                {
                    rnd = TheOtherRoles.rnd.Next(shuffleTargets.Count);
                    if (shuffleTargets[rnd] == PlayerControl.LocalPlayer.PlayerId) continue;
                    if (!tempList.Contains(rnd))
                    {
                        tempList.Add(rnd);
                        break;
                    }
                    coutner++;
                }
                var to = Helpers.playerById((byte)shuffleTargets[rnd]);
                MorphHandler.morphToPlayer(p, to);
                colorPairs[p.PlayerId] = to.PlayerId;
            }
            randomColorFlag = true;
        }

        public static void reMorph(byte playerId)
        {
            if (colorPairs.ContainsKey(playerId))
            {
                MorphHandler.morphToPlayer(Helpers.playerById(playerId), Helpers.playerById(colorPairs[playerId]));
            }
        }

        public static void resetColors()
        {
            colorPairs = new Dictionary<byte, byte>();
            var allPlayers = PlayerControl.AllPlayerControls;
            foreach (var p in allPlayers)
            {
                MorphHandler.morphToPlayer(p, p);
            }
            randomColorFlag = false;
        }

        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
        public class OnGameEndPatch
        {

            public static void Prefix(AmongUsClient __instance, [HarmonyArgument(0)] ref EndGameResult endGameResult)
            {
                Munou.endGameFlag = true;
            }
        }
    }
}
