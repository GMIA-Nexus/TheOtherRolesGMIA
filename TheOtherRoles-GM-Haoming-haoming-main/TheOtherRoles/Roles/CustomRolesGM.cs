using System;
using System.Collections.Generic;
using HarmonyLib;
using TheOtherRoles.Patches;
using UnityEngine;
using static TheOtherRoles.GameHistory;

namespace TheOtherRoles
{
    [HarmonyPatch]
    public static class TheOtherRolesGM
    {

        public static void clearAndReloadRoles()
        {
            Morphling.clearAndReload();
            Camouflager.clearAndReload();
            Shifter.clearAndReload();
            Swapper.clearAndReload();
            GM.clearAndReload();

            Lovers.Clear();
            Opportunist.Clear();
            Ninja.Clear();
            Madmate.Clear();
            CreatedMadmate.Clear();
            PlagueDoctor.Clear();
            Lighter.Clear();
            SerialKiller.Clear();
            Fox.Clear();
            Immoralist.Clear();
            FortuneTeller.Clear();
            LastImpostor.Clear();
            SoulPlayer.Clear();
            Munou.Clear();
            SchrodingersCat.Clear();
            Trapper.Clear();
            BomberA.Clear();
            BomberB.Clear();
            EvilTracker.Clear();
            Puppeteer.Clear();
            MimicK.Clear();
            MimicA.Clear();
            Mini.Clear();
            AntiTeleport.Clear();
            JekyllAndHyde.Clear();
            Akujo.Clear();
            Moriarty.Clear();
            Sherlock.Clear();
            Role.ClearAll();
        }

        public static void FixedUpdate(PlayerControl player)
        {
            Role.allRoles.DoIf(x => x.player == player, x => x.FixedUpdate());
            Modifier.allModifiers.DoIf(x => x.player == player, x => x.FixedUpdate());
        }

        public static void OnMeetingStart()
        {
            Role.allRoles.Do(x => x.OnMeetingStart());
            Modifier.allModifiers.Do(x => x.OnMeetingStart());

            GM.resetZoom();
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(3f, new Action<float>((p) =>
            {
                if (p == 1)
                {
                    Camouflager.resetCamouflage();
                    Morphling.resetMorph();
                }
            })));
        }

        public static void OnMeetingEnd()
        {
            Role.allRoles.Do(x => x.OnMeetingEnd());
            Modifier.allModifiers.Do(x => x.OnMeetingEnd());

            CustomOverlays.hideInfoOverlay();
            CustomOverlays.hideBlackBG();
        }

        [HarmonyPatch(typeof(GameData), nameof(GameData.HandleDisconnect), new Type[] { typeof(PlayerControl), typeof(DisconnectReasons) })]
        class HandleDisconnectPatch
        {
            public static void Postfix(GameData __instance, PlayerControl player, DisconnectReasons reason)
            {
                if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
                {
                    Role.allRoles.Do(x => x.HandleDisconnect(player, reason));
                    Modifier.allModifiers.Do(x => x.HandleDisconnect(player, reason));

                    Lovers.HandleDisconnect(player, reason);
                    Shifter.HandleDisconnect(player, reason);

                    finalStatuses[player.PlayerId] = FinalStatus.Disconnected;
                }
            }
        }

        public static class Morphling
        {
            public static PlayerControl morphling;
            public static Color color = Palette.ImpostorRed;
            private static Sprite sampleSprite;
            private static Sprite morphSprite;

            public static float cooldown = 30f;
            public static float duration = 10f;

            public static PlayerControl currentTarget;
            public static PlayerControl sampledTarget;
            public static PlayerControl morphTarget;
            public static float morphTimer = 0f;

            public static void handleMorphing()
            {
                if (morphling == null) return;

                // first, if camo is active, don't do anything
                if (Camouflager.camouflager != null && Camouflager.camouflageTimer > 0f) return;

                // next, if we're currently morphed, set our skin to the target
                if (morphTimer > 0f && morphTarget != null)
                {
                    morphling.morphToPlayer(morphTarget);
                }
                else
                {
                    morphling.resetMorph();
                }
            }

            public static void startMorph(PlayerControl target)
            {
                morphTarget = target;
                morphTimer = duration;
                handleMorphing();
            }

            public static void resetMorph()
            {
                morphTarget = null;
                morphTimer = 0f;
                handleMorphing();
            }

            public static void clearAndReload()
            {
                resetMorph();
                morphling = null;
                currentTarget = null;
                sampledTarget = null;
                morphTarget = null;
                morphTimer = 0f;
                cooldown = CustomOptionHolder.morphlingCooldown.getFloat();
                duration = CustomOptionHolder.morphlingDuration.getFloat();
            }

            public static Sprite getSampleSprite()
            {
                if (sampleSprite) return sampleSprite;
                sampleSprite = ModTranslation.getImage("SampleButton", 115f);
                return sampleSprite;
            }

            public static Sprite getMorphSprite()
            {
                if (morphSprite) return morphSprite;
                morphSprite = ModTranslation.getImage("MorphButton", 115f);
                return morphSprite;
            }
        }

        public static class Camouflager
        {
            public static PlayerControl camouflager;
            public static Color color = Palette.ImpostorRed;

            public static float cooldown = 30f;
            public static float duration = 10f;
            public static float camouflageTimer = 0f;
            public static bool randomColors = false;

            public static NetworkedPlayerInfo.PlayerOutfit camoData;

            private static Sprite buttonSprite;
            public static Sprite getButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModTranslation.getImage("CamoButton", 115f);
                return buttonSprite;
            }

            public static void startCamouflage()
            {
                camouflageTimer = duration;
                if (Helpers.MushroomSabotageActive()) return; // Dont overwrite the fungle "camo"
                if (randomColors)
                    camoData.ColorId = (byte)TheOtherRoles.rnd.Next(0, Palette.PlayerColors.Length);
                else
                    camoData.ColorId = 6;

                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (p == null) continue;
                    p.setOutfit(camoData, visible: false);
                }
            }

            public static void resetCamouflage()
            {
                camouflageTimer = 0f;
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (p.isRole(RoleType.Ninja) && Ninja.isStealthed(p))
                        continue;
                    if (p == null) continue;

                    // special case for morphling
                    if (Morphling.morphling?.PlayerId == p.PlayerId)
                    {
                        Morphling.handleMorphing();
                    }
                    else
                    {
                        p.resetMorph();
                    }
                }
            }

            public static void clearAndReload()
            {
                resetCamouflage();
                camouflager = null;
                camouflageTimer = 0f;
                cooldown = CustomOptionHolder.camouflagerCooldown.getFloat();
                duration = CustomOptionHolder.camouflagerDuration.getFloat();
                randomColors = CustomOptionHolder.camouflagerRandomColors.getBool();

                camoData = new NetworkedPlayerInfo.PlayerOutfit
                {
                    PlayerName = "",
                    HatId = "",
                    ColorId = 6,
                    SkinId = "",
                    PetId = "",
                    VisorId = "",
                    NamePlateId = ""
                };
            }
        }

        public static class Shifter
        {
            public static PlayerControl shifter;
            public static List<int> pastShifters = new();
            public static Color color = new Color32(102, 102, 102, byte.MaxValue);

            public static PlayerControl futureShift;
            public static PlayerControl currentTarget;
            public static bool shiftModifiers = false;

            public static bool isNeutral = false;
            public static bool shiftPastShifters = false;

            public static void HandleDisconnect(PlayerControl player, DisconnectReasons reason)
            {
                if (futureShift == player) futureShift = null;
            }

            private static Sprite buttonSprite;
            public static Sprite getButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModTranslation.getImage("ShiftButton", 115f);
                return buttonSprite;
            }

            public static void clearAndReload()
            {
                shifter = null;
                pastShifters = new List<int>();
                currentTarget = null;
                futureShift = null;
                shiftModifiers = CustomOptionHolder.shifterShiftsModifiers.getBool();
                shiftPastShifters = CustomOptionHolder.shifterPastShifters.getBool();
                isNeutral = false;
            }
        }

        public static class Swapper
        {
            public static PlayerControl swapper;
            public static Color color = new Color32(134, 55, 86, byte.MaxValue);
            private static Sprite spriteCheck;
            public static bool canCallEmergency = false;
            public static bool canOnlySwapOthers = false;
            public static int numSwaps = 2;

            public static byte playerId1 = Byte.MaxValue;
            public static byte playerId2 = Byte.MaxValue;

            public static Sprite getCheckSprite()
            {
                if (spriteCheck) return spriteCheck;
                spriteCheck = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.SwapperCheck.png", 150f);
                return spriteCheck;
            }

            public static void clearAndReload()
            {
                swapper = null;
                playerId1 = Byte.MaxValue;
                playerId2 = Byte.MaxValue;
                canCallEmergency = CustomOptionHolder.swapperCanCallEmergency.getBool();
                canOnlySwapOthers = CustomOptionHolder.swapperCanOnlySwapOthers.getBool();
                numSwaps = Mathf.RoundToInt(CustomOptionHolder.swapperNumSwaps.getFloat());
            }
        }


        public static class GM
        {
            public static PlayerControl gm;
            public static Color color = new Color32(255, 91, 112, byte.MaxValue);

            public static bool gmIsHost = true;
            public static bool diesAtStart = true;
            public static bool hasTasks = false;
            public static bool canSabotage = false;
            public static bool canWarp = true;
            public static bool canKill = false;

            private static Sprite zoomInSprite;
            private static Sprite zoomOutSprite;

            public static Sprite getZoomInSprite()
            {
                if (zoomInSprite) return zoomInSprite;
                zoomInSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.GMZoomIn.png", 115f / 2f);
                return zoomInSprite;
            }
            public static Sprite getZoomOutSprite()
            {
                if (zoomOutSprite) return zoomOutSprite;
                zoomOutSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.GMZoomOut.png", 115f / 2f);
                return zoomOutSprite;
            }

            public static void resetZoom()
            {
                Camera.main.orthographicSize = 3.0f;
                FastDestroyableSingleton<HudManager>.Instance.UICamera.orthographicSize = 3.0f;
                FastDestroyableSingleton<HudManager>.Instance.transform.localScale = Vector3.one;
            }

            public static void FixedUpdate()
            {
            }

            public static void clearAndReload()
            {
                gm = null;
                gmIsHost = CustomOptionHolder.gmIsHost.getBool();
                diesAtStart = CustomOptionHolder.gmDiesAtStart.getBool();
                hasTasks = false;
                canSabotage = false;
                zoomInSprite = null;
                zoomOutSprite = null;
                canWarp = CustomOptionHolder.gmCanWarp.getBool();
                canKill = CustomOptionHolder.gmCanKill.getBool();

                foreach (PoolablePlayer p in TORMapOptions.playerIcons.Values)
                {
                    if (p != null && p.gameObject != null) p.gameObject.SetActive(false);
                }
            }
        }
    }
}
