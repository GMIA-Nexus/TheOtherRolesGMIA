using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using Il2CppSystem.Data;
using PowerTools;
using TheOtherRoles.Objects;
using TheOtherRoles.Utilities;
using TMPro;
using UnityEngine;

namespace TheOtherRoles.Patches
{
    [HarmonyPatch]
    public class SpawnInMinigamePatch
    {
        public static List<SpawnCandidate> SpawnCandidates;
        public static void reset()
        {
            resetSpawnCandidates();
        }
        public static void resetSpawnCandidates()
        {
            SpawnCandidates = new List<SpawnCandidate>();
            if (CustomOptionHolder.airshipAdditionalSpawn.getBool())
            {
                SpawnCandidates.Add(new SpawnCandidate(StringNames.VaultRoom, new Vector2(-8.8f, 8.6f), "TheOtherRoles.Resources.Locations.VaultButton.png", "rollover_brig"));
                SpawnCandidates.Add(new SpawnCandidate(StringNames.MeetingRoom, new Vector2(11.0f, 14.7f), "TheOtherRoles.Resources.Locations.MeetingButton.png", "rollover_brig"));
                SpawnCandidates.Add(new SpawnCandidate(StringNames.Cockpit, new Vector2(-22.0f, -1.2f), "TheOtherRoles.Resources.Locations.CockpitButton.png", "rollover_brig"));
                SpawnCandidates.Add(new SpawnCandidate(StringNames.Electrical, new Vector2(16.4f, -8.5f), "TheOtherRoles.Resources.Locations.ElectricalButton.png", "rollover_brig"));
                SpawnCandidates.Add(new SpawnCandidate(StringNames.Lounge, new Vector2(30.9f, 7.5f), "TheOtherRoles.Resources.Locations.LoungeButton.png", "rollover_brig"));
                SpawnCandidates.Add(new SpawnCandidate(StringNames.Medical, new Vector2(25.5f, -5.0f), "TheOtherRoles.Resources.Locations.MedicalButton.png", "rollover_brig"));
                SpawnCandidates.Add(new SpawnCandidate(StringNames.Security, new Vector2(10.3f, -16.2f), "TheOtherRoles.Resources.Locations.SecurityButton.png", "rollover_brig"));
                SpawnCandidates.Add(new SpawnCandidate(StringNames.ViewingDeck, new Vector2(-14.1f, -16.2f), "TheOtherRoles.Resources.Locations.ViewingButton.png", "rollover_brig"));
                SpawnCandidates.Add(new SpawnCandidate(StringNames.Armory, new Vector2(-10.7f, -6.3f), "TheOtherRoles.Resources.Locations.ArmoryButton.png", "rollover_brig"));
                SpawnCandidates.Add(new SpawnCandidate(StringNames.Comms, new Vector2(-11.8f, 3.2f), "TheOtherRoles.Resources.Locations.CommunicationsButton.png", "rollover_brig"));
                SpawnCandidates.Add(new SpawnCandidate(StringNames.Showers, new Vector2(20.8f, 2.8f), "TheOtherRoles.Resources.Locations.ShowersButton.png", "rollover_brig"));
                SpawnCandidates.Add(new SpawnCandidate(StringNames.GapRoom, new Vector2(13.8f, 6.4f), "TheOtherRoles.Resources.Locations.GapButton.png", "rollover_brig"));
                foreach (var spawnCandidate in SpawnCandidates)
                {
                    spawnCandidate.ReloadTexture();
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SpawnInMinigame), nameof(SpawnInMinigame.Begin))]
        public static bool Prefix(SpawnInMinigame __instance, PlayerTask task)
        {
            __instance.MyTask = task;
            __instance.MyNormTask = task as NormalPlayerTask;
            if (PlayerControl.LocalPlayer)
            {
                if (MapBehaviour.Instance)
                {
                    MapBehaviour.Instance.Close();
                }
                PlayerControl.LocalPlayer.NetTransform.Halt();
            }
            __instance.StartCoroutine(__instance.CoAnimateOpen());


            List<SpawnInMinigame.SpawnLocation> list = __instance.Locations.ToList<SpawnInMinigame.SpawnLocation>();
            foreach (var spawnCandidate in SpawnCandidates)
            {
                SpawnInMinigame.SpawnLocation spawnlocation = new()
                {
                    Location = spawnCandidate.SpawnLocation,
                    Image = spawnCandidate.GetSprite(),
                    Name = spawnCandidate.LocationKey,
                    Rollover = new AnimationClip(),
                    RolloverSfx = __instance.DefaultRolloverSound
                };
                list.Add(spawnlocation);
            }

            SpawnInMinigame.SpawnLocation[] array = list.ToArray<SpawnInMinigame.SpawnLocation>();
            array.Shuffle(0);
            array = (from s in array.Take(__instance.LocationButtons.Length)
                     orderby s.Location.x, s.Location.y descending
                     select s).ToArray<SpawnInMinigame.SpawnLocation>();
            PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(new Vector2(-25f, 40f));

            for (int i = 0; i < __instance.LocationButtons.Length; i++)
            {
                PassiveButton passiveButton = __instance.LocationButtons[i];
                SpawnInMinigame.SpawnLocation pt = array[i];
                passiveButton.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => SpawnAt(__instance, pt.Location)));
                passiveButton.GetComponent<SpriteAnim>().Stop();
                passiveButton.GetComponent<SpriteRenderer>().sprite = pt.Image;
                // passiveButton.GetComponentInChildren<TextMeshPro>().text = FastDestroyableSingleton<TranslationController>.Instance.GetString(pt.Name, Array.Empty<object>());
                passiveButton.GetComponentInChildren<TextMeshPro>().text = FastDestroyableSingleton<TranslationController>.Instance.GetString(pt.Name, new Il2CppReferenceArray<Il2CppSystem.Object>(0));
                ButtonAnimRolloverHandler component = passiveButton.GetComponent<ButtonAnimRolloverHandler>();
                component.StaticOutImage = pt.Image;
                component.RolloverAnim = pt.Rollover;
                component.HoverSound = pt.RolloverSfx ? pt.RolloverSfx : __instance.DefaultRolloverSound;
            }


            PlayerControl.LocalPlayer.gameObject.SetActive(false);
            PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(new Vector2(-25f, 40f));
            __instance.StartCoroutine(__instance.RunTimer());
            ControllerManager.Instance.OpenOverlayMenu(__instance.name, null, __instance.DefaultButtonSelected, __instance.ControllerSelectable, false);
            PlayerControl.HideCursorTemporarily();
            ConsoleJoystick.SetMode_Menu();
            return false;
        }

        public static void SpawnAt(SpawnInMinigame __instance, Vector3 spawnAt)
        {
            if (__instance.amClosing != Minigame.CloseState.None)
            {
                return;
            }
            __instance.gotButton = true;
            PlayerControl.LocalPlayer.gameObject.SetActive(true);
            __instance.StopAllCoroutines();
            PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(spawnAt);
            FastDestroyableSingleton<HudManager>.Instance.PlayerCam.SnapToTarget();
            __instance.Close();
        }
    }
}
