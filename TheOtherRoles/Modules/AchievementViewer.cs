using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheOtherRoles.MetaContext;
using UnityEngine;

namespace TheOtherRoles.Modules
{
    public class AchievementViewer : MonoBehaviour
    {
        static AchievementViewer() => ClassInjector.RegisterTypeInIl2Cpp<AchievementViewer>();
        static public MainMenuManager MainMenu;

        private MetaScreen myScreen = null!;

        protected void Close()
        {
            TransitionFade.Instance.DoTransitionFade(gameObject, null!, () => MainMenu?.mainMenuUI.SetActive(true), () => GameObject.Destroy(gameObject));
        }

        static public void Open(MainMenuManager mainMenu)
        {
            MainMenu = mainMenu;

            var obj = Helpers.CreateObject<AchievementViewer>("AchievementViewer", Camera.main.transform, new Vector3(0, 0, -30f));
            TransitionFade.Instance.DoTransitionFade(null!, obj.gameObject, () => { mainMenu.mainMenuUI.SetActive(false); }, () => { obj.OnShown(); });
        }

        static public GUIContext GenerateContext(float scrollerHeight, float width, string scrollerTag = null, Predicate<ITORAchievement> predicate = null, string shownText = null)
        {
            scrollerTag ??= "Achievements";

            var gui = TORGUIContextEngine.API;

            List<GUIScrollDynamicInnerContent> inner = new();
            var attr = new TextAttributes(gui.GetAttribute(AttributeParams.OblongLeft)) { FontSize = new(1.85f) };
            var headerAttr = new TextAttributes(gui.GetAttribute(AttributeParams.StandardLeft)) { FontSize = new(1.1f) };
            var detailTitleAttr = new TextAttributes(gui.GetAttribute(AttributeParams.StandardBaredBoldLeft)) { FontSize = new(1.8f) };
            var detailDetailAttr = new TextAttributes(gui.GetAttribute(AttributeParams.StandardBaredLeft)) { FontSize = new(1.5f), Size = new(5f, 6f) };
            var groupAttr = new TextAttributes(gui.GetAttribute(AttributeParams.OblongLeft)) { FontSize = new(1.6f) };

            void AddGroup(string group, IEnumerable<ITORAchievement> achievements)
            {
                bool first = true;
                foreach (var a in achievements.Where(a => (predicate?.Invoke(a) ?? true) && !a.IsHidden))
                {
                    if (first)
                    {
                        inner.Add(new(GUIAlignment.Left, new TORGUIText(GUIAlignment.Left, groupAttr, new TranslateTextComponent("achievementGroup" + group)), 0.5f));
                        first = false;
                    }

                    List<GUIContext> context = [
                        new TORGUIMargin(GUIAlignment.Left, new(0f, 0.13f)),
                        new TORGUIText(GUIAlignment.Left, headerAttr, a.GetHeaderComponent()),
                        new TORGUIMargin(GUIAlignment.Left, new(0f, -0.12f)),
                        new TORGUIText(GUIAlignment.Left, attr, a.GetTitleComponent(ITORAchievement.HiddenComponent)) { OverlayContext = a.GetOverlayContext(true, false, true, false, a.IsCleared),
#if WINDOWS
                            OnClickText = (() => { if (a.IsCleared) { TORAchievementManager.SetOrToggleTitle(a); } }, true)
#endif
                        },
                    ];

                    float height = 0.75f;
                    var progress = a.GetDetailContext();
                    if (progress != null)
                    {
                        context.Add(progress);
                        height += 0.35f;
                    }

                    var achievementContent = new VerticalContextsHolder(GUIAlignment.Center, context);

                    var aContenxt = new HorizontalContextsHolder(GUIAlignment.Left,
                        new VerticalContextsHolder(GUIAlignment.Center, gui.VerticalMargin(0.05f),
                    new TORGUIImage(GUIAlignment.Left, new WrapSpriteLoader(() => ITORAchievement.TrophySprite.GetSprite(a.Trophy)), new(0.38f, 0.38f), a.IsCleared ? Color.white : new UnityEngine.Color(0.2f, 0.2f, 0.2f)) { IsMasked = true }),
                    new TORGUIMargin(GUIAlignment.Left, new(0.15f, 0.1f)),
                    achievementContent
                    );

                    inner.Add(new(GUIAlignment.Left, aContenxt, height));
                }
            }

            AddGroup("Recently", TORAchievementManager.RecentlyCleared);
            AddGroup("Roles", TORAchievementManager.AllAchievements.Where(a => !a.RelatedRole.IsEmpty()));
            AddGroup("Seasonal", TORAchievementManager.AllAchievements.Where(a => a.RelatedRole.IsEmpty() && !a.AchievementType().IsEmpty() && a.AchievementType().First() == AchievementType.Seasonal));

            var scroller = new GUIScrollDynamicView(GUIAlignment.Center, new(4.7f, scrollerHeight), inner) { ScrollerTag = scrollerTag, WithMask = true };

            var cul = TORAchievementManager.Aggregate(predicate);
            List<GUIContext> footerList = new();
            for (int i = 0; i < cul.Length; i++)
            {
                int copiedIndex = i;
                if (footerList.Count != 0) footerList.Add(new TORGUIMargin(GUIAlignment.Center, new(0.2f, 0f)));

                footerList.Add(new TORGUIImage(GUIAlignment.Left, new WrapSpriteLoader(() => ITORAchievement.TrophySprite.GetSprite(copiedIndex)), new(0.5f, 0.5f)));
                footerList.Add(new TORGUIMargin(GUIAlignment.Center, new(0.05f, 0f)));
                footerList.Add(new TORGUIText(GUIAlignment.Left, detailDetailAttr, new RawTextComponent(cul[i].num + "/" + cul[i].max)));
            }
            footerList.Add(new TORGUIMargin(GUIAlignment.Center, new(0.3f, 0f)));
            footerList.Add(new TORGUIText(GUIAlignment.Right, detailDetailAttr, new RawTextComponent(shownText ?? ModTranslation.getString(predicate != null ? "achievementShown" : "achievementAllAchievements") + ": " + cul.Sum(c => c.num) + "/" + cul.Sum(c => c.max))));
            var footer = new HorizontalContextsHolder(GUIAlignment.Center, footerList.ToArray());

            return new VerticalContextsHolder(GUIAlignment.Left, scroller, new TORGUIMargin(GUIAlignment.Center, new(0f, 0.15f)), footer)
            { FixedWidth = width };
        }

        public void OnShown()
        {
            var gui = TORGUIContextEngine.Instance;

            var title = new TORGUIText(GUIAlignment.Left, gui.GetAttribute(AttributeAsset.OblongHeader), new TranslateTextComponent("achievementTitle"));

            gameObject.SetActive(true);
            myScreen.SetContext(new VerticalContextsHolder(GUIAlignment.Left, title, GenerateContext(3.85f, 9f)), out _);

        }

        public void Awake()
        {
            if (MainMenu != null)
            {
                var backBlackPrefab = MainMenu.playerCustomizationPrefab.transform.GetChild(1);
                GameObject.Instantiate(backBlackPrefab.gameObject, transform);
                var backGroundPrefab = MainMenu.playerCustomizationPrefab.transform.GetChild(2);
                var backGround = GameObject.Instantiate(backGroundPrefab.gameObject, transform);
                GameObject.Destroy(backGround.transform.GetChild(2).gameObject);

                var closeButtonPrefab = MainMenu.playerCustomizationPrefab.transform.GetChild(0).GetChild(0);
                var closeButton = GameObject.Instantiate(closeButtonPrefab.gameObject, transform);
                GameObject.Destroy(closeButton.GetComponent<AspectPosition>());
                var button = closeButton.GetComponent<PassiveButton>();
                button.gameObject.SetActive(true);
                button.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                button.OnClick.AddListener((Action)(() => Close()));
                button.gameObject.SetAsUIAspectContent(AspectPosition.EdgeAlignments.LeftTop, new(0.4f, 0.4f, -50f));
            }

            myScreen = Helpers.CreateObject<MetaScreen>("Screen", transform, new Vector3(0, -0.1f, -10f));
            myScreen.SetBorder(new(9f, 5.5f));
        }
    }
}
