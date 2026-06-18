using System;
using TheOtherRoles.MetaContext;
using TheOtherRoles.Modules;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    public class TaskMaster : RoleBase<TaskMaster>
    {
        public static bool becomeATaskMasterWhenCompleteAllTasks = false;
        public static Color color = new Color32(225, 86, 75, byte.MaxValue);
        public static bool isTaskComplete = false;
        public static byte clearExTasks = 0;
        public static byte allExTasks = 0;
        public static byte oldTaskMasterPlayerId = byte.MaxValue;
        public static bool triggerTaskMasterWin = false;
        public static bool canVent = true;

        public TaskMaster()
        {
            RoleId = roleId = RoleId.TaskMaster;
        }

        public override void OnDeath(PlayerControl killer = null)
        {
            if (PlayerControl.LocalPlayer == player)
            {
                var (taskComplete, total) = TasksHandler.taskInfo(PlayerControl.LocalPlayer.Data);
                if (taskComplete == 0 && total > 0)
                    _ = new StaticAchievementToken("taskMaster.another1");
            }
        }

        public static void clearAndReload()
        {
            becomeATaskMasterWhenCompleteAllTasks = CustomOptionHolder.taskMasterBecomeATaskMasterWhenCompleteAllTasks.getBool();
            canVent = CustomOptionHolder.taskMasterCanVent.getBool();
            clearExTasks = 0;
            allExTasks = 0;
            oldTaskMasterPlayerId = byte.MaxValue;
            triggerTaskMasterWin = false;
            isTaskComplete = false;
            players = [];
        }
    }
}
