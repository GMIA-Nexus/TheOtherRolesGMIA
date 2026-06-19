using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using TheOtherRoles;
using Newtonsoft.Json.Linq;
using AmongUs.GameOptions;

namespace TheOtherRoles.Objects
{
    public sealed class HaomingMenu : MonoBehaviour
    {
        public static GameObject prefab;
        public static GameObject menuObj;
        private static GameObject content;
        private Button closeButton;
        private Button loadSettingsButton;
        private TMP_Dropdown dropdown;
        private TextMeshProUGUI log;
        private TextMeshProUGUI TittleNameText;
        private TextMeshProUGUI FileNameText;
        private TextMeshProUGUI MainTittleNameText;
        private string logText;
        private string title;
        private string fileName;

        private static int selected = 0;
        public static GameObject menuPrefab;
        public static GameObject loadSettingsPrefab;

        public void Awake()
        {
            if (prefab == null)
            {
                prefab = this.gameObject;
                this.gameObject.SetActive(false);
                return;
            }
            else
            {
                this.gameObject.SetActive(true);
            }

            if (menuObj) GameObject.Destroy(menuObj);
            PlayerControl.LocalPlayer.moveable = false;
            PlayerControl.LocalPlayer.NetTransform.Halt();
            if (menuObj != null)
            {
                GameObject.Destroy(menuObj);
            }

            menuObj = GameObject.Instantiate(menuPrefab, this.transform);

            var buttons = menuObj.GetComponentsInChildren<Button>();

            // Closeボタン有効化
            closeButton = buttons.FirstOrDefault(x => x.name == "CloseButton");
            closeButton.onClick = new Button.ButtonClickedEvent();
            closeButton.onClick.AddListener((UnityAction)close);
            closeButton.GetComponentInChildren<Text>().text = string.Empty;


            // LoadSettingsButton有効化
            loadSettingsButton = buttons.FirstOrDefault(x => x.name == "LoadSettingsButton");
            loadSettingsButton.onClick = new Button.ButtonClickedEvent();
            loadSettingsButton.onClick.AddListener((UnityAction)showloadSettingsMenu);
            loadSettingsButton.GetComponentInChildren<TextMeshProUGUI>().text = "Regulations";

            // メニュー表示ボタン有効化
            menuObj.SetActive(true);
            showloadSettingsMenu();
        }

        private void FixedUpdate()
        {
            PlayerControl.LocalPlayer.moveable = false;
            if (Input.GetKey(KeyCode.Escape))
            {
                close();
            }
        }

        public void OnEnable()
        {
            this.enabled = true;
        }

        public void OnDisable()
        {
            if (menuObj) menuObj.SetActive(false);
        }

        public void OnDestroy()
        {
            PlayerControl.LocalPlayer.moveable = true;
        }

        void showloadSettingsMenu()
        {
            title = "";
            fileName = "";

            string filePath = GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek ? Path.GetDirectoryName(Application.dataPath) + @"\Regulations\HideNSeek\" : Path.GetDirectoryName(Application.dataPath) + @"\Regulations\Normal\";
            bool exists = System.IO.Directory.Exists(filePath);
            if (!exists) System.IO.Directory.CreateDirectory(filePath);

            if (content) GameObject.Destroy(content);
            content = GameObject.Instantiate(loadSettingsPrefab, menuObj.transform);
            content.SetActive(true);

            var buttons = content.GetComponentsInChildren<Button>();
            closeButton = buttons.FirstOrDefault(x => x.name == "CloseButton");
            closeButton.onClick = new Button.ButtonClickedEvent();
            closeButton.onClick.AddListener((UnityAction)close);
            var saveButton = buttons.FirstOrDefault(x => x.name == "SaveButton");
            saveButton.GetComponentInChildren<TextMeshProUGUI>().text = ModTranslation.getString("saveButtonText");
            saveButton.onClick = new Button.ButtonClickedEvent();
            saveButton.onClick.AddListener((UnityAction)save);
            var loadButton = buttons.FirstOrDefault(x => x.name == "LoadButton");
            loadButton.GetComponentInChildren<TextMeshProUGUI>().text = ModTranslation.getString("loadButtonText");
            loadButton.onClick = new Button.ButtonClickedEvent();
            loadButton.onClick.AddListener((UnityAction)load);
            List<TextMeshProUGUI> texts = content.GetComponentsInChildren<TextMeshProUGUI>().ToList();
            TittleNameText = texts.FirstOrDefault(x => x.name == "TittleName");
            TittleNameText.text = ModTranslation.getString("TittleNameText");
            FileNameText = texts.FirstOrDefault(x => x.name == "FileName");
            FileNameText.text = ModTranslation.getString("FileNameText");
            MainTittleNameText = texts.FirstOrDefault(x => x.name == "MainTittleName");
            MainTittleNameText.text = ModTranslation.getString("MainTittleNameText");
            dropdown = content.GetComponentsInChildren<TMP_Dropdown>().FirstOrDefault(x => x.name == "Dropdown");
            dropdown.ClearOptions();
            var optionDataList = new Il2CppSystem.Collections.Generic.List<TMP_Dropdown.OptionData>();
            List<string> optionList = new();
            var fileList = getFileList();
            foreach (var file in fileList)
            {
                var optionData = new TMP_Dropdown.OptionData();
                optionData.text = getTitleFromFile(file);
                optionDataList.Add(optionData);
            }
            dropdown.AddOptions(optionDataList);
            dropdown.value = selected;
            dropdown.RefreshShownValue();
            dropdown.onValueChanged = new TMP_Dropdown.DropdownEvent();
            dropdown.onValueChanged.AddListener((UnityAction<int>)onValueChanged);


            var inputFields = content.GetComponentsInChildren<TMP_InputField>();
            var titleField = inputFields.FirstOrDefault(x => x.name == "TitleInputField");
            titleField.onValueChanged = new TMP_InputField.OnChangeEvent();
            titleField.onValueChanged.AddListener((UnityAction<String>)onTitleChanged);
            var fileNameField = inputFields.FirstOrDefault(x => x.name == "FileNameInputField");
            fileNameField.onValueChanged = new TMP_InputField.OnChangeEvent();
            fileNameField.onValueChanged.AddListener((UnityAction<String>)onFileNameChanged);

            var scrollView = content.GetComponentsInChildren<ScrollRect>().FirstOrDefault(x => x.name == "Scroll View");
            log = scrollView.GetComponentsInChildren<TextMeshProUGUI>().FirstOrDefault(x => x.name == "Log");
            log.text = logText;
        }

        void onTitleChanged(string value)
        {
            title = value;
        }

        void onFileNameChanged(string value)
        {
            fileName = value;
        }

        void onValueChanged(int value)
        {
            selected = value;
            showloadSettingsMenu();
        }
        void close()
        {
            GameObject.Destroy(this.gameObject);
        }
        void load()
        {
            var fileList = getFileList();
            Regulation.load(fileList[selected]);
            sendLog(string.Format(ModTranslation.getString("loadedLog"), fileList[selected]));
        }

        void sendLog(string s)
        {
            logText = s + "\n" + log.text;
            log.text = logText;
        }

        void save()
        {
            string filePath = GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek ? Path.GetDirectoryName(Application.dataPath) + @"\Regulations\HideNSeek\" : Path.GetDirectoryName(Application.dataPath) + @"\Regulations\Normal\";
            bool exists = System.IO.Directory.Exists(filePath);
            if (!exists) System.IO.Directory.CreateDirectory(filePath);
            if (fileName == null || fileName == string.Empty)
            {
                sendLog(ModTranslation.getString("fileNameEmpty"));
                return;
            }

            if (title == null || title == string.Empty)
            {
                sendLog(ModTranslation.getString("tittleEmpty"));
                return;
            }
            if (!Regex.IsMatch(fileName, @".*\.json"))
            {
                fileName += ".json";
            }
            filePath += fileName;
            Regulation.save(filePath, title);
            sendLog(string.Format(ModTranslation.getString("savedLog"), title, filePath));
            showloadSettingsMenu();
        }

        List<string> getFileList()
        {
            string filePath = GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek ? Path.GetDirectoryName(Application.dataPath) + @"\Regulations\HideNSeek\" : Path.GetDirectoryName(Application.dataPath) + @"\Regulations\Normal\";
            var fileList = Directory.GetFiles(filePath, "*.json").ToList();
            return fileList;
        }

        string getTitleFromFile(string file)
        {
            string json = File.ReadAllText(file);
            JToken jobj = JObject.Parse(json)["title"];
            return jobj != null ? jobj.ToString() : file;
        }

        class Regulation
        {
            public static void save(string filePath, string title)
            {
                var value = new Dictionary<string, object>();
                value.Add("title", title);

                // AmongUsオプション保存
                if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.Normal)
                {
                    value.Add("Map", (int)GameOptionsManager.Instance.currentNormalGameOptions.MapId);
                    value.Add("NumImpostors", GameOptionsManager.Instance.currentNormalGameOptions.NumImpostors);
                    value.Add("ConfirmEjection", GameOptionsManager.Instance.currentNormalGameOptions.ConfirmImpostor ? 1 : 0);
                    value.Add("EmergencyMeetings", GameOptionsManager.Instance.currentNormalGameOptions.NumEmergencyMeetings);
                    value.Add("EmergencyCooldown", GameOptionsManager.Instance.currentNormalGameOptions.EmergencyCooldown);
                    value.Add("DiscussionTime", GameOptionsManager.Instance.currentNormalGameOptions.DiscussionTime);
                    value.Add("VotingTime", GameOptionsManager.Instance.currentNormalGameOptions.VotingTime);
                    value.Add("AnonymousVotes", GameOptionsManager.Instance.currentNormalGameOptions.AnonymousVotes ? 1 : 0);
                    value.Add("PlayerSpeed", (int)(GameOptionsManager.Instance.currentNormalGameOptions.PlayerSpeedMod / 0.25));
                    value.Add("CrewmateVision", (int)(GameOptionsManager.Instance.currentNormalGameOptions.CrewLightMod / 0.25));
                    value.Add("ImpostorVision", (int)(GameOptionsManager.Instance.currentNormalGameOptions.ImpostorLightMod / 0.25));
                    value.Add("KillCooldown", (int)(GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown / 2.5));
                    value.Add("KillDistance", GameOptionsManager.Instance.currentNormalGameOptions.KillDistance);
                    value.Add("VisualTask", GameOptionsManager.Instance.currentNormalGameOptions.VisualTasks ? 1 : 0);
                    value.Add("TaskBarUpdates", (int)GameOptionsManager.Instance.currentNormalGameOptions.TaskBarMode);
                    value.Add("CommonTasks", GameOptionsManager.Instance.currentNormalGameOptions.NumCommonTasks);
                    value.Add("LongTasks", GameOptionsManager.Instance.currentNormalGameOptions.NumLongTasks);
                    value.Add("ShortTasks", GameOptionsManager.Instance.currentNormalGameOptions.NumShortTasks);
                }
                else if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek)
                {
                    value.Add("Map", (int)GameOptionsManager.Instance.currentHideNSeekGameOptions.MapId);
                    value.Add("ImpostorPlayerID", GameOptionsManager.Instance.currentHideNSeekGameOptions.ImpostorPlayerID);
                    value.Add("EscapeTime", (int)GameOptionsManager.Instance.currentHideNSeekGameOptions.EscapeTime);
                    value.Add("FinalCountdownTime", (int)GameOptionsManager.Instance.currentHideNSeekGameOptions.FinalCountdownTime);
                    value.Add("ShowCrewmateNames", GameOptionsManager.Instance.currentHideNSeekGameOptions.ShowCrewmateNames ? 1 : 0);
                    value.Add("UseFlashlight", GameOptionsManager.Instance.currentHideNSeekGameOptions.useFlashlight ? 1 : 0);
                    value.Add("PlayerSpeedMod", (int)GameOptionsManager.Instance.currentHideNSeekGameOptions.PlayerSpeedMod);
                    value.Add("NumCommonTasks", GameOptionsManager.Instance.currentHideNSeekGameOptions.NumCommonTasks);
                    value.Add("NumShortTasks", GameOptionsManager.Instance.currentHideNSeekGameOptions.NumShortTasks);
                    value.Add("NumLongTasks", GameOptionsManager.Instance.currentHideNSeekGameOptions.NumLongTasks);
                    value.Add("CrewmateFlashlightSize", (int)GameOptionsManager.Instance.currentHideNSeekGameOptions.CrewmateFlashlightSize);
                    value.Add("CrewLightMod", (int)GameOptionsManager.Instance.currentHideNSeekGameOptions.CrewLightMod);
                    value.Add("ImpostorLightMod", (int)GameOptionsManager.Instance.currentHideNSeekGameOptions.ImpostorLightMod);
                    value.Add("ImpostorFlashlightSize", (int)GameOptionsManager.Instance.currentHideNSeekGameOptions.ImpostorFlashlightSize);
                    value.Add("CrewmateVentUses", GameOptionsManager.Instance.currentHideNSeekGameOptions.CrewmateVentUses);
                    value.Add("CrewmateTimeInVent", (int)GameOptionsManager.Instance.currentHideNSeekGameOptions.CrewmateTimeInVent);
                    value.Add("SeekerFinalSpeed", (int)GameOptionsManager.Instance.currentHideNSeekGameOptions.SeekerFinalSpeed);
                    value.Add("SeekerFinalMap", GameOptionsManager.Instance.currentHideNSeekGameOptions.SeekerFinalMap ? 1 : 0);
                    value.Add("SeekerPings", GameOptionsManager.Instance.currentHideNSeekGameOptions.SeekerPings ? 1 : 0);
                    value.Add("MaxPingTime", (int)GameOptionsManager.Instance.currentHideNSeekGameOptions.MaxPingTime);
                }
                else
                    TheOtherRolesPlugin.Logger.LogError("Game Mode is None!!!!!");

                // MODオプション保存
                if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.Normal)
                {
                    var mod_options = new List<object>();
                    foreach (var option in CustomOption.options)
                    {
                        if (option.id != -1)
                        {
                            var item = new Dictionary<string, object>();
                            item.Add("id", option.id);
                            item.Add("value", option.selection);
                            mod_options.Add(item);
                        }
                    }
                    value.Add("mod_options", mod_options);
                }
                // json変換
                string data = Helpers.SerializeObject(value);

                File.WriteAllText(filePath, data);
            }
            public static void load(string file)
            {
                string json = File.ReadAllText(file);
                JToken jobj = JObject.Parse(json);
                string title = jobj["title"].ToString();
                if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.Normal)
                {
                    GameOptionsManager.Instance.currentNormalGameOptions.MapId = (byte)int.Parse(jobj["Map"].ToString());
                    GameOptionsManager.Instance.currentNormalGameOptions.NumImpostors = int.Parse(jobj["NumImpostors"].ToString());
                    GameOptionsManager.Instance.currentNormalGameOptions.ConfirmImpostor = jobj["ConfirmEjection"].ToString() == "1" ? true : false;
                    GameOptionsManager.Instance.currentNormalGameOptions.NumEmergencyMeetings = int.Parse(jobj["EmergencyMeetings"].ToString());
                    GameOptionsManager.Instance.currentNormalGameOptions.EmergencyCooldown = int.Parse(jobj["EmergencyCooldown"].ToString());
                    GameOptionsManager.Instance.currentNormalGameOptions.DiscussionTime = int.Parse(jobj["DiscussionTime"].ToString());
                    GameOptionsManager.Instance.currentNormalGameOptions.VotingTime = int.Parse(jobj["VotingTime"].ToString());
                    GameOptionsManager.Instance.currentNormalGameOptions.AnonymousVotes = jobj["AnonymousVotes"].ToString() == "1" ? true : false;
                    GameOptionsManager.Instance.currentNormalGameOptions.PlayerSpeedMod = int.Parse(jobj["PlayerSpeed"].ToString()) * 0.25f;
                    GameOptionsManager.Instance.currentNormalGameOptions.CrewLightMod = int.Parse(jobj["CrewmateVision"].ToString()) * 0.25f;
                    GameOptionsManager.Instance.currentNormalGameOptions.ImpostorLightMod = int.Parse(jobj["ImpostorVision"].ToString()) * 0.25f;
                    GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown = int.Parse(jobj["KillCooldown"].ToString()) * 2.5f;
                    GameOptionsManager.Instance.currentNormalGameOptions.KillDistance = int.Parse(jobj["KillDistance"].ToString());
                    GameOptionsManager.Instance.currentNormalGameOptions.VisualTasks = jobj["VisualTask"].ToString() == "1" ? true : false;
                    GameOptionsManager.Instance.currentNormalGameOptions.TaskBarMode = (AmongUs.GameOptions.TaskBarMode)int.Parse(jobj["TaskBarUpdates"].ToString());
                    GameOptionsManager.Instance.currentNormalGameOptions.NumCommonTasks = int.Parse(jobj["CommonTasks"].ToString());
                    GameOptionsManager.Instance.currentNormalGameOptions.NumLongTasks = int.Parse(jobj["LongTasks"].ToString());
                    GameOptionsManager.Instance.currentNormalGameOptions.NumShortTasks = int.Parse(jobj["ShortTasks"].ToString());
                }
                else if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek)
                {
                    GameOptionsManager.Instance.currentHideNSeekGameOptions.MapId = (byte)int.Parse(jobj["Map"].ToString());
                    GameOptionsManager.Instance.currentHideNSeekGameOptions.ImpostorPlayerID = int.Parse(jobj["ImpostorPlayerID"].ToString());
                    GameOptionsManager.Instance.currentHideNSeekGameOptions.EscapeTime = int.Parse(jobj["EscapeTime"].ToString());
                    GameOptionsManager.Instance.currentHideNSeekGameOptions.FinalCountdownTime = int.Parse(jobj["FinalCountdownTime"].ToString());
                    GameOptionsManager.Instance.currentHideNSeekGameOptions.ShowCrewmateNames = jobj["ShowCrewmateNames"].ToString() == "1" ? true : false;
                    GameOptionsManager.Instance.currentHideNSeekGameOptions.useFlashlight = jobj["UseFlashlight"].ToString() == "1" ? true : false;
                    GameOptionsManager.Instance.currentHideNSeekGameOptions.PlayerSpeedMod = int.Parse(jobj["PlayerSpeedMod"].ToString());
                    GameOptionsManager.Instance.currentHideNSeekGameOptions.NumCommonTasks = int.Parse(jobj["NumCommonTasks"].ToString());
                    GameOptionsManager.Instance.currentHideNSeekGameOptions.NumShortTasks = int.Parse(jobj["NumShortTasks"].ToString());
                    GameOptionsManager.Instance.currentHideNSeekGameOptions.NumLongTasks = int.Parse(jobj["NumLongTasks"].ToString());
                    GameOptionsManager.Instance.currentHideNSeekGameOptions.CrewmateFlashlightSize = int.Parse(jobj["CrewmateFlashlightSize"].ToString());
                    GameOptionsManager.Instance.currentHideNSeekGameOptions.CrewLightMod = int.Parse(jobj["CrewLightMod"].ToString());
                    GameOptionsManager.Instance.currentHideNSeekGameOptions.ImpostorLightMod = int.Parse(jobj["ImpostorLightMod"].ToString());
                    GameOptionsManager.Instance.currentHideNSeekGameOptions.ImpostorFlashlightSize = int.Parse(jobj["ImpostorFlashlightSize"].ToString());
                    GameOptionsManager.Instance.currentHideNSeekGameOptions.CrewmateVentUses = int.Parse(jobj["CrewmateVentUses"].ToString());
                    GameOptionsManager.Instance.currentHideNSeekGameOptions.CrewmateTimeInVent = int.Parse(jobj["CrewmateTimeInVent"].ToString());
                    GameOptionsManager.Instance.currentHideNSeekGameOptions.SeekerFinalSpeed = int.Parse(jobj["SeekerFinalSpeed"].ToString());
                    GameOptionsManager.Instance.currentHideNSeekGameOptions.SeekerFinalMap = jobj["SeekerFinalMap"].ToString() == "1" ? true : false;
                    GameOptionsManager.Instance.currentHideNSeekGameOptions.SeekerPings = jobj["SeekerPings"].ToString() == "1" ? true : false;
                    GameOptionsManager.Instance.currentHideNSeekGameOptions.MaxPingTime = int.Parse(jobj["MaxPingTime"].ToString());
                }
                else
                    TheOtherRolesPlugin.Logger.LogError("Game Mode is None!!!!!");
                if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.Normal)
                {
                    jobj = jobj["mod_options"];
                    for (JToken current = jobj.First; current != null; current = current.Next)
                    {
                        int id = int.Parse(current["id"].ToString());
                        int value = int.Parse(current["value"].ToString());
                        CustomOption.options.FirstOrDefault(x => x.id == id)?.updateSelection(value, loadPreset: true);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(PassiveButton), nameof(PassiveButton.ReceiveClickDown))]
        class PassiveButtonReveiceClickDown
        {
            public static bool Prefix(PassiveButton __instance)
            {
                if (HaomingMenu.menuObj) return false;
                return true;

            }
        }
        [HarmonyPatch(typeof(UiElement), nameof(UiElement.ReceiveMouseOver))]
        class UiElementReceiveMouseOver
        {
            public static bool Prefix(PassiveButton __instance)
            {
                if (HaomingMenu.menuObj) return false;
                return true;

            }
        }

        [HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
        class LobbyBehaviourStartPatch
        {
            public static void Postfix(LobbyBehaviour __instance)
            {
                var panel = GameObject.Find("Lobby(Clone)/SmallBox/Panel");
                var leftBox = GameObject.Find("Lobby(Clone)/Leftbox");
                var newPanel = GameObject.Instantiate(panel, leftBox.transform);
                var console = newPanel.GetComponentInChildren<OptionsConsole>();
                var obj = new GameObject("HaomingMenu");
                obj.AddComponent<HaomingMenu>();
                console.MenuPrefab = HaomingMenu.prefab;
                leftBox.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            }
        }
    }
}
