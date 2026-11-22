using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Hazel;
using TheOtherRoles.MetaContext;
using TheOtherRoles.Modules;
using TheOtherRoles.Patches;
using TheOtherRoles.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static TheOtherRoles.Patches.PlayerControlFixedUpdatePatch;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    [TORRPCHolder]
    public class SchrodingersCat : RoleBase<SchrodingersCat>
    {
        public SchrodingersCat()
        {
            RoleId = roleId = RoleId.SchrodingersCat;
        }

        static public SpriteLoader hintSprite = SpriteLoader.FromResource("TheOtherRoles.Resources.SchrodingersCatHint.png", 100f);
        static public IEnumerable<HelpSprite> GetHelpSprites()
        {
            if (CustomOptionHolder.schrodingersCatHideRole.getBool() && CustomOptionHolder.schrodingersCatCanChooseImpostor.getBool())
            {
                yield return new(Morphling.getSampleSprite(), "schrodingersCatSwitchHint");
                yield return new(hintSprite, "schrodingersCatButtonHint");
            }
        }

        public override void ResetRole(bool isShifted)
        {
            if (!isShifted)
            {
                RoleInfo.schrodingersCat.color = color;
                RoleInfo.schrodingersCat.isNeutral = true;
                team = Team.None;
            }
            if (teams != null) teams.ForEach(x => x.gameObject.SetActive(false));
        }

        public static RemoteProcess<byte> SetTeam = RemotePrimitiveProcess.OfByte("SchrodingersCatSetTeam", (message, _) =>
        {
            switch ((Team)message)
            {
                case Team.Crewmate:
                    setCrewFlag();
                    break;
                case Team.Impostor:
                    setImpostorFlag();
                    allPlayers.ForEach(x => FastDestroyableSingleton<RoleManager>.Instance.SetRole(x, RoleTypes.Impostor));
                    break;
                case Team.Jackal:
                    setJackalFlag();
                    break;
                case Team.JekyllAndHyde:
                    setJekyllAndHydeFlag();
                    break;
                case Team.Moriarty:
                    setMoriartyFlag();
                    break;
                default:
                    setCrewFlag();
                    break;
            }
            if (PlayerControl.LocalPlayer.isRole(RoleId.SchrodingersCat)) new StaticAchievementToken("schrodingersCat.another1");
            allPlayers.ForEach(x => TORGameManager.Instance?.RecordRoleHistory(x));
        });

        public override void OnDeath(PlayerControl killer = null)
        {
            if (killer != null && killer != player)
            {
                if (PlayerControl.LocalPlayer == player) _ = new StaticAchievementToken("schrodingersCat.common1");
                player.clearAllTasks();
                if (!hasTeam())
                {
                    if (killer.Data.Role.IsImpostor)
                    {
                        setImpostorFlag();
                        if (becomesImpostor) FastDestroyableSingleton<RoleManager>.Instance.SetRole(player, RoleTypes.Impostor);
                    }
                    else if (killer.isRole(RoleId.Jackal) || killer.isRole(RoleId.Sidekick))
                        setJackalFlag();
                    else if (killer.isRole(RoleId.JekyllAndHyde))
                        setJekyllAndHydeFlag();
                    else if (killer.isRole(RoleId.Moriarty))
                        setMoriartyFlag();
                    else if (killer.isRole(RoleId.Yandere))
                        setYandereFlag();
                    else
                    {
                        if (!justDieOnKilledByCrew)
                            setCrewFlag();
                    }

                    player.ModRevive();
                    DeadBody[] array = Object.FindObjectsOfType<DeadBody>();
                    for (int i = 0; i < array.Length; i++)
                        if (GameData.Instance.GetPlayerById(array[i].ParentId).PlayerId == player.PlayerId)                             array[i].gameObject.active = false;
                    DeadPlayer deadPlayerEntry = GameHistory.deadPlayers.Where(x => x.player.PlayerId == player.PlayerId).FirstOrDefault();
                    if (deadPlayerEntry != null) GameHistory.deadPlayers.Remove(deadPlayerEntry);
                    TORGameManager.Instance?.RecordRoleHistory(player);
                    TORGameManager.Instance?.GameStatistics.RecordEvent(new(GameStatistics.EventVariation.Revive, null, 1 << player.PlayerId) { RelatedTag = EventDetail.Revive });
                }
            }
            else
            {
                // Assign the default ghost role to let the Schrodinger's Cat have the haunt button
                if (!hasTeam())
                {
                    isExiled = true;
                    if (AmongUsClient.Instance.AmHost) FastDestroyableSingleton<RoleManager>.Instance.AssignRoleOnDeath(player, false);
                }
            }
        }

        public static Color color = Color.grey;
        public static Team team;

        public static PoolablePlayer playerTemplate;
        public static GameObject parent;
        public static List<PoolablePlayer> teams;
        public static bool shownMenu = false;
        public static PlayerControl currentTarget;

        public static bool isExiled = false;

        public enum Team
        {
            None,
            Impostor,
            Crewmate,
            Jackal,
            JekyllAndHyde,
            Moriarty,
            Yandere
        }

        public static bool isTeamJackalAlive()
        {
            foreach (var p in PlayerControl.AllPlayerControls.GetFastEnumerator())
                if (p.isRole(RoleId.Jackal) && !p.Data.IsDead)
                    return true;
                else if (p.isRole(RoleId.Sidekick) && !p.Data.IsDead)
                    return true;
            return false;
        }

        public static bool isLastImpostor()
        {
            foreach (var p in PlayerControl.AllPlayerControls.GetFastEnumerator())
                if (PlayerControl.LocalPlayer != p && p.Data.Role.IsImpostor && !p.Data.IsDead) return false;
            return true;
        }

        public static void setImpostorFlag()
        {
            RoleInfo.schrodingersCat.color = Palette.ImpostorRed;
            RoleInfo.schrodingersCat.isNeutral = false;
            team = Team.Impostor;
        }

        public static void setJackalFlag()
        {
            RoleInfo.schrodingersCat.color = Jackal.color;
            team = Team.Jackal;
        }

        public static void setJekyllAndHydeFlag()
        {
            RoleInfo.schrodingersCat.color = JekyllAndHyde.color;
            team = Team.JekyllAndHyde;
        }

        public static void setMoriartyFlag()
        {
            RoleInfo.schrodingersCat.color = Moriarty.color;
            team = Team.Moriarty;
        }

        public static void setYandereFlag()
        {
            RoleInfo.schrodingersCat.color = Yandere.color;
            team = Team.Yandere;
        }

        public static void setCrewFlag()
        {
            RoleInfo.schrodingersCat.color = Color.white;
            RoleInfo.schrodingersCat.isNeutral = false;
            team = Team.Crewmate;
        }

        public static int countLovers(Team team)
        {
            if (SchrodingersCat.team != team) return 0;
            int counter = 0;
            foreach (var player in allPlayers)
                if (player.isLovers()) counter += 1;
            return counter;
        }

        public static bool tasksComplete(PlayerControl p)
        {
            int counter = 0;
            var option = GameOptionsManager.Instance.currentNormalGameOptions;
            int totalTasks = option.NumLongTasks + option.NumShortTasks + option.NumCommonTasks;
            if (totalTasks == 0) return true;
            foreach (var task in p.Data.Tasks)
                if (task.Complete)
                    counter++;
            return counter == totalTasks;
        }

        public static float killCooldown;
        public static bool becomesImpostor;
        public static bool cantKillUntilLastOne;
        public static bool justDieOnKilledByCrew;
        public static bool hideRole;
        public static bool canChooseImpostor;

        public static bool hasTeam() => team != Team.None;

        public static void showMenu()
        {
            if (!shownMenu)
                if (teams.Count == 0)
                {
                    var colorBG = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.White.png", 100f);
                    var hudManager = FastDestroyableSingleton<HudManager>.Instance;
                    parent = new GameObject("PoolableParent");
                    parent.transform.parent = hudManager.transform;
                    parent.transform.localPosition = new Vector3(0, 0, 0);
                    var impostor = createPoolable(parent, "impostor", 0, (UnityAction)(() =>
                    {
                        SetTeam.Invoke((byte)Team.Impostor);
                        showMenu();
                    }));
                    teams.Add(impostor);
                    if (Jackal.exists || Sidekick.exists)
                    {
                        var jackal = createPoolable(parent, "jackal", 1, (UnityAction)(() =>
                        {
                            SetTeam.Invoke((byte)Team.Jackal);
                            showMenu();
                        }));
                        teams.Add(jackal);
                    }
                    if (Moriarty.exists)
                    {
                        var moriarty = createPoolable(parent, "moriarty", 2, (UnityAction)(() =>
                        {
                            SetTeam.Invoke((byte)Team.Moriarty);
                            showMenu();
                        }));
                        teams.Add(moriarty);
                    }
                    if (JekyllAndHyde.exists)
                    {
                        var jekyllAndHyde = createPoolable(parent, "jekyllAndHyde", 6, (UnityAction)(() =>
                        {
                            SetTeam.Invoke((byte)Team.JekyllAndHyde);
                            showMenu();
                        }));
                        teams.Add(jekyllAndHyde);
                    }
                    if (Yandere.exists)
                    {
                        var yandere = createPoolable(parent, "yandere", 12, (UnityAction)(() =>
                        {
                            SetTeam.Invoke((byte)Team.Yandere);
                            showMenu();
                        }));
                        teams.Add(yandere);
                    }
                    var crewmate = createPoolable(parent, "crewmate", 10, (UnityAction)(() =>
                    {
                        SetTeam.Invoke((byte)Team.Crewmate);
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
            else
                teams.ForEach(x =>
                {
                    x.gameObject.SetActive(false);
                });
            shownMenu = !shownMenu;
        }

        public static bool isJackalButtonEnable()
        {
            if (team == Team.Jackal && isRole(PlayerControl.LocalPlayer) && !PlayerControl.LocalPlayer.Data.IsDead)
                if (!isTeamJackalAlive() || !cantKillUntilLastOne)
                    return true;
            return false;
        }

        public static bool isJekyllAndHydeButtonEnable()
        {
            if (team == Team.JekyllAndHyde && isRole(PlayerControl.LocalPlayer) && !PlayerControl.LocalPlayer.Data.IsDead)
                if (!JekyllAndHyde.hasAlivePlayers || !cantKillUntilLastOne)
                    return true;
            return false;
        }

        public static bool isYandereButtonEnable()
        {
            if (team == Team.Yandere && isRole(PlayerControl.LocalPlayer) && !PlayerControl.LocalPlayer.Data.IsDead)
                if (!Yandere.hasAlivePlayers || !cantKillUntilLastOne)
                    return true;
            return false;
        }

        public override void OnKill(PlayerControl target)
        {
            if (PlayerControl.LocalPlayer == player && team == Team.Impostor)
                player.SetKillTimerUnchecked(killCooldown);
        }

        public override void OnMeetingEnd(PlayerControl exiled = null)
        {
            if (PlayerControl.LocalPlayer == player && PlayerControl.LocalPlayer.Data.Role.IsImpostor)
                PlayerControl.LocalPlayer.SetKillTimerUnchecked(killCooldown);
        }

        public static bool isMoriartyButtonEnable()
        {
            if (team == Team.Moriarty && isRole(PlayerControl.LocalPlayer) && !PlayerControl.LocalPlayer.Data.IsDead)
                if (!Moriarty.hasAlivePlayers || !cantKillUntilLastOne)
                    return true;
            return false;
        }

        public override void FixedUpdate()
        {
            if (player != PlayerControl.LocalPlayer) return;
            var untargetables = new List<PlayerControl>();
            if (team == Team.Jackal)
            {
                if (!isTeamJackalAlive() || !cantKillUntilLastOne)
                {
                    untargetables.AddRange(Jackal.allPlayers);
                    untargetables.AddRange(Sidekick.allPlayers);
                    currentTarget = setTarget(untargetablePlayers: untargetables);
                    setPlayerOutline(currentTarget, Palette.ImpostorRed);
                }
            }
            else if (team == Team.JekyllAndHyde)
            {
                if (!JekyllAndHyde.hasAlivePlayers || !cantKillUntilLastOne)
                {
                    untargetables.AddRange(JekyllAndHyde.allPlayers);
                    currentTarget = setTarget(untargetablePlayers: untargetables);
                    setPlayerOutline(currentTarget, JekyllAndHyde.color);
                }
            }
            else if (team == Team.Yandere)
            {
                if (!Yandere.hasAlivePlayers || !cantKillUntilLastOne)
                {
                    untargetables.AddRange(Yandere.allPlayers);
                    currentTarget = setTarget(untargetablePlayers: untargetables);
                    setPlayerOutline(currentTarget, Yandere.color);
                }
            }
            else if (team == Team.Moriarty)
            {
                if (!Moriarty.hasAlivePlayers || !cantKillUntilLastOne)
                {
                    untargetables.AddRange(Moriarty.allPlayers);
                    currentTarget = setTarget(untargetablePlayers: untargetables);
                    setPlayerOutline(currentTarget, Moriarty.color);
                }
            }
            else if (team == Team.Impostor && !isLastImpostor() && cantKillUntilLastOne) {
                FastDestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(null);
            }

            if (player.Data.IsDead || hasTeam() || MeetingHud.Instance || ExileController.Instance) {
                if (shownMenu) showMenu();
                return;
            }
        }

        private static PoolablePlayer createPoolable(GameObject parent, string name, int color, UnityAction func)
        {
            var poolable = Object.Instantiate(playerTemplate, parent.transform);
            var actionButton = Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.KillButton, poolable.gameObject.transform);
            SpriteRenderer spriteRenderer = actionButton.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = null;
            actionButton.transform.localPosition = new Vector3(0, 0, 0);
            actionButton.gameObject.SetActive(true);
            actionButton.gameObject.ForEachChild((Il2CppSystem.Action<GameObject>)((c) => { if (c.name.Equals("HotKeyGuide")) Object.Destroy(c); }));
            PassiveButton button = actionButton.GetComponent<PassiveButton>();
            button.OnClick = new Button.ButtonClickedEvent();
            button.OnClick.AddListener(func);
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
                float x = teams.Count % 2 != 0 ? offset * (i - center) : offset * (i - center) + offset * 0.5f;
                teams[i].transform.localPosition = new Vector3(x, 0, 0);
                teams[i].transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                teams[i].GetComponentInChildren<ActionButton>().transform.position = teams[i].transform.position;
            }
        }

        public static void clearAndReload()
        {
            currentTarget = null;
            team = Team.None;
            canChooseImpostor = CustomOptionHolder.schrodingersCatHideRole.getBool() && CustomOptionHolder.schrodingersCatCanChooseImpostor.getBool();
            hideRole = CustomOptionHolder.schrodingersCatHideRole.getBool();
            justDieOnKilledByCrew = CustomOptionHolder.schrodingersCatJustDieOnKilledByCrew.getBool();
            cantKillUntilLastOne = CustomOptionHolder.schrodingersCatCantKillUntilLastOne.getBool();
            becomesImpostor = CustomOptionHolder.schrodingersCatBecomesImpostor.getBool();
            killCooldown = CustomOptionHolder.schrodingersCatKillCooldown.getFloat();
            RoleInfo.schrodingersCat.color = color;
            RoleInfo.schrodingersCat.isNeutral = true;
            shownMenu = false;
            if (teams != null) teams.ForEach(x => { if (x != null && x.gameObject) x.gameObject.SetActive(false); });
            teams = [];
            isExiled = false;
            players = [];
        }
    }
}
