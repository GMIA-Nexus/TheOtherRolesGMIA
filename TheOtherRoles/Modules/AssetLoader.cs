using System.Collections.Generic;
using System.Reflection;
using Reactor.Utilities.Extensions;
using TheOtherRoles.Objects;
using UnityEngine;

namespace TheOtherRoles.Modules
{
    public static class AssetLoader
    {
        private static readonly Assembly dll = Assembly.GetExecutingAssembly();
        private static bool flag = false;
        static internal AssetBundle AssetBundle { get; private set; } = null!;

        public static Dictionary<string, AudioClip> AudioClips { get; private set; } = new();

        public static void LoadAssets()
        {
            if (flag) return;
            flag = true;

            try
            {
#if WINDOWS
                var AssetBundleStream = dll.GetManifestResourceStream("TheOtherRoles.Resources.AssetsBundle.assetsbundle-Windows");
#else
                var AssetBundleStream = dll.GetManifestResourceStream("TheOtherRoles.Resources.AssetsBundle.assetsbundle-Android");
#endif

                AssetBundle = AssetBundle.LoadFromMemory(AssetBundleStream.ReadFully());
                var allAssetNames = AssetBundle.GetAllAssetNames();
                foreach (var assetName in allAssetNames)
                {
                    var asset = AssetBundle.LoadAsset(assetName);
                    if (asset.TryCast<AudioClip>() is AudioClip audioClip) {
                        AudioClips.Add(assetName, AssetBundle.LoadAsset<AudioClip>(assetName).DontUnload());
                    }
                }

                Trap.activate = AssetBundle.LoadAsset<AudioClip>("TrapperActivate.ogg");
                Trap.countdown = AssetBundle.LoadAsset<AudioClip>("TrapperCountdown.ogg");
                Trap.disable = AssetBundle.LoadAsset<AudioClip>("TrapperDisable.ogg");
                Trap.kill = AssetBundle.LoadAsset<AudioClip>("TrapperKill.ogg");
                Trap.place = AssetBundle.LoadAsset<AudioClip>("TrapperPlace.ogg");

                FoxTask.prefab = AssetBundle.LoadAsset<GameObject>("FoxTask.prefab").DontUnload();
                Shrine.sprite = AssetBundle.LoadAsset<Sprite>("shrine2.png").DontUnload();
                Helpers.achievementMaterialShader = AssetBundle.LoadAsset<Shader>("Sprites-White.shader").DontUnload();
                Helpers.achievementProgressShader = AssetBundle.LoadAsset<Shader>("Sprites-Progress.shader").DontUnload();
            }
            catch
            {
            }
        }

        public static AudioClip GetAudioClip(string path)
        {
            if (!path.Contains("assets")) path = "assets/audio/" + path.ToLower() + ".ogg";
            return AudioClips.TryGetValue(path, out var clip) ? clip : null;
        }
    }
}
