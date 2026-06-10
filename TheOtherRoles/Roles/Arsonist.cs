using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TheOtherRoles.MetaContext;
using TheOtherRoles.Modules;
using TheOtherRoles.Patches;
using UnityEngine;
using static TheOtherRoles.Patches.PlayerControlFixedUpdatePatch;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    [TORRPCHolder]
    public class Arsonist : RoleBase<Arsonist> {
        public static Color color = new Color32(238, 112, 46, byte.MaxValue);

        public Arsonist()
        {
            RoleId = roleId = RoleId.Arsonist;
            currentTarget = null;
            douseTarget = null;
            dousedPlayers = [];
        }

        static public IEnumerable<HelpSprite> GetHelpSprites()
        {
            yield return new(getDouseSprite(), "arsonistDouseHint");
            yield return new(getIgniteSprite(), "arsonistIgniteHint");
        }

        static public IEnumerable<DocumentReplacement> GetReplacementPart()
        {
            yield return new("%SEC%", duration.ToString());
        }

        public override GUIContext ProgressContext { get => ProgressGUI.Holder(ProgressGUI.OneLineText(ModTranslation.getString("roleInfoDousedLeft")),
            ProgressGUI.Holder(PlayerControl.AllPlayerControls.ToArray().Where(x => x != player && !x.Data.IsDead && !dousedPlayers.Contains(x)).Select(p =>
            ProgressGUI.OneLineText("-" + p.Data.PlayerName))).Move(new(0.04f, 0f))); }

        public static float cooldown = 30f;
        public static float duration { get { return CustomOptionHolder.arsonistDuration.getFloat(); } }
        public bool triggerArsonistWin = false;

        public PlayerControl currentTarget;
        public PlayerControl douseTarget;
        public List<PlayerControl> dousedPlayers = [];

        private static Sprite douseSprite;
        public static Sprite getDouseSprite() {
            if (douseSprite) return douseSprite;
            douseSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.DouseButton.png", 115f);
            return douseSprite;
        }

        public static RemoteProcess<byte> TriggerWin = RemotePrimitiveProcess.OfByte("ArsonistWin", (message, _) =>
        {
            PlayerControl player = Helpers.playerById(message);
            var arsonist = getRole(player);
            if (player == null || arsonist == null) return;
            arsonist.triggerArsonistWin = true;
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (p != player && !p.Data.IsDead)
                {
                    if (p.isRole(RoleId.NekoKabocha)) NekoKabocha.getRole(p).otherKiller = player;
                    p.Exiled();
                    GameHistory.overrideDeathReasonAndKiller(p, DeadPlayer.CustomDeathReason.Arson, player);
                }
            }
        });

        public static RemoteProcess<(byte arsonistId, byte playerId)> Douse = new("ArsonistDouse", (message, _) =>
        {
            PlayerControl player = Helpers.playerById(message.arsonistId);
            PlayerControl target = Helpers.playerById(message.playerId);
            var arsonist = getRole(player);
            if (player == null || arsonist == null || target == null) return;
            arsonist.dousedPlayers.Add(target);
        });

        private static Sprite igniteSprite;
        public static Sprite getIgniteSprite() {
            if (igniteSprite) return igniteSprite;
            igniteSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.IgniteButton.png", 115f);
            return igniteSprite;
        }

        public bool dousedEveryoneAlive() {
            return PlayerControl.AllPlayerControls.ToArray().All(x => { return x == player || x.Data.IsDead || x.Data.Disconnected || dousedPlayers.Any(y => y.PlayerId == x.PlayerId); });
        }

        public override void ResetRole(bool isShifted)
        {
            TORMapOptions.resetPoolables();
        }

        public override void OnMeetingEnd(PlayerControl exiled = null)
        {
            douseTarget = null;
            if (player == PlayerControl.LocalPlayer)
            {
                var notDoused = new List<PlayerControl>();

                int visibleCounter = 0;
                Vector3 newBottomLeft = IntroCutsceneOnDestroyPatch.bottomLeft;
                var BottomLeft = newBottomLeft + new Vector3(-0.25f, -0.25f, 0);
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (((!p.Data.IsDead && !p.Data.Disconnected) || (exiled != null && exiled.PlayerId == p.PlayerId)) && !dousedPlayers.Contains(p) && player != p) notDoused.Add(p);
                    if (!TORMapOptions.playerIcons.ContainsKey(p.PlayerId) || player == p) continue;
                    TORMapOptions.playerIcons[p.PlayerId].transform.localScale = Vector3.one * 0.2f;
                    if (p.Data.IsDead || p.Data.Disconnected) {
                        TORMapOptions.playerIcons[p.PlayerId].gameObject.SetActive(false);
                    }
                    else {
                        TORMapOptions.playerIcons[p.PlayerId].transform.localPosition = BottomLeft + Vector3.right * visibleCounter * 0.35f;
                        TORMapOptions.playerIcons[p.PlayerId].gameObject.SetActive(true);
                        visibleCounter++;
                    }
                    if (!dousedPlayers.Contains(p)) TORMapOptions.playerIcons[p.PlayerId].setSemiTransparent(true);
                }

                if (notDoused.Count == 1 && exiled != null && notDoused[0].PlayerId == exiled.PlayerId)
                    _ = new StaticAchievementToken("arsonist.another1");
            }
        }

        public override void FixedUpdate()
        {
            if (player != PlayerControl.LocalPlayer) return;
            List<PlayerControl> untargetables;
            if (douseTarget != null) {
                untargetables = [];
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player.PlayerId != douseTarget.PlayerId)
                        untargetables.Add(player);
                }
            }
            else {
                untargetables = dousedPlayers;
            }
            currentTarget = setTarget(untargetablePlayers: untargetables);
            if (currentTarget != null) setPlayerOutline(currentTarget, color);
        }

        public static void clearAndReload() {
            if (players != null) players.Do(x => x.triggerArsonistWin = false);
            cooldown = CustomOptionHolder.arsonistCooldown.getFloat();
            players = [];
        }
    }
}
