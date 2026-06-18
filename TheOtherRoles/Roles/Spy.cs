using TheOtherRoles.MetaContext;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    public class Spy : RoleBase<Spy> {
        public static Color color = Palette.ImpostorRed;

        public Spy()
        {
            RoleId = roleId = RoleId.Spy;
        }

        public static bool impostorsCanKillAnyone = true;
        public static bool canEnterVents = false;
        public static bool hasImpostorVision = false;

        public static void clearAndReload() {
            impostorsCanKillAnyone = CustomOptionHolder.spyImpostorsCanKillAnyone.getBool();
            canEnterVents = CustomOptionHolder.spyCanEnterVents.getBool();
            hasImpostorVision = CustomOptionHolder.spyHasImpostorVision.getBool();
            players = [];
        }
    }
}
