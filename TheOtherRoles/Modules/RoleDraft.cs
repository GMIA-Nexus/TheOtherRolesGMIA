using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using Reactor.Utilities.Extensions;
using TheOtherRoles.Patches;
using TheOtherRoles.Roles;
using UnityEngine;
using UnityEngine.UI;

namespace TheOtherRoles.Modules
{
    [HarmonyPatch]
    class RoleDraft
    {
        public static bool isEnabled => CustomOptionHolder.isDraftMode.getBool() && (TORMapOptions.gameMode == CustomGamemodes.Classic || TORMapOptions.gameMode == CustomGamemodes.Guesser);
        public static bool isRunning = false;

        public static List<byte> pickOrder = new();
        public static bool picked = false;
        public static float timer = 0f;
        private static List<ActionButton> buttons = new();
        private static TMPro.TextMeshPro feedText;
        private static Scroller scroller;
        public static List<(byte, bool)> alreadyPicked = new();
        public static IEnumerator CoSelectRoles(IntroCutscene __instance)
        {
            if (!isEnabled) yield break;

            isRunning = true;
            SoundEffectsManager.play("GMIATheme", volume: 1f, true, true);
            alreadyPicked.Clear();
            bool playedAlert = false;
            feedText = UnityEngine.Object.Instantiate(__instance.TeamTitle, __instance.transform);
            var aspectPosition = feedText.gameObject.AddComponent<AspectPosition>();
            aspectPosition.Alignment = AspectPosition.EdgeAlignments.LeftTop;
            aspectPosition.DistanceFromEdge = new Vector2(1.62f, 1.2f);
            aspectPosition.AdjustPosition();
            feedText.transform.localScale = new Vector3(0.6f, 0.6f, 1);
            feedText.text = $"<size=200%>{ModTranslation.getString("roleDraftPicks")}</size>\n\n";
            feedText.alignment = TMPro.TextAlignmentOptions.TopLeft;
            feedText.autoSizeTextContainer = true;
            feedText.fontSize = 3f;
            feedText.enableAutoSizing = false;
            scroller = feedText.gameObject.AddComponent<Scroller>();
            scroller.Inner = feedText.transform;
            scroller.allowY = true;
            scroller.gameObject.SetActive(true);
            scroller.enabled = true;
            scroller.ContentYBounds = new FloatRange(2f, 2f);
            scroller.ScrollToTop();
            __instance.TeamTitle.transform.localPosition = __instance.TeamTitle.transform.localPosition + new Vector3(1f, 0f);
            __instance.TeamTitle.text = ModTranslation.getString("roleDraftCurrent");
            __instance.BackgroundBar.enabled = false;
            __instance.TeamTitle.transform.localScale = new Vector3(0.25f, 0.25f, 1f);
            __instance.TeamTitle.autoSizeTextContainer = true;
            __instance.TeamTitle.enableAutoSizing = false;
            __instance.TeamTitle.fontSize = 5;
            __instance.TeamTitle.alignment = TMPro.TextAlignmentOptions.Top;
            __instance.ImpostorText.gameObject.SetActive(false);
            GameObject.Find("BackgroundLayer")?.SetActive(false);
            foreach (var player in UnityEngine.Object.FindObjectsOfType<PoolablePlayer>())
            {
                if (player.name.Contains("Dummy"))
                {
                    player.gameObject.SetActive(false);
                }
            }
            __instance.FrontMost.gameObject.SetActive(false);

            if (AmongUsClient.Instance.AmHost)
            {
                sendPickOrder();
            }

            while (pickOrder.Count == 0)
            {
                yield return null;
            }

            int impostorCount = PlayerControl.AllPlayerControls.ToArray().ToList().Where(x => x.Data.Role.IsImpostor).Count();
            int allPlayerCount = PlayerControl.AllPlayerControls.Count;
            RoleManagerSelectRolesPatch.RoleAssignmentData roleData = RoleManagerSelectRolesPatch.getRoleAssignmentData();
            roleData.crewSettings.Add((byte)RoleId.Sheriff, CustomOptionHolder.sheriffSpawnRate.data);
            if (CustomOptionHolder.sheriffSpawnRate.getSelection() > 0)
                roleData.crewSettings.Add((byte)RoleId.Deputy, (CustomOptionHolder.deputySpawnRate.getSelection(), (int)CustomOptionHolder.deputyRoleCount.getFloat()));

            // Assign paired roles
            if (impostorCount >= 2)
            {
                roleData.impSettings.Add((byte)RoleId.MimicA, CustomOptionHolder.mimicSpawnRate.data);
                roleData.impSettings.Add((byte)RoleId.MimicK, CustomOptionHolder.mimicSpawnRate.data);
                roleData.impSettings.Add((byte)RoleId.BomberA, CustomOptionHolder.bomberSpawnRate.data);
                roleData.impSettings.Add((byte)RoleId.BomberB, CustomOptionHolder.bomberSpawnRate.data);
            }
            if (impostorCount >= 3)
            {
                roleData.impSettings.Add((byte)RoleId.Godfather, CustomOptionHolder.mafiaSpawnRate.data);
                roleData.impSettings.Add((byte)RoleId.Janitor, CustomOptionHolder.mafiaSpawnRate.data);
                roleData.impSettings.Add((byte)RoleId.Mafioso, CustomOptionHolder.mafiaSpawnRate.data);
            }

            // Swapper, Yasuna, Guesser
            roleData.crewSettings.Add((byte)RoleId.Swapper, (Mathf.CeilToInt((10 - CustomOptionHolder.swapperIsImpRate.getSelection()) * CustomOptionHolder.swapperSpawnRate.getSelection() / 10f), 1));
            roleData.impSettings.Add((byte)RoleId.Swapper, (Mathf.CeilToInt(CustomOptionHolder.swapperIsImpRate.getSelection() * CustomOptionHolder.swapperSpawnRate.getSelection() / 10f), 1));

            roleData.crewSettings.Add((byte)RoleId.Yasuna, (Mathf.CeilToInt((10 - CustomOptionHolder.yasunaIsImpYasunaRate.getSelection()) * CustomOptionHolder.yasunaSpawnRate.getSelection() / 10f), 1));
            roleData.impSettings.Add((byte)RoleId.EvilYasuna, (Mathf.CeilToInt(CustomOptionHolder.yasunaIsImpYasunaRate.getSelection() * CustomOptionHolder.yasunaSpawnRate.getSelection() / 10f), 1));

            if (TORMapOptions.gameMode != CustomGamemodes.Guesser)
            {
                roleData.crewSettings.Add((byte)RoleId.NiceGuesser, (Mathf.CeilToInt((CustomOptionHolder.guesserSpawnBothRate.getSelection() > 0 ? 10 :
                    10 - CustomOptionHolder.guesserIsImpGuesserRate.getSelection()) * CustomOptionHolder.guesserSpawnRate.getSelection() / 10f), 1));
                roleData.impSettings.Add((byte)RoleId.EvilGuesser, (Mathf.CeilToInt((CustomOptionHolder.guesserSpawnBothRate.getSelection() > 0 ? 10 : CustomOptionHolder.guesserIsImpGuesserRate.getSelection())
                    * CustomOptionHolder.guesserSpawnRate.getSelection() / 10f), 1));
            }

            bool assignWatcherEqually = CustomOptionHolder.watcherAssignEqually.getSelection() == 0;
            int niceWatcherCount = Mathf.CeilToInt(CustomOptionHolder.watcherSpawnRate.count / 2f);
            roleData.crewSettings.Add((byte)RoleId.NiceWatcher, assignWatcherEqually ? (CustomOptionHolder.watcherSpawnRate.getSelection(), niceWatcherCount) :
                (Mathf.CeilToInt(CustomOptionHolder.watcherSpawnRate.getSelection() * (10 - CustomOptionHolder.watcherIsImpWatcherRate.getSelection()) / 10f), CustomOptionHolder.watcherSpawnRate.count));
            roleData.impSettings.Add((byte)RoleId.EvilWatcher, assignWatcherEqually ? (CustomOptionHolder.watcherSpawnRate.getSelection(), CustomOptionHolder.watcherSpawnRate.count - niceWatcherCount) :
                (Mathf.CeilToInt(CustomOptionHolder.watcherSpawnRate.getSelection() * CustomOptionHolder.watcherIsImpWatcherRate.getSelection() / 10f), CustomOptionHolder.watcherSpawnRate.count));
            
            roleData.crewSettings.Add((byte)RoleId.Shifter, (Mathf.CeilToInt((10 - CustomOptionHolder.shifterIsNeutralRate.getSelection()) * CustomOptionHolder.shifterSpawnRate.getSelection() / 10f), 1));
            roleData.neutralSettings.Add((byte)RoleId.Shifter, (Mathf.CeilToInt(CustomOptionHolder.shifterIsNeutralRate.getSelection() * CustomOptionHolder.shifterSpawnRate.getSelection() / 10f), 1));

            while (pickOrder.Count > 0) {
                picked = false;
                timer = 0;
                float maxTimer = CustomOptionHolder.draftModeTimeToChoose.getFloat();
                string playerText = "";
                while (timer < maxTimer || !picked) {
                    if (pickOrder.Count == 0)
                        break;

                    // wait for pick
                    timer += Time.deltaTime;
                    if (PlayerControl.LocalPlayer.PlayerId == pickOrder[0]) {
                        if (!playedAlert) {
                            playedAlert = true;
                            SoundManager.Instance.PlaySound(ShipStatus.Instance.SabotageSound, false, 1f, null);
                        }
                        // Animate beginning of choice, by changing background color
                        float min = 50 / 255f;
                        Color backGroundColor = new(min, min, min, 1);
                        if (timer < 1) {
                            float max = 230 / 255f;
                            if (timer < 0.5f) { // White flash                              
                                float p = timer / 0.5f;
                                float value = (float)Math.Pow(p, 2f) * max;
                                backGroundColor = new Color(value, value, value, 1);
                            } else {
                                float p = (1 - timer) / 0.5f;
                                float value = (float)Math.Pow(p, 2f) * max + (1 - (float)Math.Pow(p, 2f)) * min;
                                backGroundColor = new Color(value, value, value, 1);
                            }

                        }
                        HudManager.Instance.FullScreen.color = backGroundColor;
                        GameObject.Find("BackgroundLayer")?.SetActive(false);

                        // enable pick, wait for pick
                        Color youColor = timer - (int)timer > 0.5 ? Color.red : Color.yellow;
                        playerText = Helpers.cs(youColor, ModTranslation.getString("roleDraftYou") + "!");
                        // Available Roles:
                        List<RoleInfo> availableRoles = new();
                        foreach (RoleInfo roleInfo in RoleInfo.allRoleInfos) {
                            if (roleInfo.isModifier) continue;

                            // Remove Impostor Roles
                            if (PlayerControl.LocalPlayer.Data.Role.IsImpostor && !roleInfo.isImpostor) continue;
                            if (!PlayerControl.LocalPlayer.Data.Role.IsImpostor && roleInfo.isImpostor) continue;

                            if (roleInfo.isNeutral && roleData.neutralSettings.ContainsKey((byte)roleInfo.roleId) && roleData.neutralSettings[(byte)roleInfo.roleId].rate == 0) continue;
                            else if (roleInfo.isImpostor && roleData.impSettings.ContainsKey((byte)roleInfo.roleId) && roleData.impSettings[(byte)roleInfo.roleId].rate == 0) continue;
                            else if (!roleInfo.isImpostor && !roleInfo.isNeutral && roleData.crewSettings.ContainsKey((byte)roleInfo.roleId) && roleData.crewSettings[(byte)roleInfo.roleId].rate == 0) continue;
                            else if (roleInfo.roleId == RoleId.Sidekick) continue;
                            else if (roleInfo.roleId == RoleId.Immoralist) continue;
                            if (roleInfo.roleId == RoleId.Deputy && (!Sheriff.exists || Sheriff.players.Count <= Deputy.players.Count)) continue;
                            if (roleInfo.roleId == RoleId.Pursuer) continue;
                            if (roleInfo.roleId == RoleId.Spy && impostorCount < 2) continue;
                            if (roleInfo.roleId == RoleId.Yasuna && alreadyPicked.Any(x => x.Item1 == (byte)RoleId.EvilYasuna)) continue;
                            if (roleInfo.roleId == RoleId.EvilYasuna && alreadyPicked.Any(x => x.Item1 == (byte)RoleId.Yasuna)) continue;
                            if (TORMapOptions.gameMode == CustomGamemodes.Guesser && (roleInfo.roleId == RoleId.EvilGuesser || roleInfo.roleId == RoleId.NiceGuesser)) continue;
                            if ((roleInfo.roleId == RoleId.NiceWatcher || roleInfo.roleId == RoleId.EvilWatcher) && alreadyPicked.Where(x => x.Item1 is ((byte)RoleId.NiceWatcher) or ((byte)RoleId.EvilWatcher)).ToList().Count
                                >= CustomOptionHolder.watcherSpawnRate.count) continue;
                            if (alreadyPicked.Any(x => x.Item1 == (byte)roleInfo.roleId) && roleInfo.roleId != RoleId.Crewmate) {
                                var list = roleData.crewSettings;
                                if (roleData.impSettings.ContainsKey((byte)roleInfo.roleId)) list = roleData.impSettings;
                                else if (roleData.neutralSettings.ContainsKey((byte)roleInfo.roleId)) list = roleData.neutralSettings;
                                if (list.ContainsKey((byte)roleInfo.roleId) && list[(byte)roleInfo.roleId].count <= alreadyPicked.Where(x => x.Item1 == (byte)roleInfo.roleId).ToList().Count)
                                    continue;
                            }
                            if (CustomOptionHolder.crewmateRolesFill.getBool() && roleInfo.roleId == RoleId.Crewmate) continue;

                            int impsPicked = alreadyPicked.Where(x => RoleInfo.roleInfoById[((RoleId)x.Item1, x.Item2)].isImpostor).Count();
                            if (roleInfo.roleId is RoleId.BomberA or RoleId.BomberB or RoleId.MimicA or RoleId.MimicK) {
                                if (impostorCount - impsPicked < 2) continue;
                                if (roleInfo.roleId == RoleId.BomberB && !BomberA.exists) continue;
                            }
                            else if (roleInfo.roleId is RoleId.Godfather or RoleId.Mafioso or RoleId.Janitor) {
                                if (impostorCount - impsPicked < 3) continue;
                            }

                            // Hanlde forcing of 100% roles for impostors
                            if (PlayerControl.LocalPlayer.Data.Role.IsImpostor) {
                                int impsMax = CustomOptionHolder.impostorRolesCountMax.getSelection();
                                int impsMin = CustomOptionHolder.impostorRolesCountMin.getSelection();
                                if (impsMin > impsMax) impsMin = impsMax;
                                int impsLeft = pickOrder.Where(x => Helpers.playerById(x).Data.Role.IsImpostor).Count();
                                int imps100 = roleData.impSettings.Where(x => x.Value.rate == 10).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x).Count();
                                if (imps100 > impsMax) imps100 = impsMax;
                                int imps100Picked = alreadyPicked.Where(x => roleData.impSettings.GetValueSafe(x.Item1).rate == 10).Count();
                                if (imps100 - imps100Picked >= impsLeft && !(roleData.impSettings.Where(x => x.Value.rate == 10 && x.Key == (byte)roleInfo.roleId).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x).Any())) continue;
                                if (impsMin - impsPicked >= impsLeft && roleInfo.roleId == RoleId.Impostor) continue;
                                if (impsPicked >= impsMax && roleInfo.roleId != RoleId.Impostor) continue;
                            }

                            // Player is no impostor! Handle forcing of 100% roles for crew and neutral
                            else {
                                // No more neutrals possible!
                                int neutralsPicked = alreadyPicked.Where(x => RoleInfo.roleInfoById[((RoleId)x.Item1, x.Item2)].isNeutral).Count();
                                int crewPicked = alreadyPicked.Count - impsPicked - neutralsPicked;
                                int neutralsMax = CustomOptionHolder.neutralRolesCountMax.getSelection();
                                int neutralsMin = CustomOptionHolder.neutralRolesCountMin.getSelection();
                                int neutrals100 = roleData.neutralSettings.Where(x => x.Value.rate == 10).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x).Count();
                                if (neutrals100 > neutralsMin) neutralsMin = neutrals100;
                                if (neutralsMin > neutralsMax) neutralsMin = neutralsMax;

                                // If crewmate fill disabled and crew picked the amount of allowed crewmates alreay: no more crewmate except vanilla crewmate allowed!
                                int crewLimit = allPlayerCount - impostorCount - (neutralsMin > neutrals100 ? neutralsMin : neutrals100 > neutralsMax ? neutralsMax : neutrals100);
                                int maxCrew = CustomOptionHolder.crewmateRolesFill.getBool() ? CustomOptionHolder.crewmateRolesCountMax.getSelection() : crewLimit;
                                if (maxCrew > crewLimit)
                                    maxCrew = crewLimit;
                                if (crewPicked >= crewLimit && !roleInfo.isNeutral && roleInfo.roleId != RoleId.Crewmate) continue;
                                // Fill roles means no crewmates allowed!
                                if (CustomOptionHolder.crewmateRolesFill.getBool() && roleInfo.roleId == RoleId.Crewmate) continue;

                                bool allowAnyNeutral = false;
                                if (neutralsPicked >= neutralsMax && roleInfo.isNeutral) continue;
                                // More neutrals needed? Then no more crewmates! This takes precedence over crew roles set to 100%!
                                var crewmatesLeft = pickOrder.Count - pickOrder.Where(x => Helpers.playerById(x).Data.Role.IsImpostor).Count();

                                if (crewmatesLeft <= neutralsMin - neutralsPicked && !roleInfo.isNeutral) {
                                    continue;
                                } else if (neutralsMin - neutrals100 > neutralsPicked)
                                    allowAnyNeutral = true;
                                // Handle 100% Roles PER Faction.

                                int neutrals100Picked = alreadyPicked.Where(x => roleData.neutralSettings.GetValueSafe(x.Item1).rate == 10).Count();
                                if (neutrals100 > neutralsMax) neutrals100 = neutralsMax;

                                int crew100 = roleData.crewSettings.Where(x => x.Value.rate == 10).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x).Count();
                                int crew100Picked = alreadyPicked.Where(x => roleData.crewSettings.GetValueSafe(x.Item1).rate == 10).Count();
                                if (neutrals100 > neutralsMax) neutrals100 = neutralsMax;

                                if (crew100 > maxCrew) crew100 = maxCrew;
                                if ((neutrals100 - neutrals100Picked >= crewmatesLeft || roleInfo.isNeutral && neutrals100 - neutrals100Picked >= neutralsMax - neutralsPicked) && !(neutrals100Picked >= neutralsMax) && !(roleData.neutralSettings.Where(x => x.Value.rate == 10 && x.Key == (byte)roleInfo.roleId).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x).Any())) continue;
                                if (!(allowAnyNeutral && roleInfo.isNeutral) && crew100 - crew100Picked >= crewmatesLeft && !(roleData.crewSettings.Where(x => x.Value.rate == 10 && x.Key == (byte)roleInfo.roleId).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x).Any())) continue;

                                if (!(allowAnyNeutral && roleInfo.isNeutral) && neutrals100 + crew100 - neutrals100Picked - crew100Picked >= crewmatesLeft && !(roleData.crewSettings.Where(x => x.Value.rate == 10 && x.Key == (byte)roleInfo.roleId).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x).Any()
                                    || roleData.neutralSettings.Where(x => x.Value.rate == 10 && x.Key == (byte)roleInfo.roleId).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x).Any())) continue;

                            }
                            // Handle role pairings that are blocked, e.g. Vampire Warlock, Cleaner Vulture etc.
                            bool blocked = false;
                            foreach (var blockedRoleId in CustomOptionHolder.blockedRolePairings) {
                                if (alreadyPicked.Any(x => blockedRoleId.Any(rid => (byte)rid.roleId == x.Item1 && Helpers.isSpecialRoleInfo(rid) == x.Item2)) && blockedRoleId.ToList().Contains(roleInfo)) {
                                    blocked = true;
                                    break;
                                }
                            }
                            if (blocked) continue;


                            availableRoles.Add(roleInfo);
                        }

                        var fixedRoleList = new List<RoleInfo>();
                        if (PlayerControl.LocalPlayer.Data.Role.IsImpostor)
                        {
                            if (alreadyPicked.Any(x => x.Item1 == (byte)RoleId.Godfather) || alreadyPicked.Any(x => x.Item1 == (byte)RoleId.Mafioso)
                                || alreadyPicked.Any(x => x.Item1 == (byte)RoleId.Janitor))
                            {
                                fixedRoleList = new List<RoleInfo>() { RoleInfo.godfather, RoleInfo.mafioso, RoleInfo.janitor };
                                fixedRoleList.RemoveAll(x => alreadyPicked.Any(y => y.Item1 == (byte)x.roleId));
                            }
                            if (alreadyPicked.Any(x => x.Item1 == (byte)RoleId.BomberA) && !alreadyPicked.Any(x => x.Item1 == (byte)RoleId.BomberB)) {
                                fixedRoleList = new List<RoleInfo>() { RoleInfo.bomberB };
                            }
                            if (alreadyPicked.Any(x => x.Item1 == (byte)RoleId.MimicA) || alreadyPicked.Any(x => x.Item1 == (byte)RoleId.MimicK)) {
                                fixedRoleList = new List<RoleInfo>() { RoleInfo.mimicK, RoleInfo.mimicA };
                                fixedRoleList.RemoveAll(x => alreadyPicked.Any(y => y.Item1 == (byte)x.roleId));
                            }

                            if (fixedRoleList.Count > 0) availableRoles = fixedRoleList;
                        }

                        // Fallback for if all roles are somehow removed. (This is only the case if there is a bug, hence print a warning
                        if (availableRoles.Count == 0) {
                            if (PlayerControl.LocalPlayer.Data.Role.IsImpostor)
                                availableRoles.Add(RoleInfo.impostor);
                            else
                                availableRoles.Add(RoleInfo.crewmate);
                            TheOtherRolesPlugin.Logger.LogWarning("Draft Mode: Fallback triggered, because no roles were left. Forced addition of basegame Imp/Crewmate");
                        }

                        List<RoleInfo> originalAvailable = new(availableRoles);

                        // remove some roles, so that you can't always get the same roles:
                        if (availableRoles.Count > CustomOptionHolder.draftModeAmountOfChoices.getFloat()) {
                            int countToRemove = availableRoles.Count - (int)CustomOptionHolder.draftModeAmountOfChoices.getFloat();
                            while (countToRemove-- > 0) {
                                var toRemove = availableRoles.OrderBy(_ => Guid.NewGuid()).First();
                                availableRoles.Remove(toRemove);
                            }
                        }

                        if (timer >= maxTimer) {
                            var randomRole = originalAvailable.OrderBy(_ => Guid.NewGuid()).First();
                            if (randomRole.roleId == RoleId.Shifter)
                            {
                                bool shifterIsNeutral = randomRole == RoleInfo.chainshifter;
                                Shifter.SetType.Invoke(shifterIsNeutral);
                            }
                            sendPick((byte)randomRole.roleId, originalAvailable.Count > 1, randomRole == RoleInfo.niceshifter || randomRole == RoleInfo.niceSwapper);
                        }


                        if (GameObject.Find("RoleButton") == null) {
                            SoundEffectsManager.play("timemasterShield");
                            int i = 0;
                            int buttonsPerRow = 4;
                            int count = fixedRoleList.Count == 0 && availableRoles.Count > 1 ? availableRoles.Count + 1 : availableRoles.Count;
                            int lastRow = count / buttonsPerRow;
                            int buttonsInLastRow = count % buttonsPerRow;

                            ActionButton createButton(string textToDisplay, Color color, float row, float col)
                            {
                                if (buttonsInLastRow != 0 && row == lastRow) {
                                    col += (buttonsPerRow - buttonsInLastRow) / 2f;
                                }
                                // planned rows: maximum of 4, hence the following calculation for rows as well:
                                row += (4 - lastRow - 1) / 2f;

                                ActionButton actionButton = UnityEngine.Object.Instantiate(HudManager.Instance.KillButton, __instance.TeamTitle.transform);
                                actionButton.gameObject.SetActive(true);
                                actionButton.gameObject.name = "RoleButton";
                                actionButton.transform.localPosition = new Vector3(-14.4f + col * 8f, -9f - row * 3f);
                                actionButton.transform.localScale = new Vector3(2f, 2f);
                                actionButton.SetCoolDown(0, 0);
                                GameObject textHolder = new("textHolder");
                                var text = textHolder.AddComponent<TMPro.TextMeshPro>();
                                text.text = textToDisplay.Replace(" ", "\n");
                                text.horizontalAlignment = TMPro.HorizontalAlignmentOptions.Center;
                                text.fontSize = 5;
                                textHolder.layer = actionButton.gameObject.layer;
                                text.color = color;
                                textHolder.transform.SetParent(actionButton.transform, false);
                                textHolder.transform.localPosition = new Vector3(0, text.text.Contains("\n") ? -1.975f : -2.2f, -1);

                                HudManager.Instance.StartCoroutine(Effects.Lerp(0.5f, new Action<float>((p) => {
                                    actionButton.OverrideText("");
                                })));
                                buttons.Add(actionButton);

                                return actionButton;
                            }

                            foreach (RoleInfo roleInfo in availableRoles) {
                                float row = i / buttonsPerRow;
                                float col = i % buttonsPerRow;

                                var actionButton = createButton(roleInfo.name.Replace(" ", "\n"), roleInfo.color, row, col);

                                PassiveButton button = actionButton.GetComponent<PassiveButton>();
                                button.OnClick = new Button.ButtonClickedEvent();
                                button.OnClick.AddListener((Action)(() => {
                                    if (roleInfo.roleId == RoleId.Shifter)
                                    {
                                        bool shifterIsNeutral = roleInfo == RoleInfo.chainshifter;
                                        Shifter.SetType.Invoke(shifterIsNeutral);
                                    }
                                    sendPick((byte)roleInfo.roleId, isSpecialRole: roleInfo == RoleInfo.niceshifter || roleInfo == RoleInfo.niceSwapper);
                                }));
                                i++;
                            }

                            // Create Random button
                            if (fixedRoleList.Count == 0 && availableRoles.Count > 1)
                            {
                                float row = i / buttonsPerRow;
                                float col = i % buttonsPerRow;

                                var actionButton = createButton(ModTranslation.getString("roleDraftRandom"), Color.green, row, col);
                                PassiveButton button = actionButton.GetComponent<PassiveButton>();
                                button.OnClick = new Button.ButtonClickedEvent();
                                button.OnClick.AddListener((Action)(() => {
                                    var randomRole = originalAvailable.OrderBy(_ => Guid.NewGuid()).First();
                                    if (randomRole.roleId == RoleId.Shifter)
                                    {
                                        bool shifterIsNeutral = randomRole == RoleInfo.chainshifter;
                                        Shifter.SetType.Invoke(shifterIsNeutral);
                                    }
                                    sendPick((byte)randomRole.roleId, true, randomRole == RoleInfo.niceshifter || randomRole == RoleInfo.niceSwapper);
                                }));
                            }
                        }

                    } else {
                        int currentPick = alreadyPicked.Count + 1;
                        playerText = string.Format(ModTranslation.getString("roleDraftAnonymousPlayer"), currentPick);
                        HudManager.Instance.FullScreen.color = Color.black;
                    }
                    __instance.TeamTitle.text = $"{Helpers.cs(Color.white, $"<size=280%>{ModTranslation.getString("roleDraftWelcome")}</size>")}\n\n\n<size=200%> {ModTranslation.getString("roleDraftCurrent")}</size>\n\n\n<size=250%>{playerText}</size>";
                    int waitMore = pickOrder.IndexOf(PlayerControl.LocalPlayer.PlayerId);
                    string waitMoreText = "";
                    if (waitMore > 0) {
                        waitMoreText = $" ({string.Format(ModTranslation.getString("roleDraftTurns"), waitMore)})";
                    }
                    __instance.TeamTitle.text += $"\n\n{waitMoreText}\n{string.Format(ModTranslation.getString("roleDraftRandomSelection"), (int)(maxTimer + 1 - timer))}\n {(SoundManager.MusicVolume > -80 ? ModTranslation.getString("roleDraftMusic") : "")}";
                    yield return null;
                }
            }
            HudManager.Instance.FullScreen.color = Color.black;
            __instance.FrontMost.gameObject.SetActive(true);
            GameObject.Find("BackgroundLayer")?.SetActive(true);
            scroller.enabled = false;
            if (AmongUsClient.Instance.AmHost)
            {
                RoleManagerSelectRolesPatch.assignRoleTargets(null); // Assign targets for Lawyer & Prosecutor
                if (RoleManagerSelectRolesPatch.isGuesserGamemode) RoleManagerSelectRolesPatch.assignGuesserGamemode();
                RoleManagerSelectRolesPatch.assignModifiers(); // Assign modifier

                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.FinishShipStatusBegin, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.finishShipStatusBegin();
            }

            float myTimer = 0f;
            while (myTimer < 3f)
            {
                myTimer += Time.deltaTime;
                Color c = new(0, 0, 0, myTimer / 3.0f);
                __instance.FrontMost.color = c;
                yield return null;
            }

            SoundEffectsManager.stop("GMIATheme");
            isRunning = false;
            yield break;
        }

        public static void receivePick(byte playerId, byte roleId, bool isRandom, bool isSpecialRole)
        {
            if (!isEnabled) return;
            RPCProcedure.setRole(roleId, playerId);
            alreadyPicked.Add((roleId, isSpecialRole));
            try
            {
                pickOrder.Remove(playerId);
                timer = 0;
                picked = true;                
                RoleInfo roleInfo = RoleInfo.allRoleInfos.First(x => (byte)x.roleId == roleId);
                if (isSpecialRole)
                {
                    if ((RoleId)roleId == RoleId.Swapper) roleInfo = RoleInfo.niceSwapper;
                    else if ((RoleId)roleId == RoleId.Shifter) roleInfo = RoleInfo.niceshifter;
                }
                string roleString = Helpers.cs(roleInfo.color, roleInfo.name);
                int roleLength = roleInfo.name.Length;  // Not used for now, but stores the amount of charactes of the roleString.
                bool localIsPlayer = playerId == PlayerControl.LocalPlayer.PlayerId;
                string suffix = localIsPlayer ? $" ({Helpers.cs(roleInfo.color, roleInfo.name)})" : "";
                if (!CustomOptionHolder.draftModeShowRoles.getBool()) {
                    roleString = ModTranslation.getString("roleDraftUnknown");
                    roleLength = roleString.Length;
                }                   
                else if (CustomOptionHolder.draftModeHideImpRoles.getBool() && roleInfo.isImpostor) {
                    roleString = Helpers.cs(Palette.ImpostorRed, ModTranslation.getString("roleDraftImpostor")) + suffix;
                    roleLength = "Impostor Role".Length;
                }                    
                else if (CustomOptionHolder.draftModeHideNeutralRoles.getBool() && roleInfo.isNeutral) {
                    roleString = Helpers.cs(Palette.Blue, ModTranslation.getString("roleDraftNeutral")) + suffix;
                    roleLength = "Neutral Role".Length;
                }
                else if (CustomOptionHolder.draftModeHideCrewRoles.getBool() && !roleInfo.isImpostor && !roleInfo.isNeutral) {
                    roleString = Helpers.cs(Color.white, ModTranslation.getString("roleDraftCrewmate")) + suffix;
                    roleLength = "Crewmate Role".Length;
                }
                roleString = isRandom ? Helpers.cs(Color.green, ModTranslation.getString("roleDraftRandom")) + suffix : roleString;
                string line = $"{(localIsPlayer ? ModTranslation.getString("roleDraftYou") : alreadyPicked.Count)}:";
                int spaceCount = Math.Max(0, 6 - line.Length);
                line += string.Concat(Enumerable.Repeat(" ", spaceCount)) + roleString;
                feedText.text += line + "\n";
                scroller.ContentYBounds = new FloatRange(2f, 2f + feedText.text.Count(c => c == '\n') * 0.06f);
                SoundEffectsManager.play("select");
            }
            catch (Exception e) { TheOtherRolesPlugin.Logger.LogError(e); }
        }

        public static void sendPick(byte RoleId, bool isRandom = false, bool isSpecialRole = false)
        {
            SoundEffectsManager.stop("timeMasterShield");
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.DraftModePick, SendOption.Reliable, -1);
            writer.Write(PlayerControl.LocalPlayer.PlayerId);
            writer.Write(RoleId);
            writer.Write(isRandom);
            writer.Write(isSpecialRole);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            receivePick(PlayerControl.LocalPlayer.PlayerId, RoleId, isRandom, isSpecialRole);

            // destroy all the buttons:
            foreach (var button in buttons)
            {
                if (button != null && button.gameObject != null)
                    button?.gameObject?.Destroy();
            }
            buttons.Clear();
        }


        public static void sendPickOrder()
        {
            pickOrder = PlayerControl.AllPlayerControls.ToArray().Select(x => x.PlayerId).OrderBy(_ => Guid.NewGuid()).ToList().ToList();
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.DraftModePickOrder, SendOption.Reliable, -1);
            writer.Write((byte)pickOrder.Count);
            foreach (var item in pickOrder)
            {
                writer.Write(item);
            }
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }


        public static void receivePickOrder(int amount, MessageReader reader)
        {
            pickOrder.Clear();
            for (int i = 0; i < amount; i++)
            {
                pickOrder.Add(reader.ReadByte());
            }
        }
    }
}
