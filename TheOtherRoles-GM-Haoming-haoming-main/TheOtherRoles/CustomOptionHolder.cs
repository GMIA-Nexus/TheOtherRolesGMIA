using System.Collections.Generic;
using Il2CppSystem.Buffers;
using UnityEngine;
using static TheOtherRoles.CustomOption;
using static TheOtherRoles.TheOtherRoles;
using static TheOtherRoles.TheOtherRolesGM;

namespace TheOtherRoles
{

    public class CustomOptionHolder
    {
        public static string[] rates = new string[] { "0%", "10%", "20%", "30%", "40%", "50%", "60%", "70%", "80%", "90%", "100%" };
        public static string[] presets = new string[] { "preset1", "preset2", "randomPresetSkeld", "randomPresetMira", "randomPresetPolus", "randomPresetAir", "randomPresetSub" };
        public static CustomOption presetSelection;
        public static CustomOption crewmateRolesCountMin;
        public static CustomOption crewmateRolesCountMax;
        public static CustomOption neutralRolesCountMin;
        public static CustomOption neutralRolesCountMax;
        public static CustomOption impostorRolesCountMin;
        public static CustomOption impostorRolesCountMax;

        public static CustomRoleOption mafiaSpawnRate;
        public static CustomOption mafiosoCanSabotage;
        public static CustomOption mafiosoCanRepair;
        public static CustomOption mafiosoCanVent;
        public static CustomOption janitorCooldown;
        public static CustomOption janitorCanSabotage;
        public static CustomOption janitorCanRepair;
        public static CustomOption janitorCanVent;

        public static CustomRoleOption morphlingSpawnRate;
        public static CustomOption morphlingCooldown;
        public static CustomOption morphlingDuration;

        public static CustomRoleOption camouflagerSpawnRate;
        public static CustomOption camouflagerCooldown;
        public static CustomOption camouflagerDuration;
        public static CustomOption camouflagerRandomColors;

        public static CustomRoleOption evilHackerSpawnRate;
        public static CustomOption evilHackerCanHasBetterAdmin;
        public static CustomOption evilHackerCanCreateMadmate;
        public static CustomOption evilHackerCanCreateMadmateFromFox;
        public static CustomOption evilHackerCanCreateMadmateFromJackal;
        public static CustomOption evilHackerCanMoveEvenIfUsesAdmin;
        public static CustomOption evilHackerCanInheritAbility;
        public static CustomOption evilHackerCanSeeDoorStatus;
        public static CustomOption createdMadmateCanDieToSheriff;
        public static CustomOption createdMadmateCanEnterVents;
        public static CustomOption createdMadmateHasImpostorVision;
        public static CustomOption createdMadmateCanSabotage;
        public static CustomOption createdMadmateCanFixComm;
        public static CustomOption createdMadmateAbility;
        public static CustomOption createdMadmateNumTasks;
        public static CustomOption createdMadmateExileCrewmate;

        public static CustomRoleOption vampireSpawnRate;
        public static CustomOption vampireKillDelay;
        public static CustomOption vampireCooldown;
        public static CustomOption vampireCanKillNearGarlics;

        public static CustomRoleOption eraserSpawnRate;
        public static CustomOption eraserCooldown;
        public static CustomOption eraserCooldownIncrease;
        public static CustomOption eraserCanEraseAnyone;

        public static CustomRoleOption miniSpawnRate;
        public static CustomOption miniGrowingUpDuration;
        public static CustomOption miniIsImpRate;

        public static CustomOption loversSpawnRate;
        public static CustomOption loversNumCouples;
        public static CustomOption loversImpLoverRate;
        public static CustomOption loversBothDie;
        public static CustomOption loversCanHaveAnotherRole;
        public static CustomOption loversSeparateTeam;
        public static CustomOption loversTasksCount;
        public static CustomOption loversEnableChat;

        public static CustomRoleOption antiTeleportSpawnRate;

        public static CustomRoleOption guesserSpawnRate;
        public static CustomOption guesserIsImpGuesserRate;
        public static CustomOption guesserNumberOfShots;
        public static CustomOption guesserOnlyAvailableRoles;
        public static CustomOption guesserHasMultipleShotsPerMeeting;
        public static CustomOption guesserShowInfoInGhostChat;
        public static CustomOption guesserKillsThroughShield;
        public static CustomOption guesserEvilCanKillSpy;
        public static CustomOption guesserSpawnBothRate;
        public static CustomOption guesserCantGuessSnitchIfTaskDone;

        public static CustomRoleOption jesterSpawnRate;
        public static CustomOption jesterCanCallEmergency;
        public static CustomOption jesterCanSabotage;
        public static CustomOption jesterHasImpostorVision;

        public static CustomRoleOption arsonistSpawnRate;
        public static CustomOption arsonistCooldown;
        public static CustomOption arsonistDuration;
        public static CustomOption arsonistCanBeLovers;

        public static CustomRoleOption jackalSpawnRate;
        public static CustomOption jackalKillCooldown;
        public static CustomOption jackalCreateSidekickCooldown;
        public static CustomOption jackalCanUseVents;
        public static CustomOption jackalCanCreateSidekick;
        public static CustomOption sidekickPromotesToJackal;
        public static CustomOption sidekickCanKill;
        public static CustomOption sidekickCanUseVents;
        public static CustomOption jackalPromotedFromSidekickCanCreateSidekick;
        public static CustomOption jackalCanCreateSidekickFromImpostor;
        public static CustomOption jackalCanCreateSidekickFromFox;
        public static CustomOption jackalAndSidekickHaveImpostorVision;
        public static CustomOption jackalCanSeeEngineerVent;

        public static CustomRoleOption bountyHunterSpawnRate;
        public static CustomOption bountyHunterBountyDuration;
        public static CustomOption bountyHunterReducedCooldown;
        public static CustomOption bountyHunterPunishmentTime;
        public static CustomOption bountyHunterShowArrow;
        public static CustomOption bountyHunterArrowUpdateInterval;

        public static CustomRoleOption witchSpawnRate;
        public static CustomOption witchCooldown;
        public static CustomOption witchAdditionalCooldown;
        public static CustomOption witchCanSpellAnyone;
        public static CustomOption witchSpellCastingDuration;
        public static CustomOption witchTriggerBothCooldowns;
        public static CustomOption witchVoteSavesTargets;

        public static CustomRoleOption assassinSpawnRate;
        public static CustomOption assassinCooldown;
        public static CustomOption assassinKnowsTargetLocation;
        public static CustomOption assassinTraceTime;
        public static CustomOption assassinTraceColorTime;

        public static CustomRoleOption shifterSpawnRate;
        public static CustomOption shifterIsNeutralRate;
        public static CustomOption shifterShiftsModifiers;
        public static CustomOption shifterPastShifters;

        public static CustomRoleOption fortuneTellerSpawnRate;
        public static CustomOption fortuneTellerNumTasks;
        public static CustomOption fortuneTellerResults;
        public static CustomOption fortuneTellerDistance;
        public static CustomOption fortuneTellerDuration;

        public static CustomRoleOption mayorSpawnRate;
        public static CustomOption mayorNumVotes;

        public static CustomRoleOption engineerSpawnRate;
        public static CustomOption engineerNumberOfFixes;
        public static CustomOption engineerHighlightForImpostors;
        public static CustomOption engineerHighlightForTeamJackal;

        public static CustomRoleOption sheriffSpawnRate;
        public static CustomOption sheriffCooldown;
        public static CustomOption sheriffNumShots;
        public static CustomOption sheriffCanKillNeutrals;
        public static CustomOption sheriffMisfireKillsTarget;
        public static CustomOption sheriffCanKillNoDeadBody;

        public static CustomRoleOption sherlockSpawnRate;
        public static CustomOption sherlockCooldown;
        public static CustomOption sherlockRechargeTasksNumber;
        public static CustomOption sherlockInvestigateDistance;

        public static CustomRoleOption lighterSpawnRate;
        public static CustomOption lighterModeLightsOnVision;
        public static CustomOption lighterModeLightsOffVision;
        public static CustomOption lighterCooldown;
        public static CustomOption lighterDuration;
        public static CustomOption lighterCanSeeNinja;

        public static CustomRoleOption detectiveSpawnRate;
        public static CustomOption detectiveAnonymousFootprints;
        public static CustomOption detectiveFootprintInterval;
        public static CustomOption detectiveFootprintDuration;
        public static CustomOption detectiveReportNameDuration;
        public static CustomOption detectiveReportColorDuration;

        public static CustomRoleOption timeMasterSpawnRate;
        public static CustomOption timeMasterCooldown;
        public static CustomOption timeMasterRewindTime;
        public static CustomOption timeMasterShieldDuration;

        public static CustomRoleOption medicSpawnRate;
        public static CustomOption medicShowShielded;
        public static CustomOption medicShowAttemptToShielded;
        public static CustomOption medicSetShieldAfterMeeting;
        public static CustomOption medicShowAttemptToMedic;

        public static CustomRoleOption swapperSpawnRate;
        public static CustomOption swapperIsImpRate;
        public static CustomOption swapperCanCallEmergency;
        public static CustomOption swapperCanOnlySwapOthers;
        public static CustomOption swapperNumSwaps;

        public static CustomRoleOption seerSpawnRate;
        public static CustomOption seerMode;
        public static CustomOption seerSoulDuration;
        public static CustomOption seerLimitSoulDuration;

        public static CustomRoleOption hackerSpawnRate;
        public static CustomOption hackerCooldown;
        public static CustomOption hackerHackeringDuration;
        public static CustomOption hackerOnlyColorType;
        public static CustomOption hackerToolsNumber;
        public static CustomOption hackerRechargeTasksNumber;
        public static CustomOption hackerNoMove;

        public static CustomRoleOption trackerSpawnRate;
        public static CustomOption trackerUpdateInterval;
        public static CustomOption trackerResetTargetAfterMeeting;
        public static CustomOption trackerCanTrackCorpses;
        public static CustomOption trackerCorpsesTrackingCooldown;
        public static CustomOption trackerCorpsesTrackingDuration;

        public static CustomRoleOption snitchSpawnRate;
        public static CustomOption snitchLeftTasksForReveal;
        public static CustomOption snitchIncludeTeamJackal;
        public static CustomOption snitchTeamJackalUseDifferentArrowColor;

        public static CustomRoleOption spySpawnRate;
        public static CustomOption spyCanDieToSheriff;
        public static CustomOption spyImpostorsCanKillAnyone;
        public static CustomOption spyCanEnterVents;
        public static CustomOption spyHasImpostorVision;

        public static CustomRoleOption tricksterSpawnRate;
        public static CustomOption tricksterPlaceBoxCooldown;
        public static CustomOption tricksterLightsOutCooldown;
        public static CustomOption tricksterLightsOutDuration;

        public static CustomRoleOption cleanerSpawnRate;
        public static CustomOption cleanerCooldown;

        public static CustomRoleOption warlockSpawnRate;
        public static CustomOption warlockCooldown;
        public static CustomOption warlockRootTime;

        public static CustomRoleOption securityGuardSpawnRate;
        public static CustomOption securityGuardCooldown;
        public static CustomOption securityGuardTotalScrews;
        public static CustomOption securityGuardCamPrice;
        public static CustomOption securityGuardVentPrice;
        public static CustomOption securityGuardCamDuration;
        public static CustomOption securityGuardCamMaxCharges;
        public static CustomOption securityGuardCamRechargeTasksNumber;
        public static CustomOption securityGuardNoMove;

        public static CustomRoleOption baitSpawnRate;
        public static CustomOption baitHighlightAllVents;
        public static CustomOption baitReportDelay;
        public static CustomOption baitShowKillFlash;
        public static CustomOption baitCanBeGuessed;

        public static CustomRoleOption vultureSpawnRate;
        public static CustomOption vultureCooldown;
        public static CustomOption vultureNumberToWin;
        public static CustomOption vultureCanUseVents;
        public static CustomOption vultureShowArrows;

        public static CustomRoleOption mediumSpawnRate;
        public static CustomOption mediumCooldown;
        public static CustomOption mediumDuration;
        public static CustomOption mediumOneTimeUse;

        public static CustomRoleOption lawyerSpawnRate;
        public static CustomOption lawyerTargetKnows;
        public static CustomOption lawyerWinsAfterMeetings;
        public static CustomOption lawyerNeededMeetings;
        public static CustomOption lawyerVision;
        public static CustomOption lawyerKnowsRole;
        public static CustomOption pursuerCooldown;
        public static CustomOption pursuerBlanksNumber;

        public static CustomOption airshipOptimizeMap;
        public static CustomOption airshipEnableWallCheck;
        public static CustomOption airshipReactorDuration;
        public static CustomOption airshipRandomSpawn;
        public static CustomOption airshipAdditionalSpawn;
        public static CustomOption airshipSynchronizedSpawning;
        public static CustomOption airshipSetOriginalCooldown;
        public static CustomOption airshipInitialDoorCooldown;
        public static CustomOption airshipInitialSabotageCooldown;
        public static CustomOption airshipOldAdmin;
        public static CustomOption airshipRestrictedAdmin;
        public static CustomOption airshipDisableGapSwitchBoard;
        public static CustomOption airshipDisableMovingPlatform;
        public static CustomOption airshipAdditionalLadder;
        public static CustomOption airshipOneWayLadder;
        public static CustomOption airshipReplaceSafeTask;

        public static CustomOption maxNumberOfMeetings;
        public static CustomOption blockSkippingInEmergencyMeetings;
        public static CustomOption noVoteIsSelfVote;
        public static CustomOption hidePlayerNames;

        public static CustomOption allowParallelMedBayScans;

        public static CustomOption dynamicMap;
        public static CustomOption dynamicMapEnableSkeld;
        public static CustomOption dynamicMapEnableMira;
        public static CustomOption dynamicMapEnablePolus;
        public static CustomOption dynamicMapEnableDleks;
        public static CustomOption dynamicMapEnableAirShip;
        public static CustomOption dynamicMapEnableSubmerged;
        public static CustomOption dynamicMapSeparateSettings;
        public static CustomOption dynamicMapEnableFungle;

        // GM Edition options
        public static CustomRoleOption madmateSpawnRate;
        public static CustomOption madmateCanDieToSheriff;
        public static CustomOption madmateCanEnterVents;
        public static CustomOption madmateHasImpostorVision;
        public static CustomOption madmateCanSabotage;
        public static CustomOption madmateCanFixComm;
        public static CustomOption madmateType;
        public static CustomRoleSelectionOption madmateFixedRole;
        public static CustomOption madmateAbility;
        public static CustomTasksOption madmateTasks;
        public static CustomOption madmateExilePlayer;

        public static CustomRoleOption opportunistSpawnRate;

        public static CustomRoleOption ninjaSpawnRate;
        public static CustomOption ninjaStealthCooldown;
        public static CustomOption ninjaStealthDuration;
        public static CustomOption ninjaKillPenalty;
        public static CustomOption ninjaSpeedBonus;
        public static CustomOption ninjaFadeTime;
        public static CustomOption ninjaCanVent;
        public static CustomOption ninjaCanBeTargeted;

        public static CustomOption gmEnabled;
        public static CustomOption gmIsHost;
        public static CustomOption gmHasTasks;
        public static CustomOption gmDiesAtStart;
        public static CustomOption gmCanWarp;
        public static CustomOption gmCanKill;

        public static CustomRoleOption plagueDoctorSpawnRate;
        public static CustomOption plagueDoctorInfectCooldown;
        public static CustomOption plagueDoctorNumInfections;
        public static CustomOption plagueDoctorDistance;
        public static CustomOption plagueDoctorDuration;
        public static CustomOption plagueDoctorImmunityTime;
        public static CustomOption plagueDoctorInfectKiller;
        public static CustomOption plagueDoctorResetMeeting;
        public static CustomOption plagueDoctorWinDead;

        public static CustomRoleOption nekoKabochaSpawnRate;
        public static CustomOption nekoKabochaRevengeCrew;
        public static CustomOption nekoKabochaRevengeNeutral;
        public static CustomOption nekoKabochaRevengeImpostor;
        public static CustomOption nekoKabochaRevengeExile;

        public static CustomDualRoleOption watcherSpawnRate;

        public static CustomOption hideSettings;
        public static CustomOption restrictDevices;
        public static CustomOption restrictAdmin;
        public static CustomOption restrictAdminTime;
        public static CustomOption restrictAdminText;
        public static CustomOption restrictCameras;
        public static CustomOption restrictCamerasTime;
        public static CustomOption restrictCamerasText;
        public static CustomOption restrictVitals;
        public static CustomOption restrictVitalsTime;
        public static CustomOption restrictVitalsText;

        public static CustomOption hideOutOfSightNametags;
        public static CustomOption refundVotesOnDeath;

        public static CustomOption uselessOptions;
        public static CustomOption playerColorRandom;
        public static CustomOption playerNameDupes;
        public static CustomOption disableVents;

        public static CustomRoleOption serialKillerSpawnRate;
        public static CustomOption serialKillerKillCooldown;
        public static CustomOption serialKillerSuicideTimer;
        public static CustomOption serialKillerResetTimer;
        public static CustomRoleOption foxSpawnRate;
        public static CustomOption foxNumTasks;
        public static CustomOption foxStayTime;
        public static CustomOption foxTaskType;
        public static CustomOption foxCanCreateImmoralist;
        public static CustomOption foxCrewWinsByTasks;
        public static CustomOption foxImpostorWinsBySabotage;
        public static CustomOption foxStealthCooldown;
        public static CustomOption foxStealthDuration;
        public static CustomRoleOption munouSpawnRate;
        public static CustomOption munouType;
        public static CustomOption munouProbability;
        public static CustomOption munouNumShufflePlayers;

        public static CustomOption lastImpostorEnable;
        public static CustomOption lastImpostorNumKills;
        public static CustomOption lastImpostorFunctions;
        public static CustomOption lastImpostorResults;
        public static CustomOption lastImpostorNumShots;

        public static CustomRoleOption schrodingersCatSpawnRate;
        public static CustomOption schrodingersCatKillCooldown;
        public static CustomOption schrodingersCatBecomesImpostor;
        public static CustomOption schrodingersCatKillsKiller;
        public static CustomOption schrodingersCatCantKillUntilLastOne;
        public static CustomOption schrodingersCatBecomesWhichTeamsOnExiled;
        public static CustomOption schrodingersCatJustDieOnKilledByCrew;
        public static CustomOption schrodingersCatHideRole;
        public static CustomOption schrodingersCatCanWinAsCrewmate;
        public static CustomOption schrodingersCatCanChooseImpostor;

        public static CustomRoleOption trapperSpawnRate;
        public static CustomOption trapperNumTrap;
        public static CustomOption trapperKillTimer;
        public static CustomOption trapperCooldown;
        public static CustomOption trapperMaxDistance;
        public static CustomOption trapperTrapRange;
        public static CustomOption trapperExtensionTime;
        public static CustomOption trapperPenaltyTime;
        public static CustomOption trapperBonusTime;
        public static CustomRoleOption bomberSpawnRate;
        public static CustomOption bomberCooldown;
        public static CustomOption bomberDuration;
        public static CustomOption bomberCountAsOne;
        public static CustomOption bomberShowEffects;
        public static CustomOption bomberIfOneDiesBothDie;
        public static CustomOption bomberHasOneVote;
        public static CustomOption bomberAlwaysShowArrow;

        public static CustomRoleOption evilTrackerSpawnRate;
        public static CustomOption evilTrackerCooldown;
        public static CustomOption evilTrackerResetTargetAfterMeeting;
        public static CustomOption evilTrackerCanSeeDeathFlash;
        public static CustomOption evilTrackerCanSeeTargetTask;
        public static CustomOption evilTrackerCanSeeTargetPosition;
        public static CustomOption evilTrackerCanSetTargetOnMeeting;

        public static CustomRoleOption puppeteerSpawnRate;
        public static CustomOption puppeteerNumKills;
        public static CustomOption puppeteerSampleDuration;
        public static CustomOption puppeteerCanControlDummyEvenIfDead;
        public static CustomOption puppeteerPenaltyOnDeath;
        public static CustomOption puppeteerLosesSenriganOnDeath;

        public static CustomRoleOption mimicSpawnRate;
        public static CustomOption mimicCountAsOne;
        public static CustomOption mimicIfOneDiesBothDie;
        public static CustomOption mimicHasOneVote;

        public static CustomRoleOption jekyllAndHydeSpawnRate;
        public static CustomOption jekyllAndHydeNumberToWin;
        public static CustomOption jekyllAndHydeCooldown;
        public static CustomOption jekyllAndHydeSuicideTimer;
        public static CustomOption jekyyllAndHydeResetAfterMeeting;
        public static CustomTasksOption jekyllAndHydeTasks;
        public static CustomOption jekyllAndHydeNumTasks;

        public static CustomRoleOption moriartySpawnRate;
        public static CustomOption moriartyBrainwashTime;
        public static CustomOption moriartyBrainwashCooldown;
        public static CustomOption moriartyNumberToWin;
        public static CustomOption moriartyBrainwashDistance;
        public static CustomOption moriartyKillDistance;
        public static CustomRoleOption cupidSpawnRate;
        public static CustomOption cupidTimeLimit;
        public static CustomOption cupidShield;

        public static CustomOption deadImpsBlockSabotage;
        public static CustomOption anyPlayerCanStopStart;
        public static CustomOption enableEventMode;
        public static CustomOption delayBeforeMeeting;
        public static CustomOption randomWireTask;
        public static CustomOption numWireTask;
        public static CustomOption additionalWireTask;
        public static CustomOption disableVentAnimation;
        public static CustomOption exceptOnTask;
        public static CustomOption additionalEmergencyCooldown;
        public static CustomOption additionalEmergencyCooldownTime;
        public static CustomOption additionalVents;
        public static CustomOption specimenVital;
        public static CustomOption polusRandomSpawn;
        public static CustomOption enableSenrigan;
        public static CustomOption canWinByTaskWithoutLivingPlayer;
        public static CustomOption deadImpostorCanSeeKillColdown;
        public static CustomOption impostorCanIgnoreComms;

        public static CustomRoleOption akujoSpawnRate;
        public static CustomOption akujoTimeLimit;
        public static CustomOption akujoKnowsRoles;
        public static CustomOption akujoNumKeeps;
        public static CustomOption akujoSheriffKillsHonmei;

        public static CustomRoleOption yoyoSpawnRate;
        public static CustomOption yoyoBlinkDuration;
        public static CustomOption yoyoMarkCooldown;
        public static CustomOption yoyoMarkStaysOverMeeting;
        public static CustomOption yoyoHasAdminTable;
        public static CustomOption yoyoAdminTableCooldown;
        public static CustomOption yoyoSilhouetteVisibility;


        internal static Dictionary<byte, byte[]> blockedRolePairings = new();

        public static string cs(Color c, string s)
        {
            return string.Format("<color=#{0:X2}{1:X2}{2:X2}{3:X2}>{4}</color>", ToByte(c.r), ToByte(c.g), ToByte(c.b), ToByte(c.a), s);
        }

        private static byte ToByte(float f)
        {
            f = Mathf.Clamp01(f);
            return (byte)(f * 255);
        }

        public static void Load()
        {
            CustomOption.vanillaSettings = TheOtherRolesPlugin.Instance.Config.Bind("Preset0", "VanillaOptions", "");

            // Role Options
            presetSelection = CustomOption.Create(0, CustomOptionType.General, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "presetSelection"), presets, null, true);

            // Using new id's for the options to not break compatibility with older versions
            crewmateRolesCountMin = CustomOption.Create(300, CustomOptionType.General, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "crewmateRolesCountMin"), 0f, 0f, 15f, 1f, null, true, heading: "headingMinMax");
            crewmateRolesCountMax = CustomOption.Create(301, CustomOptionType.General, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "crewmateRolesCountMax"), 0f, 0f, 15f, 1f);
            neutralRolesCountMin = CustomOption.Create(302, CustomOptionType.General, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "neutralRolesCountMin"), 0f, 0f, 15f, 1f);
            neutralRolesCountMax = CustomOption.Create(303, CustomOptionType.General, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "neutralRolesCountMax"), 0f, 0f, 15f, 1f);
            impostorRolesCountMin = CustomOption.Create(304, CustomOptionType.General, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "impostorRolesCountMin"), 0f, 0f, 15f, 1f);
            impostorRolesCountMax = CustomOption.Create(305, CustomOptionType.General, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "impostorRolesCountMax"), 0f, 0f, 15f, 1f);


            gmEnabled = CustomOption.Create(400, CustomOptionType.General, cs(GM.color, "gm"), false, null, true);
            gmIsHost = CustomOption.Create(401, CustomOptionType.General, "gmIsHost", true, gmEnabled);
            //gmHasTasks = CustomOption.Create(402, "gmHasTasks", false, gmEnabled);
            gmCanWarp = CustomOption.Create(405, CustomOptionType.General, "gmCanWarp", true, gmEnabled);
            gmCanKill = CustomOption.Create(406, CustomOptionType.General, "gmCanKill", false, gmEnabled);
            gmDiesAtStart = CustomOption.Create(404, CustomOptionType.General, "gmDiesAtStart", true, gmEnabled);


            mafiaSpawnRate = new CustomRoleOption(10, CustomOptionType.Impostor, "mafia", Janitor.color, 1);
            mafiosoCanSabotage = CustomOption.Create(12, CustomOptionType.Impostor, "mafiosoCanSabotage", false, mafiaSpawnRate);
            mafiosoCanRepair = CustomOption.Create(13, CustomOptionType.Impostor, "mafiosoCanRepair", false, mafiaSpawnRate);
            mafiosoCanVent = CustomOption.Create(14, CustomOptionType.Impostor, "mafiosoCanVent", false, mafiaSpawnRate);
            janitorCooldown = CustomOption.Create(11, CustomOptionType.Impostor, "janitorCooldown", 30f, 2.5f, 60f, 2.5f, mafiaSpawnRate, format: "unitSeconds");
            janitorCanSabotage = CustomOption.Create(15, CustomOptionType.Impostor, "janitorCanSabotage", false, mafiaSpawnRate);
            janitorCanRepair = CustomOption.Create(16, CustomOptionType.Impostor, "janitorCanRepair", false, mafiaSpawnRate);
            janitorCanVent = CustomOption.Create(17, CustomOptionType.Impostor, "janitorCanVent", false, mafiaSpawnRate);

            morphlingSpawnRate = new CustomRoleOption(20, CustomOptionType.Impostor, "morphling", Morphling.color, 1);
            morphlingCooldown = CustomOption.Create(21, CustomOptionType.Impostor, "morphlingCooldown", 30f, 2.5f, 60f, 2.5f, morphlingSpawnRate, format: "unitSeconds");
            morphlingDuration = CustomOption.Create(22, CustomOptionType.Impostor, "morphlingDuration", 10f, 1f, 20f, 0.5f, morphlingSpawnRate, format: "unitSeconds");

            camouflagerSpawnRate = new CustomRoleOption(30, CustomOptionType.Impostor, "camouflager", Camouflager.color, 1);
            camouflagerCooldown = CustomOption.Create(31, CustomOptionType.Impostor, "camouflagerCooldown", 30f, 2.5f, 60f, 2.5f, camouflagerSpawnRate, format: "unitSeconds");
            camouflagerDuration = CustomOption.Create(32, CustomOptionType.Impostor, "camouflagerDuration", 10f, 1f, 20f, 0.5f, camouflagerSpawnRate, format: "unitSeconds");
            camouflagerRandomColors = CustomOption.Create(33, CustomOptionType.Impostor, "camouflagerRandomColors", false, camouflagerSpawnRate);


            evilHackerSpawnRate = new CustomRoleOption(1900, CustomOptionType.Impostor, "evilHacker", EvilHacker.color, 1);
            evilHackerCanHasBetterAdmin = CustomOption.Create(1912, CustomOptionType.Impostor, "evilHackerCanHasBetterAdmin", false, evilHackerSpawnRate);
            evilHackerCanCreateMadmate = CustomOption.Create(1901, CustomOptionType.Impostor, "evilHackerCanCreateMadmate", false, evilHackerSpawnRate);
            evilHackerCanMoveEvenIfUsesAdmin = CustomOption.Create(1913, CustomOptionType.Impostor, "evilHackerCanMoveEvenIfUsesAdmin", true, evilHackerSpawnRate);
            evilHackerCanInheritAbility = CustomOption.Create(1914, CustomOptionType.Impostor, "evilHackerCanInheritAbility", false, evilHackerSpawnRate);
            evilHackerCanSeeDoorStatus = CustomOption.Create(1915, CustomOptionType.Impostor, "evilHackerCanSeeDoorStatus", true, evilHackerSpawnRate);
            createdMadmateCanDieToSheriff = CustomOption.Create(1902, CustomOptionType.Impostor, "createdMadmateCanDieToSheriff", false, evilHackerCanCreateMadmate);
            createdMadmateCanEnterVents = CustomOption.Create(1903, CustomOptionType.Impostor, "createdMadmateCanEnterVents", false, evilHackerCanCreateMadmate);
            evilHackerCanCreateMadmateFromFox = CustomOption.Create(1904, CustomOptionType.Impostor, "evilHackerCanCreateMadmateFromFox", false, evilHackerCanCreateMadmate);
            evilHackerCanCreateMadmateFromJackal = CustomOption.Create(1905, CustomOptionType.Impostor, "evilHackerCanCreateMadmateFromJackal", false, evilHackerCanCreateMadmate);
            createdMadmateHasImpostorVision = CustomOption.Create(1906, CustomOptionType.Impostor, "createdMadmateHasImpostorVision", false, evilHackerCanCreateMadmate);
            createdMadmateCanSabotage = CustomOption.Create(1907, CustomOptionType.Impostor, "createdMadmateCanSabotage", false, evilHackerCanCreateMadmate);
            createdMadmateCanFixComm = CustomOption.Create(1908, CustomOptionType.Impostor, "createdMadmateCanFixComm", true, evilHackerCanCreateMadmate);
            createdMadmateAbility = CustomOption.Create(1909, CustomOptionType.Impostor, "madmateAbility", new string[] { "madmateNone", "madmateFanatic" }, evilHackerCanCreateMadmate);
            createdMadmateNumTasks = CustomOption.Create(1910, CustomOptionType.Impostor, "createdMadmateNumTasks", 4f, 1f, 20f, 1f, createdMadmateAbility);
            createdMadmateExileCrewmate = CustomOption.Create(1911, CustomOptionType.Impostor, "createdMadmateExileCrewmate", false, evilHackerCanCreateMadmate);

            vampireSpawnRate = new CustomRoleOption(40, CustomOptionType.Impostor, "vampire", Vampire.color, 1);
            vampireKillDelay = CustomOption.Create(41, CustomOptionType.Impostor, "vampireKillDelay", 10f, 1f, 20f, 1f, vampireSpawnRate, format: "unitSeconds");
            vampireCooldown = CustomOption.Create(42, CustomOptionType.Impostor, "vampireCooldown", 30f, 2.5f, 60f, 2.5f, vampireSpawnRate, format: "unitSeconds");
            vampireCanKillNearGarlics = CustomOption.Create(43, CustomOptionType.Impostor, "vampireCanKillNearGarlics", true, vampireSpawnRate);

            eraserSpawnRate = new CustomRoleOption(230, CustomOptionType.Impostor, "eraser", Eraser.color, 1);
            eraserCooldown = CustomOption.Create(231, CustomOptionType.Impostor, "eraserCooldown", 30f, 5f, 120f, 5f, eraserSpawnRate, format: "unitSeconds");
            eraserCooldownIncrease = CustomOption.Create(233, CustomOptionType.Impostor, "eraserCooldownIncrease", 10f, 0f, 120f, 2.5f, eraserSpawnRate, format: "unitSeconds");
            eraserCanEraseAnyone = CustomOption.Create(232, CustomOptionType.Impostor, "eraserCanEraseAnyone", false, eraserSpawnRate);

            tricksterSpawnRate = new CustomRoleOption(250, CustomOptionType.Impostor, "trickster", Trickster.color, 1);
            tricksterPlaceBoxCooldown = CustomOption.Create(251, CustomOptionType.Impostor, "tricksterPlaceBoxCooldown", 10f, 2.5f, 30f, 2.5f, tricksterSpawnRate, format: "unitSeconds");
            tricksterLightsOutCooldown = CustomOption.Create(252, CustomOptionType.Impostor, "tricksterLightsOutCooldown", 30f, 5f, 60f, 5f, tricksterSpawnRate, format: "unitSeconds");
            tricksterLightsOutDuration = CustomOption.Create(253, CustomOptionType.Impostor, "tricksterLightsOutDuration", 15f, 5f, 60f, 2.5f, tricksterSpawnRate, format: "unitSeconds");

            cleanerSpawnRate = new CustomRoleOption(260, CustomOptionType.Impostor, "cleaner", Cleaner.color, 1);
            cleanerCooldown = CustomOption.Create(261, CustomOptionType.Impostor, "cleanerCooldown", 30f, 2.5f, 60f, 2.5f, cleanerSpawnRate, format: "unitSeconds");

            warlockSpawnRate = new CustomRoleOption(270, CustomOptionType.Impostor, "warlock", Warlock.color, 1);
            warlockCooldown = CustomOption.Create(271, CustomOptionType.Impostor, "warlockCooldown", 30f, 2.5f, 60f, 2.5f, warlockSpawnRate, format: "unitSeconds");
            warlockRootTime = CustomOption.Create(272, CustomOptionType.Impostor, "warlockRootTime", 5f, 0f, 15f, 1f, warlockSpawnRate, format: "unitSeconds");

            bountyHunterSpawnRate = new CustomRoleOption(320, CustomOptionType.Impostor, "bountyHunter", BountyHunter.color, 1);
            bountyHunterBountyDuration = CustomOption.Create(321, CustomOptionType.Impostor, "bountyHunterBountyDuration", 60f, 10f, 180f, 10f, bountyHunterSpawnRate, format: "unitSeconds");
            bountyHunterReducedCooldown = CustomOption.Create(322, CustomOptionType.Impostor, "bountyHunterReducedCooldown", 2.5f, 2.5f, 30f, 2.5f, bountyHunterSpawnRate, format: "unitSeconds");
            bountyHunterPunishmentTime = CustomOption.Create(323, CustomOptionType.Impostor, "bountyHunterPunishmentTime", 20f, 0f, 60f, 2.5f, bountyHunterSpawnRate, format: "unitSeconds");
            bountyHunterShowArrow = CustomOption.Create(324, CustomOptionType.Impostor, "bountyHunterShowArrow", true, bountyHunterSpawnRate);
            bountyHunterArrowUpdateInterval = CustomOption.Create(325, CustomOptionType.Impostor, "bountyHunterArrowUpdateInterval", 15f, 2.5f, 60f, 2.5f, bountyHunterShowArrow, format: "unitSeconds");

            witchSpawnRate = new CustomRoleOption(390, CustomOptionType.Impostor, "witch", Witch.color, 1);
            witchCooldown = CustomOption.Create(391, CustomOptionType.Impostor, "witchSpellCooldown", 30f, 2.5f, 120f, 2.5f, witchSpawnRate, format: "unitSeconds");
            witchAdditionalCooldown = CustomOption.Create(392, CustomOptionType.Impostor, "witchAdditionalCooldown", 10f, 0f, 60f, 5f, witchSpawnRate, format: "unitSeconds");
            witchCanSpellAnyone = CustomOption.Create(393, CustomOptionType.Impostor, "witchCanSpellAnyone", false, witchSpawnRate);
            witchSpellCastingDuration = CustomOption.Create(394, CustomOptionType.Impostor, "witchSpellDuration", 1f, 0f, 10f, 1f, witchSpawnRate, format: "unitSeconds");
            witchTriggerBothCooldowns = CustomOption.Create(395, CustomOptionType.Impostor, "witchTriggerBoth", true, witchSpawnRate);
            witchVoteSavesTargets = CustomOption.Create(396, CustomOptionType.Impostor, "witchSaveTargets", true, witchSpawnRate);

            assassinSpawnRate = new CustomRoleOption(410, CustomOptionType.Impostor, "assassin", Assassin.color, 1);
            assassinCooldown = CustomOption.Create(411, CustomOptionType.Impostor, "assassinMarkCooldown", 30f, 10f, 120f, 5f, assassinSpawnRate);
            assassinKnowsTargetLocation = CustomOption.Create(412, CustomOptionType.Impostor, "assassinKnowsLocationOfTarget", true, assassinSpawnRate);
            assassinTraceTime = CustomOption.Create(413, CustomOptionType.Impostor, "assassinTraceDuration", 5f, 1f, 20f, 0.5f, assassinSpawnRate);
            assassinTraceColorTime = CustomOption.Create(414, CustomOptionType.Impostor, "assassinTimeTillTraceColorHasFaded", 2f, 0f, 20f, 0.5f, assassinSpawnRate);

            ninjaSpawnRate = new CustomRoleOption(1000, CustomOptionType.Impostor, "ninja", Ninja.color, 3);
            ninjaStealthCooldown = CustomOption.Create(1002, CustomOptionType.Impostor, "ninjaStealthCooldown", 30f, 2.5f, 60f, 2.5f, ninjaSpawnRate, format: "unitSeconds");
            ninjaStealthDuration = CustomOption.Create(1003, CustomOptionType.Impostor, "ninjaStealthDuration", 15f, 2.5f, 60f, 2.5f, ninjaSpawnRate, format: "unitSeconds");
            ninjaFadeTime = CustomOption.Create(1004, CustomOptionType.Impostor, "ninjaFadeTime", 0.5f, 0.0f, 2.5f, 0.5f, ninjaSpawnRate, format: "unitSeconds");
            ninjaKillPenalty = CustomOption.Create(1005, CustomOptionType.Impostor, "ninjaKillPenalty", 10f, 0f, 60f, 2.5f, ninjaSpawnRate, format: "unitSeconds");
            ninjaSpeedBonus = CustomOption.Create(1006, CustomOptionType.Impostor, "ninjaSpeedBonus", 125f, 50f, 200f, 5f, ninjaSpawnRate, format: "unitPercent");
            ninjaCanBeTargeted = CustomOption.Create(1007, CustomOptionType.Impostor, "ninjaCanBeTargeted", true, ninjaSpawnRate);
            ninjaCanVent = CustomOption.Create(1008, CustomOptionType.Impostor, "ninjaCanVent", false, ninjaSpawnRate);

            serialKillerSpawnRate = new CustomRoleOption(1010, CustomOptionType.Impostor, "serialKiller", SerialKiller.color, 3);
            serialKillerKillCooldown = CustomOption.Create(1012, CustomOptionType.Impostor, "serialKillerKillCooldown", 15f, 2.5f, 60f, 2.5f, serialKillerSpawnRate, format: "unitSeconds");
            serialKillerSuicideTimer = CustomOption.Create(1013, CustomOptionType.Impostor, "serialKillerSuicideTimer", 40f, 2.5f, 60f, 2.5f, serialKillerSpawnRate, format: "unitSeconds");
            serialKillerResetTimer = CustomOption.Create(1014, CustomOptionType.Impostor, "serialKillerResetTimer", true, serialKillerSpawnRate);

            trapperSpawnRate = new CustomRoleOption(1070, CustomOptionType.Impostor, "trapper", Trapper.color, 1);
            trapperNumTrap = CustomOption.Create(1079, CustomOptionType.Impostor, "trapperNumTrap", 2f, 1f, 10f, 1f, trapperSpawnRate);
            trapperExtensionTime = CustomOption.Create(1071, CustomOptionType.Impostor, "trapperExtensionTime", 5f, 2f, 10f, 0.5f, trapperSpawnRate);
            trapperCooldown = CustomOption.Create(1072, CustomOptionType.Impostor, "trapperCooldown", 10f, 2.5f, 60f, 2.5f, trapperSpawnRate);
            trapperKillTimer = CustomOption.Create(1073, CustomOptionType.Impostor, "trapperKillTimer", 5f, 1f, 30f, 1f, trapperSpawnRate);
            trapperTrapRange = CustomOption.Create(1074, CustomOptionType.Impostor, "trapperTrapRange", 1f, 0.5f, 5f, 0.1f, trapperSpawnRate);
            trapperMaxDistance = CustomOption.Create(1076, CustomOptionType.Impostor, "trapperMaxDistance", 10f, 1f, 50f, 1f, trapperSpawnRate);
            trapperPenaltyTime = CustomOption.Create(1077, CustomOptionType.Impostor, "trapperPenaltyTime", 10f, 0f, 50f, 1f, trapperSpawnRate);
            trapperBonusTime = CustomOption.Create(1078, CustomOptionType.Impostor, "trapperBonusTime", 10f, 0f, 50f, 1f, trapperSpawnRate);

            bomberSpawnRate = new CustomRoleOption(1030, CustomOptionType.Impostor, "bomber", BomberA.color, 1);
            bomberCooldown = CustomOption.Create(1031, CustomOptionType.Impostor, "bomberCooldown", 20f, 2f, 60f, 1f, bomberSpawnRate);
            bomberDuration = CustomOption.Create(1032, CustomOptionType.Impostor, "bomberDuration", 2f, 0f, 60f, 0.5f, bomberSpawnRate);
            bomberCountAsOne = CustomOption.Create(1033, CustomOptionType.Impostor, "bomberCountAsOne", true, bomberSpawnRate);
            bomberShowEffects = CustomOption.Create(1034, CustomOptionType.Impostor, "bomberShowEffects", true, bomberSpawnRate);
            bomberIfOneDiesBothDie = CustomOption.Create(1035, CustomOptionType.Impostor, "bomberIfOneDiesBothDie", true, bomberSpawnRate);
            bomberHasOneVote = CustomOption.Create(1036, CustomOptionType.Impostor, "bomberHasOneVote", true, bomberSpawnRate);
            bomberAlwaysShowArrow = CustomOption.Create(1037, CustomOptionType.Impostor, "bomberAlwaysShowArrow", true, bomberSpawnRate);

            evilTrackerSpawnRate = new CustomRoleOption(1050, CustomOptionType.Impostor, "evilTracker", EvilTracker.color, 3);
            evilTrackerCooldown = CustomOption.Create(1052, CustomOptionType.Impostor, "evilTrackerCooldown", 10f, 0f, 60f, 1f, evilTrackerSpawnRate);
            evilTrackerResetTargetAfterMeeting = CustomOption.Create(1053, CustomOptionType.Impostor, "evilTrackerResetTargetAfterMeeting", true, evilTrackerSpawnRate);
            evilTrackerCanSeeDeathFlash = CustomOption.Create(1054, CustomOptionType.Impostor, "evilTrackerCanSeeDeathFlash", true, evilTrackerSpawnRate);
            evilTrackerCanSeeTargetTask = CustomOption.Create(1055, CustomOptionType.Impostor, "evilTrackerCanSeeTargetTask", true, evilTrackerSpawnRate);
            evilTrackerCanSeeTargetPosition = CustomOption.Create(1056, CustomOptionType.Impostor, "evilTrackerCanSeeTargetPosition", true, evilTrackerSpawnRate);
            evilTrackerCanSetTargetOnMeeting = CustomOption.Create(1057, CustomOptionType.Impostor, "evilTrackerCanSetTargetOnMeeting", true, evilTrackerSpawnRate);

            mimicSpawnRate = new CustomRoleOption(1080, CustomOptionType.Impostor, "mimic", MimicK.color, 1);
            mimicCountAsOne = CustomOption.Create(1081, CustomOptionType.Impostor, "mimicCountAsOne", true, mimicSpawnRate);
            mimicIfOneDiesBothDie = CustomOption.Create(1082, CustomOptionType.Impostor, "mimicIfOneDiesBothDie", true, mimicSpawnRate);
            mimicHasOneVote = CustomOption.Create(1083, CustomOptionType.Impostor, "mimicHasOneVote", true, mimicSpawnRate);

            yoyoSpawnRate = new CustomRoleOption(470, CustomOptionType.Impostor, "yoyo", Yoyo.color, 1);
            yoyoBlinkDuration = CustomOption.Create(471, CustomOptionType.Impostor, "yoyoBlinkDuration", 20f, 2.5f, 120f, 2.5f, yoyoSpawnRate);
            yoyoMarkCooldown = CustomOption.Create(472, CustomOptionType.Impostor, "yoyoMarkCooldown", 20f, 2.5f, 120f, 2.5f, yoyoSpawnRate);
            yoyoMarkStaysOverMeeting = CustomOption.Create(473, CustomOptionType.Impostor, "yoyoMarkStaysOverMeeting", true, yoyoSpawnRate);
            yoyoHasAdminTable = CustomOption.Create(474, CustomOptionType.Impostor, "yoyoHasAdminTable", true, yoyoSpawnRate);
            yoyoAdminTableCooldown = CustomOption.Create(475, CustomOptionType.Impostor, "yoyoAdminTableCooldown", 20f, 2.5f, 120f, 2.5f, yoyoHasAdminTable);
            yoyoSilhouetteVisibility = CustomOption.Create(476, CustomOptionType.Impostor, "yoyoSilhouetteVisibility", new string[] { "0%", "10%", "20%", "30%", "40%", "50%" }, yoyoSpawnRate);

            nekoKabochaSpawnRate = new CustomRoleOption(1020, CustomOptionType.Impostor, "nekoKabocha", NekoKabocha.color, 3);
            nekoKabochaRevengeCrew = CustomOption.Create(1021, CustomOptionType.Impostor, "nekoKabochaRevengeCrew", true, nekoKabochaSpawnRate);
            nekoKabochaRevengeNeutral = CustomOption.Create(1022, CustomOptionType.Impostor, "nekoKabochaRevengeNeutral", true, nekoKabochaSpawnRate);
            nekoKabochaRevengeImpostor = CustomOption.Create(1023, CustomOptionType.Impostor, "nekoKabochaRevengeImpostor", true, nekoKabochaSpawnRate);
            nekoKabochaRevengeExile = CustomOption.Create(1024, CustomOptionType.Impostor, "nekoKabochaRevengeExile", false, nekoKabochaSpawnRate);


            madmateSpawnRate = new CustomRoleOption(360, CustomOptionType.Modifier, "madmate", Madmate.color);
            madmateType = CustomOption.Create(366, CustomOptionType.Modifier, "madmateType", new string[] { "madmateDefault", "madmateWithRole", "madmateRandom" }, madmateSpawnRate);
            madmateFixedRole = new CustomRoleSelectionOption(369, CustomOptionType.Modifier, "madmateFixedRole", Madmate.validRoles, madmateType);
            madmateAbility = CustomOption.Create(367, CustomOptionType.Modifier, "madmateAbility", new string[] { "madmateNone", "madmateFanatic" }, madmateSpawnRate);
            madmateTasks = new CustomTasksOption(368, CustomOptionType.Modifier, 1, 1, 3, madmateAbility);
            madmateCanDieToSheriff = CustomOption.Create(361, CustomOptionType.Modifier, "madmateCanDieToSheriff", false, madmateSpawnRate);
            madmateCanEnterVents = CustomOption.Create(362, CustomOptionType.Modifier, "madmateCanEnterVents", false, madmateSpawnRate);
            madmateHasImpostorVision = CustomOption.Create(363, CustomOptionType.Modifier, "madmateHasImpostorVision", false, madmateSpawnRate);
            madmateCanSabotage = CustomOption.Create(364, CustomOptionType.Modifier, "madmateCanSabotage", false, madmateSpawnRate);
            madmateCanFixComm = CustomOption.Create(365, CustomOptionType.Modifier, "madmateCanFixComm", true, madmateSpawnRate);
            madmateExilePlayer = CustomOption.Create(10370, CustomOptionType.Modifier, "madmateExileCrewmate", false, madmateSpawnRate);

            miniSpawnRate = new CustomRoleOption(180, CustomOptionType.Modifier, "mini", Mini.color, 15);
            miniGrowingUpDuration = CustomOption.Create(181, CustomOptionType.Modifier, "miniGrowingUpDuration", 400f, 100f, 1500f, 100f, miniSpawnRate, format: "unitSeconds");

            loversSpawnRate = new CustomRoleOption(50, CustomOptionType.Modifier, "lovers", Lovers.color, 1);
            loversImpLoverRate = CustomOption.Create(51, CustomOptionType.Modifier, "loversImpLoverRate", rates, loversSpawnRate);
            loversNumCouples = CustomOption.Create(57, CustomOptionType.Modifier, "loversNumCouples", 1f, 1f, 7f, 1f, loversSpawnRate, format: "unitCouples");
            loversBothDie = CustomOption.Create(52, CustomOptionType.Modifier, "loversBothDie", true, loversSpawnRate);
            loversCanHaveAnotherRole = CustomOption.Create(53, CustomOptionType.Modifier, "loversCanHaveAnotherRole", true, loversSpawnRate);
            loversSeparateTeam = CustomOption.Create(56, CustomOptionType.Modifier, "loversSeparateTeam", true, loversSpawnRate);
            loversTasksCount = CustomOption.Create(55, CustomOptionType.Modifier, "loversTasksCount", false, loversSpawnRate);
            loversEnableChat = CustomOption.Create(54, CustomOptionType.Modifier, "loversEnableChat", true, loversSpawnRate);

            antiTeleportSpawnRate = new CustomRoleOption(1090, CustomOptionType.Modifier, "antiTeleport", AntiTeleport.color, 15);

            guesserSpawnRate = new CustomRoleOption(310, CustomOptionType.Neutral, "guesser", Guesser.color, 1);
            guesserIsImpGuesserRate = CustomOption.Create(311, CustomOptionType.Neutral, "guesserIsImpGuesserRate", rates, guesserSpawnRate);
            guesserSpawnBothRate = CustomOption.Create(317, CustomOptionType.Neutral, "guesserSpawnBothRate", rates, guesserSpawnRate);
            guesserNumberOfShots = CustomOption.Create(312, CustomOptionType.Neutral, "guesserNumberOfShots", 2f, 1f, 15f, 1f, guesserSpawnRate, format: "unitShots");
            guesserOnlyAvailableRoles = CustomOption.Create(313, CustomOptionType.Neutral, "guesserOnlyAvailableRoles", true, guesserSpawnRate);
            guesserHasMultipleShotsPerMeeting = CustomOption.Create(314, CustomOptionType.Neutral, "guesserHasMultipleShotsPerMeeting", false, guesserSpawnRate);
            guesserShowInfoInGhostChat = CustomOption.Create(315, CustomOptionType.Neutral, "guesserToGhostChat", true, guesserSpawnRate);
            guesserKillsThroughShield = CustomOption.Create(316, CustomOptionType.Neutral, "guesserPierceShield", true, guesserSpawnRate);
            guesserEvilCanKillSpy = CustomOption.Create(318, CustomOptionType.Neutral, "guesserEvilCanKillSpy", true, guesserSpawnRate);
            guesserCantGuessSnitchIfTaskDone = CustomOption.Create(319, CustomOptionType.Neutral, "guesserCantGuessSnitchIfTaskDone", true, guesserSpawnRate);

            swapperSpawnRate = new CustomRoleOption(150, CustomOptionType.Neutral, "swapper", Swapper.color, 1);
            swapperIsImpRate = CustomOption.Create(153, CustomOptionType.Neutral, "swapperIsImpRate", rates, swapperSpawnRate);
            swapperNumSwaps = CustomOption.Create(154, CustomOptionType.Neutral, "swapperNumSwaps", 2f, 1f, 15f, 1f, swapperSpawnRate, format: "unitTimes");
            swapperCanCallEmergency = CustomOption.Create(151, CustomOptionType.Neutral, "swapperCanCallEmergency", false, swapperSpawnRate);
            swapperCanOnlySwapOthers = CustomOption.Create(152, CustomOptionType.Neutral, "swapperCanOnlySwapOthers", false, swapperSpawnRate);

            jesterSpawnRate = new CustomRoleOption(60, CustomOptionType.Neutral, "jester", Jester.color, 1);
            jesterCanCallEmergency = CustomOption.Create(61, CustomOptionType.Neutral, "jesterCanCallEmergency", true, jesterSpawnRate);
            jesterCanSabotage = CustomOption.Create(62, CustomOptionType.Neutral, "jesterCanSabotage", true, jesterSpawnRate);
            jesterHasImpostorVision = CustomOption.Create(63, CustomOptionType.Neutral, "jesterHasImpostorVision", false, jesterSpawnRate);

            arsonistSpawnRate = new CustomRoleOption(290, CustomOptionType.Neutral, "arsonist", Arsonist.color, 1);
            arsonistCooldown = CustomOption.Create(291, CustomOptionType.Neutral, "arsonistCooldown", 12.5f, 2.5f, 60f, 2.5f, arsonistSpawnRate, format: "unitSeconds");
            arsonistDuration = CustomOption.Create(292, CustomOptionType.Neutral, "arsonistDuration", 3f, 0f, 10f, 1f, arsonistSpawnRate, format: "unitSeconds");
            arsonistCanBeLovers = CustomOption.Create(293, CustomOptionType.Neutral, "arsonistCanBeLovers", false, arsonistSpawnRate);

            opportunistSpawnRate = new CustomRoleOption(380, CustomOptionType.Neutral, "opportunist", Opportunist.color);

            jackalSpawnRate = new CustomRoleOption(220, CustomOptionType.Neutral, "jackal", Jackal.color, 1);
            jackalKillCooldown = CustomOption.Create(221, CustomOptionType.Neutral, "jackalKillCooldown", 30f, 2.5f, 60f, 2.5f, jackalSpawnRate, format: "unitSeconds");
            jackalCanUseVents = CustomOption.Create(223, CustomOptionType.Neutral, "jackalCanUseVents", true, jackalSpawnRate);
            jackalAndSidekickHaveImpostorVision = CustomOption.Create(430, CustomOptionType.Neutral, "jackalAndSidekickHaveImpostorVision", false, jackalSpawnRate);
            jackalCanCreateSidekick = CustomOption.Create(224, CustomOptionType.Neutral, "jackalCanCreateSidekick", false, jackalSpawnRate);
            jackalCreateSidekickCooldown = CustomOption.Create(222, CustomOptionType.Neutral, "jackalCreateSidekickCooldown", 30f, 2.5f, 60f, 2.5f, jackalCanCreateSidekick, format: "unitSeconds");
            sidekickPromotesToJackal = CustomOption.Create(225, CustomOptionType.Neutral, "sidekickPromotesToJackal", false, jackalCanCreateSidekick);
            sidekickCanKill = CustomOption.Create(226, CustomOptionType.Neutral, "sidekickCanKill", false, jackalCanCreateSidekick);
            sidekickCanUseVents = CustomOption.Create(227, CustomOptionType.Neutral, "sidekickCanUseVents", true, jackalCanCreateSidekick);
            jackalPromotedFromSidekickCanCreateSidekick = CustomOption.Create(228, CustomOptionType.Neutral, "jackalPromotedFromSidekickCanCreateSidekick", true, jackalCanCreateSidekick);
            jackalCanCreateSidekickFromImpostor = CustomOption.Create(229, CustomOptionType.Neutral, "jackalCanCreateSidekickFromImpostor", true, jackalCanCreateSidekick);
            jackalCanCreateSidekickFromFox = CustomOption.Create(431, CustomOptionType.Neutral, "jackalCanCreateSidekickFromFox", true, jackalCanCreateSidekick);

            vultureSpawnRate = new CustomRoleOption(340, CustomOptionType.Neutral, "vulture", Vulture.color, 1);
            vultureCooldown = CustomOption.Create(341, CustomOptionType.Neutral, "vultureCooldown", 15f, 2.5f, 60f, 2.5f, vultureSpawnRate, format: "unitSeconds");
            vultureNumberToWin = CustomOption.Create(342, CustomOptionType.Neutral, "vultureNumberToWin", 4f, 1f, 12f, 1f, vultureSpawnRate);
            vultureCanUseVents = CustomOption.Create(343, CustomOptionType.Neutral, "vultureCanUseVents", true, vultureSpawnRate);
            vultureShowArrows = CustomOption.Create(344, CustomOptionType.Neutral, "vultureShowArrows", true, vultureSpawnRate);

            lawyerSpawnRate = new CustomRoleOption(350, CustomOptionType.Neutral, "lawyer", Lawyer.color, 1);
            lawyerTargetKnows = CustomOption.Create(351, CustomOptionType.Neutral, "lawyerTargetKnows", true, lawyerSpawnRate);
            lawyerWinsAfterMeetings = CustomOption.Create(352, CustomOptionType.Neutral, "lawyerWinsMeeting", false, lawyerSpawnRate);
            lawyerNeededMeetings = CustomOption.Create(353, CustomOptionType.Neutral, "lawyerMeetingsNeeded", 5f, 1f, 15f, 1f, lawyerWinsAfterMeetings);
            lawyerVision = CustomOption.Create(354, CustomOptionType.Neutral, "lawyerVision", 1f, 0.25f, 3f, 0.25f, lawyerSpawnRate, format: "unitMultiplier");
            lawyerKnowsRole = CustomOption.Create(355, CustomOptionType.Neutral, "lawyerKnowsRole", false, lawyerSpawnRate);
            pursuerCooldown = CustomOption.Create(356, CustomOptionType.Neutral, "pursuerBlankCool", 30f, 2.5f, 60f, 2.5f, lawyerSpawnRate, format: "unitSeconds");
            pursuerBlanksNumber = CustomOption.Create(357, CustomOptionType.Neutral, "pursuerNumBlanks", 5f, 0f, 20f, 1f, lawyerSpawnRate, format: "unitShots");

            shifterSpawnRate = new CustomRoleOption(70, CustomOptionType.Neutral, "shifter", Shifter.color, 1);
            shifterIsNeutralRate = CustomOption.Create(72, CustomOptionType.Neutral, "shifterIsNeutralRate", rates, shifterSpawnRate);
            shifterShiftsModifiers = CustomOption.Create(71, CustomOptionType.Neutral, "shifterShiftsModifiers", false, shifterSpawnRate);
            shifterPastShifters = CustomOption.Create(73, CustomOptionType.Neutral, "shifterPastShifters", false, shifterSpawnRate);

            plagueDoctorSpawnRate = new CustomRoleOption(900, CustomOptionType.Neutral, "plagueDoctor", PlagueDoctor.color, 1);
            plagueDoctorInfectCooldown = CustomOption.Create(901, CustomOptionType.Neutral, "plagueDoctorInfectCooldown", 10f, 2.5f, 60f, 2.5f, plagueDoctorSpawnRate, format: "unitSeconds");
            plagueDoctorNumInfections = CustomOption.Create(902, CustomOptionType.Neutral, "plagueDoctorNumInfections", 1f, 1f, 15, 1f, plagueDoctorSpawnRate, format: "unitPlayers");
            plagueDoctorDistance = CustomOption.Create(903, CustomOptionType.Neutral, "plagueDoctorDistance", 1.0f, 0.25f, 5.0f, 0.25f, plagueDoctorSpawnRate, format: "unitMeters");
            plagueDoctorDuration = CustomOption.Create(904, CustomOptionType.Neutral, "plagueDoctorDuration", 5f, 1f, 30f, 1f, plagueDoctorSpawnRate, format: "unitSeconds");
            plagueDoctorImmunityTime = CustomOption.Create(905, CustomOptionType.Neutral, "plagueDoctorImmunityTime", 10f, 1f, 30f, 1f, plagueDoctorSpawnRate, format: "unitSeconds");
            //plagueDoctorResetMeeting = CustomOption.Create(907, "plagueDoctorResetMeeting", false, plagueDoctorSpawnRate);
            plagueDoctorInfectKiller = CustomOption.Create(906, CustomOptionType.Neutral, "plagueDoctorInfectKiller", true, plagueDoctorSpawnRate);
            plagueDoctorWinDead = CustomOption.Create(908, CustomOptionType.Neutral, "plagueDoctorWinDead", true, plagueDoctorSpawnRate);


            watcherSpawnRate = new CustomDualRoleOption(1040, CustomOptionType.Neutral, "watcher", Watcher.color, RoleType.Watcher, 15);

            akujoSpawnRate = new CustomRoleOption(1120, CustomOptionType.Neutral, "akujo", Akujo.color, 7, roleEnabled: true);
            akujoTimeLimit = CustomOption.Create(1121, CustomOptionType.Neutral, "akujoTimeLimit", 300f, 30f, 1200f, 30f, akujoSpawnRate, format: "unitSeconds");
            akujoKnowsRoles = CustomOption.Create(1122, CustomOptionType.Neutral, "akujoKnowsRoles", false, akujoSpawnRate);
            akujoNumKeeps = CustomOption.Create(1113, CustomOptionType.Neutral, "akujoNumKeeps", 1f, 1f, 15f, 1f, akujoSpawnRate, format: "unitPlayers");
            akujoSheriffKillsHonmei = CustomOption.Create(1114, CustomOptionType.Neutral, "akujoSheriffKillsHonmei", true, akujoSpawnRate);


            foxSpawnRate = new CustomRoleOption(910, CustomOptionType.Neutral, "fox", Fox.color, 1);
            foxNumTasks = CustomOption.Create(911, CustomOptionType.Neutral, "foxNumTasks", 4f, 1f, 10f, 1f, foxSpawnRate);
            foxStayTime = CustomOption.Create(913, CustomOptionType.Neutral, "foxStayTime", 5f, 1f, 20f, 1f, foxSpawnRate);
            foxTaskType = CustomOption.Create(914, CustomOptionType.Neutral, "foxTaskType", new string[] { "foxTaskSerial", "foxTaskParallel" }, foxSpawnRate);
            foxCrewWinsByTasks = CustomOption.Create(912, CustomOptionType.Neutral, "foxCrewWinsByTasks", true, foxSpawnRate);
            foxImpostorWinsBySabotage = CustomOption.Create(919, CustomOptionType.Neutral, "foxImpostorWinsBySabotage", true, foxSpawnRate);
            foxStealthCooldown = CustomOption.Create(916, CustomOptionType.Neutral, "foxStealthCooldown", 15f, 1f, 30f, 1f, foxSpawnRate, format: "unitSeconds");
            foxStealthDuration = CustomOption.Create(917, CustomOptionType.Neutral, "foxStealthDuration", 15f, 1f, 30f, 1f, foxSpawnRate, format: "unitSeconds");
            foxCanCreateImmoralist = CustomOption.Create(918, CustomOptionType.Neutral, "foxCanCreateImmoralist", true, foxSpawnRate);


            fortuneTellerSpawnRate = new CustomRoleOption(940, CustomOptionType.Crewmate, "fortuneTeller", FortuneTeller.color, 15);
            fortuneTellerNumTasks = CustomOption.Create(941, CustomOptionType.Crewmate, "fortuneTellerNumTasks", 4f, 0f, 25f, 1f, fortuneTellerSpawnRate);
            fortuneTellerResults = CustomOption.Create(942, CustomOptionType.Crewmate, "fortuneTellerResults ", new string[] { "fortuneTellerResultCrew", "fortuneTellerResultTeam", "fortuneTellerResultRole" }, fortuneTellerSpawnRate);
            fortuneTellerDuration = CustomOption.Create(943, CustomOptionType.Crewmate, "fortuneTellerDuration ", 20f, 1f, 50f, 0.5f, fortuneTellerSpawnRate, format: "unitSeconds");
            fortuneTellerDistance = CustomOption.Create(944, CustomOptionType.Crewmate, "fortuneTellerDistance ", 2.5f, 1f, 10f, 0.5f, fortuneTellerSpawnRate, format: "unitMeters");

            schrodingersCatSpawnRate = new CustomRoleOption(970, CustomOptionType.Neutral, "schrodingersCat", SchrodingersCat.color, 1);
            schrodingersCatKillCooldown = CustomOption.Create(971, CustomOptionType.Neutral, "schrodingersCatKillCooldown", 20f, 1f, 60f, 0.5f, schrodingersCatSpawnRate);
            schrodingersCatBecomesImpostor = CustomOption.Create(972, CustomOptionType.Neutral, "schrodingersCatBecomesImpostor", true, schrodingersCatSpawnRate);
            schrodingersCatKillsKiller = CustomOption.Create(973, CustomOptionType.Neutral, "schrodingersCatKillsKiller", false, schrodingersCatSpawnRate);
            schrodingersCatCantKillUntilLastOne = CustomOption.Create(974, CustomOptionType.Neutral, "schrodingersCatCantKillUntilLastOne", false, schrodingersCatSpawnRate);
            schrodingersCatBecomesWhichTeamsOnExiled = CustomOption.Create(975, CustomOptionType.Neutral, "schrodingersCatBecomesWhichTeamsOnExiled", new string[] { "schrodingersCatNone", "schrodingersCatCrew", "schrodingersCatRandom" }, schrodingersCatSpawnRate);
            schrodingersCatJustDieOnKilledByCrew = CustomOption.Create(976, CustomOptionType.Neutral, "schrodingersCatJustDieOnKilledByCrew", false, schrodingersCatSpawnRate);
            schrodingersCatHideRole = CustomOption.Create(977, CustomOptionType.Neutral, "schrodingersCatHideRole", false, schrodingersCatSpawnRate);
            schrodingersCatCanWinAsCrewmate = CustomOption.Create(978, CustomOptionType.Neutral, "schrodingersCatCanWinAsCrewmate", false, schrodingersCatHideRole);
            schrodingersCatCanChooseImpostor = CustomOption.Create(979, CustomOptionType.Neutral, "schrodingersCatCanChooseTeam", false, schrodingersCatHideRole);

            puppeteerSpawnRate = new CustomRoleOption(1060, CustomOptionType.Neutral, "puppeteer", Puppeteer.color, 1);
            puppeteerNumKills = CustomOption.Create(1061, CustomOptionType.Neutral, "puppeteerNumKills", 3f, 1f, 15f, 1f, puppeteerSpawnRate);
            puppeteerSampleDuration = CustomOption.Create(1062, CustomOptionType.Neutral, "puppeteerSampleDuration", 1f, 0f, 20f, 0.25f, puppeteerSpawnRate);
            puppeteerCanControlDummyEvenIfDead = CustomOption.Create(1063, CustomOptionType.Neutral, "puppeteerCanControlDummyEvenIfDead", true, puppeteerSpawnRate);
            puppeteerPenaltyOnDeath = CustomOption.Create(1064, CustomOptionType.Neutral, "puppeteerPenaltyOnDeath", 1f, 0f, 5f, 1f, puppeteerCanControlDummyEvenIfDead);
            puppeteerLosesSenriganOnDeath = CustomOption.Create(1065, CustomOptionType.Neutral, "puppeteerLosesSenriganOnDeath", true, puppeteerCanControlDummyEvenIfDead);

            jekyllAndHydeSpawnRate = new CustomRoleOption(1100, CustomOptionType.Neutral, "jekyllAndHyde", JekyllAndHyde.color, 1);
            jekyllAndHydeNumberToWin = CustomOption.Create(1101, CustomOptionType.Neutral, "jekyllAndHydeNumberToWin", 3f, 1f, 10f, 1f, jekyllAndHydeSpawnRate);
            jekyllAndHydeCooldown = CustomOption.Create(1103, CustomOptionType.Neutral, "jekyllAndHydeCooldown", 17.5f, 0f, 30f, 2.5f, jekyllAndHydeSpawnRate);
            jekyllAndHydeSuicideTimer = CustomOption.Create(1104, CustomOptionType.Neutral, "jekyllAndHydeSuicideTimer", 40f, 10f, 90f, 2.5f, jekyllAndHydeSpawnRate);
            jekyyllAndHydeResetAfterMeeting = CustomOption.Create(1105, CustomOptionType.Neutral, "jekyllAndHydeResetAfterMeeting", true, jekyllAndHydeSpawnRate);
            jekyllAndHydeTasks = new CustomTasksOption(1106, CustomOptionType.Neutral, 1, 2, 3, jekyllAndHydeSpawnRate);
            jekyllAndHydeNumTasks = CustomOption.Create(1107, CustomOptionType.Neutral, "jekyllAndHydeNumTasks", 3f, 1f, 10f, 1f, jekyllAndHydeSpawnRate);

            moriartySpawnRate = new CustomRoleOption(1130, CustomOptionType.Neutral, "moriarty", Moriarty.color, 1);
            moriartyBrainwashCooldown = CustomOption.Create(1131, CustomOptionType.Neutral, "moriartyBrainwashCooldown", 0f, 0f, 20f, 0.25f, moriartySpawnRate);
            moriartyBrainwashTime = CustomOption.Create(1132, CustomOptionType.Neutral, "moriartyBrainwashTime", 30f, 1f, 60f, 1f, moriartySpawnRate);
            moriartyNumberToWin = CustomOption.Create(1134, CustomOptionType.Neutral, "moriartyNumberToWin", 2f, 0f, 10f, 1f, moriartySpawnRate);
            moriartyBrainwashDistance = CustomOption.Create(1135, CustomOptionType.Neutral, "moriartyBrainwashDistance", new string[] { "short", "medium", "long" }, moriartySpawnRate);
            moriartyKillDistance = CustomOption.Create(1136, CustomOptionType.Neutral, "moriartyKillDistance", new string[] { "short", "medium", "long" }, moriartySpawnRate);

            sherlockSpawnRate = new CustomRoleOption(1140, CustomOptionType.Crewmate, "sherlock", Sherlock.color, 15);
            sherlockRechargeTasksNumber = CustomOption.Create(1141, CustomOptionType.Crewmate, "sherlockRechargeTasksNumber", 4f, 1f, 15f, 1f, sherlockSpawnRate);
            sherlockCooldown = CustomOption.Create(1142, CustomOptionType.Crewmate, "sherlockCooldown", 10f, 0f, 40f, 2.5f, sherlockSpawnRate);
            sherlockInvestigateDistance = CustomOption.Create(1143, CustomOptionType.Crewmate, "sherlockInvestigateDistance", 5f, 1f, 15f, 1f, sherlockSpawnRate);

            cupidSpawnRate = new CustomRoleOption(1150, CustomOptionType.Neutral, "cupid", Cupid.color, 1);
            cupidTimeLimit = CustomOption.Create(1151, CustomOptionType.Neutral, "cupidTimeLimit", 300f, 30f, 1200f, 30f, cupidSpawnRate, format: "unitSeconds");
            cupidShield = CustomOption.Create(1152, CustomOptionType.Neutral, "cupidShield", true, cupidSpawnRate);

            munouSpawnRate = new CustomRoleOption(960, CustomOptionType.Modifier, "incompetent", Munou.color, 15);
            munouType = CustomOption.Create(963, CustomOptionType.Modifier, "incompetentType", new string[] { "incompetentSimple", "incompetentRandom" }, munouSpawnRate);
            munouProbability = CustomOption.Create(961, CustomOptionType.Modifier, "incompetentProbability", 60f, 0f, 100f, 10f, munouSpawnRate);
            munouNumShufflePlayers = CustomOption.Create(962, CustomOptionType.Modifier, "incompetentNumShufflePlayers", 4f, 2f, 15f, 1f, munouSpawnRate);

            mayorSpawnRate = new CustomRoleOption(80, CustomOptionType.Crewmate, "mayor", Mayor.color, 1);
            mayorNumVotes = CustomOption.Create(81, CustomOptionType.Crewmate, "mayorNumVotes", 2f, 2f, 10f, 1f, mayorSpawnRate, format: "unitVotes");

            engineerSpawnRate = new CustomRoleOption(90, CustomOptionType.Crewmate, "engineer", Engineer.color, 1);
            engineerNumberOfFixes = CustomOption.Create(91, CustomOptionType.Crewmate, "engineerNumFixes", 1f, 0f, 3f, 1f, engineerSpawnRate);
            engineerHighlightForImpostors = CustomOption.Create(92, CustomOptionType.Crewmate, "engineerImpostorsSeeVent", true, engineerSpawnRate);
            engineerHighlightForTeamJackal = CustomOption.Create(93, CustomOptionType.Crewmate, "engineerJackalSeeVent", true, engineerSpawnRate);

            sheriffSpawnRate = new CustomRoleOption(100, CustomOptionType.Crewmate, "sheriff", Sheriff.color, 15);
            sheriffCooldown = CustomOption.Create(101, CustomOptionType.Crewmate, "sheriffCooldown", 30f, 2.5f, 60f, 2.5f, sheriffSpawnRate, format: "unitSeconds");
            sheriffNumShots = CustomOption.Create(103, CustomOptionType.Crewmate, "sheriffNumShots", 2f, 1f, 15f, 1f, sheriffSpawnRate, format: "unitShots");
            sheriffMisfireKillsTarget = CustomOption.Create(104, CustomOptionType.Crewmate, "sheriffMisfireKillsTarget", false, sheriffSpawnRate);
            sheriffCanKillNoDeadBody = CustomOption.Create(105, CustomOptionType.Crewmate, "sheriffCanKillNoDeadBody", true, sheriffSpawnRate);
            sheriffCanKillNeutrals = CustomOption.Create(102, CustomOptionType.Crewmate, "sheriffCanKillNeutrals", false, sheriffSpawnRate);


            lighterSpawnRate = new CustomRoleOption(110, CustomOptionType.Crewmate, "lighter", Lighter.color, 15);
            lighterModeLightsOnVision = CustomOption.Create(111, CustomOptionType.Crewmate, "lighterModeLightsOnVision", 2f, 0.25f, 5f, 0.25f, lighterSpawnRate, format: "unitMultiplier");
            lighterModeLightsOffVision = CustomOption.Create(112, CustomOptionType.Crewmate, "lighterModeLightsOffVision", 0.75f, 0.25f, 5f, 0.25f, lighterSpawnRate, format: "unitMultiplier");
            lighterCooldown = CustomOption.Create(113, CustomOptionType.Crewmate, "lighterCooldown", 30f, 5f, 120f, 5f, lighterSpawnRate, format: "unitSeconds");
            lighterDuration = CustomOption.Create(114, CustomOptionType.Crewmate, "lighterDuration", 5f, 2.5f, 60f, 2.5f, lighterSpawnRate, format: "unitSeconds");
            lighterCanSeeNinja = CustomOption.Create(115, CustomOptionType.Crewmate, "lighterCanSeeNinja", true, lighterSpawnRate);

            detectiveSpawnRate = new CustomRoleOption(120, CustomOptionType.Crewmate, "detective", Detective.color, 1);
            detectiveAnonymousFootprints = CustomOption.Create(121, CustomOptionType.Crewmate, "detectiveAnonymousFootprints", false, detectiveSpawnRate);
            detectiveFootprintInterval = CustomOption.Create(122, CustomOptionType.Crewmate, "detectiveFootprintInterval", 0.5f, 0.25f, 10f, 0.25f, detectiveSpawnRate, format: "unitSeconds");
            detectiveFootprintDuration = CustomOption.Create(123, CustomOptionType.Crewmate, "detectiveFootprintDuration", 5f, 0.25f, 10f, 0.25f, detectiveSpawnRate, format: "unitSeconds");
            detectiveReportNameDuration = CustomOption.Create(124, CustomOptionType.Crewmate, "detectiveReportNameDuration", 0, 0, 60, 2.5f, detectiveSpawnRate, format: "unitSeconds");
            detectiveReportColorDuration = CustomOption.Create(125, CustomOptionType.Crewmate, "detectiveReportColorDuration", 20, 0, 120, 2.5f, detectiveSpawnRate, format: "unitSeconds");

            timeMasterSpawnRate = new CustomRoleOption(130, CustomOptionType.Crewmate, "timeMaster", TimeMaster.color, 1);
            timeMasterCooldown = CustomOption.Create(131, CustomOptionType.Crewmate, "timeMasterCooldown", 30f, 2.5f, 120f, 2.5f, timeMasterSpawnRate, format: "unitSeconds");
            timeMasterRewindTime = CustomOption.Create(132, CustomOptionType.Crewmate, "timeMasterRewindTime", 3f, 1f, 10f, 1f, timeMasterSpawnRate, format: "unitSeconds");
            timeMasterShieldDuration = CustomOption.Create(133, CustomOptionType.Crewmate, "timeMasterShieldDuration", 3f, 1f, 20f, 1f, timeMasterSpawnRate, format: "unitSeconds");

            medicSpawnRate = new CustomRoleOption(140, CustomOptionType.Crewmate, "medic", Medic.color, 1);
            medicShowShielded = CustomOption.Create(143, CustomOptionType.Crewmate, "medicShowShielded", new string[] { "medicShowShieldedAll", "medicShowShieldedBoth", "medicShowShieldedMedic" }, medicSpawnRate);
            medicShowAttemptToShielded = CustomOption.Create(144, CustomOptionType.Crewmate, "medicShowAttemptToShielded", false, medicSpawnRate);
            medicSetShieldAfterMeeting = CustomOption.Create(145, CustomOptionType.Crewmate, "medicSetShieldAfterMeeting", false, medicSpawnRate);
            medicShowAttemptToMedic = CustomOption.Create(146, CustomOptionType.Crewmate, "medicSeesMurderAttempt", false, medicSpawnRate);

            seerSpawnRate = new CustomRoleOption(160, CustomOptionType.Crewmate, "seer", Seer.color, 1);
            seerMode = CustomOption.Create(161, CustomOptionType.Crewmate, "seerMode", new string[] { "seerModeBoth", "seerModeFlash", "seerModeSouls" }, seerSpawnRate);
            seerLimitSoulDuration = CustomOption.Create(163, CustomOptionType.Crewmate, "seerLimitSoulDuration", false, seerSpawnRate);
            seerSoulDuration = CustomOption.Create(162, CustomOptionType.Crewmate, "seerSoulDuration", 15f, 0f, 120f, 5f, seerLimitSoulDuration, format: "unitSeconds");

            hackerSpawnRate = new CustomRoleOption(170, CustomOptionType.Crewmate, "hacker", Hacker.color, 1);
            hackerCooldown = CustomOption.Create(171, CustomOptionType.Crewmate, "hackerCooldown", 30f, 5f, 60f, 5f, hackerSpawnRate, format: "unitSeconds");
            hackerHackeringDuration = CustomOption.Create(172, CustomOptionType.Crewmate, "hackerHackeringDuration", 10f, 2.5f, 60f, 2.5f, hackerSpawnRate, format: "unitSeconds");
            hackerOnlyColorType = CustomOption.Create(173, CustomOptionType.Crewmate, "hackerOnlyColorType", false, hackerSpawnRate);
            hackerToolsNumber = CustomOption.Create(174, CustomOptionType.Crewmate, "hackerToolsNumber", 5f, 1f, 30f, 1f, hackerSpawnRate);
            hackerRechargeTasksNumber = CustomOption.Create(175, CustomOptionType.Crewmate, "hackerRechargeTasksNumber", 2f, 1f, 5f, 1f, hackerSpawnRate);
            hackerNoMove = CustomOption.Create(176, CustomOptionType.Crewmate, "hackerNoMove", true, hackerSpawnRate);

            trackerSpawnRate = new CustomRoleOption(200, CustomOptionType.Crewmate, "tracker", Tracker.color, 1);
            trackerUpdateInterval = CustomOption.Create(201, CustomOptionType.Crewmate, "trackerUpdateInterval", 5f, 1f, 30f, 1f, trackerSpawnRate);
            trackerResetTargetAfterMeeting = CustomOption.Create(202, CustomOptionType.Crewmate, "trackerResetTargetAfterMeeting", false, trackerSpawnRate);
            trackerCanTrackCorpses = CustomOption.Create(203, CustomOptionType.Crewmate, "trackerTrackCorpses", true, trackerSpawnRate);
            trackerCorpsesTrackingCooldown = CustomOption.Create(204, CustomOptionType.Crewmate, "trackerCorpseCooldown", 30f, 0f, 120f, 5f, trackerCanTrackCorpses, format: "unitSeconds");
            trackerCorpsesTrackingDuration = CustomOption.Create(205, CustomOptionType.Crewmate, "trackerCorpseDuration", 5f, 2.5f, 30f, 2.5f, trackerCanTrackCorpses, format: "unitSeconds");

            snitchSpawnRate = new CustomRoleOption(210, CustomOptionType.Crewmate, "snitch", Snitch.color, 1);
            snitchLeftTasksForReveal = CustomOption.Create(211, CustomOptionType.Crewmate, "snitchLeftTasksForReveal", 1f, 0f, 5f, 1f, snitchSpawnRate);
            snitchIncludeTeamJackal = CustomOption.Create(212, CustomOptionType.Crewmate, "snitchIncludeTeamJackal", false, snitchSpawnRate);
            snitchTeamJackalUseDifferentArrowColor = CustomOption.Create(213, CustomOptionType.Crewmate, "snitchTeamJackalUseDifferentArrowColor", true, snitchIncludeTeamJackal);

            spySpawnRate = new CustomRoleOption(240, CustomOptionType.Crewmate, "spy", Spy.color, 1);
            spyCanDieToSheriff = CustomOption.Create(241, CustomOptionType.Crewmate, "spyCanDieToSheriff", false, spySpawnRate);
            spyImpostorsCanKillAnyone = CustomOption.Create(242, CustomOptionType.Crewmate, "spyImpostorsCanKillAnyone", true, spySpawnRate);
            spyCanEnterVents = CustomOption.Create(243, CustomOptionType.Crewmate, "spyCanEnterVents", false, spySpawnRate);
            spyHasImpostorVision = CustomOption.Create(244, CustomOptionType.Crewmate, "spyHasImpostorVision", false, spySpawnRate);

            securityGuardSpawnRate = new CustomRoleOption(280, CustomOptionType.Crewmate, "securityGuard", SecurityGuard.color, 1);
            securityGuardCooldown = CustomOption.Create(281, CustomOptionType.Crewmate, "securityGuardCooldown", 30f, 2.5f, 60f, 2.5f, securityGuardSpawnRate, format: "unitSeconds");
            securityGuardTotalScrews = CustomOption.Create(282, CustomOptionType.Crewmate, "securityGuardTotalScrews", 7f, 1f, 15f, 1f, securityGuardSpawnRate, format: "unitScrews");
            securityGuardCamPrice = CustomOption.Create(283, CustomOptionType.Crewmate, "securityGuardCamPrice", 2f, 1f, 15f, 1f, securityGuardSpawnRate, format: "unitScrews");
            securityGuardVentPrice = CustomOption.Create(284, CustomOptionType.Crewmate, "securityGuardVentPrice", 1f, 1f, 15f, 1f, securityGuardSpawnRate, format: "unitScrews");
            securityGuardCamDuration = CustomOption.Create(285, CustomOptionType.Crewmate, "securityGuardCamDuration", 10f, 2.5f, 60f, 2.5f, securityGuardSpawnRate, format: "unitSeconds");
            securityGuardCamMaxCharges = CustomOption.Create(286, CustomOptionType.Crewmate, "securityGuardCamMaxCharges", 5f, 1f, 30f, 1f, securityGuardSpawnRate);
            securityGuardCamRechargeTasksNumber = CustomOption.Create(287, CustomOptionType.Crewmate, "securityGuardCamRechargeTasksNumber", 3f, 1f, 10f, 1f, securityGuardSpawnRate);
            securityGuardNoMove = CustomOption.Create(288, CustomOptionType.Crewmate, "securityGuardNoMove", true, securityGuardSpawnRate);

            baitSpawnRate = new CustomRoleOption(330, CustomOptionType.Crewmate, "bait", Bait.color, 1);
            baitHighlightAllVents = CustomOption.Create(331, CustomOptionType.Crewmate, "baitHighlightAllVents", false, baitSpawnRate);
            baitReportDelay = CustomOption.Create(332, CustomOptionType.Crewmate, "baitReportDelay", 0f, 0f, 10f, 1f, baitSpawnRate, format: "unitSeconds");
            baitShowKillFlash = CustomOption.Create(333, CustomOptionType.Crewmate, "baitShowKillFlash", true, baitSpawnRate);
            baitCanBeGuessed = CustomOption.Create(334, CustomOptionType.Crewmate, "baitCanBeGuessed", true, baitSpawnRate);

            mediumSpawnRate = new CustomRoleOption(370, CustomOptionType.Crewmate, "medium", Medium.color, 1);
            mediumCooldown = CustomOption.Create(371, CustomOptionType.Crewmate, "mediumCooldown", 30f, 5f, 120f, 5f, mediumSpawnRate, format: "unitSeconds");
            mediumDuration = CustomOption.Create(372, CustomOptionType.Crewmate, "mediumDuration", 3f, 0f, 15f, 1f, mediumSpawnRate, format: "unitSeconds");
            mediumOneTimeUse = CustomOption.Create(373, CustomOptionType.Crewmate, "mediumOneTimeUse", false, mediumSpawnRate);

            // Other options

            randomWireTask = CustomOption.Create(9909, CustomOptionType.General, "randomWireTask", false, null, true, heading: "torMapOptionsHeading");
            additionalWireTask = CustomOption.Create(9914, CustomOptionType.General, "additionalWireTask", false, randomWireTask);
            numWireTask = CustomOption.Create(9913, CustomOptionType.General, "numWireTask", 3f, 1f, 10f, 1f, randomWireTask);
            enableSenrigan = CustomOption.Create(9920, CustomOptionType.General, "enableSenrigan", true);
            canWinByTaskWithoutLivingPlayer = CustomOption.Create(9932, CustomOptionType.General, "canWinByTaskWithoutLivingPlayer", true);
            deadImpostorCanSeeKillColdown = CustomOption.Create(9933, CustomOptionType.General, "deadImpostorCanSeeKillCooldown", true);
            impostorCanIgnoreComms = CustomOption.Create(9936, CustomOptionType.General, "impostorCanIgnoreComms", false);
            disableVentAnimation = CustomOption.Create(9910, CustomOptionType.General, "disableVentAnimation", false);
            exceptOnTask = CustomOption.Create(9931, CustomOptionType.General, "exceptOnTask", false);
            additionalEmergencyCooldown = CustomOption.Create(9934, CustomOptionType.General, "additionalEmergencyCooldown", 0f, 0f, 15f, 1f, format: "unitPlayers");
            additionalEmergencyCooldownTime = CustomOption.Create(9935, CustomOptionType.General, "additionalEmergencyCooldownTime", 10f, 0f, 60f, 1f, additionalEmergencyCooldown, format: "unitSeconds");

            delayBeforeMeeting = CustomOption.Create(9921, CustomOptionType.General, "delayBeforeMeeting", 0f, 0f, 10f, 0.25f);
            additionalVents = CustomOption.Create(9905, CustomOptionType.General, "additionalVents", false);

            specimenVital = CustomOption.Create(9906, CustomOptionType.General, "specimenVital", false);
            polusRandomSpawn = CustomOption.Create(9907, CustomOptionType.General, "polusRandomSpawn", false);

            airshipOptimizeMap = CustomOption.Create(9922, CustomOptionType.General, "airshipOptimizeMap", true);
            airshipEnableWallCheck = CustomOption.Create(9908, CustomOptionType.General, "airshipEnableWallCheck", true);
            airshipReactorDuration = CustomOption.Create(9999, CustomOptionType.General, "airshipReactorDuration", 60f, 0f, 600f, 1f, format: "unitSeconds");
            airshipRandomSpawn = CustomOption.Create(9916, CustomOptionType.General, "airshipRandomSpawn", false);
            airshipAdditionalSpawn = CustomOption.Create(9917, CustomOptionType.General, "airshipAdditionalSpawn", false);
            airshipSynchronizedSpawning = CustomOption.Create(97918, CustomOptionType.General, "airshipSynchronizedSpawning", false);
            airshipSetOriginalCooldown = CustomOption.Create(9919, CustomOptionType.General, "airshipSetOriginalCooldown", false);
            airshipInitialDoorCooldown = CustomOption.Create(9923, CustomOptionType.General, "airshipInitialDoorCooldown", 0f, 0f, 60f, 1f);
            airshipInitialSabotageCooldown = CustomOption.Create(9924, CustomOptionType.General, "airshipInitialSabotageCooldown", 15f, 0f, 60f, 1f);
            airshipOldAdmin = CustomOption.Create(9925, CustomOptionType.General, "airshipOldAdmin", false);
            airshipRestrictedAdmin = CustomOption.Create(9926, CustomOptionType.General, "airshipRestrictedAdmin", false);
            airshipDisableGapSwitchBoard = CustomOption.Create(9927, CustomOptionType.General, "airshipDisableGapSwitchBoard", false);
            airshipDisableMovingPlatform = CustomOption.Create(9928, CustomOptionType.General, "airshipDisableMovingPlatform", false);
            airshipAdditionalLadder = CustomOption.Create(9929, CustomOptionType.General, "airshipAdditionalLadder", false);
            airshipOneWayLadder = CustomOption.Create(9930, CustomOptionType.General, "airshipOneWayLadder", false);
            airshipReplaceSafeTask = CustomOption.Create(9937, CustomOptionType.General, "airshipReplaceSafeTask", false);


            lastImpostorEnable = CustomOption.Create(9900, CustomOptionType.General, "lastImpostorEnable", true, null, true, heading: "lastImpostor");
            lastImpostorFunctions = CustomOption.Create(9901, CustomOptionType.General, "lastImpostorFunctions", new string[] { ModTranslation.getString("lastImpostorDivine"), ModTranslation.getString("lastImpostorGuesser") }, lastImpostorEnable);
            lastImpostorNumKills = CustomOption.Create(9902, CustomOptionType.General, "lastImpostorNumKills", 3f, 0f, 10f, 1f, lastImpostorEnable);
            lastImpostorResults = CustomOption.Create(9903, CustomOptionType.General, "fortuneTellerResults ", new string[] { "fortuneTellerResultCrew", "fortuneTellerResultTeam", "fortuneTellerResultRole" }, lastImpostorEnable);
            lastImpostorNumShots = CustomOption.Create(9904, CustomOptionType.General, "lastImpostorNumShots", 1f, 1f, 15f, 1f, lastImpostorEnable);


            deadImpsBlockSabotage = CustomOption.Create(523, CustomOptionType.General, cs(Palette.ImpostorRed, "deadImpsBlockSabotage"), false, null, true, heading: "gameplaySettingsHeading");
            anyPlayerCanStopStart = CustomOption.Create(2, CustomOptionType.General, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "anyPlayerCanStopStart"), false);
            if (EventUtility.canBeEnabled) enableEventMode = CustomOption.Create(10423, CustomOptionType.General, cs(Color.green, "enableEventMode"), true);
            maxNumberOfMeetings = CustomOption.Create(3, CustomOptionType.General, "maxNumberOfMeetings", 10, 0, 15, 1);
            blockSkippingInEmergencyMeetings = CustomOption.Create(4, CustomOptionType.General, "blockSkippingInEmergencyMeetings", false);
            noVoteIsSelfVote = CustomOption.Create(5, CustomOptionType.General, "noVoteIsSelfVote", false);
            hideOutOfSightNametags = CustomOption.Create(550, CustomOptionType.General, "hideOutOfSightNametags", false);
            refundVotesOnDeath = CustomOption.Create(551, CustomOptionType.General, "refundVotesOnDeath", true);
            allowParallelMedBayScans = CustomOption.Create(540, CustomOptionType.General, "parallelMedbayScans", false);
            hideSettings = CustomOption.Create(520, CustomOptionType.General, "hideSettings", false);

            restrictDevices = CustomOption.Create(510, CustomOptionType.General, "restrictDevices", new string[] { "optionOff", "restrictPerTurn", "restrictPerGame" });
            restrictAdmin = CustomOption.Create(501, CustomOptionType.General, "disableAdmin", true, restrictDevices);
            restrictAdminTime = CustomOption.Create(502, CustomOptionType.General, "disableAdminTime", 30f, 0f, 600f, 1f, restrictAdmin, format: "unitSeconds");
            restrictAdminText = CustomOption.Create(503, CustomOptionType.General, "restrictAdminText", true, restrictAdmin);
            restrictCameras = CustomOption.Create(505, CustomOptionType.General, "disableCameras", true, restrictDevices);
            restrictCamerasTime = CustomOption.Create(506, CustomOptionType.General, "disableCamerasTime", 30f, 0f, 600f, 1f, restrictCameras, format: "unitSeconds");
            restrictCamerasText = CustomOption.Create(509, CustomOptionType.General, "restrictCamerasText", true, restrictCameras);
            restrictVitals = CustomOption.Create(507, CustomOptionType.General, "disableVitals", true, restrictDevices);
            restrictVitalsTime = CustomOption.Create(508, CustomOptionType.General, "disableVitalsTime", 30f, 0f, 600f, 1f, restrictVitals, format: "unitSeconds");
            restrictVitalsText = CustomOption.Create(511, CustomOptionType.General, "restrictVitalsText", true, restrictVitals);

            uselessOptions = CustomOption.Create(530, CustomOptionType.General, "uselessOptions", false, null, isHeader: true);
            dynamicMap = CustomOption.Create(8, CustomOptionType.General, "playRandomMaps", false, uselessOptions);
            dynamicMapEnableSkeld = CustomOption.Create(531, CustomOptionType.General, "dynamicMapEnableSkeld", true, dynamicMap, false);
            dynamicMapEnableMira = CustomOption.Create(532, CustomOptionType.General, "dynamicMapEnableMira", true, dynamicMap, false);
            dynamicMapEnablePolus = CustomOption.Create(533, CustomOptionType.General, "dynamicMapEnablePolus", true, dynamicMap, false);
            dynamicMapEnableAirShip = CustomOption.Create(534, CustomOptionType.General, "dynamicMapEnableAirShip", true, dynamicMap, false);
            dynamicMapEnableFungle = CustomOption.Create(513, CustomOptionType.General, "dynamicMapEnableFungle", rates, dynamicMap, false);
            dynamicMapEnableSubmerged = CustomOption.Create(535, CustomOptionType.General, "dynamicMapEnableSubmerged", true, dynamicMap, false);
            dynamicMapSeparateSettings = CustomOption.Create(512, CustomOptionType.General, "dynamicMapSeparateSettings", true, dynamicMap, false);

            disableVents = CustomOption.Create(504, CustomOptionType.General, "disableVents", false, uselessOptions);
            hidePlayerNames = CustomOption.Create(6, CustomOptionType.General, "hidePlayerNames", false, uselessOptions);
            playerNameDupes = CustomOption.Create(522, CustomOptionType.General, "playerNameDupes", false, uselessOptions);
            playerColorRandom = CustomOption.Create(521, CustomOptionType.General, "playerColorRandom", false, uselessOptions);

            blockedRolePairings.Add((byte)RoleType.Vampire, new[] { (byte)RoleType.Warlock });
            blockedRolePairings.Add((byte)RoleType.Warlock, new[] { (byte)RoleType.Vampire });
            blockedRolePairings.Add((byte)RoleType.Vulture, new[] { (byte)RoleType.Cleaner });
            blockedRolePairings.Add((byte)RoleType.Cleaner, new[] { (byte)RoleType.Vulture });
        }
    }

}
