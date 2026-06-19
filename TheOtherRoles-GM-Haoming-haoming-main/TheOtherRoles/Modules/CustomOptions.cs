using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Hazel;
using InnerNet;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static TheOtherRoles.CustomOption;
using static TheOtherRoles.TheOtherRoles;
using Object = UnityEngine.Object;

namespace TheOtherRoles;

public class CustomOption
{
    public enum CustomOptionType
    {
        General,
        Impostor,
        Neutral,
        Crewmate,
        Modifier
    }

    public static List<CustomOption> options = new();
    public static int preset;
    public static ConfigEntry<string> vanillaSettings;
    public List<CustomOption> children;

    public int defaultSelection;
    public ConfigEntry<int> entry;
    public string format;
    public string heading = "";

    public int id;
    public bool isHeader;
    public bool isHidden;
    public string name;
    public Action onChange;
    public OptionBehaviour optionBehaviour;
    public CustomOption parent;
    public int selection;
    public object[] selections;
    public CustomOptionType type;

    // Option creation
    public CustomOption()
    {
    }

    public CustomOption(int id, CustomOptionType type, string name, object[] selections, object defaultValue,
        CustomOption parent, bool isHeader, bool isHidden, string format, Action onChange = null, string heading = "")
    {
        Init(id, type, name, selections, defaultValue, parent, isHeader, isHidden, format, onChange, heading);
    }

    public virtual bool enabled => Helpers.RolesEnabled && getBool();

    public void Init(int id, CustomOptionType type, string name, object[] selections, object defaultValue,
        CustomOption parent, bool isHeader, bool isHidden, string format, Action onChange = null, string heading = "")
    {
        this.id = id;
        this.name = name;
        this.format = format;
        this.selections = selections;
        int index = Array.IndexOf(selections, defaultValue);
        defaultSelection = index >= 0 ? index : 0;
        this.parent = parent;
        this.isHeader = isHeader;
        this.isHidden = isHidden;
        this.type = type;
        this.onChange = onChange;
        this.heading = heading;

        children = new List<CustomOption>();
        if (parent != null) parent.children.Add(this);

        selection = 0;
        if (id > 0)
        {
            entry = TheOtherRolesPlugin.Instance.Config.Bind($"Preset{preset}", id.ToString(), defaultSelection);
            selection = Mathf.Clamp(entry.Value, 0, selections.Length - 1);

            if (options.Any(x => x.id == id))
                Helpers.log($"CustomOption id {id} is used in multiple places.");
        }

        options.Add(this);
    }

    public static CustomOption Create(int id, CustomOptionType type, string name, string[] selections,
        CustomOption parent = null, bool isHeader = false, bool isHidden = false, string format = "",
        Action onChange = null, string heading = "")
    {
        return new CustomOption(id, type, name, selections, "", parent, isHeader, isHidden, format, onChange, heading);
    }


    public static CustomOption Create(int id, CustomOptionType type, string name, float defaultValue, float min,
        float max, float step, CustomOption parent = null, bool isHeader = false, bool isHidden = false,
        string format = "", Action onChange = null, string heading = "")
    {
        List<float> selections = new();
        for (float s = min; s <= max; s += step)
            selections.Add(s);
        return new CustomOption(id, type, name, selections.Cast<object>().ToArray(), defaultValue, parent, isHeader,
            isHidden, format, onChange, heading);
    }

    public static CustomOption Create(int id, CustomOptionType type, string name, bool defaultValue,
        CustomOption parent = null, bool isHeader = false, bool isHidden = false, string format = "",
        Action onChange = null, string heading = "")
    {
        return new CustomOption(id, type, name, new[] { "optionOff", "optionOn" },
            defaultValue ? "optionOn" : "optionOff", parent, isHeader, isHidden, format, onChange, heading);
    }

    // Static behaviour

    public static void switchPreset(int newPreset)
    {
        saveVanillaOptions();
        preset = newPreset;
        vanillaSettings = TheOtherRolesPlugin.Instance.Config.Bind($"Preset{preset}", "GameOptions", "");
        loadVanillaOptions();
        foreach (CustomOption option in options)
        {
            if (option.id == 0) continue;

            option.entry =
                TheOtherRolesPlugin.Instance.Config.Bind($"Preset{preset}", option.id.ToString(),
                    option.defaultSelection);
            option.selection = Mathf.Clamp(option.entry.Value, 0, option.selections.Length - 1);
            if (option.optionBehaviour != null && option.optionBehaviour is StringOption stringOption)
            {
                stringOption.oldValue = stringOption.Value = option.selection;
                stringOption.ValueText.text = option.getString();
            }
        }
    }

    public static void saveVanillaOptions()
    {
        vanillaSettings.Value =
            Convert.ToBase64String(
                GameOptionsManager.Instance.gameOptionsFactory.ToBytes(
                    GameManager.Instance.LogicOptions.currentGameOptions, false));
    }

    public static bool loadVanillaOptions()
    {
        string optionsString = vanillaSettings.Value;
        if (optionsString == "") return false;
        IGameOptions gameOptions =
            GameOptionsManager.Instance.gameOptionsFactory.FromBytes(Convert.FromBase64String(optionsString));
        if (gameOptions.Version < 8)
        {
            TheOtherRolesPlugin.Logger.LogMessage("tried to paste old settings, not doing this!");
            return false;
        }

        GameOptionsManager.Instance.GameHostOptions = gameOptions;
        GameOptionsManager.Instance.CurrentGameOptions = GameOptionsManager.Instance.GameHostOptions;
        GameManager.Instance.LogicOptions.SetGameOptions(GameOptionsManager.Instance.CurrentGameOptions);
        GameManager.Instance.LogicOptions.SyncOptions();
        return true;
    }

    public static void ShareOptionChange(uint optionId)
    {
        CustomOption option = options.FirstOrDefault(x => x.id == optionId);
        if (option == null) return;
        MessageWriter writer = AmongUsClient.Instance!.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
            (byte)CustomRPC.ShareOptions, SendOption.Reliable);
        writer.Write((byte)1);
        writer.WritePacked((uint)option.id);
        writer.WritePacked(Convert.ToUInt32(option.selection));
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }

    public static void ShareOptionSelections()
    {
        if (PlayerControl.AllPlayerControls.ToArray().ToList().Count <= 1 ||
            (AmongUsClient.Instance!.AmHost == false && PlayerControl.LocalPlayer == null)) return;
        List<CustomOption> optionsList = new(options);
        while (optionsList.Any())
        {
            byte amount = (byte)Math.Min(optionsList.Count, 200); // takes less than 3 bytes per option on average
            MessageWriter writer = AmongUsClient.Instance!.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                (byte)CustomRPC.ShareOptions, SendOption.Reliable);
            writer.Write(amount);
            for (int i = 0; i < amount; i++)
            {
                CustomOption option = optionsList[0];
                optionsList.RemoveAt(0);
                writer.WritePacked((uint)option.id);
                writer.WritePacked(Convert.ToUInt32(option.selection));
            }

            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
    }

    // Getter

    public virtual int getSelection()
    {
        return selection;
    }

    public virtual bool getBool()
    {
        return selection > 0;
    }

    public virtual float getFloat()
    {
        return (float)selections[selection];
    }

    public int getQuantity()
    {
        return selection + 1;
    }

    public virtual string getString()
    {
        string sel = selections[selection].ToString();
        if (format != "") return string.Format(ModTranslation.getString(format), sel);
        if (sel == "optionOn") return "<color=#FFFF00FF>" + ModTranslation.getString(sel) + "</color>";
        else if (sel == "optionOff") return "<color=#CCCCCCFF>" + ModTranslation.getString(sel) + "</color>";
        return ModTranslation.getString(sel);
    }

    public virtual string getName()
    {
        return ModTranslation.getString(name);
    }

    public string getHeading()
    {
        if (heading == "") return "";
        return ModTranslation.getString(heading);
    }
    // Option changes

    public void updateSelection(int newSelection, bool notifyUsers = true, bool loadPreset = false)
    {
        newSelection = Mathf.Clamp((newSelection + selections.Length) % selections.Length, 0, selections.Length - 1);
        bool doNeedNotifier = AmongUsClient.Instance?.AmClient == true && notifyUsers && selection != newSelection;
        if (doNeedNotifier)
            try
            {
                selection = newSelection;
                if (GameStartManager.Instance != null && GameStartManager.Instance.LobbyInfoPane != null &&
                    GameStartManager.Instance.LobbyInfoPane.LobbyViewSettingsPane != null &&
                    GameStartManager.Instance.LobbyInfoPane.LobbyViewSettingsPane.gameObject.activeSelf)
                    LobbyViewSettingsPaneChangeTabPatch.Postfix(
                        GameStartManager.Instance.LobbyInfoPane.LobbyViewSettingsPane,
                        GameStartManager.Instance.LobbyInfoPane.LobbyViewSettingsPane.currentTab);
            }
            catch
            {
            }

        selection = newSelection;
        if (doNeedNotifier)
        {
            CustomOption originalParent = parent;
            if (originalParent != null)
                while (originalParent.parent != null)
                    originalParent = originalParent.parent;
            DestroyableSingleton<HudManager>.Instance.Notifier.AddModSettingsChangeMessage((StringNames)(id + 6000),
                getString(),
                (originalParent != null ? originalParent.getName().Replace("- ", "") + ": " : "") +
                getName().Replace("- ", ""), false);
        }

        if (AmongUsClient.Instance?.AmHost == true && !loadPreset)
        {
            GameOptionsMenu currentTab = GameOptionsMenuStartPatch.currentTabs.FirstOrDefault(x => x.active)
                .GetComponent<GameOptionsMenu>();
            if (currentTab != null)
            {
                CustomOptionType optionType = options.First(x => x.optionBehaviour == currentTab.Children[0]).type;
                GameOptionsMenuStartPatch.updateGameOptionsMenu(optionType, currentTab);
            }
        }

        if (optionBehaviour != null && optionBehaviour is StringOption stringOption)
        {
            stringOption.oldValue = stringOption.Value = selection;
            stringOption.ValueText.text = getString();

            if (AmongUsClient.Instance?.AmHost == true && PlayerControl.LocalPlayer)
            {
                if (id == 0 && selection != preset)
                {
                    switchPreset(selection); // Switch presets
                    ShareOptionSelections();
                }
                else if (entry != null)
                {
                    entry.Value = selection; // Save selection to config
                    ShareOptionChange((uint)id); // Share single selection
                }
            }
        }
        else if (id == 0 && AmongUsClient.Instance?.AmHost == true && PlayerControl.LocalPlayer)
        {
            // Share the preset switch for random maps, even if the menu isnt open!
            switchPreset(selection);
            ShareOptionSelections(); // Share all selections
        }
    }

    public static byte[] serializeOptions()
    {
        using (MemoryStream memoryStream = new())
        using (BinaryWriter binaryWriter = new(memoryStream))
        {
            int lastId = -1;
            foreach (CustomOption option in options.OrderBy(x => x.id))
            {
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

    public static int deserializeOptions(byte[] inputValues)
    {
        BinaryReader reader = new(new MemoryStream(inputValues));
        int lastId = -1;
        bool somethingApplied = false;
        int errors = 0;
        while (reader.BaseStream.Position < inputValues.Length)
            try
            {
                int selection = reader.ReadByte();
                int id = -1;
                bool consecutive = selection >= 128;
                if (consecutive)
                {
                    selection -= 128;
                    id = lastId + 1;
                }
                else
                    id = reader.ReadUInt16();

                if (id == 0) continue;
                lastId = id;
                CustomOption option = options.First(option => option.id == id);
                option.entry = TheOtherRolesPlugin.Instance.Config.Bind($"Preset{preset}", option.id.ToString(),
                    option.defaultSelection);
                option.selection = selection;
                if (option.optionBehaviour != null && option.optionBehaviour is StringOption stringOption)
                {
                    stringOption.oldValue = stringOption.Value = option.selection;
                    stringOption.ValueText.text = option.getString();
                }

                somethingApplied = true;
            }
            catch (Exception e)
            {
                TheOtherRolesPlugin.Logger.LogWarning(
                    $"id:{lastId}:{e}: while deserializing - tried to paste invalid settings!");
                errors++;
            }

        return Convert.ToInt32(somethingApplied) + (errors > 0 ? 0 : 1);
    }

    // Copy to or paste from clipboard (as string)
    public static void copyToClipboard()
    {
        GUIUtility.systemCopyBuffer =
            $"{TheOtherRolesPlugin.VersionString}!{Convert.ToBase64String(serializeOptions())}!{vanillaSettings.Value}";
    }

    public static int pasteFromClipboard()
    {
        string allSettings = GUIUtility.systemCopyBuffer;
        int torOptionsFine = 0;
        bool vanillaOptionsFine = false;
        try
        {
            string[] settingsSplit = allSettings.Split("!");
            Version versionInfo = Version.Parse(settingsSplit[0]);
            string torSettings = settingsSplit[1];
            string vanillaSettingsSub = settingsSplit[2];
            torOptionsFine = deserializeOptions(Convert.FromBase64String(torSettings));
            ShareOptionSelections();
            if (TheOtherRolesPlugin.Version > versionInfo && versionInfo < Version.Parse("2.3.137"))
            {
                vanillaOptionsFine = false;
                FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer,
                    "Host Info: Pasting vanilla settings failed, TORGMH Options applied!");
            }
            else
            {
                vanillaSettings.Value = vanillaSettingsSub;
                vanillaOptionsFine = loadVanillaOptions();
            }
        }
        catch (Exception e)
        {
            TheOtherRolesPlugin.Logger.LogWarning($"{e}: tried to paste invalid settings!\n{allSettings}");
            string errorStr = allSettings.Length > 2 ? allSettings.Substring(0, 3) : "(empty clipboard) ";
            FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer,
                $"Host Info: You tried to paste invalid settings: \"{errorStr}...\"");
        }

        return Convert.ToInt32(vanillaOptionsFine) + torOptionsFine;
    }
}

public class CustomRoleOption : CustomOption
{
    public CustomOption countOption;
    public bool roleEnabled = true;

    public CustomRoleOption(int id, CustomOptionType type, string name, Color color, int max = 15,
        bool roleEnabled = true) :
        base(id, type, Helpers.cs(color, name), CustomOptionHolder.rates, "", null, true, false, "")
    {
        this.roleEnabled = roleEnabled;

        if (max <= 0 || !roleEnabled)
        {
            isHidden = true;
            this.roleEnabled = false;
        }

        if (max > 1)
            countOption = Create(id + 10000, type, "roleNumAssigned", 1f, 1f, 15f, 1f, this, false, isHidden,
                "unitPlayers");
    }

    public override bool enabled => Helpers.RolesEnabled && roleEnabled && selection > 0;

    public int rate => enabled ? selection : 0;

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

    public (int, int) data => (rate, count);
}

public class CustomDualRoleOption : CustomRoleOption
{
    public static List<CustomDualRoleOption> dualRoles = new();
    public CustomOption roleAssignEqually;
    public CustomOption roleImpChance;
    public RoleType roleType;

    public CustomDualRoleOption(int id, CustomOptionType type, string name, Color color, RoleType roleType,
        int max = 15, bool roleEnabled = true) : base(id, type, name, color, max, roleEnabled)
    {
        roleAssignEqually = new CustomOption(id + 100001, type, "roleAssignEqually", new[] { "optionOn", "optionOff" },
            "optionOff", this, false, isHidden, "");
        roleImpChance = Create(id + 100000, type, "roleImpChance", CustomOptionHolder.rates, roleAssignEqually, false,
            isHidden);

        this.roleType = roleType;
        dualRoles.Add(this);
    }

    public int impChance => roleImpChance.getSelection();

    public bool assignEqually => roleAssignEqually.getSelection() == 0;
}

public class CustomTasksOption : CustomOption
{
    public CustomOption commonTasksOption;
    public CustomOption longTasksOption;
    public CustomOption shortTasksOption;

    public CustomTasksOption(int id, CustomOptionType type, int commonDef, int longDef, int shortDef,
        CustomOption parent = null)
    {
        commonTasksOption = Create(id + 20000, type, "numCommonTasks", commonDef, 0f, 4f, 1f, parent);
        longTasksOption = Create(id + 20001, type, "numLongTasks", longDef, 0f, 15f, 1f, parent);
        shortTasksOption = Create(id + 20002, type, "numShortTasks", shortDef, 0f, 23f, 1f, parent);
    }

    public int commonTasks => Mathf.RoundToInt(commonTasksOption.getSelection());
    public int longTasks => Mathf.RoundToInt(longTasksOption.getSelection());
    public int shortTasks => Mathf.RoundToInt(shortTasksOption.getSelection());

    public List<byte> generateTasks()
    {
        return Helpers.generateTasks(commonTasks, shortTasks, longTasks);
    }
}

public class CustomRoleSelectionOption : CustomOption
{
    public List<RoleType> roleTypes;

    public CustomRoleSelectionOption(int id, CustomOptionType type, string name, List<RoleType> roleTypes = null,
        CustomOption parent = null)
    {
        if (roleTypes == null) roleTypes = Enum.GetValues(typeof(RoleType)).Cast<RoleType>().ToList();

        this.roleTypes = roleTypes;
        string[] strings = roleTypes.Select(
            x =>
                x == RoleType.NoRole
                    ? "optionOff"
                    : ModTranslation.getString(x.ToString()[..1].ToLower() + x.ToString()[1..])
            //RoleInfo.allRoleInfos.First(y => y.roleType == x).nameColored
        ).ToArray();

        Init(id, type, name, strings, 0, parent, false, false, "");
    }

    public RoleType role => roleTypes[selection];
}

/*public class CustomOptionBlank : CustomOption
{
    public CustomOptionBlank(CustomOption parent)
    {
        this.parent = parent;
        id = -1;
        name = "";
        isHeader = false;
        isHidden = true;
        children = new List<CustomOption>();
        selections = new[] { "" };
        options.Add(this);
    }

    public override int getSelection()
    {
        return 0;
    }

    public override bool getBool()
    {
        return true;
    }

    public override float getFloat()
    {
        return 0f;
    }

    public override string getString()
    {
        return "";
    }

    public override void updateSelection(int newSelection, bool notifyUsers = true)
    {
    }
}*/

[HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.ChangeTab))]
internal class GameOptionsMenuChangeTabPatch
{
    public static void Postfix(GameSettingMenu __instance, int tabNum, bool previewOnly)
    {
        if (previewOnly) return;
        foreach (GameObject tab in GameOptionsMenuStartPatch.currentTabs)
            if (tab != null)
                tab.SetActive(false);
        foreach (PassiveButton pbutton in GameOptionsMenuStartPatch.currentButtons) pbutton.SelectButton(false);
        if (tabNum > 2)
        {
            tabNum -= 3;
            GameOptionsMenuStartPatch.currentTabs[tabNum].SetActive(true);
            GameOptionsMenuStartPatch.currentButtons[tabNum].SelectButton(true);
        }
    }
}

[HarmonyPatch(typeof(LobbyViewSettingsPane), nameof(LobbyViewSettingsPane.SetTab))]
internal class LobbyViewSettingsPaneRefreshTabPatch
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
internal class LobbyViewSettingsPaneChangeTabPatch
{
    public static void Postfix(LobbyViewSettingsPane __instance, StringNames category)
    {
        int tabNum = (int)category;

        foreach (PassiveButton pbutton in LobbyViewSettingsPatch.currentButtons) pbutton.SelectButton(false);
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
internal class LobbyViewSettingsPaneUpdatePatch
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
internal class LobbyViewSettingsPatch
{
    public static List<PassiveButton> currentButtons = new();
    public static List<CustomOptionType> currentButtonTypes = new();
    public static bool gameModeChangedFlag;

    public static void createCustomButton(LobbyViewSettingsPane __instance, int targetMenu, string buttonName,
        string buttonText, CustomOptionType optionType)
    {
        buttonName = "View" + buttonName;
        GameObject buttonTemplate = GameObject.Find("OverviewTab");
        GameObject torSettingsButton = GameObject.Find(buttonName);
        if (torSettingsButton == null)
        {
            torSettingsButton = GameObject.Instantiate(buttonTemplate, buttonTemplate.transform.parent);
            torSettingsButton.transform.localPosition += Vector3.right * 1.75f * (targetMenu - 2);
            torSettingsButton.name = buttonName;
            __instance.StartCoroutine(Effects.Lerp(2f,
                new Action<float>(p =>
                {
                    torSettingsButton.transform.FindChild("FontPlacer").GetComponentInChildren<TextMeshPro>().text =
                        buttonText;
                })));
            PassiveButton torSettingsPassiveButton = torSettingsButton.GetComponent<PassiveButton>();
            torSettingsPassiveButton.OnClick.RemoveAllListeners();
            torSettingsPassiveButton.OnClick.AddListener((Action)(() =>
            {
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
        currentButtons.ForEach(x => x?.Destroy());
        currentButtons.Clear();
        currentButtonTypes.Clear();

        removeVanillaTabs(__instance);

        createSettingTabs(__instance);
    }

    public static void removeVanillaTabs(LobbyViewSettingsPane __instance)
    {
        GameObject.Find("RolesTabs")?.Destroy();
        GameObject overview = GameObject.Find("OverviewTab");
        if (!gameModeChangedFlag)
        {
            overview.transform.localScale = new Vector3(0.5f * overview.transform.localScale.x,
                overview.transform.localScale.y, overview.transform.localScale.z);
            overview.transform.localPosition += new Vector3(-1.2f, 0f, 0f);
        }

        overview.transform.Find("FontPlacer").transform.localScale = new Vector3(1.35f, 1f, 1f);
        overview.transform.Find("FontPlacer").transform.localPosition = new Vector3(-0.6f, -0.1f, 0f);
        gameModeChangedFlag = false;
    }

    public static void drawTab(LobbyViewSettingsPane __instance, CustomOptionType optionType)
    {
        List<CustomOption> relevantOptions = options.Where(x => x.type == optionType).ToList();

        if ((int)optionType == 99)
        {
            // Create 4 Groups with Role settings only
            relevantOptions.Clear();
            relevantOptions.AddRange(options.Where(x => x.type == CustomOptionType.Impostor && x.isHeader));
            relevantOptions.AddRange(options.Where(x => x.type == CustomOptionType.Neutral && x.isHeader));
            relevantOptions.AddRange(options.Where(x => x.type == CustomOptionType.Crewmate && x.isHeader));
            relevantOptions.AddRange(options.Where(x => x.type == CustomOptionType.Modifier && x.isHeader));
            foreach (CustomOption option in options)
                if (option.parent != null && option.parent.getSelection() > 0)
                    if (option.id == 224) //Sidekick
                        relevantOptions.Insert(relevantOptions.IndexOf(CustomOptionHolder.jackalSpawnRate) + 1, option);
        }

        for (int j = 0; j < __instance.settingsInfo.Count; j++) __instance.settingsInfo[j].gameObject.Destroy();
        __instance.settingsInfo.Clear();

        float num = 1.44f;
        int i = 0;
        int singles = 0;
        int headers = 0;
        int lines = 0;
        CustomOptionType curType = CustomOptionType.Modifier;

        foreach (CustomOption option in relevantOptions)
        {
            if ((option.isHeader && (int)optionType != 99) || ((int)optionType == 99 && curType != option.type))
            {
                curType = option.type;
                if (i != 0) num -= 0.59f;
                if (i % 2 != 0) singles++;
                headers++; // for header
                CategoryHeaderMasked categoryHeaderMasked = Object.Instantiate(__instance.categoryHeaderOrigin);
                categoryHeaderMasked.SetHeader(StringNames.ImpostorsCategory, 61);
                categoryHeaderMasked.Title.text = option.getHeading() != "" ? option.getHeading() : option.getName();
                if ((int)optionType == 99)
                    categoryHeaderMasked.Title.text = new Dictionary<CustomOptionType, string>
                    {
                        { CustomOptionType.Impostor, ModTranslation.getString("impostorRoles") },
                        { CustomOptionType.Neutral, ModTranslation.getString("neutralRoles") },
                        { CustomOptionType.Crewmate, ModTranslation.getString("crewmateRoles") },
                        { CustomOptionType.Modifier, ModTranslation.getString("modifiers") }
                    }[curType];
                categoryHeaderMasked.transform.SetParent(__instance.settingsContainer);
                categoryHeaderMasked.transform.localScale = Vector3.one;
                categoryHeaderMasked.transform.localPosition = new Vector3(-9.77f, num, -2f);
                __instance.settingsInfo.Add(categoryHeaderMasked.gameObject);
                num -= 0.85f;
                i = 0;
            }
            else if (option.parent != null && (option.parent.selection == 0 ||
                                               (option.parent.parent != null && option.parent.parent.selection == 0)))
                continue; // Hides options, for which the parent is disabled!

            if (option == CustomOptionHolder.crewmateRolesCountMax ||
                option == CustomOptionHolder.neutralRolesCountMax || option == CustomOptionHolder.impostorRolesCountMax)
                continue;

            ViewSettingsInfoPanel viewSettingsInfoPanel = Object.Instantiate(__instance.infoPanelOrigin);
            viewSettingsInfoPanel.transform.SetParent(__instance.settingsContainer);
            viewSettingsInfoPanel.transform.localScale = Vector3.one;
            float num2;
            if (i % 2 == 0)
            {
                lines++;
                num2 = -8.95f;
                if (i > 0) num -= 0.59f;
            }
            else
                num2 = -3f;

            viewSettingsInfoPanel.transform.localPosition = new Vector3(num2, num, -2f);
            int value = option.getSelection();
            Tuple<string, string> settingTuple = handleSpecialOptionsView(option, option.getName(),
                ModTranslation.getString(option.selections[value].ToString()));
            viewSettingsInfoPanel.SetInfo(StringNames.ImpostorsCategory, settingTuple.Item2, 61);
            viewSettingsInfoPanel.titleText.text = settingTuple.Item1;
            if (option.isHeader && (int)optionType != 99 && option.getHeading() == "" &&
                (option.type == CustomOptionType.Neutral || option.type == CustomOptionType.Crewmate ||
                 option.type == CustomOptionType.Impostor ||
                 option.type == CustomOptionType.Modifier))
                viewSettingsInfoPanel.titleText.text = ModTranslation.getString("spawnChance");
            if ((int)optionType == 99)
                if (option.type == CustomOptionType.Modifier)
                    viewSettingsInfoPanel.settingText.text = viewSettingsInfoPanel.settingText.text +
                                                             GameOptionsDataPatch.buildModifierExtras(option);
            __instance.settingsInfo.Add(viewSettingsInfoPanel.gameObject);

            i++;
        }

        float actual_spacing = ((headers * 0.85f) + (lines * 0.59f)) / (headers + lines);
        __instance.scrollBar.CalculateAndSetYBounds(__instance.settingsInfo.Count + (singles * 2) + headers, 2f, 6f,
            actual_spacing);
    }

    private static Tuple<string, string> handleSpecialOptionsView(CustomOption option, string defaultString,
        string defaultVal)
    {
        string name = defaultString;
        string val = defaultVal;
        if (option == CustomOptionHolder.crewmateRolesCountMin)
        {
            val = "";
            name = ModTranslation.getString("crewmateRoles");
            int min = CustomOptionHolder.crewmateRolesCountMin.getSelection();
            int max = CustomOptionHolder.crewmateRolesCountMax.getSelection();
            if (min > max) min = max;
            val += min == max ? $"{max}" : $"{min} - {max}";
        }

        if (option == CustomOptionHolder.neutralRolesCountMin)
        {
            name = ModTranslation.getString("neutralRoles");
            int min = CustomOptionHolder.neutralRolesCountMin.getSelection();
            int max = CustomOptionHolder.neutralRolesCountMax.getSelection();
            if (min > max) min = max;
            val = min == max ? $"{max}" : $"{min} - {max}";
        }

        if (option == CustomOptionHolder.impostorRolesCountMin)
        {
            name = ModTranslation.getString("impostorRoles");
            int min = CustomOptionHolder.impostorRolesCountMin.getSelection();
            int max = CustomOptionHolder.impostorRolesCountMax.getSelection();
            if (max > GameOptionsManager.Instance.currentGameOptions.NumImpostors)
                max = GameOptionsManager.Instance.currentGameOptions.NumImpostors;
            if (min > max) min = max;
            val = min == max ? $"{max}" : $"{min} - {max}";
        }

        return new Tuple<string, string>(name, val);
    }


    public static void createSettingTabs(LobbyViewSettingsPane __instance)
    {
        // Handle different gamemodes and tabs needed therein.
        int next = 3;
        // create TOR settings
        createCustomButton(__instance, next++, "TORSettings", ModTranslation.getString("torSettings"),
            CustomOptionType.General);
        // create TOR settings
        createCustomButton(__instance, next++, "RoleOverview", ModTranslation.getString("roleOverview"),
            (CustomOptionType)99);
        // IMp
        createCustomButton(__instance, next++, "ImpostorSettings", ModTranslation.getString("impostorRoles"),
            CustomOptionType.Impostor);

        // Neutral
        createCustomButton(__instance, next++, "NeutralSettings", ModTranslation.getString("neutralRoles"),
            CustomOptionType.Neutral);
        // Crew
        createCustomButton(__instance, next++, "CrewmateSettings", ModTranslation.getString("crewmateRoles"),
            CustomOptionType.Crewmate);
        // Modifier
        createCustomButton(__instance, next++, "ModifierSettings", ModTranslation.getString("modifiers"),
            CustomOptionType.Modifier);
    }
}

[HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.CreateSettings))]
internal class GameOptionsMenuCreateSettingsPatch
{
    public static void Postfix(GameOptionsMenu __instance)
    {
        if (__instance.gameObject.name == "GAME SETTINGS TAB")
            adaptTaskCount(__instance);
    }

    private static void adaptTaskCount(GameOptionsMenu __instance)
    {
        // Adapt task count for main options
        NumberOption commonTasksOption = __instance.Children.ToArray()
            .FirstOrDefault(x => x.TryCast<NumberOption>()?.intOptionName == Int32OptionNames.NumCommonTasks)
            .Cast<NumberOption>();
        if (commonTasksOption != null) commonTasksOption.ValidRange = new FloatRange(0f, 4f);
        NumberOption shortTasksOption = __instance.Children.ToArray()
            .FirstOrDefault(x => x.TryCast<NumberOption>()?.intOptionName == Int32OptionNames.NumShortTasks)
            .TryCast<NumberOption>();
        if (shortTasksOption != null) shortTasksOption.ValidRange = new FloatRange(0f, 23f);
        NumberOption longTasksOption = __instance.Children.ToArray()
            .FirstOrDefault(x => x.TryCast<NumberOption>()?.intOptionName == Int32OptionNames.NumLongTasks)
            .TryCast<NumberOption>();
        if (longTasksOption != null) longTasksOption.ValidRange = new FloatRange(0f, 15f);
    }
}

[HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.Start))]
internal class GameOptionsMenuStartPatch
{
    public static List<GameObject> currentTabs = new();
    public static List<PassiveButton> currentButtons = new();

    public static void Postfix(GameSettingMenu __instance)
    {
        currentTabs.ForEach(x => x?.Destroy());
        currentButtons.ForEach(x => x?.Destroy());
        currentTabs = new List<GameObject>();
        currentButtons = new List<PassiveButton>();

        if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return;

        removeVanillaTabs(__instance);

        createSettingTabs(__instance);

        // create copy to clipboard and paste from clipboard buttons.
        GameObject template = GameObject.Find("PlayerOptionsMenu(Clone)").transform.Find("CloseButton").gameObject;
        GameObject holderGO = new("copyPasteButtonParent");
        SpriteRenderer bgrenderer = holderGO.AddComponent<SpriteRenderer>();
        bgrenderer.sprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.CopyPasteBG.png", 175f);
        holderGO.transform.SetParent(template.transform.parent, false);
        holderGO.transform.localPosition = template.transform.localPosition + new Vector3(-8.3f, 0.73f, -2f);
        holderGO.layer = template.layer;
        holderGO.SetActive(true);
        GameObject copyButton = GameObject.Instantiate(template, holderGO.transform);
        copyButton.transform.localPosition = new Vector3(-0.3f, 0.02f, -2f);
        PassiveButton copyButtonPassive = copyButton.GetComponent<PassiveButton>();
        SpriteRenderer copyButtonRenderer = copyButton.GetComponentInChildren<SpriteRenderer>();
        SpriteRenderer copyButtonActiveRenderer = copyButton.transform.GetChild(1).GetComponent<SpriteRenderer>();
        copyButtonRenderer.sprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Copy.png", 100f);
        copyButton.transform.GetChild(1).transform.localPosition = Vector3.zero;
        copyButtonActiveRenderer.sprite =
            Helpers.loadSpriteFromResources("TheOtherRoles.Resources.CopyActive.png", 100f);
        copyButtonPassive.OnClick.RemoveAllListeners();
        copyButtonPassive.OnClick = new Button.ButtonClickedEvent();
        copyButtonPassive.OnClick.AddListener((Action)(() =>
        {
            copyToClipboard();
            copyButtonRenderer.color = Color.green;
            copyButtonActiveRenderer.color = Color.green;
            __instance.StartCoroutine(Effects.Lerp(1f, new Action<float>(p =>
            {
                if (p > 0.95)
                {
                    copyButtonRenderer.color = Color.white;
                    copyButtonActiveRenderer.color = Color.white;
                }
            })));
        }));
        GameObject pasteButton = GameObject.Instantiate(template, holderGO.transform);
        pasteButton.transform.localPosition = new Vector3(0.3f, 0.02f, -2f);
        PassiveButton pasteButtonPassive = pasteButton.GetComponent<PassiveButton>();
        SpriteRenderer pasteButtonRenderer = pasteButton.GetComponentInChildren<SpriteRenderer>();
        SpriteRenderer pasteButtonActiveRenderer = pasteButton.transform.GetChild(1).GetComponent<SpriteRenderer>();
        pasteButtonRenderer.sprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Paste.png", 100f);
        pasteButtonActiveRenderer.sprite =
            Helpers.loadSpriteFromResources("TheOtherRoles.Resources.PasteActive.png", 100f);
        pasteButtonPassive.OnClick.RemoveAllListeners();
        pasteButtonPassive.OnClick = new Button.ButtonClickedEvent();
        pasteButtonPassive.OnClick.AddListener((Action)(() =>
        {
            pasteButtonRenderer.color = Color.yellow;
            int success = pasteFromClipboard();
            pasteButtonRenderer.color = success == 3 ? Color.green : success == 0 ? Color.red : Color.yellow;
            pasteButtonActiveRenderer.color = success == 3 ? Color.green : success == 0 ? Color.red : Color.yellow;
            __instance.StartCoroutine(Effects.Lerp(1f, new Action<float>(p =>
            {
                if (p > 0.95)
                {
                    pasteButtonRenderer.color = Color.white;
                    pasteButtonActiveRenderer.color = Color.white;
                }
            })));
        }));
    }

    private static void createSettings(GameOptionsMenu menu, List<CustomOption> options)
    {
        float num = 1.5f;
        foreach (CustomOption option in options)
        {
            if (option.isHeader)
            {
                CategoryHeaderMasked categoryHeaderMasked = Object.Instantiate(menu.categoryHeaderOrigin, Vector3.zero,
                    Quaternion.identity, menu.settingsContainer);
                categoryHeaderMasked.SetHeader(StringNames.ImpostorsCategory, 20);
                categoryHeaderMasked.Title.text = option.getHeading() != "" ? option.getHeading() : option.getName();
                categoryHeaderMasked.transform.localScale = Vector3.one * 0.63f;
                categoryHeaderMasked.transform.localPosition = new Vector3(-0.903f, num, -2f);
                num -= 0.63f;
            }
            else if (option.parent != null && (option.parent.selection == 0 ||
                                               (option.parent.parent != null && option.parent.parent.selection == 0)))
                continue; // Hides options, for which the parent is disabled!

            OptionBehaviour optionBehaviour = Object.Instantiate(menu.stringOptionOrigin, Vector3.zero,
                Quaternion.identity, menu.settingsContainer);
            optionBehaviour.transform.localPosition = new Vector3(0.952f, num, -2f);
            optionBehaviour.SetClickMask(menu.ButtonClickMask);

            // "SetUpFromData"
            SpriteRenderer[] componentsInChildren = optionBehaviour.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < componentsInChildren.Length; i++)
                componentsInChildren[i].material.SetInt(PlayerMaterial.MaskLayer, 20);
            foreach (TextMeshPro textMeshPro in optionBehaviour.GetComponentsInChildren<TextMeshPro>(true))
            {
                textMeshPro.fontMaterial.SetFloat("_StencilComp", 3f);
                textMeshPro.fontMaterial.SetFloat("_Stencil", 20);
            }

            StringOption stringOption = optionBehaviour as StringOption;
            stringOption.OnValueChanged = new Action<OptionBehaviour>(o => { });
            stringOption.TitleText.text = option.getName();
            if (option.isHeader && option.getHeading() == "" && (option.type == CustomOptionType.Neutral ||
                                                                 option.type == CustomOptionType.Crewmate ||
                                                                 option.type == CustomOptionType.Impostor ||
                                                                 option.type == CustomOptionType.Modifier))
                stringOption.TitleText.text = ModTranslation.getString("spawnChance");
            if (stringOption.TitleText.text.Length > 25)
                stringOption.TitleText.fontSize = 2.2f;
            if (stringOption.TitleText.text.Length > 40)
                stringOption.TitleText.fontSize = 2f;
            stringOption.Value = stringOption.oldValue = option.selection;
            stringOption.ValueText.text = option.getString();
            option.optionBehaviour = stringOption;

            menu.Children.Add(optionBehaviour);
            num -= 0.45f;
            menu.scrollBar.SetYBoundsMax(-num - 1.65f);
        }

        for (int i = 0; i < menu.Children.Count; i++)
        {
            OptionBehaviour optionBehaviour = menu.Children[i];
            if (AmongUsClient.Instance && !AmongUsClient.Instance.AmHost) optionBehaviour.SetAsPlayer();
        }
    }

    private static void removeVanillaTabs(GameSettingMenu __instance)
    {
        GameObject.Find("What Is This?")?.Destroy();
        GameObject.Find("GamePresetButton")?.Destroy();
        GameObject.Find("RoleSettingsButton")?.Destroy();
        __instance.ChangeTab(1, false);
    }

    public static void createCustomButton(GameSettingMenu __instance, int targetMenu, string buttonName,
        string buttonText)
    {
        GameObject leftPanel = GameObject.Find("LeftPanel");
        GameObject buttonTemplate = GameObject.Find("GameSettingsButton");
        if (targetMenu == 3)
        {
            buttonTemplate.transform.localPosition -= Vector3.up * 0.85f;
            buttonTemplate.transform.localScale *= Vector2.one * 0.75f;
        }

        GameObject torSettingsButton = GameObject.Find(buttonName);
        if (torSettingsButton == null)
        {
            torSettingsButton = GameObject.Instantiate(buttonTemplate, leftPanel.transform);
            torSettingsButton.transform.localPosition += Vector3.up * 0.5f * (targetMenu - 2);
            torSettingsButton.name = buttonName;
            __instance.StartCoroutine(Effects.Lerp(2f,
                new Action<float>(p =>
                {
                    torSettingsButton.transform.FindChild("FontPlacer").GetComponentInChildren<TextMeshPro>().text =
                        buttonText;
                })));
            PassiveButton torSettingsPassiveButton = torSettingsButton.GetComponent<PassiveButton>();
            torSettingsPassiveButton.OnClick.RemoveAllListeners();
            torSettingsPassiveButton.OnClick.AddListener((Action)(() => { __instance.ChangeTab(targetMenu, false); }));
            torSettingsPassiveButton.OnMouseOut.RemoveAllListeners();
            torSettingsPassiveButton.OnMouseOver.RemoveAllListeners();
            torSettingsPassiveButton.SelectButton(false);
            currentButtons.Add(torSettingsPassiveButton);
        }
    }

    public static void createGameOptionsMenu(GameSettingMenu __instance, CustomOptionType optionType,
        string settingName)
    {
        GameObject tabTemplate = GameObject.Find("GAME SETTINGS TAB");
        currentTabs.RemoveAll(x => x == null);

        GameObject torSettingsTab = GameObject.Instantiate(tabTemplate, tabTemplate.transform.parent);
        torSettingsTab.name = settingName;

        GameOptionsMenu torSettingsGOM = torSettingsTab.GetComponent<GameOptionsMenu>();

        updateGameOptionsMenu(optionType, torSettingsGOM);

        currentTabs.Add(torSettingsTab);
        torSettingsTab.SetActive(false);
    }

    public static void updateGameOptionsMenu(CustomOptionType optionType, GameOptionsMenu torSettingsGOM)
    {
        foreach (OptionBehaviour child in torSettingsGOM.Children) child.Destroy();
        torSettingsGOM.scrollBar.transform.FindChild("SliderInner").DestroyChildren();
        torSettingsGOM.Children.Clear();
        List<CustomOption> relevantOptions = options.Where(x => x.type == optionType).ToList();
        createSettings(torSettingsGOM, relevantOptions);
    }

    private static void createSettingTabs(GameSettingMenu __instance)
    {
        // Handle different gamemodes and tabs needed therein.
        int next = 3;
        {
            // create TOR settings
            createCustomButton(__instance, next++, "TORSettings", ModTranslation.getString("torSettings"));
            createGameOptionsMenu(__instance, CustomOptionType.General, "TORSettings");

            // IMp
            createCustomButton(__instance, next++, "ImpostorSettings", ModTranslation.getString("impostorRoles"));
            createGameOptionsMenu(__instance, CustomOptionType.Impostor, "ImpostorSettings");

            // Neutral
            createCustomButton(__instance, next++, "NeutralSettings", ModTranslation.getString("neutralRoles"));
            createGameOptionsMenu(__instance, CustomOptionType.Neutral, "NeutralSettings");
            // Crew
            createCustomButton(__instance, next++, "CrewmateSettings", ModTranslation.getString("crewmateRoles"));
            createGameOptionsMenu(__instance, CustomOptionType.Crewmate, "CrewmateSettings");
            // Modifier
            createCustomButton(__instance, next++, "ModifierSettings", ModTranslation.getString("modifiers"));
            createGameOptionsMenu(__instance, CustomOptionType.Modifier, "ModifierSettings");
        }
    }
}

[HarmonyPatch(typeof(StringOption), nameof(StringOption.Initialize))]
public class StringOptionEnablePatch
{
    public static bool Prefix(StringOption __instance)
    {
        CustomOption option = options.FirstOrDefault(option => option.optionBehaviour == __instance);
        if (option == null) return true;

        __instance.OnValueChanged = new Action<OptionBehaviour>(o => { });
        //__instance.TitleText.text = option.getName();
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
        CustomOption option = options.FirstOrDefault(option => option.optionBehaviour == __instance);
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
        CustomOption option = options.FirstOrDefault(option => option.optionBehaviour == __instance);
        if (option == null) return true;
        option.updateSelection(option.selection - 1);
        return false;
    }
}

[HarmonyPatch(typeof(StringOption), nameof(StringOption.FixedUpdate))]
public class StringOptionFixedUpdate
{
    public static void Postfix(StringOption __instance)
    {
        if (!IL2CPPChainloader.Instance.Plugins.TryGetValue("com.DigiWorm.LevelImposter", out PluginInfo _)) return;
        CustomOption option = options.FirstOrDefault(option => option.optionBehaviour == __instance);
        if (option == null) return;
        __instance.Value = __instance.oldValue = option.selection;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings))]
public class RpcSyncSettingsPatch
{
    public static void Postfix()
    {
        ShareOptionSelections();
        saveVanillaOptions();
    }
}

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.CoSpawnPlayer))]
public class AmongUsClientOnPlayerJoinedPatch
{
    public static void Postfix()
    {
        if (PlayerControl.LocalPlayer != null && AmongUsClient.Instance.AmHost)
            //GameManager.Instance.LogicOptions.SyncOptions();
            ShareOptionSelections();
    }
}

[HarmonyPatch]
internal class GameOptionsDataPatch
{
    public static int maxPage = 7;

    private static string buildRoleOptions()
    {
        string impRoles = buildOptionsOfType(CustomOptionType.Impostor, true) + "\n";
        string neutralRoles = buildOptionsOfType(CustomOptionType.Neutral, true) + "\n";
        string crewRoles = buildOptionsOfType(CustomOptionType.Crewmate, true) + "\n";
        string modifiers = buildOptionsOfType(CustomOptionType.Modifier, true);
        return impRoles + neutralRoles + crewRoles + modifiers;
    }

    public static string buildModifierExtras(CustomOption customOption)
    {
        // find options children with quantity
        IEnumerable<CustomOption> children = options.Where(o => o.parent == customOption);
        List<CustomOption> quantity = children.Where(o => o.getName().Contains("Quantity")).ToList();
        if (customOption.getSelection() == 0) return "";
        if (quantity.Count == 1) return $" ({quantity[0].getQuantity()})";
        if (customOption == CustomOptionHolder.loversSpawnRate)
            return " (1 " + ModTranslation.getString("buildModifierExtras") +
                   $" {CustomOptionHolder.loversImpLoverRate.getSelection() * 10}%)";
        return "";
    }

    private static string buildOptionsOfType(CustomOptionType type, bool headerOnly)
    {
        StringBuilder sb = new("\n");
        IEnumerable<CustomOption> options = CustomOption.options.Where(o => o.type == type);

        foreach (CustomOption option in options)
            if (option.parent == null)
            {
                string line = $"{option.getName()}: {option.getString()}";
                if (type == CustomOptionType.Modifier) line += buildModifierExtras(option);
                sb.AppendLine(line);
            }
            else if (option.parent.getSelection() > 0)
            {
                if (option.id == 224) //Sidekick
                    sb.AppendLine(
                        $"- {Helpers.cs(Sidekick.color, ModTranslation.getString("sidekick"))}: {option.getString()}");
                else if (option.id == 1901) // Created Madmate
                    sb.AppendLine($"- {Helpers.cs(Madmate.color, Madmate.fullName)}: {option.getString()}");
            }

        if (headerOnly) return sb.ToString();
        sb = new StringBuilder();

        foreach (CustomOption option in options)
            if (option.parent != null)
            {
                bool isIrrelevant = option.parent.getSelection() == 0 ||
                                    (option.parent.parent != null && option.parent.parent.getSelection() == 0);

                Color c = isIrrelevant ? Color.grey : Color.white; // No use for now
                if (isIrrelevant) continue;
                sb.AppendLine(Helpers.cs(c, $"{option.getName()}: {option.getString()}"));
            }
            else
            {
                if (option == CustomOptionHolder.crewmateRolesCountMin)
                {
                    string optionName = CustomOptionHolder.cs(new Color(204f / 255f, 204f / 255f, 0, 1f),
                        ModTranslation.getString("crewmateRoles"));
                    int min = CustomOptionHolder.crewmateRolesCountMin.getSelection();
                    int max = CustomOptionHolder.crewmateRolesCountMax.getSelection();
                    string optionValue = "";
                    if (min > max) min = max;
                    optionValue += min == max ? $"{max}" : $"{min} - {max}";
                    sb.AppendLine($"{optionName}: {optionValue}");
                }
                else if (option == CustomOptionHolder.neutralRolesCountMin)
                {
                    string optionName = CustomOptionHolder.cs(new Color(204f / 255f, 204f / 255f, 0, 1f),
                        ModTranslation.getString("neutralRoles"));
                    int min = CustomOptionHolder.neutralRolesCountMin.getSelection();
                    int max = CustomOptionHolder.neutralRolesCountMax.getSelection();
                    if (min > max) min = max;
                    string optionValue = min == max ? $"{max}" : $"{min} - {max}";
                    sb.AppendLine($"{optionName}: {optionValue}");
                }
                else if (option == CustomOptionHolder.impostorRolesCountMin)
                {
                    string optionName = CustomOptionHolder.cs(new Color(204f / 255f, 204f / 255f, 0, 1f),
                        ModTranslation.getString("impostorRoles"));
                    int min = CustomOptionHolder.impostorRolesCountMin.getSelection();
                    int max = CustomOptionHolder.impostorRolesCountMax.getSelection();
                    if (max > GameOptionsManager.Instance.currentGameOptions.NumImpostors)
                        max = GameOptionsManager.Instance.currentGameOptions.NumImpostors;
                    if (min > max) min = max;
                    string optionValue = min == max ? $"{max}" : $"{min} - {max}";
                    sb.AppendLine($"{optionName}: {optionValue}");
                }

                else if (option == CustomOptionHolder.crewmateRolesCountMax ||
                         option == CustomOptionHolder.neutralRolesCountMax ||
                         option == CustomOptionHolder.impostorRolesCountMax)
                {
                }
                else
                    sb.AppendLine($"\n{option.getName()}:{option.getString()}");
            }

        return sb.ToString();
    }

    public static string buildAllOptions(string vanillaSettings = "", bool hideExtras = false)
    {
        if (vanillaSettings == "")
            vanillaSettings =
                GameOptionsManager.Instance.CurrentGameOptions.ToHudString(PlayerControl.AllPlayerControls.Count);
        int counter = TheOtherRolesPlugin.optionsPage;
        string hudString = counter != 0 && !hideExtras
            ? Helpers.cs(DateTime.Now.Second % 2 == 0 ? Color.white : Color.red,
                $"{ModTranslation.getString("useScrollWheel")}\n\n")
            : "";

        maxPage = 7;
        switch (counter)
        {
            case 0:
                hudString += (!hideExtras ? "" : ModTranslation.getString("page1")) + vanillaSettings;
                break;
            case 1:
                hudString += ModTranslation.getString("page2") + buildOptionsOfType(CustomOptionType.General, false);
                break;
            case 2:
                hudString += ModTranslation.getString("page3") + buildRoleOptions();
                break;
            case 3:
                hudString += ModTranslation.getString("page4") + buildOptionsOfType(CustomOptionType.Impostor, false);
                break;
            case 4:
                hudString += ModTranslation.getString("page5") + buildOptionsOfType(CustomOptionType.Neutral, false);
                break;
            case 5:
                hudString += ModTranslation.getString("page6") + buildOptionsOfType(CustomOptionType.Crewmate, false);
                break;
            case 6:
                hudString += ModTranslation.getString("page7") + buildOptionsOfType(CustomOptionType.Modifier, false);
                break;
        }

        if (!hideExtras || counter != 0)
            hudString += string.Format(ModTranslation.getString("pressTabForMore"), counter + 1, maxPage);
        return hudString;
    }


    [HarmonyPatch(typeof(IGameOptionsExtensions), nameof(IGameOptionsExtensions.ToHudString))]
    private static void Postfix(ref string __result)
    {
        if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek)
            return; // Allow Vanilla Hide N Seek
        __result = buildAllOptions(__result);
    }

    [HarmonyPatch(typeof(StringGameSetting), nameof(StringGameSetting.GetValueString))]
    [HarmonyPrefix]
    public static bool AjdustStringForViewPanel(StringGameSetting __instance, float value, ref string __result)
    {
        if (__instance.OptionName != Int32OptionNames.KillDistance) return true;
        __result = LegacyGameOptions.KillDistanceStrings[(int)value];
        return false;
    }
}

[HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
public static class GameOptionsNextPagePatch
{
    public static void Postfix(KeyboardJoystick __instance)
    {
        int page = TheOtherRolesPlugin.optionsPage;
        if (Input.GetKeyDown(KeyCode.Tab)) TheOtherRolesPlugin.optionsPage = (TheOtherRolesPlugin.optionsPage + 1) % 7;
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) TheOtherRolesPlugin.optionsPage = 0;
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) TheOtherRolesPlugin.optionsPage = 1;
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) TheOtherRolesPlugin.optionsPage = 2;
        if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) TheOtherRolesPlugin.optionsPage = 3;
        if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5)) TheOtherRolesPlugin.optionsPage = 4;
        if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6)) TheOtherRolesPlugin.optionsPage = 5;
        if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7)) TheOtherRolesPlugin.optionsPage = 6;
        if (Input.GetKeyDown(KeyCode.F1))
            HudManagerUpdate.ToggleSettings(HudManager.Instance);
        if (TheOtherRolesPlugin.optionsPage >= GameOptionsDataPatch.maxPage) TheOtherRolesPlugin.optionsPage = 0;
    }
}

//This class is taken and adapted from Town of Us Reactivated, https://github.com/eDonnes124/Town-Of-Us-R/blob/master/source/Patches/CustomOption/Patches.cs, Licensed under GPLv3
[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public class HudManagerUpdate
{
    private static readonly TextMeshPro GameSettings = null;

    public static float
        MinX, /*-5.3F*/
        OriginalY = 2.9F,
        MinY = 2.9F;

    public static Scroller Scroller;
    private static Vector3 LastPosition;
    private static float lastAspect;
    private static bool setLastPosition;

    private static readonly TextMeshPro[] settingsTMPs = new TextMeshPro[4];
    private static GameObject settingsBackground;

    private static PassiveButton toggleSettingsButton;
    private static GameObject toggleSettingsButtonObject;

    public static void Prefix(HudManager __instance)
    {
        if (GameSettings?.transform == null) return;

        // Sets the MinX position to the left edge of the screen + 0.1 units
        Rect safeArea = Screen.safeArea;
        float aspect = Mathf.Min(Camera.main.aspect, safeArea.width / safeArea.height);
        float safeOrthographicSize = CameraSafeArea.GetSafeOrthographicSize(Camera.main);
        MinX = 0.1f - (safeOrthographicSize * aspect);

        if (!setLastPosition || aspect != lastAspect)
        {
            LastPosition = new Vector3(MinX, MinY);
            lastAspect = aspect;
            setLastPosition = true;
            if (Scroller != null) Scroller.ContentXBounds = new FloatRange(MinX, MinX);
        }

        CreateScroller(__instance);

        Scroller.gameObject.SetActive(GameSettings.gameObject.activeSelf);

        if (!Scroller.gameObject.active) return;

        int rows = GameSettings.text.Count(c => c == '\n');
        float LobbyTextRowHeight = 0.06F;
        float maxY = Mathf.Max(MinY, (rows * LobbyTextRowHeight) + ((rows - 38) * LobbyTextRowHeight));

        Scroller.ContentYBounds = new FloatRange(MinY, maxY);

        // Prevent scrolling when the player is interacting with a menu
        if (PlayerControl.LocalPlayer.CanMove != true)
        {
            GameSettings.transform.localPosition = LastPosition;

            return;
        }

        if (GameSettings.transform.localPosition.x != MinX ||
            GameSettings.transform.localPosition.y < MinY) return;

        LastPosition = GameSettings.transform.localPosition;
    }

    private static void CreateScroller(HudManager __instance)
    {
        if (Scroller != null) return;

        Transform target = GameSettings.transform;

        Scroller = new GameObject("SettingsScroller").AddComponent<Scroller>();
        Scroller.transform.SetParent(GameSettings.transform.parent);
        Scroller.gameObject.layer = 5;

        Scroller.transform.localScale = Vector3.one;
        Scroller.allowX = false;
        Scroller.allowY = true;
        Scroller.active = true;
        Scroller.velocity = new Vector2(0, 0);
        Scroller.ScrollbarYBounds = new FloatRange(0, 0);
        Scroller.ContentXBounds = new FloatRange(MinX, MinX);
        Scroller.enabled = true;

        Scroller.Inner = target;
        target.SetParent(Scroller.transform);
    }

    [HarmonyPrefix]
    public static void Prefix2(HudManager __instance)
    {
        if (!settingsTMPs[0]) return;
        foreach (TextMeshPro tmp in settingsTMPs) tmp.text = "";
        string settingsString = GameOptionsDataPatch.buildAllOptions(hideExtras: true);
        string[] blocks = settingsString.Split("\n\n", StringSplitOptions.RemoveEmptyEntries);
        ;
        string curString = "";
        string curBlock;
        int j = 0;
        for (int i = 0; i < blocks.Length; i++)
        {
            curBlock = blocks[i];
            if (Helpers.lineCount(curBlock) + Helpers.lineCount(curString) < 43)
                curString += curBlock + "\n\n";
            else
            {
                settingsTMPs[j].text = curString;
                j++;

                curString = "\n" + curBlock + "\n\n";
                if (curString.Substring(0, 2) != "\n\n") curString = "\n" + curString;
            }
        }

        if (j < settingsTMPs.Length) settingsTMPs[j].text = curString;
        int blockCount = 0;
        foreach (TextMeshPro tmp in settingsTMPs)
            if (tmp.text != "")
                blockCount++;
        for (int i = 0; i < blockCount; i++)
            settingsTMPs[i].transform.localPosition = new Vector3((-blockCount * 1.2f) + (2.7f * i), 2.2f, -500f);
    }

    public static void OpenSettings(HudManager __instance)
    {
        if (__instance.FullScreen == null || (MapBehaviour.Instance && MapBehaviour.Instance.IsOpen)) return;
        settingsBackground = GameObject.Instantiate(__instance.FullScreen.gameObject, __instance.transform);
        settingsBackground.SetActive(true);
        SpriteRenderer renderer = settingsBackground.GetComponent<SpriteRenderer>();
        renderer.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        renderer.enabled = true;

        for (int i = 0; i < settingsTMPs.Length; i++)
        {
            settingsTMPs[i] = GameObject.Instantiate(__instance.KillButton.cooldownTimerText, __instance.transform);
            settingsTMPs[i].alignment = TextAlignmentOptions.TopLeft;
            settingsTMPs[i].enableWordWrapping = false;
            settingsTMPs[i].transform.localScale = Vector3.one * 0.25f;
            settingsTMPs[i].gameObject.SetActive(true);
        }
    }

    public static void CloseSettings()
    {
        foreach (TextMeshPro tmp in settingsTMPs)
            if (tmp)
                tmp.gameObject.Destroy();

        if (settingsBackground) settingsBackground.Destroy();
    }

    public static void ToggleSettings(HudManager __instance)
    {
        if (settingsTMPs[0]) CloseSettings();
        else OpenSettings(__instance);
    }

    [HarmonyPostfix]
    public static void Postfix(HudManager __instance)
    {
        if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started) return;
        if (!toggleSettingsButton || !toggleSettingsButtonObject)
        {
            // add a special button for settings viewing:
            toggleSettingsButtonObject =
                GameObject.Instantiate(__instance.MapButton.gameObject, __instance.MapButton.transform.parent);
            toggleSettingsButtonObject.transform.localPosition =
                __instance.MapButton.transform.localPosition + new Vector3(0, -1.25f, -500f);
            toggleSettingsButtonObject.name = "TOGGLESETTINGSBUTTON";
            SpriteRenderer renderer =
                toggleSettingsButtonObject.transform.Find("Inactive").GetComponent<SpriteRenderer>();
            SpriteRenderer rendererActive =
                toggleSettingsButtonObject.transform.Find("Active").GetComponent<SpriteRenderer>();
            toggleSettingsButtonObject.transform.Find("Background").localPosition = Vector3.zero;
            renderer.sprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Settings_Button.png", 100f);
            rendererActive.sprite =
                Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Settings_ButtonActive.png", 100);
            toggleSettingsButton = toggleSettingsButtonObject.GetComponent<PassiveButton>();
            toggleSettingsButton.OnClick.RemoveAllListeners();
            toggleSettingsButton.OnClick.AddListener((Action)(() => ToggleSettings(__instance)));
        }

        toggleSettingsButtonObject.SetActive(__instance.MapButton.gameObject.active &&
                                             !(MapBehaviour.Instance && MapBehaviour.Instance.IsOpen) &&
                                             GameOptionsManager.Instance.currentGameOptions.GameMode !=
                                             GameModes.HideNSeek);
        toggleSettingsButtonObject.transform.localPosition =
            __instance.MapButton.transform.localPosition + new Vector3(0, -0.8f, -500f);
    }
}
