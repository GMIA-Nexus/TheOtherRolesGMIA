using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using static TheOtherRoles.TheOtherRoles;
using TheOtherRoles.Objects;
using System;
using TheOtherRoles.Utilities;
using UnityEngine;
using TheOtherRoles.Modules;
using TheOtherRoles.Roles;

namespace TheOtherRoles.Patches {
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.BeginForGameplay))]
    [HarmonyPriority(Priority.First)]
    class ExileControllerBeginPatch {
        public static void Prefix(ExileController __instance, [HarmonyArgument(0)] ref NetworkedPlayerInfo exiled) {
            // Medic shield
            if (AmongUsClient.Instance.AmHost) {
                foreach (var medic in Medic.players) {
                    if (medic.futureShielded != null && !medic.player.Data.IsDead) { // We need to send the RPC from the host here, to make sure that the order of shifting and setting the shield is correct(for that reason the futureShifted and futureShielded are being synced)
                        Medic.Shield.Invoke((medic.futureShielded.PlayerId, medic.player.PlayerId));
                    }
                    if (medic.usedShield) medic.meetingAfterShielding = true;  // Has to be after the setting of the shield
                }
            }

            // Shifter shift
            if (Shifter.allPlayers.Count > 0 && AmongUsClient.Instance.AmHost && Shifter.futureShift != null) { // We need to send the RPC from the host here, to make sure that the order of shifting and erasing is correct (for that reason the futureShifted and futureErased are being synced)
                PlayerControl oldShifter = Shifter.allPlayers.FirstOrDefault();
                byte oldTaskMasterPlayerId = Shifter.futureShift.isRole(RoleId.TaskMaster) && TaskMaster.isTaskComplete ? Shifter.futureShift.PlayerId : byte.MaxValue;
                Shifter.Shift.Invoke(Shifter.futureShift.PlayerId);

                if (oldShifter.isRole(RoleId.TaskMaster))
                {
                    byte clearTasks = 0;
                    for (int i = 0; i < oldShifter.Data.Tasks.Count; ++i)
                    {
                        if (oldShifter.Data.Tasks[i].Complete)
                            ++clearTasks;
                    }
                    bool allTasksCompleted = clearTasks == oldShifter.Data.Tasks.Count;
                    byte[] taskTypeIds = allTasksCompleted ? TaskMasterTaskHelper.GetTaskMasterTasks(oldShifter) : null;
                    MessageWriter writer2 = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.TaskMasterSetExTasks, Hazel.SendOption.Reliable, -1);
                    writer2.Write(oldShifter.PlayerId);
                    writer2.Write(oldTaskMasterPlayerId);
                    if (taskTypeIds != null)
                        writer2.Write(taskTypeIds);
                    AmongUsClient.Instance.FinishRpcImmediately(writer2);
                    RPCProcedure.taskMasterSetExTasks(oldShifter.PlayerId, oldTaskMasterPlayerId, taskTypeIds);
                }
            }
            Shifter.futureShift = null;

            // Eraser erase
            if (Eraser.allPlayers.Count > 0 && AmongUsClient.Instance.AmHost && Eraser.futureErased != null) {  // We need to send the RPC from the host here, to make sure that the order of shifting and erasing is correct (for that reason the futureShifted and futureErased are being synced)
                foreach (PlayerControl target in Eraser.futureErased) {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ErasePlayerRoles, Hazel.SendOption.Reliable, -1);
                    writer.Write(target.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.erasePlayerRoles(target.PlayerId);
                    Eraser.alreadyErased.Add(target.PlayerId);
                }
            }
            Eraser.futureErased = new List<PlayerControl>();

            // Trickster boxes
            if (Trickster.allPlayers.Count > 0 && JackInTheBox.hasJackInTheBoxLimitReached()) {
                JackInTheBox.convertToVents();
                if (PlayerControl.LocalPlayer.isRole(RoleId.Trickster))
                    _ = new StaticAchievementToken("trickster.common1");
            }

            // Activate portals.
            Portal.meetingEndsUpdate();            

            // Witch execute casted spells
            if (Witch.allPlayers.Count > 0) {
                foreach (var witch in Witch.players)
                {
                    if (AmongUsClient.Instance.AmHost)
                    {
                        if (witch.player == null || witch.futureSpelled == null) continue;
                        bool exiledIsWitch = exiled != null && exiled.PlayerId == witch.player.PlayerId;
                        bool witchDiesWithExiledLover = exiled != null && Lovers.bothDie && exiled.Object.isLovers() && exiled.Object.getPartner() == witch.player;

                        if ((witchDiesWithExiledLover || exiledIsWitch) && Witch.witchVoteSavesTargets) witch.futureSpelled = [];
                        foreach (PlayerControl target in witch.futureSpelled) {
                            if (target != null && !target.Data.IsDead && Helpers.checkMuderAttempt(witch.player, target, true) == MurderAttemptResult.PerformKill){
                                if (target == Lawyer.target) {
                                    foreach (var lawyer in Lawyer.allPlayers)
                                        Lawyer.PromoteToPursuer.Invoke(lawyer.PlayerId);
                                }
                                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedExilePlayer, Hazel.SendOption.Reliable, -1);
                                writer.Write(target.PlayerId);
                                AmongUsClient.Instance.FinishRpcImmediately(writer);
                                RPCProcedure.uncheckedExilePlayer(target.PlayerId);

                                MessageWriter writer3 = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareGhostInfo, Hazel.SendOption.Reliable, -1);
                                writer3.Write(PlayerControl.LocalPlayer.PlayerId);
                                writer3.Write((byte)RPCProcedure.GhostInfoTypes.DeathReasonAndKiller);
                                writer3.Write(target.PlayerId);
                                writer3.Write((byte)DeadPlayer.CustomDeathReason.WitchExile);
                                writer3.Write(witch.player.PlayerId);
                                AmongUsClient.Instance.FinishRpcImmediately(writer3);

                                GameHistory.overrideDeathReasonAndKiller(target, DeadPlayer.CustomDeathReason.WitchExile, killer: witch.player);
                            }
                        }
                    }

                    witch.futureSpelled = [];
                }
            }

            // SecurityGuard vents and cameras
            var allCameras = MapUtilities.CachedShipStatus.AllCameras.ToList();
            TORMapOptions.camerasToAdd.ForEach(camera => {
                camera.gameObject.SetActive(true);
                camera.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
                allCameras.Add(camera);
            });
            MapUtilities.CachedShipStatus.AllCameras = allCameras.ToArray();
            TORMapOptions.camerasToAdd = new List<SurvCamera>();

            foreach (Vent vent in TORMapOptions.ventsToSeal) {
                PowerTools.SpriteAnim animator = vent.GetComponent<PowerTools.SpriteAnim>(); 
                vent.EnterVentAnim = vent.ExitVentAnim = null;
                Sprite newSprite = animator == null ? SecurityGuard.getStaticVentSealedSprite() : SecurityGuard.getAnimatedVentSealedSprite();
                SpriteRenderer rend = vent.myRend;
                if (Helpers.isFungle())
                {
                    newSprite = SecurityGuard.getFungleVentSealedSprite();
                    rend = vent.transform.GetChild(3).GetComponent<SpriteRenderer>();
                    animator = vent.transform.GetChild(3).GetComponent<PowerTools.SpriteAnim>();
                }
                animator?.Stop();
                rend.sprite = newSprite;
                if (SubmergedCompatibility.IsSubmerged && vent.Id == 0) vent.myRend.sprite = SecurityGuard.getSubmergedCentralUpperSealedSprite();
                if (SubmergedCompatibility.IsSubmerged && vent.Id == 14) vent.myRend.sprite = SecurityGuard.getSubmergedCentralLowerSealedSprite();
                rend.color = Color.white;
                vent.name = "SealedVent_" + vent.name;
            }
            TORMapOptions.ventsToSeal = new List<Vent>();

            EventUtility.meetingEndsUpdate();
        }        
    }

    [HarmonyPatch]
    class ExileControllerWrapUpPatch {

        [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
        class BaseExileControllerPatch {
            public static void Postfix(ExileController __instance) {
                NetworkedPlayerInfo networkedPlayer = __instance.initData.networkedPlayer;
                WrapUpPostfix((networkedPlayer != null) ? networkedPlayer.Object : null);
            }
        }

        [HarmonyPatch(typeof(AirshipExileController._WrapUpAndSpawn_d__11), "MoveNext")]
        class AirshipExileControllerPatch {
            public static void Postfix(AirshipExileController._WrapUpAndSpawn_d__11 __instance) {
                NetworkedPlayerInfo networkedPlayer = __instance.__4__this.initData.networkedPlayer;
                WrapUpPostfix((networkedPlayer != null) ? networkedPlayer.Object : null);
            }
        }

        // Workaround to add a "postfix" to the destroying of the exile controller (i.e. cutscene) and SpwanInMinigame of submerged
        [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Destroy), [typeof(GameObject)])]
        public static void Prefix(GameObject obj) {
            // Nightvision:
            if (obj != null && obj.name != null && obj.name.Contains("FungleSecurity"))
            {
                SurveillanceMinigamePatch.resetNightVision();
                return;
            }

            if (!SubmergedCompatibility.IsSubmerged) return;
            if (obj.name.Contains("ExileCutscene")) {
                WrapUpPostfix(obj.GetComponent<ExileController>().initData.networkedPlayer?.Object);
            } else if (obj.name.Contains("SpawnInMinigame")) {
                AntiTeleport.setPosition();
                Chameleon.lastMoved.Clear();
            }
        }

        static void WrapUpPostfix(PlayerControl exiled)
        {
            TORGameManager.Instance?.GameStatistics.RecordEvent(new(GameStatistics.EventVariation.MeetingEnd, null, 0) { RelatedTag = EventDetail.MeetingEnd });
            if (exiled != null)
                TORGameManager.Instance?.GameStatistics.RecordEvent(new(GameStatistics.EventVariation.Exile, null, 1 << exiled.PlayerId) { RelatedTag = EventDetail.Exiled });
            
            // Prosecutor win condition
            /*if (exiled != null && Lawyer.lawyer != null && Lawyer.target != null && Lawyer.isProsecutor && Lawyer.target.PlayerId == exiled.PlayerId && !Lawyer.lawyer.Data.IsDead)
                Lawyer.triggerProsecutorWin = true;*/

            // Mini exile lose condition
            if (exiled != null && Mini.mini != null && Mini.mini.PlayerId == exiled.PlayerId && !Mini.isGrownUp() && !Mini.mini.Data.Role.IsImpostor && !RoleInfo.getRoleInfoForPlayer(Mini.mini).Any(x => x.isNeutral)) {
                Mini.triggerMiniLose = true;
            }
            // Jester win condition
            else if (exiled != null && Jester.exists && exiled.isRole(RoleId.Jester)) {
                Jester.TriggerWin.Invoke(exiled.PlayerId);
            }


            // Reset custom button timers where necessary
            CustomButton.MeetingEndedUpdate();

            foreach (var role in new List<Role>(Role.allRoles)) role.OnMeetingEnd(exiled);

            // Mini set adapted cooldown
            if (Mini.mini != null && PlayerControl.LocalPlayer == Mini.mini && Mini.mini.Data.Role.IsImpostor) {
                var multiplier = Mini.isGrownUp() ? 0.66f : 2f;
                Mini.mini.SetKillTimer(PlayerControl.LocalPlayer.GetKillCooldown() * multiplier);
            }

            if (Antique.antiques != null && Antique.antiques.Count > 0) {
                if (Archaeologist.revealAntique == Archaeologist.RevealAntique.AfterMeeting) {
                    var revealed = Antique.antiques.Where(x => x.isBroken).ToList();
                    foreach (var a in revealed) {
                        a.revealAntique();
                    }
                }
            }

            // Reset Yasuna settings.
            Yasuna.specialVoteTargetPlayerId = byte.MaxValue;

            // Tracker reset deadBodyPositions
            Tracker.deadBodyPositions = [];

            // AntiTeleport set position
            AntiTeleport.setPosition();

            // Remove DeadBodys
            DeadBody[] array = UnityEngine.Object.FindObjectsOfType<DeadBody>();
            for (int i = 0; i < array.Length; i++)
            {
                UnityEngine.Object.Destroy(array[i].gameObject);
            }

            if (exiled != null && !PlayerControl.LocalPlayer.Data.IsDead && Helpers.CurrentMonth == 3)
                _ = new StaticAchievementToken("graduation");

            MapBehaviourPatch.resetRealTasks();

            if (CustomOptionHolder.randomGameStartPosition.getBool() && (AntiTeleport.antiTeleport.FindAll(x => x.PlayerId == PlayerControl.LocalPlayer.PlayerId).Count == 0))
            { //Random spawn on round start
                var spawnLoc = MapData.GetCurrentMapData()?.SpawnPos ?? [];
                if (spawnLoc.Length > 0) PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(spawnLoc[rnd.Next(spawnLoc.Length)]);
            }

            if (CustomOptionHolder.activateProps.getBool() && !CustomGameModes.FreePlayGM.isFreePlayGM) Props.placeProps();

            // Invert add meeting
            if (Invert.meetings > 0) Invert.meetings--;

            Chameleon.lastMoved?.Clear();

            /*foreach (Trap trap in Trap.traps) trap.triggerable = false;
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown / 2 + 2, new Action<float>((p) => {
            if (p == 1f) foreach (Trap trap in Trap.traps) trap.triggerable = true;
            })));*/

            if (!Yoyo.markStaysOverMeeting)
                Silhouette.clearSilhouettes();

            AssignRolePatch.blockAssignRole = true;
        }
    }

    [HarmonyPatch(typeof(SpawnInMinigame), nameof(SpawnInMinigame.Close))]  // Set position of AntiTp players AFTER they have selected a spawn.
    class AirshipSpawnInPatch {
        static void Postfix() {
            AntiTeleport.setPosition();
            Chameleon.lastMoved.Clear();
        }
    }

    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), [typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>)])]
    class ExileControllerMessagePatch {
        static void Postfix(ref string __result, [HarmonyArgument(0)]StringNames id) {
            try {
                if (ExileController.Instance != null && ExileController.Instance.initData != null)
                {
                    PlayerControl player = ExileController.Instance.initData.networkedPlayer.Object;
                    if (player == null) return;
                    // Exile role text
                    if (id is StringNames.ExileTextPN or StringNames.ExileTextSN or StringNames.ExileTextPP or StringNames.ExileTextSP) {
                        __result = player.Data.PlayerName + " was The " + String.Join(" ", RoleInfo.getRoleInfoForPlayer(player, false, includeHidden: true).Select(x => x.name).ToArray());
                    }
                    // Hide number of remaining impostors on Jester win
                    if (id is StringNames.ImpostorsRemainP or StringNames.ImpostorsRemainS) {
                        if (player.isRole(RoleId.Jester)) __result = "";
                    }
                    if (Yasuna.specialVoteTargetPlayerId != byte.MaxValue)
                    {
                        if (CustomOptionHolder.yasunaSpecificMessageMode.getBool()) __result += ModTranslation.getString("yasunaSpecialIndicator");
                        Tiebreaker.isTiebreak = false;
                        Yasuna.specialVoteTargetPlayerId = byte.MaxValue;
                    }
                    if (Tiebreaker.isTiebreak) __result += ModTranslation.getString("tiebreakerSpecialIndicator");
                    Tiebreaker.isTiebreak = false;
                }
            } catch {
                // pass - Hopefully prevent leaving while exiling to softlock game
            }
        }
    }
}
