using System;
using System.Collections.Generic;
using System.Linq;
using Hazel;
using TheOtherRoles.MetaContext;
using TheOtherRoles.Modules;
using TheOtherRoles.Utilities;
using UnityEngine;
using static TheOtherRoles.Patches.PlayerControlFixedUpdatePatch;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    public class JekyllAndHyde : RoleBase<JekyllAndHyde>
    {
        public static Color color = Color.grey;
        public PlayerControl currentTarget;

        static public IEnumerable<HelpSprite> GetHelpSprites()
        {
            yield return new(PlagueDoctor.getSyringeIcon(), "jekyllAndHydeSwitchHint");
            yield return new(SerialKiller.getButtonSprite(), "jekyllAndHydeSuicideHint");
        }

        public JekyllAndHyde()
        {
            RoleId = roleId = RoleId.JekyllAndHyde;
            numUsed = 0;
            currentTarget = null;
            status = Status.None;
            counter = 0;
        }

        public enum Status
        {
            None,
            Jekyll,
            Hyde,
        }

        public Status status = Status.None;
        public int counter = 0;
        public static int numberToWin = 3;
        public static float suicideTimer = 40f;
        public static bool reset = true;
        public static float cooldown = 18f;
        public int numUsed;
        public bool oddIsJekyll;
        public static bool triggerWin = false;
        public static int numCommonTasks;
        public static int numLongTasks;
        public static int numShortTasks;
        public static int numTasks;

        public static bool isOdd(int n)
        {
            return n % 2 == 1;
        }

        public override void OnKill(PlayerControl target)
        {
            if (target != player)
            {
                if (!target.isRole(RoleId.SchrodingersCat) && !SchrodingersCat.hasTeam()) counter++;
                if (counter >= numberToWin) triggerWin = true;
                if (PlayerControl.LocalPlayer == player) HudManagerStartPatch.jekyllAndHydeSuicideButton.Timer = suicideTimer;
            }
        }

        public bool isJekyll()
        {
            if (status == Status.None)
            {
                var alive = PlayerControl.AllPlayerControls.GetFastEnumerator().ToArray().Where(x =>
                {
                    return !x.Data.IsDead;
                });
                bool ret = oddIsJekyll ? isOdd(alive.Count()) : !isOdd(alive.Count());
                return ret;
            }
            return status == Status.Jekyll;
        }

        public override void OnFinishShipStatusBegin()
        {
            player.generateAndAssignTasks(numCommonTasks, numShortTasks, numLongTasks);
            oddIsJekyll = rnd.Next(0, 2) == 1;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetOddIsJekyll, SendOption.Reliable, -1);
            writer.Write(oddIsJekyll);
            writer.Write(player.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            HudManagerStartPatch.jekyllAndHydeSuicideButton.Timer = suicideTimer;
        }

        public int getNumDrugs()
        {
            int counter = player.Data.Tasks.ToArray().Where(t => t.Complete).Count();
            return (int)Math.Floor((float)counter / numTasks);
        }

        public override void FixedUpdate()
        {
            if (PlayerControl.LocalPlayer != player || isJekyll()) return;
            var untargetables = new List<PlayerControl>();
            if (SchrodingersCat.team == SchrodingersCat.Team.JekyllAndHyde) untargetables.AddRange(SchrodingersCat.allPlayers);
            currentTarget = setTarget(untargetablePlayers: untargetables);
            setPlayerOutline(currentTarget, color);
        }

        public static int countLovers()
        {
            int counter = 0;
            foreach (var player in allPlayers)
                if (player.isLovers()) counter += 1;
            return counter;
        }

        public static void clearAndReload()
        {
            triggerWin = false;
            numTasks = (int)CustomOptionHolder.jekyllAndHydeNumTasks.getFloat();
            numCommonTasks = (int)CustomOptionHolder.jekyllAndHydeCommonTasks.getFloat();
            numShortTasks = (int)CustomOptionHolder.jekyllAndHydeShortTasks.getFloat();
            numLongTasks = (int)CustomOptionHolder.jekyllAndHydeLongTasks.getFloat();
            reset = CustomOptionHolder.jekyllAndHydeResetAfterMeeting.getBool();
            numberToWin = (int)CustomOptionHolder.jekyllAndHydeNumberToWin.getFloat();
            cooldown = CustomOptionHolder.jekyllAndHydeCooldown.getFloat();
            suicideTimer = CustomOptionHolder.jekyllAndHydeSuicideTimer.getFloat();
            players = [];
        }
    }
}
