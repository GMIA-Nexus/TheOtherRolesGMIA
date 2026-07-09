using System;
using System.Collections.Generic;
using System.Linq;
using TheOtherRoles;
using TheOtherRoles.MetaContext;
using TheOtherRoles.Modules;
using TheOtherRoles.Objects;
using TheOtherRoles.Utilities;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    [TORRPCHolder]
    public class Medium : RoleBase<Medium> {
        public DeadPlayer target;
        public DeadPlayer soulTarget;
        public static Color color = new Color32(98, 120, 115, byte.MaxValue);
        public List<Tuple<DeadPlayer, Vector3>> deadBodies = [];
        public static List<Tuple<DeadPlayer, Vector3>> futureDeadBodies = [];
        public List<SpriteRenderer> souls = [];
        public static DateTime meetingStartTime = DateTime.UtcNow;
        public List<PlayerControl> questioned = [];
        public List<Arrow> arrows = [];

        static public IEnumerable<DocumentReplacement> GetReplacementPart()
        {
            yield return new("%OPT%", oneTimeUse ? ModTranslation.getString("mediumOPTHint") : "");
        }

        public Medium()
        {
            RoleId = roleId = RoleId.Medium;
            target = null;
            soulTarget = null;
            acTokenCommon = null;
            acTokenChallenge = null;
            deadBodies = [];
            souls = [];
            questioned = [];
            arrows = [];
        }

        public override void FixedUpdate()
        {
            setDeadTarget();
            arrowUpdate();
        }

        void setDeadTarget()
        {
            if (player != PlayerControl.LocalPlayer || deadBodies == null || MapUtilities.CachedShipStatus?.AllVents == null) return;

            if (!player.Data.IsDead)
            {
                DeadPlayer target = null;
                Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
                float closestDistance = float.MaxValue;
                float usableDistance = MapUtilities.CachedShipStatus.AllVents.FirstOrDefault().UsableDistance;
                foreach ((DeadPlayer dp, Vector3 ps) in deadBodies)
                {
                    float distance = Vector2.Distance(ps, truePosition);
                    if (distance <= usableDistance && distance < closestDistance)
                    {
                        closestDistance = distance;
                        target = dp;
                    }
                }
                this.target = target;
            }
        }

        public static RemoteProcess<(byte playerId, byte mediumId)> Question = new("MediumQuestion", (message, _) =>
        {
            var player = Helpers.playerById(message.playerId);
            var m = Helpers.playerById(message.mediumId);
            var medium = getRole(m);
            if (player == null || medium == null) return;
            medium.questioned.TryAdd(player);

            if (PlayerControl.LocalPlayer == player) {
                Helpers.showFlash(Palette.CrewmateBlue, message: ModTranslation.getString("mediumQuestionText"), textColor: Color.white, textSprite: RoleHelpers.GetRoleIcon(RoleId.Medium).GetSprite());
            }
        });

        public static float cooldown = 30f;
        public static float duration = 3f;
        public static bool oneTimeUse { get { return CustomOptionHolder.mediumOneTimeUse.getBool(); } }
        public static float chanceAdditionalInfo = 0f;
        public static bool showQuestionTarget = true;

        public AchievementToken<int> acTokenCommon = null;
        public AchievementToken<(List<byte> additionals, bool cleared)> acTokenChallenge = null;

        public override void PostInit()
        {
            if (PlayerControl.LocalPlayer != player) return;
            acTokenCommon ??= new("medium.common1", 0, (val, _) => val >= 3);
            acTokenChallenge ??= new("medium.challenge", ([], false), (val, _) => val.cleared);
        }

        private static Sprite soulSprite;

        enum SpecialMediumInfo {
            SheriffSuicide,
            ThiefSuicide,
            ActiveLoverDies,
            PassiveLoverSuicide,
            LawyerKilledByClient,
            JackalKillsSidekick,
            ImpostorTeamkill,
            BuskerPseudocide,
            SubmergedO2,
            WarlockSuicide,
            BodyCleaned,
        }

        public static Sprite getSoulSprite() {
            if (soulSprite) return soulSprite;
            soulSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Soul.png", 500f);
            return soulSprite;
        }

        private static Sprite question;
        public static Sprite getQuestionSprite() {
            if (question) return question;
            question = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.MediumButton.png", 115f);
            return question;
        }

        void arrowUpdate()
        {
            foreach (Arrow arrow in arrows) arrow.arrow.SetActive(false);
            if (questioned.Count == 0 || PlayerControl.LocalPlayer.Data.IsDead) return;
            if (PlayerControl.LocalPlayer == player)
            {
                int arrowIndex = 0;
                foreach (var p in questioned)
                {
                    if (p == null) continue;
                    if (arrowIndex >= arrows.Count)
                        arrows.Add(new Arrow(color));
                    if (arrowIndex < arrows.Count && arrows[arrowIndex] != null)
                    {
                        arrows[arrowIndex].arrow.SetActive(true);
                        arrows[arrowIndex].Update(p.transform.position, color);
                    }
                    arrowIndex++;
                }
            }
            else if (questioned.Contains(PlayerControl.LocalPlayer))
            {
                if (arrows.Count == 0) arrows.Add(new Arrow(Color.white));
                if (arrows.Count != 0 && arrows[0] != null)
                {
                    arrows[0].arrow.SetActive(true);
                    arrows[0].image.color = Color.white;
                    arrows[0].Update(player.transform.position);
                }
            }
        }

        public override void ResetRole(bool isShifted)
        {
            if (souls != null) {
                foreach (SpriteRenderer sr in souls) UnityEngine.Object.Destroy(sr.gameObject);
                souls = [];
            }
            if (arrows != null)
                foreach (Arrow arrow in arrows)
                    if (arrow?.arrow != null)
                        UnityEngine.Object.Destroy(arrow.arrow);
            arrows = [];
        }

        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason)
        {
            if (this.player == player)
            {
                if (arrows != null)
                    foreach (Arrow arrow in arrows)
                        if (arrow?.arrow != null)
                            UnityEngine.Object.Destroy(arrow.arrow);
                arrows = [];
            }
        }

        public override void OnMeetingEnd(PlayerControl exiled = null)
        {
            soulTarget = null;
            questioned = [];
            if (PlayerControl.LocalPlayer == player) {
                acTokenChallenge.Value.cleared |= exiled != null && acTokenChallenge.Value.additionals.Any(x => x == exiled.PlayerId);
                acTokenChallenge.Value.additionals = [];

                if (souls != null) {
                    foreach (SpriteRenderer sr in souls) UnityEngine.Object.Destroy(sr.gameObject);
                    souls = [];
                }

                if (futureDeadBodies != null) {
                    foreach ((DeadPlayer db, Vector3 ps) in futureDeadBodies) {
                        GameObject s = new();
                        //s.transform.position = ps;
                        s.transform.position = new Vector3(ps.x, ps.y, ps.y / 1000 - 1f);
                        s.layer = 5;
                        var rend = s.AddComponent<SpriteRenderer>();
                        s.AddSubmergedComponent(SubmergedCompatibility.Classes.ElevatorMover);
                        rend.sprite = getSoulSprite();
                        souls.Add(rend);
                    }
                    deadBodies = futureDeadBodies;
                    futureDeadBodies = [];
                }
            }
        }

        public static void clearAndReload() {
            futureDeadBodies = [];
            meetingStartTime = DateTime.UtcNow;
            cooldown = CustomOptionHolder.mediumCooldown.getFloat();
            duration = CustomOptionHolder.mediumDuration.getFloat();
            chanceAdditionalInfo = CustomOptionHolder.mediumChanceAdditionalInfo.getSelection() / 10f;
            showQuestionTarget = CustomOptionHolder.mediumRevealTarget.getBool();
            players = [];
        }

        public string getInfo(PlayerControl target, PlayerControl killer, DeadPlayer.CustomDeathReason deathReason)
        {
            string msg = "";

            List<SpecialMediumInfo> infos = [];
            // collect fitting death info types.
            // suicides:
            if (killer == target) {
                if (target.isRole(RoleId.Sheriff) && deathReason != DeadPlayer.CustomDeathReason.LoverSuicide) infos.Add(SpecialMediumInfo.SheriffSuicide);
                if (target.isLovers()) infos.Add(SpecialMediumInfo.PassiveLoverSuicide);
                if (target.isRole(RoleId.Thief) && deathReason != DeadPlayer.CustomDeathReason.LoverSuicide) infos.Add(SpecialMediumInfo.ThiefSuicide);
                if (target.isRole(RoleId.Warlock) && deathReason != DeadPlayer.CustomDeathReason.LoverSuicide) infos.Add(SpecialMediumInfo.WarlockSuicide);
                if (target.isRole(RoleId.Busker) && deathReason != DeadPlayer.CustomDeathReason.LoverSuicide) infos.Add(SpecialMediumInfo.BuskerPseudocide);
            } else {
                if (target.isLovers()) infos.Add(SpecialMediumInfo.ActiveLoverDies);
                if (target.Data.Role.IsImpostor && killer.Data.Role.IsImpostor && !Thief.formerThief.Contains(killer)) infos.Add(SpecialMediumInfo.ImpostorTeamkill);
            }
            if (target.isRole(RoleId.Sidekick) && killer.isRole(RoleId.Jackal)) infos.Add(SpecialMediumInfo.JackalKillsSidekick);
            if (target.isRole(RoleId.Lawyer) && killer == Lawyer.target) infos.Add(SpecialMediumInfo.LawyerKilledByClient);
            if (this.target.wasCleaned) infos.Add(SpecialMediumInfo.BodyCleaned);
            
            if (infos.Count > 0) {
                var selectedInfo = infos[rnd.Next(infos.Count)];
                _ = new StaticAchievementToken("medium.another1.specialInfo");
                switch (selectedInfo) {
                    case SpecialMediumInfo.SheriffSuicide:
                        msg = ModTranslation.getString("mediumSheriffSuicide");
                        break;
                    case SpecialMediumInfo.WarlockSuicide:
                        msg = ModTranslation.getString("mediumWarlockSuicide");
                        break;
                    case SpecialMediumInfo.ThiefSuicide:
                        msg = ModTranslation.getString("mediumThiefSuicide");
                        break;
                    case SpecialMediumInfo.ActiveLoverDies:
                        msg = ModTranslation.getString("mediumActiveLoverDies");
                        break;
                    case SpecialMediumInfo.PassiveLoverSuicide:
                        msg = ModTranslation.getString("mediumPassiveLoverSuicide");
                        break;
                    case SpecialMediumInfo.LawyerKilledByClient:
                        msg = ModTranslation.getString("mediumLawyerKilledByClient");
                        break;
                    case SpecialMediumInfo.BuskerPseudocide:
                        msg = ModTranslation.getString("mediumBuskerPseudocide");
                        break;
                    case SpecialMediumInfo.JackalKillsSidekick:
                        msg = ModTranslation.getString("mediumJackalKillsSidekick");
                        break;
                    case SpecialMediumInfo.ImpostorTeamkill:
                        msg = ModTranslation.getString("mediumImpostorTeamkill");
                        break;
                    case SpecialMediumInfo.BodyCleaned:
                        msg = ModTranslation.getString("mediumBodyCleaned");
                        break;
                }
            } else {
                int randomNumber = rnd.Next(4);
                string typeOfColor = Helpers.isLighterColor(this.target.killerIfExisting.Data.DefaultOutfit.ColorId) ? ModTranslation.getString("mediumSoulPlayerLighter") : ModTranslation.getString("mediumSoulPlayerDarker");
                float timeSinceDeath = (float)(meetingStartTime - this.target.timeOfDeath).TotalMilliseconds;
                var roleString = RoleInfo.GetRolesString(this.target.player, false, includeHidden: true);
                var roleInfo = RoleInfo.getRoleInfoForPlayer(this.target.player);

                if (randomNumber == 0)
                {
                    if (!roleInfo.Contains(RoleInfo.impostor) && !roleInfo.Contains(RoleInfo.crewmate)) {
                        msg = string.Format(ModTranslation.getString("mediumQuestion1"), RoleInfo.GetRolesString(this.target.player, false, includeHidden: true));
                    } else {
                        msg = string.Format(ModTranslation.getString("mediumQuestion5"), roleString);
                    }

                    _ = new StaticAchievementToken("medium.another1.myRole");
                }
                else if (randomNumber == 1)
                {
                    msg = string.Format(ModTranslation.getString("mediumQuestion2"), typeOfColor);
                    _ = new StaticAchievementToken("medium.another1.killersColor");
                }
                else if (randomNumber == 2)
                {
                    msg = string.Format(ModTranslation.getString("mediumQuestion3"), Math.Round(timeSinceDeath / 1000));
                    _ = new StaticAchievementToken("medium.another1.deathStamp");
                }
                else
                {
                    msg = string.Format(ModTranslation.getString("mediumQuestion4"), RoleInfo.GetRolesString(this.target.killerIfExisting, false, false, true, includeHidden: true));
                    _ = new StaticAchievementToken("medium.another1.killersRole");
                }
            }

            if (rnd.NextDouble() < chanceAdditionalInfo) {
                int count = 0;
                string condition = "";
                var alivePlayersList = PlayerControl.AllPlayerControls.ToArray().Where(pc => !pc.Data.IsDead);
                switch (rnd.Next(3)) {
                    case 0:
                        count = alivePlayersList.Where(pc => pc.Data.Role.IsImpostor || new List<RoleInfo>() { RoleInfo.jackal, RoleInfo.sidekick, RoleInfo.sheriff, RoleInfo.thief, RoleInfo.jekyllAndHyde, RoleInfo.moriarty,
                        SchrodingersCat.hasTeam() && SchrodingersCat.team != SchrodingersCat.Team.Crewmate ? RoleInfo.schrodingersCat : null}.Contains(RoleInfo.getRoleInfoForPlayer(pc, false).FirstOrDefault())).Count();
                        condition = ModTranslation.getString($"mediumKiller{(count == 1 ? "" : "Plural")}");
                        break;
                    case 1:
                        count = alivePlayersList.Where(Helpers.roleCanUseVents).Count();
                        condition = ModTranslation.getString($"mediumPlayerUseVents{(count == 1 ? "" : "Plural")}");
                        break;
                    case 2:
                        count = alivePlayersList.Where(pc => Helpers.isNeutral(pc) && !pc.isRole(RoleId.Jackal) && !pc.isRole(RoleId.Sidekick) && !pc.isRole(RoleId.Thief) && !pc.isRole(RoleId.JekyllAndHyde) && !pc.isRole(RoleId.Moriarty)
                        && !pc.isRole(RoleId.Yandere) && !pc.isRole(RoleId.Pelican) && !(pc.isRole(RoleId.SchrodingersCat) && SchrodingersCat.hasTeam() && SchrodingersCat.team != SchrodingersCat.Team.Crewmate)).Count();
                        condition = ModTranslation.getString($"mediumPlayerNeutral{(count == 1 ? "" : "Plural")}");
                        break;
                    case 3:
                        //count = alivePlayersList.Where(pc =>
                        break;
                }
                msg += $"\n" + string.Format(ModTranslation.getString("mediumAskPrefix"), string.Format(ModTranslation.getString($"mediumStillAlive{(count == 1 ? "" : "Plural")}"), string.Format(condition, count)));

                acTokenChallenge.Value.additionals.Add(this.target.killerIfExisting.PlayerId);
            }

            return string.Format(ModTranslation.getString("mediumSoulPlayerPrefix"), this.target.player.Data.PlayerName) + msg;
        }
    }
}
