using AmongUs.GameOptions;
using TheOtherRoles.Modules;
using TheOtherRoles.Utilities;
using UnityEngine;
using static TheOtherRoles.Patches.PlayerControlFixedUpdatePatch;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    [TORRPCHolder]
    public class EvilHacker : RoleBase<EvilHacker>
    {
        public static Color color = Palette.ImpostorRed;
        public static bool canHasBetterAdmin = false;
        public bool canCreateMadmate = false;
        public static bool canSeeDoorStatus = true;
        public static bool canCreateMadmateFromJackal;
        public static bool canInheritAbility;
        public PlayerControl currentTarget;

        public EvilHacker()
        {
            RoleId = roleId = RoleId.EvilHacker;
            currentTarget = null;
            acTokenChallenge = null;
            canCreateMadmate = CustomOptionHolder.evilHackerCanCreateMadmate.getBool();
        }

        public static RemoteProcess<(byte targetId, byte evilHackerId)> CreatesMadmate = new("EvilHackerCreateMadmate", (message, _) =>
        {
            PlayerControl player = Helpers.playerById(message.targetId);
            PlayerControl evilHacker = Helpers.playerById(message.evilHackerId);
            var evilHackerRole = getRole(evilHacker);
            if (evilHacker == null || evilHackerRole == null) return;
            if (canCreateMadmateFromJackal || !player.isRole(RoleId.Jackal))
            {
                FastDestroyableSingleton<RoleManager>.Instance.SetRole(player, RoleTypes.Crewmate);

                // タスクがないプレイヤーがMadmateになった場合はショートタスクを必要数割り当てる
                RPCProcedure.erasePlayerRoles(player.PlayerId, true, true, false);
                if (CreatedMadmate.hasTasks && player == PlayerControl.LocalPlayer)
                    player.generateAndAssignTasks(0, CreatedMadmate.numTasks, 0);

                player.addModifier(RoleId.CreatedMadmate);
                if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId) PlayerControl.LocalPlayer.moveable = true;
            }
            evilHackerRole.canCreateMadmate = false;
            return;
        });

        public AchievementToken<(bool admin, bool cleared)> acTokenChallenge = null;

        public override void PostInit()
        {
            if (PlayerControl.LocalPlayer != player) return;
            acTokenChallenge ??= new("evilHacker.challenge", (false, false), (val, _) => val.cleared);
        }

        private static Sprite madmateButtonSprite;

        public static Sprite getMadmateButtonSprite()
        {
            if (madmateButtonSprite) return madmateButtonSprite;
            madmateButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.SidekickButton.png", 115f);
            return madmateButtonSprite;
        }

        public static bool isInherited()
        {
            return canInheritAbility && exists && livingPlayers.Count == 0 && PlayerControl.LocalPlayer.Data.Role.IsImpostor;
        }

        public override void OnKill(PlayerControl target)
        {
            if (PlayerControl.LocalPlayer == player)
            {
                acTokenChallenge.Value.cleared |= acTokenChallenge.Value.admin;
                acTokenChallenge.Value.admin = false;
            }
        }

        public override void FixedUpdate()
        {
            if (player != PlayerControl.LocalPlayer) return;
            currentTarget = setTarget(true);
            setPlayerOutline(currentTarget, color);
        }

        public static void clearAndReload()
        {
            canHasBetterAdmin = CustomOptionHolder.evilHackerCanHasBetterAdmin.getBool();
            canCreateMadmateFromJackal = CustomOptionHolder.evilHackerCanCreateMadmateFromJackal.getBool();
            canInheritAbility = CustomOptionHolder.evilHackerCanInheritAbility.getBool();
            canSeeDoorStatus = CustomOptionHolder.evilHackerCanSeeDoorStatus.getBool();
            players = [];
        }
    }
}
