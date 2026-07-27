using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using TheOtherRoles.MetaContext;
using TheOtherRoles.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.UI.Button;
using Object = UnityEngine.Object;

namespace TheOtherRoles.Modules {
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public class MainMenuPatch {
        private static bool horseButtonState = TORMapOptions.enableHorseMode;

        private static void Prefix(MainMenuManager __instance) {
            // Force Reload of SoundEffectHolder
            CheckAndUnpatch();
        }

        private static void CheckAndUnpatch()
        {
            // 获取所有已加载的插件
            var loadedPlugins = IL2CPPChainloader.Instance.Plugins.Values;
            // 检查是否有目标插件
            var targetPlugin = loadedPlugins.FirstOrDefault(plugin => plugin.Metadata.Name == "MalumMenu");

            if (targetPlugin != null)
            {
                TheOtherRolesPlugin.Logger.LogMessage("Hacker Plugin Found.\n GMIA Does Not Allow You Using Such Plugins.\nWill Unpatch Soon.");
                Harmony.UnpatchAll();//当进入MainMenu时检测加载如果有MM 就自动关闭
            }
        }

        public static void addSceneChangeCallbacks() {
            SceneManager.add_sceneLoaded((Action<Scene, LoadSceneMode>)((scene, _) => {
                if (!scene.name.Equals("MatchMaking", StringComparison.Ordinal)) return;
                TORMapOptions.gameMode = CustomGamemodes.Classic;
                // Add buttons For Guesser Mode, Hide N Seek in this scene.
                // find "HostLocalGameButton"
                var template = GameObject.FindObjectOfType<HostLocalGameButton>();
                var gameButton = template.transform.FindChild("CreateGameButton");
                var gameButtonPassiveButton = gameButton.GetComponentInChildren<PassiveButton>();

                var guesserButton = GameObject.Instantiate<Transform>(gameButton, gameButton.parent);
                guesserButton.transform.localPosition += new Vector3(0f, -0.5f);
                var guesserButtonText = guesserButton.GetComponentInChildren<TMPro.TextMeshPro>();
                var guesserButtonPassiveButton = guesserButton.GetComponentInChildren<PassiveButton>();
                
                guesserButtonPassiveButton.OnClick = new Button.ButtonClickedEvent();
                guesserButtonPassiveButton.OnClick.AddListener((System.Action)(() => {
                    TORMapOptions.gameMode = CustomGamemodes.Guesser;
                    template.OnClick();
                }));

                var HideNSeekButton = GameObject.Instantiate<Transform>(gameButton, gameButton.parent);
                HideNSeekButton.transform.localPosition += new Vector3(1.7f, -0.5f);
                var HideNSeekButtonText = HideNSeekButton.GetComponentInChildren<TMPro.TextMeshPro>();
                var HideNSeekButtonPassiveButton = HideNSeekButton.GetComponentInChildren<PassiveButton>();
                
                HideNSeekButtonPassiveButton.OnClick = new Button.ButtonClickedEvent();
                HideNSeekButtonPassiveButton.OnClick.AddListener((System.Action)(() => {
                    TORMapOptions.gameMode = CustomGamemodes.HideNSeek;
                    template.OnClick();
                }));

                var FreePlayButton = GameObject.Instantiate<Transform>(gameButton, gameButton.parent);
                FreePlayButton.transform.localPosition += new Vector3(3.4f, -0.5f);
                var FreePlaytButtonText = FreePlayButton.GetComponentInChildren<TMPro.TextMeshPro>();
                var FreePlayButtonPassiveButton = FreePlayButton.GetComponentInChildren<PassiveButton>();

                FreePlayButtonPassiveButton.OnClick = new Button.ButtonClickedEvent();
                FreePlayButtonPassiveButton.OnClick.AddListener((System.Action)(() => {
                    TORMapOptions.gameMode = CustomGamemodes.FreePlay;
                    template.OnClick();
                }));

                template.StartCoroutine(Effects.Lerp(0.1f, new System.Action<float>((p) => {
                    guesserButtonText.SetText(ModTranslation.getString("torGuesser"));
                    HideNSeekButtonText.SetText(ModTranslation.getString("torHideNSeek"));
                    FreePlaytButtonText.SetText(ModTranslation.getString("torFreePlay"));
                 })));
            }));
        }
    }

    [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
    public static class VersionShower_Start
    {
        public static void Postfix(VersionShower __instance)
        {
            __instance.text.text = $"Among Us v{DestroyableSingleton<ReferenceDataManager>.Instance.Refdata.userFacingVersion} - " +
                $"{Helpers.GradientColorText("FFD700", "FF0000", $"The Other Roles GM IA")} <color=#FCCE03FF>v{TheOtherRolesPlugin.VersionString + TheOtherRolesPlugin.SubVersionString
                + (TheOtherRolesPlugin.betaDays > 0 ? "-BETA" : "")}</color>";
        }
    }

    // Original credits to Nebula On the Ship
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Awake))]
    public static class MainMenuSetUpPatch
    {
        public static Sprite sprite;
        public static Sprite discordSprite;
        public static GameObject modScreen = null;
        public static GameObject aboutScreen = null;
        public static Dictionary<string, string> contributors = new()
        { { "Imp11", "mainMenuDeveloper" }, { "Amongus", "mainMenuDeveloper" },  { "Fangkuai", "mainMenuArtist" },
            { "Elinmei", "mainMenuDeveloper" }, { "Yuunozikkyou" , "mainMenuTranslator"}, { "TAIK", "mainMenuTranslator"} };

        public static void Postfix(MainMenuManager __instance)
        {
            VanillaAsset.LoadAssetAtInitialize();
            var scalerList = __instance.mainMenuUI.GetComponent<SlicedAspectScaler>();

            var leftPanel = __instance.mainMenuUI.transform.FindChild("AspectScaler").FindChild("LeftPanel");
            var auLogo = leftPanel.FindChild("Sizer").GetComponent<AspectSize>();
            auLogo.PercentWidth = 0.14f;
            auLogo.DoSetUp();
            auLogo.transform.localPosition += new Vector3(-0.8f, 0.25f, 0f);

            float height = __instance.newsButton.transform.localPosition.y - __instance.myAccountButton.transform.localPosition.y;

            // disable old panels and create a new button
            var reworkedPanel = Helpers.CreateObject<SpriteRenderer>("ReworkedLeftPanel", leftPanel, new Vector3(0f, height * 0.5f, 0f));
            var oldPanel = leftPanel.GetComponent<SpriteRenderer>();
            reworkedPanel.sprite = oldPanel.sprite;
            reworkedPanel.tileMode = oldPanel.tileMode;
            reworkedPanel.drawMode = oldPanel.drawMode;
            reworkedPanel.size = oldPanel.size + new Vector2(0f, 0.5f);
            leftPanel.gameObject.GetComponent<AspectScaledAsset>().AddScaledSprite(reworkedPanel);
            oldPanel.enabled = false;

            loadSprite();

            // Move the buttons upwards
            foreach (var button in __instance.mainButtons.GetFastEnumerator())
                if (Math.Abs(button.transform.localPosition.x) < 0.1f) button.transform.localPosition += new Vector3(0f, height, 0f);
            leftPanel.FindChild("Main Buttons").FindChild("Divider").transform.localPosition += new Vector3(0f, height, 0f);

            var modButton = Object.Instantiate(__instance.settingsButton, __instance.settingsButton.transform.parent);
            modButton.transform.localPosition += new Vector3(0f, -height, 0f);
            modButton.gameObject.name = "TORButton";
            modButton.gameObject.ForEachChild((Il2CppSystem.Action<GameObject>)((obj) => {
                var icon = obj.transform.FindChild("Icon");
                if (icon != null)
                {
                    icon.localScale = new Vector3(1f, 1f, 1f);
                    icon.GetComponent<SpriteRenderer>().sprite = sprite;
                }
            }));
            scalerList.objectsToScale.Add(modButton.GetComponent<AspectScaledAsset>());

            var modPassiveButton = modButton.GetComponent<PassiveButton>();
            modPassiveButton.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
            modPassiveButton.OnClick.AddListener((System.Action)(() =>
            {
                __instance.ResetScreen();
                modScreen.SetActive(true);
                __instance.screenTint.enabled = true;
            }));
            modButton.transform.FindChild("FontPlacer").GetChild(0).GetComponent<TextTranslatorTMP>().SetModText("mainMenuGMIATitle");

            modScreen = Object.Instantiate(__instance.accountButtons, __instance.accountButtons.transform.parent);
            modScreen.name = "modScreen";
            modScreen.transform.GetChild(0).GetChild(0).GetComponent<TextTranslatorTMP>().SetModText("GMIA");
            __instance.mainButtons.Add(modButton);

            Object.Destroy(modScreen.transform.GetChild(4).gameObject);

            foreach (var button in __instance.mainButtons.GetFastEnumerator())
            {
                if (button.activeSprites)
                {
                    var name = button.name;
                    bool shouldRotate = name != "TORButton" && name != "Inventory Button";
                    bool shouldMove = name != "TORButton";
                    var rendererTransform = button.activeSprites.transform.FindChild("Icon");
                    if (rendererTransform)
                    {

                        if (shouldRotate) rendererTransform.localEulerAngles -= new Vector3(0f, 0f, 10f);
                        rendererTransform.localScale += new Vector3(0.12f, 0.12f, 0f);

                        if (shouldMove)
                        {
                            var aspectPos = rendererTransform.GetComponent<AspectPosition>();
                            if (aspectPos)
                            {
                                aspectPos.DistanceFromEdge += new Vector3(-0.02f, 0.1f, 0f);
                                aspectPos.AdjustPosition();
                            }
                            else
                            {
                                rendererTransform.localPosition += new Vector3(-0.02f, 0.1f, 0f);
                            }
                        }
                    }
                }
            }

            var temp = modScreen.transform.GetChild(3);
            int index = 0;

            void SetUpButton(string button, Action clickAction)
            {
                GameObject obj = temp.gameObject;
                if (index > 0) obj = Object.Instantiate(obj, obj.transform.parent);

                obj.transform.GetChild(0).GetChild(0).GetComponent<TextTranslatorTMP>().SetModText(button);
                var passiveButton = obj.GetComponent<PassiveButton>();
                passiveButton.OnClick = new ButtonClickedEvent();
                passiveButton.OnClick.AddListener((System.Action)(() => {
                    clickAction.Invoke();
                }));
                obj.transform.localPosition = new Vector3(0f, 0.98f - index * 0.68f, 0f);

                index++;
            }
            SetUpButton("modAbout", () => {
                __instance.ResetScreen();
                if (!aboutScreen) createAboutScreen();
                aboutScreen?.SetActive(true);
                __instance.screenTint.enabled = true;
            });
            SetUpButton("modHowToPlay", () =>
            {
                PlayManager.Open(__instance);
            });
            SetUpButton("modAchievements", () => {
                AchievementViewer.Open(__instance);
            });

            foreach (var asset in modScreen.GetComponentsInChildren<AspectScaledAsset>()) scalerList.objectsToScale.Add(asset);

            var discordRenderer = Helpers.CreateObject<SpriteRenderer>("DiscordButton", __instance.transform, Vector3.zero);
            discordRenderer.gameObject.SetAsUIAspectContent(AspectPosition.EdgeAlignments.RightBottom, new(0.34f, 1f, -6f));
            discordRenderer.sprite = discordSprite;
            var discordButton = discordRenderer.gameObject.SetUpButton(true, discordRenderer);
            discordButton.OnMouseOver.AddListener((Action)(() => TORGUIManager.Instance.SetHelpContext(discordButton, ModTranslation.getString("mainMenuDiscordText"))));
            discordButton.OnMouseOut.AddListener((Action)(() => TORGUIManager.Instance.HideHelpContextIf(discordButton)));
            discordButton.OnClick.AddListener((Action)(() => Application.OpenURL("https://discord.gg/w7msq53dq7")));
            discordButton.gameObject.AddComponent<CircleCollider2D>().radius = 0.25f;

            void createAboutScreen()
            {
                aboutScreen = Helpers.CreateObject("About", __instance.accountButtons.transform.parent, new Vector3(0, 0, -1f));
                aboutScreen.transform.localScale = modScreen!.transform.localScale;
                var screen = MetaScreen.GenerateScreen(new Vector2(6.2f, 4.1f), aboutScreen.transform, new Vector3(-0.1f, 0, 0f), false, false, false);
                TextAttribute detailAttribute = new(TextAttribute.BoldAttr)
                {
                    FontMaterial = VanillaAsset.StandardMaskedFontMaterial,
                    Size = new Vector2(5.8f, 0.4f),
                    Alignment = TMPro.TextAlignmentOptions.TopLeft,
                    FontSize = 2f,
                    FontMaxSize = 2f,
                    FontMinSize = 1.7f
                };
                TextAttribute descriptionAttribute = new(TextAttribute.ContentAttr)
                {
                    FontMaterial = VanillaAsset.StandardMaskedFontMaterial,
                    Size = new Vector2(5.8f, 0.4f),
                    Alignment = TMPro.TextAlignmentOptions.TopLeft,
                    FontSize = 1.7f,
                    FontMaxSize = 1.7f,
                    FontMinSize = 1.3f
                };
                TextAttribute titleAttribute = new(TextAttribute.NormalAttr)
                {
                    FontMaterial = VanillaAsset.StandardMaskedFontMaterial,
                    Size = new Vector2(3.4f, 0.3f),
                    Alignment = TextAlignmentOptions.Center,
                    FontSize = 4f,
                    FontMaxSize = 4f,
                    FontMinSize = 3.5f
                };

                var inner = new MetaContextOld();
                inner.Append(new MetaContextOld.Text(titleAttribute) { RawText = ModTranslation.getString("modAbout").ToUpper(), Alignment = IMetaContextOld.AlignmentOption.Center });
                inner.Append(new MetaContextOld.VerticalMargin(0.5f));
                foreach (var contributor in contributors)
                {
                    inner.Append(new CombinedContextOld(0.5f,
                    new MetaContextOld.Image(Helpers.loadSpriteFromResources($"TheOtherRoles.Resources.Contributors.{contributor.Key}.png", 50f)) { Width = 0.7f },
                    new MetaContextOld.Text(detailAttribute) { RawText = $"{ModTranslation.getString(contributor.Value)} - {contributor.Key}" })
                    { Alignment = IMetaContextOld.AlignmentOption.Left });
                    inner.Append(new MetaContextOld.VerticalMargin(0.3f));
                    var lines = Helpers.removeLineFeed(ModTranslation.getString($"mainMenu{contributor.Key}AboutSection"));
                    foreach (var l in lines)
                    {
                        inner.Append(new MetaContextOld.Text(descriptionAttribute) { RawText = l });
                    }
                    inner.Append(new CombinedContextOld(0.5f,
                        new MetaContextOld.VerticalMargin(0.3f)
                        ));
                }
                screen.SetContext(new MetaContextOld.ScrollView(new Vector2(6.2f, 4.1f), inner, true) { Alignment = IMetaContextOld.AlignmentOption.Center });
            }
        }

        public static void loadSprite()
        {
            if (sprite == null) sprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.LogoButton.png", 100f);
            if (discordSprite == null) discordSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.DiscordIcon.png", 100f);
        }
    }

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.ResetScreen))]
    public static class MainMenuClearScreenPatch
    {
        public static void Postfix(MainMenuManager __instance)
        {
            if (MainMenuSetUpPatch.modScreen) MainMenuSetUpPatch.modScreen?.SetActive(false);
            if (MainMenuSetUpPatch.aboutScreen) MainMenuSetUpPatch.aboutScreen?.SetActive(false);
        }
    }
}
