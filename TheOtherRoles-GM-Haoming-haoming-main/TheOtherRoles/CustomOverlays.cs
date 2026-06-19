using System;
using System.Linq;
using AmongUs.Data;
using HarmonyLib;
using InnerNet;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TheOtherRoles;

[Harmony]
public class CustomOverlays
{
    public static bool overlayShown;
    private static SpriteRenderer roleUnderlay;
    private static TextMeshPro infoOverlayRoles;
    private static Sprite colorBG;
    private static SpriteRenderer meetingUnderlay;
    private static SpriteRenderer infoUnderlay;
    private static Scroller scroller;

    public static bool initializeOverlays()
    {
        HudManager hudManager = FastDestroyableSingleton<HudManager>.Instance;
        if (hudManager == null) return false;

        if (meetingUnderlay == null)
        {
            meetingUnderlay = Object.Instantiate(hudManager.FullScreen, hudManager.transform);
            meetingUnderlay.transform.localPosition = new Vector3(0f, 0f, 20f);
            meetingUnderlay.gameObject.SetActive(true);
            meetingUnderlay.enabled = false;
        }

        if (colorBG == null) colorBG = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.White.png", 100f);

        if (infoOverlayRoles == null)
        {
            infoOverlayRoles = Object.Instantiate(hudManager.TaskPanel.taskText, hudManager.transform);
            infoOverlayRoles.fontSize = infoOverlayRoles.fontSizeMin = infoOverlayRoles.fontSizeMax = 1.15f;
            infoOverlayRoles.outlineWidth += 0.02f;
            infoOverlayRoles.autoSizeTextContainer = false;
            infoOverlayRoles.enableWordWrapping = false;
            infoOverlayRoles.alignment = TextAlignmentOptions.TopLeft;
            infoOverlayRoles.transform.position = Vector3.zero;
            infoOverlayRoles.transform.localPosition = new Vector3(-3.5f, 2.5f, -910f) + new Vector3(2.5f, 0.0f, 0.0f);
            infoOverlayRoles.transform.localScale = Vector3.one;
            infoOverlayRoles.color = Palette.White;
            infoOverlayRoles.enabled = false;
        }

        if (infoUnderlay == null)
        {
            infoUnderlay = Object.Instantiate(meetingUnderlay, hudManager.transform);
            infoUnderlay.transform.localPosition = new Vector3(0f, 0f, -900f);
            infoUnderlay.gameObject.SetActive(true);
            infoUnderlay.enabled = false;
        }

        if (scroller == null)
        {
            scroller = infoUnderlay.gameObject.AddComponent<Scroller>();
            scroller.Inner = infoOverlayRoles.transform;
            scroller.allowY = true;
            scroller.gameObject.SetActive(true);
            scroller.enabled = false;
        }

        return true;
    }

    public static void hideBlackBG()
    {
        if (meetingUnderlay == null) return;
        meetingUnderlay.enabled = false;
    }

    public static void showInfoOverlay()
    {
        if (overlayShown) return;

        HudManager hudManager = FastDestroyableSingleton<HudManager>.Instance;
        if (MapUtilities.CachedShipStatus == null || PlayerControl.LocalPlayer == null || hudManager == null ||
            FastDestroyableSingleton<HudManager>.Instance.IsIntroDisplayed ||
            (!PlayerControl.LocalPlayer.CanMove && MeetingHud.Instance == null))
            return;

        if (!initializeOverlays()) return;

        //HudManagerUpdate.CloseSettings();

        if (MapBehaviour.Instance != null)
            MapBehaviour.Instance.Close();

        hudManager.SetHudActive(false);

        overlayShown = true;

        Transform parent;
        if (MeetingHud.Instance != null)
            parent = MeetingHud.Instance.transform;
        else
            parent = hudManager.transform;

        infoUnderlay.transform.parent = parent;
        infoOverlayRoles.transform.parent = parent;

        infoUnderlay.sprite = colorBG;
        infoUnderlay.color = new Color(0.1f, 0.1f, 0.1f, 0.88f);
        infoUnderlay.transform.localScale = new Vector3(7.5f, 5f, 1f);
        infoUnderlay.enabled = true;

        string rolesText = "";
        foreach (RoleInfo r in RoleInfo.getRoleInfoForPlayer(PlayerControl.LocalPlayer))
        {
            string roleDesc = r.fullDescription;
            rolesText += $"<size=180%>{Helpers.cs(r.color, r.name)}</size>" + "\n" +
                         $"<size=130%>{Helpers.cs(r.color, r.introDescription)}</size>" +
                         (roleDesc != "" ? $"\n{r.fullDescription}" : "") + "\n\n";
        }

        int rows = rolesText.Count(c => c == '\n');
        int infoCount = RoleInfo.getRoleInfoForPlayer(PlayerControl.LocalPlayer).Count;
        float maxY = Mathf.Max(1.15f,
            (((infoCount * 2.2f) + rows - 18f) *
             (DataManager.Settings.Language.CurrentLanguage == SupportedLangs.English ? 0.06f : 0.09f)) + 1.165f);
        scroller.enabled = true;
        scroller.ContentYBounds = new FloatRange(1.15f, maxY);
        scroller.ScrollToTop();

        infoOverlayRoles.text = rolesText;
        infoOverlayRoles.enabled = true;

        Color underlayTransparent = new(0.1f, 0.1f, 0.1f, 0.0f);
        Color underlayOpaque = new(0.1f, 0.1f, 0.1f, 0.88f);
        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(0.2f, new Action<float>(t =>
        {
            infoUnderlay.color = Color.Lerp(underlayTransparent, underlayOpaque, t);
            infoOverlayRoles.color = Color.Lerp(Palette.ClearWhite, Palette.White, t);
        })));
    }

    public static void hideInfoOverlay()
    {
        if (!overlayShown) return;

        if (MeetingHud.Instance == null) FastDestroyableSingleton<HudManager>.Instance.SetHudActive(true);

        overlayShown = false;
        Color underlayTransparent = new(0.1f, 0.1f, 0.1f, 0.0f);
        Color underlayOpaque = new(0.1f, 0.1f, 0.1f, 0.88f);

        if (scroller != null) scroller.enabled = false;

        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(0.2f, new Action<float>(t =>
        {
            if (infoUnderlay != null)
            {
                infoUnderlay.color = Color.Lerp(underlayOpaque, underlayTransparent, t);
                if (t >= 1.0f) infoUnderlay.enabled = false;
            }

            if (infoOverlayRoles != null)
            {
                infoOverlayRoles.color = Color.Lerp(Palette.White, Palette.ClearWhite, t);
                if (t >= 1.0f) infoOverlayRoles.enabled = false;
            }
        })));
    }

    public static void toggleInfoOverlay()
    {
        if (overlayShown)
            hideInfoOverlay();
        else
            showInfoOverlay();
    }

    public static void resetOverlays()
    {
        hideBlackBG();
        hideInfoOverlay();
        Object.Destroy(meetingUnderlay);
        Object.Destroy(infoUnderlay);
        Object.Destroy(infoOverlayRoles);
        Object.Destroy(roleUnderlay);
        Object.Destroy(scroller);

        overlayShown = false;
        roleUnderlay = null;
        meetingUnderlay = infoUnderlay = null;
    }

    [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
    public static class CustomOverlayKeybinds
    {
        public static void Postfix(KeyboardJoystick __instance)
        {
            ChatController cc = DestroyableSingleton<ChatController>.Instance;
            bool isOpen = cc != null && cc?.IsOpenOrOpening == true;
            if (Input.GetKeyDown(KeyCode.H) && AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started &&
                !isOpen && !Minigame.Instance && !ExileController.Instance) toggleInfoOverlay();
        }
    }
}
