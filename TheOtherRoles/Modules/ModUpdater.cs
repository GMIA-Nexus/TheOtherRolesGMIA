using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using AmongUs.Data;
using Assets.InnerNet;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Unity.IL2CPP;
using BepInEx.Unity.IL2CPP.Utils;
using Mono.Cecil;
using Newtonsoft.Json.Linq;
using TMPro;
using Twitch;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Action = System.Action;
using IntPtr = System.IntPtr;
using Version = SemanticVersioning.Version;

namespace TheOtherRoles.Modules
{
    public class ModUpdateBehaviour : MonoBehaviour
    {
        public static bool showPopUp = true;
        public static bool updateInProgress = false;

        public static ModUpdateBehaviour Instance { get; private set; }
        public ModUpdateBehaviour(IntPtr ptr) : base(ptr) { }
        public class UpdateData
        {
            public string Content { get { return DataManager.Settings.Language.CurrentLanguage == SupportedLangs.SChinese ? ContentZh : ContentDefault; } }

            string ContentDefault;
            string ContentZh;
            public string Tag;
            public string TimeString;
            public JObject Request;
            public Version Version => Version.Parse(Tag);

            public UpdateData(JObject data)
            {
                Tag = data["tag_name"]?.ToString().TrimStart('v');
                TimeString = DateTime.FromBinary(((Il2CppSystem.DateTime)data["published_at"]).ToBinaryRaw()).ToString();
                Request = data;

                ContentDefault = data["body"]?.ToString();
                var content = ContentDefault;
                int zhTagStartIndex = content.IndexOf("### ZH");
                int zhTagEndIndex = zhTagStartIndex != -1 ? zhTagStartIndex + "### ZH".Length + 2 : -1;
                int tagStartIndex = content.IndexOf("### EN");
                int tagEndIndex = tagStartIndex != -1 ? tagStartIndex + "### EN".Length + 2 : -1;

                if (zhTagStartIndex == -1 && tagStartIndex == -1)
                {
                    ContentDefault = ContentZh = content;
                }
                else if (zhTagStartIndex != -1 && tagStartIndex == -1)
                {
                    ContentDefault = ContentZh = content[tagEndIndex..];
                }
                else if (zhTagStartIndex == -1 && tagStartIndex != -1)
                {
                    ContentDefault = ContentZh = content[zhTagEndIndex..];
                }
                else
                {
                    if (zhTagStartIndex < tagEndIndex)
                    {
                        ContentZh = content[zhTagEndIndex..tagStartIndex];
                        ContentDefault = content[tagEndIndex..];
                    }
                    else
                    {
                        ContentDefault = content[tagEndIndex..zhTagStartIndex];
                        ContentZh = content[zhTagEndIndex..];
                    }
                }
            }

            public bool IsNewer(Version version)
            {
                if (!Version.TryParse(Tag, out var myVersion)) return false;
                return myVersion.BaseVersion() > version.BaseVersion();
            }
        }

        public UpdateData TORUpdate;

        [HideFromIl2Cpp]
        public UpdateData RequiredUpdateData => TORUpdate;

        public void Awake()
        {
            if (Instance) Destroy(this);
            Instance = this;

            SceneManager.add_sceneLoaded((System.Action<Scene, LoadSceneMode>)(OnSceneLoaded));
            this.StartCoroutine(CoCheckUpdates());

            foreach (var file in Directory.GetFiles(Paths.PluginPath, "*.old"))
            {
                File.Delete(file);
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (updateInProgress || scene.name != "MainMenu") return;
            if (RequiredUpdateData is null)
            {
                showPopUp = false;
                return;
            }

            var template = GameObject.Find("ExitGameButton");
            if (!template) return;

            var button = Instantiate(template, null);
            var buttonTransform = button.transform;
            //buttonTransform.localPosition = new Vector3(-2f, -2f);
            button.GetComponent<AspectPosition>().anchorPoint = new Vector2(0.458f, 0.124f);

            PassiveButton passiveButton = button.GetComponent<PassiveButton>();
            passiveButton.OnClick = new Button.ButtonClickedEvent();
            passiveButton.OnClick.AddListener((Action)(() =>
            {
                this.StartCoroutine(CoUpdate());
                button.SetActive(false);
            }));

            var text = button.transform.GetComponentInChildren<TMPro.TMP_Text>();
            string t = ModTranslation.getString("updateGMIA");

            StartCoroutine(Effects.Lerp(0.1f, (System.Action<float>)(p => text.SetText(t))));
            passiveButton.OnMouseOut.AddListener((Action)(() => text.color = Color.red));
            passiveButton.OnMouseOver.AddListener((Action)(() => text.color = Color.white));

            var announcement = $"<size=150%>{string.Format(ModTranslation.getString("updateText"), TORUpdate.Tag)}</size>\n{TORUpdate.Content}";
            var mgr = FindObjectOfType<MainMenuManager>(true);

            try
            {
                string updateVersion = TORUpdate.Content[^5..];
                if (Version.Parse(TheOtherRolesPlugin.VersionString).BaseVersion() < Version.Parse(updateVersion).BaseVersion())
                {
                    passiveButton.OnClick.RemoveAllListeners();
                    passiveButton.OnClick = new Button.ButtonClickedEvent();
                    passiveButton.OnClick.AddListener((Action)(() => {
                        mgr.StartCoroutine(CoShowAnnouncement($"<size=150%>{ModTranslation.getString("updateManual")}</size>"));
                    }));
                }
            }
            catch
            {
                TheOtherRolesPlugin.Logger.LogError("parsing version for auto updater failed :(");
            }
            if (showPopUp) mgr.StartCoroutine(CoShowAnnouncement(announcement, shortTitle: "GMIA Update", date: TORUpdate.TimeString));
            showPopUp = false;
        }

        [HideFromIl2Cpp]
        public IEnumerator CoUpdate()
        {
            updateInProgress = true;
            var updateName = ModTranslation.getString("GMIA");

            var popup = Instantiate(TwitchManager.Instance.TwitchPopup);
            popup.TextAreaTMP.fontSize *= 0.7f;
            popup.TextAreaTMP.enableAutoSizing = false;

            popup.Show();

            var button = popup.transform.GetChild(2).gameObject;
            button.SetActive(false);
            popup.TextAreaTMP.text = string.Format(ModTranslation.getString("updatingGMIA"), updateName);

            var download = Task.Run(DownloadUpdate);
            while (!download.IsCompleted) yield return null;

            button.SetActive(true);
            popup.TextAreaTMP.text = download.Result ? string.Format(ModTranslation.getString("updateSuccessful"), updateName) : ModTranslation.getString("updateNotSuccessful");
        }


        private static int announcementNumber = 501;
        [HideFromIl2Cpp]
        public IEnumerator CoShowAnnouncement(string announcement, bool show = true, string shortTitle = "GMIA Update", string title = "", string date = "")
        {
            var mgr = FindObjectOfType<MainMenuManager>(true);
            var popUpTemplate = UnityEngine.Object.FindObjectOfType<AnnouncementPopUp>(true);
            if (popUpTemplate == null)
            {
                TheOtherRolesPlugin.Logger.LogError("couldnt show credits, popUp is null");
                yield return null;
            }
            var popUp = UnityEngine.Object.Instantiate(popUpTemplate);

            popUp.gameObject.SetActive(true);

            Assets.InnerNet.Announcement creditsAnnouncement = new()
            {
                Id = "torAnnouncement",
                Language = 0,
                Number = announcementNumber++,
                Title = title == "" ? "The Other Roles Announcement" : title,
                ShortTitle = shortTitle,
                SubTitle = "",
                PinState = false,
                Date = date == "" ? DateTime.Now.Date.ToString() : date,
                Text = announcement,
            };
            mgr.StartCoroutine(Effects.Lerp(0.1f, new Action<float>((p) => {
                if (p == 1)
                {
                    var backup = DataManager.Player.Announcements.allAnnouncements;
                    DataManager.Player.Announcements.allAnnouncements = new();
                    popUp.Init(false);
                    DataManager.Player.Announcements.SetAnnouncements(new Announcement[] { creditsAnnouncement });
                    popUp.CreateAnnouncementList();
                    popUp.UpdateAnnouncementText(creditsAnnouncement.Number);
                    popUp.visibleAnnouncements[0].PassiveButton.OnClick.RemoveAllListeners();
                    DataManager.Player.Announcements.allAnnouncements = backup;
                }
            })));
        }

        [HideFromIl2Cpp]
        public static IEnumerator CoCheckUpdates()
        {
            var torUpdateCheck = Task.Run(() => Instance.GetGithubUpdate("GMIA-Nexus", "TheOtherRolesGMIA"));
            while (!torUpdateCheck.IsCompleted) yield return null;
            if (torUpdateCheck.Result != null && torUpdateCheck.Result.IsNewer(Version.Parse(TheOtherRolesPlugin.VersionString)))
            {
                Instance.TORUpdate = torUpdateCheck.Result;
            }
            Instance.OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        [HideFromIl2Cpp]
        public async Task<UpdateData> GetGithubUpdate(string owner, string repo)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "TheOtherRoles Updater");

            try
            {
                var req = await client.GetAsync($"https://api.github.com/repos/{owner}/{repo}/releases/latest", HttpCompletionOption.ResponseContentRead);

                if (!req.IsSuccessStatusCode) return null;

                var dataString = await req.Content.ReadAsStringAsync();
                JObject data = JObject.Parse(dataString);
                return new UpdateData(data);
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        [HideFromIl2Cpp]
        public async Task<bool> DownloadUpdate()
        {
            var data = TORUpdate;

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "TheOtherRoles Updater");

            JToken assets = data.Request["assets"];
            string downloadURI = "";
                for (JToken current = assets.First; current != null; current = current.Next)
                {
                    string browser_download_url = current["browser_download_url"]?.ToString();
                    if (browser_download_url != null && current["content_type"] != null)
                    {
                        if (current["content_type"].ToString().Equals("application/x-msdownload") &&
                            browser_download_url.EndsWith(".dll"))
                        {
                            downloadURI = (Helpers.isChinese() ? "https://cdn.gh-proxy.org/" : "") + browser_download_url;
                            break;
                        }
                    }
                }

            if (downloadURI.Length == 0) return false;

            var res = await client.GetAsync(downloadURI, HttpCompletionOption.ResponseContentRead);
            string filePath = Path.Combine(Paths.PluginPath, "TheOtherRoles.dll");
            if (File.Exists(filePath + ".old")) File.Delete(filePath + ".old");
            if (File.Exists(filePath)) File.Move(filePath, filePath + ".old");

            await using var responseStream = await res.Content.ReadAsStreamAsync();
            await using var fileStream = File.Create(filePath);
            await responseStream.CopyToAsync(fileStream);

            return true;
        }
    }
}
