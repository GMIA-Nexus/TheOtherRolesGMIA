using System;
using HarmonyLib;
using static TheOtherRoles.TheOtherRolesGM;

namespace TheOtherRoles
{
    [HarmonyPatch]
    public static class TasksHandler
    {

        [HarmonyPatch(typeof(NormalPlayerTask), nameof(NormalPlayerTask.FixedUpdate))]
        public static class NormalPlayerTaskPatch
        {
            public static void Postfix(NormalPlayerTask __instance)
            {
                if (!TORMapOptions.hideTaskArrows) return;
                bool commsActive = false;
                foreach (PlayerTask t in PlayerControl.LocalPlayer.myTasks)
                {
                    if (t.TaskType == TaskTypes.FixComms)
                    {
                        commsActive = true;
                        break;
                    }
                }
                if (!(__instance.TaskType == TaskTypes.SortRecords) && !commsActive)
                {
                    bool showArrows = !__instance.IsComplete && __instance.TaskStep > 0;
                    __instance.Arrow?.gameObject?.SetActive(showArrows);
                }
            }
        }

        [HarmonyPatch(typeof(AirshipUploadTask), nameof(AirshipUploadTask.FixedUpdate))]
        public static class AirshipUploadTaskPatch
        {
            public static void Postfix(AirshipUploadTask __instance)
            {
                if (!TORMapOptions.hideTaskArrows) return;
                bool commsActive = false;
                foreach (PlayerTask t in PlayerControl.LocalPlayer.myTasks)
                {
                    if (t.TaskType == TaskTypes.FixComms)
                    {
                        commsActive = true;
                        break;
                    }
                }
                if (!commsActive)
                {
                    bool showArrows = !TORMapOptions.hideTaskArrows && !__instance.IsComplete && __instance.TaskStep > 0;
                    __instance.Arrows?.DoIf(x => x != null, x => x.gameObject?.SetActive(showArrows));
                }
            }
        }

        [HarmonyPatch(typeof(NormalPlayerTask), nameof(NormalPlayerTask.UpdateArrowAndLocation))]
        public static class NormalPlayerTaskUpdateArrowPatch
        {
            public static void Postfix(NormalPlayerTask __instance)
            {
                if (TORMapOptions.hideTaskArrows)
                {
                    __instance.Arrow?.gameObject?.SetActive(false);
                }
            }
        }

        [HarmonyPatch(typeof(AirshipUploadTask), nameof(AirshipUploadTask.UpdateArrowAndLocation))]
        public static class AirshipUploadTaskUpdateArrowPatch
        {
            public static void Postfix(AirshipUploadTask __instance)
            {
                if (TORMapOptions.hideTaskArrows)
                {
                    __instance.Arrows?.DoIf(x => x != null, x => x.gameObject?.SetActive(false));
                }
            }
        }

        public static Tuple<int, int> taskInfo(NetworkedPlayerInfo playerInfo)
        {
            int TotalTasks = 0;
            int CompletedTasks = 0;
            if (playerInfo != null && !playerInfo.Disconnected && playerInfo.Tasks != null &&
                playerInfo.Object &&
                (GameOptionsManager.Instance.currentNormalGameOptions.GhostsDoTasks || !playerInfo.IsDead) &&
                playerInfo.Role && playerInfo.Role.TasksCountTowardProgress &&
                !(playerInfo.Object.isGM() && !GM.hasTasks) &&
                !(playerInfo.Object.isLovers() && !Lovers.hasTasks) &&
                !playerInfo.Object.hasFakeTasks() && !playerInfo.Role.IsImpostor
                )
            {

                foreach (var playerInfoTask in playerInfo.Tasks)
                {
                    TotalTasks++;
                    if (playerInfoTask.Complete) CompletedTasks++;
                }
            }
            return Tuple.Create(CompletedTasks, TotalTasks);
        }

        [HarmonyPatch(typeof(GameData), nameof(GameData.RecomputeTaskCounts))]
        private static class GameDataRecomputeTaskCountsPatch
        {
            private static bool Prefix(GameData __instance)
            {
                __instance.TotalTasks = 0;
                __instance.CompletedTasks = 0;
                foreach (var playerInfo in GameData.Instance.AllPlayers)
                {
                    if (playerInfo.Object &&
                        ((playerInfo.Object?.isLovers() == true && !Lovers.tasksCount) ||
                         (playerInfo.PlayerId == Shifter.shifter?.PlayerId && Shifter.isNeutral) || // Neutral shifter has tasks, but they don't count
                          playerInfo.PlayerId == Lawyer.lawyer?.PlayerId || // Tasks of the Lawyer do not count
                         (playerInfo.PlayerId == Pursuer.pursuer?.PlayerId && Pursuer.pursuer.Data.IsDead) || // Tasks of the Pursuer only count, if he's alive
                          playerInfo.Object?.isRole(RoleType.Fox) == true ||
                          playerInfo.Object?.isRole(RoleType.JekyllAndHyde) == true ||
                         (Madmate.hasTasks && playerInfo.Object?.hasModifier(ModifierType.Madmate) == true) ||
                         (CreatedMadmate.hasTasks && playerInfo.Object?.hasModifier(ModifierType.CreatedMadmate) == true)
                        )
                    )
                        continue;
                    var (playerCompleted, playerTotal) = taskInfo(playerInfo);
                    __instance.TotalTasks += playerTotal;
                    __instance.CompletedTasks += playerCompleted;
                }
                return false;
            }
        }


    }
}
