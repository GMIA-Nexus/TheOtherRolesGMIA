using System;
using System.Collections.Generic;
using TheOtherRoles.Modules;
using TheOtherRoles.Objects;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    public class Snitch : RoleBase<Snitch> {
        public static Color color = new Color32(184, 251, 79, byte.MaxValue);

        public List<Arrow> localArrows = [];
        public static int taskCountForReveal { get { return Mathf.RoundToInt(CustomOptionHolder.snitchLeftTasksForReveal.getFloat()); } }
        public static bool includeTeamEvil = false;
        public static bool teamEvilUseDifferentArrowColor = true;

        public Snitch()
        {
            RoleId = roleId = RoleId.Snitch;
            localArrows = [];
        }

        public override void ResetRole(bool isShifted)
        {
            if (localArrows != null)
                foreach (Arrow arrow in localArrows)
                    if (arrow?.arrow != null)
                        UnityEngine.Object.Destroy(arrow.arrow);
            localArrows = [];
        }

        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason)
        {
            if (player == this.player)
            {
                if (localArrows != null)
                    foreach (Arrow arrow in localArrows)
                        if (arrow?.arrow != null)
                            UnityEngine.Object.Destroy(arrow.arrow);
                localArrows = [];
            }
        }

        public override void OnMeetingStart()
        {
            if (PlayerControl.LocalPlayer == player)
            {
                var (taskComplete, taskTotal) = TasksHandler.taskInfo(player.Data);
                if (!PlayerControl.LocalPlayer.Data.IsDead && taskTotal > 0 && taskComplete == taskTotal) _ = new StaticAchievementToken("snitch.challenge");
            }
        }

        static public IEnumerable<DocumentReplacement> GetReplacementPart() => [new("%NUM%", taskCountForReveal.ToString())];

        public override void FixedUpdate()
        {
            if (localArrows == null) return;
            foreach (Arrow arrow in localArrows) arrow.arrow.SetActive(false);

            if (!hasAlivePlayers) return;
            var local = PlayerControl.LocalPlayer;

            var (playerCompleted, playerTotal) = TasksHandler.taskInfo(player.Data);
            int numberOfTasks = playerTotal - playerCompleted;

            if (!local.Data.IsDead && numberOfTasks <= taskCountForReveal && (local.Data.Role.IsImpostor || (includeTeamEvil && (local.isRole(RoleId.Jackal) || local.isRole(RoleId.Sidekick)
                || local.isRole(RoleId.Moriarty) || local.isRole(RoleId.JekyllAndHyde) || local.isRole(RoleId.Fox)) || local.isRole(RoleId.Immoralist) || local.isRole(RoleId.Pelican)
                || local.isRole(RoleId.Yandere) || (local.isRole(RoleId.SchrodingersCat) && SchrodingersCat.hasTeam() && SchrodingersCat.team != SchrodingersCat.Team.Crewmate))))
            {
                if (localArrows.Count == 0) localArrows.Add(new Arrow(Color.blue));
                if (localArrows.Count != 0 && localArrows[0] != null)
                {
                    localArrows[0].arrow.SetActive(true);
                    localArrows[0].image.color = Color.blue;
                    localArrows[0].Update(player.transform.position);
                }
            }
            else if (local == player && numberOfTasks == 0)
            {
                int arrowIndex = 0;
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    bool arrowForImp = p.Data.Role.IsImpostor;
                    bool arrowForTeamJackal = includeTeamEvil && (p.isRole(RoleId.Jackal) || p.isRole(RoleId.Sidekick) || (p.isRole(RoleId.SchrodingersCat) && SchrodingersCat.team == SchrodingersCat.Team.Jackal));
                    bool arrowForFox = includeTeamEvil && (p.isRole(RoleId.Fox) || p.isRole(RoleId.Immoralist));
                    bool arrowForPelican = includeTeamEvil && p.isRole(RoleId.Pelican);
                    bool arrowForYandere = includeTeamEvil && (p.isRole(RoleId.Yandere) || (p.isRole(RoleId.SchrodingersCat) && SchrodingersCat.team == SchrodingersCat.Team.Yandere));
                    bool arrowForJekyll = includeTeamEvil && (p.isRole(RoleId.JekyllAndHyde) || (p.isRole(RoleId.SchrodingersCat) && SchrodingersCat.team == SchrodingersCat.Team.JekyllAndHyde));
                    bool arrowForMoriarty = includeTeamEvil && (p.isRole(RoleId.Moriarty) || (p.isRole(RoleId.SchrodingersCat) && SchrodingersCat.team == SchrodingersCat.Team.Moriarty));

                    Color color = Palette.ImpostorRed;
                    if (teamEvilUseDifferentArrowColor) {
                        if (arrowForTeamJackal) color = Jackal.color;
                        else if (arrowForFox) color = Fox.color;
                        else if (arrowForJekyll) color = JekyllAndHyde.color;
                        else if (arrowForMoriarty) color = Moriarty.color;
                        else if (arrowForPelican) color = Pelican.color;
                        else if (arrowForYandere) color = Yandere.color;
                    }

                    if (!p.Data.IsDead && (arrowForImp || arrowForTeamJackal || arrowForFox || arrowForJekyll || arrowForMoriarty || arrowForPelican || arrowForYandere))
                    {
                        if (arrowIndex >= localArrows.Count)
                            localArrows.Add(new Arrow(color));
                        if (arrowIndex < localArrows.Count && localArrows[arrowIndex] != null)
                        {
                            localArrows[arrowIndex].arrow.SetActive(true);
                            localArrows[arrowIndex].Update(p.transform.position, color);
                        }
                        arrowIndex++;
                    }
                }
            }
        }

        public static void clearAndReload() {
            includeTeamEvil = CustomOptionHolder.snitchIncludeTeamEvil.getBool();
            teamEvilUseDifferentArrowColor = CustomOptionHolder.snitchTeamEvilUseDifferentArrowColor.getBool();
            players = [];
        }
    }
}
