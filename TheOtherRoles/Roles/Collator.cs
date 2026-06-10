using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rewired.Utils.Classes.Data;
using TheOtherRoles.MetaContext;
using TheOtherRoles.Modules;
using TheOtherRoles.Patches;
using UnityEngine;
using static TheOtherRoles.Patches.PlayerControlFixedUpdatePatch;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles;

[TORRPCHolder]
public class Collator : RoleBase<Collator>
{
    public static Color32 color = new(37, 159, 148, 255);
    public int trialsLeft;
    public static float cooldown;
    public static bool madmateAsCrew;
    public static bool strictNeutral;

    public List<(PlayerControl player, TeamCategory role)> sampledPlayers = [];
    public (SpriteRenderer tube, SpriteRenderer sample)[] allSamples = [];
    public List<(PlayerControl player1, PlayerControl player2, bool matched)> sharedResults = [];

    public PlayerControl currentTarget;

    static public readonly IDividedSpriteLoader tubeSprite = DividedSpriteLoader.FromResource("TheOtherRoles.Resources.CollatorTube.png", 125f, 2, 1);

    private GameObject aJustObj = null;

    private AchievementToken<int> acTokenChallenge = null;

    static public IEnumerable<DocumentReplacement> GetReplacementPart()
    {
        var num = CustomOptionHolder.collatorNumberOfTrials.getFloat();
        yield return new("%NUM%", num == 1 ? ModTranslation.getString("collatorNUMSingle") : string.Format(ModTranslation.getString("collatorNUMPlural"), num));
    }

    public enum TeamCategory
    {
        Crewmate,
        Impostor,
        Jackal,
        JekyllAndHyde,
        Moriarty,
        Yandere,
        Pelican,
        NeutralStandalone
    }

    private int ActualSampledPlayers => sampledPlayers.DistinctBy(p => p.player.PlayerId).Count();

    private static Sprite buttonSprite;
    public static Sprite getButtonSprite()
    {
        if (buttonSprite) return buttonSprite;
        buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.CollatorSampleButton.png", 115f);
        return buttonSprite;
    }

    public override void ResetRole(bool isShifted)
    {
        if (aJustObj) Object.Destroy(aJustObj);
        aJustObj = null;
        if (allSamples != null)
        {
            foreach (var p in allSamples)
            {
                Object.Destroy(p.tube);
                Object.Destroy(p.sample);
            }
        }
        sampledPlayers = [];
        allSamples = [];
        sharedResults = [];
    }

    public static RemoteProcess<(byte collatorId, byte player1Id, byte player2Id, bool matched)> ShareResults = new("CollatorShareResults", (message, _) =>
    {
        var collatorPlayer = Helpers.playerById(message.collatorId);
        var player1 = Helpers.playerById(message.player1Id);
        var player2 = Helpers.playerById(message.player2Id);
        var collator = getRole(collatorPlayer);
        if (collator != null && collatorPlayer && player1 && player2)
            collator.sharedResults.Add((player1, player2, message.matched));
    });

    public override GUIContext ProgressContext { get {
            var res = sharedResults.Where(x => x.player1 != null && x.player2 != null).ToList();
            return ProgressGUI.Holder(
            ProgressGUI.OneLineText(ModTranslation.getString("roleInfoCollatorResults")),
                (res.Count == 0 ?
                    ProgressGUI.OneLineText(ModTranslation.getString("roleInfoCollatorResultsZero")) :
                    ProgressGUI.Holder(
                    res.Select(result => ProgressGUI.OneLineText((result.player1?.Data.PlayerName ?? "") + ", " + (result.player2?.Data.PlayerName ?? "") + " ⇒ " + (result.matched ? ModTranslation.getString("collatorMatched").Color(Color.green) : ModTranslation.getString("collatorUnmatched").Color(Color.red))))
                )).Move(new(0.1f, 0f))
            );
        } }

    public override void FixedUpdate()
    {
        if (PlayerControl.LocalPlayer != player) return;
        setSampleTarget();
        if (PlayerControl.LocalPlayer.Data.IsDead)
        {
            aJustObj.gameObject.SetActive(false);
            return;
        }
        aJustObj.gameObject.transform.localPosition = IntroCutsceneOnDestroyPatch.bottomLeft + new Vector3(0f, 0f, -62f);
        aJustObj.gameObject.SetActive(true);
        if (sampledPlayers.Any(x => x.player == null))
        {
            sampledPlayers.RemoveAll(x => x.player == null);
            UpdateSamples();
        }
    }

    void setSampleTarget()
    {
        if (PlayerControl.LocalPlayer.Data.IsDead || sampledPlayers.Count >= allSamples.Length) return;
        currentTarget = setTarget(untargetablePlayers: [.. sampledPlayers.Select(x => x.player)]);
        setPlayerOutline(currentTarget, color);
    }

    public override void PostInit()
    {
        if (PlayerControl.LocalPlayer != player) return;
        acTokenChallenge ??= new("collator.challenge", 0, (val, _) => val >= 3);

        aJustObj = Helpers.CreateObject("CollatorObject", HudManager.Instance.transform, Vector3.zero);
        var aJust = Helpers.CreateObject<ScriptBehaviour>("CollatorIconsAJust", aJustObj.transform, Vector3.zero);
        aJust.UpdateHandler += () =>
        {
            if (MeetingHud.Instance)
            {
                aJust.transform.localScale = new(0.65f, 0.65f, 1f);
                aJust.transform.localPosition = new Vector3(-0.45f, -0.17f, 0f);
            }
            else
            {
                aJust.transform.localScale = Vector3.one;
                aJust.transform.localPosition = Vector3.zero;
            }
        };

        allSamples = new (SpriteRenderer tube, SpriteRenderer sample)[2];
        for (int i = 0; i < allSamples.Length; i++)
        {
            var tube = Helpers.CreateObject<SpriteRenderer>("SampleTube", aJust.transform, Vector3.zero, LayerMask.NameToLayer("UI"));
            tube.sprite = tubeSprite.GetSprite(0);

            var sample = Helpers.CreateObject<SpriteRenderer>("SampleColored", tube.transform, new(0, 0, 0.1f));
            sample.sprite = tubeSprite.GetSprite(1);

            tube.transform.localPosition = new((float)i * 0.4f - 0.3f, 0f, 0f);

            allSamples[i] = (tube, sample);
        }

        UpdateSamples();
    }

    void RegisterResults((PlayerControl player, TeamCategory role) player1, (PlayerControl player, TeamCategory role) player2)
    {
        if (player1.player == null || player2.player == null) return;

        _ = new StaticAchievementToken("collator.common1");

        if (player1.role == TeamCategory.Impostor && player2.role == TeamCategory.Impostor)
            _ = new StaticAchievementToken("collator.common2");

        bool matched = player1.role == player2.role && player1.role != TeamCategory.NeutralStandalone && player2.role != TeamCategory.NeutralStandalone;

        if (!matched) _ = new StaticAchievementToken("collator.another1");
        acTokenChallenge.Value++;

        string msg = ModTranslation.getString("collatorTargets") + "\n" + "  " + Helpers.cs(player1.player.Data.Color, player1.player.Data.PlayerName) + "\n" +
             "  " + Helpers.cs(player2.player.Data.Color, player2.player.Data.PlayerName) + "\n" + (matched ? ModTranslation.getString("collatorMatched").Color(Color.green) :
             ModTranslation.getString("collatorUnmatched").Color(Color.red)).Bold();

        MeetingOverlayHolder.RegisterOverlay(TORGUIContextEngine.API.VerticalHolder(GUIAlignment.Left,
                    new TORGUIText(GUIAlignment.Left, TORGUIContextEngine.API.GetAttribute(AttributeAsset.OverlayTitle), new TranslateTextComponent("collatorInfo")),
                    new TORGUIText(GUIAlignment.Left, TORGUIContextEngine.API.GetAttribute(AttributeAsset.OverlayContent), new RawTextComponent(msg)))
                    , MeetingOverlayHolder.IconsSprite[4], color);
        ShareResults.Invoke((player.PlayerId, player1.player.PlayerId, player2.player.PlayerId, matched));
    }

    public static TeamCategory CheckTeam(RoleInfo role)
    {
        var team = TeamCategory.NeutralStandalone;

        if (role.isImpostor) team = TeamCategory.Impostor;
        else if (!role.isNeutral) team = TeamCategory.Crewmate;
        else if (role.roleId == RoleId.Moriarty || (role.roleId == RoleId.SchrodingersCat && SchrodingersCat.team == SchrodingersCat.Team.Moriarty)) team = TeamCategory.Moriarty;
        else if (role.roleId == RoleId.JekyllAndHyde || (role.roleId == RoleId.JekyllAndHyde && SchrodingersCat.team == SchrodingersCat.Team.JekyllAndHyde)) team = TeamCategory.JekyllAndHyde;
        else if (role.roleId == RoleId.Jackal || role.roleId == RoleId.Sidekick ||
            (role.roleId == RoleId.SchrodingersCat && SchrodingersCat.team == SchrodingersCat.Team.Jackal)) team = TeamCategory.Jackal;
        else if (role.roleId == RoleId.Yandere || (role.roleId == RoleId.SchrodingersCat && SchrodingersCat.team == SchrodingersCat.Team.Yandere)) team = TeamCategory.Yandere;
        else if (role.roleId == RoleId.Pelican) team = TeamCategory.Pelican;

        if (!strictNeutral && role.isNeutral)
            team = TeamCategory.Jackal;

        return team;
    }

    public override void OnMeetingStart()
    {
        if (PlayerControl.LocalPlayer == player && ActualSampledPlayers >= 2)
        {
            RegisterResults(sampledPlayers[0], sampledPlayers[1]);
            sampledPlayers.Clear();
            UpdateSamples();
            trialsLeft--;
        }
    }

    public override void OnMeetingEnd(PlayerControl exiled = null)
    {
        sampledPlayers.Clear();
        UpdateSamples();
    }

    public void UpdateSamples()
    {
        for (int i = 0; i < allSamples.Length; i++)
        {
            PlayerControl player = sampledPlayers.Count > i ? sampledPlayers[i].player : null;
            if (player != null)
            {
                allSamples[i].sample.gameObject.SetActive(true);
                allSamples[i].sample.color = player.Data.Color;
            }
            else allSamples[i].sample.gameObject.SetActive(false);
        }
    }

    public IEnumerator CoShakeTube(int index)
    {
        var tube = allSamples[index].tube;
        var transform = tube.transform;
        float p = 0f;
        while (p < 1f)
        {
            p += Time.deltaTime * 1.15f;
            transform.localEulerAngles = new(0f, 0f, 24f * Mathf.Sin(p * 29.2f) * (1f - p));
            transform.localScale = Vector3.one * (1f + (1f - (p * p)) * 0.4f);
            yield return null;
        }
        transform.localEulerAngles = new(0f, 0f, 0f);
        transform.localScale = Vector3.one;
    }

    public Collator()
    {
        RoleId = roleId = RoleId.Collator;
        trialsLeft = Mathf.RoundToInt(CustomOptionHolder.collatorNumberOfTrials.getFloat());
        sampledPlayers = [];
        allSamples = [];
        sharedResults = [];
        currentTarget = null;
        acTokenChallenge = null;
    }

    public static void clearAndReload()
    {
        cooldown = CustomOptionHolder.collatorCooldown.getFloat();
        madmateAsCrew = CustomOptionHolder.collatorMadmateSpecifiedAsCrewmate.getBool();
        strictNeutral = CustomOptionHolder.collatorStrictNeutralRoles.getBool();
        players = [];
    }
}
