using System.Collections.Generic;
using System.Linq;
using Hazel;
using TheOtherRoles.Modules;
using TheOtherRoles.Patches;
using TheOtherRoles.Utilities;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;
using static TheOtherRoles.Objects.CustomButton;

namespace TheOtherRoles.Roles
{
    [TORRPCHolder]
    public class Bait : RoleBase<Bait>
    {
        public Bait()
        {
            RoleId = roleId = RoleId.Bait;
            reportDelay = CustomOptionHolder.baitReportDelay.getFloat();
            reported = true;
            acTokenChallenge = null;
            numUses = Mathf.RoundToInt(CustomOptionHolder.baitNumberOfEmits.getFloat());
        }

        public override void OnMeetingStart()
        {
            reported = true;
        }

        static public IEnumerable<HelpSprite> GetHelpSprites() {
            yield return new(getButtonSprite(), "baitEmitHint");
        }

        static public IEnumerable<DocumentReplacement> GetReplacementPart() => [new("%OPT%", ModTranslation.getString($"baitOPT{(highlightAllVents ? 2 : 1)}Hint"))];

        public static RemoteProcess<byte> Emit = RemotePrimitiveProcess.OfByte("BaitEmit", (message, _) =>
        {
            if (PlayerControl.LocalPlayer.PlayerId == message || Deputy.handcuffedKnows.ContainsKey(message)) return;
            for (int i = 0; i < buttons.Count; i++)
            {
                try
                {
                    if (!buttons[i].HasButton() || buttons[i].isSuicide || buttons[i].isEffectActive) continue;
                    buttons[i].Timer = buttons[i].MaxTimer;
                    buttons[i].Update();
                }
                catch (System.NullReferenceException)
                {
                    System.Console.WriteLine("[WARNING] NullReferenceException from HudUpdate().HasButton(), if theres only one warning its fine");
                }
            }
            if (PlayerControl.LocalPlayer.Data.Role.IsImpostor)
                PlayerControl.LocalPlayer.SetKillTimerUnchecked(Mathf.Max(PlayerControl.LocalPlayer.GetKillCooldown(), PlayerControl.LocalPlayer.killTimer));
        });

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.BaitButton.png", 115f);
            return buttonSprite;
        }

        public override void FixedUpdate()
        {
            if (player != PlayerControl.LocalPlayer) return;

            // Bait report
            if (PlayerControl.LocalPlayer.Data.IsDead && !reported)
            {
                reportDelay -= Time.fixedDeltaTime;
                DeadPlayer deadPlayer = GameHistory.deadPlayers?.Where(x => x.player?.PlayerId == player.PlayerId)?.FirstOrDefault();
                if (deadPlayer.killerIfExisting != null && reportDelay <= 0f)
                {
                    _ = new StaticAchievementToken("bait.common1");

                    Helpers.handleVampireBiteOnBodyReport(); // Manually call Vampire handling, since the CmdReportDeadBody Prefix won't be called
                    Helpers.HandleUndertakerDropOnBodyReport();
                    Helpers.handleTrapperTrapOnBodyReport();

                    byte reporter = deadPlayer.killerIfExisting.PlayerId;

                    if (Madmate.madmate.Any(x => x.PlayerId == player.PlayerId))
                    {
                        var candidates = PlayerControl.AllPlayerControls.GetFastEnumerator().ToArray().Where(x => !x.Data.IsDead && !x.Data.Role.IsImpostor).ToList();
                        int i = rnd.Next(0, candidates.Count);
                        reporter = candidates.Count > 0 ? candidates[i].PlayerId : deadPlayer.killerIfExisting.PlayerId;
                    }

                    acTokenChallenge.Value.killerId = reporter;

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedCmdReportDeadBody, SendOption.Reliable, -1);
                    writer.Write(reporter);
                    writer.Write(player.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.uncheckedCmdReportDeadBody(reporter, player.PlayerId);
                    reported = true;
                }
            }

            // Bait Vents
            if (MapUtilities.CachedShipStatus?.AllVents != null)
            {
                var ventsWithPlayers = new List<int>();
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player == null) continue;

                    if (player.inVent)
                    {
                        Vent target = MapUtilities.CachedShipStatus.AllVents.OrderBy(x => Vector2.Distance(x.transform.position, player.GetTruePosition())).FirstOrDefault();
                        if (target != null) ventsWithPlayers.Add(target.Id);
                    }
                }

                foreach (Vent vent in MapUtilities.CachedShipStatus.AllVents)
                {
                    if (vent.myRend == null || vent.myRend.material == null) continue;
                    if (ventsWithPlayers.Contains(vent.Id) || ventsWithPlayers.Count > 0 && highlightAllVents)
                    {
                        vent.myRend.material.SetFloat("_Outline", 1f);
                        vent.myRend.material.SetColor("_OutlineColor", Color.yellow);
                    }
                    else
                        vent.myRend.material.SetFloat("_Outline", 0);
                }
            }
        }

        public override void OnMeetingEnd(PlayerControl exiled = null)
        {
            if (PlayerControl.LocalPlayer == player && PlayerControl.LocalPlayer.Data.IsDead && exiled != null)
                acTokenChallenge.Value.cleared |= acTokenChallenge.Value.killerId == exiled.PlayerId;
        }

        public static Color color = new Color32(0, 247, 255, byte.MaxValue);

        public static bool highlightAllVents { get { return CustomOptionHolder.baitHighlightAllVents.getBool(); } }
        public float reportDelay = 0f;
        public static bool showKillFlash = true;
        public static bool canBeGuessed = true;
        public static float cooldown = 30f;
        public int numUses = 5;

        public bool reported = true;

        public AchievementToken<(byte killerId, bool cleared)> acTokenChallenge = null;
        public AchievementToken<int> acTokenChallenge2 = null;

        public override void PostInit()
        {
            if (PlayerControl.LocalPlayer != player) return;
            acTokenChallenge ??= new("bait.challenge", (byte.MaxValue, false), (val, _) => val.cleared);
            acTokenChallenge2 ??= new("bait.challenge2", 0, (val, _) => val >= 3);
        }

        public override void OnDeath(PlayerControl killer = null)
        {
            if (killer != null)
            {
                reported = false;
                if (showKillFlash && killer != player && killer == PlayerControl.LocalPlayer)
                {
                    Helpers.showFlash(new Color(204f / 255f, 102f / 255f, 0f / 255f));
                    var text = Helpers.CreateAndShowNotification(ModTranslation.getString("baitReportNotification"), Color.white, new Vector3(0f, 1f, -20f), spr: Illustrations.GetRoleIcon(RoleId.Bait).GetSprite());
                    text.AdjustNotification();
                }
            }
        }

        public static void clearAndReload()
        {
            showKillFlash = CustomOptionHolder.baitShowKillFlash.getBool();
            canBeGuessed = CustomOptionHolder.baitCanBeGuessed.getBool();
            players = [];
        }
    }
}
