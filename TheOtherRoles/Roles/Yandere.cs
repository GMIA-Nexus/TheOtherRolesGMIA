using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheOtherRoles.MetaContext;
using TheOtherRoles.Modules;
using TheOtherRoles.Objects;
using TheOtherRoles.Utilities;
using UnityEngine;
using static TheOtherRoles.Patches.PlayerControlFixedUpdatePatch;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles;

[TORRPCHolder]
public class Yandere : RoleBase<Yandere>
{
    public static Color32 color = new(199, 21, 133, byte.MaxValue);
    public static PlayerControl target = null;
    public static PlayerControl currentTarget = null;
    public static bool isRunaway = false;
    public static List<PlayerControl> nuisance = [];
    public static List<Arrow> nuisanceArrow = [];
    public static Arrow targetArrow = new(color);
    public static Dictionary<byte, float> progress = [];
    public static float reducedCooldown;
    public static float cooldownPunishment;
    public static int maxNuisance;
    public static bool hasImpVision;
    public static float nuisanceRange;
    public static float nuisanceRegisterDuration;
    private static bool meetingFlag;
    private static float timeLimit;
    private static float timer;
    private static bool isRunawayNextMeeting;

    public Yandere()
    {
        roleId = RoleId = RoleId.Yandere;
    }

    public static bool canWinWithTarget { get
        {
            return hasAlivePlayers && target != null && !target.Data.IsDead;
        } }

    public override void FixedUpdate()
    {
        yandereSetTarget();
        updateProgress();
        arrowUpdate();
        checkRunaway();
    }

    public override void OnMeetingStart()
    {
        meetingFlag = true;
    }

    public override GUIContext ProgressWidget { get => ProgressGUI.Holder(target != null ? ProgressGUI.OneLineText(ModTranslation.getString("roleInfoYandereTargetFull") + ": " + target?.Data.PlayerName ?? "") : null); }

    public override void OnMeetingEnd(PlayerControl exiled = null)
    {
        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(10f, new Action<float>((p) =>
        {
            if (p == 1f)
                meetingFlag = false;
        })));
        if (isRunawayNextMeeting)
        {
            isRunaway = true;
            if (PlayerControl.LocalPlayer == player)
                _ = new StaticAchievementToken("yandere.another1");
        }
    }

    public void yandereSetTarget()
    {
        if (PlayerControl.LocalPlayer != player || PlayerControl.LocalPlayer.Data.IsDead || (nuisance.Count == 0 && !isRunaway)) return;
        var untargetablePlayers = new List<PlayerControl>();
        if (SchrodingersCat.exists && SchrodingersCat.team == SchrodingersCat.Team.Yandere) untargetablePlayers.AddRange(SchrodingersCat.allPlayers);
        currentTarget = setTarget(untargetablePlayers: untargetablePlayers);
        setPlayerOutline(currentTarget, color);
    }

    public void arrowUpdate()
    {
        if (PlayerControl.LocalPlayer != player) return;
        if (PlayerControl.LocalPlayer.Data.IsDead)
        {
            clearArrows();
            return;
        }
        foreach (var arrow in nuisanceArrow) arrow?.arrow.SetActive(false);
        if (targetArrow?.arrow != null)
        {
            targetArrow.arrow.SetActive(false);
            if (target != null && !target.Data.IsDead)
            {
                targetArrow.arrow.SetActive(true);
                targetArrow.Update(target.transform.position);
            }
        }
        if (nuisance.Count > 0)
        {
            int arrowIndex = 0;
            foreach (var p in nuisance)
            {
                if (!p.Data.IsDead)
                {
                    if (arrowIndex >= nuisanceArrow.Count)
                        nuisanceArrow.Add(new Arrow(Color.white));
                    if (arrowIndex < nuisanceArrow.Count && nuisanceArrow[arrowIndex] != null)
                    {
                        nuisanceArrow[arrowIndex].arrow.SetActive(true);
                        nuisanceArrow[arrowIndex].Update(p.transform.position, Color.white);
                    }
                    arrowIndex++;
                }
            }
        }
    }

    public void checkRunaway()
    {
        if (PlayerControl.LocalPlayer != player || PlayerControl.LocalPlayer.Data.IsDead) return;
        if (isRunaway || isRunawayNextMeeting) return;
        if (nuisance.Count == 0 && !meetingFlag)
        {
            timer += Time.deltaTime;
            if (timer >= timeLimit)
            {
                isRunawayNextMeeting = true;
                return;
            }
        }
        else
        {
            timer = 0f;
        }
    }

    public static void handleYandere(PlayerControl player)
    {
        if (player == target)
            isRunaway = true;
    }

    public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason)
    {
        if (player == target)
            isRunaway = true;
    }

    public void updateProgress()
    {
        if (PlayerControl.LocalPlayer != player || isRunaway || PlayerControl.LocalPlayer.Data.IsDead || meetingFlag) return;
        nuisance.RemoveAll(x => x == null || x.Data.IsDead);
        foreach (PlayerControl p in PlayerControl.AllPlayerControls)
        {
            if (p.Data.IsDead || p == player || p == target || p.inVent) continue;
            if (!progress.ContainsKey(p.PlayerId)) progress[p.PlayerId] = 0f;
            if (nuisance.Contains(p)) continue;
            float distance = Vector3.Distance(p.transform.position, target.transform.position);
            bool anythingBetween = PhysicsHelpers.AnythingBetween(p.GetTruePosition(), player.GetTruePosition(), Constants.ShipAndObjectsMask, false);

            if (!anythingBetween && distance <= nuisanceRange) progress[p.PlayerId] += Time.fixedDeltaTime;
            else progress[p.PlayerId] = 0f;

            if (progress[p.PlayerId] >= nuisanceRegisterDuration && nuisance.Count < maxNuisance) nuisance.Add(p);
        }
    }

    public static RemoteProcess<byte> SetTarget = RemotePrimitiveProcess.OfByte("YandereSetTarget", (message, _) => target = Helpers.playerById(message));

    public static void clearArrows()
    {
        if (nuisanceArrow != null)
            foreach (Arrow arrow in nuisanceArrow)
                if (arrow?.arrow != null)
                    UnityEngine.Object.Destroy(arrow.arrow);
        nuisanceArrow = [];

        if (targetArrow?.arrow != null)
            UnityEngine.Object.Destroy(targetArrow.arrow);
        targetArrow = new Arrow(color);
        if (targetArrow.arrow != null) targetArrow.arrow.SetActive(false);
    }

    public override void ResetRole(bool isShifted)
    {
        clearArrows();
    }

    public static void clearAndReload()
    {
        isRunaway = false;
        target = null;
        currentTarget = null;
        nuisance = [];
        progress = [];
        meetingFlag = true;
        timer = 0f;
        clearArrows();
        reducedCooldown = CustomOptionHolder.yandereReducedCooldown.getFloat();
        cooldownPunishment = CustomOptionHolder.yandereCooldownPunishment.getFloat();
        hasImpVision = CustomOptionHolder.yandereHasImpVision.getBool();
        maxNuisance = Mathf.RoundToInt(CustomOptionHolder.yandereMaxNuisance.getFloat());
        nuisanceRange = CustomOptionHolder.yandereNuisanceRange.getFloat();
        nuisanceRegisterDuration = CustomOptionHolder.yandereNuisanceRegisterDuration.getFloat();
        timeLimit = CustomOptionHolder.yandereRunawayTimeLimit.getFloat();
        players = [];
    }
}
