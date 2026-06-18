using System;
using System.Collections.Generic;
using System.Linq;
using TheOtherRoles.MetaContext;
using TheOtherRoles.Modules;
using TheOtherRoles.Objects;
using TheOtherRoles.Patches;
using TheOtherRoles.Utilities;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    public class BountyHunter : RoleBase<BountyHunter> {
        public static Color color = Palette.ImpostorRed;

        public BountyHunter()
        {
            RoleId = roleId = RoleId.BountyHunter;
            bounty = null;
            arrowUpdateTimer = 0f;
            acTokenChallenge = null;
            bountyUpdateTimer = 0f;
        }

        public override void OnMeetingEnd(PlayerControl exiled = null)
        {
            if (player == PlayerControl.LocalPlayer)
                bountyUpdateTimer = 0f;
        }

        static public IEnumerable<DocumentReplacement> GetReplacementPart()
        {
            yield return new("%SEC%", arrowUpdateIntervall.ToString());
            yield return new("%OPT%", showArrow ? ModTranslation.getString("bountyHunterOPTHint") : "");
        }

        public override void FixedUpdate()
        {
            if (PlayerControl.LocalPlayer != player) return;

            if (player.Data.IsDead)
            {
                if (arrow != null) UnityEngine.Object.Destroy(arrow.arrow);
                arrow = null;
                if (cooldownText != null && cooldownText.gameObject != null) UnityEngine.Object.Destroy(cooldownText.gameObject);
                cooldownText = null;
                bounty = null;
                TORMapOptions.resetPoolables();
                return;
            }

            arrowUpdateTimer -= Time.fixedDeltaTime;
            bountyUpdateTimer -= Time.fixedDeltaTime;

            if (bounty == null || bountyUpdateTimer <= 0f)
            {
                // Set new bounty
                bounty = null;
                arrowUpdateTimer = 0f; // Force arrow to update
                bountyUpdateTimer = bountyDuration;
                var possibleTargets = new List<PlayerControl>();
                foreach (PlayerControl p in PlayerControl.AllPlayerControls) {
                    if (!p.Data.IsDead && !p.Data.Disconnected && p != p.Data.Role.IsImpostor && !p.isRole(RoleId.Spy) && !Sidekick.players.Any(x => x.player == p && x.wasTeamRed) && !Jackal.players.Any(x => x.player == p && x.wasTeamRed) && (p != Mini.mini || Mini.isGrownUp()) &&
                        !Akujo.players.Any(x => x.player == p && (x.keeps.Contains(player) || x.honmei == player))) possibleTargets.Add(p);
                }
                if (possibleTargets.Count == 0) return;
                bounty = possibleTargets[rnd.Next(0, possibleTargets.Count)];
                if (bounty == null) return;

                // Show poolable player
                if (FastDestroyableSingleton<HudManager>.Instance != null && FastDestroyableSingleton<HudManager>.Instance.UseButton != null)
                {
                    TORMapOptions.resetPoolables();
                    if (TORMapOptions.playerIcons.ContainsKey(bounty.PlayerId) && TORMapOptions.playerIcons[bounty.PlayerId].gameObject != null)
                        TORMapOptions.playerIcons[bounty.PlayerId].gameObject.SetActive(true);
                }
            }

            // Hide in meeting
            if (MeetingHud.Instance && TORMapOptions.playerIcons.ContainsKey(bounty.PlayerId) && TORMapOptions.playerIcons[bounty.PlayerId].gameObject != null)
                TORMapOptions.playerIcons[bounty.PlayerId].gameObject.SetActive(false);

            // Update Cooldown Text
            if (cooldownText != null)
            {
                cooldownText.text = Mathf.CeilToInt(Mathf.Clamp(bountyUpdateTimer, 0, bountyDuration)).ToString();
                cooldownText.gameObject.SetActive(!MeetingHud.Instance);  // Show if not in meeting
                cooldownText.transform.localPosition = IntroCutsceneOnDestroyPatch.bottomLeft + new Vector3(0f, -0.35f, -62f);
            }
            else {
                generateCooldownText();
            }

            // Update Arrow
            if (showArrow && bounty != null)
            {
                arrow ??= new Arrow(Color.red);
                if (arrowUpdateTimer <= 0f)
                {
                    arrow.Update(bounty.transform.position);
                    arrowUpdateTimer = arrowUpdateIntervall;
                }
                arrow.Update();
            }
        }

        public override void OnKill(PlayerControl target)
        {
            if (PlayerControl.LocalPlayer == player)
            {
                if (acTokenChallenge.Value.kills == 0)
                    acTokenChallenge.Value.history = DateTime.UtcNow;
                acTokenChallenge.Value.kills++;
                if (acTokenChallenge.Value.kills >= 3)
                {
                    acTokenChallenge.Value.cleared |= DateTime.UtcNow.Subtract(acTokenChallenge.Value.history).TotalSeconds <= 30;
                    acTokenChallenge.Value.kills = 0;
                }
                if (target == bounty)
                {
                    _ = new StaticAchievementToken("bountyHunter.common1");
                    player.SetKillTimer(bountyKillCooldown);
                    bountyUpdateTimer = 0f; // Force bounty update
                }
                else
                {
                    _ = new StaticAchievementToken("bountyHunter.another1");
                    player.SetKillTimer(GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown + punishmentTime);
                }
            }
        }

        public Arrow arrow;
        public static float bountyDuration = 30f;
        public static bool showArrow { get { return CustomOptionHolder.bountyHunterShowArrow.getBool(); } }
        public static float bountyKillCooldown = 0f;
        public static float punishmentTime = 15f;
        public static float arrowUpdateIntervall { get { return CustomOptionHolder.bountyHunterArrowUpdateIntervall.getFloat(); } }

        public float arrowUpdateTimer = 0f;
        public float bountyUpdateTimer = 0f;
        public PlayerControl bounty;
        public TMPro.TextMeshPro cooldownText;

        public AchievementToken<(DateTime history, int kills, bool cleared)> acTokenChallenge;

        public override void PostInit()
        {
            if (PlayerControl.LocalPlayer != player) return;
            acTokenChallenge ??= new("bountyHunter.challenge", (DateTime.UtcNow, 0, false), (val, _) => val.cleared);
        }

        public override void ResetRole(bool isShifted)
        {
            if (arrow?.arrow != null) UnityEngine.Object.Destroy(arrow.arrow);
            arrow = null;
            TORMapOptions.resetPoolables();
            if (cooldownText != null && cooldownText.gameObject != null) {
                UnityEngine.Object.Destroy(cooldownText.gameObject);
                cooldownText = null;
            }
        }

        public void generateCooldownText()
        {
            if (FastDestroyableSingleton<HudManager>.Instance != null)
            {
                cooldownText = UnityEngine.Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText, FastDestroyableSingleton<HudManager>.Instance.transform);
                cooldownText.alignment = TMPro.TextAlignmentOptions.Center;
                cooldownText.transform.localScale = Vector3.one * 0.4f;
                cooldownText.gameObject.SetActive(PlayerControl.LocalPlayer == player);
            }
        }

        public static void clearAndReload() {
            bountyDuration = CustomOptionHolder.bountyHunterBountyDuration.getFloat();
            bountyKillCooldown = CustomOptionHolder.bountyHunterReducedCooldown.getFloat();
            punishmentTime = CustomOptionHolder.bountyHunterPunishmentTime.getFloat();
            players = [];
        }
    }
}
