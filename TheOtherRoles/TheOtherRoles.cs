using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AmongUs.Data;
using HarmonyLib;
using Hazel;
using TheOtherRoles.CustomGameModes;
using TheOtherRoles.Modules;
using TheOtherRoles.Objects;
using TheOtherRoles.Patches;
using TheOtherRoles.Roles;
using TheOtherRoles.Utilities;
using UnityEngine;

namespace TheOtherRoles
{
    [HarmonyPatch]
    public static class TheOtherRoles
    {
        public static System.Random rnd = new((int)DateTime.Now.Ticks);

        public static void clearAndReloadRoles() {
            Jester.clearAndReload();
            Mayor.clearAndReload();
            Portalmaker.clearAndReload();
            Engineer.clearAndReload();
            Sheriff.clearAndReload();
            Deputy.clearAndReload();
            Lighter.clearAndReload();
            Godfather.clearAndReload();
            Mafioso.clearAndReload();
            Janitor.clearAndReload();
            Detective.clearAndReload();
            TimeMaster.clearAndReload();
            Medic.clearAndReload();
            Shifter.clearAndReload();
            Swapper.clearAndReload();
            Lovers.clearAndReload();
            Seer.clearAndReload();
            Bait.clearAndReload();
            Morphling.clearAndReload();
            Camouflager.clearAndReload();
            Hacker.clearAndReload();
            Tracker.clearAndReload();
            Vampire.clearAndReload();
            Snitch.clearAndReload();
            Jackal.clearAndReload();
            Sidekick.clearAndReload();
            Eraser.clearAndReload();
            Spy.clearAndReload();
            Trickster.clearAndReload();
            Cleaner.clearAndReload();
            Warlock.clearAndReload();
            SecurityGuard.clearAndReload();
            Arsonist.clearAndReload();
            BountyHunter.clearAndReload();
            Vulture.clearAndReload();
            Medium.clearAndReload();
            Lawyer.clearAndReload();
            Pursuer.clearAndReload();
            Witch.clearAndReload();
            Assassin.clearAndReload();
            Thief.clearAndReload();

            // GMIA
            Ninja.clearAndReload();
            NekoKabocha.clearAndReload();
            SerialKiller.clearAndReload();
            EvilTracker.clearAndReload();
            Undertaker.clearAndReload();
            MimicK.clearAndReload();
            MimicA.clearAndReload();
            BomberA.clearAndReload();
            BomberB.clearAndReload();
            EvilHacker.clearAndReload();
            Trapper.clearAndReload();
            Blackmailer.clearAndReload();
            Yoyo.clearAndReload();
            Zephyr.clearAndReload();
            LastImpostor.clearAndReload();
            FortuneTeller.clearAndReload();
            Sprinter.clearAndReload();
            Veteran.clearAndReload();
            Sherlock.clearAndReload();
            TaskMaster.clearAndReload();
            Yasuna.clearAndReload();
            Madmate.clearAndReload();
            CreatedMadmate.clearAndReload();
            Teleporter.clearAndReload();
            Busker.clearAndReload();
            Noisemaker.clearAndReload();
            Archaeologist.clearAndReload();
            Collator.clearAndReload();
            Jailor.clearAndReload();
            Watcher.clearAndReload();
            Opportunist.clearAndReload();
            Moriarty.clearAndReload();
            Akujo.clearAndReload();
            PlagueDoctor.clearAndReload();
            JekyllAndHyde.clearAndReload();
            Cupid.clearAndReload();
            Fox.clearAndReload();
            Immoralist.clearAndReload();
            SchrodingersCat.clearAndReload();
            Kataomoi.clearAndReload();
            Doomsayer.clearAndReload();
            Pelican.clearAndReload();
            Yandere.clearAndReload();
            Puppeteer.clearAndReload();
            Role.ClearAll();

            // Modifier
            Bloody.clearAndReload();
            AntiTeleport.clearAndReload();
            Tiebreaker.clearAndReload();
            Sunglasses.clearAndReload();
            Mini.clearAndReload();
            Vip.clearAndReload();
            Invert.clearAndReload();
            Chameleon.clearAndReload();
            Armored.clearAndReload();
            Multitasker.clearAndReload();
            Diseased.clearAndReload();
            Radar.clearAndReload();

            // Gamemodes
            HandleGuesser.clearAndReload();
            HideNSeek.clearAndReload();
            FreePlayGM.clearAndReload();
        }

        public static class RoleData
        {
            public static Dictionary<RoleId, Type> allRoleIds = new()
            {
                // Crewmate
                { RoleId.Portalmaker, typeof(RoleBase<Portalmaker>) },
                { RoleId.Mayor, typeof(RoleBase<Mayor>) },
                { RoleId.Engineer, typeof(RoleBase<Engineer>) },
                { RoleId.Sheriff, typeof(RoleBase<Sheriff>) },
                { RoleId.Deputy, typeof(RoleBase<Deputy>) },
                { RoleId.Lighter, typeof(RoleBase<Lighter>) },
                { RoleId.Detective, typeof(RoleBase<Detective>) },
                { RoleId.TimeMaster, typeof(RoleBase<TimeMaster>) },
                { RoleId.Medic, typeof(RoleBase<Medic>) },
                { RoleId.Swapper, typeof(RoleBase<Swapper>) },
                { RoleId.Seer, typeof(RoleBase<Seer>) },
                { RoleId.Hacker, typeof(RoleBase<Hacker>) },
                { RoleId.Tracker, typeof(RoleBase<Tracker>) },
                { RoleId.Snitch, typeof(RoleBase<Snitch>) },
                { RoleId.Spy, typeof(RoleBase<Spy>) },
                { RoleId.SecurityGuard, typeof(RoleBase<SecurityGuard>) },
                { RoleId.NiceGuesser, typeof(RoleBase<NiceGuesser>) },
                { RoleId.Medium, typeof(RoleBase<Medium>) },
                { RoleId.FortuneTeller, typeof(RoleBase<FortuneTeller>) },
                { RoleId.TaskMaster, typeof(RoleBase<TaskMaster>) },
                { RoleId.Yasuna, typeof(RoleBase<NiceYasuna>) },
                { RoleId.Teleporter, typeof(RoleBase<Teleporter>) },
                { RoleId.Sherlock, typeof(RoleBase<Sherlock>) },
                { RoleId.Veteran, typeof(RoleBase<Veteran>) },
                { RoleId.Noisemaker, typeof(RoleBase<Noisemaker>) },
                { RoleId.Busker, typeof(RoleBase<Busker>) },
                { RoleId.Archaeologist, typeof(RoleBase<Archaeologist>) },
                { RoleId.NiceWatcher, typeof(RoleBase<NiceWatcher>) },
                { RoleId.Bait, typeof(RoleBase<Bait>) },
                { RoleId.Sprinter, typeof(RoleBase<Sprinter>) },
                { RoleId.Collator, typeof(RoleBase<Collator>) },
                { RoleId.Jailor, typeof(RoleBase<Jailor>) },

                // Impostor
                { RoleId.Godfather, typeof(RoleBase<Godfather>) },
                { RoleId.Mafioso, typeof(RoleBase<Mafioso>) },
                { RoleId.Janitor, typeof(RoleBase<Janitor>) },
                { RoleId.Morphling, typeof(RoleBase<Morphling>) },
                { RoleId.Camouflager, typeof(RoleBase<Camouflager>) },
                { RoleId.Vampire, typeof(RoleBase<Vampire>) },
                { RoleId.Eraser, typeof(RoleBase<Eraser>) },
                { RoleId.Trickster, typeof(RoleBase<Trickster>) },
                { RoleId.Cleaner, typeof(RoleBase<Cleaner>) },
                { RoleId.Warlock, typeof(RoleBase<Warlock>) },
                { RoleId.EvilGuesser, typeof(RoleBase<EvilGuesser>) },
                { RoleId.BountyHunter, typeof(RoleBase<BountyHunter>) },
                { RoleId.NekoKabocha, typeof(RoleBase<NekoKabocha>) },
                { RoleId.MimicK, typeof(RoleBase<MimicK>) },
                { RoleId.MimicA, typeof(RoleBase<MimicA>) },
                { RoleId.EvilYasuna, typeof(RoleBase<EvilYasuna>) },
                { RoleId.Trapper, typeof(RoleBase<Trapper>) },
                { RoleId.BomberA, typeof(RoleBase<BomberA>) },
                { RoleId.BomberB, typeof(RoleBase<BomberB>) },
                { RoleId.Yoyo, typeof(RoleBase<Yoyo>) },
                { RoleId.EvilHacker, typeof(RoleBase<EvilHacker>) },
                { RoleId.Blackmailer, typeof(RoleBase<Blackmailer>) },
                { RoleId.Undertaker, typeof(RoleBase<Undertaker>) },
                { RoleId.SerialKiller, typeof(RoleBase<SerialKiller>) },
                { RoleId.EvilTracker, typeof(RoleBase<EvilTracker>) },
                { RoleId.Witch, typeof(RoleBase<Witch>) },
                { RoleId.EvilWatcher, typeof(RoleBase<EvilWatcher>) },
                { RoleId.Assassin, typeof(RoleBase<Assassin>) },
                { RoleId.Ninja, typeof(RoleBase<Ninja>) },
                { RoleId.Zephyr, typeof(RoleBase<Zephyr>) },

                // Neutral
                { RoleId.Jester, typeof(RoleBase<Jester>) },
                { RoleId.Jackal, typeof(RoleBase<Jackal>) },
                { RoleId.Sidekick, typeof(RoleBase<Sidekick>) },
                { RoleId.Arsonist, typeof(RoleBase<Arsonist>) },
                { RoleId.Vulture, typeof(RoleBase<Vulture>) },
                { RoleId.Lawyer, typeof(RoleBase<Lawyer>) },
                { RoleId.Pursuer, typeof(RoleBase<Pursuer>) },
                { RoleId.SchrodingersCat, typeof(RoleBase<SchrodingersCat>) },
                { RoleId.Kataomoi, typeof(RoleBase<Kataomoi>) },
                { RoleId.JekyllAndHyde, typeof(RoleBase<JekyllAndHyde>) },
                { RoleId.Shifter, typeof(RoleBase<Shifter>) },
                { RoleId.Moriarty, typeof(RoleBase<Moriarty>) },
                { RoleId.Akujo, typeof(RoleBase<Akujo>) },
                { RoleId.Opportunist, typeof(RoleBase<Opportunist>) },
                { RoleId.Thief, typeof(RoleBase<Thief>) },
                { RoleId.Doomsayer, typeof(RoleBase<Doomsayer>) },
                { RoleId.Cupid, typeof(RoleBase<Cupid>) },
                { RoleId.PlagueDoctor, typeof(RoleBase<PlagueDoctor>) },
                { RoleId.Fox, typeof(RoleBase<Fox>) },
                { RoleId.Immoralist, typeof(RoleBase<Immoralist>) },
                { RoleId.Pelican, typeof(RoleBase<Pelican>) },
                { RoleId.Yandere, typeof(RoleBase<Yandere>) },
                { RoleId.Puppeteer, typeof(RoleBase<Puppeteer>) }
            };

            public static IEnumerable<HelpSprite> GetHelp(RoleId roleId)
            {
                return allRoleIds.TryGetValue(roleId, out var type) ? type.GetGenericArguments()[0].GetMethod("GetHelpSprites", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, null) as IEnumerable<HelpSprite> ?? [] : [];
            }

            public static IEnumerable<DocumentReplacement> GetReplacementPart(RoleId roleId)
            {
                return allRoleIds.TryGetValue(roleId, out var type) ? type.GetGenericArguments()[0].GetMethod("GetReplacementPart", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, null) as IEnumerable<DocumentReplacement> ?? [] : [];
            }
        }

        static internal class Illustrations
        {
            static private Dictionary<string, MetaContext.Image> illustrationCache = [];
            static private Dictionary<string, MetaContext.Image> roleIconCache = [];

            private static MetaContext.Image GetImageInternal(RoleId roleId, string fieldName, string pathPrefix, Dictionary<string, MetaContext.Image> cache)
            {
                string key = roleId.ToString();
                if (RoleData.allRoleIds.TryGetValue(roleId, out var type))
                {
                    var field = type.GetGenericArguments()[0].GetField(fieldName, BindingFlags.Public | BindingFlags.Static);
                    if (field?.GetValue(null) is MetaContext.Image img) return img;
                }

                if (!cache.TryGetValue(key, out var loader))
                    cache[key] = loader = new MetaContext.TORSpriteLoaderWithDefault($"{pathPrefix}{key}.png", null);
                return loader;
            }

            static public MetaContext.Image GetIllustration(RoleId roleId)  => GetImageInternal(roleId, "Illustration", "Assets/Sprites/Illustrations/", illustrationCache);

            static public MetaContext.Image GetRoleIcon(RoleId roleId) => GetImageInternal(roleId, "RoleIcon", "Assets/Sprites/RoleIcons/", roleIconCache);
        }

        public static void OnDeath(this PlayerControl player, PlayerControl killer)
        {
            foreach (var role in new List<Role>(Role.allRoles)) {
                if (role.player == player) role.OnDeath(killer);
            }
            if (player.isLovers())
                Lovers.killLovers(player, killer);
            if (Cupid.isCupidLovers(player))
                Cupid.killCupid(player, killer);
            Kataomoi.killKataomoi(player, killer);
            Akujo.killAkujo(player, killer);
            Yandere.handleYandere(player);
            if (Helpers.ShowKillAnimation) {
                RPCProcedure.updateMeeting(player.PlayerId);
                if (FastDestroyableSingleton<HudManager>.Instance != null && PlayerControl.LocalPlayer == player)
                    if (MeetingHudPatch.guesserUI != null) MeetingHudPatch.guesserUIExitButton.OnClick.Invoke();
                
                // remove shoot button from targets for all guessers and close their guesserUI
                if ((GuesserGM.isGuesser(PlayerControl.LocalPlayer.PlayerId) || PlayerControl.LocalPlayer.isRole(RoleId.Doomsayer)) && !PlayerControl.LocalPlayer.Data.IsDead &&
                    (GuesserGM.remainingShots(PlayerControl.LocalPlayer.PlayerId) > 0 || PlayerControl.LocalPlayer.isRole(RoleId.Doomsayer)) && MeetingHud.Instance) {
                    MeetingHud.Instance.playerStates.ToList().ForEach(x => { if (x.TargetPlayerId == player.PlayerId && x.transform.FindChild("ShootButton") != null) UnityEngine.Object.Destroy(x.transform.FindChild("ShootButton").gameObject); });

                    if (MeetingHudPatch.guesserUI != null && MeetingHudPatch.guesserUIExitButton != null) {
                        if (MeetingHudPatch.guesserCurrentTarget == player.PlayerId)
                            MeetingHudPatch.guesserUIExitButton.OnClick.Invoke();
                    }
                }
            }
        }

        public static void OnKill(this PlayerControl player, PlayerControl target)
        {
            foreach (var role in new List<Role>(Role.allRoles)) {
                if (role.player == player) role.OnKill(target);
            }
        }

        public abstract class Role
        {
            public static List<Role> allRoles = [];
            public PlayerControl player;
            public RoleId roleId;
            public virtual MetaContext.GUIContext ProgressContext { get; set; } = null;

            /// <summary>
            /// On meeting starts
            /// </summary>
            public virtual void OnMeetingStart() { }
            /// <summary>
            /// On meeting ends after exile
            /// </summary>
            public virtual void OnMeetingEnd(PlayerControl exiled = null) { }
            /// <summary>
            /// On fixed update for every player
            /// </summary>
            public virtual void FixedUpdate() { }
            /// <summary>
            /// On killing the target for every player
            /// </summary>
            /// <param name="target">Victim of the kill</param>
            public virtual void OnKill(PlayerControl target) { }
            /// <summary>
            /// On death for every player
            /// </summary>
            /// <param name="killer">The murderer (or null for exile)</param>
            public virtual void OnDeath(PlayerControl killer = null) { }
            public virtual void OnFinishShipStatusBegin() { }
            public virtual void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
            /// <summary>
            /// On role being erased/shifted
            /// </summary>
            /// <param name="isShifted">Determines whether the role is shifted or erased</param>
            public virtual void ResetRole(bool isShifted) { }
            /// <summary>
            /// After being set as the role
            /// </summary>
            public virtual void PostInit() { }

            public static void ClearAll() {
                allRoles = [];
            }
        }

        public abstract class RoleBase<T> : Role where T : RoleBase<T>, new()
        {
            public static List<T> players = [];
            public static RoleId RoleId;

            public void Init(PlayerControl player)
            {
                this.player = player;
                players.Add((T)this);
                allRoles.Add(this);
            }

            public static T local
            {
                get
                {
                    return players.FirstOrDefault(x => x.player == PlayerControl.LocalPlayer);
                }
            }

            public static bool exists
            {
                get { return players.Count > 0; }
            }

            public static bool hasAlivePlayers
            {
                get { return livingPlayers.Count > 0; }
            }

            public static List<PlayerControl> allPlayers
            {
                get
                {
                    return players.Select(x => x.player).Where(x => x != null).ToList();
                }
            }

            public static List<PlayerControl> livingPlayers
            {
                get
                {
                    return players.Select(x => x.player).Where(x => x != null && !x.Data.IsDead).ToList();
                }
            }

            public static T getRole(PlayerControl player = null)
            {
                player ??= PlayerControl.LocalPlayer;
                return players.FirstOrDefault(x => x.player == player);
            }

            public static bool isRole(PlayerControl player)
            {
                return players.Any(x => x.player == player);
            }

            public static void setRole(PlayerControl player)
            {
                if (!isRole(player))
                {
                    T role = new();
                    role.Init(player);
                    role.PostInit();
                    TORGameManager.Instance?.RecordRoleHistory(player);
                }
            }

            public static void eraseRole(PlayerControl player)
            {
                players.DoIf(x => x.player == player, x => x.ResetRole(false));
                players.RemoveAll(x => x.player == player && x.roleId == RoleId);
                allRoles.RemoveAll(x => x.player == player && x.roleId == RoleId);
                TORGameManager.Instance?.RecordRoleHistory(player);
            }

            public static void swapRole(PlayerControl p1, PlayerControl p2)
            {
                var index = players.FindIndex(x => x.player == p1);
                if (index >= 0)
                {
                    players.DoIf(x => x.player == p1, x => x.ResetRole(true));
                    players[index].player = p2;
                    players.DoIf(x => x.player == p2, x => x.PostInit());
                }
            }
        }
    }

    public class Couple
    {
        public PlayerControl lover1;
        public PlayerControl lover2;
        public Color color;

        public Couple(PlayerControl lover1, PlayerControl lover2, Color color)
        {
            this.lover1 = lover1;
            this.lover2 = lover2;
            this.color = color;
            notAckedExiledIsLover = false;
        }

        public bool notAckedExiledIsLover = false;

        public string icon
        {
            get
            {
                return Helpers.cs(color, " ♥");
            }
        }

        public bool existing
        {
            get
            {
                return lover1 != null && lover2 != null && !lover1.Data.Disconnected && !lover2.Data.Disconnected;
            }
        }

        public bool alive
        {
            get
            {
                return lover1 != null && lover2 != null && !lover1.Data.IsDead && !lover2.Data.IsDead;
            }
        }

        public bool existingAndAlive
        {
            get
            {
                return existing && alive;
            }
        }

        public bool existingWithKiller
        {
            get
            {
                return existing && (Helpers.isKiller(lover1) || Helpers.isKiller(lover2)) && !(lover1.isRole(RoleId.Thief)
                || lover2.isRole(RoleId.Thief));
            }
        }

        public bool hasAliveKillingLover
        {
            get
            {
                return existingAndAlive && existingWithKiller;
            }
        }
    }

    public static class Lovers {
        public static List<Couple> couples = [];
        public static Color color = new Color32(232, 57, 185, byte.MaxValue);
        public static List<Color> loverIconColors =
        [
            color,                         // pink
            new Color32(255, 165, 0, 255), // orange
            new Color32(255, 255, 0, 255), // yellow
            new Color32(0, 255, 0, 255),   // green
            new Color32(0, 0, 255, 255),   // blue
            new Color32(0, 255, 255, 255), // light blue
            new Color32(255, 0, 0, 255),   // red
            new Color32(255, 255, 240, 255), // ivory
            new Color32(238, 130, 238, 255), // violet
            new Color32(255, 127, 80, 255), // coral
            new Color32(0, 250, 154, 255), // aquamarine
        ];

        public static string getIcon(PlayerControl player)
        {
            if (isLovers(player))
            {
                var couple = couples.Find(x => x.lover1 == player || x.lover2 == player);
                return couple.icon;
            }
            return "";
        }

        public static Color getColor(PlayerControl player)
        {
            if (isLovers(player))
            {
                var couple = couples.Find(x => x.lover1 == player || x.lover2 == player);
                return couple.color;
            }
            return color;
        }

        public static bool isLovers(PlayerControl player)
        {
            return getCouple(player) != null;
        }

        public static Couple getCouple(PlayerControl player)
        {
            foreach (var pair in couples)
            {
                if (pair.lover1?.PlayerId == player?.PlayerId || pair.lover2?.PlayerId == player?.PlayerId) return pair;
            }
            return null;
        }

        public static PlayerControl getPartner(PlayerControl player)
        {
            var couple = getCouple(player);
            if (couple != null)
            {
                return player?.PlayerId == couple.lover1?.PlayerId ? couple.lover2 : couple.lover1;
            }
            return null;
        }

        public static void killLovers(PlayerControl player, PlayerControl killer = null)
        {
            if (!player.isLovers()) return;

            if (!bothDie) return;

            var partner = getPartner(player);
            if (partner != null)
            {
                if (!partner.Data.IsDead)
                {
                    if (killer != null)
                    {
                        partner.MurderPlayer(partner, MurderResultFlags.Succeeded);
                    }
                    else
                    {
                        if (partner.isRole(RoleId.NekoKabocha))
                            NekoKabocha.getRole(partner).otherKiller = player;
                        if (PlayerControl.LocalPlayer == partner && Helpers.ShowKillAnimation)
                            FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(partner.Data, partner.Data);
                        partner.Exiled();
                    }

                    if (Busker.players.Any(x => x.player == partner && x.pseudocideFlag) && PlayerControl.LocalPlayer == partner)
                    {
                        var busker = Busker.getRole(partner);
                        busker.dieBusker(true);
                    }

                    GameHistory.overrideDeathReasonAndKiller(partner, DeadPlayer.CustomDeathReason.LoverSuicide);
                }
            }
        }

        public static void addCouple(PlayerControl player1, PlayerControl player2)
        {
            var availableColors = new List<Color>(loverIconColors);
            foreach (var couple in couples) {
                availableColors.RemoveAll(x => x == couple.color);
            }
            var newCouple = new Couple(player1, player2, availableColors[0]);
            couples.Add(newCouple);
        }

        public static void eraseCouple(PlayerControl player)
        {
            couples.RemoveAll(x => x.lover1 == player || x.lover2 == player);
        }

        public static void swapLovers(PlayerControl player1, PlayerControl player2)
        {
            var couple1 = couples.FindIndex(x => x.lover1 == player1 || x.lover2 == player1);
            var couple2 = couples.FindIndex(x => x.lover1 == player2 || x.lover2 == player2);

            // trying to swap within the same couple, just ignore
            if (couple1 == couple2) return;

            if (couple1 >= 0)
            {
                if (couples[couple1].lover1 == player1) couples[couple1].lover1 = player2;
                if (couples[couple1].lover2 == player1) couples[couple1].lover2 = player2;
            }

            if (couple2 >= 0)
            {
                if (couples[couple2].lover1 == player2) couples[couple2].lover1 = player1;
                if (couples[couple2].lover2 == player2) couples[couple2].lover2 = player1;
            }
        }

        public static bool anyAlive()
        {
            foreach (var couple in couples)
            {
                if (couple.alive) return true;
            }
            return false;
        }

        public static bool anyNonKillingCouples()
        {
            foreach (var couple in couples)
            {
                if (!couple.hasAliveKillingLover) return true;
            }
            return false;
        }

        public static bool existingAndAlive(PlayerControl player)
        {
            var couple = getCouple(player);
            return couple != null && couple.existingAndAlive && !couple.notAckedExiledIsLover;
        }

        public static bool existingWithKiller(PlayerControl player)
        {
            return getCouple(player)?.existingWithKiller == true;
        }

        public static void HandleDisconnect(PlayerControl player, DisconnectReasons reason)
        {
            eraseCouple(player);
        }

        public static bool bothDie = true;
        public static bool enableChat = true;

        public static bool hasAliveKillingLover(this PlayerControl player) {
            if (!existingAndAlive(player) || !existingWithKiller(player))
                return false;
            return player != null && isLovers(player);
        }

        public static void clearAndReload() {
            bothDie = CustomOptionHolder.modifierLoverBothDie.getBool();
            enableChat = CustomOptionHolder.modifierLoverEnableChat.getBool();
            couples = [];
        }
    }

    public static class CreatedMadmate
    {
        public static List<PlayerControl> createdMadmate = [];

        public static bool canEnterVents;
        public static bool hasImpostorVision;
        public static bool canSabotage;
        public static bool canFixComm;
        public static bool canDieToSheriff;
        public static bool hasTasks;
        public static int numTasks;

        public static bool tasksComplete(PlayerControl player)
        {
            if (!hasTasks) return false;

            int counter = 0;
            int totalTasks = numTasks;
            if (totalTasks == 0) return true;
            foreach (var task in player.Data.Tasks)
            {
                if (task.Complete)
                {
                    counter++;
                }
            }
            return counter >= totalTasks;
        }

        public static void clearAndReload()
        {
            createdMadmate = [];
            canEnterVents = CustomOptionHolder.createdMadmateCanEnterVents.getBool();
            canDieToSheriff = CustomOptionHolder.createdMadmateCanDieToSheriff.getBool();
            hasTasks = CustomOptionHolder.createdMadmateAbility.getBool();
            canSabotage = CustomOptionHolder.createdMadmateCanSabotage.getBool();
            canFixComm = CustomOptionHolder.createdMadmateCanFixComm.getBool();
            numTasks = (int)CustomOptionHolder.createdMadmateCommonTasks.getFloat();
        }
    }

    public static class LastImpostor
    {
        public static PlayerControl lastImpostor;
        public static bool isEnable;
        public static int killCounter;
        public static int maxKillCounter;
        public static bool isOriginalGuesser;
        public static int numShots;
        public static bool hasMultipleShots;

        public static (bool, PlayerControl) doNeedPromotion()
        {
            if (!isEnable || !HandleGuesser.isGuesserGm) return (false, null);

            var impList = new List<PlayerControl>();
            foreach (var p in PlayerControl.AllPlayerControls.GetFastEnumerator())
            {
                if (p.Data.Role.IsImpostor && !p.Data.IsDead && !p.Data.Disconnected) impList.Add(p);
            }

            return (impList.Count == 1, impList.FirstOrDefault());
        }

        public static void promoteToLastImpostor()
        {
            var promoteImp = doNeedPromotion();
            if (!promoteImp.Item1) return;
            RPCProcedure.ImpostorPromotesToLastImpostor.Invoke(promoteImp.Item2?.PlayerId ?? byte.MaxValue);
        }

        public static bool isCounterMax()
        {
            if (maxKillCounter <= killCounter) return true;
            return false;
        }

        public static void clearAndReload()
        {
            lastImpostor = null;
            isEnable = CustomOptionHolder.guesserGamemodeEnableLastImpostor.getBool();
            killCounter = 0;
            maxKillCounter = Mathf.RoundToInt(CustomOptionHolder.guesserGamemodeLastImpostorNumKills.getFloat());
            isOriginalGuesser = false;
            numShots = Mathf.RoundToInt(CustomOptionHolder.guesserGamemodeLastImpostorNumShots.getFloat());
            hasMultipleShots = CustomOptionHolder.guesserGamemodeLastImpostorHasMultipleShots.getBool();
        }
    }

    public static class Armored
    {
        public static PlayerControl armored;

        public static bool isBrokenArmor = false;
        public static void clearAndReload()
        {
            armored = null;
            isBrokenArmor = false;
        }
    }

    public static class Bloody {
        public static List<PlayerControl> bloody = [];

        public static float duration = 5f;

        public static void clearAndReload() {
            bloody = [];
            duration = CustomOptionHolder.modifierBloodyDuration.getFloat();
        }
    }

    public static class AntiTeleport {
        public static List<PlayerControl> antiTeleport = [];
        public static Vector3 position;

        public static void clearAndReload() {
            antiTeleport = [];
            position = Vector3.zero;
        }

        public static void setPosition() {
            if (position == Vector3.zero) return;  // Check if this has been set, otherwise first spawn on submerged will fail
            if (antiTeleport.FindAll(x => x.PlayerId == PlayerControl.LocalPlayer.PlayerId).Count > 0) {
                PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(position);
                if (SubmergedCompatibility.IsSubmerged) {
                    SubmergedCompatibility.ChangeFloor(position.y > -7);
                }
            }
        }
    }

    public static class Tiebreaker {
        public static PlayerControl tiebreaker;

        public static bool isTiebreak = false;

        public static void clearAndReload() {
            tiebreaker = null;
            isTiebreak = false;
        }
    }

    public static class Sunglasses {
        public static List<PlayerControl> sunglasses = [];
        public static int vision = 1;

        public static void clearAndReload() {
            sunglasses = [];
            vision = CustomOptionHolder.modifierSunglassesVision.getSelection() + 1;
        }
    }
    public static class Mini {
        public static PlayerControl mini;
        public static Color color = Color.yellow;
        public const float defaultColliderRadius = 0.2233912f;
        public const float defaultColliderOffset = 0.3636057f;

        public static float growingUpDuration = 400f;
        public static bool isGrowingUpInMeeting = true;
        public static DateTime timeOfGrowthStart = DateTime.UtcNow;
        public static DateTime timeOfMeetingStart = DateTime.UtcNow;
        public static float ageOnMeetingStart = 0f;
        public static bool triggerMiniLose = false;

        public static void clearAndReload() {
            mini = null;
            triggerMiniLose = false;
            growingUpDuration = CustomOptionHolder.modifierMiniGrowingUpDuration.getFloat();
            isGrowingUpInMeeting = CustomOptionHolder.modifierMiniGrowingUpInMeeting.getBool();
            timeOfGrowthStart = DateTime.UtcNow;
        }

        public static float growingProgress() {
            float timeSinceStart = (float)(DateTime.UtcNow - timeOfGrowthStart).TotalMilliseconds;
            return Mathf.Clamp(timeSinceStart / (growingUpDuration * 1000), 0f, 1f);
        }

        public static bool isGrownUp() {
            return growingProgress() == 1f;
        }

    }
    public static class Vip {
        public static List<PlayerControl> vip = [];
        public static bool showColor = true;

        public static void clearAndReload() {
            vip = [];
            showColor = CustomOptionHolder.modifierVipShowColor.getBool();
        }
    }

    public static class Invert {
        public static List<PlayerControl> invert = [];
        public static int meetings = 3;

        public static void clearAndReload() {
            invert = [];
            meetings = (int) CustomOptionHolder.modifierInvertDuration.getFloat();
        }
    }

    public static class Diseased
    {
        public static List<PlayerControl> diseased = [];
        public static Dictionary<byte, int> active = [];
        public static float multiplier;

        public static void clearAndReload()
        {
            diseased = [];
            active = [];
            multiplier = CustomOptionHolder.modifierDiseasedMultiplier.getFloat();
        }
    }

    public static class Radar
    {
        public static PlayerControl radar;
        public static Arrow localArrow;
        public static Color color = new Color32(255, 0, 128, byte.MaxValue);

        public static void clearAndReload()
        {
            radar = null;
            if (localArrow?.arrow != null)
                UnityEngine.Object.Destroy(localArrow.arrow);
        }
    }

    public static class Multitasker
    {
        public static List<PlayerControl> multitasker = [];

        public static void clearAndReload()
        {
            multitasker = [];
        }
    }

    public static class Madmate
    {
        public static Color color = Palette.ImpostorRed;
        public static List<PlayerControl> madmate = [];
        public static bool hasTasks;
        public static bool canDieToSheriff;
        public static bool canVent;
        public static bool hasImpostorVision;
        public static bool canFixComm;
        public static bool canSabotage;
        public static int commonTasks;
        public static int shortTasks;
        public static int longTasks;
        public static RoleId fixedRole;
        
        public static string fullName { get { return ModTranslation.getString("madmate"); } }
        public static string prefix { get { return ModTranslation.getString("madmatePrefix"); } }

        public static List<RoleId> validRoles =
        [
            RoleId.Jester,
            RoleId.Shifter,
            RoleId.Mayor,
            RoleId.Engineer,
            RoleId.Sheriff,
            RoleId.Lighter,
            RoleId.Detective,
            RoleId.TimeMaster,
            RoleId.Medic,
            RoleId.Swapper,
            RoleId.Seer,
            RoleId.Hacker,
            RoleId.Tracker,
            RoleId.SecurityGuard,
            RoleId.Bait,
            RoleId.Medium,
            RoleId.NiceGuesser,
            RoleId.NiceWatcher,
            RoleId.Busker,
            RoleId.Yasuna,
            RoleId.Noisemaker,
            RoleId.Archaeologist
        ];

        public static bool tasksComplete(PlayerControl player)
        {
            if (!hasTasks) return false;

            int counter = 0;
            int totalTasks = commonTasks + longTasks + shortTasks;
            if (totalTasks == 0) return true;
            foreach (var task in player.Data.Tasks)
            {
                if (task.Complete)
                {
                    counter++;
                }
            }
            return counter == totalTasks;
        }

        public static void clearAndReload()
        {
            hasTasks = CustomOptionHolder.madmateAbility.getBool();
            madmate = [];
            canDieToSheriff = CustomOptionHolder.madmateCanDieToSheriff.getBool();
            canVent = CustomOptionHolder.madmateCanEnterVents.getBool();
            hasImpostorVision = CustomOptionHolder.madmateHasImpostorVision.getBool();
            canFixComm = CustomOptionHolder.madmateCanFixComm.getBool();
            canSabotage = CustomOptionHolder.madmateCanSabotage.getBool();
            shortTasks = (int)CustomOptionHolder.madmateShortTasks.getFloat();
            commonTasks = (int)CustomOptionHolder.madmateCommonTasks.getFloat();
            longTasks = (int)CustomOptionHolder.madmateLongTasks.getFloat();
            fixedRole = TORMapOptions.gameMode == CustomGamemodes.Guesser ? validRoles.Where(x => x != RoleId.NiceGuesser).ToList()[
                CustomOptionHolder.madmateFixedRoleGuesserGamemode.getSelection()] :
                validRoles[CustomOptionHolder.madmateFixedRole.getSelection()];
        }
    }

    public static class Chameleon {
        public static List<PlayerControl> chameleon = [];
        public static float minVisibility = 0.2f;
        public static float holdDuration = 1f;
        public static float fadeDuration = 0.5f;
        public static Dictionary<byte, float> lastMoved;

        public static void clearAndReload() {
            chameleon = [];
            lastMoved = [];
            holdDuration = CustomOptionHolder.modifierChameleonHoldDuration.getFloat();
            fadeDuration = CustomOptionHolder.modifierChameleonFadeDuration.getFloat();
            minVisibility = CustomOptionHolder.modifierChameleonMinVisibility.getSelection() / 10f;
        }

        public static float visibility(byte playerId) {
            if (Ninja.players.Any(x => x.player && x.player.PlayerId == playerId && x.stealthed) || Sprinter.players.Any(x => x.player && x.player.PlayerId == playerId && x.sprinting)
                || (Fox.allPlayers.Any(x => x.PlayerId == playerId) && Fox.stealthed) || (Kataomoi.allPlayers.Any(x => x.PlayerId == playerId) && Kataomoi.isStalking())) return 1f;
            float visibility = 1f;
            if (lastMoved != null && lastMoved.ContainsKey(playerId)) {
                var tStill = Time.time - lastMoved[playerId];
                if (tStill > holdDuration) {
                    if (tStill - holdDuration > fadeDuration) visibility = minVisibility;
                    else visibility = (1 - (tStill - holdDuration) / fadeDuration) * (1 - minVisibility) + minVisibility;
                }
            }
            if (PlayerControl.LocalPlayer.Data.IsDead && visibility < 0.1f) {  // Ghosts can always see!
                visibility = 0.1f;
            }
            return visibility;
        }

        public static void update() {
            foreach (var chameleonPlayer in chameleon) {
                //if (chameleonPlayer == Assassin.assassin && Assassin.isInvisble) continue;  // Dont make Assassin visible...
                if (Assassin.players.Any(x => x.player == chameleonPlayer && x.isInvisble) || Ninja.isStealthed(chameleonPlayer) || Sprinter.isSprinting(chameleonPlayer) || (chameleonPlayer.isRole(RoleId.Fox) && Fox.stealthed) ||
                    (chameleonPlayer.isRole(RoleId.Kataomoi) && Kataomoi.isStalking())) continue;
                // check movement by animation
                PlayerPhysics playerPhysics = chameleonPlayer.MyPhysics;
                var currentPhysicsAnim = playerPhysics.Animations.Animator.GetCurrentAnimation();
                if (currentPhysicsAnim != playerPhysics.Animations.group.IdleAnim) {
                    lastMoved[chameleonPlayer.PlayerId] = Time.time;
                }
                // calculate and set visibility
                float visibility = Chameleon.visibility(chameleonPlayer.PlayerId);
                float petVisibility = visibility;
                if (chameleonPlayer.Data.IsDead) {
                    visibility = 0.5f;
                    petVisibility = 1f;
                }

                try {  // Sometimes renderers are missing for weird reasons. Try catch to avoid exceptions
                    chameleonPlayer.cosmetics.currentBodySprite.BodySprite.color = chameleonPlayer.cosmetics.currentBodySprite.BodySprite.color.SetAlpha(visibility);
                    if (DataManager.Settings.Accessibility.ColorBlindMode) chameleonPlayer.cosmetics.colorBlindText.color = chameleonPlayer.cosmetics.colorBlindText.color.SetAlpha(visibility);
                    chameleonPlayer.SetHatAndVisorAlpha(visibility);
                    chameleonPlayer.cosmetics.skin.layer.color = chameleonPlayer.cosmetics.skin.layer.color.SetAlpha(visibility);
                    chameleonPlayer.cosmetics.nameText.color = chameleonPlayer.cosmetics.nameText.color.SetAlpha(visibility);
                    foreach (var rend in chameleonPlayer.cosmetics.currentPet.renderers) rend.color = rend.color.SetAlpha(petVisibility);
                    foreach (var shadowRend in chameleonPlayer.cosmetics.currentPet.shadows) shadowRend.color = shadowRend.color.SetAlpha(petVisibility);

                    //if (chameleonPlayer.cosmetics.skin.layer.color == chameleonPlayer.cosmetics.skin.layer.color.SetAlpha(visibility) && visibility == minVisibility) TheOtherRolesPlugin.Logger.LogMessage("Chameleon");
                    //chameleonPlayer.cosmetics.currentPet.renderers[0].color = chameleonPlayer.cosmetics.currentPet.renderers[0].color.SetAlpha(petVisibility);
                    //chameleonPlayer.cosmetics.currentPet.shadows[0].color = chameleonPlayer.cosmetics.currentPet.shadows[0].color.SetAlpha(petVisibility);
                } catch { }
            }
                
        }

        public static void removeChameleonFully(PlayerControl player) {
            try
            {  // Sometimes renderers are missing for weird reasons. Try catch to avoid exceptions
                player.cosmetics.currentBodySprite.BodySprite.color = player.cosmetics.currentBodySprite.BodySprite.color.SetAlpha(1f);
                if (DataManager.Settings.Accessibility.ColorBlindMode) player.cosmetics.colorBlindText.color = player.cosmetics.colorBlindText.color.SetAlpha(1f);
                player.SetHatAndVisorAlpha(1f);
                player.cosmetics.skin.layer.color = player.cosmetics.skin.layer.color.SetAlpha(1f);
                player.cosmetics.nameText.color = player.cosmetics.nameText.color.SetAlpha(1f);
                foreach (var rend in player.cosmetics.currentPet.renderers) rend.color = rend.color.SetAlpha(1f);
                foreach (var shadowRend in player.cosmetics.currentPet.shadows) shadowRend.color = shadowRend.color.SetAlpha(1f);
                if (lastMoved.ContainsKey(player.PlayerId)) lastMoved.Remove(player.PlayerId);
            }
            catch { }
        }
    }
}
