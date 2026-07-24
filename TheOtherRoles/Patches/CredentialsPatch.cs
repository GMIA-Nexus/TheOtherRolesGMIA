using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AmongUs.GameOptions;
using HarmonyLib;
using TheOtherRoles;
using TheOtherRoles.CustomGameModes;
using TheOtherRoles.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace TheOtherRoles.Patches {
    [HarmonyPatch]
    public static class CredentialsPatch {
        public static string fullCredentialsVersion = $"<size=130%>{Helpers.GradientColorText("FFD700", "FF0000", $"TheOtherRoles GM IA")}</size>" + $" v{TheOtherRolesPlugin.Version.ToString() + TheOtherRolesPlugin.SubVersionString + (TheOtherRolesPlugin.betaDays > 0 ? "-BETA" : "")}";
        public static string fullCredentials = "fullCredentials";

        public static string mainMenuCredentials = "mainMenuCredentials";

        public static string contributorsCredentials = "contributorsCredentials";

        [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
        internal static class PingTrackerPatch
        {
            static void Postfix(PingTracker __instance){
                __instance.text.alignment = AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started ? TextAlignmentOptions.Top : TextAlignmentOptions.TopLeft;
                var position = __instance.GetComponent<AspectPosition>();
                position.Alignment = AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started ? AspectPosition.EdgeAlignments.Top : AspectPosition.EdgeAlignments.LeftTop;
                if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started) {
                    string gameModeText = $"";
                    if (HideNSeek.isHideNSeekGM) gameModeText = ModTranslation.getString("gamemodeHideNSeek");
                    else if (HandleGuesser.isGuesserGm) gameModeText = ModTranslation.getString("gamemodeGuesser");
                    else if (FreePlayGM.isFreePlayGM) gameModeText = ModTranslation.getString("gamemodeFreePlay");
                    if (gameModeText != "") gameModeText = Helpers.cs(Color.yellow, gameModeText) + "\n";
                    __instance.text.text = $"<size=130%>{Helpers.GradientColorText("FFD700", "FF0000", $"TheOtherRoles GM IA")}</size>" + $" v{TheOtherRolesPlugin.Version.ToString() + (TheOtherRolesPlugin.betaDays > 0 ? "-BETA" : "")}\n{gameModeText}" + __instance.text.text;
                    position.DistanceFromEdge = new Vector3(1.5f, 0.11f, 0);
                } else {
                    string gameModeText = $"";
                    if (TORMapOptions.gameMode == CustomGamemodes.HideNSeek) gameModeText = ModTranslation.getString("gamemodeHideNSeek");
                    else if (TORMapOptions.gameMode == CustomGamemodes.Guesser) gameModeText = ModTranslation.getString("gamemodeGuesser");
                    else if (TORMapOptions.gameMode == CustomGamemodes.FreePlay) gameModeText = ModTranslation.getString("gamemodeFreePlay");
                    if (gameModeText != "") gameModeText = Helpers.cs(Color.yellow, gameModeText);

                    __instance.text.text = $"{ModTranslation.getString(fullCredentialsVersion)}\n{ModTranslation.getString(fullCredentials)}\n {__instance.text.text}";
                    position.DistanceFromEdge = new Vector3(0.5f, 0.11f);

                    try
                    {
                        var GameModeText = GameObject.Find("GameModeText")?.GetComponent<TextMeshPro>();
                        GameModeText.text = gameModeText == "" ? (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek ? "Van. HideNSeek" : ModTranslation.getString("modeClassic")) : gameModeText;
                        var ModeLabel = GameObject.Find("ModeLabel")?.GetComponentInChildren<TextMeshPro>();
                        ModeLabel.text = ModTranslation.getString("modeLabel");
                    }
                    catch { }
                }
                position.AdjustPosition();
            }
        }

        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
        public static class LogoPatch
        {
            public static SpriteRenderer renderer;
            public static Sprite bannerSprite;
            public static Sprite horseBannerSprite;
            public static Sprite banner2Sprite;
            private static PingTracker instance;
            public static GameObject motdObject;
            public static TextMeshPro motdText;

            static void Postfix(PingTracker __instance) {
                var torLogo = new GameObject("bannerLogo_TOR");
                torLogo.transform.SetParent(GameObject.Find("RightPanel").transform, false);
                torLogo.transform.localPosition = new Vector3(-0.4f, 1f, 5f);

                renderer = torLogo.AddComponent<SpriteRenderer>();
                loadSprites();
                renderer.sprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Banner.png", 300f);

                instance = __instance;
                loadSprites();
                // renderer.sprite = TORMapOptions.enableHorseMode ? horseBannerSprite : bannerSprite;
                renderer.sprite = EventUtility.isEnabled ? banner2Sprite : bannerSprite;
                var credentialObject = new GameObject("credentialsTOR");
                var credentials = credentialObject.AddComponent<TextMeshPro>();
                credentials.SetText($"v{TheOtherRolesPlugin.Version.ToString() + (TheOtherRolesPlugin.betaDays > 0 ? "-BETA" : "")}\n<size=30f%>\n</size>{ModTranslation.getString(mainMenuCredentials)}\n<size=30%>\n</size>{ModTranslation.getString(contributorsCredentials)}");
                credentials.alignment = TMPro.TextAlignmentOptions.Center;
                credentials.fontSize *= 0.05f;

                credentials.transform.SetParent(torLogo.transform);
                //credentials.transform.localPosition = Vector3.down * 2;
                credentials.transform.localPosition = Vector3.down + new Vector3(0f, -0.6f, 0f);

                motdObject = new GameObject("torMOTD");
                motdText = motdObject.AddComponent<TextMeshPro>();
                motdText.alignment = TMPro.TextAlignmentOptions.Center;
                motdText.fontSize *= 0.04f;

                motdText.transform.SetParent(torLogo.transform);
                motdText.enableWordWrapping = true;
                var rect = motdText.gameObject.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(5.2f, 0.25f);

                motdText.transform.localPosition = Vector3.down * 2.6f;
                motdText.color = new Color(1, 53f / 255, 31f / 255);
                Material mat = motdText.fontSharedMaterial;
                mat.shaderKeywords = new string[] { "OUTLINE_ON" };
                motdText.SetOutlineColor(Color.white);
                motdText.SetOutlineThickness(0.025f);
            }

            public static void loadSprites() {
                if (bannerSprite == null) bannerSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Banner.png", 300f);
                if (banner2Sprite == null) banner2Sprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Banner2.png", 300f);
                if (horseBannerSprite == null) horseBannerSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.bannerTheHorseRoles.png", 300f);
            }

            public static void updateSprite() {
                loadSprites();
                if (renderer != null) {
                    float fadeDuration = 1f;
                    instance.StartCoroutine(Effects.Lerp(fadeDuration, new Action<float>((p) => {
                        renderer.color = new Color(1, 1, 1, 1 - p);
                        if (p == 1) {
                            renderer.sprite = TORMapOptions.enableHorseMode ? horseBannerSprite : bannerSprite;
                            instance.StartCoroutine(Effects.Lerp(fadeDuration, new Action<float>((p) => {
                                renderer.color = new Color(1, 1, 1, p);
                            })));
                        }
                    })));
                }
            }
        }

        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.LateUpdate))]
        public static class MOTD
        {
            public static List<string> motds = new();
            private static float timer = 0f;
            private static float maxTimer = 5f;
            private static int currentIndex = 0;

            public static void Postfix()
            {
                if (motds.Count == 0)
                {
                    timer = maxTimer;
                    return;
                }
                if (motds.Count > currentIndex && LogoPatch.motdText != null)
                    LogoPatch.motdText.SetText(motds[currentIndex]);
                else return;

                // fade in and out:
                float alpha = Mathf.Clamp01(Mathf.Min([timer, maxTimer - timer]));
                if (motds.Count == 1) alpha = 1;
                LogoPatch.motdText.color = LogoPatch.motdText.color.SetAlpha(alpha);
                timer -= Time.deltaTime;
                if (timer <= 0)
                {
                    timer = maxTimer;
                    currentIndex = (currentIndex + 1) % motds.Count;
                }
            }

            public static void loadMOTDs()
            {
                string url = Helpers.isChinese() ? "https://gitee.com/dabaoimp11/GMIAMOTDs/raw/master/MOTDs.txt" : "https://raw.githubusercontent.com/GMIA-Nexus/GMIAMOTDs/main/MOTDs.txt";
                var request = UnityWebRequest.Get(url);
                request.SendWebRequest();

                // Wait for the request to complete
                while (!request.isDone) { }

                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    TheOtherRolesPlugin.Logger.LogError($"Couldn't fetch mod news from Server: {request.error}");
                    return;
                }

                string motdsText = request.downloadHandler.text;
                foreach (string line in motdsText.Split('\n', StringSplitOptions.RemoveEmptyEntries))
                {
                    MOTD.motds.Add(line.Trim());
                }
            }
        }
    }
}
