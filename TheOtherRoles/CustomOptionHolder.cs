using System.Collections.Generic;
using System.Linq;
using TheOtherRoles.Roles;
using UnityEngine;
using Types = TheOtherRoles.CustomOption.CustomOptionType;

namespace TheOtherRoles {
    public class CustomOptionHolder {
        public static string[] rates = ["0%", "10%", "20%", "30%", "40%", "50%", "60%", "70%", "80%", "90%", "100%"];
        public static string[] ratesModifier = ["1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24"];
        public static string[] presets = ["preset1", "preset2", "Random Preset Skeld", "Random Preset Mira HQ", "Random Preset Polus", "Random Preset Airship", "Random Preset Submerged"];

        public static CustomOption presetSelection;
        public static CustomOption activateRoles;
        public static CustomOption crewmateRolesCountMin;
        public static CustomOption crewmateRolesCountMax;
        public static CustomOption crewmateRolesFill;
        public static CustomOption neutralRolesCountMin;
        public static CustomOption neutralRolesCountMax;
        public static CustomOption impostorRolesCountMin;
        public static CustomOption impostorRolesCountMax;
        public static CustomOption modifiersCountMin;
        public static CustomOption modifiersCountMax;

        public static CustomOption isDraftMode;
        public static CustomOption draftModeAmountOfChoices;
        public static CustomOption draftModeTimeToChoose;
        public static CustomOption draftModeShowRoles;
        public static CustomOption draftModeHideCrewRoles;
        public static CustomOption draftModeHideImpRoles;
        public static CustomOption draftModeHideNeutralRoles;

        public static CustomOption anyPlayerCanStopStart;
        public static CustomOption enableEventMode;
        public static CustomOption eventReallyNoMini;
        public static CustomOption eventKicksPerRound;
        public static CustomOption eventHeavyAge;
        public static CustomOption freePlayGameModeNumDummies;

        public static CustomRoleOption mafiaSpawnRate;
        public static CustomOption godfatherShareInfo;
        public static CustomOption janitorCanSabotage;
        public static CustomOption janitorImpostorsCanSeeDeadBody;
        public static CustomOption mafiosoNumberOfSkips;

        public static CustomRoleOption morphlingSpawnRate;
        public static CustomOption morphlingCooldown;
        public static CustomOption morphlingDuration;

        public static CustomRoleOption camouflagerSpawnRate;
        public static CustomOption camouflagerCooldown;
        public static CustomOption camouflagerDuration;

        public static CustomRoleOption vampireSpawnRate;
        public static CustomOption vampireKillDelay;
        public static CustomOption vampireCooldown;
        public static CustomOption vampireCooldownDecrease;
        public static CustomOption vampireCanKillNearGarlics;

        public static CustomRoleOption eraserSpawnRate;
        public static CustomOption eraserCooldown;
        public static CustomOption eraserCooldownIncrease;
        public static CustomOption eraserCanEraseAnyone;

        public static CustomRoleOption guesserSpawnRate;
        public static CustomOption guesserIsImpGuesserRate;
        public static CustomOption guesserNumberOfShots;
        public static CustomOption guesserHasMultipleShotsPerMeeting;
        public static CustomOption guesserKillsThroughShield;
        public static CustomOption guesserEvilCanKillSpy;
        public static CustomOption guesserSpawnBothRate;
        public static CustomOption guesserCantGuessSnitchIfTaksDone;
        public static CustomOption guesserCantGuessFortuneTeller;

        public static CustomRoleOption watcherSpawnRate;
        public static CustomOption watcherAssignEqually;
        public static CustomOption watcherIsImpWatcherRate;
        public static CustomOption watcherSeeGuesses;
        public static CustomOption watcherSeeYasunaVotes;
        public static CustomOption watcherCanKill;

        public static CustomRoleOption jesterSpawnRate;
        public static CustomOption jesterCanCallEmergency;
        public static CustomOption jesterHasImpostorVision;
        public static CustomOption jesterCanVent;

        public static CustomRoleOption arsonistSpawnRate;
        public static CustomOption arsonistCooldown;
        public static CustomOption arsonistDuration;

        public static CustomRoleOption jackalSpawnRate;
        public static CustomOption jackalKillCooldown;
        public static CustomOption jackalCreateSidekickCooldown;
        public static CustomOption jackalCanSabotageLights;
        public static CustomOption jackalCanUseVents;
        public static CustomOption jackalCanCreateSidekick;
        public static CustomOption sidekickPromotesToJackal;
        public static CustomOption sidekickCanKill;
        public static CustomOption sidekickCanUseVents;
        public static CustomOption sidekickCanSabotageLights;
        public static CustomOption jackalPromotedFromSidekickCanCreateSidekick;
        public static CustomOption jackalCanCreateSidekickFromImpostor;
        public static CustomOption jackalAndSidekickHaveImpostorVision;

        public static CustomRoleOption opportunistSpawnRate;

        public static CustomRoleOption plagueDoctorSpawnRate;
        public static CustomOption plagueDoctorInfectCooldown;
        public static CustomOption plagueDoctorNumInfections;
        public static CustomOption plagueDoctorDistance;
        public static CustomOption plagueDoctorDuration;
        public static CustomOption plagueDoctorImmunityTime;
        public static CustomOption plagueDoctorInfectKiller;
        public static CustomOption plagueDoctorWinDead;

        public static CustomRoleOption jekyllAndHydeSpawnRate;
        public static CustomOption jekyllAndHydeNumberToWin;
        public static CustomOption jekyllAndHydeCooldown;
        public static CustomOption jekyllAndHydeSuicideTimer;
        public static CustomOption jekyllAndHydeResetAfterMeeting;
        public static CustomOption jekyllAndHydeCommonTasks;
        public static CustomOption jekyllAndHydeShortTasks;
        public static CustomOption jekyllAndHydeLongTasks;
        public static CustomOption jekyllAndHydeNumTasks;

        public static CustomRoleOption foxSpawnRate;
        public static CustomOption foxNumTasks;
        public static CustomOption foxStayTime;
        public static CustomOption foxTaskType;
        public static CustomOption foxCanCreateImmoralist;
        public static CustomOption foxCrewWinsByTasks;
        public static CustomOption foxImpostorWinsBySabotage;
        public static CustomOption foxStealthCooldown;
        public static CustomOption foxStealthDuration;
        public static CustomOption foxNumRepairs;

        public static CustomRoleOption bountyHunterSpawnRate;
        public static CustomOption bountyHunterBountyDuration;
        public static CustomOption bountyHunterReducedCooldown;
        public static CustomOption bountyHunterPunishmentTime;
        public static CustomOption bountyHunterShowArrow;
        public static CustomOption bountyHunterArrowUpdateIntervall;

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
        public static CustomOption assassinInvisibleDuration;

        public static CustomRoleOption ninjaSpawnRate;
        public static CustomOption ninjaStealthCooldown;
        public static CustomOption ninjaStealthDuration;
        public static CustomOption ninjaKillPenalty;
        public static CustomOption ninjaSpeedBonus;
        public static CustomOption ninjaFadeTime;
        public static CustomOption ninjaCanVent;
        public static CustomOption ninjaCanBeTargeted;

        public static CustomRoleOption serialKillerSpawnRate;
        public static CustomOption serialKillerKillCooldown;
        public static CustomOption serialKillerSuicideTimer;
        public static CustomOption serialKillerResetTimer;

        public static CustomRoleOption yoyoSpawnRate;
        public static CustomOption yoyoBlinkDuration;
        public static CustomOption yoyoMarkCooldown;
        public static CustomOption yoyoBlackoutRange;
        public static CustomOption yoyoBlackoutDuration;
        public static CustomOption yoyoMarkStaysOverMeeting;
        public static CustomOption yoyoSilhouetteVisibility;

        public static CustomRoleOption mayorSpawnRate;
        public static CustomOption mayorNumVotes;
        public static CustomOption mayorMeetingButton;
        public static CustomOption mayorMaxRemoteMeetings;
        public static CustomOption mayorChooseSingleVote;

        public static CustomRoleOption portalmakerSpawnRate;
        public static CustomOption portalmakerCooldown;
        public static CustomOption portalmakerUsePortalCooldown;
        public static CustomOption portalmakerLogOnlyColorType;
        public static CustomOption portalmakerLogHasTime;
        public static CustomOption portalmakerCanPortalFromAnywhere;

        public static CustomRoleOption engineerSpawnRate;
        public static CustomOption engineerNumberOfFixes;
        public static CustomOption engineerHighlightForImpostors;
        public static CustomOption engineerHighlightForTeamJackal;

        public static CustomRoleOption sheriffSpawnRate;
        public static CustomOption sheriffCooldown;
        public static CustomOption sheriffCanKillNeutrals;
        public static CustomOption deputySpawnRate;
        public static CustomOption deputyRoleCount;

        public static CustomOption deputyNumberOfHandcuffs;
        public static CustomOption deputyHandcuffCooldown;
        public static CustomOption deputyGetsPromoted;
        public static CustomOption deputyKeepsHandcuffs;
        public static CustomOption deputyHandcuffDuration;
        public static CustomOption deputyKnowsSheriff;
        public static CustomOption deputyStopsGameEnd;

        public static CustomRoleOption lighterSpawnRate;
        public static CustomOption lighterModeLightsOnVision;
        public static CustomOption lighterModeLightsOffVision;
        public static CustomOption lighterCooldown;
        public static CustomOption lighterDuration;
        public static CustomOption lighterCanSeeInvisible;

        public static CustomRoleOption sprinterSpawnRate;
        public static CustomOption sprinterCooldown;
        public static CustomOption sprinterDuration;
        public static CustomOption sprinterFadeTime;
        public static CustomOption sprinterSpeedBonus;

        public static CustomRoleOption fortuneTellerSpawnRate;
        public static CustomOption fortuneTellerNumTasks;
        public static CustomOption fortuneTellerResults;
        public static CustomOption fortuneTellerRevealOnImpDivine;
        public static CustomOption fortuneTellerDistance;
        public static CustomOption fortuneTellerDuration;

        public static CustomRoleOption detectiveSpawnRate;
        public static CustomOption detectiveAnonymousFootprints;
        public static CustomOption detectiveFootprintIntervall;
        public static CustomOption detectiveFootprintDuration;
        public static CustomOption detectiveReportNameDuration;
        public static CustomOption detectiveReportColorDuration;
        public static CustomOption detectiveInspectCooldown;
        public static CustomOption detectiveInspectDuration;

        public static CustomRoleOption timeMasterSpawnRate;
        public static CustomOption timeMasterCooldown;
        public static CustomOption timeMasterRewindTime;
        public static CustomOption timeMasterShieldDuration;
        public static CustomOption timeMasterReviveDuringRewind;

        public static CustomRoleOption medicSpawnRate;
        public static CustomOption medicShowShielded;
        public static CustomOption medicShowAttemptToShielded;
        public static CustomOption medicSetOrShowShieldAfterMeeting;
        public static CustomOption medicShowAttemptToMedic;
        public static CustomOption medicSetShieldAfterMeeting;
        public static CustomOption medicCanUseVitals;
        public static CustomOption medicSeesDeathReasonOnVitals;

        public static CustomRoleOption veteranSpawnRate;
        public static CustomOption veteranCooldown;
        public static CustomOption veteranAlertDuration;
        public static CustomOption veteranAlertNumber;

        public static CustomRoleOption jailorSpawnRate;
        public static CustomOption jailorCooldown;
        public static CustomOption jailorNumberOfJails;
        public static CustomOption jailorSuicidesIfFalseJail;
        public static CustomOption jailorTargetDiesIfFalseJail;

        public static CustomRoleOption noisemakerSpawnRate;
        public static CustomOption noisemakerCooldown;
        public static CustomOption noisemakerSoundDuration;
        public static CustomOption noisemakerSoundNumber;
        public static CustomOption noisemakerSoundTarget;

        public static CustomRoleOption sherlockSpawnRate;
        public static CustomOption sherlockCooldown;
        public static CustomOption sherlockRechargeTasksNumber;
        public static CustomOption sherlockInvestigateDistance;

        public static CustomRoleOption swapperSpawnRate;
        public static CustomOption swapperIsImpRate;
        public static CustomOption swapperCanCallEmergency;
        public static CustomOption swapperCanOnlySwapOthers;
        public static CustomOption swapperSwapsNumber;
        public static CustomOption swapperRechargeTasksNumber;

        public static CustomRoleOption seerSpawnRate;
        public static CustomOption seerMode;
        public static CustomOption seerSoulDuration;
        public static CustomOption seerLimitSoulDuration;
        public static CustomOption seerCanSeeKillTeams;

        public static CustomRoleOption hackerSpawnRate;
        public static CustomOption hackerCooldown;
        public static CustomOption hackerHackeringDuration;
        public static CustomOption hackerOnlyColorType;
        public static CustomOption hackerToolsNumber;
        public static CustomOption hackerRechargeTasksNumber;
        public static CustomOption hackerNoMove;

        public static CustomRoleOption collatorSpawnRate;
        public static CustomOption collatorCooldown;
        public static CustomOption collatorNumberOfTrials;
        public static CustomOption collatorMadmateSpecifiedAsCrewmate;
        public static CustomOption collatorStrictNeutralRoles;

        public static CustomRoleOption baitSpawnRate;
        public static CustomOption baitHighlightAllVents;
        public static CustomOption baitReportDelay;
        public static CustomOption baitShowKillFlash;
        public static CustomOption baitCanBeGuessed;
        public static CustomOption baitEmitCooldown;
        public static CustomOption baitNumberOfEmits;

        public static CustomRoleOption trackerSpawnRate;
        public static CustomOption trackerUpdateIntervall;
        public static CustomOption trackerResetTargetAfterMeeting;
        public static CustomOption trackerCanTrackCorpses;
        public static CustomOption trackerCorpsesTrackingCooldown;
        public static CustomOption trackerCorpsesTrackingDuration;
        public static CustomOption trackerTrackingMethod;
        public static CustomOption trackerCanKill;
        public static CustomOption trackerKillCooldown;

        public static CustomRoleOption archaeologistSpawnRate;
        public static CustomOption archaeologistCooldown;
        public static CustomOption archaeologistExploreDuration;
        public static CustomOption archaeologistArrowDuration;
        public static CustomOption archaeologistNumCandidates;
        public static CustomOption archaeologistRevealAntiqueMode;

        public static CustomRoleOption snitchSpawnRate;
        public static CustomOption snitchLeftTasksForReveal;
        public static CustomOption snitchIncludeTeamEvil;
        public static CustomOption snitchTeamEvilUseDifferentArrowColor;

        public static CustomRoleOption shifterSpawnRate;
        public static CustomOption shifterIsNeutralRate;
        public static CustomOption shifterShiftsModifiers;
        public static CustomOption shifterShiftsMedicShield;
        public static CustomOption shifterPastShifters;

        public static CustomRoleOption spySpawnRate;
        public static CustomOption spyCanDieToSheriff;
        public static CustomOption spyImpostorsCanKillAnyone;
        public static CustomOption spyCanEnterVents;
        public static CustomOption spyHasImpostorVision;

        public static CustomRoleOption taskMasterSpawnRate;
        public static CustomOption taskMasterBecomeATaskMasterWhenCompleteAllTasks;
        public static CustomOption taskMasterExtraCommonTasks;
        public static CustomOption taskMasterExtraShortTasks;
        public static CustomOption taskMasterExtraLongTasks;
        public static CustomOption taskMasterCanVent;

        public static CustomRoleOption buskerSpawnRate;
        public static CustomOption buskerCooldown;
        public static CustomOption buskerDuration;
        public static CustomOption buskerRestrictInformation;

        public static CustomRoleOption teleporterSpawnRate;
        public static CustomOption teleporterCooldown;
        public static CustomOption teleporterTeleportNumber;

        public static CustomRoleOption tricksterSpawnRate;
        public static CustomOption tricksterPlaceBoxCooldown;
        public static CustomOption tricksterBoxKillPenalty;
        public static CustomOption tricksterLightsOutCooldown;
        public static CustomOption tricksterLightsOutDuration;

        public static CustomRoleOption blackmailerSpawnRate;
        public static CustomOption blackmailerCooldown;
        public static CustomOption blackmailerBlockTargetVote;
        public static CustomOption blackmailerBlockTargetAbility;

        public static CustomRoleOption nekoKabochaSpawnRate;
        public static CustomOption nekoKabochaRevengeCrew;
        public static CustomOption nekoKabochaRevengeNeutral;
        public static CustomOption nekoKabochaRevengeImpostor;
        public static CustomOption nekoKabochaRevengeExile;

        public static CustomRoleOption evilTrackerSpawnRate;
        public static CustomOption evilTrackerCooldown;
        public static CustomOption evilTrackerResetTargetAfterMeeting;
        public static CustomOption evilTrackerCanSeeDeathFlash;
        public static CustomOption evilTrackerCanSeeTargetPosition;
        public static CustomOption evilTrackerCanSeeTargetTask;
        public static CustomOption evilTrackerCanSetTargetOnMeeting;

        public static CustomRoleOption evilHackerSpawnRate;
        public static CustomOption evilHackerCanHasBetterAdmin;
        public static CustomOption evilHackerCanCreateMadmate;
        public static CustomOption evilHackerCanSeeDoorStatus;
        public static CustomOption evilHackerCanCreateMadmateFromJackal;
        public static CustomOption evilHackerCanInheritAbility;
        public static CustomOption createdMadmateCanDieToSheriff;
        public static CustomOption createdMadmateCanEnterVents;
        public static CustomOption createdMadmateHasImpostorVision;
        public static CustomOption createdMadmateCanSabotage;
        public static CustomOption createdMadmateCanFixComm;
        public static CustomOption createdMadmateAbility;
        public static CustomOption createdMadmateCommonTasks;

        public static CustomRoleOption zephyrSpawnRate;
        public static CustomOption zephyrCooldown;
        public static CustomOption zephyrNumberOfCannons;
        public static CustomOption zephyrCannonRange;
        public static CustomOption zephyrCannonAttenuation;
        public static CustomOption zephyrTriggerBothCooldown;
        public static CustomOption zephyrLeaveEvidence;

        public static CustomRoleOption trapperSpawnRate;
        public static CustomOption trapperNumTrap;
        public static CustomOption trapperKillTimer;
        public static CustomOption trapperCooldown;
        public static CustomOption trapperMaxDistance;
        public static CustomOption trapperTrapRange;
        public static CustomOption trapperExtensionTime;
        public static CustomOption trapperPenaltyTime;
        public static CustomOption trapperBonusTime;

        public static CustomRoleOption undertakerSpawnRate;
        public static CustomOption undertakerSpeedDecrease;
        public static CustomOption undertakerCanVentWhileDragging;
        public static CustomOption undertakerConnectsVent;

        public static CustomRoleOption cleanerSpawnRate;
        public static CustomOption cleanerCooldown;
        public static CustomOption cleanerCanSeeBodies;

        public static CustomRoleOption warlockSpawnRate;
        public static CustomOption warlockCooldown;
        public static CustomOption warlockRootTime;

        public static CustomRoleOption securityGuardSpawnRate;
        public static CustomOption securityGuardCooldown;
        public static CustomOption securityGuardFlushCooldown;
        public static CustomOption securityGuardTotalScrews;
        public static CustomOption securityGuardCamPrice;
        public static CustomOption securityGuardVentPrice;
        public static CustomOption securityGuardCamDuration;
        public static CustomOption securityGuardCamMaxCharges;
        public static CustomOption securityGuardCamRechargeTasksNumber;
        public static CustomOption securityGuardNoMove;

        public static CustomRoleOption vultureSpawnRate;
        public static CustomOption vultureCooldown;
        public static CustomOption vultureNumberToWin;
        public static CustomOption vultureCanUseVents;
        public static CustomOption vultureShowArrows;

        public static CustomRoleOption mediumSpawnRate;
        public static CustomOption mediumCooldown;
        public static CustomOption mediumDuration;
        public static CustomOption mediumOneTimeUse;
        public static CustomOption mediumRevealTarget;
        public static CustomOption mediumChanceAdditionalInfo;

        public static CustomRoleOption doomsayerSpawnRate;
        public static CustomOption doomsayerCanObserve;
        public static CustomOption doomsayerObserveCooldown;
        public static CustomOption doomsayerNumberOfObserves;
        public static CustomOption doomsayerGuessesToWin;
        public static CustomOption doomsayerMultipleGuesses;
        public static CustomOption doomsayerIndicator;
        public static CustomOption doomsayerMaxMisses;

        public static CustomRoleOption yandereSpawnRate;
        public static CustomOption yandereReducedCooldown;
        public static CustomOption yandereCooldownPunishment;
        public static CustomOption yandereNuisanceRange;
        public static CustomOption yandereNuisanceRegisterDuration;
        public static CustomOption yandereMaxNuisance;
        public static CustomOption yandereRunawayTimeLimit;
        public static CustomOption yandereHasImpVision;

        public static CustomRoleOption lawyerSpawnRate;
        public static CustomOption lawyerTargetKnows;
        //public static CustomOption lawyerIsProsecutorChance;
        public static CustomOption lawyerTargetCanBeJester;
        public static CustomOption lawyerVision;
        public static CustomOption lawyerKnowsRole;
        public static CustomOption lawyerWinsAfterMeetings;
        public static CustomOption lawyerNeededMeetings;
        public static CustomOption pursuerCooldown;
        public static CustomOption pursuerBlanksNumber;

        public static CustomRoleOption cupidSpawnRate;
        public static CustomOption cupidTimeLimit;
        public static CustomOption cupidShield;

        public static CustomRoleOption pelicanSpawnRate;
        public static CustomOption pelicanCooldown;
        public static CustomOption pelicanHasImpVision;
        public static CustomOption pelicanCanUseVents;

        public static CustomRoleOption schrodingersCatSpawnRate;
        public static CustomOption schrodingersCatKillCooldown;
        public static CustomOption schrodingersCatBecomesImpostor;
        public static CustomOption schrodingersCatCantKillUntilLastOne;
        public static CustomOption schrodingersCatJustDieOnKilledByCrew;
        public static CustomOption schrodingersCatHideRole;
        public static CustomOption schrodingersCatCanChooseImpostor;

        public static CustomRoleOption kataomoiSpawnRate;
        public static CustomOption kataomoiStareCooldown;
        public static CustomOption kataomoiStareDuration;
        public static CustomOption kataomoiStareCount;
        public static CustomOption kataomoiStalkingCooldown;
        public static CustomOption kataomoiStalkingDuration;
        public static CustomOption kataomoiStalkingFadeTime;
        public static CustomOption kataomoiSearchCooldown;
        public static CustomOption kataomoiSearchDuration;

        public static CustomRoleOption moriartySpawnRate;
        public static CustomOption moriartyBrainwashTime;
        public static CustomOption moriartyBrainwashCooldown;
        public static CustomOption moriartyNumberToWin;
        public static CustomOption moriartySherlockAddition;
        public static CustomOption moriartyKillIndicate;

        public static CustomRoleOption akujoSpawnRate;
        public static CustomOption akujoTimeLimit;
        public static CustomOption akujoKnowsRoles;
        public static CustomOption akujoNumKeeps;

        public static CustomRoleOption yasunaSpawnRate;
        public static CustomOption yasunaIsImpYasunaRate;
        public static CustomOption yasunaNumberOfSpecialVotes;
        public static CustomOption yasunaSpecificMessageMode;

        public static CustomRoleOption thiefSpawnRate;
        public static CustomOption thiefCooldown;
        public static CustomOption thiefHasImpVision;
        public static CustomOption thiefCanUseVents;
        public static CustomOption thiefCanKillSheriff;
        public static CustomOption thiefCanStealWithGuess;

        public static CustomRoleOption mimicSpawnRate;
        public static CustomOption mimicCountAsOne;
        public static CustomOption mimicIfOneDiesBothDie;
        public static CustomOption mimicHasOneVote;
        public static CustomOption mimicSpecialCooldown;

        public static CustomRoleOption bomberSpawnRate;
        public static CustomOption bomberCooldown;
        public static CustomOption bomberDuration;
        public static CustomOption bomberCountAsOne;
        public static CustomOption bomberShowEffects;
        public static CustomOption bomberDestructiveRadius;
        public static CustomOption bomberIfOneDiesBothDie;
        public static CustomOption bomberHasOneVote;
        public static CustomOption bomberAlwaysShowArrow;

        public static CustomOption modifiersAreHidden;

        public static CustomOption modifierLover;
        public static CustomOption modifierLoverImpLoverRate;
        public static CustomOption modifierLoverQuantity;
        public static CustomOption modifierLoverBothDie;
        public static CustomOption modifierLoverEnableChat;

        public static CustomOption modifierBloody;
        public static CustomOption modifierBloodyQuantity;
        public static CustomOption modifierBloodyDuration;

        public static CustomOption modifierAntiTeleport;
        public static CustomOption modifierAntiTeleportQuantity;

        public static CustomOption modifierTieBreaker;

        public static CustomOption modifierRadar;

        public static CustomOption modifierSunglasses;
        public static CustomOption modifierSunglassesQuantity;
        public static CustomOption modifierSunglassesVision;
        
        public static CustomOption modifierMini;
        public static CustomOption modifierMiniGrowingUpDuration;
        public static CustomOption modifierMiniGrowingUpInMeeting;

        public static CustomOption modifierVip;
        public static CustomOption modifierVipQuantity;
        public static CustomOption modifierVipShowColor;

        public static CustomOption modifierInvert;
        public static CustomOption modifierInvertQuantity;
        public static CustomOption modifierInvertDuration;

        public static CustomOption modifierDiseased;
        public static CustomOption modifierDiseasedQuantity;
        public static CustomOption modifierDiseasedMultiplier;

        public static CustomOption modifierChameleon;
        public static CustomOption modifierChameleonQuantity;
        public static CustomOption modifierChameleonHoldDuration;
        public static CustomOption modifierChameleonFadeDuration;
        public static CustomOption modifierChameleonMinVisibility;

        public static CustomOption modifierMultitasker;
        public static CustomOption modifierMultitaskerQuantity;

        public static CustomOption modifierArmored;

        public static CustomOption madmateSpawnRate;
        public static CustomOption madmateQuantity;
        public static CustomOption madmateFixedRole;
        public static CustomOption madmateFixedRoleGuesserGamemode;
        public static CustomOption madmateCanDieToSheriff;
        public static CustomOption madmateCanEnterVents;
        public static CustomOption madmateCanSabotage;
        public static CustomOption madmateHasImpostorVision;
        public static CustomOption madmateCanFixComm;
        public static CustomOption madmateAbility;
        public static CustomOption madmateCommonTasks;
        public static CustomOption madmateShortTasks;
        public static CustomOption madmateLongTasks;

        //public static CustomOption modifierShifter;

        public static CustomOption maxNumberOfMeetings;
        public static CustomOption blockSkippingInEmergencyMeetings;
        public static CustomOption noVoteIsSelfVote;
        public static CustomOption hidePlayerNames;
        public static CustomOption allowParallelMedBayScans;
        public static CustomOption shieldFirstKill;
        public static CustomOption finishTasksBeforeHauntingOrZoomingOut;
        public static CustomOption camsNightVision;
        public static CustomOption camsNoNightVisionIfImpVision;
        public static CustomOption enableImpostorChat;
        public static CustomOption additionalVents;
        public static CustomOption specimenVital;
        public static CustomOption airshipLadder;
        public static CustomOption airshipOptimize;
        public static CustomOption airshipAdditionalSpawn;
        public static CustomOption fungleElectrical;
        public static CustomOption randomGameStartPosition;
        public static CustomOption activateProps;
        public static CustomOption numAccelTraps;
        public static CustomOption accelerationDuration;
        public static CustomOption speedAcceleration;
        public static CustomOption numDecelTraps;
        public static CustomOption decelerationDuration;
        public static CustomOption speedDeceleration;
        public static CustomOption decelUpdateInterval;

        public static CustomOption dynamicMap;
        public static CustomOption dynamicMapEnableSkeld;
        public static CustomOption dynamicMapEnableMira;
        public static CustomOption dynamicMapEnablePolus;
        public static CustomOption dynamicMapEnableAirShip;
        public static CustomOption dynamicMapEnableSubmerged;
        public static CustomOption dynamicMapEnableFungle;
        public static CustomOption dynamicMapSeparateSettings;

        //Guesser Gamemode
        public static CustomOption guesserGamemodeCrewNumber;
        public static CustomOption guesserGamemodeNeutralNumber;
        public static CustomOption guesserGamemodeImpNumber;
        public static CustomOption guesserForceJackalGuesser;
        public static CustomOption guesserForceThiefGuesser;
        public static CustomOption guesserGamemodeHaveModifier;
        public static CustomOption guesserGamemodeNumberOfShots;
        public static CustomOption guesserGamemodeHasMultipleShotsPerMeeting;
        public static CustomOption guesserGamemodeKillsThroughShield;
        public static CustomOption guesserGamemodeEvilCanKillSpy;
        public static CustomOption guesserGamemodeCantGuessSnitchIfTaksDone;
        public static CustomOption guesserGamemodeCantGuessFortuneTeller;
        public static CustomOption guesserGamemodeCrewGuesserNumberOfTasks;
        public static CustomOption guesserGamemodeSidekickIsAlwaysGuesser;
        public static CustomOption guesserGamemodeEnableLastImpostor;
        public static CustomOption guesserGamemodeLastImpostorNumKills;
        public static CustomOption guesserGamemodeLastImpostorNumShots;
        public static CustomOption guesserGamemodeLastImpostorHasMultipleShots;

        // Hide N Seek Gamemode
        public static CustomOption hideNSeekHunterCount;
        public static CustomOption hideNSeekKillCooldown;
        public static CustomOption hideNSeekHunterVision;
        public static CustomOption hideNSeekHuntedVision;
        public static CustomOption hideNSeekTimer;
        public static CustomOption hideNSeekCommonTasks;
        public static CustomOption hideNSeekShortTasks;
        public static CustomOption hideNSeekLongTasks;
        public static CustomOption hideNSeekTaskWin;
        public static CustomOption hideNSeekTaskPunish;
        public static CustomOption hideNSeekCanSabotage;
        public static CustomOption hideNSeekMap;
        public static CustomOption hideNSeekHunterWaiting;

        public static CustomOption hunterLightCooldown;
        public static CustomOption hunterLightDuration;
        public static CustomOption hunterLightVision;
        public static CustomOption hunterLightPunish;
        public static CustomOption hunterAdminCooldown;
        public static CustomOption hunterAdminDuration;
        public static CustomOption hunterAdminPunish;
        public static CustomOption hunterArrowCooldown;
        public static CustomOption hunterArrowDuration;
        public static CustomOption hunterArrowPunish;

        public static CustomOption huntedShieldCooldown;
        public static CustomOption huntedShieldDuration;
        public static CustomOption huntedShieldRewindTime;
        public static CustomOption huntedShieldNumber;

        internal static Dictionary<byte, byte[]> blockedRolePairings = new();

        public static string cs(Color c, string s) {
            return string.Format("<color=#{0:X2}{1:X2}{2:X2}{3:X2}>{4}</color>", ToByte(c.r), ToByte(c.g), ToByte(c.b), ToByte(c.a), s);
        }
 
        private static byte ToByte(float f) {
            f = Mathf.Clamp01(f);
            return (byte)(f * 255);
        }

        public static bool isMapSelectionOption(CustomOption option) {
            return option == hideNSeekMap;
        }

        public static void Load() {

            CustomOption.vanillaSettings = TheOtherRolesPlugin.Instance.Config.Bind("Preset0", "VanillaOptions", "");

            // Role Options
            presetSelection = CustomOption.Create(0, Types.General, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "presetSelection"), presets, null, true);

            if (Utilities.EventUtility.canBeEnabled) enableEventMode = CustomOption.Create(10423, Types.General, cs(Color.green, "enableEventMode"), true, null, true);

            // Using new id's for the options to not break compatibilty with older versions
            crewmateRolesCountMin = CustomOption.Create(300, Types.General, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "crewmateRolesCountMin"), 24f, 0f, 24f, 1f, null, true, "unitPlayers", heading: "headingMinMax");
            crewmateRolesCountMax = CustomOption.Create(301, Types.General, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "crewmateRolesCountMax"), 24f, 0f, 24f, 1f, format: "unitPlayers");
            neutralRolesCountMin = CustomOption.Create(302, Types.General, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "neutralRolesCountMin"), 24f, 0f, 24f, 1f, format: "unitPlayers");
            neutralRolesCountMax = CustomOption.Create(303, Types.General, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "neutralRolesCountMax"), 24f, 0f, 24f, 1f, format: "unitPlayers");
            impostorRolesCountMin = CustomOption.Create(304, Types.General, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "impostorRolesCountMin"), 24f, 0f, 24f, 1f, format: "unitPlayers");
            impostorRolesCountMax = CustomOption.Create(305, Types.General, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "impostorRolesCountMax"), 24f, 0f, 24f, 1f, format: "unitPlayers");
            modifiersCountMin = CustomOption.Create(306, Types.General, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "modifiersCountMin"), 24f, 0f, 24f, 1f, format: "unitPlayers");
            modifiersCountMax = CustomOption.Create(307, Types.General, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "modifiersCountMax"), 24f, 0f, 24f, 1f, format: "unitPlayers");
            crewmateRolesFill = CustomOption.Create(308, Types.General, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "crewmateRolesFill"), false);

            isDraftMode = CustomOption.Create(600, Types.General, cs(Color.yellow, "enableDraftMode"), false, null, true, heading: "headingRoleDraft");
            draftModeAmountOfChoices = CustomOption.Create(601, Types.General, cs(Color.yellow, "draftModeAmountOfChoices"), 3f, 2f, 6f, 1f, isDraftMode, false, format: "unitScrews");
            draftModeTimeToChoose = CustomOption.Create(602, Types.General, cs(Color.yellow, "draftModeTimeToChoose"), 5f, 3f, 20f, 1f, isDraftMode, false, format: "unitSeconds");
            draftModeShowRoles = CustomOption.Create(603, Types.General, cs(Color.yellow, "draftModeShowRoles"), false, isDraftMode, false);
            draftModeHideCrewRoles = CustomOption.Create(606, Types.General, cs(Color.yellow, "draftModeHideCrewRoles"), false, draftModeShowRoles, false);
            draftModeHideImpRoles = CustomOption.Create(604, Types.General, cs(Color.yellow, "draftModeHideImpRoles"), false, draftModeShowRoles, false);
            draftModeHideNeutralRoles = CustomOption.Create(605, Types.General, cs(Color.yellow, "draftModeHideNeutralRoles"), false, draftModeShowRoles, false);

            mafiaSpawnRate = new CustomRoleOption(18, Types.Impostor, "mafia", Janitor.color, 1);
            godfatherShareInfo = CustomOption.Create(24, Types.Impostor, "godfatherShareInfo", true, mafiaSpawnRate);
            janitorImpostorsCanSeeDeadBody = CustomOption.Create(19, Types.Impostor, "janitorImpostorsCanSeeDeadBody", true, mafiaSpawnRate);
            janitorCanSabotage = CustomOption.Create(25, Types.Impostor, "janitorCanSabotage", true, mafiaSpawnRate);
            mafiosoNumberOfSkips = CustomOption.Create(23, Types.Impostor, "mafiosoNumberOfSkips", 2f, 1f, 15f, 1f, mafiaSpawnRate, false, "unitScrews");

            morphlingSpawnRate = new CustomRoleOption(20, Types.Impostor, "morphling", Morphling.color);
            morphlingCooldown = CustomOption.Create(21, Types.Impostor, "morphlingCooldown", 30f, 10f, 60f, 2.5f, morphlingSpawnRate, false, "unitSeconds");
            morphlingDuration = CustomOption.Create(22, Types.Impostor, "morphlingDuration", 10f, 1f, 20f, 0.5f, morphlingSpawnRate, false, "unitSeconds");

            camouflagerSpawnRate = new CustomRoleOption(30, Types.Impostor, "camouflager", Camouflager.color, 1);
            camouflagerCooldown = CustomOption.Create(31, Types.Impostor, "camouflagerCooldown", 30f, 10f, 60f, 2.5f, camouflagerSpawnRate, false, "unitSeconds");
            camouflagerDuration = CustomOption.Create(32, Types.Impostor, "camouflagerDuration", 10f, 1f, 20f, 0.5f, camouflagerSpawnRate, false, "unitSeconds");

            vampireSpawnRate = new CustomRoleOption(40, Types.Impostor, "vampire", Vampire.color);
            vampireKillDelay = CustomOption.Create(41, Types.Impostor, "vampireKillDelay", 10f, 1f, 20f, 1f, vampireSpawnRate, false, "unitSeconds");
            vampireCooldown = CustomOption.Create(42, Types.Impostor, "vampireCooldown", 30f, 10f, 60f, 2.5f, vampireSpawnRate, false, "unitSeconds");
            vampireCooldownDecrease = CustomOption.Create(43, Types.Impostor, "vampireCooldownDecrease", 10f, 0f, 120f, 2.5f, vampireSpawnRate, false, "unitSeconds");
            vampireCanKillNearGarlics = CustomOption.Create(44, Types.Impostor, "vampireCanKillNearGarlics", true, vampireSpawnRate);

            eraserSpawnRate = new CustomRoleOption(230, Types.Impostor, "eraser", Eraser.color);
            eraserCooldown = CustomOption.Create(231, Types.Impostor, "eraserCooldown", 30f, 10f, 120f, 5f, eraserSpawnRate, false, "unitSeconds");
            eraserCooldownIncrease = CustomOption.Create(233, Types.Impostor, "eraserCooldownIncrease", 10f, 0f, 120f, 2.5f, eraserSpawnRate, format: "unitSeconds");
            eraserCanEraseAnyone = CustomOption.Create(232, Types.Impostor, "eraserCanEraseAnyone", false, eraserSpawnRate);

            tricksterSpawnRate = new CustomRoleOption(250, Types.Impostor, "trickster", Trickster.color, 1);
            tricksterPlaceBoxCooldown = CustomOption.Create(251, Types.Impostor, "tricksterPlaceBoxCooldown", 10f, 2.5f, 30f, 2.5f, tricksterSpawnRate, false, "unitSeconds");
            tricksterBoxKillPenalty = CustomOption.Create(254, Types.Impostor, "tricksterBoxKillPenalty", 2.5f, 0f, 30f, 2.5f, tricksterSpawnRate, false, "unitSeconds");
            tricksterLightsOutCooldown = CustomOption.Create(252, Types.Impostor, "tricksterLightsOutCooldown", 30f, 10f, 60f, 5f, tricksterSpawnRate, false, "unitSeconds");
            tricksterLightsOutDuration = CustomOption.Create(253, Types.Impostor, "tricksterLightsOutDuration", 15f, 5f, 60f, 2.5f, tricksterSpawnRate, false, "unitSeconds");

            cleanerSpawnRate = new CustomRoleOption(260, Types.Impostor, "cleaner", Cleaner.color);
            cleanerCooldown = CustomOption.Create(261, Types.Impostor, "cleanerCooldown", 30f, 10f, 60f, 2.5f, cleanerSpawnRate, false, "unitSeconds");
            cleanerCanSeeBodies = CustomOption.Create(262, Types.Impostor, "cleanerCanSeeBodies", true, cleanerSpawnRate);

            warlockSpawnRate = new CustomRoleOption(270, Types.Impostor, "warlock", Warlock.color);
            warlockCooldown = CustomOption.Create(271, Types.Impostor, "warlockCooldown", 30f, 10f, 60f, 2.5f, warlockSpawnRate, false, "unitSeconds");
            warlockRootTime = CustomOption.Create(272, Types.Impostor, "warlockRootTime", 5f, 0f, 15f, 1f, warlockSpawnRate, false, "unitSeconds");

            bountyHunterSpawnRate = new CustomRoleOption(320, Types.Impostor, "bountyHunter", BountyHunter.color);
            bountyHunterBountyDuration = CustomOption.Create(321, Types.Impostor, "bountyHunterBountyDuration", 60f, 10f, 180f, 10f, bountyHunterSpawnRate, false, "unitSeconds");
            bountyHunterReducedCooldown = CustomOption.Create(322, Types.Impostor, "bountyHunterReducedCooldown", 2.5f, 0f, 30f, 2.5f, bountyHunterSpawnRate, false, "unitSeconds");
            bountyHunterPunishmentTime = CustomOption.Create(323, Types.Impostor, "bountyHunterPunishmentTime", 20f, 0f, 60f, 2.5f, bountyHunterSpawnRate, false, "unitSeconds");
            bountyHunterShowArrow = CustomOption.Create(324, Types.Impostor, "bountyHunterShowArrow", true, bountyHunterSpawnRate);
            bountyHunterArrowUpdateIntervall = CustomOption.Create(325, Types.Impostor, "bountyHunterArrowUpdateInterval", 15f, 2.5f, 60f, 2.5f, bountyHunterShowArrow, false, "unitSeconds");

            witchSpawnRate = new CustomRoleOption(370, Types.Impostor, "witch", Witch.color);
            witchCooldown = CustomOption.Create(371, Types.Impostor, "witchSpellCooldown", 30f, 10f, 120f, 5f, witchSpawnRate, false, "unitSeconds");
            witchAdditionalCooldown = CustomOption.Create(372, Types.Impostor, "witchAdditionalCooldown", 10f, 0f, 60f, 5f, witchSpawnRate, false, "unitSeconds");
            witchCanSpellAnyone = CustomOption.Create(373, Types.Impostor, "witchCanSpellAnyone", false, witchSpawnRate);
            witchSpellCastingDuration = CustomOption.Create(374, Types.Impostor, "witchSpellDuration", 1f, 0f, 10f, 1f, witchSpawnRate, false, "unitSeconds");
            witchTriggerBothCooldowns = CustomOption.Create(375, Types.Impostor, "witchTriggerBoth", true, witchSpawnRate);
            witchVoteSavesTargets = CustomOption.Create(376, Types.Impostor, "witchSaveTargets", true, witchSpawnRate);

            assassinSpawnRate = new CustomRoleOption(380, Types.Impostor, "assassin", Assassin.color);
            assassinCooldown = CustomOption.Create(381, Types.Impostor, "assassinCooldown", 30f, 10f, 120f, 5f, assassinSpawnRate, false, "unitSeconds");
            assassinKnowsTargetLocation = CustomOption.Create(382, Types.Impostor, "assassinKnowsTargetLocation", true, assassinSpawnRate);
            assassinTraceTime = CustomOption.Create(383, Types.Impostor, "assassinTraceDuration", 5f, 1f, 20f, 0.5f, assassinSpawnRate, false, "unitSeconds");
            assassinTraceColorTime = CustomOption.Create(384, Types.Impostor, "assassinTraceColorTime", 2f, 0f, 20f, 0.5f, assassinSpawnRate, false, "unitSeconds");
            assassinInvisibleDuration = CustomOption.Create(385, Types.Impostor, "assassinInvisibleDuration", 3f, 0f, 20f, 1f, assassinSpawnRate, false, "unitSeconds");

            ninjaSpawnRate = new CustomRoleOption(5050, Types.Impostor, "ninja", Ninja.color);
            ninjaStealthCooldown = CustomOption.Create(5051, Types.Impostor, "ninjaStealthCooldown", 30f, 2.5f, 60f, 2.5f, ninjaSpawnRate, false, "unitSeconds");
            ninjaStealthDuration = CustomOption.Create(5052, Types.Impostor, "ninjaStealthDuration", 15f, 2.5f, 60f, 2.5f, ninjaSpawnRate, false, "unitSeconds");
            ninjaFadeTime = CustomOption.Create(5053, Types.Impostor, "ninjaFadeTime", 0.5f, 0.0f, 2.5f, 0.5f, ninjaSpawnRate, false, "unitSeconds");
            ninjaKillPenalty = CustomOption.Create(5054, Types.Impostor, "ninjaKillPenalty", 10f, 0f, 60f, 2.5f, ninjaSpawnRate, false, "unitSeconds");
            ninjaSpeedBonus = CustomOption.Create(5055, Types.Impostor, "ninjaSpeedBonus", 1.25f, 0.5f, 2f, 0.25f, ninjaSpawnRate, false, "unitTimes");
            ninjaCanBeTargeted = CustomOption.Create(5056, Types.Impostor, "ninjaCanBeTargeted", true, ninjaSpawnRate);
            ninjaCanVent = CustomOption.Create(5057, Types.Impostor, "ninjaCanVent", false, ninjaSpawnRate);

            serialKillerSpawnRate = new CustomRoleOption(4010, Types.Impostor, "serialKiller", SerialKiller.color);
            serialKillerKillCooldown = CustomOption.Create(4011, Types.Impostor, "serialKillerKillCooldown", 15f, 2.5f, 60f, 2.5f, serialKillerSpawnRate, false, "unitSeconds");
            serialKillerSuicideTimer = CustomOption.Create(4012, Types.Impostor, "serialKillerSuicideTimer", 40f, 2.5f, 60f, 2.5f, serialKillerSpawnRate, false, "unitSeconds");
            serialKillerResetTimer = CustomOption.Create(4013, Types.Impostor, "serialKillerResetTimer", true, serialKillerSpawnRate);

            nekoKabochaSpawnRate = new CustomRoleOption(880, Types.Impostor, "nekoKabocha", NekoKabocha.color);
            nekoKabochaRevengeCrew = CustomOption.Create(881, Types.Impostor, "nekoKabochaRevengeCrew", true, nekoKabochaSpawnRate);
            nekoKabochaRevengeImpostor = CustomOption.Create(882, Types.Impostor, "nekoKabochaRevengeImpostor", true, nekoKabochaSpawnRate);
            nekoKabochaRevengeNeutral = CustomOption.Create(883, Types.Impostor, "nekoKabochaRevengeNeutral", true, nekoKabochaSpawnRate);
            nekoKabochaRevengeExile = CustomOption.Create(884, Types.Impostor, "nekoKabochaRevengeExile", false, nekoKabochaSpawnRate);

            evilTrackerSpawnRate = new CustomRoleOption(4026, Types.Impostor, "evilTracker", EvilTracker.color);
            evilTrackerCooldown = CustomOption.Create(4027, Types.Impostor, "evilTrackerCooldown", 10f, 0f, 60f, 5f, evilTrackerSpawnRate, false, "unitSeconds");
            evilTrackerResetTargetAfterMeeting = CustomOption.Create(4028, Types.Impostor, "evilTrackerResetTargetAfterMeeting", true, evilTrackerSpawnRate);
            evilTrackerCanSeeDeathFlash = CustomOption.Create(4029, Types.Impostor, "evilTrackerCanSeeDeathFlash", true, evilTrackerSpawnRate);
            evilTrackerCanSeeTargetPosition = CustomOption.Create(4031, Types.Impostor, "evilTrackerCanSeeTargetPosition", true, evilTrackerSpawnRate);
            evilTrackerCanSeeTargetTask = CustomOption.Create(4030, Types.Impostor, "evilTrackerCanSeeTargetTask", true, evilTrackerSpawnRate);
            evilTrackerCanSetTargetOnMeeting = CustomOption.Create(4032, Types.Impostor, "evilTrackerCanSetTargetOnMeeting", true, evilTrackerSpawnRate);

            undertakerSpawnRate = new CustomRoleOption(4056, Types.Impostor, "undertaker", Undertaker.color, 1);
            undertakerSpeedDecrease = CustomOption.Create(4057, Types.Impostor, "undertakerSpeedDecrease", 0f, -80f, 180f, 10f, undertakerSpawnRate, false, "unitPercent");
            undertakerCanVentWhileDragging = CustomOption.Create(4058, Types.Impostor, "undertakerCanVentWhileDragging", true, undertakerSpawnRate);
            undertakerConnectsVent = CustomOption.Create(4059, Types.Impostor, "undertakerConnectsVent", true, undertakerSpawnRate);

            yoyoSpawnRate = new CustomRoleOption(470, Types.Impostor, "yoyo", Yoyo.color);
            yoyoBlinkDuration = CustomOption.Create(471, Types.Impostor, "yoyoBlinkDuration", 20f, 2.5f, 120f, 2.5f, yoyoSpawnRate, format: "unitSeconds");
            yoyoMarkCooldown = CustomOption.Create(472, Types.Impostor, "yoyoMarkCooldown", 20f, 2.5f, 120f, 2.5f, yoyoSpawnRate, format: "unitSeconds");
            yoyoBlackoutRange = CustomOption.Create(474, Types.Impostor, "yoyoBlackoutRange", 0f, 2f, 10f, 0.5f, yoyoSpawnRate, false, "unitMeters");
            yoyoBlackoutDuration = CustomOption.Create(475, Types.Impostor, "yoyoBlackoutDuration", 1f, 0.5f, 3f, 0.5f, yoyoSpawnRate, false, "unitSeconds");
            yoyoMarkStaysOverMeeting = CustomOption.Create(473, Types.Impostor, "yoyoMarkStaysOverMeeting", true, yoyoSpawnRate);
            yoyoSilhouetteVisibility = CustomOption.Create(476, Types.Impostor, "yoyoSilhouetteVisibility", ["0%", "10%", "20%", "30%", "40%", "50%"], yoyoSpawnRate);

            blackmailerSpawnRate = new CustomRoleOption(710, Types.Impostor, "blackmailer", Blackmailer.color);
            blackmailerCooldown = CustomOption.Create(711, Types.Impostor, "blackmailerCooldown", 30f, 5f, 120f, 5f, blackmailerSpawnRate, false, "unitSeconds");
            blackmailerBlockTargetVote = CustomOption.Create(712, Types.Impostor, "blackmailerBlockTargetVote", true, blackmailerSpawnRate);
            blackmailerBlockTargetAbility = CustomOption.Create(713, Types.Impostor, "blackmailerBlockTargetAbility", true, blackmailerSpawnRate);

            evilHackerSpawnRate = new CustomRoleOption(8001, Types.Impostor, "evilHacker", EvilHacker.color);
            evilHackerCanHasBetterAdmin = CustomOption.Create(8002, Types.Impostor, "evilHackerCanHasBetterAdmin", false, evilHackerSpawnRate);
            evilHackerCanSeeDoorStatus = CustomOption.Create(8015, Types.Impostor, "evilHackerCanSeeDoorStatus", true, evilHackerSpawnRate);
            evilHackerCanCreateMadmate = CustomOption.Create(8000, Types.Impostor, "evilHackerCanCreateMadmate", true, evilHackerSpawnRate);
            evilHackerCanCreateMadmateFromJackal = CustomOption.Create(8013, Types.Impostor, "evilHackerCanCreateMadmateFromJackal", true, evilHackerCanCreateMadmate);
            createdMadmateCanDieToSheriff = CustomOption.Create(8004, Types.Impostor, "createdMadmateCanDieToSheriff", true, evilHackerCanCreateMadmate);
            createdMadmateCanEnterVents = CustomOption.Create(8005, Types.Impostor, "createdMadmateCanEnterVents", true, evilHackerCanCreateMadmate);
            createdMadmateCanFixComm = CustomOption.Create(8006, Types.Impostor, "createdMadmateCanFixComm", false, evilHackerCanCreateMadmate);
            createdMadmateCanSabotage = CustomOption.Create(8007, Types.Impostor, "createdMadmateCanSabotage", false, evilHackerCanCreateMadmate);
            createdMadmateHasImpostorVision = CustomOption.Create(8008, Types.Impostor, "createdMadmateHasImpostorVision", true, evilHackerCanCreateMadmate);
            createdMadmateAbility = CustomOption.Create(8009, Types.Impostor, "createdMadmateAbility", true, evilHackerCanCreateMadmate);
            createdMadmateCommonTasks = CustomOption.Create(8010, Types.Impostor, "createdMadmateCommonTasks", 1f, 1f, 3f, 1f, createdMadmateAbility, false, "unitScrews");
            evilHackerCanInheritAbility = CustomOption.Create(8014, Types.Impostor, "evilHackerCanInheritAbility", false, evilHackerSpawnRate);

            zephyrSpawnRate = new CustomRoleOption(9100, Types.Impostor, "zephyr", Zephyr.color);
            zephyrCooldown = CustomOption.Create(9105, Types.Impostor, "zephyrCooldown", 30f, 5f, 120f, 2.5f, zephyrSpawnRate, false, "unitSeconds");
            zephyrNumberOfCannons = CustomOption.Create(9103, Types.Impostor, "zephyrNumberOfCannons", 5f, 1f, 10f, 1f, zephyrSpawnRate, false, "unitScrews");
            zephyrCannonRange = CustomOption.Create(9102, Types.Impostor, "zephyrCannonPower", 5f, 2.5f, 40f, 2.5f, zephyrSpawnRate, false, "unitTimes");
            zephyrCannonAttenuation = CustomOption.Create(9104, Types.Impostor, "zephyrCannonAttenuation", 0.75f, 0.25f, 2f, 0.125f, zephyrSpawnRate, false, "unitTimes");
            zephyrTriggerBothCooldown = CustomOption.Create(9107, Types.Impostor, "zephyrTriggerBothCooldown", true, zephyrSpawnRate);
            zephyrLeaveEvidence = CustomOption.Create(9106, Types.Impostor, "zephyrLeaveEvidence", true, zephyrSpawnRate);

            trapperSpawnRate = new CustomRoleOption(8016, Types.Impostor, "trapper", Trapper.color, 1);
            trapperNumTrap = CustomOption.Create(8017, Types.Impostor, "trapperNumTrap", 2f, 1f, 10f, 1f, trapperSpawnRate, false, "unitScrews");
            trapperExtensionTime = CustomOption.Create(8018, Types.Impostor, "trapperExtensionTime", 5f, 2f, 10f, 0.5f, trapperSpawnRate, false, "unitSeconds");
            trapperCooldown = CustomOption.Create(8019, Types.Impostor, "trapperCooldown", 10f, 10f, 60f, 2.5f, trapperSpawnRate, false, "unitSeconds");
            trapperKillTimer = CustomOption.Create(8025, Types.Impostor, "trapperKillTimer", 5f, 1f, 30f, 1f, trapperSpawnRate, false, "unitSeconds");
            trapperTrapRange = CustomOption.Create(8021, Types.Impostor, "trapperTrapRange", 1f, 0.5f, 5f, 0.1f, trapperSpawnRate, false, "unitMeters");
            trapperMaxDistance = CustomOption.Create(8022, Types.Impostor, "trapperMaxDistance", 10f, 1f, 50f, 1f, trapperSpawnRate, false, "unitMeters");
            trapperPenaltyTime = CustomOption.Create(8023, Types.Impostor, "trapperPenaltyTime", 10f, 0f, 50f, 1f, trapperSpawnRate, false, "unitSeconds");
            trapperBonusTime = CustomOption.Create(8024, Types.Impostor, "trapperBonusTime", 10f, 0f, 50f, 1f, trapperSpawnRate, false, "unitSeconds");

            mimicSpawnRate = new CustomRoleOption(5000, Types.Impostor, "mimic", MimicK.color, 1);
            mimicCountAsOne = CustomOption.Create(5001, Types.Impostor, "mimicCountAsOne", true, mimicSpawnRate);
            mimicIfOneDiesBothDie = CustomOption.Create(5002, Types.Impostor, "mimicIfOneDiesBothDies", true, mimicSpawnRate);
            mimicHasOneVote = CustomOption.Create(5003, Types.Impostor, "mimicHasOneVote", true, mimicSpawnRate);
            mimicSpecialCooldown = CustomOption.Create(5004, Types.Impostor, "mimicSpecialCooldown", 20f, 2f, 60f, 1f, mimicSpawnRate, false, "unitSeconds");

            bomberSpawnRate = new CustomRoleOption(6076, Types.Impostor, "bomber", BomberA.color, 1);
            bomberCooldown = CustomOption.Create(6077, Types.Impostor, "bomberCooldown", 20f, 2f, 30f, 2f, bomberSpawnRate, false, "unitSeconds");
            bomberDuration = CustomOption.Create(6078, Types.Impostor, "bomberDuration", 2f, 1f, 10f, 0.5f, bomberSpawnRate, false, "unitSeconds");
            bomberCountAsOne = CustomOption.Create(6079, Types.Impostor, "bomberCountAsOne", true, bomberSpawnRate);
            bomberShowEffects = CustomOption.Create(6080, Types.Impostor, "bomberShowEffects", true, bomberSpawnRate);
            bomberIfOneDiesBothDie = CustomOption.Create(6081, Types.Impostor, "bomberIfOneDiesBothDie", true, bomberSpawnRate);
            bomberHasOneVote = CustomOption.Create(6801, Types.Impostor, "bomberHasOneVote", true, bomberSpawnRate);
            bomberAlwaysShowArrow = CustomOption.Create(6802, Types.Impostor, "bomberAlwaysShowArrow", true, bomberSpawnRate);

            guesserSpawnRate = new CustomRoleOption(310, Types.Neutral, "guesser", Guesser.color, 1);
            guesserIsImpGuesserRate = CustomOption.Create(311, Types.Neutral, "guesserIsImpGuesserRate", rates, guesserSpawnRate);
            guesserNumberOfShots = CustomOption.Create(312, Types.Neutral, "guesserNumberOfShots", 2f, 1f, 24f, 1f, guesserSpawnRate, false, "unitShots");
            guesserHasMultipleShotsPerMeeting = CustomOption.Create(313, Types.Neutral, "guesserHasMultipleShotsPerMeeting", false, guesserSpawnRate);
            guesserKillsThroughShield  = CustomOption.Create(315, Types.Neutral, "guesserKillsThroughShield", true, guesserSpawnRate);
            guesserEvilCanKillSpy  = CustomOption.Create(316, Types.Neutral, "guesserEvilCanKillSpy", true, guesserSpawnRate);
            guesserSpawnBothRate = CustomOption.Create(317, Types.Neutral, "guesserSpawnBothRate", rates, guesserSpawnRate);
            guesserCantGuessSnitchIfTaksDone = CustomOption.Create(318, Types.Neutral, "guesserCantGuessSnitchIfTaksDone", true, guesserSpawnRate);
            guesserCantGuessFortuneTeller = CustomOption.Create(319, Types.Neutral, "guesserCantGuessFortuneTeller", true, guesserSpawnRate);

            swapperSpawnRate = new CustomRoleOption(150, Types.Neutral, "swapper", Swapper.color, 1);
            swapperIsImpRate = CustomOption.Create(4036, Types.Neutral, "swapperIsImpRate", rates, swapperSpawnRate);
            swapperCanCallEmergency = CustomOption.Create(151, Types.Neutral, "swapperCanCallEmergency", false, swapperSpawnRate);
            swapperCanOnlySwapOthers = CustomOption.Create(152, Types.Neutral, "swapperCanOnlySwapOthers", false, swapperSpawnRate);

            swapperSwapsNumber = CustomOption.Create(153, Types.Neutral, "swapperSwapsNumber", 1f, 0f, 15f, 1f, swapperSpawnRate, false, "unitShots");
            swapperRechargeTasksNumber = CustomOption.Create(154, Types.Neutral, "swapperRechargeTasksNumber", 2f, 1f, 10f, 1f, swapperSpawnRate, false, "unitScrews");

            watcherSpawnRate = new CustomRoleOption(1035, Types.Neutral, "watcher", Watcher.color);
            watcherAssignEqually = CustomOption.Create(1037, Types.Neutral, "watcherAssignEqually", ["optionOn", "optionOff"], watcherSpawnRate);
            watcherIsImpWatcherRate = CustomOption.Create(1036, Types.Neutral, "watcherisImpWatcherRate", rates, watcherAssignEqually);
            watcherSeeGuesses = CustomOption.Create(5080, Types.Neutral, "watcherSeeGuesses", true, watcherSpawnRate);
            watcherSeeYasunaVotes = CustomOption.Create(5081, Types.Neutral, "watcherSeeYasunaVotes", true, watcherSpawnRate);
            watcherCanKill = CustomOption.Create(5082, Types.Neutral, "watcherCanKill", true, watcherSpawnRate);

            yasunaSpawnRate = new CustomRoleOption(6040, Types.Neutral, "yasuna", Yasuna.color, 1);
            yasunaIsImpYasunaRate = CustomOption.Create(6041, Types.Neutral, "yasunaIsImpYasunaRate", rates, yasunaSpawnRate);
            yasunaNumberOfSpecialVotes = CustomOption.Create(6042, Types.Neutral, "yasunaNumberOfSpecialVotes", 1f, 1f, 15f, 1f, yasunaSpawnRate, false, "unitShots");
            yasunaSpecificMessageMode = CustomOption.Create(6043, Types.Neutral, "yasunaSpecificMessageMode", true, yasunaSpawnRate);

            jesterSpawnRate = new CustomRoleOption(60, Types.Neutral, "jester", Jester.color);
            jesterCanCallEmergency = CustomOption.Create(61, Types.Neutral, "jesterCanCallEmergency", true, jesterSpawnRate);
            jesterHasImpostorVision = CustomOption.Create(62, Types.Neutral, "jesterHasImpostorVision", false, jesterSpawnRate);
            jesterCanVent = CustomOption.Create(6088, Types.Neutral, "jesterCanVent", false, jesterSpawnRate);

            arsonistSpawnRate = new CustomRoleOption(290, Types.Neutral, "arsonist", Arsonist.color);
            arsonistCooldown = CustomOption.Create(291, Types.Neutral, "arsonistCooldown", 12.5f, 2.5f, 60f, 2.5f, arsonistSpawnRate, false, "unitSeconds");
            arsonistDuration = CustomOption.Create(292, Types.Neutral, "arsonistDuration", 3f, 1f, 10f, 1f, arsonistSpawnRate, false, "unitSeconds");

            jackalSpawnRate = new CustomRoleOption(220, Types.Neutral, "jackal", Jackal.color);
            jackalKillCooldown = CustomOption.Create(221, Types.Neutral, "jackalKillCooldown", 30f, 10f, 60f, 2.5f, jackalSpawnRate, false, "unitSeconds");
            jackalCreateSidekickCooldown = CustomOption.Create(222, Types.Neutral, "jackalCreateSidekickCooldown", 30f, 10f, 60f, 2.5f, jackalSpawnRate, false, "unitSeconds");
            jackalCanUseVents = CustomOption.Create(223, Types.Neutral, "jackalCanUseVents", true, jackalSpawnRate);
            jackalCanSabotageLights = CustomOption.Create(431, Types.Neutral, "jackalCanSabotageLights", true, jackalSpawnRate);
            jackalCanCreateSidekick = CustomOption.Create(224, Types.Neutral, "jackalCanCreateSidekick", false, jackalSpawnRate);
            sidekickPromotesToJackal = CustomOption.Create(225, Types.Neutral, "sidekickPromotesToJackal", false, jackalCanCreateSidekick);
            sidekickCanKill = CustomOption.Create(226, Types.Neutral, "sidekickCanKill", false, jackalCanCreateSidekick);
            sidekickCanUseVents = CustomOption.Create(227, Types.Neutral, "sidekickCanUseVents", true, jackalCanCreateSidekick);
            sidekickCanSabotageLights = CustomOption.Create(432, Types.Neutral, "sidekickCanSabotageLights", true, jackalCanCreateSidekick);
            jackalPromotedFromSidekickCanCreateSidekick = CustomOption.Create(228, Types.Neutral, "jackalPromotedFromSidekickCanCreateSidekick", true, sidekickPromotesToJackal);
            jackalCanCreateSidekickFromImpostor = CustomOption.Create(229, Types.Neutral, "jackalCanCreateSidekickFromImpostor", true, jackalCanCreateSidekick);
            jackalAndSidekickHaveImpostorVision = CustomOption.Create(430, Types.Neutral, "jackalAndSidekickHaveImpostorVision", false, jackalSpawnRate);

            vultureSpawnRate = new CustomRoleOption(340, Types.Neutral, "vulture", Vulture.color);
            vultureCooldown = CustomOption.Create(341, Types.Neutral, "vultureCooldown", 15f, 10f, 60f, 2.5f, vultureSpawnRate, false, "unitSeconds");
            vultureNumberToWin = CustomOption.Create(342, Types.Neutral, "vultureNumberToWin", 4f, 1f, 10f, 1f, vultureSpawnRate, false, "unitScrews");
            vultureCanUseVents = CustomOption.Create(343, Types.Neutral, "vultureCanUseVents", true, vultureSpawnRate);
            vultureShowArrows = CustomOption.Create(344, Types.Neutral, "vultureShowArrows", true, vultureSpawnRate);

            lawyerSpawnRate = new CustomRoleOption(350, Types.Neutral, "lawyer", Lawyer.color, 1);
            lawyerTargetKnows = CustomOption.Create(358, Types.Neutral, "lawyerTargetKnows", true, lawyerSpawnRate);
            lawyerVision = CustomOption.Create(354, Types.Neutral, "lawyerVision", 1f, 0.25f, 3f, 0.25f, lawyerSpawnRate, false, "unitTimes");
            lawyerKnowsRole = CustomOption.Create(355, Types.Neutral, "lawyerKnowsRole", false, lawyerSpawnRate); 
            lawyerWinsAfterMeetings = CustomOption.Create(352, Types.Neutral, "lawyerWinsMeeting", false, lawyerSpawnRate);
            lawyerNeededMeetings = CustomOption.Create(353, Types.Neutral, "lawyerMeetingsNeeded", 5f, 1f, 15f, 1f, lawyerWinsAfterMeetings, false, "unitShots");
            lawyerTargetCanBeJester = CustomOption.Create(351, Types.Neutral, "lawyerTargetCanBeJester", false, lawyerSpawnRate);
            pursuerCooldown = CustomOption.Create(356, Types.Neutral, "pursuerCooldown", 30f, 5f, 60f, 2.5f, lawyerSpawnRate, false, "unitSeconds");
            pursuerBlanksNumber = CustomOption.Create(357, Types.Neutral, "pursuerBlanksNumber", 5f, 1f, 20f, 1f, lawyerSpawnRate, false, "unitScrews");

            shifterSpawnRate = new CustomRoleOption(1100, Types.Neutral, "shifter", Shifter.color, 1);
            shifterIsNeutralRate = CustomOption.Create(6007, Types.Neutral, "shifterIsNeutralRate", rates, shifterSpawnRate);
            shifterShiftsModifiers = CustomOption.Create(1101, Types.Neutral, "shifterShiftsModifiers", false, shifterSpawnRate);
            shifterShiftsMedicShield = CustomOption.Create(1102, Types.Neutral, "shifterShiftsMedicShield", false, shifterSpawnRate);
            shifterPastShifters = CustomOption.Create(6008, Types.Neutral, "shifterPastShifters", false, shifterSpawnRate);

            opportunistSpawnRate = new CustomRoleOption(4003, Types.Neutral, "opportunist", Opportunist.color);

            plagueDoctorSpawnRate = new CustomRoleOption(6000, Types.Neutral, "plagueDoctor", PlagueDoctor.color, 1);
            plagueDoctorInfectCooldown = CustomOption.Create(6001, Types.Neutral, "plagueDoctorInfectCooldown", 10f, 2.5f, 60f, 2.5f, plagueDoctorSpawnRate, false, "unitSeconds");
            plagueDoctorNumInfections = CustomOption.Create(6002, Types.Neutral, "plagueDoctorNumInfections", 1f, 1f, 3f, 1f, plagueDoctorSpawnRate, false, "unitPlayers");
            plagueDoctorDistance = CustomOption.Create(6003, Types.Neutral, "plagueDoctorDistance", 1f, 0.25f, 5f, 0.25f, plagueDoctorSpawnRate, false, "unitMeters");
            plagueDoctorDuration = CustomOption.Create(6004, Types.Neutral, "plagueDoctorDuration", 5f, 1f, 30f, 1f, plagueDoctorSpawnRate, false, "unitSeconds");
            plagueDoctorImmunityTime = CustomOption.Create(6005, Types.Neutral, "plagueDoctorImmunityTime", 10f, 1f, 30f, 1f, plagueDoctorSpawnRate, false, "unitSeconds");
            plagueDoctorInfectKiller = CustomOption.Create(6006, Types.Neutral, "plagueDoctorInfectKiller", true, plagueDoctorSpawnRate);
            plagueDoctorWinDead = CustomOption.Create(5999, Types.Neutral, "plagueDoctorWinDead", true, plagueDoctorSpawnRate);

            kataomoiSpawnRate = new CustomRoleOption(8300, Types.Neutral, "kataomoi", Kataomoi.color, 1);
            kataomoiStareCooldown = CustomOption.Create(8301, Types.Neutral, "kataomoiStareCooldown", 20f, 2.5f, 60f, 2.5f, kataomoiSpawnRate, false, "unitSeconds");
            kataomoiStareDuration = CustomOption.Create(8302, Types.Neutral, "kataomoiStareDuration", 3f, 1f, 10f, 1f, kataomoiSpawnRate, false, "unitSeconds");
            kataomoiStareCount = CustomOption.Create(8303, Types.Neutral, "kataomoiStareCount", 5f, 1f, 100f, 1f, kataomoiSpawnRate, false, "unitShots");
            kataomoiStalkingCooldown = CustomOption.Create(8304, Types.Neutral, "kataomoiStalkingCooldown", 20f, 2.5f, 60f, 2.5f, kataomoiSpawnRate, false, "unitSeconds");
            kataomoiStalkingDuration = CustomOption.Create(8305, Types.Neutral, "kataomoiStalkingDuration", 10f, 1f, 30f, 1f, kataomoiSpawnRate, false, "unitSeconds");
            kataomoiStalkingFadeTime = CustomOption.Create(8306, Types.Neutral, "kataomoiStalkingFadeTime", 0.5f, 0.0f, 2.5f, 0.5f, kataomoiSpawnRate, false, "unitSeconds");
            kataomoiSearchCooldown = CustomOption.Create(8307, Types.Neutral, "kataomoiSearchCooldown", 10f, 2.5f, 60f, 2.5f, kataomoiSpawnRate, false, "unitSeconds");
            kataomoiSearchDuration = CustomOption.Create(8308, Types.Neutral, "kataomoiSearchDuration", 10f, 1f, 30f, 1f, kataomoiSpawnRate, false, "unitSeconds");

            schrodingersCatSpawnRate = new CustomRoleOption(8400, Types.Neutral, "schrodingersCat", SchrodingersCat.color, 1);
            schrodingersCatKillCooldown = CustomOption.Create(971, Types.Neutral, "schrodingersCatKillCooldown", 20f, 1f, 60f, 0.5f, schrodingersCatSpawnRate, format: "unitSeconds");
            schrodingersCatBecomesImpostor = CustomOption.Create(972, Types.Neutral, "schrodingersCatBecomesImpostor", true, schrodingersCatSpawnRate);
            schrodingersCatCantKillUntilLastOne = CustomOption.Create(974, Types.Neutral, "schrodingersCatCantKillUntilLastOne", false, schrodingersCatSpawnRate);
            schrodingersCatJustDieOnKilledByCrew = CustomOption.Create(976, Types.Neutral, "schrodingersCatJustDieOnKilledByCrew", false, schrodingersCatSpawnRate);
            schrodingersCatHideRole = CustomOption.Create(977, Types.Neutral, "schrodingersCatHideRole", false, schrodingersCatSpawnRate);
            schrodingersCatCanChooseImpostor = CustomOption.Create(979, Types.Neutral, "schrodingersCatCanChooseTeam", false, schrodingersCatHideRole);

            doomsayerSpawnRate = new CustomRoleOption(9040, Types.Neutral, "doomsayer", Doomsayer.color);
            doomsayerCanObserve = CustomOption.Create(9044, Types.Neutral, "doomsayerCanObserve", true, doomsayerSpawnRate);
            doomsayerObserveCooldown = CustomOption.Create(9041, Types.Neutral, "doomsayerObserveCooldown", 30f, 5f, 60f, 1f, doomsayerCanObserve, format: "unitSeconds");
            doomsayerNumberOfObserves = CustomOption.Create(9047, Types.Neutral, "doomsayerNumberOfObserves", 3f, 1f, 10f, 1f, doomsayerCanObserve, format: "unitShots");
            doomsayerGuessesToWin = CustomOption.Create(9042, Types.Neutral, "doomsayerGuessesToWin", 3f, 1f, 24f, 1f, doomsayerSpawnRate, format: "unitScrews");
            doomsayerMultipleGuesses = CustomOption.Create(9045, Types.Neutral, "doomsayerMultipleGuesses", true, doomsayerSpawnRate);
            doomsayerMaxMisses = CustomOption.Create(9046, Types.Neutral, "doomsayerMaxMisses", 3f, 0f, 24f, 1f, doomsayerSpawnRate, format: "unitShots");
            doomsayerIndicator = CustomOption.Create(9043, Types.Neutral, "doomsayerIndicator", true, doomsayerSpawnRate);

            akujoSpawnRate = new CustomRoleOption(8100, Types.Neutral, "akujo", Akujo.color, 7);
            akujoTimeLimit = CustomOption.Create(8101, Types.Neutral, "akujoTimeLimit", 300f, 30f, 1200f, 30f, akujoSpawnRate, false, "unitSeconds");
            akujoNumKeeps = CustomOption.Create(8102, Types.Neutral, "akujoNumKeeps", 2f, 1f, 10f, 1f, akujoSpawnRate, false, "unitPlayers");
            akujoKnowsRoles = CustomOption.Create(8103, Types.Neutral, "akujoKnowsRoles", true, akujoSpawnRate);

            cupidSpawnRate = new CustomRoleOption(9050, Types.Neutral, "cupid", Cupid.color, 3);
            cupidTimeLimit = CustomOption.Create(9051, Types.Neutral, "cupidTimeLimit", 300f, 30f, 1200f, 30f, cupidSpawnRate, false, "unitSeconds");
            cupidShield = CustomOption.Create(9052, Types.Neutral, "cupidShield", true, cupidSpawnRate);

            pelicanSpawnRate = new CustomRoleOption(9060, Types.Neutral, "pelican", Pelican.color);
            pelicanCooldown = CustomOption.Create(9061, Types.Neutral, "pelicanCooldown", 25f, 2.5f, 60f, 2.5f, pelicanSpawnRate, false, "unitSeconds");
            pelicanCanUseVents = CustomOption.Create(9063, Types.Neutral, "pelicanCanUseVents", true, pelicanSpawnRate);
            pelicanHasImpVision = CustomOption.Create(9064, Types.Neutral, "pelicanHasImpVision", true, pelicanSpawnRate);

            yandereSpawnRate = new CustomRoleOption(8500, Types.Neutral, "yandere", Yandere.color, 1);
            yandereReducedCooldown = CustomOption.Create(8501, Types.Neutral, "yandereReducedCooldown", 5f, 0f, 30f, 1f, yandereSpawnRate, false, "unitSeconds");
            yandereCooldownPunishment = CustomOption.Create(8502, Types.Neutral, "yandereCooldownPunishment", 10f, 0f, 60f, 1f, yandereSpawnRate, false, "unitSeconds");
            yandereNuisanceRange = CustomOption.Create(8503, Types.Neutral, "yandereNuisanceRange", 1.8f, 0.5f, 5f, 0.1f, yandereSpawnRate, false, "unitMeters");
            yandereNuisanceRegisterDuration = CustomOption.Create(8504, Types.Neutral, "yandereNuisanceRegisterDuration", 2f, 0.1f, 7.5f, 0.1f, yandereSpawnRate, false, "unitSeconds");
            yandereMaxNuisance = CustomOption.Create(8505, Types.Neutral, "yandereMaxNuisance", 4f, 1f, 24f, 1f, yandereSpawnRate, false, "unitScrews");
            yandereRunawayTimeLimit = CustomOption.Create(8507, Types.Neutral, "yandereRunawayTimeLimit", 60f, 10f, 180f, 10f, yandereSpawnRate, false, "unitSeconds");
            yandereHasImpVision = CustomOption.Create(8506, Types.Neutral, "yandereHasImpVision", true, yandereSpawnRate);

            jekyllAndHydeSpawnRate = new CustomRoleOption(8104, Types.Neutral, "jekyllAndHyde", JekyllAndHyde.color);
            jekyllAndHydeNumberToWin = CustomOption.Create(8105, Types.Neutral, "jekyllAndHydeNumberToWin", 3f, 1f, 10f, 1f, jekyllAndHydeSpawnRate, false, "unitScrews");
            jekyllAndHydeCooldown = CustomOption.Create(8106, Types.Neutral, "jekyllAndHydeCooldown", 18f, 2f, 30f, 1f, jekyllAndHydeSpawnRate, false, "unitSeconds");
            jekyllAndHydeSuicideTimer = CustomOption.Create(8107, Types.Neutral, "jekyllAndHydeSuicideTimer", 40f, 2.5f, 60f, 2.5f, jekyllAndHydeSpawnRate, false, "unitSeconds");
            jekyllAndHydeResetAfterMeeting = CustomOption.Create(8112, Types.Neutral, "jekyllAndHydeResetAfterMeeting", true, jekyllAndHydeSpawnRate);
            jekyllAndHydeCommonTasks = CustomOption.Create(8108, Types.Neutral, "jekyllAndHydeCommonTasks", 1f, 1f, 4f, 1f, jekyllAndHydeSpawnRate, false, "unitScrews");
            jekyllAndHydeShortTasks = CustomOption.Create(8109, Types.Neutral, "jekyllAndHydeShortTasks", 3f, 1f, 20f, 1f, jekyllAndHydeSpawnRate, false, "unitScrews");
            jekyllAndHydeLongTasks = CustomOption.Create(8110, Types.Neutral, "jekyllAndHydeLongTasks", 2f, 0f, 6f, 1f, jekyllAndHydeSpawnRate, false, "unitScrews");
            jekyllAndHydeNumTasks = CustomOption.Create(8111, Types.Neutral, "jekyllAndHydeNumTasks", 3f, 1f, 10f, 1f, jekyllAndHydeSpawnRate, false, "unitScrews");

            foxSpawnRate = new CustomRoleOption(910, Types.Neutral, "fox", Fox.color, 1);
            foxNumTasks = CustomOption.Create(911, Types.Neutral, "foxNumTasks", 4f, 1f, 10f, 1f, foxSpawnRate, false, "unitScrews");
            foxStayTime = CustomOption.Create(913, Types.Neutral, "foxStayTime", 5f, 1f, 20f, 1f, foxSpawnRate, false, "unitSeconds");
            foxTaskType = CustomOption.Create(914, Types.Neutral, "foxTaskType", ["foxTaskSerial", "foxTaskParallel"], foxSpawnRate);
            foxCrewWinsByTasks = CustomOption.Create(912, Types.Neutral, "foxCrewWinsByTasks", true, foxSpawnRate);
            foxImpostorWinsBySabotage = CustomOption.Create(919, Types.Neutral, "foxImpostorWinsBySabotage", true, foxSpawnRate);
            foxStealthCooldown = CustomOption.Create(916, Types.Neutral, "foxStealthCooldown", 15f, 1f, 30f, 1f, foxSpawnRate, false, "unitSeconds");
            foxStealthDuration = CustomOption.Create(917, Types.Neutral, "foxStealthDuration", 15f, 1f, 30f, 1f, foxSpawnRate, false, "unitSeconds");
            foxCanCreateImmoralist = CustomOption.Create(918, Types.Neutral, "foxCanCreateImmoralist", true, foxSpawnRate);
            foxNumRepairs = CustomOption.Create(920, Types.Neutral, "foxNumRepair", 1f, 0f, 10f, 1f, foxSpawnRate, false, "unitShots");

            mayorSpawnRate = new CustomRoleOption(80, Types.Crewmate, "mayor", Mayor.color);
            mayorNumVotes = CustomOption.Create(81, Types.Crewmate, "mayorNumVotes", 2f, 2f, 24f, 1f, mayorSpawnRate, false, "unitVotes");
            mayorMeetingButton = CustomOption.Create(83, Types.Crewmate, "mayorMeetingButton", true, mayorSpawnRate);
            mayorMaxRemoteMeetings = CustomOption.Create(84, Types.Crewmate, "mayorMaxRemoteMeetings", 1f, 1f, 5f, 1f, mayorMeetingButton, false, "unitShots");
            mayorChooseSingleVote = CustomOption.Create(85, Types.Crewmate, "mayorChooseSingleVote", ["optionOff", "mayorOnBeforeVoting", "mayorOnUntilMeeting"], mayorSpawnRate);

            engineerSpawnRate = new CustomRoleOption(90, Types.Crewmate, "engineer", Engineer.color);
            engineerNumberOfFixes = CustomOption.Create(91, Types.Crewmate, "engineerNumberOfFixes", 1f, 1f, 6f, 1f, engineerSpawnRate, false, "unitShots");
            engineerHighlightForImpostors = CustomOption.Create(92, Types.Crewmate, "engineerHighlightForImpostors", true, engineerSpawnRate);
            engineerHighlightForTeamJackal = CustomOption.Create(93, Types.Crewmate, "engineerHighlightForTeamJackal", true, engineerSpawnRate);

            sheriffSpawnRate = new CustomRoleOption(100, Types.Crewmate, "sheriff", Sheriff.color);
            sheriffCooldown = CustomOption.Create(101, Types.Crewmate, "sheriffCooldown", 30f, 10f, 60f, 2.5f, sheriffSpawnRate, false, "unitSeconds");
            sheriffCanKillNeutrals = CustomOption.Create(102, Types.Crewmate, "sheriffCanKillNeutrals", false, sheriffSpawnRate);
            deputySpawnRate = CustomOption.Create(103, Types.Crewmate, "sheriffDeputy", rates, sheriffSpawnRate); 
            deputyRoleCount = CustomOption.Create(4017, Types.Crewmate, "deputyRoleCount", 1f, 1f, 24f, 1f, deputySpawnRate, format: "unitPlayers");
            deputyNumberOfHandcuffs = CustomOption.Create(104, Types.Crewmate, "deputyNumberOfHandcuffs", 3f, 1f, 10f, 1f, deputySpawnRate, false, "unitScrews");
            deputyHandcuffCooldown = CustomOption.Create(105, Types.Crewmate, "deputyHandcuffCooldown", 30f, 10f, 60f, 2.5f, deputySpawnRate, false, "unitSeconds");
            deputyHandcuffDuration = CustomOption.Create(106, Types.Crewmate, "deputyHandcuffDuration", 15f, 5f, 60f, 2.5f, deputySpawnRate, false, "unitSeconds");
            deputyKnowsSheriff = CustomOption.Create(107, Types.Crewmate, "deputyKnowsSheriff", true, deputySpawnRate);
            deputyGetsPromoted = CustomOption.Create(108, Types.Crewmate, "deputyGetsPromoted", ["optionOff", "deputyOnImmediately", "deputyOnAfterMeeting"], deputySpawnRate);
            deputyKeepsHandcuffs = CustomOption.Create(109, Types.Crewmate, "deputyKeepsHandcuffs", true, deputyGetsPromoted);
            deputyStopsGameEnd = CustomOption.Create(4016, Types.Crewmate, "deputyStopsGameEnd", false, deputySpawnRate);

            lighterSpawnRate = new CustomRoleOption(110, Types.Crewmate, "lighter", Lighter.color);
            lighterModeLightsOnVision = CustomOption.Create(111, Types.Crewmate, "lighterModeLightsOnVision", 1.5f, 0.25f, 5f, 0.25f, lighterSpawnRate, false, "unitTimes");
            lighterModeLightsOffVision = CustomOption.Create(112, Types.Crewmate, "lighterModeLightsOffVision", 0.5f, 0.25f, 5f, 0.25f, lighterSpawnRate, false, "unitTimes");
            lighterCooldown = CustomOption.Create(115, Types.Crewmate, "lighterCooldown", 30f, 5f, 120f, 5f, lighterSpawnRate, format: "unitSeconds");
            lighterDuration = CustomOption.Create(116, Types.Crewmate, "lighterDuration", 5f, 2.5f, 60f, 2.5f, lighterSpawnRate, format: "unitSeconds");
            lighterCanSeeInvisible = CustomOption.Create(114, Types.Crewmate, "lighterCanSeeInvisible", true, lighterSpawnRate);

            sprinterSpawnRate = new CustomRoleOption(4005, Types.Crewmate, "sprinter", Sprinter.color);
            sprinterCooldown = CustomOption.Create(4006, Types.Crewmate, "sprinterCooldown", 30f, 2.5f, 60f, 2.5f, sprinterSpawnRate, false, "unitSeconds");
            sprinterDuration = CustomOption.Create(4007, Types.Crewmate, "sprintDuration", 15f, 10f, 60f, 2.5f, sprinterSpawnRate, false, "unitSeconds");
            sprinterFadeTime = CustomOption.Create(4008, Types.Crewmate, "sprintFadeTime", 0.5f, 0.0f, 2.5f, 0.5f, sprinterSpawnRate, false, "unitSeconds");
            sprinterSpeedBonus = CustomOption.Create(4009, Types.Crewmate, "sprinterSpeedBonus", 1.25f, 0.5f, 2f, 0.25f, sprinterSpawnRate, false, "unitTimes");

            detectiveSpawnRate = new CustomRoleOption(120, Types.Crewmate, "detective", Detective.color);
            detectiveAnonymousFootprints = CustomOption.Create(121, Types.Crewmate, "detectiveAnonymousFootprints", false, detectiveSpawnRate);
            detectiveFootprintIntervall = CustomOption.Create(122, Types.Crewmate, "detectiveFootprintInterval", 0.5f, 0.25f, 10f, 0.25f, detectiveSpawnRate, false, "unitSeconds");
            detectiveFootprintDuration = CustomOption.Create(123, Types.Crewmate, "detectiveFootprintDuration", 5f, 0.25f, 10f, 0.25f, detectiveSpawnRate, false, "unitSeconds");
            detectiveReportNameDuration = CustomOption.Create(124, Types.Crewmate, "detectiveReportNameDuration", 0, 0, 60, 2.5f, detectiveSpawnRate, false, "unitSeconds");
            detectiveReportColorDuration = CustomOption.Create(125, Types.Crewmate, "detectiveReportColorDuration", 20, 0, 120, 2.5f, detectiveSpawnRate, false, "unitSeconds");
            detectiveInspectCooldown = CustomOption.Create(126, Types.Crewmate, "detectiveInspectCooldown", 15f, 5f, 60f, 1f, detectiveSpawnRate, format: "unitSeconds");
            detectiveInspectDuration = CustomOption.Create(127, Types.Crewmate, "detectiveInspectDuration", 10f, 3f, 60f, 1f, detectiveSpawnRate, false, "unitSeconds");

            timeMasterSpawnRate = new CustomRoleOption(130, Types.Crewmate, "timeMaster", TimeMaster.color);
            timeMasterCooldown = CustomOption.Create(131, Types.Crewmate, "timeMasterCooldown", 30f, 10f, 120f, 2.5f, timeMasterSpawnRate, false, "unitSeconds");
            timeMasterRewindTime = CustomOption.Create(132, Types.Crewmate, "timeMasterRewindTime", 3f, 1f, 10f, 1f, timeMasterSpawnRate, false, "unitSeconds");
            timeMasterShieldDuration = CustomOption.Create(133, Types.Crewmate, "timeMasterShieldDuration", 3f, 1f, 20f, 1f, timeMasterSpawnRate, false, "unitSeconds");
            timeMasterReviveDuringRewind = CustomOption.Create(135, Types.Crewmate, "timeMasterRewindDuringRewind", false, timeMasterSpawnRate);

            medicSpawnRate = new CustomRoleOption(140, Types.Crewmate, "medic", Medic.color);
            medicShowShielded = CustomOption.Create(143, Types.Crewmate, "medicShowShielded", ["medicShowShieldedAll", "medicShowShieldedBoth", "medicShowShieldedMedic"], medicSpawnRate);
            medicShowAttemptToShielded = CustomOption.Create(144, Types.Crewmate, "medicShowAttemptToShielded", false, medicSpawnRate);
            medicSetOrShowShieldAfterMeeting = CustomOption.Create(145, Types.Crewmate, "medicSetOrShowShieldAfterMeeting", ["medicInstantly", "medicVisibleAfterMeeting", "medicAftermeeting"], medicSpawnRate);
            medicShowAttemptToMedic = CustomOption.Create(146, Types.Crewmate, "medicShowAttemptToMedic", false, medicSpawnRate);
            medicCanUseVitals = CustomOption.Create(147, Types.Crewmate, "medicCanUseVitals", true, medicSpawnRate);
            medicSeesDeathReasonOnVitals = CustomOption.Create(148, Types.Crewmate, "medicSeesDeathReasonOnVitals", true, medicCanUseVitals);

            fortuneTellerSpawnRate = new CustomRoleOption(940, Types.Crewmate, "fortuneTeller", FortuneTeller.color);
            fortuneTellerResults = CustomOption.Create(941, Types.Crewmate, "fortuneTellerResults", ["fortuneTellerResultCrew", "fortuneTellerResultTeam", "fortuneTellerResultRole"], fortuneTellerSpawnRate);
            fortuneTellerNumTasks = CustomOption.Create(942, Types.Crewmate, "fortuneTellerNumTasks", 4f, 0f, 25f, 1f, fortuneTellerSpawnRate, false, "unitScrews");
            fortuneTellerDuration = CustomOption.Create(943, Types.Crewmate, "fortuneTellerDuration", 20f, 1f, 50f, 1f, fortuneTellerSpawnRate, false, "unitSeconds");
            fortuneTellerDistance = CustomOption.Create(944, Types.Crewmate, "fortuneTellerDistance", 2.5f, 1f, 10f, 0.5f, fortuneTellerSpawnRate, false, "unitMeters");
            fortuneTellerRevealOnImpDivine = CustomOption.Create(955, Types.Crewmate, "fortuneTellerRevealOnImpDivine", true, fortuneTellerSpawnRate);

            collatorSpawnRate = new CustomRoleOption(945, Types.Crewmate, "collator", Collator.color);
            collatorCooldown = CustomOption.Create(946, Types.Crewmate, "collatorCooldown", 15f, 1f, 60f, 1f, collatorSpawnRate, false, "unitSeconds");
            collatorNumberOfTrials = CustomOption.Create(947, Types.Crewmate, "collatorNumberOfTrials", 2f, 1f, 15f, 1f, collatorSpawnRate, false, "unitScrews");
            collatorMadmateSpecifiedAsCrewmate = CustomOption.Create(948, Types.Crewmate, "collatorMadmateSpecifiedAsCrewmate", true, collatorSpawnRate);
            collatorStrictNeutralRoles = CustomOption.Create(949, Types.Crewmate, "collatorStrictNeutralRoles", false, collatorSpawnRate);

            seerSpawnRate = new CustomRoleOption(160, Types.Crewmate, "seer", Seer.color);
            seerMode = CustomOption.Create(161, Types.Crewmate, "seerMode", ["seerModeBoth", "seerModeFlash", "seerModeSouls"], seerSpawnRate);
            seerLimitSoulDuration = CustomOption.Create(163, Types.Crewmate, "seerLimitSoulDuration", false, seerSpawnRate);
            seerSoulDuration = CustomOption.Create(162, Types.Crewmate, "seerSoulDuration", 15f, 0f, 120f, 5f, seerLimitSoulDuration, false, "unitSeconds");
            seerCanSeeKillTeams = CustomOption.Create(164, Types.Crewmate, "seerCanSeeKillTeams", true, seerSpawnRate);

            hackerSpawnRate = new CustomRoleOption(170, Types.Crewmate, "hacker", Hacker.color);
            hackerCooldown = CustomOption.Create(171, Types.Crewmate, "hackerCooldown", 30f, 5f, 60f, 5f, hackerSpawnRate, false, "unitSeconds");
            hackerHackeringDuration = CustomOption.Create(172, Types.Crewmate, "hackerHackeringDuration", 10f, 2.5f, 60f, 2.5f, hackerSpawnRate, false, "unitSeconds");
            hackerOnlyColorType = CustomOption.Create(173, Types.Crewmate, "hackerOnlyColorType", false, hackerSpawnRate);
            hackerToolsNumber = CustomOption.Create(174, Types.Crewmate, "hackerToolsNumber", 5f, 1f, 30f, 1f, hackerSpawnRate, false, "unitScrews");
            hackerRechargeTasksNumber = CustomOption.Create(175, Types.Crewmate, "hackerRechargeTasksNumber", 2f, 1f, 5f, 1f, hackerSpawnRate, false, "unitScrews");
            hackerNoMove = CustomOption.Create(176, Types.Crewmate, "hackerNoMove", true, hackerSpawnRate);

            noisemakerSpawnRate = new CustomRoleOption(9090, Types.Crewmate, "noisemaker", Noisemaker.color);
            noisemakerCooldown = CustomOption.Create(9091, Types.Crewmate, "noisemakerCooldown", 30f, 5f, 120f, 2.5f, noisemakerSpawnRate, false, "unitSeconds");
            noisemakerSoundDuration = CustomOption.Create(9092, Types.Crewmate, "noisemakerSoundDuration", 5f, 2.5f, 20f, 2.5f, noisemakerSpawnRate, false, "unitSeconds");
            noisemakerSoundNumber = CustomOption.Create(9093, Types.Crewmate, "noisemakerSoundNumber", 5f, 3f, 20f, 1f, noisemakerSpawnRate, false, "unitScrews");
            noisemakerSoundTarget = CustomOption.Create(9094, Types.Crewmate, "noisemakerSoundTarget", ["noisemakerSoundNoisemaker", "noisemakerSoundCrewmate", "noisemakerSoundEveryone"], noisemakerSpawnRate);

            baitSpawnRate = new CustomRoleOption(1030, Types.Crewmate, "bait", Bait.color);
            baitHighlightAllVents = CustomOption.Create(1031, Types.Crewmate, "baitHighlightAllVents", false, baitSpawnRate);
            baitReportDelay = CustomOption.Create(1032, Types.Crewmate, "baitReportDelay", 0f, 0f, 10f, 1f, baitSpawnRate, false, "unitSeconds");
            baitShowKillFlash = CustomOption.Create(1033, Types.Crewmate, "baitShowKillFlash", true, baitSpawnRate);
            baitCanBeGuessed = CustomOption.Create(1034, Types.Crewmate, "baitCanBeGuessed", true, baitSpawnRate);
            baitEmitCooldown = CustomOption.Create(9035, Types.Crewmate, "baitEmitCooldown", 30f, 1f, 60f, 1f, baitSpawnRate, false, "unitSeconds");
            baitNumberOfEmits = CustomOption.Create(9036, Types.Crewmate, "baitNumberOfEmits", 5f, 1f, 10f, 1f, baitSpawnRate, false, "unitScrews");

            veteranSpawnRate = new CustomRoleOption(4050, Types.Crewmate, "veteran", Veteran.color);
            veteranCooldown = CustomOption.Create(4051, Types.Crewmate, "veteranCooldown", 30f, 10f, 60f, 2.5f, veteranSpawnRate, false, "unitSeconds");
            veteranAlertDuration = CustomOption.Create(4052, Types.Crewmate, "veteranAlertDuration", 3f, 1f, 20f, 1f, veteranSpawnRate, false, "unitSeconds");
            veteranAlertNumber = CustomOption.Create(4053, Types.Crewmate, "veteranAlertNumber", 5f, 1f, 15f, 1f, veteranSpawnRate, false, "unitScrews");

            jailorSpawnRate = new CustomRoleOption(4054, Types.Crewmate, "jailor", Jailor.color);
            jailorCooldown = CustomOption.Create(4055, Types.Crewmate, "jailorCooldown", 30f, 1f, 60f, 1f, jailorSpawnRate, false, "unitSeconds");
            jailorNumberOfJails = CustomOption.Create(4076, Types.Crewmate, "jailorNumberOfJails", 5f, 1f, 15f, 1f, jailorSpawnRate, false, "unitScrews");
            jailorSuicidesIfFalseJail = CustomOption.Create(4077, Types.Crewmate, "jailorSuicidesIfFalseJail", true, jailorSpawnRate);
            jailorTargetDiesIfFalseJail = CustomOption.Create(4078, Types.Crewmate, "jailorTargetDiesIfFalseJail", false, jailorSpawnRate);

            buskerSpawnRate = new CustomRoleOption(8040, Types.Crewmate, "busker", Busker.color);
            buskerCooldown = CustomOption.Create(8041, Types.Crewmate, "buskerCooldown", 20f, 5f, 60f, 2.5f, buskerSpawnRate, false, "unitSeconds");
            buskerDuration = CustomOption.Create(8042, Types.Crewmate, "buskerDuration", 10f, 5f, 30f, 2.5f, buskerSpawnRate, false, "unitSeconds");
            buskerRestrictInformation = CustomOption.Create(8043, Types.Crewmate, "buskerRestrictInformation", true, buskerSpawnRate);

            teleporterSpawnRate = new CustomRoleOption(9000, Types.Crewmate, "teleporter", Teleporter.color);
            teleporterCooldown = CustomOption.Create(9001, Types.Crewmate, "teleporterCooldown", 30f, 5f, 120f, 5f, teleporterSpawnRate, false, "unitSeconds");
            teleporterTeleportNumber = CustomOption.Create(9003, Types.Crewmate, "teleporterTeleportNumber", 3f, 1f, 10f, 1f, teleporterSpawnRate, false, "unitScrews");

            trackerSpawnRate = new CustomRoleOption(200, Types.Crewmate, "tracker", Tracker.color);
            trackerUpdateIntervall = CustomOption.Create(201, Types.Crewmate, "trackerUpdateInterval", 5f, 1f, 30f, 1f, trackerSpawnRate, false, "unitSeconds");
            trackerResetTargetAfterMeeting = CustomOption.Create(202, Types.Crewmate, "trackerResetTargetAfterMeeting", false, trackerSpawnRate);
            trackerCanTrackCorpses = CustomOption.Create(203, Types.Crewmate, "trackerCanTrackCorpses", true, trackerSpawnRate);
            trackerCorpsesTrackingCooldown = CustomOption.Create(204, Types.Crewmate, "trackerCorpsesTrackingCooldown", 30f, 5f, 120f, 5f, trackerCanTrackCorpses, false, "unitSeconds");
            trackerCorpsesTrackingDuration = CustomOption.Create(205, Types.Crewmate, "trackerCorpsesTrackingDuration", 5f, 2.5f, 30f, 2.5f, trackerCanTrackCorpses, false, "unitSeconds");
            trackerTrackingMethod = CustomOption.Create(206, Types.Crewmate, "trackerTrackingMethod", ["trackerArrow", "trackerProximity", "trackerBoth"], trackerSpawnRate);
            trackerCanKill = CustomOption.Create(208, Types.Crewmate, "trackerCanKill", true, trackerSpawnRate);
            trackerKillCooldown = CustomOption.Create(207, Types.Crewmate, "trackerKillCooldown", 30f, 5f, 60f, 2.5f, trackerCanKill, false, "unitSeconds");

            sherlockSpawnRate = new CustomRoleOption(5070, Types.Crewmate, "sherlock", Sherlock.color);
            sherlockCooldown = CustomOption.Create(5071, Types.Crewmate, "sherlockCooldown", 10f, 0f, 40f, 2.5f, sherlockSpawnRate, false, "unitSeconds");
            sherlockInvestigateDistance = CustomOption.Create(5072, Types.Crewmate, "sherlockInvestigateDistance", 5f, 1f, 15f, 1f, sherlockSpawnRate, false, "unitMeters");
            sherlockRechargeTasksNumber = CustomOption.Create(5073, Types.Crewmate, "sherlockRechargeTasksNumber", 2f, 1f, 5f, 1f, sherlockSpawnRate, false, "unitScrews");

            snitchSpawnRate = new CustomRoleOption(210, Types.Crewmate, "snitch", Snitch.color);
            snitchLeftTasksForReveal = CustomOption.Create(219, Types.Crewmate, "snitchLeftTasksForReveal", 5f, 0f, 25f, 1f, snitchSpawnRate, false, "unitScrews");
            snitchIncludeTeamEvil = CustomOption.Create(211, Types.Crewmate, "snitchIncludeTeamEvil", true, snitchSpawnRate);
            snitchTeamEvilUseDifferentArrowColor = CustomOption.Create(212, Types.Crewmate, "snitchTeamEvilUseDifferentArrowColor", true, snitchIncludeTeamEvil);

            archaeologistSpawnRate = new CustomRoleOption(7030, Types.Crewmate, "archaeologist", Archaeologist.color, 1);
            archaeologistCooldown = CustomOption.Create(7031, Types.Crewmate, "archaeologistCooldown", 20f, 5f, 60f, 1f, archaeologistSpawnRate, format: "unitSeconds");
            archaeologistArrowDuration = CustomOption.Create(7034, Types.Crewmate, "archaeologistArrowDuration", 5f, 1f, 60f, 1f, archaeologistSpawnRate, format: "unitSeconds");
            archaeologistExploreDuration = CustomOption.Create(7032, Types.Crewmate, "archaeologistExploreDuration", 3f, 0f, 15f, 1f, archaeologistSpawnRate, format: "unitSeconds");
            archaeologistNumCandidates = CustomOption.Create(7036, Types.Crewmate, "archaeologistNumCandidates", 3f, 2f, 6f, 1f, archaeologistSpawnRate, format: "unitPlayers");
            archaeologistRevealAntiqueMode = CustomOption.Create(7035, Types.Crewmate, "archaeologistRevealAntiqueMode", ["archaeologistModeNever", "archaeologistModeImmediately", "archaeologistModeAfterMeeting"], archaeologistSpawnRate);

            spySpawnRate = new CustomRoleOption(240, Types.Crewmate, "spy", Spy.color);
            spyCanDieToSheriff = CustomOption.Create(241, Types.Crewmate, "spyCanDieToSheriff", false, spySpawnRate);
            spyImpostorsCanKillAnyone = CustomOption.Create(242, Types.Crewmate, "spyImpostorsCanKillAnyone", true, spySpawnRate);
            spyCanEnterVents = CustomOption.Create(243, Types.Crewmate, "spyCanEnterVents", false, spySpawnRate);
            spyHasImpostorVision = CustomOption.Create(244, Types.Crewmate, "spyHasImpostorVision", false, spySpawnRate);

            taskMasterSpawnRate = new CustomRoleOption(7020, Types.Crewmate, "taskMaster", TaskMaster.color, 1);
            taskMasterBecomeATaskMasterWhenCompleteAllTasks = CustomOption.Create(7021, Types.Crewmate, "taskMasterBecomeATaskMasterWhenCompleteAllTasks", false, taskMasterSpawnRate);
            taskMasterExtraCommonTasks = CustomOption.Create(7022, Types.Crewmate, "taskMasterExtraCommonTasks", 2f, 0f, 3f, 1f, taskMasterSpawnRate, false, "unitScrews");
            taskMasterExtraShortTasks = CustomOption.Create(7023, Types.Crewmate, "taskMasterExtraShortTasks", 2f, 1f, 23f, 1f, taskMasterSpawnRate, false, "unitScrews");
            taskMasterExtraLongTasks = CustomOption.Create(7024, Types.Crewmate, "taskMasterExtraLongTasks", 2f, 0f, 15f, 1f, taskMasterSpawnRate, false, "unitScrews");
            taskMasterCanVent = CustomOption.Create(7025, Types.Crewmate, "taskMasterCanVent", true, taskMasterSpawnRate);

            portalmakerSpawnRate = new CustomRoleOption(390, Types.Crewmate, "portalmaker", Portalmaker.color, 1);
            portalmakerCooldown = CustomOption.Create(391, Types.Crewmate, "portalmakerCooldown", 30f, 10f, 60f, 2.5f, portalmakerSpawnRate, false, "unitSeconds");
            portalmakerUsePortalCooldown = CustomOption.Create(392, Types.Crewmate, "portalmakerUsePortalCooldown", 30f, 10f, 60f, 2.5f, portalmakerSpawnRate, false, "unitSeconds");
            portalmakerLogOnlyColorType = CustomOption.Create(393, Types.Crewmate, "portalmakerLogOnlyColorType", true, portalmakerSpawnRate);
            portalmakerLogHasTime = CustomOption.Create(394, Types.Crewmate, "portalmakerLogHasTime", true, portalmakerSpawnRate);
            portalmakerCanPortalFromAnywhere = CustomOption.Create(395, Types.Crewmate, "portalmakerCanPortalFromAnywhere", true, portalmakerSpawnRate);

            securityGuardSpawnRate = new CustomRoleOption(280, Types.Crewmate, "securityGuard", SecurityGuard.color, 1);
            securityGuardCooldown = CustomOption.Create(281, Types.Crewmate, "securityGuardCooldown", 30f, 10f, 60f, 2.5f, securityGuardSpawnRate, false, "unitSeconds");
            securityGuardTotalScrews = CustomOption.Create(282, Types.Crewmate, "securityGuardTotalScrews", 7f, 1f, 15f, 1f, securityGuardSpawnRate, false, "unitScrews");
            securityGuardCamPrice = CustomOption.Create(283, Types.Crewmate, "securityGuardCamPrice", 2f, 1f, 15f, 1f, securityGuardSpawnRate, false, "unitScrews");
            securityGuardVentPrice = CustomOption.Create(284, Types.Crewmate, "securityGuardVentPrice", 1f, 1f, 15f, 1f, securityGuardSpawnRate, false, "unitScrews");
            securityGuardFlushCooldown = CustomOption.Create(289, Types.Crewmate, "securityGuardFlushCooldown", 30f, 5f, 120f, 1f, securityGuardSpawnRate, false, "unitSeconds");
            securityGuardCamDuration = CustomOption.Create(285, Types.Crewmate, "securityGuardCamDuration", 10f, 2.5f, 60f, 2.5f, securityGuardSpawnRate, false, "unitSeconds");
            securityGuardCamMaxCharges = CustomOption.Create(286, Types.Crewmate, "securityGuardCamMaxCharges", 5f, 1f, 30f, 1f, securityGuardSpawnRate, false, "unitScrews");
            securityGuardCamRechargeTasksNumber = CustomOption.Create(287, Types.Crewmate, "securityGuardCamRechargeTasksNumber", 3f, 1f, 10f, 1f, securityGuardSpawnRate, false, "unitScrews");
            securityGuardNoMove = CustomOption.Create(288, Types.Crewmate, "securityGuardNoMove", true, securityGuardSpawnRate);

            mediumSpawnRate = new CustomRoleOption(360, Types.Crewmate, "medium", Medium.color);
            mediumCooldown = CustomOption.Create(361, Types.Crewmate, "mediumCooldown", 30f, 5f, 120f, 5f, mediumSpawnRate, false, "unitSeconds");
            mediumDuration = CustomOption.Create(362, Types.Crewmate, "mediumDuration", 3f, 0f, 15f, 1f, mediumSpawnRate, false, "unitSeconds");
            mediumOneTimeUse = CustomOption.Create(363, Types.Crewmate, "mediumOneTimeUse", false, mediumSpawnRate);
            mediumRevealTarget = CustomOption.Create(365, Types.Crewmate, "mediumRevealTarget", true, mediumSpawnRate);
            mediumChanceAdditionalInfo = CustomOption.Create(364, Types.Crewmate, "mediumChanceAdditionalInfo", rates, mediumSpawnRate);

            thiefSpawnRate = new CustomRoleOption(400, Types.Neutral, "thief", Thief.color);
            thiefCooldown = CustomOption.Create(401, Types.Neutral, "thiefCooldown", 30f, 5f, 120f, 5f, thiefSpawnRate, false, "unitSeconds");
            thiefCanKillSheriff = CustomOption.Create(402, Types.Neutral, "thiefCanKillSheriff", true, thiefSpawnRate);
            thiefHasImpVision = CustomOption.Create(403, Types.Neutral, "thiefHasImpVision", true, thiefSpawnRate);
            thiefCanUseVents = CustomOption.Create(404, Types.Neutral, "thiefCanUseVents", true, thiefSpawnRate);
            thiefCanStealWithGuess = CustomOption.Create(405, Types.Neutral, "thiefCanStealWithGuess", false, thiefSpawnRate);

            moriartySpawnRate = new CustomRoleOption(8030, Types.Neutral, "moriarty", Moriarty.color);
            moriartyBrainwashCooldown = CustomOption.Create(8031, Types.Neutral, "moriartyBrainwashCooldown", 30f, 10f, 60f, 1f, moriartySpawnRate, false, "unitSeconds");
            moriartyBrainwashTime = CustomOption.Create(8032, Types.Neutral, "moriartyBrainwashTime", 30f, 1f, 60f, 1f, moriartySpawnRate, false, "unitSeconds");
            moriartyNumberToWin = CustomOption.Create(8033, Types.Neutral, "moriartyNumberToWin", 3f, 1f, 10f, 1f, moriartySpawnRate, false, "unitScrews");
            moriartySherlockAddition = CustomOption.Create(8045, Types.Neutral, "moriartySherlockAddition", 2f, 0f, 5f, 1f, moriartySpawnRate, false, "unitScrews");
            moriartyKillIndicate = CustomOption.Create(8044, Types.Neutral, "moriartyKillIndicate", false, moriartySpawnRate);

            /*trapperSpawnRate = CustomOption.Create(410, Types.Crewmate, cs(Trapper.color, "Trapper"), rates, null, true);
            trapperCooldown = CustomOption.Create(420, Types.Crewmate, "Trapper Cooldown", 30f, 5f, 120f, 5f, trapperSpawnRate);
            trapperMaxCharges = CustomOption.Create(440, Types.Crewmate, "Max Traps Charges", 5f, 1f, 15f, 1f, trapperSpawnRate);
            trapperRechargeTasksNumber = CustomOption.Create(450, Types.Crewmate, "Number Of Tasks Needed For Recharging", 2f, 1f, 15f, 1f, trapperSpawnRate);
            trapperTrapNeededTriggerToReveal = CustomOption.Create(451, Types.Crewmate, "Trap Needed Trigger To Reveal", 3f, 2f, 10f, 1f, trapperSpawnRate);
            trapperAnonymousMap = CustomOption.Create(452, Types.Crewmate, "Show Anonymous Map", false, trapperSpawnRate);
            trapperInfoType = CustomOption.Create(453, Types.Crewmate, "Trap Information Type", new string[] { "Role", "Good/Evil Role", "Name" }, trapperSpawnRate);
            trapperTrapDuration = CustomOption.Create(454, Types.Crewmate, "Trap Duration", 5f, 1f, 15f, 1f, trapperSpawnRate);*/

            // Modifier (1000 - 1999)
            modifiersAreHidden = CustomOption.Create(1009, Types.Modifier, cs(Color.yellow, "vipbloodyHidden"), true, null, true, heading: cs(Color.yellow, "modifiersAreHidden"));

            modifierBloody = CustomOption.Create(1000, Types.Modifier, cs(Color.yellow, "bloody"), rates, null, true, color: Color.yellow);
            modifierBloodyQuantity = CustomOption.Create(1001, Types.Modifier, cs(Color.yellow, "bloodyQuantity"), ratesModifier, modifierBloody);
            modifierBloodyDuration = CustomOption.Create(1002, Types.Modifier, "bloodDuration", 10f, 3f, 60f, 1f, modifierBloody, false, "unitSeconds");

            modifierAntiTeleport = CustomOption.Create(1010, Types.Modifier, cs(Color.yellow, "antiTeleport"), rates, null, true, color: Color.yellow);
            modifierAntiTeleportQuantity = CustomOption.Create(1011, Types.Modifier, cs(Color.yellow, "antiTeleportQuantity"), ratesModifier, modifierAntiTeleport);

            modifierTieBreaker = CustomOption.Create(1020, Types.Modifier, cs(Color.yellow, "tiebreakerLongDesc"), rates, null, true, color: Color.yellow);

            modifierRadar = CustomOption.Create(1800, Types.Modifier, cs(Color.yellow, "radar"), rates, null, true, color: Color.yellow);

            modifierLover = CustomOption.Create(1040, Types.Modifier, cs(Color.yellow, "lovers"), rates, null, true, color: Color.yellow);
            modifierLoverImpLoverRate = CustomOption.Create(1041, Types.Modifier, "loversImpLoverRate", rates, modifierLover);
            modifierLoverQuantity = CustomOption.Create(1044, Types.Modifier, "loversQuantity", 1f, 1f, 6f, 1f, modifierLover, format: "unitCouples");
            modifierLoverBothDie = CustomOption.Create(1042, Types.Modifier, "loversBothDie", true, modifierLover);
            modifierLoverEnableChat = CustomOption.Create(1043, Types.Modifier, "loversEnableChat", true, modifierLover);

            modifierSunglasses = CustomOption.Create(1050, Types.Modifier, cs(Color.yellow, "sunglasses"), rates, null, true, color: Color.yellow);
            modifierSunglassesQuantity = CustomOption.Create(1051, Types.Modifier, cs(Color.yellow, "sunglassesQuantity"), ratesModifier, modifierSunglasses);
            modifierSunglassesVision = CustomOption.Create(1052, Types.Modifier, "sunglassesVision", ["-10%", "-20%", "-30%", "-40%", "-50%"], modifierSunglasses);

            modifierMini = CustomOption.Create(1061, Types.Modifier, cs(Color.yellow, "mini"), rates, null, true, color: Color.yellow);
            modifierMiniGrowingUpDuration = CustomOption.Create(1062, Types.Modifier, "miniGrowingUpDuration", 400f, 100f, 1500f, 100f, modifierMini, false, "unitSeconds");
            modifierMiniGrowingUpInMeeting = CustomOption.Create(1063, Types.Modifier, "miniGrowingUpInMeeting", true, modifierMini);
            if (Utilities.EventUtility.canBeEnabled || Utilities.EventUtility.isEnabled)
            {
                eventKicksPerRound = CustomOption.Create(10424, Types.Modifier, cs(Color.green, "eventKicksPerRound"), 4f, 0f, 14f, 1f, modifierMini);
                eventHeavyAge = CustomOption.Create(10425, Types.Modifier, cs(Color.green, "eventHeavyAge"), 12f, 6f, 18f, 0.5f, modifierMini);
                eventReallyNoMini = CustomOption.Create(10426, Types.Modifier, cs(Color.green, "eventReallyNoMini"), false, modifierMini, invertedParent: true);
            }

            modifierVip = CustomOption.Create(1070, Types.Modifier, cs(Color.yellow, "vip"), rates, null, true, color: Color.yellow);
            modifierVipQuantity = CustomOption.Create(1071, Types.Modifier, cs(Color.yellow, "vipQuantity"), ratesModifier, modifierVip);
            modifierVipShowColor = CustomOption.Create(1072, Types.Modifier, "vipShowColor", true, modifierVip);

            modifierInvert = CustomOption.Create(1080, Types.Modifier, cs(Color.yellow, "invert"), rates, null, true, color: Color.yellow);
            modifierInvertQuantity = CustomOption.Create(1081, Types.Modifier, cs(Color.yellow, "invertQuantity"), ratesModifier, modifierInvert);
            modifierInvertDuration = CustomOption.Create(1082, Types.Modifier, "invertDuration", 3f, 1f, 15f, 1f, modifierInvert, false, "unitScrews");

            modifierDiseased = CustomOption.Create(4990, Types.Modifier, cs(Color.yellow, "diseased"), rates, null, true, color: Color.yellow);
            modifierDiseasedQuantity = CustomOption.Create(4991, Types.Modifier, cs(Color.yellow, "diseasedQuantity"), ratesModifier, modifierDiseased);
            modifierDiseasedMultiplier = CustomOption.Create(4992, Types.Modifier, "diseasedMultiplier", 3f, 1.25f, 5f, 0.25f, modifierDiseased, false, "unitTimes");

            modifierChameleon = CustomOption.Create(1090, Types.Modifier, cs(Color.yellow, "chameleon"), rates, null, true, color: Color.yellow);
            modifierChameleonQuantity = CustomOption.Create(1091, Types.Modifier, cs(Color.yellow, "chameleonQuantity"), ratesModifier, modifierChameleon);
            modifierChameleonHoldDuration = CustomOption.Create(1092, Types.Modifier, "chameleonHoldDuration", 3f, 1f, 10f, 0.5f, modifierChameleon, false, "unitSeconds");
            modifierChameleonFadeDuration = CustomOption.Create(1093, Types.Modifier, "chameleonFadeDuration", 1f, 0.25f, 10f, 0.25f, modifierChameleon, false, "unitSeconds");
            modifierChameleonMinVisibility = CustomOption.Create(1094, Types.Modifier, "chameleonMinVisibility", ["0%", "10%", "20%", "30%", "40%", "50%"], modifierChameleon);

            modifierMultitasker = CustomOption.Create(1996, Types.Modifier, cs(Color.yellow, "multitasker"), rates, null, true, color: Color.yellow);
            modifierMultitaskerQuantity = CustomOption.Create(1997, Types.Modifier, cs(Color.yellow, "multitaskerQuantity"), ratesModifier, modifierMultitasker);

            modifierArmored = CustomOption.Create(9101, Types.Modifier, cs(Color.yellow, "armored"), rates, null, true, color: Color.yellow);

            madmateSpawnRate = CustomOption.Create(4041, Types.Modifier, cs(Color.yellow, "madmate"), rates, null, true, color: Color.yellow);
            madmateQuantity = CustomOption.Create(7005, Types.Modifier, cs(Color.yellow, "madmateQuantity"), ratesModifier, madmateSpawnRate);
            madmateFixedRole = CustomOption.Create(7006, Types.Modifier, "madmateFixedRole", Madmate.validRoles, madmateSpawnRate);
            madmateFixedRoleGuesserGamemode = CustomOption.Create(7007, Types.Modifier, "madmateFixedRole", Madmate.validRoles.Where(x => x != RoleId.NiceGuesser).ToList(), madmateSpawnRate);
            madmateAbility = CustomOption.Create(4047, Types.Modifier, "madmateAbility", true, madmateSpawnRate);
            madmateCommonTasks = CustomOption.Create(4049, Types.Modifier, "madmateCommonTasks", 1f, 0f, 3f, 1f, madmateAbility, false, "unitScrews");
            madmateShortTasks = CustomOption.Create(4048, Types.Modifier, "madmateShortTasks", 3f, 0f, 4f, 1f, madmateAbility, false, "unitScrews");
            madmateLongTasks = CustomOption.Create(7000, Types.Modifier, "madmateLongTasks", 1f, 0f, 4f, 1f, madmateAbility, false, "unitScrews");
            madmateCanDieToSheriff = CustomOption.Create(4042, Types.Modifier, "madmateCanDieToSheriff", false, madmateSpawnRate);
            madmateCanEnterVents = CustomOption.Create(4043, Types.Modifier, "madmateCanEnterVents", false, madmateSpawnRate);
            madmateCanSabotage = CustomOption.Create(7010, Types.Modifier, "madmateCanSabotage", false, madmateSpawnRate);
            madmateHasImpostorVision = CustomOption.Create(4044, Types.Modifier, "madmateHasImpostorVision", false, madmateSpawnRate);
            madmateCanFixComm = CustomOption.Create(7001, Types.Modifier, "madmateCanFixComm", true, madmateSpawnRate);

            //modifierShifter = CustomOption.Create(1100, Types.Modifier, cs(Color.yellow, "Shifter"), rates, null, true);

            // Guesser Gamemode (2000 - 2999)
            guesserGamemodeCrewNumber = CustomOption.Create(2001, Types.Guesser, cs(Guesser.color, "guesserGamemodeCrewNumber"), 24f, 0f, 24f, 1f, null, true, "unitPlayers", heading: "headingAmountOfGuessers");
            guesserGamemodeNeutralNumber = CustomOption.Create(2002, Types.Guesser, cs(Guesser.color, "guesserGamemodeNeutralNumber"), 24f, 0f, 24f, 1f, null, false, "unitPlayers");
            guesserGamemodeImpNumber = CustomOption.Create(2003, Types.Guesser, cs(Guesser.color, "guesserGamemodeImpNumber"), 24f, 0f, 24f, 1f, null, false, "unitPlayers");
            guesserForceJackalGuesser = CustomOption.Create(2007, Types.Guesser, "guesserForceJackalGuesser", false, null, true, heading: "headingForceGuesser");
            guesserGamemodeSidekickIsAlwaysGuesser = CustomOption.Create(2012, Types.Guesser, "guesserGamemodeSidekickIsAlwaysGuesser", false, null);
            guesserForceThiefGuesser = CustomOption.Create(2011, Types.Guesser, "guesserForceThiefGuesser", false, null, true);
            guesserGamemodeHaveModifier = CustomOption.Create(2004, Types.Guesser, "guesserGamemodeHaveModifier", true, null, true, heading: "headingGeneralGuesser");
            guesserGamemodeNumberOfShots = CustomOption.Create(2005, Types.Guesser, "guesserGamemodeNumberOfShots", 3f, 1f, 24f, 1f, null, false, "unitShots");
            guesserGamemodeHasMultipleShotsPerMeeting = CustomOption.Create(2006, Types.Guesser, "guesserGamemodeHasMultipleShotsPerMeeting", false, null);
            guesserGamemodeCrewGuesserNumberOfTasks = CustomOption.Create(2013, Types.Guesser, "guesserGamemodeCrewGuesserNumberOfTasks", 0f, 0f, 15f, 1f, null, format: "unitScrews");
            guesserGamemodeKillsThroughShield = CustomOption.Create(2008, Types.Guesser, "guesserGamemodeKillsThroughShield", true, null);
            guesserGamemodeEvilCanKillSpy = CustomOption.Create(2009, Types.Guesser, "guesserGamemodeEvilCanKillSpy", true, null);
            guesserGamemodeCantGuessSnitchIfTaksDone = CustomOption.Create(2010, Types.Guesser, "guesserGamemodeCantGuessSnitchIfTaksDone", true, null);
            guesserGamemodeCantGuessFortuneTeller = CustomOption.Create(2021, Types.Guesser, "guesserGamemodeCantGuessFortuneTeller", true, null);
            guesserGamemodeEnableLastImpostor = CustomOption.Create(2017, Types.Guesser, "guesserGamemodeEnableLastImpostor", false, null, true, heading: "headingLastImpostor");
            guesserGamemodeLastImpostorNumKills = CustomOption.Create(2016, Types.Guesser, "guesserGamemodeLastImpostorNumKills", 3f, 0f, 24f, 1f, guesserGamemodeEnableLastImpostor, format: "unitPlayers");
            guesserGamemodeLastImpostorNumShots = CustomOption.Create(2018, Types.Guesser, "guesserGamemodeLastImpostorNumShots", 3f, 1f, 24f, 1f, guesserGamemodeEnableLastImpostor, format: "unitShots");
            guesserGamemodeLastImpostorHasMultipleShots = CustomOption.Create(2019, Types.Guesser, "guesserGamemodeLastImpostorHasMultipleShots", true, guesserGamemodeEnableLastImpostor);

            // Hide N Seek Gamemode (3000 - 3999)
            hideNSeekMap = CustomOption.Create(3020, Types.HideNSeekMain, cs(Color.yellow, "hideNSeekMap"), ["The Skeld", "Mira", "Polus", "Airship", "Fungle"], null, true, onChange: () => { int map = hideNSeekMap.selection; if (map >= 3) map++; GameOptionsManager.Instance.currentNormalGameOptions.MapId = (byte)map; });
            hideNSeekHunterCount = CustomOption.Create(3000, Types.HideNSeekMain, cs(Color.yellow, "hideNSeekHunterCount"), 1f, 1f, 3f, 1f, format: "unitPlayers");
            hideNSeekKillCooldown = CustomOption.Create(3021, Types.HideNSeekMain, cs(Color.yellow, "hideNSeekKillCooldown"), 10f, 2.5f, 60f, 2.5f, format: "unitSeconds");
            hideNSeekHunterVision = CustomOption.Create(3001, Types.HideNSeekMain, cs(Color.yellow, "hideNSeekHunterVision"), 0.5f, 0.25f, 2f, 0.25f, format: "unitTimes");
            hideNSeekHuntedVision = CustomOption.Create(3002, Types.HideNSeekMain, cs(Color.yellow, "hideNSeekHuntedVision"), 2f, 0.25f, 5f, 0.25f, format: "unitTimes");
            hideNSeekCommonTasks = CustomOption.Create(3023, Types.HideNSeekMain, cs(Color.yellow, "hideNSeekCommonTasks"), 1f, 0f, 4f, 1f, format: "unitScrews");
            hideNSeekShortTasks = CustomOption.Create(3024, Types.HideNSeekMain, cs(Color.yellow, "hideNSeekShortTasks"), 3f, 1f, 23f, 1f, format: "unitScrews");
            hideNSeekLongTasks = CustomOption.Create(3025, Types.HideNSeekMain, cs(Color.yellow, "hideNSeekLongTasks"), 3f, 0f, 15f, 1f, format: "unitScrews");
            hideNSeekTimer = CustomOption.Create(3003, Types.HideNSeekMain, cs(Color.yellow, "hideNSeekTimer"), 5f, 1f, 30f, 1f);
            hideNSeekTaskWin = CustomOption.Create(3004, Types.HideNSeekMain, cs(Color.yellow, "hideNSeekTaskWin"), false);
            hideNSeekTaskPunish = CustomOption.Create(3017, Types.HideNSeekMain, cs(Color.yellow, "hideNSeekTaskPunish"), 10f, 0f, 30f, 1f, format: "unitSeconds");
            hideNSeekCanSabotage = CustomOption.Create(3019, Types.HideNSeekMain, cs(Color.yellow, "hideNSeekCanSabotage"), false);
            hideNSeekHunterWaiting = CustomOption.Create(3022, Types.HideNSeekMain, cs(Color.yellow, "hideNSeekHunterWaiting"), 15f, 2.5f, 60f, 2.5f, format: "unitSeconds");

            hunterLightCooldown = CustomOption.Create(3005, Types.HideNSeekRoles, cs(Color.red, "hunterLightCooldown"), 30f, 5f, 60f, 1f, null, true, "unitSeconds", heading: "headingHunterLight");
            hunterLightDuration = CustomOption.Create(3006, Types.HideNSeekRoles, cs(Color.red, "hunterLightDuration"), 5f, 1f, 60f, 1f, format: "unitSeconds");
            hunterLightVision = CustomOption.Create(3007, Types.HideNSeekRoles, cs(Color.red, "hunterLightVision"), 3f, 1f, 5f, 0.25f, format: "unitTimes");
            hunterLightPunish = CustomOption.Create(3008, Types.HideNSeekRoles, cs(Color.red, "hunterLightPunish"), 5f, 0f, 30f, 1f, format: "unitSeconds");
            hunterAdminCooldown = CustomOption.Create(3009, Types.HideNSeekRoles, cs(Color.red, "hunterAdminCooldown"), 30f, 5f, 60f, 1f, format: "unitSeconds");
            hunterAdminDuration = CustomOption.Create(3010, Types.HideNSeekRoles, cs(Color.red, "hunterAdminDuration"), 5f, 1f, 60f, 1f, format: "unitSeconds");
            hunterAdminPunish = CustomOption.Create(3011, Types.HideNSeekRoles, cs(Color.red, "hunterAdminPunish"), 5f, 0f, 30f, 1f, format: "unitSeconds");
            hunterArrowCooldown = CustomOption.Create(3012, Types.HideNSeekRoles, cs(Color.red, "hunterArrowCooldown"), 30f, 5f, 60f, 1f, format: "unitSeconds");
            hunterArrowDuration = CustomOption.Create(3013, Types.HideNSeekRoles, cs(Color.red, "hunterArrowDuration"), 5f, 0f, 60f, 1f, format: "unitSeconds");
            hunterArrowPunish = CustomOption.Create(3014, Types.HideNSeekRoles, cs(Color.red, "hunterArrowPunish"), 5f, 0f, 30f, 1f, format: "unitSeconds");

            huntedShieldCooldown = CustomOption.Create(3015, Types.HideNSeekRoles, cs(Color.gray, "huntedShieldCooldown"), 30f, 5f, 60f, 1f, null, true, "unitSeconds", heading: "headingHuntedShield");
            huntedShieldDuration = CustomOption.Create(3016, Types.HideNSeekRoles, cs(Color.gray, "huntedShieldDuration"), 5f, 1f, 60f, 1f, format: "unitSeconds");
            huntedShieldRewindTime = CustomOption.Create(3018, Types.HideNSeekRoles, cs(Color.gray, "huntedShieldRewindTime"), 3f, 1f, 10f, 1f, format: "unitSeconds");
            huntedShieldNumber = CustomOption.Create(3026, Types.HideNSeekRoles, cs(Color.gray, "huntedShieldNumber"), 3f, 1f, 15f, 1f, format: "unitScrews");

            // Other options
            maxNumberOfMeetings = CustomOption.Create(3, Types.General, "maxNumberOfMeetings", 10, 0, 15, 1, null, true, "unitShots", heading: "headingGameplay");
            freePlayGameModeNumDummies = CustomOption.Create(10429, Types.General, cs(Color.green, "freePlayGameModeNumDummies"), 1f, 0f, 23f, 1f, format: "unitPlayers");
            anyPlayerCanStopStart = CustomOption.Create(2, Types.General, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "anyPlayerCanStopStart"), false, null, false);
            blockSkippingInEmergencyMeetings = CustomOption.Create(4, Types.General, "blockSkippingInEmergencyMeetings", false);
            noVoteIsSelfVote = CustomOption.Create(5, Types.General, "noVoteIsSelfVote", false, blockSkippingInEmergencyMeetings);
            hidePlayerNames = CustomOption.Create(6, Types.General,  "hidePlayerNames", false);
            allowParallelMedBayScans = CustomOption.Create(7, Types.General, "allowParallelMedBayScans", false);
            shieldFirstKill = CustomOption.Create(8, Types.General, "shieldFirstKill", false);
            finishTasksBeforeHauntingOrZoomingOut = CustomOption.Create(9, Types.General, "finishTasksBeforeHauntingOrZoomingOut", true);
            enableImpostorChat = CustomOption.Create(6075, Types.General, "enableImpostorChat", true);
            additionalVents = CustomOption.Create(5060, Types.General, "additionalVents", false);
            specimenVital = CustomOption.Create(5061, Types.General, "specimenVital", false);
            airshipLadder = CustomOption.Create(6070, Types.General, "airshipLadder", false);
            airshipOptimize = CustomOption.Create(6072, Types.General, "airshipOptimize", false);
            airshipAdditionalSpawn = CustomOption.Create(6073, Types.General, "airshipAdditionalSpawn", false);
            fungleElectrical = CustomOption.Create(6074, Types.General, "fungleElectrical", false);
            randomGameStartPosition = CustomOption.Create(6071, Types.General, "randomGameStartPosition", false);

            camsNightVision = CustomOption.Create(11, Types.General, "camsNightVision", false, null, true, heading: "headingNightVision");
            camsNoNightVisionIfImpVision = CustomOption.Create(12, Types.General, "camsNoNightVisionIfImpVision", false, camsNightVision, false);

            activateProps = CustomOption.Create(6083, Types.General, "activateProps", false, null, true, heading: "headingPropSetting");
            numAccelTraps = CustomOption.Create(6084, Types.General, "numAccelTraps", 1f, 0f, 5f, 1f, activateProps, false, "unitScrews");
            accelerationDuration = CustomOption.Create(6085, Types.General, "accelerationDuration", 5f, 1f, 20f, 1f, activateProps, false, "unitSeconds");
            speedAcceleration = CustomOption.Create(6086, Types.General, "speedAcceleration", 1.25f, 0.5f, 2f, 0.25f, activateProps, false, "unitTimes");
            numDecelTraps = CustomOption.Create(6087, Types.General, "numDecelTraps", 1f, 0f, 3f, 1f, activateProps, false, "unitScrews");
            decelerationDuration = CustomOption.Create(6091, Types.General, "decelerationDuration", 5f, 1f, 20f, 1f, activateProps, false, "unitSeconds");
            speedDeceleration = CustomOption.Create(6089, Types.General, "speedDeceleration", -0.5f, -0.8f, -0.1f, 0.1f, activateProps, false, "unitTimes");
            decelUpdateInterval = CustomOption.Create(6090, Types.General, "decelUpdateInterval", 10f, 5f, 60f, 2.5f, activateProps, false, "unitSeconds");


            dynamicMap = CustomOption.Create(500, Types.General, "dynamicMap", false, null, true, heading: "headingMapSetting");
            dynamicMapEnableSkeld = CustomOption.Create(501, Types.General, "Skeld", rates, dynamicMap, false);
            dynamicMapEnableMira = CustomOption.Create(502, Types.General, "Mira", rates, dynamicMap, false);
            dynamicMapEnablePolus = CustomOption.Create(503, Types.General, "Polus", rates, dynamicMap, false);
            dynamicMapEnableAirShip = CustomOption.Create(504, Types.General, "Airship", rates, dynamicMap, false);
            dynamicMapEnableSubmerged = CustomOption.Create(505, Types.General, "Submerged", rates, dynamicMap, false);
            dynamicMapEnableFungle = CustomOption.Create(506, Types.General, "Fungle", rates, dynamicMap, false);
            dynamicMapSeparateSettings = CustomOption.Create(509, Types.General, "dynamicMapSeparateSettings", false, dynamicMap, false);

            blockedRolePairings.Add((byte)RoleId.Vampire, [(byte)RoleId.Warlock]);
            blockedRolePairings.Add((byte)RoleId.Warlock, [(byte)RoleId.Vampire]);
            blockedRolePairings.Add((byte)RoleId.Spy, [(byte)RoleId.Mini]);
            blockedRolePairings.Add((byte)RoleId.Mini, [(byte)RoleId.Spy]);
            blockedRolePairings.Add((byte)RoleId.Vulture, [(byte)RoleId.Cleaner]);
            blockedRolePairings.Add((byte)RoleId.Cleaner, [(byte)RoleId.Vulture]);
        }
    }
}
