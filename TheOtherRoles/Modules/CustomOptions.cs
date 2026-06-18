using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AmongUs.Data;
using AmongUs.GameOptions;
using BepInEx.Configuration;
using HarmonyLib;
using Hazel;
using Il2CppSystem.Linq;
using Reactor.Utilities.Extensions;
using TheOtherRoles.Modules;
using TheOtherRoles.Roles;
using TheOtherRoles.Utilities;
using TMPro;
using UnityEngine;
using static TheOtherRoles.CustomOption;

namespace TheOtherRoles {
    public class CustomOption {
        public enum CustomOptionType {
            General,
            Impostor,
            Neutral,
            Crewmate,
            Modifier,
            Guesser,
            HideNSeekMain,
            HideNSeekRoles
        }

        public static List<CustomOption> options = new();
        public static int preset = 0;
        public static ConfigEntry<string> vanillaSettings;

        public int id;
        public string name;
        public string format;
        public System.Object[] selections;

        public int defaultSelection;
        public ConfigEntry<int> entry;
        public int selection;
        public OptionBehaviour optionBehaviour;
        public CustomOption parent;
        public bool isHeader;
        public CustomOptionType type;
        public Action onChange = null;
        public string heading = "";
        public bool invertedParent;
        public Color color;
        public List<CustomOption> children;

        // Option creation

        public CustomOption(int id, CustomOptionType type, string name,  System.Object[] selections, System.Object defaultValue, CustomOption parent, bool isHeader, string format, Color color, Action onChange = null, string heading = "", bool invertedParent = false) {
            this.id = id;
            this.name = parent == null ? name : "- " + name;
            this.format = format;
            this.selections = selections;
            int index = Array.IndexOf(selections, defaultValue);
            this.defaultSelection = index >= 0 ? index : 0;
            this.parent = parent;
            this.isHeader = isHeader;
            this.type = type;
            this.onChange = onChange;
            this.heading = heading;
            this.invertedParent = invertedParent;
            this.color = color;
            this.children = [];
            if (parent != null) parent.children.Add(this);
            selection = 0;
            if (id != 0) {
                entry = TheOtherRolesPlugin.Instance.Config.Bind($"Preset{preset}", id.ToString(), defaultSelection);
                selection = Mathf.Clamp(entry.Value, 0, selections.Length - 1);
            }
            options.Add(this);
        }

        public static CustomOption Create(int id, CustomOptionType type, string name, string[] selections, CustomOption parent = null, bool isHeader = false, string format = "", Action onChange = null, string heading = "", bool invertedParent = false, Color color = default) {
            return new CustomOption(id, type, name, selections, "", parent, isHeader, format, color == default ? Color.white : color, onChange, heading, invertedParent);
        }

        public static CustomOption Create(int id, CustomOptionType type, string name, float defaultValue, float min, float max, float step, CustomOption parent = null, bool isHeader = false, string format = "", Action onChange = null, string heading = "", bool invertedParent = false, Color color = default) {
            List<object> selections = new();
            for (float s = min; s <= max; s += step)
                selections.Add(s);
            return new CustomOption(id, type, name, selections.ToArray(), defaultValue, parent, isHeader, format, color == default ? color == default ? Color.white : color : color, onChange, heading, invertedParent);
        }

        public static CustomOption Create(int id, CustomOptionType type, string name, bool defaultValue, CustomOption parent = null, bool isHeader = false, string format = "", Action onChange = null, string heading = "", bool invertedParent = false, Color color = default) {
            return new CustomOption(id, type, name, new string[]{ "optionOff", "optionOn" }, defaultValue ? "optionOn" : "optionOff", parent, isHeader, format, Color.white, onChange, heading, invertedParent);
        }

        public static CustomOption Create(int id, CustomOptionType type, string name, List<RoleId> roleId, CustomOption parent = null, bool isHeader = false, Color color = default) {
            return new CustomOption(id, type, name, roleId.Select(x => x == RoleId.Jester ? "optionOff" : RoleInfo.allRoleInfos.FirstOrDefault(y => y.roleId == x
            && y.color != Palette.ImpostorRed && !y.isOrgNeutral).nameKey).ToArray(), 0, parent, isHeader, "", color == default ? Color.white : color);
        }

        // Static behaviour

        public static void switchPreset(int newPreset) {
            saveVanillaOptions();
            CustomOption.preset = newPreset;
            vanillaSettings = TheOtherRolesPlugin.Instance.Config.Bind($"Preset{preset}", "GameOptions", "");
            loadVanillaOptions();
            foreach (CustomOption option in CustomOption.options) {
                if (option.id == 0) continue;

                option.entry = TheOtherRolesPlugin.Instance.Config.Bind($"Preset{preset}", option.id.ToString(), option.defaultSelection);
                option.selection = Mathf.Clamp(option.entry.Value, 0, option.selections.Length - 1);
                if (option.optionBehaviour != null && option.optionBehaviour is StringOption stringOption) {
                    stringOption.oldValue = stringOption.Value = option.selection;
                    stringOption.ValueText.text = option.getString();
                }
            }

            if (AmongUsClient.Instance?.AmHost == true)
            {
                TORRoleSettingsManager.RebuildCurrent();
            }
        }

        public static void saveVanillaOptions() {
            vanillaSettings.Value = Convert.ToBase64String(GameOptionsManager.Instance.gameOptionsFactory.ToBytes(GameManager.Instance.LogicOptions.currentGameOptions, false));
        }

        public static bool loadVanillaOptions() {
            string optionsString = vanillaSettings.Value;
            if (optionsString == "") return false;
            IGameOptions gameOptions = GameOptionsManager.Instance.gameOptionsFactory.FromBytes(Convert.FromBase64String(optionsString));
            if (gameOptions.Version < 8)
            {
                TheOtherRolesPlugin.Logger.LogMessage("tried to paste old settings, not doing this!");
                return false;
            }
            GameOptionsManager.Instance.GameHostOptions = gameOptions;
            GameManager.Instance.LogicOptions.SetGameOptions(gameOptions);
            GameOptionsManager.Instance.CurrentGameOptions = gameOptions;
            GameManager.Instance.LogicOptions.SyncOptions();
            return true;
        }

        public static void ShareOptionChange(uint optionId) {
            var option = options.FirstOrDefault(x => x.id == optionId);
            if (option == null) return;
            var writer = AmongUsClient.Instance!.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareOptions, SendOption.Reliable, -1);
            writer.Write((byte)1);
            writer.WritePacked((uint)option.id);
            writer.WritePacked(Convert.ToUInt32(option.selection));
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void ShareOptionSelections() {
            if (PlayerControl.AllPlayerControls.Count <= 1 || AmongUsClient.Instance!.AmHost == false && PlayerControl.LocalPlayer == null) return;
            var optionsList = new List<CustomOption>(CustomOption.options);
            while (optionsList.Any())
            {
                byte amount = (byte) Math.Min(optionsList.Count, 200); // takes less than 3 bytes per option on average
                var writer = AmongUsClient.Instance!.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareOptions, SendOption.Reliable, -1);
                writer.Write(amount);
                for (int i = 0; i < amount; i++)
                {
                    var option = optionsList[0];
                    optionsList.RemoveAt(0);
                    writer.WritePacked((uint) option.id);
                    writer.WritePacked(Convert.ToUInt32(option.selection));
                }
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
        }

        public static bool ShouldBeEnabled(CustomOption option)
        {
            bool enabled = true;
            var parent = option.parent;
            while (parent != null && enabled)
            {
                enabled = parent.selection != 0 || parent.invertedParent;
                parent = parent.parent;
            }
            return enabled;
        }

        // Getter

        public int getSelection() {
            return selection;
        }

        public bool getBool() {
            return selection > 0;
        }

        public float getFloat() {
            return (float)selections[selection];
        }

        public int getQuantity() {
            return selection + 1;
        }

        public string getString()
        {
            string sel = selections[selection].ToString();
            if (format != "")
            {
                return string.Format(ModTranslation.getString(format), sel);
            }

            if (sel is "optionOn"  or "deputyOnImmediately" or "deputyOnAfterMeeting" or "mayorOnUntilMeeting" or "mayorOnBeforeVoting")
            {
                return "<color=#FFFF00FF>" + ModTranslation.getString(sel) + "</color>";
            }
            else if (sel == "optionOff")
            {
                return "<color=#CCCCCCFF>" + ModTranslation.getString(sel) + "</color>";
            }

            return ModTranslation.getString(sel);
        }

        public string getName()
        {
            return ModTranslation.getString(name);
        }

        public string getHeading()
        {
            if (heading == "") return "";
            return ModTranslation.getString(heading);
        }

        public Color getColor()
        {
            return color;
        }

        // Option changes

        public void updateSelection(int newSelection, bool notifyUsers = true) {
            newSelection = Mathf.Clamp((newSelection + selections.Length) % selections.Length, 0, selections.Length - 1);
            bool doNeedNotifier = AmongUsClient.Instance?.AmClient == true && notifyUsers && selection != newSelection;
            if (doNeedNotifier)
            {
                try
                {
                    selection = newSelection;
                    if (GameStartManager.Instance != null && GameStartManager.Instance.LobbyInfoPane != null && GameStartManager.Instance.LobbyInfoPane.LobbyViewSettingsPane != null && GameStartManager.Instance.LobbyInfoPane.LobbyViewSettingsPane.gameObject.activeSelf)
                    {
                        LobbyViewSettingsPaneChangeTabPatch.Postfix(GameStartManager.Instance.LobbyInfoPane.LobbyViewSettingsPane, GameStartManager.Instance.LobbyInfoPane.LobbyViewSettingsPane.currentTab);
                    }
                    HelpMenu.OnUpdateOptions();
                }
                catch { }
            }
            selection = newSelection;
            try {
                if (onChange != null) onChange();
            } catch { }
            if (doNeedNotifier) {
                CustomOption originalParent = parent;
                if (originalParent != null) {
                    while (originalParent.parent != null)
                        originalParent = originalParent.parent;
                }
                DestroyableSingleton<HudManager>.Instance.Notifier.AddModSettingsChangeMessage((StringNames)(this.id + 6000), getString(),
                    (originalParent != null ? originalParent.getName().Replace("- ", "") + ": " : "") + getName().Replace("- ", ""), false);
            }
            if (optionBehaviour != null && optionBehaviour is StringOption stringOption) {
                stringOption.oldValue = stringOption.Value = selection;
                stringOption.ValueText.text = getString();

                if (AmongUsClient.Instance?.AmHost == true && PlayerControl.LocalPlayer) {
                    if (id == 0 && selection != preset) {
                        switchPreset(selection); // Switch presets
                        ShareOptionSelections();
                    } else if (entry != null) {
                        entry.Value = selection; // Save selection to config
                        ShareOptionChange((uint)id);// Share single selection
                    }
                }
            } else if (id == 0 && AmongUsClient.Instance?.AmHost == true && PlayerControl.LocalPlayer) {  // Share the preset switch for random maps, even if the menu isnt open!
                switchPreset(selection);
                ShareOptionSelections();// Share all selections
            }

            if (AmongUsClient.Instance?.AmHost == true)
            {
                TORRoleSettingsManager.RebuildCurrent();
            }
        }

        public static byte[] serializeOptions() {
            using (MemoryStream memoryStream = new()) {
                using (BinaryWriter binaryWriter = new(memoryStream)) {
                    int lastId = -1;
                    foreach (var option in CustomOption.options.OrderBy(x => x.id)) {
                        if (option.id == 0) continue;
                        bool consecutive = lastId + 1 == option.id;
                        lastId = option.id;

                        binaryWriter.Write((byte)(option.selection + (consecutive ? 128 : 0)));
                        if (!consecutive) binaryWriter.Write((ushort)option.id);
                    }
                    binaryWriter.Flush();
                    memoryStream.Position = 0L;
                    return memoryStream.ToArray();
                }
            }
        }

        public static int deserializeOptions(byte[] inputValues) {
            BinaryReader reader = new(new MemoryStream(inputValues));
            int lastId = -1;
            bool somethingApplied = false;
            int errors = 0;
            while (reader.BaseStream.Position < inputValues.Length) {
                try {
                    int selection = reader.ReadByte();
                    int id = -1;
                    bool consecutive = selection >= 128;
                    if (consecutive) {
                        selection -= 128;
                        id = lastId + 1;
                    } else {
                        id = reader.ReadUInt16();
                    }
                    if (id == 0) continue;
                    lastId = id;
                    CustomOption option = options.First(option => option.id == id);
                    option.entry = TheOtherRolesPlugin.Instance.Config.Bind($"Preset{preset}", option.id.ToString(), option.defaultSelection);
                    option.selection = selection;
                    if (option.optionBehaviour != null && option.optionBehaviour is StringOption stringOption)
                    {
                        stringOption.oldValue = stringOption.Value = option.selection;
                        stringOption.ValueText.text = option.getString();
                    }
                    somethingApplied = true;
                } catch (Exception e) {
                    TheOtherRolesPlugin.Logger.LogWarning($"id:{lastId}:{e}: while deserializing - tried to paste invalid settings!");
                    errors++;
                }
            }
            return Convert.ToInt32(somethingApplied) + (errors > 0 ? 0 : 1);
        }

        // Copy to or paste from clipboard (as string)
        public static void copyToClipboard() {
            GUIUtility.systemCopyBuffer = $"{TheOtherRolesPlugin.VersionString}!{Convert.ToBase64String(serializeOptions())}!{vanillaSettings.Value}";
        }

        public static int pasteFromClipboard() {
            string allSettings = GUIUtility.systemCopyBuffer;
            int torOptionsFine = 0;
            bool vanillaOptionsFine = false;
            try {
                var settingsSplit = allSettings.Split("!");
                Version versionInfo = Version.Parse(settingsSplit[0]);
                string torSettings = settingsSplit[1];
                string vanillaSettingsSub = settingsSplit[2];
                torOptionsFine = deserializeOptions(Convert.FromBase64String(torSettings));

                try
                {
                    if (AmongUsClient.Instance?.AmHost == true)
                    {
                        TORRoleSettingsManager.RebuildCurrent();
                    }
                }
                catch
                { }

                ShareOptionSelections();
                if (TheOtherRolesPlugin.Version > versionInfo && versionInfo < Version.Parse("1.2.7"))
                {
                    vanillaOptionsFine = false;
                    FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, "Host Info: Pasting vanilla settings failed, TOR Options applied!");
                }
                else
                {
                    vanillaSettings.Value = vanillaSettingsSub;
                    vanillaOptionsFine = loadVanillaOptions();
                }
            } catch (Exception e) {
                TheOtherRolesPlugin.Logger.LogWarning($"{e}: tried to paste invalid settings!\n{allSettings}");
                string errorStr = allSettings.Length > 2 ? allSettings.Substring(0, 3) : "(empty clipboard) ";
                FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, $"Host Info: You tried to paste invalid settings: \"{errorStr}...\"");
                SoundEffectsManager.play("fail");
            }
            return Convert.ToInt32(vanillaOptionsFine) + torOptionsFine;
        }
    }

    public class CustomRoleOption : CustomOption
    {
        public CustomOption countOption = null;
        public bool roleEnabled = true;

        public bool enabled
        {
            get
            {
                return roleEnabled && selection > 0;
            }
        }

        public int rate
        {
            get
            {
                return enabled ? selection : 0;
            }
        }

        public int count
        {
            get
            {
                if (!enabled)
                    return 0;

                if (countOption != null)
                    return Mathf.RoundToInt(countOption.getFloat());

                return 1;
            }
        }

        public (int, int) data
        {
            get
            {
                return (rate, count);
            }
        }

        public CustomRoleOption(int id, CustomOptionType type, string name, Color color, int max = 24, bool roleEnabled = true) :
            base(id, type, Helpers.cs(color, name), CustomOptionHolder.rates, "", null, true, "", color)
        {
            this.roleEnabled = roleEnabled;

            if (max <= 0 || !roleEnabled) {
                this.roleEnabled = false;
            }

            if (max > 1)
                countOption = Create(id + 20000, type, "roleNumAssigned", 1f, 1f, max, 1f, this, false, "unitPlayers");
        }
    }

    // ===== ATR-style mod settings via RolesSettingsMenu =====

    [HarmonyPatch(typeof(RolesSettingsMenu), nameof(RolesSettingsMenu.InitialSetup))]
    class RolesSettingsMenuInitialSetupPatch
    {
        public static bool Prefix() => GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek;
    }

    [HarmonyPatch(typeof(RolesSettingsMenu), nameof(RolesSettingsMenu.SetQuotaTab))]
    class RolesSettingsMenuSetQuotaTabPatch
    {
        public static bool Prefix() => GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek;
    }

    [HarmonyPatch(typeof(RolesSettingsMenu), nameof(RolesSettingsMenu.OpenChancesTab))]
    class RolesSettingsMenuOpenChancesTabPatch
    {
        public static bool Prefix(RolesSettingsMenu __instance)
        {
            if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek)
                return true;
            TORRoleSettingsManager.OpenChances(__instance);
            return false;
        }
    }

    [HarmonyPatch(typeof(RolesSettingsMenu), nameof(RolesSettingsMenu.Update))]
    class RolesSettingsMenuUpdatePatch
    {
        public static bool Prefix(RolesSettingsMenu __instance)
        {
            if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek)
                return true;
            TORRoleSettingsManager.OnUpdate(__instance);
            return false;
        }
    }

    static class TORRoleSettingsManager
    {
        static Dictionary<string, PassiveButton> tabButtons = new();
        static Dictionary<string, List<OptionBehaviour>> createdOptions = new();
        static Dictionary<string, List<CategoryHeaderMasked>> createdHeaders = new();
        static string currentCategory = "";
        static bool tabsBuilt = false;

        static string[] GetCategories()
        {
            if (TORMapOptions.gameMode == CustomGamemodes.HideNSeek) return new string[0];
            var cats = new List<string> { "general" };
            if (TORMapOptions.gameMode == CustomGamemodes.Guesser) cats.Add("guesser");
            cats.Add("impostor"); cats.Add("neutral"); cats.Add("crewmate"); cats.Add("modifier");
            return cats.ToArray();
        }

        static CustomOptionType CatToType(string cat) => cat switch
        {
            "general" => CustomOptionType.General, "guesser" => CustomOptionType.Guesser,
            "impostor" => CustomOptionType.Impostor, "neutral" => CustomOptionType.Neutral,
            "crewmate" => CustomOptionType.Crewmate, "modifier" => CustomOptionType.Modifier,
            _ => CustomOptionType.General,
        };

        static List<CustomOption> FilterForCategory(string cat)
        {
            var t = CatToType(cat);
            var list = (t == CustomOptionType.General && TORMapOptions.gameMode == CustomGamemodes.Guesser)
                ? CustomOption.options.Where(x => x.type == CustomOptionType.General || x.type == CustomOptionType.Guesser).ToList()
                : CustomOption.options.Where(x => x.type == t).ToList();
            if (TORMapOptions.gameMode == CustomGamemodes.Guesser)
                list = list.Where(x => !(new List<int> { 310, 311, 312, 313, 314, 315, 316, 317, 318, 319, 7006 }).Contains(x.id)).ToList();
            else if (t == CustomOptionType.General)
                list = list.Where(x => x.id != 7007).ToList();
            if (TORMapOptions.gameMode != CustomGamemodes.FreePlay)
                list = list.Where(x => x.id != 10429).ToList();
            return list;
        }

        public static void OpenChances(RolesSettingsMenu menu)
        {
            if (!tabsBuilt) BuildTabs(menu);

            menu.RoleChancesSettings.SetActive(false);
            menu.AdvancedRolesSettings.SetActive(true);
            var bg = menu.AdvancedRolesSettings.transform.Find("Background");
            if (bg != null) bg.gameObject.SetActive(false);
            var ilb = menu.AdvancedRolesSettings.transform.Find("InfoLabelBackground");
            if (ilb != null) ilb.gameObject.SetActive(false);
            var db = menu.AdvancedRolesSettings.transform.Find("DescBackground");
            if (db != null) db.gameObject.SetActive(false);
            var ib = menu.AdvancedRolesSettings.transform.Find("Imagebackground");
            if (ib != null) ib.gameObject.SetActive(false);

            SwitchCategory(menu, "general");
        }

        static void BuildTabs(RolesSettingsMenu menu)
        {
            foreach (var kv in tabButtons)
                if (kv.Value != null && kv.Value.gameObject != null) kv.Value.gameObject.Destroy();
            tabButtons.Clear();
            menu.AllButton?.gameObject.SetActive(false);
            menu.advHeader?.gameObject.SetActive(false);

            float xPos = -1.928f - 0.762f;
            foreach (string cat in GetCategories())
            {
                var tab = UnityEngine.Object.Instantiate<RoleSettingsTabButton>(
                    menu.roleSettingsTabButtonOrigin, Vector3.zero, Quaternion.identity, menu.roleSettingsTabParent);
                tab.transform.localPosition = new Vector3(xPos, 2.275f, -2f);
                tab.icon.color = Color.white;
                if (tab.Button != null)
                {
                    tab.Button.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                    string c = cat;
                    tab.Button.OnClick.AddListener((Action)(() => SwitchCategory(menu, c)));
                }
                tabButtons[cat] = tab.Button;
                xPos += 0.762f;
            }
            tabsBuilt = true;
        }

        static CategoryHeaderMasked headerPrefab;
        static CategoryHeaderMasked HeaderPrefab
        {
            get
            {
                if (headerPrefab == null)
                {
                    var gom = UnityEngine.Object.FindObjectOfType<GameOptionsMenu>(true);
                    if (gom != null) headerPrefab = gom.categoryHeaderOrigin;
                }
                return headerPrefab;
            }
        }

        static void SwitchCategory(RolesSettingsMenu menu, string category)
        {
            if (createdHeaders.ContainsKey(currentCategory))
                foreach (var h in createdHeaders[currentCategory])
                    if (h != null && h.gameObject != null) h.gameObject.Destroy();
            if (createdOptions.ContainsKey(currentCategory))
                foreach (var o in createdOptions[currentCategory])
                    if (o != null && o.gameObject != null) o.gameObject.Destroy();
            if (createdHeaders.ContainsKey(currentCategory)) createdHeaders[currentCategory].Clear();
            if (createdOptions.ContainsKey(currentCategory)) createdOptions[currentCategory].Clear();

            currentCategory = category;

            foreach (var kv in tabButtons)
                if (kv.Value != null) kv.Value.SelectButton(kv.Key == category);

            menu.advHeader?.gameObject.SetActive(false);
            menu.roleTitleText?.gameObject.SetActive(false);
            menu.roleDescriptionText?.gameObject.SetActive(false);
            menu.roleHeaderSprite?.gameObject.SetActive(false);
            menu.roleScreenshot?.gameObject.SetActive(false);

            var opts = FilterForCategory(category);
            if (HeaderPrefab != null && menu.stringOptionOrigin != null)
                CreateSettingsInContainer(menu, opts);
        }

        static void CreateSettingsInContainer(RolesSettingsMenu menu, List<CustomOption> options)
        {
            var parent = menu.AdvancedRolesSettings.transform;
            var clickMask = menu.ButtonClickMask;

            if (!createdHeaders.ContainsKey(currentCategory))
                createdHeaders[currentCategory] = new List<CategoryHeaderMasked>();
            if (!createdOptions.ContainsKey(currentCategory))
                createdOptions[currentCategory] = new List<OptionBehaviour>();

            float num = 0.8f;
            foreach (CustomOption option in options)
            {
                if (option.isHeader)
                {
                    var header = UnityEngine.Object.Instantiate<CategoryHeaderMasked>(HeaderPrefab, Vector3.zero, Quaternion.identity, parent);
                    header.Title.text = option.heading != "" ? option.getHeading() : option.getName();
                    header.Background.material.SetInt(PlayerMaterial.MaskLayer, 20);
                    if (header.Divider != null) header.Divider.material.SetInt(PlayerMaterial.MaskLayer, 20);
                    header.Title.fontMaterial.SetFloat("_StencilComp", 3f);
                    header.Title.fontMaterial.SetFloat("_Stencil", 20f);
                    header.transform.localScale = Vector3.one * 0.63f;
                    header.transform.localPosition = new Vector3(-0.55f, num, -2f);
                    header.transform.GetChild(0).GetComponent<SpriteRenderer>().color = option.getColor();
                    if (header.transform.childCount > 1) header.transform.GetChild(1).GetComponent<SpriteRenderer>().color = option.getColor();
                    createdHeaders[currentCategory].Add(header);
                    num -= 0.63f;
                }
                else if (!ShouldBeEnabled(option)) continue;
                else if (option.parent != null && option.parent.selection != 0 && option.invertedParent) continue;

                var ob = UnityEngine.Object.Instantiate<StringOption>(menu.stringOptionOrigin, Vector3.zero, Quaternion.identity, parent);
                ob.transform.localPosition = new Vector3(1.28f, num, -2f);
                ob.SetClickMask(clickMask);

                SpriteRenderer[] sr = ob.GetComponentsInChildren<SpriteRenderer>(true);
                for (int i = 0; i < sr.Length; i++) sr[i].material.SetInt(PlayerMaterial.MaskLayer, 20);
                foreach (TextMeshPro tmp in ob.GetComponentsInChildren<TextMeshPro>(true))
                { tmp.fontMaterial.SetFloat("_StencilComp", 3f); tmp.fontMaterial.SetFloat("_Stencil", 20f); }

                var so = ob as StringOption;
                so.OnValueChanged = new Action<OptionBehaviour>((o) => { });
                so.TitleText.text = option.getName();
                if (option.heading == "" && (option.type == CustomOptionType.Neutral || option.type == CustomOptionType.Crewmate || option.type == CustomOptionType.Impostor || option.type == CustomOptionType.Modifier))
                    so.TitleText.text = ModTranslation.getString("optionSpawnChance");
                if (so.TitleText.text.Length > 25) so.TitleText.fontSize = 2.2f;
                if (so.TitleText.text.Length > 40) so.TitleText.fontSize = 2f;
                so.Value = so.oldValue = option.selection;
                so.ValueText.text = option.getString();
                option.optionBehaviour = so;

                createdOptions[currentCategory].Add(ob);
                num -= 0.45f;
                menu.scrollBar.SetYBoundsMax(-num - 1.65f);
            }

            for (int i = 0; i < createdOptions[currentCategory].Count; i++)
                if (AmongUsClient.Instance && !AmongUsClient.Instance.AmHost)
                    createdOptions[currentCategory][i].SetAsPlayer();
        }

        public static void RebuildCurrent()
        {
            if (string.IsNullOrEmpty(currentCategory)) return;
            var menu = UnityEngine.Object.FindObjectOfType<RolesSettingsMenu>();
            if (menu != null) SwitchCategory(menu, currentCategory);
        }

        public static void OnUpdate(RolesSettingsMenu menu)
        {
        }
    }

    [HarmonyPatch(typeof(LobbyViewSettingsPane), nameof(LobbyViewSettingsPane.SetTab))]
    class LobbyViewSettingsPaneRefreshTabPatch
    {
        public static bool Prefix(LobbyViewSettingsPane __instance)
        {
            if ((int)__instance.currentTab < 15)
            {
                LobbyViewSettingsPaneChangeTabPatch.Postfix(__instance, __instance.currentTab);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(LobbyViewSettingsPane), nameof(LobbyViewSettingsPane.ChangeTab))]
    class LobbyViewSettingsPaneChangeTabPatch
    {
        public static void Postfix(LobbyViewSettingsPane __instance, StringNames category)
        {
            int tabNum = (int)category;

            foreach (var pbutton in LobbyViewSettingsPatch.currentButtons)
            {
                pbutton.SelectButton(false);
            }
            if (tabNum > 20) // StringNames are in the range of 3000+ 
                return;
            __instance.taskTabButton.SelectButton(false);

            if (tabNum > 2)
            {
                tabNum -= 3;
                //GameOptionsMenuStartPatch.currentTabs[tabNum].SetActive(true);
                LobbyViewSettingsPatch.currentButtons[tabNum].SelectButton(true);
                LobbyViewSettingsPatch.drawTab(__instance, LobbyViewSettingsPatch.currentButtonTypes[tabNum]);
            }
        }
    }

    [HarmonyPatch(typeof(LobbyViewSettingsPane), nameof(LobbyViewSettingsPane.Update))]
    class LobbyViewSettingsPaneUpdatePatch
    {
        public static void Postfix(LobbyViewSettingsPane __instance)
        {
            if (LobbyViewSettingsPatch.currentButtons.Count == 0)
            {
                LobbyViewSettingsPatch.gameModeChangedFlag = true;
                LobbyViewSettingsPatch.Postfix(__instance);

            }
        }
    }

    [HarmonyPatch(typeof(LobbyViewSettingsPane), nameof(LobbyViewSettingsPane.Awake))]
    class LobbyViewSettingsPatch
    {
        public static List<PassiveButton> currentButtons = new();
        public static List<CustomOptionType> currentButtonTypes = new();
        public static bool gameModeChangedFlag = false;

        public static void createCustomButton(LobbyViewSettingsPane __instance, int targetMenu, string buttonName, string buttonText, CustomOptionType optionType)
        {
            buttonName = "View" + buttonName;
            var buttonTemplate = GameObject.Find("OverviewTab");
            var torSettingsButton = GameObject.Find(buttonName);
            if (torSettingsButton == null)
            {
                torSettingsButton = GameObject.Instantiate(buttonTemplate, buttonTemplate.transform.parent);
                torSettingsButton.transform.localPosition += Vector3.right * 1.75f * (targetMenu - 2);
                torSettingsButton.name = buttonName;
                __instance.StartCoroutine(Effects.Lerp(2f, new Action<float>(p => { torSettingsButton.transform.FindChild("FontPlacer").GetComponentInChildren<TextMeshPro>().text = buttonText; })));
                var torSettingsPassiveButton = torSettingsButton.GetComponent<PassiveButton>();
                torSettingsPassiveButton.OnClick.RemoveAllListeners();
                torSettingsPassiveButton.OnClick.AddListener((System.Action)(() => {
                    __instance.ChangeTab((StringNames)targetMenu);
                }));
                torSettingsPassiveButton.OnMouseOut.RemoveAllListeners();
                torSettingsPassiveButton.OnMouseOver.RemoveAllListeners();
                torSettingsPassiveButton.SelectButton(false);
                currentButtons.Add(torSettingsPassiveButton);
                currentButtonTypes.Add(optionType);
            }
        }

        public static void Postfix(LobbyViewSettingsPane __instance)
        {
            currentButtons.ForEach(x => { if (x != null) x?.Destroy(); });
            currentButtons.Clear();
            currentButtonTypes.Clear();

            removeVanillaTabs(__instance);

            createSettingTabs(__instance);

        }

        public static void removeVanillaTabs(LobbyViewSettingsPane __instance)
        {
            GameObject.Find("RolesTabs")?.Destroy();
            var overview = GameObject.Find("OverviewTab");
            if (!gameModeChangedFlag)
            {
                overview.transform.localScale = new Vector3(0.5f * overview.transform.localScale.x, overview.transform.localScale.y, overview.transform.localScale.z);
                overview.transform.localPosition += new Vector3(-1.2f, 0f, 0f);

            }
            overview.transform.Find("FontPlacer").transform.localScale = new Vector3(1.35f, 1f, 1f);
            overview.transform.Find("FontPlacer").transform.localPosition = new Vector3(-0.6f, -0.1f, 0f);
            gameModeChangedFlag = false;
        }

        public static void drawTab(LobbyViewSettingsPane __instance, CustomOptionType optionType)
        {

            var relevantOptions = options.Where(x => x.type == optionType || x.type == CustomOption.CustomOptionType.Guesser && optionType == CustomOptionType.General).ToList();

            if ((int)optionType == 99)
            {
                // Create 4 Groups with Role settings only
                relevantOptions.Clear();
                relevantOptions.AddRange(options.Where(x => x.type == CustomOptionType.Impostor && x.isHeader));
                relevantOptions.AddRange(options.Where(x => x.type == CustomOptionType.Neutral && x.isHeader));
                relevantOptions.AddRange(options.Where(x => x.type == CustomOptionType.Crewmate && x.isHeader));
                relevantOptions.AddRange(options.Where(x => x.type == CustomOptionType.Modifier && x.isHeader));
                foreach (var option in options)
                {
                    if (option.parent != null && option.parent.getSelection() > 0)
                    {
                        if (option.id == 103) //Deputy
                            relevantOptions.Insert(relevantOptions.IndexOf(CustomOptionHolder.sheriffSpawnRate) + 1, option);
                        else if (option.id == 224) //Sidekick
                            relevantOptions.Insert(relevantOptions.IndexOf(CustomOptionHolder.jackalSpawnRate) + 1, option);
                        else if (option.id == 918) // Immoralist
                            relevantOptions.Insert(relevantOptions.IndexOf(CustomOptionHolder.foxSpawnRate) + 1, option);
                        else if (option.id == 8000) //Prosecutor
                            relevantOptions.Insert(relevantOptions.IndexOf(CustomOptionHolder.evilHackerSpawnRate) + 1, option);
                    }
                }
            }

            if (TORMapOptions.gameMode == CustomGamemodes.Guesser) // Exclude guesser options in neutral mode
                relevantOptions = [.. relevantOptions.Where(x => !new List<int> { 310, 311, 312, 313, 314, 315, 316, 317, 318, 319, 7006 }.Contains(x.id))];
            else
                relevantOptions = relevantOptions.Where(x => x.id != 7007).ToList();

            if (TORMapOptions.gameMode != CustomGamemodes.FreePlay)
                relevantOptions = relevantOptions.Where(x => x.id != 10429).ToList();

            for (int j = 0; j < __instance.settingsInfo.Count; j++)
            {
                __instance.settingsInfo[j].gameObject.Destroy();
            }
            __instance.settingsInfo.Clear();

            float num = 1.44f;
            int i = 0;
            int singles = 1;
            int headers = 0;
            int lines = 0;
            var curType = CustomOptionType.Modifier;
            int numBonus = 0;

            foreach (var option in relevantOptions)
            {
                if (option.isHeader && (int)optionType != 99 || (int)optionType == 99 && curType != option.type)
                {
                    curType = option.type;
                    if (i != 0) {
                        num -= 0.85f;
                        numBonus++;
                    }
                    if (i % 2 != 0) singles++;
                    headers++; // for header
                    CategoryHeaderMasked categoryHeaderMasked = UnityEngine.Object.Instantiate<CategoryHeaderMasked>(__instance.categoryHeaderOrigin);
                    categoryHeaderMasked.SetHeader(StringNames.ImpostorsCategory, 61);
                    string titleText = option.heading != "" ? option.getHeading() : option.getName();
                    categoryHeaderMasked.Title.text = titleText;
                    if ((int)optionType == 99)
                        categoryHeaderMasked.Title.text = new Dictionary<CustomOptionType, string>() { { CustomOptionType.Impostor, ModTranslation.getString("impostorRoles") }, { CustomOptionType.Neutral, ModTranslation.getString("neutralRoles") },
                            { CustomOptionType.Crewmate, ModTranslation.getString("crewmateRoles") }, { CustomOptionType.Modifier, ModTranslation.getString("modifiers") } }[curType];
                    categoryHeaderMasked.transform.SetParent(__instance.settingsContainer);
                    categoryHeaderMasked.transform.localScale = Vector3.one;
                    categoryHeaderMasked.transform.localPosition = new Vector3(-9.77f, num, -2f);
                    __instance.settingsInfo.Add(categoryHeaderMasked.gameObject);
                    if ((int)optionType != 99) {
                        categoryHeaderMasked.transform.GetChild(0).GetComponent<SpriteRenderer>().color = option.getColor();
                        categoryHeaderMasked.transform.GetChild(1).GetComponent<SpriteRenderer>().color = option.getColor();
                    }
                    num -= 1.05f;
                    i = 0;
                }
                else if (!ShouldBeEnabled(option)) continue;  // Hides options, for which the parent is disabled!
                if (option == CustomOptionHolder.crewmateRolesCountMax || option == CustomOptionHolder.neutralRolesCountMax || option == CustomOptionHolder.impostorRolesCountMax || option == CustomOptionHolder.modifiersCountMax || option == CustomOptionHolder.crewmateRolesFill)
                    continue;

                ViewSettingsInfoPanel viewSettingsInfoPanel = UnityEngine.Object.Instantiate<ViewSettingsInfoPanel>(__instance.infoPanelOrigin);
                viewSettingsInfoPanel.transform.SetParent(__instance.settingsContainer);
                viewSettingsInfoPanel.transform.localScale = Vector3.one;
                float num2;
                if (i % 2 == 0) {
                    lines++;
                    num2 = -8.95f;
                    if (i > 0) {
                        num -= 0.85f;
                    }
                }
                else {
                    num2 = -3f;
                }
                viewSettingsInfoPanel.transform.localPosition = new Vector3(num2, num, -2f);
                int value = option.getSelection();
                var settingTuple = handleSpecialOptionsView(option, option.getName(), option.getString());
                viewSettingsInfoPanel.SetInfo(StringNames.ImpostorsCategory, settingTuple.Item2, 61);
                viewSettingsInfoPanel.titleText.text = settingTuple.Item1;
                if (option.isHeader && (int)optionType != 99 && option.heading == "" && (option.type == CustomOptionType.Neutral || option.type == CustomOptionType.Crewmate || option.type == CustomOptionType.Impostor || option.type == CustomOptionType.Modifier)) {
                    viewSettingsInfoPanel.titleText.text = ModTranslation.getString("optionSpawnChance");
                }
                if ((int)optionType == 99) {
                    if (option.type == CustomOptionType.Modifier)
                        viewSettingsInfoPanel.settingText.text = viewSettingsInfoPanel.settingText.text + GameOptionsDataPatch.buildModifierExtras(option);
                    if (option is CustomRoleOption roleOption)
                        viewSettingsInfoPanel.settingText.text += roleOption.enabled && roleOption.countOption != null ? $" ({roleOption.count})" : "";
                }
                __instance.settingsInfo.Add(viewSettingsInfoPanel.gameObject);

                i++;
            }
            float actual_spacing = (headers * 1.05f + lines * 0.85f) / (headers + lines) * 1.01f;
            __instance.scrollBar.CalculateAndSetYBounds(__instance.settingsInfo.Count + singles * 2 + headers, 2f, 6f, actual_spacing);

        }
        private static Tuple<string, string> handleSpecialOptionsView(CustomOption option, string defaultString, string defaultVal)
        {
            string name = defaultString;
            string val = defaultVal;
            if (option == CustomOptionHolder.crewmateRolesCountMin)
            {
                val = "";
                name = ModTranslation.getString("crewmateRoles");
                var min = CustomOptionHolder.crewmateRolesCountMin.getSelection();
                var max = CustomOptionHolder.crewmateRolesCountMax.getSelection();
                if (CustomOptionHolder.crewmateRolesFill.getBool())
                {
                    var crewCount = PlayerControl.AllPlayerControls.Count - GameOptionsManager.Instance.currentGameOptions.NumImpostors;
                    int minNeutral = CustomOptionHolder.neutralRolesCountMin.getSelection();
                    int maxNeutral = CustomOptionHolder.neutralRolesCountMax.getSelection();
                    if (minNeutral > maxNeutral) minNeutral = maxNeutral;
                    min = crewCount - maxNeutral;
                    max = crewCount - minNeutral;
                    if (min < 0) min = 0;
                    if (max < 0) max = 0;
                    val = ModTranslation.getString("crewmateFill");
                }
                if (min > max) min = max;
                val += (min == max) ? $"{max}" : $"{min} - {max}";
            }
            if (option == CustomOptionHolder.neutralRolesCountMin)
            {
                name = ModTranslation.getString("neutralRoles");
                var min = CustomOptionHolder.neutralRolesCountMin.getSelection();
                var max = CustomOptionHolder.neutralRolesCountMax.getSelection();
                if (min > max) min = max;
                val = (min == max) ? $"{max}" : $"{min} - {max}";
            }
            if (option == CustomOptionHolder.impostorRolesCountMin)
            {
                name = ModTranslation.getString("impostorRoles");
                var min = CustomOptionHolder.impostorRolesCountMin.getSelection();
                var max = CustomOptionHolder.impostorRolesCountMax.getSelection();
                if (max > GameOptionsManager.Instance.currentGameOptions.NumImpostors) max = GameOptionsManager.Instance.currentGameOptions.NumImpostors;
                if (min > max) min = max;
                val = (min == max) ? $"{max}" : $"{min} - {max}";
            }
            if (option == CustomOptionHolder.modifiersCountMin)
            {
                name = ModTranslation.getString("modifiers");
                var min = CustomOptionHolder.modifiersCountMin.getSelection();
                var max = CustomOptionHolder.modifiersCountMax.getSelection();
                if (min > max) min = max;
                val = (min == max) ? $"{max}" : $"{min} - {max}";
            }
            return new(name, val);
        }

        public static void createSettingTabs(LobbyViewSettingsPane __instance)
        {
            // Handle different gamemodes and tabs needed therein.
            int next = 3;
            if (TORMapOptions.gameMode == CustomGamemodes.Guesser || TORMapOptions.gameMode == CustomGamemodes.Classic || TORMapOptions.gameMode == CustomGamemodes.FreePlay)
            {

                // create TOR settings
                createCustomButton(__instance, next++, "TORSettings", ModTranslation.getString("torNewSettings"), CustomOptionType.General);
                // create TOR settings
                createCustomButton(__instance, next++, "RoleOverview", ModTranslation.getString("roleOverview"), (CustomOptionType)99);
                // IMp
                createCustomButton(__instance, next++, "ImpostorSettings", ModTranslation.getString("impostorRoles"), CustomOptionType.Impostor);

                // Neutral
                createCustomButton(__instance, next++, "NeutralSettings", ModTranslation.getString("neutralRoles"), CustomOptionType.Neutral);
                // Crew
                createCustomButton(__instance, next++, "CrewmateSettings", ModTranslation.getString("crewmateRoles"), CustomOptionType.Crewmate);
                // Modifier
                createCustomButton(__instance, next++, "ModifierSettings", ModTranslation.getString("modifiers"), CustomOptionType.Modifier);

            }
            else if (TORMapOptions.gameMode == CustomGamemodes.HideNSeek)
            {
                // create Main HNS settings
                createCustomButton(__instance, next++, "HideNSeekMain", ModTranslation.getString("hideNSeekMain"), CustomOptionType.HideNSeekMain);
                // create HNS Role settings
                createCustomButton(__instance, next++, "HideNSeekRoles", ModTranslation.getString("hideNSeekRoles"), CustomOptionType.HideNSeekRoles);
            }
        }
    }

    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.CreateSettings))]
    class GameOptionsMenuCreateSettingsPatch
    {
        public static void Postfix(GameOptionsMenu __instance)
        {
            if (__instance.gameObject.name == "GAME SETTINGS TAB")
                adaptTaskCount(__instance);
        }

        private static void adaptTaskCount(GameOptionsMenu __instance)
        {
            // Adapt task count for main options
            var commonTasksOption = __instance.Children.ToArray().FirstOrDefault(x => x.TryCast<NumberOption>()?.intOptionName == Int32OptionNames.NumCommonTasks).Cast<NumberOption>();
            if (commonTasksOption != null) commonTasksOption.ValidRange = new FloatRange(0f, 4f);
            var shortTasksOption = __instance.Children.ToArray().FirstOrDefault(x => x.TryCast<NumberOption>()?.intOptionName == Int32OptionNames.NumShortTasks).TryCast<NumberOption>();
            if (shortTasksOption != null) shortTasksOption.ValidRange = new FloatRange(0f, 23f);
            var longTasksOption = __instance.Children.ToArray().FirstOrDefault(x => x.TryCast<NumberOption>()?.intOptionName == Int32OptionNames.NumLongTasks).TryCast<NumberOption>();
            if (longTasksOption != null) longTasksOption.ValidRange = new FloatRange(0f, 15f);
        }
    }

    class CreateGameOptionsTORBehaviour : MonoBehaviour
    {
        static CreateGameOptionsTORBehaviour() => ClassInjector.RegisterTypeInIl2Cpp<CreateGameOptionsTORBehaviour>();

        public CreateOptionsPicker MyPicker;

        void Awake()
        {
            MyPicker = gameObject.GetComponent<CreateOptionsPicker>();

            MyPicker.transform.FindChild("Game Mode").gameObject.SetActive(false);

            var impostorsRoot = MyPicker.transform.FindChild("Impostors");
            if (impostorsRoot)
            {
                impostorsRoot.transform.localPosition = new(-1.955f, -0.44f, 0f);

                var temp = impostorsRoot.transform.GetChild(1);

                var list = MyPicker.ImpostorButtons.ToList();
                for (int i = 4; i <= 6; i++)
                {
                    var obj = GameObject.Instantiate(temp, impostorsRoot);
                    obj.name = i.ToString();
                    obj.transform.localPosition = new((i - 1) * 0.6f, 0f, 0f);
                    obj.GetChild(0).GetComponent<TextMeshPro>().text = i.ToString();
                    var passiveButton = obj.gameObject.GetComponent<PassiveButton>();
                    passiveButton.OnClick = new();
                    int impostors = i;
                    passiveButton.OnClick.AddListener((Action)(() => MyPicker.SetImpostorButtons(impostors)));

                    list.Add(obj.GetComponent<ImpostorsOptionButton>());
                }

                MyPicker.ImpostorButtons = list.ToArray();
            }
        }

        void OnEnable()
        {
            if (!MyPicker) return;

            bool isCustomServer = Helpers.isCustomServer();

            if (MyPicker.MaxPlayersRoot)
            {
                //以前のボタンを削除する
                MyPicker.optionsMenu.ControllerSelectable.Clear();
                MyPicker.MaxPlayerButtons.Clear();

                Helpers.Sequential(MyPicker.MaxPlayersRoot.childCount).Skip(1).Select(i => MyPicker.MaxPlayersRoot.GetChild(i).gameObject).ToArray().Do(GameObject.Destroy);

                for (int i = 4; i <= (isCustomServer ? 24 : 15); i++)
                {
                    SpriteRenderer spriteRenderer = GameObject.Instantiate<SpriteRenderer>(MyPicker.MaxPlayerButtonPrefab, MyPicker.MaxPlayersRoot);
                    spriteRenderer.transform.localPosition = new Vector3((i - 4) % 12 * 0.5f, i / 16 * -0.47f, 0f);
                    int numPlayers = i;
                    spriteRenderer.name = numPlayers.ToString();
                    PassiveButton component = spriteRenderer.GetComponent<PassiveButton>();
                    component.OnClick.AddListener((Action)(() => MyPicker.SetMaxPlayersButtons(numPlayers)));
                    spriteRenderer.GetComponentInChildren<TextMeshPro>().text = numPlayers.ToString();
                    MyPicker.MaxPlayerButtons.Add(spriteRenderer);
                    MyPicker.optionsMenu.ControllerSelectable.Add(component);
                }
            }

            var subMenu = MyPicker.transform.FindChild("SubMenu");
            subMenu.transform.localPosition = new(1.11f, isCustomServer ? -0.4f : 0f, 0f);
            subMenu.GetComponent<ShiftButtonsCrossplayEnabled>().enabled = false;

            //4人以上のオプションはカスタムサーバーのみ使用可能
            for (int i = 4; i <= 6; i++) MyPicker.ImpostorButtons[i - 1].gameObject.SetActive(isCustomServer);

            var options = MyPicker.GetTargetOptions();
            MyPicker.SetMaxPlayersButtons(isCustomServer ? 24 : 15);
        }
    }

    [HarmonyPatch(typeof(CreateGameOptions), nameof(CreateGameOptions.UpdateServerText))]
    public static class CreateGameOptionsUpdateRegionPatch
    {
        private static void Postfix(CreateGameOptions __instance)
        {
            __instance.capacityOption.ValidRange.max = Helpers.isCustomServer() ? 24f : 15f;
            __instance.capacityOption.Value = __instance.capacityOption.ValidRange.Clamp(__instance.capacityOption.Value);
            __instance.capacityOption.UpdateValue();
            __instance.capacityOption.AdjustButtonsActiveState();
        }
    }

    [HarmonyPatch(typeof(CreateOptionsPicker), nameof(CreateOptionsPicker.Awake))]
    class CreateGameOptionsShowPatch
    {
        public static bool Prefix(CreateOptionsPicker __instance)
        {
            // Set MaxImpostors values
            int[] maxImpostors = Helpers.MaxImpostors;
            LegacyGameOptions.MaxImpostors = maxImpostors;
            NormalGameOptionsV10.MaxImpostors = maxImpostors;
            NormalGameOptionsV09.MaxImpostors = maxImpostors;
            NormalGameOptionsV08.MaxImpostors = maxImpostors;
            NormalGameOptionsV07.MaxImpostors = maxImpostors;

            // Set RecommendedImpostors values
            int[] recommendedImpostors = Helpers.RecommendedImpostors;
            LegacyGameOptions.RecommendedImpostors = recommendedImpostors;
            NormalGameOptionsV10.RecommendedImpostors = recommendedImpostors;
            NormalGameOptionsV09.RecommendedImpostors = recommendedImpostors;
            NormalGameOptionsV08.RecommendedImpostors = recommendedImpostors;
            NormalGameOptionsV07.RecommendedImpostors = recommendedImpostors;

            // Set RecommendedKillCooldown values
            int[] recommendedKillCooldown = Helpers.RecommendedKillCooldown;
            LegacyGameOptions.RecommendedKillCooldown = recommendedKillCooldown;
            NormalGameOptionsV10.RecommendedKillCooldown = recommendedKillCooldown;
            NormalGameOptionsV09.RecommendedKillCooldown = recommendedKillCooldown;
            NormalGameOptionsV08.RecommendedKillCooldown = recommendedKillCooldown;
            NormalGameOptionsV07.RecommendedKillCooldown = recommendedKillCooldown;

            // Set MinPlayers values
            int[] minPlayers = Helpers.MinPlayers;
            LegacyGameOptions.MinPlayers = minPlayers;
            NormalGameOptionsV10.MinPlayers = minPlayers;
            NormalGameOptionsV09.MinPlayers = minPlayers;
            NormalGameOptionsV08.MinPlayers = minPlayers;
            NormalGameOptionsV07.MinPlayers = minPlayers;

            //ゲームモードはノーマル固定
            DataManager.Settings.Multiplayer.LastPlayedGameMode = AmongUs.GameOptions.GameModes.Normal;
            DataManager.Settings.Save();
            GameOptionsManager.Instance.SwitchGameMode(AmongUs.GameOptions.GameModes.Normal);

            __instance.gameObject.AddComponent<CreateGameOptionsTORBehaviour>();

            return false;
        }
    }

    [HarmonyPatch(typeof(CreateOptionsPicker), nameof(CreateOptionsPicker.Refresh))]
    internal class CreateGameOptionsStartPatch
    {
        private static int impostors = 1;

        public static void Prefix(CreateOptionsPicker __instance)
        {
            impostors = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumImpostors);
            Debug.Log("Impostors(A): " + impostors.ToString());
        }

        public static void Postfix(CreateOptionsPicker __instance)
        {
            impostors = Math.Min(impostors, Helpers.isCustomServer() ? 6 : 3);
            __instance.SetImpostorButtons(impostors);
            Debug.Log("Impostors(B): " + impostors.ToString());
        }
    }

    [HarmonyPatch(typeof(CreateGameOptions), nameof(CreateGameOptions.Confirm))]
    public static class CreateGameOptionsConfirmPatch
    {
        private static bool Prefix(CreateGameOptions __instance)
        {
            if (!DestroyableSingleton<MatchMaker>.Instance.Connecting(__instance))
                return false;
            GameOptionsManager.Instance.GameHostOptions.TryGetInt(Int32OptionNames.MaxPlayers, out int index);
            int[] maxImpostors = Helpers.MaxImpostors;
            GameOptionsManager.Instance.GameHostOptions.TryGetInt(Int32OptionNames.NumImpostors, out int num);
            if (num > maxImpostors[index])
                GameOptionsManager.Instance.GameHostOptions.SetInt(Int32OptionNames.NumImpostors, maxImpostors[index]);
            if (num == 0)
                GameOptionsManager.Instance.GameHostOptions.SetInt(Int32OptionNames.NumImpostors, 1);
            __instance.CoStartGame();
            return false;
        }
    }

    [HarmonyPatch(typeof(CreateGameOptions), nameof(CreateGameOptions.OpenConfirmPopup))]
    static class CreateGameOptionsOpenConfirmPopupPatch
    {
        static void Postfix(CreateGameOptions __instance)
        {
            __instance.containerConfirm.GetChild(10).gameObject.SetActive(false);
            __instance.containerConfirm.GetChild(8).localPosition = new(4f, - 0.47f, - 0.1f);
            __instance.containerConfirm.GetChild(5).GetChild(2).GetComponent<TextMeshPro>().SetText(
                TORMapOptions.gameMode is CustomGamemodes.Classic ? DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameTypeClassic) :
                (TORMapOptions.gameMode is CustomGamemodes.Guesser ? ModTranslation.getString("gamemodeGuesser") : ModTranslation.getString("gamemodeHideNSeek")));
        }
    }

    [HarmonyPatch(typeof(CreateGameOptions), nameof(CreateGameOptions.Show))]
    static class CreateGameOptionsOpenShowPatch
    {
        static void Postfix(CreateGameOptions __instance)
        {
            if ((CreateGameOptionsPatch.modeButtonGS != null && CreateGameOptionsPatch.modeButtonGS.IsSelected()) ||
                (CreateGameOptionsPatch.modeButtonHK != null && CreateGameOptionsPatch.modeButtonHK.IsSelected()))
                __instance.modeButtons[0].SelectButton(false);
        }
    }

    [HarmonyPatch(typeof(CreateGameOptions), nameof(CreateGameOptions.Start))]
    public static class CreateGameOptionsPatch
    {
        public static PassiveButton modeButtonGS;
        public static PassiveButton modeButtonHK;

        private static void Postfix(CreateGameOptions __instance)
        {
            __instance.tooltip.transform.parent.gameObject.SetActive(false);
            __instance.mapPicker.transform.SetLocalY(-1.245f);
            __instance.capacityOption.transform.SetLocalY(-1.15f);
            __instance.levelButtons[0].transform.parent.gameObject.SetActive(false);
            __instance.serverButton.transform.parent.SetLocalY(-1.84f);
            __instance.serverDropdown.transform.SetLocalY(-2.63f);
            __instance.modeButtons[0].transform.parent.SetLocalY(-2.55f);
            __instance.modeButtons[1].gameObject.SetActive(false);

            TORMapOptions.gameMode = CustomGamemodes.Classic;

            modeButtonGS = UnityEngine.Object.Instantiate(__instance.modeButtons[0], __instance.modeButtons[0].transform);
            modeButtonGS.name = "TORGUESSER";
            changeButtonText(modeButtonGS, ModTranslation.getString("torGuesser"));
            modeButtonGS.transform.localPosition = new Vector3(5.8f, 0f, -3f);
            modeButtonGS.OnClick.RemoveAllListeners();
            __instance.StartCoroutine(Effects.Lerp(0.1f, new Action<float>(p => modeButtonGS.SelectButton(false))));
            modeButtonGS.OnClick.AddListener((Action)(() =>
            {
                TORMapOptions.gameMode = CustomGamemodes.Guesser;
                modeButtonGS.SelectButton(true);
                __instance.modeButtons[0].SelectButton(false);
                modeButtonHK.SelectButton(false);
            }
            ));

            modeButtonHK = UnityEngine.Object.Instantiate(modeButtonGS, __instance.modeButtons[0].transform);
            modeButtonHK.name = "TORHIDENSEEK";
            changeButtonText(modeButtonHK, ModTranslation.getString("torHideNSeek"));
            modeButtonHK.transform.localPosition = new Vector3(2.91f, 0f, -3f);
            modeButtonHK.OnClick.RemoveAllListeners();
            __instance.StartCoroutine(Effects.Lerp(0.1f, new Action<float>(p => modeButtonHK.SelectButton(false))));
            modeButtonHK.OnClick.AddListener((Action)(() =>
            {
                TORMapOptions.gameMode = CustomGamemodes.HideNSeek;
                modeButtonHK.SelectButton(true);
                __instance.modeButtons[0].SelectButton(false);
                modeButtonGS.SelectButton(false);
            }
            ));

            __instance.modeButtons[0].OnClick.AddListener((Action)(() =>
            {
                TORMapOptions.gameMode = CustomGamemodes.Classic;
                modeButtonGS.SelectButton(false);
                modeButtonHK.SelectButton(false);
            }
            ));
        }

        private static void changeButtonText(PassiveButton passiveButton, string buttonText)
        {
            var selectedInactive = passiveButton.transform.FindChild("SelectedInactive/ClassicText");
            var inactive = passiveButton.transform.FindChild("Inactive/ClassicText");
            var highlight = passiveButton.transform.FindChild("Highlight/ClassicText");
            var selectedHighlight = passiveButton.transform.FindChild("SelectedHighlight/ClassicText");

            selectedInactive.gameObject.GetComponentInChildren<TextTranslatorTMP>().Destroy();
            selectedInactive.gameObject.GetComponentInChildren<TMP_Text>().SetText(buttonText);

            inactive.gameObject.GetComponentInChildren<TextTranslatorTMP>().Destroy();
            inactive.gameObject.GetComponentInChildren<TMP_Text>().SetText(buttonText);

            highlight.gameObject.GetComponentInChildren<TextTranslatorTMP>().Destroy();
            highlight.gameObject.GetComponentInChildren<TMP_Text>().SetText(buttonText);

            selectedHighlight.gameObject.GetComponentInChildren<TextTranslatorTMP>().Destroy();
            selectedHighlight.gameObject.GetComponentInChildren<TMP_Text>().SetText(buttonText);
        }
    }

    [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.SetUpFromData))]
    public static class TryGetIntArrayV09Patch
    {
        private static bool Prefix(NumberOption __instance, [HarmonyArgument(0)] BaseGameSetting data, [HarmonyArgument(1)] int maskLayer)
        {
            if (data.Type != OptionTypes.Int || data.Title != StringNames.GameNumImpostors)
                return true;
            __instance.data = data;
            __instance.GetComponentsInChildren<SpriteRenderer>(true).Do(r => r.material.SetInt(PlayerMaterial.MaskLayer, maskLayer));
            __instance.GetComponentsInChildren<TextMeshPro>(true).Do(t =>
            {
                t.fontMaterial.SetFloat("_StencilComp", 3f);
                t.fontMaterial.SetFloat("_Stencil", maskLayer);
            });
            IntGameSetting intGameSetting = data.Cast<IntGameSetting>();
            __instance.Title = intGameSetting.Title;
            __instance.Value = intGameSetting.Value;
            __instance.Increment = intGameSetting.Increment;
            GameOptionsManager.Instance.CurrentGameOptions.TryGetInt(Int32OptionNames.MaxPlayers, out int index);
            __instance.ValidRange = new FloatRange(intGameSetting.ValidRange.min, AmongUsClient.Instance?.AmLocalHost ?? false ? 6f : Helpers.MaxImpostors[index]);
            __instance.FormatString = intGameSetting.FormatString;
            __instance.ZeroIsInfinity = intGameSetting.ZeroIsInfinity;
            __instance.SuffixType = intGameSetting.SuffixType;
            __instance.intOptionName = intGameSetting.OptionName;
            return false;
        }
    }

    [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.Start))]
    class GameOptionsMenuStartPatch
    {
        public static void Postfix(GameSettingMenu __instance)
        {
            if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return;

            // GameObject.Find("What Is This?")?.Destroy();
            GameObject.Find("GamePresetButton")?.Destroy();

            __instance.ChangeTab(1, false);

            var template = GameObject.Find("PlayerOptionsMenu(Clone)")?.transform.Find("CloseButton")?.gameObject;
            if (template != null)
            {
                var holderGO = new GameObject("copyPasteButtonParent");
                var bgrenderer = holderGO.AddComponent<SpriteRenderer>();
                bgrenderer.sprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.CopyPasteBG.png", 175f);
                holderGO.transform.SetParent(template.transform.parent, false);
                holderGO.transform.localPosition = template.transform.localPosition + new Vector3(-8.3f, 0.73f, -2f);
                holderGO.layer = template.layer;
                holderGO.SetActive(true);
                var copyButton = GameObject.Instantiate(template, holderGO.transform);
                copyButton.transform.localPosition = new Vector3(-0.3f, 0.02f, -2f);
                var cbp = copyButton.GetComponent<PassiveButton>();
                var cbr = copyButton.GetComponentInChildren<SpriteRenderer>();
                var cbar = copyButton.transform.GetChild(1).GetComponent<SpriteRenderer>();
                cbr.sprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Copy.png", 100f);
                copyButton.transform.GetChild(1).transform.localPosition = Vector3.zero;
                cbar.sprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.CopyActive.png", 100f);
                cbp.OnClick.RemoveAllListeners();
                cbp.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                cbp.OnClick.AddListener((Action)(() => { copyToClipboard(); cbr.color = Color.green; cbar.color = Color.green; __instance.StartCoroutine(Effects.Lerp(1f, new Action<float>((p) => { if (p > 0.95) { cbr.color = Color.white; cbar.color = Color.white; } }))); }));
                var pasteButton = GameObject.Instantiate(template, holderGO.transform);
                pasteButton.transform.localPosition = new Vector3(0.3f, 0.02f, -2f);
                var pbp = pasteButton.GetComponent<PassiveButton>();
                var pbr = pasteButton.GetComponentInChildren<SpriteRenderer>();
                var pbar = pasteButton.transform.GetChild(1).GetComponent<SpriteRenderer>();
                pbr.sprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Paste.png", 100f);
                pbar.sprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.PasteActive.png", 100f);
                pbp.OnClick.RemoveAllListeners();
                pbp.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                pbp.OnClick.AddListener((Action)(() => { pbr.color = Color.yellow; int success = pasteFromClipboard(); pbr.color = success == 3 ? Color.green : success == 0 ? Color.red : Color.yellow; pbar.color = success == 3 ? Color.green : success == 0 ? Color.red : Color.yellow; __instance.StartCoroutine(Effects.Lerp(1f, new Action<float>((p) => { if (p > 0.95) { pbr.color = Color.white; pbar.color = Color.white; } }))); }));
            }
        }

    }



    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Initialize))]
    public class StringOptionEnablePatch {
        public static bool Prefix(StringOption __instance) {
            CustomOption option = CustomOption.options.FirstOrDefault(option => option.optionBehaviour == __instance);
            if (option == null) return true;

            __instance.OnValueChanged = new Action<OptionBehaviour>((o) => {});
            __instance.Value = __instance.oldValue = option.selection;
            __instance.ValueText.text = option.getString();
            
            return false;
        }
    }

    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Increase))]
    public class StringOptionIncreasePatch
    {
        public static bool Prefix(StringOption __instance)
        {
            CustomOption option = CustomOption.options.FirstOrDefault(option => option.optionBehaviour == __instance);
            if (option == null) return true;
            option.updateSelection(option.selection + 1);
            return false;
        }
    }

    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Decrease))]
    public class StringOptionDecreasePatch
    {
        public static bool Prefix(StringOption __instance)
        {
            CustomOption option = CustomOption.options.FirstOrDefault(option => option.optionBehaviour == __instance);
            if (option == null) return true;
            option.updateSelection(option.selection - 1);
            return false;
        }
    }

    [HarmonyPatch(typeof(LobbyInfoPane), nameof(LobbyInfoPane.RefreshPane))]
    public class LobbyPaneRefreshPatch
    {
        public static void Postfix()
        {
            HelpMenu.OnUpdateOptions();
        }
    }

    [HarmonyPatch(typeof(OptionsConsole), nameof(OptionsConsole.CanUse))]
    public class OptionsConsoleCanUsePatch
    {
        public static void Prefix(OptionsConsole __instance)
        {
            __instance.HostOnly = false;
        }
    }

    [HarmonyPatch(typeof(OptionsConsole), nameof(OptionsConsole.Use))]
    public class OptionsConsoleUsePatch
    {
        public static bool Prefix(OptionsConsole __instance)
        {
            if (__instance.MenuPrefab != null && !__instance.MenuPrefab.TryGetComponent<GameSettingMenu>(out _)) return true;

            __instance.CanUse(PlayerControl.LocalPlayer.Data, out var flag, out _);
            if (!flag) return false;

            PlayerControl.LocalPlayer.NetTransform.Halt();

            if (AmongUsClient.Instance.AmHost)
            {
                GameObject gameObject = GameObject.Instantiate<GameObject>(__instance.MenuPrefab);
                gameObject.transform.SetParent(Camera.main.transform, false);
                gameObject.transform.localPosition = __instance.CustomPosition;
                DestroyableSingleton<TransitionFade>.Instance.DoTransitionFade(null, gameObject.gameObject, null);
            }
            else
            {
                HelpMenu.TryOpenHelpScreen(HelpMenu.HelpTab.Options);
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings))]
    public class RpcSyncSettingsPatch
    {
        public static void Postfix()
        {
            CustomOption.saveVanillaOptions();
        }
    }

    [HarmonyPatch(typeof(PlayerPhysics._CoSpawnPlayer_d__42), "MoveNext")]
    public class AmongUsClientOnPlayerJoinedPatch {
        public static void Postfix() {
            if (PlayerControl.LocalPlayer != null && AmongUsClient.Instance.AmHost) {
                    GameManager.Instance.LogicOptions.SyncOptions();
                    CustomOption.ShareOptionSelections();
            }
        }
    }


    [HarmonyPatch] 
    class GameOptionsDataPatch
    {
        /*private static IEnumerable<MethodBase> TargetMethods() {
            return typeof(IGameOptionsExtensions.).GetMethods().Where(x => x.ReturnType == typeof(string) && x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType == typeof(int));
        }*/

        public static string buildRoleOptions() {
            var impRoles = buildOptionsOfType(CustomOption.CustomOptionType.Impostor, true) + "\n";
            var neutralRoles = buildOptionsOfType(CustomOption.CustomOptionType.Neutral, true) + "\n";
            var crewRoles = buildOptionsOfType(CustomOption.CustomOptionType.Crewmate, true) + "\n";
            var modifiers = buildOptionsOfType(CustomOption.CustomOptionType.Modifier, true);
            return impRoles + neutralRoles + crewRoles + modifiers;
        }
        public static string buildModifierExtras(CustomOption customOption) {
            // find options children with quantity
            var children = CustomOption.options.Where(o => o.parent == customOption);
            var quantity = children.Where(o => o.name.Contains("Quantity")).ToList();
            if (customOption.getSelection() == 0) return "";
            if (quantity.Count == 1) return $" ({quantity[0].getQuantity()})";
            return "";
        }

        public static string buildOptionsOfType(CustomOption.CustomOptionType type, bool headerOnly) {
            StringBuilder sb = new("\n");
            var options = CustomOption.options.Where(o => o.type == type);
            if (TORMapOptions.gameMode == CustomGamemodes.Guesser) {
                if (type == CustomOption.CustomOptionType.General)
                    options = CustomOption.options.Where(o => o.type == type || o.type == CustomOption.CustomOptionType.Guesser);
                List<int> remove = new() { 308, 310, 311, 312, 313, 314, 315, 316, 317, 318, 319, 7006 };
                options = options.Where(x => !remove.Contains(x.id));
            } else if (TORMapOptions.gameMode == CustomGamemodes.Classic) 
                options = options.Where(x => !(x.type == CustomOption.CustomOptionType.Guesser || x == CustomOptionHolder.crewmateRolesFill || x.id == 7007));
            else if (TORMapOptions.gameMode == CustomGamemodes.HideNSeek)
                options = options.Where(x => (x.type == CustomOption.CustomOptionType.HideNSeekMain || x.type == CustomOption.CustomOptionType.HideNSeekRoles));
            if (TORMapOptions.gameMode != CustomGamemodes.FreePlay)
                options = options.Where(x => x.id != 10429);
            foreach (var option in options) {
                if (option.parent == null) {
                    string line = $"{option.getName().Replace("\n", " ")}: {option.getString()}";
                    if (type == CustomOption.CustomOptionType.Modifier) line += buildModifierExtras(option);
                    if (option is CustomRoleOption roleOption && roleOption.enabled && roleOption.countOption != null) line += $" ({roleOption.count})";
                    sb.AppendLine(line);
                }
                else if (option.parent.getSelection() > 0 || option.invertedParent && option.parent.getSelection() == 0) {
                    if (option.id == 103) //Deputy
                        sb.AppendLine($"- {Helpers.cs(Deputy.color, ModTranslation.getString("deputy"))}: {option.getString()}");
                    else if (option.id == 224) //Sidekick
                        sb.AppendLine($"- {Helpers.cs(Sidekick.color, ModTranslation.getString("sidekick"))}: {option.getString()}");
                    else if (option.id == 918) // Immoralist
                        sb.AppendLine($"- {Helpers.cs(Immoralist.color, ModTranslation.getString("immoralist"))}: {option.getString()}");
                    else if (option.id == 8000) // Created Madmate
                        sb.AppendLine($"- {Helpers.cs(Madmate.color, Madmate.fullName)}: {option.getString()}");
                    //else if (option.id == 358) //Prosecutor
                    //sb.AppendLine($"- {Helpers.cs(Lawyer.color, "Prosecutor")}: {option.selections[option.selection].ToString()}");
                }
            }
            if (headerOnly) return sb.ToString();
            else sb = new StringBuilder();

            foreach (CustomOption option in options) {
                if (TORMapOptions.gameMode == CustomGamemodes.HideNSeek && option.type != CustomOptionType.HideNSeekMain && option.type != CustomOptionType.HideNSeekRoles) continue;
                if (option.parent != null) {
                    bool isIrrelevant = !ShouldBeEnabled(option);

                    Color c = isIrrelevant ? Color.grey : Color.white;  // No use for now
                    if (isIrrelevant) continue;
                    sb.AppendLine(Helpers.cs(c, $"{option.getName().Replace("\n", " ")}: {option.getString()}"));
                } else {
                    if (option == CustomOptionHolder.crewmateRolesCountMin) {
                        var optionName = CustomOptionHolder.cs(new Color(204f / 255f, 204f / 255f, 0, 1f), ModTranslation.getString("crewmateRoles"));
                        var min = CustomOptionHolder.crewmateRolesCountMin.getSelection();
                        var max = CustomOptionHolder.crewmateRolesCountMax.getSelection();
                        string optionValue = "";
                        if (CustomOptionHolder.crewmateRolesFill.getBool()) {
                            var crewCount = PlayerControl.AllPlayerControls.Count - GameOptionsManager.Instance.currentGameOptions.NumImpostors;
                            int minNeutral = CustomOptionHolder.neutralRolesCountMin.getSelection();
                            int maxNeutral = CustomOptionHolder.neutralRolesCountMax.getSelection();
                            if (minNeutral > maxNeutral) minNeutral = maxNeutral;
                            min = crewCount - maxNeutral;
                            max = crewCount - minNeutral;
                            if (min < 0) min = 0;
                            if (max < 0) max = 0;
                            optionValue = ModTranslation.getString("crewmateFill");
                        }
                        if (min > max) min = max;
                        optionValue += (min == max) ? $"{max}" : $"{min} - {max}";
                        sb.AppendLine($"{optionName}: {optionValue}");
                    } else if (option == CustomOptionHolder.neutralRolesCountMin) {
                        var optionName = CustomOptionHolder.cs(new Color(204f / 255f, 204f / 255f, 0, 1f), ModTranslation.getString("neutralRoles"));
                        var min = CustomOptionHolder.neutralRolesCountMin.getSelection();
                        var max = CustomOptionHolder.neutralRolesCountMax.getSelection();
                        if (min > max) min = max;
                        var optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
                        sb.AppendLine($"{optionName}: {optionValue}");
                    } else if (option == CustomOptionHolder.impostorRolesCountMin) {
                        var optionName = CustomOptionHolder.cs(new Color(204f / 255f, 204f / 255f, 0, 1f), ModTranslation.getString("impostorRoles"));
                        var min = CustomOptionHolder.impostorRolesCountMin.getSelection();
                        var max = CustomOptionHolder.impostorRolesCountMax.getSelection();
                        if (max > GameOptionsManager.Instance.currentGameOptions.NumImpostors) max = GameOptionsManager.Instance.currentGameOptions.NumImpostors;
                        if (min > max) min = max;
                        var optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
                        sb.AppendLine($"{optionName}: {optionValue}");
                    } else if (option == CustomOptionHolder.modifiersCountMin) {
                        var optionName = CustomOptionHolder.cs(new Color(204f / 255f, 204f / 255f, 0, 1f), ModTranslation.getString("modifiers"));
                        var min = CustomOptionHolder.modifiersCountMin.getSelection();
                        var max = CustomOptionHolder.modifiersCountMax.getSelection();
                        if (min > max) min = max;
                        var optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
                        sb.AppendLine($"{optionName}: {optionValue}");
                    } else if ((option == CustomOptionHolder.crewmateRolesCountMax) || (option == CustomOptionHolder.neutralRolesCountMax) || (option == CustomOptionHolder.impostorRolesCountMax) || option == CustomOptionHolder.modifiersCountMax) {
                        continue;
                    } else {
                        sb.AppendLine($"\n{option.getName().Replace("\n", " ")}: {option.getString()}");
                    }
                }
            }
            return sb.ToString();
        }

        private static string[] mapName = new string[] { "Skeld", "Mira", "Polus", "Dlesk", "Airship", "Fungle" };
        public static string buildAllOptions() {
            StringBuilder builder = new();

            //バニラオプション
            var vanillaOptions = GameOptionsManager.Instance.currentNormalGameOptions;
            var translator = TranslationController.Instance;

            void AddHeader(string header)
            {
                builder.Append("\n\n");
                builder.Append(ModTranslation.getString(header));
            }
            void AddOption(StringNames option, string value, string format = "")
            {
                builder.Append("\n- ");
                builder.Append(translator.GetString(option));
                builder.Append(": ");
                builder.Append(string.IsNullOrEmpty(format) ? value : string.Format(ModTranslation.getString(format), value));
            }
            string SimpleTrigger(bool trigger) => trigger ? "<color=#FFFF00FF>" + ModTranslation.getString("optionOn") + "</color>" : "<color=#CCCCCCFF>" + ModTranslation.getString("optionOff") + "</color>";
            builder.Append(ModTranslation.getString("vanillaOptionsHeader"));
            AddOption(StringNames.GameMapName, ModTranslation.getString("map" + mapName[vanillaOptions.MapId]));
            AddHeader("vanillaOptionsImpostorHeader");
            AddOption(StringNames.GameNumImpostors, vanillaOptions.NumImpostors.ToString());
            AddOption(StringNames.GameKillCooldown, vanillaOptions.KillCooldown.ToString(), "unitSeconds");
            AddOption(StringNames.GameImpostorLight, vanillaOptions.ImpostorLightMod.ToString(), "unitTimes");
            AddOption(StringNames.GameKillDistance, translator.GetString(vanillaOptions.KillDistance switch { 0 => StringNames.SettingShort, 1 => StringNames.SettingMedium, _ => StringNames.SettingLong }));
            AddHeader("vanillaOptionsCrewmateHeader");
            AddOption(StringNames.GamePlayerSpeed, vanillaOptions.PlayerSpeedMod.ToString(), "unitTimes");
            AddOption(StringNames.GameCrewLight, vanillaOptions.CrewLightMod.ToString(), "unitTimes");
            AddHeader("vanillaOptionsMeetingHeader");
            AddOption(StringNames.GameNumMeetings, vanillaOptions.NumEmergencyMeetings.ToString());
            AddOption(StringNames.GameEmergencyCooldown, vanillaOptions.EmergencyCooldown.ToString(), "unitSeconds");
            AddOption(StringNames.GameDiscussTime, vanillaOptions.DiscussionTime.ToString(), "unitSeconds");
            AddOption(StringNames.GameVotingTime, vanillaOptions.VotingTime.ToString(), "unitSeconds");
            AddOption(StringNames.GameAnonymousVotes, SimpleTrigger(vanillaOptions.AnonymousVotes));
            AddOption(StringNames.GameConfirmImpostor, SimpleTrigger(vanillaOptions.ConfirmImpostor));
            AddHeader("vanillaOptionsTaskHeader");
            AddOption(StringNames.GameTaskBarMode, translator.GetString(vanillaOptions.TaskBarMode switch { AmongUs.GameOptions.TaskBarMode.Normal => StringNames.SettingNormalTaskMode, AmongUs.GameOptions.TaskBarMode.MeetingOnly => StringNames.SettingMeetingTaskMode, _ => StringNames.SettingInvisibleTaskMode }));
            AddOption(StringNames.GameCommonTasks, vanillaOptions.NumCommonTasks.ToString());
            AddOption(StringNames.GameLongTasks, vanillaOptions.NumLongTasks.ToString());
            AddOption(StringNames.GameShortTasks, vanillaOptions.NumShortTasks.ToString());
            AddOption(StringNames.GameVisualTasks, SimpleTrigger(vanillaOptions.VisualTasks));

            string hudString = "";
            if (TORMapOptions.gameMode == CustomGamemodes.HideNSeek) {
                hudString += buildOptionsOfType(CustomOptionType.HideNSeekMain, false) + buildOptionsOfType(CustomOptionType.HideNSeekRoles, false);
            } else {
                hudString += buildOptionsOfType(CustomOptionType.General, false) + buildRoleOptions() + buildOptionsOfType(CustomOptionType.Impostor, false) +
                    buildOptionsOfType(CustomOptionType.Neutral, false) + buildOptionsOfType(CustomOptionType.Crewmate, false) +
                    buildOptionsOfType(CustomOptionType.Modifier, false);
            }

            return builder.ToString() + "\n\n" + hudString;
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class HudManagerUpdate {

        static PassiveButton toggleSettingsButton;
        static GameObject toggleSettingsButtonObject;
        static GameObject toggleZoomButtonObject;
        static PassiveButton toggleZoomButton;
        [HarmonyPostfix]
        public static void Postfix(HudManager __instance) {
            if (!toggleSettingsButton || !toggleSettingsButtonObject) {
                // add a special button for settings viewing:
                toggleSettingsButtonObject = GameObject.Instantiate(__instance.MapButton.gameObject, __instance.MapButton.transform.parent);
                toggleSettingsButtonObject.transform.localPosition = __instance.MapButton.transform.localPosition + new Vector3(0, -1.25f, -500f);
                toggleSettingsButtonObject.name = "TOGGLESETTINGSBUTTON";
                SpriteRenderer renderer = toggleSettingsButtonObject.transform.Find("Inactive").GetComponent<SpriteRenderer>();
                SpriteRenderer rendererActive = toggleSettingsButtonObject.transform.Find("Active").GetComponent<SpriteRenderer>();
                toggleSettingsButtonObject.transform.Find("Background").localPosition = Vector3.zero;
                renderer.sprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Settings_Button.png", 100f);
                rendererActive.sprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Settings_ButtonActive.png", 100);
                toggleSettingsButton = toggleSettingsButtonObject.GetComponent<PassiveButton>();
                toggleSettingsButton.OnClick.RemoveAllListeners();
                toggleSettingsButton.OnClick.AddListener((Action)(() => HelpMenu.TryOpenHelpScreen(HelpMenu.HelpTab.Options)));
            }
            toggleSettingsButtonObject.SetActive((GameStates.IsLobby ? __instance.SettingsButton?.gameObject?.active ?? false : __instance.MapButton?.gameObject?.active ?? false) && !(MapBehaviour.Instance && MapBehaviour.Instance.IsOpen) && GameOptionsManager.Instance.currentGameOptions.GameMode != GameModes.HideNSeek);
            toggleSettingsButtonObject.transform.localPosition = GameStates.IsLobby ? (__instance.SettingsButton?.transform?.localPosition ?? new Vector3()) + new Vector3(-1.45f, 0.03f, -200f) : __instance.MapButton.transform.localPosition + new Vector3(0, -0.8f, -500f);

            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;
            if (!toggleZoomButton || !toggleZoomButtonObject)
            {
                // add a special button for settings viewing:
                toggleZoomButtonObject = GameObject.Instantiate(__instance.MapButton.gameObject, __instance.MapButton.transform.parent);
                toggleZoomButtonObject.transform.localPosition = __instance.MapButton.transform.localPosition + new Vector3(0, -1.25f, -500f);
                toggleZoomButtonObject.name = "TOGGLEZOOMBUTTON";
                SpriteRenderer tZrenderer = toggleZoomButtonObject.transform.Find("Inactive").GetComponent<SpriteRenderer>();
                SpriteRenderer tZArenderer = toggleZoomButtonObject.transform.Find("Active").GetComponent<SpriteRenderer>();
                toggleZoomButtonObject.transform.Find("Background").localPosition = Vector3.zero;
                tZrenderer.sprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Minus_Button.png", 100f);
                tZArenderer.sprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Minus_ButtonActive.png", 100);
                toggleZoomButton = toggleZoomButtonObject.GetComponent<PassiveButton>();
                toggleZoomButton.OnClick.RemoveAllListeners();
                toggleZoomButton.OnClick.AddListener((Action)(() => Helpers.toggleZoom()));
            }
            if (PlayerControl.LocalPlayer != null)
            {
                var (playerCompleted, playerTotal) = TasksHandler.taskInfo(PlayerControl.LocalPlayer.Data);
                int numberOfLeftTasks = playerTotal - playerCompleted;
                bool zoomButtonActive = !(PlayerControl.LocalPlayer == null || !PlayerControl.LocalPlayer.Data.IsDead || (!RoleManager.IsGhostRole(PlayerControl.LocalPlayer.Data.RoleType) && !CustomGameModes.FreePlayGM.isFreePlayGM) || MeetingHud.Instance || ExileController.Instance);
                zoomButtonActive &= numberOfLeftTasks <= 0 || !CustomOptionHolder.finishTasksBeforeHauntingOrZoomingOut.getBool();
                toggleZoomButtonObject.SetActive(zoomButtonActive);
                var posOffset = Helpers.zoomOutStatus ? new Vector3(-1.27f, -7.92f, -52f) : new Vector3(0, -1.6f, -52f);
                toggleZoomButtonObject.transform.localPosition = HudManager.Instance.MapButton.transform.localPosition + posOffset;
            }
        }
    }
}
