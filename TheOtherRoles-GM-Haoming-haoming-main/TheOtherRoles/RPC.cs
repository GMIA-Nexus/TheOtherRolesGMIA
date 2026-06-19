using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using TheOtherRoles.Modules;
using TheOtherRoles.Objects;
using TheOtherRoles.Patches;
using UnityEngine;
using static TheOtherRoles.GameHistory;
using static TheOtherRoles.HudManagerStartPatch;
using static TheOtherRoles.TORMapOptions;
using static TheOtherRoles.TheOtherRoles;
using static TheOtherRoles.TheOtherRolesGM;
using AmongUs.GameOptions;
using Assets.CoreScripts;
using InnerNet;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

namespace TheOtherRoles
{
    enum CustomRPC
    {
        // Main Controls

        ResetVariables = 100,
        FinishResetVariables,
        ShareOptions,
        SetRole,
        FinishSetRole,
        SetLovers,
        SetCupidLovers,
        VersionHandshake,
        UseUncheckedVent,
        UncheckedMurderPlayer,
        UncheckedCmdReportDeadBody,
        OverrideNativeRole,
        UncheckedExilePlayer,
        UncheckedEndGame,
        UncheckedSetTasks,
        DynamicMapOption,
        FinishShipStatusBegin,
        StopStart,
        StopStartSound,

        // Role functionality

        EngineerFixLights = 140,// 因为130-144をSubmergedが使用する所以要跳过
        EngineerFixSubmergedOxygen,
        EngineerUsedRepair,
        CleanBody,
        SheriffKill,
        MedicSetShielded,
        ShieldedMurderAttempt,
        TimeMasterShield,
        TimeMasterRewindTime,
        ShifterShift,
        SwapperSwap,
        MorphlingMorph,
        CamouflagerCamouflage,
        TrackerUsedTracker,
        VampireSetBitten,
        PlaceGarlic,
        EvilHackerCreatesMadmate,
        JackalCreatesSidekick,
        SidekickPromotes,
        ErasePlayerRoles,
        SetFutureErased,
        SetFutureShifted,
        SetFutureShielded,
        SetFutureSpelled,
        WitchSpellCast,
        PlaceJackInTheBox,
        LightsOut,
        PlaceCamera,
        SealVent,
        ArsonistWin,
        GuesserShoot,
        VultureWin,
        LawyerWin,
        LawyerSetTarget,
        LawyerPromotesToPursuer,
        SetBlanked, // 175

        // GM Edition functionality
        AddModifier = 180,
        NinjaStealth,
        SetShifterType,
        GMKill,
        GMRevive,
        UseAdminTime,
        UseCameraTime,
        UseVitalsTime,
        ArsonistDouse,
        VultureEat,
        PlagueDoctorWin,
        PlagueDoctorSetInfected,
        PlagueDoctorUpdateProgress,
        NekoKabochaExile,
        SerialKillerSuicide,
        FortuneTellerUsedDivine,
        FoxStealth,
        FoxCreatesImmoralist,
        SwapperAnimate,
        ImpostorPromotesToLastImpostor,
        SchrodingersCatSuicide,
        SchrodingersCatSetTeam,
        PlaceTrap,
        ClearTrap,
        ActivateTrap,
        DisableTrap,
        TrapperKill,
        TrapperMeetingFlag,
        RandomSpawn,
        PlantBomb,
        ReleaseBomb,
        BomberKill,
        SpawnDummy,
        WalkDummy,
        MoveDummy,
        PuppeteerStealth,
        PuppeteerMorph,
        PuppeteerWin,
        PuppeteerKill,
        PuppeteerClimbRadder,
        PuppeteerUsePlatform,
        mimicMorph,
        mimicResetMorph,
        Synchronize,
        PlaceAssassinTrace,
        SetOddIsJekyll,
        ShareRealTasks,
        WorkaroundSetRoles,
        SyncKillTimer,
        AkujoSetHonmei,
        AkujoSetKeep,
        AkujoSuicide,
        SetBrainwash,
        MoriartyKill,
        CupidSuicide,
        SetCupidShield,
        YoyoMarkLocation,
        YoyoBlink,
    }

    public static class RPCProcedure
    {

        // Main Controls

        public static void resetVariables()
        {
            Garlic.clearGarlics();
            JackInTheBox.clearJackInTheBoxes();
            TORMapOptions.clearAndReloadTORMapOptions();
            TheOtherRoles.clearAndReloadRoles();
            GameHistory.clearGameHistory();
            setCustomButtonCooldowns();
            AdminPatch.ResetData();
            CameraPatch.ResetData();
            VitalsPatch.ResetData();
            MapBehaviorPatch.reset();
            CustomOverlays.resetOverlays();
            SpecimenVital.clearAndReload();
            AdditionalVents.clearAndReload();
            BombEffect.clearBombEffects();
            Trap.clearAllTraps();
            AssassinTrace.clearTraces();
            SpawnInMinigamePatch.reset();
            MapBehaviorPatch.resetRealTasks();
            CustomNormalPlayerTask.reset();
            Shrine.reset();
            Silhouette.clearSilhouettes();
            GameStartManagerPatch.GameStartManagerUpdatePatch.startingTimer = 0;

            KillAnimationCoPerformKillPatch.hideNextAnimation = false;

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.FinishResetVariables, Hazel.SendOption.Reliable, -1);
            writer.Write(PlayerControl.LocalPlayer.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.finishResetVariables(PlayerControl.LocalPlayer.PlayerId);


        }

        public static void finishResetVariables(byte playerId)
        {
            var checkList = RoleAssignmentPatch.AmongUsClientCoStartGameHostPatch.checkList;
            if (checkList != null)
            {
                if (checkList.ContainsKey(playerId))
                {
                    checkList[playerId] = true;
                }
            }
        }

        public static void finishShipStatusBegin()
        {
            PlayerControl.LocalPlayer.OnFinishShipStatusBegin();
        }

        public static void ShareOptions(int numberOfOptions, MessageReader reader)
        {
            try
            {
                for (int i = 0; i < numberOfOptions; i++)
                {
                    uint optionId = reader.ReadPackedUInt32();
                    uint selection = reader.ReadPackedUInt32();
                    CustomOption option = CustomOption.options.FirstOrDefault(option => option.id == (int)optionId);
                    option.updateSelection((int)selection, i == numberOfOptions - 1);
                }
            }
            catch (Exception e)
            {
                TheOtherRolesPlugin.Logger.LogError("Error while deserializing options: " + e.Message);
            }
        }

        public static void stopStart(byte playerId)
        {
            if (AmongUsClient.Instance.AmHost && CustomOptionHolder.anyPlayerCanStopStart.getBool())
            {
                GameStartManager.Instance.ResetStartState();
                PlayerControl.LocalPlayer.RpcSendChat(string.Format(ModTranslation.getString("stopStartMessage"), Helpers.playerById(playerId).Data.PlayerName));
            }
        }
        public static void stopStartSound()
        {
            SoundManager.Instance.StopSound(GameStartManager.Instance.gameStartSound);
        }

        public static void workaroundSetRoles(byte numberOfRoles, MessageReader reader)
        {
            for (int i = 0; i < numberOfRoles; i++)
            {
                byte playerId = (byte)reader.ReadPackedUInt32();
                byte roleId = (byte)reader.ReadPackedUInt32();
                try
                {
                    setRole(roleId, playerId);
                }
                catch (Exception e)
                {
                    TheOtherRolesPlugin.Logger.LogError("Error while deserializing roles: " + e.Message);
                }
            }

        }

        public static void setRole(byte roleId, byte playerId)
        {
            Patches.Logger.info($"{GameData.Instance.GetPlayerById(playerId).PlayerName}({playerId}): {Enum.GetName(typeof(RoleType), roleId)}", "setRole");
            PlayerControl.AllPlayerControls.ToArray().ToArray().DoIf(
                x => x.PlayerId == playerId,
                x => x.setRole((RoleType)roleId)
            );
        }

        public static void finishSetRole()
        {
            RoleAssignmentPatch.isAssigned = true;
        }

        public static void addModifier(byte modId, byte playerId)
        {
            PlayerControl.AllPlayerControls.ToArray().ToArray().DoIf(
                x => x.PlayerId == playerId,
                x => x.addModifier((ModifierType)modId)
            );
        }

        public static void setLovers(byte playerId1, byte playerId2)
        {
            Lovers.addCouple(Helpers.playerById(playerId1), Helpers.playerById(playerId2));
        }
        public static void setCupidLovers(byte playerId1, byte playerId2, byte cupidId)
        {
            var p1 = Helpers.playerById(playerId1);
            var p2 = Helpers.playerById(playerId2);
            var cupid = Cupid.allRoles.FirstOrDefault(x => x.player.PlayerId == cupidId) as Cupid;
            cupid.lovers1 = p1;
            cupid.lovers2 = p2;
            Cupid.breakCouple(p1, p2);
            Cupid.breakCouple(p2, p1);
            Lovers.addCouple(p1, p2);
        }
        public static void setCupidShield(byte cupidId, byte targetId)
        {
            var cupid = Cupid.players.FirstOrDefault(x => x.player.PlayerId == cupidId);
            cupid.shielded = Helpers.getPlayerById(targetId);
        }

        public static void overrideNativeRole(byte playerId, byte roleType)
        {
            var player = Helpers.playerById(playerId);
            player.roleAssigned = false;
            FastDestroyableSingleton<RoleManager>.Instance.SetRole(player, (RoleTypes)roleType);
        }

        public static void versionHandshake(int major, int minor, int build, int revision, Guid guid, int clientId)
        {
            System.Version ver;
            if (revision < 0)
                ver = new System.Version(major, minor, build);
            else
                ver = new System.Version(major, minor, build, revision);
            GameStartManagerPatch.playerVersions[clientId] = new GameStartManagerPatch.PlayerVersion(ver, guid);
        }

        public static void useUncheckedVent(int ventId, byte playerId, byte isEnter)
        {
            PlayerControl player = Helpers.playerById(playerId);
            if (player == null) return;
            // Fill dummy MessageReader and call MyPhysics.HandleRpc as the corountines cannot be accessed
            MessageReader reader = new();
            byte[] bytes = BitConverter.GetBytes(ventId);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            reader.Buffer = bytes;
            reader.Length = bytes.Length;

            JackInTheBox.startAnimation(ventId);
            player.MyPhysics.HandleRpc(isEnter != 0 ? (byte)19 : (byte)20, reader);
        }

        public static void uncheckedMurderPlayer(byte sourceId, byte targetId, byte showAnimation)
        {
            PlayerControl source = Helpers.playerById(sourceId);
            PlayerControl target = Helpers.playerById(targetId);
            if (source != null && target != null)
            {
                if (showAnimation == 0) KillAnimationCoPerformKillPatch.hideNextAnimation = true;
                source.MurderPlayer(target);
            }
        }

        public static void uncheckedCmdReportDeadBody(byte sourceId, byte targetId)
        {
            PlayerControl source = Helpers.playerById(sourceId);
            PlayerControl target = Helpers.playerById(targetId);
            if (source != null && target != null) source.ReportDeadBody(target.Data);
        }

        public static void uncheckedExilePlayer(byte targetId)
        {
            PlayerControl target = Helpers.playerById(targetId);
            if (target != null)
            {
                target.Exiled();
            }
        }

        public static void uncheckedEndGame(byte reason)
        {
            AmongUsClient.Instance.GameState = InnerNet.InnerNetClient.GameStates.Ended;
            var obj2 = AmongUsClient.Instance.allClients;
            lock (obj2)
            {
                AmongUsClient.Instance.allClients.Clear();
            }

            var obj = AmongUsClient.Instance.Dispatcher;
            lock (obj)
            {
                AmongUsClient.Instance.Dispatcher.Add(new Action(() =>
                {
                    GameManager.Instance.enabled = false;
                    GameManager.Instance.ShouldCheckForGameEnd = false;
                    AmongUsClient.Instance.OnGameEnd(new EndGameResult((GameOverReason)reason, false));

                    if (AmongUsClient.Instance.AmHost)
                        GameManager.Instance.RpcEndGame((GameOverReason)reason, false);
                }));
            }
        }

        public static void uncheckedSetTasks(byte playerId, byte[] taskTypeIds)
        {
            var player = Helpers.playerById(playerId);
            player.clearAllTasks();

            player.Data.SetTasks(taskTypeIds);
        }

        public static void dynamicMapOption(byte mapId)
        {
            GameOptionsManager.Instance.currentNormalGameOptions.MapId = mapId;
        }

        // Role functionality

        public static void engineerFixLights()
        {
            SwitchSystem switchSystem = MapUtilities.CachedShipStatus.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
            switchSystem.ActualSwitches = switchSystem.ExpectedSwitches;
        }

        public static void engineerFixSubmergedOxygen()
        {
            SubmergedCompatibility.RepairOxygen();
        }

        public static void engineerUsedRepair()
        {
            Engineer.remainingFixes--;
        }

        public static void cleanBody(byte playerId)
        {
            DeadBody[] array = UnityEngine.Object.FindObjectsOfType<DeadBody>();
            for (int i = 0; i < array.Length; i++)
            {
                if (GameData.Instance.GetPlayerById(array[i].ParentId).PlayerId == playerId)
                {
                    UnityEngine.Object.Destroy(array[i].gameObject);
                }
            }
        }

        public static void sheriffKill(byte sheriffId, byte targetId, bool misfire)
        {
            PlayerControl sheriff = Helpers.playerById(sheriffId);
            PlayerControl target = Helpers.playerById(targetId);
            if (sheriff == null || target == null) return;

            Sheriff role = Sheriff.getRole(sheriff);
            if (role != null)
                role.numShots--;

            if (misfire)
            {
                sheriff.MurderPlayer(sheriff);
                finalStatuses[sheriffId] = FinalStatus.Misfire;

                if (!Sheriff.misfireKillsTarget) return;
                finalStatuses[targetId] = FinalStatus.Misfire;
            }

            sheriff.MurderPlayer(target);
        }

        public static void timeMasterRewindTime()
        {
            TimeMaster.shieldActive = false; // Shield is no longer active when rewinding
            if (TimeMaster.timeMaster != null && TimeMaster.timeMaster == PlayerControl.LocalPlayer)
            {
                resetTimeMasterButton();
            }
            FastDestroyableSingleton<HudManager>.Instance.FullScreen.color = new Color(0f, 0.5f, 0.8f, 0.3f);
            FastDestroyableSingleton<HudManager>.Instance.FullScreen.enabled = true;
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(TimeMaster.rewindTime / 2, new Action<float>((p) =>
            {
                if (p == 1f) FastDestroyableSingleton<HudManager>.Instance.FullScreen.enabled = false;
            })));

            if (TimeMaster.timeMaster == null || PlayerControl.LocalPlayer == TimeMaster.timeMaster) return; // Time Master himself does not rewind
            if (PlayerControl.LocalPlayer.isGM()) return; // GM does not rewind

            TimeMaster.isRewinding = true;

            if (MapBehaviour.Instance)
                MapBehaviour.Instance.Close();
            if (Minigame.Instance)
                Minigame.Instance.ForceClose();
            PlayerControl.LocalPlayer.moveable = false;
        }

        public static void timeMasterShield()
        {
            TimeMaster.shieldActive = true;
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(TimeMaster.shieldDuration, new Action<float>((p) =>
            {
                if (p == 1f) TimeMaster.shieldActive = false;
            })));
        }

        public static void medicSetShielded(byte shieldedId)
        {
            Medic.usedShield = true;
            Medic.shielded = Helpers.playerById(shieldedId);
            Medic.futureShielded = null;
        }

        public static void shieldedMurderAttempt()
        {
            if (Medic.shielded == null || Medic.medic == null) return;

            bool isShieldedAndShow = Medic.shielded == PlayerControl.LocalPlayer && Medic.showAttemptToShielded;
            bool isMedicAndShow = Medic.medic == PlayerControl.LocalPlayer && Medic.showAttemptToMedic;

            if ((isShieldedAndShow || isMedicAndShow) && FastDestroyableSingleton<HudManager>.Instance?.FullScreen != null)
            {
                Color c = Palette.ImpostorRed;
                Helpers.showFlash(new Color(c.r, c.g, c.b));
            }
        }

        public static void shifterShift(byte targetId)
        {
            PlayerControl oldShifter = Shifter.shifter;
            PlayerControl player = Helpers.playerById(targetId);
            if (player == null || oldShifter == null) return;

            Shifter.futureShift = null;
            if (!Shifter.isNeutral)
                Shifter.clearAndReload();

            if (player == GM.gm)
            {
                return;
            }

            // Suicide (exile) when impostor or impostor variants
            if (!Shifter.isNeutral && (player.Data.Role.IsImpostor || player.isNeutral() || player.hasModifier(ModifierType.Madmate) || player.hasModifier(ModifierType.CreatedMadmate)))
            {
                oldShifter.Exiled();
                finalStatuses[oldShifter.PlayerId] = FinalStatus.Suicide;
                return;
            }

            if (Shifter.shiftModifiers)
            {
                // Switch shield
                if (Medic.shielded != null && Medic.shielded == player)
                {
                    Medic.shielded = oldShifter;
                }
                else if (Medic.shielded != null && Medic.shielded == oldShifter)
                {
                    Medic.shielded = player;
                }

                player.swapModifiers(oldShifter);
                Lovers.swapLovers(oldShifter, player);
            }

            // Shift role
            player.swapRoles(oldShifter);

            if (Shifter.isNeutral)
            {
                Shifter.shifter = player;
                Shifter.pastShifters.Add(oldShifter.PlayerId);

                if (player.Data.Role.IsImpostor)
                {
                    FastDestroyableSingleton<RoleManager>.Instance.SetRole(player, RoleTypes.Crewmate);
                    FastDestroyableSingleton<RoleManager>.Instance.SetRole(oldShifter, RoleTypes.Impostor);
                }
            }

            if (Lawyer.lawyer != null && Lawyer.target == player)
            {
                Lawyer.target = oldShifter;
            }

            // Set cooldowns to max for both players
            if (PlayerControl.LocalPlayer == oldShifter || PlayerControl.LocalPlayer == player)
                CustomButton.ResetAllCooldowns();
        }

        public static void swapperSwap(byte playerId1, byte playerId2)
        {
            if (MeetingHud.Instance)
            {
                Swapper.playerId1 = playerId1;
                Swapper.playerId2 = playerId2;
            }
        }

        public static void swapperAnimate()
        {
            MeetingHudPatch.animateSwap = true;
        }

        public static void morphlingMorph(byte playerId)
        {
            PlayerControl target = Helpers.playerById(playerId);
            if (Morphling.morphling == null || target == null) return;
            Morphling.startMorph(target);
        }

        public static void camouflagerCamouflage()
        {
            if (Camouflager.camouflager == null) return;
            Camouflager.startCamouflage();
        }

        public static void vampireSetBitten(byte targetId, byte performReset)
        {
            if (performReset != 0)
            {
                Vampire.bitten = null;
                return;
            }

            if (Vampire.vampire == null) return;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.PlayerId == targetId && !player.Data.IsDead)
                {
                    Vampire.bitten = player;
                }
            }
        }

        public static void placeGarlic(byte[] buff)
        {
            Vector3 position = Vector3.zero;
            position.x = BitConverter.ToSingle(buff, 0 * sizeof(float));
            position.y = BitConverter.ToSingle(buff, 1 * sizeof(float));
            new Garlic(position);
        }

        public static void trackerUsedTracker(byte targetId)
        {
            Tracker.usedTracker = true;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                if (player.PlayerId == targetId)
                    Tracker.tracked = player;
        }

        public static void evilHackerCreatesMadmate(byte targetId)
        {
            PlayerControl player = Helpers.playerById(targetId);
            if (!EvilHacker.canCreateMadmateFromJackal && player.isRole(RoleType.Jackal))
            {
                EvilHacker.fakeMadmate = player;
            }
            else if (!EvilHacker.canCreateMadmateFromFox && player.isRole(RoleType.Fox))
            {
                EvilHacker.fakeMadmate = player;
            }
            else
            {
                // Jackalバグ対応
                List<PlayerControl> tmpFormerJackals = new(Jackal.formerJackals);

                // タスクがないプレイヤーがMadmateになった場合はショートタスクを必要数割り当てる
                if (Helpers.hasFakeTasks(player))
                {
                    if (CreatedMadmate.hasTasks)
                    {
                        Helpers.clearAllTasks(player);
                        player.generateAndAssignTasks(0, CreatedMadmate.numTasks, 0);
                    }
                }

                GameData.Instance.GetPlayerById(player.PlayerId); // player.RemoveInfected(); (was removed in 2022.12.08, no idea if we ever need that part again, replaced by these 2 lines.) 
                player.CoSetRole(RoleTypes.Crewmate, true);
                erasePlayerRoles(player.PlayerId, true, false);

                // Jackalバグ対応
                Jackal.formerJackals = tmpFormerJackals;

                player.addModifier(ModifierType.CreatedMadmate);
            }
            EvilHacker.canCreateMadmate = false;
            return;
        }

        public static void jackalCreatesSidekick(byte targetId)
        {
            PlayerControl player = Helpers.playerById(targetId);
            if (player == null) return;
            Patches.Logger.info($"SideKick {player.Data.PlayerName}({RoleInfo.GetRolesString(player, false, joinSeparator: " + ")})", "Jackal");

            if (!Jackal.canCreateSidekickFromImpostor && player.Data.Role.IsImpostor)
            {
                Jackal.fakeSidekick = player;
            }
            else if (!Jackal.canCreateSidekickFromFox && player.isRole(RoleType.Fox))
            {
                Jackal.fakeSidekick = player;
            }
            else
            {
                bool wasSpy = Spy.spy != null && player == Spy.spy;
                bool wasImpostor = player.Data.Role.IsImpostor;  // This can only be reached if impostors can be sidekicked.
                FastDestroyableSingleton<RoleManager>.Instance.SetRole(player, RoleTypes.Crewmate);
                erasePlayerRoles(player.PlayerId, true, false);
                Sidekick.sidekick = player;
                if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId) PlayerControl.LocalPlayer.moveable = true;
                if (wasSpy || wasImpostor) Sidekick.wasTeamRed = true;
                Sidekick.wasSpy = wasSpy;
                Sidekick.wasImpostor = wasImpostor;
                // 狐が一人もいなくなったら背徳者は死亡する
                if (Fox.exists && !Fox.isFoxAlive())
                {
                    foreach (var immoralist in Immoralist.allPlayers)
                    {
                        if (immoralist.isAlive())
                        {
                            immoralist.MurderPlayer(immoralist);
                        }
                    }
                }
            }
            Jackal.canCreateSidekick = false;
        }

        public static void sidekickPromotes()
        {
            Jackal.removeCurrentJackal();
            Jackal.jackal = Sidekick.sidekick;
            Jackal.canCreateSidekick = Jackal.jackalPromotedFromSidekickCanCreateSidekick;
            Sidekick.clearAndReload();
            return;
        }

        public static void erasePlayerRoles(byte playerId, bool ignoreLovers = false, bool clearNeutralTasks = true)
        {
            PlayerControl player = Helpers.playerById(playerId);
            if (player == null || !player.canBeErased()) return;

            // Don't give a former neutral role tasks because that destroys the balance.
            if (player.isNeutral() && clearNeutralTasks)
                player.clearAllTasks();

            player.eraseAllRoles();
            player.eraseAllModifiers();

            if (!ignoreLovers && player.isLovers())
            { // The whole Lover couple is being erased
                Lovers.eraseCouple(player);
            }
        }

        public static void setFutureErased(byte playerId)
        {
            PlayerControl player = Helpers.playerById(playerId);
            if (Eraser.futureErased == null)
                Eraser.futureErased = new List<PlayerControl>();
            if (player != null)
            {
                Eraser.futureErased.Add(player);
            }
        }

        public static void setFutureShifted(byte playerId)
        {
            if (Shifter.isNeutral && !Shifter.shiftPastShifters && Shifter.pastShifters.Contains(playerId))
                return;
            Shifter.futureShift = Helpers.playerById(playerId);
        }

        public static void setFutureShielded(byte playerId)
        {
            Medic.futureShielded = Helpers.playerById(playerId);
            Medic.usedShield = true;
        }

        public static void setFutureSpelled(byte playerId)
        {
            PlayerControl player = Helpers.playerById(playerId);
            if (Witch.futureSpelled == null)
                Witch.futureSpelled = new List<PlayerControl>();
            if (player != null)
            {
                Witch.futureSpelled.Add(player);
            }
        }

        public static void placeAssassinTrace(byte[] buff)
        {
            Vector3 position = Vector3.zero;
            position.x = BitConverter.ToSingle(buff, 0 * sizeof(float));
            position.y = BitConverter.ToSingle(buff, 1 * sizeof(float));
            new AssassinTrace(position, Assassin.traceTime);
        }

        public static void placeJackInTheBox(byte[] buff)
        {
            Vector3 position = Vector3.zero;
            position.x = BitConverter.ToSingle(buff, 0 * sizeof(float));
            position.y = BitConverter.ToSingle(buff, 1 * sizeof(float));
            new JackInTheBox(position);
        }

        public static void lightsOut()
        {
            Trickster.lightsOutTimer = Trickster.lightsOutDuration;
            // If the local player is impostor indicate lights out
            if (PlayerControl.LocalPlayer.Data.Role.IsImpostor)
            {
                new CustomMessage(ModTranslation.getString("tricksterLightsOutText"), Trickster.lightsOutDuration);
            }
        }

        public static void placeCamera(byte[] buff, byte roomId)
        {
            var referenceCamera = UnityEngine.Object.FindObjectOfType<SurvCamera>();
            if (referenceCamera == null) return; // Mira HQ

            SecurityGuard.remainingScrews -= SecurityGuard.camPrice;
            SecurityGuard.placedCameras++;

            Vector3 position = Vector3.zero;
            position.x = BitConverter.ToSingle(buff, 0 * sizeof(float));
            position.y = BitConverter.ToSingle(buff, 1 * sizeof(float));

            SystemTypes roomType = (SystemTypes)roomId;

            var camera = UnityEngine.Object.Instantiate<SurvCamera>(referenceCamera);
            camera.transform.position = new Vector3(position.x, position.y, referenceCamera.transform.position.z - 1f);
            camera.CamName = $"Security Camera {SecurityGuard.placedCameras}";
            camera.Offset = new Vector3(0f, 0f, camera.Offset.z);

            camera.NewName = roomType switch
            {
                SystemTypes.Hallway => StringNames.Hallway,
                SystemTypes.Storage => StringNames.Storage,
                SystemTypes.Cafeteria => StringNames.Cafeteria,
                SystemTypes.Reactor => StringNames.Reactor,
                SystemTypes.UpperEngine => StringNames.UpperEngine,
                SystemTypes.Nav => StringNames.Nav,
                SystemTypes.Admin => StringNames.Admin,
                SystemTypes.Electrical => StringNames.Electrical,
                SystemTypes.LifeSupp => StringNames.LifeSupp,
                SystemTypes.Shields => StringNames.Shields,
                SystemTypes.MedBay => StringNames.MedBay,
                SystemTypes.Security => StringNames.Security,
                SystemTypes.Weapons => StringNames.Weapons,
                SystemTypes.LowerEngine => StringNames.LowerEngine,
                SystemTypes.Comms => StringNames.Comms,
                SystemTypes.Decontamination => StringNames.Decontamination,
                SystemTypes.Launchpad => StringNames.Launchpad,
                SystemTypes.LockerRoom => StringNames.LockerRoom,
                SystemTypes.Laboratory => StringNames.Laboratory,
                SystemTypes.Balcony => StringNames.Balcony,
                SystemTypes.Office => StringNames.Office,
                SystemTypes.Greenhouse => StringNames.Greenhouse,
                SystemTypes.Dropship => StringNames.Dropship,
                SystemTypes.Decontamination2 => StringNames.Decontamination2,
                SystemTypes.Outside => StringNames.Outside,
                SystemTypes.Specimens => StringNames.Specimens,
                SystemTypes.BoilerRoom => StringNames.BoilerRoom,
                SystemTypes.VaultRoom => StringNames.VaultRoom,
                SystemTypes.Cockpit => StringNames.Cockpit,
                SystemTypes.Armory => StringNames.Armory,
                SystemTypes.Kitchen => StringNames.Kitchen,
                SystemTypes.ViewingDeck => StringNames.ViewingDeck,
                SystemTypes.HallOfPortraits => StringNames.HallOfPortraits,
                SystemTypes.CargoBay => StringNames.CargoBay,
                SystemTypes.Ventilation => StringNames.Ventilation,
                SystemTypes.Showers => StringNames.Showers,
                SystemTypes.Engine => StringNames.Engine,
                SystemTypes.Brig => StringNames.Brig,
                SystemTypes.MeetingRoom => StringNames.MeetingRoom,
                SystemTypes.Records => StringNames.Records,
                SystemTypes.Lounge => StringNames.Lounge,
                SystemTypes.GapRoom => StringNames.GapRoom,
                SystemTypes.MainHall => StringNames.MainHall,
                SystemTypes.Medical => StringNames.Medical,
                _ => StringNames.ExitButton,
            };
            if (GameOptionsManager.Instance.currentNormalGameOptions.MapId is 2 or 4) camera.transform.localRotation = new Quaternion(0, 0, 1, 1); // Polus and Airship

            if (PlayerControl.LocalPlayer == SecurityGuard.securityGuard)
            {
                camera.gameObject.SetActive(true);
                camera.gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.5f);
            }
            else
            {
                camera.gameObject.SetActive(false);
            }
            TORMapOptions.camerasToAdd.Add(camera);
        }

        public static void sealVent(int ventId)
        {
            Vent vent = MapUtilities.CachedShipStatus.AllVents.FirstOrDefault((x) => x != null && x.Id == ventId);
            if (vent == null) return;

            SecurityGuard.remainingScrews -= SecurityGuard.ventPrice;
            if (PlayerControl.LocalPlayer == SecurityGuard.securityGuard)
            {
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
                rend.color = new Color(1f, 1f, 1f, 0.5f);
                vent.name = "FutureSealedVent_" + vent.name;
            }

            TORMapOptions.ventsToSeal.Add(vent);
        }

        public static void arsonistDouse(byte playerId)
        {
            Arsonist.dousedPlayers.Add(Helpers.playerById(playerId));
        }

        public static void arsonistWin()
        {
            Arsonist.triggerArsonistWin = true;
            var livingPlayers = PlayerControl.AllPlayerControls.ToArray().ToArray().Where(p => !p.isRole(RoleType.Arsonist) && p.isAlive());
            foreach (PlayerControl p in livingPlayers)
            {
                p.Exiled();
                finalStatuses[p.PlayerId] = FinalStatus.Torched;
            }
        }

        public static void vultureEat(byte playerId)
        {
            cleanBody(playerId);
            Vulture.eatenBodies++;
        }

        public static void vultureWin()
        {
            Vulture.triggerVultureWin = true;
        }

        public static void lawyerWin()
        {
            Lawyer.triggerLawyerWin = true;
        }

        public static void lawyerSetTarget(byte playerId)
        {
            Lawyer.target = Helpers.playerById(playerId);
        }

        public static void lawyerPromotesToPursuer()
        {
            PlayerControl player = Lawyer.lawyer;
            PlayerControl client = Lawyer.target;
            Lawyer.clearAndReload();
            Pursuer.pursuer = player;

            if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId && client != null)
            {
                Transform playerInfoTransform = client.cosmetics.nameText.transform.parent.FindChild("Info");
                TMPro.TextMeshPro playerInfo = playerInfoTransform != null ? playerInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                if (playerInfo != null) playerInfo.text = "";
            }
        }

        public static void guesserShoot(byte killerId, byte dyingTargetId, byte guessedTargetId, byte guessedRoleType)
        {
            PlayerControl killer = Helpers.playerById(killerId);
            PlayerControl dyingTarget = Helpers.playerById(dyingTargetId);
            if (dyingTarget == null) return;
            if (dyingTarget.isRole(RoleType.NekoKabocha))
            {
                NekoKabocha.meetingKill(dyingTarget, killer);
            }
            dyingTarget.Exiled();
            PlayerControl dyingLoverPartner = Lovers.bothDie ? dyingTarget.getPartner() : null; // Lover check

            Guesser.remainingShots(killer, true);

            if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(dyingTarget.KillSfx, false, 0.8f);

            PlayerControl guesser = Helpers.playerById(killerId);
            if (FastDestroyableSingleton<HudManager>.Instance != null && guesser != null)
                if (PlayerControl.LocalPlayer == dyingTarget)
                    FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(guesser.Data, dyingTarget.Data);
                else if (dyingLoverPartner != null && PlayerControl.LocalPlayer == dyingLoverPartner)
                    FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(dyingLoverPartner.Data, dyingLoverPartner.Data);

            PlayerControl guessedTarget = Helpers.playerById(guessedTargetId);
            if (Guesser.showInfoInGhostChat && PlayerControl.LocalPlayer.Data.IsDead && guessedTarget != null)
            {
                RoleInfo roleInfo = RoleInfo.allRoleInfos.FirstOrDefault(x => (byte)x.roleType == guessedRoleType);
                string msg = string.Format(ModTranslation.getString("guesserGuessChat"), roleInfo.name, guessedTarget.Data.PlayerName);
                if (AmongUsClient.Instance.AmClient && FastDestroyableSingleton<HudManager>.Instance)
                    FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(guesser, msg);
                if (msg.IndexOf("who", StringComparison.OrdinalIgnoreCase) >= 0)
                    FastDestroyableSingleton<UnityTelemetry>.Instance.SendWho();
            }
        }

        public static void setBlanked(byte playerId, byte value)
        {
            PlayerControl target = Helpers.playerById(playerId);
            if (target == null) return;
            Pursuer.blankedList.RemoveAll(x => x.PlayerId == playerId);
            if (value > 0) Pursuer.blankedList.Add(target);
        }

        public static void witchSpellCast(byte playerId)
        {
            uncheckedExilePlayer(playerId);
            finalStatuses[playerId] = FinalStatus.Spelled;
        }

        public static void setShifterType(bool isNeutral)
        {
            Shifter.isNeutral = isNeutral;
        }

        public static void ninjaStealth(byte playerId, bool stealthed)
        {
            PlayerControl player = Helpers.playerById(playerId);
            Ninja.setStealthed(player, stealthed);
        }
        public static void foxStealth(byte playerId, bool stealthed)
        {
            PlayerControl player = Helpers.playerById(playerId);
            Fox.setStealthed(player, stealthed);
        }

        public static void foxCreatesImmoralist(byte targetId)
        {
            PlayerControl player = Helpers.playerById(targetId);
            FastDestroyableSingleton<RoleManager>.Instance.SetRole(player, RoleTypes.Crewmate);
            erasePlayerRoles(player.PlayerId, true);
            player.setRole(RoleType.Immoralist);
            player.clearAllTasks();
        }

        public static void akujoSetHonmei(byte akujoId, byte targetId)
        {
            Akujo akujo = Akujo.getRole(Helpers.playerById(akujoId));
            PlayerControl target = Helpers.playerById(targetId);

            if (akujo != null)
            {
                akujo.setHonmei(target);
            }
        }

        public static void akujoSetKeep(byte akujoId, byte targetId)
        {
            Akujo akujo = Akujo.getRole(Helpers.playerById(akujoId));
            PlayerControl target = Helpers.playerById(targetId);

            if (akujo != null)
            {
                akujo.setKeep(target);
            }
        }

        public static void akujoSuicide(byte akujoId)
        {
            Akujo akujo = Akujo.getRole(Helpers.playerById(akujoId));
            if (akujo != null)
            {
                akujo.player.MurderPlayer(akujo.player);
                finalStatuses[akujo.player.PlayerId] = FinalStatus.Loneliness;
            }
        }

        public static void cupidSuicide(byte cupidId, bool isScapegoat, bool isExiled)
        {
            Cupid cupid = Cupid.getRole(Helpers.playerById(cupidId));
            if (cupid != null)
            {
                if (!isExiled)
                {
                    cupid.player.MurderPlayer(cupid.player);
                }
                else
                {
                    cupid.player.Exiled();
                }
                finalStatuses[cupid.player.PlayerId] = isScapegoat ? FinalStatus.Scapegoat : FinalStatus.Suicide;
            }

        }

        public static void impostorPromotesToLastImpostor(byte targetId)
        {
            PlayerControl player = Helpers.playerById(targetId);
            player.addModifier(ModifierType.LastImpostor);
        }

        public static void GMKill(byte targetId)
        {
            PlayerControl target = Helpers.playerById(targetId);

            if (target == null) return;
            target.MyPhysics.ExitAllVents();
            target.Exiled();
            finalStatuses[target.PlayerId] = FinalStatus.GMExecuted;

            PlayerControl partner = target.getPartner(); // Lover check
            if (partner != null)
            {
                partner?.MyPhysics.ExitAllVents();
                finalStatuses[partner.PlayerId] = FinalStatus.GMExecuted;
            }

            if (FastDestroyableSingleton<HudManager>.Instance != null && GM.gm != null)
            {
                if (PlayerControl.LocalPlayer == target)
                    FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(GM.gm.Data, target.Data);
                else if (partner != null && PlayerControl.LocalPlayer == partner)
                    FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(GM.gm.Data, partner.Data);
            }
        }

        public static void GMRevive(byte targetId)
        {
            PlayerControl target = Helpers.playerById(targetId);
            if (target == null) return;
            target.Revive();
            updateMeeting(targetId, false);
            finalStatuses[target.PlayerId] = FinalStatus.Alive;

            PlayerControl partner = target.getPartner(); // Lover check
            if (partner != null)
            {
                partner.Revive();
                updateMeeting(partner.PlayerId, false);
                finalStatuses[partner.PlayerId] = FinalStatus.Alive;
            }

            if (PlayerControl.LocalPlayer.isGM())
            {
                FastDestroyableSingleton<HudManager>.Instance.ShadowQuad.gameObject.SetActive(false);
            }
        }

        public static void updateMeeting(byte targetId, bool dead = true)
        {
            if (MeetingHud.Instance)
            {
                foreach (PlayerVoteArea pva in MeetingHud.Instance.playerStates)
                {
                    if (pva.TargetPlayerId == targetId)
                    {
                        pva.SetDead(pva.DidReport, dead);
                        pva.Overlay.gameObject.SetActive(dead);
                    }

                    // Give players back their vote if target is shot dead
                    if (Helpers.RefundVotes && dead)
                    {
                        if (pva.VotedFor != targetId) continue;
                        pva.UnsetVote();
                        var voteAreaPlayer = Helpers.playerById(pva.TargetPlayerId);
                        if (!voteAreaPlayer.AmOwner) continue;
                        MeetingHud.Instance.ClearVote();
                    }
                }

                if (AmongUsClient.Instance.AmHost)
                    MeetingHud.Instance.CheckForEndVoting();
            }
        }

        public static void useAdminTime(float time)
        {
            TORMapOptions.restrictAdminTime -= time;
        }

        public static void useCameraTime(float time)
        {
            TORMapOptions.restrictCamerasTime -= time;
        }

        public static void useVitalsTime(float time)
        {
            TORMapOptions.restrictVitalsTime -= time;
        }

        public static void plagueDoctorWin()
        {
            PlagueDoctor.triggerPlagueDoctorWin = true;
            var livingPlayers = PlayerControl.AllPlayerControls.ToArray().ToArray().Where(p => !p.isRole(RoleType.PlagueDoctor) && p.isAlive());
            foreach (PlayerControl p in livingPlayers)
            {
                // Check again so we don't re-kill any lovers
                if (p.isAlive())
                    p.Exiled();
                finalStatuses[p.PlayerId] = FinalStatus.Diseased;
            }
        }

        public static void plagueDoctorInfected(byte targetId)
        {
            var p = Helpers.playerById(targetId);
            if (!PlagueDoctor.infected.ContainsKey(targetId))
            {
                PlagueDoctor.infected[targetId] = p;
            }
        }

        public static void plagueDoctorProgress(byte targetId, float progress)
        {
            PlagueDoctor.progress[targetId] = progress;
        }

        public static void nekoKabochaExile(byte playerId)
        {
            uncheckedExilePlayer(playerId);
            finalStatuses[playerId] = FinalStatus.Revenge;
        }

        public static void serialKillerSuicide(byte serialKillerId)
        {
            PlayerControl serialKiller = Helpers.playerById(serialKillerId);
            if (serialKiller == null) return;
            serialKiller.MurderPlayer(serialKiller);
        }
        public static void fortuneTellerUsedDivine(byte fortuneTellerId, byte targetId)
        {
            PlayerControl fortuneTeller = Helpers.playerById(fortuneTellerId);
            PlayerControl target = Helpers.playerById(targetId);
            if (target == null) return;
            if (target.isDead()) return;
            // 呪殺
            if (target.isRole(RoleType.Fox) || target.isRole(RoleType.SchrodingersCat) || target.isRole(RoleType.Puppeteer))
            {
                if (!PlayerControl.LocalPlayer.isRole(RoleType.FortuneTeller))
                {
                    KillAnimationCoPerformKillPatch.hideNextAnimation = true;
                    fortuneTeller.MurderPlayer(target);
                }
                else
                {
                    target.MurderPlayer(target);
                }
            }
            // インポスターの場合は占い師の位置に矢印を表示 ラストインポスターの占いの場合は表示しない
            if (fortuneTeller.isRole(RoleType.FortuneTeller) && PlayerControl.LocalPlayer.isImpostor())
            {
                FortuneTeller.fortuneTellerMessage(ModTranslation.getString("fortuneTellerDivinedSomeone"), 5f, Color.white);
                FortuneTeller.setDivinedFlag(fortuneTeller, true);
            }
            // 占われたのが背徳者の場合は通知を表示
            if (target.isRole(RoleType.Immoralist) && PlayerControl.LocalPlayer.isRole(RoleType.Immoralist))
            {
                FortuneTeller.fortuneTellerMessage(ModTranslation.getString("fortuneTellerDivinedYou"), 5f, Color.white);
            }
        }

        public static void schrodingersCatSuicide()
        {
            KillAnimationCoPerformKillPatch.hideNextAnimation = true;
            SchrodingersCat.killer.MurderPlayer(SchrodingersCat.killer);
            SchrodingersCat.killer = null;
        }

        public static void schrodingersCatSetTeam(byte team)
        {
            switch ((SchrodingersCat.Team)team)
            {
                case SchrodingersCat.Team.Crew:
                    SchrodingersCat.setCrewFlag();
                    break;
                case SchrodingersCat.Team.Impostor:
                    SchrodingersCat.setImpostorFlag();
                    if (SchrodingersCat.becomesImpostor)
                    {
                        SchrodingersCat.allPlayers.ForEach(x => FastDestroyableSingleton<RoleManager>.Instance.SetRole(x, RoleTypes.Impostor));
                    }
                    break;
                case SchrodingersCat.Team.Jackal:
                    SchrodingersCat.setJackalFlag();
                    break;
                case SchrodingersCat.Team.JekyllAndHyde:
                    SchrodingersCat.setJekyllAndHydeFlag();
                    break;
                case SchrodingersCat.Team.Moriarty:
                    SchrodingersCat.setMoriartyFlag();
                    break;
                default:
                    SchrodingersCat.setCrewFlag();
                    break;
            }
        }
        public static void placeTrap(byte[] buff)
        {
            Vector3 pos = Vector3.zero;
            pos.x = BitConverter.ToSingle(buff, 0 * sizeof(float));
            pos.y = BitConverter.ToSingle(buff, 1 * sizeof(float)) - 0.2f;
            Trap trap = new(pos);
        }
        public static void clearTrap()
        {
            Trap.clearAllTraps();
        }
        public static void disableTrap(byte trapId)
        {
            Trap.disableTrap(trapId);
        }
        public static void activateTrap(byte trapId, byte trapperId, byte playerId)
        {
            var trapper = Helpers.playerById(trapperId);
            var player = Helpers.playerById(playerId);
            Trap.activateTrap(trapId, trapper, player);
        }
        public static void trapperKill(byte trapId, byte trapperId, byte playerId)
        {
            var trapper = Helpers.playerById(trapperId);
            var target = Helpers.playerById(playerId);
            Trap.trapKill(trapId, trapper, target);
        }
        public static void trapperMeetingFlag()
        {
            Trap.onMeeting();
        }
        public static void randomSpawn(byte playerId, byte locId)
        {
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(3f, new Action<float>((p) =>
            { // Delayed action
                if (p == 1f)
                {
                    Vector2 InitialSpawnCenter = new(16.64f, -2.46f);
                    Vector2 MeetingSpawnCenter = new(17.4f, -16.286f);
                    Vector2 ElectricalSpawn = new(5.53f, -9.84f);
                    Vector2 O2Spawn = new(3.28f, -21.67f);
                    Vector2 SpecimenSpawn = new(36.54f, -20.84f);
                    Vector2 LaboSpawn = new(34.91f, -6.50f);
                    var loc = locId switch
                    {
                        0 => InitialSpawnCenter,
                        1 => MeetingSpawnCenter,
                        2 => ElectricalSpawn,
                        3 => O2Spawn,
                        4 => SpecimenSpawn,
                        5 => LaboSpawn,
                        _ => InitialSpawnCenter,
                    };
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    {
                        if (player.Data.PlayerId == playerId)
                        {
                            player.transform.position = loc;
                            break;
                        }
                    }
                }
            })));
        }
        public static void plantBomb(byte playerId)
        {
            var p = Helpers.playerById(playerId);
            if (PlayerControl.LocalPlayer.isRole(RoleType.BomberA)) BomberB.bombTarget = p;
            if (PlayerControl.LocalPlayer.isRole(RoleType.BomberB)) BomberA.bombTarget = p;
        }
        public static void releaseBomb(byte killer, byte target)
        {
            // 同時押しでダブルキルが発生するのを防止するためにBomberAで一度受け取ってから実行する
            if (PlayerControl.LocalPlayer.isRole(RoleType.BomberA))
            {
                if (BomberA.bombTarget != null && BomberB.bombTarget != null)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.BomberKill, Hazel.SendOption.Reliable, -1);
                    writer.Write(killer);
                    writer.Write(target);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.bomberKill(killer, target);
                }
            }
        }
        public static void bomberKill(byte killer, byte target)
        {
            BomberA.bombTarget = null;
            BomberB.bombTarget = null;
            var k = Helpers.playerById(killer);
            var t = Helpers.playerById(target);
            if (t.isAlive())
            {
                KillAnimationCoPerformKillPatch.hideNextAnimation = true;
                k.MurderPlayer(t);
                if (BomberA.showEffects)
                {
                    new BombEffect(t);
                }
            }
            BomberA.bomberButton.Timer = BomberA.bomberButton.MaxTimer;
            BomberB.bomberButton.Timer = BomberB.bomberButton.MaxTimer;
        }

        public static void spawnDummy(byte playerId, Vector3 pos)
        {
            var playerControl = UnityEngine.Object.Instantiate(AmongUsClient.Instance.PlayerPrefab);
            playerControl.PlayerId = playerId;

            Puppeteer.dummy = playerControl;
            GameData.Instance.AddDummy(playerControl);
            AmongUsClient.Instance.Spawn(playerControl, -2, InnerNet.SpawnFlags.IsClientCharacter);

            playerControl.transform.position = pos;
            playerControl.GetComponent<DummyBehaviour>().enabled = false;
            playerControl.NetTransform.enabled = true;
            playerControl.NetTransform.Halt();
            playerControl.Visible = false;
            playerControl.Data.RpcSetTasks(new byte[0]);
            //playerControl.Data.Tasks = new Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo>();
            // GameData.Instance.RpcSetTasks(playerControl.PlayerId, new byte[0]);
            // playerControl.clearAllTasks();
        }

        public static void walkDummy(Vector3 direction)
        {
            if (Puppeteer.dummy == null) return;
            var dummy = Puppeteer.dummy;
            dummy.MyPhysics.body.velocity = Vector2.zero;
            if (PlayerControl.LocalPlayer.isRole(RoleType.Puppeteer))
            {
                PlayerControl.LocalPlayer.MyPhysics.body.velocity = Vector2.zero;
                KillAnimation.SetMovement(PlayerControl.LocalPlayer, false);
                KillAnimation.SetMovement(PlayerControl.LocalPlayer, true);
            }
            KillAnimation.SetMovement(dummy, false);
            KillAnimation.SetMovement(dummy, true);
            if (PlayerControl.LocalPlayer.isRole(RoleType.Puppeteer))
            {
                dummy.MyPhysics.body.interpolation = RigidbodyInterpolation2D.Interpolate;
                PlayerControl.LocalPlayer.MyPhysics.body.velocity = new Vector2(0, 0);
            }
            dummy.MyPhysics.body.velocity = direction * dummy.MyPhysics.TrueSpeed;
        }

        public static void moveDummy(Vector3 pos, bool spawn = false)
        {
            if (Puppeteer.dummy == null) return;
            var dummy = Puppeteer.dummy;
            if (SubmergedCompatibility.IsSubmerged && spawn)
            {
                bool toUpper = pos.y > -7;
                SubmergedPatch.ChangePlayerFloorState(dummy.PlayerId, toUpper);
                // bool toUpper = pos.y > -7;
                // MonoBehaviour _floorHandler = ((Component)SubmergedPatch.GetFloorHandlerMethod.Invoke(null, new object[] { dummy })).TryCast(SubmergedPatch.FloorHandlerType) as MonoBehaviour;
                // SubmergedPatch.RpcRequestChangeFloorMethod.Invoke(_floorHandler, new object[] { toUpper });
            }
            dummy.transform.position = pos;
            dummy.NetTransform.Halt();
            dummy.Visible = true;
            dummy.moveable = true;
        }

        public static void puppeteerStealth(bool stealthed)
        {
            Puppeteer.setStealthed(stealthed);
        }
        public static void puppeteerMorph(byte playerId)
        {
            if (Puppeteer.dummy != null)
            {
                var to = Helpers.playerById(playerId);
                MorphHandler.setOutfit(Puppeteer.dummy, to.Data.DefaultOutfit);
            }
        }
        public static void puppeteerWin()
        {
            Puppeteer.triggerPuppeteerWin = true;
            var livingPlayers = PlayerControl.AllPlayerControls.ToArray().ToArray().Where(p => !p.isRole(RoleType.Puppeteer) && p.isAlive());
            foreach (PlayerControl p in livingPlayers)
            {
                // p.Exiled();
                finalStatuses[p.PlayerId] = FinalStatus.Spelled;
            }
        }

        public static void puppeteerKill(byte killer, byte target)
        {
            var k = Helpers.playerById(killer);
            var t = Helpers.playerById(target);
            KillAnimationCoPerformKillPatch.hideNextAnimation = true;
            k.MurderPlayer(t);
        }

        public static void puppeteerClimbRadder(byte dummyId, byte targetId)
        {
            PlayerControl dummy = Helpers.playerById(dummyId);
            Ladder target = FastDestroyableSingleton<AirshipStatus>.Instance.GetComponentsInChildren<Ladder>().ToList().Find(x => x.Id == targetId);
            if (target == null) return;
            dummy.MyPhysics.ClimbLadder(target, (byte)(dummy.MyPhysics.lastClimbLadderSid + 1));
        }
        public static void puppeteerUsePlatform(byte dummyId)
        {
            PlayerControl dummy = Helpers.playerById(dummyId);
            MovingPlatformBehaviour target = FastDestroyableSingleton<AirshipStatus>.Instance.GapPlatform;
            if (target == null) return;
            dummy.NetTransform.Halt();
            target.Use(dummy);
        }

        public static void mimicMorph(byte mimicAId, byte mimicBId)
        {
            PlayerControl mimicA = Helpers.playerById(mimicAId);
            PlayerControl mimicB = Helpers.playerById(mimicBId);
            MorphHandler.morphToPlayer(mimicA, mimicB);
            MimicA.isMorph = true;
        }

        public static void mimicResetMorph(byte mimicAId)
        {
            PlayerControl mimicA = Helpers.playerById(mimicAId);
            MorphHandler.resetMorph(mimicA);
            MimicA.isMorph = false;
        }

        public static void synchronize(byte playerId, int tag)
        {
            SpawnInMinigamePatch.synchronizeData.Synchronize((SpawnInMinigamePatch.SynchronizeTag)tag, playerId);
        }

        public static void setOddIsJekyll(bool b)
        {
            JekyllAndHyde.oddIsJekyll = b;
        }

        public static void shareRealTasks(MessageReader reader)
        {
            byte count = reader.ReadByte();
            for (int i = 0; i < count; i++)
            {
                byte playerId = reader.ReadByte();
                byte[] taskTmp = reader.ReadBytes(4);
                float x = System.BitConverter.ToSingle(taskTmp, 0);
                taskTmp = reader.ReadBytes(4);
                float y = System.BitConverter.ToSingle(taskTmp, 0);
                Vector2 pos = new Vector2(x, y);
                if (!MapBehaviorPatch.realTasks.ContainsKey(playerId)) MapBehaviorPatch.realTasks[playerId] = new Il2CppSystem.Collections.Generic.List<Vector2>();
                MapBehaviorPatch.realTasks[playerId].Add(pos);
            }
        }

        public static void syncKillTimer(byte playerId, float timer)
        {
            if (!SoulPlayer.killTimers.ContainsKey(playerId))
            {
                SoulPlayer.killTimers.Add(playerId, timer);
            }
            else
            {
                SoulPlayer.killTimers[playerId] = timer;
            }
        }

        public static void setBrainwash(byte playerId)
        {
            var p = Helpers.playerById(playerId);
            Moriarty.target = p;
            Moriarty.brainwashed.Add(p);
        }
        public static void moriartyKill(byte targetId)
        {
            PlayerControl target = Helpers.getPlayerById(targetId);
            finalStatuses[targetId] = FinalStatus.BrainWashKill;
            if (PlayerControl.LocalPlayer == Moriarty.target)
            {
                if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(target.KillSfx, false, 0.8f);
            }
            Moriarty.counter += 1;
        }
        public static void yoyoMarkLocation(byte[] buff)
        {
            if (Yoyo.yoyo == null) return;
            Vector3 position = Vector3.zero;
            position.x = BitConverter.ToSingle(buff, 0 * sizeof(float));
            position.y = BitConverter.ToSingle(buff, 1 * sizeof(float));
            Yoyo.markLocation(position);
            new Silhouette(position, -1, false);
        }

        public static void yoyoBlink(bool isFirstJump, byte[] buff)
        {
            if (Yoyo.yoyo == null || Yoyo.markedLocation == null) return;
            var markedPos = (Vector3)Yoyo.markedLocation;
            Yoyo.yoyo.NetTransform.SnapTo(markedPos);

            var markedSilhouette = Silhouette.silhouettes.FirstOrDefault(s => s.gameObject.transform.position.x == markedPos.x && s.gameObject.transform.position.y == markedPos.y);
            if (markedSilhouette != null)
                markedSilhouette.permanent = false;

            Vector3 position = Vector3.zero;
            position.x = BitConverter.ToSingle(buff, 0 * sizeof(float));
            position.y = BitConverter.ToSingle(buff, 1 * sizeof(float));
            // Create Silhoutte At Start Position:
            if (isFirstJump)
            {
                Yoyo.markLocation(position);
                new Silhouette(position, Yoyo.blinkDuration, true);
            }
            else
            {
                new Silhouette(position, 5, true);
                Yoyo.markedLocation = null;
            }
        }


        [HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.HandleRpc))]
        class CustomNetworkTransformRPCHandlerPatch
        {
            public static bool Prefix(CustomNetworkTransform __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
            {
                var rpcType = (RpcCalls)callId;
                MessageReader subReader = MessageReader.Get(reader);
                switch (rpcType)
                {
                    case RpcCalls.SnapTo:
                        Vector2 position = NetHelpers.ReadVector2(subReader);
                        ushort minSid = subReader.ReadUInt16();
                        Patches.Logger.info($"{__instance.name} => x:{position.x} y:{position.y} minSid:{minSid}", "SnapTo");
                        break;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
        class RPCHandlerPatch
        {
            public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
            {
                var rpcType = (RpcCalls)callId;
                MessageReader subReader = MessageReader.Get(reader);
                switch (rpcType)
                {
                    case RpcCalls.StartMeeting:
                        var p = Helpers.getPlayerById(subReader.ReadByte());
                        Patches.Logger.info($"{__instance.getNameWithRole()} => {p?.getNameWithRole() ?? "null"}", "StartMeeting");
                        break;
                }
                return true;
            }
            static void Postfix([HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
            {
                byte packetId = callId;
                try
                {
                    CustomRPC rpc = (CustomRPC)packetId;
                    Patches.Logger.info(Helpers.GetRpcName(callId), "RPC");
                    switch (packetId)
                    {
                        // Main Controls
                        case (byte)CustomRPC.ResetVariables:
                            RPCProcedure.resetVariables();
                            break;
                        case (byte)CustomRPC.FinishResetVariables:
                            RPCProcedure.finishResetVariables(reader.ReadByte());
                            break;
                        case (byte)CustomRPC.FinishShipStatusBegin:
                            RPCProcedure.finishShipStatusBegin();
                            break;
                        case (byte)CustomRPC.ShareOptions:
                            RPCProcedure.ShareOptions((int)reader.ReadPackedUInt32(), reader);
                            break;
                        case (byte)CustomRPC.StopStart:
                            RPCProcedure.stopStart(reader.ReadByte());
                            break;
                        case (byte)CustomRPC.StopStartSound:
                            RPCProcedure.stopStartSound();
                            break;
                        case (byte)CustomRPC.SetRole:
                            byte roleId = reader.ReadByte();
                            byte playerId = reader.ReadByte();
                            RPCProcedure.setRole(roleId, playerId);
                            break;
                        case (byte)CustomRPC.FinishSetRole:
                            RPCProcedure.finishSetRole();
                            break;
                        case (byte)CustomRPC.SetLovers:
                            RPCProcedure.setLovers(reader.ReadByte(), reader.ReadByte());
                            break;
                        case (byte)CustomRPC.SetCupidLovers:
                            RPCProcedure.setCupidLovers(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                            break;
                        case (byte)CustomRPC.OverrideNativeRole:
                            RPCProcedure.overrideNativeRole(reader.ReadByte(), reader.ReadByte());
                            break;
                        case (byte)CustomRPC.VersionHandshake:
                            int major = reader.ReadPackedInt32();
                            int minor = reader.ReadPackedInt32();
                            int patch = reader.ReadPackedInt32();
                            int versionOwnerId = reader.ReadPackedInt32();
                            byte revision = 0xFF;
                            Guid guid;
                            if (reader.Length - reader.Position >= 17)
                            { // enough bytes left to read
                                revision = reader.ReadByte();
                                // GUID
                                byte[] gbytes = reader.ReadBytes(16);
                                guid = new Guid(gbytes);
                            }
                            else
                            {
                                guid = new Guid(new byte[16]);
                            }
                            RPCProcedure.versionHandshake(major, minor, patch, revision == 0xFF ? -1 : revision, guid, versionOwnerId);
                            break;
                        case (byte)CustomRPC.UseUncheckedVent:
                            int ventId = reader.ReadPackedInt32();
                            byte ventingPlayer = reader.ReadByte();
                            byte isEnter = reader.ReadByte();
                            RPCProcedure.useUncheckedVent(ventId, ventingPlayer, isEnter);
                            break;
                        case (byte)CustomRPC.UncheckedMurderPlayer:
                            byte source = reader.ReadByte();
                            byte target = reader.ReadByte();
                            byte showAnimation = reader.ReadByte();
                            RPCProcedure.uncheckedMurderPlayer(source, target, showAnimation);
                            break;
                        case (byte)CustomRPC.UncheckedExilePlayer:
                            byte exileTarget = reader.ReadByte();
                            RPCProcedure.uncheckedExilePlayer(exileTarget);
                            break;
                        case (byte)CustomRPC.UncheckedCmdReportDeadBody:
                            byte reportSource = reader.ReadByte();
                            byte reportTarget = reader.ReadByte();
                            RPCProcedure.uncheckedCmdReportDeadBody(reportSource, reportTarget);
                            break;
                        case (byte)CustomRPC.UncheckedEndGame:
                            RPCProcedure.uncheckedEndGame(reader.ReadByte());
                            break;
                        case (byte)CustomRPC.UncheckedSetTasks:
                            RPCProcedure.uncheckedSetTasks(reader.ReadByte(), reader.ReadBytesAndSize());
                            break;
                        case (byte)CustomRPC.DynamicMapOption:
                            byte mapId = reader.ReadByte();
                            RPCProcedure.dynamicMapOption(mapId);
                            break;

                        // Role functionality

                        case (byte)CustomRPC.EngineerFixLights:
                            RPCProcedure.engineerFixLights();
                            break;
                        case (byte)CustomRPC.EngineerFixSubmergedOxygen:
                            RPCProcedure.engineerFixSubmergedOxygen();
                            break;
                        case (byte)CustomRPC.EngineerUsedRepair:
                            RPCProcedure.engineerUsedRepair();
                            break;
                        case (byte)CustomRPC.CleanBody:
                            RPCProcedure.cleanBody(reader.ReadByte());
                            break;
                        case (byte)CustomRPC.SheriffKill:
                            RPCProcedure.sheriffKill(reader.ReadByte(), reader.ReadByte(), reader.ReadBoolean());
                            break;
                        case (byte)CustomRPC.TimeMasterRewindTime:
                            RPCProcedure.timeMasterRewindTime();
                            break;
                        case (byte)CustomRPC.TimeMasterShield:
                            RPCProcedure.timeMasterShield();
                            break;
                        case (byte)CustomRPC.MedicSetShielded:
                            RPCProcedure.medicSetShielded(reader.ReadByte());
                            break;
                        case (byte)CustomRPC.ShieldedMurderAttempt:
                            RPCProcedure.shieldedMurderAttempt();
                            break;
                        case (byte)CustomRPC.ShifterShift:
                            RPCProcedure.shifterShift(reader.ReadByte());
                            break;
                        case (byte)CustomRPC.SwapperSwap:
                            byte playerId1 = reader.ReadByte();
                            byte playerId2 = reader.ReadByte();
                            RPCProcedure.swapperSwap(playerId1, playerId2);
                            break;
                        case (byte)CustomRPC.MorphlingMorph:
                            RPCProcedure.morphlingMorph(reader.ReadByte());
                            break;
                        case (byte)CustomRPC.CamouflagerCamouflage:
                            RPCProcedure.camouflagerCamouflage();
                            break;
                        case (byte)CustomRPC.VampireSetBitten:
                            byte bittenId = reader.ReadByte();
                            byte reset = reader.ReadByte();
                            RPCProcedure.vampireSetBitten(bittenId, reset);
                            break;
                        case (byte)CustomRPC.PlaceGarlic:
                            RPCProcedure.placeGarlic(reader.ReadBytesAndSize());
                            break;
                        case (byte)CustomRPC.TrackerUsedTracker:
                            RPCProcedure.trackerUsedTracker(reader.ReadByte());
                            break;
                        case (byte)CustomRPC.JackalCreatesSidekick:
                            RPCProcedure.jackalCreatesSidekick(reader.ReadByte());
                            break;
                        case (byte)CustomRPC.SidekickPromotes:
                            RPCProcedure.sidekickPromotes();
                            break;
                        case (byte)CustomRPC.ErasePlayerRoles:
                            RPCProcedure.erasePlayerRoles(reader.ReadByte());
                            break;
                        case (byte)CustomRPC.SetFutureErased:
                            RPCProcedure.setFutureErased(reader.ReadByte());
                            break;
                        case (byte)CustomRPC.SetFutureShifted:
                            RPCProcedure.setFutureShifted(reader.ReadByte());
                            break;
                        case (byte)CustomRPC.SetFutureShielded:
                            RPCProcedure.setFutureShielded(reader.ReadByte());
                            break;
                        case (byte)CustomRPC.PlaceJackInTheBox:
                            RPCProcedure.placeJackInTheBox(reader.ReadBytesAndSize());
                            break;
                        case (byte)CustomRPC.LightsOut:
                            RPCProcedure.lightsOut();
                            break;
                        case (byte)CustomRPC.PlaceCamera:
                            RPCProcedure.placeCamera(reader.ReadBytesAndSize(), reader.ReadByte());
                            break;
                        case (byte)CustomRPC.SealVent:
                            RPCProcedure.sealVent(reader.ReadPackedInt32());
                            break;
                        case (byte)CustomRPC.ArsonistWin:
                            RPCProcedure.arsonistWin();
                            break;
                        case (byte)CustomRPC.GuesserShoot:
                            byte killerId = reader.ReadByte();
                            byte dyingTarget = reader.ReadByte();
                            byte guessedTarget = reader.ReadByte();
                            byte guessedRoleType = reader.ReadByte();
                            RPCProcedure.guesserShoot(killerId, dyingTarget, guessedTarget, guessedRoleType);
                            break;
                        case (byte)CustomRPC.VultureWin:
                            RPCProcedure.vultureWin();
                            break;
                        case (byte)CustomRPC.LawyerWin:
                            RPCProcedure.lawyerWin();
                            break;
                        case (byte)CustomRPC.LawyerSetTarget:
                            RPCProcedure.lawyerSetTarget(reader.ReadByte());
                            break;
                        case (byte)CustomRPC.LawyerPromotesToPursuer:
                            RPCProcedure.lawyerPromotesToPursuer();
                            break;
                        case (byte)CustomRPC.SetBlanked:
                            var pid = reader.ReadByte();
                            var blankedValue = reader.ReadByte();
                            RPCProcedure.setBlanked(pid, blankedValue);
                            break;
                        case (byte)CustomRPC.SetFutureSpelled:
                            RPCProcedure.setFutureSpelled(reader.ReadByte());
                            break;
                        case (byte)CustomRPC.WitchSpellCast:
                            RPCProcedure.witchSpellCast(reader.ReadByte());
                            break;
                        case (byte)CustomRPC.PlaceAssassinTrace:
                            RPCProcedure.placeAssassinTrace(reader.ReadBytesAndSize());
                            break;

                        // GM functionality
                        case (byte)CustomRPC.AddModifier:
                            RPCProcedure.addModifier(reader.ReadByte(), reader.ReadByte());
                            break;
                        case (byte)CustomRPC.SetShifterType:
                            RPCProcedure.setShifterType(reader.ReadBoolean());
                            break;
                        case (byte)CustomRPC.NinjaStealth:
                            RPCProcedure.ninjaStealth(reader.ReadByte(), reader.ReadBoolean());
                            break;
                        case (byte)CustomRPC.ArsonistDouse:
                            RPCProcedure.arsonistDouse(reader.ReadByte());
                            break;
                        case (byte)CustomRPC.VultureEat:
                            RPCProcedure.vultureEat(reader.ReadByte());
                            break;

                        case (byte)CustomRPC.GMKill:
                            RPCProcedure.GMKill(reader.ReadByte());
                            break;
                        case (byte)CustomRPC.GMRevive:
                            RPCProcedure.GMRevive(reader.ReadByte());
                            break;
                        case (byte)CustomRPC.UseAdminTime:
                            RPCProcedure.useAdminTime(reader.ReadSingle());
                            break;
                        case (byte)CustomRPC.UseCameraTime:
                            RPCProcedure.useCameraTime(reader.ReadSingle());
                            break;
                        case (byte)CustomRPC.UseVitalsTime:
                            RPCProcedure.useVitalsTime(reader.ReadSingle());
                            break;
                        case (byte)CustomRPC.PlagueDoctorWin:
                            RPCProcedure.plagueDoctorWin();
                            break;
                        case (byte)CustomRPC.PlagueDoctorSetInfected:
                            RPCProcedure.plagueDoctorInfected(reader.ReadByte());
                            break;
                        case (byte)CustomRPC.PlagueDoctorUpdateProgress:
                            byte progressTarget = reader.ReadByte();
                            byte[] progressByte = reader.ReadBytes(4);
                            float progress = System.BitConverter.ToSingle(progressByte, 0);
                            RPCProcedure.plagueDoctorProgress(progressTarget, progress);
                            break;
                        case (byte)CustomRPC.NekoKabochaExile:
                            RPCProcedure.nekoKabochaExile(reader.ReadByte());
                            break;
                        case (byte)CustomRPC.SerialKillerSuicide:
                            RPCProcedure.serialKillerSuicide(reader.ReadByte());
                            break;
                        case (byte)CustomRPC.SwapperAnimate:
                            RPCProcedure.swapperAnimate();
                            break;
                        case (byte)CustomRPC.EvilHackerCreatesMadmate:
                            RPCProcedure.evilHackerCreatesMadmate(reader.ReadByte());
                            break;
                        case (byte)CustomRPC.FortuneTellerUsedDivine:
                            byte fId = reader.ReadByte();
                            byte tId = reader.ReadByte();
                            RPCProcedure.fortuneTellerUsedDivine(fId, tId);
                            break;
                        case (byte)CustomRPC.FoxStealth:
                            RPCProcedure.foxStealth(reader.ReadByte(), reader.ReadBoolean());
                            break;
                        case (byte)CustomRPC.FoxCreatesImmoralist:
                            RPCProcedure.foxCreatesImmoralist(reader.ReadByte());
                            break;
                        case (byte)CustomRPC.AkujoSetHonmei:
                            RPCProcedure.akujoSetHonmei(reader.ReadByte(), reader.ReadByte());
                            break;
                        case (byte)CustomRPC.AkujoSetKeep:
                            RPCProcedure.akujoSetKeep(reader.ReadByte(), reader.ReadByte());
                            break;
                        case (byte)CustomRPC.AkujoSuicide:
                            RPCProcedure.akujoSuicide(reader.ReadByte());
                            break;
                        case (byte)CustomRPC.CupidSuicide:
                            RPCProcedure.cupidSuicide(reader.ReadByte(), reader.ReadBoolean(), reader.ReadBoolean());
                            break;
                        case (byte)CustomRPC.SetCupidShield:
                            RPCProcedure.setCupidShield(reader.ReadByte(), reader.ReadByte());
                            break;
                        case (byte)CustomRPC.ImpostorPromotesToLastImpostor:
                            RPCProcedure.impostorPromotesToLastImpostor(reader.ReadByte());
                            break;
                        case (byte)CustomRPC.SchrodingersCatSuicide:
                            RPCProcedure.schrodingersCatSuicide();
                            break;
                        case (byte)CustomRPC.SchrodingersCatSetTeam:
                            RPCProcedure.schrodingersCatSetTeam(reader.ReadByte());
                            break;
                        case (byte)CustomRPC.PlaceTrap:
                            RPCProcedure.placeTrap(reader.ReadBytesAndSize());
                            break;
                        case (byte)CustomRPC.ClearTrap:
                            RPCProcedure.clearTrap();
                            break;
                        case (byte)CustomRPC.ActivateTrap:
                            RPCProcedure.activateTrap(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                            break;
                        case (byte)CustomRPC.DisableTrap:
                            RPCProcedure.disableTrap(reader.ReadByte());
                            break;
                        case (byte)CustomRPC.TrapperKill:
                            RPCProcedure.trapperKill(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                            break;
                        case (byte)CustomRPC.TrapperMeetingFlag:
                            RPCProcedure.trapperMeetingFlag();
                            break;
                        case (byte)CustomRPC.RandomSpawn:
                            byte pId = reader.ReadByte();
                            byte locId = reader.ReadByte();
                            RPCProcedure.randomSpawn(pId, locId);
                            break;
                        case (byte)CustomRPC.PlantBomb:
                            RPCProcedure.plantBomb(reader.ReadByte());
                            break;
                        case (byte)CustomRPC.ReleaseBomb:
                            RPCProcedure.releaseBomb(reader.ReadByte(), reader.ReadByte());
                            break;
                        case (byte)CustomRPC.BomberKill:
                            RPCProcedure.bomberKill(reader.ReadByte(), reader.ReadByte());
                            break;
                        case (byte)CustomRPC.SpawnDummy:
                            byte newId = reader.ReadByte();
                            byte[] spawnTmp = reader.ReadBytes(4);
                            float spawnX = System.BitConverter.ToSingle(spawnTmp, 0);
                            spawnTmp = reader.ReadBytes(4);
                            float spawnY = System.BitConverter.ToSingle(spawnTmp, 0);
                            spawnTmp = reader.ReadBytes(4);
                            float spawnZ = System.BitConverter.ToSingle(spawnTmp, 0);
                            RPCProcedure.spawnDummy(newId, new Vector3(spawnX, spawnY, spawnZ));
                            break;
                        case (byte)CustomRPC.MoveDummy:
                            byte[] moveTmp = reader.ReadBytes(4);
                            float moveX = System.BitConverter.ToSingle(moveTmp, 0);
                            moveTmp = reader.ReadBytes(4);
                            float moveY = System.BitConverter.ToSingle(moveTmp, 0);
                            moveTmp = reader.ReadBytes(4);
                            float moveZ = System.BitConverter.ToSingle(moveTmp, 0);
                            bool spawn = reader.ReadBoolean();
                            RPCProcedure.moveDummy(new Vector3(moveX, moveY, moveZ), spawn);
                            break;
                        case (byte)CustomRPC.WalkDummy:
                            byte[] walkTmp = reader.ReadBytes(4);
                            float walkX = System.BitConverter.ToSingle(walkTmp, 0);
                            walkTmp = reader.ReadBytes(4);
                            float walkY = System.BitConverter.ToSingle(walkTmp, 0);
                            RPCProcedure.walkDummy(new Vector3(walkX, walkY, 0f));
                            break;
                        case (byte)CustomRPC.PuppeteerStealth:
                            RPCProcedure.puppeteerStealth(reader.ReadBoolean());
                            break;
                        case (byte)CustomRPC.PuppeteerMorph:
                            RPCProcedure.puppeteerMorph(reader.ReadByte());
                            break;
                        case (byte)CustomRPC.PuppeteerKill:
                            RPCProcedure.puppeteerKill(reader.ReadByte(), reader.ReadByte());
                            break;
                        case (byte)CustomRPC.PuppeteerWin:
                            RPCProcedure.puppeteerWin();
                            break;
                        case (byte)CustomRPC.PuppeteerClimbRadder:
                            RPCProcedure.puppeteerClimbRadder(reader.ReadByte(), reader.ReadByte());
                            break;
                        case (byte)CustomRPC.PuppeteerUsePlatform:
                            RPCProcedure.puppeteerUsePlatform(reader.ReadByte());
                            break;
                        case (byte)CustomRPC.mimicMorph:
                            RPCProcedure.mimicMorph(reader.ReadByte(), reader.ReadByte());
                            break;
                        case (byte)CustomRPC.mimicResetMorph:
                            RPCProcedure.mimicResetMorph(reader.ReadByte());
                            break;
                        case (byte)CustomRPC.Synchronize:
                            RPCProcedure.synchronize(reader.ReadByte(), reader.ReadInt32());
                            break;
                        case (byte)CustomRPC.SetOddIsJekyll:
                            RPCProcedure.setOddIsJekyll(reader.ReadBoolean());
                            break;
                        case (byte)CustomRPC.ShareRealTasks:
                            RPCProcedure.shareRealTasks(reader);
                            break;
                        case (byte)CustomRPC.SyncKillTimer:
                            byte impostorId = reader.ReadByte();
                            byte[] timerTmp = reader.ReadBytes(4);
                            float timer = System.BitConverter.ToSingle(timerTmp, 0);
                            RPCProcedure.syncKillTimer(impostorId, timer);
                            break;
                        case (byte)CustomRPC.SetBrainwash:
                            RPCProcedure.setBrainwash(reader.ReadByte());
                            break;
                        case (byte)CustomRPC.MoriartyKill:
                            RPCProcedure.moriartyKill(reader.ReadByte());
                            break;
                        case (byte)CustomRPC.WorkaroundSetRoles:
                            RPCProcedure.workaroundSetRoles(reader.ReadByte(), reader);
                            break;
                        case (byte)CustomRPC.YoyoMarkLocation:
                            RPCProcedure.yoyoMarkLocation(reader.ReadBytesAndSize());
                            break;
                        case (byte)CustomRPC.YoyoBlink:
                            RPCProcedure.yoyoBlink(reader.ReadByte() == byte.MaxValue, reader.ReadBytesAndSize());
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Patches.Logger.error($"CallID:{callId} {ex}", "CustomRPC");
                }
            }
        }
    }
}
