using System;
using System.Collections.Generic;
using TheOtherRoles.MetaContext;
using TheOtherRoles.Modules;
using TheOtherRoles.Utilities;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    public class Seer : RoleBase<Seer> {
        public static Color color = new Color32(97, 178, 108, byte.MaxValue);
        public static List<Vector3> deadBodyPositions = [];

        public static float soulDuration = 15f;
        public static bool limitSoulDuration = false;
        public static int mode = 0;
        public static bool canSeeKillTeams = true;

        public Seer()
        {
            RoleId = roleId = RoleId.Seer;
            acTokenChallenge = null;
            acTokenAnother = null;
        }

        public override void OnMeetingEnd(PlayerControl exiled = null)
        {
            if (PlayerControl.LocalPlayer == player)
            {
                acTokenAnother.Value.impKill = false;
                acTokenAnother.Value.lastMeeting = TORGameManager.Instance.CurrentTime;

                if (deadBodyPositions != null && (mode == 0 || mode == 2))
                {
                    foreach (Vector3 pos in deadBodyPositions)
                    {
                        GameObject soul = new();
                        //soul.transform.position = pos;
                        soul.transform.position = new Vector3(pos.x, pos.y, pos.y / 1000 - 1f);
                        soul.layer = 5;
                        var rend = soul.AddComponent<SpriteRenderer>();
                        soul.AddSubmergedComponent(SubmergedCompatibility.Classes.ElevatorMover);
                        rend.sprite = getSoulSprite();

                        if (limitSoulDuration)
                            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(soulDuration, new Action<float>((p) => {
                                if (rend != null)
                                {
                                    var tmp = rend.color;
                                    tmp.a = Mathf.Clamp01(1 - p);
                                    rend.color = tmp;
                                }
                                if (p == 1f && rend != null && rend.gameObject != null) UnityEngine.Object.Destroy(rend.gameObject);
                            })));
                    }
                    deadBodyPositions = [];
                }
            }
        }

        public override void OnMeetingStart()
        {
            if (PlayerControl.LocalPlayer == player)
            {
                acTokenChallenge.Value = 0;
                if (TORGameManager.Instance.CurrentTime - acTokenAnother.Value.lastMeeting <= 20 && acTokenAnother.Value.impKill)
                    acTokenAnother.Value.cleared = true;

                if (!PlayerControl.LocalPlayer.Data.IsDead && canSeeKillTeams)
                {
                    var killList = new List<string>();
                    var teamInfos = new (Func<KillInfo, int> selector, Color color, string name)[]
                    {
                        (kt => kt.impostor, Palette.ImpostorRed, "impostor"),
                        (kt => kt.crewmate, Color.white, "crewmate"),
                        (kt => kt.jackal, Jackal.color, "jackal"),
                        (kt => kt.jekyllAndHyde, JekyllAndHyde.color, "jekyllAndHyde"),
                        (kt => kt.moriarty, Moriarty.color, "moriarty"),
                        (kt => kt.yandere, Yandere.color, "yandere")
                    };

                    foreach (var (selector, color, name) in teamInfos)
                    {
                        if (selector(killTeams) > 0)
                            killList.Add(Helpers.cs(color, ModTranslation.getString(name)) + ": " + selector(killTeams).ToString());
                    }

                    if (killList.Count > 0)
                        MeetingOverlayHolder.RegisterOverlay(TORGUIContextEngine.API.VerticalHolder(GUIAlignment.Left,
                        new TORGUIText(GUIAlignment.Left, TORGUIContextEngine.API.GetAttribute(AttributeAsset.OverlayTitle), new TranslateTextComponent("seerMeetingInfo")),
                        new TORGUIText(GUIAlignment.Left, TORGUIContextEngine.API.GetAttribute(AttributeAsset.OverlayContent), new RawTextComponent(string.Join("\n", killList))))
                        , MeetingOverlayHolder.IconsSprite[3], color);
                    killTeams = new();
                }
            }
        }

        public struct KillInfo
        {
            public int impostor;
            public int crewmate;
            public int jackal;
            public int moriarty;
            public int jekyllAndHyde;
            public int pelican;
            public int yandere;

            public KillInfo() {
                impostor = crewmate = jackal = moriarty = jekyllAndHyde = pelican = yandere = 0;
            }
        }

        public static KillInfo killTeams = new();

        public AchievementToken<int> acTokenChallenge = null;
        public AchievementToken<(float lastMeeting, bool impKill, bool cleared)> acTokenAnother = null;

        public override void PostInit()
        {
            if (PlayerControl.LocalPlayer != player) return;
            acTokenAnother ??= new("seer.another1", (0, false, false), (val, _) => val.cleared);
            acTokenChallenge ??= new("seer.challenge", 0, (val, _) => val >= 5);
        }

        private static Sprite soulSprite;
        public static Sprite getSoulSprite() {
            if (soulSprite) return soulSprite;
            soulSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Soul.png", 500f);
            return soulSprite;
        }

        public static void clearAndReload() {
            deadBodyPositions = [];
            limitSoulDuration = CustomOptionHolder.seerLimitSoulDuration.getBool();
            soulDuration = CustomOptionHolder.seerSoulDuration.getFloat();
            mode = CustomOptionHolder.seerMode.getSelection();
            canSeeKillTeams = CustomOptionHolder.seerCanSeeKillTeams.getBool();
            killTeams = new();
            players = [];
        }
    }
}
