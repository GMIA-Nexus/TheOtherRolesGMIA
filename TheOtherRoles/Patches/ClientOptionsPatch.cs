using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using HarmonyLib;
using Rewired.UI.ControlMapper;
using Rewired.Utils.Classes.Data;
using TheOtherRoles.MetaContext;
using TheOtherRoles.Modules;
using TheOtherRoles.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.UI.Button;
using Object = UnityEngine.Object;

namespace TheOtherRoles.Patches 
{
    public class ClientOption
    {
        public enum ClientOptionType
        {
            SpoilerAfterDeath,
            ShowRoleSummary,
            PlayLobbyMusic,
            ShowOnlySpawnableAssignableOnFilter,
            ShowLighterDarker,
            EnableSoundEffects,
            ShowChatNotification,
            ToggleCursor,
            ShowVentsOnMap,
            ShowExtraInfo,
            ScreenshotOnEnd
        }
        static private readonly DataSaver ClientOptionSaver = new("ClientOption");

        static internal readonly Dictionary<ClientOptionType, ClientOption> AllOptions = [];
        static public int GetValue(ClientOptionType option) => AllOptions.TryGetValue(option, out var entry) ? entry.Value : 0;
        DataEntry<int> configEntry;
        string id;
        string[] selections;
        ClientOptionType type;

        public ClientOption(ClientOptionType type, string name, string[] selections, int defaultValue)
        {
            id = name;
            configEntry = new IntegerDataEntry(name, ClientOptionSaver, defaultValue);
            this.selections = selections;
            this.type = type;
            AllOptions[type] = this;
        }

        public string RawKey => id;
        public string DisplayName => ModTranslation.getString(id);
        public string DisplayDetail => ModTranslation.getString(id + "Detail", tryFind: true);
        public string DisplayValue => ModTranslation.getString(selections[configEntry.Value]);
        public int Value => configEntry.Value;
        public Action OnValueChanged;
        public bool ShowOnClientSetting { get; set; } = true;

        public void Increment()
        {
            configEntry.Value = (configEntry.Value + 1) % selections.Length;
            OnValueChanged?.Invoke();
        }

        public static void Load()
        {
            string[] simpleSwitch = ["optionOff", "optionOn"];
#if WINDOWS
            _ = new ClientOption(ClientOptionType.ToggleCursor, "toggleCursor", simpleSwitch, 0) {
                OnValueChanged = () => Helpers.enableCursor(GetValue(ClientOptionType.ToggleCursor) == 1)
            };
#endif
            _ = new ClientOption(ClientOptionType.SpoilerAfterDeath, "spoilerAfterDeath", simpleSwitch, 1);
            _ = new ClientOption(ClientOptionType.ShowRoleSummary, "showRoleSummaryButton", simpleSwitch, 1);
            _ = new ClientOption(ClientOptionType.PlayLobbyMusic, "playLobbyMusic", simpleSwitch, 1)
            {
                OnValueChanged = () =>
                {
                    if (!LobbyBehaviour.Instance) return;
                    bool playMusic = AllOptions[ClientOptionType.PlayLobbyMusic].Value == 1;

                    if (playMusic)
                    {
                        SoundManager.Instance.CrossFadeSound("MapTheme", LobbyBehaviour.Instance.MapTheme, 0.5f, 1.5f);
                    }
                    else
                    {
                        SoundManager.Instance.CrossFadeSound("MapTheme", null, 0.5f, 1.5f);
                    }
                }
            };
            _ = new ClientOption(ClientOptionType.ShowOnlySpawnableAssignableOnFilter, "showOnlySpawnableAssignableOnFilter", simpleSwitch, 0) { ShowOnClientSetting = false };
            _ = new ClientOption(ClientOptionType.ShowLighterDarker, "showLighterDarker", simpleSwitch, 0);
            _ = new ClientOption(ClientOptionType.EnableSoundEffects, "enableSoundEffects", simpleSwitch, 1);
            _ = new ClientOption(ClientOptionType.ShowChatNotification, "showChatNotification", simpleSwitch, 1);
            _ = new ClientOption(ClientOptionType.ShowVentsOnMap, "showVentsOnMap", simpleSwitch, 0);
            _ = new ClientOption(ClientOptionType.ShowExtraInfo, "showExtraInfo", simpleSwitch, 1);
#if WINDOWS
            _ = new ClientOption(ClientOptionType.ScreenshotOnEnd, "screenshotOnEnd", simpleSwitch, 0);
#endif
        }
    }

    [HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Start))]
    public static class StartOptionMenuPatch
    {
        public static void Postfix(OptionsMenuBehaviour __instance)
        {
            __instance.transform.localPosition = new(0, 0, -700f);

            foreach (var button in __instance.GetComponentsInChildren<CustomButton>(true))
            {
                if (button.name != "DoneButton") continue;

                button.onClick.AddListener((Action)(() => {
                    if (AmongUsClient.Instance && AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
                        HudManager.Instance.ShowVanillaKeyGuide();
                }));
            }

            var tabs = new List<TabGroup>(__instance.Tabs.ToArray());

            PassiveButton passiveButton;

            GameObject torTab = new("TORTab");
            torTab.transform.SetParent(__instance.transform);
            torTab.transform.localScale = new Vector3(1f, 1f, 1f);
            torTab.SetActive(false);

            var torScreen = MetaScreen.GenerateScreen(new(5.6f, 4.5f), torTab.transform, new(0f, -0.28f, -10f), false, false, false);

            void SetTORContext()
            {
                var buttonAttr = new TextAttribute(TextAttribute.BoldAttr) { Size = new Vector2(2.11f, 0.22f) };
                MetaContextOld torContext = new();
                torContext.Append(ClientOption.AllOptions.Values.Where(o => o.ShowOnClientSetting), (option) => new MetaContextOld.Button(() => {
                    option.Increment();
                    SetTORContext();
                }, buttonAttr)
                {
                    TextMargin = 0.19f,
                    RawText = option.DisplayName + " : " + option.DisplayValue,
                    PostBuilder = (button, _, _) =>
                    {
                        var detail = option.DisplayDetail;
                        if (detail != null)
                        {
                            button.SetRawOverlay(detail);
                        }
                    }
                }, 2, -1, 0, 0.51f);
                torContext.Append(new MetaContextOld.VerticalMargin(0.2f));

                torScreen.SetContext(torContext);
            }

            SetTORContext();

            tabs[^1] = Object.Instantiate(tabs[1], null);
            var torButton = tabs[^1];
            torButton.gameObject.name = "TORButton";
            torButton.transform.SetParent(tabs[0].transform.parent);
            torButton.transform.localScale = new Vector3(1f, 1f, 1f);
            torButton.Content = torTab;
            var textObj = torButton.transform.FindChild("Text_TMP").gameObject;
            textObj.GetComponent<TextTranslatorTMP>().enabled = false;
            textObj.GetComponent<TMPro.TMP_Text>().text = "GMIA";

            passiveButton = torButton.gameObject.GetComponent<PassiveButton>();
            passiveButton.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
            passiveButton.OnClick.AddListener((UnityEngine.Events.UnityAction)(() =>
            {
                __instance.OpenTabGroup(tabs.Count - 1);
                SetTORContext();
            }
            ));

            float y = tabs[0].transform.localPosition.y, z = tabs[0].transform.localPosition.z;
            if (tabs.Count == 3)
                for (int i = 0; i < 3; i++) tabs[i].transform.localPosition = new Vector3(1.7f * (float)(i - 1), y, z);
            else if (tabs.Count == 4)
                for (int i = 0; i < 4; i++) tabs[i].transform.localPosition = new Vector3(1.62f * ((float)i - 1.5f), y, z);

            __instance.Tabs = new Il2CppReferenceArray<TabGroup>(tabs.ToArray());
        }
    }
}
