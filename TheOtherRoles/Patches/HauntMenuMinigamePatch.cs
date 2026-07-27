using HarmonyLib;
using AmongUs.GameOptions;
using System.Collections.Generic;
using System.Linq;
using System;
using TheOtherRoles.CustomGameModes;
using TheOtherRoles.Roles;

namespace TheOtherRoles.Patches {
    [HarmonyPatch]
    public static class HauntMenuMinigamePatch {

        // Show the role name instead of just Crewmate / Impostor
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HauntMenuMinigame), nameof(HauntMenuMinigame.SetHauntTarget))]
        public static void Postfix(HauntMenuMinigame __instance) {
            if (GameOptionsManager.Instance.currentGameOptions.GameMode != GameModes.Normal) return;
            var target = __instance.HauntTarget;
            var roleInfo = RoleInfo.getRoleInfoForPlayer(target, false, includeHidden: true);
            string roleString = (roleInfo.Count > 0 && ClientOption.GetValue(ClientOption.ClientOptionType.SpoilerAfterDeath) == 1) ? ((Madmate.madmate.Any(x => x.PlayerId == target.PlayerId) || CreatedMadmate.createdMadmate.Any(x => x.PlayerId == target.PlayerId)) ? (roleInfo.Contains(RoleInfo.crewmate) ? Madmate.fullName : Madmate.prefix + roleInfo[0].name) : roleInfo[0].name) : "";
            if (__instance.HauntTarget.Data.IsDead) {
                __instance.FilterText.text = string.Format(ModTranslation.getString("hauntMenuGhost"), roleString);
                return;
            }
            __instance.FilterText.text = roleString;
            return;
        }

        // The impostor filter now includes neutral roles
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HauntMenuMinigame), nameof(HauntMenuMinigame.MatchesFilter))]
        public static void MatchesFilterPostfix(HauntMenuMinigame __instance, PlayerControl pc, ref bool __result) {
            if (GameOptionsManager.Instance.currentGameOptions.GameMode != GameModes.Normal) return;
            if (__instance.filterMode == HauntMenuMinigame.HauntFilters.Impostor) {
                var info = RoleInfo.getRoleInfoForPlayer(pc, false, true);
                __result = (pc.Data.Role.IsImpostor || info.Any(x => x.isNeutral)) && !pc.Data.IsDead;
            }
        }


        // Shows the "haunt evil roles button"
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HauntMenuMinigame), nameof(HauntMenuMinigame.Start))]
        public static bool StartPrefix(HauntMenuMinigame __instance) {
            if (GameOptionsManager.Instance.currentGameOptions.GameMode != GameModes.Normal || ClientOption.GetValue(ClientOption.ClientOptionType.SpoilerAfterDeath) == 0) return true;
            __instance.FilterButtons[0].gameObject.SetActive(true);
            int numActive = 0;
            int numButtons = __instance.FilterButtons.Count((PassiveButton s) => s.isActiveAndEnabled);
            float edgeDist = 0.6f * (float)numButtons;
		    for (int i = 0; i< __instance.FilterButtons.Length; i++)
		    {
			    PassiveButton passiveButton = __instance.FilterButtons[i];
			    if (passiveButton.isActiveAndEnabled)
			    {
				    passiveButton.transform.SetLocalX(FloatRange.SpreadToEdges(-edgeDist, edgeDist, numActive, numButtons));
				    numActive++;
			    }
            }
            return false;
        }

        // Moves the haunt menu a bit further down
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HauntMenuMinigame), nameof(HauntMenuMinigame.FixedUpdate))]
        public static void UpdatePostfix(HauntMenuMinigame __instance) {
            if (GameOptionsManager.Instance.currentGameOptions.GameMode != GameModes.Normal) return;
            if (PlayerControl.LocalPlayer.Data.Role.IsImpostor && !PlayerControl.LocalPlayer.isRole(RoleId.Vampire))
                __instance.gameObject.transform.localPosition = new UnityEngine.Vector3(-6f, -1.1f, __instance.gameObject.transform.localPosition.z);
            return;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AbilityButton), nameof(AbilityButton.Update))]
        public static void showOrHideAbilityButtonPostfix(AbilityButton __instance) {
            bool isPropHunt = GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek;
            if (!RoleManager.IsGhostRole(PlayerControl.LocalPlayer.Data.RoleType)) return;
            if (PlayerControl.LocalPlayer.Data.IsDead && (CustomOptionHolder.finishTasksBeforeHauntingOrZoomingOut.getBool() || isPropHunt)) {
                // player has haunt button.
                var (playerCompleted, playerTotal) = TasksHandler.taskInfo(PlayerControl.LocalPlayer.Data);
                int numberOfLeftTasks = playerTotal - playerCompleted;
                if (numberOfLeftTasks <= 0 || isPropHunt)
                    __instance.Show();
                else
                    __instance.Hide();
            }
        }
    }
}
