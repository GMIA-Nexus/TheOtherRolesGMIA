using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using TheOtherRoles.CustomGameModes;
using TheOtherRoles.Modules;
using TheOtherRoles.Objects;
using TheOtherRoles.Roles;
using TheOtherRoles.Utilities;
using UnityEngine;

namespace TheOtherRoles.Patches {
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    class HudManagerUpdatePatch
    {
        private static Dictionary<byte, (string name, Color color)> TagColorDict = new();
        static void resetNameTagsAndColors() {
            var localPlayer = PlayerControl.LocalPlayer;
            var myData = PlayerControl.LocalPlayer.Data;
            var amImpostor = myData.Role.IsImpostor;

            var dict = TagColorDict;
            dict.Clear();
            
            foreach (var data in GameData.Instance.AllPlayers.GetFastEnumerator())
            {
                var player = data.Object;
                string text = data.PlayerName;
                Color color;
                if (player)
                {
                    var playerName = text;
                    foreach (var morphling in Morphling.players) {
                        if (morphling.morphTimer > 0f && morphling.morphTarget != null && morphling.player == player)
                            playerName = morphling.morphTarget.Data.PlayerName;
                    }
                    if (MimicA.isMorph && player.isRole(RoleId.MimicA)) playerName = MimicK.allPlayers.FirstOrDefault().Data.PlayerName;
                    if (MimicK.victim != null && player.isRole(RoleId.MimicK)) playerName = MimicK.victim.Data.PlayerName;
                    var nameText = player.cosmetics.nameText;
                
                    nameText.text = Helpers.hidePlayerName(localPlayer, player) ? "" : playerName;
                    nameText.color = color = amImpostor && data.Role.IsImpostor ? Palette.ImpostorRed : Color.white;
                    nameText.color = nameText.color.SetAlpha(Chameleon.visibility(player.PlayerId));
                }
                else
                {
                    color = Color.white;
                }
                
                
                dict.Add(data.PlayerId, (text, color));
            }
            
            if (MeetingHud.Instance != null) 
            {
                foreach (PlayerVoteArea playerVoteArea in MeetingHud.Instance.playerStates)
                {
                    var data = dict[playerVoteArea.TargetPlayerId];
                    var text = playerVoteArea.NameText;
                    text.text = data.name;
                    text.color = data.color;
                }
            }
        }

        static void setPlayerNameColor(PlayerControl p, Color color) {
            p.cosmetics.nameText.color = color.SetAlpha(Chameleon.visibility(p.PlayerId));
            if (MeetingHud.Instance != null)
                foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                    if (player.NameText != null && p.PlayerId == player.TargetPlayerId)
                        player.NameText.color = color;
        }

        static void setNameColors() {
            var localPlayer = PlayerControl.LocalPlayer;
            var localRole = RoleInfo.getRoleInfoForPlayer(localPlayer, false).FirstOrDefault();
            setPlayerNameColor(localPlayer, localRole.color);

            /*if (Jester.jester != null && Jester.jester == localPlayer)
                setPlayerNameColor(Jester.jester, Jester.color);
            else if (Mayor.mayor != null && Mayor.mayor == localPlayer)
                setPlayerNameColor(Mayor.mayor, Mayor.color);
            else if (Engineer.engineer != null && Engineer.engineer == localPlayer)
                setPlayerNameColor(Engineer.engineer, Engineer.color);
            else if (Sheriff.sheriff != null && Sheriff.sheriff == localPlayer) {
                setPlayerNameColor(Sheriff.sheriff, Sheriff.color);
                if (Deputy.deputy != null && Deputy.knowsSheriff) {
                    setPlayerNameColor(Deputy.deputy, Deputy.color);
                }
            } else*/
            if (localPlayer.isRole(RoleId.Deputy)) 
            {
                setPlayerNameColor(localPlayer, Deputy.color);
                var sheriff = Deputy.getRole(localPlayer).sheriff;
                if (sheriff != null && sheriff.player != null && Deputy.knowsSheriff) {
                    setPlayerNameColor(sheriff.player, Sheriff.color);
                }
            }
            else if (localPlayer.isRole(RoleId.Sheriff))
            {
                setPlayerNameColor(localPlayer, Sheriff.color);
                var deputy = Sheriff.getDeputy(localPlayer);
                if (deputy != null && deputy.player != null && Deputy.knowsSheriff) setPlayerNameColor(deputy.player, Sheriff.color);
            }
            /*else if (Portalmaker.portalmaker != null && Portalmaker.portalmaker == localPlayer)
                setPlayerNameColor(Portalmaker.portalmaker, Portalmaker.color);
            else if (Lighter.lighter != null && Lighter.lighter == localPlayer)
                setPlayerNameColor(Lighter.lighter, Lighter.color);
            else if (Detective.detective != null && Detective.detective == localPlayer)
                setPlayerNameColor(Detective.detective, Detective.color);
            else if (TimeMaster.timeMaster != null && TimeMaster.timeMaster == localPlayer)
                setPlayerNameColor(TimeMaster.timeMaster, TimeMaster.color);
            else if (Medic.medic != null && Medic.medic == localPlayer)
                setPlayerNameColor(Medic.medic, Medic.color);
            else if (Shifter.shifter != null && Shifter.shifter == localPlayer)
                setPlayerNameColor(Shifter.shifter, Shifter.color);
            else if (Swapper.swapper != null && Swapper.swapper == localPlayer)
                setPlayerNameColor(Swapper.swapper, Swapper.color);
            else if (Seer.seer != null && Seer.seer == localPlayer)
                setPlayerNameColor(Seer.seer, Seer.color);
            else if (Hacker.hacker != null && Hacker.hacker == localPlayer)
                setPlayerNameColor(Hacker.hacker, Hacker.color);
            else if (Tracker.tracker != null && Tracker.tracker == localPlayer)
                setPlayerNameColor(Tracker.tracker, Tracker.color);
            else if (Snitch.snitch != null && Snitch.snitch == localPlayer)
                setPlayerNameColor(Snitch.snitch, Snitch.color);*/
            else if (localPlayer.isRole(RoleId.Jackal)) {
                // Jackal can see his sidekick
                setPlayerNameColor(localPlayer, Jackal.color);
                var jackal = Jackal.getRole(localPlayer);
                var sidekick = Jackal.getSidekick(localPlayer);
                if (sidekick != null && sidekick.player != null) {
                    setPlayerNameColor(sidekick.player, Jackal.color);
                }
                if (jackal.fakeSidekick != null) {
                    setPlayerNameColor(jackal.fakeSidekick, Jackal.color);
                }
                if (SchrodingersCat.exists && SchrodingersCat.team == SchrodingersCat.Team.Jackal)
                    foreach (var p in SchrodingersCat.allPlayers)
                        setPlayerNameColor(p, Jackal.color);
            }
            else if (localPlayer.isRole(RoleId.FortuneTeller) && (FortuneTeller.isCompletedNumTasks(PlayerControl.LocalPlayer) || PlayerControl.LocalPlayer.Data.IsDead))
            {
                setPlayerNameColor(PlayerControl.LocalPlayer, FortuneTeller.color);
            }
            else if (localPlayer.isRole(RoleId.TaskMaster))
            {
                setPlayerNameColor(PlayerControl.LocalPlayer, !TaskMaster.becomeATaskMasterWhenCompleteAllTasks || TaskMaster.isTaskComplete ? TaskMaster.color : RoleInfo.crewmate.color);
            }
            else if (localPlayer.isRole(RoleId.Swapper)) {
                setPlayerNameColor(localPlayer, localPlayer.Data.Role.IsImpostor ? Palette.ImpostorRed : Swapper.color);
            }
            else if (Yasuna.isYasuna(localPlayer.PlayerId))
            {
                setPlayerNameColor(localPlayer, localPlayer.Data.Role.IsImpostor ? Palette.ImpostorRed : Yasuna.color);
            }
            else if (localPlayer.isRole(RoleId.Kataomoi))
            {
                if (Kataomoi.target != null)
                    setPlayerNameColor(Kataomoi.target, Kataomoi.color);
            }
            else if (localPlayer.isRole(RoleId.Yandere))
            {
                if (Yandere.target != null)
                    setPlayerNameColor(Yandere.target, Yandere.color);
                if (SchrodingersCat.exists && SchrodingersCat.team == SchrodingersCat.Team.Yandere)
                    foreach (var p in SchrodingersCat.allPlayers)
                        setPlayerNameColor(p, Yandere.color);
            }
            /*else if (Spy.spy != null && Spy.spy == localPlayer) {
                setPlayerNameColor(Spy.spy, Spy.color);
            } else if (SecurityGuard.securityGuard != null && SecurityGuard.securityGuard == localPlayer) {
                setPlayerNameColor(SecurityGuard.securityGuard, SecurityGuard.color);
            } else if (Arsonist.arsonist != null && Arsonist.arsonist == localPlayer) {
                setPlayerNameColor(Arsonist.arsonist, Arsonist.color);
            } else if (Guesser.niceGuesser != null && Guesser.niceGuesser == localPlayer) {
                setPlayerNameColor(Guesser.niceGuesser, Guesser.color);
            } else if (Guesser.evilGuesser != null && Guesser.evilGuesser == localPlayer) {
                setPlayerNameColor(Guesser.evilGuesser, Palette.ImpostorRed);
            } else if (Vulture.vulture != null && Vulture.vulture == localPlayer) {
                setPlayerNameColor(Vulture.vulture, Vulture.color);
            } else if (Medium.medium != null && Medium.medium == localPlayer) {
                setPlayerNameColor(Medium.medium, Medium.color);
            } else if (Trapper.trapper != null && Trapper.trapper == localPlayer) {
                setPlayerNameColor(Trapper.trapper, Trapper.color);
            } else if (Lawyer.lawyer != null && Lawyer.lawyer == localPlayer) {
                setPlayerNameColor(Lawyer.lawyer, Lawyer.color);
            } else if (Pursuer.pursuer != null && Pursuer.pursuer == localPlayer) {
                setPlayerNameColor(Pursuer.pursuer, Pursuer.color);
            }*/

            // No else if here, as a Lover of team Jackal needs the colors
            if (localPlayer.isRole(RoleId.Sidekick))
            {
                // Sidekick can see the jackal
                setPlayerNameColor(localPlayer, Sidekick.color);
                var jackal = Sidekick.getRole(localPlayer).jackal;
                if (jackal != null && jackal.player != null)
                {
                    setPlayerNameColor(jackal.player, Jackal.color);
                }
                if (SchrodingersCat.exists && SchrodingersCat.team == SchrodingersCat.Team.Jackal)
                    foreach (var p in SchrodingersCat.allPlayers) setPlayerNameColor(p, Jackal.color);
            }

            if (localPlayer.isRole(RoleId.SchrodingersCat))
            {
                if (SchrodingersCat.team == SchrodingersCat.Team.Impostor) {
                    foreach (var p in PlayerControl.AllPlayerControls.GetFastEnumerator()) {
                        if (p.Data.Role.IsImpostor) setPlayerNameColor(p, Palette.ImpostorRed);
                    }
                }
                else if (SchrodingersCat.team == SchrodingersCat.Team.Jackal)
                {
                    foreach (var p in Jackal.allPlayers)
                        setPlayerNameColor(p, Jackal.color);
                    foreach (var p in Sidekick.allPlayers)
                        setPlayerNameColor(p, Sidekick.color);
                }
                else if (SchrodingersCat.team == SchrodingersCat.Team.JekyllAndHyde && JekyllAndHyde.exists) {
                    foreach (var jekyllAndHyde in JekyllAndHyde.allPlayers)
                        setPlayerNameColor(jekyllAndHyde, JekyllAndHyde.color);
                }
                else if (SchrodingersCat.team == SchrodingersCat.Team.Moriarty && Moriarty.exists) {
                    foreach (var moriarty in Moriarty.allPlayers)
                        setPlayerNameColor(moriarty, Moriarty.color);
                }
                else if (SchrodingersCat.team == SchrodingersCat.Team.Yandere && Yandere.exists) {
                    foreach (var yandere in Yandere.allPlayers)
                        setPlayerNameColor(yandere, Yandere.color);
                }
            }

            if (SchrodingersCat.exists)
            {
                if (localPlayer.isRole(RoleId.JekyllAndHyde) && SchrodingersCat.team == SchrodingersCat.Team.JekyllAndHyde)
                    foreach (var p in SchrodingersCat.allPlayers)
                        setPlayerNameColor(p, JekyllAndHyde.color);
                if (localPlayer.isRole(RoleId.Moriarty) && SchrodingersCat.team == SchrodingersCat.Team.Moriarty)
                    foreach (var p in SchrodingersCat.allPlayers)
                        setPlayerNameColor(p, Moriarty.color);
            }

            if (PlayerControl.LocalPlayer.isRole(RoleId.Yoyo))
            {
                foreach (var black in Yoyo.local.blackout) {
                    if (black != null && !black.Data.IsDead)
                        setPlayerNameColor(black, Color.black);
                }
            }

            // No else if here, as the Impostors need the Spy name to be colored
            if (localPlayer.Data.Role.IsImpostor) {
                foreach (var sidekick in Sidekick.players) {
                    if (sidekick.player != null && sidekick.wasTeamRed) {
                        setPlayerNameColor(sidekick.player, Spy.color);
                    }
                }
                foreach (var jackal in Jackal.players) {
                    if (jackal.player != null && jackal.wasTeamRed) {
                        setPlayerNameColor(jackal.player, Spy.color);
                    }
                }
                foreach (var spy in Spy.allPlayers) setPlayerNameColor(spy, Spy.color);
            }
            if (localPlayer.isRole(RoleId.Fox)) { 
                setPlayerNameColor(localPlayer, Fox.color);
                foreach (var immoralist in Immoralist.allPlayers)
                    setPlayerNameColor(immoralist, Immoralist.color);
            }
            if (localPlayer.isRole(RoleId.Immoralist)) {
                setPlayerNameColor(localPlayer, Immoralist.color);
                foreach (var fox in Fox.allPlayers)
                    setPlayerNameColor(fox, Immoralist.color);
            }
            if (Madmate.madmate.Contains(localPlayer))
            {
                setPlayerNameColor(PlayerControl.LocalPlayer, Madmate.color);
                if (Madmate.tasksComplete(localPlayer))
                {
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls.GetFastEnumerator())
                    {
                        if (p.isRole(RoleId.Spy) || p.Data.Role.IsImpostor || Jackal.players.Any(x => x.player == p && x.wasTeamRed) || Sidekick.players.Any(x => x.player == p && x.wasTeamRed))
                        {
                            setPlayerNameColor(p, Palette.ImpostorRed);
                        }
                    }
                }
            }
            if (CreatedMadmate.createdMadmate.Contains(localPlayer))
            {
                setPlayerNameColor(PlayerControl.LocalPlayer, Madmate.color);
                if (CreatedMadmate.tasksComplete(localPlayer))
                {
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls.GetFastEnumerator())
                    {
                        if (p.isRole(RoleId.Spy) || p.Data.Role.IsImpostor || Jackal.players.Any(x => x.player == p && x.wasTeamRed) || Sidekick.players.Any(x => x.player == p && x.wasTeamRed))
                        {
                            setPlayerNameColor(p, Palette.ImpostorRed);
                        }
                    }
                }
            }
            // Crewmate roles with no changes: Mini
            // Impostor roles with no changes: Morphling, Camouflager, Vampire, Godfather, Eraser, Janitor, Cleaner, Warlock, BountyHunter,  Witch and Mafioso
        }

        static void setNameTags() {
            // Mafia
            if (PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data.Role.IsImpostor) {
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    if (player.isRole(RoleId.Godfather))
                            player.cosmetics.nameText.text = player.Data.PlayerName + $" ({ModTranslation.getString("mafiaG")})";
                    else if (player.isRole(RoleId.Mafioso))
                            player.cosmetics.nameText.text = player.Data.PlayerName + $" ({ModTranslation.getString("mafiaM")})";
                    else if (player.isRole(RoleId.Janitor))
                            player.cosmetics.nameText.text = player.Data.PlayerName + $" ({ModTranslation.getString("mafiaJ")})";
                if (MeetingHud.Instance != null)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                        if (Godfather.players.Any(x => x.player.PlayerId == player.TargetPlayerId))
                            player.NameText.text = Godfather.allPlayers.FirstOrDefault(x => x.PlayerId == player.TargetPlayerId).Data.PlayerName + $" ({ModTranslation.getString("mafiaG")})";
                        else if (Mafioso.allPlayers.Any(x => x.PlayerId == player.TargetPlayerId))
                            player.NameText.text = Mafioso.allPlayers.FirstOrDefault(x => x.PlayerId == player.TargetPlayerId).Data.PlayerName + $" ({ModTranslation.getString("mafiaM")})";
                        else if (Janitor.allPlayers.Any(x => x.PlayerId == player.TargetPlayerId))
                            player.NameText.text = Janitor.allPlayers.FirstOrDefault(x => x.PlayerId == player.TargetPlayerId).Data.PlayerName + $" ({ModTranslation.getString("mafiaJ")})";
            }

            // Lovers
            if (PlayerControl.LocalPlayer.isLovers() && !PlayerControl.LocalPlayer.Data.IsDead)
            {
                string suffix = Lovers.getIcon(PlayerControl.LocalPlayer);
                if (Cupid.isCupidLovers(PlayerControl.LocalPlayer))
                    suffix = Helpers.cs(Cupid.color, " ♥");
                var lover1 = PlayerControl.LocalPlayer;
                var lover2 = PlayerControl.LocalPlayer.getPartner();

                lover1.cosmetics.nameText.text += suffix;
                lover2.cosmetics.nameText.text += suffix;

                if (MeetingHud.Instance)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                        if (lover1.PlayerId == player.TargetPlayerId || lover2.PlayerId == player.TargetPlayerId)
                            player.NameText.text += suffix;
            }

            // Cupid
            if (PlayerControl.LocalPlayer.isRole(RoleId.Cupid))
            {
                string suffix = Helpers.cs(Cupid.color, " ♥");
                var cupid = Cupid.local;
                if (cupid.lovers1) cupid.lovers1.cosmetics.nameText.text += suffix;
                if (cupid.lovers2) cupid.lovers2.cosmetics.nameText.text += suffix;

                if (MeetingHud.Instance != null)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                        if ((cupid.lovers1 && cupid.lovers1?.PlayerId == player.TargetPlayerId) || (cupid.lovers2 && cupid.lovers2?.PlayerId == player.TargetPlayerId))
                            player.NameText.text += suffix;
            }

            // Kataomoi
            if (PlayerControl.LocalPlayer.isRole(RoleId.Kataomoi) && Kataomoi.target != null)
            {
                string suffix = Helpers.cs(Kataomoi.color, " ♥");
                Kataomoi.target.cosmetics.nameText.text += suffix;

                if (MeetingHud.Instance != null)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                        if (Kataomoi.target.PlayerId == player.TargetPlayerId)
                            player.NameText.text += suffix;
            }

            if (PlayerControl.LocalPlayer.isRole(RoleId.Yandere) && Yandere.target != null)
            {
                string suffix = Helpers.cs(Yandere.color, " ♥");
                Yandere.target.cosmetics.nameText.text += suffix;

                if (MeetingHud.Instance != null)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                        if (Yandere.target.PlayerId == player.TargetPlayerId)
                            player.NameText.text += suffix;
            }

            // Akujo
            if (!PlayerControl.LocalPlayer.Data.IsDead && (PlayerControl.LocalPlayer.isRole(RoleId.Akujo) || Akujo.isKeep(PlayerControl.LocalPlayer) || Akujo.isHonmei(PlayerControl.LocalPlayer)))
            {
                foreach (var akujo in Akujo.players)
                {
                    if (akujo == null || akujo.player == null) continue;
                    var honmeiSuffix = Helpers.cs(akujo.iconColor, " ♥");
                    var keepSuffix = Helpers.cs(Color.grey, " ♥");
                    if (PlayerControl.LocalPlayer == akujo.player) {
                        if (akujo.honmei) akujo.honmei.cosmetics.nameText.text += honmeiSuffix;
                        foreach (var keep in akujo.keeps)
                            keep.cosmetics.nameText.text += keepSuffix;

                        if (MeetingHud.Instance)
                        {
                            foreach (var player in MeetingHud.Instance.playerStates)
                            {
                                if (player.TargetPlayerId == akujo.honmei?.PlayerId)
                                    player.NameText.text += honmeiSuffix;
                                else if (akujo.keeps.Any(x => x.PlayerId == player.TargetPlayerId))
                                    player.NameText.text += keepSuffix;
                            }
                        }
                    }
                    else if (PlayerControl.LocalPlayer == akujo.honmei || akujo.keeps.Contains(PlayerControl.LocalPlayer))
                    {
                        akujo.player.cosmetics.nameText.text += honmeiSuffix;
                        if (MeetingHud.Instance)
                            foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                                if (player.TargetPlayerId == akujo.player?.PlayerId)
                                    player.NameText.text += honmeiSuffix;
                    }
                }
            }

            // Lawyer or Prosecutor
            bool localIsLawyer = Lawyer.target != null && PlayerControl.LocalPlayer.isRole(RoleId.Lawyer);
            bool localIsKnowingTarget = Lawyer.target != null && Lawyer.targetKnows && Lawyer.target == PlayerControl.LocalPlayer;

            if (localIsLawyer || (localIsKnowingTarget && Lawyer.hasAlivePlayers)) {
                Color color = Lawyer.color;
                PlayerControl target = Lawyer.target;
                string suffix = Helpers.cs(color, " §");
                target.cosmetics.nameText.text += suffix;

                if (MeetingHud.Instance != null)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                        if (player.TargetPlayerId == target.PlayerId)
                            player.NameText.text += suffix;
            }

            // Former Thief
            if (Thief.formerThief != null && (Thief.formerThief.Contains(PlayerControl.LocalPlayer) || PlayerControl.LocalPlayer.Data.IsDead)) {
                string suffix = Helpers.cs(Thief.color, " $");
                foreach (var formerThief in Thief.formerThief)
                    formerThief.cosmetics.nameText.text += suffix;
                if (MeetingHud.Instance != null)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                        if (Thief.formerThief.Any(x => x.PlayerId == player.TargetPlayerId))
                            player.NameText.text += suffix;
            }

            // Add medic shield info:
            foreach (var medic in Medic.players) {
                if (MeetingHud.Instance != null && Medic.shieldVisible(medic.shielded)) {
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                        if (player.TargetPlayerId == medic.shielded.PlayerId)
                            player.NameText.text = Helpers.cs(Medic.color, "[") + player.NameText.text + Helpers.cs(Medic.color, "]");
                }
            }
        }

        static void timerUpdate() {
            var dt = Time.deltaTime;
            foreach (var hacker in Hacker.players)
                hacker.hackerTimer -= dt;
            Trickster.lightsOutTimer -= dt;
            foreach (var tracker in Tracker.players)
                tracker.corpsesTrackingTimer -= dt;
            foreach (var assassin in Assassin.players)
                assassin.invisibleTimer -= dt;
            // PropHunt timer managed inside PropHuntGM.update()
            foreach (byte key in Deputy.handcuffedKnows.Keys)
                Deputy.handcuffedKnows[key] -= dt;
        }

        public static void miniUpdate() {
            //  || Mini.mini == MimicK.mimicK && MimicK.victim != null
            // the above line deleted in 2024.3.9, specified the MimicK instead
            if (Mini.mini == null || Camouflager.camouflageTimer > 0f || Helpers.MushroomSabotageActive() || (Mini.mini.isRole(RoleId.MimicA) && MimicA.isMorph) || Morphling.players.Any(x => x.player == Mini.mini && x.morphTimer > 0f) || Ninja.isStealthed(Mini.mini)
                || (Mini.mini.isRole(RoleId.Fox) && Fox.stealthed) || Assassin.players.Any(x => x.player == Mini.mini && x.isInvisble) || Sprinter.isSprinting(Mini.mini) || (Mini.mini.isRole(RoleId.Kataomoi) && Kataomoi.isStalking()) || SurveillanceMinigamePatch.nightVisionIsActive) return;
                
            float growingProgress = Mini.growingProgress();
            float scale = growingProgress * 0.35f + 0.35f;
            string suffix = "";
            if (growingProgress != 1f)
                suffix = " <color=#FAD934FF>(" + Mathf.FloorToInt(growingProgress * 18) + ")</color>"; 
            if (!Mini.isGrowingUpInMeeting && MeetingHud.Instance != null && Mini.ageOnMeetingStart != 0 && !(Mini.ageOnMeetingStart >= 18))
                suffix = " <color=#FAD934FF>(" + Mini.ageOnMeetingStart + ")</color>";

            if (!(Mini.mini.isRole(RoleId.MimicK) && MimicK.victim != null)) Mini.mini.cosmetics.nameText.text += suffix;
            if (MeetingHud.Instance != null) {
                foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                    if (player.NameText != null && Mini.mini.PlayerId == player.TargetPlayerId)
                        player.NameText.text += suffix;
            }

            foreach (var morphling in Morphling.players) {
                if (morphling.morphTarget == Mini.mini && morphling.morphTimer > 0f)
                    morphling.player.cosmetics.nameText.text += suffix;
            }
            if (MimicA.isMorph && Mini.mini.isRole(RoleId.MimicK))
                foreach (var mimicA in MimicA.allPlayers)
                    mimicA.cosmetics.nameText.text += suffix;
        }

        static void updateImpostorKillButton(HudManager __instance) {
            if (!PlayerControl.LocalPlayer.Data.Role.IsImpostor) return;
            if (MeetingHud.Instance) {
                __instance.KillButton.Hide();
                return;
            }
            bool enabled = true;
            if (PlayerControl.LocalPlayer.isRole(RoleId.Vampire))
                enabled = false;
            else if (PlayerControl.LocalPlayer.isRole(RoleId.MimicA) && MimicK.isAlive())
                enabled = false;
            else if (PlayerControl.LocalPlayer.isRole(RoleId.BomberA) && BomberB.isAlive())
                enabled = false;
            else if (PlayerControl.LocalPlayer.isRole(RoleId.BomberB) && BomberA.isAlive())
                enabled = false;

            if (enabled) __instance.KillButton.Show();
            else __instance.KillButton.Hide();

            if (Deputy.handcuffedKnows.ContainsKey(PlayerControl.LocalPlayer.PlayerId) && Deputy.handcuffedKnows[PlayerControl.LocalPlayer.PlayerId] > 0) __instance.KillButton.Hide();
        }

        static void updateReportButton(HudManager __instance) {
            if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return;
            if (Deputy.handcuffedKnows.ContainsKey(PlayerControl.LocalPlayer.PlayerId) && Deputy.handcuffedKnows[PlayerControl.LocalPlayer.PlayerId] > 0 || MeetingHud.Instance) __instance.ReportButton.Hide();
            else if (!__instance.ReportButton.isActiveAndEnabled) __instance.ReportButton.Show();
        }
         
        static void updateVentButton(HudManager __instance)
        {
            if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return;
            if ((Deputy.handcuffedKnows.ContainsKey(PlayerControl.LocalPlayer.PlayerId) && Deputy.handcuffedKnows[PlayerControl.LocalPlayer.PlayerId] > 0) || MeetingHud.Instance || PlayerControl.LocalPlayer.roleCanUseVents() == false) __instance.ImpostorVentButton.Hide();
            else if (PlayerControl.LocalPlayer.roleCanUseVents() && !__instance.ImpostorVentButton.isActiveAndEnabled) __instance.ImpostorVentButton.Show();

            if (Madmate.madmate.Any(x => x.PlayerId == PlayerControl.LocalPlayer.PlayerId))
                __instance.ImpostorVentButton.transform.localPosition = PlayerControl.LocalPlayer.isRole(RoleId.Engineer) ? CustomButton.ButtonPositions.lowerRowRight : CustomButton.ButtonPositions.upperRowLeft;
            if (CreatedMadmate.createdMadmate.Any(x => x.PlayerId == PlayerControl.LocalPlayer.PlayerId) && CreatedMadmate.canEnterVents) __instance.ImpostorVentButton.transform.localPosition = CustomButton.ButtonPositions.lowerRowRight;

            if (Rewired.ReInput.players.GetPlayer(0).GetButtonDown(RewiredConsts.Action.UseVent) && !PlayerControl.LocalPlayer.Data.Role.IsImpostor && PlayerControl.LocalPlayer.roleCanUseVents()) {
                __instance.ImpostorVentButton.DoClick();
            }
        }

        static void updateUseButton(HudManager __instance) {
            if (MeetingHud.Instance) __instance.UseButton.Hide();
        }

        static void updateSabotageButton(HudManager __instance) {
            if (MeetingHud.Instance || TORMapOptions.gameMode == CustomGamemodes.PropHunt || !PlayerControl.LocalPlayer.roleCanUseSabotage()) __instance.SabotageButton.Hide();
            else if (PlayerControl.LocalPlayer.roleCanUseSabotage() && !__instance.SabotageButton.isActiveAndEnabled) __instance.SabotageButton.Show();

            if (Helpers.ShowButtons) {
                if ((CreatedMadmate.createdMadmate.Any(x => x.PlayerId == PlayerControl.LocalPlayer.PlayerId) && CreatedMadmate.canSabotage)
                    || (Madmate.madmate.Any(x => x.PlayerId == PlayerControl.LocalPlayer.PlayerId) && Madmate.canSabotage))
                    __instance.SabotageButton.transform.localPosition = CustomButton.ButtonPositions.upperRowCenter + __instance.UseButton.transform.localPosition;
            }
        }

        static void updateVisibility()
        {
            if (PlayerControl.LocalPlayer.isRole(RoleId.Medium) && !PlayerControl.LocalPlayer.Data.IsDead)
            {
                foreach (var player in Medium.local.questioned)
                {
                    if (player != null)
                        player.Visible = true;
                }
            }

            if (!MeetingHud.Instance && Pelican.players.Any(x => x.eatenPlayers.Any(p => p.PlayerId == PlayerControl.LocalPlayer.PlayerId)))
            {
                HudManager.Instance.ShadowQuad?.gameObject?.SetActive(true);
                foreach (var p in PlayerControl.AllPlayerControls)
                    if (p.Data.IsDead && p != PlayerControl.LocalPlayer)
                        p.Visible = false;
            }
        }

        static void Postfix(HudManager __instance)
        {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started || GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return;
            
            EventUtility.Update();

            CustomButton.HudUpdate();
            resetNameTagsAndColors();
            setNameColors();
            setNameTags();

            // Impostors
            updateImpostorKillButton(__instance);
            // Timer updates
            timerUpdate();
            // Mini
            miniUpdate();

            // Deputy Sabotage, Use and Vent Button Disabling
            updateReportButton(__instance);
            updateVentButton(__instance);
            // Meeting hide buttons if needed (used for the map usage, because closing the map would show buttons)
            updateSabotageButton(__instance);
            updateUseButton(__instance);
            updateVisibility();
            if (!MeetingHud.Instance) __instance.AbilityButton?.Update();
            TORGameManager.Instance?.OnUpdate();
            foreach (var arrow in new List<Sherlock.SherlockDetectArrow>(Sherlock.SherlockDetectArrow.allArrows))
                arrow.HudUpdate();
            foreach (var reporter in DeadBodyReporter.DeadBodyReporters.ToList())
                reporter.Update();

            // Fix dead player's pets being visible by just always updating whether the pet should be visible at all.
            foreach (PlayerControl target in PlayerControl.AllPlayerControls) {
                var pet = target.GetPet();
                if (pet != null) {
                    pet.Visible = ((PlayerControl.LocalPlayer.Data.IsDead && target.Data.IsDead) || !target.Data.IsDead) && !target.inVent;
                }
            }
        }
    }
}
