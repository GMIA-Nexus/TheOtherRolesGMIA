using System.Collections.Generic;
using Hazel;
using TheOtherRoles.CustomGameModes;
using TheOtherRoles.MetaContext;
using TheOtherRoles.Modules;
using UnityEngine;
using static TheOtherRoles.Patches.PlayerControlFixedUpdatePatch;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    [TORRPCHolder]
    public class Sidekick : RoleBase<Sidekick> {
        public static Color color = new Color32(0, 180, 235, byte.MaxValue);

        public PlayerControl currentTarget;

        public bool wasTeamRed;
        public bool wasImpostor;
        public bool wasSpy;
        public Jackal jackal;

        public static float cooldown = 30f;
        public static bool canUseVents = true;
        public static bool canKill = true;
        public static bool promotesToJackal = true;
        public static bool hasImpostorVision = false;
        public static bool canSabotageLights;

        public static int countLovers()
        {
            int counter = 0;
            foreach (var player in allPlayers)
                if (player.isLovers()) counter += 1;
            return counter;
        }

        public override GUIContext ProgressContext { get => ProgressGUI.Holder(jackal != null && jackal.player ? ProgressGUI.OneLineText(ModTranslation.getString("jackal") + ": " + jackal.player.Data.PlayerName) : null); }

        public static RemoteProcess<byte> PromoteToJackal = RemotePrimitiveProcess.OfByte("SidekickPromotes", (message, _) =>
        {
            PlayerControl player = Helpers.playerById(message);
            var sidekick = getRole(player);
            if (FreePlayGM.isFreePlayGM || sidekick == null) return;
            if (PlayerControl.LocalPlayer.PlayerId == message) new StaticAchievementToken("sidekick.challenge");
            bool wasTeamRed = sidekick.wasTeamRed;
            bool wasSpy = sidekick.wasSpy;
            bool wasImpostor = sidekick.wasImpostor;
            eraseRole(player);
            player.setRole(RoleId.Jackal);
            var jackal = Jackal.getRole(player);
            jackal.removeCurrentJackal();
            jackal.canCreateSidekick = Jackal.jackalPromotedFromSidekickCanCreateSidekick;
            jackal.wasTeamRed = wasTeamRed;
            jackal.wasSpy = wasSpy;
            jackal.wasImpostor = wasImpostor;
            return;
        });

        public Sidekick()
        {
            RoleId = roleId = RoleId.Sidekick;
            jackal = null;
            currentTarget = null;
            wasTeamRed = wasImpostor = wasSpy = false;
        }

        public override void FixedUpdate()
        {
            sidekickSetTarget();
            sidekickCheckPromotion();
        }

        private void sidekickSetTarget()
        {
            if (player != PlayerControl.LocalPlayer) return;
            var untargetablePlayers = new List<PlayerControl>();
            if (jackal != null) untargetablePlayers.Add(jackal.player);
            if (Mini.mini != null && !Mini.isGrownUp()) untargetablePlayers.Add(Mini.mini); // Exclude Sidekick from targeting the Mini unless it has grown up
            if (SchrodingersCat.exists && SchrodingersCat.team == SchrodingersCat.Team.Jackal) untargetablePlayers.AddRange(SchrodingersCat.allPlayers);
            currentTarget = setTarget(untargetablePlayers: untargetablePlayers);
            if (canKill) setPlayerOutline(currentTarget, Palette.ImpostorRed);
        }

        private void sidekickCheckPromotion()
        {
            if (PlayerControl.LocalPlayer != player || player.Data.IsDead == true || !promotesToJackal) return;
            if (jackal == null || jackal.player == null || jackal.player.Data.IsDead || jackal?.player.Data?.Disconnected == true)
            {
                PromoteToJackal.Invoke(PlayerControl.LocalPlayer.PlayerId);
            }
        }

        public static void clearAndReload() {
            cooldown = CustomOptionHolder.jackalKillCooldown.getFloat();
            canUseVents = CustomOptionHolder.sidekickCanUseVents.getBool();
            canKill = CustomOptionHolder.sidekickCanKill.getBool();
            promotesToJackal = CustomOptionHolder.sidekickPromotesToJackal.getBool();
            hasImpostorVision = CustomOptionHolder.jackalAndSidekickHaveImpostorVision.getBool();
            canSabotageLights = CustomOptionHolder.sidekickCanSabotageLights.getBool();
            players = [];
        }
    }
}
