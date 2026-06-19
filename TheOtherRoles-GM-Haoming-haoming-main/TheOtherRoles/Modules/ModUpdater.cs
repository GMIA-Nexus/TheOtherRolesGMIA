using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AmongUs.Data;
using Assets.InnerNet;
using BepInEx;
using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime.Attributes;
using TMPro;
using Twitch;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TheOtherRoles.Modules;

public class ModUpdater : MonoBehaviour
{
    public const string RepositoryOwner = "FangkuaiYa";
    public const string RepositoryName = "TheOtherRoles-GM-Haoming";

    private bool _busy;
    public List<GithubRelease> Releases;
    private bool showPopUp = true;

    public ModUpdater(IntPtr ptr) : base(ptr)
    {
    }

    public static ModUpdater Instance { get; private set; }

    public void Awake()
    {
        if (Instance) Destroy(Instance);
        Instance = this;
        foreach (string file in Directory.GetFiles(Paths.PluginPath, "*.old")) File.Delete(file);
    }

    private void Start()
    {
        if (_busy) return;
        this.StartCoroutine(CoCheckForUpdate());
        SceneManager.add_sceneLoaded((Action<Scene, LoadSceneMode>)OnSceneLoaded);
    }


    [HideFromIl2Cpp]
    public void StartDownloadRelease(GithubRelease release)
    {
        if (_busy) return;
        this.StartCoroutine(CoDownloadRelease(release));
    }

    [HideFromIl2Cpp]
    private IEnumerator CoCheckForUpdate()
    {
        _busy = true;
        UnityWebRequest www = new();
        www.SetMethod(UnityWebRequest.UnityWebRequestMethod.Get);
        www.SetUrl($"https://api.github.com/repos/{RepositoryOwner}/{RepositoryName}/releases");
        www.downloadHandler = new DownloadHandlerBuffer();
        UnityWebRequestAsyncOperation operation = www.SendWebRequest();

        while (!operation.isDone) yield return new WaitForEndOfFrame();

        if (www.isNetworkError || www.isHttpError) yield break;

        Releases = JsonSerializer.Deserialize<List<GithubRelease>>(www.downloadHandler.text);
        www.downloadHandler.Dispose();
        www.Dispose();
        Releases.Sort(SortReleases);
        _busy = false;
    }

    [HideFromIl2Cpp]
    private IEnumerator CoDownloadRelease(GithubRelease release)
    {
        _busy = true;

        GenericPopup popup = Instantiate(TwitchManager.Instance.TwitchPopup);
        popup.TextAreaTMP.fontSize *= 0.7f;
        popup.TextAreaTMP.enableAutoSizing = false;

        popup.Show();

        GameObject button = popup.transform.GetChild(2).gameObject;
        button.SetActive(false);
        popup.TextAreaTMP.text = ModTranslation.getString("updatePleaseWait");

        GithubAsset asset = release.Assets.Find(FilterPluginAsset);
        UnityWebRequest www = new();
        www.SetMethod(UnityWebRequest.UnityWebRequestMethod.Get);
        www.SetUrl(Helpers.isChinese()
            ? "https://dl.fangkuai.fun/ModFiles/TheOtherRoles-GM-Haoming/TheOtherRolesGM.dll"
            : asset.DownloadUrl);
        www.downloadHandler = new DownloadHandlerBuffer();
        UnityWebRequestAsyncOperation operation = www.SendWebRequest();

        while (!operation.isDone)
        {
            int stars = Mathf.CeilToInt(www.downloadProgress * 10);
            string progress = string.Format(ModTranslation.getString("updateInDownloading"),
                new string((char)0x25A0, stars) + new string((char)0x25A1, 10 - stars));
            popup.TextAreaTMP.text = progress;
            yield return new WaitForEndOfFrame();
        }

        if (www.isNetworkError || www.isHttpError)
        {
            popup.TextAreaTMP.text = ModTranslation.getString("updateFailed");
            yield break;
        }

        popup.TextAreaTMP.text = ModTranslation.getString("updateCopying");

        string filePath = Path.Combine(Paths.PluginPath, asset.Name);

        if (File.Exists(filePath + ".old")) File.Delete(filePath + "old");
        if (File.Exists(filePath)) File.Move(filePath, filePath + ".old");

        Task persistTask = File.WriteAllBytesAsync(filePath, www.downloadHandler.data);
        bool hasError = false;
        while (!persistTask.IsCompleted)
        {
            if (persistTask.Exception != null)
            {
                hasError = true;
                break;
            }

            yield return new WaitForEndOfFrame();
        }

        www.downloadHandler.Dispose();
        www.Dispose();

        if (!hasError) popup.TextAreaTMP.text = ModTranslation.getString("updateRestart");
        button.SetActive(true);
        _busy = false;
    }

    [HideFromIl2Cpp]
    private static bool FilterLatestRelease(GithubRelease release)
    {
        return release.IsNewer(TheOtherRolesPlugin.Version) && release.Assets.Any(FilterPluginAsset);
    }

    [HideFromIl2Cpp]
    private static bool FilterPluginAsset(GithubAsset asset)
    {
        return asset.Name == "TheOtherRolesGM.dll";
    }

    [HideFromIl2Cpp]
    private static int SortReleases(GithubRelease a, GithubRelease b)
    {
        if (a.IsNewer(b.Version)) return -1;
        if (b.IsNewer(a.Version)) return 1;
        return 0;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_busy || scene.name != "MainMenu") return;
        GithubRelease latestRelease = Releases.FirstOrDefault();
        if (latestRelease == null || latestRelease.Version <= TheOtherRolesPlugin.Version)
            return;

        GameObject template = GameObject.Find("ExitGameButton");
        if (!template) return;

        GameObject button = Instantiate(template, null);
        Transform buttonTransform = button.transform;
        //buttonTransform.localPosition = new Vector3(-2f, -2f);
        button.GetComponent<AspectPosition>().anchorPoint = new Vector2(0.458f, 0.124f);

        PassiveButton passiveButton = button.GetComponent<PassiveButton>();
        passiveButton.OnClick = new Button.ButtonClickedEvent();
        passiveButton.OnClick.AddListener((Action)(() =>
        {
            StartDownloadRelease(latestRelease);
            button.SetActive(false);
        }));

        TMP_Text text = button.transform.GetComponentInChildren<TMP_Text>();
        string t = ModTranslation.getString("updateButton");
        StartCoroutine(Effects.Lerp(0.1f, (Action<float>)(p => text.SetText(t))));
        passiveButton.OnMouseOut.AddListener((Action)(() => text.color = Color.red));
        passiveButton.OnMouseOver.AddListener((Action)(() => text.color = Color.white));
        string announcement = string.Format(ModTranslation.getString("announcementUpdate"), latestRelease.Tag,
            latestRelease.Description);
        MainMenuManager mgr = FindObjectOfType<MainMenuManager>(true);
        if (showPopUp)
            mgr.StartCoroutine(CoShowAnnouncement(announcement, shortTitle: "TOR GMH Update",
                date: latestRelease.PublishedAt));
        showPopUp = false;
    }

    [HideFromIl2Cpp]
    public IEnumerator CoShowAnnouncement(string announcement, bool show = true, string shortTitle = "TOR GMH Update",
        string title = "", string date = "")
    {
        MainMenuManager mgr = FindObjectOfType<MainMenuManager>(true);
        AnnouncementPopUp popUpTemplate = FindObjectOfType<AnnouncementPopUp>(true);
        if (popUpTemplate == null)
        {
            TheOtherRolesPlugin.Logger.LogError("couldnt show credits, popUp is null");
            yield return null;
        }

        AnnouncementPopUp popUp = Instantiate(popUpTemplate);

        popUp.gameObject.SetActive(true);

        Announcement creditsAnnouncement = new()
        {
            Id = "torgmhAnnouncement",
            Language = 0,
            Number = 6969,
            Title = title == "" ? "The Other Roles GM Haoming Announcement" : title,
            ShortTitle = shortTitle,
            SubTitle = "",
            PinState = false,
            Date = date == "" ? DateTime.Now.Date.ToString() : date,
            Text = announcement
        };
        mgr.StartCoroutine(Effects.Lerp(0.1f, new Action<float>(p =>
        {
            if (p == 1)
            {
                Il2CppSystem.Collections.Generic.List<Announcement> backup =
                    DataManager.Player.Announcements.allAnnouncements;
                DataManager.Player.Announcements.allAnnouncements =
                    new Il2CppSystem.Collections.Generic.List<Announcement>();
                popUp.Init(false);
                DataManager.Player.Announcements.SetAnnouncements(new[] { creditsAnnouncement });
                popUp.CreateAnnouncementList();
                popUp.UpdateAnnouncementText(creditsAnnouncement.Number);
                popUp.visibleAnnouncements[0].PassiveButton.OnClick.RemoveAllListeners();
                DataManager.Player.Announcements.allAnnouncements = backup;
            }
        })));
    }
}

public class GithubRelease
{
    [JsonPropertyName("id")] public int Id { get; set; }

    [JsonPropertyName("tag_name")] public string Tag { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("draft")] public bool Draft { get; set; }

    [JsonPropertyName("prerelease")] public bool Prerelease { get; set; }

    [JsonPropertyName("created_at")] public string CreatedAt { get; set; }

    [JsonPropertyName("published_at")] public string PublishedAt { get; set; }

    [JsonPropertyName("body")] public string Description { get; set; }

    [JsonPropertyName("assets")] public List<GithubAsset> Assets { get; set; }

    public Version Version => Version.Parse(Tag.Replace("v", string.Empty));

    public bool IsNewer(Version version)
    {
        return Version > version;
    }
}

public class GithubAsset
{
    [JsonPropertyName("url")] public string Url { get; set; }

    [JsonPropertyName("id")] public int Id { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("size")] public int Size { get; set; }

    [JsonPropertyName("browser_download_url")]
    public string DownloadUrl { get; set; }
}
