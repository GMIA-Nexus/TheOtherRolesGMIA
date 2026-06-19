using System;
using System.Collections.Generic;
using AmongUs.Data;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TheOtherRoles.Modules;

[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
public class ModUpdaterButton
{
    private static void Prefix(MainMenuManager __instance)
    {
        AssetLoader.LoadAssets();
        Helpers.LoadREAssets();

        GameObject template = GameObject.Find("ExitGameButton");

        GameObject menuobj = Object.Instantiate(template, null);
        Object.Destroy(menuobj.GetComponent<AspectPosition>());
        menuobj.transform.localPosition = new Vector3(4.4473f, -1.7764f, 0);

        TextMeshPro menubutton = menuobj.GetComponentInChildren<TextMeshPro>();
        menubutton.transform.localPosition = new Vector3(4.4473f, -1.7764f, 0);
        menubutton.alignment = TextAlignmentOptions.Right;
        __instance.StartCoroutine(Effects.Lerp(0.1f,
            new Action<float>(p =>
            {
                menubutton.SetText(DataManager.Settings.Language.CurrentLanguage == SupportedLangs.SChinese
                    ? "联系与反馈"
                    : "GITHUB");
            })));

        PassiveButton passiveButtonmenu = menuobj.GetComponent<PassiveButton>();
        SpriteRenderer buttonSpritemenu = menuobj.transform.FindChild("Inactive").GetComponent<SpriteRenderer>();

        passiveButtonmenu.OnClick = new Button.ButtonClickedEvent();
        passiveButtonmenu.OnClick.AddListener((Action)(() =>
            Application.OpenURL(DataManager.Settings.Language.CurrentLanguage == SupportedLangs.SChinese
                ? "https://qm.qq.com/q/CfsaQuYZBm"
                : "https://github.com/FangKuaiYa/TheOtherRoles-GM-Haoming")));
        Color menuColor = Color.cyan;
        buttonSpritemenu.color = menubutton.color = menuColor;
        passiveButtonmenu.OnMouseOut.AddListener((Action)delegate
        {
            buttonSpritemenu.color = menubutton.color = menuColor;
        });
    }
}
