using System;
using System.Collections.Generic;
using System.Linq;
using TheOtherRoles.Roles;
using TheOtherRoles.Utilities;
using UnityEngine;
using static Il2CppSystem.Globalization.TimeSpanFormat;

namespace TheOtherRoles
{
    public class RoleInfo {
        public Color color;
        public Color orgColor;
        public string name { get { return ModTranslation.getString(nameKey); } }
        public string introDescription { get { return ModTranslation.getString(nameKey + "IntroDesc"); } }
        public string shortDescription { get { return ModTranslation.getString(nameKey + "ShortDesc"); } }
        public string fullDescription { get { return ModTranslation.getString(nameKey + "FullDesc"); } }
        public string blurb { get { return ModTranslation.getString(nameKey + "Blurb"); } }
        public RoleId roleId;
        public bool isNeutral;
        public bool isOrgNeutral;
        public bool isModifier;
        public string nameKey;
        private bool roleIsImpostor(bool isOrg) => (isOrg ? orgColor : color) == Palette.ImpostorRed && !(roleId == RoleId.Spy);
        public bool isImpostor => roleIsImpostor(false);
        public bool isOrgImpostor => roleIsImpostor(true);
        public static Dictionary<(RoleId, bool), RoleInfo> roleInfoById = new();    // For those that share the same RoleId

        public RoleInfo(string name, Color color, RoleId roleId, bool isNeutral = false, bool isModifier = false)
        {
            nameKey = name;
            this.color = orgColor = color;
            this.roleId = roleId;
            this.isNeutral = isOrgNeutral = isNeutral;
            this.isModifier = isModifier;
            roleInfoById.TryAdd((roleId, nameKey is "niceSwapper" or "niceShifter"), this);
        }

        public static RoleInfo jester = new("jester", Jester.color, RoleId.Jester, true);
        public static RoleInfo mayor = new("mayor", Mayor.color, RoleId.Mayor);
        public static RoleInfo portalmaker = new("portalmaker", Portalmaker.color, RoleId.Portalmaker);
        public static RoleInfo engineer = new("engineer", Engineer.color, RoleId.Engineer);
        public static RoleInfo sheriff = new("sheriff", Sheriff.color, RoleId.Sheriff);
        public static RoleInfo deputy = new("deputy", Sheriff.color, RoleId.Deputy);
        public static RoleInfo lighter = new("lighter", Lighter.color, RoleId.Lighter);
        public static RoleInfo godfather = new("godfather", Godfather.color, RoleId.Godfather);
        public static RoleInfo mafioso = new("mafioso", Mafioso.color, RoleId.Mafioso);
        public static RoleInfo janitor = new("janitor", Janitor.color, RoleId.Janitor);
        public static RoleInfo morphling = new("morphling", Morphling.color, RoleId.Morphling);
        public static RoleInfo camouflager = new("camouflager", Camouflager.color, RoleId.Camouflager);
        public static RoleInfo vampire = new("vampire", Vampire.color, RoleId.Vampire);
        public static RoleInfo eraser = new("eraser", Eraser.color, RoleId.Eraser);
        public static RoleInfo trickster = new("trickster", Trickster.color, RoleId.Trickster);
        public static RoleInfo cleaner = new("cleaner", Cleaner.color, RoleId.Cleaner);
        public static RoleInfo warlock = new("warlock", Warlock.color, RoleId.Warlock);
        public static RoleInfo bountyHunter = new("bountyHunter", BountyHunter.color, RoleId.BountyHunter);
        public static RoleInfo detective = new("detective", Detective.color, RoleId.Detective);
        public static RoleInfo bait = new("bait", Bait.color, RoleId.Bait);
        public static RoleInfo timeMaster = new("timeMaster", TimeMaster.color, RoleId.TimeMaster);
        public static RoleInfo medic = new("medic", Medic.color, RoleId.Medic);
        public static RoleInfo niceSwapper = new("niceSwapper", Swapper.color, RoleId.Swapper);
        public static RoleInfo seer = new("seer", Seer.color, RoleId.Seer);
        public static RoleInfo hacker = new("hacker", Hacker.color, RoleId.Hacker);
        public static RoleInfo niceshifter = new("niceShifter", Shifter.color, RoleId.Shifter);
        public static RoleInfo tracker = new("tracker", Tracker.color, RoleId.Tracker);
        public static RoleInfo snitch = new("snitch", Snitch.color, RoleId.Snitch);
        public static RoleInfo jackal = new("jackal", Jackal.color, RoleId.Jackal, true);
        public static RoleInfo sidekick = new("sidekick", Sidekick.color, RoleId.Sidekick, true);
        public static RoleInfo spy = new("spy", Spy.color, RoleId.Spy);
        public static RoleInfo securityGuard = new("securityGuard", SecurityGuard.color, RoleId.SecurityGuard);
        public static RoleInfo arsonist = new("arsonist", Arsonist.color, RoleId.Arsonist, true);
        public static RoleInfo goodGuesser = new("niceGuesser", Guesser.color, RoleId.NiceGuesser);
        public static RoleInfo badGuesser = new("evilGuesser", Palette.ImpostorRed, RoleId.EvilGuesser);
        public static RoleInfo niceWatcher = new("niceWatcher", Watcher.color, RoleId.NiceWatcher);
        public static RoleInfo evilWatcher = new("evilWatcher", Palette.ImpostorRed, RoleId.EvilWatcher);
        public static RoleInfo vulture = new("vulture", Vulture.color, RoleId.Vulture, true);
        public static RoleInfo medium = new("medium", Medium.color, RoleId.Medium);
        //public static RoleInfo trapper = new RoleInfo("Trapper", Trapper.color, "Place traps to find the Impostors", "Place traps", RoleId.Trapper);
        public static RoleInfo lawyer = new("lawyer", Lawyer.color, RoleId.Lawyer, true);
        //public static RoleInfo prosecutor = new RoleInfo("Prosecutor", Lawyer.color, "Vote out your target", "Vote out your target", RoleId.Prosecutor, true);
        public static RoleInfo pursuer = new("pursuer", Pursuer.color, RoleId.Pursuer, true);
        public static RoleInfo impostor = new("impostor", Palette.ImpostorRed, RoleId.Impostor);
        public static RoleInfo crewmate = new("crewmate", Color.white, RoleId.Crewmate);
        public static RoleInfo witch = new("witch", Witch.color, RoleId.Witch);
        public static RoleInfo assassin = new("assassin", Assassin.color, RoleId.Assassin);
        public static RoleInfo thief = new("thief", Thief.color, RoleId.Thief, true);
        //public static RoleInfo bomber = new RoleInfo("Bomber", Bomber.color, "Bomb all Crewmates", "Bomb all Crewmates", RoleId.Bomber);

        // GMIA functional
        public static RoleInfo ninja = new("ninja", Ninja.color, RoleId.Ninja);
        public static RoleInfo nekoKabocha = new("nekoKabocha", NekoKabocha.color, RoleId.NekoKabocha);
        public static RoleInfo serialKiller = new("serialKiller", SerialKiller.color, RoleId.SerialKiller);
        public static RoleInfo evilTracker = new("evilTracker", EvilTracker.color, RoleId.EvilTracker);
        public static RoleInfo undertaker = new("undertaker", Undertaker.color, RoleId.Undertaker);
        public static RoleInfo mimicK = new("mimicK", MimicK.color, RoleId.MimicK);
        public static RoleInfo mimicA = new("mimicA", MimicA.color, RoleId.MimicA);
        public static RoleInfo bomberA = new("bomber", BomberA.color, RoleId.BomberA);
        public static RoleInfo bomberB = new("bomber", BomberB.color, RoleId.BomberB);
        public static RoleInfo evilSwapper = new("evilSwapper", Palette.ImpostorRed, RoleId.Swapper);
        public static RoleInfo evilHacker = new("evilHacker", EvilHacker.color, RoleId.EvilHacker);
        public static RoleInfo trapper = new("trapper", Trapper.color, RoleId.Trapper);
        public static RoleInfo blackmailer = new("blackmailer", Blackmailer.color, RoleId.Blackmailer);
        public static RoleInfo yoyo = new("yoyo", Yoyo.color, RoleId.Yoyo);
        public static RoleInfo zephyr = new("zephyr", Zephyr.color, RoleId.Zephyr);
        public static RoleInfo fortuneTeller = new("fortuneTeller", FortuneTeller.color, RoleId.FortuneTeller);
        public static RoleInfo veteran = new("veteran", Veteran.color, RoleId.Veteran);
        public static RoleInfo sprinter = new("sprinter", Sprinter.color, RoleId.Sprinter);
        public static RoleInfo sherlock = new("sherlock", Sherlock.color, RoleId.Sherlock);
        public static RoleInfo yasuna = new("niceYasuna", Yasuna.color, RoleId.Yasuna);
        public static RoleInfo taskMaster = new("taskMaster", TaskMaster.color, RoleId.TaskMaster);
        public static RoleInfo teleporter = new("teleporter", Teleporter.color, RoleId.Teleporter);
        public static RoleInfo busker = new("busker", Busker.color, RoleId.Busker);
        public static RoleInfo noisemaker = new("noisemaker", Noisemaker.color, RoleId.Noisemaker);
        public static RoleInfo archaeologist = new("archaeologist", Archaeologist.color, RoleId.Archaeologist);
        public static RoleInfo collator = new("collator", Collator.color, RoleId.Collator);
        public static RoleInfo jailor = new("jailor", Jailor.color, RoleId.Jailor);
        public static RoleInfo evilYasuna = new("evilYasuna", Palette.ImpostorRed, RoleId.EvilYasuna);
        public static RoleInfo opportunist = new("opportunist", Opportunist.color, RoleId.Opportunist, true);
        public static RoleInfo chainshifter = new("corruptedShifter", Shifter.color, RoleId.Shifter, true);
        public static RoleInfo moriarty = new("moriarty", Moriarty.color, RoleId.Moriarty, true);
        public static RoleInfo akujo = new("akujo", Akujo.color, RoleId.Akujo, true);
        public static RoleInfo plagueDoctor = new("plagueDoctor", PlagueDoctor.color, RoleId.PlagueDoctor, true);
        public static RoleInfo puppeteer = new("puppeteer", Puppeteer.color, RoleId.Puppeteer, true);
        public static RoleInfo jekyllAndHyde = new("jekyllAndHyde", JekyllAndHyde.color, RoleId.JekyllAndHyde, true);
        public static RoleInfo cupid = new("cupid", Cupid.color, RoleId.Cupid, true);
        public static RoleInfo fox = new("fox", Fox.color, RoleId.Fox, true);
        public static RoleInfo immoralist = new("immoralist", Immoralist.color, RoleId.Immoralist, true);
        public static RoleInfo schrodingersCat = new("schrodingersCat", SchrodingersCat.color, RoleId.SchrodingersCat, true);
        public static RoleInfo kataomoi = new("kataomoi", Kataomoi.color, RoleId.Kataomoi, true);
        public static RoleInfo doomsayer = new("doomsayer", Doomsayer.color, RoleId.Doomsayer, true);
        public static RoleInfo pelican = new("pelican", Pelican.color, RoleId.Pelican, true);
        public static RoleInfo yandere = new("yandere", Yandere.color, RoleId.Yandere, true);

        public static RoleInfo hunter = new("hunter", Palette.ImpostorRed, RoleId.Impostor);
        public static RoleInfo hunted = new("hunted", Color.white, RoleId.Crewmate);

        // Modifier
        public static RoleInfo bloody = new("bloody", Color.yellow, RoleId.Bloody, false, true);
        public static RoleInfo antiTeleport = new("antiTeleportPostfix", Color.yellow, RoleId.AntiTeleport, false, true);
        public static RoleInfo tiebreaker = new("tiebreaker", Color.yellow, RoleId.Tiebreaker, false, true);
        //public static RoleInfo bait = new RoleInfo("Bait", Color.yellow, "Bait your enemies", "Bait your enemies", RoleId.Bait, false, true);
        public static RoleInfo sunglasses = new("sunglasses", Color.yellow, RoleId.Sunglasses, false, true);
        public static RoleInfo lover = new("lover", Lovers.color, RoleId.Lover, false, true);
        public static RoleInfo mini = new("mini", Color.yellow, RoleId.Mini, false, true);
        public static RoleInfo vip = new("vip", Color.yellow, RoleId.Vip, false, true);
        public static RoleInfo invert = new("invert", Color.yellow, RoleId.Invert, false, true);
        public static RoleInfo chameleon = new("chameleon", Color.yellow, RoleId.Chameleon, false, true);
        public static RoleInfo multitasker = new("multitasker", Color.yellow, RoleId.Multitasker, false, true);
        public static RoleInfo diseased = new("diseased", Color.yellow, RoleId.Diseased, false, true);
        public static RoleInfo radar = new("radar", Color.yellow, RoleId.Radar, false, true);
        public static RoleInfo armored = new ("armored", Color.yellow, RoleId.Armored, false, true);
        //public static RoleInfo shifter = new RoleInfo("Shifter", Color.yellow, "Shift your role", "Shift your role", RoleId.Shifter, false, true);

        public static List<RoleInfo> Killing = [
            sheriff,
            deputy,
            jailor,
            bountyHunter,
            witch,
            jekyllAndHyde,
            thief,
            serialKiller,
            pelican
        ];

        public static List<RoleInfo> Trick =
        [
            nekoKabocha,
            ninja,
            veteran,
            niceSwapper,
            evilSwapper,
            mayor,
            bait
        ];

        public static List<RoleInfo> Detect =
        [
            snitch,
            fortuneTeller,
            hacker,
            evilHacker,
            blackmailer,
            archaeologist,
            collator
        ];

        public static List<RoleInfo> Body =
        [
            sherlock,
            medium,
            vulture,
            cleaner,
            undertaker,
            vampire,
            niceWatcher,
            evilWatcher
        ];

        public static List<RoleInfo> Panic =
        [
            teleporter,
            yasuna,
            evilYasuna,
            trapper,
            zephyr,
            plagueDoctor,
            eraser,
            morphling,
            doomsayer
        ];

        public static List<RoleInfo> Team =
        [
            godfather,
            mafioso,
            janitor,
            bomberA,
            bomberB,
            jackal,
            sidekick,
            fox,
            immoralist,
            mimicK,
            mimicA,
            spy
        ];

        public static List<RoleInfo> Protection =
        [
            akujo,
            cupid,
            medic,
            yandere,
            lawyer,
            engineer,
            camouflager,
            securityGuard
        ];

        public static List<RoleInfo> Outlook =
        [
            schrodingersCat,
            moriarty,
            warlock,
            seer,
            taskMaster,
            timeMaster,
            portalmaker
        ];

        public static List<RoleInfo> Hunting =
        [
            tracker,
            evilTracker,
            detective,
            sprinter,
            kataomoi,
            assassin,
            noisemaker,
            lighter,
            yoyo
        ];

        static public List<List<RoleInfo>> Complex =
        [
            [godfather, mafioso, janitor],
            [mimicK, mimicA],
            [bomberA],
            [goodGuesser, badGuesser],
            [niceSwapper, evilSwapper],
            [niceWatcher, evilWatcher],
            [yasuna, evilYasuna],
            [niceshifter, chainshifter]
        ];

        public static List<RoleInfo> allRoleInfos = new() {
            impostor,
            godfather,
            mafioso,
            janitor,
            morphling,
            camouflager,
            vampire,
            eraser,
            trickster,
            cleaner,
            warlock,
            bountyHunter,
            witch,
            assassin,
            ninja,
            nekoKabocha,
            serialKiller,
            evilTracker,
            evilHacker,
            evilSwapper,
            trapper,
            undertaker,
            blackmailer,
            mimicK,
            mimicA,
            bomberA,
            bomberB,
            evilYasuna,
            yoyo,
            zephyr,
            goodGuesser,
            badGuesser,
            evilWatcher,
            lover,
            jester,
            arsonist,
            jackal,
            sidekick,
            vulture,
            pursuer,
            lawyer,
            thief,
            opportunist,
            chainshifter,
            plagueDoctor,
            fox,
            immoralist,
            akujo,
            puppeteer,
            jekyllAndHyde,
            moriarty,
            cupid,
            schrodingersCat,
            kataomoi,
            doomsayer,
            pelican,
            yandere,
            crewmate,
            mayor,
            portalmaker,
            engineer,
            sheriff,
            deputy,
            niceshifter, 
            bait,
            lighter,
            detective,
            timeMaster,
            medic,
            niceSwapper,
            niceWatcher,
            seer,
            hacker,
            tracker,
            snitch,
            spy,
            securityGuard,
            medium,
            fortuneTeller,
            sprinter,
            veteran,
            sherlock,
            yasuna,
            taskMaster,
            teleporter,
            busker,
            noisemaker,
            archaeologist,
            collator,
            jailor,
            bloody,
            antiTeleport,
            tiebreaker,
            sunglasses,
            mini,
            vip,
            invert,
            chameleon,
            multitasker,
            diseased,
            radar,
            armored
        };

        public static List<RoleInfo> getRoleInfoForPlayer(PlayerControl p, bool showModifier = true, bool includeHidden = false, RoleId[] excludeRoles = null) {
            List<RoleInfo> infos = new();
            if (p == null) return infos;

            // Modifier
            if (showModifier) {
                // after dead modifier
                if (!CustomOptionHolder.modifiersAreHidden.getBool() || PlayerControl.LocalPlayer.Data.IsDead || AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Ended)
                {
                    if (Bloody.bloody.Any(x => x.PlayerId == p.PlayerId)) infos.Add(bloody);
                    if (Vip.vip.Any(x => x.PlayerId == p.PlayerId)) infos.Add(vip);
                }
                if (p.isLovers()) infos.Add(lover);
                if (p == Tiebreaker.tiebreaker) infos.Add(tiebreaker);
                if (AntiTeleport.antiTeleport.Any(x => x.PlayerId == p.PlayerId)) infos.Add(antiTeleport);
                if (Sunglasses.sunglasses.Any(x => x.PlayerId == p.PlayerId)) infos.Add(sunglasses);
                if (p == Mini.mini) infos.Add(mini);
                if (Invert.invert.Any(x => x.PlayerId == p.PlayerId)) infos.Add(invert);
                if (Chameleon.chameleon.Any(x => x.PlayerId == p.PlayerId)) infos.Add(chameleon);
                if (p == Armored.armored) infos.Add(armored);
                if (Multitasker.multitasker.Any(x => x.PlayerId == p.PlayerId)) infos.Add(multitasker);
                if (Diseased.diseased.Any(x => x.PlayerId == p.PlayerId)) infos.Add(diseased);
                if (p == Radar.radar) infos.Add(radar);
                //if (p == Shifter.shifter) infos.Add(shifter);
            }

            int count = infos.Count;  // Save count after modifiers are added so that the role count can be checked

            // Special roles
            if (p.isRole(RoleId.Jester)) infos.Add(jester);
            if (p.isRole(RoleId.Mayor)) infos.Add(mayor);
            if (p.isRole(RoleId.Portalmaker)) infos.Add(portalmaker);
            if (p.isRole(RoleId.Engineer)) infos.Add(engineer);
            if (p.isRole(RoleId.Sheriff)) infos.Add(sheriff);
            if (p.isRole(RoleId.Deputy)) infos.Add(deputy);
            if (p.isRole(RoleId.Lighter)) infos.Add(lighter);
            if (p.isRole(RoleId.Godfather)) infos.Add(godfather);
            if (p.isRole(RoleId.Mafioso)) infos.Add(mafioso);
            if (p.isRole(RoleId.Janitor)) infos.Add(janitor);
            if (p.isRole(RoleId.Morphling)) infos.Add(morphling);
            if (p.isRole(RoleId.Camouflager)) infos.Add(camouflager);
            if (p.isRole(RoleId.Vampire)) infos.Add(vampire);
            if (p.isRole(RoleId.Eraser)) infos.Add(eraser);
            if (p.isRole(RoleId.Trickster)) infos.Add(trickster);
            if (p.isRole(RoleId.Cleaner)) infos.Add(cleaner);
            if (p.isRole(RoleId.Warlock)) infos.Add(warlock);
            if (p.isRole(RoleId.Witch)) infos.Add(witch);
            if (p.isRole(RoleId.Assassin)) infos.Add(assassin);
            if (p.isRole(RoleId.Detective)) infos.Add(detective);
            if (p.isRole(RoleId.TimeMaster)) infos.Add(timeMaster);
            if (p.isRole(RoleId.Medic)) infos.Add(medic);
            if (p.isRole(RoleId.Swapper)) infos.Add(p.Data.Role.IsImpostor ? evilSwapper : niceSwapper);
            if (p.isRole(RoleId.Seer)) infos.Add(seer);
            if (p.isRole(RoleId.Hacker)) infos.Add(hacker);
            if (p.isRole(RoleId.Tracker)) infos.Add(tracker);
            if (p.isRole(RoleId.Snitch)) infos.Add(snitch);
            if (p.isRole(RoleId.Jackal)) infos.Add(jackal);
            if (p.isRole(RoleId.Sidekick)) infos.Add(sidekick);
            if (p.isRole(RoleId.Spy)) infos.Add(spy);
            if (p.isRole(RoleId.SecurityGuard)) infos.Add(securityGuard);
            if (p.isRole(RoleId.Bait)) infos.Add(bait);
            if (p.isRole(RoleId.Veteran)) infos.Add(veteran);
            if (p.isRole(RoleId.Sherlock)) infos.Add(sherlock);
            if (p.isRole(RoleId.Sprinter)) infos.Add(sprinter);
            if (Yasuna.isYasuna(p.PlayerId)) infos.Add(p.Data.Role.IsImpostor ? evilYasuna : yasuna);
            if (p.isRole(RoleId.Moriarty)) infos.Add(moriarty);
            if (p.isRole(RoleId.Puppeteer)) infos.Add(puppeteer);
            if (p.isRole(RoleId.JekyllAndHyde)) infos.Add(jekyllAndHyde);
            if (p.isRole(RoleId.Akujo)) infos.Add(akujo);
            if (p.isRole(RoleId.Teleporter)) infos.Add(teleporter);
            if (p.isRole(RoleId.Cupid)) infos.Add(cupid);
            if (p.isRole(RoleId.Blackmailer)) infos.Add(blackmailer);
            if (p.isRole(RoleId.Fox)) infos.Add(fox);
            if (p.isRole(RoleId.Kataomoi)) infos.Add(kataomoi);
            if (p.isRole(RoleId.Immoralist)) infos.Add(immoralist);
            if (p.isRole(RoleId.Busker)) infos.Add(busker);
            if (p.isRole(RoleId.Noisemaker)) infos.Add(noisemaker);
            if (p.isRole(RoleId.Archaeologist)) infos.Add(archaeologist);
            if (p.isRole(RoleId.Yoyo)) infos.Add(yoyo);
            if (p.isRole(RoleId.Doomsayer)) infos.Add(doomsayer);
            if (p.isRole(RoleId.Zephyr)) infos.Add(zephyr);
            if (p.isRole(RoleId.Collator)) infos.Add(collator);
            if (p.isRole(RoleId.Jailor)) infos.Add(jailor);
            if (p.isRole(RoleId.Pelican)) infos.Add(pelican);
            if (p.isRole(RoleId.Yandere)) infos.Add(yandere);
            if (p.isRole(RoleId.FortuneTeller))
            {
                if (PlayerControl.LocalPlayer.Data.IsDead || includeHidden)
                {
                    infos.Add(fortuneTeller);
                }
                else
                {
                    var info = FortuneTeller.isCompletedNumTasks(p) ? fortuneTeller : crewmate;
                    infos.Add(info);
                }
            }
            if (p.isRole(RoleId.TaskMaster))
            {
                if (PlayerControl.LocalPlayer.Data.IsDead || includeHidden || !TaskMaster.becomeATaskMasterWhenCompleteAllTasks) infos.Add(taskMaster);
                else infos.Add(TaskMaster.isTaskComplete ? taskMaster : crewmate);
            }
            if (p.isRole(RoleId.PlagueDoctor)) infos.Add(plagueDoctor);
            if (p.isRole(RoleId.SchrodingersCat)) infos.Add(!SchrodingersCat.hideRole || includeHidden || PlayerControl.LocalPlayer.Data.IsDead
                || SchrodingersCat.hasTeam() || SchrodingersCat.tasksComplete(PlayerControl.LocalPlayer) ? schrodingersCat : crewmate);
            if (p.isRole(RoleId.Opportunist)) infos.Add(opportunist);
            if (p.isRole(RoleId.Shifter)) infos.Add(Shifter.isNeutral ? chainshifter : niceshifter);
            if (p.isRole(RoleId.Arsonist)) infos.Add(arsonist);
            if (p.isRole(RoleId.NiceGuesser)) infos.Add(goodGuesser);
            if (p.isRole(RoleId.EvilGuesser)) infos.Add(badGuesser);
            if (p.isRole(RoleId.NiceWatcher)) infos.Add(niceWatcher);
            if (p.isRole(RoleId.EvilWatcher)) infos.Add(evilWatcher);
            if (p.isRole(RoleId.BountyHunter)) infos.Add(bountyHunter);
            if (p.isRole(RoleId.Ninja)) infos.Add(ninja);
            if (p.isRole(RoleId.NekoKabocha)) infos.Add(nekoKabocha);
            if (p.isRole(RoleId.SerialKiller)) infos.Add(serialKiller);
            if (p.isRole(RoleId.EvilTracker)) infos.Add(evilTracker);
            if (p.isRole(RoleId.EvilHacker)) infos.Add(evilHacker);
            if (p.isRole(RoleId.Undertaker)) infos.Add(undertaker);
            if (p.isRole(RoleId.Trapper)) infos.Add(trapper);
            if (p.isRole(RoleId.MimicK)) infos.Add(mimicK);
            if (p.isRole(RoleId.MimicA)) infos.Add(mimicA);
            if (p.isRole(RoleId.BomberA)) infos.Add(bomberA);
            if (p.isRole(RoleId.BomberB)) infos.Add(bomberB);
            if (p.isRole(RoleId.Vulture)) infos.Add(vulture);
            if (p.isRole(RoleId.Medium)) infos.Add(medium);
            if (p.isRole(RoleId.Lawyer)) infos.Add(lawyer);
            if (p.isRole(RoleId.Pursuer)) infos.Add(pursuer);
            if (p.isRole(RoleId.Thief)) infos.Add(thief);

            // Default roles (just impostor, just crewmate, or hunter / hunted for hide n seek
            if (infos.Count == count) {
                if (p.Data.Role.IsImpostor)
                    infos.Add(TORMapOptions.gameMode == CustomGamemodes.HideNSeek ? hunter : impostor);
                else
                    infos.Add(TORMapOptions.gameMode == CustomGamemodes.HideNSeek ? hunted : crewmate);
            }

            if (excludeRoles != null)
                infos.RemoveAll(x => excludeRoles.Contains(x.roleId));

            return infos;
        }

        public static String GetRolesString(PlayerControl p, bool useColors, bool showModifier = true, bool suppressGhostInfo = false, bool includeHidden = false, RoleId[] excludeRoles = null) {
            string roleName;
            roleName = String.Join(" ", getRoleInfoForPlayer(p, showModifier, includeHidden, excludeRoles).Select(x => useColors ? Helpers.cs(x.color, x.name) : x.name).ToArray());

            if (Madmate.madmate.Any(x => x.PlayerId == p.PlayerId) || CreatedMadmate.createdMadmate.Any(x => x.PlayerId == p.PlayerId))
            {
                if (getRoleInfoForPlayer(p, true, includeHidden, excludeRoles).Contains(crewmate))
                {
                    roleName = useColors ? Helpers.cs(Madmate.color, Madmate.fullName) : Madmate.fullName;
                    if (showModifier && getRoleInfoForPlayer(p, true, includeHidden, excludeRoles).Where(x => x.isModifier).FirstOrDefault() != null) roleName = string.Join(" ", getRoleInfoForPlayer(p, true, includeHidden, excludeRoles).Where(x => x.isModifier).Select(x => useColors ? Helpers.cs(x.color, x.name) : x.name).ToArray()) + " " + roleName;
                }
                else
                {
                    string prefix = useColors ? Helpers.cs(Madmate.color, Madmate.prefix) : Madmate.prefix;
                    roleName = string.Join(" ", getRoleInfoForPlayer(p, false, includeHidden, excludeRoles).Select(x => useColors ? Helpers.cs(Madmate.color, x.name) : x.name).ToArray());
                    roleName = prefix + roleName;
                    if (showModifier && getRoleInfoForPlayer(p, true, includeHidden, excludeRoles).Where(x => x.isModifier).FirstOrDefault() != null) roleName = string.Join(" ", getRoleInfoForPlayer(p, true, includeHidden, excludeRoles).Where(x => x.isModifier).Select(x => useColors ? Helpers.cs(x.color, x.name) : x.name).ToArray()) + " " + roleName;
                }
            }

            if (Lawyer.target != null && p.PlayerId == Lawyer.target.PlayerId && PlayerControl.LocalPlayer != Lawyer.target) 
                roleName += useColors ? Helpers.cs(Pursuer.color, " §") : " §";
            if (HandleGuesser.isGuesserGm && HandleGuesser.isGuesser(p.PlayerId))
            {
                int remainingShots = HandleGuesser.remainingShots(p.PlayerId);
                var (playerCompleted, playerTotal) = TasksHandler.taskInfo(p.Data);
                var color = Color.white;
                string guesserModifier = p == LastImpostor.lastImpostor ? ModTranslation.getString("lastImpostorGuesserModifier") : ModTranslation.getString("guesserModifier");
                if ((!Helpers.isEvil(p) && playerCompleted < HandleGuesser.tasksToUnlock) || remainingShots == 0 || (p == LastImpostor.lastImpostor
                    && !LastImpostor.isOriginalGuesser && !LastImpostor.isCounterMax())) color = Color.gray;
                roleName += useColors ? Helpers.cs(color, guesserModifier) : guesserModifier;
            }
            if (!suppressGhostInfo && p != null) {
                if (p.isRole(RoleId.Shifter) && (PlayerControl.LocalPlayer.isRole(RoleId.Shifter) || Helpers.shouldShowGhostInfo()) && Shifter.futureShift != null)
                    roleName += Helpers.cs(Color.yellow, " ← " + Shifter.futureShift.Data.PlayerName);
                foreach (var vulture in Vulture.players) {
                    if (p == vulture.player && (PlayerControl.LocalPlayer == p || Helpers.shouldShowGhostInfo()))
                        roleName += Helpers.cs(Vulture.color, $" ({Vulture.vultureNumberToWin - vulture.eatenBodies} {ModTranslation.getString("roleInfoRemaining")})");
                }
                if (Helpers.shouldShowGhostInfo()) {
                    if (p.isLovers()) roleName = Helpers.cs(Lovers.getColor(p), ModTranslation.getString("lover")) + " " + roleName;
                    if (Eraser.futureErased.Contains(p))
                        roleName = Helpers.cs(Color.gray, ModTranslation.getString("roleInfoErased")) + roleName;
                    if (Deputy.handcuffedPlayers.Contains(p.PlayerId))
                        roleName = Helpers.cs(Color.gray, ModTranslation.getString("roleInfoCuffed")) + roleName;
                    if (Deputy.handcuffedKnows.ContainsKey(p.PlayerId))  // Active cuff
                        roleName = Helpers.cs(Deputy.color, ModTranslation.getString("roleInfoCuffed")) + roleName;
                    if (Warlock.players.Any(x => x.curseVictim == p))
                        roleName = Helpers.cs(Warlock.color, ModTranslation.getString("roleInfoCursed")) + roleName;
                    if (Assassin.players.Any(x => x.player && x.assassinMarked == p))
                        roleName = Helpers.cs(Assassin.color, ModTranslation.getString("roleInfoMarked")) + roleName;
                    if (Pursuer.blankedList.Contains(p) && !p.Data.IsDead)
                        roleName = Helpers.cs(Pursuer.color, ModTranslation.getString("roleInfoBlanked")) + roleName;

                    if (!MeetingHud.Instance)
                    {
                        if (FortuneTeller.players.Any(x => x.divineTarget == p))
                            roleName = Helpers.cs(FortuneTeller.color, ModTranslation.getString("roleInfoDivined")) + roleName;
                        if (Witch.players.Any(x => x.futureSpelled.Contains(p)))
                            roleName = Helpers.cs(Witch.color, "☆ ") + roleName;
                        foreach (var arsonist in Arsonist.players) {
                            if (arsonist.player == p)
                                roleName += Helpers.cs(Arsonist.color, $" ({PlayerControl.AllPlayerControls.ToArray().Count(x => { return x != p && !x.Data.IsDead && !x.Data.Disconnected && !arsonist.dousedPlayers.Any(y => y.PlayerId == x.PlayerId); })} {ModTranslation.getString("roleInfoRemaining")})");
                        }
                        if (Akujo.isKeep(p))
                            roleName = Helpers.cs(Color.gray, ModTranslation.getString("roleInfoBackup")) + roleName;
                        if (Akujo.isHonmei(p))
                            roleName = Helpers.cs(Akujo.players.FirstOrDefault(x => x.honmei == p).iconColor, ModTranslation.getString("roleInfoHonmei")) + roleName;
                        if (p == Kataomoi.target)
                            roleName = Helpers.cs(Kataomoi.color, ModTranslation.getString("roleInfoKataomoiTarget")) + roleName;
                        if (p == Yandere.target)
                            roleName = Helpers.cs(Yandere.color, ModTranslation.getString("roleInfoYandereTarget")) + roleName;
                        if (Pelican.players.Any(x => x.eatenPlayers.Contains(p)))
                            roleName = Helpers.cs(Pelican.color, ModTranslation.getString("roleInfoSwallowIndicator")) + roleName;

                        // Death Reason on Ghosts
                        if (p.Data.IsDead) {
                            roleName += getStatesString(p);
                        }
                    }
                }
            }
            return roleName;
        }

        public static string getStatesString(PlayerControl p, bool useColors = true)
        {
            string deathReasonString = "";
            var deadPlayer = GameHistory.deadPlayers.FirstOrDefault(x => x.player.PlayerId == p.PlayerId);

            Color killerColor = new();
            if (deadPlayer != null && deadPlayer.killerIfExisting != null)
            {
                killerColor = getRoleInfoForPlayer(deadPlayer.killerIfExisting, false, true).FirstOrDefault().color;
                if (Madmate.madmate.Any(x => x.PlayerId == deadPlayer.killerIfExisting.PlayerId) || CreatedMadmate.createdMadmate.Any(x => x.PlayerId == deadPlayer.killerIfExisting.PlayerId)) killerColor = Palette.ImpostorRed;
            }

            if (deadPlayer != null)
            {
                switch (deadPlayer.deathReason)
                {
                    case DeadPlayer.CustomDeathReason.Disconnect:
                        deathReasonString = ModTranslation.getString("roleSummaryDisconnected");
                        break;
                    case DeadPlayer.CustomDeathReason.Exile:
                        deathReasonString = ModTranslation.getString("roleSummaryExiled");
                        break;
                    case DeadPlayer.CustomDeathReason.Kill:
                        deathReasonString = string.Format(ModTranslation.getString("roleSummaryKilled"), Helpers.cs(killerColor, deadPlayer.killerIfExisting.Data.PlayerName));
                        break;
                    case DeadPlayer.CustomDeathReason.Guess:
                        if (deadPlayer.killerIfExisting.Data.PlayerName == p.Data.PlayerName)
                            deathReasonString = ModTranslation.getString("roleSummaryFailedGuess");
                        else
                            deathReasonString = string.Format(ModTranslation.getString("roleSummaryGuess"), Helpers.cs(killerColor, deadPlayer.killerIfExisting.Data.PlayerName));
                        break;
                    case DeadPlayer.CustomDeathReason.Shift:
                        deathReasonString = $" - {Helpers.cs(Color.yellow, ModTranslation.getString("roleSummaryShift"))} {Helpers.cs(killerColor, deadPlayer.killerIfExisting.Data.PlayerName)}";
                        break;
                    case DeadPlayer.CustomDeathReason.WitchExile:
                        deathReasonString = string.Format(ModTranslation.getString("roleSummarySpelled"), Helpers.cs(killerColor, deadPlayer.killerIfExisting.Data.PlayerName));
                        break;
                    case DeadPlayer.CustomDeathReason.LoverSuicide:
                        deathReasonString = $" - {Helpers.cs(Lovers.color, ModTranslation.getString("roleSummaryLoverDied"))}";
                        break;
                    case DeadPlayer.CustomDeathReason.Revenge:
                        deathReasonString = string.Format(ModTranslation.getString("roleSummaryRevenge"), Helpers.cs(killerColor, deadPlayer.killerIfExisting.Data.PlayerName));
                        break;
                    case DeadPlayer.CustomDeathReason.Suicide:
                        deathReasonString = ModTranslation.getString("roleSummarySuicide");
                        break;
                    case DeadPlayer.CustomDeathReason.KataomoiStare:
                        deathReasonString = $" - {Helpers.cs(Kataomoi.color, ModTranslation.getString("roleSummaryKataomoiStare"))}";
                        break;
                    case DeadPlayer.CustomDeathReason.BrainwashedKilled:
                        deathReasonString = string.Format(ModTranslation.getString("roleSummaryBrainwashedKilled"), Helpers.cs(killerColor, deadPlayer.killerIfExisting.Data.PlayerName));
                        break;
                    case DeadPlayer.CustomDeathReason.Blown:
                        deathReasonString = string.Format(ModTranslation.getString("roleSummaryBlown"), Helpers.cs(killerColor, deadPlayer.killerIfExisting.Data.PlayerName));
                        break;
                    case DeadPlayer.CustomDeathReason.LoveStolen:
                        deathReasonString = $" - {Helpers.cs(Lovers.color, ModTranslation.getString("roleSummaryLoveStolen"))}";
                        break;
                    case DeadPlayer.CustomDeathReason.Loneliness:
                        deathReasonString = $" - {Helpers.cs(Akujo.color, ModTranslation.getString("roleSummaryLoneliness"))}";
                        break;
                    case DeadPlayer.CustomDeathReason.Disease:
                        deathReasonString = string.Format(ModTranslation.getString("roleSummaryDisease"), Helpers.cs(killerColor, deadPlayer.killerIfExisting.Data.PlayerName));
                        break;
                    case DeadPlayer.CustomDeathReason.Jailed:
                        deathReasonString = string.Format(ModTranslation.getString("roleSummaryJailed"), Helpers.cs(killerColor, deadPlayer.killerIfExisting.Data.PlayerName));
                        break;
                    case DeadPlayer.CustomDeathReason.Scapegoat:
                        deathReasonString = $" - {Helpers.cs(Cupid.color, ModTranslation.getString("roleSummaryScapegoat"))}";
                        break;
                    case DeadPlayer.CustomDeathReason.Swallowed:
                        deathReasonString = string.Format(ModTranslation.getString("roleSummarySwallowed"), Helpers.cs(killerColor, deadPlayer.killerIfExisting.Data.PlayerName));
                        break;
                    //case DeadPlayer.CustomDeathReason.LawyerSuicide:
                    //deathReasonString = $" - {Helpers.cs(Lawyer.color, "bad Lawyer")}";
                    //break;
                    case DeadPlayer.CustomDeathReason.Bomb:
                        deathReasonString = string.Format(ModTranslation.getString("roleSummaryBombed"), Helpers.cs(killerColor, deadPlayer.killerIfExisting.Data.PlayerName));
                        break;
                    case DeadPlayer.CustomDeathReason.Divined:
                        deathReasonString = string.Format(ModTranslation.getString("roleSummaryDivined"), Helpers.cs(killerColor, deadPlayer.killerIfExisting.Data.PlayerName));
                        break;
                    case DeadPlayer.CustomDeathReason.Pseudocide:
                        deathReasonString = $" - {Helpers.cs(Busker.color, ModTranslation.getString("roleSummaryPseudocide"))}";
                        break;
                    case DeadPlayer.CustomDeathReason.Arson:
                        deathReasonString = string.Format(ModTranslation.getString("roleSummaryTorched"), Helpers.cs(killerColor, deadPlayer.killerIfExisting.Data.PlayerName));
                        break;
                }
            }
            if (!useColors)
                deathReasonString = System.Text.RegularExpressions.Regex.Replace(deathReasonString, @"<color=#([0-9a-fA-F]+)>|</color>", string.Empty);

            return deathReasonString;
        }
    }
}
