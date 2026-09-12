using System.Text.RegularExpressions;
using AmongUs.Data;
using HarmonyLib;
using InnerNet;
using Reactor.Utilities.Extensions;
using TheOtherRoles.Modules;
using System.Collections.Generic;
using System.Linq;
using System;
using TMPro;
using UnityEngine;
namespace TheOtherRoles.Patches
{
    [HarmonyPatch]
    public static class LobbyJoin
    {
        private static GameObject EnterCodeSelectServer;
        public static MainMenuManager MainMenuManager = null;

        [HarmonyPatch(typeof(EnterCodeManager), nameof(EnterCodeManager.OnEnable))]
        [HarmonyPostfix]
        public static void OnEnable(EnterCodeManager __instance)
        {
            try
            {
                var mainmenu = MainMenuManager;
                if (mainmenu != null)
                {
                    ServerDropdown dropdown = null;
                    if (!EnterCodeSelectServer)
                    {
                        var obj = UnityEngine.Object.Instantiate(mainmenu.createGameScreen.serverButton.transform.parent.gameObject, __instance.enterCodeField.transform.parent);
                        obj.name = "EnterCodeSelectServer";
                        obj.transform.localPosition = new(2.8596f, 1.234f);
                        dropdown = UnityEngine.Object.Instantiate(mainmenu.createGameScreen.serverDropdown, obj.transform);
                        EnterCodeSelectServer = obj;
                    }
                    else
                    {
                        dropdown = EnterCodeSelectServer.GetComponentInChildren<ServerDropdown>();
                    }
                    mainmenu.entercodeField.transform.parent.localPosition = new(-1.1106f, 0.12f);
                    List<TextMeshPro> tmps = new();
                    EnterCodeSelectServer.transform.Find("BlackSquare").transform.localPosition = new(0.554f, 0.658f, -1f);
                    GameObject serverButton = EnterCodeSelectServer.transform.Find("ServerBox").gameObject;
                    serverButton.transform.localPosition = new(-0.97f, 0f);
                    tmps.Add(serverButton.transform.Find("Inactive/ClassicText").GetComponent<TextMeshPro>());
                    tmps.Add(serverButton.transform.Find("Highlight/ClassicText").GetComponent<TextMeshPro>());
                    void UpdateServer(string text)
                    {
                        tmps.Do(t => t.text = text);
                    }
                    string SetCurrentServer()
                    {
                        IRegionInfo currentRegion = DestroyableSingleton<ServerManager>.Instance.CurrentRegion;
                        return DestroyableSingleton<TranslationController>.Instance.GetStringWithDefault(currentRegion.TranslateName, currentRegion.Name, new Il2CppReferenceArray<Il2CppSystem.Object>(Array.Empty<Il2CppSystem.Object>()));
                    }
                    void OpenServerDropdown()
                    {
                        dropdown?.gameObject.SetActive(true);
                        serverButton.SetActive(false);
                    }
                    void CloseServerDropdown()
                    {
                        serverButton.SetActive(true);
                    }
                    UpdateServer(SetCurrentServer());
                    dropdown.transform.localPosition = new(0.5348f, -1.2209f, -15f);
                    dropdown.transform.localScale = UnityEngine.Vector2.one;
                    dropdown.Initialize(new System.Action<string>(s => UpdateServer(s)), new System.Action(() => CloseServerDropdown()));
                    var button = EnterCodeSelectServer.transform.Find("ServerBox").GetComponent<PassiveButton>();
                    button.OnClick = new();
                    button.OnClick.AddListener((System.Action)(() =>
                    {
                        OpenServerDropdown();
                    }));
                    dropdown.transform.GetChild(0).GetComponent<PassiveButton>().enabled = false;

                }
            }
            catch { }
        }
    }
}
