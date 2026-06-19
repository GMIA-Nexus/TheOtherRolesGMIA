using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HarmonyLib;
using UnityEngine;
using static TheOtherRoles.GameHistory;
using static TheOtherRoles.TheOtherRoles;
using static TheOtherRoles.TheOtherRolesGM;

namespace TheOtherRoles.Patches
{
    enum CustomGameOverReason
    {
        LoversWin = 10,
        TeamJackalWin = 11,
        MiniLose = 12,
        JesterWin = 13,
        ArsonistWin = 14,
        VultureWin = 15,
        LawyerSoloWin = 16,
        PlagueDoctorWin = 17,
        FoxWin = 18,
        PuppeteerWin = 19,
        JekyllAndHydeWin = 20,
        AkujoWin = 21,
        ForceEnd = 22,
        MoriartyWin = 23,
    }

    enum WinCondition
    {
        Default,
        LoversTeamWin,
        LoversSoloWin,
        JesterWin,
        JackalWin,
        MiniLose,
        ArsonistWin,
        OpportunistWin,
        VultureWin,
        LawyerSoloWin,
        AdditionalLawyerBonusWin,
        AdditionalLawyerStolenWin,
        AdditionalAlivePursuerWin,
        PlagueDoctorWin,
        FoxWin,
        EveryoneDied,
        PuppeteerWin,
        JekyllAndHydeWin,
        AkujoWin,
        ForceEnd,
        MoriartyWin,
    }

    enum FinalStatus
    {
        Alive,
        Torched,
        Spelled,
        Sabotage,
        Exiled,
        Dead,
        Suicide,
        Misfire,
        Revenge,
        Diseased,
        Divined,
        Loneliness,
        BrainWashKill,
        Scapegoat,
        GMExecuted,
        Disconnected
    }


    static class AdditionalTempData
    {
        // Should be implemented using a proper GameOverReason in the future
        public static WinCondition winCondition = WinCondition.Default;
        public static List<WinCondition> additionalWinConditions = new();
        public static List<PlayerRoleInfo> playerRoles = new();
        public static bool isGM = false;
        public static GameOverReason gameOverReason = GameOverReason.CrewmatesByTask;

        public static Dictionary<int, PlayerControl> plagueDoctorInfected = new();
        public static Dictionary<int, float> plagueDoctorProgress = new();

        public static void clear()
        {
            playerRoles.Clear();
            additionalWinConditions.Clear();
            winCondition = WinCondition.Default;
            isGM = false;
        }

        internal class PlayerRoleInfo
        {
            public string PlayerName { get; set; }
            public string NameSuffix { get; set; }
            public List<RoleInfo> Roles { get; set; }
            public string RoleString { get; set; }
            public int ColorId = 0;
            public int TasksCompleted { get; set; }
            public int TasksTotal { get; set; }
            public FinalStatus Status { get; set; }
            public int PlayerId { get; set; }
        }
    }

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.CheckTaskCompletion))]
    public class ShipStatusCheckTaskCompletionPatch
    {
        public static bool Prefix(ref bool __result, ShipStatus __instance)
        {
            // クルーメイトが生存していない場合はタスク勝利できない
            if (!CustomOptionHolder.canWinByTaskWithoutLivingPlayer.getBool() && !Helpers.isCrewmateAlive())
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    public static class OnGameEndPatch
    {

        public static void Prefix(AmongUsClient __instance, [HarmonyArgument(0)] ref EndGameResult endGameResult)
        {
            Camouflager.resetCamouflage();
            Morphling.resetMorph();

            AdditionalTempData.gameOverReason = endGameResult.GameOverReason;
            if ((int)endGameResult.GameOverReason >= 10) endGameResult.GameOverReason = GameOverReason.ImpostorsByKill;
        }

        public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ref EndGameResult endGameResult)
        {
            Logger.info("-----------Game Ended-----------", "Phase");
            var gameOverReason = AdditionalTempData.gameOverReason;
            // 狐の勝利条件を満たしたか確認する
            Boolean isFoxAlive = Fox.isFoxAlive();

            Boolean isFoxCompletedTasks = Fox.isFoxCompletedTasks(); // 生存中の狐が1匹でもタスクを全て終えていること
            if (isFoxAlive && isFoxCompletedTasks)
            {
                // タスク・サボタージュ勝利の場合はオプションの設定次第
                if (gameOverReason == GameOverReason.CrewmatesByTask && !Fox.crewWinsByTasks)
                {
                    gameOverReason = (GameOverReason)CustomGameOverReason.FoxWin;
                }
                else if (gameOverReason == GameOverReason.ImpostorsBySabotage && !Fox.impostorWinsBySabotage)
                {
                    gameOverReason = (GameOverReason)CustomGameOverReason.FoxWin;
                }

                // 第三陣営の勝利以外の場合に狐が生存していたら狐の勝ち
                else if (gameOverReason is not ((GameOverReason)CustomGameOverReason.PlagueDoctorWin) and
                not ((GameOverReason)CustomGameOverReason.ArsonistWin) and
                not ((GameOverReason)CustomGameOverReason.JesterWin) and
                not ((GameOverReason)CustomGameOverReason.VultureWin) and
                not ((GameOverReason)CustomGameOverReason.AkujoWin) and
                not ((GameOverReason)CustomGameOverReason.PuppeteerWin) and
                not ((GameOverReason)CustomGameOverReason.JekyllAndHydeWin) and
                not ((GameOverReason)GameOverReason.CrewmatesByTask) and
                not ((GameOverReason)GameOverReason.ImpostorsBySabotage))
                {
                    gameOverReason = (GameOverReason)CustomGameOverReason.FoxWin;
                }
            }
            AdditionalTempData.clear();

            //foreach (var pc in PlayerControl.AllPlayerControls)
            var excludeRoles = new RoleType[] { RoleType.Lovers };
            foreach (var p in GameData.Instance.AllPlayers)
            {
                //var p = pc.Data;
                var roles = RoleInfo.getRoleInfoForPlayer(p.Object, excludeRoles, includeHidden: true);
                var (tasksCompleted, tasksTotal) = TasksHandler.taskInfo(p);
                var finalStatus = finalStatuses[p.PlayerId] =
                    p.Disconnected == true ? FinalStatus.Disconnected :
                    finalStatuses.ContainsKey(p.PlayerId) ? finalStatuses[p.PlayerId] :
                    p.IsDead == true ? FinalStatus.Dead :
                    gameOverReason == GameOverReason.ImpostorsBySabotage && !p.Role.IsImpostor ? FinalStatus.Sabotage :
                    FinalStatus.Alive;

                if (gameOverReason == GameOverReason.CrewmatesByTask && p.Object.isCrew()) tasksCompleted = tasksTotal;

                AdditionalTempData.playerRoles.Add(new AdditionalTempData.PlayerRoleInfo()
                {
                    PlayerName = p.PlayerName,
                    PlayerId = p.PlayerId,
                    ColorId = p.DefaultOutfit.ColorId,
                    NameSuffix = Lovers.getIcon(p.Object) + Cupid.getIcon(p.Object) + Akujo.getIcon(p.Object),
                    Roles = roles,
                    RoleString = RoleInfo.GetRolesString(p.Object, true, excludeRoles, true),
                    TasksTotal = tasksTotal,
                    TasksCompleted = tasksCompleted,
                    Status = finalStatus,
                });
            }

            AdditionalTempData.isGM = CustomOptionHolder.gmEnabled.getBool() && PlayerControl.LocalPlayer.isGM();
            AdditionalTempData.plagueDoctorInfected = PlagueDoctor.infected;
            AdditionalTempData.plagueDoctorProgress = PlagueDoctor.progress;


            // Remove Jester, Arsonist, Vulture, Jackal, former Jackals and Sidekick from winners (if they win, they'll be readded)
            List<PlayerControl> notWinners = new();
            if (Jester.jester != null) notWinners.Add(Jester.jester);
            if (Sidekick.sidekick != null) notWinners.Add(Sidekick.sidekick);
            if (Jackal.jackal != null) notWinners.Add(Jackal.jackal);
            if (Arsonist.arsonist != null) notWinners.Add(Arsonist.arsonist);
            if (Vulture.vulture != null) notWinners.Add(Vulture.vulture);
            if (Lawyer.lawyer != null) notWinners.Add(Lawyer.lawyer);
            if (Pursuer.pursuer != null) notWinners.Add(Pursuer.pursuer);

            notWinners.AddRange(Jackal.formerJackals);
            // notWinners.AddRange(Madmate.allPlayers);
            // notWinners.AddRange(CreatedMadmate.allPlayers);
            notWinners.AddRange(Opportunist.allPlayers);
            notWinners.AddRange(PlagueDoctor.allPlayers);
            notWinners.AddRange(Fox.allPlayers);
            notWinners.AddRange(Immoralist.allPlayers);
            notWinners.AddRange(Puppeteer.allPlayers);
            notWinners.AddRange(JekyllAndHyde.allPlayers);
            notWinners.AddRange(Akujo.allPlayers);
            notWinners.AddRange(AkujoHonmei.allPlayers);
            notWinners.AddRange(Moriarty.allPlayers);
            notWinners.AddRange(Cupid.allPlayers);
            if (Puppeteer.dummy != null) notWinners.Add(Puppeteer.dummy);
            // if (SchrodingersCat.team != SchrodingersCat.Team.Crew && !(SchrodingersCat.team == SchrodingersCat.Team.None && SchrodingersCat.canWinAsCrewmate)) notWinners.AddRange(SchrodingersCat.allPlayers);

            // Neutral shifter can't win
            if (Shifter.shifter != null && Shifter.isNeutral) notWinners.Add(Shifter.shifter);

            // GM can't win at all, and we're treating lovers as a separate class
            if (GM.gm != null) notWinners.Add(GM.gm);

            if (Lovers.separateTeam)
            {
                foreach (var couple in Lovers.couples)
                {
                    notWinners.Add(couple.lover1);
                    notWinners.Add(couple.lover2);
                }
            }

            bool saboWin = gameOverReason == GameOverReason.ImpostorsBySabotage;
            bool impostorWin = gameOverReason == GameOverReason.ImpostorsByKill || gameOverReason == GameOverReason.ImpostorsByVote || gameOverReason == GameOverReason.ImpostorDisconnect;
            bool crewWin = gameOverReason == GameOverReason.CrewmatesByTask || gameOverReason == GameOverReason.CrewmatesByVote || gameOverReason == GameOverReason.CrewmateDisconnect;

            bool jesterWin = Jester.jester != null && gameOverReason == (GameOverReason)CustomGameOverReason.JesterWin;
            bool arsonistWin = Arsonist.arsonist != null && gameOverReason == (GameOverReason)CustomGameOverReason.ArsonistWin;
            bool miniLose = Mini.exists && gameOverReason == (GameOverReason)CustomGameOverReason.MiniLose;
            bool loversWin = Lovers.anyAlive() && !(Lovers.separateTeam && gameOverReason == GameOverReason.CrewmatesByTask);
            bool teamJackalWin = gameOverReason == (GameOverReason)CustomGameOverReason.TeamJackalWin;
            bool vultureWin = Vulture.vulture != null && gameOverReason == (GameOverReason)CustomGameOverReason.VultureWin;
            bool lawyerSoloWin = Lawyer.lawyer != null && gameOverReason == (GameOverReason)CustomGameOverReason.LawyerSoloWin;
            bool plagueDoctorWin = PlagueDoctor.exists && gameOverReason == (GameOverReason)CustomGameOverReason.PlagueDoctorWin;
            bool foxWin = Fox.exists && gameOverReason == (GameOverReason)CustomGameOverReason.FoxWin;
            bool puppeteerWin = Puppeteer.exists && gameOverReason == (GameOverReason)CustomGameOverReason.PuppeteerWin;
            bool jekyllAndHydeWin = JekyllAndHyde.exists && gameOverReason == (GameOverReason)CustomGameOverReason.JekyllAndHydeWin;
            bool moriartyWin = Moriarty.exists && gameOverReason == (GameOverReason)CustomGameOverReason.MoriartyWin;
            bool everyoneDead = AdditionalTempData.playerRoles.All(x => x.Status != FinalStatus.Alive);
            bool akujoWin = Akujo.numAlive > 0 && gameOverReason != GameOverReason.CrewmatesByTask;
            bool forceEnd = gameOverReason == (GameOverReason)CustomGameOverReason.ForceEnd;


            // 勝利画面が正常にでないことがあるのでインポスター・クルーの勝利者追加処理をここに移動
            if (impostorWin)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                foreach (var p in PlayerControl.AllPlayerControls)
                {
                    if (p.isImpostor() || p.hasModifier(ModifierType.Madmate) || p.hasModifier(ModifierType.CreatedMadmate))
                    {
                        CachedPlayerData wpd = new(p.Data);
                        EndGameResult.CachedWinners.Add(wpd);
                    }
                    else if (p.isRole(RoleType.SchrodingersCat))
                    {
                        if (SchrodingersCat.team == SchrodingersCat.Team.Impostor)
                        {
                            CachedPlayerData wpd = new(p.Data);
                            EndGameResult.CachedWinners.Add(wpd);
                        }
                    }
                }
            }
            else if (crewWin)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                foreach (var p in PlayerControl.AllPlayerControls)
                {
                    if (p.isCrew() && !p.hasModifier(ModifierType.Madmate) && !p.hasModifier(ModifierType.CreatedMadmate))
                    {
                        CachedPlayerData wpd = new(p.Data);
                        EndGameResult.CachedWinners.Add(wpd);
                    }
                    else if (p.isRole(RoleType.SchrodingersCat))
                    {
                        if (SchrodingersCat.team == SchrodingersCat.Team.Crew || (SchrodingersCat.team == SchrodingersCat.Team.None && SchrodingersCat.canWinAsCrewmate))
                        {
                            CachedPlayerData wpd = new(p.Data);
                            EndGameResult.CachedWinners.Add(wpd);
                        }
                    }
                }
            }

            // 勝利画面から不要なキャラを追放する
            List<CachedPlayerData> winnersToRemove = new();
            foreach (CachedPlayerData winner in EndGameResult.CachedWinners)
            {
                if (notWinners.Any(x => x.Data.PlayerName == winner.PlayerName)) winnersToRemove.Add(winner);
            }
            foreach (var winner in winnersToRemove) EndGameResult.CachedWinners.Remove(winner);

            // Mini lose
            if (miniLose)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                // CachedPlayerData wpd = new CachedPlayerData(Mini.mini.Data);
                //wpd.IsYou = false; // If "no one is the Mini", it will display the Mini, but also show defeat to everyone
                // EndGameResult.CachedWinners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.MiniLose;
            }

            // Jester win
            else if (jesterWin)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                CachedPlayerData wpd = new(Jester.jester.Data);
                EndGameResult.CachedWinners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.JesterWin;
            }

            // Arsonist win
            else if (arsonistWin)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                CachedPlayerData wpd = new(Arsonist.arsonist.Data);
                EndGameResult.CachedWinners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.ArsonistWin;
            }

            else if (plagueDoctorWin)
            {
                foreach (var pd in PlagueDoctor.players)
                {
                    EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                    CachedPlayerData wpd = new(pd.player.Data);
                    EndGameResult.CachedWinners.Add(wpd);
                    AdditionalTempData.winCondition = WinCondition.PlagueDoctorWin;
                }
            }
            // Puppeter win
            else if (puppeteerWin)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                foreach (var puppeteer in Puppeteer.players)
                {
                    CachedPlayerData wpd = new(puppeteer.player.Data);
                    EndGameResult.CachedWinners.Add(wpd);
                }
                AdditionalTempData.winCondition = WinCondition.PuppeteerWin;
            }

            else if (jekyllAndHydeWin)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                foreach (var jekyllAndHyde in JekyllAndHyde.players)
                {
                    CachedPlayerData wpd = new(jekyllAndHyde.player.Data);
                    EndGameResult.CachedWinners.Add(wpd);
                }
                if (SchrodingersCat.team == SchrodingersCat.Team.JekyllAndHyde)
                {
                    foreach (var schrodingersCat in SchrodingersCat.allPlayers)
                    {
                        CachedPlayerData wpd = new(schrodingersCat.Data);
                        EndGameResult.CachedWinners.Add(wpd);
                    }
                }
                AdditionalTempData.winCondition = WinCondition.JekyllAndHydeWin;
            }

            else if (moriartyWin)
            {
                if (Moriarty.counter < Moriarty.numberToWin)
                {
                    EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                    AdditionalTempData.winCondition = WinCondition.EveryoneDied;
                }
                else
                {
                    EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                    foreach (var moriarty in Moriarty.players)
                    {
                        CachedPlayerData wpd = new(moriarty.player.Data);
                        EndGameResult.CachedWinners.Add(wpd);
                    }
                    if (SchrodingersCat.team == SchrodingersCat.Team.Moriarty)
                    {
                        foreach (var schrodingersCat in SchrodingersCat.allPlayers)
                        {
                            CachedPlayerData wpd = new(schrodingersCat.Data);
                            EndGameResult.CachedWinners.Add(wpd);
                        }
                    }
                    AdditionalTempData.winCondition = WinCondition.MoriartyWin;
                }
            }

            else if (everyoneDead)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                AdditionalTempData.winCondition = WinCondition.EveryoneDied;
            }

            // Vulture win
            else if (vultureWin)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                CachedPlayerData wpd = new(Vulture.vulture.Data);
                EndGameResult.CachedWinners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.VultureWin;
            }

            // Akujo win conditions
            else if (akujoWin)
            {
                AdditionalTempData.winCondition = WinCondition.AkujoWin;
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                foreach (var akujo in Akujo.players)
                {
                    if (akujo.player.isAlive() && akujo.honmei?.player != null && akujo.honmei.player.isAlive())
                    {
                        EndGameResult.CachedWinners.Add(new CachedPlayerData(akujo.player.Data));
                        EndGameResult.CachedWinners.Add(new CachedPlayerData(akujo.honmei.player.Data));
                    }
                    if (akujo.player.isAlive() && akujo.cupidHonmei != null && akujo.cupidHonmei.isAlive())
                    {
                        EndGameResult.CachedWinners.Add(new CachedPlayerData(akujo.player.Data));
                        EndGameResult.CachedWinners.Add(new CachedPlayerData(akujo.cupidHonmei.Data));
                    }
                }
            }

            // Lovers win conditions
            else if (loversWin)
            {
                // Double win for lovers, crewmates also win
                if (GameManager.Instance.DidHumansWin(gameOverReason) && !Lovers.separateTeam && Lovers.anyNonKillingCouples())
                {
                    AdditionalTempData.winCondition = WinCondition.LoversTeamWin;
                    AdditionalTempData.additionalWinConditions.Add(WinCondition.LoversTeamWin);
                }
                // Lovers solo win
                else
                {
                    AdditionalTempData.winCondition = WinCondition.LoversSoloWin;
                    EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();

                    foreach (var couple in Lovers.couples)
                    {
                        if (couple.existingAndAlive)
                        {
                            EndGameResult.CachedWinners.Add(new CachedPlayerData(couple.lover1.Data));
                            EndGameResult.CachedWinners.Add(new CachedPlayerData(couple.lover2.Data));
                        }
                    }
                }
            }

            // Jackal win condition (should be implemented using a proper GameOverReason in the future)
            else if (teamJackalWin)
            {
                // Jackal wins if nobody except jackal is alive
                AdditionalTempData.winCondition = WinCondition.JackalWin;
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                if (Jackal.jackal != null)
                {
                    CachedPlayerData wpd = new(Jackal.jackal.Data)
                    {
                        IsImpostor = false
                    };
                    EndGameResult.CachedWinners.Add(wpd);
                }
                // If there is a sidekick. The sidekick also wins
                if (Sidekick.sidekick != null)
                {
                    CachedPlayerData wpdSidekick = new(Sidekick.sidekick.Data)
                    {
                        IsImpostor = false
                    };
                    EndGameResult.CachedWinners.Add(wpdSidekick);
                }
                foreach (var player in Jackal.formerJackals)
                {
                    CachedPlayerData wpdFormerJackal = new(player.Data)
                    {
                        IsImpostor = false
                    };
                    EndGameResult.CachedWinners.Add(wpdFormerJackal);
                }
                if (SchrodingersCat.team == SchrodingersCat.Team.Jackal)
                {
                    foreach (var player in SchrodingersCat.allPlayers)
                    {
                        CachedPlayerData wpdSchrodingersCat = new(player.Data)
                        {
                            IsImpostor = false
                        };
                        EndGameResult.CachedWinners.Add(wpdSchrodingersCat);
                    }
                }
            }
            // Lawyer solo win
            else if (lawyerSoloWin)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                CachedPlayerData wpd = new(Lawyer.lawyer.Data);
                EndGameResult.CachedWinners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.LawyerSoloWin;
            }
            else if (foxWin)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                foreach (var fox in Fox.players)
                {
                    CachedPlayerData wpd = new(fox.player.Data);
                    EndGameResult.CachedWinners.Add(wpd);
                }
                foreach (var immoralist in Immoralist.players)
                {
                    CachedPlayerData wpd = new(immoralist.player.Data);
                    EndGameResult.CachedWinners.Add(wpd);
                }
                AdditionalTempData.winCondition = WinCondition.FoxWin;
            }


            if (forceEnd)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                AdditionalTempData.winCondition = WinCondition.ForceEnd;
            }



            // キューピッドが悪女と勝利する
            if (akujoWin)
            {
                foreach (var p in Cupid.players)
                {
                    if (p.player.isDead()) continue;
                    if ((p.lovers1 != null && p.lovers1.isRole(RoleType.Akujo)) || (p.lovers2 != null && p.lovers2.isRole(RoleType.Akujo)))
                    {
                        CachedPlayerData wpd = new(p.player.Data);
                        EndGameResult.CachedWinners.Add(wpd);
                    }
                }
            }
            // キューピッドがLoversと勝利する
            else if (loversWin)
            {
                foreach (var cupid in Cupid.players)
                {
                    if (cupid.lovers1 != null & cupid.lovers1.isAlive() && cupid.lovers2 != null && cupid.lovers2.isAlive())
                    {
                        CachedPlayerData wpd = new(cupid.player.Data);
                        EndGameResult.CachedWinners.Add(wpd);
                    }
                }
            }


            // Possible Additional winner: Lawyer
            if (!lawyerSoloWin && Lawyer.lawyer != null && Lawyer.target != null && Lawyer.target.isAlive())
            {
                CachedPlayerData winningClient = null;
                foreach (CachedPlayerData winner in EndGameResult.CachedWinners)
                {
                    if (winner.PlayerName == Lawyer.target.Data.PlayerName)
                        winningClient = winner;
                }

                if (winningClient != null)
                { // The Lawyer wins if the client is winning (and alive, but if he wasn't the Lawyer shouldn't exist anymore)
                    if (!EndGameResult.CachedWinners.ToArray().Any(x => x.PlayerName == Lawyer.lawyer.Data.PlayerName))
                        EndGameResult.CachedWinners.Add(new CachedPlayerData(Lawyer.lawyer.Data));
                    if (Lawyer.lawyer.isAlive())
                    { // The Lawyer steals the clients win
                        EndGameResult.CachedWinners.Remove(winningClient);
                        AdditionalTempData.additionalWinConditions.Add(WinCondition.AdditionalLawyerStolenWin);
                    }
                    else
                    { // The Lawyer wins together with the client
                        AdditionalTempData.additionalWinConditions.Add(WinCondition.AdditionalLawyerBonusWin);
                    }
                }
            }

            // Extra win conditions for non-impostor roles
            if (!saboWin)
            {
                bool oppWin = false;
                foreach (var p in Opportunist.livingPlayers)
                {
                    if (!EndGameResult.CachedWinners.ToArray().Any(x => x.PlayerName == p.Data.PlayerName))
                        EndGameResult.CachedWinners.Add(new CachedPlayerData(p.Data));
                    oppWin = true;
                }
                if (oppWin)
                    AdditionalTempData.additionalWinConditions.Add(WinCondition.OpportunistWin);

                // Possible Additional winner: Pursuer
                if (Pursuer.pursuer != null && Pursuer.pursuer.isAlive())
                {
                    if (!EndGameResult.CachedWinners.ToArray().Any(x => x.PlayerName == Pursuer.pursuer.Data.PlayerName))
                        EndGameResult.CachedWinners.Add(new CachedPlayerData(Pursuer.pursuer.Data));
                    AdditionalTempData.additionalWinConditions.Add(WinCondition.AdditionalAlivePursuerWin);
                }
            }

            foreach (CachedPlayerData wpd in EndGameResult.CachedWinners)
            {
                wpd.IsDead = wpd.IsDead || AdditionalTempData.playerRoles.Any(x => x.PlayerName == wpd.PlayerName && x.Status != FinalStatus.Alive);
            }

            // Reset Settings
            RPCProcedure.resetVariables();
            EventUtility.gameEndsUpdate();
        }

        public class EndGameNavigationPatch
        {
            public static TMPro.TMP_Text textRenderer;

            [HarmonyPatch(typeof(EndGameNavigation), nameof(EndGameNavigation.ShowProgression))]
            public class ShowProgressionPatch
            {
                public static void Prefix()
                {
                    if (textRenderer != null)
                    {
                        textRenderer.gameObject.SetActive(false);
                    }
                }
            }

            [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
            public class EndGameManagerSetUpPatch
            {
                public static void Postfix(EndGameManager __instance)
                {
                    // Delete and readd PoolablePlayers always showing the name and role of the player
                    foreach (PoolablePlayer pb in __instance.transform.GetComponentsInChildren<PoolablePlayer>())
                    {
                        UnityEngine.Object.Destroy(pb.gameObject);
                    }
                    int num = Mathf.CeilToInt(7.5f);
                    List<CachedPlayerData> list = EndGameResult.CachedWinners.ToArray().ToList().OrderBy(delegate (CachedPlayerData b)
                    {
                        if (!b.IsYou)
                        {
                            return 0;
                        }
                        return -1;
                    }).ToList<CachedPlayerData>();
                    for (int i = 0; i < list.Count; i++)
                    {
                        CachedPlayerData cachedPlayerData2 = list[i];
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
                        if (cachedPlayerData2.IsDead)
                        {
                            poolablePlayer.SetBodyAsGhost();
                            poolablePlayer.SetDeadFlipX(i % 2 == 0);
                        }
                        else
                        {
                            poolablePlayer.SetFlipX(i % 2 == 0);
                        }
                        poolablePlayer.UpdateFromPlayerOutfit(cachedPlayerData2.Outfit, PlayerMaterial.MaskType.None, cachedPlayerData2.IsDead, true);

                        poolablePlayer.cosmetics.nameText.color = Color.white;
                        poolablePlayer.cosmetics.nameText.lineSpacing *= 0.7f;
                        poolablePlayer.cosmetics.nameText.transform.localScale = new Vector3(1f / vector.x, 1f / vector.y, 1f / vector.z);
                        poolablePlayer.cosmetics.nameText.transform.localPosition = new Vector3(poolablePlayer.cosmetics.nameText.transform.localPosition.x, poolablePlayer.cosmetics.nameText.transform.localPosition.y, -15f);
                        poolablePlayer.cosmetics.nameText.text = cachedPlayerData2.PlayerName;

                        foreach (var data in AdditionalTempData.playerRoles)
                        {
                            if (data.PlayerName != cachedPlayerData2.PlayerName) continue;
                            poolablePlayer.cosmetics.nameText.text += data.NameSuffix + $"\n<size=80%>{data.RoleString}</size>";
                        }
                    }

                    // Additional code
                    GameObject bonusTextObject = UnityEngine.Object.Instantiate(__instance.WinText.gameObject);
                    bonusTextObject.transform.position = new Vector3(__instance.WinText.transform.position.x, __instance.WinText.transform.position.y - 0.8f, __instance.WinText.transform.position.z);
                    bonusTextObject.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
                    textRenderer = bonusTextObject.GetComponent<TMPro.TMP_Text>();
                    textRenderer.text = "";

                    if (AdditionalTempData.isGM)
                    {
                        __instance.WinText.text = ModTranslation.getString("gmGameOver");
                        __instance.WinText.color = GM.color;
                    }

                    string bonusText = "";

                    if (AdditionalTempData.winCondition == WinCondition.JesterWin)
                    {
                        bonusText = "jesterWin";
                        textRenderer.color = Jester.color;
                        __instance.BackgroundBar.material.SetColor("_Color", Jester.color);
                    }
                    else if (AdditionalTempData.winCondition == WinCondition.ArsonistWin)
                    {
                        bonusText = "arsonistWin";
                        textRenderer.color = Arsonist.color;
                        __instance.BackgroundBar.material.SetColor("_Color", Arsonist.color);
                    }
                    else if (AdditionalTempData.winCondition == WinCondition.VultureWin)
                    {
                        bonusText = "vultureWin";
                        textRenderer.color = Vulture.color;
                        __instance.BackgroundBar.material.SetColor("_Color", Vulture.color);
                    }
                    else if (AdditionalTempData.winCondition == WinCondition.LawyerSoloWin)
                    {
                        bonusText = "lawyerWin";
                        textRenderer.color = Lawyer.color;
                        __instance.BackgroundBar.material.SetColor("_Color", Lawyer.color);
                    }
                    else if (AdditionalTempData.winCondition == WinCondition.PlagueDoctorWin)
                    {
                        bonusText = "plagueDoctorWin";
                        textRenderer.color = PlagueDoctor.color;
                        __instance.BackgroundBar.material.SetColor("_Color", PlagueDoctor.color);
                    }
                    else if (AdditionalTempData.winCondition == WinCondition.FoxWin)
                    {
                        bonusText = "foxWin";
                        textRenderer.color = Fox.color;
                        __instance.BackgroundBar.material.SetColor("_Color", Fox.color);
                    }
                    else if (AdditionalTempData.winCondition == WinCondition.PuppeteerWin)
                    {
                        bonusText = "puppeteerWin";
                        textRenderer.color = Puppeteer.color;
                        __instance.BackgroundBar.material.SetColor("_Color", Puppeteer.color);
                    }
                    else if (AdditionalTempData.winCondition == WinCondition.JekyllAndHydeWin)
                    {
                        bonusText = "jekyllAndHydeWin";
                        textRenderer.color = JekyllAndHyde.color;
                        __instance.BackgroundBar.material.SetColor("_Color", JekyllAndHyde.color);
                    }
                    else if (AdditionalTempData.winCondition == WinCondition.MoriartyWin)
                    {
                        bonusText = "moriartyWin";
                        textRenderer.color = Moriarty.color;
                        __instance.BackgroundBar.material.SetColor("_Color", Moriarty.color);
                    }
                    else if (AdditionalTempData.winCondition == WinCondition.LoversTeamWin)
                    {
                        bonusText = "crewWin";
                        textRenderer.color = Lovers.color;
                        __instance.BackgroundBar.material.SetColor("_Color", Lovers.color);
                    }
                    else if (AdditionalTempData.winCondition == WinCondition.LoversSoloWin)
                    {
                        bonusText = "loversWin";
                        textRenderer.color = Lovers.color;
                        __instance.BackgroundBar.material.SetColor("_Color", Lovers.color);
                    }
                    else if (AdditionalTempData.winCondition == WinCondition.AkujoWin)
                    {
                        bonusText = "akujoWin";
                        textRenderer.color = Akujo.color;
                        __instance.BackgroundBar.material.SetColor("_Color", Akujo.color);
                    }
                    else if (AdditionalTempData.winCondition == WinCondition.JackalWin)
                    {
                        bonusText = "jackalWin";
                        textRenderer.color = Jackal.color;
                        __instance.BackgroundBar.material.SetColor("_Color", Jackal.color);
                    }
                    else if (AdditionalTempData.winCondition == WinCondition.EveryoneDied)
                    {
                        bonusText = "everyoneDied";
                        textRenderer.color = Palette.DisabledGrey;
                        __instance.BackgroundBar.material.SetColor("_Color", Palette.DisabledGrey);
                    }
                    else if (AdditionalTempData.winCondition == WinCondition.ForceEnd)
                    {
                        bonusText = "forceEnd";
                        textRenderer.color = Palette.DisabledGrey;
                        __instance.BackgroundBar.material.SetColor("_Color", Palette.DisabledGrey);
                    }
                    else if (AdditionalTempData.winCondition == WinCondition.MiniLose)
                    {
                        bonusText = "miniDied";
                        textRenderer.color = Mini.color;
                        __instance.BackgroundBar.material.SetColor("_Color", Palette.DisabledGrey);
                    }
                    else if (AdditionalTempData.gameOverReason is GameOverReason.CrewmatesByTask or GameOverReason.CrewmatesByVote)
                    {
                        bonusText = "crewWin";
                        textRenderer.color = Palette.White;
                    }
                    else if (AdditionalTempData.gameOverReason is GameOverReason.ImpostorsByKill or GameOverReason.ImpostorsBySabotage or GameOverReason.ImpostorsByVote)
                    {
                        bonusText = "impostorWin";
                        textRenderer.color = Palette.ImpostorRed;
                    }

                    if (AdditionalTempData.winCondition == WinCondition.Default)
                    {
                        switch (AdditionalTempData.gameOverReason)
                        {
                            case GameOverReason.ImpostorDisconnect:
                                bonusText = ModTranslation.getString("impostorDisconnect");
                                textRenderer.color = Color.red;
                                break;
                            case GameOverReason.ImpostorsByKill:
                                bonusText = ModTranslation.getString("impostorByKill");
                                textRenderer.color = Color.red;
                                break;
                            case GameOverReason.ImpostorsBySabotage:
                                bonusText = ModTranslation.getString("impostorBySabotage");
                                textRenderer.color = Color.red;
                                break;
                            case GameOverReason.ImpostorsByVote:
                                bonusText = ModTranslation.getString("impostorByVote");
                                textRenderer.color = Color.red;
                                break;
                            case GameOverReason.CrewmatesByTask:
                                bonusText = ModTranslation.getString("humansByTask");
                                textRenderer.color = Color.white;
                                break;
                            case GameOverReason.CrewmateDisconnect:
                                bonusText = ModTranslation.getString("humansDisconnect");
                                textRenderer.color = Color.white;
                                break;
                            case GameOverReason.CrewmatesByVote:
                                bonusText = ModTranslation.getString("humansByVote");
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
                            case WinCondition.LoversTeamWin:
                                extraText += ModTranslation.getString("loversExtra");
                                break;
                            case WinCondition.AdditionalAlivePursuerWin:
                                extraText += ModTranslation.getString("pursuerExtra");
                                break;
                            default:
                                break;
                        }
                    }

                    if (extraText.Length > 0)
                    {
                        textRenderer.text = string.Format(ModTranslation.getString(bonusText + "Extra"), extraText);
                    }
                    else
                    {
                        textRenderer.text = ModTranslation.getString(bonusText);
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

                    if (TORMapOptions.showRoleSummary)
                    {
                        var position = Camera.main.ViewportToWorldPoint(new Vector3(0f, 1f, Camera.main.nearClipPlane));
                        GameObject roleSummary = UnityEngine.Object.Instantiate(__instance.WinText.gameObject);
                        roleSummary.transform.position = new Vector3(__instance.Navigation.ExitButton.transform.position.x + 0.1f, position.y - 0.1f, -214f);
                        roleSummary.transform.localScale = new Vector3(1f, 1f, 1f);

                        var roleSummaryText = new StringBuilder();
                        roleSummaryText.AppendLine(ModTranslation.getString("roleSummaryText"));
                        AdditionalTempData.playerRoles.Sort((x, y) =>
                        {
                            RoleInfo roleX = x.Roles.FirstOrDefault();
                            RoleInfo roleY = y.Roles.FirstOrDefault();
                            RoleType idX = roleX == null ? RoleType.NoRole : roleX.roleType;
                            RoleType idY = roleY == null ? RoleType.NoRole : roleY.roleType;

                            if (x.Status == y.Status)
                            {
                                if (idX == idY)
                                {
                                    return x.PlayerName.CompareTo(y.PlayerName);
                                }
                                return idX.CompareTo(idY);
                            }
                            return x.Status.CompareTo(y.Status);

                        });
                        Logger.info(textRenderer.text, "Result");
                        bool plagueExists = AdditionalTempData.playerRoles.Any(x => x.Roles.Contains(RoleInfo.plagueDoctor));
                        Logger.info("----------Game Result-----------", "Result");
                        foreach (var data in AdditionalTempData.playerRoles)
                        {
                            if (data.PlayerName == "") continue;
                            var taskInfo = data.TasksTotal > 0 ? $"<color=#FAD934FF>{data.TasksCompleted}/{data.TasksTotal}</color>" : "";
                            string aliveDead = ModTranslation.getString("roleSummary" + data.Status.ToString(), def: "-");
                            string result = $"{data.PlayerName + data.NameSuffix}<pos=18.5%>{taskInfo}<pos=25%>{aliveDead}<pos=34%>{data.RoleString}";
                            if (plagueExists && !data.Roles.Contains(RoleInfo.plagueDoctor))
                            {
                                result += "<pos=52.5%>";
                                if (AdditionalTempData.plagueDoctorInfected.ContainsKey(data.PlayerId))
                                {
                                    result += Helpers.cs(Color.red, ModTranslation.getString("plagueDoctorInfectedText"));
                                }
                                else
                                {
                                    float progress = AdditionalTempData.plagueDoctorProgress.ContainsKey(data.PlayerId) ? AdditionalTempData.plagueDoctorProgress[data.PlayerId] : 0f;
                                    result += PlagueDoctor.getProgressString(progress);

                                }
                            }
                            roleSummaryText.AppendLine(result);
                            Logger.info(result, "Result");
                        }
                        Logger.info("--------------------------------", "Result");

                        TMPro.TMP_Text roleSummaryTextMesh = roleSummary.GetComponent<TMPro.TMP_Text>();
                        roleSummaryTextMesh.alignment = TMPro.TextAlignmentOptions.TopLeft;
                        roleSummaryTextMesh.color = Color.white;
                        roleSummaryTextMesh.outlineWidth *= 1.2f;
                        roleSummaryTextMesh.fontSizeMin = 1.25f;
                        roleSummaryTextMesh.fontSizeMax = 1.25f;
                        roleSummaryTextMesh.fontSize = 1.25f;

                        var roleSummaryTextMeshRectTransform = roleSummaryTextMesh.GetComponent<RectTransform>();
                        roleSummaryTextMeshRectTransform.anchoredPosition = new Vector2(position.x + 3.5f, position.y - 0.1f);
                        roleSummaryTextMesh.text = roleSummaryText.ToString();

                        // webhook
                        if (AmongUsClient.Instance.AmHost)
                        {
                            List<Dictionary<string, object>> msg = new();
                            Dictionary<string, object> embeds = new();
                            List<Dictionary<string, object>> fields = new();
                            foreach (var data in AdditionalTempData.playerRoles)
                            {
                                if (data.PlayerName == "") continue;
                                // var taskInfo = data.TasksTotal > 0 ? $"{data.TasksCompleted}/{data.TasksTotal}" : "タスクなし";
                                var taskInfo = string.Format("{0:D2}", data.TasksCompleted) + "/" + string.Format("{0:D2}", data.TasksTotal);
                                string aliveDead = ModTranslation.getString("roleSummary" + data.Status.ToString(), def: "-");
                                string result = "";
                                result += EndGameResult.CachedWinners.ToArray().Count(x => x.PlayerName == data.PlayerName) != 0 ? ":crown: | " : ":skull: | ";
                                result += string.Format("{0,-6} | {1,-2} | {2}", taskInfo, aliveDead, data.RoleString);
                                if (plagueExists && !data.Roles.Contains(RoleInfo.plagueDoctor))
                                {
                                    result += " | ";
                                    if (AdditionalTempData.plagueDoctorInfected.ContainsKey(data.PlayerId))
                                    {
                                        result += Helpers.cs(Color.red, ModTranslation.getString("plagueDoctorInfectedText"));
                                    }
                                    else
                                    {
                                        float progress = AdditionalTempData.plagueDoctorProgress.ContainsKey(data.PlayerId) ? AdditionalTempData.plagueDoctorProgress[data.PlayerId] : 0f;
                                        result += PlagueDoctor.getProgressString(progress);
                                    }
                                }
                                Dictionary<string, object> item = new();
                                item.Add("name", Webhook.colorIdToEmoji(data.ColorId) + data.PlayerName + data.NameSuffix);
                                item.Add("value", Regex.Replace(result, @"<[^>]*>", ""));
                                // item.Add("inline", true);
                                fields.Add(item);
                            }

                            embeds.Add("fields", fields);
                            msg.Add(embeds);
                            Webhook.post(msg, bonusText, extraText);
                        }
                    }
                    AdditionalTempData.clear();
                }
            }

            [HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
            class CheckEndCriteriaPatch
            {
                public static bool Prefix(ShipStatus __instance)
                {
                    if (!GameData.Instance) return false;
                    if (DestroyableSingleton<TutorialManager>.InstanceExists) return true; // InstanceExists | Don't check Custom Criteria when in Tutorial
                    if (FastDestroyableSingleton<HudManager>.Instance.IsIntroDisplayed) return false;

                    var statistics = new PlayerStatistics(__instance);
                    if (CheckAndEndGameForMiniLose(__instance)) return false;
                    if (CheckAndEndGameForJesterWin(__instance)) return false;
                    if (CheckAndEndGameForLawyerMeetingWin(__instance)) return false;
                    if (CheckAndEndGameForArsonistWin(__instance)) return false;
                    if (CheckAndEndGameForVultureWin(__instance)) return false;
                    if (CheckAndEndGameForPlagueDoctorWin(__instance)) return false;
                    if (CheckAndEndGameForPuppeteerWin(__instance)) return false;
                    if (CheckAndEndGameForJekyllAndHydeWin(__instance, statistics)) return false;
                    if (CheckAndEndGameForMoriartyWin(__instance, statistics)) return false;
                    if (CheckAndEndGameForSabotageWin(__instance)) return false;
                    if (CheckAndEndGameForTaskWin(__instance)) return false;
                    if (CheckAndEndGameForLoverWin(__instance, statistics)) return false;
                    if (CheckAndEndGameForAkujoWin(__instance, statistics)) return false;
                    if (CheckAndEndGameForJackalWin(__instance, statistics)) return false;
                    if (CheckAndEndGameForImpostorWin(__instance, statistics)) return false;
                    if (CheckAndEndGameForCrewmateWin(__instance, statistics)) return false;
                    return false;
                }

                private static bool CheckAndEndGameForMiniLose(ShipStatus __instance)
                {
                    if (Mini.triggerMiniLose)
                    {
                        UncheckedEndGame(CustomGameOverReason.MiniLose);
                        return true;
                    }
                    return false;
                }

                private static bool CheckAndEndGameForJesterWin(ShipStatus __instance)
                {
                    if (Jester.triggerJesterWin)
                    {
                        UncheckedEndGame(CustomGameOverReason.JesterWin);
                        return true;
                    }
                    return false;
                }

                private static bool CheckAndEndGameForArsonistWin(ShipStatus __instance)
                {
                    if (Arsonist.triggerArsonistWin)
                    {
                        UncheckedEndGame(CustomGameOverReason.ArsonistWin);
                        return true;
                    }
                    return false;
                }

                private static bool CheckAndEndGameForVultureWin(ShipStatus __instance)
                {
                    if (Vulture.triggerVultureWin)
                    {
                        UncheckedEndGame(CustomGameOverReason.VultureWin);
                        return true;
                    }
                    return false;
                }

                private static bool CheckAndEndGameForLawyerMeetingWin(ShipStatus __instance)
                {
                    if (Lawyer.triggerLawyerWin)
                    {
                        UncheckedEndGame(CustomGameOverReason.LawyerSoloWin);
                        return true;
                    }
                    return false;
                }

                private static bool CheckAndEndGameForPlagueDoctorWin(ShipStatus __instance)
                {
                    if (PlagueDoctor.triggerPlagueDoctorWin)
                    {
                        UncheckedEndGame(CustomGameOverReason.PlagueDoctorWin);
                        return true;
                    }
                    return false;
                }
                private static bool CheckAndEndGameForPuppeteerWin(ShipStatus __instance)
                {
                    if (Puppeteer.triggerPuppeteerWin)
                    {
                        UncheckedEndGame(CustomGameOverReason.PuppeteerWin);
                        return true;
                    }
                    return false;
                }
                private static bool CheckAndEndGameForJekyllAndHydeWin(ShipStatus __instance, PlayerStatistics statistics)
                {
                    if (JekyllAndHyde.triggerWin)
                    {
                        UncheckedEndGame(CustomGameOverReason.JekyllAndHydeWin);
                        return true;
                    }

                    if (statistics.JekyllAndHydeAlive >= statistics.TotalAlive - statistics.JekyllAndHydeAlive - statistics.FoxAlive &&
                        statistics.TeamImpostorsAlive == 0 && statistics.TeamJackalAlive == 0 && statistics.MoriartyAlive == 0 &&
                        (statistics.JekyllAndHydeLovers == 0 || statistics.JekyllAndHydeLovers >= statistics.CouplesAlive * 2)
                       )
                    {
                        UncheckedEndGame(CustomGameOverReason.JekyllAndHydeWin);
                        return true;
                    }

                    return false;
                }
                private static bool CheckAndEndGameForMoriartyWin(ShipStatus __instance, PlayerStatistics statistics)
                {
                    // Moriartyが生存していること
                    if (!Moriarty.isAlive()) return false;
                    if (statistics.MoriartyAlive >= statistics.TotalAlive - statistics.MoriartyAlive - statistics.FoxAlive &&
                        statistics.TeamImpostorsAlive == 0 && statistics.TeamJackalAlive == 0 && statistics.JekyllAndHydeAlive == 0 &&
                        (statistics.MoriartyLovers == 0 || statistics.MoriartyLovers >= statistics.CouplesAlive * 2)
                       )
                    {
                        UncheckedEndGame(CustomGameOverReason.MoriartyWin);
                        return true;
                    }

                    return false;
                }


                private static bool CheckAndEndGameForSabotageWin(ShipStatus __instance)
                {
                    if (MapUtilities.Systems == null) return false;
                    var systemType = MapUtilities.Systems.ContainsKey(SystemTypes.LifeSupp) ? MapUtilities.Systems[SystemTypes.LifeSupp] : null;
                    if (systemType != null)
                    {
                        LifeSuppSystemType lifeSuppSystemType = systemType.TryCast<LifeSuppSystemType>();
                        if (lifeSuppSystemType != null && lifeSuppSystemType.Countdown < 0f)
                        {
                            EndGameForSabotage(__instance);
                            lifeSuppSystemType.Countdown = 10000f;
                            return true;
                        }
                    }
                    var systemType2 = MapUtilities.Systems.ContainsKey(SystemTypes.Reactor) ? MapUtilities.Systems[SystemTypes.Reactor] : null;
                    if (systemType2 == null)
                    {
                        systemType2 = MapUtilities.Systems.ContainsKey(SystemTypes.Laboratory) ? MapUtilities.Systems[SystemTypes.Laboratory] : null;
                    }
                    if (systemType2 != null)
                    {
                        ICriticalSabotage criticalSystem = systemType2.TryCast<ICriticalSabotage>();
                        if (criticalSystem != null && criticalSystem.Countdown < 0f)
                        {
                            EndGameForSabotage(__instance);
                            criticalSystem.ClearSabotage();
                            return true;
                        }
                    }
                    return false;
                }
                private static bool CheckAndEndGameForTaskWin(ShipStatus __instance)
                {
                    if (GameData.Instance.TotalTasks > 0 && GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks)
                    {
                        UncheckedEndGame(GameOverReason.CrewmatesByTask);
                        return true;
                    }

                    if (Fox.exists && !Fox.crewWinsByTasks)
                    {
                        // 狐生存かつタスク完了時に生存中のクルーがタスクを全て終わらせたら勝ち
                        // 死んだプレイヤーが意図的にタスクを終了させないのを防止するため
                        bool isFoxAlive = Fox.isFoxAlive();
                        bool isFoxCompletedtasks = Fox.isFoxCompletedTasks();
                        int numDeadPlayerUncompletedTasks = 0;
                        foreach (var player in PlayerControl.AllPlayerControls.ToArray())
                        {
                            foreach (var task in player.Data.Tasks)
                            {
                                if (player.Data.IsDead && player.isCrew() && !player.hasModifier(ModifierType.Madmate) && !player.hasModifier(ModifierType.CreatedMadmate))
                                {
                                    if (!task.Complete)
                                    {
                                        numDeadPlayerUncompletedTasks++;
                                    }
                                }
                            }
                        }

                        if (isFoxCompletedtasks && isFoxAlive && GameData.Instance.TotalTasks > 0 && GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks + numDeadPlayerUncompletedTasks)
                        {
                            UncheckedEndGame(GameOverReason.CrewmatesByTask);
                            return true;
                        }
                    }

                    return false;
                }

                private static bool CheckAndEndGameForLoverWin(ShipStatus __instance, PlayerStatistics statistics)
                {
                    if (statistics.CouplesAlive == 1 && statistics.TotalAlive <= 3)
                    {
                        UncheckedEndGame(CustomGameOverReason.LoversWin);
                        return true;
                    }
                    return false;
                }

                private static bool CheckAndEndGameForAkujoWin(ShipStatus __instance, PlayerStatistics statistics)
                {
                    // if we have a majority, akujo wins, same as lovers
                    if (Akujo.numAlive == 1 && statistics.TotalAlive <= 3)
                    {
                        UncheckedEndGame(CustomGameOverReason.AkujoWin);
                        return true;
                    }
                    return false;
                }

                private static bool CheckAndEndGameForJackalWin(ShipStatus __instance, PlayerStatistics statistics)
                {
                    if (statistics.TeamJackalAlive >= statistics.TotalAlive - statistics.TeamJackalAlive - statistics.FoxAlive &&
                        statistics.TeamImpostorsAlive == 0 && statistics.JekyllAndHydeAlive == 0 && statistics.MoriartyAlive == 0 &&
                        (statistics.TeamJackalLovers == 0 || statistics.TeamJackalLovers >= statistics.CouplesAlive * 2)
                       )
                    {
                        UncheckedEndGame(CustomGameOverReason.TeamJackalWin);
                        return true;
                    }
                    return false;
                }

                private static bool CheckAndEndGameForImpostorWin(ShipStatus __instance, PlayerStatistics statistics)
                {
                    if (statistics.TeamImpostorsAlive >= statistics.TotalAlive - statistics.TeamImpostorsAlive - statistics.FoxAlive &&
                        statistics.TeamJackalAlive == 0 && statistics.JekyllAndHydeAlive == 0 && statistics.MoriartyAlive == 0 &&
                        (statistics.TeamImpostorLovers == 0 || statistics.TeamImpostorLovers >= statistics.CouplesAlive * 2)
                       )
                    {
                        var endReason = GameData.LastDeathReason switch
                        {
                            DeathReason.Exile => GameOverReason.ImpostorsByVote,
                            DeathReason.Kill => GameOverReason.ImpostorsByKill,
                            _ => GameOverReason.ImpostorsByVote,
                        };
                        UncheckedEndGame(endReason);
                        return true;
                    }
                    return false;
                }

                private static bool CheckAndEndGameForCrewmateWin(ShipStatus __instance, PlayerStatistics statistics)
                {
                    if (statistics.TeamCrew > 0 && statistics.TeamImpostorsAlive == 0 && statistics.TeamJackalAlive == 0 && statistics.JekyllAndHydeAlive == 0 && statistics.MoriartyAlive == 0)
                    {
                        UncheckedEndGame(GameOverReason.CrewmatesByVote);
                        return true;
                    }
                    return false;
                }

                private static void EndGameForSabotage(ShipStatus __instance)
                {
                    UncheckedEndGame(GameOverReason.ImpostorsBySabotage);
                    return;
                }

                private static void UncheckedEndGame(GameOverReason reason)
                {
                    GameManager.Instance.RpcEndGame(reason, false);
                    /*MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedEndGame, Hazel.SendOption.Reliable, -1);
                    writer.Write((byte)reason);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.uncheckedEndGame((byte)reason);*/
                }

                private static void UncheckedEndGame(CustomGameOverReason reason)
                {
                    UncheckedEndGame((GameOverReason)reason);
                }
            }

            internal class PlayerStatistics
            {
                public int TeamImpostorsAlive { get; set; }
                public int TeamJackalAlive { get; set; }
                public int TeamLoversAlive { get; set; }
                public int CouplesAlive { get; set; }
                public int TeamCrew { get; set; }
                public int NeutralAlive { get; set; }
                public int TotalAlive { get; set; }
                public int TeamImpostorLovers { get; set; }
                public int TeamJackalLovers { get; set; }
                public int JekyllAndHydeLovers { get; set; }
                public int FoxAlive { get; set; }
                public int JekyllAndHydeAlive { get; set; }
                public int MoriartyAlive { get; set; }
                public int MoriartyLovers { get; set; }

                public PlayerStatistics(ShipStatus __instance)
                {
                    GetPlayerCounts();
                }

                private bool isLover(NetworkedPlayerInfo p)
                {
                    foreach (var couple in Lovers.couples)
                    {
                        if (p.PlayerId == couple.lover1.PlayerId || p.PlayerId == couple.lover2.PlayerId) return true;
                    }
                    return false;
                }

                private void GetPlayerCounts()
                {
                    int numJackalAlive = 0;
                    int numImpostorsAlive = 0;
                    int numTotalAlive = 0;
                    int numNeutralAlive = 0;
                    int numCrew = 0;
                    int numJekyllAndHydeAlive = JekyllAndHyde.livingPlayers.Count;
                    int numMoriartyAlive = Moriarty.livingPlayers.Count;

                    int numLoversAlive = 0;
                    int numCouplesAlive = 0;
                    int impLovers = 0;
                    int jackalLovers = 0;


                    foreach (var playerInfo in GameData.Instance.AllPlayers)
                    {
                        if (!playerInfo.Disconnected)
                        {
                            if (playerInfo.Object.isCrew()) numCrew++;
                            if (!playerInfo.IsDead && !playerInfo.Object.isGM())
                            {
                                numTotalAlive++;

                                bool lover = isLover(playerInfo);
                                if (lover) numLoversAlive++;

                                if (playerInfo.Role.IsImpostor)
                                {
                                    numImpostorsAlive++;
                                    if (lover) impLovers++;
                                }
                                if (Jackal.jackal != null && Jackal.jackal.PlayerId == playerInfo.PlayerId)
                                {
                                    numJackalAlive++;
                                    if (lover) jackalLovers++;
                                }
                                if (Sidekick.sidekick != null && Sidekick.sidekick.PlayerId == playerInfo.PlayerId)
                                {
                                    numJackalAlive++;
                                    if (lover) jackalLovers++;
                                }

                                if (SchrodingersCat.team == SchrodingersCat.Team.Jackal)
                                {
                                    if (Helpers.playerById(playerInfo.PlayerId).isRole(RoleType.SchrodingersCat))
                                    {
                                        numJackalAlive++;
                                    }
                                }

                                if (playerInfo.Object.isNeutral()) numNeutralAlive++;
                            }
                        }
                    }

                    foreach (var couple in Lovers.couples)
                    {
                        if (couple.alive) numCouplesAlive++;
                    }

                    // In the special case of Mafia being enabled, but only the janitor's left alive,
                    // count it as zero impostors alive bc they can't actually do anything.
                    if (Godfather.godfather?.isDead() == true && Mafioso.mafioso?.isDead() == true && Janitor.janitor?.isDead() == false)
                    {
                        numImpostorsAlive = 0;
                    }

                    // 爆弾魔を一人としてカウントする、猫の自爆中はインポスターのカウントを一人減らす
                    // PlayerControl.isAlive()を使うと会議での追放時にカウントバグが発生するので使用禁止
                    if (SchrodingersCat.killer != null && !(SchrodingersCat.killer.Data.IsDead || SchrodingersCat.killer.Data.Disconnected) && SchrodingersCat.team == SchrodingersCat.Team.Impostor)
                    {
                        numImpostorsAlive--;
                    }
                    else if (BomberA.isAlive() && BomberB.isAlive() && BomberA.countAsOne)
                    {
                        numImpostorsAlive--;
                        numTotalAlive--;
                    }
                    else if (MimicK.isAlive() && MimicA.isAlive() && MimicK.countAsOne)
                    {
                        numImpostorsAlive--;
                        numTotalAlive--;
                    }


                    // 猫の自爆中はジャッカルのカウントを一人減らす
                    if (SchrodingersCat.killer != null && !(SchrodingersCat.killer.Data.IsDead || SchrodingersCat.killer.Data.Disconnected) && SchrodingersCat.team == SchrodingersCat.Team.Jackal)
                    {
                        numJackalAlive--;
                    }
                    if (SchrodingersCat.livingPlayers.Count > 0 && SchrodingersCat.team == SchrodingersCat.Team.JekyllAndHyde)
                    {
                        numJekyllAndHydeAlive = JekyllAndHyde.livingPlayers.Count + SchrodingersCat.livingPlayers.Count;
                    }
                    if (SchrodingersCat.livingPlayers.Count > 0 && SchrodingersCat.team == SchrodingersCat.Team.Moriarty)
                    {
                        numMoriartyAlive = Moriarty.livingPlayers.Count + SchrodingersCat.livingPlayers.Count;
                    }

                    // 猫の自爆中はジキルとハイドのカウントを一人減らす
                    if (SchrodingersCat.killer != null && !(SchrodingersCat.killer.Data.IsDead || SchrodingersCat.killer.Data.Disconnected) && SchrodingersCat.team == SchrodingersCat.Team.JekyllAndHyde)
                    {
                        --numJekyllAndHydeAlive;
                    }

                    if (SchrodingersCat.killer != null && !(SchrodingersCat.killer.Data.IsDead || SchrodingersCat.killer.Data.Disconnected) && SchrodingersCat.team == SchrodingersCat.Team.Moriarty)
                    {
                        --numMoriartyAlive;
                    }

                    // 人形使いのダミーはカウントしない
                    if (Puppeteer.dummy != null)
                    {
                        numTotalAlive--;
                    }

                    // モリアーティに洗脳されているユーザーはモリアーティー陣営としてカウントする
                    if (Moriarty.target != null)
                    {
                        if (Moriarty.target.isImpostor() || (Moriarty.target.isRole(RoleType.SchrodingersCat) && SchrodingersCat.team == SchrodingersCat.Team.Impostor)) numImpostorsAlive -= 1;
                        else if (Moriarty.target.isRole(RoleType.Jackal) || Moriarty.target.isRole(RoleType.Sidekick) || (Moriarty.target.isRole(RoleType.SchrodingersCat) && SchrodingersCat.team == SchrodingersCat.Team.Jackal)) numJackalAlive -= 1;
                        else if (Moriarty.target.isRole(RoleType.JekyllAndHyde) || (Moriarty.target.isRole(RoleType.SchrodingersCat) && SchrodingersCat.team == SchrodingersCat.Team.JekyllAndHyde)) numJekyllAndHydeAlive -= 1;
                        else numCrew -= 1;
                        numMoriartyAlive += 1;
                    }

                    TeamCrew = numCrew;
                    TeamJackalAlive = numJackalAlive;
                    TeamImpostorsAlive = numImpostorsAlive;
                    TeamLoversAlive = numLoversAlive;
                    NeutralAlive = numNeutralAlive;
                    TotalAlive = numTotalAlive;
                    CouplesAlive = numCouplesAlive;
                    TeamImpostorLovers = impLovers;
                    TeamJackalLovers = jackalLovers;
                    FoxAlive = Fox.livingPlayers.Count;
                    JekyllAndHydeAlive = numJekyllAndHydeAlive;
                    JekyllAndHydeLovers = JekyllAndHyde.countLovers();
                    MoriartyAlive = numMoriartyAlive;
                    MoriartyLovers = Moriarty.countLovers();
                }
            }
        }
    }
}
