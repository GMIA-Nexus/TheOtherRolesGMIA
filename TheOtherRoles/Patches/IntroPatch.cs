using HarmonyLib;
using System;
using System.Reflection;
using System.IO;
using static TheOtherRoles.TheOtherRoles;
using UnityEngine;
using UnityEngine.Video;
using System.Collections.Generic;
using System.Linq;
using Hazel;
using Reactor.Utilities.Extensions;
using TheOtherRoles.Utilities;
using TheOtherRoles.CustomGameModes;
using TheOtherRoles.Objects;
using TheOtherRoles.Modules;
using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using TheOtherRoles.Roles;
using PowerTools;

namespace TheOtherRoles.Patches
{
#if WINDOWS
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
#endif
    class IntroCutsceneOnDestroyPatch
    {
        public static PoolablePlayer playerPrefab;
        public static Vector3 bottomLeft;
        public static void Prefix(IntroCutscene __instance) {
            // Generate and initialize player icons
            int playerCounter = 0;
            int propHuntCounter = 0;
            List<RPCInvoker> allInvokers = [];
            if (PlayerControl.LocalPlayer != null && FastDestroyableSingleton<HudManager>.Instance != null) {
                float aspect = Camera.main.aspect;
                float safeOrthographicSize = CameraSafeArea.GetSafeOrthographicSize(Camera.main);
                float xpos = 1.75f - safeOrthographicSize * aspect * 1.70f;
                float ypos = 0.15f - safeOrthographicSize * 1.7f;
                bottomLeft = new Vector3(xpos / 2, ypos/2, -61f);

                foreach (PlayerControl p in PlayerControl.AllPlayerControls) {
                    NetworkedPlayerInfo data = p.Data;
                    PoolablePlayer player = UnityEngine.Object.Instantiate<PoolablePlayer>(__instance.PlayerPrefab, FastDestroyableSingleton<HudManager>.Instance.transform);
                    playerPrefab = __instance.PlayerPrefab;
                    p.SetPlayerMaterialColors(player.cosmetics.currentBodySprite.BodySprite);
                    player.SetSkin(data.DefaultOutfit.SkinId, data.DefaultOutfit.ColorId);
                    player.cosmetics.SetHat(data.DefaultOutfit.HatId, data.DefaultOutfit.ColorId);
                   // PlayerControl.SetPetImage(data.DefaultOutfit.PetId, data.DefaultOutfit.ColorId, player.PetSlot);
                    player.cosmetics.nameText.text = data.PlayerName;
                    player.SetFlipX(true);
                    TORMapOptions.playerIcons[p.PlayerId] = player;

                    player.gameObject.SetActive(false);

                    if (PlayerControl.LocalPlayer.isRole(RoleId.Arsonist) && p != PlayerControl.LocalPlayer) {
                        player.transform.localPosition = bottomLeft + new Vector3(-0.25f, -0.25f, 0) + Vector3.right * playerCounter++ * 0.35f;
                        player.transform.localScale = Vector3.one * 0.2f;
                        player.setSemiTransparent(true);
                        player.gameObject.SetActive(true);
                    } else if (PropHuntGM.isPropHuntGM) {
                        player.transform.localPosition = bottomLeft + new Vector3(-1.25f, -0.1f, 0) +
                                                         Vector3.right * propHuntCounter++ * 0.4f;
                        player.transform.localScale = Vector3.one * 0.3f;
                        if (p.Data.Role.IsImpostor)
                            player.cosmetics.nameText.text += $"{Helpers.cs(Palette.ImpostorRed, $" ({ModTranslation.getString("hunter")})")}";
                        else
                            player.cosmetics.nameText.text += $"{Helpers.cs(Palette.CrewmateBlue, $" ({ModTranslation.getString("prop")})")}";
                        player.gameObject.SetActive(true);

                    } else {   //  This can be done for all players not just for the bounty hunter as it was before. Allows the thief to have the correct position and scaling
                        player.transform.localPosition = bottomLeft;
                        player.transform.localScale = Vector3.one * 0.4f;
                        player.gameObject.SetActive(false);
                    }
                }
            }

            // Force Bounty Hunter to load a new Bounty when the Intro is over
            foreach (var bountyHunter in BountyHunter.players) {
                if (bountyHunter.bounty != null) {
                    if (PlayerControl.LocalPlayer == bountyHunter.player) bountyHunter.bountyUpdateTimer = 0f;
                }
            }

            // Place props
            if (CustomOptionHolder.activateProps.getBool() && !FreePlayGM.isFreePlayGM)
            {
                Props.placeProps();
            }

            TORGameManager.Instance?.GameStatistics.RecordEvent(new(GameStatistics.EventVariation.GameStart, null, 0) { RelatedTag = EventDetail.GameStart });

            if (CustomOptionHolder.randomGameStartPosition.getBool())
            { //Random spawn on game start
                var spawnLoc = MapData.GetCurrentMapData()?.SpawnPos ?? [];
                if (spawnLoc.Length > 0) PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(spawnLoc[rnd.Next(spawnLoc.Length)]);
            }

            // First kill
            if (AmongUsClient.Instance.AmHost && TORMapOptions.shieldFirstKill && TORMapOptions.firstKillName != "" && !PropHuntGM.isPropHuntGM) {
                PlayerControl target = PlayerControl.AllPlayerControls.ToArray().ToList().FirstOrDefault(x => x.Data.PlayerName.Equals(TORMapOptions.firstKillName));
                if (target != null) {
                    allInvokers.Add(RPCProcedure.SetFirstKill.GetInvoker(target.PlayerId));
                }
            }
            TORMapOptions.firstKillName = "";

            if (Helpers.isAirship() && CustomOptionHolder.airshipOptimize.getBool() && Helpers.hasImpVision(GameData.Instance.GetPlayerById(PlayerControl.LocalPlayer.PlayerId)))
            {
                var obj = ShipStatus.Instance.FastRooms[SystemTypes.GapRoom].gameObject;
                OneWayShadows oneWayShadow = obj.transform.FindChild("Shadow").FindChild("LedgeShadow").GetComponent<OneWayShadows>();
                oneWayShadow.gameObject.SetActive(false);
            }

            TORGameManager.Instance?.OnGameStart();
            HudManager.Instance.ShowVanillaKeyGuide();

            if (AmongUsClient.Instance.AmHost) {
                var imp = LastImpostor.doNeedPromotion();
                if (imp.Item1)
                    allInvokers.Add(RPCProcedure.ImpostorPromotesToLastImpostor.GetInvoker(imp.Item2?.PlayerId ?? byte.MaxValue));
            }

            Kataomoi.generateText();

            SchrodingersCat.playerTemplate = UnityEngine.Object.Instantiate(__instance.PlayerPrefab, FastDestroyableSingleton<HudManager>.Instance.transform);
            SchrodingersCat.playerTemplate.UpdateFromPlayerOutfit(PlayerControl.LocalPlayer.Data.DefaultOutfit, PlayerMaterial.MaskType.ComplexUI, false, true);
            SchrodingersCat.playerTemplate.SetFlipX(true);
            SchrodingersCat.playerTemplate.gameObject.SetActive(false);
            SchrodingersCat.playerTemplate.cosmetics.currentPet?.gameObject.SetActive(false);
            SchrodingersCat.playerTemplate.cosmetics.nameText.text = "";
            SchrodingersCat.playerTemplate.gameObject.SetActive(false);

            AssignRolePatch.blockAssignRole = true;

            if (AmongUsClient.Instance.AmHost && (Archaeologist.exists || FreePlayGM.isFreePlayGM))
            {
                allInvokers.Add(RPCProcedure.PlaceAntique.GetInvoker());
            }

            // Add Electrical
            FungleAdditionalElectrical.CreateElectrical();

            if (CustomOptionHolder.foxSpawnRate.getSelection() > 0 && (Shrine.allShrine == null || Shrine.allShrine.Count == 0))
            {
                Shrine.activateShrines(GameOptionsManager.Instance.currentNormalGameOptions.MapId);
                List<Byte> taskIdList = new();
                Shrine.allShrine.ForEach(shrine => taskIdList.Add((byte)shrine.console.ConsoleId));
                taskIdList.Shuffle();
                var cpt = new CustomNormalPlayerTask("foxTaskStay", Il2CppType.Of<FoxTask>(), Fox.numTasks, taskIdList.ToArray(), Shrine.allShrine.Find(x => x.console.ConsoleId == taskIdList.ToArray()[0]).console.Room, true);
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (p.isRole(RoleId.Fox))
                    {
                        p.clearAllTasks();
                        cpt.addTaskToPlayer(p.PlayerId);
                    }
                }
            }

            if (allInvokers.Count > 0) CombinedRemoteProcess.CombinedRPC.Invoke([.. allInvokers]);

            EventUtility.gameStartsUpdate();

            if (PropHuntGM.isPropHuntGM) {
                // Freeze hunters during blackout
                foreach (PlayerControl player in PropHuntGM.getHunters()) {
                    player.moveable = false;
                    player.NetTransform.Halt();
                }
                RPCProcedure.PropHuntStartTimer.Invoke(true);

                // Play intro video during blackout
                var assembly = Assembly.GetExecutingAssembly();
                var resourceBundle = assembly.GetManifestResourceStream("TheOtherRoles.Resources.IntroAnimation.intro");
                var assetBundle = AssetBundle.LoadFromMemory(resourceBundle.ReadFully());
                var introVid = assetBundle.LoadAsset("Assets/Video/intro.webm").Cast<VideoClip>();
                var camera = GameObject.Find("Main Camera");
                var videoPlayer = camera.AddComponent<VideoPlayer>();
                videoPlayer.playOnAwake = false;
                videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
                videoPlayer.targetCameraAlpha = 1F;
                videoPlayer.source = VideoSource.VideoClip;
                videoPlayer.clip = introVid;
                videoPlayer.aspectRatio = VideoAspectRatio.FitVertically;
                videoPlayer.frame = (21 - (int)PropHuntGM.initialBlackoutTime) * 25;
                videoPlayer.isLooping = false;
                videoPlayer.Play();

                FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(PropHuntGM.initialBlackoutTime + 10f / 25, new Action<float>((p) => {
                    if (p == 1f) {
                        RPCProcedure.PropHuntStartTimer.Invoke(false);
                        PlayerControl.LocalPlayer.moveable = true;
                        HudManager.Instance.FullScreen.enabled = false;
                        UnityEngine.Object.Destroy(videoPlayer);
                        assetBundle.Unload(false);
                    } else {
                        HudManager.Instance.FullScreen.enabled = true;
                        HudManager.Instance.FullScreen.gameObject.SetActive(true);
                    }
                })));

                ShipStatusPatch.originalNumCrewVisionOption = GameOptionsManager.Instance.currentNormalGameOptions.CrewLightMod;
                ShipStatusPatch.originalNumImpVisionOption = GameOptionsManager.Instance.currentNormalGameOptions.ImpostorLightMod;
                ShipStatusPatch.originalNumKillCooldownOption = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown;

                GameOptionsManager.Instance.currentNormalGameOptions.ImpostorLightMod = CustomOptionHolder.propHunterVision.getFloat();
                GameOptionsManager.Instance.currentNormalGameOptions.CrewLightMod = CustomOptionHolder.propVision.getFloat();
                GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown = CustomOptionHolder.hunterHitCooldown.getFloat();
            }
        }
    }

    [HarmonyPatch]
    class IntroPatch {
        public static IEnumerator CoBegin(IntroCutscene __instance)
        {
            SoundManager.Instance.PlaySound(__instance.IntroStinger, false, 1f, null);
            if (GameManager.Instance.IsNormal())
            {
                __instance.LogPlayerRoleData();
                __instance.HideAndSeekPanels.SetActive(false);
                __instance.CrewmateRules.SetActive(false);
                __instance.ImpostorRules.SetActive(false);
                __instance.ImpostorName.gameObject.SetActive(false);
                __instance.ImpostorTitle.gameObject.SetActive(false);
                var list = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                list =
                    IntroCutscene.SelectTeamToShow(
                        (Func<NetworkedPlayerInfo, bool>)(pcd =>
                            !PlayerControl.LocalPlayer.Data.Role.IsImpostor ||
                            pcd.Role.TeamType == PlayerControl.LocalPlayer.Data.Role.TeamType
                        )
                    );
                if (PlayerControl.LocalPlayer.Data.Role.IsImpostor)
                {
                    __instance.ImpostorText.gameObject.SetActive(false);
                }
                else
                {
                    int adjustedNumImpostors = GameManager.Instance.LogicOptions.GetAdjustedNumImpostors(GameData.Instance.PlayerCount);
                    if (adjustedNumImpostors == 1)
                    {
                        __instance.ImpostorText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.NumImpostorsS, new UnityEngine.Object());
                    }
                    else
                    {
                        var parameters = new Il2CppReferenceArray<Il2CppSystem.Object>(new Il2CppSystem.Object[] { (Il2CppSystem.Object)adjustedNumImpostors });
                        __instance.ImpostorText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.NumImpostorsP, parameters);
                    }
                    __instance.ImpostorText.text = __instance.ImpostorText.text.Replace("[FF1919FF]", "<color=#FF1919FF>");
                    __instance.ImpostorText.text = __instance.ImpostorText.text.Replace("[]", "</color>");
                }
                yield return __instance.ShowTeam(list, 3f);
                yield return RoleDraft.CoSelectRoles(__instance).WrapToIl2Cpp();
                yield return SetUpRoleTextPatch.SetRoleTexts(__instance).WrapToIl2Cpp();
            }
            else
            {
                __instance.LogPlayerRoleData();
                __instance.HideAndSeekPanels.SetActive(true);
                if (PlayerControl.LocalPlayer.Data.Role.IsImpostor)
                {
                    __instance.CrewmateRules.SetActive(false);
                    __instance.ImpostorRules.SetActive(true);
                }
                else
                {
                    __instance.CrewmateRules.SetActive(true);
                    __instance.ImpostorRules.SetActive(false);
                }
                Il2CppSystem.Collections.Generic.List<PlayerControl> list2 = IntroCutscene.SelectTeamToShow(
                    (Func<NetworkedPlayerInfo, bool>)(pcd => PlayerControl.LocalPlayer.Data.Role.IsImpostor != pcd.Role.IsImpostor)
                );
                PlayerControl impostor = PlayerControl.AllPlayerControls.Find(
                    (Il2CppSystem.Predicate<PlayerControl>)(pc => pc.Data.Role.IsImpostor)
                );
                GameManager.Instance.SetSpecialCosmetics(impostor);
                __instance.ImpostorName.gameObject.SetActive(true);
                __instance.ImpostorTitle.gameObject.SetActive(true);
                __instance.BackgroundBar.enabled = false;
                __instance.TeamTitle.gameObject.SetActive(false);
                if (impostor != null)
                {
                    __instance.ImpostorName.text = impostor.Data.PlayerName;
                }
                else
                {
                    __instance.ImpostorName.text = "???";
                }
                yield return new WaitForSecondsRealtime(0.1f);
                PoolablePlayer playerSlot = null;
                if (impostor != null)
                {
                    playerSlot = __instance.CreatePlayer(1, 1, impostor.Data, false);
                    playerSlot.SetBodyType(PlayerBodyTypes.Normal);
                    playerSlot.SetFlipX(false);
                    playerSlot.transform.localPosition = __instance.impostorPos;
                    playerSlot.transform.localScale = Vector3.one * __instance.impostorScale;
                }
                yield return ShipStatus.Instance.CosmeticsCache.PopulateFromPlayers();
                yield return new WaitForSecondsRealtime(6f);
                if (playerSlot != null)
                {
                    playerSlot.gameObject.SetActive(false);
                }
                __instance.HideAndSeekPanels.SetActive(false);
                __instance.CrewmateRules.SetActive(false);
                __instance.ImpostorRules.SetActive(false);
                LogicOptionsHnS logicOptionsHnS = GameManager.Instance.LogicOptions as LogicOptionsHnS;
                LogicHnSMusic logicHnSMusic = GameManager.Instance.GetLogicComponent<LogicHnSMusic>() as LogicHnSMusic;
                if (logicHnSMusic != null)
                {
                    logicHnSMusic.StartMusicWithIntro();
                }
                if (PlayerControl.LocalPlayer.Data.Role.IsImpostor)
                {
                    float crewmateLeadTime = (float)logicOptionsHnS.GetCrewmateLeadTime();
                    __instance.HideAndSeekTimerText.gameObject.SetActive(true);
                    PoolablePlayer poolablePlayer;
                    AnimationClip animationClip;
                    if (AprilFoolsMode.ShouldHorseAround())
                    {
                        poolablePlayer = __instance.HorseWrangleVisualSuit;
                        poolablePlayer.gameObject.SetActive(true);
                        poolablePlayer.SetBodyType(PlayerBodyTypes.Seeker);
                        animationClip = __instance.HnSSeekerSpawnHorseAnim;
                        __instance.HorseWrangleVisualPlayer.SetBodyType(PlayerBodyTypes.Normal);
                        __instance.HorseWrangleVisualPlayer.UpdateFromPlayerData(PlayerControl.LocalPlayer.Data, PlayerControl.LocalPlayer.CurrentOutfitType, PlayerMaterial.MaskType.None, false, null, false);
                    }
                    else if (AprilFoolsMode.ShouldLongAround())
                    {
                        poolablePlayer = __instance.HideAndSeekPlayerVisual;
                        poolablePlayer.gameObject.SetActive(true);
                        poolablePlayer.SetBodyType(PlayerBodyTypes.LongSeeker);
                        animationClip = __instance.HnSSeekerSpawnLongAnim;
                    }
                    else
                    {
                        poolablePlayer = __instance.HideAndSeekPlayerVisual;
                        poolablePlayer.gameObject.SetActive(true);
                        poolablePlayer.SetBodyType(PlayerBodyTypes.Seeker);
                        animationClip = __instance.HnSSeekerSpawnAnim;
                    }
                    poolablePlayer.SetBodyCosmeticsVisible(false);
                    poolablePlayer.UpdateFromPlayerData(PlayerControl.LocalPlayer.Data, PlayerControl.LocalPlayer.CurrentOutfitType, PlayerMaterial.MaskType.None, false, null, false);
                    SpriteAnim component = poolablePlayer.GetComponent<SpriteAnim>();
                    poolablePlayer.gameObject.SetActive(true);
                    poolablePlayer.ToggleName(false);
                    component.Play(animationClip, 1f);
                    while (crewmateLeadTime > 0f)
                    {
                        __instance.HideAndSeekTimerText.text = Mathf.RoundToInt(crewmateLeadTime).ToString();
                        crewmateLeadTime -= Time.deltaTime;
                        yield return null;
                    }
                }
                else
                {
                    ShipStatus.Instance.HideCountdown = (float)logicOptionsHnS.GetCrewmateLeadTime();
                    if (AprilFoolsMode.ShouldHorseAround())
                    {
                        if (impostor != null)
                        {
                            impostor.AnimateCustom(__instance.HnSSeekerSpawnHorseInGameAnim);
                        }
                    }
                    else if (AprilFoolsMode.ShouldLongAround())
                    {
                        if (impostor != null)
                        {
                            impostor.AnimateCustom(__instance.HnSSeekerSpawnLongInGameAnim);
                        }
                    }
                    else if (impostor != null)
                    {
                        impostor.AnimateCustom(__instance.HnSSeekerSpawnAnim);
                        impostor.cosmetics.SetBodyCosmeticsVisible(false);
                    }
                }
                impostor = null;
                playerSlot = null;
            }
            ShipStatus.Instance.StartSFX();
            UnityEngine.Object.Destroy(__instance.gameObject);
#if ANDROID
            IntroCutsceneOnDestroyPatch.Prefix(__instance);
#endif
            yield break;
        }

        public static void setupIntroTeamIcons(IntroCutscene __instance, ref  Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam) {
            // Intro solo teams
            if (Helpers.isNeutral(PlayerControl.LocalPlayer) && !(PlayerControl.LocalPlayer.isRole(RoleId.SchrodingersCat) && SchrodingersCat.hideRole)) {
                var soloTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                soloTeam.Add(PlayerControl.LocalPlayer);
                yourTeam = soloTeam;
            }

            // Add the Spy to the Impostor team (for the Impostors)
            if (Spy.exists && PlayerControl.LocalPlayer.Data.Role.IsImpostor) {
                List<PlayerControl> players = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
                var fakeImpostorTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>(); // The local player always has to be the first one in the list (to be displayed in the center)
                fakeImpostorTeam.Add(PlayerControl.LocalPlayer);
                foreach (PlayerControl p in players) {
                    if (PlayerControl.LocalPlayer != p && (p.isRole(RoleId.Spy) || p.Data.Role.IsImpostor))
                        fakeImpostorTeam.Add(p);
                }
                yourTeam = fakeImpostorTeam;
            }

            // Role draft: If spy is enabled, don't show the team
            if (RoleDraft.isEnabled && CustomOptionHolder.spySpawnRate.getSelection() > 0 && PlayerControl.AllPlayerControls.ToArray().ToList().Where(x => x.Data.Role.IsImpostor).Count() > 1)
            {
                var fakeImpostorTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>(); // The local player always has to be the first one in the list (to be displayed in the center)
                fakeImpostorTeam.Add(PlayerControl.LocalPlayer);
                yourTeam = fakeImpostorTeam;
            }
        }

        public static void setupIntroTeam(IntroCutscene __instance, ref  Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam) {
            List<RoleInfo> infos = RoleInfo.getRoleInfoForPlayer(PlayerControl.LocalPlayer);
            RoleInfo roleInfo = infos.Where(info => !info.isModifier).FirstOrDefault();
            var neutralColor = new Color32(76, 84, 78, 255);
            if (roleInfo == null || roleInfo == RoleInfo.crewmate) {
                if (RoleDraft.isEnabled && CustomOptionHolder.neutralRolesCountMax.getSelection() > 0) {
                    __instance.TeamTitle.text = $"<size=60%>{FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Crewmate)}" +
                        Helpers.cs(Color.white, " / ") + Helpers.cs(neutralColor, ModTranslation.getString("roleIntroNeutral")) + "</size>";
                }
                return;
            }
            if (roleInfo == null) return;
            if (roleInfo.isNeutral && !(PlayerControl.LocalPlayer.isRole(RoleId.SchrodingersCat) && SchrodingersCat.hideRole)) {
                __instance.BackgroundBar.material.color = neutralColor;
                __instance.TeamTitle.text = ModTranslation.getString("roleIntroNeutral");
                __instance.TeamTitle.color = neutralColor;
            }
        }

        public static IEnumerator<WaitForSeconds> EndShowRole(IntroCutscene __instance) {
            yield return new WaitForSeconds(5f);
            __instance.YouAreText.gameObject.SetActive(false);
            __instance.RoleText.gameObject.SetActive(false);
            __instance.RoleBlurbText.gameObject.SetActive(false);
            __instance.ourCrewmate.gameObject.SetActive(false);
           
        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CreatePlayer))]
        class CreatePlayerPatch {
            public static void Postfix(IntroCutscene __instance, bool impostorPositioning, ref PoolablePlayer __result) {
                if (impostorPositioning) __result.SetNameColor(Palette.ImpostorRed);
            }
        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin))]
        class IntroCutsceneCoBeginPatch
        {
            public static bool Prefix(IntroCutscene __instance, ref Il2CppSystem.Collections.IEnumerator __result)
            {
                __result = CoBegin(__instance).WrapToIl2Cpp();

                return false;
            }
        }

        class SetUpRoleTextPatch {
            static int seed = 0;
            static public IEnumerator SetRoleTexts(IntroCutscene __instance) {
                seed = rnd.Next(5000);

                // Don't override the intro of the vanilla roles
                List<RoleInfo> infos = RoleInfo.getRoleInfoForPlayer(PlayerControl.LocalPlayer);
                RoleInfo roleInfo = infos.Where(info => !info.isModifier).FirstOrDefault();
                List<RoleInfo> modifierInfo = infos.Where(info => info.isModifier).ToList();

                if (roleInfo == RoleInfo.fortuneTeller && FortuneTeller.numTasks > 0) {
                    roleInfo = RoleInfo.crewmate;
                }

                if (EventUtility.isEnabled) {
                    var roleInfos = RoleInfo.allRoleInfos.Where(x => !x.isModifier).ToList();
                    if (roleInfo.isNeutral) roleInfos.RemoveAll(x => !x.isNeutral);
                    if (roleInfo.color == Palette.ImpostorRed) roleInfos.RemoveAll(x => x.color != Palette.ImpostorRed);
                    if (!roleInfo.isNeutral && roleInfo.color != Palette.ImpostorRed) roleInfos.RemoveAll(x => x.color == Palette.ImpostorRed || x.isNeutral);
                    var rnd = new System.Random(seed);
                    roleInfo = roleInfos[rnd.Next(roleInfos.Count)];
                }

                __instance.RoleBlurbText.text = "";
                __instance.RoleBlurbText.transform.localPosition = new(0.0965f, -2.12f, -36f);
                __instance.RoleBlurbText.rectTransform.sizeDelta = new(12.8673f, 0.7f);
                __instance.RoleBlurbText.alignment = TMPro.TextAlignmentOptions.Top;
                if (roleInfo != null) {
                    __instance.YouAreText.color = roleInfo.color;
                    __instance.RoleText.text = roleInfo.name;
                    __instance.RoleText.color = roleInfo.color;
                    __instance.RoleBlurbText.text = roleInfo.introDescription;
                    __instance.RoleBlurbText.color = roleInfo.color;
                }

                // Setup Madmate Intro
                if (Madmate.madmate.Any(x => x.PlayerId == PlayerControl.LocalPlayer.PlayerId))
                {
                    if (roleInfo == RoleInfo.crewmate) __instance.RoleText.text = ModTranslation.getString("madmate");
                    else __instance.RoleText.text = ModTranslation.getString("madmatePrefix") + __instance.RoleText.text;
                    __instance.YouAreText.color = Madmate.color;
                    __instance.RoleText.color = Madmate.color;
                    __instance.RoleBlurbText.text = ModTranslation.getString("madmateIntroDesc");
                    __instance.RoleBlurbText.color = Madmate.color;
                }

                if (modifierInfo != null) {
                    foreach (var info in modifierInfo) {
                        if (info.roleId != RoleId.Lover)
                            __instance.RoleBlurbText.text += Helpers.cs(info.color, $"\n{info.introDescription}");
                        else {
                            PlayerControl otherLover = PlayerControl.LocalPlayer.getPartner();
                            __instance.RoleBlurbText.text += "\n" + Helpers.cs(Lovers.color, String.Format(ModTranslation.getString("loversFlavor"), otherLover?.Data?.PlayerName ?? ""));
                        }
                    }
                }
                if (Deputy.knowsSheriff && Deputy.exists && Sheriff.exists) {
                    if (infos.Any(info => info.roleId == RoleId.Sheriff)) {
                        var deputy = Sheriff.getDeputy(PlayerControl.LocalPlayer);
                        if (deputy != null) __instance.RoleBlurbText.text += Helpers.cs(Sheriff.color, string.Format(ModTranslation.getString("deputyIntroLine"), deputy?.player?.Data?.PlayerName ?? ""));
                    }
                    else if (infos.Any(info => info.roleId == RoleId.Deputy))
                        __instance.RoleBlurbText.text += Helpers.cs(Sheriff.color, string.Format(ModTranslation.getString("sheriffIntroLine"), Deputy.getRole(PlayerControl.LocalPlayer)?.sheriff?.player?.Data?.PlayerName ?? ""));
                }
                if (infos.Any(info => info.roleId == RoleId.Kataomoi)) {
                    __instance.RoleBlurbText.text += Helpers.cs(Kataomoi.color, string.Format(ModTranslation.getString("kataomoiIntroLine"), Kataomoi.target?.Data?.PlayerName ?? ""));
                }
                if (infos.Any(info => info.roleId == RoleId.Yandere)) {
                    __instance.RoleBlurbText.text += Helpers.cs(Yandere.color, string.Format(ModTranslation.getString("yandereIntroLine"), Yandere.target?.Data?.PlayerName ?? ""));
                }

                SoundManager.Instance.PlaySound(PlayerControl.LocalPlayer.Data.Role.IntroSound, false, 1f, null);
                __instance.YouAreText.gameObject.SetActive(true);
                __instance.RoleText.gameObject.SetActive(true);
                __instance.RoleBlurbText.gameObject.SetActive(true);
                if (__instance.ourCrewmate == null)
                {
                    __instance.ourCrewmate = __instance.CreatePlayer(0, 1, PlayerControl.LocalPlayer.Data, false);
                    __instance.ourCrewmate.gameObject.SetActive(false);
                }
                __instance.ourCrewmate.gameObject.SetActive(true);
                __instance.ourCrewmate.transform.localPosition = new Vector3(0f, -1.05f, -18f);
                __instance.ourCrewmate.transform.localScale = new Vector3(1f, 1f, 1f);
                __instance.ourCrewmate.ToggleName(false);
                yield return new WaitForSeconds(2.5f);
                __instance.YouAreText.gameObject.SetActive(false);
                __instance.RoleText.gameObject.SetActive(false);
                __instance.RoleBlurbText.gameObject.SetActive(false);
                __instance.ourCrewmate.gameObject.SetActive(false);
                yield break;
            }
        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
        class BeginCrewmatePatch {
            public static void Prefix(IntroCutscene __instance, ref  Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay) {
                setupIntroTeamIcons(__instance, ref teamToDisplay);
            }

            public static void Postfix(IntroCutscene __instance, ref  Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay) {
                setupIntroTeam(__instance, ref teamToDisplay);
            }
        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
        class BeginImpostorPatch {
            public static void Prefix(IntroCutscene __instance, ref  Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam) {
                setupIntroTeamIcons(__instance, ref yourTeam);
            }

            public static void Postfix(IntroCutscene __instance, ref  Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam) {
                setupIntroTeam(__instance, ref yourTeam);
            }
        }
    }

    /* Horses are broken since 2024.3.5 - keeping this code in case they return.
     * [HarmonyPatch(typeof(AprilFoolsMode), nameof(AprilFoolsMode.ShouldHorseAround))]
    public static class ShouldAlwaysHorseAround {
        public static bool Prefix(ref bool __result) {
            __result = EventUtility.isEnabled && !EventUtility.disableEventMode;
            return false;
        }
    }*/

    [HarmonyPatch(typeof(AprilFoolsMode), nameof(AprilFoolsMode.ShouldShowAprilFoolsToggle))]
    public static class ShouldShowAprilFoolsToggle
    {
        public static void Postfix(ref bool __result)
        {
            __result = __result || EventUtility.isEventDate || EventUtility.canBeEnabled;  // Extend it to a 7 day window instead of just 1st day of the Month
        }
    }
}

