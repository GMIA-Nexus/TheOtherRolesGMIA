using System.Collections.Generic;
using TheOtherRoles.Modules;
using UnityEngine;
using static TheOtherRoles.Patches.PlayerControlFixedUpdatePatch;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    public class Warlock: RoleBase<Warlock> {
        public static Color color = Palette.ImpostorRed;

        public PlayerControl currentTarget;
        public PlayerControl curseVictim;
        public PlayerControl curseVictimTarget;

        static public IEnumerable<HelpSprite> GetHelpSprites()
        {
            yield return new HelpSprite(getCurseButtonSprite(), "warlockCurseHint");
            yield return new HelpSprite(getCurseKillButtonSprite(), "warlockCurseKillHint");
        }

        static public IEnumerable<DocumentReplacement> GetReplacementPart()
        {
            yield return new("%SEC%", rootTime.ToString());
            yield return new("%OPT%", canKillImpostors ? ModTranslation.getString("warlockCanKillImpostorsOption") : ModTranslation.getString("warlockCannotKillImpostorsOption"));
        }

        public static float cooldown = 30f;
        public static float rootTime { get { return CustomOptionHolder.warlockRootTime.getFloat(); } }
        static public bool canKillImpostors { get { return CustomOptionHolder.warlockCanKillImpostors.getBool(); } }

        private static Sprite curseButtonSprite;
        private static Sprite curseKillButtonSprite;

        public Warlock()
        {
            RoleId = roleId = RoleId.Warlock;
            currentTarget = null;
            curseVictim = null;
            curseVictimTarget = null;
        }

        public override void OnMeetingEnd(PlayerControl exiled = null)
        {
            curseVictim = null;
            curseVictimTarget = null;
        }

        public static Sprite getCurseButtonSprite() {
            if (curseButtonSprite) return curseButtonSprite;
            curseButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.CurseButton.png", 115f);
            return curseButtonSprite;
        }

        public static Sprite getCurseKillButtonSprite() {
            if (curseKillButtonSprite) return curseKillButtonSprite;
            curseKillButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.CurseKillButton.png", 115f);
            return curseKillButtonSprite;
        }

        public static void clearAndReload() {
            cooldown = CustomOptionHolder.warlockCooldown.getFloat();
            players = [];
        }

        public override void OnKill(PlayerControl target)
        {
            if (PlayerControl.LocalPlayer == player && HudManagerStartPatch.warlockCurseButton != null)
                if (player.killTimer > HudManagerStartPatch.warlockCurseButton.Timer)
                    HudManagerStartPatch.warlockCurseButton.Timer = player.killTimer;
        }

        public override void FixedUpdate()
        {
            if (player != PlayerControl.LocalPlayer) return;
            if (curseVictim != null && (curseVictim.Data.Disconnected || curseVictim.Data.IsDead))                 // If the cursed victim is disconnected or dead reset the curse so a new curse can be applied
                resetCurse();
            if (curseVictim == null) {
                currentTarget = setTarget();
                setPlayerOutline(currentTarget, color);
            }
            else {
                curseVictimTarget = setTarget(!canKillImpostors, targetingPlayer: curseVictim);
                setPlayerOutline(curseVictimTarget, color);
            }
        }

        public void resetCurse() {
            HudManagerStartPatch.warlockCurseButton.Timer = HudManagerStartPatch.warlockCurseButton.MaxTimer;
            HudManagerStartPatch.warlockCurseButton.Sprite = getCurseButtonSprite();
            HudManagerStartPatch.warlockCurseButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
            currentTarget = null;
            curseVictim = null;
            curseVictimTarget = null;
        }
    }
}
