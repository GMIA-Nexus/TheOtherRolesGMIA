using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using Sentry.Internal.Extensions;
using TheOtherRoles.CustomGameModes;
using TheOtherRoles.MetaContext;
using TheOtherRoles.Modules;
using TheOtherRoles.Objects;
using TheOtherRoles.Roles;
using TheOtherRoles.Utilities;
using UnityEngine;
using static TheOtherRoles.GameHistory;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Patches {
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    public static class PlayerControlFixedUpdatePatch {
        // Helpers

        public static PlayerControl setTarget(bool onlyCrewmates = false, bool targetPlayersInVents = false, List<PlayerControl> untargetablePlayers = null, PlayerControl targetingPlayer = null) {
            PlayerControl result = null;
            float num = AmongUs.GameOptions.LegacyGameOptions.KillDistances[Mathf.Clamp(GameOptionsManager.Instance.currentNormalGameOptions.KillDistance, 0, 2)];
            if (!MapUtilities.CachedShipStatus) return result;
            if (targetingPlayer == null) targetingPlayer = PlayerControl.LocalPlayer;
            if (targetingPlayer.Data.IsDead) return result;

            untargetablePlayers ??= new List<PlayerControl>();

            if (!Ninja.canBeTargeted) {
                foreach (var ninja in Ninja.players) {
                    if (ninja.player && ninja.stealthed)
                        untargetablePlayers.Add(ninja.player);
                }
            }
            foreach (var sprinter in Sprinter.players) {
                if (sprinter.player && sprinter.sprinting)
                    untargetablePlayers.Add(sprinter.player);
            }
            if (Fox.exists && Fox.stealthed) {
                untargetablePlayers.AddRange(Fox.allPlayers);
            }
            if (Kataomoi.exists && Kataomoi.target != null && Kataomoi.isStalking()) {
                untargetablePlayers.AddRange(Kataomoi.allPlayers);
            }

            Vector2 truePosition = targetingPlayer.GetTruePosition();
            foreach (var playerInfo in GameData.Instance.AllPlayers.GetFastEnumerator())
            {
                if (!playerInfo.Disconnected && playerInfo.PlayerId != targetingPlayer.PlayerId && !playerInfo.IsDead && (!onlyCrewmates || !playerInfo.Role.IsImpostor)) {
                    PlayerControl @object = playerInfo.Object;
                    if (untargetablePlayers != null && untargetablePlayers.Any(x => x == @object)) {
                        // if that player is not targetable: skip check
                        continue;
                    }                    

                    if (@object && (!@object.inVent || targetPlayersInVents)) {
                        Vector2 vector = @object.GetTruePosition() - truePosition;
                        float magnitude = vector.magnitude;
                        if (magnitude <= num && !PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized, magnitude, Constants.ShipAndObjectsMask)) {
                            result = @object;
                            num = magnitude;
                        }
                    }
                }
            }
            return result;
        }

        public static void setPlayerOutline(PlayerControl target, Color color) {
            if (target == null || target.cosmetics?.currentBodySprite?.BodySprite == null) return;
            color = color.SetAlpha(Chameleon.visibility(target.PlayerId));

            target.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", 1f);
            target.cosmetics.currentBodySprite.BodySprite.material.SetColor("_OutlineColor", color);
        }

        // Update functions

        static void setBasePlayerOutlines() {
            foreach (PlayerControl target in PlayerControl.AllPlayerControls) {
                if (target == null || target.cosmetics?.currentBodySprite?.BodySprite == null) continue;

                bool isMorphedMorphling = Morphling.players.Any(x => x.player == target && x.morphTarget != null && x.morphTimer > 0f);
                bool isMimicKShield = target.isRole(RoleId.MimicK) && MimicK.victim != null;
                bool isMimicAMorph = target.isRole(RoleId.MimicA) && MimicA.isMorph;
                bool hasVisibleShield = false;
                Color color = Medic.shieldedColor;
                if (Camouflager.camouflageTimer <= 0f && !Helpers.MushroomSabotageActive() && Medic.shieldVisible(target))
                    hasVisibleShield = true;

                if (Camouflager.camouflageTimer <= 0f && !Helpers.MushroomSabotageActive() && TORMapOptions.firstKillPlayer != null && TORMapOptions.shieldFirstKill && ((target == TORMapOptions.firstKillPlayer && !isMorphedMorphling && !isMimicKShield && !isMimicAMorph) || (isMorphedMorphling && Morphling.getRole(target).morphTarget == TORMapOptions.firstKillPlayer)
                    || (isMimicAMorph && TORMapOptions.firstKillPlayer.isRole(RoleId.MimicK)))) {
                    hasVisibleShield = true;
                    color = Color.blue;
                }

                if (PlayerControl.LocalPlayer.Data.IsDead && Armored.armored != null && target == Armored.armored && !Armored.isBrokenArmor && !hasVisibleShield)
                {
                    hasVisibleShield = true;
                    color = Color.yellow;
                }

                if (hasVisibleShield) {
                target.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", 1f);
                target.cosmetics.currentBodySprite.BodySprite.material.SetColor("_OutlineColor", color);
                }
                else {
                    target.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", 0f);
                }
            }
        }

        static void setPetVisibility() {
            bool localalive = !PlayerControl.LocalPlayer.Data.IsDead;
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                bool playeralive = !player.Data.IsDead;
                player.cosmetics.SetPetVisible((localalive && playeralive) || !localalive);
            }
        }

        public static void bendTimeUpdate() {
            if (TimeMaster.isRewinding) {
                if (localPlayerPositions.Count > 0) {
                    // Set position
                    var next = localPlayerPositions[0];
                    if (next.Item2 == true) {
                        // Exit current vent if necessary
                        if (PlayerControl.LocalPlayer.inVent) {
                            foreach (Vent vent in MapUtilities.CachedShipStatus.AllVents) {
                                bool canUse;
                                bool couldUse;
                                vent.CanUse(PlayerControl.LocalPlayer.Data, out canUse, out couldUse);
                                if (canUse) {
                                    PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(vent.Id);
                                    vent.SetButtons(false);
                                }
                            }
                        }
                        // Set position
                        PlayerControl.LocalPlayer.transform.position = next.Item1;
                    }
                    else if (localPlayerPositions.Any(x => x.Item2 == true)) {
                        PlayerControl.LocalPlayer.transform.position = next.Item1;
                    }
                    if (SubmergedCompatibility.IsSubmerged) {
                        SubmergedCompatibility.ChangeFloor(next.Item1.y > -7);
                    }

                    localPlayerPositions.RemoveAt(0);

                    if (localPlayerPositions.Count > 1) localPlayerPositions.RemoveAt(0); // Skip every second position to rewinde twice as fast, but never skip the last position

                    // Revive local player
                    if (TimeMaster.exists && TimeMaster.reviveDuringReweind && PlayerControl.LocalPlayer.Data.IsDead)
                    {
                        DeadPlayer deadPlayer = deadPlayers.Where(x => x.player == PlayerControl.LocalPlayer).FirstOrDefault();
                        if (deadPlayer != null && next.Item3 < deadPlayer.timeOfDeath)
                            RPCProcedure.RpcRevive.Invoke(PlayerControl.LocalPlayer.PlayerId);
                    }
                }
                else {
                    TimeMaster.isRewinding = false;
                    PlayerControl.LocalPlayer.moveable = true;
                }
            }
            else {
                while (localPlayerPositions.Count >= Mathf.Round(TimeMaster.rewindTime / Time.fixedDeltaTime)) localPlayerPositions.RemoveAt(localPlayerPositions.Count - 1);
                localPlayerPositions.Insert(0, new(PlayerControl.LocalPlayer.transform.position, PlayerControl.LocalPlayer.CanMove, DateTime.UtcNow)); // CanMove = CanMove
            }
        }

        static void impostorSetTarget() {
            if (!PlayerControl.LocalPlayer.Data.Role.IsImpostor || (!PlayerControl.LocalPlayer.CanMove && !(PlayerControl.LocalPlayer.isRole(RoleId.Trickster) && Trickster.isInTricksterVent)) ||
                PlayerControl.LocalPlayer.Data.IsDead || (PlayerControl.LocalPlayer.isRole(RoleId.Undertaker) && Undertaker.DraggedBody != null)) { // !isImpostor || !canMove || isDead
                FastDestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(null);
                return;
            }

            PlayerControl target = null;
            if (Spy.exists) {
                if (Spy.impostorsCanKillAnyone) {
                    target = setTarget(false, true);
                }
                else {
                    var untargetables = new List<PlayerControl>();
                    untargetables.AddRange(Spy.allPlayers);
                    untargetables.AddRange(Jackal.players.Where(x => x.wasTeamRed).Select(x => x.player));
                    untargetables.AddRange(Sidekick.players.Where(x => x.wasTeamRed).Select(x => x.player));
                    target = setTarget(true, true, untargetables);
                }
            }
            else {
                var untargetables = new List<PlayerControl>();
                untargetables.AddRange(Jackal.players.Where(x => x.wasTeamRed).Select(x => x.player));
                untargetables.AddRange(Sidekick.players.Where(x => x.wasTeamRed).Select(x => x.player));
                target = setTarget(true, true, untargetables);
            }

            FastDestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(target); // Includes setPlayerOutline(target, Palette.ImpstorRed);
        }

        public static void playerSizeUpdate(PlayerControl p) {
            // Set default player size
            CircleCollider2D collider = p.Collider.CastFast<CircleCollider2D>();

            p.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            collider.radius = Mini.defaultColliderRadius;
            collider.offset = Mini.defaultColliderOffset * Vector2.down;

            // Set adapted player size to Mini and Morphling
            if (Mini.mini == null || Camouflager.camouflageTimer > 0f || Helpers.MushroomSabotageActive() || Morphling.players.Any(x => x.player == Mini.mini && x.morphTimer > 0f) || (Mini.mini.isRole(RoleId.MimicA) && MimicA.isMorph)) return;

            float growingProgress = Mini.growingProgress();
            float scale = growingProgress * 0.35f + 0.35f;
            float correctedColliderRadius = Mini.defaultColliderRadius * 0.7f / scale; // scale / 0.7f is the factor by which we decrease the player size, hence we need to increase the collider size by 0.7f / scale

            if (p == Mini.mini && !(Mini.mini.isRole(RoleId.MimicK) && MimicK.victim != null)) {
                p.transform.localScale = new Vector3(scale, scale, 1f);
                collider.radius = correctedColliderRadius;
            }
            if (Morphling.players.Any(x => x.morphTarget == Mini.mini && x.morphTimer > 0f) ||
                (p.isRole(RoleId.MimicA) && MimicA.isMorph && Mini.mini.isRole(RoleId.MimicK))) {
                p.transform.localScale = new Vector3(scale, scale, 1f);
                collider.radius = correctedColliderRadius;
            }
        }

        static void accelTrapUpdate()
        {
            if (!CustomOptionHolder.activateProps.getBool()) return;
            if (Props.AccelTrap.accels == null || Props.AccelTrap.accels.Count == 0 || MeetingHud.Instance) return;
            try
            {
                foreach (var acce in Props.AccelTrap.accels)
                {
                    if (acce.accelTrap == null) return;
                    if (!PlayerControl.LocalPlayer.Data.IsDead && acce.accelTrap.transform != null
                        && Vector3.Distance(PlayerControl.LocalPlayer.transform.position, acce.accelTrap.transform.position) < 0.25f &&
                        !PlayerControl.LocalPlayer.inVent)
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ActivateAccel, Hazel.SendOption.Reliable, -1);
                        writer.Write(PlayerControl.LocalPlayer.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.activateAccel(PlayerControl.LocalPlayer.PlayerId);
                    }
                }
                if (Props.AccelTrap.acceled.ContainsKey(PlayerControl.LocalPlayer) && DateTime.UtcNow.Subtract(Props.AccelTrap.acceled[PlayerControl.LocalPlayer]).TotalSeconds >
                        CustomOptionHolder.accelerationDuration.getFloat())
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.DeactivateAccel, Hazel.SendOption.Reliable, -1);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.deactivateAccel(PlayerControl.LocalPlayer.PlayerId);
                }
            }
            catch (NullReferenceException e)
            {
                TheOtherRolesPlugin.Logger.LogWarning(e.Message);
            }
        }

        public static void updatePlayerInfo() {
            bool commsActive = false;
            foreach (PlayerTask t in PlayerControl.LocalPlayer.myTasks)
            {
                if (t.TaskType == TaskTypes.FixComms)
                {
                    commsActive = true;
                    break;
                }
            }

            Vector3 colorBlindTextMeetingInitialLocalPos = new(0.3384f, -0.16666f, -0.01f);
            Vector3 colorBlindTextMeetingInitialLocalScale = new(0.9f, 1f, 1f);
            foreach (PlayerControl p in PlayerControl.AllPlayerControls) {
                
                // Colorblind Text in Meeting
                PlayerVoteArea playerVoteArea = MeetingHud.Instance?.playerStates?.FirstOrDefault(x => x.TargetPlayerId == p.PlayerId);
                if (playerVoteArea != null && playerVoteArea.ColorBlindName.gameObject.active) {
                    playerVoteArea.ColorBlindName.transform.localPosition = colorBlindTextMeetingInitialLocalPos + new Vector3(0f, 0.4f, 0f);
                    playerVoteArea.ColorBlindName.transform.localScale = colorBlindTextMeetingInitialLocalScale * 0.8f;
                }

                // Colorblind Text During the round
                if (p.cosmetics.colorBlindText != null && p.cosmetics.showColorBlindText && p.cosmetics.colorBlindText.gameObject.active) {
                    p.cosmetics.colorBlindText.transform.localPosition = new Vector3(0, -1f, 0f);
                }

                p.cosmetics.nameText.transform.parent.SetLocalZ(-0.0001f);  // This moves both the name AND the colorblindtext behind objects (if the player is behind the object), like the rock on polus

                if ((Lawyer.lawyerKnowsRole && PlayerControl.LocalPlayer.isRole(RoleId.Lawyer) && p == Lawyer.target) || (Akujo.knowsRoles && Akujo.isPartner(PlayerControl.LocalPlayer, p)) || p == PlayerControl.LocalPlayer || (PlayerControl.LocalPlayer.Data.IsDead
                    && RoleManager.IsGhostRole(PlayerControl.LocalPlayer.Data.RoleType)) || (Godfather.shouldShowInfo(PlayerControl.LocalPlayer) && Godfather.killed.Contains(p)) || FreePlayGM.isFreePlayGM) {
                    Transform playerInfoTransform = p.cosmetics.nameText.transform.parent.FindChild("Info");
                    TMPro.TextMeshPro playerInfo = playerInfoTransform != null ? playerInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                    if (playerInfo == null) {
                        playerInfo = UnityEngine.Object.Instantiate(p.cosmetics.nameText, p.cosmetics.nameText.transform.parent);
                        playerInfo.transform.localPosition += Vector3.up * 0.225f;
                        playerInfo.fontSize *= 0.75f;
                        playerInfo.gameObject.name = "Info";
                        playerInfo.color = playerInfo.color.SetAlpha(1f);
                    }
    
                    Transform meetingInfoTransform = playerVoteArea != null ? playerVoteArea.NameText.transform.parent.FindChild("Info") : null;
                    TMPro.TextMeshPro meetingInfo = meetingInfoTransform != null ? meetingInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                    if (meetingInfo == null && playerVoteArea != null) {
                        meetingInfo = UnityEngine.Object.Instantiate(playerVoteArea.NameText, playerVoteArea.NameText.transform.parent);
                        meetingInfo.transform.localPosition += Vector3.down * 0.2f;
                        meetingInfo.fontSize *= 0.60f;
                        meetingInfo.gameObject.name = "Info";
                    }

                    // Set player name higher to align in middle
                    if (meetingInfo != null && playerVoteArea != null) {
                        var playerName = playerVoteArea.NameText;
                        playerName.transform.localPosition = new Vector3(0.3384f, 0.0311f, -0.1f);
                    }

                    bool isTaskMasterExTask = p.isRole(RoleId.TaskMaster) && TaskMaster.isTaskComplete;
                    var (tasksCompleted, tasksTotal) = TasksHandler.taskInfo(p.Data);
                    var (exTasksCompleted, exTasksTotal) = TasksHandler.taskInfo(p.Data, true);
                    string roleNames = RoleInfo.GetRolesString(p, true, false);
                    string roleText = RoleInfo.GetRolesString(p, true, ClientOption.GetValue(ClientOption.ClientOptionType.SpoilerAfterDeath) == 1, false, true, [RoleId.Lover]);
                    string taskInfo = tasksTotal > 0 ? $"<color=#FAD934FF>({(commsActive ? "?" : tasksCompleted)}/{tasksTotal})</color>" : "";
                    string exTaskInfo = exTasksTotal > 0 ? $"<color=#E1564BFF>({exTasksCompleted}/{exTasksTotal})</color>" : "";

                    string playerInfoText = "";
                    string meetingInfoText = "";                    
                    if (p == PlayerControl.LocalPlayer) {
                        playerInfoText = $"{roleNames}";
                        if (p.isRole(RoleId.Swapper)) playerInfoText = $"{roleNames}" + Helpers.cs((p.Data.Role.IsImpostor || Madmate.madmate.Any(x => x.PlayerId == p.PlayerId)) ? Palette.ImpostorRed : Swapper.color, $" ({Swapper.charges})");
                        if (HudManager.Instance.TaskPanel != null) {
                            TMPro.TextMeshPro tabText = HudManager.Instance.TaskPanel.tab.transform.FindChild("TabText_TMP").GetComponent<TMPro.TextMeshPro>();
                            tabText.SetText(String.Format("{0} {1}", isTaskMasterExTask ? ModTranslation.getString("taskMasterExTasks") : TranslationController.Instance.GetString(StringNames.Tasks), isTaskMasterExTask ? exTaskInfo : taskInfo));
                        }
                        if (!isTaskMasterExTask)
                            meetingInfoText = $"{roleNames} {taskInfo}".Trim();
                        else
                            meetingInfoText = $"{roleNames} {exTaskInfo}".Trim();
                    }
                    else if (!PlayerControl.LocalPlayer.Data.IsDead && Akujo.isPartner(PlayerControl.LocalPlayer, p) && Akujo.knowsRoles) {
                        playerInfoText = roleText;
                        meetingInfoText = roleNames;
                    }
                    else if (ClientOption.GetValue(ClientOption.ClientOptionType.SpoilerAfterDeath) == 1) {
                        if (!isTaskMasterExTask)
                        {
                            playerInfoText = $"{roleText} {taskInfo}".Trim();
                            meetingInfoText = $"{roleNames} {taskInfo}".Trim();
                        }
                        else
                        {
                            playerInfoText = $"{roleText} {exTaskInfo}".Trim();
                            meetingInfoText = $"{roleNames} {exTaskInfo}".Trim();
                        }
                    }
                    else if ((Lawyer.lawyerKnowsRole && PlayerControl.LocalPlayer.isRole(RoleId.Lawyer) && p == Lawyer.target)
                        || (Godfather.shouldShowInfo(PlayerControl.LocalPlayer) && Godfather.killed.Contains(p))) {
                        playerInfoText = $"{roleText}";
                        meetingInfoText = $"{roleNames}";
                    }

                    playerInfo.text = playerInfoText;
                    playerInfo.gameObject.SetActive(p.Visible);
                    if (meetingInfo != null) meetingInfo.text = MeetingHud.Instance.state == MeetingHud.VoteStates.Results ? "" : meetingInfoText;
                }                
            }
        }

        private static void multitaskerUpdate()
        {
            if (Multitasker.multitasker.Any(x => x.PlayerId == PlayerControl.LocalPlayer.PlayerId))
            {
                if (PlayerControl.LocalPlayer.Data.IsDead) return;
                if (!Minigame.Instance) return;

                var Base = Minigame.Instance as MonoBehaviour;
                SpriteRenderer[] rends = Base.GetComponentsInChildren<SpriteRenderer>();
                for (var i = 0; i < rends.Length; i++)
                {
                    var oldColor1 = rends[i].color[0];
                    var oldColor2 = rends[i].color[1];
                    var oldColor3 = rends[i].color[2];
                    rends[i].color = new Color(oldColor1, oldColor2, oldColor3, 0.5f);
                }
            }
        }

        private static void radarUpdate()
        {
            if (MeetingHud.Instance)
                return;

            if (PlayerControl.LocalPlayer != Radar.radar || PlayerControl.LocalPlayer.Data.IsDead)
            {
                if (Radar.localArrow?.arrow != null)
                    UnityEngine.Object.Destroy(Radar.localArrow?.arrow);
                Radar.localArrow = null;
                return;
            }

            PlayerControl closestPlayer = null;
            float closestDistance = float.MaxValue;
            Vector2 refPosition = PlayerControl.LocalPlayer.GetTruePosition();

            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.Data.IsDead || player.PlayerId == PlayerControl.LocalPlayer.PlayerId || !player.Collider.enabled)
                    continue;

                float distance = Vector2.Distance(refPosition, player.GetTruePosition());
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPlayer = player;
                }
            }

            if (closestPlayer != null)
            {
                if (Radar.localArrow == null)
                {
                    Radar.localArrow = new Arrow(Radar.color);
                    Radar.localArrow.arrow.SetActive(true);
                }
                Radar.localArrow.Update(closestPlayer.transform.position);
            }
            else
            {
                if (Radar.localArrow != null && Radar.localArrow.arrow != null)
                {
                    Radar.localArrow.arrow.SetActive(false);
                }
            }
        }

        static void decelTrapUpdate()
        {
            if (!CustomOptionHolder.activateProps.getBool()) return;
            if (Props.DecelTrap.decels == null || Props.DecelTrap.decels.Count == 0 || MeetingHud.Instance) return;
            try
            {
                foreach (var decel in Props.DecelTrap.decels)
                {
                    if (decel.decelTrap == null) return;
                    if (!PlayerControl.LocalPlayer.Data.IsDead && decel.decelTrap.transform != null
                        && Vector3.Distance(PlayerControl.LocalPlayer.transform.position, decel.decelTrap.transform.position) < 0.6f &&
                        !PlayerControl.LocalPlayer.inVent && !decel.isTriggered)
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ActivateDecel, Hazel.SendOption.Reliable, -1);
                        writer.Write(Props.DecelTrap.getId(decel));
                        writer.Write(PlayerControl.LocalPlayer.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.activateDecel(Props.DecelTrap.getId(decel), PlayerControl.LocalPlayer.PlayerId);
                    }

                    if (decel.isTriggered && DateTime.UtcNow.Subtract(decel.activateTime).TotalSeconds >= CustomOptionHolder.decelUpdateInterval.getFloat())
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UntriggerDecel, Hazel.SendOption.Reliable, -1);
                        writer.Write(Props.DecelTrap.getId(decel));
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.untriggerDecel(Props.DecelTrap.getId(decel));
                    }

                    if (Props.DecelTrap.deceled.ContainsKey(PlayerControl.LocalPlayer) && DateTime.UtcNow.Subtract(Props.DecelTrap.deceled[PlayerControl.LocalPlayer]).TotalSeconds >
                        CustomOptionHolder.decelerationDuration.getFloat())
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.DeactivateDecel, Hazel.SendOption.Reliable, -1);
                        writer.Write(PlayerControl.LocalPlayer.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.deactivateDecel(PlayerControl.LocalPlayer.PlayerId);
                    }
                }
            }
            catch (NullReferenceException e)
            {
                TheOtherRolesPlugin.Logger.LogWarning(e.Message);
            }
        }

        static bool mushroomSaboWasActive = false;
        static void morphlingAndCamouflagerUpdate() {
            bool mushRoomSaboIsActive = Helpers.MushroomSabotageActive();
            if (!mushroomSaboWasActive) mushroomSaboWasActive = mushRoomSaboIsActive;

            float oldCamouflageTimer = Camouflager.camouflageTimer;
            Dictionary<byte, float> oldMorphTimer = new();
            Camouflager.camouflageTimer = Mathf.Max(0f, Camouflager.camouflageTimer - Time.fixedDeltaTime);
            foreach (var morphling in Morphling.players) {
                oldMorphTimer[morphling.player.PlayerId] = morphling.morphTimer;
                morphling.morphTimer = Mathf.Max(0f, morphling.morphTimer - Time.fixedDeltaTime);
            }

            if (mushRoomSaboIsActive) return;

            // Camouflage reset and set Morphling look if necessary
            if (oldCamouflageTimer > 0f && Camouflager.camouflageTimer <= 0f) {
                Camouflager.resetCamouflage();
                foreach (var morphling in Morphling.players) {
                    if (morphling.morphTimer > 0f && morphling.morphTarget != null) {
                        PlayerControl target = morphling.morphTarget;
                        morphling.player.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId, target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId);
                    }
                }
                if (MimicK.isAlive() && MimicK.victim != null)
                {
                    PlayerControl target = MimicK.victim;
                    foreach (var mimicK in MimicK.allPlayers)
                        mimicK.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId, target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId);
                }
                if (MimicA.isAlive() && MimicA.isMorph)
                {
                    PlayerControl target = MimicK.allPlayers.FirstOrDefault();
                    foreach (var mimicA in MimicA.allPlayers)
                        mimicA.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId, target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId);
                }
            }

            // If the MushRoomSabotage ends while Morph is still active set the Morphlings look to the target's look
            if (mushroomSaboWasActive)
            {
                foreach (var morphling in Morphling.players) {
                    if (morphling.morphTimer > 0f && morphling.morphTarget != null) {
                        PlayerControl target = morphling.morphTarget;
                        morphling.player.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId, target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId);
                    }
                }
                if (MimicK.isAlive() && MimicK.victim != null)
                {
                    PlayerControl target = MimicK.victim;
                    foreach (var mimicK in MimicK.allPlayers)
                        mimicK.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId, target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId);
                }
                if (MimicA.isAlive() && MimicA.isMorph)
                {
                    PlayerControl target = MimicK.allPlayers.FirstOrDefault();
                    foreach (var mimicA in MimicA.allPlayers)
                        mimicA.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId, target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId);
                }
                if (Camouflager.camouflageTimer > 0)
                {
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                        player.setLook("", 6, "", "", "", "");
                }
            }

            // Morphling reset (only if camouflage is inactive)
            if (Camouflager.camouflageTimer <= 0f) {
                foreach (var morphling in Morphling.players) {
                    if (morphling.morphTimer <= 0f && oldMorphTimer.TryGetValue(morphling.player.PlayerId, out float value) && value > 0f) morphling.resetMorph();
                }
            }
            mushroomSaboWasActive = false;
        }

        // Mini set adapted button cooldown for Vampire, Sheriff, Jackal, Sidekick, Warlock, Cleaner
        public static void miniCooldownUpdate() {
            if (Mini.mini != null && PlayerControl.LocalPlayer == Mini.mini) {
                var multiplier = Mini.isGrownUp() ? 0.66f : 2f;
                HudManagerStartPatch.sheriffKillButton.MaxTimer = Sheriff.cooldown * multiplier;
                HudManagerStartPatch.vampireKillButton.MaxTimer = Vampire.cooldown * multiplier;
                HudManagerStartPatch.jackalKillButton.MaxTimer = Jackal.cooldown * multiplier;
                HudManagerStartPatch.sidekickKillButton.MaxTimer = Sidekick.cooldown * multiplier;
                HudManagerStartPatch.warlockCurseButton.MaxTimer = Warlock.cooldown * multiplier;
                HudManagerStartPatch.cleanerCleanButton.MaxTimer = Cleaner.cooldown * multiplier;
                HudManagerStartPatch.witchSpellButton.MaxTimer = (Witch.cooldown + (Witch.local != null ? Witch.local.currentCooldownAddition : 0f)) * multiplier;
                HudManagerStartPatch.assassinButton.MaxTimer = Assassin.cooldown * multiplier;
                HudManagerStartPatch.thiefKillButton.MaxTimer = Thief.cooldown * multiplier;
                HudManagerStartPatch.serialKillerButton.EffectDuration = SerialKiller.suicideTimer * (Mini.isGrownUp() ? 1f : 2f);
                HudManagerStartPatch.schrodingersCatKillButton.MaxTimer = SchrodingersCat.killCooldown * multiplier;
            }
        }

        static void hunterUpdate() {
            if (!HideNSeek.isHideNSeekGM) return;
            int minutes = (int)HideNSeek.timer / 60;
            int seconds = (int)HideNSeek.timer % 60;
            string suffix = $" {minutes:00}:{seconds:00}";

            if (HideNSeek.timerText == null) {
                RoomTracker roomTracker = FastDestroyableSingleton<HudManager>.Instance?.roomTracker;
                if (roomTracker != null) {
                    GameObject gameObject = UnityEngine.Object.Instantiate(roomTracker.gameObject);

                    gameObject.transform.SetParent(FastDestroyableSingleton<HudManager>.Instance.transform);
                    UnityEngine.Object.DestroyImmediate(gameObject.GetComponent<RoomTracker>());
                    HideNSeek.timerText = gameObject.GetComponent<TMPro.TMP_Text>();

                    // Use local position to place it in the player's view instead of the world location
                    gameObject.transform.localPosition = new Vector3(0, -1.8f, gameObject.transform.localPosition.z);
                    if (AmongUs.Data.DataManager.Settings.Gameplay.StreamerMode) gameObject.transform.localPosition = new Vector3(0, 2f, gameObject.transform.localPosition.z);
                }
            } else {
                if (HideNSeek.isWaitingTimer) {
                    HideNSeek.timerText.text = "<color=#0000cc>" + suffix + "</color>";
                    HideNSeek.timerText.color = Color.blue;
                } else {
                    HideNSeek.timerText.text = "<color=#FF0000FF>" + suffix + "</color>";
                    HideNSeek.timerText.color = Color.red;
                }
            }
            if (HideNSeek.isHunted() && !Hunted.taskPunish && !HideNSeek.isWaitingTimer) {
                var (playerCompleted, playerTotal) = TasksHandler.taskInfo(PlayerControl.LocalPlayer.Data);
                int numberOfTasks = playerTotal - playerCompleted;
                if (numberOfTasks == 0) {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareTimer, Hazel.SendOption.Reliable, -1);
                    writer.Write(HideNSeek.taskPunish);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.shareTimer(HideNSeek.taskPunish);

                    Hunted.taskPunish = true;
                }
            }

            if (!HideNSeek.isHunter()) return;

            byte playerId = PlayerControl.LocalPlayer.PlayerId;
            foreach (Arrow arrow in Hunter.localArrows) arrow.arrow.SetActive(false);
            if (Hunter.arrowActive) {
                int arrowIndex = 0;
                foreach (PlayerControl p in PlayerControl.AllPlayerControls) {
                    if (!p.Data.IsDead && !p.Data.Role.IsImpostor) {
                        if (arrowIndex >= Hunter.localArrows.Count) {
                            Hunter.localArrows.Add(new Arrow(Color.blue));
                        }
                        if (arrowIndex < Hunter.localArrows.Count && Hunter.localArrows[arrowIndex] != null) {
                            Hunter.localArrows[arrowIndex].arrow.SetActive(true);
                            Hunter.localArrows[arrowIndex].Update(p.transform.position, Color.blue);
                        }
                        arrowIndex++;
                    }
                }
            }
        }

        public static void Postfix(PlayerControl __instance) {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started || GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return;

            // Mini and Morphling shrink
            playerSizeUpdate(__instance);
            
            if (PlayerControl.LocalPlayer == __instance) {
                // Update player outlines
                setBasePlayerOutlines();
                // Update Role Description
                Helpers.refreshRoleDescription(PlayerControl.LocalPlayer);
                // Update Player Info
                updatePlayerInfo();
                // Update pet visibility
                setPetVisibility();

                // Time Master
                bendTimeUpdate();
                // Vampire
                Garlic.UpdateAll();
                // Impostor
                impostorSetTarget();
                // Morphling and Camouflager
                morphlingAndCamouflagerUpdate();
                // Deputy
                Deputy.handcuffUpdate();
                // Assassin
                AssassinTrace.UpdateAll();

                // -- MODIFIER--
                // Chameleon (invis stuff, timers)
                Chameleon.update();
                // Multitasker
                multitaskerUpdate();
                // mini (for the cooldowns)
                miniCooldownUpdate();
                // Yo-yo
                Silhouette.UpdateAll();
                // Radar
                radarUpdate();

                // Props
                accelTrapUpdate();
                decelTrapUpdate();

                // -- GAME MODE --
                hunterUpdate();
            }

            foreach (var role in new List<Role>(Role.allRoles)) {
                if (role.player == __instance) role.FixedUpdate();
            }
        }
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.WalkPlayerTo))]
    class PlayerPhysicsWalkPlayerToPatch {
        private static Vector2 offset = Vector2.zero;
        public static void Prefix(PlayerPhysics __instance) {
            bool correctOffset = Camouflager.camouflageTimer <= 0f && !Helpers.MushroomSabotageActive() && (__instance.myPlayer == Mini.mini || Morphling.players.Any(x => x.player == __instance.myPlayer && x.morphTimer > 0f && x.morphTarget == Mini.mini)
                 || (__instance.myPlayer.isRole(RoleId.MimicA) && MimicA.isMorph && Mini.mini.isRole(RoleId.MimicK)));
            correctOffset = correctOffset && !Morphling.players.Any(x => x.player == Mini.mini && x.morphTimer > 0f) && !(Mini.mini.isRole(RoleId.MimicA) && MimicA.isMorph) && !(Mini.mini.isRole(RoleId.MimicK) && MimicK.victim != null);
            if (correctOffset) {
                float currentScaling = (Mini.growingProgress() + 1) * 0.5f;
                __instance.myPlayer.Collider.offset = currentScaling * Mini.defaultColliderOffset * Vector2.down;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
    class PlayerControlReportDeadBodyPatch {
        public static bool Prefix(PlayerControl __instance) {
            if (HideNSeek.isHideNSeekGM) return false;
            Helpers.handleVampireBiteOnBodyReport();
            Helpers.HandleUndertakerDropOnBodyReport();
            Helpers.handleTrapperTrapOnBodyReport();
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.LocalPlayer.ReportDeadBody))]
    class BodyReportPatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] NetworkedPlayerInfo target)
        {
            if (Moriarty.brainwashed?.FindAll(x => x.PlayerId == __instance?.PlayerId).Count > 0)
            {
                return false;
            }
            return true;
        }

        public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)]NetworkedPlayerInfo target)
        {
            // Medic or Detective report
            bool isMedicReport = PlayerControl.LocalPlayer.isRole(RoleId.Medic) && Medic.allPlayers.Any(x => x.PlayerId == __instance.PlayerId);
            bool isDetectiveReport = PlayerControl.LocalPlayer.isRole(RoleId.Detective) && Detective.allPlayers.Any(x => x.PlayerId == __instance.PlayerId);
            if (isMedicReport || isDetectiveReport)
            {
                DeadPlayer deadPlayer = deadPlayers?.Where(x => x.player?.PlayerId == target?.PlayerId)?.FirstOrDefault();

                if (deadPlayer != null && deadPlayer.killerIfExisting != null) {
                    float timeSinceDeath = ((float)(DateTime.UtcNow - deadPlayer.timeOfDeath).TotalMilliseconds);
                    string msg = "";

                    if (isMedicReport) {
                        msg = string.Format(ModTranslation.getString("medicReport"), Math.Round(timeSinceDeath / 1000));
                    } else if (isDetectiveReport) {
                        Detective.local.acTokenChallenge.Value.reported = true;
                        Detective.local.acTokenChallenge.Value.killerId = deadPlayer.killerIfExisting.PlayerId;
                        if (timeSinceDeath < Detective.reportNameDuration * 1000) {
                            msg = string.Format(ModTranslation.getString("detectiveReportName"), deadPlayer.killerIfExisting.Data.PlayerName);
                            Detective.local.acTokenCommon.Value = true;
                        } else if (timeSinceDeath < Detective.reportColorDuration * 1000) {
                            var typeOfColor = Helpers.isLighterColor(deadPlayer.killerIfExisting.Data.DefaultOutfit.ColorId) ? ModTranslation.getString("detectiveColorLight") : ModTranslation.getString("detectiveColorDark");
                            msg = string.Format(ModTranslation.getString("detectiveReportColor"), typeOfColor);
                        } else {
                            msg = ModTranslation.getString("detectiveReportNone");
                        }
                    }

                    MeetingOverlayHolder.RegisterOverlay(TORGUIContextEngine.API.VerticalHolder(GUIAlignment.Left,
                    new TORGUIText(GUIAlignment.Left, TORGUIContextEngine.API.GetAttribute(AttributeAsset.OverlayTitle), new TranslateTextComponent(isDetectiveReport ? "detectiveReportInfo" : "medicReportInfo")),
                    new TORGUIText(GUIAlignment.Left, TORGUIContextEngine.API.GetAttribute(AttributeAsset.OverlayContent), new RawTextComponent($"{deadPlayer.player?.Data?.PlayerName}:\n" + msg)))
                    , MeetingOverlayHolder.IconsSprite[isMedicReport ? 3 : 1], isMedicReport ? Medic.color : Detective.color);

                    if (!string.IsNullOrWhiteSpace(msg))
                    {   
                        if (AmongUsClient.Instance.AmClient && FastDestroyableSingleton<HudManager>.Instance)
                        {
                            FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, msg, false);

                            // Ghost Info
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareGhostInfo, Hazel.SendOption.Reliable, -1);
                            writer.Write(PlayerControl.LocalPlayer.PlayerId);
                            writer.Write((byte)RPCProcedure.GhostInfoTypes.DetectiveOrMedicInfo);
                            writer.Write(msg);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                        }
                        /*if (msg.IndexOf("who", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            FastDestroyableSingleton<Assets.CoreScripts.Telemetry>.Instance.SendWho();
                        }*/
                    }
                }
            }  
        }
    }

    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.AssignRoleOnDeath))]
    public static class AssignRolePatch
    {
        static public bool blockAssignRole = true;
        public static bool Prefix([HarmonyArgument(0)] PlayerControl player, bool specialRolesAllowed)
        {
            if ((player.isRole(RoleId.SchrodingersCat) && !SchrodingersCat.hasTeam() && !SchrodingersCat.isExiled) || Busker.players.Any(x => x.player == player && x.pseudocideFlag) || FreePlayGM.isFreePlayGM)
                return false;
            if (TimeMaster.hasAlivePlayers && blockAssignRole && TimeMaster.reviveDuringReweind)
                return false;

            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    public static class MurderPlayerPatch
    {
        public static bool resetToCrewmate = false;
        public static bool resetToDead = false;

        public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)]PlayerControl target)
        {
            // Allow everyone to murder players
            resetToCrewmate = !__instance.Data.Role.IsImpostor;
            resetToDead = __instance.Data.IsDead;
            __instance.Data.Role.TeamType = RoleTeamTypes.Impostor;
            __instance.Data.IsDead = false;
        }

        public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)]PlayerControl target)
        {
            // Collect dead player info
            DeadPlayer deadPlayer = new(target, DateTime.UtcNow, DeadPlayer.CustomDeathReason.Kill, __instance);
            deadPlayers.Add(deadPlayer);

            // Reset killer to crewmate if resetToCrewmate
            if (resetToCrewmate) __instance.Data.Role.TeamType = RoleTeamTypes.Crewmate;
            if (resetToDead) __instance.Data.IsDead = true;

            // Remove fake tasks when player dies
            if (target.shouldClearTask())
                target.clearAllTasks();

            // First kill (set before lover suicide)
            if (TORMapOptions.firstKillName == "") TORMapOptions.firstKillName = target.Data.PlayerName;

            TORGameManager.Instance?.GameStatistics.RecordEvent(new(GameStatistics.EventVariation.Kill, __instance.PlayerId, 1 << target.PlayerId) { RelatedTag = EventDetail.Kill});

            // Pursuer promotion trigger on murder (the host sends the call such that everyone recieves the update before a possible game End)
            if (target == Lawyer.target && AmongUsClient.Instance.AmHost) {
                foreach (var lawyer in Lawyer.allPlayers)
                {
                    Lawyer.PromoteToPursuer.Invoke(lawyer.PlayerId);
                }
            }

            Helpers.HandleRoleFlashOnDeath(target);

            // Medium add body
            if (Medium.futureDeadBodies != null) {
                Medium.futureDeadBodies.Add(new Tuple<DeadPlayer, Vector3>(deadPlayer, target.transform.position));
            }

            if (PlayerControl.LocalPlayer == target && Helpers.CurrentMonth == 4 && Helpers.isSkeld())
                _ = new StaticAchievementToken("rainyStep");

            if (PlayerControl.LocalPlayer.isRole(RoleId.Seer)) {
                if (__instance.Data.Role.IsImpostor && __instance != target) Seer.local.acTokenAnother.Value.impKill = true;
                if (Seer.canSeeKillTeams)
                {
                    switch (__instance)
                    {
                        case var _ when __instance.Data.Role.IsImpostor:
                            Seer.killTeams.impostor++;
                            break;
                        case var _ when __instance.isRole(RoleId.Jackal) || __instance.isRole(RoleId.Sidekick) || (__instance.isRole(RoleId.SchrodingersCat) && SchrodingersCat.team == SchrodingersCat.Team.Jackal):
                            Seer.killTeams.jackal++;
                            break;
                        case var _ when __instance.isRole(RoleId.JekyllAndHyde) || (__instance.isRole(RoleId.SchrodingersCat) && SchrodingersCat.team == SchrodingersCat.Team.JekyllAndHyde):
                            Seer.killTeams.jekyllAndHyde++;
                            break;
                        case var _ when __instance.isRole(RoleId.Moriarty) || (__instance.isRole(RoleId.SchrodingersCat) && SchrodingersCat.team == SchrodingersCat.Team.Moriarty):
                            Seer.killTeams.moriarty++;
                            break;
                        case var _ when __instance.isRole(RoleId.Pelican):
                            Seer.killTeams.pelican++;
                            break;
                        case var _ when __instance.isRole(RoleId.Yandere) || (__instance.isRole(RoleId.SchrodingersCat) && SchrodingersCat.team == SchrodingersCat.Team.Yandere):
                            Seer.killTeams.yandere++;
                            break;
                        case var _ when !Helpers.isNeutral(__instance):
                            Seer.killTeams.crewmate++;
                            break;
                    }
                }
            }

            if (target.Data.Role.IsImpostor && AmongUsClient.Instance.AmHost)
                LastImpostor.promoteToLastImpostor();

            if (__instance == LastImpostor.lastImpostor && target != LastImpostor.lastImpostor) LastImpostor.killCounter++;


            if (PlayerControl.LocalPlayer.isRole(RoleId.Camouflager) && Camouflager.camouflageTimer > 0f)
            {
                Camouflager.acTokenChallenge.Value.kills++;
                Camouflager.acTokenChallenge.Value.cleared |= Camouflager.acTokenChallenge.Value.kills >= 3;
            }

            if (PlayerControl.LocalPlayer.isRole(RoleId.Tracker) && Tracker.local.tracked == target)
                Tracker.local.numShots++;

            if (PlayerControl.LocalPlayer.isRole(RoleId.Teleporter))
            {
                var teleportT = target.PlayerId == Teleporter.local.acTokenChallenge.Value.target1 ? Teleporter.local.acTokenChallenge.Value.target2 : Teleporter.local.acTokenChallenge.Value.target1;
                PlayerControl teleportTarget = null;
                if (teleportT != byte.MaxValue) teleportTarget = Helpers.playerById(teleportT);
                if (teleportTarget != null)
                {
                    if (target.PlayerId != teleportT && DateTime.UtcNow.Subtract(Teleporter.local.acTokenChallenge.Value.swapTime).TotalSeconds <= 4f)
                    {
                        if (Teleporter.local.acTokenChallenge.Value.target2 != byte.MaxValue && (teleportTarget.isRole(RoleId.Snitch) || teleportTarget.isRole(RoleId.Mayor) || teleportTarget.isRole(RoleId.NiceGuesser) || teleportTarget.isRole(RoleId.FortuneTeller)
                            || teleportTarget.isRole(RoleId.Sheriff)))
                            Teleporter.local.acTokenChallenge.Value.cleared = true;
                    }
                }
            }

            if (PlayerControl.LocalPlayer.isRole(RoleId.Trickster) && Trickster.lightsOutTimer > 0f)
            {
                Trickster.acTokenChallenge.Value.kills++;
                Trickster.acTokenChallenge.Value.cleared |= Trickster.acTokenChallenge.Value.kills >= 2;
            }

            // Evil Tracker see flash
            if (__instance.Data.Role.IsImpostor && __instance != PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.isRole(RoleId.EvilTracker) && !PlayerControl.LocalPlayer.Data.IsDead && EvilTracker.canSeeDeathFlash)
            {
                Helpers.showFlash(new Color(42f / 255f, 187f / 255f, 245f / 255f), message: ModTranslation.getString("evilTrackerInfo"));
            }

            if (__instance.Data.Role.IsImpostor && PlayerControl.LocalPlayer.isRole(RoleId.Spy))
            {
                if (target == PlayerControl.LocalPlayer)  _ = new StaticAchievementToken("spy.another1");
                else
                {
                    if (Helpers.isVisible(PlayerControl.LocalPlayer, target))
                        _ = new StaticAchievementToken("spy.challenge");
                }
            }

            if (__instance == PlayerControl.LocalPlayer && target.isRole(RoleId.Shifter) && Shifter.isNeutral && Shifter.pastShifters.Contains(PlayerControl.LocalPlayer.PlayerId))
                _ = new StaticAchievementToken("corruptedShifter.challenge");

            if (Sprinter.isSprinting(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer != target)
            {
                if (Helpers.isVisible(PlayerControl.LocalPlayer, target))
                    _ = new StaticAchievementToken("sprinter.challenge");
            }

            if (Helpers.isVisible(PlayerControl.LocalPlayer, target))
            {
                if (PlayerControl.LocalPlayer.isRole(RoleId.NiceWatcher))
                {
                    NiceWatcher.local.canKill = true;
                    HudManagerStartPatch.watcherKillButton.Timer = 5f;
                }
                else if (PlayerControl.LocalPlayer.isRole(RoleId.EvilWatcher) && __instance != PlayerControl.LocalPlayer)
                {
                    if (PlayerControl.LocalPlayer.killTimer > 1f)
                        PlayerControl.LocalPlayer.SetKillTimerUnchecked(1f);
                    else {
                        EvilWatcher.local.extraKill = true;
                    }
                }
            }

            __instance.OnKill(target);

            // Sherlock record log
            Sherlock.killLog.Add(Tuple.Create(__instance.PlayerId, Tuple.Create(target.PlayerId, target.transform.position + Vector3.zero)));

            target.OnDeath(__instance);

            // Mini Set Impostor Mini kill timer (Due to mini being a modifier, all "SetKillTimers" must have happened before this!)
            if (Mini.mini != null && __instance == Mini.mini && __instance == PlayerControl.LocalPlayer) {
                float multiplier = 1f;
                if (Mini.mini != null && PlayerControl.LocalPlayer == Mini.mini) multiplier = Mini.isGrownUp() ? 0.66f : 2f;
                Mini.mini.SetKillTimer(__instance.killTimer * multiplier);
            }

            if (PlayerControl.LocalPlayer == __instance && Diseased.diseased.Any(x => x.PlayerId == target.PlayerId))
            {
                HudManagerStartPatch.jackalKillButton.Timer = Jackal.cooldown * Diseased.multiplier;
                HudManagerStartPatch.sheriffKillButton.Timer = Sheriff.cooldown * Diseased.multiplier;
                HudManagerStartPatch.vampireKillButton.Timer = Vampire.cooldown * Diseased.multiplier;
                HudManagerStartPatch.sidekickKillButton.Timer = Sidekick.cooldown * Diseased.multiplier;
                HudManagerStartPatch.schrodingersCatKillButton.Timer = SchrodingersCat.killCooldown * Diseased.multiplier;
                PlayerControl.LocalPlayer.SetKillTimerUnchecked(__instance.killTimer * Diseased.multiplier);
            }

            // Add Bloody Modifier
            if (Bloody.bloody.Any(x => x.PlayerId == target.PlayerId)) {
                Bloodytrail.StartBloodTrail(__instance, target);
                if (PlayerControl.LocalPlayer == __instance && Helpers.CurrentMonth == 8 && Helpers.isFungle()) {
                    _ = new StaticAchievementToken("watermelon");
                }
            }

            // HideNSeek
            if (HideNSeek.isHideNSeekGM) {
                int visibleCounter = 0;
                Vector3 bottomLeft = IntroCutsceneOnDestroyPatch.bottomLeft + new Vector3(-0.25f, -0.25f, 0);
                foreach (PlayerControl p in PlayerControl.AllPlayerControls) {
                    if (!TORMapOptions.playerIcons.ContainsKey(p.PlayerId) || p.Data.Role.IsImpostor) continue;
                    if (p.Data.IsDead || p.Data.Disconnected) {
                        TORMapOptions.playerIcons[p.PlayerId].gameObject.SetActive(false);
                    }
                    else {
                        TORMapOptions.playerIcons[p.PlayerId].transform.localPosition = bottomLeft + Vector3.right * visibleCounter * 0.35f;
                        visibleCounter++;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Revive))]
    class RevivePatch
    {
        public static bool Prefix(PlayerControl __instance)
        {
            __instance.Data.IsDead = false;
            __instance.gameObject.layer = LayerMask.NameToLayer("Players");
            __instance.MyPhysics.ResetMoveState(true);
            __instance.clickKillCollider.enabled = true;
            __instance.cosmetics.SetPetSource(__instance);
            __instance.cosmetics.SetNameMask(true);
            if (__instance.AmOwner)
            {
                DestroyableSingleton<HudManager>.Instance.ShadowQuad.gameObject.SetActive(true);
                DestroyableSingleton<HudManager>.Instance.SetHudActive(true);
                DestroyableSingleton<HudManager>.Instance.Chat.ForceClosed();
                DestroyableSingleton<HudManager>.Instance.Chat.SetVisible(false);
            }
            DeadPlayer deadPlayerEntry = deadPlayers.Where(x => x.player.PlayerId == __instance.PlayerId).FirstOrDefault();
            if (deadPlayerEntry != null) deadPlayers.Remove(deadPlayerEntry);
            TORGameManager.Instance?.GameStatistics.RecordEvent(new(GameStatistics.EventVariation.Revive, null, 1 << __instance.PlayerId) { RelatedTag = EventDetail.Revive });
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
    static class PlayerControlSetCoolDownPatch {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)]float time) {
            if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return true;
            if (PlayerControl.LocalPlayer.GetKillCooldown() <= 0f) return false;
            float multiplier = 1f;
            float addition = 0f;
            if (Mini.mini != null && PlayerControl.LocalPlayer == Mini.mini) multiplier = Mini.isGrownUp() ? 0.66f : 2f;
            if (PlayerControl.LocalPlayer.isRole(RoleId.BountyHunter)) addition = BountyHunter.punishmentTime;
            if (PlayerControl.LocalPlayer.isRole(RoleId.Trickster)) addition = Trickster.boxKillPenalty;
            if (Ninja.isPenalized(PlayerControl.LocalPlayer)) addition = Ninja.killPenalty;

            float max = Mathf.Max(PlayerControl.LocalPlayer.GetKillCooldown() * multiplier + addition, __instance.killTimer);
            __instance.SetKillTimerUnchecked(Mathf.Clamp(time, 0f, max), max);
            return false;
        }

        public static void SetKillTimerUnchecked(this PlayerControl player, float time, float max = float.NegativeInfinity)
        {
            if (max == float.NegativeInfinity) max = time;
            player.killTimer = time;
            FastDestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(time, max);
        }
    }

    [HarmonyPatch(typeof(KillAnimation._CoPerformKill_d__2), "MoveNext")]
    class KillAnimationCoPerformKillPatch {
        public static bool hideNextAnimation = false;
        public static void Prefix(KillAnimation._CoPerformKill_d__2 __instance) {
            if (hideNextAnimation)
                __instance.source = __instance.target;
            hideNextAnimation = false;
        }
    }

    [HarmonyPatch(typeof(KillAnimation), nameof(KillAnimation.SetMovement))]
    class KillAnimationSetMovementPatch {
        private static int? colorId = null;
        public static void Prefix(PlayerControl source, bool canMove) {
            Color color = source.cosmetics.currentBodySprite.BodySprite.material.GetColor("_BodyColor");
            if (Morphling.players.Any(x => x.player.PlayerId == source.Data.PlayerId)) {
                var index = Palette.PlayerColors.IndexOf(color);
                if (index != -1) colorId = index;
            }
        }

        public static void Postfix(PlayerControl source, bool canMove) {
            if (colorId.HasValue) source.RawSetColor(colorId.Value);
            colorId = null;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
    public static class ExilePlayerPatch
    {
        public static void Postfix(PlayerControl __instance)
        {
            // Collect dead player info
            DeadPlayer deadPlayer = new(__instance, DateTime.UtcNow, DeadPlayer.CustomDeathReason.Exile, null);
            GameHistory.deadPlayers.Add(deadPlayer);


            // Remove fake tasks when player dies
            if (__instance.shouldClearTask())
                __instance.clearAllTasks();

            __instance.OnDeath(killer: null);

            if (__instance.Data.Role.IsImpostor && AmongUsClient.Instance.AmHost)
                LastImpostor.promoteToLastImpostor();

            // Pursuer promotion trigger on exile & suicide (the host sends the call such that everyone recieves the update before a possible game End)
            if (__instance == Lawyer.target) {
                if (AmongUsClient.Instance.AmHost && ((!Lawyer.target.isRole(RoleId.Jester)) || Lawyer.targetWasGuessed)) {
                    foreach (var lawyer in Lawyer.allPlayers)
                    {
                        Lawyer.PromoteToPursuer.Invoke(lawyer.PlayerId);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
    public static class PlayerPhysicsFixedUpdate {
        public static void Postfix(PlayerPhysics __instance)
        {
            bool shouldInvert = Invert.invert.FindAll(x => x.PlayerId == PlayerControl.LocalPlayer.PlayerId).Count > 0 && Invert.meetings > 0;  // xor. if already invert, eventInvert will turn it off for 10s
            if (__instance.AmOwner &&
                AmongUsClient.Instance &&
                AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started &&
                !PlayerControl.LocalPlayer.Data.IsDead && 
                shouldInvert && 
                GameData.Instance && 
                __instance.myPlayer.CanMove)  
                __instance.body.velocity *= -1;

            if (__instance.AmOwner &&
                AmongUsClient.Instance &&
                AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started &&
                !PlayerControl.LocalPlayer.Data.IsDead &&
                GameData.Instance &&
                __instance.myPlayer.CanMove &&
                PlayerControl.LocalPlayer.isRole(RoleId.Undertaker) &&
                Undertaker.DraggedBody != null)
            {
                __instance.body.velocity *= 1f + Undertaker.speedDecrease / 100f;
            }

            Kataomoi.fixedUpdate(__instance);
        }
    }
    
    [HarmonyPatch(typeof(GameData), nameof(GameData.HandleDisconnect), [typeof(PlayerControl), typeof(DisconnectReasons)])]
    public static class GameDataHandleDisconnectPatch {
        public static void Prefix(GameData __instance, PlayerControl player, DisconnectReasons reason) {
            if (MeetingHud.Instance) {
                MeetingHudPatch.swapperCheckAndReturnSwap(MeetingHud.Instance, player.PlayerId);
                MeetingHudPatch.yasunaCheckAndReturnSpecialVote(MeetingHud.Instance, player.PlayerId);
            }
            if (AmongUsClient.Instance && AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started) {
                TORGameManager.Instance?.GameStatistics.RecordEvent(new(GameStatistics.EventVariation.Disconnect, player.PlayerId, 0) { RelatedTag = EventDetail.Disconnect });
                Role.allRoles.Do(x => x.HandleDisconnect(player, reason));
                Lovers.HandleDisconnect(player, reason);
            }
            if (RoleDraft.isEnabled && RoleDraft.isRunning) {
                if (RoleDraft.pickOrder != null && RoleDraft.pickOrder.Count > 0) {
                    RoleDraft.pickOrder.Remove(player.PlayerId);
                    if (player.PlayerId == RoleDraft.pickOrder[0]) {
                        RoleDraft.timer = 0;
                        RoleDraft.picked = true;
                    }
                }
            }
        }
        public static void Postfix(GameData __instance, PlayerControl player, DisconnectReasons reason)
        {
            if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
            {
                if (AmongUsClient.Instance.AmHost && player != null && player.Data?.Role?.IsImpostor == true) LastImpostor.promoteToLastImpostor();
            }
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.ExitGame))]
    public static class LocalPlayerExitPatch
    {
        public static void Postfix()
        {
            Props.clearProps();
            if (RoleInfo.schrodingersCat != null)
            {
                RoleInfo.schrodingersCat.color = SchrodingersCat.color;
                RoleInfo.schrodingersCat.isNeutral = true;
            }
        }
    }
}
