using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmongUs.GameOptions;
using TheOtherRoles.Objects;
using TMPro;
using Twitch;
using UnityEngine;

namespace TheOtherRoles.MetaContext;

public class VanillaAsset
{
    public class VanillaAudioClip
    {
        private string name;
        private AudioClip clip = null;
        public AudioClip Clip
        {
            get
            {
                if (clip) return clip;
                clip = Helpers.FindAsset<AudioClip>(name);
                return clip!;
            }
        }

        public VanillaAudioClip(string name)
        {
            this.name = name;
        }
    }

    static public Sprite CloseButtonSprite { get; private set; } = null!;
    static public TMPro.TextMeshPro StandardTextPrefab { get; private set; } = null!;
    static public PlayerCustomizationMenu PlayerOptionsMenuPrefab { get; private set; } = null!;
    static public Sprite PopUpBackSprite { get; private set; } = null!;
    static public Sprite FullScreenSprite { get; private set; } = null!;
    static public Sprite TextButtonSprite { get; private set; } = null!;
    static public VanillaAudioClip HoverClip { get; private set; } = new("UI_Hover")!;
    static public VanillaAudioClip SelectClip { get; private set; } = new("UI_Select")!;
    static public bool Loaded = false;

    static public Material OblongMaskedFontMaterial
    {
        get
        {
            if (oblongMaskedFontMaterial == null) oblongMaskedFontMaterial = Helpers.FindAsset<Material>("Brook Atlas Material Masked");
            return oblongMaskedFontMaterial!;
        }
    }
    static private Material oblongMaskedFontMaterial = null;
    static private TMP_FontAsset preSpawnFont = null;
    static public TMP_FontAsset PreSpawnFont
    {
        get
        {
            if (preSpawnFont == null) preSpawnFont = Helpers.FindAsset<TMP_FontAsset>("DIN_Pro_Bold_700 SDF")!;
            return preSpawnFont;
        }
    }
    static public Material StandardMaskedFontMaterial
    {
        get
        {
            if (standardMaskedFontMaterial == null) standardMaskedFontMaterial = Helpers.FindAsset<Material>("LiberationSans SDF - BlackOutlineMasked")!;
            return standardMaskedFontMaterial!;
        }
    }
    static private Material standardMaskedFontMaterial = null;
    static private TMP_FontAsset versionFont = null;
    static public TMP_FontAsset VersionFont
    {
        get
        {
            if (versionFont == null) versionFont = Helpers.FindAsset<TMP_FontAsset>("Barlow-Medium SDF");
            return versionFont!;
        }
    }

    static private TMP_FontAsset brookFont = null;
    static public TMP_FontAsset BrookFont
    {
        get
        {
            if (brookFont == null) brookFont = Helpers.FindAsset<TMP_FontAsset>("Brook SDF")!;
            return brookFont;
        }
    }

    private static Material highlightMaterial = null;
    public static Material GetHighlightMaterial()
    {
        if (highlightMaterial != null) return new Material(highlightMaterial);
        foreach (var mat in UnityEngine.Resources.FindObjectsOfTypeAll(Il2CppType.Of<Material>()))
        {
            if (mat.name == "HighlightMat")
            {
                highlightMaterial = mat.TryCast<Material>();
                break;
            }
        }
        return new Material(highlightMaterial);
    }

    static public readonly ShipStatus[] MapAsset = new ShipStatus[6];
    static public Vector2 GetMapCenter(byte mapId) => MapAsset[mapId].MapPrefab.transform.GetChild(5).localPosition;
    static public float GetMapScale(byte mapId) => MapAsset[mapId].MapScale;
    static public Vector2 ConvertToMinimapPos(Vector2 pos, Vector2 center, float scale) => (pos / scale) + center;
    static public Vector2 ConvertToMinimapPos(Vector2 pos, byte mapId) => ConvertToMinimapPos(pos, GetMapCenter(mapId), GetMapScale(mapId));
    static public Vector2 ConvertFromMinimapPosToWorld(Vector2 minimapPos, Vector2 center, float scale) => (minimapPos - center) * scale;
    static public Vector2 ConvertFromMinimapPosToWorld(Vector2 minimapPos, byte mapId) => ConvertFromMinimapPosToWorld(minimapPos, GetMapCenter(mapId), GetMapScale(mapId));

    static public void LoadAssetsOnTitle()
    {
        var twitchPopUp = TwitchManager.Instance.transform.GetChild(0);
        PopUpBackSprite = twitchPopUp.GetChild(3).GetComponent<SpriteRenderer>().sprite;
        FullScreenSprite = twitchPopUp.GetChild(0).GetComponent<SpriteRenderer>().sprite;
        CloseButtonSprite = Helpers.FindAsset<Sprite>("closeButton")!;
        TextButtonSprite = twitchPopUp.GetChild(2).GetComponent<SpriteRenderer>().sprite;

        StandardTextPrefab = UnityEngine.Object.Instantiate(twitchPopUp.GetChild(1).GetComponent<TMPro.TextMeshPro>(), (Transform)null);
        StandardTextPrefab.gameObject.hideFlags = HideFlags.HideAndDontSave;
        UnityEngine.Object.Destroy(StandardTextPrefab.spriteAnimator);
        UnityEngine.Object.DontDestroyOnLoad(StandardTextPrefab.gameObject);
    }

    private static AudioClip GetIntroSound(RoleTypes roleType)
    {
        return RoleManager.Instance.AllRoles.Find((Il2CppSystem.Predicate<RoleBehaviour>)(role => role.Role == roleType))?.IntroSound;
    }

    public static AudioClip ScientistIntroSound => GetIntroSound(RoleTypes.Scientist);
    public static AudioClip EngineerIntroSound => GetIntroSound(RoleTypes.Engineer);
    public static AudioClip NoisemakerIntroSound => GetIntroSound(RoleTypes.Noisemaker);
    public static AudioClip TrackerIntroSound => GetIntroSound(RoleTypes.Tracker);
    public static AudioClip DetectiveIntroSound => GetIntroSound(RoleTypes.Detective);
    public static AudioClip JudgeIntroSound => GetIntroSound(RoleTypes.Judge);
    public static AudioClip ShapeshifterIntroSound => GetIntroSound(RoleTypes.Shapeshifter);
    public static AudioClip PhantomIntroSound => GetIntroSound(RoleTypes.Phantom);
    public static AudioClip ViperIntroSound => GetIntroSound(RoleTypes.Viper);

    public static void PlaySelectSE() => SoundManager.Instance.PlaySound(SelectClip.Clip, false, 0.8f);
    public static void PlayHoverSE() => SoundManager.Instance.PlaySound(HoverClip.Clip, false, 0.8f);

    static public void LoadAssetAtInitialize()
    {
        if (Loaded) return;
        PlayerOptionsMenuPrefab = Helpers.FindAsset<PlayerCustomizationMenu>("LobbyPlayerCustomizationMenu")!;
        Loaded = true;
    }

    static public Scroller GenerateScroller(Vector2 size, Transform transform, Vector3 scrollBarLocalPos, Transform target, FloatRange bounds, float scrollerHeight)
    {
        var barBack = GameObject.Instantiate(PlayerOptionsMenuPrefab.transform.GetChild(4).FindChild("UI_ScrollbarTrack").gameObject, transform);
        var bar = GameObject.Instantiate(PlayerOptionsMenuPrefab.transform.GetChild(4).FindChild("UI_Scrollbar").gameObject, transform);
        barBack.transform.localPosition = scrollBarLocalPos + new Vector3(0.12f, 0f, 0f);
        bar.transform.localPosition = scrollBarLocalPos;

        var scrollBar = bar.GetComponent<Scrollbar>();

        var scroller = Helpers.CreateObject<Scroller>("Scroller", transform, new Vector3(0, 0, 5));
        var collider = scroller.gameObject.AddComponent<BoxCollider2D>();
        collider.size = size;

        scrollBar.parent = scroller;
        scrollBar.graphic = bar.GetComponent<SpriteRenderer>();
        scrollBar.trackGraphic = barBack.GetComponent<SpriteRenderer>();
        scrollBar.trackGraphic.size = new Vector2(scrollBar.trackGraphic.size.x, scrollerHeight);

        var ratio = scrollerHeight / 3.88f;

        scroller.Inner = target;
        scroller.SetBounds(bounds, null);
        scroller.allowY = true;
        scroller.allowX = false;
        scroller.ScrollbarYBounds = new FloatRange(-1.8f * ratio + scrollBarLocalPos.y + 0.4f, 1.8f * ratio + scrollBarLocalPos.y - 0.4f);
        scroller.ScrollbarY = scrollBar;
        scroller.active = true;

        scroller.name = ClickMaskName;
        var instanceId = scroller.gameObject.GetInstanceID();
        var buttonManager = PassiveButtonManager.Instance;
        var predicate = (Il2CppSystem.Predicate<PassiveUiElement>)(b => b != null && b && b.isActiveAndEnabled && b.name == ClickMaskName);
        scroller.gameObject.AddComponent<ScriptBehaviour>().UpdateHandler += () =>
        {
            var found = buttonManager.Buttons.Find(predicate);
            if (found != null && found && found.gameObject.GetInstanceID() == instanceId)
            {
                scroller.MouseMustBeOverToScroll = false;
                collider.size = size;
            }
            else
            {
                scroller.MouseMustBeOverToScroll = true;
                collider.size = Vector2.zero;
                scroller.mouseOver = false;
            }
        };

        //scroller.Colliders = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Collider2D>(new Collider2D[] { hitBox });

        scroller.ScrollToTop();

        return scroller;
    }

    private const string ClickMaskName = "TORScroller";

    public static PlayerDisplay GetPlayerDisplay()
    {
        AmongUsClient.Instance.PlayerPrefab.gameObject.SetActive(false);
        var display = UnityEngine.Object.Instantiate(AmongUsClient.Instance.PlayerPrefab.gameObject);
        AmongUsClient.Instance.PlayerPrefab.gameObject.SetActive(true);

        UnityEngine.Object.Destroy(display.GetComponent<PlayerControl>());
        UnityEngine.Object.Destroy(display.GetComponent<PlayerPhysics>());
        UnityEngine.Object.Destroy(display.GetComponent<Rigidbody2D>());
        UnityEngine.Object.Destroy(display.GetComponent<CircleCollider2D>());
        UnityEngine.Object.Destroy(display.GetComponent<CustomNetworkTransform>());
        UnityEngine.Object.Destroy(display.GetComponent<BoxCollider2D>());
        UnityEngine.Object.Destroy(display.GetComponent<DummyBehaviour>());
        UnityEngine.Object.Destroy(display.GetComponent<AudioSource>());
        UnityEngine.Object.Destroy(display.GetComponent<PassiveButton>());
        UnityEngine.Object.Destroy(display.GetComponent<HnSImpostorScreamSfx>());

        display.gameObject.SetActive(true);

        return display.AddComponent<PlayerDisplay>();
    }
}
