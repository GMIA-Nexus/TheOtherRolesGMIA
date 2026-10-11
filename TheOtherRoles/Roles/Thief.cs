using System;
using System.Collections.Generic;
using UnityEngine;
using static TheOtherRoles.Patches.PlayerControlFixedUpdatePatch;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    public class Thief : RoleBase<Thief> {
        public static Color color = new Color32(71, 99, 45, byte.MaxValue);
        public PlayerControl currentTarget = null;
        public static List<PlayerControl> formerThief = [];

        public Thief()
        {
            RoleId = roleId = RoleId.Thief;
            suicideFlag = false;
            currentTarget = null;
        }

        public override void FixedUpdate()
        {
            if (player != PlayerControl.LocalPlayer) return;
            List<PlayerControl> untargetables = [];
            if (Mini.exists && !Mini.isGrownUp()) untargetables.AddRange(Mini.allPlayers);
            currentTarget = setTarget(onlyCrewmates: false, untargetablePlayers: untargetables);
            setPlayerOutline(currentTarget, color);
        }

        public static float cooldown = 30f;

        public bool suicideFlag = false;  // Used as a flag for suicide

        public static bool hasImpostorVision;
        public static bool canUseVents;
        public static bool canKillSheriff;
        public static bool canStealWithGuess;

        public static void clearAndReload() {
            formerThief = [];
            hasImpostorVision = CustomOptionHolder.thiefHasImpVision.getBool();
            cooldown = CustomOptionHolder.thiefCooldown.getFloat();
            canUseVents = CustomOptionHolder.thiefCanUseVents.getBool();
            canKillSheriff = CustomOptionHolder.thiefCanKillSheriff.getBool();
            canStealWithGuess = CustomOptionHolder.thiefCanStealWithGuess.getBool();
            players = [];
        }

        public static bool isFailedThiefKill(PlayerControl target, PlayerControl killer, RoleInfo targetRole) {
            return isRole(killer) && !target.Data.Role.IsImpostor && !new List<RoleInfo> { RoleInfo.jackal, canKillSheriff ? RoleInfo.sheriff : null, RoleInfo.sidekick, RoleInfo.moriarty, RoleInfo.jekyllAndHyde, SchrodingersCat.hasTeam() && SchrodingersCat.team != SchrodingersCat.Team.Crewmate ? RoleInfo.schrodingersCat : 
                null, RoleInfo.pelican, RoleInfo.yandere }.Contains(targetRole);
        }
    }
}
