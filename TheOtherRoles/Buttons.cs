using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Hazel;
using Il2CppSystem.Runtime.Remoting.Messaging;
using Reactor.Utilities;
using TheOtherRoles.CustomGameModes;
using TheOtherRoles.MetaContext;
using TheOtherRoles.Modules;
using TheOtherRoles.Objects;
using TheOtherRoles.Patches;
using TheOtherRoles.Roles;
using TheOtherRoles.Utilities;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    static class HudManagerStartPatch
    {
        private static bool initialized = false;

        private static CustomButton engineerRepairButton;
        public static CustomButton sheriffKillButton;
        private static CustomButton deputyHandcuffButton;
        private static CustomButton timeMasterShieldButton;
        private static CustomButton medicShieldButton;
        private static CustomButton shifterShiftButton;
        private static CustomButton morphlingButton;
        private static CustomButton camouflagerButton;
        private static CustomButton portalmakerPlacePortalButton;
        private static CustomButton usePortalButton;
        private static CustomButton portalmakerMoveToPortalButton;
        private static CustomButton hackerButton;
        public static CustomButton hackerVitalsButton;
        public static CustomButton hackerAdminTableButton;
        private static CustomButton trackerTrackPlayerButton;
        private static CustomButton trackerTrackCorpsesButton;
        public static CustomButton trackerKillButton;
        public static CustomButton vampireKillButton;
        public static CustomButton garlicButton;
        public static CustomButton jackalKillButton;
        public static CustomButton sidekickKillButton;
        private static CustomButton jackalSidekickButton;
        private static CustomButton eraserButton;
        private static CustomButton placeJackInTheBoxButton;        
        private static CustomButton lightsOutButton;
        public static CustomButton cleanerCleanButton;
        public static CustomButton warlockCurseButton;
        public static CustomButton securityGuardButton;
        public static CustomButton securityGuardCamButton;
        public static CustomButton securityGuardFlushButton;
        public static CustomButton arsonistButton;
        public static CustomButton serialKillerButton;
        public static CustomButton vultureEatButton;
        public static CustomButton mediumButton;
        public static CustomButton pursuerButton;
        public static CustomButton witchSpellButton;
        public static CustomButton assassinButton;
        public static CustomButton mayorMeetingButton;
        public static CustomButton thiefKillButton;
        public static CustomButton ninjaButton;
        public static CustomButton evilTrackerButton;
        public static CustomButton sprintButton;
        public static List<CustomButton> fortuneTellerButtons;
        public static CustomButton fortuneTellerLeftButton;
        public static CustomButton fortuneTellerRightButton;
        public static CustomButton veteranAlertButton;
        public static CustomButton undertakerDragButton;
        public static CustomButton mimicAAdminButton;
        public static CustomButton mimicAMorphButton;
        public static CustomButton sherlockInvestigateButton;
        public static CustomButton sherlockWatchButton;
        public static CustomButton bomberAPlantBombButton;
        public static CustomButton bomberAReleaseBombButton;
        public static CustomButton bomberBPlantBombButton;
        public static CustomButton bomberBReleaseBombButton;
        public static CustomButton jackalAndSidekickSabotageLightsButton;
        public static CustomButton evilHackerButton;
        public static CustomButton evilHackerCreatesMadmateButton;
        public static CustomButton pelicanKillButton;
        public static CustomButton trapperSetTrapButton;
        public static CustomButton blackmailerButton;
        public static CustomButton moriartyBrainwashButton;
        public static CustomButton moriartyKillButton;
        public static CustomButton akujoHonmeiButton;
        public static CustomButton akujoBackupButton;
        public static CustomButton plagueDoctorButton;
        public static CustomButton lighterButton;
        public static CustomButton jekyllAndHydeKillButton;
        public static CustomButton jekyllAndHydeDrugButton;
        public static CustomButton jekyllAndHydeSuicideButton;
        public static CustomButton teleporterTeleportButton;
        public static CustomButton detectiveButton;
        public static CustomButton kataomoiButton;
        public static CustomButton kataomoiStalkingButton;
        public static CustomButton kataomoiSearchButton;
        public static CustomButton cupidArrowButton;
        public static CustomButton cupidShieldButton;
        public static CustomButton foxStealthButton;
        public static CustomButton foxRepairButton;
        public static CustomButton foxImmoralistButton;
        public static CustomButton immoralistButton;
        public static CustomButton buskerButton;
        public static CustomButton noisemakerButton;
        public static CustomButton schrodingersCatKillButton;
        public static CustomButton schrodingersCatSwitchButton;
        public static CustomButton yoyoButton;
        public static CustomButton archaeologistDetectButton;
        public static CustomButton archaeologistExcavateButton;
        public static CustomButton medicVitalsButton;
        public static CustomButton zephyrButton;
        public static CustomButton jailorButton;
        public static CustomButton doomsayerButton;
        public static CustomButton yandereButton;
        public static CustomButton baitButton;
        public static CustomButton collatorButton;
        public static CustomButton operateButton;
        public static CustomButton watcherKillButton;
        public static CustomButton freePlaySuicideButton;
        public static CustomButton freePlayReviveButton;
        public static CustomButton eventKickButton;
        //public static CustomButton trapperButton;
        //public static CustomButton bomberButton;
        //public static CustomButton defuseButton;
        public static CustomButton zoomOutButton;
        //public static CustomButton roleSummaryButton;
        public static CustomButton accelAttributeButton;
        public static CustomButton decelAttributeButton;

        // PropHunt Buttons
        public static CustomButton propDisguiseButton;
        public static CustomButton propHuntUnstuckButton;
        public static CustomButton propHuntRevealButton;
        public static CustomButton propHuntInvisButton;
        public static CustomButton propHuntSpeedboostButton;
        public static CustomButton propHuntAdminButton;
        public static CustomButton propHuntFindButton;
        private static GameObject propSpriteHolder;
        private static SpriteRenderer propSpriteRenderer;

        public static Dictionary<byte, List<CustomButton>> deputyHandcuffedButtons = null;
        public static PoolablePlayer targetDisplay;

        public static TMPro.TMP_Text securityGuardButtonScrewsText;
        public static TMPro.TMP_Text securityGuardChargesText;
        public static TMPro.TMP_Text deputyButtonHandcuffsText;
        public static TMPro.TMP_Text tricksterBoxesText;
        public static TMPro.TMP_Text engineerRepairText;
        public static TMPro.TMP_Text foxRepairText;
        public static TMPro.TMP_Text vultureRemainingText;
        public static TMPro.TMP_Text pursuerButtonBlanksText;
        public static TMPro.TMP_Text veteranButtonAlertText;
        public static TMPro.TMP_Text hackerAdminTableChargesText;
        public static TMPro.TMP_Text hackerVitalsChargesText;
        public static TMPro.TMP_Text sherlockNumInvestigateText;
        public static TMPro.TMP_Text sherlockNumKillTimerText;
        public static TMPro.TMP_Text moriartyKillCounterText;
        public static TMPro.TMP_Text jekyllAndHydeKillCounterText;
        public static TMPro.TMP_Text jekyllAndHydeDrugText;
        public static TMPro.TMP_Text trackerText;
        public static TMPro.TMP_Text doomsayerUsesText;
        public static TMPro.TMP_Text zephyrUsesText;
        public static TMPro.TMP_Text collatorUsesText;
        public static TMPro.TMP_Text jailorUsesText;
        public static TMPro.TMP_Text baitUsesText;
        public static TMPro.TMP_Text akujoTimeRemainingText;
        public static TMPro.TMP_Text akujoHonmeiText;
        public static TMPro.TMP_Text akujoBackupLeftText;
        public static TMPro.TMP_Text cupidTimeRemainingText;
        public static TMPro.TMP_Text cupidLoversText;
        public static TMPro.TMP_Text plagueDoctornumInfectionsText;
        public static TMPro.TMP_Text teleporterNumLeftText;
        public static TMPro.TMP_Text noisemakerButtonText;
        //public static TMPro.TMP_Text trapperChargesText;
        public static TMPro.TMP_Text portalmakerButtonText1;
        public static TMPro.TMP_Text portalmakerButtonText2;
        public static TMPro.TMP_Text portalmakerButtonNumText;

        public static Props.Proptip accelAttributePropTip;
        public static Props.Proptip decelAttributePropTip;

        public static void setCustomButtonCooldowns() {
            if (!initialized) {
                try {
                    createButtonsPostfix(HudManager.Instance);
                } 
                catch {
                    TheOtherRolesPlugin.Logger.LogWarning("Button cooldowns not set, either the gamemode does not require them or there's something wrong.");
                    return;
                }
            }
            engineerRepairButton.MaxTimer = 0f;
            sheriffKillButton.MaxTimer = Sheriff.cooldown;
            deputyHandcuffButton.MaxTimer = Deputy.handcuffCooldown;
            timeMasterShieldButton.MaxTimer = TimeMaster.cooldown;
            medicShieldButton.MaxTimer = 0f;
            shifterShiftButton.MaxTimer = 0f;
            morphlingButton.MaxTimer = Morphling.cooldown;
            camouflagerButton.MaxTimer = Camouflager.cooldown;
            portalmakerPlacePortalButton.MaxTimer = Portalmaker.cooldown;
            usePortalButton.MaxTimer = Portalmaker.usePortalCooldown;
            portalmakerMoveToPortalButton.MaxTimer = Portalmaker.usePortalCooldown;
            hackerButton.MaxTimer = Hacker.cooldown;
            hackerVitalsButton.MaxTimer = Hacker.cooldown;
            hackerAdminTableButton.MaxTimer = Hacker.cooldown;
            vampireKillButton.MaxTimer = Vampire.cooldown;
            trackerTrackPlayerButton.MaxTimer = 0f;
            trackerKillButton.MaxTimer = Tracker.killCooldown;
            garlicButton.MaxTimer = 0f;
            jackalKillButton.MaxTimer = Jackal.cooldown;
            sidekickKillButton.MaxTimer = Sidekick.cooldown;
            jackalSidekickButton.MaxTimer = Jackal.createSidekickCooldown;
            eraserButton.MaxTimer = Eraser.cooldown;
            placeJackInTheBoxButton.MaxTimer = Trickster.placeBoxCooldown;
            lightsOutButton.MaxTimer = Trickster.lightsOutCooldown;
            cleanerCleanButton.MaxTimer = Cleaner.cooldown;
            warlockCurseButton.MaxTimer = Warlock.cooldown;
            yoyoButton.MaxTimer = Yoyo.markCooldown;
            pelicanKillButton.MaxTimer = Pelican.cooldown;
            securityGuardButton.MaxTimer = SecurityGuard.cooldown;
            securityGuardCamButton.MaxTimer = SecurityGuard.cooldown;
            securityGuardFlushButton.MaxTimer = SecurityGuard.flushCooldown;
            medicVitalsButton.MaxTimer = 0f;
            zephyrButton.MaxTimer = Zephyr.cooldown;
            lighterButton.MaxTimer = Lighter.cooldown;
            arsonistButton.MaxTimer = Arsonist.cooldown;
            kataomoiButton.MaxTimer = Kataomoi.stareCooldown;
            kataomoiStalkingButton.MaxTimer = Kataomoi.stalkingCooldown;
            kataomoiSearchButton.MaxTimer = Kataomoi.searchCooldown;
            vultureEatButton.MaxTimer = Vulture.cooldown;
            watcherKillButton.MaxTimer = 5f;
            detectiveButton.MaxTimer = Detective.inspectCooldown;
            mediumButton.MaxTimer = Medium.cooldown;
            pursuerButton.MaxTimer = Pursuer.cooldown;
            trackerTrackCorpsesButton.MaxTimer = Tracker.corpsesTrackingCooldown;
            baitButton.MaxTimer = Bait.cooldown;
            witchSpellButton.MaxTimer = Witch.cooldown;
            assassinButton.MaxTimer = Assassin.cooldown;
            thiefKillButton.MaxTimer = Thief.cooldown;
            mayorMeetingButton.MaxTimer = GameManager.Instance.LogicOptions.GetEmergencyCooldown();
            yandereButton.MaxTimer = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown;
            ninjaButton.MaxTimer = Ninja.stealthCooldown;
            serialKillerButton.MaxTimer = SerialKiller.suicideTimer;
            archaeologistDetectButton.MaxTimer = Archaeologist.cooldown;
            archaeologistExcavateButton.MaxTimer = Archaeologist.cooldown;
            doomsayerButton.MaxTimer = Doomsayer.cooldown;
            collatorButton.MaxTimer = Collator.cooldown;
            jailorButton.MaxTimer = Jailor.cooldown;
            foreach (var button in fortuneTellerButtons)
            {
                button.MaxTimer = 0f;
                button.Timer = 0f;
            }
            fortuneTellerLeftButton.MaxTimer = 0f;
            fortuneTellerLeftButton.Timer = 0f;
            fortuneTellerRightButton.MaxTimer = 0f;
            fortuneTellerRightButton.Timer = 0f;
            //serialKillerButton.MaxTimer = 0f;
            evilTrackerButton.MaxTimer = EvilTracker.cooldown;
            trapperSetTrapButton.MaxTimer = Trapper.cooldown;
            sprintButton.MaxTimer = Sprinter.sprintCooldown;
            veteranAlertButton.MaxTimer = Veteran.cooldown;
            plagueDoctorButton.MaxTimer = PlagueDoctor.infectCooldown;
            teleporterTeleportButton.MaxTimer = Teleporter.teleportCooldown;
            undertakerDragButton.MaxTimer = 0f;
            mimicAAdminButton.MaxTimer = 0f;
            mimicAMorphButton.MaxTimer = 0f;
            sherlockInvestigateButton.MaxTimer = Sherlock.cooldown;
            sherlockWatchButton.MaxTimer = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown;
            bomberAPlantBombButton.MaxTimer = BomberA.cooldown;
            bomberBPlantBombButton.MaxTimer = BomberA.cooldown;
            bomberAReleaseBombButton.MaxTimer = 0f;
            bomberBReleaseBombButton.MaxTimer = 0f;
            evilHackerButton.MaxTimer = 0f;
            evilHackerCreatesMadmateButton.MaxTimer = 0f;
            moriartyBrainwashButton.MaxTimer = Moriarty.brainwashCooldown;
            moriartyKillButton.MaxTimer = 0f;
            noisemakerButton.MaxTimer = Noisemaker.cooldown;
            schrodingersCatKillButton.MaxTimer = SchrodingersCat.killCooldown;
            schrodingersCatSwitchButton.MaxTimer = 0f;
            schrodingersCatSwitchButton.Timer = 0f;
            zephyrButton.MaxTimer = Zephyr.cooldown;
            //roleSummaryButton.Timer = 0f;
            //roleSummaryButton.MaxTimer = 0f;
            cupidArrowButton.MaxTimer = 0f;
            cupidShieldButton.MaxTimer = 0f;
            accelAttributeButton.MaxTimer = 0f;
            accelAttributeButton.Timer = 0f;
            decelAttributeButton.MaxTimer = 0f;
            decelAttributeButton.Timer = 0f;
            blackmailerButton.MaxTimer = Blackmailer.cooldown;
            jekyllAndHydeKillButton.MaxTimer = JekyllAndHyde.cooldown;
            jekyllAndHydeSuicideButton.MaxTimer = JekyllAndHyde.suicideTimer;
            jekyllAndHydeDrugButton.MaxTimer = 0f;
            jekyllAndHydeDrugButton.Timer = 0f;
            akujoHonmeiButton.MaxTimer = 0f;
            akujoBackupButton.MaxTimer = 0f;
            foxStealthButton.MaxTimer = Fox.stealthCooldown;
            foxRepairButton.MaxTimer = 0f;
            foxImmoralistButton.MaxTimer = 20f;
            immoralistButton.MaxTimer = 20f;
            buskerButton.MaxTimer = Busker.cooldown;
            jackalAndSidekickSabotageLightsButton.MaxTimer = 0f;
            operateButton.MaxTimer = 0f;
            operateButton.Timer = 0f;
            freePlayReviveButton.MaxTimer = 0f;
            freePlayReviveButton.Timer = 0f;
            freePlaySuicideButton.MaxTimer = 0f;
            freePlaySuicideButton.Timer = 0f;
            //trapperButton.MaxTimer = Trapper.cooldown;
            //bomberButton.MaxTimer = Bomber.bombCooldown;
            //defuseButton.MaxTimer = 0f;
            //defuseButton.Timer = 0f;

            propDisguiseButton.MaxTimer = 1f;
            propHuntUnstuckButton.MaxTimer = PropPH.unstuckCooldown;
            propHuntRevealButton.MaxTimer = HunterPH.revealCooldown;
            propHuntInvisButton.MaxTimer = PropPH.invisCooldown;
            propHuntSpeedboostButton.MaxTimer = PropPH.speedboostCooldown;
            propHuntAdminButton.MaxTimer = HunterPH.adminCooldown;
            propHuntFindButton.MaxTimer = HunterPH.findCooldown;

            timeMasterShieldButton.EffectDuration = TimeMaster.shieldDuration;
            hackerButton.EffectDuration = Hacker.duration;
            hackerVitalsButton.EffectDuration = Hacker.duration;
            hackerAdminTableButton.EffectDuration = Hacker.duration;
            vampireKillButton.EffectDuration = Vampire.delay;
            camouflagerButton.EffectDuration = Camouflager.duration;
            morphlingButton.EffectDuration = Morphling.duration;
            lightsOutButton.EffectDuration = Trickster.lightsOutDuration;
            arsonistButton.EffectDuration = Arsonist.duration;
            mediumButton.EffectDuration = Medium.duration;
            trackerTrackCorpsesButton.EffectDuration = Tracker.corpsesTrackingDuration;
            witchSpellButton.EffectDuration = Witch.spellCastingDuration;
            securityGuardCamButton.EffectDuration = SecurityGuard.duration;
            ninjaButton.EffectDuration = Ninja.stealthDuration;
            sprintButton.EffectDuration = Sprinter.sprintDuration;
            kataomoiButton.EffectDuration = Kataomoi.stareDuration;
            kataomoiStalkingButton.EffectDuration = Kataomoi.stalkingDuration;
            kataomoiSearchButton.EffectDuration = Kataomoi.searchDuration;
            bomberAPlantBombButton.EffectDuration = BomberA.duration;
            bomberBPlantBombButton.EffectDuration = BomberA.duration;
            archaeologistDetectButton.EffectDuration = Archaeologist.arrowDuration;
            archaeologistExcavateButton.EffectDuration = Archaeologist.detectDuration;
            //serialKillerButton.EffectDuration = SerialKiller.suicideTimer;
            veteranAlertButton.EffectDuration = Veteran.alertDuration;
            lighterButton.EffectDuration = Lighter.duration;
            foxStealthButton.EffectDuration = Fox.stealthDuration;
            detectiveButton.EffectDuration = Detective.inspectDuration;
            buskerButton.EffectDuration = Busker.duration;
            propHuntUnstuckButton.EffectDuration = PropPH.unstuckDuration;
            propHuntRevealButton.EffectDuration = HunterPH.revealDuration;
            propHuntInvisButton.EffectDuration = PropPH.invisDuration;
            propHuntSpeedboostButton.EffectDuration = PropPH.speedboostDuration;
            propHuntAdminButton.EffectDuration = HunterPH.adminDuration;
            propHuntFindButton.EffectDuration = HunterPH.findDuration;
            //defuseButton.EffectDuration = Bomber.defuseDuration;
            //bomberButton.EffectDuration = Bomber.destructionTime + Bomber.bombActiveAfter;
            // Already set the timer to the max, as the button is enabled during the game and not available at the start
            lightsOutButton.Timer = lightsOutButton.MaxTimer;
            zoomOutButton.MaxTimer = 0f;
        }

        public static void resetTimeMasterButton() {
            timeMasterShieldButton.Timer = timeMasterShieldButton.MaxTimer;
            timeMasterShieldButton.isEffectActive = false;
            timeMasterShieldButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
            SoundEffectsManager.stop("timemasterShield");
        }

        public static void resetHuntedRewindButton() {
            huntedShieldButton.Timer = huntedShieldButton.MaxTimer;
            huntedShieldButton.isEffectActive = false;
            huntedShieldButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
            SoundEffectsManager.stop("timemasterShield");
        }

        private static void addReplacementHandcuffedButton(CustomButton button, Vector3? positionOffset = null, Func<bool> couldUse = null)
        {
            Vector3 positionOffsetValue = positionOffset ?? button.PositionOffset;  // For non custom buttons, we can set these manually.
            positionOffsetValue.z = -0.1f;
            couldUse ??= button.CouldUse;
            CustomButton replacementHandcuffedButton = new(() => { }, () => { return true; }, couldUse, () => { }, Deputy.getHandcuffedButtonSprite(), positionOffsetValue, button.hudManager, button.hotkey,
                true, Deputy.handcuffDuration, () => { }, button.mirror);
            replacementHandcuffedButton.actionButtonGameObject.ForEachChild((Il2CppSystem.Action<GameObject>)((c) => { if (c.name.Equals("HotKeyGuide")) GameObject.Destroy(c); }));
            ButtonEffect.SetMouseActionIcon(replacementHandcuffedButton.actionButtonGameObject, true, ModTranslation.getString("buttonsHandcuffed"), false);
            replacementHandcuffedButton.Timer = replacementHandcuffedButton.EffectDuration;
            replacementHandcuffedButton.actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
            replacementHandcuffedButton.isEffectActive = true;
            if (deputyHandcuffedButtons.ContainsKey(PlayerControl.LocalPlayer.PlayerId))
                deputyHandcuffedButtons[PlayerControl.LocalPlayer.PlayerId].Add(replacementHandcuffedButton);
            else
                deputyHandcuffedButtons.Add(PlayerControl.LocalPlayer.PlayerId, new List<CustomButton> { replacementHandcuffedButton });
        }
        
        // Disables / Enables all Buttons (except the ones disabled in the Deputy class), and replaces them with new buttons.
        public static void setAllButtonsHandcuffedStatus(bool handcuffed, bool reset = false)
        {
            if (reset) {
                deputyHandcuffedButtons = new Dictionary<byte, List<CustomButton>>();
                return;
            }
            if (handcuffed && !deputyHandcuffedButtons.ContainsKey(PlayerControl.LocalPlayer.PlayerId))
            {
                int maxI = CustomButton.buttons.Count;
                for (int i = 0; i < maxI; i++)
                {
                    try
                    {
                        if (CustomButton.buttons[i].HasButton() && !CustomButton.buttons[i].isSuicide)  // For each custombutton the player has
                        {
                            addReplacementHandcuffedButton(CustomButton.buttons[i]);  // The new buttons are the only non-handcuffed buttons now!
                        }
                        if (!CustomButton.buttons[i].isSuicide) CustomButton.buttons[i].isHandcuffed = true;
                    }
                    catch (NullReferenceException)
                    {
                        System.Console.WriteLine("[WARNING] NullReferenceException from MeetingEndedUpdate().HasButton(), if theres only one warning its fine");  // Note: idk what this is good for, but i copied it from above /gendelo
                    }
                }
                // Non Custom (Vanilla) Buttons. The Originals are disabled / hidden in UpdatePatch.cs already, just need to replace them. Can use any button, as we replace onclick etc anyways.
                // Kill Button if enabled for the Role
                if (FastDestroyableSingleton<HudManager>.Instance.KillButton.isActiveAndEnabled) addReplacementHandcuffedButton(arsonistButton, CustomButton.ButtonPositions.upperRowRight, couldUse: () => { return FastDestroyableSingleton<HudManager>.Instance.KillButton.currentTarget != null; });
                // Vent Button if enabled
                if (PlayerControl.LocalPlayer.roleCanUseVents()) addReplacementHandcuffedButton(arsonistButton, FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.transform.localPosition, couldUse: () => { return FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.currentTarget != null; });
                // Report Button
                addReplacementHandcuffedButton(arsonistButton, (!PlayerControl.LocalPlayer.Data.Role.IsImpostor) ? new Vector3(-1f, -0.06f, 0): CustomButton.ButtonPositions.lowerRowRight, () => { return FastDestroyableSingleton<HudManager>.Instance.ReportButton.graphic.color == Palette.EnabledColor; });
            }
            else if (!handcuffed && deputyHandcuffedButtons.ContainsKey(PlayerControl.LocalPlayer.PlayerId))  // Reset to original. Disables the replacements, enables the original buttons.
            {
                foreach (CustomButton replacementButton in deputyHandcuffedButtons[PlayerControl.LocalPlayer.PlayerId])
                {
                    replacementButton.HasButton = () => { return false; };
                    replacementButton.Update(); // To make it disappear properly.
                    ButtonEffect.SetMouseActionIcon(replacementButton.actionButtonGameObject, false);
                    CustomButton.buttons.Remove(replacementButton);
                }
                deputyHandcuffedButtons.Remove(PlayerControl.LocalPlayer.PlayerId);

                foreach (CustomButton button in CustomButton.buttons)
                {
                    button.isHandcuffed = false;
                }
            }
        }

        private static void setButtonTargetDisplay(PlayerControl target, CustomButton button = null, Vector3? offset=null) {
            if (target == null || button == null) {
                if (targetDisplay != null) {  // Reset the poolable player
                    targetDisplay.gameObject.SetActive(false);
                    GameObject.Destroy(targetDisplay.gameObject);
                    targetDisplay = null;
                }
                return;
            }
            // Add poolable player to the button so that the target outfit is shown
            button.actionButton.cooldownTimerText.transform.localPosition = new Vector3(0, 0, -1f);  // Before the poolable player
            targetDisplay = UnityEngine.Object.Instantiate<PoolablePlayer>(Patches.IntroCutsceneOnDestroyPatch.playerPrefab, button.actionButton.transform);
            NetworkedPlayerInfo data = target.Data;
            target.SetPlayerMaterialColors(targetDisplay.cosmetics.currentBodySprite.BodySprite);
            targetDisplay.SetSkin(data.DefaultOutfit.SkinId, data.DefaultOutfit.ColorId);
            targetDisplay.SetHat(data.DefaultOutfit.HatId, data.DefaultOutfit.ColorId);
            targetDisplay.cosmetics.nameText.text = "";  // Hide the name!
            targetDisplay.transform.localPosition = new Vector3(0f, 0.22f, -0.01f);
            if (offset != null) targetDisplay.transform.localPosition += (Vector3)offset;
            targetDisplay.transform.localScale = Vector3.one * 0.33f;
            targetDisplay.setSemiTransparent(false);
            targetDisplay.gameObject.SetActive(true);
        }
        public static void Prefix(HudManager __instance)
        {
            TORGameManager.Instance?.Abandon();
        }

        public static void Postfix(HudManager __instance) {
            initialized = false;

            try
            {
                _ = new TORGameManager();
                createButtonsPostfix(__instance);
            } catch { }
        }
         
        public static void createButtonsPostfix(HudManager __instance) {
            // get map id, or raise error to wait...
            var mapId = GameOptionsManager.Instance.currentNormalGameOptions.MapId;

            // Engineer Repair
            engineerRepairButton = new CustomButton(
                () => {
                    engineerRepairButton.Timer = 0f;
                    Engineer.local.remainingFixes--;
                    _ = new StaticAchievementToken("engineer.common1");
                    Engineer.UseRepair.Invoke();
                    SoundEffectsManager.play("engineerRepair");
                    foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks.GetFastEnumerator()) {
                        if (task.TaskType == TaskTypes.FixLights) {
                            Engineer.FixLights.Invoke();
                        } else if (task.TaskType == TaskTypes.RestoreOxy) {
                            MapUtilities.CachedShipStatus.RpcUpdateSystem(SystemTypes.LifeSupp, 0 | 64);
                            MapUtilities.CachedShipStatus.RpcUpdateSystem(SystemTypes.LifeSupp, 1 | 64);
                        } else if (task.TaskType == TaskTypes.ResetReactor) {
                            MapUtilities.CachedShipStatus.RpcUpdateSystem(SystemTypes.Reactor, 16);
                        } else if (task.TaskType == TaskTypes.ResetSeismic) {
                            MapUtilities.CachedShipStatus.RpcUpdateSystem(SystemTypes.Laboratory, 16);
                        } else if (task.TaskType == TaskTypes.FixComms) {
                            MapUtilities.CachedShipStatus.RpcUpdateSystem(SystemTypes.Comms, 16 | 0);
                            MapUtilities.CachedShipStatus.RpcUpdateSystem(SystemTypes.Comms, 16 | 1);
                        } else if (task.TaskType == TaskTypes.StopCharles) {
                            MapUtilities.CachedShipStatus.RpcUpdateSystem(SystemTypes.HeliSabotage, 0 | 16);
                            MapUtilities.CachedShipStatus.RpcUpdateSystem(SystemTypes.HeliSabotage, 1 | 16);
                        } else if (SubmergedCompatibility.IsSubmerged && task.TaskType == SubmergedCompatibility.RetrieveOxygenMask) {
                            Engineer.FixSubmergedOxygen.Invoke();
                        }

                    }
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Engineer) && Engineer.local.remainingFixes > 0 && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => {
                    if (engineerRepairText != null) engineerRepairText.text = Engineer.local.remainingFixes.ToString();
                    bool sabotageActive = false;
                    foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks.GetFastEnumerator())
                        if (task.TaskType == TaskTypes.FixLights || task.TaskType == TaskTypes.RestoreOxy || task.TaskType == TaskTypes.ResetReactor || task.TaskType == TaskTypes.ResetSeismic || task.TaskType == TaskTypes.FixComms || task.TaskType == TaskTypes.StopCharles
                            || (SubmergedCompatibility.IsSubmerged && task.TaskType == SubmergedCompatibility.RetrieveOxygenMask))
                            sabotageActive = true;
                    return sabotageActive && Engineer.local.remainingFixes > 0 && PlayerControl.LocalPlayer.CanMove;
                },
                () => {},
                Engineer.getButtonSprite(),
                CustomButton.ButtonPositions.upperRowRight,
                __instance,
                KeyCode.F,
                buttonText: ModTranslation.getString("RepairText"),
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            );
            engineerRepairText = engineerRepairButton.ShowUsesIcon(3);

            // Sheriff Kill
            sheriffKillButton = new CustomButton(
                () => {
                    MurderAttemptResult murderAttemptResult = Helpers.checkMuderAttempt(Sheriff.local.player, Sheriff.local.currentTarget);
                    if (murderAttemptResult == MurderAttemptResult.SuppressKill) return;

                    if (murderAttemptResult is MurderAttemptResult.PerformKill or MurderAttemptResult.ReverseKill) {
                        byte targetId = 0;
                        if (((Sheriff.local.currentTarget.Data.Role.IsImpostor && (Sheriff.local.currentTarget != Mini.mini || Mini.isGrownUp())) ||
                            (Sheriff.spyCanDieToSheriff && Sheriff.local.currentTarget.isRole(RoleId.Spy)) ||
                            (Sheriff.canKillNeutrals && Helpers.isNeutral(Sheriff.local.currentTarget)) ||
                            Sheriff.local.currentTarget.isRole(RoleId.Jackal) || Sheriff.local.currentTarget.isRole(RoleId.Sidekick) ||
                            (CreatedMadmate.createdMadmate.Any(x => x.PlayerId == Sheriff.local.currentTarget.PlayerId) && CreatedMadmate.canDieToSheriff) ||
                            (Madmate.canDieToSheriff && Madmate.madmate.Any(x => x.PlayerId == Sheriff.local.currentTarget.PlayerId))) &&
                            !Madmate.madmate.Any(y => y.PlayerId == Sheriff.local.player.PlayerId)) 
                        {
                            _ = new StaticAchievementToken("sheriff.common1");
                            targetId = Sheriff.local.currentTarget.PlayerId;
                        }
                        else {
                            _= new StaticAchievementToken("sheriff.another1");
                            targetId = PlayerControl.LocalPlayer.PlayerId;
                        }

                        MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedMurderPlayer, Hazel.SendOption.Reliable, -1);
                        killWriter.Write(Sheriff.local.player.Data.PlayerId);
                        killWriter.Write(targetId);
                        killWriter.Write(byte.MaxValue);
                        AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                        RPCProcedure.uncheckedMurderPlayer(Sheriff.local.player.Data.PlayerId, targetId, Byte.MaxValue);
                    }

                    sheriffKillButton.Timer = sheriffKillButton.MaxTimer;
                    Sheriff.local.currentTarget = null;
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Sheriff) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Sheriff.local.currentTarget && PlayerControl.LocalPlayer.CanMove; },
                () => { sheriffKillButton.Timer = sheriffKillButton.MaxTimer;},
                __instance.KillButton.graphic.sprite,
                CustomButton.ButtonPositions.upperRowRight,
                __instance,
                KeyCode.Q
            );

            // Deputy Handcuff
            deputyHandcuffButton = new CustomButton(
                () => {
                    bool promotedDeputy = PlayerControl.LocalPlayer.isRole(RoleId.Sheriff);
                    byte targetId = 0;
                    targetId = promotedDeputy ? Sheriff.local.currentTarget.PlayerId : Deputy.local.currentTarget.PlayerId;  // If the deputy is now the sheriff, sheriffs target, else deputies target

                    if (Helpers.isEvil(Helpers.playerById(targetId)))
                        _ = new StaticAchievementToken("deputy.common1");

                    Deputy.Handcuff.Invoke(targetId);
                    if (promotedDeputy) {
                        Sheriff.local.remainingHandcuffs--;
                    }
                    else {
                        Deputy.local.currentTarget = null;
                        Deputy.local.remainingHandcuffs--;
                    }
                    deputyHandcuffButton.Timer = deputyHandcuffButton.MaxTimer;

                    SoundEffectsManager.play("deputyHandcuff");
                },
                () => { return (PlayerControl.LocalPlayer.isRole(RoleId.Deputy) || (PlayerControl.LocalPlayer.isRole(RoleId.Sheriff) && Sheriff.local.isFormerDeputy && Deputy.keepsHandcuffsOnPromotion)) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => {
                    if (deputyButtonHandcuffsText != null) deputyButtonHandcuffsText.text = $"{(PlayerControl.LocalPlayer.isRole(RoleId.Sheriff) ? Sheriff.local.remainingHandcuffs : Deputy.local.remainingHandcuffs)}";
                    return ((PlayerControl.LocalPlayer.isRole(RoleId.Deputy) && Deputy.local.currentTarget && Deputy.local.remainingHandcuffs > 0) || (PlayerControl.LocalPlayer.isRole(RoleId.Sheriff) && Sheriff.local.isFormerDeputy && Sheriff.local.currentTarget
                    && Sheriff.local.remainingHandcuffs > 0)) && PlayerControl.LocalPlayer.CanMove;
                },
                () => { deputyHandcuffButton.Timer = deputyHandcuffButton.MaxTimer; },
                Deputy.getButtonSprite(),
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                KeyCode.F,
                buttonText: ModTranslation.getString("HandcuffText"),
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            );
            // Deputy Handcuff button handcuff counter
            deputyButtonHandcuffsText = deputyHandcuffButton.ShowUsesIcon(3);

            jackalAndSidekickSabotageLightsButton = new CustomButton(
                () => {
                    ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Sabotage, (byte)SystemTypes.Electrical);
                },
                () => {
                    return ((PlayerControl.LocalPlayer.isRole(RoleId.Jackal) && Jackal.canSabotageLights) ||
                            (PlayerControl.LocalPlayer.isRole(RoleId.Sidekick) && Sidekick.canSabotageLights)) && !PlayerControl.LocalPlayer.Data.IsDead
                             && (!Helpers.isFungle() || CustomOptionHolder.fungleElectrical.getBool());
                },
                () => {
                    if (Helpers.sabotageTimer() > jackalAndSidekickSabotageLightsButton.Timer || Helpers.sabotageActive())
                        jackalAndSidekickSabotageLightsButton.Timer = Helpers.sabotageTimer() + 5f;  // this will give imps time to do another sabotage.
                    return Helpers.canUseSabotage();
                },
                () => {
                    jackalAndSidekickSabotageLightsButton.Timer = Helpers.sabotageTimer() + 5f;
                },
                Trickster.getLightsOutButtonSprite(),
                CustomButton.ButtonPositions.upperRowCenter,
                __instance,
                KeyCode.G,
                buttonText: ModTranslation.getString("LightsOutText")
            );

            // Time Master Rewind Time
            timeMasterShieldButton = new CustomButton(
                () => {
                    TimeMaster.UseShield.Invoke(PlayerControl.LocalPlayer.PlayerId);
                    SoundEffectsManager.play("timemasterShield");
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.TimeMaster) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () => {
                    timeMasterShieldButton.Timer = timeMasterShieldButton.MaxTimer;
                    timeMasterShieldButton.isEffectActive = false;
                    timeMasterShieldButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                TimeMaster.getButtonSprite(),
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                KeyCode.F, 
                true,
                TimeMaster.shieldDuration,
                () => {
                    timeMasterShieldButton.Timer = timeMasterShieldButton.MaxTimer;
                    SoundEffectsManager.stop("timemasterShield");

                },
                buttonText: ModTranslation.getString("TimeShieldText"),
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            );

            // Medic Shield
            medicShieldButton = new CustomButton(
                () => {
                    medicShieldButton.Timer = 0f;

                    if (!Helpers.isEvil(Medic.local.currentTarget))
                        _ = new StaticAchievementToken("medic.common1");

                    if (Medic.setShieldAfterMeeting)
                        Medic.FutureShield.Invoke((Medic.local.currentTarget.PlayerId, PlayerControl.LocalPlayer.PlayerId));
                    else
                        Medic.Shield.Invoke((Medic.local.currentTarget.PlayerId, PlayerControl.LocalPlayer.PlayerId));

                    Medic.local.meetingAfterShielding = false;
                    SoundEffectsManager.play("medicShield");
                    },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Medic) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return !Medic.local.usedShield && Medic.local.currentTarget && PlayerControl.LocalPlayer.CanMove; },
                () => {},
                Medic.getButtonSprite(),
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                KeyCode.F,
                buttonText: ModTranslation.getString("ShieldText"),
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            );

            medicVitalsButton = new CustomButton(
                () => {
                    medicVitalsButton.Timer = 0f;
                    var vitalsMinigame = Medic.OpenSpecialVitalsMinigame();
                    _ = new StaticAchievementToken("medic.common2");

                    if (Medic.seesDeathReasonOnVitals)
                    {
                        IEnumerator CoUpdateState(VitalsPanel panel, PlayerControl player)
                        {
                            TMPro.TextMeshPro text = UnityEngine.Object.Instantiate(vitalsMinigame.SabText, panel.transform);
                            UnityEngine.Object.DestroyImmediate(text.GetComponent<AlphaBlink>());
                            text.gameObject.SetActive(false);
                            text.transform.localScale = Vector3.one * 0.5f;
                            text.transform.localPosition = new Vector3(-0.75f, -0.23f, 0f);
                            text.color = new Color(0.8f, 0.8f, 0.8f);
                            while (true)
                            {
                                string deathReason = Medic.GetDeathReason(player);
                                if ((panel.IsDiscon || panel.IsDead) && deathReason != null)
                                {
                                    text.gameObject.SetActive(true);
                                    text.text = ModTranslation.getString("medic" + deathReason);
                                }
                                else
                                {
                                    text.gameObject.SetActive(false);
                                }
                                yield return null;
                            }
                        }

                        vitalsMinigame.vitals.DoIf(panel => panel.isActiveAndEnabled, panel => panel.StartCoroutine(CoUpdateState(panel, Helpers.playerById(panel.PlayerInfo.PlayerId))));
                    }

                    IEnumerator CoUpdate()
                    {
                        int lastAliveCount = PlayerControl.AllPlayerControls.GetFastEnumerator().Count(x => x != null && !x.Data.IsDead && !Busker.players.Any(y => y.pseudocideFlag && y.player?.PlayerId == x.PlayerId));
                        while (vitalsMinigame.amClosing != Minigame.CloseState.Closing) yield return null;
                        if (lastAliveCount != PlayerControl.AllPlayerControls.GetFastEnumerator().Count(x => x != null && !x.Data.IsDead && !Busker.players.Any(y => y.pseudocideFlag && y.player?.PlayerId == x.PlayerId)))
                            _ = new StaticAchievementToken("medic.another1");
                    }
                    vitalsMinigame.StartCoroutine(CoUpdate().WrapToIl2Cpp());
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Medic) && Medic.canUseVitals && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () => { },
                Hacker.getVitalsSprite(),
                CustomButton.ButtonPositions.upperRowRight,
                __instance,
                KeyCode.H,
                buttonText: TranslationController.Instance.GetString(StringNames.VitalsLabel),
                abilityTexture: CustomButton.ButtonLabelType.AdminButton
            );

            zephyrButton = new CustomButton(
                () =>
                {
                    Zephyr.local.RpcCheckCannon(PlayerControl.LocalPlayer.GetTruePosition());
                    zephyrButton.Timer = zephyrButton.MaxTimer;

                    if (Zephyr.triggerBothCooldown)
                        PlayerControl.LocalPlayer.killTimer = PlayerControl.LocalPlayer.GetKillCooldown();

                    _ = new StaticAchievementToken("zephyr.common1");
                    _ = new StaticAchievementToken("zephyr.another1");
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Zephyr) && !PlayerControl.LocalPlayer.Data.IsDead && Zephyr.local.numUses > 0; },
                () =>
                {
                    if (zephyrUsesText != null) zephyrUsesText.text = Zephyr.local.numUses.ToString();
                    return PlayerControl.LocalPlayer.CanMove;
                },
                () =>
                {
                    zephyrButton.Timer = zephyrButton.MaxTimer = Zephyr.cooldown;
                },
                Zephyr.getButtonSprite(),
                CustomButton.ButtonPositions.upperRowLeft,
                __instance,
                KeyCode.F,
                buttonText: ModTranslation.getString("zephyrCannonText"),
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            );
            zephyrUsesText = zephyrButton.ShowUsesIcon(0);

            baitButton = new CustomButton(
                () =>
                {
                    Bait.Emit.Invoke(PlayerControl.LocalPlayer.PlayerId); ;
                    baitButton.Timer = baitButton.MaxTimer;
                    Bait.local.numUses--;
                    _ = new StaticAchievementToken("bait.common2");
                    Bait.local.acTokenChallenge2.Value++;
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Bait) && !PlayerControl.LocalPlayer.Data.IsDead && Bait.local.numUses > 0; },
                () =>
                {
                    if (baitUsesText != null) baitUsesText.text = Bait.local.numUses.ToString();
                    return PlayerControl.LocalPlayer.CanMove;
                },
                () =>
                {
                    baitButton.Timer = baitButton.MaxTimer = Bait.cooldown;
                },
                Bait.getButtonSprite(),
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                KeyCode.F,
                buttonText: ModTranslation.getString("BaitEmitText"),
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            );
            baitUsesText = baitButton.ShowUsesIcon(3);

            // Shifter shift
            shifterShiftButton = new CustomButton(
                () => {
                    if (Helpers.checkSuspendAction(PlayerControl.LocalPlayer, Shifter.currentTarget)) return;
                    Shifter.SetFutureShifted.Invoke(Shifter.currentTarget.PlayerId);
                    SoundEffectsManager.play("shifterShift");
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Shifter) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Shifter.currentTarget && Shifter.futureShift == null && PlayerControl.LocalPlayer.CanMove; },
                () => { },
                Shifter.getButtonSprite(),
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                KeyCode.F,
                buttonText: ModTranslation.getString("ShiftText"),
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            );

            lighterButton = new CustomButton(
                () =>
                {
                    Lighter.local.lightActive = true;
                    Lighter.local.light();
                    Lighter.local.acTokenChallenge.Value++;
                    SoundEffectsManager.play("lighterLight");
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Lighter) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () =>
                {
                    lighterButton.Timer = lighterButton.MaxTimer = Lighter.cooldown;
                    lighterButton.isEffectActive = false;
                    lighterButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Helpers.loadSpriteFromResources("TheOtherRoles.Resources.LighterButton.png", 115f),
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                KeyCode.F,
                true,
                Lighter.duration,
                () =>
                {
                    Lighter.local.lightActive = false;
                    lighterButton.Timer = lighterButton.MaxTimer;
                    SoundEffectsManager.play("lighterLight");
                },
                buttonText: ModTranslation.getString("LighterText"),
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            );

            // Morphling morph
            morphlingButton = new CustomButton(
                () => {
                    if (Morphling.local.sampledTarget != null) {
                         _ = new StaticAchievementToken("morphling.common1");
                        Morphling.Morph.Invoke((Morphling.local.sampledTarget.PlayerId, PlayerControl.LocalPlayer.PlayerId));
                        Morphling.local.sampledTarget = null;
                        morphlingButton.EffectDuration = Morphling.duration;
                        morphlingButton.shakeOnEnd = true;
                        SoundEffectsManager.play("morphlingMorph");
                    } else if (Morphling.local.currentTarget != null) {
                        if (Helpers.checkSuspendAction(PlayerControl.LocalPlayer, Morphling.local.currentTarget)) return;
                        Morphling.local.acTokenChallenge.Value.playerId = Morphling.local.currentTarget.PlayerId;
                        Morphling.local.acTokenChallenge.Value.kill = false;
                        Morphling.local.sampledTarget = Morphling.local.currentTarget;
                        morphlingButton.Sprite = Morphling.getMorphSprite();
                        morphlingButton.buttonText = ModTranslation.getString("MorphText");
                        morphlingButton.shakeOnEnd = false;
                        morphlingButton.EffectDuration = 1f;
                        SoundEffectsManager.play("morphlingSample");

                        // Add poolable player to the button so that the target outfit is shown
                        setButtonTargetDisplay(Morphling.local.sampledTarget, morphlingButton);
                    }
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Morphling) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return (Morphling.local.currentTarget || Morphling.local.sampledTarget) && PlayerControl.LocalPlayer.CanMove && !Helpers.MushroomSabotageActive(); },
                () => { 
                    morphlingButton.Timer = morphlingButton.MaxTimer;
                    morphlingButton.Sprite = Morphling.getSampleSprite();
                    morphlingButton.buttonText = ModTranslation.getString("SampleText");
                    morphlingButton.isEffectActive = false;
                    morphlingButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                    setButtonTargetDisplay(null);
                },
                Morphling.getSampleSprite(),
                CustomButton.ButtonPositions.upperRowLeft,
                __instance,
                KeyCode.F,
                true,
                Morphling.duration,
                () => {
                    if (Morphling.local.sampledTarget == null) {
                        morphlingButton.Timer = morphlingButton.MaxTimer;
                        morphlingButton.Sprite = Morphling.getSampleSprite();
                        morphlingButton.buttonText = ModTranslation.getString("SampleText");
                        SoundEffectsManager.play("morphlingMorph");

                        // Reset the poolable player
                        setButtonTargetDisplay(null);
                    }
                },
                buttonText: ModTranslation.getString("SampleText"),
                actionName: ModTranslation.getString("morphlingButton")
            );

            // Camouflager camouflage
            camouflagerButton = new CustomButton(
                () => {
                    _ = new StaticAchievementToken("camouflager.common1");
                    Camouflager.ActivateCamo.Invoke();
                    SoundEffectsManager.play("morphlingMorph");
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Camouflager) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () => {
                    camouflagerButton.Timer = camouflagerButton.MaxTimer;
                    camouflagerButton.isEffectActive = false;
                    camouflagerButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Camouflager.getButtonSprite(),
                CustomButton.ButtonPositions.upperRowLeft,
                __instance,
                KeyCode.F,
                true,
                Camouflager.duration,
                () => {
                    camouflagerButton.Timer = camouflagerButton.MaxTimer;
                    SoundEffectsManager.play("morphlingMorph");
                },
                buttonText: ModTranslation.getString("CamoText")
            );

            // Hacker button
            hackerButton = new CustomButton(
                () => {
                    Hacker.local.hackerTimer = Hacker.duration;
                    SoundEffectsManager.play("hackerHack");
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Hacker) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return true; },
                () => {
                    hackerButton.Timer = hackerButton.MaxTimer;
                    hackerButton.isEffectActive = false;
                    hackerButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Hacker.getButtonSprite(),
                CustomButton.ButtonPositions.upperRowRight,
                __instance,
                KeyCode.F,
                true,
                0f,
                () => { hackerButton.Timer = hackerButton.MaxTimer;},
                buttonText: ModTranslation.getString("HackerText"),
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            );

            hackerAdminTableButton = new CustomButton(
               () => {
                   _ = new StaticAchievementToken("hacker.common1");
                   if (!MapBehaviour.Instance || !MapBehaviour.Instance.isActiveAndEnabled) {
                       HudManager __instance = FastDestroyableSingleton<HudManager>.Instance;
                       __instance.InitMap();
                       MapBehaviour.Instance.ShowCountOverlay(allowedToMove: true, showLivePlayerPosition: true, includeDeadBodies: true);
                   }
                   if (Hacker.cantMove) PlayerControl.LocalPlayer.moveable = false;
                   PlayerControl.LocalPlayer.NetTransform.Halt(); // Stop current movement 
                   Hacker.local.chargesAdminTable--;
               },
               () => { return PlayerControl.LocalPlayer.isRole(RoleId.Hacker) && !PlayerControl.LocalPlayer.Data.IsDead;},
               () => {
                   if (hackerAdminTableChargesText != null) hackerAdminTableChargesText.text = $"{Hacker.local.chargesAdminTable} / {Hacker.toolsNumber}";
                   return Hacker.local.chargesAdminTable > 0; 
               },
               () => {
                   hackerAdminTableButton.Timer = hackerAdminTableButton.MaxTimer;
                   hackerAdminTableButton.isEffectActive = false;
                   hackerAdminTableButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
               },
               Hacker.getAdminSprite(),
               CustomButton.ButtonPositions.lowerRowRight,
               __instance,
               KeyCode.Q,
               true,
               0f,
               () => { 
                   hackerAdminTableButton.Timer = hackerAdminTableButton.MaxTimer;
                   if (!hackerVitalsButton.isEffectActive) PlayerControl.LocalPlayer.moveable = true;
                   if (MapBehaviour.Instance && MapBehaviour.Instance.isActiveAndEnabled) MapBehaviour.Instance.Close();
               },
               GameOptionsManager.Instance.currentNormalGameOptions.MapId == 3,
               FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Admin),
               abilityTexture: CustomButton.ButtonLabelType.AdminButton
           );

            // Hacker Admin Table Charges
            hackerAdminTableChargesText = GameObject.Instantiate(hackerAdminTableButton.actionButton.cooldownTimerText, hackerAdminTableButton.actionButton.cooldownTimerText.transform.parent);
            hackerAdminTableChargesText.text = "";
            hackerAdminTableChargesText.enableWordWrapping = false;
            hackerAdminTableChargesText.transform.localScale = Vector3.one * 0.5f;
            hackerAdminTableChargesText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            hackerVitalsButton = new CustomButton(
               () => {
                   if (GameOptionsManager.Instance.currentNormalGameOptions.MapId != 1) {
                       _ = new StaticAchievementToken("hacker.common2");
                       if (Hacker.local.vitals == null) {
                           var e = UnityEngine.Object.FindObjectsOfType<SystemConsole>().FirstOrDefault(x => x.gameObject.name.Contains("panel_vitals") || x.gameObject.name.Contains("Vitals"));
                           if (e == null || Camera.main == null) return;
                           Hacker.local.vitals = UnityEngine.Object.Instantiate(e.MinigamePrefab, Camera.main.transform, false);
                       }
                       Hacker.local.vitals.transform.SetParent(Camera.main.transform, false);
                       Hacker.local.vitals.transform.localPosition = new Vector3(0.0f, 0.0f, -50f);
                       Hacker.local.vitals.Begin(null);
                   } else {
                       if (Hacker.local.doorLog == null) {
                           var e = UnityEngine.Object.FindObjectsOfType<SystemConsole>().FirstOrDefault(x => x.gameObject.name.Contains("SurvLogConsole"));
                           if (e == null || Camera.main == null) return;
                           Hacker.local.doorLog = UnityEngine.Object.Instantiate(e.MinigamePrefab, Camera.main.transform, false);
                       }
                       Hacker.local.doorLog.transform.SetParent(Camera.main.transform, false);
                       Hacker.local.doorLog.transform.localPosition = new Vector3(0.0f, 0.0f, -50f);
                       Hacker.local.doorLog.Begin(null);
                   }

                   if (Hacker.cantMove) PlayerControl.LocalPlayer.moveable = false;
                   PlayerControl.LocalPlayer.NetTransform.Halt(); // Stop current movement 

                   Hacker.local.chargesVitals--;
               },
               () => { return PlayerControl.LocalPlayer.isRole(RoleId.Hacker) && !PlayerControl.LocalPlayer.Data.IsDead && GameOptionsManager.Instance.currentGameOptions.MapId != 0 && GameOptionsManager.Instance.currentNormalGameOptions.MapId != 3; },
               () => {
                   if (hackerVitalsChargesText != null) hackerVitalsChargesText.text = $"{Hacker.local.chargesVitals} / {Hacker.toolsNumber}";
                   hackerVitalsButton.actionButton.graphic.sprite = Helpers.isMira() ? Hacker.getLogSprite() : Hacker.getVitalsSprite();
                   hackerVitalsButton.actionButton.OverrideText(Helpers.isMira() ?
                       TranslationController.Instance.GetString(StringNames.DoorlogLabel) :
                       TranslationController.Instance.GetString(StringNames.VitalsLabel));
                   return Hacker.local.chargesVitals > 0;
               },
               () => {
                   hackerVitalsButton.Timer = hackerVitalsButton.MaxTimer;
                   hackerVitalsButton.isEffectActive = false;
                   hackerVitalsButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
               },
               Hacker.getVitalsSprite(),
               CustomButton.ButtonPositions.lowerRowCenter,
               __instance,
               KeyCode.Q,
               true,
               0f,
               () => { 
                   hackerVitalsButton.Timer = hackerVitalsButton.MaxTimer;
                   if(!hackerAdminTableButton.isEffectActive) PlayerControl.LocalPlayer.moveable = true;
                   if (Minigame.Instance ) {
                       if (Helpers.isMira()) Hacker.local.doorLog.ForceClose();
                       else Hacker.local.vitals.ForceClose();
                   }
               },
               false,
              Helpers.isMira() ?
              TranslationController.Instance.GetString(StringNames.DoorlogLabel) :
              TranslationController.Instance.GetString(StringNames.VitalsLabel),
              abilityTexture: CustomButton.ButtonLabelType.AdminButton
           );

            // Hacker Vitals Charges
            hackerVitalsChargesText = GameObject.Instantiate(hackerVitalsButton.actionButton.cooldownTimerText, hackerVitalsButton.actionButton.cooldownTimerText.transform.parent);
            hackerVitalsChargesText.text = "";
            hackerVitalsChargesText.enableWordWrapping = false;
            hackerVitalsChargesText.transform.localScale = Vector3.one * 0.5f;
            hackerVitalsChargesText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            // Tracker button
            trackerTrackPlayerButton = new CustomButton(
                () => {
                    if (Tracker.local.currentTarget.Data.Role.IsImpostor)
                        _ = new StaticAchievementToken("tracker.common1");

                    Tracker.UseTracker.Invoke((Tracker.local.currentTarget.PlayerId, PlayerControl.LocalPlayer.PlayerId));
                    SoundEffectsManager.play("trackerTrackPlayer");
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Tracker) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove && Tracker.local.currentTarget != null && !Tracker.local.usedTracker; },
                () => { if(Tracker.resetTargetAfterMeeting) Tracker.local.resetTracked(); },
                Tracker.getButtonSprite(),
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                KeyCode.F,
                buttonText: ModTranslation.getString("TrackerText"),
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            );

            trackerTrackCorpsesButton = new CustomButton(
                () => { Tracker.local.corpsesTrackingTimer = Tracker.corpsesTrackingDuration;
                            SoundEffectsManager.play("trackerTrackCorpses"); },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Tracker) && !PlayerControl.LocalPlayer.Data.IsDead && Tracker.canTrackCorpses; },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () => {
                    trackerTrackCorpsesButton.Timer = trackerTrackCorpsesButton.MaxTimer;
                    trackerTrackCorpsesButton.isEffectActive = false;
                    trackerTrackCorpsesButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Tracker.getTrackCorpsesButtonSprite(),
                CustomButton.ButtonPositions.lowerRowCenter,
                __instance,
                KeyCode.G,
                true,
                Tracker.corpsesTrackingDuration,
                () => {
                    trackerTrackCorpsesButton.Timer = trackerTrackCorpsesButton.MaxTimer;
                },
                buttonText: ModTranslation.getString("PathfindText"),
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            );

            trackerKillButton = new(
                () =>
                {
                    if (Helpers.checkMurderAttemptAndKill(PlayerControl.LocalPlayer, Tracker.local.currentTarget) == MurderAttemptResult.SuppressKill) return;
                    trackerKillButton.Timer = trackerKillButton.MaxTimer;
                    Tracker.local.currentTarget = null;
                    Tracker.local.numShots--;
                    _ = new StaticAchievementToken("tracker.challenge2");
                },
                () =>
                {
                    return PlayerControl.LocalPlayer.isRole(RoleId.Tracker) && Tracker.canKill && !PlayerControl.LocalPlayer.Data.IsDead;
                },
                () =>
                {
                    if (trackerText != null) {
                        trackerText.text = $"{Tracker.local.numShots}";
                    }
                    return PlayerControl.LocalPlayer.CanMove && Tracker.local.numShots > 0 && Tracker.local.currentTarget;
                },
                () => { trackerKillButton.Timer = trackerKillButton.MaxTimer; },
                Tracker.getKillButtonSprite(),
                CustomButton.ButtonPositions.upperRowRight,
                __instance,
                KeyCode.Q,
                buttonText: FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.KillLabel),
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            );
            trackerText = trackerKillButton.ShowUsesIcon(3);

            watcherKillButton = new(
                () =>
                {
                    if (Helpers.checkMurderAttemptAndKill(PlayerControl.LocalPlayer, NiceWatcher.local.currentTarget) == MurderAttemptResult.SuppressKill) return;
                    NiceWatcher.local.canKill = false;
                    NiceWatcher.local.currentTarget = null;
                },
                () =>
                {
                    return PlayerControl.LocalPlayer.isRole(RoleId.NiceWatcher) && NiceWatcher.local.canKill && !PlayerControl.LocalPlayer.Data.IsDead;
                },
                () => { return PlayerControl.LocalPlayer.CanMove && NiceWatcher.local.currentTarget; },
                () => { },
                __instance.KillButton.graphic.sprite,
                CustomButton.ButtonPositions.upperRowRight,
                __instance,
                KeyCode.Q
            );

            detectiveButton = new CustomButton(
                () =>
                {
                    foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(PlayerControl.LocalPlayer.GetTruePosition(), PlayerControl.LocalPlayer.MaxReportDistance, Constants.PlayersOnlyMask)) {
                        if (collider2D.tag == "DeadBody") {
                            DeadBody component = collider2D.GetComponent<DeadBody>();
                            if (component && !component.Reported) {
                                Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
                                Vector2 truePosition2 = component.TruePosition;
                                if (Vector2.Distance(truePosition2, truePosition) <= PlayerControl.LocalPlayer.MaxReportDistance && PlayerControl.LocalPlayer.CanMove && !PhysicsHelpers.AnythingBetween(truePosition, truePosition2, Constants.ShipAndObjectsMask, false)) {
                                    NetworkedPlayerInfo playerInfo = GameData.Instance.GetPlayerById(component.ParentId);
                                    var dp = GameHistory.deadPlayers?.Where(x => x.player?.PlayerId == playerInfo.PlayerId)?.FirstOrDefault();
                                    if (dp != null && dp.killerIfExisting != null && !dp.killerIfExisting.Data.IsDead)
                                    {
                                        _ = new StaticAchievementToken("detective.another1");
                                        TORGUIManager.Instance.StartCoroutine(Helpers.CoAnimIcon(dp.killerIfExisting).WrapToIl2Cpp());
                                    }
                                    Detective.inspectCooldown = detectiveButton.Timer = detectiveButton.MaxTimer;
                                    break;
                                }
                            }
                        }
                    }
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Detective) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove && __instance.ReportButton.graphic.color == Palette.EnabledColor; },
                () =>
                {
                    detectiveButton.Timer = detectiveButton.MaxTimer;
                    detectiveButton.isEffectActive = false;
                    detectiveButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Detective.getButtonSprite(),
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                KeyCode.F,
                true,
                Detective.inspectDuration,
                () => { detectiveButton.Timer = detectiveButton.MaxTimer; },
                buttonText: ModTranslation.getString("InspectText"),
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            );
    
            vampireKillButton = new CustomButton(
                () => {
                    if (Helpers.checkSuspendAction(PlayerControl.LocalPlayer, Vampire.local.currentTarget)) return;
                    MurderAttemptResult murder = Helpers.checkMuderAttempt(PlayerControl.LocalPlayer, Vampire.local.currentTarget);
                    if (murder == MurderAttemptResult.PerformKill) {
                        if (Vampire.local.targetNearGarlic) {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedMurderPlayer, Hazel.SendOption.Reliable, -1);
                            writer.Write(PlayerControl.LocalPlayer.PlayerId);
                            writer.Write(Vampire.local.currentTarget.PlayerId);
                            writer.Write(Byte.MaxValue);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.uncheckedMurderPlayer(PlayerControl.LocalPlayer.PlayerId, Vampire.local.currentTarget.PlayerId, Byte.MaxValue);

                            vampireKillButton.HasEffect = false; // Block effect on this click
                            vampireKillButton.Timer = vampireKillButton.MaxTimer;
                        } else {
                            _ = new StaticAchievementToken("vampire.common1");
                            Vampire.local.bitten = Vampire.local.currentTarget;
                            // Notify players about bitten
                            Vampire.SetBitten.Invoke((Vampire.local.bitten.PlayerId, 0, PlayerControl.LocalPlayer.PlayerId));

                            byte lastTimer = (byte)Vampire.delay;
                            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Vampire.delay, new Action<float>((p) => { // Delayed action
                                if (p == 1f) {
                                    // Perform kill if possible and reset bitten (regardless whether the kill was successful or not)
                                    var res = Helpers.checkMurderAttemptAndKill(PlayerControl.LocalPlayer, Vampire.local.bitten, showAnimation: false);
                                    if (res == MurderAttemptResult.PerformKill)
                                    {
                                        Vampire.local.acTokenChallenge.Value.deathTime = DateTime.UtcNow;
                                        Vampire.SetBitten.Invoke((byte.MaxValue, byte.MaxValue, PlayerControl.LocalPlayer.PlayerId));
                                    }
                                }
                            })));
                            SoundEffectsManager.play("vampireBite");

                            vampireKillButton.HasEffect = true; // Trigger effect on this click
                        }
                    } else if (murder == MurderAttemptResult.BlankKill) {
                        vampireKillButton.Timer = vampireKillButton.MaxTimer;
                        vampireKillButton.HasEffect = false;
                    } else {
                        vampireKillButton.HasEffect = false;
                    }
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Vampire) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => {
                    if (Vampire.local.targetNearGarlic && Vampire.canKillNearGarlics) {
                        vampireKillButton.actionButton.graphic.sprite = __instance.KillButton.graphic.sprite;
                        vampireKillButton.buttonText = TranslationController.Instance.GetString(StringNames.KillLabel);
                    }
                    else {
                        vampireKillButton.actionButton.graphic.sprite = Vampire.getButtonSprite();
                        vampireKillButton.buttonText = ModTranslation.getString("VampireText");
                    }
                    return Vampire.local.currentTarget != null && PlayerControl.LocalPlayer.CanMove && (!Vampire.local.targetNearGarlic || Vampire.canKillNearGarlics);
                },
                () => {
                    vampireKillButton.Timer = vampireKillButton.MaxTimer;
                    vampireKillButton.isEffectActive = false;
                    vampireKillButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Vampire.getButtonSprite(),
                CustomButton.ButtonPositions.upperRowLeft,
                __instance,
                KeyCode.Q,
                false,
                0f,
                () => {
                    vampireKillButton.Timer = vampireKillButton.MaxTimer = Mathf.Max(1f, vampireKillButton.MaxTimer - Vampire.cooldownDecrease);
                },
                buttonText: ModTranslation.getString("VampireText")
            );

            garlicButton = new CustomButton(
                () => {
                    Vampire.localPlacedGarlic = true;
                    var pos = PlayerControl.LocalPlayer.transform.position;
                    RPCProcedure.PlaceGarlic.Invoke(pos);
                    SoundEffectsManager.play("garlic");
                },
                () => { return !Vampire.localPlacedGarlic && !PlayerControl.LocalPlayer.Data.IsDead && Vampire.garlicsActive && !PropHuntGM.isPropHuntGM; },
                () => { return PlayerControl.LocalPlayer.CanMove && !Vampire.localPlacedGarlic; },
                () => { },
                Vampire.getGarlicButtonSprite(),
#if WINDOWS
                new Vector3(0, -0.06f, 0),
#else
                new Vector3(1f, -0.06f, 0),
#endif
                __instance,
                null,
                true,
                ModTranslation.getString("GarlicText"),
                CustomButton.ButtonLabelType.UseButton
            );

            portalmakerPlacePortalButton = new CustomButton(
                () => {
                    portalmakerPlacePortalButton.Timer = portalmakerPlacePortalButton.MaxTimer;

                    var pos = PlayerControl.LocalPlayer.transform.position;
                    Portalmaker.PlacePortal.Invoke(pos);
                    SoundEffectsManager.play("tricksterPlaceBox");
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Portalmaker) && !PlayerControl.LocalPlayer.Data.IsDead && Portal.secondPortal == null; },
                () =>
                {
                    if (portalmakerButtonNumText != null) portalmakerButtonNumText.text = $"{(Portal.firstPortal == null ? 2 : 1)}";
                    return PlayerControl.LocalPlayer.CanMove && Portal.secondPortal == null;
                },
                () => { portalmakerPlacePortalButton.Timer = portalmakerPlacePortalButton.MaxTimer; },
                Portalmaker.getPlacePortalButtonSprite(),
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                KeyCode.F,
                buttonText: ModTranslation.getString("PlacePortalText"),
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            );
            portalmakerButtonNumText = portalmakerPlacePortalButton.ShowUsesIcon(3);

            usePortalButton = new CustomButton(
                () => {
                    bool didTeleport = false;
                    Vector3 exit = Portal.findExit(PlayerControl.LocalPlayer.transform.position).portalGameObject.transform.position;
                    Vector3 entry = Portal.findEntry(PlayerControl.LocalPlayer.transform.position);

                    bool portalMakerSoloTeleport = !Portal.locationNearEntry(PlayerControl.LocalPlayer.transform.position);
                    if (portalMakerSoloTeleport) {
                        _ = new StaticAchievementToken("portalmaker.another1");
                        exit = Portal.firstPortal.portalGameObject.transform.position;
                        entry = PlayerControl.LocalPlayer.transform.position;
                    }

                    PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(entry);

                    if (!PlayerControl.LocalPlayer.Data.IsDead)
                    {  // Ghosts can portal too, but non-blocking and only with a local animation
                        Portalmaker.UsePortal.Invoke((PlayerControl.LocalPlayer.PlayerId, portalMakerSoloTeleport ? (byte)1 : (byte)0));
                    }
                    else {
                        Portalmaker.UsePortal.LocalInvoke((PlayerControl.LocalPlayer.PlayerId, portalMakerSoloTeleport ? (byte)1 : (byte)0));
                    }
                    usePortalButton.Timer = usePortalButton.MaxTimer;
                    portalmakerMoveToPortalButton.Timer = usePortalButton.MaxTimer;
                    SoundEffectsManager.play("portalUse");
                    FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Portal.teleportDuration, new Action<float>((p) => { // Delayed action
                        PlayerControl.LocalPlayer.moveable = false;
                        PlayerControl.LocalPlayer.NetTransform.Halt();
                        if (p >= 0.5f && p <= 0.53f && !didTeleport && !MeetingHud.Instance) {
                            if (SubmergedCompatibility.IsSubmerged) {
                                SubmergedCompatibility.ChangeFloor(exit.y > -7);
                            }
                            PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(exit);
                            didTeleport = true;
                        }
                        if (p == 1f) {
                            PlayerControl.LocalPlayer.moveable = true;
                        }
                    })));
                    },
                () => {
                    if (PlayerControl.LocalPlayer.isRole(RoleId.Portalmaker) && Portal.bothPlacedAndEnabled)
                        portalmakerButtonText1.text = Portal.locationNearEntry(PlayerControl.LocalPlayer.transform.position) || !Portalmaker.canPortalFromAnywhere ? "" : "1. " + Portal.firstPortal.room;
                    return Portal.bothPlacedAndEnabled; },
                () => { return PlayerControl.LocalPlayer.CanMove && (Portal.locationNearEntry(PlayerControl.LocalPlayer.transform.position) || (Portalmaker.canPortalFromAnywhere && PlayerControl.LocalPlayer.isRole(RoleId.Portalmaker))) && !Portal.isTeleporting; },
                () => { usePortalButton.Timer = usePortalButton.MaxTimer; },
                Portalmaker.getUsePortalButtonSprite(),
                new Vector3(0.9f, -0.06f, 0),
                __instance,
                KeyCode.J,
                mirror: true,
                ModTranslation.getString("PortalTeleportText"),
                CustomButton.ButtonLabelType.UseButton
            );

            portalmakerMoveToPortalButton = new CustomButton(
                () => {
                    _ = new StaticAchievementToken("portalmaker.another1");

                    bool didTeleport = false;
                    Vector3 exit = Portal.secondPortal.portalGameObject.transform.position;

                    if (!PlayerControl.LocalPlayer.Data.IsDead)
                    {  // Ghosts can portal too, but non-blocking and only with a local animation
                        Portalmaker.UsePortal.Invoke((PlayerControl.LocalPlayer.PlayerId, 2));
                    }
                    else {
                        Portalmaker.UsePortal.LocalInvoke((PlayerControl.LocalPlayer.PlayerId, 2));
                    }
                    usePortalButton.Timer = usePortalButton.MaxTimer;
                    portalmakerMoveToPortalButton.Timer = usePortalButton.MaxTimer;
                    SoundEffectsManager.play("portalUse");
                    FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Portal.teleportDuration, new Action<float>((p) => { // Delayed action
                        PlayerControl.LocalPlayer.moveable = false;
                        PlayerControl.LocalPlayer.NetTransform.Halt();
                        if (p >= 0.5f && p <= 0.53f && !didTeleport && !MeetingHud.Instance) {
                            if (SubmergedCompatibility.IsSubmerged) {
                                SubmergedCompatibility.ChangeFloor(exit.y > -7);
                            }
                            PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(exit);
                            didTeleport = true;
                        }
                        if (p == 1f) {
                            PlayerControl.LocalPlayer.moveable = true;
                        }
                    })));
                },
                () => { return Portalmaker.canPortalFromAnywhere && Portal.bothPlacedAndEnabled && PlayerControl.LocalPlayer.isRole(RoleId.Portalmaker); },
                () => { return PlayerControl.LocalPlayer.CanMove && !Portal.locationNearEntry(PlayerControl.LocalPlayer.transform.position) && !Portal.isTeleporting; },
                () => { portalmakerMoveToPortalButton.Timer = usePortalButton.MaxTimer; },
                Portalmaker.getUsePortalButtonSprite(),
                new Vector3(0.9f, 1f, 0),
                __instance,
                KeyCode.G,
                mirror: true,
                ModTranslation.getString("PortalTeleportText"),
                CustomButton.ButtonLabelType.UseButton
            );


            portalmakerButtonText1 = GameObject.Instantiate(usePortalButton.actionButton.cooldownTimerText, usePortalButton.actionButton.cooldownTimerText.transform.parent);
            portalmakerButtonText1.text = "";
            portalmakerButtonText1.enableWordWrapping = false;
            portalmakerButtonText1.transform.localScale = Vector3.one * 0.5f;
            portalmakerButtonText1.transform.localPosition += new Vector3(-0.05f, 0.55f, -1f);

            portalmakerButtonText2 = GameObject.Instantiate(portalmakerMoveToPortalButton.actionButton.cooldownTimerText, portalmakerMoveToPortalButton.actionButton.cooldownTimerText.transform.parent);
            portalmakerButtonText2.text = "";
            portalmakerButtonText2.enableWordWrapping = false;
            portalmakerButtonText2.transform.localScale = Vector3.one * 0.5f;
            portalmakerButtonText2.transform.localPosition += new Vector3(-0.05f, 0.55f, -1f);



            // Jackal Sidekick Button
            jackalSidekickButton = new CustomButton(
                () => {
                    if (Helpers.checkSuspendAction(PlayerControl.LocalPlayer, Jackal.local.currentTarget)) return;
                    _ = new StaticAchievementToken("jackal.common1");
                    Jackal.CreateSidekick.Invoke((Jackal.local.currentTarget.PlayerId, Jackal.local.player.PlayerId));
                    SoundEffectsManager.play("jackalSidekick");
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Jackal) && Jackal.local.canCreateSidekick && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Jackal.local.canCreateSidekick && Jackal.local.currentTarget != null && PlayerControl.LocalPlayer.CanMove; },
                () => { jackalSidekickButton.Timer = jackalSidekickButton.MaxTimer;},
                Jackal.getSidekickButtonSprite(),
                CustomButton.ButtonPositions.lowerRowCenter,
                __instance,
                KeyCode.F,
                buttonText: ModTranslation.getString("SidekickText")
            );

            // Jackal Kill
            jackalKillButton = new CustomButton(
                () => {
                    if (Helpers.checkMurderAttemptAndKill(PlayerControl.LocalPlayer, Jackal.local.currentTarget) == MurderAttemptResult.SuppressKill) return;

                    jackalKillButton.Timer = jackalKillButton.MaxTimer; 
                    Jackal.local.currentTarget = null;
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Jackal) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Jackal.local.currentTarget && PlayerControl.LocalPlayer.CanMove; },
                () => { jackalKillButton.Timer = jackalKillButton.MaxTimer;},
                __instance.KillButton.graphic.sprite,
                CustomButton.ButtonPositions.upperRowRight,
                __instance,
                KeyCode.Q
            );

            yandereButton = new CustomButton(
                () =>
                {
                    if (Helpers.checkMurderAttemptAndKill(PlayerControl.LocalPlayer, Yandere.currentTarget) == MurderAttemptResult.SuppressKill) return;
                    yandereButton.MaxTimer = Yandere.nuisance.Contains(Yandere.currentTarget) ? Mathf.Max(1f, yandereButton.MaxTimer - Yandere.reducedCooldown) : yandereButton.MaxTimer + Yandere.cooldownPunishment;
                    if (Yandere.isRunaway) yandereButton.MaxTimer = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown;
                    yandereButton.Timer = yandereButton.MaxTimer;
                    Yandere.currentTarget = null;
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Yandere) && !PlayerControl.LocalPlayer.Data.IsDead && (Yandere.isRunaway || Yandere.nuisance.Count > 0); },
                () => { return Yandere.currentTarget && PlayerControl.LocalPlayer.CanMove; },
                () => { yandereButton.Timer = yandereButton.MaxTimer = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown; },
                __instance.KillButton.graphic.sprite,
                CustomButton.ButtonPositions.upperRowRight,
                __instance,
                KeyCode.Q
            );

            jekyllAndHydeKillButton = new CustomButton(
                // OnClick
                () =>
                {
                    if (Helpers.checkMurderAttemptAndKill(PlayerControl.LocalPlayer, JekyllAndHyde.local.currentTarget) == MurderAttemptResult.SuppressKill) return;

                    jekyllAndHydeKillButton.Timer = jekyllAndHydeKillButton.MaxTimer;
                    JekyllAndHyde.local.currentTarget = null;
                },
                // HasButton
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.JekyllAndHyde) && !JekyllAndHyde.local.isJekyll() && !PlayerControl.LocalPlayer.Data.IsDead; },
                // CouldUse
                () =>
                {
                    if (jekyllAndHydeKillCounterText != null)
                    {
                        jekyllAndHydeKillCounterText.text = $"{JekyllAndHyde.local.counter}/{JekyllAndHyde.numberToWin}";
                    }
                    return JekyllAndHyde.local.currentTarget != null && PlayerControl.LocalPlayer.CanMove;
                },
                // OnMeetingEnds
                () =>
                {
                    jekyllAndHydeKillButton.Timer = jekyllAndHydeKillButton.MaxTimer = JekyllAndHyde.cooldown;
                },
                __instance.KillButton.graphic.sprite,
                CustomButton.ButtonPositions.upperRowRight,
                __instance,
                KeyCode.Q,
                false
            );
            jekyllAndHydeKillCounterText = GameObject.Instantiate(jekyllAndHydeKillButton.actionButton.cooldownTimerText, jekyllAndHydeKillButton.actionButton.cooldownTimerText.transform.parent);
            jekyllAndHydeKillCounterText.text = "";
            jekyllAndHydeKillCounterText.enableWordWrapping = false;
            jekyllAndHydeKillCounterText.transform.localScale = Vector3.one * 0.5f;
            jekyllAndHydeKillCounterText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            jekyllAndHydeSuicideButton = new CustomButton(
                () => { },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.JekyllAndHyde) && !JekyllAndHyde.local.isJekyll() && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return true; },
                () =>
                {
                    if (JekyllAndHyde.reset) jekyllAndHydeSuicideButton.Timer = JekyllAndHyde.suicideTimer;
                },
                SerialKiller.getButtonSprite(),
                CustomButton.ButtonPositions.upperRowCenter,
                __instance,
                KeyCode.None,
                true,
                JekyllAndHyde.suicideTimer,
                () =>
                {
                    byte targetId = PlayerControl.LocalPlayer.PlayerId;
                    MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SerialKillerSuicide, Hazel.SendOption.Reliable, -1); killWriter.Write(targetId);
                    killWriter.Write(targetId);
                    AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                    RPCProcedure.serialKillerSuicide(targetId);
                },
                buttonText: ModTranslation.getString("serialKillerSuicideText"),
                isSuicide: true,
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            )
            {
                isEffectActive = true
            };
            ButtonEffect.SetMouseActionIcon(jekyllAndHydeSuicideButton.actionButtonGameObject, true, ModTranslation.getString("buttonsNormalSuicide"), false, ButtonEffect.ActionIconType.InfoAction);

            jekyllAndHydeDrugButton = new CustomButton(
                // OnClick
                () =>
                {
                    _ = new StaticAchievementToken("jekyllAndHyde.common1");
                    JekyllAndHyde.local.oddIsJekyll = !JekyllAndHyde.local.oddIsJekyll;
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetOddIsJekyll, Hazel.SendOption.Reliable, -1);
                    writer.Write(JekyllAndHyde.local.oddIsJekyll);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    jekyllAndHydeSuicideButton.Timer = jekyllAndHydeSuicideButton.MaxTimer;
                    jekyllAndHydeKillButton.Timer = jekyllAndHydeKillButton.MaxTimer;
                    JekyllAndHyde.local.numUsed += 1;
                    SoundEffectsManager.play("jekyllAndHydeDrug");
                },
                // HasButton
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.JekyllAndHyde) && !PlayerControl.LocalPlayer.Data.IsDead && JekyllAndHyde.local.numUsed < JekyllAndHyde.local.getNumDrugs(); },
                // CouldUse
                () =>
                {
                    if (jekyllAndHydeDrugText != null)
                    {
                        jekyllAndHydeDrugText.text = $"{JekyllAndHyde.local.numUsed} / {JekyllAndHyde.local.getNumDrugs()}";
                    }
                    return PlayerControl.LocalPlayer.CanMove;
                },
                // OnMeetingEnds
                () =>
                {
                    jekyllAndHydeDrugButton.Timer = jekyllAndHydeDrugButton.MaxTimer;
                },
                PlagueDoctor.getSyringeIcon(),
                CustomButton.ButtonPositions.upperRowLeft,
                __instance,
                KeyCode.F,
                false,
                ModTranslation.getString("DrugText"),
                CustomButton.ButtonLabelType.UseButton,
                actionName: ModTranslation.getString("drugButtonSwitch")
            );
            jekyllAndHydeDrugText = GameObject.Instantiate(jekyllAndHydeDrugButton.actionButton.cooldownTimerText, jekyllAndHydeDrugButton.actionButton.cooldownTimerText.transform.parent); 
            jekyllAndHydeDrugText.text = "";
            jekyllAndHydeDrugText.enableWordWrapping = false;
            jekyllAndHydeDrugText.transform.localScale = Vector3.one * 0.5f;
            jekyllAndHydeDrugText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            teleporterTeleportButton = new CustomButton(
                () =>
                {
                    List<byte> transportTargets = [];
                    foreach (var player in PlayerControl.AllPlayerControls)
                    {
                        if (!player.Data.Disconnected)
                        {
                            if (!player.Data.IsDead) transportTargets.Add(player.PlayerId);
                            else
                            {
                                foreach (var body in UnityEngine.Object.FindObjectsOfType<DeadBody>())
                                {
                                    if (body.ParentId == player.PlayerId) transportTargets.Add(player.PlayerId);
                                }
                            }
                        }
                    }
                    byte[] transporttargetIDs = transportTargets.ToArray();
                    var pk = new Teleporter.PlayerMenu((x) =>
                    {
                        Teleporter.local.target1 = x;
                        Teleporter.local.SwappingMenus = true;
                        Coroutines.Start(Teleporter.local.OpenSecondMenu());
                    }, (y) =>
                    {
                        return transporttargetIDs.Contains(y.PlayerId);
                    });
                    Coroutines.Start(pk.Open(0f, true));
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Teleporter) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (teleporterNumLeftText != null) {
                        teleporterNumLeftText.text = $"{Teleporter.local.teleportNumber}";
                    }
                    return Teleporter.local.teleportNumber > 0 && PlayerControl.LocalPlayer.CanMove && !Teleporter.local.SwappingMenus;
                },
                () => { teleporterTeleportButton.Timer = teleporterTeleportButton.MaxTimer; },
                Teleporter.getButtonSprite(),
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                KeyCode.F,
                false,
                ModTranslation.getString("TeleporterTeleportText"),
                CustomButton.ButtonLabelType.UseButton
            );
            teleporterNumLeftText = teleporterTeleportButton.ShowUsesIcon(3);

            plagueDoctorButton = new CustomButton(
                () =>
                {
                    if (Helpers.checkSuspendAction(PlayerControl.LocalPlayer, PlagueDoctor.currentTarget)) return;

                    byte targetId = PlagueDoctor.currentTarget.PlayerId;

                    PlagueDoctor.SetInfected.Invoke(targetId);
                    PlagueDoctor.numInfections--;
                    _ = new StaticAchievementToken("plagueDoctor.common1");

                    plagueDoctorButton.Timer = plagueDoctorButton.MaxTimer;
                    PlagueDoctor.currentTarget = null;
                    SoundEffectsManager.play("plagueDoctorSyringe");
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.PlagueDoctor) && PlagueDoctor.numInfections > 0 && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => {
                    if (plagueDoctornumInfectionsText != null)
                    {
                        if (PlagueDoctor.numInfections > 0)
                            plagueDoctornumInfectionsText.text = $"{PlagueDoctor.numInfections}";
                        else
                            plagueDoctornumInfectionsText.text = "";
                    }
                    return PlagueDoctor.currentTarget != null && PlagueDoctor.numInfections > 0 && PlayerControl.LocalPlayer.CanMove;
                },
                () => { plagueDoctorButton.Timer = plagueDoctorButton.MaxTimer; },
                PlagueDoctor.getSyringeIcon(),
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                KeyCode.F,
                buttonText: ModTranslation.getString("InfectText"),
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            );
            plagueDoctornumInfectionsText = plagueDoctorButton.ShowUsesIcon(2);

            // EvilHacker creates madmate button
            evilHackerCreatesMadmateButton = new CustomButton(
                () =>
                {
                    /*
                     * creates madmate
                     */
                    if (Helpers.checkSuspendAction(PlayerControl.LocalPlayer, EvilHacker.local.currentTarget)) return;
                    EvilHacker.CreatesMadmate.Invoke((EvilHacker.local.currentTarget.PlayerId, PlayerControl.LocalPlayer.PlayerId));
                    _ = new StaticAchievementToken("evilHacker.common1");

                },
                () =>
                {
                    return PlayerControl.LocalPlayer.isRole(RoleId.EvilHacker) && EvilHacker.local.canCreateMadmate && !PlayerControl.LocalPlayer.Data.IsDead;
                },
                () => { return EvilHacker.local.currentTarget && PlayerControl.LocalPlayer.CanMove; },
                () => { },
                EvilHacker.getMadmateButtonSprite(),
                CustomButton.ButtonPositions.lowerRowCenter,
                __instance,
                KeyCode.F,
                buttonText: ModTranslation.getString("MadmateText")
            );

            eventKickButton = new CustomButton(
                () => {
                    EventUtility.kickTarget();
                },
                () => { return EventUtility.isEnabled && Mini.mini != null && !Mini.mini.Data.IsDead && PlayerControl.LocalPlayer != Mini.mini; },
                () => { return EventUtility.currentTarget != null; },
                () => { },
                EventUtility.getKickButtonSprite(),
                CustomButton.ButtonPositions.highRowRight,
                __instance,
                KeyCode.K,
                true,
                3f,
                () => {
                    // onEffectEnds
                    eventKickButton.Timer = 69;
                },
                buttonText: "KICK"
                );

            blackmailerButton = new CustomButton(
               () => { // Action when Pressed
                   if (Blackmailer.local.currentTarget != null)
                   {
                       if (Helpers.checkSuspendAction(PlayerControl.LocalPlayer, Blackmailer.local.currentTarget)) return;
                       _ = new StaticAchievementToken("blackmailer.common1");
                       Blackmailer.local.acTokenChallenge.Value.cleared |= Blackmailer.local.acTokenChallenge.Value.witness.Any(x => x == Blackmailer.local.currentTarget.PlayerId);

                       Blackmailer.Blackmail.Invoke((Blackmailer.local.currentTarget.PlayerId, PlayerControl.LocalPlayer.PlayerId));
                       blackmailerButton.Timer = blackmailerButton.MaxTimer;
                       SoundEffectsManager.play("blackmailerSilence");
                   }
               },
               () => { return PlayerControl.LocalPlayer.isRole(RoleId.Blackmailer) && !PlayerControl.LocalPlayer.Data.IsDead; },
               () => { return Blackmailer.local.currentTarget != null && PlayerControl.LocalPlayer.CanMove; },
               () => {
                   blackmailerButton.Timer = blackmailerButton.MaxTimer;
               },
               Blackmailer.getBlackmailButtonSprite(),
               CustomButton.ButtonPositions.upperRowLeft, //brb
               __instance,
               KeyCode.F,
               buttonText: ModTranslation.getString("BlackmailerText")
           );

            // Sidekick Kill
            sidekickKillButton = new CustomButton(
                () => {
                    if (Helpers.checkMurderAttemptAndKill(PlayerControl.LocalPlayer, Sidekick.local.currentTarget) == MurderAttemptResult.SuppressKill) return;
                    sidekickKillButton.Timer = sidekickKillButton.MaxTimer; 
                    Sidekick.local.currentTarget = null;
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Sidekick) && Sidekick.canKill && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Sidekick.local.currentTarget && PlayerControl.LocalPlayer.CanMove; },
                () => { sidekickKillButton.Timer = sidekickKillButton.MaxTimer;},
                __instance.KillButton.graphic.sprite,
                CustomButton.ButtonPositions.upperRowRight,
                __instance,
                KeyCode.Q
            );

            collatorButton = new CustomButton(
                () => {
                    var sample = Collator.local.currentTarget;
                    var info = RoleInfo.getRoleInfoForPlayer(sample, false, true).FirstOrDefault();

                    if ((Madmate.madmate.Any(x => x.PlayerId == sample.PlayerId) || CreatedMadmate.createdMadmate.Any(x => x.PlayerId == sample.PlayerId)) && !Collator.madmateAsCrew)
                        info = RoleInfo.impostor;

                    Collator.local.sampledPlayers.Add((sample, Collator.CheckTeam(info)));

                    TORGUIManager.Instance.StartCoroutine(Collator.local.CoShakeTube(Collator.local.sampledPlayers.Count - 1).WrapToIl2Cpp());
                    Collator.local.UpdateSamples();
                    collatorButton.Timer = collatorButton.MaxTimer;
                    Collator.local.currentTarget = null;
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Collator) && Collator.local.sampledPlayers.Count < Collator.local.allSamples.Length && Collator.local.trialsLeft > 0 && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (collatorUsesText != null)
                    {
                        if (Collator.local.trialsLeft > 0)
                            collatorUsesText.text = $"{Collator.local.trialsLeft}";
                        else
                            collatorUsesText.text = "";
                    }
                    return Collator.local.currentTarget && PlayerControl.LocalPlayer.CanMove;
                },
                () => { collatorButton.Timer = collatorButton.MaxTimer; },
                Collator.getButtonSprite(),
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                KeyCode.F,
                buttonText: ModTranslation.getString("CollatorText"),
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            );
            collatorUsesText = collatorButton.ShowUsesIcon(3);

            jailorButton = new CustomButton(
                () => {
                    jailorButton.Timer = jailorButton.MaxTimer;
                    Jailor.Jail.Invoke((PlayerControl.LocalPlayer.PlayerId, Jailor.local.currentTarget.PlayerId));
                    Jailor.local.currentTarget = null;
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Jailor) && Jailor.local.remainingUses > 0 && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (jailorUsesText != null)
                    {
                        if (Jailor.local.remainingUses > 0)
                            jailorUsesText.text = $"{Jailor.local.remainingUses}";
                        else
                            jailorUsesText.text = "";
                    }
                    return Jailor.local.currentTarget && PlayerControl.LocalPlayer.CanMove;
                },
                () => { jailorButton.Timer = jailorButton.MaxTimer; },
                Jailor.getButtonSprite(),
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                KeyCode.F,
                buttonText: ModTranslation.getString("JailorText"),
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            );
            jailorUsesText = jailorButton.ShowUsesIcon(3);

            // Eraser erase button
            eraserButton = new CustomButton(
                () => {
                    if (Helpers.checkSuspendAction(PlayerControl.LocalPlayer, Eraser.local.currentTarget)) return;
                    eraserButton.MaxTimer += Eraser.cooldownIncrease;
                    eraserButton.Timer = eraserButton.MaxTimer;

                    _ = new StaticAchievementToken("eraser.common1");
                    if (Eraser.local.currentTarget.Data.Role.IsImpostor) _ = new StaticAchievementToken("eraser.another1");
                    Eraser.local.acTokenChallenge.Value++;

                    Eraser.SetFutureErased.Invoke(Eraser.local.currentTarget.PlayerId);
                    SoundEffectsManager.play("eraserErase");
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Eraser) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove && Eraser.local.currentTarget != null; },
                () => { eraserButton.Timer = eraserButton.MaxTimer;},
                Eraser.getButtonSprite(),
                CustomButton.ButtonPositions.upperRowLeft,
                __instance,
                KeyCode.F,
                buttonText: ModTranslation.getString("EraserText")
            );

            trapperSetTrapButton = new CustomButton(
                () =>
                { // ボタンが押された時に実行
                    if (!PlayerControl.LocalPlayer.CanMove || Trap.hasTrappedPlayer()) return;
                    Trapper.setTrap();
                    trapperSetTrapButton.Timer = trapperSetTrapButton.MaxTimer;
                },
                () =>
                { /*ボタン有効になる条件*/
                    return PlayerControl.LocalPlayer.isRole(RoleId.Trapper) && !PlayerControl.LocalPlayer.Data.IsDead;
                },
                () =>
                { /*ボタンが使える条件*/
                    return PlayerControl.LocalPlayer.CanMove && !Trap.hasTrappedPlayer();
                },
                () =>
                { /*ミーティング終了時*/
                    trapperSetTrapButton.Timer = trapperSetTrapButton.MaxTimer;
                },
                Trapper.getTrapButtonSprite(),
                CustomButton.ButtonPositions.upperRowLeft,
                __instance,
                KeyCode.F,
                buttonText: ModTranslation.getString("PlaceTrapText"),
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            );

            moriartyBrainwashButton = new CustomButton(
                () =>
                {
                    if (Moriarty.local.currentTarget != null)
                    {
                        if (Helpers.checkSuspendAction(PlayerControl.LocalPlayer, Moriarty.local.currentTarget)) return;
                        _ = new StaticAchievementToken("moriarty.common1");
                        Moriarty.Brainwash.Invoke((Moriarty.local.currentTarget.PlayerId, PlayerControl.LocalPlayer.PlayerId));
                        SoundEffectsManager.play("moriartyBrainwash");

                        // 洗脳終了までのカウントダウン
                        Moriarty.local.generateBrainwashText();
                    }
                    Moriarty.local.tmpTarget = null;
                    moriartyBrainwashButton.Timer = moriartyBrainwashButton.MaxTimer;
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Moriarty) && !PlayerControl.LocalPlayer.Data.IsDead && Moriarty.local.target == null; },
                () => { return PlayerControl.LocalPlayer.CanMove && Moriarty.local.currentTarget != null; },
                () => { moriartyBrainwashButton.Timer = moriartyBrainwashButton.MaxTimer; },
                Moriarty.getBrainwashIcon(),
                CustomButton.ButtonPositions.upperRowLeft,
                __instance,
                KeyCode.F,
                buttonText: ModTranslation.getString("BrainwashText"),
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            );

            moriartyKillButton = new CustomButton(
                () =>
                {
                    MurderAttemptResult murder = Helpers.checkMurderAttemptAndKill(PlayerControl.LocalPlayer, Moriarty.local.killTarget, showAnimation: false);
                    if (murder == MurderAttemptResult.PerformKill)
                    {
                        Moriarty.UpdateCounter.Invoke((Moriarty.local.killTarget.PlayerId, PlayerControl.LocalPlayer.PlayerId));
                        Moriarty.local.target = null;
                        moriartyBrainwashButton.Timer = moriartyBrainwashButton.MaxTimer;
                    }
                },
                // HasButton
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Moriarty) && !PlayerControl.LocalPlayer.Data.IsDead; },
                // CouldUse
                () =>
                {
                    if (moriartyKillCounterText != null)
                    {
                        moriartyKillCounterText.text = $"{Moriarty.local.counter}/{Moriarty.numberToWin}";
                    }
                    moriartyKillButton.buttonText = Moriarty.local.killTarget ? Moriarty.local.killTarget.Data.PlayerName : ModTranslation.getString("moriartyTargetNone");
                    return Moriarty.local.killTarget != null && PlayerControl.LocalPlayer.CanMove;
                },
                // OnMeetingEnds
                () =>
                {
                    moriartyKillButton.Timer = moriartyKillButton.MaxTimer;
                    Moriarty.brainwashed.Clear();
                },
                __instance.KillButton.graphic.sprite,
                CustomButton.ButtonPositions.upperRowRight,
                __instance,
                KeyCode.Q,
                buttonText: ModTranslation.getString("moriartyTargetNone"),
                actionName: ModTranslation.getString("moriartyKillTarget")
            );
            moriartyKillCounterText = GameObject.Instantiate(moriartyKillButton.actionButton.cooldownTimerText, moriartyKillButton.actionButton.cooldownTimerText.transform.parent);
            moriartyKillCounterText.text = "";
            moriartyKillCounterText.enableWordWrapping = false;
            moriartyKillCounterText.transform.localScale = Vector3.one * 0.5f;
            moriartyKillCounterText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            placeJackInTheBoxButton = new CustomButton(
                () => {
                    placeJackInTheBoxButton.Timer = placeJackInTheBoxButton.MaxTimer;

                    var pos = PlayerControl.LocalPlayer.transform.position;
                    Trickster.PlaceBox.Invoke(pos);
                    SoundEffectsManager.play("tricksterPlaceBox");
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Trickster) && !PlayerControl.LocalPlayer.Data.IsDead && !JackInTheBox.hasJackInTheBoxLimitReached(); },
                () =>
                {
                    if (tricksterBoxesText != null) tricksterBoxesText.text = (JackInTheBox.JackInTheBoxLimit - JackInTheBox.AllJackInTheBoxes.Count).ToString();
                    return PlayerControl.LocalPlayer.CanMove && !JackInTheBox.hasJackInTheBoxLimitReached();
                },
                () => { placeJackInTheBoxButton.Timer = placeJackInTheBoxButton.MaxTimer;},
                Trickster.getPlaceBoxButtonSprite(),
                CustomButton.ButtonPositions.upperRowLeft,
                __instance,
                KeyCode.F,
                buttonText: ModTranslation.getString("PlaceJackInTheBoxText")
            );
            tricksterBoxesText = placeJackInTheBoxButton.ShowUsesIcon(0);

            lightsOutButton = new CustomButton(
                () => {
                    _ = new StaticAchievementToken("trickster.common2");
                    Trickster.LightsOut.Invoke();
                    SoundEffectsManager.play("lighterLight");
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Trickster) && !PlayerControl.LocalPlayer.Data.IsDead && JackInTheBox.hasJackInTheBoxLimitReached() && JackInTheBox.boxesConvertedToVents; },
                () => { return PlayerControl.LocalPlayer.CanMove && JackInTheBox.hasJackInTheBoxLimitReached() && JackInTheBox.boxesConvertedToVents; },
                () => { 
                    lightsOutButton.Timer = lightsOutButton.MaxTimer;
                    lightsOutButton.isEffectActive = false;
                    lightsOutButton.actionButton.graphic.color = Palette.EnabledColor;
                    if (Trickster.acTokenChallenge != null) Trickster.acTokenChallenge.Value.kills = 0;
                },
                Trickster.getLightsOutButtonSprite(),
                CustomButton.ButtonPositions.upperRowLeft,
                __instance,
                KeyCode.F,
                true,
                Trickster.lightsOutDuration,
                () => {
                    lightsOutButton.Timer = lightsOutButton.MaxTimer;
                    SoundEffectsManager.play("lighterLight");
                    Trickster.acTokenChallenge.Value.kills = 0;
                },
                buttonText: ModTranslation.getString("LightsOutText")
            );

            // Cleaner Clean
            cleanerCleanButton = new CustomButton(
                () => {
                    foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(PlayerControl.LocalPlayer.GetTruePosition(), PlayerControl.LocalPlayer.MaxReportDistance, Constants.PlayersOnlyMask)) {
                        if (collider2D.tag == "DeadBody")
                        {
                            DeadBody component = collider2D.GetComponent<DeadBody>();
                            if (component && !component.Reported)
                            {
                                Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
                                Vector2 truePosition2 = component.TruePosition;
                                if (Vector2.Distance(truePosition2, truePosition) <= PlayerControl.LocalPlayer.MaxReportDistance && PlayerControl.LocalPlayer.CanMove && !PhysicsHelpers.AnythingBetween(truePosition, truePosition2, Constants.ShipAndObjectsMask, false))
                                {
                                    _ = new StaticAchievementToken("cleaner.common1");
                                    _ = new StaticAchievementToken("cleaner.challenge");

                                    NetworkedPlayerInfo playerInfo = GameData.Instance.GetPlayerById(component.ParentId);

                                    RPCProcedure.CleanBody.Invoke((playerInfo.PlayerId, PlayerControl.LocalPlayer.PlayerId));

                                    PlayerControl.LocalPlayer.killTimer = PlayerControl.LocalPlayer.GetKillCooldown();
                                    cleanerCleanButton.Timer = cleanerCleanButton.MaxTimer;
                                    SoundEffectsManager.play("cleanerClean");
                                    break;
                                }
                            }
                        }
                    }
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Cleaner) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return __instance.ReportButton.graphic.color == Palette.EnabledColor && PlayerControl.LocalPlayer.CanMove; },
                () => { cleanerCleanButton.Timer = cleanerCleanButton.MaxTimer; },
                Cleaner.getButtonSprite(),
                CustomButton.ButtonPositions.upperRowLeft,
                __instance,
                KeyCode.F,
                buttonText: ModTranslation.getString("CleanText")
            );

            // Warlock curse
            warlockCurseButton = new CustomButton(
                () => {
                    if (Warlock.local.curseVictim == null) {
                        if (Helpers.checkSuspendAction(PlayerControl.LocalPlayer, Warlock.local.currentTarget)) return;
                        // Apply Curse
                        Warlock.local.curseVictim = Warlock.local.currentTarget;
                        warlockCurseButton.Sprite = Warlock.getCurseKillButtonSprite();
                        warlockCurseButton.Timer = 1f;
                        warlockCurseButton.buttonText = ModTranslation.getString("CurseKillText");
                        SoundEffectsManager.play("warlockCurse");

                        // Ghost Info
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareGhostInfo, Hazel.SendOption.Reliable, -1);
                        writer.Write(PlayerControl.LocalPlayer.PlayerId);
                        writer.Write((byte)RPCProcedure.GhostInfoTypes.WarlockTarget);
                        writer.Write(Warlock.local.curseVictim.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);

                    } else if (Warlock.local.curseVictim != null && Warlock.local.curseVictimTarget != null) {
                        MurderAttemptResult murder = Helpers.checkMurderAttemptAndKill(PlayerControl.LocalPlayer, Warlock.local.curseVictimTarget, showAnimation: false);
                        if (murder == MurderAttemptResult.SuppressKill) return;
                        if (murder == MurderAttemptResult.PerformKill) {
                            _ = new StaticAchievementToken("warlock.common1");
                            _ = new StaticAchievementToken("warlock.challenge");
                            if (Warlock.local.curseVictimTarget == PlayerControl.LocalPlayer)
                                _ = new StaticAchievementToken("warlock.another1");
                        }

                        warlockCurseButton.buttonText = ModTranslation.getString("CurseText");
                        // If blanked or killed
                        if (Warlock.rootTime > 0) {
                            AntiTeleport.position = PlayerControl.LocalPlayer.transform.position;
                            PlayerControl.LocalPlayer.moveable = false;
                            PlayerControl.LocalPlayer.NetTransform.Halt(); // Stop current movement so the warlock is not just running straight into the next object
                            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Warlock.rootTime, new Action<float>((p) => { // Delayed action
                                if (p == 1f) {
                                    PlayerControl.LocalPlayer.moveable = true;
                                }
                            })));
                        }

                        Warlock.local.curseVictim = null;
                        Warlock.local.curseVictimTarget = null;
                        warlockCurseButton.Sprite = Warlock.getCurseButtonSprite();
                        PlayerControl.LocalPlayer.killTimer = PlayerControl.LocalPlayer.GetKillCooldown();
                        warlockCurseButton.Timer = warlockCurseButton.MaxTimer;

                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareGhostInfo, Hazel.SendOption.Reliable, -1);
                        writer.Write(PlayerControl.LocalPlayer.PlayerId);
                        writer.Write((byte)RPCProcedure.GhostInfoTypes.WarlockTarget);
                        writer.Write(Byte.MaxValue); // This will set it to null!
                        AmongUsClient.Instance.FinishRpcImmediately(writer);

                    }
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Warlock) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return ((Warlock.local.curseVictim == null && Warlock.local.currentTarget != null) || (Warlock.local.curseVictim != null && Warlock.local.curseVictimTarget != null)) && PlayerControl.LocalPlayer.CanMove; },
                () => {
                    warlockCurseButton.Timer = warlockCurseButton.MaxTimer;
                    warlockCurseButton.Sprite = Warlock.getCurseButtonSprite();
                    warlockCurseButton.buttonText = ModTranslation.getString("CurseText");
                },
                Warlock.getCurseButtonSprite(),
                CustomButton.ButtonPositions.upperRowLeft,
                __instance,
                KeyCode.F,
                buttonText: ModTranslation.getString("CurseText"),
                actionName: ModTranslation.getString("warlockButton")
            );

            // Security Guard button
            securityGuardButton = new CustomButton(
                () => {
                    if (SecurityGuard.ventTarget != null) { // Seal vent
                        SecurityGuard.acTokenCommon.Value.vent = true;

                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SealVent, Hazel.SendOption.Reliable);
                        writer.WritePacked(SecurityGuard.ventTarget.Id);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.sealVent(SecurityGuard.ventTarget.Id);
                        SecurityGuard.ventTarget = null;
                        
                    } else if (GameOptionsManager.Instance.currentNormalGameOptions.MapId != 1 && !SubmergedCompatibility.IsSubmerged) { // Place camera if there's no vent and it's not MiraHQ or Submerged
                        SecurityGuard.acTokenCommon.Value.camera = true;
                        
                        var pos = PlayerControl.LocalPlayer.transform.position;
                        byte[] buff = new byte[sizeof(float) * 2];
                        Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, buff, 0*sizeof(float), sizeof(float));
                        Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, buff, 1*sizeof(float), sizeof(float));

                        byte roomId;
                        try {
                            roomId = (byte)FastDestroyableSingleton<HudManager>.Instance.roomTracker.LastRoom.RoomId;
                        } catch {
                            roomId = (byte)SystemTypes.Outside;
                        }

                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PlaceCamera, Hazel.SendOption.Reliable);
                        writer.WriteBytesAndSize(buff);
                        writer.Write(roomId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.placeCamera(buff, roomId); 
                    }
                    SoundEffectsManager.play("securityGuardPlaceCam");  // Same sound used for both types (cam or vent)!
                    securityGuardButton.Timer = securityGuardButton.MaxTimer;
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.SecurityGuard) && !PlayerControl.LocalPlayer.Data.IsDead && SecurityGuard.remainingScrews >= Mathf.Min(SecurityGuard.ventPrice, SecurityGuard.camPrice); },
                () => {
                    securityGuardButton.actionButton.graphic.sprite = (SecurityGuard.ventTarget == null && !Helpers.isMira() && !Helpers.isFungle() && GameOptionsManager.Instance.currentNormalGameOptions.MapId != 1 && !SubmergedCompatibility.IsSubmerged) ? SecurityGuard.getPlaceCameraButtonSprite() : SecurityGuard.getCloseVentButtonSprite(); 
                    if (SecurityGuard.ventTarget == null && !Helpers.isMira() && !Helpers.isFungle() && GameOptionsManager.Instance.currentNormalGameOptions.MapId != 1 && !SubmergedCompatibility.IsSubmerged)
                    {
                        securityGuardButton.buttonText = ModTranslation.getString("PlaceCameraText");
                    }
                    else
                    {
                        securityGuardButton.buttonText = ModTranslation.getString("CloseVentText");
                    }
                    if (securityGuardButtonScrewsText != null) securityGuardButtonScrewsText.text = $"{SecurityGuard.remainingScrews}/{SecurityGuard.totalScrews}";

                    if (SecurityGuard.ventTarget != null)
                        return SecurityGuard.remainingScrews >= SecurityGuard.ventPrice && PlayerControl.LocalPlayer.CanMove;
                    return !Helpers.isMira() && !Helpers.isFungle() && !SubmergedCompatibility.IsSubmerged && SecurityGuard.remainingScrews >= SecurityGuard.camPrice && PlayerControl.LocalPlayer.CanMove;
                },
                () => { securityGuardButton.Timer = securityGuardButton.MaxTimer; },
                SecurityGuard.getPlaceCameraButtonSprite(),
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                KeyCode.F,
                abilityTexture: CustomButton.ButtonLabelType.UseButton,
                buttonText: ModTranslation.getString("PlaceCameraText"),
                actionName: ModTranslation.getString("securityGuardButton")
            );

            // Security Guard button screws counter
            securityGuardButtonScrewsText = GameObject.Instantiate(securityGuardButton.actionButton.cooldownTimerText, securityGuardButton.actionButton.cooldownTimerText.transform.parent);
            securityGuardButtonScrewsText.text = "";
            securityGuardButtonScrewsText.enableWordWrapping = false;
            securityGuardButtonScrewsText.transform.localScale = Vector3.one * 0.5f;
            securityGuardButtonScrewsText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            securityGuardCamButton = new CustomButton(
                () => {
                    if (!Helpers.isMira()) {
                        if (SecurityGuard.minigame == null) {
                            SecurityGuard.acTokenChallenge.Value++;

                            byte mapId = GameOptionsManager.Instance.currentNormalGameOptions.MapId;
                            UnityEngine.Object.FindObjectsOfType<SystemConsole>().ToList().ForEach(x => TheOtherRolesPlugin.Logger.LogMessage($"{x.name} {x.GetType()}, {x.MinigamePrefab.TaskType}"));
                            var e = UnityEngine.Object.FindObjectsOfType<SystemConsole>().FirstOrDefault(x => x.gameObject.name.Contains("Surv_Panel") || x.name.Contains("Cam") || x.name.Contains("BinocularsSecurityConsole"));
                            if (Helpers.isSkeld() || mapId == 3) e = UnityEngine.Object.FindObjectsOfType<SystemConsole>().FirstOrDefault(x => x.gameObject.name.Contains("SurvConsole"));
                            else if (Helpers.isAirship()) e = UnityEngine.Object.FindObjectsOfType<SystemConsole>().FirstOrDefault(x => x.gameObject.name.Contains("task_cams"));
                            if (e == null || Camera.main == null) return;
                            SecurityGuard.minigame = UnityEngine.Object.Instantiate(e.MinigamePrefab, Camera.main.transform, false);
                        }
                        SecurityGuard.minigame.transform.SetParent(Camera.main.transform, false);
                        SecurityGuard.minigame.transform.localPosition = new Vector3(0.0f, 0.0f, -50f);
                        SecurityGuard.minigame.Begin(null);
                    } else {
                        if (SecurityGuard.minigame == null) {
                            var e = UnityEngine.Object.FindObjectsOfType<SystemConsole>().FirstOrDefault(x => x.gameObject.name.Contains("SurvLogConsole"));
                            if (e == null || Camera.main == null) return;
                            SecurityGuard.minigame = UnityEngine.Object.Instantiate(e.MinigamePrefab, Camera.main.transform, false);
                        }
                        SecurityGuard.minigame.transform.SetParent(Camera.main.transform, false);
                        SecurityGuard.minigame.transform.localPosition = new Vector3(0.0f, 0.0f, -50f);
                        SecurityGuard.minigame.Begin(null);
                    }
                    SecurityGuard.charges--;

                    if (SecurityGuard.cantMove) PlayerControl.LocalPlayer.moveable = false;
                    PlayerControl.LocalPlayer.NetTransform.Halt(); // Stop current movement 
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.SecurityGuard) && !PlayerControl.LocalPlayer.Data.IsDead && SecurityGuard.remainingScrews < Mathf.Min(SecurityGuard.ventPrice, SecurityGuard.camPrice)
                               && !SubmergedCompatibility.IsSubmerged; },
                () => {
                    if (securityGuardChargesText != null) securityGuardChargesText.text = $"{SecurityGuard.charges} / {SecurityGuard.maxCharges}";
                    securityGuardCamButton.actionButton.graphic.sprite = Helpers.isMira() ? SecurityGuard.getLogSprite() : SecurityGuard.getCamSprite();
                    securityGuardCamButton.actionButton.OverrideText(Helpers.isMira() ?
                        TranslationController.Instance.GetString(StringNames.SecurityLogsSystem) :
                        ModTranslation.getString("securityGuardCamButton"));
                    return PlayerControl.LocalPlayer.CanMove && SecurityGuard.charges > 0;
                },
                () => {
                    securityGuardCamButton.Timer = securityGuardCamButton.MaxTimer;
                    securityGuardCamButton.isEffectActive = false;
                    securityGuardCamButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                SecurityGuard.getCamSprite(),
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                KeyCode.G,
                true,
                0f,
                () => {
                    securityGuardCamButton.Timer = securityGuardCamButton.MaxTimer;
                    if (Minigame.Instance) {
                        SecurityGuard.minigame.ForceClose();
                    }
                    PlayerControl.LocalPlayer.moveable = true;
                },
                false,
                Helpers.isMira() ?
                TranslationController.Instance.GetString(StringNames.SecurityLogsSystem) :
                ModTranslation.getString("securityGuardCamButton"),
                abilityTexture: CustomButton.ButtonLabelType.AdminButton
            );

            // Security Guard cam button charges
            securityGuardChargesText = GameObject.Instantiate(securityGuardCamButton.actionButton.cooldownTimerText, securityGuardCamButton.actionButton.cooldownTimerText.transform.parent);
            securityGuardChargesText.text = "";
            securityGuardChargesText.enableWordWrapping = false;
            securityGuardChargesText.transform.localScale = Vector3.one * 0.5f;
            securityGuardChargesText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            securityGuardFlushButton = new(
                () =>
                {
                    _ = new StaticAchievementToken("securityGuard.another1");
                    if (PlayerControl.AllPlayerControls.ToArray().Any(x => x.inVent && Helpers.isVisible(PlayerControl.LocalPlayer, x)))
                        _ = new StaticAchievementToken("securityGuard.another2");

                    SecurityGuard.RpcFlush.Invoke();
                    securityGuardFlushButton.Timer = securityGuardFlushButton.MaxTimer;
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.SecurityGuard) && SecurityGuard.remainingScrews < Mathf.Min(SecurityGuard.ventPrice, SecurityGuard.camPrice) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () => { securityGuardFlushButton.Timer = securityGuardFlushButton.MaxTimer; },
                SecurityGuard.getFlushSprite(),
                CustomButton.ButtonPositions.lowerRowCenter,
                __instance,
                KeyCode.K,
                buttonText: ModTranslation.getString("FlushText")
            );

            // Arsonist button
            arsonistButton = new CustomButton(
                () => {
                    bool dousedEveryoneAlive = Arsonist.local.dousedEveryoneAlive();
                    if (dousedEveryoneAlive) {
                        _ = new StaticAchievementToken("arsonist.challenge");
                        Arsonist.TriggerWin.Invoke(PlayerControl.LocalPlayer.PlayerId);
                        arsonistButton.HasEffect = false;
                    } else if (Arsonist.local.currentTarget != null) {
                        if (Helpers.checkSuspendAction(PlayerControl.LocalPlayer, Arsonist.local.currentTarget)) return;
                        Arsonist.local.douseTarget = Arsonist.local.currentTarget;
                        arsonistButton.HasEffect = true;
                        SoundEffectsManager.play("arsonistDouse");
                    }
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Arsonist) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => {
                    bool dousedEveryoneAlive = Arsonist.local.dousedEveryoneAlive();
                    if (dousedEveryoneAlive)
                    {
                        arsonistButton.actionButton.graphic.sprite = Arsonist.getIgniteSprite();
                        arsonistButton.buttonText = ModTranslation.getString("IgniteText");
                    }
                    
                    if (arsonistButton.isEffectActive && Arsonist.local.douseTarget != Arsonist.local.currentTarget) {
                        Arsonist.local.douseTarget = null;
                        arsonistButton.Timer = 0f;
                        arsonistButton.isEffectActive = false;
                    }

                    return PlayerControl.LocalPlayer.CanMove && (dousedEveryoneAlive || Arsonist.local.currentTarget != null);
                },
                () => {
                    arsonistButton.Timer = arsonistButton.MaxTimer;
                    arsonistButton.isEffectActive = false;
                },
                Arsonist.getDouseSprite(),
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                KeyCode.F,
                true,
                Arsonist.duration,
                () => {
                    if (Arsonist.local.douseTarget != null) Arsonist.Douse.Invoke((PlayerControl.LocalPlayer.PlayerId, Arsonist.local.douseTarget.PlayerId));
                    _ = new StaticAchievementToken("arsonist.common1");
                    arsonistButton.Timer = Arsonist.local.dousedEveryoneAlive() ? 0 : arsonistButton.MaxTimer;

                    foreach (PlayerControl p in Arsonist.local.dousedPlayers) {
                        if (TORMapOptions.playerIcons.ContainsKey(p.PlayerId)) {
                            TORMapOptions.playerIcons[p.PlayerId].setSemiTransparent(false);
                        }
                    }
                    Arsonist.local.douseTarget = null;
                },
                buttonText: ModTranslation.getString("DouseText"),
                shakeOnEnd: false
            );

            // Veteran Alert
            veteranAlertButton = new CustomButton(
                () => {
                    Veteran.ActivateAlert.Invoke(PlayerControl.LocalPlayer.PlayerId);

                    Veteran.local.remainingAlerts--;
                    SoundEffectsManager.play("veteranAlert");
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Veteran) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => {
                    if (veteranButtonAlertText != null) veteranButtonAlertText.text = $"{Veteran.local.remainingAlerts}";
                    return PlayerControl.LocalPlayer.CanMove && Veteran.local.remainingAlerts > 0; 
                },
                () => {
                    veteranAlertButton.Timer = veteranAlertButton.MaxTimer;
                    veteranAlertButton.isEffectActive = false;
                    veteranAlertButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Veteran.getButtonSprite(),
                CustomButton.ButtonPositions.lowerRowRight, //brb
                __instance,
                KeyCode.F,
                true,
                Veteran.alertDuration,
                () => { veteranAlertButton.Timer = veteranAlertButton.MaxTimer; },
                buttonText: ModTranslation.getString("AlertText"),
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            );
            veteranButtonAlertText = veteranAlertButton.ShowUsesIcon(3);

            archaeologistDetectButton = new CustomButton(
                () =>
                {
                    var remainingList = Antique.antiques.Where(x => !x.isBroken).ToList();
                    var id = rnd.Next(remainingList.Count);
                    Archaeologist.DetectAntique.Invoke((byte)id);
                    _ = new StaticAchievementToken("archaeologist.common1");
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Archaeologist) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove && Archaeologist.hasRemainingAntique(); },
                () => { archaeologistDetectButton.Timer = archaeologistDetectButton.MaxTimer = Archaeologist.cooldown; },
                Archaeologist.getDetectSprite(),
                CustomButton.ButtonPositions.upperRowRight,
                __instance,
                KeyCode.F,
                true,
                Archaeologist.arrowDuration,
                () =>
                {
                    archaeologistDetectButton.Timer = archaeologistDetectButton.MaxTimer;
                    Archaeologist.arrowActive = false;
                },
                buttonText: ModTranslation.getString("DetectText"),
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            );

            archaeologistExcavateButton = new CustomButton(
                () =>
                {
                    if (Archaeologist.target != null)
                    {
                        Archaeologist.antiqueTarget = Archaeologist.target;
                        archaeologistExcavateButton.HasEffect = true;
                    }
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Archaeologist) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (archaeologistExcavateButton.isEffectActive && Archaeologist.target != Archaeologist.antiqueTarget)
                    {
                        Archaeologist.antiqueTarget = null;
                        archaeologistExcavateButton.Timer = 0f;
                        archaeologistExcavateButton.isEffectActive = false;
                    }
                    return Archaeologist.target != null && PlayerControl.LocalPlayer.CanMove && Archaeologist.hasRemainingAntique();
                },
                () =>
                {
                    archaeologistExcavateButton.Timer = archaeologistExcavateButton.MaxTimer = Archaeologist.cooldown;
                    archaeologistExcavateButton.isEffectActive = false;
                    archaeologistExcavateButton.actionButton.graphic.color = Palette.EnabledColor;
                    Archaeologist.antiqueTarget = null;
                },
                Archaeologist.getExcavateSprite(),
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                KeyCode.G,
                true,
                Archaeologist.detectDuration,
                () =>
                {
                    byte index = (byte)Archaeologist.antiqueTarget.id;
                    Archaeologist.ExcavateAntique.Invoke(index);
                    Archaeologist.revealed.RemoveAll(x => x.isBroken);
                    archaeologistExcavateButton.Timer = archaeologistExcavateButton.MaxTimer;
                },
                buttonText: ModTranslation.getString("ExcavateText"),
                abilityTexture: CustomButton.ButtonLabelType.UseButton,
                shakeOnEnd: false
            );

            //createRoleSummaryButton(__instance);

            accelAttributeButton = new CustomButton(
                () => { },
                () => { return Props.AccelTrap.acceled.ContainsKey(PlayerControl.LocalPlayer) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (accelAttributePropTip != null)
                        accelAttributePropTip.ProptipText = string.Format(ModTranslation.getString("accelTrapPropTip"), Math.Abs(Math.Round(CustomOptionHolder.accelerationDuration.getFloat() - DateTime.UtcNow.Subtract(Props.AccelTrap.acceled[PlayerControl.LocalPlayer]).TotalSeconds)));
                    return true;
                },
                () => { },
                Helpers.loadSpriteFromResources("TheOtherRoles.Resources.AccelAttribute.png", 250f),
                new Vector3(-0.5f, 1f, 0f),
                __instance,
                null,
                true,
                isSuicide: true
            )
            {
                showButtonText = false
            };

            accelAttributePropTip = accelAttributeButton.actionButtonGameObject.AddComponent<Props.Proptip>();

            decelAttributeButton = new CustomButton(
                () => { },
                () => { return Props.DecelTrap.deceled.ContainsKey(PlayerControl.LocalPlayer) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (decelAttributePropTip != null)
                        decelAttributePropTip.ProptipText = string.Format(ModTranslation.getString("decelTrapPropTip"), Math.Abs(Math.Round(CustomOptionHolder.decelerationDuration.getFloat() - DateTime.UtcNow.Subtract(Props.DecelTrap.deceled[PlayerControl.LocalPlayer]).TotalSeconds)));
                    return true;
                },
                () => { },
                Helpers.loadSpriteFromResources("TheOtherRoles.Resources.DecelAttribute.png", 250f),
                new Vector3(0.1f, 1f, 0),
                __instance,
                null,
                true,
                isSuicide: true
            )
            {
                showButtonText = false
            };

            decelAttributePropTip = decelAttributeButton.actionButtonGameObject.AddComponent<Props.Proptip>();

            bomberAPlantBombButton = new CustomButton(
                // OnClick
                () =>
                {
                    if (BomberA.currentTarget != null)
                    {
                        BomberA.tmpTarget = BomberA.currentTarget;
                        bomberAPlantBombButton.HasEffect = true;
                    }
                },
                // HasButton
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.BomberA) && !PlayerControl.LocalPlayer.Data.IsDead && BomberB.isAlive(); },
                // CouldUse
                () =>
                {
                    if (bomberAPlantBombButton.isEffectActive && BomberA.tmpTarget != BomberA.currentTarget)
                    {
                        BomberA.tmpTarget = null;
                        bomberAPlantBombButton.Timer = 0f;
                        bomberAPlantBombButton.isEffectActive = false;
                    }

                    return PlayerControl.LocalPlayer.CanMove && BomberA.currentTarget != null;
                },
                // OnMeetingEnds
                () =>
                {
                    bomberAPlantBombButton.Timer = bomberAPlantBombButton.MaxTimer;
                    bomberAPlantBombButton.isEffectActive = false;
                    BomberA.tmpTarget = null;                    
                },
                BomberA.getBomberButtonSprite(),
                CustomButton.ButtonPositions.upperRowCenter,
                __instance,
                KeyCode.F,
                true,
                BomberA.duration,
                // OnEffectsEnd
                () =>
                {
                    if (BomberA.tmpTarget != null)
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PlantBomb, Hazel.SendOption.Reliable, -1);
                        writer.Write(BomberA.tmpTarget.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        BomberA.bombTarget = BomberA.tmpTarget;
                    }
                    _ = new StaticAchievementToken("bomber.common1");
                    BomberA.tmpTarget = null;
                    bomberAPlantBombButton.Timer = bomberAPlantBombButton.MaxTimer;
                    SoundEffectsManager.play("bomberPlantBomb");
                },
                buttonText: ModTranslation.getString("PlantBombText"),
                shakeOnEnd: false
            );

            bomberBPlantBombButton = new CustomButton(
                // OnClick
                () =>
                {
                    if (BomberB.currentTarget != null)
                    {
                        BomberB.tmpTarget = BomberB.currentTarget;
                        bomberBPlantBombButton.HasEffect = true;
                    }
                },
                // HasButton
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.BomberB) && !PlayerControl.LocalPlayer.Data.IsDead && BomberA.isAlive(); },
                // CouldUse
                () =>
                {
                    if (bomberBPlantBombButton.isEffectActive && BomberB.tmpTarget != BomberB.currentTarget)
                    {
                        BomberB.tmpTarget = null;
                        bomberBPlantBombButton.Timer = 0f;
                        bomberBPlantBombButton.isEffectActive = false;
                    }

                    return PlayerControl.LocalPlayer.CanMove && BomberB.currentTarget != null;
                },
                // OnMeetingEnds
                () =>
                {
                    bomberBPlantBombButton.Timer = bomberBPlantBombButton.MaxTimer;
                    bomberBPlantBombButton.isEffectActive = false;
                    BomberB.tmpTarget = null;                    
                },
                BomberA.getBomberButtonSprite(),
                CustomButton.ButtonPositions.upperRowCenter,
                __instance,
                KeyCode.F,
                true,
                BomberA.duration,
                // OnEffectsEnd
                () =>
                {
                    if (BomberB.tmpTarget != null)
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PlantBomb, Hazel.SendOption.Reliable, -1);
                        writer.Write(BomberB.tmpTarget.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        BomberB.bombTarget = BomberB.tmpTarget;
                    }
                    _ = new StaticAchievementToken("bomber.common1");
                    BomberB.tmpTarget = null;
                    bomberBPlantBombButton.Timer = bomberBPlantBombButton.MaxTimer;
                    SoundEffectsManager.play("bomberPlantBomb");
                },
                buttonText: ModTranslation.getString("PlantBombText"),
                shakeOnEnd: false
            );

            bomberAReleaseBombButton = new CustomButton(
                // OnClick
                () =>
                {
                    var bomberB = BomberB.allPlayers.FirstOrDefault();
                    float distance = Vector2.Distance(PlayerControl.LocalPlayer.transform.localPosition, bomberB.transform.localPosition);

                    if (PlayerControl.LocalPlayer.CanMove && BomberA.bombTarget != null && BomberB.bombTarget != null && bomberB != null && !bomberB.Data.IsDead && distance < 1)
                    {
                        var target = BomberA.bombTarget;
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ReleaseBomb, Hazel.SendOption.Reliable, -1);
                        writer.Write(PlayerControl.LocalPlayer.PlayerId);
                        writer.Write(target.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.releaseBomb(PlayerControl.LocalPlayer.PlayerId, target.PlayerId);
                    }
                    _ = new StaticAchievementToken("bomber.challenge");
                },
                // HasButton
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.BomberA) && !PlayerControl.LocalPlayer.Data.IsDead && BomberB.isAlive(); },
                // CouldUse
                () =>
                {
                    var bomberB = BomberB.allPlayers.FirstOrDefault();
                    float distance = Vector2.Distance(PlayerControl.LocalPlayer.transform.localPosition, bomberB.transform.localPosition);

                    return PlayerControl.LocalPlayer.CanMove && BomberA.bombTarget != null && BomberB.bombTarget != null && bomberB != null && !bomberB.Data.IsDead && distance < 1;
                },
                // OnMeetingEnds
                () =>
                {
                    bomberAReleaseBombButton.Timer = bomberAReleaseBombButton.MaxTimer;
                },
                BomberA.getReleaseButtonSprite(),
                CustomButton.ButtonPositions.lowerRowCenter,
                __instance,
                KeyCode.Q,
                false,
                ModTranslation.getString("ReleaseBombText")
            );

            bomberBReleaseBombButton = new CustomButton(
                // OnClick
                () =>
                {
                    var bomberA = BomberA.allPlayers.FirstOrDefault();
                    float distance = Vector2.Distance(PlayerControl.LocalPlayer.transform.localPosition, bomberA.transform.localPosition);

                    if (PlayerControl.LocalPlayer.CanMove && BomberA.bombTarget != null && BomberB.bombTarget != null && bomberA != null && !bomberA.Data.IsDead && distance < 1)
                    {
                        var target = BomberB.bombTarget;
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ReleaseBomb, Hazel.SendOption.Reliable, -1);
                        writer.Write(PlayerControl.LocalPlayer.PlayerId);
                        writer.Write(target.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.releaseBomb(PlayerControl.LocalPlayer.PlayerId, target.PlayerId);
                    }
                    _ = new StaticAchievementToken("bomber.challenge");
                },
                // HasButton
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.BomberB) && !PlayerControl.LocalPlayer.Data.IsDead && BomberA.isAlive(); },
                // CouldUse
                () =>
                {
                    var bomberA = BomberA.allPlayers.FirstOrDefault();
                    float distance = Vector2.Distance(PlayerControl.LocalPlayer.transform.localPosition, bomberA.transform.localPosition);

                    return PlayerControl.LocalPlayer.CanMove && BomberA.bombTarget != null && BomberB.bombTarget != null && !bomberA.Data.IsDead && bomberA != null && distance < 1;
                },
                // OnMeetingEnds
                () =>
                {
                    bomberBReleaseBombButton.Timer = bomberBReleaseBombButton.MaxTimer;
                },
                BomberA.getReleaseButtonSprite(),
                CustomButton.ButtonPositions.lowerRowCenter,
                __instance,
                KeyCode.Q,
                false,
                ModTranslation.getString("ReleaseBombText")
            );

            undertakerDragButton = new CustomButton(
                () =>
                {
                    _ = new StaticAchievementToken("undertaker.common1");
                    var bodyComponent = Undertaker.TargetBody;
                    if (Undertaker.DraggedBody == null && bodyComponent != null)
                    {
                        Undertaker.DragBody.Invoke(bodyComponent.ParentId);
                    }
                    else if (Undertaker.DraggedBody != null)
                    {
                        var position = Undertaker.DraggedBody.transform.position;
                        Undertaker.DropBody.Invoke(position);
                    }
                }, // Action OnClick
                () =>
                {
                    return PlayerControl.LocalPlayer.isRole(RoleId.Undertaker) &&
                           !PlayerControl.LocalPlayer.Data.IsDead;
                }, // Bool HasButton
                () =>
                {
                    if (Undertaker.DraggedBody != null)
                    {
                        undertakerDragButton.Sprite = Undertaker.getDropButtonSprite();
                        undertakerDragButton.buttonText = ModTranslation.getString("DropBodyText");
                    }
                    else
                    {
                        undertakerDragButton.Sprite = Undertaker.getDragButtonSprite();
                        undertakerDragButton.buttonText = ModTranslation.getString("DragBodyText");
                    }
                    return ((Undertaker.TargetBody != null && Undertaker.DraggedBody == null)
                            || (Undertaker.DraggedBody != null && Undertaker.CanDropBody))
                           && PlayerControl.LocalPlayer.CanMove;
                }, // Bool CouldUse
                () => { }, // Action OnMeetingEnds
                Undertaker.getDragButtonSprite(), // Sprite sprite,
                CustomButton.ButtonPositions.upperRowLeft, // Vector3 PositionOffset
                __instance, // HudManager hudManager
                KeyCode.F,
                buttonText: ModTranslation.getString("DropBodyText")
            );

            buskerButton = new CustomButton(
                () =>
                {
                    if (buskerButton.isEffectActive)
                    {
                        Busker.local.buttonInterrupted = true;
                        buskerButton.Timer = 0f;
                        return;
                    }

                    Busker.Pseudocide.Invoke((PlayerControl.LocalPlayer.PlayerId, false, false));
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Busker) && (!PlayerControl.LocalPlayer.Data.IsDead || Busker.local.checkPseudocide()); },
                () =>
                {
                    if (buskerButton.isEffectActive)
                    {
                        buskerButton.buttonText = ModTranslation.getString("ReviveText");
                    }
                    else
                    {
                        buskerButton.buttonText = ModTranslation.getString("PseudocideText");
                    }
                    return PlayerControl.LocalPlayer.CanMove && (!Busker.local.pseudocideFlag || MapData.GetCurrentMapData().CheckMapArea(PlayerControl.LocalPlayer.transform.position));
                },
                () =>
                {
                    buskerButton.Timer = buskerButton.MaxTimer = Busker.cooldown;
                },
                Busker.getButtonSprite(),
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                KeyCode.F,
                true,
                Busker.duration,
                () =>
                {
                    if (Busker.local.buttonInterrupted)
                    {
                        Busker.local.buttonInterrupted = false;
                        _ = new StaticAchievementToken("busker.common1");
                        Busker.local.acTokenChallenge.Value.pseudocide = DateTime.UtcNow;
                        Busker.Revive.Invoke(PlayerControl.LocalPlayer.PlayerId);
                        buskerButton.Timer = buskerButton.MaxTimer = Busker.cooldown;
                    }
                    else
                    {
                        _ = new StaticAchievementToken("busker.another1");
                        Busker.local.dieBusker();
                    }
                },
                buttonText: ModTranslation.getString("PseudocideText"),
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            )
            {
                effectCancellable = true
            };

            noisemakerButton = new CustomButton(
                () =>
                {
                    Noisemaker.SetSounded.Invoke((Noisemaker.local.currentTarget.PlayerId, PlayerControl.LocalPlayer.PlayerId));
                    Noisemaker.local.numSound--;
                    noisemakerButton.Timer = noisemakerButton.MaxTimer;
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Noisemaker) && Noisemaker.local.numSound > 0 && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (noisemakerButtonText != null)
                        noisemakerButtonText.text = Noisemaker.local.numSound > 0 ? $"{Noisemaker.local.numSound}" : "";
                    return PlayerControl.LocalPlayer.CanMove && Noisemaker.local.currentTarget != null && Noisemaker.local.target == null;
                },
                () =>
                {
                    noisemakerButton.Timer = noisemakerButton.MaxTimer = Noisemaker.cooldown;
                    foreach (var noisemaker in Noisemaker.players) noisemaker.target = null;
                },
                Noisemaker.getButtonSprite(),
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                KeyCode.F,
                buttonText: ModTranslation.getString("NoisemakerText"),
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            );
            noisemakerButtonText = noisemakerButton.ShowUsesIcon(3);

            schrodingersCatKillButton = new CustomButton(
                () =>
                {
                    if (Helpers.checkMurderAttemptAndKill(PlayerControl.LocalPlayer, SchrodingersCat.currentTarget) == MurderAttemptResult.SuppressKill) return;

                    schrodingersCatKillButton.Timer = schrodingersCatKillButton.MaxTimer;
                    SchrodingersCat.currentTarget = null;
                },
                () => { return SchrodingersCat.isJackalButtonEnable() || SchrodingersCat.isJekyllAndHydeButtonEnable() || SchrodingersCat.isMoriartyButtonEnable() || SchrodingersCat.isYandereButtonEnable(); },
                () => { return SchrodingersCat.currentTarget && PlayerControl.LocalPlayer.CanMove; },
                () => { schrodingersCatKillButton.Timer = schrodingersCatKillButton.MaxTimer; },
                __instance.KillButton.graphic.sprite,
                CustomButton.ButtonPositions.upperRowRight,
                __instance,
                KeyCode.Q
            );

            schrodingersCatSwitchButton = new CustomButton(
                () =>
                {
                    SchrodingersCat.showMenu();
                },
                () => { return !SchrodingersCat.hasTeam() && SchrodingersCat.canChooseImpostor && PlayerControl.LocalPlayer.isRole(RoleId.SchrodingersCat) && SchrodingersCat.tasksComplete(PlayerControl.LocalPlayer) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () => { schrodingersCatSwitchButton.Timer = 0; },
                Morphling.getMorphSprite(),
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                KeyCode.F,
                buttonText: ModTranslation.getString("SwitchTeamText"),
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            );

            // Sherlock Investigate
            sherlockInvestigateButton = new CustomButton(
                () =>
                {
                    if (Sherlock.killPopup == null) Sherlock.killPopup = GameManagerCreator.Instance.HideAndSeekManagerPrefab.DeathPopupPrefab;
                    int count = 0;
                    string msg = "";
                    foreach (var item in Sherlock.killLog)
                    {
                        float distance = Vector3.Distance(item.Item2.Item2, PlayerControl.LocalPlayer.transform.position);
                        if (distance < Sherlock.investigateDistance)
                        {
                            var newPopUp = UnityEngine.Object.Instantiate(Sherlock.killPopup, __instance.transform.parent);
                            PlayerControl killer = Helpers.getPlayerById(item.Item1);
                            PlayerControl target = Helpers.getPlayerById(item.Item2.Item1);
                            string killerTeam = RoleInfo.GetRolesString(killer, useColors: true, showModifier: false, includeHidden: true);
                            newPopUp.gameObject.transform.GetChild(0).GetComponent<TextTranslatorTMP>().enabled = false;
                            newPopUp.gameObject.transform.GetChild(0).GetComponent<TMPro.TextMeshPro>().text = string.Format(ModTranslation.getString("sherlockMessage2"), killerTeam);
                            newPopUp.gameObject.transform.position += count % 4 * new Vector3(0, -1.2f, 0) + new Vector3(3f - (int)(count / 4) * 3.8f, -0.25f, __instance.MapButton.transform.localPosition.z - 600f);
                            newPopUp.ModShow(target, 0);
                            count++;
                            msg += $"{string.Format(ModTranslation.getString("sherlockMeeting"), target?.Data?.PlayerName, killerTeam)}\n";
                        }
                    }
                    if (!string.IsNullOrEmpty(msg))
                    {
                        msg = msg.TrimEnd('\n');
                        MeetingOverlayHolder.RegisterOverlay(TORGUIContextEngine.API.VerticalHolder(GUIAlignment.Left,
                        new TORGUIText(GUIAlignment.Left, TORGUIContextEngine.API.GetAttribute(AttributeAsset.OverlayTitle), new TranslateTextComponent("sherlockInfo")),
                        new TORGUIText(GUIAlignment.Left, TORGUIContextEngine.API.GetAttribute(AttributeAsset.OverlayContent), new RawTextComponent(msg)))
                        , MeetingOverlayHolder.IconsSprite[2], Sherlock.color);
                    }
                    if (count == 0)
                    {
                        RoleHelpers.RoleMessage(RoleId.Sherlock, "sherlockMessage1");
                    }
                    if (count >= 3)
                        _ = new StaticAchievementToken("sherlock.common1");
                    Sherlock.local.numUsed += 1;
                    sherlockInvestigateButton.Timer = sherlockInvestigateButton.MaxTimer;
                    SoundEffectsManager.play("sherlockInvestigate");
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Sherlock) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (sherlockNumInvestigateText != null)
                    {
                        sherlockNumInvestigateText.text = $"{Sherlock.local.numUsed} / {Sherlock.local.getNumInvestigate()}";
                    }

                    return PlayerControl.LocalPlayer.CanMove && Sherlock.local.numUsed < Sherlock.local.getNumInvestigate();
                },
                () =>
                {
                    sherlockInvestigateButton.Timer = sherlockInvestigateButton.MaxTimer;
                    Sherlock.killLog.Clear();
                },
                Sherlock.getInvestigateIcon(),
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                KeyCode.F,
                buttonText: ModTranslation.getString("InvestigateText"),
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            );
            sherlockNumInvestigateText = GameObject.Instantiate(sherlockInvestigateButton.actionButton.cooldownTimerText, sherlockInvestigateButton.actionButton.cooldownTimerText.transform.parent);
            sherlockNumInvestigateText.text = "";
            sherlockNumInvestigateText.enableWordWrapping = false;
            sherlockNumInvestigateText.transform.localScale = Vector3.one * 0.5f;
            sherlockNumInvestigateText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            // Sherlock Watch
            sherlockWatchButton = new CustomButton(
                () => { },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Sherlock) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (sherlockNumKillTimerText != null)
                    {
                        sherlockNumKillTimerText.text = $"{Sherlock.killTimerCounter}";
                    }
                    if (sherlockWatchButton.Timer <= 0)
                    {
                        Sherlock.killTimerCounter += 1;
                        sherlockWatchButton.Timer = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown;
                        Sherlock.local.acTokenChallenge.Value |= Sherlock.killTimerCounter >= 4 && !MeetingHud.Instance;
                    }

                    return PlayerControl.LocalPlayer.CanMove && Sherlock.local.numUsed < Sherlock.local.getNumInvestigate();
                },
                () =>
                {
                    sherlockWatchButton.Timer = sherlockWatchButton.MaxTimer;
                    Sherlock.killTimerCounter = 0;
                },
                Sherlock.getWatchIcon(),
                CustomButton.ButtonPositions.upperRowRight,
                __instance,
                KeyCode.H,
                actionName: ModTranslation.getString("sherlockWatchButton")
            )
            { buttonText = "" };
            sherlockNumKillTimerText = GameObject.Instantiate(sherlockWatchButton.actionButton.cooldownTimerText, sherlockWatchButton.actionButton.cooldownTimerText.transform.parent);
            sherlockNumKillTimerText.text = "";
            sherlockNumKillTimerText.enableWordWrapping = false;
            sherlockNumKillTimerText.transform.localScale = Vector3.one * 0.5f;
            sherlockNumKillTimerText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            cupidArrowButton = new CustomButton(
                () =>
                {
                    if (Helpers.checkSuspendAction(PlayerControl.LocalPlayer, Cupid.local.currentTarget)) return;

                    var text  = Helpers.CreateAndShowNotification(string.Format(ModTranslation.getString("cupidLoversNotification"), Cupid.local.currentTarget?.Data?.PlayerName ?? ""), Color.white, new Vector3(0f, 1f, -20f), null, RoleHelpers.GetRoleIcon(RoleId.Cupid).GetSprite());
                    text.AdjustNotification();

                    if (Cupid.local.lovers1 == null)
                    {
                        Cupid.local.lovers1 = Cupid.local.currentTarget;
                    }
                    else
                    {
                        if (Cupid.local.currentTarget != Cupid.local.lovers1)
                        {
                            Cupid.local.lovers2 = Cupid.local.currentTarget;
                        }
                    }
                    if (Cupid.local.lovers1 != null && Cupid.local.lovers2 != null)
                    {
                        _ = new StaticAchievementToken("cupid.common1");
                        Cupid.createLovers();
                    }
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Cupid) && !PlayerControl.LocalPlayer.Data.IsDead && Cupid.local.lovers2 == null && Cupid.local.timeLeft > 0; },
                () =>
                {
                    if (cupidLoversText != null) cupidLoversText.text = Cupid.local.lovers1 == null ? "2" : "1";
                    return PlayerControl.LocalPlayer.CanMove && Cupid.local.currentTarget != null;
                },
                () => { cupidArrowButton.Timer = cupidArrowButton.MaxTimer; },
                Cupid.getArrowSprite(),
                CustomButton.ButtonPositions.upperRowRight,
                __instance,
                KeyCode.F,
                buttonText: ModTranslation.getString("CupidArrowText"),
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            );
            cupidTimeRemainingText = GameObject.Instantiate(cupidArrowButton.actionButton.cooldownTimerText, __instance.transform);
            cupidTimeRemainingText.text = "";
            cupidTimeRemainingText.enableWordWrapping = false;
            cupidTimeRemainingText.transform.localScale = Vector3.one * 0.45f;
            cupidTimeRemainingText.transform.localPosition = cupidArrowButton.actionButton.cooldownTimerText.transform.parent.localPosition + new Vector3(-0.1f, 0.35f, 0f);
            ButtonEffect.SetMouseActionIcon(cupidArrowButton.actionButtonGameObject, true, ModTranslation.getString("buttonsCupidSuicide"), true, ButtonEffect.ActionIconType.InfoAction);
            cupidLoversText = cupidArrowButton.ShowUsesIcon(4);

            cupidShieldButton = new CustomButton(
                () =>
                {
                    if (Helpers.checkSuspendAction(PlayerControl.LocalPlayer, Cupid.local.currentTarget)) return;
                    Cupid.SetShielded.Invoke((Cupid.local.shieldTarget.PlayerId, PlayerControl.LocalPlayer.PlayerId));

                    var text = Helpers.CreateAndShowNotification(string.Format(ModTranslation.getString("cupidSetShieldedNotification"), Cupid.local.currentTarget?.Data?.PlayerName ?? ""), Color.white, new Vector3(0f, 1f, -20f), null, RoleHelpers.GetRoleIcon(RoleId.Cupid).GetSprite());
                    text.AdjustNotification();
                },
                () => { return Cupid.isShieldOn && PlayerControl.LocalPlayer.isRole(RoleId.Cupid) && !PlayerControl.LocalPlayer.Data.IsDead && Cupid.local.shielded == null; },
                () => { return PlayerControl.LocalPlayer.CanMove && Cupid.local.shieldTarget != null; },

                () => { cupidShieldButton.Timer = cupidShieldButton.MaxTimer; },
                Medic.getButtonSprite(),
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                KeyCode.G,
                buttonText: ModTranslation.getString("ShieldText"),
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            );

            // Akujo Honmei
            akujoHonmeiButton = new CustomButton(
                () =>
                {
                    if (Helpers.checkSuspendAction(PlayerControl.LocalPlayer, Akujo.local.currentTarget)) return;
                    _ = new StaticAchievementToken("akujo.common2");
                    Akujo.SetHonmei.Invoke((PlayerControl.LocalPlayer.PlayerId, Akujo.local.currentTarget.PlayerId));

                    var text = Helpers.CreateAndShowNotification(string.Format(ModTranslation.getString("akujoSetHonmeiNotification"), Akujo.local.currentTarget?.Data?.PlayerName ?? ""), Color.white, new Vector3(0f, 1f, -20f), null, RoleHelpers.GetRoleIcon(RoleId.Akujo).GetSprite());
                    text.AdjustNotification();
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Akujo) && !PlayerControl.LocalPlayer.Data.IsDead && Akujo.local.honmei == null && Akujo.local.cupidHonmei == null && Akujo.local.timeLeft > 0; },
                () =>
                {                    
                    return PlayerControl.LocalPlayer.CanMove && Akujo.local.currentTarget != null;
                },
                () => { akujoHonmeiButton.Timer = akujoHonmeiButton.MaxTimer; },
                Akujo.getHonmeiSprite(),
                CustomButton.ButtonPositions.upperRowRight,
                __instance,
                KeyCode.F,
                buttonText: ModTranslation.getString("AkujoHonmeiText"),
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            );
            akujoHonmeiText = akujoHonmeiButton.ShowUsesIcon(4);
            akujoHonmeiText.text = "1";
            akujoTimeRemainingText = GameObject.Instantiate(akujoHonmeiButton.actionButton.cooldownTimerText, __instance.transform);
            akujoTimeRemainingText.text = "";
            akujoTimeRemainingText.enableWordWrapping = false;
            akujoTimeRemainingText.transform.localScale = Vector3.one * 0.45f;
            akujoTimeRemainingText.transform.localPosition = akujoHonmeiButton.actionButton.cooldownTimerText.transform.parent.localPosition + new Vector3(-0.1f, 0.35f, 0f);
            ButtonEffect.SetMouseActionIcon(akujoHonmeiButton.actionButtonGameObject, true, ModTranslation.getString("buttonsAkujoSuicide"), true, ButtonEffect.ActionIconType.InfoAction);

            // Akujo Keep
            akujoBackupButton = new CustomButton(
                () =>
                {
                    if (Helpers.checkSuspendAction(PlayerControl.LocalPlayer, Akujo.local.currentTarget)) return;
                    _ = new StaticAchievementToken("akujo.common1");
                    Akujo.SetKeep.Invoke((PlayerControl.LocalPlayer.PlayerId, Akujo.local.currentTarget.PlayerId));

                    var text = Helpers.CreateAndShowNotification(string.Format(ModTranslation.getString("akujoSetKeepNotification"), Akujo.local.currentTarget?.Data?.PlayerName ?? ""), Color.white, new Vector3(0f, 1f, -20f), null, RoleHelpers.GetRoleIcon(RoleId.Akujo).GetSprite());
                    text.AdjustNotification();
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Akujo) && !PlayerControl.LocalPlayer.Data.IsDead && Akujo.local.keepsLeft > 0 && Akujo.local.timeLeft > 0; },
                () =>
                {
                    if (akujoBackupLeftText != null)
                    {
                        if (Akujo.local.keepsLeft > 0)
                            akujoBackupLeftText.text = Akujo.local.keepsLeft.ToString();
                        else
                            akujoBackupLeftText.text = "";
                    }
                    return PlayerControl.LocalPlayer.CanMove && Akujo.local.currentTarget != null;
                },
                () => { akujoBackupButton.Timer = akujoBackupButton.MaxTimer; },
                Akujo.getKeepSprite(),
                CustomButton.ButtonPositions.upperRowCenter,
                __instance,
                KeyCode.K,
                buttonText: ModTranslation.getString("AkujoBackupText"),
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            );
            akujoBackupLeftText = akujoBackupButton.ShowUsesIcon(4);

            // Mimic(Assistant) Morph
            mimicAMorphButton = new CustomButton(
                () =>
                {
                    if (!MimicA.isMorph)
                    {
                        var partnerId = MimicK.allPlayers.FirstOrDefault().PlayerId;
                        MimicA.Morph.Invoke((PlayerControl.LocalPlayer.PlayerId, partnerId));
                    }
                    else
                    {
                        MimicA.ResetMorph.Invoke(PlayerControl.LocalPlayer.PlayerId);
                    }
                    MimicA.acTokenCommon.Value++;
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.MimicA) && !PlayerControl.LocalPlayer.Data.IsDead && MimicK.isAlive(); },
                () => { return PlayerControl.LocalPlayer.CanMove && !Helpers.MushroomSabotageActive(); },
                () => { },
                Morphling.getMorphSprite(),
                CustomButton.ButtonPositions.upperRowLeft,
                __instance,
                KeyCode.F,
                false
            );

            // Mimic(Assistant) Admin
            mimicAAdminButton = new CustomButton(
                () =>
                {
                    if (!MapBehaviour.Instance || !MapBehaviour.Instance.isActiveAndEnabled)
                    {
                        HudManager __instance = FastDestroyableSingleton<HudManager>.Instance;
                        __instance.InitMap();
                        MapBehaviour.Instance.ShowCountOverlay(allowedToMove: true, showLivePlayerPosition: true, includeDeadBodies: true);
                    }
                    _ = new StaticAchievementToken("mimicA.common1");
                },
                () => {
                    return PlayerControl.LocalPlayer.isRole(RoleId.MimicA) && !PlayerControl.LocalPlayer.Data.IsDead
                    && MimicK.isAlive();
                },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () => { },
                Hacker.getAdminSprite(),
                CustomButton.ButtonPositions.upperRowCenter,
                __instance,
                KeyCode.H,
                false,
                FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Admin),
                abilityTexture: CustomButton.ButtonLabelType.AdminButton
            );

            // Ninja Stealth
            ninjaButton = new CustomButton(
                () =>
                {
                    if (ninjaButton.isEffectActive)
                    {
                        ninjaButton.Timer = 0;
                        return;
                    }

                    _ = new StaticAchievementToken("ninja.common1");
                    Ninja.Stealth.Invoke((PlayerControl.LocalPlayer.PlayerId, true));
                    SoundEffectsManager.play("ninjaStealth");
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Ninja) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (ninjaButton.isEffectActive)
                    {
                        ninjaButton.buttonText = ModTranslation.getString("NinjaUnstealthText");
                    }
                    else
                    {
                        ninjaButton.buttonText = ModTranslation.getString("NinjaText");
                    }
                    return PlayerControl.LocalPlayer.CanMove;
                },
                () =>
                {
                    ninjaButton.Timer = ninjaButton.MaxTimer = Ninja.stealthCooldown;
                    ninjaButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                    ninjaButton.isEffectActive = false;
                },
                Ninja.getButtonSprite(),
                CustomButton.ButtonPositions.upperRowLeft,
                __instance,
                KeyCode.F,
                true,
                Ninja.stealthDuration,
                () =>
                {
                    ninjaButton.Timer = ninjaButton.MaxTimer = Ninja.stealthCooldown;
                    Ninja.Stealth.Invoke((PlayerControl.LocalPlayer.PlayerId, false));

                    PlayerControl.LocalPlayer.SetKillTimer(Math.Max(PlayerControl.LocalPlayer.killTimer, Ninja.killPenalty));
                },
                buttonText: ModTranslation.getString("NinjaText")
            )
            {
                effectCancellable = true
            };

            // Kataomoi button
            kataomoiButton = new CustomButton(
                () => {
                    if (Kataomoi.canLove())
                    {
                        var murderAttemptResult = Helpers.checkMuderAttempt(PlayerControl.LocalPlayer, Kataomoi.currentTarget);
                        if (murderAttemptResult == MurderAttemptResult.SuppressKill) return;

                        Kataomoi.TriggerWin.Invoke();

                        if (murderAttemptResult == MurderAttemptResult.PerformKill)
                        {
                            _ = new StaticAchievementToken("kataomoi.challenge");
                            byte targetId = Kataomoi.currentTarget.PlayerId;
                            MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedMurderPlayer, Hazel.SendOption.Reliable, -1);
                            killWriter.Write(PlayerControl.LocalPlayer.PlayerId);
                            killWriter.Write(targetId);
                            killWriter.Write(byte.MaxValue);
                            AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                            RPCProcedure.uncheckedMurderPlayer(PlayerControl.LocalPlayer.PlayerId, targetId, Byte.MaxValue);
                        }

                        kataomoiButton.HasEffect = false;
                    }
                    else if (Kataomoi.currentTarget != null)
                    {
                        kataomoiButton.HasEffect = true;
                    }
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Kataomoi) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => {
                    if (Kataomoi.canLove()) {
                        kataomoiButton.actionButton.graphic.sprite = Kataomoi.getLoveSprite();
                        kataomoiButton.buttonText = ModTranslation.getString("KataomoiLoveText");
                    }

                    if (kataomoiButton.isEffectActive && Kataomoi.target != null && Kataomoi.target != Kataomoi.currentTarget)
                    {
                        kataomoiButton.Timer = 0f;
                        kataomoiButton.isEffectActive = false;
                    }

                    return PlayerControl.LocalPlayer.CanMove && Kataomoi.currentTarget != null;
                },
                () => {
                    kataomoiButton.Timer = kataomoiButton.MaxTimer;
                    kataomoiButton.isEffectActive = false;
                    kataomoiButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Kataomoi.getStareSprite(),
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                KeyCode.H,
                true,
                Kataomoi.stareDuration,
                () => {
                    _ = new StaticAchievementToken("kataomoi.common1");
                    Kataomoi.doStare();
                    kataomoiButton.Timer = Kataomoi.canLove() ? 0 : kataomoiButton.MaxTimer;
                },
                buttonText: ModTranslation.getString("KataomoiStareText"),
                abilityTexture: CustomButton.ButtonLabelType.UseButton,
                shakeOnEnd: false
            );

            // Kataomoi search button
            kataomoiSearchButton = new CustomButton(
                () => {
                    if (!Kataomoi.exists) return;
                    Kataomoi.doSearch();
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Kataomoi) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => {
                    return PlayerControl.LocalPlayer.CanMove && Kataomoi.target != null;
                },
                () => {
                    kataomoiSearchButton.Timer = kataomoiSearchButton.MaxTimer;
                    kataomoiSearchButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                    kataomoiSearchButton.isEffectActive = false;
                    Kataomoi.resetSearch();
                },
                Kataomoi.getSearchSprite(),
                CustomButton.ButtonPositions.upperRowRight,
                __instance,
                KeyCode.G,
                true,
                Kataomoi.searchDuration,
                () => {
                    kataomoiSearchButton.Timer = kataomoiSearchButton.MaxTimer;
                    Kataomoi.resetSearch();
                },
                buttonText: ModTranslation.getString("KataomoiSearchText")
            );

            // Kataomoi stalking button
            kataomoiStalkingButton = new CustomButton(
                () => {
                    if (!Kataomoi.exists) return;

                    byte playerId = PlayerControl.LocalPlayer.PlayerId;
                    Kataomoi.SetStalking.Invoke(playerId);
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Kataomoi) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => {
                    return PlayerControl.LocalPlayer.CanMove;
                },
                () => {
                    kataomoiStalkingButton.Timer = kataomoiStalkingButton.MaxTimer;
                    kataomoiStalkingButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                    kataomoiStalkingButton.isEffectActive = false;
                },
                Kataomoi.getStalkingSprite(),
                CustomButton.ButtonPositions.upperRowCenter,
                __instance,
                KeyCode.F,
                true,
                Kataomoi.stalkingDuration,
                () => {
                    kataomoiStalkingButton.Timer = kataomoiStalkingButton.MaxTimer;
                },
                buttonText: ModTranslation.getString("KataomoiStalkText"),
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            );

            foxStealthButton = new CustomButton(
                () =>
                {
                    if (foxStealthButton.isEffectActive)
                    {
                        foxStealthButton.Timer = 0;
                        return;
                    }

                    Fox.SetStealth.Invoke(true);
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Fox) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (foxStealthButton.isEffectActive)
                    {
                        foxStealthButton.buttonText = ModTranslation.getString("FoxUnstealthText");
                    }
                    else
                    {
                        foxStealthButton.buttonText = ModTranslation.getString("FoxStealthText");
                    }
                    return PlayerControl.LocalPlayer.CanMove;
                },
                () =>
                {
                    foxStealthButton.Timer = foxStealthButton.MaxTimer = Fox.stealthCooldown;
                    foxStealthButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                    foxStealthButton.isEffectActive = false;
                },
                Fox.getHideButtonSprite(),
                CustomButton.ButtonPositions.upperRowCenter,
                __instance,
                KeyCode.F,
                true,
                Fox.stealthDuration,
                () =>
                {
                    foxStealthButton.Timer = foxStealthButton.MaxTimer = Fox.stealthCooldown;
                    Fox.SetStealth.Invoke(false);
                },
                buttonText: ModTranslation.getString("FoxStealthText"),
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            )
            {
                effectCancellable = true
            };

            foxRepairButton = new CustomButton(
                () =>
                {
                    bool sabotageActive = false;
                    foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
                        if (task.TaskType is TaskTypes.FixLights or TaskTypes.RestoreOxy or TaskTypes.ResetReactor or TaskTypes.ResetSeismic or TaskTypes.FixComms or TaskTypes.StopCharles)
                            sabotageActive = true;
                    if (!sabotageActive) return;

                    foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
                    {
                        if (task.TaskType == TaskTypes.FixLights)
                        {
                            Engineer.FixLights.Invoke();
                        }
                        else if (task.TaskType == TaskTypes.RestoreOxy)
                        {
                            MapUtilities.CachedShipStatus.RpcUpdateSystem(SystemTypes.LifeSupp, 0 | 64);
                            MapUtilities.CachedShipStatus.RpcUpdateSystem(SystemTypes.LifeSupp, 1 | 64);
                        }
                        else if (task.TaskType == TaskTypes.ResetReactor)
                        {
                            MapUtilities.CachedShipStatus.RpcUpdateSystem(SystemTypes.Reactor, 16);
                        }
                        else if (task.TaskType == TaskTypes.ResetSeismic)
                        {
                            MapUtilities.CachedShipStatus.RpcUpdateSystem(SystemTypes.Laboratory, 16);
                        }
                        else if (task.TaskType == TaskTypes.FixComms)
                        {
                            MapUtilities.CachedShipStatus.RpcUpdateSystem(SystemTypes.Comms, 16 | 0);
                            MapUtilities.CachedShipStatus.RpcUpdateSystem(SystemTypes.Comms, 16 | 1);
                        }
                        else if (task.TaskType == TaskTypes.StopCharles)
                        {
                            MapUtilities.CachedShipStatus.RpcUpdateSystem(SystemTypes.HeliSabotage, 0 | 16);
                            MapUtilities.CachedShipStatus.RpcUpdateSystem(SystemTypes.HeliSabotage, 1 | 16);
                        }
                    }
                    Fox.numRepair -= 1;
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Fox) && !PlayerControl.LocalPlayer.Data.IsDead && Fox.numRepair > 0; },
                () =>
                {
                    if (foxRepairText != null) foxRepairText.text = Fox.numRepair.ToString();
                    bool sabotageActive = false;
                    foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
                        if (task.TaskType is TaskTypes.FixLights or TaskTypes.RestoreOxy or TaskTypes.ResetReactor or TaskTypes.ResetSeismic or TaskTypes.FixComms or TaskTypes.StopCharles)
                            sabotageActive = true;
                    return sabotageActive && Fox.numRepair > 0 && PlayerControl.LocalPlayer.CanMove;
                },
                () => { foxRepairButton.Timer = foxRepairButton.MaxTimer = 0f; },
                Fox.getRepairButtonSprite(),
                CustomButton.ButtonPositions.upperRowRight,
                __instance,
                KeyCode.G,
                abilityTexture: CustomButton.ButtonLabelType.UseButton,
                buttonText: ModTranslation.getString("RepairText")
            );
            foxRepairText = foxRepairButton.ShowUsesIcon(4);

            foxImmoralistButton = new CustomButton(
                () =>
                {
                    if (Helpers.checkSuspendAction(PlayerControl.LocalPlayer, Fox.currentTarget)) return;
                    _ = new StaticAchievementToken("fox.common1");
                    Fox.CreatesImmoralist.Invoke(Fox.currentTarget.PlayerId);
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Fox) && Fox.canCreateImmoralist && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return Fox.currentTarget != null && PlayerControl.LocalPlayer.CanMove; },
                () => { foxImmoralistButton.Timer = foxImmoralistButton.MaxTimer = 20f; },
                Fox.getImmoralistButtonSprite(),
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                KeyCode.I,
                abilityTexture: CustomButton.ButtonLabelType.UseButton,
                buttonText: ModTranslation.getString("FoxImmoralistText")
            );

            immoralistButton = new CustomButton(
                () =>
                {
                    _ = new StaticAchievementToken("immoralist.challenge");
                    byte targetId = PlayerControl.LocalPlayer.PlayerId;
                    MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SerialKillerSuicide, Hazel.SendOption.Reliable, -1); killWriter.Write(targetId);
                    AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                    RPCProcedure.serialKillerSuicide(targetId);
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Immoralist) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return true; },
                () =>
                {
                    immoralistButton.Timer = immoralistButton.MaxTimer = 20f;
                },
                Immoralist.getButtonSprite(),
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                KeyCode.F,
                buttonText: ModTranslation.getString("ImmoralistSuicideText")
            );

            // Yoyo button
            yoyoButton = new CustomButton(
                () => {
                    var pos = PlayerControl.LocalPlayer.transform.position;

                    if (Yoyo.local.markedLocation == null)
                    {
                        TheOtherRolesPlugin.Logger.LogMessage($"marked location is null in button press");
                        Yoyo.MarkLocation.Invoke((pos, PlayerControl.LocalPlayer.PlayerId));
                        SoundEffectsManager.play("tricksterPlaceBox");
                        yoyoButton.Sprite = Yoyo.getBlinkButtonSprite();
                        yoyoButton.Timer = 10f;
                        yoyoButton.HasEffect = false;
                        yoyoButton.buttonText = ModTranslation.getString("BlinkText");
                    }
                    else
                    {
                        TheOtherRolesPlugin.Logger.LogMessage("in else for some reason");
                        // Jump to location
                        TheOtherRolesPlugin.Logger.LogMessage($"trying to blink!");

                        Yoyo.local.ActivateBlackout((Vector3)Yoyo.local.markedLocation);
                        Yoyo.Blink.Invoke((true, pos, PlayerControl.LocalPlayer.PlayerId));
                        yoyoButton.EffectDuration = Yoyo.blinkDuration;
                        yoyoButton.Timer = 10f;
                        yoyoButton.HasEffect = true;
                        yoyoButton.buttonText = ModTranslation.getString("ReturnText");
                        SoundEffectsManager.play("morphlingMorph");
                    }
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Yoyo) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () => {
                    if (Yoyo.markStaysOverMeeting) {
                        yoyoButton.Timer = 10f;
                    }
                    else
                    {
                        yoyoButton.Timer = yoyoButton.MaxTimer;
                        yoyoButton.Sprite = Yoyo.getMarkButtonSprite();
                        yoyoButton.buttonText = ModTranslation.getString("MarkText");
                    }
                },
                Yoyo.getMarkButtonSprite(),
                CustomButton.ButtonPositions.upperRowLeft,
                __instance,
                KeyCode.F,
                false,
                Yoyo.blinkDuration,
                () => {
                    if (TransportationToolPatches.isUsingTransportation(PlayerControl.LocalPlayer))
                    {
                        yoyoButton.Timer = 0.5f;
                        yoyoButton.DeputyTimer = 0.5f;
                        yoyoButton.isEffectActive = true;
                        yoyoButton.actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                        return;
                    }
                    else if (PlayerControl.LocalPlayer.inVent) {
                        __instance.ImpostorVentButton.DoClick();
                    }

                    // jump back!
                    var pos = PlayerControl.LocalPlayer.transform.position;
                    var exit = (Vector3)Yoyo.local.markedLocation;
                    if (SubmergedCompatibility.IsSubmerged) {
                        SubmergedCompatibility.ChangeFloor(exit.y > -7);
                    }
                    // Activate blackout here, else the exit will be null
                    Yoyo.local.ActivateBlackout(exit);
                    Yoyo.Blink.Invoke((false, pos, PlayerControl.LocalPlayer.PlayerId));

                    yoyoButton.Timer = yoyoButton.MaxTimer;
                    yoyoButton.isEffectActive = false;
                    yoyoButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                    yoyoButton.HasEffect = false;
                    yoyoButton.Sprite = Yoyo.getMarkButtonSprite();
                    yoyoButton.buttonText = ModTranslation.getString("MarkText");
                    SoundEffectsManager.play("morphlingMorph");
                    if (Minigame.Instance)
                    {
                        Minigame.Instance.Close();
                    }
                    _ = new StaticAchievementToken("yoyo.challenge");
                },
                buttonText: ModTranslation.getString("MarkText"),
                abilityTexture: CustomButton.ButtonLabelType.UseButton,
                actionName: ModTranslation.getString("yoyoButton")
            );

            // Serial Killer Suicide Countdown
            serialKillerButton = new CustomButton(
                () => { },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.SerialKiller) && !PlayerControl.LocalPlayer.Data.IsDead && SerialKiller.local.isCountDown; },
                () => { return true; },
                () =>
                {
                    if (SerialKiller.resetTimer) {
                        serialKillerButton.Timer = SerialKiller.suicideTimer;
                    }
                },
                SerialKiller.getButtonSprite(),
                CustomButton.ButtonPositions.upperRowLeft,
                __instance,
                KeyCode.None,
                true,
                SerialKiller.suicideTimer,
                () =>
                {
                    _ = new StaticAchievementToken("serialKiller.another1");
                    byte targetId = PlayerControl.LocalPlayer.PlayerId;
                    MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SerialKillerSuicide, Hazel.SendOption.Reliable, -1); killWriter.Write(targetId);
                    killWriter.Write(targetId);
                    AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                    RPCProcedure.serialKillerSuicide(targetId);
                },
                buttonText: ModTranslation.getString("serialKillerSuicideText"),
                isSuicide: true,
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            )
            {
                isEffectActive = true
            };
            ButtonEffect.SetMouseActionIcon(serialKillerButton.actionButtonGameObject, true, ModTranslation.getString("buttonsNormalSuicide"), false, ButtonEffect.ActionIconType.InfoAction);

            // Evil Tracker track
            evilTrackerButton = new CustomButton(
                () => {
                    if (Helpers.checkSuspendAction(PlayerControl.LocalPlayer, EvilTracker.local.currentTarget)) return;
                    _ = new StaticAchievementToken("evilTracker.common1");
                    EvilTracker.local.target = EvilTracker.local.currentTarget;
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.EvilTracker) && EvilTracker.local.target == null &&  !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return EvilTracker.local.currentTarget != null && EvilTracker.local.target == null && PlayerControl.LocalPlayer.CanMove; },
                () =>
                {
                    evilTrackerButton.Timer = evilTrackerButton.MaxTimer;
                },
                EvilTracker.getEvilTrackerButtonSprite(),
                CustomButton.ButtonPositions.upperRowLeft,
                __instance,
                KeyCode.F,
                buttonText: ModTranslation.getString("TrackerText")
            );

            // Vulture Eat
            vultureEatButton = new CustomButton(
                () => {
                    foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(PlayerControl.LocalPlayer.GetTruePosition(), PlayerControl.LocalPlayer.MaxReportDistance, Constants.PlayersOnlyMask)) {
                        if (collider2D.tag == "DeadBody") {
                            DeadBody component = collider2D.GetComponent<DeadBody>();
                            if (component && !component.Reported) {
                                Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
                                Vector2 truePosition2 = component.TruePosition;
                                if (Vector2.Distance(truePosition2, truePosition) <= PlayerControl.LocalPlayer.MaxReportDistance && PlayerControl.LocalPlayer.CanMove && !PhysicsHelpers.AnythingBetween(truePosition, truePosition2, Constants.ShipAndObjectsMask, false)) {
                                    NetworkedPlayerInfo playerInfo = GameData.Instance.GetPlayerById(component.ParentId);
                                    _ = new StaticAchievementToken("vulture.common1");
                                    _ = new StaticAchievementToken("vulture.common2");
                                    RPCProcedure.CleanBody.Invoke((playerInfo.PlayerId, PlayerControl.LocalPlayer.PlayerId));

                                    Vulture.cooldown = vultureEatButton.Timer = vultureEatButton.MaxTimer;
                                    SoundEffectsManager.play("vultureEat");
                                    break;
                                }
                            }
                        }
                    }
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Vulture) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (vultureRemainingText != null) vultureRemainingText.text = (Vulture.vultureNumberToWin - Vulture.local.eatenBodies).ToString();
                    return __instance.ReportButton.graphic.color == Palette.EnabledColor && PlayerControl.LocalPlayer.CanMove;
                },
                () => { vultureEatButton.Timer = vultureEatButton.MaxTimer; },
                Vulture.getButtonSprite(),
                CustomButton.ButtonPositions.lowerRowCenter,
                __instance,
                KeyCode.F,
                buttonText: ModTranslation.getString("VultureText")
            );
            vultureRemainingText = vultureEatButton.ShowUsesIcon(2);

            pelicanKillButton = new CustomButton(
            () =>
            {
                if (Helpers.checkMuderAttempt(PlayerControl.LocalPlayer, Pelican.local.currentTarget) != MurderAttemptResult.PerformKill) return;
                Pelican.RpcSwallow.Invoke((PlayerControl.LocalPlayer.PlayerId, Pelican.local.currentTarget.PlayerId));
                Pelican.local.currentTarget = null;
                pelicanKillButton.Timer = pelicanKillButton.MaxTimer;
                _ = new StaticAchievementToken("pelican.common1");
                _ = new StaticAchievementToken("pelican.common2");
            },
            () => { return PlayerControl.LocalPlayer.isRole(RoleId.Pelican) && !PlayerControl.LocalPlayer.Data.IsDead; },
            () => { return Pelican.local.currentTarget && PlayerControl.LocalPlayer.CanMove; },
            () => { pelicanKillButton.Timer = pelicanKillButton.MaxTimer; },
            Vulture.getButtonSprite(),
            CustomButton.ButtonPositions.upperRowRight,
            __instance,
            KeyCode.Q,
            buttonText: ModTranslation.getString("SwallowText"),
            abilityTexture: CustomButton.ButtonLabelType.UseButton
            );

            // EvilHacker button
            evilHackerButton = new CustomButton(
                () => {
                    if (PlayerControl.LocalPlayer.isRole(RoleId.EvilHacker))
                        EvilHacker.local.acTokenChallenge.Value.admin = true;
                    if (!MapBehaviour.Instance || !MapBehaviour.Instance.isActiveAndEnabled)
                    {
                        HudManager __instance = FastDestroyableSingleton<HudManager>.Instance;
                        __instance.InitMap();
                        MapBehaviour.Instance.ShowCountOverlay(allowedToMove: true, showLivePlayerPosition: true, includeDeadBodies: true);
                    }
                },
                () => {
                    return (PlayerControl.LocalPlayer.isRole(RoleId.EvilHacker) || EvilHacker.isInherited()) &&
                      !PlayerControl.LocalPlayer.Data.IsDead && !mimicAAdminButton.HasButton();
                },
                () =>
                {
                    if (EvilHacker.isInherited())
                        evilHackerButton.PositionOffset = CustomButton.ButtonPositions.lowerRowCenter;
                    else
                        evilHackerButton.PositionOffset = CustomButton.ButtonPositions.upperRowLeft;
                    return PlayerControl.LocalPlayer.CanMove;
                },
                () => { },
                Hacker.getAdminSprite(),
                CustomButton.ButtonPositions.upperRowLeft,
                __instance,
                KeyCode.H,
                false,
                FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Admin),
                abilityTexture: CustomButton.ButtonLabelType.AdminButton
            );

            // Medium button
            mediumButton = new CustomButton(
                () => {
                    if (Medium.local.target != null) {
                        Medium.local.soulTarget = Medium.local.target;
                        mediumButton.HasEffect = true;
                        SoundEffectsManager.play("mediumAsk");
                    }
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Medium) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => {
                    if (mediumButton.isEffectActive && Medium.local.target != Medium.local.soulTarget) {
                        Medium.local.soulTarget = null;
                        mediumButton.Timer = 0f;
                        mediumButton.isEffectActive = false;
                    }
                    return Medium.local.target != null && PlayerControl.LocalPlayer.CanMove;
                },
                () => {
                    mediumButton.Timer = mediumButton.MaxTimer;
                    mediumButton.isEffectActive = false;
                },
                Medium.getQuestionSprite(),
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                KeyCode.F,
                true,
                Medium.duration,
                () => {
                    Medium.local.acTokenCommon.Value++;
                    _ = new StaticAchievementToken("medium.common2");
                    mediumButton.Timer = mediumButton.MaxTimer;
                    if (Medium.local.target == null || Medium.local.target.player == null) return;
                    if (Medium.showQuestionTarget) Medium.Question.Invoke((Medium.local.target.player.PlayerId, PlayerControl.LocalPlayer.PlayerId));
                    string msg = Medium.local.getInfo(Medium.local.target.player, Medium.local.target.killerIfExisting, Medium.local.target.deathReason);
                    FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, msg, false);

                    // Ghost Info
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareGhostInfo, Hazel.SendOption.Reliable, -1);
                    writer.Write(Medium.local.target.player.PlayerId);
                    writer.Write((byte)RPCProcedure.GhostInfoTypes.MediumInfo);
                    writer.Write(msg);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);

                    // Remove soul
                    if (Medium.oneTimeUse) {
                        float closestDistance = float.MaxValue;
                        SpriteRenderer target = null;

                        foreach ((DeadPlayer db, Vector3 ps) in Medium.local.deadBodies) {
                            if (db == Medium.local.target) {
                                Tuple<DeadPlayer, Vector3> deadBody = Tuple.Create(db, ps);
                                Medium.local.deadBodies.Remove(deadBody);
                                break;
                            }

                        }
                        foreach (SpriteRenderer rend in Medium.local.souls) {
                            float distance = Vector2.Distance(rend.transform.position, PlayerControl.LocalPlayer.GetTruePosition());
                            if (distance < closestDistance) {
                                closestDistance = distance;
                                target = rend;
                            }
                        }

                        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(5f, new Action<float>((p) => {
                            if (target != null) {
                                var tmp = target.color;
                                tmp.a = Mathf.Clamp01(1 - p);
                                target.color = tmp;
                            }
                            if (p == 1f && target != null && target.gameObject != null) UnityEngine.Object.Destroy(target.gameObject);
                        })));

                        Medium.local.souls.Remove(target);
                    }
                    SoundEffectsManager.stop("mediumAsk");
                },
                buttonText: ModTranslation.getString("MediumText"),
                abilityTexture: CustomButton.ButtonLabelType.UseButton,
                shakeOnEnd: false
            );

            doomsayerButton = new(
                () =>
                {
                    if (Doomsayer.local.currentTarget != null)
                    {
                        if (Helpers.checkSuspendAction(PlayerControl.LocalPlayer, Doomsayer.local.currentTarget)) return;
                        Doomsayer.Observe.Invoke((Doomsayer.local.currentTarget.PlayerId, PlayerControl.LocalPlayer.PlayerId));
                        Doomsayer.local.usesLeft--;
                        doomsayerButton.Timer = doomsayerButton.MaxTimer;
                        _ = new StaticAchievementToken("doomsayer.common1");
                    }
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Doomsayer) && !PlayerControl.LocalPlayer.Data.IsDead && Doomsayer.local.usesLeft > 0; },
                () =>
                {
                    if (doomsayerUsesText != null) doomsayerUsesText.text = Doomsayer.local.usesLeft.ToString();
                    return PlayerControl.LocalPlayer.CanMove && Doomsayer.local.currentTarget != null;
                },
                () =>
                {
                    doomsayerButton.Timer = doomsayerButton.MaxTimer = Doomsayer.cooldown;
                },
                Doomsayer.getButtonSprite(),
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                KeyCode.F,
                buttonText: ModTranslation.getString("DoomsayerText"),
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            );
            doomsayerUsesText = doomsayerButton.ShowUsesIcon(1);

            // Fortune Teller button
            fortuneTellerButtons = new List<CustomButton>();
            Vector3 fortuneTellerCalcPos(byte index)
            {
                //int adjIndex = index < PlayerControl.LocalPlayer.PlayerId ? index % 15 : (index > 15 ? index % 15 : index - 1);
                int adjIndex = 0;
                if (index >= PlayerControl.LocalPlayer.PlayerId) index--;
                adjIndex = index % 14;
                //return new Vector3(-0.25f, -0.15f, 0) + Vector3.right * adjIndex * 0.55f;
                return new Vector3(0.95f, -0.15f, -61f) + Vector3.right * adjIndex * 0.55f;
            }

            Action fortuneTellerButtonOnClick(byte index)
            {
                return () =>
                {
                    if (PlayerControl.LocalPlayer.CanMove && FortuneTeller.local.numUsed < 1 && FortuneTeller.local.canDivine(index))
                    {
                        PlayerControl p = Helpers.playerById(index);
                        FortuneTeller.local.divine(p);
                    }
                };
            };

            Func<bool> fortuneTellerHasButton(byte index)
            {
                return () =>
                {
                    return PlayerControl.LocalPlayer.isRole(RoleId.FortuneTeller);
                };
            }

            void setButtonPos(byte index)
            {
                Vector3 pos = fortuneTellerCalcPos(index);
                Vector3 scale = new(0.4f, 0.5f, 1.0f);

                Vector3 iconBase = new Vector3(-0.82f, 0.19f, 0) + IntroCutsceneOnDestroyPatch.bottomLeft;

                fortuneTellerButtons[index].PositionOffset = pos;
                fortuneTellerButtons[index].actionButton.transform.localScale = scale;
                TORMapOptions.playerIcons[index].transform.localPosition = pos + iconBase;
            }

            void setIconPos(byte index, bool transparent)
            {
                TORMapOptions.playerIcons[index].transform.localScale = Vector3.one * 0.25f;
                TORMapOptions.playerIcons[index].gameObject.SetActive(PlayerControl.LocalPlayer.CanMove);
                TORMapOptions.playerIcons[index].setSemiTransparent(transparent);
            }

            Func<bool> fortuneTellerCouldUse(byte index)
            {
                return () =>
                {
                    int adjustedIndex = index < PlayerControl.LocalPlayer.PlayerId ? index : index - 1;
                    //　占い師以外の場合、リソースがない場合はボタンを表示しない
                    if (!TORMapOptions.playerIcons.ContainsKey(index) ||
                        !PlayerControl.LocalPlayer.isRole(RoleId.FortuneTeller) ||
                        PlayerControl.LocalPlayer.Data.IsDead ||
                        PlayerControl.LocalPlayer.PlayerId == index ||
                        !FortuneTeller.isCompletedNumTasks(PlayerControl.LocalPlayer) ||
                        FortuneTeller.local.numUsed >= 1 || (adjustedIndex < 14 && FortuneTeller.local.pageIndex == 2) ||
                        (adjustedIndex >= 14 && FortuneTeller.local.pageIndex == 1))
                    {
                        if (TORMapOptions.playerIcons.ContainsKey(index)) TORMapOptions.playerIcons[index].gameObject.SetActive(false);
                        if (fortuneTellerButtons.Count > index) fortuneTellerButtons[index].setActive(false);
                        return false;
                    }

                    // ボタンの位置を変更
                    setButtonPos(index);

                    // ボタンにテキストを設定
                    bool status = true;
                    if (FortuneTeller.local.playerStatus.ContainsKey(index))
                    {
                        status = FortuneTeller.local.playerStatus[index];
                    }

                    if (status)
                    {
                        var progress = FortuneTeller.local.progress.ContainsKey(index) ? FortuneTeller.local.progress[index] : 0f;
                        fortuneTellerButtons[index].buttonText = $"{progress:0.0}/{FortuneTeller.duration:0.0}";
                    }
                    else
                    {
                        fortuneTellerButtons[index].buttonText = ModTranslation.getString("fortuneTellerDead");
                    }

                    // アイコンの位置と透明度を変更
                    setIconPos(index, !FortuneTeller.local.canDivine(index));

                    TORMapOptions.playerIcons[index].gameObject.SetActive(!(MapBehaviour.Instance && MapBehaviour.Instance.IsOpen) &&
                      !MeetingHud.Instance &&
                      !ExileController.Instance && PlayerControl.LocalPlayer.CanMove);
                    fortuneTellerButtons[index].setActive(!(MapBehaviour.Instance && MapBehaviour.Instance.IsOpen) &&
                      !MeetingHud.Instance &&
                      !ExileController.Instance && PlayerControl.LocalPlayer.CanMove);
                    return PlayerControl.LocalPlayer.CanMove && FortuneTeller.local.numUsed < 1 && FortuneTeller.local.canDivine(index);
                };
            }

            for (byte i = 0; i < 24; i++)
            {
                CustomButton fortuneTellerButton = new(
                    // Action OnClick
                    fortuneTellerButtonOnClick(i),
                    // bool HasButton
                    fortuneTellerHasButton(i),
                    // bool CouldUse
                    fortuneTellerCouldUse(i),
                    // Action OnMeetingEnds
                    () => { },
                    // sprite
                    null,
                    // position
                    Vector3.zero,
                    __instance,
                    // keyboard shortcut
                    KeyCode.None,
                    true,
                    abilityTexture: CustomButton.ButtonLabelType.UseButton,
                    isSuicide: true
                )
                {
                    Timer = 0.0f,
                    MaxTimer = 0.0f
                };

                fortuneTellerButtons.Add(fortuneTellerButton);
            }

            fortuneTellerLeftButton = new CustomButton(
                () =>
                {
                    FortuneTeller.local.pageIndex = 1;
                },
                () => {
                    return PlayerControl.LocalPlayer.isRole(RoleId.FortuneTeller) && !PlayerControl.LocalPlayer.Data.IsDead &&
                    FortuneTeller.local.pageIndex == 2 && FortuneTeller.isCompletedNumTasks(PlayerControl.LocalPlayer) && FortuneTeller.local.numUsed < 1 &&
                    TORMapOptions.playerIcons.Count >= 16;
                },
                () =>
                {
                    return PlayerControl.LocalPlayer.CanMove;
                },
                () => { },
                FortuneTeller.getLeftButtonSprite(),
                new Vector3(0.5f, -0.15f, -61f),
                __instance,
                KeyCode.None,
                true
            )
            {
                showButtonText = false
            };

            fortuneTellerRightButton = new CustomButton(
                () =>
                {
                    FortuneTeller.local.pageIndex = 2;
                },
                () =>
                {
                    return PlayerControl.LocalPlayer.isRole(RoleId.FortuneTeller) && !PlayerControl.LocalPlayer.Data.IsDead &&
                    FortuneTeller.local.pageIndex == 1 && FortuneTeller.isCompletedNumTasks(PlayerControl.LocalPlayer) && FortuneTeller.local.numUsed < 1 &&
                    TORMapOptions.playerIcons.Count >= 16;
                },
                () =>
                {
                    return PlayerControl.LocalPlayer.CanMove;
                },
                () => { },
                FortuneTeller.getRightButtonSprite(),
                new Vector3(8.6f, -0.15f, -70f),
                __instance,
                KeyCode.None,
                true
            )
            {
                showButtonText = false
            };

            // Pursuer button
            pursuerButton = new CustomButton(
                () => {
                    if (Pursuer.local.target != null) {
                        if (Helpers.checkSuspendAction(PlayerControl.LocalPlayer, Pursuer.local.target)) return;
                        if (Pursuer.local.target.Data.Role.IsImpostor) _ = new StaticAchievementToken("pursuer.common1");
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetBlanked, Hazel.SendOption.Reliable, -1);
                        writer.Write(Pursuer.local.target.PlayerId);
                        writer.Write(Byte.MaxValue);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.setBlanked(Pursuer.local.target.PlayerId, Byte.MaxValue);

                        Pursuer.local.target = null;

                        Pursuer.local.blanks++;
                        pursuerButton.Timer = pursuerButton.MaxTimer;
                        SoundEffectsManager.play("pursuerBlank");

                        var text = Helpers.CreateAndShowNotification(string.Format(ModTranslation.getString("pursuerBlankNotification"), Pursuer.local.target?.Data?.PlayerName ?? ""), Color.white, new Vector3(0f, 1f, -20f), spr: RoleHelpers.GetRoleIcon(RoleId.Pursuer).GetSprite());
                        text.AdjustNotification();
                    }
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Pursuer) && !PlayerControl.LocalPlayer.Data.IsDead && Pursuer.local.blanks < Pursuer.blanksNumber; },
                () => {
                    if (pursuerButtonBlanksText != null) pursuerButtonBlanksText.text = $"{Pursuer.blanksNumber - Pursuer.local.blanks}";

                    return Pursuer.blanksNumber > Pursuer.local.blanks && PlayerControl.LocalPlayer.CanMove && Pursuer.local.target != null;
                },
                () => { pursuerButton.Timer = pursuerButton.MaxTimer; },
                Pursuer.getTargetSprite(),
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                KeyCode.F,
                buttonText: ModTranslation.getString("PursuerText"),
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            );

            // Pursuer button blanks left
            pursuerButtonBlanksText = pursuerButton.ShowUsesIcon(1);


            // Witch Spell button
            witchSpellButton = new CustomButton(
                () => {
                    if (Helpers.checkSuspendAction(PlayerControl.LocalPlayer, Witch.local.currentTarget)) return;
                    if (Witch.local.currentTarget != null) {
                        Witch.local.spellCastingTarget = Witch.local.currentTarget;
                        SoundEffectsManager.play("witchSpell");
                    }
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Witch) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => {
                    if (witchSpellButton.isEffectActive && Witch.local.spellCastingTarget != Witch.local.currentTarget) {
                        Witch.local.spellCastingTarget = null;
                        witchSpellButton.Timer = 0f;
                        witchSpellButton.isEffectActive = false;
                    }
                    return PlayerControl.LocalPlayer.CanMove && Witch.local.currentTarget != null;
                },
                () => {
                    witchSpellButton.Timer = witchSpellButton.MaxTimer;
                    witchSpellButton.isEffectActive = false;
                },
                Witch.getButtonSprite(),
                CustomButton.ButtonPositions.upperRowLeft,
                __instance,
                KeyCode.F,
                true,
                Witch.spellCastingDuration,
                () => {
                    if (Witch.local.spellCastingTarget == null) return;
                    MurderAttemptResult attempt = Helpers.checkMuderAttempt(PlayerControl.LocalPlayer, Witch.local.spellCastingTarget);
                    if (attempt == MurderAttemptResult.PerformKill) {
                        _ = new StaticAchievementToken("witch.common1");
                        Witch.local.acTokenChallenge.Value++;
                        Witch.SetFutureSpelled.Invoke((Witch.local.currentTarget.PlayerId, PlayerControl.LocalPlayer.PlayerId));
                    }
                    if (attempt is MurderAttemptResult.BlankKill or MurderAttemptResult.PerformKill) {
                        Witch.local.currentCooldownAddition += Witch.cooldownAddition;
                        witchSpellButton.MaxTimer = Witch.cooldown + Witch.local.currentCooldownAddition;
                        PlayerControlFixedUpdatePatch.miniCooldownUpdate();  // Modifies the MaxTimer if the witch is the mini
                        witchSpellButton.Timer = witchSpellButton.MaxTimer;
                        if (Witch.triggerBothCooldowns) {
                            float multiplier = (Mini.mini != null && PlayerControl.LocalPlayer == Mini.mini) ? (Mini.isGrownUp() ? 0.66f : 2f) : 1f;
                            PlayerControl.LocalPlayer.killTimer = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown * multiplier;
                        }
                    } else {
                        witchSpellButton.Timer = 0f;
                    }
                    Witch.local.spellCastingTarget = null;
                },
                buttonText: ModTranslation.getString("WitchText"),
                shakeOnEnd: false
            );

            sprintButton = new CustomButton(
                () =>
                {
                    if (sprintButton.isEffectActive)
                    {
                        sprintButton.Timer = 0;
                        return;
                    }

                    _ = new StaticAchievementToken("sprinter.common1");
                    if (Sprinter.local.acTokenMove != null) Sprinter.local.acTokenMove.Value.pos = PlayerControl.LocalPlayer.GetTruePosition();

                    Sprinter.Sprint.Invoke((PlayerControl.LocalPlayer.PlayerId, true));
                    SoundEffectsManager.play("ninjaStealth");
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Sprinter) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (sprintButton.isEffectActive)
                    {
                        sprintButton.buttonText = ModTranslation.getString("SprinterStopText");
                    }
                    else
                    {
                        sprintButton.buttonText = ModTranslation.getString("SprintText");
                    }
                    return PlayerControl.LocalPlayer.CanMove;
                },
                () =>
                {
                    sprintButton.Timer = sprintButton.MaxTimer = Sprinter.sprintCooldown;
                    sprintButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                    sprintButton.isEffectActive = false;
                },
                Sprinter.getButtonSprite(),
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                KeyCode.F,
                true,
                Sprinter.sprintDuration,
                () =>
                {
                    sprintButton.Timer = sprintButton.MaxTimer = Sprinter.sprintCooldown;
                    if (Sprinter.local.acTokenMove != null) Sprinter.local.acTokenMove.Value.cleared |= PlayerControl.LocalPlayer.GetTruePosition().Distance(Sprinter.local.acTokenMove.Value.pos) > 15f;
                    Sprinter.Sprint.Invoke((PlayerControl.LocalPlayer.PlayerId, false));
                },
                abilityTexture: CustomButton.ButtonLabelType.UseButton,
                buttonText: ModTranslation.getString("SprintText")
            )
            {
                effectCancellable = true
            };

            // Assassin mark and assassinate button 
            assassinButton = new CustomButton(
                () =>
                {
                    MessageWriter writer;
                    if (Assassin.local.assassinMarked != null)
                    {
                        // Murder attempt with teleport
                        MurderAttemptResult attempt = Helpers.checkMuderAttempt(PlayerControl.LocalPlayer, Assassin.local.assassinMarked);
                        if (attempt == MurderAttemptResult.ReverseKill)
                        {
                            Helpers.checkMurderAttemptAndKill(Assassin.local.assassinMarked, PlayerControl.LocalPlayer);
                            return;
                        }
                        if (attempt == MurderAttemptResult.PerformKill)
                        {
                            _ = new StaticAchievementToken("assassin.common1");
                            Assassin.local.acTokenChallenge.Value.markKill = true;
                            // Create first trace before killing
                            var pos = PlayerControl.LocalPlayer.transform.position;
                            Assassin.PlaceTrace.Invoke((PlayerControl.LocalPlayer.PlayerId, pos));
                            Assassin.SetInvisible.Invoke((PlayerControl.LocalPlayer.PlayerId, byte.MinValue));

                            // Perform Kill
                            MessageWriter writer2 = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedMurderPlayer, Hazel.SendOption.Reliable, -1);
                            writer2.Write(PlayerControl.LocalPlayer.PlayerId);
                            writer2.Write(Assassin.local.assassinMarked.PlayerId);
                            writer2.Write(byte.MaxValue);
                            AmongUsClient.Instance.FinishRpcImmediately(writer2);
                            RPCProcedure.uncheckedMurderPlayer(PlayerControl.LocalPlayer.PlayerId, Assassin.local.assassinMarked.PlayerId, byte.MaxValue);

                            // Create Second trace after killing
                            pos = Assassin.local.assassinMarked.transform.position;
                            Assassin.PlaceTrace.Invoke((PlayerControl.LocalPlayer.PlayerId, pos));
                        }

                        if (attempt is MurderAttemptResult.BlankKill or MurderAttemptResult.PerformKill)
                        {
                            assassinButton.Timer = assassinButton.MaxTimer;
                            PlayerControl.LocalPlayer.killTimer = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown;
                        }
                        else if (attempt == MurderAttemptResult.SuppressKill)
                        {
                            assassinButton.Timer = 0f;
                        }
                        Assassin.local.assassinMarked = null;
                        return;
                    }
                    if (Assassin.local.currentTarget != null)
                    {
                        if (Helpers.checkSuspendAction(PlayerControl.LocalPlayer, Assassin.local.currentTarget)) return;
                        Assassin.local.assassinMarked = Assassin.local.currentTarget;
                        assassinButton.Timer = 5f;
                        SoundEffectsManager.play("warlockCurse");

                        // Ghost Info
                        writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareGhostInfo, Hazel.SendOption.Reliable, -1);
                        writer.Write(PlayerControl.LocalPlayer.PlayerId);
                        writer.Write((byte)RPCProcedure.GhostInfoTypes.AssassinMarked);
                        writer.Write(Assassin.local.assassinMarked.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                    }
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleId.Assassin) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {  // CouldUse
                    assassinButton.Sprite = Assassin.local.assassinMarked != null ? Assassin.getKillButtonSprite() : Assassin.getMarkButtonSprite();
                    return (Assassin.local.currentTarget != null || (Assassin.local.assassinMarked != null && !TransportationToolPatches.isUsingTransportation(Assassin.local.assassinMarked))) && PlayerControl.LocalPlayer.CanMove;
                },
                () =>
                {  // on meeting ends
                    assassinButton.Timer = assassinButton.MaxTimer;
                },
                Assassin.getMarkButtonSprite(),
                CustomButton.ButtonPositions.upperRowLeft,
                __instance,
                KeyCode.F
            )
            { buttonText = ""};

            mayorMeetingButton = new CustomButton(
               () => {
                   PlayerControl.LocalPlayer.NetTransform.Halt(); // Stop current movement 
                   Mayor.local.remoteMeetingsLeft--;

                   if (Mathf.RoundToInt(CustomOptionHolder.mayorMaxRemoteMeetings.getFloat()) - Mayor.local.remoteMeetingsLeft >= 3)
                       _ = new StaticAchievementToken("mayor.another1");

	               Helpers.handleVampireBiteOnBodyReport(); // Manually call Vampire handling, since the CmdReportDeadBody Prefix won't be called
                   Helpers.HandleUndertakerDropOnBodyReport();
                   Helpers.handleTrapperTrapOnBodyReport();
                   RPCProcedure.uncheckedCmdReportDeadBody(PlayerControl.LocalPlayer.PlayerId, Byte.MaxValue);

                   MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedCmdReportDeadBody, Hazel.SendOption.Reliable, -1);
                   writer.Write(PlayerControl.LocalPlayer.PlayerId);
                   writer.Write(Byte.MaxValue);
                   AmongUsClient.Instance.FinishRpcImmediately(writer);
                   mayorMeetingButton.Timer = 1f;
               },
               () => { return PlayerControl.LocalPlayer.isRole(RoleId.Mayor) && !PlayerControl.LocalPlayer.Data.IsDead && Mayor.meetingButton; },
               () => {
                   mayorMeetingButton.actionButton.OverrideText(ModTranslation.getString("mayorEmergencyLeftText") + " (" + Mayor.local.remoteMeetingsLeft + ")");
                   bool sabotageActive = false;
                   foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks.GetFastEnumerator())
                       if (task.TaskType == TaskTypes.FixLights || task.TaskType == TaskTypes.RestoreOxy || task.TaskType == TaskTypes.ResetReactor || task.TaskType == TaskTypes.ResetSeismic || task.TaskType == TaskTypes.FixComms || task.TaskType == TaskTypes.StopCharles
                           || (SubmergedCompatibility.IsSubmerged && task.TaskType == SubmergedCompatibility.RetrieveOxygenMask))
                           sabotageActive = true;
                   return !sabotageActive && PlayerControl.LocalPlayer.CanMove && (Mayor.local.remoteMeetingsLeft > 0);
               },
               () => { mayorMeetingButton.Timer = mayorMeetingButton.MaxTimer; },
               Mayor.getMeetingSprite(),
               CustomButton.ButtonPositions.lowerRowRight,
               __instance,
               KeyCode.F,
               true,
               0f,
               () => {},
               false,
               ModTranslation.getString("mayorEmergencyMeetingText")
           );

            operateButton = new CustomButton(
                () => { FreePlayGM.OpenRoleWindow(); },
                () => { return FreePlayGM.isFreePlayGM; },
                () => { return FreePlayGM.roleScreen == null; },
                () => { operateButton.Timer = operateButton.MaxTimer = 0f; },
                FreePlayGM.getOperateButtonSprite(),
                new Vector3(0f, 1f, 0f),
                __instance,
                KeyCode.Z,
                true,
                buttonText: ModTranslation.getString("FreePlayOperateText"),
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            );

            freePlayReviveButton = new CustomButton(
                () =>
                {
                    PlayerControl.LocalPlayer.Revive();
                    DeadBody[] array = UnityEngine.Object.FindObjectsOfType<DeadBody>();
                    for (int i = 0; i < array.Length; i++)
                    {
                        if (GameData.Instance.GetPlayerById(array[i].ParentId).PlayerId == PlayerControl.LocalPlayer.PlayerId)
                        {
                            UnityEngine.Object.Destroy(array[i].gameObject);
                        }
                    }
                },
                () => { return FreePlayGM.isFreePlayGM && PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    return true;
                },
                () => { freePlayReviveButton.Timer = freePlayReviveButton.MaxTimer = 0f; },
                FreePlayGM.getReviveButtonSprite(),
                new Vector3(1f, 1f, 0f),
                __instance,
                KeyCode.Y,
                true,
                buttonText: ModTranslation.getString("FreePlayReviveText"),
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            );

            freePlaySuicideButton = new CustomButton(
                () =>
                {
                    if (PlayerControl.LocalPlayer.AmOwner)
                    {
                        if (Constants.ShouldPlaySfx())
                        {
                            SoundManager.Instance.PlaySound(PlayerControl.LocalPlayer.KillSfx, false, 0.8f);
                        }
                        PlayerControl.LocalPlayer.cosmetics.SetNameMask(false);
                        PlayerControl.LocalPlayer.RpcSetScanner(false);
                    }
                    PlayerControl.LocalPlayer.MyPhysics.StartCoroutine(PlayerControl.LocalPlayer.KillAnimations.First().CoPerformKill(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer));
                },
                () => { return FreePlayGM.isFreePlayGM && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    return true;
                },
                () => { freePlaySuicideButton.Timer = freePlaySuicideButton.MaxTimer = 0f; },
                __instance.KillButton.graphic.sprite,
                new Vector3(1f, 1f, 0f),
                __instance,
                KeyCode.X,
                true,
                buttonText: ModTranslation.getString("FreePlaySuicideText"),
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            );

            thiefKillButton = new CustomButton(
                () => {
                    PlayerControl thief = PlayerControl.LocalPlayer;
                    PlayerControl target = Thief.local.currentTarget;
                    var result = Helpers.checkMuderAttempt(thief, target);
                    if (result == MurderAttemptResult.BlankKill) {
                        thiefKillButton.Timer = thiefKillButton.MaxTimer;
                        return;
                    }

                    if (Thief.local.suicideFlag) {
                        // Suicide
                        MessageWriter writer2 = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedMurderPlayer, Hazel.SendOption.Reliable, -1);
                        writer2.Write(thief.PlayerId);
                        writer2.Write(thief.PlayerId);
                        writer2.Write(0);
                        RPCProcedure.uncheckedMurderPlayer(thief.PlayerId, thief.PlayerId, 0);
                        AmongUsClient.Instance.FinishRpcImmediately(writer2);
                        if (!FreePlayGM.isFreePlayGM) PlayerControl.LocalPlayer.clearAllTasks();
                        _ = new StaticAchievementToken("thief.another1");
                    }

                    // Steal role if survived.
                    if (!PlayerControl.LocalPlayer.Data.IsDead && result == MurderAttemptResult.PerformKill) {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ThiefStealsRole, Hazel.SendOption.Reliable, -1);
                        writer.Write(target.PlayerId);
                        writer.Write(PlayerControl.LocalPlayer.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.thiefStealsRole(target.PlayerId, PlayerControl.LocalPlayer.PlayerId);
                        _ = new StaticAchievementToken("thief.challenge");
                    }
                    // Kill the victim (after becoming their role - so that no win is triggered for other teams)
                    if (result == MurderAttemptResult.PerformKill) {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedMurderPlayer, Hazel.SendOption.Reliable, -1);
                        writer.Write(thief.PlayerId);
                        writer.Write(target.PlayerId);
                        writer.Write(byte.MaxValue);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.uncheckedMurderPlayer(thief.PlayerId, target.PlayerId, byte.MaxValue);
                    }
                },
               () => { return PlayerControl.LocalPlayer.isRole(RoleId.Thief) && !PlayerControl.LocalPlayer.Data.IsDead; },
               () => { return Thief.local.currentTarget != null && PlayerControl.LocalPlayer.CanMove; },
               () => { thiefKillButton.Timer = thiefKillButton.MaxTimer; },
               __instance.KillButton.graphic.sprite,
               CustomButton.ButtonPositions.upperRowRight,
               __instance,
               KeyCode.Q
               );

            // Trapper Charges
            //trapperChargesText = GameObject.Instantiate(trapperButton.actionButton.cooldownTimerText, trapperButton.actionButton.cooldownTimerText.transform.parent);
            //trapperChargesText.text = "";
            //trapperChargesText.enableWordWrapping = false;
            //trapperChargesText.transform.localScale = Vector3.one * 0.5f;
            //trapperChargesText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            zoomOutButton = new CustomButton(
                () =>
                {
                    Helpers.toggleZoom();
                },
                () =>
                {
                    if (PlayerControl.LocalPlayer == null || !PlayerControl.LocalPlayer.Data.IsDead || PlayerControl.LocalPlayer.Data.Role.IsImpostor || Busker.players.Any(x => x.player == PlayerControl.LocalPlayer && x.pseudocideFlag)) return false;
                    var (playerCompleted, playerTotal) = TasksHandler.taskInfo(PlayerControl.LocalPlayer.Data);
                    int numberOfLeftTasks = playerTotal - playerCompleted;
                    return numberOfLeftTasks <= 0 || !CustomOptionHolder.finishTasksBeforeHauntingOrZoomingOut.getBool();
                },
                () => { return true; },
                () => { return; },
                null,  // Invisible button!
                new Vector3(0.4f, 2.8f, 0),
                __instance,
                KeyCode.KeypadPlus
                )
            {
                Timer = 0f,
                showButtonText = false
            };


            // PropHunt Buttons
            propDisguiseButton = new CustomButton(
                () => {
                    var player = PlayerControl.LocalPlayer;
                    var target = PropHuntGM.currentTarget;
                    if (target != null) {
                        player.transform.localScale = target.transform.lossyScale;
                        var writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, 128, SendOption.Reliable);
                        RPCProcedure.PropHuntSetProp.GetInvoker((player.PlayerId, target.name, target.transform.position.x)).Invoke(writer);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.PropHuntSetProp.LocalInvoke((player.PlayerId, target.name, target.transform.position.x));
                        SoundEffectsManager.play("morphlingMorph");
                    }
                },
                () => { return PropHuntGM.isPropHuntGM && !PlayerControl.LocalPlayer.Data.Role.IsImpostor && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => {
                    propSpriteRenderer.sprite = PropHuntGM.currentTarget?.GetComponent<SpriteRenderer>()?.sprite;
                    if (propSpriteRenderer.sprite == null)
                        propSpriteRenderer.sprite = PropHuntGM.currentTarget?.transform.GetComponentInChildren<SpriteRenderer>()?.sprite;
                    return PropHuntGM.currentTarget != null && propSpriteRenderer.sprite != null;
                },
                () => { },
                PropHuntGM.getDisguiseButtonSprite(),
                CustomButton.ButtonPositions.lowerRowRight,
                __instance,
                KeyCode.F,
                true,
                1f,
                () => { },
                false,
                "",
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            );
            propSpriteHolder = new GameObject("propSpriteHolder");
            propSpriteHolder.transform.SetParent(propDisguiseButton.actionButtonGameObject.transform, false);
            propSpriteHolder.transform.localPosition = new Vector3(0, 0, -2f);
            propSpriteRenderer = propSpriteHolder.AddComponent<SpriteRenderer>();

            propHuntUnstuckButton = new CustomButton(
                () => { PlayerControl.LocalPlayer.Collider.enabled = false; },
                () => { return PropHuntGM.isPropHuntGM && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return true; },
                () => { },
                PropHuntGM.getUnstuckButtonSprite(),
                CustomButton.ButtonPositions.upperRowLeft,
                __instance,
                KeyCode.LeftShift,
                true,
                PropPH.unstuckDuration,
                () => { PlayerControl.LocalPlayer.Collider.enabled = true; },
                false,
                "",
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            );

            propHuntRevealButton = new CustomButton(
                () => {
                    var candidates = PlayerControl.AllPlayerControls.ToArray().Where(x => !x.Data.Role.IsImpostor && !x.Data.IsDead && !PropHuntGM.isCurrentlyRevealed.ContainsKey(x.PlayerId)).ToList();
                    if (candidates.Count == 0) return;
                    var selected = candidates[TheOtherRoles.rnd.Next(candidates.Count)];
                    RPCProcedure.PropHuntSetRevealed.Invoke(selected.PlayerId);
                },
                () => { return PropHuntGM.isPropHuntGM && !PlayerControl.LocalPlayer.Data.IsDead && PlayerControl.LocalPlayer.Data.Role.IsImpostor; },
                () => { return PropHuntGM.timer - HunterPH.revealPunish > 0; },
                () => { },
                PropHuntGM.getRevealButtonSprite(),
                CustomButton.ButtonPositions.upperRowFarLeft,
                __instance,
                KeyCode.R,
                true,
                HunterPH.revealDuration,
                () => { },
                false,
                "",
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            );

            propHuntInvisButton = new CustomButton(
                () => {
                    RPCProcedure.PropHuntSetInvis.Invoke(PlayerControl.LocalPlayer.PlayerId);
                    SoundEffectsManager.play("morphlingMorph");
                },
                () => { return PropHuntGM.isPropHuntGM && !PlayerControl.LocalPlayer.Data.IsDead && !PlayerControl.LocalPlayer.Data.Role.IsImpostor && PropPH.enableInvis; },
                () => { return PropHuntGM.currentObject.ContainsKey(PlayerControl.LocalPlayer.PlayerId); },
                () => { },
                PropHuntGM.getInvisButtonSprite(),
                CustomButton.ButtonPositions.upperRowFarLeft,
                __instance,
                KeyCode.I,
                true,
                PropPH.invisDuration,
                () => { },
                false,
                "",
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            );

            propHuntSpeedboostButton = new CustomButton(
                () => {
                    RPCProcedure.PropHuntSetSpeedboost.Invoke(PlayerControl.LocalPlayer.PlayerId);
                    SoundEffectsManager.play("timemasterShield");
                },
                () => { return PropHuntGM.isPropHuntGM && !PlayerControl.LocalPlayer.Data.IsDead && !PlayerControl.LocalPlayer.Data.Role.IsImpostor && PropPH.enableSpeedboost; },
                () => { return true; },
                () => { },
                PropHuntGM.getSpeedboostButtonSprite(),
                CustomButton.ButtonPositions.lowerRowCenter,
                __instance,
                KeyCode.G,
                true,
                PropPH.speedboostDuration,
                () => { },
                false,
                "",
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            );

            propHuntAdminButton = new CustomButton(
                () => {
                    if (!MapBehaviour.Instance || !MapBehaviour.Instance.isActiveAndEnabled) {
                        HudManager __instance2 = FastDestroyableSingleton<HudManager>.Instance;
                        __instance2.InitMap();
                        MapBehaviour.Instance.ShowCountOverlay(allowedToMove: true, showLivePlayerPosition: true, includeDeadBodies: false);
                    }
                    PlayerControl.LocalPlayer.NetTransform.Halt();
                },
                () => { return PropHuntGM.isPropHuntGM && !PlayerControl.LocalPlayer.Data.IsDead && PlayerControl.LocalPlayer.Data.Role.IsImpostor; },
                () => { return true; },
                () => { propHuntAdminButton.Timer = propHuntAdminButton.MaxTimer; },
                Hacker.getAdminSprite(),
                CustomButton.ButtonPositions.lowerRowCenter,
                __instance,
                KeyCode.G,
                true,
                HunterPH.adminDuration,
                () => { propHuntAdminButton.Timer = propHuntAdminButton.MaxTimer; if (MapBehaviour.Instance) MapBehaviour.Instance.Close(); },
                false,
                FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Admin)
            );

            propHuntFindButton = new CustomButton(
                () => { SoundEffectsManager.play("timemasterShield"); },
                () => { return PropHuntGM.isPropHuntGM && !PlayerControl.LocalPlayer.Data.IsDead && PlayerControl.LocalPlayer.Data.Role.IsImpostor; },
                () => { return true; },
                () => { },
                PropHuntGM.getFindButtonSprite(),
                CustomButton.ButtonPositions.lowerRowCenter,
                __instance,
                KeyCode.F,
                true,
                HunterPH.findDuration,
                () => { },
                false,
                "",
                abilityTexture: CustomButton.ButtonLabelType.UseButton
            );

            // Set the default (or settings from the previous game) timers / durations when spawning the buttons
            initialized = true;
            setCustomButtonCooldowns();
            deputyHandcuffedButtons = new Dictionary<byte, List<CustomButton>>();
            
        }
    }
}
