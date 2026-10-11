using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using System;
using UnityEngine.Windows.Speech;
using static UnityEngine.GraphicsBuffer;
using System.Linq;
using TheOtherRoles.Roles;

namespace TheOtherRoles.Patches
{
    [HarmonyPatch]
    public static class TransportationToolPatches
    {
        /* 
         * Moving Plattform / Zipline / Ladders move the player out of bounds, thus we want to disable functions of the mod if the player is currently using one of these.
         * Save the players anti tp position before using it.
         * 
         * Zipline can also break camo, fix that one too.
         */

        public static bool isUsingTransportation(PlayerControl pc)
        {
            return pc.inMovingPlat || pc.onLadder;
        }

        // Zipline:
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ZiplineBehaviour), nameof(ZiplineBehaviour.Use), [typeof(PlayerControl), typeof(bool)])]
        public static void prefix3(ZiplineBehaviour __instance, PlayerControl player, bool fromTop)
        {
            AntiTeleport.position = PlayerControl.LocalPlayer.transform.position;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ZiplineBehaviour), nameof(ZiplineBehaviour.Use), [typeof(PlayerControl), typeof(bool)])]
        public static void postfix(ZiplineBehaviour __instance, PlayerControl player, bool fromTop)
        {
            // Fix camo:
            __instance.StartCoroutine(Effects.Lerp(fromTop ? __instance.downTravelTime : __instance.upTravelTime, new System.Action<float>((p) => {
                HandZiplinePoolable hand;
                __instance.playerIdHands.TryGetValue(player.PlayerId, out hand);
                if (hand != null)
                {
                    if (Camouflager.camouflageTimer <= 0 && !Helpers.MushroomSabotageActive())
                    {
                        var modInfo = player.GetOutfitInfo();
                        player.RawSetHat((modInfo?.CurrentOutfit ?? player.CurrentOutfit).HatId, (modInfo?.CurrentOutfit ?? player.CurrentOutfit).ColorId);

                        if (Ninja.isStealthed(player) || Sprinter.isSprinting(player)
                            || (player.isRole(RoleId.Fox) && Fox.stealthed) || (player.isRole(RoleId.Kataomoi) && Kataomoi.isStalking())) {
                            hand.SetPlayerColor(player.CurrentOutfit, PlayerMaterial.MaskType.None, 0f);
                        } else {
                            hand.SetPlayerColor(player.CurrentOutfit, PlayerMaterial.MaskType.None, player.cosmetics.GetPhantomRoleAlpha());
                        }
                    }
                    else
                    {
                        PlayerMaterial.SetColors(6, hand.handRenderer);
                    }
                }
            })));
        }

        // Save the position of the player prior to starting the climb / gap platform
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.ClimbLadder))]
        public static void prefix()
        {
            AntiTeleport.position = PlayerControl.LocalPlayer.transform.position;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.ClimbLadder))]
        public static void postfix2(PlayerPhysics __instance, Ladder source, byte climbLadderSid)
        {
            // Fix camo:
            var player = __instance.myPlayer;
            __instance.StartCoroutine(Effects.Lerp(5.0f, new System.Action<float>((p) => {
                if (Camouflager.camouflageTimer <= 0 && !Helpers.MushroomSabotageActive())
                {
                    foreach (var morphling in Morphling.players)  {
                        if (player == morphling.player && morphling.morphTimer > 0.1f) {
                            player.RawSetHat(morphling.morphTarget.Data.DefaultOutfit.HatId, morphling.morphTarget.Data.DefaultOutfit.ColorId);
                        }
                    }
                    if (player.isRole(RoleId.MimicK) && MimicK.victim != null && MimicK.victim.Data != null)
                        player.RawSetHat(MimicK.victim.Data.DefaultOutfit.HatId, MimicK.victim.Data.DefaultOutfit.ColorId);
                    else if (player.isRole(RoleId.MimicA) && MimicA.isMorph) {
                        var mimicK = MimicK.allPlayers.FirstOrDefault();
                        player.RawSetHat(mimicK.Data.DefaultOutfit.HatId, mimicK.Data.DefaultOutfit.ColorId);
                    }
                }
            })));
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MovingPlatformBehaviour), nameof(MovingPlatformBehaviour.UsePlatform))]
        public static void prefix2()
        {
            AntiTeleport.position = PlayerControl.LocalPlayer.transform.position;
        }
    }
}
