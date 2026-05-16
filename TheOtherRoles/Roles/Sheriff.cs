using System.Linq;
using TheOtherRoles.MetaContext;
using TheOtherRoles.Modules;
using UnityEngine;
using static TheOtherRoles.Patches.PlayerControlFixedUpdatePatch;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles;

public class Sheriff : RoleBase<Sheriff>
{
    public static Color color = new Color32(248, 205, 70, byte.MaxValue);

    public static float cooldown = 30f;
    public static bool canKillNeutrals = false;
    public static bool spyCanDieToSheriff = false;
    public bool isFormerDeputy = false;
    public float remainingHandcuffs = 0;

    public Sheriff()
    {
        RoleId = roleId = RoleId.Sheriff;
        currentTarget = null;
        isFormerDeputy = false;
        remainingHandcuffs = 0;
        acTokenChallenge = null;
    }

    public PlayerControl currentTarget;

    public AchievementToken<(bool isTriggeredFalse, bool cleared)> acTokenChallenge = null;

    public override void FixedUpdate()
    {
        if (player != PlayerControl.LocalPlayer) return;
        currentTarget = setTarget();
        setPlayerOutline(currentTarget, color);
    }

    public static void replaceCurrentSheriff(PlayerControl deputy)
    {
        setRole(deputy);
        getRole(deputy).currentTarget = null;
        cooldown = CustomOptionHolder.sheriffCooldown.getFloat();
    }

    public override void OnKill(PlayerControl target)
    {
        if (PlayerControl.LocalPlayer == player)
        {
            acTokenChallenge.Value.isTriggeredFalse = false;

            if (acTokenChallenge.Value.cleared)
            {
                foreach (var dp in GameHistory.deadPlayers)
                {
                    if (dp.player == null || !Helpers.isEvil(dp.player)) continue;
                    if (!isRole(dp.killerIfExisting))
                    {
                        acTokenChallenge.Value.cleared = false;
                        break;
                    }
                }

                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (p == null || !Helpers.isEvil(p)) continue;
                    if (!p.Data.IsDead)
                    {
                        acTokenChallenge.Value.isTriggeredFalse = true;
                        break;
                    }
                }
            }
        }
    }

    public override GUIContext ProgressWidget
    {
        get
        {
            var deputy = getDeputy(player);
            if (deputy == null || deputy?.player == null) return null;
            return ProgressGUI.Holder(ProgressGUI.OneLineText(ModTranslation.getString("deputy") + ": " + deputy.player.Data.PlayerName));
        }
    }

    public static Deputy getDeputy(PlayerControl sheriff)
    {
        return Deputy.players.FirstOrDefault(x => x.sheriff != null && x.sheriff?.player == sheriff);
    }

    public override void PostInit()
    {
        if (PlayerControl.LocalPlayer != player) return;
        acTokenChallenge ??= new("sheriff.challenge", (true, true), (val, _) => val.cleared && !val.isTriggeredFalse);
    }

    public static void clearAndReload()
    {
        cooldown = CustomOptionHolder.sheriffCooldown.getFloat();
        canKillNeutrals = CustomOptionHolder.sheriffCanKillNeutrals.getBool();
        spyCanDieToSheriff = CustomOptionHolder.spyCanDieToSheriff.getBool();
        players = [];
    }
}
