using System.Linq;
using TheOtherRoles.Modules;
using TheOtherRoles.Utilities;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    [TORRPCHolder]
    public class Engineer : RoleBase<Engineer>
    {
        public static Color color = new Color32(0, 40, 245, byte.MaxValue);
        private static Sprite buttonSprite;

        public Engineer()
        {
            RoleId = roleId = RoleId.Engineer;
            remainingFixes = Mathf.RoundToInt(CustomOptionHolder.engineerNumberOfFixes.getFloat());
            acTokenChallenge = null;
        }

        public override AudioClip IntroSound { get => MetaContext.VanillaAsset.EngineerIntroSound; }

        public int remainingFixes = 1;
        public static bool highlightForImpostors = true;
        public static bool highlightForTeamJackal = true;

        public AchievementToken<(bool inVent, bool cleared)> acTokenChallenge = null;

        public static Sprite getButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.RepairButton.png", 115f);
            return buttonSprite;
        }

        public static RemoteProcess FixLights = new("EngineerFixLights",
            (_) =>
            {
                SwitchSystem switchSystem = MapUtilities.Systems[SystemTypes.Electrical].CastFast<SwitchSystem>();
                switchSystem.ActualSwitches = switchSystem.ExpectedSwitches;
            }
        );

        public static RemoteProcess FixSubmergedOxygen = new("EngineerFixSubmergedOxygen", (_) => SubmergedCompatibility.RepairOxygen());

        public static RemoteProcess UseRepair = new("EngineerUseRepair", (_) =>
        {
            if (Helpers.shouldShowGhostInfo())
                Helpers.showFlash(color, 0.5f, ModTranslation.getString("engineerInfo"));
        });

        public override void FixedUpdate()
        {
            bool jackalHighlight = highlightForTeamJackal && (PlayerControl.LocalPlayer.isRole(RoleId.Jackal) || PlayerControl.LocalPlayer.isRole(RoleId.Sidekick));
            bool impostorHighlight = highlightForImpostors && PlayerControl.LocalPlayer.Data.Role.IsImpostor;
            if ((jackalHighlight || impostorHighlight) && MapUtilities.CachedShipStatus?.AllVents != null)
            {
                foreach (Vent vent in MapUtilities.CachedShipStatus.AllVents)
                {
                    try
                    {
                        if (vent?.myRend?.material != null)
                            if (exists && allPlayers.Any(x => x.inVent))
                            {
                                vent.myRend.material.SetFloat("_Outline", 1f);
                                vent.myRend.material.SetColor("_OutlineColor", color);
                            }
                            else if (vent.myRend.material.GetColor("_AddColor") != Color.red)
                                vent.myRend.material.SetFloat("_Outline", 0);
                    }
                    catch { }
                }
            }
            if (PlayerControl.LocalPlayer == player && MapUtilities.CachedShipStatus?.AllVents != null && acTokenChallenge != null)
            {
                if (!PlayerControl.LocalPlayer.Data.IsDead)
                {
                    if (PlayerControl.LocalPlayer.inVent && !acTokenChallenge.Value.inVent)
                        acTokenChallenge.Value.inVent = true;
                    else if (!PlayerControl.LocalPlayer.inVent && acTokenChallenge.Value.inVent)
                        acTokenChallenge.Value.inVent = false;
                }
                else if (acTokenChallenge.Value.inVent && !acTokenChallenge.Value.cleared)
                    acTokenChallenge.Value.cleared = true;
            }
        }

        public override void PostInit()
        {
            if (PlayerControl.LocalPlayer != player) return;
            acTokenChallenge ??= new("engineer.another1", (false, false), (val, _) => val.cleared);
        }

        public static void clearAndReload()
        {
            highlightForImpostors = CustomOptionHolder.engineerHighlightForImpostors.getBool();
            highlightForTeamJackal = CustomOptionHolder.engineerHighlightForTeamJackal.getBool();
            players = [];
        }
    }
}
