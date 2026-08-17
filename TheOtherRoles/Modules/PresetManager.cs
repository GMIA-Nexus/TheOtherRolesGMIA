using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TheOtherRoles.MetaContext;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace TheOtherRoles
{
    public static class PresetManager
    {
        // ======== 既存コード（CustomOption / CustomOptionHolder）から参照される Config ベースのプリセット API ========
        public static int CurrentIndex = 0;
        public static string GetCurrentConfigSection() => $"Preset{CurrentIndex}";
        public static string[] GetPresetNames() => ["preset1", "preset2", "preset3", "preset4", "preset5", "preset6"];

        public static void Load()
        {
            CurrentIndex = 0;
            string dir = Path.GetDirectoryName(Application.dataPath) + @"\CustomPreset\";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            RefreshPresetList();
        }

        // ======== CSV プリセット ========
        public static GameObject PresetInputBoxPrefab;

        public const string PresetNameTitle = "PresetName,";
        public const string IntroductionTitle = "Introduction,";

        public static List<PresetInfo> presetInfoList = new();
        static MetaScreen presetScreen = null;
        static int presetInfoPageNow = 1;
        static int presetInfoPageMax = 1;
        const int PresetInfoOnePageViewMax = 4;

        // ======== プリセットページ表示 ========
        public static void OpenPresetUI()
        {
            presetScreen = MetaScreen.GenerateWindow(new(7.4f, 4.6f), HudManager.InstanceExists ? HudManager.Instance.transform : null, Vector3.zero, true, false, background: BackgroundSetting.Modern);

            RefreshPresetList();
            RecalcPresetPage();
            UpdatePresetScreen();
        }

        static void RecalcPresetPage()
        {
            presetInfoPageMax = ((presetInfoList.Count - 1) / PresetInfoOnePageViewMax) + 1;
            presetInfoPageNow = Mathf.Clamp(presetInfoPageNow, 1, Mathf.Max(1, presetInfoPageMax));
        }

        static void RefreshPresetList()
        {
            presetInfoList.Clear();
            string dir = Path.GetDirectoryName(Application.dataPath) + @"\CustomPreset\";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            string[] fileNames = Directory.GetFiles(dir, "*.csv");
            foreach (string path in fileNames)
            {
                try
                {
                    using (var sr = new StreamReader(path, Encoding.UTF8))
                    {
                        string text = sr.ReadLine();
                        if (text == "# [CustomPreset]")
                        {
                            string name = sr.ReadLine();
                            if (name != null && name.Contains(PresetNameTitle))
                            {
                                string presetName = name.Substring(name.IndexOf(PresetNameTitle) + PresetNameTitle.Length);
                                presetInfoList.Add(new PresetInfo(presetName, path));
                            }
                        }
                    }
                }
                catch { }
            }

            presetInfoList.Sort((l, r) => l.registTime.CompareTo(r.registTime));
        }

        static void UpdatePresetScreen()
        {
            if (presetScreen == null) return;

            MetaContextOld context = new();

            // タイトル
            context.Append(new MetaContextOld.Text(new(TextAttribute.BoldAttr) { Size = new(2.2f, 0.3f) })
            {
                RawText = string.Format(ModTranslation.getString("presetTitle"), presetInfoPageNow, presetInfoPageMax)
            });
            context.Append(new MetaContextOld.VerticalMargin(0.15f));

            // プリセット一覧
            var nameAttr = new TextAttribute(TextAttribute.BoldAttr) { Size = new(3.2f, 0.32f), Alignment = TMPro.TextAlignmentOptions.Left };
            var subAttr = new TextAttribute(TextAttribute.BoldAttr) { Size = new(0.7f, 0.3f) };

            for (int i = 0; i < PresetInfoOnePageViewMax; i++)
            {
                int idx = (presetInfoPageNow - 1) * PresetInfoOnePageViewMax + i;
                if (idx >= presetInfoList.Count) break;
                var info = presetInfoList[idx];

                List<IMetaParallelPlacableOld> row = new()
                {
                    new MetaContextOld.Button(() => { }, nameAttr)
                    {
                        RawText = info.presetName,
                        PostBuilder = (button, _, _) =>
                        {
                            button.OnMouseOver.AddListener((Action)(() => TORGUIManager.Instance.SetHelpContext(button, GetPresetOverlay(info))));
                            button.OnMouseOut.AddListener((Action)(() => TORGUIManager.Instance.HideHelpContextIf(button)));
                        }
                    }
                };

                if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
                    row.Add(new MetaContextOld.Button(info.Load, subAttr) { TranslationKey = "presetLoad" });
                row.Add(new MetaContextOld.Button(() => OpenInputBox(true, info), subAttr) { TranslationKey = "presetRename" });
                row.Add(new MetaContextOld.Button(() => OnDeletePreset(info), subAttr) { TranslationKey = "presetDelete" });

                context.Append(new CombinedContextOld(0.5f, row.ToArray()));
                context.Append(new MetaContextOld.VerticalMargin(0.1f));
            }

            context.Append(new MetaContextOld.VerticalMargin(0.2f));

            // 作成・ページ送り
            var bottomAttr = new TextAttribute(TextAttribute.BoldAttr) { Size = new(1.2f, 0.3f) };
            List<IMetaParallelPlacableOld> bottom = new();
            if (AmongUsClient.Instance != null && LobbyBehaviour.Instance)
            {
                bottom.Add(new MetaContextOld.Button(() => OpenInputBox(false, null), bottomAttr) { TranslationKey = "presetCreate" });
            }
            if (presetInfoPageMax > 1)
            {
                bottom.Add(new MetaContextOld.Button(() => { if (--presetInfoPageNow <= 0) presetInfoPageNow = presetInfoPageMax; UpdatePresetScreen(); }, bottomAttr) { RawText = "◀" });
                bottom.Add(new MetaContextOld.Button(() => { if (++presetInfoPageNow > presetInfoPageMax) presetInfoPageNow = 1; UpdatePresetScreen(); }, bottomAttr) { RawText = "▶" });
            }
            context.Append(new CombinedContextOld(0.5f, bottom.ToArray()));

            presetScreen.SetContext(context);
        }

        static void OnDeletePreset(PresetInfo info)
        {
            info.Delete();
            presetInfoList.Remove(info);
            RecalcPresetPage();
            UpdatePresetScreen();
        }

        static GUIContext GetPresetOverlay(PresetInfo info)
        {
            var gui = TORGUIContextEngine.API;
            List<GUIContext> contents = new()
            {
                gui.RawText(GUIAlignment.Left, gui.GetAttribute(AttributeAsset.OverlayTitle), info.presetName)
            };
            string intro = string.IsNullOrEmpty(info.introduction) ? ModTranslation.getString("presetNoIntroduction") : info.introduction;
            contents.Add(gui.RawText(GUIAlignment.Left, gui.GetAttribute(AttributeAsset.OverlayContent), intro));
            return gui.VerticalHolder(GUIAlignment.Left, contents);
        }

        static void OpenInputBox(bool isRename, PresetInfo target)
        {
            var window = MetaScreen.GenerateWindow(new(6f, 2.3f), HudManager.InstanceExists ? HudManager.Instance.transform : null, Vector3.zero, true, true, background: BackgroundSetting.Modern);
            var gui = TORGUIContextEngine.API;
            var nameTextField = new GUITextField(GUIAlignment.Center, new(4.3f, 0.4f)) { HintText = ModTranslation.getString("presetNamePlaceholder").Color(Color.gray), DefaultText = target?.presetName ?? "", IsSharpField = false, WithMaskMaterial = false };
            var introTextField = new GUITextField(GUIAlignment.Center, new(4.3f, 0.4f)) { HintText = ModTranslation.getString("presetIntroductionPlaceholder").Color(Color.gray), DefaultText = target?.introduction ?? "", IsSharpField = false, WithMaskMaterial = false };
            var button = new GUIButton(GUIAlignment.Center, gui.GetAttribute(AttributeAsset.CenteredBoldFixed), new TranslateTextComponent("presetConfirm")) { OnClick = () =>
            {
                var nameText = nameTextField.Artifact.FirstOrDefault()?.Text ?? "";
                var introText = introTextField.Artifact.FirstOrDefault()?.Text ?? "";
                if (isRename && target != null)
                    target.Rename(nameText, introText);
                else
                    CreateNewPreset(nameText, introText);

                window.CloseScreen();
                UpdatePresetScreen();
            }
            };
            window.SetContext(gui.VerticalHolder(GUIAlignment.Center, nameTextField, introTextField, gui.VerticalMargin(0.2f), button), new Vector2(0.5f, 0.5f), out var size);
            nameTextField.Artifact.Do(field => field.GainFocus());
        }

        static void CreateNewPreset(string name, string introduction)
        {
            long registTime = DateTime.Now.Ticks;
            var presetInfo = new PresetInfo(name)
            {
                introduction = introduction ?? "",
                registTime = registTime
            };
            presetInfo.Save();
            RefreshPresetList();
            presetInfoPageNow = presetInfoPageMax = ((presetInfoList.Count - 1) / PresetInfoOnePageViewMax) + 1;
        }

        static void SetLayerRecursively(Transform transform, int layer)
        {
            transform.gameObject.layer = layer;
            for (int i = 0; i < transform.childCount; i++)
                SetLayerRecursively(transform.GetChild(i), layer);
        }

        // ======== プリセット情報 ========
        public class PresetInfo
        {
            public string presetName = "";
            public string introduction = "";
            public long registTime = 0;
            public string filePath;
            public Dictionary<int, string> optionValueTable = new();

            public PresetInfo(string presetName, string filePath = "")
            {
                this.presetName = !string.IsNullOrEmpty(presetName) ? presetName : "NewPreset";
                this.filePath = !string.IsNullOrEmpty(filePath) ? filePath : GetFilePath(this.presetName);

                if (File.Exists(filePath))
                {
                    using (var sr = new StreamReader(filePath, Encoding.UTF8))
                    {
                        int count = 0;
                        while (!sr.EndOfStream)
                        {
                            string text = sr.ReadLine();
                            if (count > 1)
                            {
                                if (text.StartsWith(IntroductionTitle))
                                {
                                    introduction = text.Substring(IntroductionTitle.Length);
                                }
                                else
                                {
                                    var elements = text.Split(',');
                                    if (elements.Length >= 2 && int.TryParse(elements[0], out var key))
                                        optionValueTable.Add(key, elements[1]);
                                }
                            }
                            ++count;
                        }
                    }

                    if (optionValueTable.ContainsKey(0) && long.TryParse(optionValueTable[0], out long time))
                        registTime = time;
                }
            }

            public void Save()
            {
                using (var sw = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("# [CustomPreset]");
                    sw.WriteLine(string.Format("{0},{1}", "PresetName", presetName));
                    sw.WriteLine(string.Format("{0}{1}", IntroductionTitle, introduction ?? ""));
                    sw.WriteLine(string.Format("{0},{1}", 0, registTime));
                    try
                    {
                        BasicOptions.Save(optionValueTable, sw);
                    }
                    catch (Exception e)
                    {
                        TheOtherRolesPlugin.Logger.LogError($"Failed to save vanilla options to preset (ignored): {e}");
                    }
                    foreach (CustomOption option in CustomOption.options)
                    {
                        if (option.id == 0) continue;
                        if (option is CustomFilterOption filterOption)
                        {
                            string value = string.Join(";", filterOption.filterSelection.Select(x => x.nameKey));
                            if (optionValueTable.TryGetValue(option.id, out var v))
                                value = v;
                            else
                                optionValueTable[option.id] = value;

                            sw.Write($"{option.id},{value}");
                        }
                        else
                        {
                            int value = option.selection;
                            if (optionValueTable.TryGetValue(option.id, out string v))
                                _ = int.TryParse(v, out value);
                            else
                                optionValueTable[option.id] = value.ToString();

                            sw.WriteLine(string.Format("{0},{1}", option.id, value));
                        }
                    }
                }
            }

            public void Load()
            {
                // 只有房主能在房间里加载预设。非房主（大厅/对局内）加载改不了房主设置，
                // 而且 ShareOptionSelections 会把自定义选项广播给全房造成不同步。
                // 主菜单（NotJoined，尚未加入房间）允许加载，因为那是在设置自己的主机选项。
                if (AmongUsClient.Instance != null && AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.NotJoined && !AmongUsClient.Instance.AmHost)
                {
                    TheOtherRolesPlugin.Logger.LogWarning("PresetManager: only the host can load a preset in the lobby.");
                    return;
                }

                try
                {
                    BasicOptions.Load(optionValueTable);
                }
                catch (Exception e)
                {
                    TheOtherRolesPlugin.Logger.LogError($"Failed to load vanilla options from preset (ignored): {e}");
                }
                foreach (CustomOption option in CustomOption.options)
                {
                    if (option.id == 0) continue;
                    if (option is CustomFilterOption filterOption)
                    {
                        string v = string.Join(";", filterOption.defaultFilterSelection.Select(x => x.nameKey));
                        if (optionValueTable.TryGetValue(option.id, out string value))
                            v = value;
                        var roleNames = v.Split(';');
                        filterOption.filterSelection.Clear();
                        foreach (var roleName in roleNames)
                        {
                            var role = RoleInfo.allRoleInfos.FirstOrDefault(x => x.nameKey == roleName);
                            if (role != null)
                                filterOption.filterSelection.Add(role);
                        }
                        filterOption.filterEntry.Value = string.Join(",", filterOption.filterSelection.Select(r => r.nameKey));
                        CustomOption.ShareFilterOptionChange((uint)option.id);
                    }
                    else
                    {
                        int v = option.defaultSelection;
                        if (optionValueTable.TryGetValue(option.id, out string value))
                            _ = int.TryParse(value, out v);
                        option.updateSelection(v, false);
                    }
                }
                CustomOption.ShareOptionSelections();
                if (PlayerControl.LocalPlayer)
                    PlayerControl.LocalPlayer.RpcSyncSettings(GameOptionsManager.Instance.gameOptionsFactory.ToBytes(GameOptionsManager.Instance.currentGameOptions, false));  // TODO Maybe simpler?? 
            }

            public void Rename(string newPresetName, string newIntroduction)
            {
                if (!string.IsNullOrEmpty(newPresetName)) presetName = newPresetName;
                introduction = newIntroduction ?? "";
                // 让 CSV 文件名跟随预设名（重名自动加序号）
                string newPath = GetFilePath(presetName);
                if (!string.Equals(Path.GetFullPath(newPath), Path.GetFullPath(filePath), StringComparison.OrdinalIgnoreCase))
                {
                    if (File.Exists(filePath))
                    {
                        if (File.Exists(newPath)) File.Delete(newPath);
                        File.Move(filePath, newPath);
                    }
                    filePath = newPath;
                }
                Save();
            }

            public void Delete()
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }

            // 用预设名作为 CSV 文件名（保留中文，替换 Windows 非法文件名字符，重名加序号）
            static string GetFilePath(string presetName)
            {
                string dir = Path.GetDirectoryName(Application.dataPath) + @"\CustomPreset\";
                string safe = SanitizeFileName(presetName);
                string path = Path.Combine(dir, safe + ".csv");
                int i = 1;
                while (File.Exists(path))
                    path = Path.Combine(dir, $"{safe} ({i++}).csv");
                return path;
            }

            static string SanitizeFileName(string name)
            {
                if (string.IsNullOrEmpty(name)) name = "NewPreset";
                var invalid = Path.GetInvalidFileNameChars();
                var sb = new StringBuilder();
                foreach (char c in name)
                    sb.Append(invalid.Contains(c) ? '_' : c);
                return sb.ToString();
            }
        }
    }
}
