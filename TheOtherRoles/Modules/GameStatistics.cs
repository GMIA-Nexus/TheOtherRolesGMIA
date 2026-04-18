using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Hazel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TheOtherRoles.MetaContext;
using TheOtherRoles.Utilities;
using Unity.Services.Core.Telemetry.Internal;
using UnityEngine;

namespace TheOtherRoles.Modules
{
    public enum GameStatisticsGatherTag
    {
        Spawn
    }

    public class TranslatableTag : CommunicableTextTag
    {
        static public List<TranslatableTag> AllTag = new();

        public string TranslateKey { get; private set; }
        string CommunicableTextTag.TranslationKey => TranslateKey;

        public string Text => ModTranslation.getString(TranslateKey);
        public int Id { get; private set; }


        public static void Load()
        {
            AllTag.Sort((tag1, tag2) => tag1.TranslateKey.CompareTo(tag2.TranslateKey));
            for (int i = 0; i < AllTag.Count; i++) AllTag[i].Id = i;
        }

        public TranslatableTag(string translateKey)
        {
            TranslateKey = translateKey;
            AllTag.Add(this);
        }

        static public TranslatableTag ValueOf(int id)
        {
            if (id < AllTag.Count && id >= 0)
                return AllTag[id];
            return null;
        }
    }

    public static class EventDetail
    {
        static public TranslatableTag Kill;
        static public TranslatableTag Exiled;
        static public TranslatableTag GameStart;
        static public TranslatableTag GameEnd;
        static public TranslatableTag MeetingEnd;
        static public TranslatableTag Report;
        static public TranslatableTag BaitReport;
        static public TranslatableTag EmergencyButton;
        static public TranslatableTag Revive;
        static public TranslatableTag Eat;
        static public TranslatableTag Clean;
        static public TranslatableTag Pseudocide;
        static public TranslatableTag Disconnect;
        static public TranslatableTag MisGuess;
        static public TranslatableTag Guessed;
        static public TranslatableTag Blown;
        static public TranslatableTag Swallowed;

        public static void Load()
        {
            Kill = new("eventKilled");
            Exiled = new("eventExiled");
            GameStart = new("eventGameStart");
            GameEnd = new("eventGameEnd");
            MeetingEnd = new("eventMeetingEnd");
            EmergencyButton = new("eventEmergencyButton");
            Report = new("eventReport");
            BaitReport = new("eventBaitReport");
            Revive = new("eventRevive");
            Eat = new("eventEat");
            Clean = new("eventClean");
            Pseudocide = new("eventPseudocide");
            Disconnect = new("eventDisconnect");
            MisGuess = new("eventMisGuess");
            Guessed = new("eventGuessed");
            Blown = new("eventBlown");
            Swallowed = new("eventSwallowed");
        }
    }

    public class GameStatistics
    {

        public class StatisticsEvent
        {
            /// <summary>
            /// 統計的イベントに大きく関わるプレイヤー
            /// </summary>
            public PlayerControl Caused { get; internal init; }
            /// <summary>
            /// 統計的イベントの影響を受けるプレイヤー
            /// </summary>
            public ReadOnlyCollection<PlayerControl> Related { get; internal init; }
            public CommunicableTextTag Detail { get; internal init; }

            internal StatisticsEvent(PlayerControl caused, PlayerControl[] related, CommunicableTextTag eventDetail)
            {
                this.Caused = caused;
                this.Related = new(related);
                this.Detail = eventDetail;
            }
        }

        public class EventHandlerAttribute : Attribute
        {
        }

        public class EventManager
        {
            public record EventHandler(ILifespan lifespan, Action<object> handler);

            public static Dictionary<Type, List<EventHandler>> allHandlers = new();

            private static void RegisterEvent(ILifespan lifespan, Type eventType, Action<object> handler)
            {
                if (!allHandlers.TryGetValue(eventType, out var handlers))
                {
                    handlers = new List<EventHandler>();
                    allHandlers[eventType] = handlers;
                }

                handlers.Add(new(lifespan, handler));
            }
            public static void RegisterEvent(ILifespan lifespan, object handler)
            {
                if (handler == null) return;
                foreach (var method in handler.GetType().GetMethods().Where(method => !method.IsStatic && method.IsDefined(typeof(EventHandlerAttribute), true) && method.GetParameters().Length == 1))
                {
                    RegisterEvent(lifespan, method.GetParameters()[0].ParameterType, (obj) => method.Invoke(handler, [obj]));
                }
            }

            public static Event HandleEvent<Event>(Event targetEvent) where Event : class
            {
                HandleEvent(typeof(Event), targetEvent);
                return targetEvent;
            }

            public static void HandleEvent(Type eventType, object targetEvent)
            {
                if (eventType == null) return;

                HandleEvent(eventType.BaseType, targetEvent);

                if (allHandlers.TryGetValue(eventType, out var handlers))
                {
                    handlers.RemoveAll(handler =>
                    {
                        if (handler.lifespan.IsDeadObject) return true;

                        if (handler.lifespan.IsAliveObject) handler.handler.Invoke(targetEvent);

                        return false;
                    });
                }
            }
        }

        public class EventVariation
        {
            static Dictionary<int, EventVariation> AllEvents = new();
            static private DividedSpriteLoader iconSprite = DividedSpriteLoader.FromResource("TheOtherRoles.Resources.GameStatisticsIcon.png", 100f, 8, 1);
            static public EventVariation Kill = new(0, iconSprite.AsLoader(0), iconSprite.AsLoader(0), true, true);
            static public EventVariation Exile = new(1, iconSprite.AsLoader(2), iconSprite.AsLoader(2), false, false);
            static public EventVariation GameStart = new(2, iconSprite.AsLoader(1), iconSprite.AsLoader(1), true, false);
            static public EventVariation GameEnd = new(3, iconSprite.AsLoader(1), iconSprite.AsLoader(1), true, false);
            static public EventVariation MeetingEnd = new(4, iconSprite.AsLoader(1), iconSprite.AsLoader(1), true, false);
            static public EventVariation Report = new(5, iconSprite.AsLoader(4), iconSprite.AsLoader(4), true, false);
            static public EventVariation EmergencyButton = new(6, iconSprite.AsLoader(3), iconSprite.AsLoader(3), true, false);
            static public EventVariation Disconnect = new(7, iconSprite.AsLoader(5), iconSprite.AsLoader(5), false, false);
            static public EventVariation Revive = new(8, iconSprite.AsLoader(6), iconSprite.AsLoader(6), true, false);
            static public EventVariation CleanBody = new(9, iconSprite.AsLoader(7), iconSprite.AsLoader(7), true, false);

            public int Id { get; private init; }
            public Image EventIcon { get; private init; }
            public Image InteractionIcon { get; private init; }
            public bool ShowPlayerPosition { get; private init; }
            public bool CanCombine { get; private init; }
            public EventVariation(int id, Image eventIcon, Image interactionIcon, bool showPlayerPosition, bool canCombine)
            {
                Id = id;
                EventIcon = eventIcon;
                InteractionIcon = interactionIcon;
                CanCombine = canCombine;

                AllEvents.Add(id, this);
                ShowPlayerPosition = showPlayerPosition;
            }
            static public EventVariation ValueOf(int id) => AllEvents[id];
        }

        public class Event
        {
            public EventVariation Variation { get; private init; }
            public float Time { get; private init; }
            public byte? SourceId { get; private init; }
            public int TargetIdMask { get; private set; }
            public Tuple<byte, Vector2>[] Position { get; private init; }
            public CommunicableTextTag RelatedTag { get; set; } = null;

            public Event(EventVariation variation, byte? sourceId, int targetIdMask, GameStatisticsGatherTag? positionTag = null)
                : this(variation, TORGameManager.Instance.CurrentTime, sourceId, targetIdMask, positionTag) { }

            public Event(EventVariation variation, float time, byte? sourceId, int targetIdMask, GameStatisticsGatherTag? positionTag)
            {
                Variation = variation;
                Time = time;
                SourceId = sourceId;
                TargetIdMask = targetIdMask;

                if (variation.ShowPlayerPosition)
                {
                    List<Tuple<byte, Vector2>> list = [];
                    foreach (var p in PlayerControl.AllPlayerControls.GetFastEnumerator())
                    {
                        if (p.Data.IsDead && p.PlayerId != sourceId && ((TargetIdMask & (1 << p.PlayerId)) == 0)) continue;

                        if (positionTag != null)
                            list.Add(new Tuple<byte, Vector2>(p.PlayerId, TORGameManager.Instance.GameStatistics.Gathering[positionTag.Value][p.PlayerId]));
                        else
                            list.Add(new Tuple<byte, Vector2>(p.PlayerId, p.transform.position));
                    }
                    Position = list.ToArray();
                }
                else
                {
                    Position = [];
                }
            }

            public bool IsSimilar(Event target)
            {
                if (!Variation.CanCombine) return false;
                return Variation == target.Variation && SourceId == target.SourceId && RelatedTag == target.RelatedTag;
            }

            public void Combine(Event target)
            {
                TargetIdMask |= target.TargetIdMask;
            }
        }

        private List<Event> allEvents { get; set; } = new List<Event>();
        public IEnumerable<Event> AllEvents => allEvents;
        public Event[] Sealed { get => allEvents.ToArray(); }

        public Dictionary<GameStatisticsGatherTag, Dictionary<byte, Vector2>> Gathering { get; set; } = new();

        public void RecordEvent(Event statisticsEvent)
        {
            EventManager.HandleEvent(new StatisticsEvent(
                Helpers.playerById(statisticsEvent.SourceId ?? 255),
                PlayerControl.AllPlayerControls.GetFastEnumerator().Where(p => ((1 << p.PlayerId) & statisticsEvent.TargetIdMask) != 0).ToArray(),
                statisticsEvent.RelatedTag));

            int index = allEvents.Count;

            if (statisticsEvent.Variation.CanCombine)
            {
                //末尾から検索
                for (int i = allEvents.Count - 1; i >= 0; i--)
                {
                    if (allEvents[i].Time > statisticsEvent.Time) index = i;

                    //ある程度以上離れた時間のイベントまで来たら検索をやめる
                    if (statisticsEvent.Time - allEvents[i].Time > 5f) break;

                    if (allEvents[i].IsSimilar(statisticsEvent))
                    {
                        allEvents[i].Combine(statisticsEvent);
                        return;
                    }
                }
            }
            allEvents.Insert(index, statisticsEvent);
        }

        public void RpcRecordEvent(EventVariation variation, TranslatableTag relatedTag, PlayerControl source, params PlayerControl[] targets)
        {
            int mask = 0;
            foreach (var p in targets) mask |= p.PlayerId;
            RpcRecordEvent(variation, relatedTag, source, mask);
        }

        public void RpcRecordEvent(EventVariation variation, TranslatableTag relatedTag, PlayerControl source, int targetMask)
        {
            RPCProcedure.RecordStatistics.Invoke((variation.Id, relatedTag.Id, source?.PlayerId ?? byte.MaxValue, targetMask, 0f));
        }

        public void RpcRecordEvent(EventVariation variation, TranslatableTag relatedTag, float timeLag, PlayerControl source, int targetMask)
        {
            RPCProcedure.RecordStatistics.Invoke((variation.Id, relatedTag.Id, source?.PlayerId ?? byte.MaxValue, targetMask, timeLag));
        }
    }

    public class CriticalPoint : MonoBehaviour
    {
        static CriticalPoint()
        {
            ClassInjector.RegisterTypeInIl2Cpp<CriticalPoint>();
        }
        static private SpriteLoader momentSprite = SpriteLoader.FromResource("TheOtherRoles.Resources.GameStatisticsMoment.png", 100f);
        static private SpriteLoader momentRingSprite = SpriteLoader.FromResource("TheOtherRoles.Resources.GameStatisticsMomentRing.png", 100f);

        public int IndexMin { get; private set; }
        public int IndexMax { get; private set; }
        GameObject ring = null!;
        public GameStatisticsViewer MyViewer = null!;

        public void SetIndex(int min, int max)
        {
            IndexMin = min; IndexMax = max;
        }

        public bool Contains(int index) => IndexMin <= index && index <= IndexMax;

        public void Start()
        {
            var renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = momentSprite.GetSprite();
            renderer.color = GameStatisticsViewer.MainColor;
            renderer.transform.localScale = new Vector3(0.65f, 0.65f, 1f);

            var ringRenderer = Helpers.CreateObject<SpriteRenderer>("Ring", transform, Vector3.zero);
            ringRenderer.sprite = momentRingSprite.GetSprite();
            ringRenderer.color = GameStatisticsViewer.MainColor;
            ringRenderer.gameObject.SetActive(false);
            ring = ringRenderer.gameObject;


            var button = renderer.gameObject.SetUpButton(true);
            button.OnMouseOver.AddListener((Action)(() =>
            {
                renderer.transform.localScale = new Vector3(1f, 1f, 1f);
                MyViewer.OnMouseOver(IndexMin);
            }));
            button.OnMouseOut.AddListener((Action)(() =>
            {
                renderer.transform.localScale = new Vector3(0.65f, 0.65f, 1f);
                MyViewer.OnMouseOut(IndexMin);
            }));
            button.OnClick.AddListener((Action)(() =>
            {
                MyViewer.OnSelect(ring.active ? -1 : IndexMin);
            }));

            var collider = renderer.gameObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.09f;
        }

        public void OnSomeIndexSelected(int selected)
        {
            ring.gameObject.SetActive(Contains(selected));
        }
    }

    public class GameStatisticsViewer : MonoBehaviour
    {
        static GameStatisticsViewer()
        {
            ClassInjector.RegisterTypeInIl2Cpp<GameStatisticsViewer>();
        }

        LineRenderer timelineBack = null!, timelineFront = null!;
        GameObject minimap = null!;
        GameObject baseOnMinimap = null!, detailHolder = null!;
        AlphaPulse mapColor = null!;
        GameStatistics.Event[] allStatistics = null!;
        GameStatistics.Event eventPiled, eventSelected, currentShown;
        GameObject CriticalPoints = null!;

        public float? SelectedTime => eventSelected?.Time;

        public PoolablePlayer PlayerPrefab = null!;
        public TMPro.TextMeshPro GameEndText = null!;
        static public GameStatisticsViewer Instance { get; private set; } = null!;

        public void Start()
        {
            allStatistics = TORGameManager.Instance!.GameStatistics.Sealed;
            if (allStatistics.Length == 0) return;

            timelineBack = Helpers.SetUpLineRenderer("TimelineBack", transform, new Vector3(0, 0, -10f), LayerMask.NameToLayer("UI"), 0.014f);
            timelineFront = Helpers.SetUpLineRenderer("TimelineFront", transform, new Vector3(0, 0, -15f), LayerMask.NameToLayer("UI"), 0.014f);

            minimap = Helpers.CreateObject("Minimap", transform, new Vector3(0, -1.62f, 0));
            var scaledMinimap = Helpers.CreateObject("Scaled", minimap.transform, new Vector3(0, 0, 0));
            scaledMinimap.transform.localScale = new Vector3(0.45f, 0.45f, 1);
            var minimapRenderer = GameObject.Instantiate(TORGameManager.Instance?.RuntimeAsset.MinimapObjPrefab, scaledMinimap.transform);
            minimapRenderer.gameObject.name = "MapGraphic";
            minimapRenderer.transform.localScale = new Vector3(1f, 1f, 1f);
            minimapRenderer.transform.localPosition = Vector3.zero;
            mapColor = minimapRenderer.GetComponent<AlphaPulse>();
            mapColor.SetColor(MainColor);
            Helpers.CreateSharpBackground(new Vector2(4.6f, 2.8f), MainColor, minimap.transform);
            baseOnMinimap = Helpers.CreateObject("Scaler", scaledMinimap.transform, TORGameManager.Instance.RuntimeAsset.MinimapPrefab.HerePoint.transform.parent.localPosition);
            detailHolder = Helpers.CreateObject("Detail", transform, new Vector3(0, -3.5f, 0));
            Hide();

            CriticalPoints = Helpers.CreateObject("CriticalMoments", transform, Vector3.zero);

            StartCoroutine(CoShowTimeLine().WrapToIl2Cpp());
        }

        public void Update()
        {
            GameStatistics.Event willShown = eventPiled ?? eventSelected;
            if (willShown != currentShown)
            {
                if (willShown == null)
                    Hide();
                else
                    Show(willShown);
                currentShown = willShown;
            }
        }

        private const float LineHalfWidth = 2.5f;
        public static readonly Color MainColor = new(0f, 242f / 255f, 156f / 255f);
        private const float BackColorRate = 0.4f;

        private IEnumerator CoShowCriticalMoment(float p, int indexMin, int indexMax)
        {
            var point = Helpers.CreateObject<CriticalPoint>("Moment", CriticalPoints.transform, new Vector3((p * 2f - 1f) * LineHalfWidth, 0f, -20f - indexMin));
            point.MyViewer = this;
            point.SetIndex(indexMin, indexMax);
            yield return null;
        }

        public void OnSelect(int index)
        {
            eventSelected = index >= 0 ? allStatistics[index] : null;
            CriticalPoints.ForEachChild((Il2CppSystem.Action<GameObject>)((obj) => obj.GetComponent<CriticalPoint>().OnSomeIndexSelected(index)));
        }

        private void ShowCriticalMoment(float p, ref int index)
        {
            var sum = allStatistics[allStatistics.Length - 1].Time - allStatistics[0].Time;
            int indexMin = index;
            while (index + 1 < allStatistics.Length && allStatistics[index + 1].Time - allStatistics[indexMin].Time < sum * 0.01f)
            {
                index++;
            }
            int indexMax = index;

            //ゲーム終了と結合する際は後ろに揃える
            if (indexMax == allStatistics.Length - 1) p = 1f;

            StartCoroutine(CoShowCriticalMoment(p, indexMin, indexMax).WrapToIl2Cpp());
            index = indexMax + 1;
        }

        private IEnumerator CoShowTimeLine()
        {
            StartCoroutine(CoShowTimeBackLine().WrapToIl2Cpp());
            yield return new WaitForSeconds(1.4f);

            timelineFront.SetPosition(0, new Vector3(-LineHalfWidth, 0));
            timelineFront.SetPosition(1, new Vector3(-LineHalfWidth, 0));
            timelineFront.SetColors(MainColor, MainColor);

            float p = 0f;

            float minTime = allStatistics[0].Time;
            float maxTime = allStatistics[allStatistics.Length - 1].Time;
            int index = 0;

            ShowCriticalMoment(0, ref index);

            float ToP(float p) => (p - minTime) / (maxTime - minTime);

            while (p < 1f)
            {
                while (index < (allStatistics.Length - 1) && ToP(allStatistics[index].Time) < p) ShowCriticalMoment(ToP(allStatistics[index].Time), ref index);

                timelineFront.SetPosition(1, new Vector3(LineHalfWidth * (p * 2f - 1f), 0));
                p += Time.deltaTime / 3f;
                yield return null;
            }
            while (index < allStatistics.Length) ShowCriticalMoment(ToP(allStatistics[index].Time), ref index);
            timelineFront.SetPosition(1, new Vector3(LineHalfWidth, 0));
        }
        private IEnumerator CoShowTimeBackLine()
        {
            float t = 0f;

            timelineBack.SetPosition(0, new Vector3(-LineHalfWidth, 0));
            timelineBack.SetColors(MainColor * BackColorRate, MainColor.AlphaMultiplied(0));

            while (true)
            {
                float log = Mathf.Log(t + 1f, 1.92f);
                float exp = t > 1.3f ? Mathf.Pow((t - 1.3f) * 0.86f, 3f) : 0f;
                t += Time.deltaTime;

                timelineBack.SetPosition(1, new Vector3(log < 1 ? log * LineHalfWidth : LineHalfWidth, 0));
                float a = exp;
                if (log > 1) a += ((log - 1) / log) * 0.3f * LineHalfWidth;
                timelineBack.endColor = MainColor.AlphaMultiplied(a > 1f ? 1f : a) * BackColorRate;

                if (log > 1f && a > 1f) break;

                yield return null;
            }

            timelineBack.SetPosition(1, new Vector3(LineHalfWidth, 0));
            timelineBack.endColor = MainColor * BackColorRate;
        }

        public void ClearDetail(bool onlyMinimap)
        {
            baseOnMinimap.ForEachChild((Il2CppSystem.Action<GameObject>)((c) => GameObject.Destroy(c)));
            if (!onlyMinimap) detailHolder.ForEachChild((Il2CppSystem.Action<GameObject>)((c) => GameObject.Destroy(c)));
        }

        public void Hide()
        {
            minimap.SetActive(false);
            detailHolder.SetActive(false);
        }
        public void Show(GameStatistics.Event statisticsEvent)
        {
            //対象となるCriticalPointを探す
            int index = 0, indexMin = 0, indexMax = 0;
            while (allStatistics[index] != statisticsEvent) index++;
            CriticalPoints.ForEachChild((Il2CppSystem.Action<GameObject>)((obj) => {
                var criticalPoint = obj.GetComponent<CriticalPoint>();
                if (criticalPoint.Contains(index))
                {
                    indexMin = criticalPoint.IndexMin;
                    indexMax = criticalPoint.IndexMax;
                }
            }));

            //CriticalPointが一致しない場合は詳細も含めてリセットする
            var lastIndex = Array.IndexOf(allStatistics, currentShown);
            var requireGenerateDetail = !(indexMin <= lastIndex && lastIndex <= indexMax);
            ClearDetail(!requireGenerateDetail);

            minimap.SetActive(true);
            detailHolder.SetActive(true);

            float p = 0f;
            foreach (var pos in statisticsEvent.Position)
            {
                var renderer = GameObject.Instantiate(TORGameManager.Instance.RuntimeAsset.MinimapPrefab.HerePoint, baseOnMinimap.transform);
                PlayerMaterial.SetColors(GameData.Instance.GetPlayerById(pos.Item1).DefaultOutfit.ColorId, renderer);
                renderer.transform.localPosition = (Vector3)(pos.Item2 / TORGameManager.Instance.RuntimeAsset.MapScale) + new Vector3(0, 0, -1f - p);
                var button = renderer.gameObject.SetUpButton();
                button.gameObject.AddComponent<BoxCollider2D>().size = new(0.3f, 0.3f);
                var hover = button.gameObject.AddComponent<TouchHover>();

                hover.OnHoverOver.AddListener((Action)(() => {
                    MetaContextOld context = new();

                    foreach (var near in statisticsEvent.Position)
                    {
                        if (near.Item2.Distance(pos.Item2) > 0.6f) continue;

                        if (context.Count > 0) context.Append(new MetaContextOld.VerticalMargin(0.1f));
                        var history = TORGameManager.Instance.RoleHistory.Where(x => x.PlayerId == near.Item1 && x.Time <= statisticsEvent.Time).LastOrDefault();
                        var role = history.RoleInfo;
                        bool isMadmate = history.IsMadmate;
                        var roleText = Helpers.cs(isMadmate ? Madmate.color : history.Color, isMadmate ? (role == RoleInfo.crewmate ? Madmate.fullName : Madmate.prefix + role.name) : role.name);
                        context.Append(new MetaContextOld.Text(TextAttribute.BoldAttrLeft) { RawText = GameData.Instance.GetPlayerById(near.Item1).PlayerName });
                        context.Append(new MetaContextOld.VariableText(new TextAttribute(TextAttribute.BoldAttrLeft) { Alignment = TMPro.TextAlignmentOptions.TopLeft }.EditFontSize(1.35f)) { RawText = roleText });
                    }

                    TORGUIManager.Instance.SetHelpContext(button, context);
                }));
                hover.OnHoverOut.AddListener((Action)(() => TORGUIManager.Instance.HideHelpContextIf(button)));

                p += 0.001f;
            }


            int num = 0;
            void EventToDetailShower(int eventIndex)
            {
                GameStatistics.Event target = allStatistics[eventIndex];

                GameObject detail = Helpers.CreateObject("EventDetail", detailHolder.transform, new Vector3(0, -0.76f * num, -10f));

                var backGround = Helpers.CreateSharpBackground(new Vector2(3.4f, 0.7f), MainColor, detail.transform);

                var collider = detail.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(3.4f, 0.7f);
                var button = detail.gameObject.SetUpButton(true, null);
                var hover = button.gameObject.AddComponent<TouchHover>();
                button.OnClick.RemoveAllListeners();
                hover.OnHoverOver.AddListener((Action)(() =>
                {
                    OnMouseOver(eventIndex);
                    backGround.color = Color.Lerp(MainColor, Color.white, 0.5f);
                }));
                hover.OnHoverOut.AddListener((Action)(() =>
                {
                    OnMouseOver(eventIndex);
                    backGround.color = MainColor;
                }));

                List<GameObject> objects = new();

                Il2CppArgument<PoolablePlayer> GeneratePlayerView(byte id)
                {
                    PoolablePlayer player = GameObject.Instantiate(PlayerPrefab, detail.transform);
                    player.UpdateFromPlayerOutfit(GameData.Instance.GetPlayerById(id).DefaultOutfit, PlayerMaterial.MaskType.None, false, true, null);
                    player.ToggleName(true);
                    player.SetName(GameData.Instance.GetPlayerById(id).PlayerName, new Vector3(3.1f, 3.1f, 1f), Color.white, -15f);
                    player.transform.localScale = new Vector3(0.24f, 0.24f, 1f);
                    player.cosmetics.nameText.transform.parent.localPosition += new Vector3(0f, -1.05f, 0f);
                    return player;
                }

                if (target.SourceId.HasValue) objects.Add(GeneratePlayerView(target.SourceId.Value).Value.gameObject);

                SpriteRenderer icon = Helpers.CreateObject<SpriteRenderer>("Icon", detail.transform, new Vector3(0, 0, -1f));
                icon.sprite = target.Variation.InteractionIcon?.GetSprite()!;
                icon.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
                if (target.RelatedTag != null)
                {
                    var text = GameObject.Instantiate(GameEndText, icon.transform);
                    text.text = target.RelatedTag.Text;
                    text.color = Color.white;
                    text.outlineWidth = 0.1f;
                    text.transform.localPosition = new Vector3(0f, -0.18f, -1f);
                    text.transform.localScale = new Vector3(0.2f / 0.7f, 0.2f / 0.7f, 1f);
                    icon.transform.localPosition += new Vector3(0f, 0.05f, 0f);
                }
                objects.Add(icon.gameObject);

                foreach (var p in GameData.Instance.AllPlayers)
                    if ((target.TargetIdMask & (1 << p.PlayerId)) != 0)
                        objects.Add(GeneratePlayerView(p.PlayerId).Value.gameObject);

                float width = Mathf.Min(1.2f, (float)(objects.Count - 1) * 0.5f);
                for (int i = 0; i < objects.Count; i++)
                {
                    float pos = objects.Count == 1 ? 0 : width * ((float)i / (objects.Count - 1) * 2f - 1f);
                    objects[i].transform.localPosition += new Vector3(pos, 0, 0f);
                }

                num++;
            }


            if (requireGenerateDetail)
            {
                for (int i = indexMin; i <= indexMax; i++)
                {
                    EventToDetailShower(i);
                }
            }

        }


        public void OnMouseOver(int index)
        {
            eventPiled = allStatistics[index];
        }
        public void OnMouseOut(int index)
        {
            if (eventPiled == allStatistics[index]) eventPiled = null;
        }
    }

    public class Il2CppArgument<T>
    {
        public T Value { get; private set; }
        public Il2CppArgument(T value)
        {
            Value = value;
        }

        public static implicit operator Il2CppArgument<T>(T value)
        {
            return new Il2CppArgument<T>(value);
        }
    }
}
