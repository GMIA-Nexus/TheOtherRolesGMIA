using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HarmonyLib;
using TMPro;
using TheOtherRoles.Modules;
using TheOtherRoles;
using static TheOtherRoles.TheOtherRoles;
using static TheOtherRoles.TheOtherRolesGM;


namespace TheOtherRoles.Objects
{
    public class CustomNormalPlayerTask
    {
        public static List<Shrine> allTasks = new();
        public static List<int> completedConsoles = new();
        public static uint counter = 0;
        NormalPlayerTask npt;

        public CustomNormalPlayerTask(string name, Il2CppSystem.Type taskType, int maxStep, byte[] Data, SystemTypes startAt, bool showTaskStep)
        {
            var task = new GameObject(name).AddComponent(taskType).TryCast<Minigame>();
            task.enabled = false;
            if (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 0)
            {
                npt = GameObject.Instantiate(MapUtilities.CachedShipStatus.ShortTasks[1]);
            }
            else
            {
                npt = GameObject.Instantiate(MapUtilities.CachedShipStatus.ShortTasks[0]);
            }
            npt.LocationDirty = true;
            npt.HasLocation = true;
            npt.MaxStep = maxStep;
            npt.Data = Data;
            npt.Id = 100 + counter;
            npt.StartAt = startAt;
            npt.ShowTaskStep = showTaskStep;
            var arrow = new GameObject("Arrow") { layer = 5 };
            npt.Arrow = arrow.AddComponent<ArrowBehaviour>();
            npt.Arrow.image = npt.Arrow.gameObject.AddComponent<SpriteRenderer>();
            npt.Arrow.image.sprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Arrow.png", 200f);
            npt.Arrow.image.color = Color.yellow;
            npt.Arrow.gameObject.SetActive(false);
            npt.MinigamePrefab = task;
            npt.name = name;
            npt.TaskType = TaskTypes.None;
            task.MyNormTask = npt;
        }

        public void addTaskToPlayer(byte playerId)
        {
            Helpers.getPlayerById(playerId).myTasks.Add(npt);
            NetworkedPlayerInfo pi = GameData.Instance.GetPlayerById(playerId);
            var taskinfo = new NetworkedPlayerInfo.TaskInfo((byte)npt.Id, npt.Id);
            taskinfo.Complete = false;
            pi.Tasks.Add(taskinfo);
        }
        public static void reset()
        {
            allTasks = new();
            counter = 0;
            completedConsoles = new();
        }
    }
    [HarmonyPatch(typeof(NormalPlayerTask), nameof(NormalPlayerTask.UpdateArrowAndLocation))]
    class UpdateArrowPatch
    {
        public static bool Prefix(NormalPlayerTask __instance)
        {
            if (__instance.Id >= 100)
            {
                if (!__instance.IsComplete && __instance.taskStep != 0)
                {
                    if (__instance.name == "stay")
                    {
                        if (Fox.taskType == Fox.TaskType.Serial)
                        {
                            __instance.Arrow.gameObject.SetActive(true);
                            var console = ShipStatus.Instance.AllConsoles.FirstOrDefault(x => x.ConsoleId == __instance.Data[__instance.taskStep]);
                            __instance.Arrow.target = console.transform.position;
                            __instance.LocationDirty = true;
                            return false;
                        }
                    }
                    else
                    {
                        __instance.Arrow.gameObject.SetActive(true);
                        var console = ShipStatus.Instance.AllConsoles.FirstOrDefault(x => x.ConsoleId == __instance.Data[__instance.taskStep]);
                        __instance.Arrow.target = console.transform.position;
                        __instance.LocationDirty = true;
                        return false;
                    }

                }
                __instance.Arrow.gameObject.SetActive(false);
                return false;
            }
            return true;
        }

    }
    [HarmonyPatch(typeof(NormalPlayerTask), nameof(NormalPlayerTask.AppendTaskText))]
    class AppendTaskTextPatch
    {
        static public bool Prefix(NormalPlayerTask __instance, [HarmonyArgument(0)] Il2CppSystem.Text.StringBuilder sb)
        {
            if (__instance.TaskType != TaskTypes.None) return true;
            bool flag = __instance.ShouldYellowText();
            if (flag)
            {
                if (__instance.IsComplete)
                {
                    sb.Append("<color=#00DD00FF>");
                }
                else
                {
                    sb.Append("<color=#FFFF00FF>");
                }
            }
            if (PlayerControl.LocalPlayer.isRole(RoleType.Fox))
            {
                if (Fox.taskType == Fox.TaskType.Serial)
                {
                    sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString(__instance.StartAt));
                    sb.Append(": ");
                }
            }
            else
            {
                sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString(__instance.StartAt));
                sb.Append(": ");
            }
            sb.Append(ModTranslation.getString(__instance.name));
            if (__instance.ShowTaskStep)
            {
                sb.Append(" (");
                sb.Append(__instance.taskStep);
                sb.Append("/");
                sb.Append(__instance.MaxStep);
                sb.Append(")");
            }
            if (flag)
            {
                sb.Append("</color>");
            }
            return false;
        }
    }
    [HarmonyPatch(typeof(NormalPlayerTask), nameof(NormalPlayerTask.ValidConsole))]
    class ValidConsolePatch
    {
        static public bool Prefix(NormalPlayerTask __instance, [HarmonyArgument(0)] Console console, out bool __result)
        {
            if (__instance.TaskType == TaskTypes.None && console.TaskTypes.Contains(TaskTypes.None))
            {
                if (console.name == "shrine" && Fox.isStealthed(PlayerControl.LocalPlayer))
                {
                    __result = false;
                    return false;
                }

                if (__instance.MaxStep > 0)
                {
                    if (console.name == "shrine" && Fox.taskType == Fox.TaskType.Parallel)
                    {
                        if (!CustomNormalPlayerTask.completedConsoles.Contains(console.ConsoleId))
                        {
                            List<byte> taskList = new();
                            for (int i = 0; i < __instance.MaxStep; i++)
                            {
                                taskList.Add(__instance.Data[i]);
                            }
                            __result = taskList.Contains((byte)console.ConsoleId);
                        }
                        else
                        {
                            __result = false;
                        }
                    }
                    else
                    {
                        if (__instance.Data[__instance.taskStep] == console.ConsoleId)
                        {
                            __result = true;
                        }
                        else
                        {
                            __result = false;
                        }
                    }
                }
                else
                {
                    __result = true;
                }
                return false;
            }
            else
            {
                __result = false;
                return true;
            }
        }
    }
}
