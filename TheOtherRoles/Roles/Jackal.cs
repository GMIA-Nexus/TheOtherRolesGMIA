using System.Collections.Generic;
using System.Linq;
using TheOtherRoles.MetaContext;
using TheOtherRoles.Modules;
using UnityEngine;
using static TheOtherRoles.Patches.PlayerControlFixedUpdatePatch;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    [TORRPCHolder]
    public class Jackal : RoleBase<Jackal> {
        public static Color color = new Color32(0, 180, 235, byte.MaxValue);
        public PlayerControl fakeSidekick;
        public PlayerControl currentTarget;
        
        public static float cooldown = 30f;
        public static float createSidekickCooldown = 30f;
        public static bool canUseVents = true;
        public bool canCreateSidekick = true;
        public static Sprite buttonSprite;
        public static bool jackalPromotedFromSidekickCanCreateSidekick = true;
        public static bool canCreateSidekickFromImpostor = true;
        public static bool hasImpostorVision = false;
        public bool wasTeamRed;
        public bool wasImpostor;
        public bool wasSpy;
        public static bool canSabotageLights;

        static public IEnumerable<HelpSprite> GetHelpSprites()
        {
            if (CustomOptionHolder.jackalCanCreateSidekick.getBool())
                yield return new(getSidekickButtonSprite(), "jackalSidekickHint");
        }

        public override GUIContext ProgressWidget { get {
                var sidekick = getSidekick(player);
                if (sidekick == null || sidekick?.player == null) return null;
                return ProgressGUI.Holder(ProgressGUI.OneLineText(ModTranslation.getString("sidekick") + ": " + sidekick.player.Data.PlayerName));
            } }

        public override void FixedUpdate()
        {
            if (player != PlayerControl.LocalPlayer) return;
            var untargetablePlayers = new List<PlayerControl>();
            if (canCreateSidekickFromImpostor)                 // Only exclude sidekick from beeing targeted if the jackal can create sidekicks from impostors
                if (getSidekick(player) != null) untargetablePlayers.Add(getSidekick(player).player);
            if (SchrodingersCat.exists && SchrodingersCat.team == SchrodingersCat.Team.Jackal) untargetablePlayers.AddRange(SchrodingersCat.allPlayers);
            if (Mini.mini != null && !Mini.isGrownUp()) untargetablePlayers.Add(Mini.mini); // Exclude Jackal from targeting the Mini unless it has grown up
            currentTarget = setTarget(untargetablePlayers: untargetablePlayers);
            setPlayerOutline(currentTarget, Palette.ImpostorRed);
        }

        public static RemoteProcess<(byte targetId, byte jackalId)> CreateSidekick = new("JackalCreateSidekick", (message, _) => RPCProcedure.jackalCreatesSidekick(message.targetId, message.jackalId));

        public Jackal()
        {
            RoleId = roleId = RoleId.Jackal;
            wasTeamRed = wasImpostor = wasSpy = false;
            currentTarget = null;
            fakeSidekick = null;
            canCreateSidekick = CustomOptionHolder.jackalCanCreateSidekick.getBool();
        }

        public static int countLovers()
        {
            int counter = 0;
            foreach (var player in allPlayers)
                if (player.isLovers()) counter += 1;
            return counter;
        }

        public override void ResetRole(bool isShifted)
        {
            if (!isShifted)
            {
                var sidekick = getSidekick(player);
                if (sidekick != null)
                    sidekick.jackal = null;
            }
        }

        public static Sidekick getSidekick(PlayerControl jackal)
        {
            return Sidekick.players.FirstOrDefault(x => x.jackal != null && x.jackal.player == jackal);
        }

        public static Sprite getSidekickButtonSprite() {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.SidekickButton.png", 115f);
            return buttonSprite;
        }

        public void removeCurrentJackal() {
            currentTarget = null;
            fakeSidekick = null;
            cooldown = CustomOptionHolder.jackalKillCooldown.getFloat();
            createSidekickCooldown = CustomOptionHolder.jackalCreateSidekickCooldown.getFloat();
        }

        public static void clearAndReload() {
            cooldown = CustomOptionHolder.jackalKillCooldown.getFloat();
            createSidekickCooldown = CustomOptionHolder.jackalCreateSidekickCooldown.getFloat();
            canUseVents = CustomOptionHolder.jackalCanUseVents.getBool();
            jackalPromotedFromSidekickCanCreateSidekick = CustomOptionHolder.jackalPromotedFromSidekickCanCreateSidekick.getBool();
            canCreateSidekickFromImpostor = CustomOptionHolder.jackalCanCreateSidekickFromImpostor.getBool();
            hasImpostorVision = CustomOptionHolder.jackalAndSidekickHaveImpostorVision.getBool();
            canSabotageLights = CustomOptionHolder.jackalCanSabotageLights.getBool();
            players = [];
        }
        
    }
}
