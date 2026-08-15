using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using AmongUs.GameOptions;
using BepInEx.Configuration;
using TMPro;
using TheOtherRoles.MetaContext;
using UnityEngine;
using UnityEngine.EventSystems;
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
            if (presetScreen) { presetScreen.CloseScreen(); presetScreen = null; }

            var parent = GetUIParent();
            if (parent == null) return;

            RefreshPresetList();
            RecalcPresetPage();

            presetScreen = MetaScreen.GenerateWindow(new(7.4f, 4.6f), parent, Vector3.zero, true, false);
            UpdatePresetScreen();
        }

        static Transform GetUIParent()
        {
            if (HudManager.Instance) return HudManager.Instance.transform;
            if (Camera.main) return Camera.main.transform;
            return null;
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

                if (AmongUsClient.Instance.AmHost && AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started)
                {
                    var loadInfo = info;
                    row.Add(new MetaContextOld.Button(() => loadInfo.Load(), subAttr) { TranslationKey = "presetLoad", Color = Color.yellow });
                }

                if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started)
                {
                    var renameInfo = info;
                    row.Add(new MetaContextOld.Button(() => OpenInputBox(true, renameInfo), subAttr) { TranslationKey = "presetRename", Color = Color.cyan });
                }

                var deleteInfo = info;
                row.Add(new MetaContextOld.Button(() => OnDeletePreset(deleteInfo), subAttr) { TranslationKey = "presetDelete", Color = new Color32(235, 76, 70, 0xff) });

                context.Append(new CombinedContextOld(0.5f, row.ToArray()));
                context.Append(new MetaContextOld.VerticalMargin(0.1f));
            }

            context.Append(new MetaContextOld.VerticalMargin(0.2f));

            // 作成・ページ送り
            var bottomAttr = new TextAttribute(TextAttribute.BoldAttr) { Size = new(1.2f, 0.3f) };
            List<IMetaParallelPlacableOld> bottom = new();
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started)
            {
                bottom.Add(new MetaContextOld.Button(() => OpenInputBox(false, null), bottomAttr) { TranslationKey = "presetCreate", Color = Color.green });
            }
            bottom.Add(new MetaContextOld.Button(() => { if (--presetInfoPageNow <= 0) presetInfoPageNow = presetInfoPageMax; UpdatePresetScreen(); }, bottomAttr) { RawText = "◀", Color = Color.white });
            bottom.Add(new MetaContextOld.Button(() => { if (++presetInfoPageNow > presetInfoPageMax) presetInfoPageNow = 1; UpdatePresetScreen(); }, bottomAttr) { RawText = "▶", Color = Color.white });
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

        // ======== 入力ボックス ========
        static MetaScreen inputBoxScreen = null;
        static PresetInputBox activeInputBox = null;
        static bool inputBoxIsRename = false;
        static PresetInfo inputBoxTarget = null;

        static void OpenInputBox(bool isRename, PresetInfo target)
        {
            inputBoxIsRename = isRename;
            inputBoxTarget = target;
            activeInputBox = null;

            if (inputBoxScreen) { inputBoxScreen.CloseScreen(); inputBoxScreen = null; }

            var parent = GetUIParent();
            if (parent == null) return;

            inputBoxScreen = MetaScreen.GenerateWindow(new(6.4f, 4.4f), parent, Vector3.zero, true, true);

            MetaContextOld context = new();
            context.Append(new MetaContextOld.Text(new(TextAttribute.BoldAttr) { Size = new(3.2f, 0.3f) })
            {
                RawText = ModTranslation.getString(isRename ? "presetRenameTitle" : "presetCreateTitle")
            });
            context.Append(new MetaContextOld.VerticalMargin(0.15f));

            if (PresetInputBoxPrefab)
            {
                context.Append(new MetaContextOld.CustomContext(new(5.8f, 2.8f), IMetaContextOld.AlignmentOption.Center, (parentTransform, center) =>
                {
                    var obj = Object.Instantiate(PresetInputBoxPrefab, parentTransform);
                    obj.transform.localPosition = new Vector3(center.x, center.y, -0.1f);
                    obj.transform.localScale = Vector3.one;
                    SetLayerRecursively(obj.transform, LayerMask.NameToLayer("UI"));

                    activeInputBox = obj.GetComponent<PresetInputBox>();
                    if (!activeInputBox) activeInputBox = obj.AddComponent<PresetInputBox>();
                    activeInputBox.CenterContent(center);

                    if (isRename && target != null) activeInputBox.SetText(target.presetName, target.introduction);
                    activeInputBox.SetTitle(ModTranslation.getString(isRename ? "presetRenameTitle" : "presetCreateTitle"));
                    activeInputBox.SetPlaceholder(ModTranslation.getString("presetNamePlaceholder"), ModTranslation.getString("presetIntroductionPlaceholder"));
                    activeInputBox.SetCharacterLimit(18, 200);
                }));
            }
            else
            {
                context.Append(new MetaContextOld.Text(new(TextAttribute.ContentAttr) { Size = new(5.4f, 0.8f) })
                {
                    RawText = ModTranslation.getString("presetInputBoxMissing")
                });
            }

            context.Append(new MetaContextOld.VerticalMargin(0.15f));

            var buttonAttr = new TextAttribute(TextAttribute.BoldAttr) { Size = new(1.2f, 0.3f) };
            context.Append(new CombinedContextOld(0.5f,
                new MetaContextOld.Button(() => OnInputBoxConfirm(), buttonAttr) { TranslationKey = "presetConfirm", Color = Color.green },
                new MetaContextOld.Button(() => CloseInputBox(), buttonAttr) { TranslationKey = "presetCancel", Color = Color.red }
            ));

            inputBoxScreen.SetContext(context);
        }

        static void OnInputBoxConfirm()
        {
            if (activeInputBox != null)
            {
                string name = activeInputBox.NameText;
                string intro = activeInputBox.IntroductionText;
                if (inputBoxIsRename && inputBoxTarget != null)
                    inputBoxTarget.Rename(name, intro);
                else
                    CreateNewPreset(name, intro);
            }
            CloseInputBox();
            UpdatePresetScreen();
        }

        static void CloseInputBox()
        {
            if (inputBoxScreen) { inputBoxScreen.CloseScreen(); inputBoxScreen = null; }
            activeInputBox = null;
            inputBoxTarget = null;
        }

        static void CreateNewPreset(string name, string introduction)
        {
            long registTime = DateTime.Now.Ticks;
            var presetInfo = new PresetInfo(name);
            presetInfo.introduction = introduction ?? "";
            presetInfo.registTime = registTime;
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
            public string presetName;
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
                        int value = option.selection;
                        if (optionValueTable.TryGetValue(option.id, out string v))
                            int.TryParse(v, out value);
                        else
                            optionValueTable[option.id] = value.ToString();
                        sw.WriteLine(string.Format("{0},{1}", option.id, value));
                    }
                }
            }

            public void Load()
            {
                // 只有房主能在房间里加载预设。非房主（大厅/对局内）加载改不了房主设置，
                // 而且 ShareOptionSelections 会把自定义选项广播给全房造成不同步。
                // 主菜单（NotJoined，尚未加入房间）允许加载，因为那是在设置自己的主机选项。
                if (AmongUsClient.Instance != null
                    && AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.NotJoined
                    && !AmongUsClient.Instance.AmHost)
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
                    int v = option.defaultSelection;
                    if (optionValueTable.TryGetValue(option.id, out string value))
                        int.TryParse(value, out v);
                    option.updateSelection(v, false);
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

        // ======== PresetInputBox.prefab 用コンポーネント ========
        public class PresetInputBox : MonoBehaviour
        {
            static PresetInputBox() => ClassInjector.RegisterTypeInIl2Cpp<PresetInputBox>();
            public PresetInputBox(System.IntPtr ptr) : base(ptr) { }
            public PresetInputBox() : base(ClassInjector.DerivedConstructorPointer<PresetInputBox>()) { ClassInjector.DerivedConstructorBody(this); }

            private TMP_InputField nameField = null;
            private TMP_InputField introductionField = null;
            private TextMeshProUGUI titleLabel = null;

            // 待应用的值缓存：即使字段还没找到也先存起来，找到后立即应用
            private string pendingTitle = null;
            private string pendingName = null;
            private string pendingIntroduction = null;
            private string pendingNamePlaceholder = null;
            private string pendingIntroductionPlaceholder = null;
            private int pendingNameLimit = -1;
            private int pendingIntroductionLimit = -1;
            private bool didLogFields = false;
            private bool lastFocusedName = false;
            private bool lastFocusedIntro = false;

            public string NameText => nameField ? nameField.text : "";
            public string IntroductionText => introductionField ? introductionField.text : "";

            public void Awake()
            {
                EnsureInitialized();
            }

            void Update()
            {
                EnsureInitialized();
                if (!nameField && !introductionField) return;

                // 参考 HaomingMenu：TMP_InputField 原生负责输入与焦点切换（点击哪个框就聚焦哪个框，
                // 输入法/光标也跟过去），这里不再劫持 Input.inputString（之前劫持导致写错框/打不了字）。
                // 只补两点：1) Tab/回车 在名字、介绍之间切换焦点；2) 打印焦点变化便于确认 TMP 原生是否生效。
                bool nf = nameField.isFocused;
                bool inf = introductionField.isFocused;
                if (nf != lastFocusedName || inf != lastFocusedIntro)
                {
                    lastFocusedName = nf;
                    lastFocusedIntro = inf;
                    TheOtherRolesPlugin.Logger.LogMessage($"PresetInputBox focus: name={nf} intro={inf}");
                }

                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    FocusField(nf && !inf ? introductionField : nameField);
                }
                else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    if (nf && !inf) FocusField(introductionField);
                }
            }

            // Tab/回车用：主动把焦点给指定输入框。平时点击切换由 TMP 原生处理。
            void FocusField(TMP_InputField field)
            {
                if (field == null) return;
                try
                {
                    field.Select();
                    field.ActivateInputField();
                }
                catch { }
            }

            void EnsureInitialized()
            {
                if (nameField != null && introductionField != null)
                {
                    ApplyPending();
                    return;
                }

                var allFields = GetComponentsInChildren<TMP_InputField>(true);
                foreach (var f in allFields)
                {
                    if (f.name == "PresetName") nameField = f;
                    else if (f.name == "PresetIntroduction") introductionField = f;
                }
                if (nameField == null && allFields.Length > 0) nameField = allFields[0];
                if (introductionField == null && allFields.Length > 1) introductionField = allFields[1];

                // 一次性诊断：打印找到了哪些输入框（数量/名字/父物体），排查 prefab 里是否有重复的输入框
                if (!didLogFields && nameField != null && introductionField != null)
                {
                    didLogFields = true;
                    var sb = new System.Text.StringBuilder($"PresetInputBox fields(count={allFields.Length}):");
                    for (int i = 0; i < allFields.Length; i++)
                        sb.Append($" [{i}]'{allFields[i].name}' parent='{allFields[i].transform.parent?.name}'");
                    TheOtherRolesPlugin.Logger.LogMessage(sb.ToString());
                }

                if (titleLabel == null)
                {
                    // 优先：名字为 "Text" 且父物体名为 "Contents" 的 TMP 文本
                    foreach (var t in GetComponentsInChildren<TextMeshProUGUI>(true))
                    {
                        if (t.name == "Text" && t.transform.parent != null && t.transform.parent.name == "Contents")
                        {
                            titleLabel = t;
                            break;
                        }
                    }
                    // 兜底：第一个既不是输入框文字也不是占位符的 TMP 文本
                    if (titleLabel == null)
                    {
                        var inputRelated = new HashSet<TMP_Text>();
                        foreach (var f in allFields)
                        {
                            if (f.textComponent) inputRelated.Add(f.textComponent);
                            if (f.placeholder && f.placeholder.TryCast<TextMeshProUGUI>() is TextMeshProUGUI ph) inputRelated.Add(ph);
                        }
                        foreach (var t in GetComponentsInChildren<TextMeshProUGUI>(true))
                        {
                            if (inputRelated.Contains(t)) continue;
                            titleLabel = t;
                            break;
                        }
                    }
                }

                ApplyPending();
            }

            void ApplyPending()
            {
                if (nameField == null || introductionField == null) return;

                if (pendingTitle != null && titleLabel) { titleLabel.text = pendingTitle; pendingTitle = null; }
                if (pendingName != null && nameField) { nameField.text = pendingName; pendingName = null; }
                if (pendingIntroduction != null && introductionField) { introductionField.text = pendingIntroduction; pendingIntroduction = null; }
                if (pendingNamePlaceholder != null && nameField) { SetPlaceholderText(nameField, pendingNamePlaceholder); pendingNamePlaceholder = null; }
                if (pendingIntroductionPlaceholder != null && introductionField) { SetPlaceholderText(introductionField, pendingIntroductionPlaceholder); pendingIntroductionPlaceholder = null; }
                if (pendingNameLimit >= 0 && nameField) { nameField.characterLimit = pendingNameLimit; pendingNameLimit = -1; }
                if (pendingIntroductionLimit >= 0 && introductionField) { introductionField.characterLimit = pendingIntroductionLimit; pendingIntroductionLimit = -1; }
            }

            public void SetTitle(string title)
            {
                pendingTitle = title;
                EnsureInitialized();
            }

            public void SetText(string name, string introduction)
            {
                pendingName = name ?? "";
                pendingIntroduction = introduction ?? "";
                EnsureInitialized();
            }

            public void SetPlaceholder(string namePlaceholder, string introductionPlaceholder)
            {
                pendingNamePlaceholder = namePlaceholder;
                pendingIntroductionPlaceholder = introductionPlaceholder;
                EnsureInitialized();
            }

            static void SetPlaceholderText(TMP_InputField field, string text)
            {
                if (field.placeholder && field.placeholder.TryCast<TextMeshProUGUI>() is TextMeshProUGUI tmp)
                    tmp.text = text;
            }

            public void SetCharacterLimit(int nameLimit, int introductionLimit)
            {
                pendingNameLimit = nameLimit;
                pendingIntroductionLimit = introductionLimit;
                EnsureInitialized();
            }

            // PresetInputBox.prefab 的内容按左下角原点排版，这里整体平移预制体，
            // 让两个输入框的中点对准窗口中心（不依赖 sizeDelta，避免把内容推飞）。
            public void CenterContent(Vector2 windowCenter)
            {
                EnsureInitialized();
                if (!nameField || !introductionField) return;
                var parent = transform.parent;
                if (parent == null) return;
                try
                {
                    Vector2 mid = (((Vector2)nameField.transform.position) + ((Vector2)introductionField.transform.position)) * 0.5f;
                    Vector2 target = (Vector2)parent.TransformPoint(windowCenter);
                    transform.position += (Vector3)(target - mid);
                }
                catch { }
            }
        }
    }
}
