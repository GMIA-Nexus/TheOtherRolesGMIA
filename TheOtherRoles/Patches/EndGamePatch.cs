using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using TheOtherRoles.CustomGameModes;
using TheOtherRoles.MetaContext;
using TheOtherRoles.Modules;
using TheOtherRoles.Roles;
using TheOtherRoles.Utilities;
using UnityEngine;

namespace TheOtherRoles.Patches {
    enum CustomGameOverReason {
        LoversWin = 10,
        TeamJackalWin = 11,
        MiniLose = 12,
        JesterWin = 13,
        ArsonistWin = 14,
        VultureWin = 15,
        MoriartyWin = 16,
        AkujoWin = 17,
        PlagueDoctorWin = 18,
        JekyllAndHydeWin = 19,
        LawyerSoloWin = 20,
        FoxWin = 21,
        KataomoiWin = 22,
        DoomsayerWin = 23,
        PelicanWin = 24,
        YandereWin = 25
        //ProsecutorWin = 16
    }

    enum WinCondition {
        Default,
        LoversTeamWin,
        LoversSoloWin,
        JesterWin,
        JackalWin,
        MiniLose,
        ArsonistWin,
        VultureWin,
        LawyerSoloWin,
        AdditionalLawyerBonusWin,
        AdditionalAlivePursuerWin,
        AdditionalLawyerStolenWin,
        OpportunistWin,
        FoxWin,
        MoriartyWin,
        AkujoWin,
        PlagueDoctorWin,
        JekyllAndHydeWin,
        KataomoiWin,
        DoomsayerWin,
        PelicanWin,
        YandereWin,
        EveryoneDied
        //ProsecutorWin
    }

    static class AdditionalTempData {
        // Should be implemented using a proper GameOverReason in the future
        public static WinCondition winCondition = WinCondition.Default;
        public static List<WinCondition> additionalWinConditions = new();
        public static List<PlayerRoleInfo> playerRoles = new();
        public static float timer = 0;
        public static Dictionary<int, PlayerControl> plagueDoctorInfected = new();
        public static Dictionary<int, float> plagueDoctorProgress = new();

        public static void clear() {
            playerRoles.Clear();
            additionalWinConditions.Clear();
            winCondition = WinCondition.Default;
            timer = 0;
        }

        internal class PlayerRoleInfo {
            public string PlayerName { get; set; }
            public List<RoleInfo> Roles {get;set;}
            public List<Color> RoleColors { get;set;}
            public string RoleNames { get; set; }
            public int TasksCompleted  {get;set;}
            public int TasksTotal  {get;set;}
            public int ExTasksCompleted { get; set; }
            public int ExTasksTotal { get; set; }
            public bool IsGuesser {get; set;}
            public int? Kills {get; set;}
            public byte PlayerId { get; set;}
            public bool IsAlive { get; set; }
            public bool IsMadmate { get; set;}
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    public static class OnGameEndPatch
    {
        public static GameOverReason gameOverReason = GameOverReason.CrewmatesByTask;
        public static void Prefix(AmongUsClient __instance, [HarmonyArgument(0)]ref EndGameResult endGameResult) {
            gameOverReason = endGameResult.GameOverReason;
            if ((int)endGameResult.GameOverReason >= 10) endGameResult.GameOverReason = GameOverReason.ImpostorsByKill;

            // Reset zoomed out ghosts
            Helpers.toggleZoom(reset: true);

            TORGameManager.Instance?.GameStatistics.RecordEvent(new(GameStatistics.EventVariation.GameEnd, null, 0) { RelatedTag = EventDetail.GameEnd });
        }

        public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)]ref EndGameResult endGameResult) {
            bool foxTaskComplete = Fox.isFoxCompletedTasks();
            if (foxTaskComplete)
            {
                if (gameOverReason == GameOverReason.CrewmatesByTask && !Fox.crewWinsByTasks)
                {
                    gameOverReason = (GameOverReason)CustomGameOverReason.FoxWin;
                }
                else if (gameOverReason == GameOverReason.ImpostorsBySabotage && !Fox.impostorWinsBySabotage)
                {
                    gameOverReason = (GameOverReason)CustomGameOverReason.FoxWin;
                }
                else if (gameOverReason is GameOverReason.CrewmatesByVote or GameOverReason.ImpostorsByKill or GameOverReason.ImpostorsByVote)
                {
                    gameOverReason = (GameOverReason)CustomGameOverReason.FoxWin;
                }
            }

            AdditionalTempData.clear();

            foreach(PlayerControl playerControl in PlayerControl.AllPlayerControls) {
                var roles = RoleInfo.getRoleInfoForPlayer(playerControl, true, true);
                var colors = new List<Color>();
                foreach (var role in roles) {
                    if (role.roleId == RoleId.Lover) {
                        colors.Add(Lovers.getColor(playerControl));
                        continue;
                    }
                    colors.Add(role.color);
                }
                var (tasksCompleted, tasksTotal) = TasksHandler.taskInfo(playerControl.Data);
                bool isGuesser = HandleGuesser.isGuesserGm && HandleGuesser.isGuesser(playerControl.PlayerId);
                bool isMadmate = Madmate.madmate.Any(x => x.PlayerId ==  playerControl.PlayerId) || CreatedMadmate.createdMadmate.Any(x => x.PlayerId == playerControl.PlayerId);
                int? killCount = GameHistory.deadPlayers.FindAll(x => x.killerIfExisting != null && x.killerIfExisting.PlayerId == playerControl.PlayerId).Count;
                if (killCount == 0 && !(new List<RoleInfo>() { RoleInfo.sheriff, RoleInfo.jackal, RoleInfo.sidekick, RoleInfo.thief, RoleInfo.moriarty, RoleInfo.jekyllAndHyde, RoleInfo.doomsayer,
                RoleInfo.pelican, RoleInfo.yandere }.Contains(RoleInfo.getRoleInfoForPlayer(playerControl, false).FirstOrDefault()) || playerControl.Data.Role.IsImpostor)) {
                    killCount = null;
                    }
                byte playerId = playerControl.PlayerId;
                bool isTaskMaster = playerControl.isRole(RoleId.TaskMaster);
                bool isTaskMasterExTasks = isTaskMaster && TaskMaster.isTaskComplete;
                string roleString = RoleInfo.GetRolesString(playerControl, true, true, false, true, [RoleId.Lover] );
                var entries = TORGameManager.Instance.RoleHistory.Where(x => x.PlayerId == playerId).ToArray();
                List<RoleHistory> history = [];
                foreach (var item in entries)
                {
                    if (history.Count == 0)
                    {
                        history.Add(item);
                        continue;
                    }
                    var last = history[^1];
                    if (item.Time - last.Time < 1f) continue;
                    if (last.RoleInfo.roleId == item.RoleInfo.roleId && Helpers.isSpecialRoleInfo(last.RoleInfo) == Helpers.isSpecialRoleInfo(item.RoleInfo) && last.IsMadmate == item.IsMadmate) continue;
                    history.Add(item);
                }
                string extraInfo = "";
                if (history.Count < 4)
                {
                    for (int i = 0; i < history.Count - 1; i++)
                    {
                        var entryRole = history[i].RoleInfo;
                        extraInfo += Helpers.cs(history[i].IsMadmate ? Palette.ImpostorRed : history[i].Color, history[i].IsMadmate ?
                            (entryRole.roleId == RoleId.Crewmate ? Madmate.fullName : Madmate.prefix + entryRole.name) : entryRole.name) + " → ";
                    }
                }
                else
                {
                    extraInfo = Helpers.cs(history[0].IsMadmate ? Palette.ImpostorRed : history[0].Color, history[0].IsMadmate ?
                            (history[0].RoleInfo.roleId == RoleId.Crewmate ? Madmate.fullName : Madmate.prefix + history[0].RoleInfo.name) : history[0].RoleInfo.name) + " → ...";
                }
                roleString = extraInfo + roleString;
                AdditionalTempData.playerRoles.Add(new AdditionalTempData.PlayerRoleInfo() { PlayerName = playerControl.Data.PlayerName, Roles = roles, RoleColors = colors, RoleNames = roleString ,TasksTotal = tasksTotal, TasksCompleted = tasksCompleted,
                    ExTasksTotal = isTaskMasterExTasks ? TaskMaster.allExTasks : isTaskMaster ? TaskMasterTaskHelper.GetTaskMasterTasks() : 0,
                    ExTasksCompleted = isTaskMasterExTasks ? TaskMaster.clearExTasks : 0,
                    IsGuesser = isGuesser, Kills = killCount, PlayerId = playerId, IsAlive = !playerControl.Data.IsDead, IsMadmate = isMadmate });
            }
            AdditionalTempData.plagueDoctorInfected = PlagueDoctor.infected;
            AdditionalTempData.plagueDoctorProgress = PlagueDoctor.progress;

            // Remove Jester, Opportunist, Arsonist, Vulture, Jackal, former Jackals and Sidekick from winners (if they win, they'll be readded)
            List<PlayerControl> notWinners =
            [
                .. Jester.allPlayers,
                .. Sidekick.allPlayers,
                .. Jackal.allPlayers,
                .. Arsonist.allPlayers,
                .. Vulture.allPlayers,
                .. Lawyer.allPlayers,
                .. Pursuer.allPlayers,
                .. Kataomoi.allPlayers,
                .. JekyllAndHyde.allPlayers,
                .. Madmate.madmate,
                .. SchrodingersCat.allPlayers,
                .. CreatedMadmate.createdMadmate,
                .. Moriarty.allPlayers,
                .. Akujo.allPlayers,
                .. Opportunist.allPlayers,
                .. Thief.allPlayers,
                .. Doomsayer.allPlayers,
                .. Cupid.allPlayers,
                .. PlagueDoctor.allPlayers,
                .. Fox.allPlayers,
                .. Immoralist.allPlayers,
                .. Pelican.allPlayers,
                .. Yandere.allPlayers
            ];
            if (Shifter.isNeutral) notWinners.AddRange(Shifter.allPlayers);

            List<CachedPlayerData> winnersToRemove = [];
            foreach (CachedPlayerData winner in EndGameResult.CachedWinners.GetFastEnumerator()) {
                if (notWinners.Any(x => x.Data.PlayerName == winner.PlayerName)) winnersToRemove.Add(winner);
            }
            foreach (var winner in winnersToRemove) EndGameResult.CachedWinners.Remove(winner);

            // Putting them all in one doesn't work
            bool saboWin = gameOverReason == GameOverReason.ImpostorsBySabotage;
            bool impostorWin = gameOverReason is GameOverReason.ImpostorsByKill or GameOverReason.ImpostorsBySabotage or GameOverReason.ImpostorDisconnect or GameOverReason.ImpostorsByVote;
            bool crewmateWin = gameOverReason is GameOverReason.CrewmatesByTask or GameOverReason.CrewmatesByVote or GameOverReason.CrewmateDisconnect;

            bool jesterWin = Jester.exists && gameOverReason == (GameOverReason)CustomGameOverReason.JesterWin;
            bool arsonistWin = Arsonist.exists && gameOverReason == (GameOverReason)CustomGameOverReason.ArsonistWin;
            bool miniLose = Mini.mini != null && gameOverReason == (GameOverReason)CustomGameOverReason.MiniLose;
            bool loversWin = Lovers.anyAlive() && (gameOverReason == (GameOverReason)CustomGameOverReason.LoversWin || (GameManager.Instance.DidHumansWin(gameOverReason) && Lovers.anyNonKillingCouples())); // Either they win if they are among the last 3 players, or they win if they are both Crewmates and both alive and the Crew wins (Team Imp/Jackal Lovers can only win solo wins)
            bool teamJackalWin = gameOverReason == (GameOverReason)CustomGameOverReason.TeamJackalWin;
            /* && ((Jackal.jackal != null && !Jackal.jackal.Data.IsDead) || (Sidekick.sidekick != null && !Sidekick.sidekick.Data.IsDead) ||
                (SchrodingersCat.schrodingersCat != null && SchrodingersCat.team == SchrodingersCat.Team.Jackal && !SchrodingersCat.schrodingersCat.Data.IsDead));*/
            bool vultureWin = Vulture.exists && gameOverReason == (GameOverReason)CustomGameOverReason.VultureWin;
            bool lawyerSoloWin = Lawyer.exists && gameOverReason == (GameOverReason)CustomGameOverReason.LawyerSoloWin;
            bool moriartyWin = Moriarty.exists && gameOverReason == (GameOverReason)CustomGameOverReason.MoriartyWin;
            bool akujoWin = Akujo.numAlive > 0 && gameOverReason != GameOverReason.CrewmatesByTask;
            bool plagueDoctorWin = PlagueDoctor.exists && gameOverReason == (GameOverReason)CustomGameOverReason.PlagueDoctorWin;
            bool kataomoiWin = Kataomoi.exists && gameOverReason == (GameOverReason)CustomGameOverReason.KataomoiWin;
            bool doomsayerWin = Doomsayer.exists && gameOverReason == (GameOverReason)CustomGameOverReason.DoomsayerWin;
            bool pelicanWin = Pelican.exists && gameOverReason == (GameOverReason)CustomGameOverReason.PelicanWin;
            bool yandereWin = Yandere.exists && gameOverReason == (GameOverReason)CustomGameOverReason.YandereWin;
            bool foxWin = Fox.exists && gameOverReason == (GameOverReason)CustomGameOverReason.FoxWin;
            bool jekyllAndHydeWin = JekyllAndHyde.exists && gameOverReason == (GameOverReason)CustomGameOverReason.JekyllAndHydeWin;
            bool everyoneDead = AdditionalTempData.playerRoles.All(x => !x.IsAlive);
            //bool prosecutorWin = Lawyer.lawyer != null && gameOverReason == (GameOverReason)CustomGameOverReason.ProsecutorWin;

            // Here we changed this to: The Pursuer wins no matter who wins except for sabotage
            //bool isPursurerLose = jesterWin || arsonistWin || miniLose || vultureWin || teamJackalWin;

            // Crewmates Win
            if (crewmateWin)
            {
                if (SchrodingersCat.team == SchrodingersCat.Team.Crewmate)
                {
                    foreach (var p in SchrodingersCat.allPlayers)
                        EndGameResult.CachedWinners.Add(new(p.Data));
                }
            }

            // Impostors Win
            if (impostorWin)
            {
                if (SchrodingersCat.team == SchrodingersCat.Team.Impostor)
                {
                    foreach (var p in SchrodingersCat.allPlayers)
                        EndGameResult.CachedWinners.Add(new CachedPlayerData(p.Data));
                }
            }

            // Mini lose
            if (miniLose) {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                CachedPlayerData wpd = new(Mini.mini.Data)
                {
                    IsYou = false // If "no one is the Mini", it will display the Mini, but also show defeat to everyone
                };
                EndGameResult.CachedWinners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.MiniLose;  
            }

            // Jester win
            else if (jesterWin) {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                foreach (var p in Jester.players)
                {
                    if (!p.triggerJesterWin || p.player == null) continue;
                    if (PlayerControl.LocalPlayer == p.player) _ = new StaticAchievementToken("jester.challenge");
                    CachedPlayerData wpd = new(p.player.Data);
                    EndGameResult.CachedWinners.Add(wpd);
                }
                AdditionalTempData.winCondition = WinCondition.JesterWin;
            }

            // Arsonist win
            else if (arsonistWin) {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                foreach (var p in Arsonist.players)
                {
                    if (!p.triggerArsonistWin) continue;
                    CachedPlayerData wpd = new(p.player.Data);
                    EndGameResult.CachedWinners.Add(wpd);
                }
                AdditionalTempData.winCondition = WinCondition.ArsonistWin;
            }

            // Kataomoi win
            else if (kataomoiWin)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                foreach (PlayerControl kataomoi in Kataomoi.allPlayers)
                {
                    CachedPlayerData wpd = new(kataomoi.Data);
                    EndGameResult.CachedWinners.Add(wpd);
                }
                AdditionalTempData.winCondition = WinCondition.KataomoiWin;
            }

            else if (plagueDoctorWin)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                AdditionalTempData.winCondition = WinCondition.PlagueDoctorWin;
                foreach (var plagueDoctor in PlagueDoctor.allPlayers)
                {
                    if (PlayerControl.LocalPlayer == plagueDoctor) _ = new StaticAchievementToken("plagueDoctor.challenge");
                    CachedPlayerData wpd = new(plagueDoctor.Data);
                    EndGameResult.CachedWinners.Add(wpd);
                }
            }

            else if (doomsayerWin)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                AdditionalTempData.winCondition = WinCondition.DoomsayerWin;
                foreach (var doomsayer in Doomsayer.players)
                {
                    if (!doomsayer.triggerWin || doomsayer.player == null) continue;
                    if (PlayerControl.LocalPlayer == doomsayer.player) _ = new StaticAchievementToken("doomsayer.challenge");
                    EndGameResult.CachedWinners.Add(new(doomsayer.player.Data));
                }
            }

            else if (pelicanWin)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                AdditionalTempData.winCondition = WinCondition.PelicanWin;
                foreach (var pelican in Pelican.allPlayers)
                {
                    if (PlayerControl.LocalPlayer == pelican) _ = new StaticAchievementToken("pelican.challenge");
                    EndGameResult.CachedWinners.Add(new(pelican.Data));
                }
            }

            // Everyone Died
            else if (everyoneDead)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                AdditionalTempData.winCondition = WinCondition.EveryoneDied;
            }

            else if (jekyllAndHydeWin)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                AdditionalTempData.winCondition = WinCondition.JekyllAndHydeWin;

                foreach (var jekyllAndHyde in JekyllAndHyde.allPlayers)
                {
                    CachedPlayerData wpd = new(jekyllAndHyde.Data);
                    EndGameResult.CachedWinners.Add(wpd);
                    if (PlayerControl.LocalPlayer == jekyllAndHyde) _ = new StaticAchievementToken("jekyllAndHyde.challenge");
                }

                if (SchrodingersCat.team == SchrodingersCat.Team.JekyllAndHyde)
                {
                    foreach (var p in SchrodingersCat.allPlayers)
                        EndGameResult.CachedWinners.Add(new CachedPlayerData(p.Data));
                }
            }

            else if (moriartyWin)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                AdditionalTempData.winCondition = WinCondition.MoriartyWin;

                foreach (var moriarty in Moriarty.allPlayers)
                {
                    CachedPlayerData wpd = new(moriarty.Data);
                    EndGameResult.CachedWinners.Add(wpd);
                    if (PlayerControl.LocalPlayer == moriarty) _ = new StaticAchievementToken("moriarty.challenge");
                }

                if (SchrodingersCat.team == SchrodingersCat.Team.Moriarty)
                {
                    foreach (var p in SchrodingersCat.allPlayers)
                        EndGameResult.CachedWinners.Add(new CachedPlayerData(p.Data));
                }
            }

            // Vulture win
            else if (vultureWin) {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                foreach (var vulture in Vulture.players)
                {
                    if (!vulture.triggerVultureWin) continue;
                    CachedPlayerData wpd = new(vulture.player.Data);
                    EndGameResult.CachedWinners.Add(wpd);
                    if (PlayerControl.LocalPlayer == vulture.player) _ = new StaticAchievementToken("vulture.challenge");
                }
                AdditionalTempData.winCondition = WinCondition.VultureWin;
            }

            // Akujo win
            else if (akujoWin)
            {
                AdditionalTempData.winCondition = WinCondition.AkujoWin;
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                foreach (var akujo in Akujo.players)
                {
                    if (akujo.player && !akujo.player.Data.IsDead)
                    {
                        EndGameResult.CachedWinners.Add(new(akujo.player.Data));
                        if (PlayerControl.LocalPlayer == akujo.player) _ = new StaticAchievementToken("akujo.challenge");

                        if (akujo.honmei && !akujo.honmei.Data.IsDead)
                            EndGameResult.CachedWinners.Add(new(akujo.honmei.Data));
                        if (akujo.cupidHonmei && !akujo.cupidHonmei.Data.IsDead)
                            EndGameResult.CachedWinners.Add(new(akujo.cupidHonmei.Data));
                    }
                }

                foreach (var p in Cupid.players)
                {
                    if (p.player == null || p.player.Data.IsDead) continue;
                    if (((p.lovers1 != null && p.lovers1.isRole(RoleId.Akujo)) || (p.lovers2 != null && p.lovers2.isRole(RoleId.Akujo)))
                        && !EndGameResult.CachedWinners.ToArray().Any(x => x.PlayerName == p.player.Data.PlayerName))
                    {
                        CachedPlayerData wpd = new(p.player.Data);
                        EndGameResult.CachedWinners.Add(wpd);
                    }
                }
            }            

            // Jester win
            /*else if (prosecutorWin) {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                CachedPlayerData wpd = new CachedPlayerData(Lawyer.lawyer.Data);
                EndGameResult.CachedWinners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.ProsecutorWin;
            }*/

            // Lovers win conditions
            else if (loversWin) {
                // Double win for lovers, crewmates also win
                if (GameManager.Instance.DidHumansWin(gameOverReason) && Lovers.anyNonKillingCouples()) {
                    AdditionalTempData.winCondition = WinCondition.LoversTeamWin;
                    EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls) {
                        if (p == null) continue;
                        if (p.isLovers() && !Lovers.existingWithKiller(p))
                            EndGameResult.CachedWinners.Add(new CachedPlayerData(p.Data));
                        else if (p.isRole(RoleId.Pursuer) && !p.Data.IsDead)
                            EndGameResult.CachedWinners.Add(new CachedPlayerData(p.Data));
                        else if (!p.isRole(RoleId.Jester) && !p.isRole(RoleId.Jackal) && !p.isRole(RoleId.Sidekick) && !p.isRole(RoleId.Arsonist) && !p.isRole(RoleId.Vulture) && !p.Data.Role.IsImpostor && !p.isRole(RoleId.Moriarty) && !p.isRole(RoleId.Akujo) && !p.isRole(RoleId.JekyllAndHyde)
                            && !p.isRole(RoleId.Cupid) && !p.isRole(RoleId.PlagueDoctor) && !Madmate.madmate.Contains(p) && !CreatedMadmate.createdMadmate.Contains(p) && !p.isRole(RoleId.Fox) && !p.isRole(RoleId.Immoralist))
                            EndGameResult.CachedWinners.Add(new CachedPlayerData(p.Data));
                    }
                }
                // Lovers solo win
                else {
                    AdditionalTempData.winCondition = WinCondition.LoversSoloWin;
                    EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                    foreach (var couple in Lovers.couples)
                    {
                        if (couple.existingAndAlive)
                        {
                            EndGameResult.CachedWinners.Add(new CachedPlayerData(couple.lover1.Data));
                            EndGameResult.CachedWinners.Add(new CachedPlayerData(couple.lover2.Data));
                            if ((PlayerControl.LocalPlayer == couple.lover1 || PlayerControl.LocalPlayer == couple.lover2) && Helpers.CurrentMonth == 2)
                                _ = new StaticAchievementToken("valentines");
                        }
                    }
                }

                foreach (var cupid in Cupid.players)
                {
                    if (cupid.player == null) continue;
                    if (cupid.lovers1 != null & !cupid.lovers1.Data.IsDead && cupid.lovers2 != null && !cupid.lovers2.Data.IsDead
                        && !EndGameResult.CachedWinners.ToArray().Any(x => x.PlayerName == cupid.player.Data.PlayerName))
                    {
                        CachedPlayerData wpd = new(cupid.player.Data);
                        EndGameResult.CachedWinners.Add(wpd);
                        if (PlayerControl.LocalPlayer == cupid.player) _ = new StaticAchievementToken("cupid.challenge");
                    }
                }
            }

            else if (yandereWin)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                AdditionalTempData.winCondition = WinCondition.YandereWin;

                foreach (var yandere in Yandere.allPlayers)
                {
                    CachedPlayerData wpd = new(yandere.Data);
                    EndGameResult.CachedWinners.Add(wpd);
                    if (PlayerControl.LocalPlayer == yandere) _ = new StaticAchievementToken("yandere.challenge");
                }

                if (Yandere.target != null) {
                    EndGameResult.CachedWinners.Add(new(Yandere.target.Data));
                }

                if (SchrodingersCat.team == SchrodingersCat.Team.Yandere)
                {
                    foreach (var p in SchrodingersCat.allPlayers)
                        EndGameResult.CachedWinners.Add(new CachedPlayerData(p.Data));
                }
            }

            // Jackal win condition (should be implemented using a proper GameOverReason in the future)
            else if (teamJackalWin) {
                // Jackal wins if nobody except jackal is alive
                AdditionalTempData.winCondition = WinCondition.JackalWin;
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                if (Jackal.exists)
                {
                    foreach (var jackal in Jackal.allPlayers) {
                        CachedPlayerData wpd = new(jackal.Data)
                        {
                            IsImpostor = false
                        };
                        EndGameResult.CachedWinners.Add(wpd);
                    }
                    if (PlayerControl.LocalPlayer.isRole(RoleId.Jackal))
                    {
                        if (GameHistory.deadPlayers != null && GameHistory.deadPlayers.Count > 0)
                        {
                            var lastDead = GameHistory.deadPlayers?.MinBy(p => DateTime.UtcNow.Subtract(p.timeOfDeath).TotalSeconds);
                            if (lastDead.player?.Data.Role.IsImpostor == true && lastDead.killerIfExisting == PlayerControl.LocalPlayer) _ = new StaticAchievementToken("jackal.challenge");
                        }
                        if (Jackal.local.canCreateSidekick) _ = new StaticAchievementToken("jackal.another1");
                    }
                }
                // If there is a sidekick. The sidekick also wins
                foreach (var sidekick in Sidekick.allPlayers) {
                    CachedPlayerData wpdSidekick = new(sidekick.Data)
                    {
                        IsImpostor = false
                    };
                    EndGameResult.CachedWinners.Add(wpdSidekick);
                }
                if (SchrodingersCat.team == SchrodingersCat.Team.Jackal)
                {
                    foreach (var p in SchrodingersCat.allPlayers)
                        EndGameResult.CachedWinners.Add(new CachedPlayerData(p.Data) { IsImpostor = false });
                }
            }

            else if (lawyerSoloWin)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                foreach (var p in Lawyer.allPlayers)
                {
                    if (PlayerControl.LocalPlayer == p) _ = new StaticAchievementToken("lawyer.challenge1");
                    CachedPlayerData wpd = new(p.Data);
                    EndGameResult.CachedWinners.Add(wpd);
                }
                AdditionalTempData.winCondition = WinCondition.LawyerSoloWin;
            }

            else if (foxWin)
            {
                AdditionalTempData.winCondition = WinCondition.FoxWin;
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();

                foreach (var fox in Fox.livingPlayers)
                {
                    if (PlayerControl.LocalPlayer == fox)
                        _ = new StaticAchievementToken("fox.challenge");
                    CachedPlayerData wpd = new(fox.Data);
                    EndGameResult.CachedWinners.Add(wpd);
                }

                foreach (var immoralist in Immoralist.allPlayers)
                {
                    CachedPlayerData wpdI = new(immoralist.Data);
                    EndGameResult.CachedWinners.Add(wpdI);
                }
            }

            // Madmate win with impostors
            if (EndGameResult.CachedWinners.ToArray().Any(x => x.IsImpostor))
            {
                if (Madmate.madmate != null)
                {
                    foreach (var p in Madmate.madmate)
                    {
                        if (EndGameResult.CachedWinners.ToArray().Any(x => x.PlayerName == p.Data.PlayerName)) continue;
                        CachedPlayerData wpd = new(p.Data);
                        EndGameResult.CachedWinners.Add(wpd);
                    }
                }
                if (CreatedMadmate.createdMadmate != null)
                {
                    foreach (var p in CreatedMadmate.createdMadmate)
                    {
                        if (EndGameResult.CachedWinners.ToArray().Any(x => x.PlayerName == p.Data.PlayerName)) continue;
                        CachedPlayerData wpd = new(p.Data);
                        EndGameResult.CachedWinners.Add(wpd);
                    }
                }
            }

            // Possible Additional winner: Lawyer
            // && !Lawyer.isProsecutor
            if (Lawyer.target != null && (!Lawyer.target.Data.IsDead || Lawyer.target.isRole(RoleId.Jester))) {
                CachedPlayerData winningClient = null;
                foreach (CachedPlayerData winner in EndGameResult.CachedWinners.GetFastEnumerator()) {
                    if (winner.PlayerName == Lawyer.target.Data.PlayerName)
                        winningClient = winner;
                }
                if (winningClient != null) { // The Lawyer wins if the client is winning (and alive, but if he wasn't the Lawyer shouldn't exist anymore)
                    if (!EndGameResult.CachedWinners.ToArray().Any(x => Lawyer.allPlayers.Any(lawyer => lawyer.Data.PlayerName == x.PlayerName)))
                    {
                        if (Lawyer.hasAlivePlayers)
                        {
                            EndGameResult.CachedWinners.Remove(winningClient);
                            foreach (var p in Lawyer.allPlayers) {
                                if (PlayerControl.LocalPlayer == p) _ = new StaticAchievementToken("lawyer.challenge2");
                                CachedPlayerData wpd = new(p.Data);
                                EndGameResult.CachedWinners.Add(wpd);
                            }
                            AdditionalTempData.additionalWinConditions.Add(WinCondition.AdditionalLawyerStolenWin); // The Lawyer replaces the client's victory
                        }
                        else
                        {
                            foreach (var p in Lawyer.allPlayers) {
                                CachedPlayerData wpd = new(p.Data);
                                EndGameResult.CachedWinners.Add(wpd);
                            }
                            AdditionalTempData.additionalWinConditions.Add(WinCondition.AdditionalLawyerBonusWin); // The Lawyer wins with the client
                        }                       
                    }                    
                } 
            }

            // Possible Additional winner: Pursuer
            if (!saboWin && !arsonistWin && !miniLose) {
                foreach (var pursuer in Pursuer.players)
                {
                    if (pursuer.player != null && !pursuer.player.Data.IsDead && !pursuer.notAckedExiled)
                    {
                        if (!EndGameResult.CachedWinners.ToArray().Any(x => x.PlayerName == pursuer.player.Data.PlayerName))
                            EndGameResult.CachedWinners.Add(new CachedPlayerData(pursuer.player.Data));
                        AdditionalTempData.additionalWinConditions.Add(WinCondition.AdditionalAlivePursuerWin);
                        if (PlayerControl.LocalPlayer == pursuer.player) _ = new StaticAchievementToken("pursuer.common2");
                    }
                }

                bool oppWin = false;
                foreach (var p in Opportunist.players)
                {
                    if (p.player == null || p.player.Data.IsDead || p.notAckedExile) continue;
                    if (!EndGameResult.CachedWinners.ToArray().Any(x => x.PlayerName == p.player.Data.PlayerName)) {
                        if (PlayerControl.LocalPlayer == p.player)
                        {
                            _ = new StaticAchievementToken("opportunist.common1");
                            _ = new StaticAchievementToken("opportunist.challenge");
                        }
                        EndGameResult.CachedWinners.Add(new CachedPlayerData(p.player.Data));
                    }
                    oppWin = true;
                }
                if (oppWin)
                    AdditionalTempData.additionalWinConditions.Add(WinCondition.OpportunistWin);
            }

            if (PlayerControl.LocalPlayer.isRole(RoleId.Lighter) && !PlayerControl.LocalPlayer.Data.IsDead)
                _ = new StaticAchievementToken("lighter.common1");

            if (PlayerControl.LocalPlayer.isRole(RoleId.Shifter) && Shifter.isNeutral &&
                !EndGameResult.CachedWinners.ToArray().Any(x => x.PlayerName == PlayerControl.LocalPlayer.Data.PlayerName))
                _ = new StaticAchievementToken("corruptedShifter.another1");

            if (EndGameResult.CachedWinners.ToArray().Any(x => x.PlayerName == PlayerControl.LocalPlayer.Data.PlayerName))
            {
                if (Helpers.CurrentMonth == 1 && Helpers.isPolus())
                    _ = new StaticAchievementToken("dancingSnow");
                else if (Helpers.CurrentMonth == 5 && Helpers.isFungle())
                    _ = new StaticAchievementToken("blossoms");
                else if (Helpers.CurrentMonth == 9 && crewmateWin)
                    _ = new StaticAchievementToken("autumnSky");
                else if (Helpers.CurrentMonth == 12 && Helpers.isPolus())
                    _ = new StaticAchievementToken("christmas");
            }
                

            AdditionalTempData.timer = ((float)(DateTime.UtcNow - HideNSeek.startTime).TotalMilliseconds) / 1000;

            // Reset Settings
            if (TORMapOptions.gameMode == CustomGamemodes.HideNSeek) ShipStatusPatch.resetVanillaSettings();
            RPCProcedure.resetVariables();
            EventUtility.gameEndsUpdate();
        }
    }

    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
    public class EndGameManagerSetUpPatch {
        public static void Postfix(EndGameManager __instance) {
            // Delete and readd PoolablePlayers always showing the name and role of the player
            foreach (PoolablePlayer pb in __instance.transform.GetComponentsInChildren<PoolablePlayer>()) {
                UnityEngine.Object.Destroy(pb.gameObject);
            }
            int num = Mathf.CeilToInt(7.5f);
            List<CachedPlayerData> list = EndGameResult.CachedWinners.ToArray().ToList().OrderBy(delegate(CachedPlayerData b)
            {
                if (!b.IsYou)
                {
                    return 0;
                }
                return -1;
            }).ToList();
            for (int i = 0; i < list.Count; i++) {
                CachedPlayerData CachedPlayerData2 = list[i];
                int num2 = (i % 2 == 0) ? -1 : 1;
                int num3 = (i + 1) / 2;
                float num4 = (float)num3 / (float)num;
                float num5 = Mathf.Lerp(1f, 0.75f, num4);
                float num6 = (float)((i == 0) ? -8 : -1);
                PoolablePlayer poolablePlayer = UnityEngine.Object.Instantiate<PoolablePlayer>(__instance.PlayerPrefab, __instance.transform);
                poolablePlayer.transform.localPosition = new Vector3(1f * (float)num2 * (float)num3 * num5, FloatRange.SpreadToEdges(-1.125f, 0f, num3, num), num6 + (float)num3 * 0.01f) * 0.9f;
                float num7 = Mathf.Lerp(1f, 0.65f, num4) * 0.9f;
                Vector3 vector = new(num7, num7, 1f);
                poolablePlayer.transform.localScale = vector;
                if (CachedPlayerData2.IsDead)
                {
                    poolablePlayer.SetBodyAsGhost();
                    poolablePlayer.SetDeadFlipX(i % 2 == 0);
                }
                else {
                    poolablePlayer.SetFlipX(i % 2 == 0);
                }

                poolablePlayer.UpdateFromPlayerOutfit(CachedPlayerData2.Outfit, PlayerMaterial.MaskType.None, CachedPlayerData2.IsDead, true);

                poolablePlayer.cosmetics.nameText.color = Color.white;
                poolablePlayer.cosmetics.nameText.transform.localScale = new Vector3(1f / vector.x, 1f / vector.y, 1f / vector.z);
                poolablePlayer.cosmetics.nameText.transform.localPosition = new Vector3(poolablePlayer.cosmetics.nameText.transform.localPosition.x, poolablePlayer.cosmetics.nameText.transform.localPosition.y, -15f);
                poolablePlayer.cosmetics.nameText.text = CachedPlayerData2.PlayerName;

                foreach (var data in AdditionalTempData.playerRoles) {
                    if (data.PlayerName != CachedPlayerData2.PlayerName) continue;
                    var roles =
                    //poolablePlayer.cosmetics.nameText.text += $"\n{string.Join("\n", data.Roles.Select(x => Helpers.cs(x.color, x.name)))}";
                    poolablePlayer.cosmetics.nameText.text += $"\n{string.Join("\n", data.Roles.Select(x => Helpers.cs((data.IsMadmate && !x.isModifier) ? Madmate.color : data.RoleColors[data.Roles.IndexOf(x)], 
                    (data.IsMadmate && !x.isModifier) ? (x == RoleInfo.crewmate ? Madmate.fullName : (Madmate.prefix + x.name)) : x.name)))}";
                }
            }

            TORGUIManager.Instance.StartCoroutine(TORAchievementManager.CoShowAchievements(TORGUIManager.Instance, TORAchievementManager.UniteAll()).WrapToIl2Cpp());
            IEnumerator CoShowStatistics()
            {
                yield return new WaitForSeconds(0.4f);
                var viewer = Helpers.CreateObject<GameStatisticsViewer>("Statistics", __instance.transform, new Vector3(0f, 2.5f, -20f), LayerMask.NameToLayer("UI"));
                viewer.PlayerPrefab = __instance.PlayerPrefab;
                viewer.GameEndText = __instance.WinText;
            }
            __instance.StartCoroutine(CoShowStatistics().WrapToIl2Cpp());

            // Additional code
            GameObject bonusText = UnityEngine.Object.Instantiate(__instance.WinText.gameObject);
            bonusText.transform.position = new Vector3(__instance.WinText.transform.position.x, __instance.WinText.transform.position.y - 0.5f, __instance.WinText.transform.position.z);
            bonusText.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            TMPro.TMP_Text textRenderer = bonusText.GetComponent<TMPro.TMP_Text>();
            textRenderer.text = "";
            string nonModTranslationText = "";

            if (AdditionalTempData.winCondition == WinCondition.JesterWin)
            {
                nonModTranslationText = "jesterWin";
                textRenderer.color = Jester.color;
                __instance.BackgroundBar.material.SetColor("_Color", Jester.color);
            }
            else if (AdditionalTempData.winCondition == WinCondition.ArsonistWin)
            {
                nonModTranslationText = "arsonistWin";
                textRenderer.color = Arsonist.color;
                __instance.BackgroundBar.material.SetColor("_Color", Arsonist.color);
            }
            else if (AdditionalTempData.winCondition == WinCondition.KataomoiWin)
            {
                nonModTranslationText = "kataomoiWin";
                textRenderer.color = Kataomoi.color;
                __instance.BackgroundBar.material.SetColor("_Color", Kataomoi.color);
            }
            else if (AdditionalTempData.winCondition == WinCondition.VultureWin)
            {
                nonModTranslationText = "vultureWin";
                textRenderer.color = Vulture.color;
                __instance.BackgroundBar.material.SetColor("_Color", Vulture.color);
            }
            else if (AdditionalTempData.winCondition == WinCondition.LawyerSoloWin)
            {
                nonModTranslationText = "lawyerWin";
                textRenderer.color = Lawyer.color;
                __instance.BackgroundBar.material.SetColor("_Color", Lawyer.color);
            }
            else if (AdditionalTempData.winCondition == WinCondition.PlagueDoctorWin)
            {
                nonModTranslationText = "plagueDoctorWin";
                textRenderer.color = PlagueDoctor.color;
                __instance.BackgroundBar.material.SetColor("_Color", PlagueDoctor.color);
            }
            else if (AdditionalTempData.winCondition == WinCondition.FoxWin)
            {
                nonModTranslationText = "foxWin";
                textRenderer.color = Fox.color;
                __instance.BackgroundBar.material.SetColor("_Color", Fox.color);
            }
            else if (AdditionalTempData.winCondition == WinCondition.DoomsayerWin)
            {
                nonModTranslationText = "doomsayerWin";
                textRenderer.color = Doomsayer.color;
                __instance.BackgroundBar.material.SetColor("_Color", Doomsayer.color);
            }
            else if (AdditionalTempData.winCondition == WinCondition.PelicanWin)
            {
                nonModTranslationText = "pelicanWin";
                textRenderer.color = Pelican.color;
                __instance.BackgroundBar.material.SetColor("_Color", Pelican.color);
            }
            else if (AdditionalTempData.winCondition == WinCondition.YandereWin)
            {
                nonModTranslationText = "yandereWin";
                textRenderer.color = Yandere.color;
                __instance.BackgroundBar.material.SetColor("_Color", Yandere.color);
            }
            else if (AdditionalTempData.winCondition == WinCondition.JekyllAndHydeWin)
            {
                nonModTranslationText = "jekyllAndHydeWin";
                textRenderer.color = JekyllAndHyde.color;
                __instance.BackgroundBar.material.SetColor("_Color", JekyllAndHyde.color);
            }
            else if (AdditionalTempData.winCondition == WinCondition.LoversTeamWin)
            {
                nonModTranslationText = "crewLoverWin";
                textRenderer.color = Lovers.color;
                __instance.BackgroundBar.material.SetColor("_Color", Lovers.color);
            }
            else if (AdditionalTempData.winCondition is WinCondition.LoversSoloWin)
            {
                nonModTranslationText = "loversWin";
                textRenderer.color = Lovers.color;
                __instance.BackgroundBar.material.SetColor("_Color", Lovers.color);
            }
            else if (AdditionalTempData.winCondition == WinCondition.JackalWin)
            {
                nonModTranslationText = "jackalWin";
                textRenderer.color = Jackal.color;
                __instance.BackgroundBar.material.SetColor("_Color", Jackal.color);
            }
            else if (AdditionalTempData.winCondition == WinCondition.EveryoneDied)
            {
                textRenderer.text = ModTranslation.getString("everyoneDied");
                textRenderer.color = Palette.DisabledGrey;
                __instance.BackgroundBar.material.SetColor("_Color", Palette.DisabledGrey);
            }
            else if (AdditionalTempData.winCondition == WinCondition.MiniLose)
            {
                nonModTranslationText = "miniDied";
                textRenderer.text = ModTranslation.getString("miniDied");
                textRenderer.color = Mini.color;
            }
            else if (AdditionalTempData.winCondition == WinCondition.AkujoWin)
            {
                nonModTranslationText = "akujoWin";
                textRenderer.color = Akujo.color;
                __instance.BackgroundBar.material.SetColor("_Color", Akujo.color);
            }
            else if (AdditionalTempData.winCondition == WinCondition.MoriartyWin)
            {
                nonModTranslationText = "moriartyWin";
                textRenderer.color = Moriarty.color;
                __instance.BackgroundBar.material.SetColor("_Color", Moriarty.color);
            }

            if (!string.IsNullOrEmpty(nonModTranslationText)) textRenderer.text = ModTranslation.getString(nonModTranslationText);

            if (AdditionalTempData.winCondition == WinCondition.Default)
            {
                switch (OnGameEndPatch.gameOverReason)
                {
                    case GameOverReason.ImpostorDisconnect:
                        textRenderer.text = ModTranslation.getString("impostorDisconnect");
                        nonModTranslationText = "impostorWin";
                        textRenderer.color = Color.red;
                        break;
                    case GameOverReason.ImpostorsByKill:
                        textRenderer.text = ModTranslation.getString("impostorByKill");
                        nonModTranslationText = "impostorWin";
                        textRenderer.color = Color.red;
                        break;
                    case GameOverReason.ImpostorsBySabotage:
                        textRenderer.text = ModTranslation.getString("impostorBySabotage");
                        nonModTranslationText = "impostorWin";
                        textRenderer.color = Color.red;
                        break;
                    case GameOverReason.ImpostorsByVote:
                        textRenderer.text = ModTranslation.getString("impostorByVote");
                        nonModTranslationText = "impostorWin";
                        textRenderer.color = Color.red;
                        break;
                    case GameOverReason.CrewmatesByTask:
                        textRenderer.text = ModTranslation.getString("humansByTask");
                        nonModTranslationText = "crewWin";
                        textRenderer.color = Color.white;
                        break;
                    case GameOverReason.CrewmateDisconnect:
                        textRenderer.text = ModTranslation.getString("humansDisconnect");
                        nonModTranslationText = "crewWin";
                        textRenderer.color = Color.white;
                        break;
                    case GameOverReason.CrewmatesByVote:
                        textRenderer.text = ModTranslation.getString("humansByVote");
                        nonModTranslationText = "crewWin";
                        textRenderer.color = Color.white;
                        break;
                }
            }

            string extraText = "";

            foreach (WinCondition w in AdditionalTempData.additionalWinConditions)
            {
                switch (w)
                {
                    case WinCondition.OpportunistWin:
                        extraText += ModTranslation.getString("opportunistExtra");
                        break;
                    case WinCondition.AdditionalAlivePursuerWin:
                        extraText += ModTranslation.getString("pursuerExtra");
                        break;
                    default:
                        break;
                }
            }

            if (extraText.Length > 0) {
                textRenderer.text = string.Format(ModTranslation.getString(nonModTranslationText + "Extra"), extraText);
            }

            foreach (WinCondition cond in AdditionalTempData.additionalWinConditions)
            {
                switch (cond)
                {
                    case WinCondition.AdditionalLawyerStolenWin:
                        textRenderer.text += $"\n{Helpers.cs(Lawyer.color, ModTranslation.getString("lawyerExtraStolen"))}";
                        break;
                    case WinCondition.AdditionalLawyerBonusWin:
                        textRenderer.text += $"\n{Helpers.cs(Lawyer.color, ModTranslation.getString("lawyerExtraBonus"))}";
                        break;
                }
            }

            if (ClientOption.GetValue(ClientOption.ClientOptionType.ShowRoleSummary) == 1 || HideNSeek.isHideNSeekGM) {
                var position = Camera.main.ViewportToWorldPoint(new Vector3(0f, 1f, Camera.main.nearClipPlane));
                GameObject roleSummary = UnityEngine.Object.Instantiate(__instance.WinText.gameObject);
                roleSummary.transform.position = new Vector3(__instance.Navigation.ExitButton.transform.position.x + 0.1f, position.y - 0.1f, -214f); 
                roleSummary.transform.localScale = new Vector3(1f, 1f, 1f);

                var roleSummaryText = new StringBuilder();
                if (HideNSeek.isHideNSeekGM) {
                    int minutes = (int)AdditionalTempData.timer / 60;
                    int seconds = (int)AdditionalTempData.timer % 60;
                    roleSummaryText.AppendLine($"<color=#FAD934FF>Time: {minutes:00}:{seconds:00}</color> \n");
                }
                roleSummaryText.AppendLine(ModTranslation.getString("roleSummaryText"));
                bool plagueExists = AdditionalTempData.playerRoles.Any(x => x.Roles.Contains(RoleInfo.plagueDoctor));
                foreach (var data in AdditionalTempData.playerRoles) {
                    //var roles = string.Join(" ", data.Roles.Select(x => Helpers.cs(x.color, x.name)));
                    string roles = data.RoleNames;
                    //if (data.IsGuesser) roles += " (Guesser)";
                    var taskInfo = data.TasksTotal > 0 ? $" - <color=#FAD934FF>({data.TasksCompleted}/{data.TasksTotal})</color>" : "";
                    var exTaskInfo = data.ExTasksTotal > 0 ? $" - Ex <color=#E1564BFF>({data.ExTasksCompleted}/{data.ExTasksTotal})</color>" : "";
                    if (data.Kills != null) taskInfo += String.Format(ModTranslation.getString("roleSummaryKillsInfo"), data.Kills);
                    string infectionInfo = "";
                    if (plagueExists && !data.Roles.Contains(RoleInfo.plagueDoctor))
                    {                        
                        if (AdditionalTempData.plagueDoctorInfected.ContainsKey(data.PlayerId))
                        {
                            infectionInfo += " - " + Helpers.cs(Color.red, ModTranslation.getString("plagueDoctorInfectedText"));
                        }
                        else
                        {
                            float progress = AdditionalTempData.plagueDoctorProgress.ContainsKey(data.PlayerId) ? AdditionalTempData.plagueDoctorProgress[data.PlayerId] : 0f;
                            infectionInfo += " - " + PlagueDoctor.getProgressString(progress);
                        }
                    }
                    roleSummaryText.AppendLine($"{Helpers.cs(data.IsAlive ? Color.white : new Color(.7f,.7f,.7f), data.PlayerName)} - {roles}{taskInfo}{exTaskInfo}{infectionInfo}"); 
                }
                TMPro.TMP_Text roleSummaryTextMesh = roleSummary.GetComponent<TMPro.TMP_Text>();
                roleSummaryTextMesh.alignment = TMPro.TextAlignmentOptions.TopLeft;
                roleSummaryTextMesh.color = Color.white;
                roleSummaryTextMesh.fontSizeMin = 1.5f;
                roleSummaryTextMesh.fontSizeMax = 1.5f;
                roleSummaryTextMesh.fontSize = 1.5f;
                
                var roleSummaryTextMeshRectTransform = roleSummaryTextMesh.GetComponent<RectTransform>();
                roleSummaryTextMeshRectTransform.anchoredPosition = new Vector2(position.x + 3.5f, position.y - 0.1f);
                roleSummaryTextMesh.text = roleSummaryText.ToString();
                LastGameHistory.SetHistory(__instance.WinText.font, new MetaContextOld.VariableText(new TextAttribute(TextAttribute.BoldAttr)
                { Font = __instance.WinText.font, Size = new(8f, 5f), Alignment = TMPro.TextAlignmentOptions.Left }.EditFontSize(1.1f, 1.1f, 1.1f))
                { RawText = roleSummaryText.ToString() }, textRenderer.text);
            }

            if (ClientOption.GetValue(ClientOption.ClientOptionType.ScreenshotOnEnd) == 1)
            {
                IEnumerator CaptureAndSave()
                {
                    var screenshotDirectory = Path.Combine(Paths.GameRootPath, "ScreenShots");
                    Directory.CreateDirectory(screenshotDirectory);
                    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    var winCondition = AdditionalTempData.winCondition.ToString();
                    var filename = $"GameEnd_{timestamp}_{winCondition}.png";
                    var filePath = Path.Combine(screenshotDirectory, filename);

                    yield return new WaitForSeconds(2.5f);
                    ScreenCapture.CaptureScreenshot(filePath);
                }
                __instance.StartCoroutine(CaptureAndSave().WrapToIl2Cpp());
            }

            AdditionalTempData.clear();
        }
    }

    [HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))] 
    class CheckEndCriteriaPatch {
        public static bool Prefix(ShipStatus __instance) {
            if (!GameData.Instance) return false;
            if (DestroyableSingleton<TutorialManager>.InstanceExists) // InstanceExists | Don't check Custom Criteria when in Tutorial
                return true;
            if (FreePlayGM.isFreePlayGM) return false;
            var statistics = new PlayerStatistics(__instance);
            if (CheckAndEndGameForMiniLose(__instance)) return false;
            if (CheckAndEndGameForJesterWin(__instance)) return false;
            if (CheckAndEndGameForKataomoiWin(__instance)) return false;
            if (CheckAndEndGameForLawyerMeetingWin(__instance)) return false;
            if (CheckAndEndGameForArsonistWin(__instance)) return false;
            if (CheckAndEndGameForVultureWin(__instance)) return false;
            if (CheckAndEndGameForDoomsayerWin(__instance)) return false;
            if (CheckAndEndGameForPlagueDoctorWin(__instance)) return false;
            if (CheckAndEndGameForJekyllAndHydeWin(__instance, statistics)) return false;
            if (CheckAndEndGameForMoriartyWin(__instance, statistics)) return false;
            if (CheckAndEndGameForYandereWin(__instance, statistics)) return false;
            if (CheckAndEndGameForSabotageWin(__instance)) return false;
            if (CheckAndEndGameForTaskWin(__instance)) return false;
            //if (CheckAndEndGameForProsecutorWin(__instance)) return false;
            if (CheckAndEndGameForLoverWin(__instance, statistics)) return false;
            if (CheckAndEndGameForAkujoWin(__instance, statistics)) return false;
            if (CheckAndEndGameForPelicanWin(__instance, statistics)) return false;
            if (CheckAndEndGameForJackalWin(__instance, statistics)) return false;
            if (CheckAndEndGameForImpostorWin(__instance, statistics)) return false;
            if (CheckAndEndGameForCrewmateWin(__instance, statistics)) return false;
            return false;
        }

        private static bool CheckAndEndGameForMiniLose(ShipStatus __instance) {
            if (Mini.triggerMiniLose) {
                //__instance.enabled = false;
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.MiniLose, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForJesterWin(ShipStatus __instance) {
            if (Jester.players.Any(x => x.player && x.triggerJesterWin)) {
                //__instance.enabled = false;
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.JesterWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForArsonistWin(ShipStatus __instance) {
            if (Arsonist.players.Any(x => x.triggerArsonistWin)) {
                //__instance.enabled = false;
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.ArsonistWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForKataomoiWin(ShipStatus __instance)
        {
            if (Kataomoi.triggerKataomoiWin)
            {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.KataomoiWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForDoomsayerWin(ShipStatus __instance)
        {
            if (Doomsayer.players.Any(x => x.player && x.triggerWin))
            {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.DoomsayerWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForVultureWin(ShipStatus __instance) {
            if (Vulture.players.Any(x => x.player && x.triggerVultureWin)) {
                //__instance.enabled = false;
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.VultureWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForLawyerMeetingWin(ShipStatus __instance)
        {
            if (Lawyer.triggerLawyerWin)
            {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.LawyerSoloWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForYandereWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if ((statistics.YandereAlive >= statistics.TotalAlive - statistics.YandereAlive && statistics.TeamPelicanAlive == 0 && statistics.TeamJackalAlive == 0
                && statistics.TeamImpostorsAlive == 0 && statistics.TeamMoriartyAlive == 0 && statistics.TeamSheriffAlive == 0) ||
                (Yandere.canWinWithTarget && statistics.TotalAlive <= 3))
            {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.YandereWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForPlagueDoctorWin(ShipStatus __instance)
        {
            if (PlagueDoctor.triggerPlagueDoctorWin)
            {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.PlagueDoctorWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForJekyllAndHydeWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (JekyllAndHyde.triggerWin || (statistics.TeamJekyllAndHydeAlive >= statistics.TotalAlive - statistics.TeamJekyllAndHydeAlive &&
                        statistics.TeamImpostorsAlive == 0 && statistics.TeamPelicanAlive == 0 && statistics.TeamJackalAlive == 0 &&
                        statistics.TeamMoriartyAlive == 0 && statistics.TeamSheriffAlive == 0 && statistics.YandereAlive == 0
                         && (statistics.JekyllAndHydeLovers == 0 || statistics.JekyllAndHydeLovers >= statistics.CouplesAlive * 2)))
            {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.JekyllAndHydeWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForSabotageWin(ShipStatus __instance) {
            if (MapUtilities.Systems == null) return false;
            var systemType = MapUtilities.Systems.ContainsKey(SystemTypes.LifeSupp) ? MapUtilities.Systems[SystemTypes.LifeSupp] : null;
            if (systemType != null) {
                LifeSuppSystemType lifeSuppSystemType = systemType.TryCast<LifeSuppSystemType>();
                if (lifeSuppSystemType != null && lifeSuppSystemType.Countdown < 0f) {
                    EndGameForSabotage(__instance);
                    lifeSuppSystemType.Countdown = 10000f;
                    return true;
                }
            }
            var systemType2 = MapUtilities.Systems.ContainsKey(SystemTypes.Reactor) ? MapUtilities.Systems[SystemTypes.Reactor] : null;
            if (systemType2 == null) {
                systemType2 = MapUtilities.Systems.ContainsKey(SystemTypes.Laboratory) ? MapUtilities.Systems[SystemTypes.Laboratory] : null;
            }
            if (systemType2 != null) {
                ICriticalSabotage criticalSystem = systemType2.TryCast<ICriticalSabotage>();
                if (criticalSystem != null && criticalSystem.Countdown < 0f) {
                    EndGameForSabotage(__instance);
                    criticalSystem.ClearSabotage();
                    return true;
                }
            }
            return false;
        }

        private static bool CheckAndEndGameForTaskWin(ShipStatus __instance) {
            if (HideNSeek.isHideNSeekGM && !HideNSeek.taskWinPossible) return false;
            if ((GameData.Instance.TotalTasks > 0 && GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks) || (TaskMaster.triggerTaskMasterWin && TaskMaster.hasAlivePlayers)) {
                //__instance.enabled = false;
                GameManager.Instance.RpcEndGame(GameOverReason.CrewmatesByTask, false);
                return true;
            }
            return false;
        }

        /*private static bool CheckAndEndGameForProsecutorWin(ShipStatus __instance) {
            if (Lawyer.triggerProsecutorWin) {
                //__instance.enabled = false;
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.ProsecutorWin, false);
                return true;
            }
            return false;
        }*/

        private static bool CheckAndEndGameForPelicanWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.TeamPelicanAlive >= statistics.TotalAlive - statistics.TeamPelicanAlive && statistics.YandereAlive == 0 &&
                statistics.TeamImpostorsAlive == 0 && statistics.TeamSheriffAlive == 0 && statistics.TeamMoriartyAlive == 0
                && statistics.TeamJekyllAndHydeAlive == 0 && statistics.TeamJackalAlive == 0)
            {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.PelicanWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForLoverWin(ShipStatus __instance, PlayerStatistics statistics) {
            if (statistics.CouplesAlive == 1 && statistics.TotalAlive <= 3) {
                //__instance.enabled = false;
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.LoversWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForAkujoWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (Akujo.numAlive == 1 && statistics.TotalAlive <= 3)
            {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.AkujoWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForJackalWin(ShipStatus __instance, PlayerStatistics statistics) {
            if (statistics.TeamJackalAlive >= statistics.TotalAlive - statistics.TeamJackalAlive && statistics.TeamPelicanAlive == 0 && statistics.YandereAlive == 0
                && statistics.TeamImpostorsAlive == 0 && statistics.TeamSheriffAlive == 0 && statistics.TeamMoriartyAlive == 0 && statistics.TeamJekyllAndHydeAlive == 0 &&
                (statistics.TeamJackalLovers == 0 || statistics.TeamJackalLovers >= statistics.CouplesAlive * 2)) {
                //__instance.enabled = false;
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.TeamJackalWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForMoriartyWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if ((statistics.TeamMoriartyAlive >= statistics.TotalAlive - statistics.TeamMoriartyAlive && statistics.YandereAlive == 0
                && statistics.TeamPelicanAlive == 0 && statistics.TeamImpostorsAlive == 0 && statistics.TeamSheriffAlive == 0 && statistics.TeamJackalAlive == 0 && statistics.TeamJekyllAndHydeAlive == 0 &&
                (statistics.MoriartyLovers == 0 || statistics.MoriartyLovers >= statistics.CouplesAlive * 2)) || Moriarty.triggerMoriartyWin)
            {
                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.MoriartyWin, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForImpostorWin(ShipStatus __instance, PlayerStatistics statistics) {
            if (HideNSeek.isHideNSeekGM) 
                if ((0 != statistics.TotalAlive - statistics.TeamImpostorsAlive)) return false;

            if (statistics.TeamImpostorsAlive >= statistics.TotalAlive - statistics.TeamImpostorsAlive && statistics.TeamPelicanAlive == 0 && statistics.YandereAlive == 0
                && statistics.TeamJackalAlive == 0 && statistics.TeamSheriffAlive == 0 && statistics.TeamMoriartyAlive == 0 && statistics.TeamJekyllAndHydeAlive == 0 &&
                (statistics.TeamImpostorLovers == 0 || statistics.TeamImpostorLovers >= statistics.CouplesAlive * 2)) {
                //__instance.enabled = false;
                GameOverReason endReason;
                switch (GameData.LastDeathReason) {
                    case DeathReason.Exile:
                        endReason = GameOverReason.ImpostorsByVote;
                        break;
                    case DeathReason.Kill:
                        endReason = GameOverReason.ImpostorsByKill;
                        break;
                    default:
                        endReason = GameOverReason.ImpostorsByVote;
                        break;
                }
                GameManager.Instance.RpcEndGame(endReason, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForCrewmateWin(ShipStatus __instance, PlayerStatistics statistics) {
            if (HideNSeek.isHideNSeekGM && HideNSeek.timer <= 0 && !HideNSeek.isWaitingTimer) {
                //__instance.enabled = false;
                GameManager.Instance.RpcEndGame(GameOverReason.CrewmatesByVote, false);
                return true;
            }
            if (statistics.TeamImpostorsAlive == 0 && statistics.TeamJackalAlive == 0 && statistics.TeamPelicanAlive == 0 &&
                statistics.TeamMoriartyAlive == 0 && statistics.TeamJekyllAndHydeAlive == 0 && statistics.YandereAlive == 0) {
                //__instance.enabled = false;
                GameManager.Instance.RpcEndGame(GameOverReason.CrewmatesByVote, false);
                return true;
            }
            return false;
        }

        private static void EndGameForSabotage(ShipStatus __instance) {
            //__instance.enabled = false;
            GameManager.Instance.RpcEndGame(GameOverReason.ImpostorsBySabotage, false);
            return;
        }

    }

    internal class PlayerStatistics {
        public int TeamImpostorsAlive {get;set;}
        public int TeamJackalAlive {get;set;}
        public int TeamSheriffAlive { get; set; }
        public int TeamMoriartyAlive { get; set; }
        public int TeamJekyllAndHydeAlive { get;set; }
        public int TeamPelicanAlive { get; set; }
        public int TotalAlive {get;set;}
        public int MoriartyLovers { get; set; }
        public int TeamImpostorLovers { get; set; }
        public int TeamJackalLovers { get; set; }
        public int JekyllAndHydeLovers { get; set; }
        public int YandereAlive { get; set; }
        public int CouplesAlive { get; set; }

        public PlayerStatistics(ShipStatus __instance) {
            GetPlayerCounts();
        }

        private bool isLover(NetworkedPlayerInfo p) {
            foreach (var couple in Lovers.couples)
            {
                if (p.PlayerId == couple.lover1.PlayerId || p.PlayerId == couple.lover2.PlayerId) return true;
            }
            return false;
        }

        private void GetPlayerCounts() {
            int numJackalAlive = Jackal.livingPlayers.Count + Sidekick.livingPlayers.Count;
            int numImpostorsAlive = 0;
            int numMoriartyAlive = Moriarty.livingPlayers.Count;
            int numJekyllAndHydeAlive = JekyllAndHyde.livingPlayers.Count;
            int numPelicanAlive = Pelican.livingPlayers.Count;
            int numYandereAlive = Yandere.livingPlayers.Count;
            if (numYandereAlive > 0) numYandereAlive += Yandere.target && !Yandere.target.Data.IsDead ? 1 : 0;
            int numCouplesAlive = 0;
            int impLovers = 0;
            int numTotalAlive = 0;

            foreach (var playerInfo in GameData.Instance.AllPlayers.GetFastEnumerator())
            {
                if (!playerInfo.Disconnected)
                {
                    if (!playerInfo.IsDead)
                    {
                        numTotalAlive++;

                        bool lover = isLover(playerInfo);

                        if (playerInfo.Role.IsImpostor) {
                            numImpostorsAlive++;
                            if (lover) impLovers++;
                        }
                    }
                }
            }

            // Count the Mimic as one if enabled
            if (MimicK.isAlive() && MimicA.isAlive() && MimicK.countAsOne)
            {
                numImpostorsAlive--;
                numTotalAlive--;
            }

            if (BomberA.isAlive() && BomberB.isAlive() && BomberA.countAsOne)
            {
                numImpostorsAlive--;
                numTotalAlive--;
            }

            foreach (var couple in Lovers.couples) {
                if (couple.alive) numCouplesAlive++;
            }

            if (SchrodingersCat.hasAlivePlayers)
            {
                switch (SchrodingersCat.team)
                {
                    case SchrodingersCat.Team.Jackal:
                        numJackalAlive++; break;
                    case SchrodingersCat.Team.Moriarty:
                        numMoriartyAlive++; break;
                    case SchrodingersCat.Team.JekyllAndHyde:
                        numJekyllAndHydeAlive++; break;
                    case SchrodingersCat.Team.Yandere:
                        numYandereAlive++; break;
                }
            }

            TeamJackalAlive = numJackalAlive;
            TeamImpostorsAlive = numImpostorsAlive;
            TeamSheriffAlive = Sheriff.allPlayers.Where(x => !x.Data.IsDead && !Madmate.madmate.Any(y => y.PlayerId == x.PlayerId)).ToList().Count +
                (Deputy.stopsGameEnd ? Deputy.livingPlayers.Count : 0);
            TeamMoriartyAlive = numMoriartyAlive;
            TeamJekyllAndHydeAlive = numJekyllAndHydeAlive;
            TeamPelicanAlive = numPelicanAlive;
            TotalAlive = numTotalAlive;
            TeamImpostorLovers = impLovers;
            TeamJackalLovers = Jackal.countLovers() + Sidekick.countLovers() + SchrodingersCat.countLovers(SchrodingersCat.Team.Jackal);
            CouplesAlive = numCouplesAlive;
            MoriartyLovers = Moriarty.countLovers() + SchrodingersCat.countLovers(SchrodingersCat.Team.Moriarty);
            JekyllAndHydeLovers = JekyllAndHyde.countLovers() + SchrodingersCat.countLovers(SchrodingersCat.Team.JekyllAndHyde);
            YandereAlive = numYandereAlive;
        }
    }
}
