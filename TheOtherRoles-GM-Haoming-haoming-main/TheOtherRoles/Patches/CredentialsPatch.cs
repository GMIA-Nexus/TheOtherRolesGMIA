using HarmonyLib;
using InnerNet;
using TMPro;
using UnityEngine;

namespace TheOtherRoles.Patches;

[HarmonyPatch]
public static class CredentialsPatch
{
    public static string baseCredentials =
        $@"<size=130%><color=#ff351f>TheOtherRoles GM H</color></size> v{TheOtherRolesPlugin.Version}";

    [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
    private static class PingTrackerPatch
    {
        private static void Postfix(PingTracker __instance)
        {
            AspectPosition position = __instance.GetComponent<AspectPosition>();
            if (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started)
            {
                __instance.text.alignment = TextAlignmentOptions.Top;
                position.Alignment = AspectPosition.EdgeAlignments.Top;
                __instance.text.text = $"{baseCredentials}\n{__instance.text.text}";
                position.DistanceFromEdge = new Vector3(1.5f, 0.11f, 0);
            }
            else
            {
                position.Alignment = AspectPosition.EdgeAlignments.LeftTop;
                __instance.text.alignment = TextAlignmentOptions.TopLeft;
                __instance.text.text =
                    $"{baseCredentials}\n{ModTranslation.getString("creditsFull")}\n{__instance.text.text}\nfangkuai.fun";
                position.DistanceFromEdge = new Vector3(0.5f, 0.11f);
            }
        }
    }

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public static class LogoPatch
    {
        static void Postfix(PingTracker __instance)
        {
            var torLogo = new GameObject("bannerLogo_TOR");
            torLogo.transform.SetParent(GameObject.Find("RightPanel").transform, false);
            torLogo.transform.localPosition = new Vector3(-0.4f, 1f, 5f);
            torLogo.AddComponent<SpriteRenderer>().sprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Banner.png", 300f);

            var credentialObject = new GameObject("credentialsTOR");
            var credentials = credentialObject.AddComponent<TextMeshPro>();
            var versionText = string.Format(ModTranslation.getString("creditsVersion"),
                TheOtherRolesPlugin.Version.ToString());
            credentials.SetText($"{versionText}\n{ModTranslation.getString("creditsMain")}\n{ModTranslation.getString("contributorsCredentials")}");
            credentials.alignment = TMPro.TextAlignmentOptions.Center;
            credentials.fontSize *= 0.05f;

            credentials.transform.SetParent(torLogo.transform);
            credentials.transform.localPosition = Vector3.down + new Vector3(0f, -0.6f, 0f);
        }
    }
}
