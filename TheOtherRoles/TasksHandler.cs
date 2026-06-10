using HarmonyLib;
using Hazel;
using System;
using System.Linq;
using TheOtherRoles.Patches;
using TheOtherRoles.Roles;
using TheOtherRoles.Utilities;

namespace TheOtherRoles {
    [HarmonyPatch]
    public static class TasksHandler {

        public static Tuple<int, int> taskInfo(NetworkedPlayerInfo playerInfo, bool isResult = false) {
            int TotalTasks = 0;
            int CompletedTasks = 0;
            if (playerInfo != null && !playerInfo.Disconnected && playerInfo.Tasks != null &&
                playerInfo.Object &&
                playerInfo.Role && playerInfo.Role.TasksCountTowardProgress &&
                !playerInfo.Object.hasFakeTasks() && !playerInfo.Role.IsImpostor
                ) {
                bool isTaskMasterEx = playerInfo.Object.isRole(RoleId.TaskMaster) && TaskMaster.isTaskComplete;
                if (!isResult && isTaskMasterEx)
                {
                    TotalTasks = CompletedTasks = GameOptionsManager.Instance.currentNormalGameOptions.NumCommonTasks + 
                    GameOptionsManager.Instance.currentNormalGameOptions.NumShortTasks + 
                    GameOptionsManager.Instance.currentNormalGameOptions.NumLongTasks;
                }
                else
                {
                    foreach (var playerInfoTask in playerInfo.Tasks.GetFastEnumerator())
                    {
                        if (playerInfoTask.Complete) CompletedTasks++;
                        TotalTasks++;
                    }
                }
            }
            return Tuple.Create(CompletedTasks, TotalTasks);
        }

        [HarmonyPatch(typeof(GameData), nameof(GameData.RecomputeTaskCounts))]
        private static class GameDataRecomputeTaskCountsPatch {
            private static bool Prefix(GameData __instance) {
               

                var totalTasks = 0;
                var completedTasks = 0;
                
                foreach (var playerInfo in GameData.Instance.AllPlayers.GetFastEnumerator())
                {
                    if (playerInfo.Object
                        && (playerInfo.Object.hasAliveKillingLover() // Tasks do not count if a Crewmate has an alive killing Lover
                        || playerInfo.Object.isRole(RoleId.Lawyer) // Tasks of the Lawyer do not count
                        || (playerInfo.Object.isRole(RoleId.Pursuer) && playerInfo.IsDead) // Tasks of the Pursuer only count, if he's alive
                        || (playerInfo.Object.isRole(RoleId.Shifter) && Shifter.isNeutral) // Chain-Shifter has tasks, but they don't count
                        || playerInfo.Object.isRole(RoleId.Thief) // Thief's tasks only count after joining crew team as sheriff (and then the thief is not the thief anymore)
                        || (Madmate.hasTasks && Madmate.madmate.Any(x => x.PlayerId == playerInfo.PlayerId))
                        || (CreatedMadmate.hasTasks && CreatedMadmate.createdMadmate.Any(x => x.PlayerId == playerInfo.PlayerId))
                        || (SchrodingersCat.hideRole && playerInfo.Object.isRole(RoleId.SchrodingersCat))
                        || playerInfo.Object.isRole(RoleId.JekyllAndHyde)
                        || playerInfo.Object.isRole(RoleId.Fox))
                        )
                        continue;
                    var (playerCompleted, playerTotal) = taskInfo(playerInfo);
                    totalTasks += playerTotal;
                    completedTasks += playerCompleted;
                }
                
                __instance.TotalTasks = totalTasks;
                __instance.CompletedTasks = completedTasks;
                return false;
            }
        }

        [HarmonyPatch(typeof(GameData), nameof(GameData.CompleteTask))]
        private static class GameDataCompleteTaskPatch
        {
            private static void Postfix(GameData __instance, [HarmonyArgument(0)] PlayerControl pc, [HarmonyArgument(1)] uint taskId)
            {
                if (AmongUsClient.Instance.AmHost && !pc.Data.IsDead && pc.isRole(RoleId.TaskMaster))
                {
                    byte clearTasks = 0;
                    for (int i = 0; i < pc.Data.Tasks.Count; ++i)
                    {
                        if (pc.Data.Tasks[i].Complete)
                            ++clearTasks;
                    }
                    bool allTasksCompleted = clearTasks == pc.Data.Tasks.Count;
                    Action action = () => {
                        if (TaskMaster.isTaskComplete)
                        {
                            byte clearTasks = 0;
                            for (int i = 0; i < pc.Data.Tasks.Count; ++i)
                            {
                                if (pc.Data.Tasks[i].Complete)
                                    ++clearTasks;
                            }
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.TaskMasterUpdateExTasks, Hazel.SendOption.Reliable, -1);
                            writer.Write(clearTasks);
                            writer.Write((byte)pc.Data.Tasks.Count);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.taskMasterUpdateExTasks(clearTasks, (byte)pc.Data.Tasks.Count);
                        }
                    };

                    if (allTasksCompleted)
                    {
                        if (!TaskMaster.isTaskComplete)
                        {
                            byte[] taskTypeIds = TaskMasterTaskHelper.GetTaskMasterTasks(pc);
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.TaskMasterSetExTasks, Hazel.SendOption.Reliable, -1);
                            writer.Write(pc.PlayerId);
                            writer.Write(byte.MaxValue);
                            writer.Write(taskTypeIds);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.taskMasterSetExTasks(pc.PlayerId, byte.MaxValue, taskTypeIds);
                            action();
                        }
                        else if (!TaskMaster.triggerTaskMasterWin)
                        {
                            action();
                            RPCProcedure.UnlockTaskMasterAch.Invoke();
                            TaskMaster.triggerTaskMasterWin = true;
                        }
                    }
                    else
                    {
                        action();
                    }
                }

                if (!PlayerControl.LocalPlayer.Data.IsDead && pc.isRole(RoleId.Snitch))
                {
                    var (taskComplete, taskTotal) = taskInfo(pc.Data);
                    if (taskTotal - taskComplete == Snitch.taskCountForReveal)
                    {
                        if (PlayerControl.LocalPlayer == pc) {
                            Snitch.snitchMessage("snitchTaskLeftNotification");
                        }
                        else if (PlayerControl.LocalPlayer.Data.Role.IsImpostor || (Snitch.IsEvilRole(PlayerControl.LocalPlayer) && Snitch.includeTeamEvil)) {
                            Snitch.snitchMessage("snitchAlmostFinishNotification");
                        }
                    }
                    if (PlayerControl.LocalPlayer == pc && taskTotal == taskComplete)
                    {
                        Snitch.snitchMessage("snitchAllTasksCompleteNotification");
                    }
                }
            }
        }

    }
}
