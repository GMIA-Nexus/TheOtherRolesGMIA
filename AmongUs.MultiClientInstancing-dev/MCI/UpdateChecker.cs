using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System;

namespace MCI;

public static class UpdateChecker
{
    public static bool NeedsUpdate;

    public static async void CheckForUpdate()
    {
        try
        {
            NeedsUpdate = await TaskUpdate();
        } catch {}
    }

    public static async Task<bool> TaskUpdate()
    {
        var http = new HttpClient();
        http.DefaultRequestHeaders.Add("User-Agent", "MCI-Agent");
        var response = await http.GetAsync("https://api.github.com/repos/MyDragonBreath/AmongUs.MultiClientInstancing/releases/latest", HttpCompletionOption.ResponseContentRead);

        if (response.StatusCode != HttpStatusCode.OK || response.Content == null)
            return false;

        var json = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<GitHubApiObject>(json);
        var tagname = data.tag_name;

        if (tagname == null)
            return false;

        return MCIPlugin.vVersion.CompareTo(Version.Parse(tagname.Replace("v", ""))) < 0;
    }

    private class GitHubApiObject
    {
        [JsonPropertyName("tag_name")]
        public string tag_name { get; set; }
        [JsonPropertyName("assets")]
        public GitHubApiAsset[] assets { get; set; }
    }

    private class GitHubApiAsset
    {
        [JsonPropertyName("browser_download_url")]
        public string browser_download_url { get; set; }
    }
}
