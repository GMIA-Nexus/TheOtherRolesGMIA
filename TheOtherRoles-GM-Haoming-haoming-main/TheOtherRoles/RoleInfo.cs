using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;
using static TheOtherRoles.TheOtherRolesGM;

namespace TheOtherRoles
{
    public class RoleInfo
    {
        public Color color;
        public virtual string name { get { return ModTranslation.getString(nameKey); } }
        public virtual string nameColored { get { return Helpers.cs(color, name); } }
        public virtual string introDescription { get { return ModTranslation.getString(nameKey + "IntroDesc"); } }
        public virtual string shortDescription { get { return ModTranslation.getString(nameKey + "ShortDesc"); } }
        public virtual string fullDescription { get { return ModTranslation.getString(nameKey + "FullDesc"); } }
        public virtual string blurb { get { return ModTranslation.getString(nameKey + "Blurb"); } }

        public bool enabled
        {
            get
            {
                return Helpers.RolesEnabled && (baseOption == null || baseOption.enabled);
            }
        }
        public RoleType roleType;

        private string nameKey;
        private CustomOption baseOption;

        RoleInfo(string name, Color color, CustomOption baseOption, RoleType roleType)
        {
            this.color = color;
            this.nameKey = name;
            this.baseOption = baseOption;
            this.roleType = roleType;
        }

        public static RoleInfo jester;
        public static RoleInfo mayor;
        public static RoleInfo engineer;
        public static RoleInfo sheriff;
        public static RoleInfo lighter;
        public static RoleInfo godfather;
        public static RoleInfo mafioso;
        public static RoleInfo janitor;
        public static RoleInfo morphling;
        public static RoleInfo camouflager;
        public static RoleInfo vampire;
        public static RoleInfo eraser;
        public static RoleInfo trickster;
        public static RoleInfo cleaner;
        public static RoleInfo warlock;
        public static RoleInfo bountyHunter;
        public static RoleInfo detective;
        public static RoleInfo timeMaster;
        public static RoleInfo medic;
        public static RoleInfo niceShifter;
        public static RoleInfo corruptedShifter;
        public static RoleInfo niceSwapper;
        public static RoleInfo evilSwapper;
        public static RoleInfo seer;
        public static RoleInfo hacker;
        public static RoleInfo tracker;
        public static RoleInfo snitch;
        public static RoleInfo jackal;
        public static RoleInfo sidekick;
        public static RoleInfo spy;
        public static RoleInfo securityGuard;
        public static RoleInfo arsonist;
        public static RoleInfo niceGuesser;
        public static RoleInfo evilGuesser;
        public static RoleInfo bait;
        public static RoleInfo impostor;
        public static RoleInfo lawyer;
        public static RoleInfo pursuer;
        public static RoleInfo crewmate;
        public static RoleInfo lovers;
        public static RoleInfo gm;
        public static RoleInfo opportunist;
        public static RoleInfo witch;
        public static RoleInfo assassin;
        public static RoleInfo vulture;
        public static RoleInfo medium;
        public static RoleInfo ninja;
        public static RoleInfo plagueDoctor;
        public static RoleInfo nekoKabocha;
        public static RoleInfo niceWatcher;
        public static RoleInfo evilWatcher;
        public static RoleInfo serialKiller;
        public static RoleInfo fox;
        public static RoleInfo immoralist;
        public static RoleInfo fortuneTeller;
        public static RoleInfo akujo;
        public static RoleInfo schrodingersCat;
        public static RoleInfo trapper;
        public static RoleInfo bomberA;
        public static RoleInfo bomberB;
        public static RoleInfo evilTracker;
        public static RoleInfo puppeteer;
        public static RoleInfo evilHacker;
        public static RoleInfo mimicK;
        public static RoleInfo mimicA;
        public static RoleInfo jekyllAndHyde;
        public static RoleInfo moriarty;
        public static RoleInfo sherlock;
        public static RoleInfo cupid;
        public static RoleInfo yoyo;
#if DEV
        public static RoleInfo nmk;
        public static RoleInfo plt;
#endif
        public static List<RoleInfo> allRoleInfos;
        public static void Load()
        {
            jester = new RoleInfo("jester", Jester.color, CustomOptionHolder.jesterSpawnRate, RoleType.Jester);
            mayor = new RoleInfo("mayor", Mayor.color, CustomOptionHolder.mayorSpawnRate, RoleType.Mayor);
            engineer = new RoleInfo("engineer", Engineer.color, CustomOptionHolder.engineerSpawnRate, RoleType.Engineer);
            sheriff = new RoleInfo("sheriff", Sheriff.color, CustomOptionHolder.sheriffSpawnRate, RoleType.Sheriff);
            lighter = new RoleInfo("lighter", Lighter.color, CustomOptionHolder.lighterSpawnRate, RoleType.Lighter);
            godfather = new RoleInfo("godfather", Godfather.color, CustomOptionHolder.mafiaSpawnRate, RoleType.Godfather);
            mafioso = new RoleInfo("mafioso", Mafioso.color, CustomOptionHolder.mafiaSpawnRate, RoleType.Mafioso);
            janitor = new RoleInfo("janitor", Janitor.color, CustomOptionHolder.mafiaSpawnRate, RoleType.Janitor);
            morphling = new RoleInfo("morphling", Morphling.color, CustomOptionHolder.morphlingSpawnRate, RoleType.Morphling);
            camouflager = new RoleInfo("camouflager", Camouflager.color, CustomOptionHolder.camouflagerSpawnRate, RoleType.Camouflager);
            vampire = new RoleInfo("vampire", Vampire.color, CustomOptionHolder.vampireSpawnRate, RoleType.Vampire);
            eraser = new RoleInfo("eraser", Eraser.color, CustomOptionHolder.eraserSpawnRate, RoleType.Eraser);
            trickster = new RoleInfo("trickster", Trickster.color, CustomOptionHolder.tricksterSpawnRate, RoleType.Trickster);
            cleaner = new RoleInfo("cleaner", Cleaner.color, CustomOptionHolder.cleanerSpawnRate, RoleType.Cleaner);
            warlock = new RoleInfo("warlock", Warlock.color, CustomOptionHolder.warlockSpawnRate, RoleType.Warlock);
            bountyHunter = new RoleInfo("bountyHunter", BountyHunter.color, CustomOptionHolder.bountyHunterSpawnRate, RoleType.BountyHunter);
            detective = new RoleInfo("detective", Detective.color, CustomOptionHolder.detectiveSpawnRate, RoleType.Detective);
            timeMaster = new RoleInfo("timeMaster", TimeMaster.color, CustomOptionHolder.timeMasterSpawnRate, RoleType.TimeMaster);
            medic = new RoleInfo("medic", Medic.color, CustomOptionHolder.medicSpawnRate, RoleType.Medic);
            niceShifter = new RoleInfo("niceShifter", Shifter.color, CustomOptionHolder.shifterSpawnRate, RoleType.Shifter);
            corruptedShifter = new RoleInfo("corruptedShifter", Shifter.color, CustomOptionHolder.shifterSpawnRate, RoleType.Shifter);
            niceSwapper = new RoleInfo("niceSwapper", Swapper.color, CustomOptionHolder.swapperSpawnRate, RoleType.Swapper);
            evilSwapper = new RoleInfo("evilSwapper", Palette.ImpostorRed, CustomOptionHolder.swapperSpawnRate, RoleType.Swapper);
            seer = new RoleInfo("seer", Seer.color, CustomOptionHolder.seerSpawnRate, RoleType.Seer);
            hacker = new RoleInfo("hacker", Hacker.color, CustomOptionHolder.hackerSpawnRate, RoleType.Hacker);
            tracker = new RoleInfo("tracker", Tracker.color, CustomOptionHolder.trackerSpawnRate, RoleType.Tracker);
            snitch = new RoleInfo("snitch", Snitch.color, CustomOptionHolder.snitchSpawnRate, RoleType.Snitch);
            jackal = new RoleInfo("jackal", Jackal.color, CustomOptionHolder.jackalSpawnRate, RoleType.Jackal);
            sidekick = new RoleInfo("sidekick", Sidekick.color, CustomOptionHolder.jackalSpawnRate, RoleType.Sidekick);
            spy = new RoleInfo("spy", Spy.color, CustomOptionHolder.spySpawnRate, RoleType.Spy);
            securityGuard = new RoleInfo("securityGuard", SecurityGuard.color, CustomOptionHolder.securityGuardSpawnRate, RoleType.SecurityGuard);
            arsonist = new RoleInfo("arsonist", Arsonist.color, CustomOptionHolder.arsonistSpawnRate, RoleType.Arsonist);
            niceGuesser = new RoleInfo("niceGuesser", Guesser.color, CustomOptionHolder.guesserSpawnRate, RoleType.NiceGuesser);
            evilGuesser = new RoleInfo("evilGuesser", Palette.ImpostorRed, CustomOptionHolder.guesserSpawnRate, RoleType.EvilGuesser);
            bait = new RoleInfo("bait", Bait.color, CustomOptionHolder.baitSpawnRate, RoleType.Bait);
            impostor = new RoleInfo("impostor", Palette.ImpostorRed, null, RoleType.Impostor);
            lawyer = new RoleInfo("lawyer", Lawyer.color, CustomOptionHolder.lawyerSpawnRate, RoleType.Lawyer);
            pursuer = new RoleInfo("pursuer", Pursuer.color, CustomOptionHolder.lawyerSpawnRate, RoleType.Pursuer);
            crewmate = new RoleInfo("crewmate", Color.white, null, RoleType.Crewmate);
            lovers = new RoleInfo("lovers", Lovers.color, CustomOptionHolder.loversSpawnRate, RoleType.Lovers);
            gm = new RoleInfo("gm", GM.color, CustomOptionHolder.gmEnabled, RoleType.GM);
            opportunist = new RoleInfo("opportunist", Opportunist.color, CustomOptionHolder.opportunistSpawnRate, RoleType.Opportunist);
            witch = new RoleInfo("witch", Witch.color, CustomOptionHolder.witchSpawnRate, RoleType.Witch);
            assassin = new RoleInfo("assassin", Assassin.color, CustomOptionHolder.ninjaSpawnRate, RoleType.Assassin);
            vulture = new RoleInfo("vulture", Vulture.color, CustomOptionHolder.vultureSpawnRate, RoleType.Vulture);
            medium = new RoleInfo("medium", Medium.color, CustomOptionHolder.mediumSpawnRate, RoleType.Medium);
            ninja = new RoleInfo("ninja", Ninja.color, CustomOptionHolder.ninjaSpawnRate, RoleType.Ninja);
            plagueDoctor = new RoleInfo("plagueDoctor", PlagueDoctor.color, CustomOptionHolder.plagueDoctorSpawnRate, RoleType.PlagueDoctor);
            nekoKabocha = new RoleInfo("nekoKabocha", NekoKabocha.color, CustomOptionHolder.nekoKabochaSpawnRate, RoleType.NekoKabocha);
            niceWatcher = new RoleInfo("niceWatcher", Watcher.color, CustomOptionHolder.watcherSpawnRate, RoleType.Watcher);
            evilWatcher = new RoleInfo("evilWatcher", Palette.ImpostorRed, CustomOptionHolder.watcherSpawnRate, RoleType.Watcher);
            serialKiller = new RoleInfo("serialKiller", SerialKiller.color, CustomOptionHolder.serialKillerSpawnRate, RoleType.SerialKiller);
            fox = new RoleInfo("fox", Fox.color, CustomOptionHolder.foxSpawnRate, RoleType.Fox);
            immoralist = new RoleInfo("immoralist", Immoralist.color, CustomOptionHolder.foxSpawnRate, RoleType.Immoralist);
            fortuneTeller = new RoleInfo("fortuneTeller", FortuneTeller.color, CustomOptionHolder.fortuneTellerSpawnRate, RoleType.FortuneTeller);
            akujo = new RoleInfo("akujo", Akujo.color, CustomOptionHolder.akujoSpawnRate, RoleType.Akujo);
            schrodingersCat = new RoleInfo("schrodingersCat", SchrodingersCat.color, CustomOptionHolder.schrodingersCatSpawnRate, RoleType.SchrodingersCat);
            trapper = new RoleInfo("trapper", Trapper.color, CustomOptionHolder.trapperSpawnRate, RoleType.Trapper);
            bomberA = new RoleInfo("bomber", BomberA.color, CustomOptionHolder.bomberSpawnRate, RoleType.BomberA);
            bomberB = new RoleInfo("bomber", BomberB.color, CustomOptionHolder.bomberSpawnRate, RoleType.BomberB);
            evilTracker = new RoleInfo("evilTracker", EvilTracker.color, CustomOptionHolder.evilTrackerSpawnRate, RoleType.EvilTracker);
            puppeteer = new RoleInfo("puppeteer", Puppeteer.color, CustomOptionHolder.puppeteerSpawnRate, RoleType.Puppeteer);
            evilHacker = new RoleInfo("evilHacker", EvilHacker.color, CustomOptionHolder.evilHackerSpawnRate, RoleType.EvilHacker);
            mimicK = new RoleInfo("mimicK", MimicK.color, CustomOptionHolder.mimicSpawnRate, RoleType.MimicK);
            mimicA = new RoleInfo("mimicA", MimicA.color, CustomOptionHolder.mimicSpawnRate, RoleType.MimicA);
            jekyllAndHyde = new RoleInfo("jekyllAndHyde", JekyllAndHyde.color, CustomOptionHolder.jekyllAndHydeSpawnRate, RoleType.JekyllAndHyde);
            moriarty = new RoleInfo("moriarty", Moriarty.color, CustomOptionHolder.moriartySpawnRate, RoleType.Moriarty);
            sherlock = new RoleInfo("sherlock", Sherlock.color, CustomOptionHolder.sherlockSpawnRate, RoleType.Sherlock);
            cupid = new RoleInfo("cupid", Cupid.color, CustomOptionHolder.cupidSpawnRate, RoleType.Cupid);
            yoyo = new RoleInfo("yoyo", Yoyo.color, CustomOptionHolder.yoyoSpawnRate, RoleType.Yoyo);
#if DEV
            nmk = new RoleInfo("nmk", NMK.color, CustomOptionHolder.nmkSpawnRate, RoleType.NMK);
            plt = new RoleInfo("PLT", PLT.color, CustomOptionHolder.pltSpawnRate, RoleType.PLT);
#endif
            allRoleInfos = new List<RoleInfo>()
            {
                impostor,
                godfather,
                mafioso,
                janitor,
                morphling,
                camouflager,
                evilHacker,
                vampire,
                eraser,
                trickster,
                cleaner,
                warlock,
                bountyHunter,
                witch,
                assassin,
                ninja,
                serialKiller,
                niceGuesser,
                evilGuesser,
                lovers,
                jester,
                arsonist,
                jackal,
                sidekick,
                vulture,
                pursuer,
                lawyer,
                crewmate,
                niceShifter,
                corruptedShifter,
                mayor,
                engineer,
                sheriff,
                lighter,
                detective,
                timeMaster,
                medic,
                niceSwapper,
                evilSwapper,
                seer,
                hacker,
                tracker,
                snitch,
                spy,
                securityGuard,
                bait,
                gm,
                opportunist,
                medium,
                plagueDoctor,
                nekoKabocha,
                niceWatcher,
                evilWatcher,
                fox,
                immoralist,
                fortuneTeller,
                akujo,
                schrodingersCat,
                trapper,
                bomberA,
                bomberB,
                evilTracker,
                puppeteer,
                mimicK,
                mimicA,
                jekyllAndHyde,
                moriarty,
                sherlock,
                cupid,
                yoyo,
#if DEV
                nmk,
                plt,
#endif
            };
        }


        public static string tl(string key)
        {
            return ModTranslation.getString(key);
        }

        public static List<RoleInfo> getRoleInfoForPlayer(PlayerControl p, RoleType[] excludeRoles = null, bool includeHidden = false)
        {
            List<RoleInfo> infos = new();
            if (p == null) return infos;

            // Special roles
            if (p.isRole(RoleType.Jester)) infos.Add(jester);
            if (p.isRole(RoleType.Mayor)) infos.Add(mayor);
            if (p.isRole(RoleType.Engineer)) infos.Add(engineer);
            if (p.isRole(RoleType.Sheriff)) infos.Add(sheriff);
            if (p.isRole(RoleType.Lighter)) infos.Add(lighter);
            if (p.isRole(RoleType.Godfather)) infos.Add(godfather);
            if (p.isRole(RoleType.Mafioso)) infos.Add(mafioso);
            if (p.isRole(RoleType.Janitor)) infos.Add(janitor);
            if (p.isRole(RoleType.Morphling)) infos.Add(morphling);
            if (p.isRole(RoleType.Camouflager)) infos.Add(camouflager);
            if (p.isRole(RoleType.EvilHacker)) infos.Add(evilHacker);
            if (p.isRole(RoleType.Vampire)) infos.Add(vampire);
            if (p.isRole(RoleType.Eraser)) infos.Add(eraser);
            if (p.isRole(RoleType.Trickster)) infos.Add(trickster);
            if (p.isRole(RoleType.Cleaner)) infos.Add(cleaner);
            if (p.isRole(RoleType.Warlock)) infos.Add(warlock);
            if (p.isRole(RoleType.Witch)) infos.Add(witch);
            if (p.isRole(RoleType.Assassin)) infos.Add(assassin);
            if (p.isRole(RoleType.Detective)) infos.Add(detective);
            if (p.isRole(RoleType.TimeMaster)) infos.Add(timeMaster);
            if (p.isRole(RoleType.Medic)) infos.Add(medic);
            if (p.isRole(RoleType.Shifter)) infos.Add(Shifter.isNeutral ? corruptedShifter : niceShifter);
            if (p.isRole(RoleType.Swapper)) infos.Add(p.Data.Role.IsImpostor ? evilSwapper : niceSwapper);
            if (p.isRole(RoleType.Seer)) infos.Add(seer);
            if (p.isRole(RoleType.Hacker)) infos.Add(hacker);
            if (p.isRole(RoleType.Tracker)) infos.Add(tracker);
            if (p.isRole(RoleType.Snitch)) infos.Add(snitch);
            if (p.isRole(RoleType.Jackal) || (Jackal.formerJackals != null && Jackal.formerJackals.Any(x => x.PlayerId == p.PlayerId))) infos.Add(jackal);
            if (p.isRole(RoleType.Sidekick)) infos.Add(sidekick);
            if (p.isRole(RoleType.Spy)) infos.Add(spy);
            if (p.isRole(RoleType.SecurityGuard)) infos.Add(securityGuard);
            if (p.isRole(RoleType.Arsonist)) infos.Add(arsonist);
            if (p.isRole(RoleType.NiceGuesser)) infos.Add(niceGuesser);
            if (p.isRole(RoleType.EvilGuesser)) infos.Add(evilGuesser);
            if (p.isRole(RoleType.BountyHunter)) infos.Add(bountyHunter);
            if (p.isRole(RoleType.Bait)) infos.Add(bait);
            if (p.isRole(RoleType.GM)) infos.Add(gm);
            if (p.isRole(RoleType.Opportunist)) infos.Add(opportunist);
            if (p.isRole(RoleType.Vulture)) infos.Add(vulture);
            if (p.isRole(RoleType.Medium)) infos.Add(medium);
            if (p.isRole(RoleType.Lawyer)) infos.Add(lawyer);
            if (p.isRole(RoleType.Pursuer)) infos.Add(pursuer);
            if (p.isRole(RoleType.Ninja)) infos.Add(ninja);
            if (p.isRole(RoleType.PlagueDoctor)) infos.Add(plagueDoctor);
            if (p.isRole(RoleType.SerialKiller)) infos.Add(serialKiller);
            if (p.isRole(RoleType.NekoKabocha)) infos.Add(nekoKabocha);
            if (p.isRole(RoleType.Watcher))
            {
                if (p.isImpostor()) infos.Add(evilWatcher);
                else infos.Add(niceWatcher);
            }
            if (p.isRole(RoleType.Fox)) infos.Add(fox);
            if (p.isRole(RoleType.Immoralist)) infos.Add(immoralist);
            if (p.isRole(RoleType.FortuneTeller))
            {
                if ((includeHidden || PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.Data.IsDead)
                {
                    infos.Add(fortuneTeller);
                }
                else
                {
                    var info = FortuneTeller.isCompletedNumTasks(p) ? fortuneTeller : crewmate;
                    infos.Add(info);
                }
            }
            if (p.isRole(RoleType.Akujo)) infos.Add(akujo);

            // はおみんオリジナル
            if (p.isRole(RoleType.SchrodingersCat))
            {
                if (SchrodingersCat.hideRole && !SchrodingersCat.hasTeam() && !includeHidden && PlayerControl.LocalPlayer.isAlive())
                {
                    infos.Add(crewmate);
                }
                else
                {
                    infos.Add(schrodingersCat);
                }
            }
            if (p.isRole(RoleType.Trapper)) infos.Add(trapper);
            if (p.isRole(RoleType.BomberA)) infos.Add(bomberA);
            if (p.isRole(RoleType.BomberB)) infos.Add(bomberB);
            if (p.isRole(RoleType.EvilTracker)) infos.Add(evilTracker);
            if (p.isRole(RoleType.Puppeteer)) infos.Add(puppeteer);
            if (p.isRole(RoleType.MimicK)) infos.Add(mimicK);
            if (p.isRole(RoleType.MimicA)) infos.Add(mimicA);
            if (p.isRole(RoleType.JekyllAndHyde)) infos.Add(jekyllAndHyde);
            if (p.isRole(RoleType.Moriarty)) infos.Add(moriarty);
            if (p.isRole(RoleType.Sherlock)) infos.Add(sherlock);
            if (p.isRole(RoleType.Cupid)) infos.Add(cupid);
            if (p.isRole(RoleType.Yoyo)) infos.Add(yoyo);
#if DEV
            if (p.isRole(RoleType.NMK)) infos.Add(nmk);
            if (p.isRole(RoleType.PLT)) infos.Add(plt);
#endif



            // Default roles
            if (infos.Count == 0 && p.Data.Role != null && p.Data.Role.IsImpostor) infos.Add(impostor); // Just Impostor
            if (infos.Count == 0 && p.Data.Role != null && !p.Data.Role.IsImpostor) infos.Add(crewmate); // Just Crewmate

            // Modifier
            if (p.isLovers()) infos.Add(lovers);

            if (excludeRoles != null)
                infos.RemoveAll(x => excludeRoles.Contains(x.roleType));

            return infos;
        }

        public static String GetRolesString(PlayerControl p, bool useColors, RoleType[] excludeRoles = null, bool includeHidden = false, string joinSeparator = " ")
        {
            if (p?.Data?.Disconnected != false) return "";

            var roleInfo = getRoleInfoForPlayer(p, excludeRoles, includeHidden);
            string roleName = String.Join(joinSeparator, roleInfo.Select(x => useColors ? Helpers.cs(x.color, x.name) : x.name).ToArray());
            if (Lawyer.target != null && p?.PlayerId == Lawyer.target.PlayerId && PlayerControl.LocalPlayer != Lawyer.target) roleName += useColors ? Helpers.cs(Pursuer.color, " §") : " §";

            if (p.hasModifier(ModifierType.Madmate) || p.hasModifier(ModifierType.CreatedMadmate))
            {
                // Madmate only
                if (roleInfo.Contains(crewmate))
                {
                    roleName = useColors ? Helpers.cs(Madmate.color, Madmate.fullName) : Madmate.fullName;
                }
                else
                {
                    string prefix = useColors ? Helpers.cs(Madmate.color, Madmate.prefix) : Madmate.prefix;
                    roleName = String.Join(joinSeparator, roleInfo.Select(x => useColors ? Helpers.cs(Madmate.color, x.name) : x.name).ToArray());
                    roleName = prefix + roleName;
                }
            }

            if (p.hasModifier(ModifierType.LastImpostor))
            {
                if (roleInfo.Contains(impostor))
                {
                    roleName = useColors ? Helpers.cs(LastImpostor.color, LastImpostor.fullName) : LastImpostor.fullName;
                }
                else
                {
                    string postfix = useColors ? Helpers.cs(LastImpostor.color, LastImpostor.postfix) : LastImpostor.postfix;
                    roleName = String.Join(joinSeparator, roleInfo.Select(x => useColors ? Helpers.cs(x.color, x.name) : x.name).ToArray());
                    roleName += postfix;
                }
            }


            if (p.hasModifier(ModifierType.Munou))
            {
                if (PlayerControl.LocalPlayer.Data.IsDead || Munou.endGameFlag)
                {
                    string postfix = useColors ? Helpers.cs(Munou.color, Munou.postfix) : Munou.postfix;
                    // roleName = String.Join(joinSeparator, roleInfo.Select(x => useColors? Helpers.cs(x.color, x.name)  : x.name).ToArray());
                    roleName += postfix;
                }
            }

            if (p.hasModifier(ModifierType.AntiTeleport))
            {
                string postfix = useColors ? Helpers.cs(AntiTeleport.color, AntiTeleport.postfix) : AntiTeleport.postfix;
                // roleName = String.Join(joinSeparator, roleInfo.Select(x => useColors? Helpers.cs(x.color, x.name)  : x.name).ToArray());
                roleName += postfix;
            }

            return roleName;
        }
        static string ReadmePage = "";
        public static async Task loadReadme()
        {
            if (ReadmePage == "")
            {
                var currentLanguage = AmongUs.Data.DataManager.Settings.Language.CurrentLanguage;
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(currentLanguage == SupportedLangs.SChinese
                ? "https://dl.fangkuai.fun/ModFiles/TheOtherRoles-GM-Haoming/README_CN.md"
                : currentLanguage == SupportedLangs.Japanese
                ? "https://raw.githubusercontent.com/FangkuaiYa/TheOtherRoles-GM-Haoming/main/README_JP.md"
                : "https://raw.githubusercontent.com/FangkuaiYa/TheOtherRoles-GM-Haoming/main/README_ENG.md");
                response.EnsureSuccessStatusCode();
                string httpres = await response.Content.ReadAsStringAsync();
                ReadmePage = httpres;
            }
        }
        public static string GetRoleDescription(RoleInfo roleInfo)
        {
            while (ReadmePage == "")
            {
            }

            int index = ReadmePage.IndexOf($"## {ModTranslation.getString(roleInfo.name)}");
            int endindex = ReadmePage.Substring(index).IndexOf("### Game Options");
            return ReadmePage.Substring(index, endindex);
        }
    }
}
