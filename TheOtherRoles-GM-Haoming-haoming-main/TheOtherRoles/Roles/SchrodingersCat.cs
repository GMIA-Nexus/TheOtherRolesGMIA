using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using TheOtherRoles.Objects;
using TheOtherRoles.Patches;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using static TheOtherRoles.Patches.PlayerControlFixedUpdatePatch;
using static TheOtherRoles.TheOtherRoles;
using AmongUs.GameOptions;

namespace TheOtherRoles
{
    [HarmonyPatch]
    public class SchrodingersCat : RoleBase<SchrodingersCat>
    {
        public enum exileType
        {
            None = 0,
            Crew = 1,
            Random = 2,
        }

        public enum Team
        {
            None = 0,
            Crew = 1,
            Impostor = 2,
            Jackal = 3,
            JekyllAndHyde = 4,
            Moriarty = 5,
        }

        public static Color color = Color.grey;
        public static Team team = Team.None;
        public static float killCooldown { get { return CustomOptionHolder.schrodingersCatKillCooldown.getFloat(); } }
        public static bool becomesImpostor { get { return CustomOptionHolder.schrodingersCatBecomesImpostor.getBool(); } }
        public static exileType becomesWhichTeamsOnExiled { get { return (exileType)CustomOptionHolder.schrodingersCatBecomesWhichTeamsOnExiled.getSelection(); } }
        public static bool cantKillUntilLastOne { get { return CustomOptionHolder.schrodingersCatCantKillUntilLastOne.getBool(); } }
        public static bool killsKiller { get { return CustomOptionHolder.schrodingersCatKillsKiller.getBool(); } }
        public static bool justDieOnKilledByCrew { get { return CustomOptionHolder.schrodingersCatJustDieOnKilledByCrew.getBool(); } }
        public static bool hideRole { get { return CustomOptionHolder.schrodingersCatHideRole.getBool(); } }
        public static bool canWinAsCrewmate { get { return CustomOptionHolder.schrodingersCatHideRole.getBool() && CustomOptionHolder.schrodingersCatCanWinAsCrewmate.getBool(); } }
        public static bool canChooseImpostor { get { return CustomOptionHolder.schrodingersCatHideRole.getBool() && CustomOptionHolder.schrodingersCatCanChooseImpostor.getBool(); } }
        public static PlayerControl killer = null;

        public SchrodingersCat()
        {
            RoleType = roleId = RoleType.SchrodingersCat;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd()
        {
            if (PlayerControl.LocalPlayer.isRole(RoleType.SchrodingersCat))
                PlayerControl.LocalPlayer.SetKillTimerUnchecked(killCooldown);
        }
        public override void FixedUpdate()
        {
            if (player == PlayerControl.LocalPlayer && team == Team.Jackal)
            {
                if (!isTeamJackalAlive() || !cantKillUntilLastOne)
                {
                    currentTarget = setTarget();
                    setPlayerOutline(currentTarget, Sheriff.color);
                }
            }
            if (player == PlayerControl.LocalPlayer && team == Team.JekyllAndHyde)
            {
                if (JekyllAndHyde.livingPlayers.Count == 0 || !cantKillUntilLastOne)
                {
                    currentTarget = setTarget();
                    setPlayerOutline(currentTarget, Sheriff.color);
                }
            }
            if (player == PlayerControl.LocalPlayer && team == Team.Moriarty)
            {
                if (Moriarty.livingPlayers.Count == 0 || !cantKillUntilLastOne)
                {
                    currentTarget = setTarget();
                    setPlayerOutline(currentTarget, Sheriff.color);
                }
            }
            if (player == PlayerControl.LocalPlayer && team == Team.Impostor && !isLastImpostor() && cantKillUntilLastOne)
            {
                FastDestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(null);
            }
        }

        public override void OnKill(PlayerControl target)
        {
            if (PlayerControl.LocalPlayer == player && team == Team.Impostor)
                player.SetKillTimerUnchecked(killCooldown);
        }
        public override void OnDeath(PlayerControl killer = null)
        {
            player.clearAllTasks();
            // 占い師の画面では呪殺したことを分からなくするために自殺処理させているので注意すること
            if (team != Team.None) return;
            if (((killer != null && killer.isCrew()) || killer.isRole(RoleType.SchrodingersCat)) && justDieOnKilledByCrew) return;
            if (killer == null)
            {
                if (becomesWhichTeamsOnExiled == exileType.Random && player == PlayerControl.LocalPlayer)
                {
                    List<Team> candidates = new();
                    candidates.Add(Team.Crew);
                    candidates.Add(Team.Impostor);
                    if (JekyllAndHyde.exists) candidates.Add(Team.JekyllAndHyde);
                    if (Moriarty.exists) candidates.Add(Team.Moriarty);
                    if (Jackal.jackal != null) candidates.Add(Team.Jackal);
                    int rndVal = rnd.Next(0, candidates.Count);
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SchrodingersCatSetTeam, Hazel.SendOption.Reliable, -1);
                    writer.Write((byte)candidates[rndVal]);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.schrodingersCatSetTeam((byte)candidates[rndVal]);
                }
                else if (becomesWhichTeamsOnExiled == exileType.Crew)
                {
                    setCrewFlag();
                }
                return;
            }
            else
            {
                bool isCrewOrSchrodingersCat = (!killer.isRole(RoleType.JekyllAndHyde) && killer.isCrew()) || killer.isRole(RoleType.SchrodingersCat);
                if (killer.isImpostor())
                {
                    setImpostorFlag();
                    if (becomesImpostor)
                        FastDestroyableSingleton<RoleManager>.Instance.SetRole(player, RoleTypes.Impostor);
                }
                else if (killer.isRole(RoleType.Jackal))
                {
                    setJackalFlag();
                }
                else if (killer.isRole(RoleType.JekyllAndHyde))
                {
                    setJekyllAndHydeFlag();
                }
                else if (killer.isRole(RoleType.Moriarty))
                {
                    setMoriartyFlag();
                }
                else if (isCrewOrSchrodingersCat)
                {
                    setCrewFlag();
                }

                // EndGamePatchでゲームを終了させないために先にkillerに値を代入する
                if (SchrodingersCat.killsKiller && !isCrewOrSchrodingersCat)
                    SchrodingersCat.killer = killer;


                // 蘇生する
                player.ReviveAndClean();
                if (SchrodingersCat.killsKiller && !isCrewOrSchrodingersCat)
                {
                    if (PlayerControl.LocalPlayer == killer)
                    {
                        // 死亡までのカウントダウン
                        TMPro.TMP_Text text;
                        RoomTracker roomTracker = FastDestroyableSingleton<HudManager>.Instance?.roomTracker;
                        GameObject gameObject = UnityEngine.Object.Instantiate(roomTracker.gameObject);
                        UnityEngine.Object.DestroyImmediate(gameObject.GetComponent<RoomTracker>());
                        gameObject.transform.SetParent(FastDestroyableSingleton<HudManager>.Instance.transform);
                        gameObject.transform.localPosition = new Vector3(0, -1.8f, gameObject.transform.localPosition.z);
                        gameObject.transform.localScale = Vector3.one * 3f;
                        text = gameObject.GetComponent<TMPro.TMP_Text>();
                        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(15f, new Action<float>((p) =>
                        {
                            string message = (15 - (p * 15f)).ToString("0");
                            bool even = ((int)(p * 15f / 0.25f)) % 2 == 0; // Bool flips every 0.25 seconds
                            string prefix = even ? "<color=#FCBA03FF>" : "<color=#FF0000FF>";
                            text.text = prefix + message + "</color>";
                            if (text != null) text.color = even ? Color.yellow : Color.red;
                            if (p == 1f && text != null && text.gameObject != null)
                            {
                                if (SchrodingersCat.killer != null && SchrodingersCat.killer.isAlive())
                                {
                                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SchrodingersCatSuicide, Hazel.SendOption.Reliable, -1);
                                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                                    RPCProcedure.schrodingersCatSuicide();
                                    SchrodingersCat.killer = null;
                                }
                                UnityEngine.Object.Destroy(text.gameObject);
                            }
                        })));
                    }
                }
            }
        }
        public override void OnFinishShipStatusBegin() { }

        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        private static CustomButton killButton;
        private static CustomButton switchButton;
        public static PlayerControl currentTarget;
        public static void MakeButtons(HudManager hm)
        {
            killButton = new CustomButton(
                () =>
                {
                    if (Helpers.checkMuderAttemptAndKill(PlayerControl.LocalPlayer, SchrodingersCat.currentTarget) == MurderAttemptResult.SuppressKill) return;

                    killButton.Timer = killButton.MaxTimer;
                    Jackal.currentTarget = null;
                },
                () => { return isJackalButtonEnable() || isJekyllAndHydeButtonEnable() || isMoriartyButtonEnable(); },
                () => { return SchrodingersCat.currentTarget && PlayerControl.LocalPlayer.CanMove; },
                () => { killButton.Timer = killButton.MaxTimer; },
                hm.KillButton.graphic.sprite,
                CustomButton.ButtonPositions.upperRowRight,
                hm,
                hm.KillButton,
                KeyCode.Q
            );
            killButton.Timer = killButton.MaxTimer = killCooldown;
            switchButton = new CustomButton(
                () =>
                {
                    showMenu();
                    // MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SchrodingersCatSetTeam, Hazel.SendOption.Reliable, -1);
                    // writer.Write((byte)SchrodingersCat.Team.Impostor);
                    // AmongUsClient.Instance.FinishRpcImmediately(writer);
                    // RPCProcedure.schrodingersCatSetTeam((byte)SchrodingersCat.Team.Impostor);
                },
                () => { return SchrodingersCat.team == SchrodingersCat.Team.None && canChooseImpostor && PlayerControl.LocalPlayer.isRole(RoleType.SchrodingersCat) && tasksComplete(PlayerControl.LocalPlayer); },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () => { switchButton.Timer = 0; },
                getSwitchButtonSprite(),
                CustomButton.ButtonPositions.lowerRowRight,
                hm,
                hm.AbilityButton,
                KeyCode.F
            )
            { buttonText = ModTranslation.getString("schrodingersCatSwitchTeamButton") };
            switchButton.Timer = switchButton.MaxTimer = 0;
        }
        public static void SetButtonCooldowns()
        {
            killButton.MaxTimer = killCooldown;
        }

        public static void Clear()
        {
            players = new List<SchrodingersCat>();
            team = Team.None;
            RoleInfo.schrodingersCat.color = color;
            killer = null;
            shownMenu = false;
            teams = new List<PoolablePlayer>();
        }

        public static void setImpostorFlag()
        {
            team = Team.Impostor;
            RoleInfo.schrodingersCat.color = Palette.ImpostorRed;
        }

        public static void setCrewFlag()
        {
            team = Team.Crew;
            RoleInfo.schrodingersCat.color = Color.white;
        }

        public static void setJackalFlag()
        {
            team = Team.Jackal;
            RoleInfo.schrodingersCat.color = Jackal.color;
        }

        public static void setJekyllAndHydeFlag()
        {
            team = Team.JekyllAndHyde;
            RoleInfo.jekyllAndHyde.color = JekyllAndHyde.color;
        }
        public static void setMoriartyFlag()
        {
            team = Team.Moriarty;
            RoleInfo.moriarty.color = Moriarty.color;
        }

        public static bool isJackalButtonEnable()
        {
            if (team == Team.Jackal && PlayerControl.LocalPlayer.isRole(RoleType.SchrodingersCat) && PlayerControl.LocalPlayer.isAlive())
            {
                if (!isTeamJackalAlive() || !cantKillUntilLastOne)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool isJekyllAndHydeButtonEnable()
        {
            if (team == Team.JekyllAndHyde && PlayerControl.LocalPlayer.isRole(RoleType.SchrodingersCat) && PlayerControl.LocalPlayer.isAlive())
            {
                if (JekyllAndHyde.livingPlayers.Count == 0 || !cantKillUntilLastOne)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool isMoriartyButtonEnable()
        {
            if (team == Team.Moriarty && PlayerControl.LocalPlayer.isRole(RoleType.SchrodingersCat) && PlayerControl.LocalPlayer.isAlive())
            {
                if (Moriarty.livingPlayers.Count == 0 || !cantKillUntilLastOne)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool isTeamJackalAlive()
        {
            foreach (var p in PlayerControl.AllPlayerControls.ToArray())
            {
                if (p.isRole(RoleType.Jackal) && p.isAlive())
                {
                    return true;
                }
                else if (p.isRole(RoleType.Sidekick) && p.isAlive())
                {
                    return true;
                }
            }
            return false;
        }

        public static bool isLastImpostor()
        {
            foreach (var p in PlayerControl.AllPlayerControls.ToArray())
            {
                if (PlayerControl.LocalPlayer != p && p.isImpostor() && p.isAlive()) return false;
            }
            return true;
        }

        public static bool hasTeam()
        {
            return team != Team.None;
        }
        private static bool tasksComplete(PlayerControl p)
        {
            int counter = 0;
            var option = GameOptionsManager.Instance.currentNormalGameOptions;
            int totalTasks = option.NumLongTasks + option.NumShortTasks + option.NumCommonTasks;
            if (totalTasks == 0) return true;
            foreach (var task in p.Data.Tasks)
            {
                if (task.Complete)
                {
                    counter++;
                }
            }
            return counter == totalTasks;
        }

        private static Sprite switchButtonSprite;
        private static Sprite getSwitchButtonSprite()
        {
            if (switchButtonSprite) return switchButtonSprite;
            switchButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.MorphButton.png", 115f);
            return switchButtonSprite;
        }

        private static Sprite blankButtonSprite;
        private static Sprite getBlankButtonSprite()
        {
            if (blankButtonSprite) return blankButtonSprite;
            blankButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.BlankButton.png", 115f);
            return blankButtonSprite;
        }

        public static PoolablePlayer playerTemplate;
        public static GameObject parent;
        private static List<PoolablePlayer> teams;
        private static bool shownMenu = false;

        private static void showMenu()
        {
            if (!shownMenu)
            {
                if (teams.Count == 0)
                {
                    var colorBG = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.White.png", 100f);
                    var hudManager = FastDestroyableSingleton<HudManager>.Instance;
                    parent = new GameObject("PoolableParent");
                    parent.transform.parent = hudManager.transform;
                    parent.transform.localPosition = new Vector3(0, 0, 0);
                    var impostor = createPoolable(parent, "schrodingersCatImpostor", 0, (UnityAction)(() =>
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SchrodingersCatSetTeam, Hazel.SendOption.Reliable, -1);
                        writer.Write((byte)SchrodingersCat.Team.Impostor);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.schrodingersCatSetTeam((byte)SchrodingersCat.Team.Impostor);
                        showMenu();
                    }));
                    teams.Add(impostor);
                    if (PlayerControl.AllPlayerControls.ToSystemList().Count(x => x.isRole(RoleType.Jackal)) > 0)
                    {
                        var jackal = createPoolable(parent, "jackal", 1, (UnityAction)(() =>
                        {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SchrodingersCatSetTeam, Hazel.SendOption.Reliable, -1);
                            writer.Write((byte)SchrodingersCat.Team.Jackal);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.schrodingersCatSetTeam((byte)SchrodingersCat.Team.Jackal);
                            showMenu();
                        }));
                        teams.Add(jackal);
                    }
                    if (PlayerControl.AllPlayerControls.ToSystemList().Count(x => x.isRole(RoleType.Moriarty)) > 0)
                    {
                        var moriarty = createPoolable(parent, "moriarty", 2, (UnityAction)(() =>
                        {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SchrodingersCatSetTeam, Hazel.SendOption.Reliable, -1);
                            writer.Write((byte)SchrodingersCat.Team.Moriarty);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.schrodingersCatSetTeam((byte)SchrodingersCat.Team.Moriarty);
                            showMenu();
                        }));
                        teams.Add(moriarty);
                    }
                    if (PlayerControl.AllPlayerControls.ToSystemList().Count(x => x.isRole(RoleType.JekyllAndHyde)) > 0)
                    {
                        var jekyllAndHyde = createPoolable(parent, "jekyllAndHyde", 6, (UnityAction)(() =>
                        {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SchrodingersCatSetTeam, Hazel.SendOption.Reliable, -1);
                            writer.Write((byte)SchrodingersCat.Team.JekyllAndHyde);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.schrodingersCatSetTeam((byte)SchrodingersCat.Team.JekyllAndHyde);
                            showMenu();
                        }));
                        teams.Add(jekyllAndHyde);
                    }
                    var crewmate = createPoolable(parent, "schrodingersCatCrew", 10, (UnityAction)(() =>
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SchrodingersCatSetTeam, Hazel.SendOption.Reliable, -1);
                        writer.Write((byte)SchrodingersCat.Team.Crew);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.schrodingersCatSetTeam((byte)SchrodingersCat.Team.Crew);
                        showMenu();
                    }));
                    teams.Add(crewmate);
                    layoutPoolable();
                }
                else
                {
                    teams.ForEach(x =>
                    {
                        x.gameObject.SetActive(true);
                    });
                    layoutPoolable();
                }
            }
            else
            {
                teams.ForEach(x =>
                {
                    x.gameObject.SetActive(false);
                });
            }
            shownMenu = !shownMenu;
        }

        private static PoolablePlayer createPoolable(GameObject parent, string name, int color, UnityAction func)
        {
            var poolable = GameObject.Instantiate(playerTemplate, parent.transform);
            var actionButton = UnityEngine.Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.KillButton, poolable.gameObject.transform);
            SpriteRenderer spriteRenderer = actionButton.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = null;
            actionButton.transform.localPosition = new Vector3(0, 0, 0);
            actionButton.gameObject.SetActive(true);
            PassiveButton button = actionButton.GetComponent<PassiveButton>();
            button.OnClick = new Button.ButtonClickedEvent();
            button.OnClick.AddListener((UnityAction)func);
            var texts = actionButton.GetComponentsInChildren<TMPro.TextMeshPro>();
            texts.ForEach(x => x.gameObject.SetActive(false));
            poolable.gameObject.SetActive(true);
            poolable.SetBodyColor(color);
            poolable.SetName(ModTranslation.getString(name));
            return poolable;
        }

        public static void layoutPoolable()
        {
            float offset = 2f;
            int center = teams.Count / 2;
            for (int i = 0; i < teams.Count; i++)
            {
                float x = teams.Count % 2 != 0 ? (offset * (i - center)) : (offset * (i - center)) + (offset * 0.5f);
                teams[i].transform.localPosition = new Vector3(x, 0, 0);
                teams[i].transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                teams[i].GetComponentInChildren<ActionButton>().transform.position = teams[i].transform.position;
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdReportDeadBody))]
        class PlayerControlCmdReportDeadBodyPatch
        {
            public static void Prefix(PlayerControl __instance)
            {
                // 時限爆弾よりも前にミーティングが来たら直後に死亡する
                if (killer != null && killsKiller)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SchrodingersCatSuicide, Hazel.SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.schrodingersCatSuicide();
                    killer = null;
                }
            }
        }
    }
}
