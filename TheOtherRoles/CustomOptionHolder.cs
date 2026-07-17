using System.Collections.Generic;
using System.Linq;
using Rewired.Utils.Classes.Data;
using TheOtherRoles.Roles;
using UnityEngine;
using UnityEngine.UIElements.Experimental;
using static UnityEngine.UIElements.BaseVerticalCollectionView;
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

        public static CustomRoleOption puppeteerSpawnRate;
        public static CustomOption puppeteerNumKills;
        public static CustomOption puppeteerSampleDuration;
        public static CustomOption puppeteerCanControlDummyEvenIfDead;
        public static CustomOption puppeteerPenaltyOnDeath;
        public static CustomOption puppeteerLosesSenriganOnDeath;

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
        public static CustomOption snitchSeesRoles;

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
        public static CustomOption warlockCanKillImpostors;

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
            presetSelection = new CustomOption(0, Types.General, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "presetSelection"), presets, "", null, true, "", Color.white, null, "", false);

            if (Utilities.EventUtility.canBeEnabled) enableEventMode = CustomOption.Create(Types.General, cs(Color.green, "enableEventMode"), true, null, true);

            // Using new id's for the options to not break compatibilty with older versions
            crewmateRolesCountMin = CustomOption.Create(Types.General, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "crewmateRolesCountMin"), 24f, 0f, 24f, 1f, null, true, "unitPlayers", heading: "headingMinMax");
            crewmateRolesCountMax = CustomOption.Create(Types.General, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "crewmateRolesCountMax"), 24f, 0f, 24f, 1f, format: "unitPlayers");
            neutralRolesCountMin = CustomOption.Create(Types.General, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "neutralRolesCountMin"), 24f, 0f, 24f, 1f, format: "unitPlayers");
            neutralRolesCountMax = CustomOption.Create(Types.General, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "neutralRolesCountMax"), 24f, 0f, 24f, 1f, format: "unitPlayers");
            impostorRolesCountMin = CustomOption.Create(Types.General, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "impostorRolesCountMin"), 24f, 0f, 24f, 1f, format: "unitPlayers");
            impostorRolesCountMax = CustomOption.Create(Types.General, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "impostorRolesCountMax"), 24f, 0f, 24f, 1f, format: "unitPlayers");
            modifiersCountMin = CustomOption.Create(Types.General, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "modifiersCountMin"), 24f, 0f, 24f, 1f, format: "unitPlayers");
            modifiersCountMax = CustomOption.Create(Types.General, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "modifiersCountMax"), 24f, 0f, 24f, 1f, format: "unitPlayers");
            crewmateRolesFill = CustomOption.Create(Types.General, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "crewmateRolesFill"), false);

            isDraftMode = CustomOption.Create(Types.General, cs(Color.yellow, "enableDraftMode"), false, null, true, heading: "headingRoleDraft");
            draftModeAmountOfChoices = CustomOption.Create(Types.General, cs(Color.yellow, "draftModeAmountOfChoices"), 3f, 2f, 6f, 1f, isDraftMode, false, format: "unitScrews");
            draftModeTimeToChoose = CustomOption.Create(Types.General, cs(Color.yellow, "draftModeTimeToChoose"), 5f, 3f, 20f, 1f, isDraftMode, false, format: "unitSeconds");
            draftModeShowRoles = CustomOption.Create(Types.General, cs(Color.yellow, "draftModeShowRoles"), false, isDraftMode, false);
            draftModeHideCrewRoles = CustomOption.Create(Types.General, cs(Color.yellow, "draftModeHideCrewRoles"), false, draftModeShowRoles, false);
            draftModeHideImpRoles = CustomOption.Create(Types.General, cs(Color.yellow, "draftModeHideImpRoles"), false, draftModeShowRoles, false);
            draftModeHideNeutralRoles = CustomOption.Create(Types.General, cs(Color.yellow, "draftModeHideNeutralRoles"), false, draftModeShowRoles, false);

            mafiaSpawnRate = new CustomRoleOption(Types.Impostor, "mafia", Janitor.color, 1);
            godfatherShareInfo = CustomOption.Create(Types.Impostor, "godfatherShareInfo", true, mafiaSpawnRate);
            janitorImpostorsCanSeeDeadBody = CustomOption.Create(Types.Impostor, "janitorImpostorsCanSeeDeadBody", true, mafiaSpawnRate);
            janitorCanSabotage = CustomOption.Create(Types.Impostor, "janitorCanSabotage", true, mafiaSpawnRate);
            mafiosoNumberOfSkips = CustomOption.Create(Types.Impostor, "mafiosoNumberOfSkips", 2f, 1f, 15f, 1f, mafiaSpawnRate, false, "unitScrews");

            morphlingSpawnRate = new CustomRoleOption(Types.Impostor, "morphling", Morphling.color);
            morphlingCooldown = CustomOption.Create(Types.Impostor, "morphlingCooldown", 30f, 10f, 60f, 2.5f, morphlingSpawnRate, false, "unitSeconds");
            morphlingDuration = CustomOption.Create(Types.Impostor, "morphlingDuration", 10f, 1f, 20f, 0.5f, morphlingSpawnRate, false, "unitSeconds");

            camouflagerSpawnRate = new CustomRoleOption(Types.Impostor, "camouflager", Camouflager.color, 1);
            camouflagerCooldown = CustomOption.Create(Types.Impostor, "camouflagerCooldown", 30f, 10f, 60f, 2.5f, camouflagerSpawnRate, false, "unitSeconds");
            camouflagerDuration = CustomOption.Create(Types.Impostor, "camouflagerDuration", 10f, 1f, 20f, 0.5f, camouflagerSpawnRate, false, "unitSeconds");

            vampireSpawnRate = new CustomRoleOption(Types.Impostor, "vampire", Vampire.color);
            vampireKillDelay = CustomOption.Create(Types.Impostor, "vampireKillDelay", 10f, 1f, 20f, 1f, vampireSpawnRate, false, "unitSeconds");
            vampireCooldown = CustomOption.Create(Types.Impostor, "vampireCooldown", 30f, 10f, 60f, 2.5f, vampireSpawnRate, false, "unitSeconds");
            vampireCooldownDecrease = CustomOption.Create(Types.Impostor, "vampireCooldownDecrease", 10f, 0f, 120f, 2.5f, vampireSpawnRate, false, "unitSeconds");
            vampireCanKillNearGarlics = CustomOption.Create(Types.Impostor, "vampireCanKillNearGarlics", true, vampireSpawnRate);

            eraserSpawnRate = new CustomRoleOption(Types.Impostor, "eraser", Eraser.color);
            eraserCooldown = CustomOption.Create(Types.Impostor, "eraserCooldown", 30f, 10f, 120f, 5f, eraserSpawnRate, false, "unitSeconds");
            eraserCooldownIncrease = CustomOption.Create(Types.Impostor, "eraserCooldownIncrease", 10f, 0f, 120f, 2.5f, eraserSpawnRate, format: "unitSeconds");
            eraserCanEraseAnyone = CustomOption.Create(Types.Impostor, "eraserCanEraseAnyone", false, eraserSpawnRate);

            tricksterSpawnRate = new CustomRoleOption(Types.Impostor, "trickster", Trickster.color, 1);
            tricksterPlaceBoxCooldown = CustomOption.Create(Types.Impostor, "tricksterPlaceBoxCooldown", 10f, 2.5f, 30f, 2.5f, tricksterSpawnRate, false, "unitSeconds");
            tricksterBoxKillPenalty = CustomOption.Create(Types.Impostor, "tricksterBoxKillPenalty", 2.5f, 0f, 30f, 2.5f, tricksterSpawnRate, false, "unitSeconds");
            tricksterLightsOutCooldown = CustomOption.Create(Types.Impostor, "tricksterLightsOutCooldown", 30f, 10f, 60f, 5f, tricksterSpawnRate, false, "unitSeconds");
            tricksterLightsOutDuration = CustomOption.Create(Types.Impostor, "tricksterLightsOutDuration", 15f, 5f, 60f, 2.5f, tricksterSpawnRate, false, "unitSeconds");

            cleanerSpawnRate = new CustomRoleOption(Types.Impostor, "cleaner", Cleaner.color);
            cleanerCooldown = CustomOption.Create(Types.Impostor, "cleanerCooldown", 30f, 10f, 60f, 2.5f, cleanerSpawnRate, false, "unitSeconds");
            cleanerCanSeeBodies = CustomOption.Create(Types.Impostor, "cleanerCanSeeBodies", true, cleanerSpawnRate);

            warlockSpawnRate = new CustomRoleOption(Types.Impostor, "warlock", Warlock.color);
            warlockCooldown = CustomOption.Create(Types.Impostor, "warlockCooldown", 30f, 10f, 60f, 2.5f, warlockSpawnRate, false, "unitSeconds");
            warlockRootTime = CustomOption.Create(Types.Impostor, "warlockRootTime", 5f, 0f, 15f, 1f, warlockSpawnRate, false, "unitSeconds");
            warlockCanKillImpostors = CustomOption.Create(Types.Impostor, "warlockCanKillImpostors", false, warlockSpawnRate);

            bountyHunterSpawnRate = new CustomRoleOption(Types.Impostor, "bountyHunter", BountyHunter.color);
            bountyHunterBountyDuration = CustomOption.Create(Types.Impostor, "bountyHunterBountyDuration", 60f, 10f, 180f, 10f, bountyHunterSpawnRate, false, "unitSeconds");
            bountyHunterReducedCooldown = CustomOption.Create(Types.Impostor, "bountyHunterReducedCooldown", 2.5f, 0f, 30f, 2.5f, bountyHunterSpawnRate, false, "unitSeconds");
            bountyHunterPunishmentTime = CustomOption.Create(Types.Impostor, "bountyHunterPunishmentTime", 20f, 0f, 60f, 2.5f, bountyHunterSpawnRate, false, "unitSeconds");
            bountyHunterShowArrow = CustomOption.Create(Types.Impostor, "bountyHunterShowArrow", true, bountyHunterSpawnRate);
            bountyHunterArrowUpdateIntervall = CustomOption.Create(Types.Impostor, "bountyHunterArrowUpdateInterval", 15f, 2.5f, 60f, 2.5f, bountyHunterShowArrow, false, "unitSeconds");

            witchSpawnRate = new CustomRoleOption(Types.Impostor, "witch", Witch.color);
            witchCooldown = CustomOption.Create(Types.Impostor, "witchSpellCooldown", 30f, 10f, 120f, 5f, witchSpawnRate, false, "unitSeconds");
            witchAdditionalCooldown = CustomOption.Create(Types.Impostor, "witchAdditionalCooldown", 10f, 0f, 60f, 5f, witchSpawnRate, false, "unitSeconds");
            witchCanSpellAnyone = CustomOption.Create(Types.Impostor, "witchCanSpellAnyone", false, witchSpawnRate);
            witchSpellCastingDuration = CustomOption.Create(Types.Impostor, "witchSpellDuration", 1f, 0f, 10f, 1f, witchSpawnRate, false, "unitSeconds");
            witchTriggerBothCooldowns = CustomOption.Create(Types.Impostor, "witchTriggerBoth", true, witchSpawnRate);
            witchVoteSavesTargets = CustomOption.Create(Types.Impostor, "witchSaveTargets", true, witchSpawnRate);

            assassinSpawnRate = new CustomRoleOption(Types.Impostor, "assassin", Assassin.color);
            assassinCooldown = CustomOption.Create(Types.Impostor, "assassinCooldown", 30f, 10f, 120f, 5f, assassinSpawnRate, false, "unitSeconds");
            assassinKnowsTargetLocation = CustomOption.Create(Types.Impostor, "assassinKnowsTargetLocation", true, assassinSpawnRate);
            assassinTraceTime = CustomOption.Create(Types.Impostor, "assassinTraceDuration", 5f, 1f, 20f, 0.5f, assassinSpawnRate, false, "unitSeconds");
            assassinTraceColorTime = CustomOption.Create(Types.Impostor, "assassinTraceColorTime", 2f, 0f, 20f, 0.5f, assassinSpawnRate, false, "unitSeconds");
            assassinInvisibleDuration = CustomOption.Create(Types.Impostor, "assassinInvisibleDuration", 3f, 0f, 20f, 1f, assassinSpawnRate, false, "unitSeconds");

            ninjaSpawnRate = new CustomRoleOption(Types.Impostor, "ninja", Ninja.color);
            ninjaStealthCooldown = CustomOption.Create(Types.Impostor, "ninjaStealthCooldown", 30f, 2.5f, 60f, 2.5f, ninjaSpawnRate, false, "unitSeconds");
            ninjaStealthDuration = CustomOption.Create(Types.Impostor, "ninjaStealthDuration", 15f, 2.5f, 60f, 2.5f, ninjaSpawnRate, false, "unitSeconds");
            ninjaFadeTime = CustomOption.Create(Types.Impostor, "ninjaFadeTime", 0.5f, 0.0f, 2.5f, 0.5f, ninjaSpawnRate, false, "unitSeconds");
            ninjaKillPenalty = CustomOption.Create(Types.Impostor, "ninjaKillPenalty", 10f, 0f, 60f, 2.5f, ninjaSpawnRate, false, "unitSeconds");
            ninjaSpeedBonus = CustomOption.Create(Types.Impostor, "ninjaSpeedBonus", 1.25f, 0.5f, 2f, 0.25f, ninjaSpawnRate, false, "unitTimes");
            ninjaCanBeTargeted = CustomOption.Create(Types.Impostor, "ninjaCanBeTargeted", true, ninjaSpawnRate);
            ninjaCanVent = CustomOption.Create(Types.Impostor, "ninjaCanVent", false, ninjaSpawnRate);

            serialKillerSpawnRate = new CustomRoleOption(Types.Impostor, "serialKiller", SerialKiller.color);
            serialKillerKillCooldown = CustomOption.Create(Types.Impostor, "serialKillerKillCooldown", 15f, 2.5f, 60f, 2.5f, serialKillerSpawnRate, false, "unitSeconds");
            serialKillerSuicideTimer = CustomOption.Create(Types.Impostor, "serialKillerSuicideTimer", 40f, 2.5f, 60f, 2.5f, serialKillerSpawnRate, false, "unitSeconds");
            serialKillerResetTimer = CustomOption.Create(Types.Impostor, "serialKillerResetTimer", true, serialKillerSpawnRate);

            nekoKabochaSpawnRate = new CustomRoleOption(Types.Impostor, "nekoKabocha", NekoKabocha.color);
            nekoKabochaRevengeCrew = CustomOption.Create(Types.Impostor, "nekoKabochaRevengeCrew", true, nekoKabochaSpawnRate);
            nekoKabochaRevengeImpostor = CustomOption.Create(Types.Impostor, "nekoKabochaRevengeImpostor", true, nekoKabochaSpawnRate);
            nekoKabochaRevengeNeutral = CustomOption.Create(Types.Impostor, "nekoKabochaRevengeNeutral", true, nekoKabochaSpawnRate);
            nekoKabochaRevengeExile = CustomOption.Create(Types.Impostor, "nekoKabochaRevengeExile", false, nekoKabochaSpawnRate);

            evilTrackerSpawnRate = new CustomRoleOption(Types.Impostor, "evilTracker", EvilTracker.color);
            evilTrackerCooldown = CustomOption.Create(Types.Impostor, "evilTrackerCooldown", 10f, 0f, 60f, 5f, evilTrackerSpawnRate, false, "unitSeconds");
            evilTrackerResetTargetAfterMeeting = CustomOption.Create(Types.Impostor, "evilTrackerResetTargetAfterMeeting", true, evilTrackerSpawnRate);
            evilTrackerCanSeeDeathFlash = CustomOption.Create(Types.Impostor, "evilTrackerCanSeeDeathFlash", true, evilTrackerSpawnRate);
            evilTrackerCanSeeTargetPosition = CustomOption.Create(Types.Impostor, "evilTrackerCanSeeTargetPosition", true, evilTrackerSpawnRate);
            evilTrackerCanSeeTargetTask = CustomOption.Create(Types.Impostor, "evilTrackerCanSeeTargetTask", true, evilTrackerSpawnRate);
            evilTrackerCanSetTargetOnMeeting = CustomOption.Create(Types.Impostor, "evilTrackerCanSetTargetOnMeeting", true, evilTrackerSpawnRate);

            undertakerSpawnRate = new CustomRoleOption(Types.Impostor, "undertaker", Undertaker.color, 1);
            undertakerSpeedDecrease = CustomOption.Create(Types.Impostor, "undertakerSpeedDecrease", 0f, -80f, 180f, 10f, undertakerSpawnRate, false, "unitPercent");
            undertakerCanVentWhileDragging = CustomOption.Create(Types.Impostor, "undertakerCanVentWhileDragging", true, undertakerSpawnRate);
            undertakerConnectsVent = CustomOption.Create(Types.Impostor, "undertakerConnectsVent", true, undertakerSpawnRate);

            yoyoSpawnRate = new CustomRoleOption(Types.Impostor, "yoyo", Yoyo.color);
            yoyoBlinkDuration = CustomOption.Create(Types.Impostor, "yoyoBlinkDuration", 20f, 2.5f, 120f, 2.5f, yoyoSpawnRate, format: "unitSeconds");
            yoyoMarkCooldown = CustomOption.Create(Types.Impostor, "yoyoMarkCooldown", 20f, 2.5f, 120f, 2.5f, yoyoSpawnRate, format: "unitSeconds");
            yoyoBlackoutRange = CustomOption.Create(Types.Impostor, "yoyoBlackoutRange", 0f, 2f, 10f, 0.5f, yoyoSpawnRate, false, "unitMeters");
            yoyoBlackoutDuration = CustomOption.Create(Types.Impostor, "yoyoBlackoutDuration", 1f, 0.5f, 3f, 0.5f, yoyoSpawnRate, false, "unitSeconds");
            yoyoMarkStaysOverMeeting = CustomOption.Create(Types.Impostor, "yoyoMarkStaysOverMeeting", true, yoyoSpawnRate);
            yoyoSilhouetteVisibility = CustomOption.Create(Types.Impostor, "yoyoSilhouetteVisibility", ["0%", "10%", "20%", "30%", "40%", "50%"], yoyoSpawnRate);

            blackmailerSpawnRate = new CustomRoleOption(Types.Impostor, "blackmailer", Blackmailer.color);
            blackmailerCooldown = CustomOption.Create(Types.Impostor, "blackmailerCooldown", 30f, 5f, 120f, 5f, blackmailerSpawnRate, false, "unitSeconds");
            blackmailerBlockTargetVote = CustomOption.Create(Types.Impostor, "blackmailerBlockTargetVote", true, blackmailerSpawnRate);
            blackmailerBlockTargetAbility = CustomOption.Create(Types.Impostor, "blackmailerBlockTargetAbility", true, blackmailerSpawnRate);

            evilHackerSpawnRate = new CustomRoleOption(Types.Impostor, "evilHacker", EvilHacker.color);
            evilHackerCanHasBetterAdmin = CustomOption.Create(Types.Impostor, "evilHackerCanHasBetterAdmin", false, evilHackerSpawnRate);
            evilHackerCanSeeDoorStatus = CustomOption.Create(Types.Impostor, "evilHackerCanSeeDoorStatus", true, evilHackerSpawnRate);
            evilHackerCanCreateMadmate = CustomOption.Create(Types.Impostor, "evilHackerCanCreateMadmate", true, evilHackerSpawnRate);
            evilHackerCanCreateMadmateFromJackal = CustomOption.Create(Types.Impostor, "evilHackerCanCreateMadmateFromJackal", true, evilHackerCanCreateMadmate);
            createdMadmateCanDieToSheriff = CustomOption.Create(Types.Impostor, "createdMadmateCanDieToSheriff", true, evilHackerCanCreateMadmate);
            createdMadmateCanEnterVents = CustomOption.Create(Types.Impostor, "createdMadmateCanEnterVents", true, evilHackerCanCreateMadmate);
            createdMadmateCanFixComm = CustomOption.Create(Types.Impostor, "createdMadmateCanFixComm", false, evilHackerCanCreateMadmate);
            createdMadmateCanSabotage = CustomOption.Create(Types.Impostor, "createdMadmateCanSabotage", false, evilHackerCanCreateMadmate);
            createdMadmateHasImpostorVision = CustomOption.Create(Types.Impostor, "createdMadmateHasImpostorVision", true, evilHackerCanCreateMadmate);
            createdMadmateAbility = CustomOption.Create(Types.Impostor, "createdMadmateAbility", true, evilHackerCanCreateMadmate);
            createdMadmateCommonTasks = CustomOption.Create(Types.Impostor, "createdMadmateCommonTasks", 1f, 1f, 3f, 1f, createdMadmateAbility, false, "unitScrews");
            evilHackerCanInheritAbility = CustomOption.Create(Types.Impostor, "evilHackerCanInheritAbility", false, evilHackerSpawnRate);

            zephyrSpawnRate = new CustomRoleOption(Types.Impostor, "zephyr", Zephyr.color);
            zephyrCooldown = CustomOption.Create(Types.Impostor, "zephyrCooldown", 30f, 5f, 120f, 2.5f, zephyrSpawnRate, false, "unitSeconds");
            zephyrNumberOfCannons = CustomOption.Create(Types.Impostor, "zephyrNumberOfCannons", 5f, 1f, 10f, 1f, zephyrSpawnRate, false, "unitScrews");
            zephyrCannonRange = CustomOption.Create(Types.Impostor, "zephyrCannonPower", 5f, 2.5f, 40f, 2.5f, zephyrSpawnRate, false, "unitTimes");
            zephyrCannonAttenuation = CustomOption.Create(Types.Impostor, "zephyrCannonAttenuation", 0.75f, 0.25f, 2f, 0.125f, zephyrSpawnRate, false, "unitTimes");
            zephyrTriggerBothCooldown = CustomOption.Create(Types.Impostor, "zephyrTriggerBothCooldown", true, zephyrSpawnRate);
            zephyrLeaveEvidence = CustomOption.Create(Types.Impostor, "zephyrLeaveEvidence", true, zephyrSpawnRate);

            trapperSpawnRate = new CustomRoleOption(Types.Impostor, "trapper", Trapper.color, 1);
            trapperNumTrap = CustomOption.Create(Types.Impostor, "trapperNumTrap", 2f, 1f, 10f, 1f, trapperSpawnRate, false, "unitScrews");
            trapperExtensionTime = CustomOption.Create(Types.Impostor, "trapperExtensionTime", 5f, 2f, 10f, 0.5f, trapperSpawnRate, false, "unitSeconds");
            trapperCooldown = CustomOption.Create(Types.Impostor, "trapperCooldown", 10f, 10f, 60f, 2.5f, trapperSpawnRate, false, "unitSeconds");
            trapperKillTimer = CustomOption.Create(Types.Impostor, "trapperKillTimer", 5f, 1f, 30f, 1f, trapperSpawnRate, false, "unitSeconds");
            trapperTrapRange = CustomOption.Create(Types.Impostor, "trapperTrapRange", 1f, 0.5f, 5f, 0.1f, trapperSpawnRate, false, "unitMeters");
            trapperMaxDistance = CustomOption.Create(Types.Impostor, "trapperMaxDistance", 10f, 1f, 50f, 1f, trapperSpawnRate, false, "unitMeters");
            trapperPenaltyTime = CustomOption.Create(Types.Impostor, "trapperPenaltyTime", 10f, 0f, 50f, 1f, trapperSpawnRate, false, "unitSeconds");
            trapperBonusTime = CustomOption.Create(Types.Impostor, "trapperBonusTime", 10f, 0f, 50f, 1f, trapperSpawnRate, false, "unitSeconds");

            mimicSpawnRate = new CustomRoleOption(Types.Impostor, "mimic", MimicK.color, 1);
            mimicCountAsOne = CustomOption.Create(Types.Impostor, "mimicCountAsOne", true, mimicSpawnRate);
            mimicIfOneDiesBothDie = CustomOption.Create(Types.Impostor, "mimicIfOneDiesBothDies", true, mimicSpawnRate);
            mimicHasOneVote = CustomOption.Create(Types.Impostor, "mimicHasOneVote", true, mimicSpawnRate);
            mimicSpecialCooldown = CustomOption.Create(Types.Impostor, "mimicSpecialCooldown", 20f, 2f, 60f, 1f, mimicSpawnRate, false, "unitSeconds");

            bomberSpawnRate = new CustomRoleOption(Types.Impostor, "bomber", BomberA.color, 1);
            bomberCooldown = CustomOption.Create(Types.Impostor, "bomberCooldown", 20f, 2f, 30f, 2f, bomberSpawnRate, false, "unitSeconds");
            bomberDuration = CustomOption.Create(Types.Impostor, "bomberDuration", 2f, 1f, 10f, 0.5f, bomberSpawnRate, false, "unitSeconds");
            bomberCountAsOne = CustomOption.Create(Types.Impostor, "bomberCountAsOne", true, bomberSpawnRate);
            bomberShowEffects = CustomOption.Create(Types.Impostor, "bomberShowEffects", true, bomberSpawnRate);
            bomberIfOneDiesBothDie = CustomOption.Create(Types.Impostor, "bomberIfOneDiesBothDie", true, bomberSpawnRate);
            bomberHasOneVote = CustomOption.Create(Types.Impostor, "bomberHasOneVote", true, bomberSpawnRate);
            bomberAlwaysShowArrow = CustomOption.Create(Types.Impostor, "bomberAlwaysShowArrow", true, bomberSpawnRate);

            guesserSpawnRate = new CustomRoleOption(Types.Neutral, "guesser", Guesser.color, 1);
            guesserIsImpGuesserRate = CustomOption.Create(Types.Neutral, "guesserIsImpGuesserRate", rates, guesserSpawnRate);
            guesserNumberOfShots = CustomOption.Create(Types.Neutral, "guesserNumberOfShots", 2f, 1f, 24f, 1f, guesserSpawnRate, false, "unitShots");
            guesserHasMultipleShotsPerMeeting = CustomOption.Create(Types.Neutral, "guesserHasMultipleShotsPerMeeting", false, guesserSpawnRate);
            guesserKillsThroughShield = CustomOption.Create(Types.Neutral, "guesserKillsThroughShield", true, guesserSpawnRate);
            guesserEvilCanKillSpy = CustomOption.Create(Types.Neutral, "guesserEvilCanKillSpy", true, guesserSpawnRate);
            guesserSpawnBothRate = CustomOption.Create(Types.Neutral, "guesserSpawnBothRate", rates, guesserSpawnRate);
            guesserCantGuessSnitchIfTaksDone = CustomOption.Create(Types.Neutral, "guesserCantGuessSnitchIfTaksDone", true, guesserSpawnRate);
            guesserCantGuessFortuneTeller = CustomOption.Create(Types.Neutral, "guesserCantGuessFortuneTeller", true, guesserSpawnRate);

            swapperSpawnRate = new CustomRoleOption(Types.Neutral, "swapper", Swapper.color, 1);
            swapperIsImpRate = CustomOption.Create(Types.Neutral, "swapperIsImpRate", rates, swapperSpawnRate);
            swapperCanCallEmergency = CustomOption.Create(Types.Neutral, "swapperCanCallEmergency", false, swapperSpawnRate);
            swapperCanOnlySwapOthers = CustomOption.Create(Types.Neutral, "swapperCanOnlySwapOthers", false, swapperSpawnRate);

            swapperSwapsNumber = CustomOption.Create(Types.Neutral, "swapperSwapsNumber", 1f, 0f, 15f, 1f, swapperSpawnRate, false, "unitShots");
            swapperRechargeTasksNumber = CustomOption.Create(Types.Neutral, "swapperRechargeTasksNumber", 2f, 1f, 10f, 1f, swapperSpawnRate, false, "unitScrews");

            watcherSpawnRate = new CustomRoleOption(Types.Neutral, "watcher", Watcher.color);
            watcherAssignEqually = CustomOption.Create(Types.Neutral, "watcherAssignEqually", ["optionOn", "optionOff"], watcherSpawnRate);
            watcherIsImpWatcherRate = CustomOption.Create(Types.Neutral, "watcherisImpWatcherRate", rates, watcherAssignEqually);
            watcherSeeGuesses = CustomOption.Create(Types.Neutral, "watcherSeeGuesses", true, watcherSpawnRate);
            watcherSeeYasunaVotes = CustomOption.Create(Types.Neutral, "watcherSeeYasunaVotes", true, watcherSpawnRate);
            watcherCanKill = CustomOption.Create(Types.Neutral, "watcherCanKill", true, watcherSpawnRate);

            yasunaSpawnRate = new CustomRoleOption(Types.Neutral, "yasuna", Yasuna.color, 1);
            yasunaIsImpYasunaRate = CustomOption.Create(Types.Neutral, "yasunaIsImpYasunaRate", rates, yasunaSpawnRate);
            yasunaNumberOfSpecialVotes = CustomOption.Create(Types.Neutral, "yasunaNumberOfSpecialVotes", 1f, 1f, 15f, 1f, yasunaSpawnRate, false, "unitShots");
            yasunaSpecificMessageMode = CustomOption.Create(Types.Neutral, "yasunaSpecificMessageMode", true, yasunaSpawnRate);

            jesterSpawnRate = new CustomRoleOption(Types.Neutral, "jester", Jester.color);
            jesterCanCallEmergency = CustomOption.Create(Types.Neutral, "jesterCanCallEmergency", true, jesterSpawnRate);
            jesterHasImpostorVision = CustomOption.Create(Types.Neutral, "jesterHasImpostorVision", false, jesterSpawnRate);
            jesterCanVent = CustomOption.Create(Types.Neutral, "jesterCanVent", false, jesterSpawnRate);

            arsonistSpawnRate = new CustomRoleOption(Types.Neutral, "arsonist", Arsonist.color);
            arsonistCooldown = CustomOption.Create(Types.Neutral, "arsonistCooldown", 12.5f, 2.5f, 60f, 2.5f, arsonistSpawnRate, false, "unitSeconds");
            arsonistDuration = CustomOption.Create(Types.Neutral, "arsonistDuration", 3f, 1f, 10f, 1f, arsonistSpawnRate, false, "unitSeconds");

            jackalSpawnRate = new CustomRoleOption(Types.Neutral, "jackal", Jackal.color);
            jackalKillCooldown = CustomOption.Create(Types.Neutral, "jackalKillCooldown", 30f, 10f, 60f, 2.5f, jackalSpawnRate, false, "unitSeconds");
            jackalCreateSidekickCooldown = CustomOption.Create(Types.Neutral, "jackalCreateSidekickCooldown", 30f, 10f, 60f, 2.5f, jackalSpawnRate, false, "unitSeconds");
            jackalCanUseVents = CustomOption.Create(Types.Neutral, "jackalCanUseVents", true, jackalSpawnRate);
            jackalCanSabotageLights = CustomOption.Create(Types.Neutral, "jackalCanSabotageLights", true, jackalSpawnRate);
            jackalCanCreateSidekick = CustomOption.Create(Types.Neutral, "jackalCanCreateSidekick", false, jackalSpawnRate);
            sidekickPromotesToJackal = CustomOption.Create(Types.Neutral, "sidekickPromotesToJackal", false, jackalCanCreateSidekick);
            sidekickCanKill = CustomOption.Create(Types.Neutral, "sidekickCanKill", false, jackalCanCreateSidekick);
            sidekickCanUseVents = CustomOption.Create(Types.Neutral, "sidekickCanUseVents", true, jackalCanCreateSidekick);
            sidekickCanSabotageLights = CustomOption.Create(Types.Neutral, "sidekickCanSabotageLights", true, jackalCanCreateSidekick);
            jackalPromotedFromSidekickCanCreateSidekick = CustomOption.Create(Types.Neutral, "jackalPromotedFromSidekickCanCreateSidekick", true, sidekickPromotesToJackal);
            jackalCanCreateSidekickFromImpostor = CustomOption.Create(Types.Neutral, "jackalCanCreateSidekickFromImpostor", true, jackalCanCreateSidekick);
            jackalAndSidekickHaveImpostorVision = CustomOption.Create(Types.Neutral, "jackalAndSidekickHaveImpostorVision", false, jackalSpawnRate);

            vultureSpawnRate = new CustomRoleOption(Types.Neutral, "vulture", Vulture.color);
            vultureCooldown = CustomOption.Create(Types.Neutral, "vultureCooldown", 15f, 10f, 60f, 2.5f, vultureSpawnRate, false, "unitSeconds");
            vultureNumberToWin = CustomOption.Create(Types.Neutral, "vultureNumberToWin", 4f, 1f, 10f, 1f, vultureSpawnRate, false, "unitScrews");
            vultureCanUseVents = CustomOption.Create(Types.Neutral, "vultureCanUseVents", true, vultureSpawnRate);
            vultureShowArrows = CustomOption.Create(Types.Neutral, "vultureShowArrows", true, vultureSpawnRate);

            lawyerSpawnRate = new CustomRoleOption(Types.Neutral, "lawyer", Lawyer.color, 1);
            lawyerTargetKnows = CustomOption.Create(Types.Neutral, "lawyerTargetKnows", true, lawyerSpawnRate);
            lawyerVision = CustomOption.Create(Types.Neutral, "lawyerVision", 1f, 0.25f, 3f, 0.25f, lawyerSpawnRate, false, "unitTimes");
            lawyerKnowsRole = CustomOption.Create(Types.Neutral, "lawyerKnowsRole", false, lawyerSpawnRate);
            lawyerWinsAfterMeetings = CustomOption.Create(Types.Neutral, "lawyerWinsMeeting", false, lawyerSpawnRate);
            lawyerNeededMeetings = CustomOption.Create(Types.Neutral, "lawyerMeetingsNeeded", 5f, 1f, 15f, 1f, lawyerWinsAfterMeetings, false, "unitShots");
            lawyerTargetCanBeJester = CustomOption.Create(Types.Neutral, "lawyerTargetCanBeJester", false, lawyerSpawnRate);
            pursuerCooldown = CustomOption.Create(Types.Neutral, "pursuerCooldown", 30f, 5f, 60f, 2.5f, lawyerSpawnRate, false, "unitSeconds");
            pursuerBlanksNumber = CustomOption.Create(Types.Neutral, "pursuerBlanksNumber", 5f, 1f, 20f, 1f, lawyerSpawnRate, false, "unitScrews");

            shifterSpawnRate = new CustomRoleOption(Types.Neutral, "shifter", Shifter.color, 1);
            shifterIsNeutralRate = CustomOption.Create(Types.Neutral, "shifterIsNeutralRate", rates, shifterSpawnRate);
            shifterShiftsModifiers = CustomOption.Create(Types.Neutral, "shifterShiftsModifiers", false, shifterSpawnRate);
            shifterShiftsMedicShield = CustomOption.Create(Types.Neutral, "shifterShiftsMedicShield", false, shifterSpawnRate);
            shifterPastShifters = CustomOption.Create(Types.Neutral, "shifterPastShifters", false, shifterSpawnRate);

            opportunistSpawnRate = new CustomRoleOption(Types.Neutral, "opportunist", Opportunist.color);

            plagueDoctorSpawnRate = new CustomRoleOption(Types.Neutral, "plagueDoctor", PlagueDoctor.color, 1);
            plagueDoctorInfectCooldown = CustomOption.Create(Types.Neutral, "plagueDoctorInfectCooldown", 10f, 2.5f, 60f, 2.5f, plagueDoctorSpawnRate, false, "unitSeconds");
            plagueDoctorNumInfections = CustomOption.Create(Types.Neutral, "plagueDoctorNumInfections", 1f, 1f, 3f, 1f, plagueDoctorSpawnRate, false, "unitPlayers");
            plagueDoctorDistance = CustomOption.Create(Types.Neutral, "plagueDoctorDistance", 1f, 0.25f, 5f, 0.25f, plagueDoctorSpawnRate, false, "unitMeters");
            plagueDoctorDuration = CustomOption.Create(Types.Neutral, "plagueDoctorDuration", 5f, 1f, 30f, 1f, plagueDoctorSpawnRate, false, "unitSeconds");
            plagueDoctorImmunityTime = CustomOption.Create(Types.Neutral, "plagueDoctorImmunityTime", 10f, 1f, 30f, 1f, plagueDoctorSpawnRate, false, "unitSeconds");
            plagueDoctorInfectKiller = CustomOption.Create(Types.Neutral, "plagueDoctorInfectKiller", true, plagueDoctorSpawnRate);
            plagueDoctorWinDead = CustomOption.Create(Types.Neutral, "plagueDoctorWinDead", true, plagueDoctorSpawnRate);

            kataomoiSpawnRate = new CustomRoleOption(Types.Neutral, "kataomoi", Kataomoi.color, 1);
            kataomoiStareCooldown = CustomOption.Create(Types.Neutral, "kataomoiStareCooldown", 20f, 2.5f, 60f, 2.5f, kataomoiSpawnRate, false, "unitSeconds");
            kataomoiStareDuration = CustomOption.Create(Types.Neutral, "kataomoiStareDuration", 3f, 1f, 10f, 1f, kataomoiSpawnRate, false, "unitSeconds");
            kataomoiStareCount = CustomOption.Create(Types.Neutral, "kataomoiStareCount", 5f, 1f, 100f, 1f, kataomoiSpawnRate, false, "unitShots");
            kataomoiStalkingCooldown = CustomOption.Create(Types.Neutral, "kataomoiStalkingCooldown", 20f, 2.5f, 60f, 2.5f, kataomoiSpawnRate, false, "unitSeconds");
            kataomoiStalkingDuration = CustomOption.Create(Types.Neutral, "kataomoiStalkingDuration", 10f, 1f, 30f, 1f, kataomoiSpawnRate, false, "unitSeconds");
            kataomoiStalkingFadeTime = CustomOption.Create(Types.Neutral, "kataomoiStalkingFadeTime", 0.5f, 0.0f, 2.5f, 0.5f, kataomoiSpawnRate, false, "unitSeconds");
            kataomoiSearchCooldown = CustomOption.Create(Types.Neutral, "kataomoiSearchCooldown", 10f, 2.5f, 60f, 2.5f, kataomoiSpawnRate, false, "unitSeconds");
            kataomoiSearchDuration = CustomOption.Create(Types.Neutral, "kataomoiSearchDuration", 10f, 1f, 30f, 1f, kataomoiSpawnRate, false, "unitSeconds");

            schrodingersCatSpawnRate = new CustomRoleOption(Types.Neutral, "schrodingersCat", SchrodingersCat.color, 1);
            schrodingersCatKillCooldown = CustomOption.Create(Types.Neutral, "schrodingersCatKillCooldown", 20f, 1f, 60f, 0.5f, schrodingersCatSpawnRate, format: "unitSeconds");
            schrodingersCatBecomesImpostor = CustomOption.Create(Types.Neutral, "schrodingersCatBecomesImpostor", true, schrodingersCatSpawnRate);
            schrodingersCatCantKillUntilLastOne = CustomOption.Create(Types.Neutral, "schrodingersCatCantKillUntilLastOne", false, schrodingersCatSpawnRate);
            schrodingersCatJustDieOnKilledByCrew = CustomOption.Create(Types.Neutral, "schrodingersCatJustDieOnKilledByCrew", false, schrodingersCatSpawnRate);
            schrodingersCatHideRole = CustomOption.Create(Types.Neutral, "schrodingersCatHideRole", false, schrodingersCatSpawnRate);
            schrodingersCatCanChooseImpostor = CustomOption.Create(Types.Neutral, "schrodingersCatCanChooseTeam", false, schrodingersCatHideRole);

            doomsayerSpawnRate = new CustomRoleOption(Types.Neutral, "doomsayer", Doomsayer.color);
            doomsayerCanObserve = CustomOption.Create(Types.Neutral, "doomsayerCanObserve", true, doomsayerSpawnRate);
            doomsayerObserveCooldown = CustomOption.Create(Types.Neutral, "doomsayerObserveCooldown", 30f, 5f, 60f, 1f, doomsayerCanObserve, format: "unitSeconds");
            doomsayerNumberOfObserves = CustomOption.Create(Types.Neutral, "doomsayerNumberOfObserves", 3f, 1f, 10f, 1f, doomsayerCanObserve, format: "unitShots");
            doomsayerGuessesToWin = CustomOption.Create(Types.Neutral, "doomsayerGuessesToWin", 3f, 1f, 24f, 1f, doomsayerSpawnRate, format: "unitScrews");
            doomsayerMultipleGuesses = CustomOption.Create(Types.Neutral, "doomsayerMultipleGuesses", true, doomsayerSpawnRate);
            doomsayerMaxMisses = CustomOption.Create(Types.Neutral, "doomsayerMaxMisses", 3f, 0f, 24f, 1f, doomsayerSpawnRate, format: "unitShots");
            doomsayerIndicator = CustomOption.Create(Types.Neutral, "doomsayerIndicator", true, doomsayerSpawnRate);

            akujoSpawnRate = new CustomRoleOption(Types.Neutral, "akujo", Akujo.color, 7);
            akujoTimeLimit = CustomOption.Create(Types.Neutral, "akujoTimeLimit", 300f, 30f, 1200f, 30f, akujoSpawnRate, false, "unitSeconds");
            akujoNumKeeps = CustomOption.Create(Types.Neutral, "akujoNumKeeps", 2f, 1f, 10f, 1f, akujoSpawnRate, false, "unitPlayers");
            akujoKnowsRoles = CustomOption.Create(Types.Neutral, "akujoKnowsRoles", true, akujoSpawnRate);

            cupidSpawnRate = new CustomRoleOption(Types.Neutral, "cupid", Cupid.color, 3);
            cupidTimeLimit = CustomOption.Create(Types.Neutral, "cupidTimeLimit", 300f, 30f, 1200f, 30f, cupidSpawnRate, false, "unitSeconds");
            cupidShield = CustomOption.Create(Types.Neutral, "cupidShield", true, cupidSpawnRate);

            pelicanSpawnRate = new CustomRoleOption(Types.Neutral, "pelican", Pelican.color);
            pelicanCooldown = CustomOption.Create(Types.Neutral, "pelicanCooldown", 25f, 2.5f, 60f, 2.5f, pelicanSpawnRate, false, "unitSeconds");
            pelicanCanUseVents = CustomOption.Create(Types.Neutral, "pelicanCanUseVents", true, pelicanSpawnRate);
            pelicanHasImpVision = CustomOption.Create(Types.Neutral, "pelicanHasImpVision", true, pelicanSpawnRate);

            yandereSpawnRate = new CustomRoleOption(Types.Neutral, "yandere", Yandere.color, 1);
            yandereReducedCooldown = CustomOption.Create(Types.Neutral, "yandereReducedCooldown", 5f, 0f, 30f, 1f, yandereSpawnRate, false, "unitSeconds");
            yandereCooldownPunishment = CustomOption.Create(Types.Neutral, "yandereCooldownPunishment", 10f, 0f, 60f, 1f, yandereSpawnRate, false, "unitSeconds");
            yandereNuisanceRange = CustomOption.Create(Types.Neutral, "yandereNuisanceRange", 1.8f, 0.5f, 5f, 0.1f, yandereSpawnRate, false, "unitMeters");
            yandereNuisanceRegisterDuration = CustomOption.Create(Types.Neutral, "yandereNuisanceRegisterDuration", 2f, 0.1f, 7.5f, 0.1f, yandereSpawnRate, false, "unitSeconds");
            yandereMaxNuisance = CustomOption.Create(Types.Neutral, "yandereMaxNuisance", 4f, 1f, 24f, 1f, yandereSpawnRate, false, "unitScrews");
            yandereRunawayTimeLimit = CustomOption.Create(Types.Neutral, "yandereRunawayTimeLimit", 60f, 10f, 180f, 10f, yandereSpawnRate, false, "unitSeconds");
            yandereHasImpVision = CustomOption.Create(Types.Neutral, "yandereHasImpVision", true, yandereSpawnRate);

            puppeteerSpawnRate = new CustomRoleOption(Types.Neutral, "puppeteer", Puppeteer.color, 1);
            puppeteerNumKills = CustomOption.Create(Types.Neutral, "puppeteerNumKills", 3f, 1f, 15f, 1f, puppeteerSpawnRate);
            puppeteerSampleDuration = CustomOption.Create(Types.Neutral, "puppeteerSampleDuration", 1f, 0f, 20f, 0.25f, puppeteerSpawnRate);
            puppeteerCanControlDummyEvenIfDead = CustomOption.Create(Types.Neutral, "puppeteerCanControlDummyEvenIfDead", true, puppeteerSpawnRate);
            puppeteerPenaltyOnDeath = CustomOption.Create(Types.Neutral, "puppeteerPenaltyOnDeath", 1f, 0f, 5f, 1f, puppeteerCanControlDummyEvenIfDead);
            puppeteerLosesSenriganOnDeath = CustomOption.Create(Types.Neutral, "puppeteerLosesSenriganOnDeath", true, puppeteerCanControlDummyEvenIfDead);

            jekyllAndHydeSpawnRate = new CustomRoleOption(Types.Neutral, "jekyllAndHyde", JekyllAndHyde.color);
            jekyllAndHydeNumberToWin = CustomOption.Create(Types.Neutral, "jekyllAndHydeNumberToWin", 3f, 1f, 10f, 1f, jekyllAndHydeSpawnRate, false, "unitScrews");
            jekyllAndHydeCooldown = CustomOption.Create(Types.Neutral, "jekyllAndHydeCooldown", 18f, 2f, 30f, 1f, jekyllAndHydeSpawnRate, false, "unitSeconds");
            jekyllAndHydeSuicideTimer = CustomOption.Create(Types.Neutral, "jekyllAndHydeSuicideTimer", 40f, 2.5f, 60f, 2.5f, jekyllAndHydeSpawnRate, false, "unitSeconds");
            jekyllAndHydeResetAfterMeeting = CustomOption.Create(Types.Neutral, "jekyllAndHydeResetAfterMeeting", true, jekyllAndHydeSpawnRate);
            jekyllAndHydeCommonTasks = CustomOption.Create(Types.Neutral, "jekyllAndHydeCommonTasks", 1f, 1f, 4f, 1f, jekyllAndHydeSpawnRate, false, "unitScrews");
            jekyllAndHydeShortTasks = CustomOption.Create(Types.Neutral, "jekyllAndHydeShortTasks", 3f, 1f, 20f, 1f, jekyllAndHydeSpawnRate, false, "unitScrews");
            jekyllAndHydeLongTasks = CustomOption.Create(Types.Neutral, "jekyllAndHydeLongTasks", 2f, 0f, 6f, 1f, jekyllAndHydeSpawnRate, false, "unitScrews");
            jekyllAndHydeNumTasks = CustomOption.Create(Types.Neutral, "jekyllAndHydeNumTasks", 3f, 1f, 10f, 1f, jekyllAndHydeSpawnRate, false, "unitScrews");

            foxSpawnRate = new CustomRoleOption(Types.Neutral, "fox", Fox.color, 1);
            foxNumTasks = CustomOption.Create(Types.Neutral, "foxNumTasks", 4f, 1f, 10f, 1f, foxSpawnRate, false, "unitScrews");
            foxStayTime = CustomOption.Create(Types.Neutral, "foxStayTime", 5f, 1f, 20f, 1f, foxSpawnRate, false, "unitSeconds");
            foxTaskType = CustomOption.Create(Types.Neutral, "foxTaskType", ["foxTaskSerial", "foxTaskParallel"], foxSpawnRate);
            foxCrewWinsByTasks = CustomOption.Create(Types.Neutral, "foxCrewWinsByTasks", true, foxSpawnRate);
            foxImpostorWinsBySabotage = CustomOption.Create(Types.Neutral, "foxImpostorWinsBySabotage", true, foxSpawnRate);
            foxStealthCooldown = CustomOption.Create(Types.Neutral, "foxStealthCooldown", 15f, 1f, 30f, 1f, foxSpawnRate, false, "unitSeconds");
            foxStealthDuration = CustomOption.Create(Types.Neutral, "foxStealthDuration", 15f, 1f, 30f, 1f, foxSpawnRate, false, "unitSeconds");
            foxCanCreateImmoralist = CustomOption.Create(Types.Neutral, "foxCanCreateImmoralist", true, foxSpawnRate);
            foxNumRepairs = CustomOption.Create(Types.Neutral, "foxNumRepair", 1f, 0f, 10f, 1f, foxSpawnRate, false, "unitShots");

            mayorSpawnRate = new CustomRoleOption(Types.Crewmate, "mayor", Mayor.color);
            mayorNumVotes = CustomOption.Create(Types.Crewmate, "mayorNumVotes", 2f, 2f, 24f, 1f, mayorSpawnRate, false, "unitVotes");
            mayorMeetingButton = CustomOption.Create(Types.Crewmate, "mayorMeetingButton", true, mayorSpawnRate);
            mayorMaxRemoteMeetings = CustomOption.Create(Types.Crewmate, "mayorMaxRemoteMeetings", 1f, 1f, 5f, 1f, mayorMeetingButton, false, "unitShots");
            mayorChooseSingleVote = CustomOption.Create(Types.Crewmate, "mayorChooseSingleVote", ["optionOff", "mayorOnBeforeVoting", "mayorOnUntilMeeting"], mayorSpawnRate);

            engineerSpawnRate = new CustomRoleOption(Types.Crewmate, "engineer", Engineer.color);
            engineerNumberOfFixes = CustomOption.Create(Types.Crewmate, "engineerNumberOfFixes", 1f, 1f, 6f, 1f, engineerSpawnRate, false, "unitShots");
            engineerHighlightForImpostors = CustomOption.Create(Types.Crewmate, "engineerHighlightForImpostors", true, engineerSpawnRate);
            engineerHighlightForTeamJackal = CustomOption.Create(Types.Crewmate, "engineerHighlightForTeamJackal", true, engineerSpawnRate);

            sheriffSpawnRate = new CustomRoleOption(Types.Crewmate, "sheriff", Sheriff.color);
            sheriffCooldown = CustomOption.Create(Types.Crewmate, "sheriffCooldown", 30f, 10f, 60f, 2.5f, sheriffSpawnRate, false, "unitSeconds");
            sheriffCanKillNeutrals = CustomOption.Create(Types.Crewmate, "sheriffCanKillNeutrals", false, sheriffSpawnRate);
            deputySpawnRate = CustomOption.Create(Types.Crewmate, "sheriffDeputy", rates, sheriffSpawnRate);
            deputyRoleCount = CustomOption.Create(Types.Crewmate, "deputyRoleCount", 1f, 1f, 24f, 1f, deputySpawnRate, format: "unitPlayers");
            deputyNumberOfHandcuffs = CustomOption.Create(Types.Crewmate, "deputyNumberOfHandcuffs", 3f, 1f, 10f, 1f, deputySpawnRate, false, "unitScrews");
            deputyHandcuffCooldown = CustomOption.Create(Types.Crewmate, "deputyHandcuffCooldown", 30f, 10f, 60f, 2.5f, deputySpawnRate, false, "unitSeconds");
            deputyHandcuffDuration = CustomOption.Create(Types.Crewmate, "deputyHandcuffDuration", 15f, 5f, 60f, 2.5f, deputySpawnRate, false, "unitSeconds");
            deputyKnowsSheriff = CustomOption.Create(Types.Crewmate, "deputyKnowsSheriff", true, deputySpawnRate);
            deputyGetsPromoted = CustomOption.Create(Types.Crewmate, "deputyGetsPromoted", ["optionOff", "deputyOnImmediately", "deputyOnAfterMeeting"], deputySpawnRate);
            deputyKeepsHandcuffs = CustomOption.Create(Types.Crewmate, "deputyKeepsHandcuffs", true, deputyGetsPromoted);
            deputyStopsGameEnd = CustomOption.Create(Types.Crewmate, "deputyStopsGameEnd", false, deputySpawnRate);

            lighterSpawnRate = new CustomRoleOption(Types.Crewmate, "lighter", Lighter.color);
            lighterModeLightsOnVision = CustomOption.Create(Types.Crewmate, "lighterModeLightsOnVision", 1.5f, 0.25f, 5f, 0.25f, lighterSpawnRate, false, "unitTimes");
            lighterModeLightsOffVision = CustomOption.Create(Types.Crewmate, "lighterModeLightsOffVision", 0.5f, 0.25f, 5f, 0.25f, lighterSpawnRate, false, "unitTimes");
            lighterCooldown = CustomOption.Create(Types.Crewmate, "lighterCooldown", 30f, 5f, 120f, 5f, lighterSpawnRate, format: "unitSeconds");
            lighterDuration = CustomOption.Create(Types.Crewmate, "lighterDuration", 5f, 2.5f, 60f, 2.5f, lighterSpawnRate, format: "unitSeconds");
            lighterCanSeeInvisible = CustomOption.Create(Types.Crewmate, "lighterCanSeeInvisible", true, lighterSpawnRate);

            sprinterSpawnRate = new CustomRoleOption(Types.Crewmate, "sprinter", Sprinter.color);
            sprinterCooldown = CustomOption.Create(Types.Crewmate, "sprinterCooldown", 30f, 2.5f, 60f, 2.5f, sprinterSpawnRate, false, "unitSeconds");
            sprinterDuration = CustomOption.Create(Types.Crewmate, "sprintDuration", 15f, 10f, 60f, 2.5f, sprinterSpawnRate, false, "unitSeconds");
            sprinterFadeTime = CustomOption.Create(Types.Crewmate, "sprintFadeTime", 0.5f, 0.0f, 2.5f, 0.5f, sprinterSpawnRate, false, "unitSeconds");
            sprinterSpeedBonus = CustomOption.Create(Types.Crewmate, "sprinterSpeedBonus", 1.25f, 0.5f, 2f, 0.25f, sprinterSpawnRate, false, "unitTimes");

            detectiveSpawnRate = new CustomRoleOption(Types.Crewmate, "detective", Detective.color);
            detectiveAnonymousFootprints = CustomOption.Create(Types.Crewmate, "detectiveAnonymousFootprints", false, detectiveSpawnRate);
            detectiveFootprintIntervall = CustomOption.Create(Types.Crewmate, "detectiveFootprintInterval", 0.5f, 0.25f, 10f, 0.25f, detectiveSpawnRate, false, "unitSeconds");
            detectiveFootprintDuration = CustomOption.Create(Types.Crewmate, "detectiveFootprintDuration", 5f, 0.25f, 10f, 0.25f, detectiveSpawnRate, false, "unitSeconds");
            detectiveReportNameDuration = CustomOption.Create(Types.Crewmate, "detectiveReportNameDuration", 0, 0, 60, 2.5f, detectiveSpawnRate, false, "unitSeconds");
            detectiveReportColorDuration = CustomOption.Create(Types.Crewmate, "detectiveReportColorDuration", 20, 0, 120, 2.5f, detectiveSpawnRate, false, "unitSeconds");
            detectiveInspectCooldown = CustomOption.Create(Types.Crewmate, "detectiveInspectCooldown", 15f, 5f, 60f, 1f, detectiveSpawnRate, format: "unitSeconds");
            detectiveInspectDuration = CustomOption.Create(Types.Crewmate, "detectiveInspectDuration", 10f, 3f, 60f, 1f, detectiveSpawnRate, false, "unitSeconds");

            timeMasterSpawnRate = new CustomRoleOption(Types.Crewmate, "timeMaster", TimeMaster.color);
            timeMasterCooldown = CustomOption.Create(Types.Crewmate, "timeMasterCooldown", 30f, 10f, 120f, 2.5f, timeMasterSpawnRate, false, "unitSeconds");
            timeMasterRewindTime = CustomOption.Create(Types.Crewmate, "timeMasterRewindTime", 3f, 1f, 10f, 1f, timeMasterSpawnRate, false, "unitSeconds");
            timeMasterShieldDuration = CustomOption.Create(Types.Crewmate, "timeMasterShieldDuration", 3f, 1f, 20f, 1f, timeMasterSpawnRate, false, "unitSeconds");
            timeMasterReviveDuringRewind = CustomOption.Create(Types.Crewmate, "timeMasterRewindDuringRewind", false, timeMasterSpawnRate);

            medicSpawnRate = new CustomRoleOption(Types.Crewmate, "medic", Medic.color);
            medicShowShielded = CustomOption.Create(Types.Crewmate, "medicShowShielded", ["medicShowShieldedAll", "medicShowShieldedBoth", "medicShowShieldedMedic"], medicSpawnRate);
            medicShowAttemptToShielded = CustomOption.Create(Types.Crewmate, "medicShowAttemptToShielded", false, medicSpawnRate);
            medicSetOrShowShieldAfterMeeting = CustomOption.Create(Types.Crewmate, "medicSetOrShowShieldAfterMeeting", ["medicInstantly", "medicVisibleAfterMeeting", "medicAftermeeting"], medicSpawnRate);
            medicShowAttemptToMedic = CustomOption.Create(Types.Crewmate, "medicShowAttemptToMedic", false, medicSpawnRate);
            medicCanUseVitals = CustomOption.Create(Types.Crewmate, "medicCanUseVitals", true, medicSpawnRate);
            medicSeesDeathReasonOnVitals = CustomOption.Create(Types.Crewmate, "medicSeesDeathReasonOnVitals", true, medicCanUseVitals);

            fortuneTellerSpawnRate = new CustomRoleOption(Types.Crewmate, "fortuneTeller", FortuneTeller.color);
            fortuneTellerResults = CustomOption.Create(Types.Crewmate, "fortuneTellerResults", ["fortuneTellerResultCrew", "fortuneTellerResultTeam", "fortuneTellerResultRole"], fortuneTellerSpawnRate);
            fortuneTellerNumTasks = CustomOption.Create(Types.Crewmate, "fortuneTellerNumTasks", 4f, 0f, 25f, 1f, fortuneTellerSpawnRate, false, "unitScrews");
            fortuneTellerDuration = CustomOption.Create(Types.Crewmate, "fortuneTellerDuration", 20f, 1f, 50f, 1f, fortuneTellerSpawnRate, false, "unitSeconds");
            fortuneTellerDistance = CustomOption.Create(Types.Crewmate, "fortuneTellerDistance", 2.5f, 1f, 10f, 0.5f, fortuneTellerSpawnRate, false, "unitMeters");
            fortuneTellerRevealOnImpDivine = CustomOption.Create(Types.Crewmate, "fortuneTellerRevealOnImpDivine", true, fortuneTellerSpawnRate);

            collatorSpawnRate = new CustomRoleOption(Types.Crewmate, "collator", Collator.color);
            collatorCooldown = CustomOption.Create(Types.Crewmate, "collatorCooldown", 15f, 1f, 60f, 1f, collatorSpawnRate, false, "unitSeconds");
            collatorNumberOfTrials = CustomOption.Create(Types.Crewmate, "collatorNumberOfTrials", 2f, 1f, 15f, 1f, collatorSpawnRate, false, "unitScrews");
            collatorMadmateSpecifiedAsCrewmate = CustomOption.Create(Types.Crewmate, "collatorMadmateSpecifiedAsCrewmate", true, collatorSpawnRate);
            collatorStrictNeutralRoles = CustomOption.Create(Types.Crewmate, "collatorStrictNeutralRoles", false, collatorSpawnRate);

            seerSpawnRate = new CustomRoleOption(Types.Crewmate, "seer", Seer.color);
            seerMode = CustomOption.Create(Types.Crewmate, "seerMode", ["seerModeBoth", "seerModeFlash", "seerModeSouls"], seerSpawnRate);
            seerLimitSoulDuration = CustomOption.Create(Types.Crewmate, "seerLimitSoulDuration", false, seerSpawnRate);
            seerSoulDuration = CustomOption.Create(Types.Crewmate, "seerSoulDuration", 15f, 0f, 120f, 5f, seerLimitSoulDuration, false, "unitSeconds");
            seerCanSeeKillTeams = CustomOption.Create(Types.Crewmate, "seerCanSeeKillTeams", true, seerSpawnRate);

            hackerSpawnRate = new CustomRoleOption(Types.Crewmate, "hacker", Hacker.color);
            hackerCooldown = CustomOption.Create(Types.Crewmate, "hackerCooldown", 30f, 5f, 60f, 5f, hackerSpawnRate, false, "unitSeconds");
            hackerHackeringDuration = CustomOption.Create(Types.Crewmate, "hackerHackeringDuration", 10f, 2.5f, 60f, 2.5f, hackerSpawnRate, false, "unitSeconds");
            hackerOnlyColorType = CustomOption.Create(Types.Crewmate, "hackerOnlyColorType", false, hackerSpawnRate);
            hackerToolsNumber = CustomOption.Create(Types.Crewmate, "hackerToolsNumber", 5f, 1f, 30f, 1f, hackerSpawnRate, false, "unitScrews");
            hackerRechargeTasksNumber = CustomOption.Create(Types.Crewmate, "hackerRechargeTasksNumber", 2f, 1f, 5f, 1f, hackerSpawnRate, false, "unitScrews");
            hackerNoMove = CustomOption.Create(Types.Crewmate, "hackerNoMove", true, hackerSpawnRate);

            noisemakerSpawnRate = new CustomRoleOption(Types.Crewmate, "noisemaker", Noisemaker.color);
            noisemakerCooldown = CustomOption.Create(Types.Crewmate, "noisemakerCooldown", 30f, 5f, 120f, 2.5f, noisemakerSpawnRate, false, "unitSeconds");
            noisemakerSoundDuration = CustomOption.Create(Types.Crewmate, "noisemakerSoundDuration", 5f, 2.5f, 20f, 2.5f, noisemakerSpawnRate, false, "unitSeconds");
            noisemakerSoundNumber = CustomOption.Create(Types.Crewmate, "noisemakerSoundNumber", 5f, 3f, 20f, 1f, noisemakerSpawnRate, false, "unitScrews");
            noisemakerSoundTarget = CustomOption.Create(Types.Crewmate, "noisemakerSoundTarget", ["noisemakerSoundNoisemaker", "noisemakerSoundCrewmate", "noisemakerSoundEveryone"], noisemakerSpawnRate);

            baitSpawnRate = new CustomRoleOption(Types.Crewmate, "bait", Bait.color);
            baitHighlightAllVents = CustomOption.Create(Types.Crewmate, "baitHighlightAllVents", false, baitSpawnRate);
            baitReportDelay = CustomOption.Create(Types.Crewmate, "baitReportDelay", 0f, 0f, 10f, 1f, baitSpawnRate, false, "unitSeconds");
            baitShowKillFlash = CustomOption.Create(Types.Crewmate, "baitShowKillFlash", true, baitSpawnRate);
            baitCanBeGuessed = CustomOption.Create(Types.Crewmate, "baitCanBeGuessed", true, baitSpawnRate);
            baitEmitCooldown = CustomOption.Create(Types.Crewmate, "baitEmitCooldown", 30f, 1f, 60f, 1f, baitSpawnRate, false, "unitSeconds");
            baitNumberOfEmits = CustomOption.Create(Types.Crewmate, "baitNumberOfEmits", 5f, 1f, 10f, 1f, baitSpawnRate, false, "unitScrews");

            veteranSpawnRate = new CustomRoleOption(Types.Crewmate, "veteran", Veteran.color);
            veteranCooldown = CustomOption.Create(Types.Crewmate, "veteranCooldown", 30f, 10f, 60f, 2.5f, veteranSpawnRate, false, "unitSeconds");
            veteranAlertDuration = CustomOption.Create(Types.Crewmate, "veteranAlertDuration", 3f, 1f, 20f, 1f, veteranSpawnRate, false, "unitSeconds");
            veteranAlertNumber = CustomOption.Create(Types.Crewmate, "veteranAlertNumber", 5f, 1f, 15f, 1f, veteranSpawnRate, false, "unitScrews");

            jailorSpawnRate = new CustomRoleOption(Types.Crewmate, "jailor", Jailor.color);
            jailorCooldown = CustomOption.Create(Types.Crewmate, "jailorCooldown", 30f, 1f, 60f, 1f, jailorSpawnRate, false, "unitSeconds");
            jailorNumberOfJails = CustomOption.Create(Types.Crewmate, "jailorNumberOfJails", 5f, 1f, 15f, 1f, jailorSpawnRate, false, "unitScrews");
            jailorSuicidesIfFalseJail = CustomOption.Create(Types.Crewmate, "jailorSuicidesIfFalseJail", true, jailorSpawnRate);
            jailorTargetDiesIfFalseJail = CustomOption.Create(Types.Crewmate, "jailorTargetDiesIfFalseJail", false, jailorSpawnRate);

            buskerSpawnRate = new CustomRoleOption(Types.Crewmate, "busker", Busker.color);
            buskerCooldown = CustomOption.Create(Types.Crewmate, "buskerCooldown", 20f, 5f, 60f, 2.5f, buskerSpawnRate, false, "unitSeconds");
            buskerDuration = CustomOption.Create(Types.Crewmate, "buskerDuration", 10f, 5f, 30f, 2.5f, buskerSpawnRate, false, "unitSeconds");
            buskerRestrictInformation = CustomOption.Create(Types.Crewmate, "buskerRestrictInformation", true, buskerSpawnRate);

            teleporterSpawnRate = new CustomRoleOption(Types.Crewmate, "teleporter", Teleporter.color);
            teleporterCooldown = CustomOption.Create(Types.Crewmate, "teleporterCooldown", 30f, 5f, 120f, 5f, teleporterSpawnRate, false, "unitSeconds");
            teleporterTeleportNumber = CustomOption.Create(Types.Crewmate, "teleporterTeleportNumber", 3f, 1f, 10f, 1f, teleporterSpawnRate, false, "unitScrews");

            trackerSpawnRate = new CustomRoleOption(Types.Crewmate, "tracker", Tracker.color);
            trackerUpdateIntervall = CustomOption.Create(Types.Crewmate, "trackerUpdateInterval", 5f, 1f, 30f, 1f, trackerSpawnRate, false, "unitSeconds");
            trackerResetTargetAfterMeeting = CustomOption.Create(Types.Crewmate, "trackerResetTargetAfterMeeting", false, trackerSpawnRate);
            trackerCanTrackCorpses = CustomOption.Create(Types.Crewmate, "trackerCanTrackCorpses", true, trackerSpawnRate);
            trackerCorpsesTrackingCooldown = CustomOption.Create(Types.Crewmate, "trackerCorpsesTrackingCooldown", 30f, 5f, 120f, 5f, trackerCanTrackCorpses, false, "unitSeconds");
            trackerCorpsesTrackingDuration = CustomOption.Create(Types.Crewmate, "trackerCorpsesTrackingDuration", 5f, 2.5f, 30f, 2.5f, trackerCanTrackCorpses, false, "unitSeconds");
            trackerTrackingMethod = CustomOption.Create(Types.Crewmate, "trackerTrackingMethod", ["trackerArrow", "trackerProximity", "trackerBoth"], trackerSpawnRate);
            trackerCanKill = CustomOption.Create(Types.Crewmate, "trackerCanKill", true, trackerSpawnRate);
            trackerKillCooldown = CustomOption.Create(Types.Crewmate, "trackerKillCooldown", 30f, 5f, 60f, 2.5f, trackerCanKill, false, "unitSeconds");

            sherlockSpawnRate = new CustomRoleOption(Types.Crewmate, "sherlock", Sherlock.color);
            sherlockCooldown = CustomOption.Create(Types.Crewmate, "sherlockCooldown", 10f, 0f, 40f, 2.5f, sherlockSpawnRate, false, "unitSeconds");
            sherlockInvestigateDistance = CustomOption.Create(Types.Crewmate, "sherlockInvestigateDistance", 5f, 1f, 15f, 1f, sherlockSpawnRate, false, "unitMeters");
            sherlockRechargeTasksNumber = CustomOption.Create(Types.Crewmate, "sherlockRechargeTasksNumber", 2f, 1f, 5f, 1f, sherlockSpawnRate, false, "unitScrews");

            snitchSpawnRate = new CustomRoleOption(Types.Crewmate, "snitch", Snitch.color);
            snitchLeftTasksForReveal = CustomOption.Create(Types.Crewmate, "snitchLeftTasksForReveal", 5f, 0f, 25f, 1f, snitchSpawnRate, false, "unitScrews");
            snitchIncludeTeamEvil = CustomOption.Create(Types.Crewmate, "snitchIncludeTeamEvil", true, snitchSpawnRate);
            snitchTeamEvilUseDifferentArrowColor = CustomOption.Create(Types.Crewmate, "snitchTeamEvilUseDifferentArrowColor", true, snitchIncludeTeamEvil);
            snitchSeesRoles = CustomOption.Create(Types.Crewmate, "snitchSeesRoles", true, snitchSpawnRate);

            archaeologistSpawnRate = new CustomRoleOption(Types.Crewmate, "archaeologist", Archaeologist.color, 1);
            archaeologistCooldown = CustomOption.Create(Types.Crewmate, "archaeologistCooldown", 20f, 5f, 60f, 1f, archaeologistSpawnRate, format: "unitSeconds");
            archaeologistArrowDuration = CustomOption.Create(Types.Crewmate, "archaeologistArrowDuration", 5f, 1f, 60f, 1f, archaeologistSpawnRate, format: "unitSeconds");
            archaeologistExploreDuration = CustomOption.Create(Types.Crewmate, "archaeologistExploreDuration", 3f, 0f, 15f, 1f, archaeologistSpawnRate, format: "unitSeconds");
            archaeologistNumCandidates = CustomOption.Create(Types.Crewmate, "archaeologistNumCandidates", 3f, 2f, 6f, 1f, archaeologistSpawnRate, format: "unitPlayers");
            archaeologistRevealAntiqueMode = CustomOption.Create(Types.Crewmate, "archaeologistRevealAntiqueMode", ["archaeologistModeNever", "archaeologistModeImmediately", "archaeologistModeAfterMeeting"], archaeologistSpawnRate);

            spySpawnRate = new CustomRoleOption(Types.Crewmate, "spy", Spy.color);
            spyCanDieToSheriff = CustomOption.Create(Types.Crewmate, "spyCanDieToSheriff", false, spySpawnRate);
            spyImpostorsCanKillAnyone = CustomOption.Create(Types.Crewmate, "spyImpostorsCanKillAnyone", true, spySpawnRate);
            spyCanEnterVents = CustomOption.Create(Types.Crewmate, "spyCanEnterVents", false, spySpawnRate);
            spyHasImpostorVision = CustomOption.Create(Types.Crewmate, "spyHasImpostorVision", false, spySpawnRate);

            taskMasterSpawnRate = new CustomRoleOption(Types.Crewmate, "taskMaster", TaskMaster.color, 1);
            taskMasterBecomeATaskMasterWhenCompleteAllTasks = CustomOption.Create(Types.Crewmate, "taskMasterBecomeATaskMasterWhenCompleteAllTasks", false, taskMasterSpawnRate);
            taskMasterExtraCommonTasks = CustomOption.Create(Types.Crewmate, "taskMasterExtraCommonTasks", 2f, 0f, 3f, 1f, taskMasterSpawnRate, false, "unitScrews");
            taskMasterExtraShortTasks = CustomOption.Create(Types.Crewmate, "taskMasterExtraShortTasks", 2f, 1f, 23f, 1f, taskMasterSpawnRate, false, "unitScrews");
            taskMasterExtraLongTasks = CustomOption.Create(Types.Crewmate, "taskMasterExtraLongTasks", 2f, 0f, 15f, 1f, taskMasterSpawnRate, false, "unitScrews");
            taskMasterCanVent = CustomOption.Create(Types.Crewmate, "taskMasterCanVent", true, taskMasterSpawnRate);

            portalmakerSpawnRate = new CustomRoleOption(Types.Crewmate, "portalmaker", Portalmaker.color, 1);
            portalmakerCooldown = CustomOption.Create(Types.Crewmate, "portalmakerCooldown", 30f, 10f, 60f, 2.5f, portalmakerSpawnRate, false, "unitSeconds");
            portalmakerUsePortalCooldown = CustomOption.Create(Types.Crewmate, "portalmakerUsePortalCooldown", 30f, 10f, 60f, 2.5f, portalmakerSpawnRate, false, "unitSeconds");
            portalmakerLogOnlyColorType = CustomOption.Create(Types.Crewmate, "portalmakerLogOnlyColorType", true, portalmakerSpawnRate);
            portalmakerLogHasTime = CustomOption.Create(Types.Crewmate, "portalmakerLogHasTime", true, portalmakerSpawnRate);
            portalmakerCanPortalFromAnywhere = CustomOption.Create(Types.Crewmate, "portalmakerCanPortalFromAnywhere", true, portalmakerSpawnRate);

            securityGuardSpawnRate = new CustomRoleOption(Types.Crewmate, "securityGuard", SecurityGuard.color, 1);
            securityGuardCooldown = CustomOption.Create(Types.Crewmate, "securityGuardCooldown", 30f, 10f, 60f, 2.5f, securityGuardSpawnRate, false, "unitSeconds");
            securityGuardTotalScrews = CustomOption.Create(Types.Crewmate, "securityGuardTotalScrews", 7f, 1f, 15f, 1f, securityGuardSpawnRate, false, "unitScrews");
            securityGuardCamPrice = CustomOption.Create(Types.Crewmate, "securityGuardCamPrice", 2f, 1f, 15f, 1f, securityGuardSpawnRate, false, "unitScrews");
            securityGuardVentPrice = CustomOption.Create(Types.Crewmate, "securityGuardVentPrice", 1f, 1f, 15f, 1f, securityGuardSpawnRate, false, "unitScrews");
            securityGuardFlushCooldown = CustomOption.Create(Types.Crewmate, "securityGuardFlushCooldown", 30f, 5f, 120f, 1f, securityGuardSpawnRate, false, "unitSeconds");
            securityGuardCamDuration = CustomOption.Create(Types.Crewmate, "securityGuardCamDuration", 10f, 2.5f, 60f, 2.5f, securityGuardSpawnRate, false, "unitSeconds");
            securityGuardCamMaxCharges = CustomOption.Create(Types.Crewmate, "securityGuardCamMaxCharges", 5f, 1f, 30f, 1f, securityGuardSpawnRate, false, "unitScrews");
            securityGuardCamRechargeTasksNumber = CustomOption.Create(Types.Crewmate, "securityGuardCamRechargeTasksNumber", 3f, 1f, 10f, 1f, securityGuardSpawnRate, false, "unitScrews");
            securityGuardNoMove = CustomOption.Create(Types.Crewmate, "securityGuardNoMove", true, securityGuardSpawnRate);

            mediumSpawnRate = new CustomRoleOption(Types.Crewmate, "medium", Medium.color);
            mediumCooldown = CustomOption.Create(Types.Crewmate, "mediumCooldown", 30f, 5f, 120f, 5f, mediumSpawnRate, false, "unitSeconds");
            mediumDuration = CustomOption.Create(Types.Crewmate, "mediumDuration", 3f, 0f, 15f, 1f, mediumSpawnRate, false, "unitSeconds");
            mediumOneTimeUse = CustomOption.Create(Types.Crewmate, "mediumOneTimeUse", false, mediumSpawnRate);
            mediumRevealTarget = CustomOption.Create(Types.Crewmate, "mediumRevealTarget", true, mediumSpawnRate);
            mediumChanceAdditionalInfo = CustomOption.Create(Types.Crewmate, "mediumChanceAdditionalInfo", rates, mediumSpawnRate);

            thiefSpawnRate = new CustomRoleOption(Types.Neutral, "thief", Thief.color);
            thiefCooldown = CustomOption.Create(Types.Neutral, "thiefCooldown", 30f, 5f, 120f, 5f, thiefSpawnRate, false, "unitSeconds");
            thiefCanKillSheriff = CustomOption.Create(Types.Neutral, "thiefCanKillSheriff", true, thiefSpawnRate);
            thiefHasImpVision = CustomOption.Create(Types.Neutral, "thiefHasImpVision", true, thiefSpawnRate);
            thiefCanUseVents = CustomOption.Create(Types.Neutral, "thiefCanUseVents", true, thiefSpawnRate);
            thiefCanStealWithGuess = CustomOption.Create(Types.Neutral, "thiefCanStealWithGuess", false, thiefSpawnRate);

            moriartySpawnRate = new CustomRoleOption(Types.Neutral, "moriarty", Moriarty.color);
            moriartyBrainwashCooldown = CustomOption.Create(Types.Neutral, "moriartyBrainwashCooldown", 30f, 10f, 60f, 1f, moriartySpawnRate, false, "unitSeconds");
            moriartyBrainwashTime = CustomOption.Create(Types.Neutral, "moriartyBrainwashTime", 30f, 1f, 60f, 1f, moriartySpawnRate, false, "unitSeconds");
            moriartyNumberToWin = CustomOption.Create(Types.Neutral, "moriartyNumberToWin", 3f, 1f, 10f, 1f, moriartySpawnRate, false, "unitScrews");
            moriartySherlockAddition = CustomOption.Create(Types.Neutral, "moriartySherlockAddition", 2f, 0f, 5f, 1f, moriartySpawnRate, false, "unitScrews");
            moriartyKillIndicate = CustomOption.Create(Types.Neutral, "moriartyKillIndicate", false, moriartySpawnRate);

            /*trapperSpawnRate = CustomOption.Create(Types.Crewmate, cs(Trapper.color, "Trapper"), rates, null, true);
            trapperCooldown = CustomOption.Create(Types.Crewmate, "Trapper Cooldown", 30f, 5f, 120f, 5f, trapperSpawnRate);
            trapperMaxCharges = CustomOption.Create(Types.Crewmate, "Max Traps Charges", 5f, 1f, 15f, 1f, trapperSpawnRate);
            trapperRechargeTasksNumber = CustomOption.Create(Types.Crewmate, "Number Of Tasks Needed For Recharging", 2f, 1f, 15f, 1f, trapperSpawnRate);
            trapperTrapNeededTriggerToReveal = CustomOption.Create(Types.Crewmate, "Trap Needed Trigger To Reveal", 3f, 2f, 10f, 1f, trapperSpawnRate);
            trapperAnonymousMap = CustomOption.Create(Types.Crewmate, "Show Anonymous Map", false, trapperSpawnRate);
            trapperInfoType = CustomOption.Create(Types.Crewmate, "Trap Information Type", new string[] { "Role", "Good/Evil Role", "Name" }, trapperSpawnRate);
            trapperTrapDuration = CustomOption.Create(Types.Crewmate, "Trap Duration", 5f, 1f, 15f, 1f, trapperSpawnRate);*/

            // Modifier (1000 - 1999)
            modifiersAreHidden = CustomOption.Create(Types.Modifier, cs(Color.yellow, "vipbloodyHidden"), true, null, true, heading: cs(Color.yellow, "modifiersAreHidden"));

            modifierBloody = CustomOption.Create(Types.Modifier, cs(Color.yellow, "bloody"), rates, null, true, color: Color.yellow);
            modifierBloodyQuantity = CustomOption.Create(Types.Modifier, cs(Color.yellow, "bloodyQuantity"), ratesModifier, modifierBloody);
            modifierBloodyDuration = CustomOption.Create(Types.Modifier, "bloodDuration", 10f, 3f, 60f, 1f, modifierBloody, false, "unitSeconds");

            modifierAntiTeleport = CustomOption.Create(Types.Modifier, cs(Color.yellow, "antiTeleport"), rates, null, true, color: Color.yellow);
            modifierAntiTeleportQuantity = CustomOption.Create(Types.Modifier, cs(Color.yellow, "antiTeleportQuantity"), ratesModifier, modifierAntiTeleport);

            modifierTieBreaker = CustomOption.Create(Types.Modifier, cs(Color.yellow, "tiebreakerLongDesc"), rates, null, true, color: Color.yellow);

            modifierRadar = CustomOption.Create(Types.Modifier, cs(Color.yellow, "radar"), rates, null, true, color: Color.yellow);

            modifierLover = CustomOption.Create(Types.Modifier, cs(Color.yellow, "lovers"), rates, null, true, color: Color.yellow);
            modifierLoverImpLoverRate = CustomOption.Create(Types.Modifier, "loversImpLoverRate", rates, modifierLover);
            modifierLoverQuantity = CustomOption.Create(Types.Modifier, "loversQuantity", 1f, 1f, 6f, 1f, modifierLover, format: "unitCouples");
            modifierLoverBothDie = CustomOption.Create(Types.Modifier, "loversBothDie", true, modifierLover);
            modifierLoverEnableChat = CustomOption.Create(Types.Modifier, "loversEnableChat", true, modifierLover);

            modifierSunglasses = CustomOption.Create(Types.Modifier, cs(Color.yellow, "sunglasses"), rates, null, true, color: Color.yellow);
            modifierSunglassesQuantity = CustomOption.Create(Types.Modifier, cs(Color.yellow, "sunglassesQuantity"), ratesModifier, modifierSunglasses);
            modifierSunglassesVision = CustomOption.Create(Types.Modifier, "sunglassesVision", ["-10%", "-20%", "-30%", "-40%", "-50%"], modifierSunglasses);

            modifierMini = CustomOption.Create(Types.Modifier, cs(Color.yellow, "mini"), rates, null, true, color: Color.yellow);
            modifierMiniGrowingUpDuration = CustomOption.Create(Types.Modifier, "miniGrowingUpDuration", 400f, 100f, 1500f, 100f, modifierMini, false, "unitSeconds");
            modifierMiniGrowingUpInMeeting = CustomOption.Create(Types.Modifier, "miniGrowingUpInMeeting", true, modifierMini);
            if (Utilities.EventUtility.canBeEnabled || Utilities.EventUtility.isEnabled)
            {
                eventKicksPerRound = CustomOption.Create(Types.Modifier, cs(Color.green, "eventKicksPerRound"), 4f, 0f, 14f, 1f, modifierMini);
                eventHeavyAge = CustomOption.Create(Types.Modifier, cs(Color.green, "eventHeavyAge"), 12f, 6f, 18f, 0.5f, modifierMini);
                eventReallyNoMini = CustomOption.Create(Types.Modifier, cs(Color.green, "eventReallyNoMini"), false, modifierMini, invertedParent: true);
            }

            modifierVip = CustomOption.Create(Types.Modifier, cs(Color.yellow, "vip"), rates, null, true, color: Color.yellow);
            modifierVipQuantity = CustomOption.Create(Types.Modifier, cs(Color.yellow, "vipQuantity"), ratesModifier, modifierVip);
            modifierVipShowColor = CustomOption.Create(Types.Modifier, "vipShowColor", true, modifierVip);

            modifierInvert = CustomOption.Create(Types.Modifier, cs(Color.yellow, "invert"), rates, null, true, color: Color.yellow);
            modifierInvertQuantity = CustomOption.Create(Types.Modifier, cs(Color.yellow, "invertQuantity"), ratesModifier, modifierInvert);
            modifierInvertDuration = CustomOption.Create(Types.Modifier, "invertDuration", 3f, 1f, 15f, 1f, modifierInvert, false, "unitScrews");

            modifierDiseased = CustomOption.Create(Types.Modifier, cs(Color.yellow, "diseased"), rates, null, true, color: Color.yellow);
            modifierDiseasedQuantity = CustomOption.Create(Types.Modifier, cs(Color.yellow, "diseasedQuantity"), ratesModifier, modifierDiseased);
            modifierDiseasedMultiplier = CustomOption.Create(Types.Modifier, "diseasedMultiplier", 3f, 1.25f, 5f, 0.25f, modifierDiseased, false, "unitTimes");

            modifierChameleon = CustomOption.Create(Types.Modifier, cs(Color.yellow, "chameleon"), rates, null, true, color: Color.yellow);
            modifierChameleonQuantity = CustomOption.Create(Types.Modifier, cs(Color.yellow, "chameleonQuantity"), ratesModifier, modifierChameleon);
            modifierChameleonHoldDuration = CustomOption.Create(Types.Modifier, "chameleonHoldDuration", 3f, 1f, 10f, 0.5f, modifierChameleon, false, "unitSeconds");
            modifierChameleonFadeDuration = CustomOption.Create(Types.Modifier, "chameleonFadeDuration", 1f, 0.25f, 10f, 0.25f, modifierChameleon, false, "unitSeconds");
            modifierChameleonMinVisibility = CustomOption.Create(Types.Modifier, "chameleonMinVisibility", ["0%", "10%", "20%", "30%", "40%", "50%"], modifierChameleon);

            modifierMultitasker = CustomOption.Create(Types.Modifier, cs(Color.yellow, "multitasker"), rates, null, true, color: Color.yellow);
            modifierMultitaskerQuantity = CustomOption.Create(Types.Modifier, cs(Color.yellow, "multitaskerQuantity"), ratesModifier, modifierMultitasker);

            modifierArmored = CustomOption.Create(Types.Modifier, cs(Color.yellow, "armored"), rates, null, true, color: Color.yellow);

            madmateSpawnRate = CustomOption.Create(Types.Modifier, cs(Color.yellow, "madmate"), rates, null, true, color: Color.yellow);
            madmateQuantity = CustomOption.Create(Types.Modifier, cs(Color.yellow, "madmateQuantity"), ratesModifier, madmateSpawnRate);
            madmateFixedRole = CustomOption.Create(Types.Modifier, "madmateFixedRole", Madmate.validRoles, madmateSpawnRate);
            madmateFixedRoleGuesserGamemode = CustomOption.Create(Types.Modifier, "madmateFixedRole", Madmate.validRoles.Where(x => x != RoleId.NiceGuesser).ToList(), madmateSpawnRate);
            madmateAbility = CustomOption.Create(Types.Modifier, "madmateAbility", true, madmateSpawnRate);
            madmateCommonTasks = CustomOption.Create(Types.Modifier, "madmateCommonTasks", 1f, 0f, 3f, 1f, madmateAbility, false, "unitScrews");
            madmateShortTasks = CustomOption.Create(Types.Modifier, "madmateShortTasks", 3f, 0f, 4f, 1f, madmateAbility, false, "unitScrews");
            madmateLongTasks = CustomOption.Create(Types.Modifier, "madmateLongTasks", 1f, 0f, 4f, 1f, madmateAbility, false, "unitScrews");
            madmateCanDieToSheriff = CustomOption.Create(Types.Modifier, "madmateCanDieToSheriff", false, madmateSpawnRate);
            madmateCanEnterVents = CustomOption.Create(Types.Modifier, "madmateCanEnterVents", false, madmateSpawnRate);
            madmateCanSabotage = CustomOption.Create(Types.Modifier, "madmateCanSabotage", false, madmateSpawnRate);
            madmateHasImpostorVision = CustomOption.Create(Types.Modifier, "madmateHasImpostorVision", false, madmateSpawnRate);
            madmateCanFixComm = CustomOption.Create(Types.Modifier, "madmateCanFixComm", true, madmateSpawnRate);

            //modifierShifter = CustomOption.Create(Types.Modifier, cs(Color.yellow, "Shifter"), rates, null, true);

            // Guesser Gamemode (2000 - 2999)
            guesserGamemodeCrewNumber = CustomOption.Create(Types.Guesser, cs(Guesser.color, "guesserGamemodeCrewNumber"), 24f, 0f, 24f, 1f, null, true, "unitPlayers", heading: "headingAmountOfGuessers");
            guesserGamemodeNeutralNumber = CustomOption.Create(Types.Guesser, cs(Guesser.color, "guesserGamemodeNeutralNumber"), 24f, 0f, 24f, 1f, null, false, "unitPlayers");
            guesserGamemodeImpNumber = CustomOption.Create(Types.Guesser, cs(Guesser.color, "guesserGamemodeImpNumber"), 24f, 0f, 24f, 1f, null, false, "unitPlayers");
            guesserForceJackalGuesser = CustomOption.Create(Types.Guesser, "guesserForceJackalGuesser", false, null, true, heading: "headingForceGuesser");
            guesserGamemodeSidekickIsAlwaysGuesser = CustomOption.Create(Types.Guesser, "guesserGamemodeSidekickIsAlwaysGuesser", false, null);
            guesserForceThiefGuesser = CustomOption.Create(Types.Guesser, "guesserForceThiefGuesser", false, null, true);
            guesserGamemodeHaveModifier = CustomOption.Create(Types.Guesser, "guesserGamemodeHaveModifier", true, null, true, heading: "headingGeneralGuesser");
            guesserGamemodeNumberOfShots = CustomOption.Create(Types.Guesser, "guesserGamemodeNumberOfShots", 3f, 1f, 24f, 1f, null, false, "unitShots");
            guesserGamemodeHasMultipleShotsPerMeeting = CustomOption.Create(Types.Guesser, "guesserGamemodeHasMultipleShotsPerMeeting", false, null);
            guesserGamemodeCrewGuesserNumberOfTasks = CustomOption.Create(Types.Guesser, "guesserGamemodeCrewGuesserNumberOfTasks", 0f, 0f, 15f, 1f, null, format: "unitScrews");
            guesserGamemodeKillsThroughShield = CustomOption.Create(Types.Guesser, "guesserGamemodeKillsThroughShield", true, null);
            guesserGamemodeEvilCanKillSpy = CustomOption.Create(Types.Guesser, "guesserGamemodeEvilCanKillSpy", true, null);
            guesserGamemodeCantGuessSnitchIfTaksDone = CustomOption.Create(Types.Guesser, "guesserGamemodeCantGuessSnitchIfTaksDone", true, null);
            guesserGamemodeCantGuessFortuneTeller = CustomOption.Create(Types.Guesser, "guesserGamemodeCantGuessFortuneTeller", true, null);
            guesserGamemodeEnableLastImpostor = CustomOption.Create(Types.Guesser, "guesserGamemodeEnableLastImpostor", false, null, true, heading: "headingLastImpostor");
            guesserGamemodeLastImpostorNumKills = CustomOption.Create(Types.Guesser, "guesserGamemodeLastImpostorNumKills", 3f, 0f, 24f, 1f, guesserGamemodeEnableLastImpostor, format: "unitPlayers");
            guesserGamemodeLastImpostorNumShots = CustomOption.Create(Types.Guesser, "guesserGamemodeLastImpostorNumShots", 3f, 1f, 24f, 1f, guesserGamemodeEnableLastImpostor, format: "unitShots");
            guesserGamemodeLastImpostorHasMultipleShots = CustomOption.Create(Types.Guesser, "guesserGamemodeLastImpostorHasMultipleShots", true, guesserGamemodeEnableLastImpostor);

            // Hide N Seek Gamemode (3000 - 3999)
            hideNSeekMap = CustomOption.Create(Types.HideNSeekMain, cs(Color.yellow, "hideNSeekMap"), ["The Skeld", "Mira", "Polus", "Airship", "Fungle"], null, true, onChange: () => { int map = hideNSeekMap.selection; if (map >= 3) map++; GameOptionsManager.Instance.currentNormalGameOptions.MapId = (byte)map; });
            hideNSeekHunterCount = CustomOption.Create(Types.HideNSeekMain, cs(Color.yellow, "hideNSeekHunterCount"), 1f, 1f, 3f, 1f, format: "unitPlayers");
            hideNSeekKillCooldown = CustomOption.Create(Types.HideNSeekMain, cs(Color.yellow, "hideNSeekKillCooldown"), 10f, 2.5f, 60f, 2.5f, format: "unitSeconds");
            hideNSeekHunterVision = CustomOption.Create(Types.HideNSeekMain, cs(Color.yellow, "hideNSeekHunterVision"), 0.5f, 0.25f, 2f, 0.25f, format: "unitTimes");
            hideNSeekHuntedVision = CustomOption.Create(Types.HideNSeekMain, cs(Color.yellow, "hideNSeekHuntedVision"), 2f, 0.25f, 5f, 0.25f, format: "unitTimes");
            hideNSeekCommonTasks = CustomOption.Create(Types.HideNSeekMain, cs(Color.yellow, "hideNSeekCommonTasks"), 1f, 0f, 4f, 1f, format: "unitScrews");
            hideNSeekShortTasks = CustomOption.Create(Types.HideNSeekMain, cs(Color.yellow, "hideNSeekShortTasks"), 3f, 1f, 23f, 1f, format: "unitScrews");
            hideNSeekLongTasks = CustomOption.Create(Types.HideNSeekMain, cs(Color.yellow, "hideNSeekLongTasks"), 3f, 0f, 15f, 1f, format: "unitScrews");
            hideNSeekTimer = CustomOption.Create(Types.HideNSeekMain, cs(Color.yellow, "hideNSeekTimer"), 5f, 1f, 30f, 1f);
            hideNSeekTaskWin = CustomOption.Create(Types.HideNSeekMain, cs(Color.yellow, "hideNSeekTaskWin"), false);
            hideNSeekTaskPunish = CustomOption.Create(Types.HideNSeekMain, cs(Color.yellow, "hideNSeekTaskPunish"), 10f, 0f, 30f, 1f, format: "unitSeconds");
            hideNSeekCanSabotage = CustomOption.Create(Types.HideNSeekMain, cs(Color.yellow, "hideNSeekCanSabotage"), false);
            hideNSeekHunterWaiting = CustomOption.Create(Types.HideNSeekMain, cs(Color.yellow, "hideNSeekHunterWaiting"), 15f, 2.5f, 60f, 2.5f, format: "unitSeconds");

            hunterLightCooldown = CustomOption.Create(Types.HideNSeekRoles, cs(Color.red, "hunterLightCooldown"), 30f, 5f, 60f, 1f, null, true, "unitSeconds", heading: "headingHunterLight");
            hunterLightDuration = CustomOption.Create(Types.HideNSeekRoles, cs(Color.red, "hunterLightDuration"), 5f, 1f, 60f, 1f, format: "unitSeconds");
            hunterLightVision = CustomOption.Create(Types.HideNSeekRoles, cs(Color.red, "hunterLightVision"), 3f, 1f, 5f, 0.25f, format: "unitTimes");
            hunterLightPunish = CustomOption.Create(Types.HideNSeekRoles, cs(Color.red, "hunterLightPunish"), 5f, 0f, 30f, 1f, format: "unitSeconds");
            hunterAdminCooldown = CustomOption.Create(Types.HideNSeekRoles, cs(Color.red, "hunterAdminCooldown"), 30f, 5f, 60f, 1f, format: "unitSeconds");
            hunterAdminDuration = CustomOption.Create(Types.HideNSeekRoles, cs(Color.red, "hunterAdminDuration"), 5f, 1f, 60f, 1f, format: "unitSeconds");
            hunterAdminPunish = CustomOption.Create(Types.HideNSeekRoles, cs(Color.red, "hunterAdminPunish"), 5f, 0f, 30f, 1f, format: "unitSeconds");
            hunterArrowCooldown = CustomOption.Create(Types.HideNSeekRoles, cs(Color.red, "hunterArrowCooldown"), 30f, 5f, 60f, 1f, format: "unitSeconds");
            hunterArrowDuration = CustomOption.Create(Types.HideNSeekRoles, cs(Color.red, "hunterArrowDuration"), 5f, 0f, 60f, 1f, format: "unitSeconds");
            hunterArrowPunish = CustomOption.Create(Types.HideNSeekRoles, cs(Color.red, "hunterArrowPunish"), 5f, 0f, 30f, 1f, format: "unitSeconds");

            huntedShieldCooldown = CustomOption.Create(Types.HideNSeekRoles, cs(Color.gray, "huntedShieldCooldown"), 30f, 5f, 60f, 1f, null, true, "unitSeconds", heading: "headingHuntedShield");
            huntedShieldDuration = CustomOption.Create(Types.HideNSeekRoles, cs(Color.gray, "huntedShieldDuration"), 5f, 1f, 60f, 1f, format: "unitSeconds");
            huntedShieldRewindTime = CustomOption.Create(Types.HideNSeekRoles, cs(Color.gray, "huntedShieldRewindTime"), 3f, 1f, 10f, 1f, format: "unitSeconds");
            huntedShieldNumber = CustomOption.Create(Types.HideNSeekRoles, cs(Color.gray, "huntedShieldNumber"), 3f, 1f, 15f, 1f, format: "unitScrews");

            // Other options
            maxNumberOfMeetings = CustomOption.Create(Types.General, "maxNumberOfMeetings", 10, 0, 15, 1, null, true, "unitShots", heading: "headingGameplay");
            freePlayGameModeNumDummies = CustomOption.Create(Types.General, cs(Color.green, "freePlayGameModeNumDummies"), 1f, 0f, 23f, 1f, format: "unitPlayers");
            anyPlayerCanStopStart = CustomOption.Create(Types.General, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "anyPlayerCanStopStart"), false, null, false);
            blockSkippingInEmergencyMeetings = CustomOption.Create(Types.General, "blockSkippingInEmergencyMeetings", false);
            noVoteIsSelfVote = CustomOption.Create(Types.General, "noVoteIsSelfVote", false, blockSkippingInEmergencyMeetings);
            hidePlayerNames = CustomOption.Create(Types.General, "hidePlayerNames", false);
            allowParallelMedBayScans = CustomOption.Create(Types.General, "allowParallelMedBayScans", false);
            shieldFirstKill = CustomOption.Create(Types.General, "shieldFirstKill", false);
            finishTasksBeforeHauntingOrZoomingOut = CustomOption.Create(Types.General, "finishTasksBeforeHauntingOrZoomingOut", true);
            enableImpostorChat = CustomOption.Create(Types.General, "enableImpostorChat", true);
            additionalVents = CustomOption.Create(Types.General, "additionalVents", false);
            specimenVital = CustomOption.Create(Types.General, "specimenVital", false);
            airshipLadder = CustomOption.Create(Types.General, "airshipLadder", false);
            airshipOptimize = CustomOption.Create(Types.General, "airshipOptimize", false);
            airshipAdditionalSpawn = CustomOption.Create(Types.General, "airshipAdditionalSpawn", false);
            fungleElectrical = CustomOption.Create(Types.General, "fungleElectrical", false);
            randomGameStartPosition = CustomOption.Create(Types.General, "randomGameStartPosition", false);

            camsNightVision = CustomOption.Create(Types.General, "camsNightVision", false, null, true, heading: "headingNightVision");
            camsNoNightVisionIfImpVision = CustomOption.Create(Types.General, "camsNoNightVisionIfImpVision", false, camsNightVision, false);

            activateProps = CustomOption.Create(Types.General, "activateProps", false, null, true, heading: "headingPropSetting");
            numAccelTraps = CustomOption.Create(Types.General, "numAccelTraps", 1f, 0f, 5f, 1f, activateProps, false, "unitScrews");
            accelerationDuration = CustomOption.Create(Types.General, "accelerationDuration", 5f, 1f, 20f, 1f, activateProps, false, "unitSeconds");
            speedAcceleration = CustomOption.Create(Types.General, "speedAcceleration", 1.25f, 0.5f, 2f, 0.25f, activateProps, false, "unitTimes");
            numDecelTraps = CustomOption.Create(Types.General, "numDecelTraps", 1f, 0f, 3f, 1f, activateProps, false, "unitScrews");
            decelerationDuration = CustomOption.Create(Types.General, "decelerationDuration", 5f, 1f, 20f, 1f, activateProps, false, "unitSeconds");
            speedDeceleration = CustomOption.Create(Types.General, "speedDeceleration", -0.5f, -0.8f, -0.1f, 0.1f, activateProps, false, "unitTimes");
            decelUpdateInterval = CustomOption.Create(Types.General, "decelUpdateInterval", 10f, 5f, 60f, 2.5f, activateProps, false, "unitSeconds");


            dynamicMap = CustomOption.Create(Types.General, "dynamicMap", false, null, true, heading: "headingMapSetting");
            dynamicMapEnableSkeld = CustomOption.Create(Types.General, "Skeld", rates, dynamicMap, false);
            dynamicMapEnableMira = CustomOption.Create(Types.General, "Mira", rates, dynamicMap, false);
            dynamicMapEnablePolus = CustomOption.Create(Types.General, "Polus", rates, dynamicMap, false);
            dynamicMapEnableAirShip = CustomOption.Create(Types.General, "Airship", rates, dynamicMap, false);
            dynamicMapEnableSubmerged = CustomOption.Create(Types.General, "Submerged", rates, dynamicMap, false);
            dynamicMapEnableFungle = CustomOption.Create(Types.General, "Fungle", rates, dynamicMap, false);
            dynamicMapSeparateSettings = CustomOption.Create(Types.General, "dynamicMapSeparateSettings", false, dynamicMap, false);

            blockedRolePairings.Add((byte)RoleId.Vampire, [(byte)RoleId.Warlock]);
            blockedRolePairings.Add((byte)RoleId.Warlock, [(byte)RoleId.Vampire]);
            blockedRolePairings.Add((byte)RoleId.Spy, [(byte)RoleId.Mini]);
            blockedRolePairings.Add((byte)RoleId.Mini, [(byte)RoleId.Spy]);
            blockedRolePairings.Add((byte)RoleId.Vulture, [(byte)RoleId.Cleaner]);
            blockedRolePairings.Add((byte)RoleId.Cleaner, [(byte)RoleId.Vulture]);
        }
    }
}
