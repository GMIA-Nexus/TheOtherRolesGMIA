using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Reflection;

namespace TheOtherRoles.Modules.CustomHats
{
    public static class CustomHatManager
    {
        public const string ResourcesDirectory = "TheOtherHats";
        public const string InnerslothPackageName = "Innersloth Hats";
        public const string DeveloperPackageName = "Developer Hats";

        internal static readonly Tuple<string, string> Repository = new("GMIA-Nexus", "TheOtherHats");
        internal static string RepositoryUrl
        {
            get
            {
                var (owner, repository) = Repository;
                return (Helpers.isChinese() ? "https://cdn.gh-proxy.org/" : "") + $"https://raw.githubusercontent.com/{owner}/{repository}/master";
            }
        }

        internal static readonly string ManifestFileName = "CustomHats.json";

#if WINDOWS
        internal static string CustomSkinsDirectory => Path.Combine(Path.GetDirectoryName(Application.dataPath)!, ResourcesDirectory);
#else
        internal static string CustomSkinsDirectory => Path.Combine(Application.persistentDataPath, ResourcesDirectory);
#endif
        internal static string HatsDirectory => CustomSkinsDirectory;

        internal static List<CustomHat> UnregisteredHats = new();
        internal static readonly Dictionary<string, HatViewData> ViewDataCache = new();
        public static readonly Dictionary<string, bool> AdaptiveCache = new();

        private static readonly HatsLoader Loader;
        public static bool HatsLoaded { get; private set; }

        static CustomHatManager()
        {
            Loader = TheOtherRolesPlugin.Instance.AddComponent<HatsLoader>();
        }

        internal static void LoadHats()
        {
            Loader.FetchHats();
        }

        public static void RegisterAllHats()
        {
            if (HatsLoaded) return;

            if (DestroyableSingleton<HatManager>.Instance == null)
            {
                TheOtherRolesPlugin.Logger.LogError("HatManager instance is null, cannot register hats.");
                return;
            }

            var currentHats = DestroyableSingleton<HatManager>.Instance.allHats;
            if (currentHats == null)
            {
                TheOtherRolesPlugin.Logger.LogError("HatManager.allHats is null, cannot register hats.");
                return;
            }

            var allHats = currentHats.ToList();
            var toRegister = new List<HatData>();

            foreach (var hat in UnregisteredHats)
            {
                try
                {
                    var hatData = CreateHatBehaviour(hat);
                    if (hatData != null)
                    {
                        toRegister.Add(hatData);
                    }
                }
                catch (Exception ex)
                {
                    TheOtherRolesPlugin.Logger.LogError($"Failed to create hat {hat.Name}: {ex.Message}");
                }
            }

            allHats.AddRange(toRegister);
            DestroyableSingleton<HatManager>.Instance.allHats = allHats.ToArray();
            UnregisteredHats.Clear();
            HatsLoaded = true;
        }

        internal static HatData CreateHatBehaviour(CustomHat ch, bool testOnly = false)
        {
            var viewData = ScriptableObject.CreateInstance<HatViewData>();
            var hat = ScriptableObject.CreateInstance<HatData>();

            var resourcePath = ch.Resource;

            var sprite = CreateHatSprite(resourcePath);
            if (sprite == null)
                throw new FileNotFoundException($"Hat image not found: {resourcePath}");

            viewData.MainImage = sprite;
            hat.name = ch.Name;
            hat.displayOrder = 99;
            hat.ProductId = "hat_" + ch.Name.Replace(' ', '_');
            hat.InFront = true;
            hat.NoBounce = true;
            hat.ChipOffset = new Vector2(0f, 0.2f);
            hat.Free = true;
            hat.StoreName = ch.Author ?? "Unknown";

            AdaptiveCache[hat.name] = ch.Adaptive;

            ViewDataCache[hat.name] = viewData;
            hat.ViewDataRef = new AssetReference(viewData.Pointer);
            return hat;
        }

        private static Sprite CreateHatSprite(string path)
        {
            var fullPath = Path.Combine(HatsDirectory, path);
            if (!File.Exists(fullPath))
            {
                TheOtherRolesPlugin.Logger.LogWarning($"File not found on disk, trying resources: {fullPath}");
                var texture = Helpers.loadTextureFromResources(path);
                if (texture == null) return null;
                var sprite = Sprite.Create(texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.53f, 0.575f),
                    texture.width * 0.375f);
                texture.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;
                sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;
                return sprite;
            }

            var textureFromDisk = Helpers.loadTextureFromDisk(fullPath);
            if (textureFromDisk == null) return null;

            var spriteFromDisk = Sprite.Create(textureFromDisk,
                new Rect(0, 0, textureFromDisk.width, textureFromDisk.height),
                new Vector2(0.53f, 0.575f),
                textureFromDisk.width * 0.375f);
            if (spriteFromDisk == null) return null;

            textureFromDisk.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;
            spriteFromDisk.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;
            return spriteFromDisk;
        }

        public static List<CustomHat> CreateHatDetailsFromFileNames(string[] fileNames, bool fromDisk = false)
        {
            var fronts = new Dictionary<string, CustomHat>();
            var backs = new Dictionary<string, string>();
            var flips = new Dictionary<string, string>();
            var backFlips = new Dictionary<string, string>();
            var climbs = new Dictionary<string, string>();

            foreach (var fileName in fileNames)
            {
                var index = fileName.LastIndexOf("\\", StringComparison.InvariantCulture) + 1;
                var s = fromDisk ? fileName[index..].Split('.')[0] : fileName.Split('.')[3];
                var p = s.Split('_');
                var options = new HashSet<string>(p);
                if (options.Contains("back") && options.Contains("flip"))
                {
                    backFlips[p[0]] = fileName;
                }
                else if (options.Contains("climb"))
                {
                    climbs[p[0]] = fileName;
                }
                else if (options.Contains("back"))
                {
                    backs[p[0]] = fileName;
                }
                else if (options.Contains("flip"))
                {
                    flips[p[0]] = fileName;
                }
                else
                {
                    fronts[p[0]] = new CustomHat
                    {
                        Resource = fileName,
                        Name = p[0].Replace('-', ' '),
                        Bounce = options.Contains("bounce"),
                        Adaptive = options.Contains("adaptive"),
                        Behind = options.Contains("behind"),
                    };
                }
            }

            var hats = new List<CustomHat>();

            foreach (var frontKvP in fronts)
            {
                var k = frontKvP.Key;
                var hat = frontKvP.Value;
                backs.TryGetValue(k, out var backResource);
                climbs.TryGetValue(k, out var climbResource);
                flips.TryGetValue(k, out var flipResource);
                backFlips.TryGetValue(k, out var backFlipResource);
                if (backResource != null) hat.BackResource = backResource;
                if (climbResource != null) hat.ClimbResource = climbResource;
                if (flipResource != null) hat.FlipResource = flipResource;
                if (backFlipResource != null) hat.BackFlipResource = backFlipResource;
                if (hat.BackResource != null) hat.Behind = true;
                hats.Add(hat);
            }

            return hats;
        }

        internal static List<CustomHat> SanitizeHats(SkinsConfigFile response)
        {
            foreach (var hat in response.Hats)
            {
                hat.Resource = SanitizeFileName(hat.Resource);
                hat.BackResource = SanitizeFileName(hat.BackResource);
                hat.ClimbResource = SanitizeFileName(hat.ClimbResource);
                hat.FlipResource = SanitizeFileName(hat.FlipResource);
                hat.BackFlipResource = SanitizeFileName(hat.BackFlipResource);
            }
            return response.Hats;
        }

        private static string SanitizeFileName(string path)
        {
            if (path == null || !path.EndsWith(".png")) return null;
            return path.Replace("\\", "")
                .Replace("/", "")
                .Replace("*", "")
                .Replace("..", "");
        }

        private static bool ResourceRequireDownload(string resFile, string resHash, HashAlgorithm algorithm)
        {
            var filePath = Path.Combine(HatsDirectory, resFile);
            if (resHash == null || !File.Exists(filePath))
                return true;

            using var stream = File.OpenRead(filePath);
            var hash = BitConverter.ToString(algorithm.ComputeHash(stream))
                .Replace("-", string.Empty)
                .ToLowerInvariant();
            return !resHash.Equals(hash);
        }

        internal static List<string> GenerateDownloadList(List<CustomHat> hats)
        {
            var algorithm = MD5.Create();
            var toDownload = new List<string>();

            foreach (var hat in hats)
            {
                var files = new List<Tuple<string, string>>
                {
                    new(hat.Resource, hat.ResHashA),
                    new(hat.BackResource, hat.ResHashB),
                    new(hat.ClimbResource, hat.ResHashC),
                    new(hat.FlipResource, hat.ResHashF),
                    new(hat.BackFlipResource, hat.ResHashBf)
                };
                foreach (var (fileName, fileHash) in files)
                {
                    if (fileName != null && ResourceRequireDownload(fileName, fileHash, algorithm))
                    {
                        toDownload.Add(fileName);
                    }
                }
            }
            return toDownload;
        }

        public static List<CustomHat> loadHorseHats()
        {
            // 愚人节帽子逻辑保持不变
            List<CustomHat> hatdatas = new();
            Assembly assembly = Assembly.GetExecutingAssembly();
            string[] resourceNames = assembly.GetManifestResourceNames();
            List<string> hatFiles = new();
            Dictionary<string, List<string>> hatFilesSorted = new();
            foreach (string resourceName in resourceNames)
            {
                if (resourceName.Contains("TheOtherRoles.Resources.HorseHats.") && resourceName.Contains(".png"))
                {
                    hatFiles.Add(resourceName);
                }
            }

            foreach (string s in hatFiles)
            {
                string value = s.Substring(0, s.LastIndexOf("HorseSpecialHat") + 17);
                if (value.Contains(".")) value.Remove(value.LastIndexOf("."));
                if (!hatFilesSorted.ContainsKey(value)) hatFilesSorted.Add(value, new List<string>());
                hatFilesSorted[value].Add(s);
            }
            int i = 0;
            foreach (var item in hatFilesSorted)
            {
                CustomHat info = new()
                {
                    Name = $"April Hat {i++:D2}",
                    Author = "A Fool",
                    Resource = item.Value.FirstOrDefault(x => !x.Contains("back")),
                    BackResource = item.Value.FirstOrDefault(x => x.Contains("back"))
                };
                info.Adaptive = info.Resource != null && info.Resource.Contains("adaptive");
                info.FlipResource = item.Value.FirstOrDefault(x => x.Contains("flip"));
                info.ClimbResource = item.Value.FirstOrDefault(x => x.Contains("climb"));
                info.Package = "April Fools Hats";
                if (info.Resource == null || info.Name == null)
                    continue;
                hatdatas.Add(info);
            }
            return hatdatas;
        }
    }
}
