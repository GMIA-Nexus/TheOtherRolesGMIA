using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx.Unity.IL2CPP.Utils;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Hazel;
using Innersloth.IO;
using TheOtherRoles.MetaContext;
using UnityEngine;
using UnityEngine.Rendering;
using GUI = TheOtherRoles.MetaContext.TORGUIContextEngine;

namespace TheOtherRoles.Modules
{
    public interface ILifespan
    {
        bool IsDeadObject { get; }
        bool IsAliveObject { get => !IsDeadObject; }
    }

    public interface IReleasable
    {
        internal void Release();
    }

    abstract public class AchievementTokenBase : ILifespan, IReleasable
    {
        public ProgressRecord Achievement { get; private init; }
        abstract public ClearDisplayState UniteTo(bool update = true);

        public AchievementTokenBase(ProgressRecord achievement)
        {
            this.Achievement = achievement;
            TORGameManager.Instance?.AllAchievementTokens.Add(this);
        }
        public bool IsDeadObject { get; private set; } = false;

        public void Release()
        {
            IsDeadObject = true;
            TORGameManager.Instance?.AllAchievementTokens.Remove(this);
        }
    }

    public class StaticAchievementToken : AchievementTokenBase
    {
        public StaticAchievementToken(ProgressRecord record) : base(record) { }
        public StaticAchievementToken(string achievement)
            : base(TORAchievementManager.GetRecord(achievement, out var a) ? a : null!) { }


        public override ClearDisplayState UniteTo(bool update)
        {
            if (IsDeadObject) return ClearDisplayState.None;

            return Achievement?.Unite(1, update) ?? ClearDisplayState.None;
        }
    }

    public class AchievementToken<T> : AchievementTokenBase
    {
        public T Value;
        public Func<T, ProgressRecord, int> Supplier { get; set; }

        public AchievementToken(ProgressRecord achievement, T value, Func<T, ProgressRecord, int> supplier) : base(achievement)
        {
            Value = value;
            Supplier = supplier;
        }

        public AchievementToken(string achievement, T value, Func<T, ProgressRecord, int> supplier)
            : this(TORAchievementManager.GetRecord(achievement, out var a) ? a : null!, value, supplier) { }

        public AchievementToken(string achievement, T value, Func<T, ProgressRecord, bool> supplier)
            : this(achievement, value, (t, ac) => supplier.Invoke(t, ac) ? 1 : 0) { }


        public override ClearDisplayState UniteTo(bool update)
        {
            if (IsDeadObject) return ClearDisplayState.None;

            return Achievement.Unite(Supplier.Invoke(Value, Achievement), update);
        }
    }

    public class AchievementType
    {
        static public AchievementType Challenge = new("challenge");
        static public AchievementType Secret = new("secret");
        static public AchievementType Seasonal = new("seasonal");

        private AchievementType(string key)
        {
            TranslationKey = key + "AchievementType";
        }
        public string TranslationKey { get; private set; }
    }

    public enum ClearDisplayState
    {
        Clear,
        FirstClear,
        None
    }

    public class ProgressRecord
    {
        private IntegerDataEntry entry;
        private string key;
        private string hashedKey;
        private int goal;
        private bool canClearOnce;

        public int Progress => entry.Value;
        public int Goal => goal;
        public bool IsCleared => goal <= entry.Value;

        public ProgressRecord(string key, int goal, bool canClearOnce)
        {
            this.goal = goal;
            this.canClearOnce = canClearOnce;
            this.key = key;
            this.hashedKey = key.ComputeConstantHashAsString();
            this.entry = new IntegerDataEntry("a." + hashedKey, TORAchievementManager.AchievementDataSaver, 0);
            TORAchievementManager.RegisterRecord(this, key);
        }

        public virtual string Id => key;
        public string TranslateKeyInfo
        {
            get
            {
                return Id.Replace(".", "");
            }
        }

        public virtual string TranslationKey => TranslateKeyInfo + "AchievementTitle";
        public string GoalTranslationKey => TranslateKeyInfo + "AchievementGoal";
        public string CondTranslationKey => TranslateKeyInfo + "AchievementCond";
        public string FlavorTranslationKey => TranslateKeyInfo + "AchievementFlavor";

        protected void UpdateProgress(int newProgress) => entry.Value = newProgress;

        //トークンによってクリアする場合はこちらから
        virtual public ClearDisplayState Unite(int localValue, bool update)
        {
            if (localValue < 0) return ClearDisplayState.None;

            int lastValue = entry.Value;
            int newValue = Math.Min(goal, lastValue + localValue);
            if (update) entry.Value = newValue;

            if (newValue >= goal && lastValue < goal)
                return ClearDisplayState.FirstClear;

            if (localValue >= goal && !canClearOnce)
                return ClearDisplayState.Clear;

            return ClearDisplayState.None;
        }
    }

    static public class TORAchievementManager
    {
        static public DataSaver AchievementDataSaver = new("Progress");
        static private StringDataEntry myTitleEntry = new("MyTitle", AchievementDataSaver, "-");
        static private Dictionary<string, ProgressRecord> allRecords = [];

        static private Dictionary<string, ITORAchievement> allNonrecords = [];
        static private List<ITORAchievement> allAchievements = [];
        static public IEnumerable<ProgressRecord> AllRecords => allRecords.Values;
        static public IEnumerable<ITORAchievement> AllAchievements => allAchievements;
        static private List<ITORAchievement> ClearedAllOrderedArchive = [];
        static public IEnumerable<ITORAchievement> RecentlyCleared => ClearedAllOrderedArchive;
        static internal void RegisterRecord(ProgressRecord progressRecord, string id)
        {
            allRecords[id] = progressRecord;
            if (progressRecord is ITORAchievement ach) RegisterAchivement(ach);
        }
        static private void RegisterAchivement(ITORAchievement ach)
        {
            allAchievements.Add(ach);
        }
        static public bool GetAchievement(string id, [MaybeNullWhen(false)] out ITORAchievement achievement)
        {
            achievement = (allRecords.TryGetValue(id, out var rec) && rec is AbstractAchievement ach) ? ach : null;
            if (achievement == null) allNonrecords.TryGetValue(id, out achievement);
            return achievement != null;
        }
        static public bool GetRecord(string id, [MaybeNullWhen(false)] out ProgressRecord record)
        {
            return allRecords.TryGetValue(id, out record);
        }

        static public void SetOrToggleTitle(ITORAchievement achievement)
        {
            if (achievement == null || MyTitle == achievement)
                MyTitle = null;
            else
                MyTitle = achievement;
        }

        static public ITORAchievement MyTitle
        {
            get
            {
                if (GetAchievement(myTitleEntry.Value, out var achievement) && achievement.IsCleared)
                    return achievement;
                return null;
            }
            set
            {
                if (value?.IsCleared ?? false)
                    myTitleEntry.Value = value.Id;
                else
                    myTitleEntry.Value = "-";

                if (PlayerControl.LocalPlayer && !ShipStatus.Instance) RPCProcedure.ShareAchievement.Invoke((PlayerControl.LocalPlayer.PlayerId, myTitleEntry.Value));
            }
        }

        public static void RequireShare()
        {
            static IEnumerator CoShareAchievement()
            {
                yield return new WaitForSeconds(0.5f);
                RPCProcedure.RpcRequireHandShake.Invoke();
            }
            AmongUsClient.Instance.StartCoroutine(CoShareAchievement().WrapToIl2Cpp());
        }

        static public (int num, int max, int hidden)[] Aggregate(Predicate<ITORAchievement> predicate)
        {
            (int num, int max, int hidden)[] result = new (int num, int max, int hidden)[3];
            for (int i = 0; i < result.Length; i++) result[i] = (0, 0, 0);
            return AllAchievements.Where(a => predicate?.Invoke(a) ?? true).Aggregate(result,
                (ac, achievement) => {
                    if (!achievement.IsHidden)
                    {
                        ac[achievement.Trophy].max++;
                        if (achievement.IsCleared) ac[achievement.Trophy].num++;
                    }
                    else
                    {
                        ac[achievement.Trophy].hidden++;
                    }
                    return ac;
                });
        }

        static public (ITORAchievement achievement, ClearDisplayState clearState)[] UniteAll()
        {
            List<(ITORAchievement achievement, ClearDisplayState clearState)> result = new();

            foreach (var token in TORGameManager.Instance?.AllAchievementTokens)
            {
                var state = token.UniteTo();
                if (state == ClearDisplayState.None) continue;
                if (token.Achievement is AbstractAchievement ach && result.All(a => a.achievement != ach)) result.Add(new(ach, state));
            }
            foreach (var achievement in AllAchievements)
            {
                var state = achievement.CheckClear();
                if (state == ClearDisplayState.None) continue;
                result.Add(new(achievement, state));
            }

            result.OrderBy(val => val.clearState);

            ClearedAllOrderedArchive.RemoveAll(a => result.Any(r => r.achievement == a));
            ClearedAllOrderedArchive.InsertRange(0, result.Select(r => r.achievement));
            if (ClearedAllOrderedArchive.Count > 10) ClearedAllOrderedArchive.RemoveRange(10, ClearedAllOrderedArchive.Count - 10);

            return result.DistinctBy(a => a.achievement).ToArray();
        }

        static XOnlyDividedSpriteLoader trophySprite = XOnlyDividedSpriteLoader.FromResource("TheOtherRoles.Resources.Trophy.png", 220f, 3);
        static public IEnumerator CoShowAchievements(MonoBehaviour coroutineHolder, params (ITORAchievement achievement, ClearDisplayState clearState)[] achievements)
        {
            int num = 0;
            (GameObject holder, GameObject animator, GameObject body, SpriteRenderer white) CreateBillboard(ITORAchievement achievement, ClearDisplayState clearState)
            {
                var billboard = Helpers.CreateObject("Billboard", null, new Vector3(3.85f, 1.75f - (float)num * 0.6f, -100f));
                var animator = Helpers.CreateObject("Animator", billboard.transform, new Vector3(0f, 0f, 0f));
                var body = Helpers.CreateObject("Body", animator.transform, new Vector3(0f, 0f, 0f));
                var background = Helpers.CreateObject<SpriteRenderer>("Background", body.transform, new Vector3(0f, 0f, 1f));
                var white = Helpers.CreateObject<SpriteRenderer>("White", animator.transform, new Vector3(0f, 0f, -2f));
                var icon = Helpers.CreateObject<SpriteRenderer>("Icon", body.transform, new Vector3(-0.95f, 0f, 0f));

                background.color = clearState == ClearDisplayState.FirstClear ? Color.yellow : new Color(0.7f, 0.7f, 0.7f);
                billboard.AddComponent<SortingGroup>();

                new MetaContextOld.Text(new(TextAttribute.BoldAttr) { Font = VanillaAsset.BrookFont, Size = new(2f, 0.4f), FontSize = 1.16f, FontMaxSize = 1.16f, FontMinSize = 1.16f }) { MyText = achievement.GetHeaderComponent() }.Generate(body, new Vector2(0.25f, 0.13f), out _);
                new MetaContextOld.Text(new(TextAttribute.NormalAttr) { Font = VanillaAsset.BrookFont, Size = new(2f, 0.4f) }) { MyText = achievement.GetTitleComponent(null) }.Generate(body, new Vector2(0.25f, -0.06f), out _);

                foreach (var renderer in new SpriteRenderer[] { background, white })
                {
                    renderer.sprite = VanillaAsset.TextButtonSprite;
                    renderer.drawMode = SpriteDrawMode.Sliced;
                    renderer.tileMode = SpriteTileMode.Continuous;
                    renderer.size = new Vector2(2.6f, 0.55f);
                }
                num++;

                var collider = billboard.AddComponent<BoxCollider2D>();
                collider.isTrigger = true;
                collider.size = new Vector2(2.6f, 0.55f);
                var button = billboard.SetUpButton();
                var hover = button.gameObject.AddComponent<TouchHover>();
                hover.OnHoverOver.AddListener((Action)(() => TORGUIManager.Instance.SetHelpContext(button, achievement.GetOverlayContext(true, false, true, false, true))));
                hover.OnHoverOut.AddListener((Action)(() => TORGUIManager.Instance.HideHelpContextIf(button)));
#if WINDOWS
                button.OnClick.AddListener((Action)(() => {
                    SetOrToggleTitle(achievement);
                    hover.OnHoverOut.Invoke();
                    hover.OnHoverOver.Invoke();
                }));
#endif

                white.material.shader = Helpers.achievementMaterialShader;
                icon.sprite = trophySprite.GetSprite(achievement.Trophy);

                return (billboard, animator, body, white);
            }

            IEnumerator CoShowFirstClear((GameObject holder, GameObject animator, GameObject body, SpriteRenderer white) billboard)
            {
                IEnumerator Shake(Transform target, float duration, float halfWidth)
                {
                    Vector3 origin = target.localPosition;
                    for (float timer = 0f; timer < duration; timer += Time.deltaTime)
                    {
                        float num = timer / duration;
                        Vector3 vector = UnityEngine.Random.insideUnitCircle * halfWidth;
                        target.localPosition = origin + vector;
                        yield return null;
                    }
                    target.localPosition = origin;
                    yield break;
                }

                billboard.body.SetActive(false);
                billboard.holder.transform.localScale = Vector3.one * 1.1f;


                coroutineHolder.StartCoroutine(Helpers.Sequence(
                    Shake(billboard.animator.transform, 0.1f, 0.01f),
                    Shake(billboard.animator.transform, 0.2f, 0.02f),
                    Shake(billboard.animator.transform, 0.3f, 0.03f),
                    Shake(billboard.animator.transform, 0.3f, 0.04f)
                    ));

                float t;

                t = 0f;
                while (t < 0.9f)
                {
                    billboard.holder.transform.localScale = Vector3.one * (1.1f + (t / 0.9f * 0.2f));
                    billboard.white.color = new Color(1f, 1f, 1f, t / 0.9f);
                    t += Time.deltaTime;
                    yield return null;
                }

                billboard.body.SetActive(true);

                float p = 1f;
                while (p > 0.0001f)
                {
                    billboard.holder.transform.localScale = Vector3.one * (1f + (p * 0.2f));
                    billboard.white.color = new Color(1f, 1f, 1f, p);
                    p -= p * 5f * Time.deltaTime;
                    yield return null;
                }

                billboard.white.gameObject.SetActive(false);
                billboard.holder.transform.localScale = Vector3.one;
            }

            IEnumerator CoShowClear((GameObject holder, GameObject animator, GameObject body, SpriteRenderer white) billboard)
            {
                billboard.white.gameObject.SetActive(false);
                float p = 3f;
                while (p > 0.0001f)
                {
                    billboard.animator.transform.localPosition = new Vector3(p, 0f, 0f);
                    p -= p * 8f * Time.deltaTime;
                    yield return null;
                }
                billboard.animator.transform.localPosition = Vector3.zero;
            }


            yield return new WaitForSeconds(1.5f);

            foreach (var ach in achievements)
            {
                var billboard = CreateBillboard(ach.achievement, ach.clearState);
                if (ach.clearState == ClearDisplayState.FirstClear)
                {
                    coroutineHolder.StartCoroutine(CoShowFirstClear(billboard).WrapToIl2Cpp());
                    yield return new WaitForSeconds(1.05f);
                }
                else
                {
                    coroutineHolder.StartCoroutine(CoShowClear(billboard).WrapToIl2Cpp());
                    yield return new WaitForSeconds(0.45f);
                }
                yield return new WaitForSeconds(1.05f);
                yield return null;
            }

            yield break;
        }

        static public void LoadAchievements()
        {
            using var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("TheOtherRoles.Resources.Achievements.dat")!);

            List<ProgressRecord> recordsList = new();
            List<AchievementType> types = new();

            while (true)
            {
                types.Clear();
                var line = reader.ReadLine();
                if (line == null) break;
                if (line.StartsWith("#")) continue;

                var args = line.Split(',');

                if (args.Length < 2) continue;

                bool clearOnce = false;
                bool noHint = false;
                bool secret = false;
                bool seasonal = false;
                bool isNotChallenge = false;
                bool isRecord = false;
                IEnumerable<ProgressRecord> records = recordsList;

                IEnumerable<RoleInfo> relatedRoles = [];

                int rarity = int.Parse(args[1]);
                int goal = 1;
                for (int i = 2; i < args.Length - 1; i++)
                {
                    var arg = args[i];

                    switch (arg)
                    {
                        case "once":
                            clearOnce = true;
                            break;
                        case "noHint":
                            noHint = true;
                            break;
                        case "secret":
                            secret = true;
                            break;
                        case "seasonal":
                            seasonal = true;
                            break;
                        case "nonChallenge":
                            isNotChallenge = true;
                            break;
                        case string a when a.StartsWith("goal-"):
                            goal = int.Parse(a.Substring(5));
                            break;
                        case "isRecord":
                            isRecord = true;
                            break;
                        case string a when a.StartsWith("record-"):
                            if (allRecords.TryGetValue(a.Substring(7), out var r))
                                recordsList.Add(r);
                            else
                                TheOtherRolesPlugin.Logger.LogError("The record \"" + a.Substring(7) + "\" was not found.");
                            break;
                    }
                }

                if (seasonal) types.Add(AchievementType.Seasonal);
                if (secret) types.Add(AchievementType.Secret);

                var nameSplitted = args[0].Split('.');
                if (nameSplitted.Length > 1)
                {
                    if (nameSplitted[0] == "combination" && nameSplitted.Length > 2 && int.TryParse(nameSplitted[1], out var num) && nameSplitted.Length >= 2 + num)
                    {
                        relatedRoles = Helpers.Sequential(num).Select(i =>
                        {
                            var roleName = nameSplitted[2 + i].Replace('-', '.');
                            return RoleInfo.allRoleInfos.FirstOrDefault(a => a.nameKey == roleName);
                        }).Where(r => r != null).ToArray()!;
                        if (rarity == 2 && !isNotChallenge) types.Add(AchievementType.Challenge);
                    }
                    else
                    {
                        nameSplitted[0] = nameSplitted[0].Replace('-', '.');
                        var cand = RoleInfo.allRoleInfos.FirstOrDefault(a => a.nameKey == nameSplitted[0]);
                        if (cand != null)
                        {
                            relatedRoles = [cand];
                            if (rarity == 2 && !isNotChallenge)
                            {
                                types.Add(AchievementType.Challenge);
                            }
                        }
                    }
                }

                if (isRecord)
                    new DisplayProgressRecord(args[0], goal, args[0] + "Record");
                else if (!records.IsEmpty())
                    new CompleteAchievement(records.ToArray(), secret, noHint, args[0], relatedRoles, types.ToArray(), rarity);
                else if (goal > 1)
                    new SumUpAchievement(secret, noHint, args[0], goal, relatedRoles, types.ToArray(), rarity);
                else
                    new StandardAchievement(clearOnce, secret, noHint, args[0], goal, relatedRoles, types.ToArray(), rarity);

                if (recordsList.Count > 0) recordsList.Clear();
            }

            foreach (var achievement in AllAchievements) achievement.CheckClear();
        }
    }

    public interface ITORAchievement
    {
        static public TextComponent HiddenComponent = new RawTextComponent("???");
        static public TextComponent HiddenDescriptiveComponent = new ColorTextComponent(new Color(0.4f, 0.4f, 0.4f), new TranslateTextComponent("achievementTitleHidden"));
        static public TextComponent HiddenDetailComponent = new ColorTextComponent(new Color(0.8f, 0.8f, 0.8f), new TranslateTextComponent("achievementTitleHiddenDetail"));
        static public TextAttributes DetailTitleAttribute = GUI.API.GetAttribute(AttributeAsset.OverlayTitle);
        static private TextAttributes DetailContentAttribute = GUI.API.GetAttribute(AttributeAsset.OverlayContent);

        string Id { get; }
        public string TranslateKeyInfo
        {
            get
            {
                return Id.Replace(".", "");
            }
        }
        string TranslationKey => TranslateKeyInfo + "AchievementTitle";
        string GoalTranslationKey => TranslateKeyInfo + "AchievementGoal";
        string CondTranslationKey => TranslateKeyInfo + "AchievementCond";
        string FlavorTranslationKey => TranslateKeyInfo + "AchievementFlavor";

        int Trophy { get; }
        bool IsHidden { get; }
        bool IsCleared { get; }
        bool NoHint { get; }
        IEnumerable<RoleInfo> RelatedRole { get; }
        IEnumerable<AchievementType> AchievementType();
        static public IDividedSpriteLoader TrophySprite = XOnlyDividedSpriteLoader.FromResource("TheOtherRoles.Resources.Trophy.png", 100f, 3);

        public IEnumerable<string> GetKeywords()
        {
            foreach (var r in RelatedRole) yield return r.name;
            yield return ModTranslation.getString(GoalTranslationKey);
            yield return ModTranslation.getString(CondTranslationKey, tryFind: true) ?? "";
            if (IsCleared)
            {
                yield return ModTranslation.getString(TranslationKey);
                yield return ModTranslation.getString(FlavorTranslationKey, tryFind: true) ?? "";
            }
        }

        public TextComponent GetHeaderComponent()
        {
            List<TextComponent> list = new();

            foreach (var r in RelatedRole)
            {
                if (list.Count != 0) list.Add(new RawTextComponent(" & "));
                list.Add(GUI.Instance.TextComponent(r.orgColor, r.nameKey));
            }

            foreach (var type in AchievementType())
            {
                if (list.Count != 0) list.Add(new RawTextComponent(" "));
                list.Add(new TranslateTextComponent(type.TranslationKey));
            }

            if (list.Count > 0)
                return new CombinedTextComponent(list.ToArray());
            else
                return null;
        }

        public GUIContext GetOverlayContext(bool hiddenNotClearedAchievement = true, bool showCleared = false, bool showTitleInfo = false, bool showTorophy = false, bool showFlavor = false)
        {
            var detailTitleAttr = new TextAttributes(GUI.API.GetAttribute(AttributeParams.StandardBaredBoldLeft)) { FontSize = new(1.8f) };
            var detailDetailAttr = new TextAttributes(GUI.API.GetAttribute(AttributeParams.StandardBaredLeft)) { FontSize = new(1.5f), Size = new(5f, 6f) };

            List<GUIContext> list = new()
            {
                new TORGUIText(GUIAlignment.Left, DetailContentAttribute, GetHeaderComponent())
            };
            List<GUIContext> titleList = new();
            if (showTorophy)
            {
                titleList.Add(new TORGUIMargin(GUIAlignment.Left, new(-0.04f, 0.2f)));
                titleList.Add(new TORGUIImage(GUIAlignment.Left, new WrapSpriteLoader(() => TrophySprite.GetSprite(Trophy)), new(0.3f, 0.3f)));
                titleList.Add(new TORGUIMargin(GUIAlignment.Left, new(0.05f, 0.2f)));
            }
            titleList.Add(new TORGUIText(GUIAlignment.Left, DetailTitleAttribute, GetTitleComponent(hiddenNotClearedAchievement ? HiddenDescriptiveComponent : null)));

            if (showCleared && IsCleared)
            {
                titleList.Add(new TORGUIMargin(GUIAlignment.Left, new(0.2f, 0.2f)));
                titleList.Add(new TORGUIText(GUIAlignment.Left, DetailContentAttribute, GUI.API.TextComponent(new(1f, 1f, 0f), "achievementCleared")));
            }

            list.Add(new HorizontalContextsHolder(GUIAlignment.Left, titleList));
            list.Add(new TORGUIText(GUIAlignment.Left, DetailContentAttribute, GetDetailComponent()));

            if (showFlavor)
            {
                var flavor = GetFlavorComponent();
                if (flavor != null)
                {
                    list.Add(new TORGUIMargin(GUIAlignment.Left, new(0f, 0.12f)));
                    list.Add(new TORGUIText(GUIAlignment.Left, DetailContentAttribute, flavor) { PostBuilder = text => text.outlineColor = Color.clear });
                }
            }

            if (showTitleInfo && IsCleared)
            {
                list.Add(new TORGUIMargin(GUIAlignment.Left, new(0f, 0.2f)));
#if WINDOWS
                list.Add(new TORGUIText(GUIAlignment.Left, DetailContentAttribute, new LazyTextComponent(() =>
                (TORAchievementManager.MyTitle == this) ?
                ("<b>" + ModTranslation.getString("achievementEquipped").Color(Color.green) + "</b><br>" + ModTranslation.getString("achievementUnequip")) :
                ModTranslation.getString("achievementEquip"))));
#else
                list.Add(GUI.API.Button(GUIAlignment.Left, GUI.API.GetAttribute(AttributeAsset.SmallWideButtonMasked), 
                                new LazyTextComponent(() => ModTranslation.getString(TORAchievementManager.MyTitle == this ? "achievementUnequip" : "achievementEquip")),
                                (Action)(() =>
                                {
                                    TORAchievementManager.SetOrToggleTitle(this);
                                    TORGUIManager.Instance.HideHelpContext();
                                })
                                ));
#endif
            }

            return new VerticalContextsHolder(GUIAlignment.Left, list) { BackImage = RelatedRole.Any() ? TheOtherRoles.RoleHelpers.GetIllustration(RelatedRole.FirstOrDefault().roleId) : null, GrayoutedBackImage = !(IsCleared || !hiddenNotClearedAchievement) };
        }

        public TextComponent GetTitleComponent(TextComponent hiddenComponent)
        {
            if (hiddenComponent != null && !IsCleared)
                return hiddenComponent;
            return new TranslateTextComponent(TranslationKey);
        }

        public TextComponent GetDetailComponent()
        {
            List<TextComponent> list = new();
            if (!NoHint || IsCleared)
                list.Add(new TranslateTextComponent(GoalTranslationKey));
            else
                list.Add(HiddenDetailComponent);
            list.Add(new LazyTextComponent(() =>
            {
                StringBuilder builder = new();
                var cond = ModTranslation.getString(CondTranslationKey, tryFind: true);
                if (cond != null && cond.Length > 0)
                {
                    builder.Append("<size=75%><br><br>");
                    builder.Append(ModTranslation.getString("achievementCond"));
                    foreach (var c in cond.Split('+'))
                    {
                        builder.Append("<br>   ");
                        builder.Append(c);
                    }
                    builder.Append("</size>");
                }
                return builder.ToString();
            }));

            return new CombinedTextComponent(list.ToArray());
        }

        TextComponent GetFlavorComponent()
        {
            var text = ModTranslation.getString(FlavorTranslationKey, tryFind: true);
            if (text == null) return null;
            return new RawTextComponent($"<color=#e7e5ca><size=78%><i>{text}</i></size></color>");
        }

        GUIContext GetDetailContext() => null;
        ClearDisplayState CheckClear() { return ClearDisplayState.None; }
    }

    public class DisplayProgressRecord : ProgressRecord
    {
        string translationKey;
        public DisplayProgressRecord(string key, int goal, string translationKey) : base(key, goal, true)
        {
            this.translationKey = translationKey;
        }

        public override string TranslationKey => translationKey.Replace(".", "");
    }

    public class CompleteAchievement : SumUpAchievement, ITORAchievement
    {
        ProgressRecord[] records;
        public CompleteAchievement(ProgressRecord[] allRecords, bool isSecret, bool noHint, string key, IEnumerable<RoleInfo> role, IEnumerable<AchievementType> type, int trophy)
            : base(isSecret, noHint, key, allRecords.Length, role, type, trophy)
        {
            this.records = allRecords;
        }

        ClearDisplayState ITORAchievement.CheckClear()
        {
            bool wasCleared = IsCleared;
            UpdateProgress(records.Count(r => r.IsCleared));

            if (!wasCleared) return IsCleared ? ClearDisplayState.FirstClear : ClearDisplayState.None;
            return ClearDisplayState.None;
        }

        static private TextAttributes TextAttr = new(GUI.Instance.GetAttribute(AttributeParams.StandardBaredLeft)) { FontSize = new(1.25f) };
        protected override void OnContextGenerated(GameObject obj)
        {
            var collider = Helpers.CreateObject<BoxCollider2D>("Overlay", obj.transform, Vector3.zero);
            collider.size = new(2f, 0.17f);
            collider.isTrigger = true;

            var button = collider.gameObject.SetUpButton();
            var hover = button.gameObject.AddComponent<TouchHover>();
            hover.OnHoverOver.AddListener((Action)(() =>
            {
                string text = string.Join("\n", records.Select(r => "- " + ModTranslation.getString(r.TranslationKey).Color(r.IsCleared ? Color.green : Color.white)));
                TORGUIManager.Instance.SetHelpContext(button, new TORGUIText(GUIAlignment.Left, TextAttr, new RawTextComponent(text)));
            }));
            hover.OnHoverOut.AddListener((Action)(() => TORGUIManager.Instance.HideHelpContextIf(button)));
        }
    }


    public class AbstractAchievement : ProgressRecord, ITORAchievement
    {
        public static AchievementToken<(bool isCleared, bool triggered)> GenerateSimpleTriggerToken(string achievement) => new(achievement, (false, false), (val, _) => val.isCleared);
        bool isSecret;
        bool noHint;

        public IEnumerable<RoleInfo> role;
        public IEnumerable<AchievementType> type;
        public int Trophy { get; private init; }
        public bool NoHint => noHint;
        public IEnumerable<RoleInfo> RelatedRole => role;
        public IEnumerable<AchievementType> AchievementType() => type;
        public bool IsHidden
        {
            get
            {
                return isSecret && !IsCleared;
            }
        }

        public AbstractAchievement(bool canClearOnce, bool isSecret, bool noHint, string key, int goal, IEnumerable<RoleInfo> role, IEnumerable<AchievementType> type, int trophy) : base(key, goal, canClearOnce)
        {
            this.isSecret = isSecret;
            this.noHint = noHint;
            this.type = type;
            this.role = role;
            this.Trophy = trophy;
        }
    }

    public class StandardAchievement : AbstractAchievement
    {
        public StandardAchievement(bool canClearOnce, bool isSecret, bool noHint, string key, int goal, IEnumerable<RoleInfo> role, IEnumerable<AchievementType> type, int trophy)
            : base(canClearOnce, isSecret, noHint, key, goal, role, type, trophy)
        {
        }
    }

    public class SumUpAchievement : AbstractAchievement, ITORAchievement
    {
        public SumUpAchievement(bool isSecret, bool noHint, string key, int goal, IEnumerable<RoleInfo> role, IEnumerable<AchievementType> type, int trophy)
            : base(true, isSecret, noHint, key, goal, role, type, trophy)
        {
        }

        SpriteLoader guageSprite = SpriteLoader.FromResource("TheOtherRoles.Resources.ProgressGuage.png", 100f);

        static private TextAttributes OblongAttribute = new(GUI.Instance.GetAttribute(AttributeParams.Oblong)) { FontSize = new(1.6f), Size = new(0.6f, 0.2f), Color = new(163, 204, 220) };
        protected virtual void OnContextGenerated(GameObject obj) { }
        GUIContext ITORAchievement.GetDetailContext()
        {
            //クリア済みなら何も出さない
            if (IsCleared) return null;

            return new TORGameObjectGUIWrapper(GUIAlignment.Left, () =>
            {
                var obj = Helpers.CreateObject("Progress", null, Vector3.zero, LayerMask.NameToLayer("UI"));
                var backGround = Helpers.CreateObject<SpriteRenderer>("Background", obj.transform, new Vector3(0f, 0f, 0f));
                var colored = Helpers.CreateObject<SpriteRenderer>("Colored", obj.transform, new Vector3(0f, 0f, -0.1f));

                backGround.sprite = guageSprite.GetSprite();
                backGround.color = new(0.21f, 0.21f, 0.21f);
                backGround.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                backGround.sortingOrder = 1;

                colored.sprite = guageSprite.GetSprite();
                colored.material.shader = Helpers.achievementProgressShader;
                colored.sharedMaterial.SetFloat("_Guage", Mathf.Min(1f, (float)Progress / (float)Goal));
                colored.sharedMaterial.color = new(56f / 255f, 110f / 255f, 191f / 255f);
                colored.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                colored.sortingOrder = 2;

                var text = new TORGUIText(GUIAlignment.Center, OblongAttribute, new RawTextComponent(Progress + "  /  " + Goal)).Instantiate(new(1f, 0.2f), out _);
                text!.transform.SetParent(obj.transform);

                OnContextGenerated(obj);

                return (obj, new(2f, 0.17f));
            });
        }
    }
}
