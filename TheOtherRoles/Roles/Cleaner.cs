using System.Collections.Generic;
using TheOtherRoles.MetaContext;
using TheOtherRoles.Modules;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    public class Cleaner : RoleBase<Cleaner> {
        public static Color color = Palette.ImpostorRed;

        public Cleaner()
        {
            RoleId = roleId = RoleId.Cleaner;
        }

        public static float cooldown = 30f;
        public static bool canSeeBodies { get { return CustomOptionHolder.cleanerCanSeeBodies.getBool(); } }

        public override void OnKill(PlayerControl target)
        {
            if (PlayerControl.LocalPlayer == player && HudManagerStartPatch.cleanerCleanButton != null)
                HudManagerStartPatch.cleanerCleanButton.Timer = HudManagerStartPatch.cleanerCleanButton.MaxTimer;
        }

        static public IEnumerable<DocumentReplacement> GetReplacementPart()
        {
            yield return new("%OPT%", canSeeBodies ? ModTranslation.getString("cleanerOPTHint") : "");
        }

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite() {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.CleanButton.png", 115f);
            return buttonSprite;
        }

        public static void clearAndReload() {
            cooldown = CustomOptionHolder.cleanerCooldown.getFloat();
            players = [];
        }
    }
}
