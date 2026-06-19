using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AmongUs.Data;
using AmongUs.Data.Player;
using AmongUs.GameOptions;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Reactor;
using Reactor.Networking.Attributes;
using TheOtherRoles.Modules;
using TheOtherRoles.Modules.CustomHats;
using TheOtherRoles.Objects;
using TheOtherRoles.Patches;
using UnityEngine;


namespace TheOtherRoles
{
    [BepInPlugin(Id, "The Other Roles GM", VersionString)]
    [BepInDependency(ReactorPlugin.Id)]
    [BepInDependency(SubmergedCompatibility.SUBMERGED_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [ReactorModFlags(Reactor.Networking.ModFlags.RequireOnAllClients)]
    //[BepInProcess("Among Us.exe")]
    public class TheOtherRolesPlugin : BasePlugin
    {
        public const string Id = "me.eisbison.theotherroles";

        public const string VersionString = "2.3.140";

        public static System.Version Version = System.Version.Parse(VersionString);
        internal static BepInEx.Logging.ManualLogSource Logger;

        public Harmony Harmony { get; } = new Harmony(Id);
        public static TheOtherRolesPlugin Instance;

        public static int optionsPage = 0;

        public static ConfigEntry<bool> DebugMode { get; private set; }
        public static ConfigEntry<bool> StreamerMode { get; set; }
        public static ConfigEntry<bool> GhostsSeeTasks { get; set; }
        public static ConfigEntry<bool> GhostsSeeRoles { get; set; }
        public static ConfigEntry<bool> GhostsSeeVotes { get; set; }
        public static ConfigEntry<bool> ShowRoleSummary { get; set; }
        public static ConfigEntry<bool> HideNameplates { get; set; }
        public static ConfigEntry<bool> ShowLighterDarker { get; set; }
        public static ConfigEntry<bool> ShowChatNotifications { get; set; }
        public static ConfigEntry<bool> HideTaskArrows { get; set; }
        public static ConfigEntry<bool> OfflineHats { get; set; }
        public static ConfigEntry<bool> HideFakeTasks { get; set; }
        public static ConfigEntry<bool> BetterSabotageMap { get; set; }
        public static ConfigEntry<bool> ForceNormalSabotageMap { get; set; }
        public static ConfigEntry<string> StreamerModeReplacementText { get; set; }
        public static ConfigEntry<string> StreamerModeReplacementColor { get; set; }
        public static ConfigEntry<string> Ip { get; set; }
        public static ConfigEntry<ushort> Port { get; set; }
        public static ConfigEntry<string> DebugRepo { get; private set; }
        public static ConfigEntry<string> ShowPopUpVersion { get; set; }
        public static ConfigEntry<string> WebhookUrl { get; set; }
        public static ConfigEntry<bool> TransparentMap { get; set; }

        public static Assembly JsonNet;
        public static IRegionInfo[] defaultRegions;
        public static void UpdateRegions()
        {
            ServerManager serverManager = FastDestroyableSingleton<ServerManager>.Instance;
            IRegionInfo[] regions = new[]
            {
                new StaticHttpRegionInfo("fangkuai-server", StringNames.NoTranslation, "https://player.fangkuai.fun", new Il2CppReferenceArray<ServerInfo>(new ServerInfo[1] { new("fangkuai-server", "https://player.fangkuai.fun", 443, false) })).CastFast<IRegionInfo>(),
                new StaticHttpRegionInfo("Custom", StringNames.NoTranslation, Ip.Value, new Il2CppReferenceArray<ServerInfo>(new ServerInfo[1] { new("Custom", Ip.Value, Port.Value, false) })).CastFast<IRegionInfo>()
            };
            IRegionInfo currentRegion = serverManager.CurrentRegion;
            foreach (IRegionInfo region in regions)
                if (region == null)
                    Logger.LogError("Could not add region");
                else
                {
                    if (currentRegion != null && region.Name.Equals(currentRegion.Name, StringComparison.OrdinalIgnoreCase))
                        currentRegion = region;
                    serverManager.AddOrUpdateRegion(region);
                }

            // AU remembers the previous region that was set, so we need to restore it
            if (currentRegion != null)
            {
                Logger.LogDebug("Resetting previous region");
                serverManager.SetRegion(currentRegion);
            }
        }
        public override void Load()
        {
            ModTranslation.Load();
            Patches.Logger.SetLogSource(Log);
            Logger = Log;

            DebugMode = Config.Bind("Custom", "Enable Debug Mode", false);
            StreamerMode = Config.Bind("Custom", "Enable Streamer Mode", false);
            GhostsSeeTasks = Config.Bind("Custom", "Ghosts See Remaining Tasks", true);
            GhostsSeeRoles = Config.Bind("Custom", "Ghosts See Roles", true);
            GhostsSeeVotes = Config.Bind("Custom", "Ghosts See Votes", true);
            ShowRoleSummary = Config.Bind("Custom", "Show Role Summary", true);
            HideNameplates = Config.Bind("Custom", "Hide Nameplates", false);
            ShowLighterDarker = Config.Bind("Custom", "Show Lighter / Darker", false);
            ShowChatNotifications = Config.Bind("Custom", "Show Chat Notifications", true);
            HideTaskArrows = Config.Bind("Custom", "Hide Task Arrows", false);
            OfflineHats = Config.Bind("Custom", "Offline Hats", false);
            HideFakeTasks = Config.Bind("Custom", "Hide Fake Tasks", false);
            BetterSabotageMap = Config.Bind("Custom", "BetterSabotageMap", false);
            ForceNormalSabotageMap = Config.Bind("Custom", "ForceNormalSabotageMap", false);
            ShowPopUpVersion = Config.Bind("Custom", "Show PopUp", "0");
            StreamerModeReplacementText = Config.Bind("Custom", "Streamer Mode Replacement Text", "\n\nThe Other Roles GM");
            StreamerModeReplacementColor = Config.Bind("Custom", "Streamer Mode Replacement Text Hex Color", "#87AAF5FF");
            DebugRepo = Config.Bind("Custom", "Debug Hat Repo", "");
            WebhookUrl = Config.Bind("Custom", "WebhookUrl", "");
            TransparentMap = Config.Bind("Custom", "TransparentMap", false);

            Ip = Config.Bind("Custom", "Custom Server IP", "127.0.0.1");
            Port = Config.Bind("Custom", "Custom Server Port", (ushort)22023);
            defaultRegions = ServerManager.DefaultRegions;

            UpdateRegions();

            LegacyGameOptions.RecommendedImpostors = Enumerable.Repeat(3, 16).ToArray();
            LegacyGameOptions.MaxImpostors = Enumerable.Repeat(15, 16).ToArray(); // Max Imp = Recommended Imp = 3
            LegacyGameOptions.MinPlayers = Enumerable.Repeat(4, 15).ToArray(); // Min Players = 4

            DebugMode = Config.Bind("Custom", "Enable Debug Mode", false);
            Instance = this;
            CustomOptionHolder.Load();
            RoleInfo.Load();
            CustomColors.Load();
            CustomHatManager.LoadHats();
            AddComponent<ModUpdater>();
            Harmony.PatchAll();
            Patches.SubmergedPatch.Patch();
            EventUtility.Load();
            SubmergedCompatibility.Initialize();

            //Newtonsoft.Jsonを読み込み
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("TheOtherRoles.Resources.Newtonsoft.Json.dll");
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            JsonNet = Assembly.Load(buffer);

            // オレオレオブジェクト有効化
            ClassInjector.RegisterTypeInIl2Cpp(typeof(FoxTask));
            ClassInjector.RegisterTypeInIl2Cpp(typeof(HaomingMenu));

            _ = RoleInfo.loadReadme();
        }
    }

    // Deactivate bans, since I always leave my local testing game and ban myself
    [HarmonyPatch(typeof(PlayerBanData), nameof(PlayerBanData.IsBanned), MethodType.Getter)]
    public static class IsBannedPatch
    {
        public static void Postfix(out bool __result)
        {
            __result = false;
        }
    }

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.Awake))]
    public static class ChatControllerAwakePatch
    {
        private static void Prefix()
        {
            if (!EOSManager.Instance.isKWSMinor)
            {
                DataManager.Settings.Multiplayer.ChatMode = InnerNet.QuickChatModes.FreeChatOrQuickChat;
            }
        }
    }

    // Debugging tools
    [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
    public static class DebugManager
    {
        private static readonly System.Random random = new((int)DateTime.Now.Ticks);
        private static List<PlayerControl> bots = new();

        public static void Postfix(KeyboardJoystick __instance)
        {
            if (AmongUsClient.Instance.AmHost && AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
            {
                //ゲーム強制終了
                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.F5))
                {
                    GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.ForceEnd, false);
                }
            }

            if (!TheOtherRolesPlugin.DebugMode.Value) return;

            // Spawn dummys
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F))
            {
                var playerControl = UnityEngine.Object.Instantiate(AmongUsClient.Instance.PlayerPrefab);

                var i = playerControl.PlayerId = (byte)GameData.Instance.GetAvailableId();

                GameData.Instance.AddDummy(playerControl);

                //playerControl.transform.position = PlayerControl.LocalPlayer.transform.position;
                playerControl.GetComponent<DummyBehaviour>().enabled = true;
                playerControl.isDummy = true;
                playerControl.SetName(AccountManager.Instance.GetRandomName());
                playerControl.SetColor(i);

                AmongUsClient.Instance.Spawn(playerControl, -2, InnerNet.SpawnFlags.None);

                playerControl.Data.RpcSetTasks(new byte[0]);
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                var obj = new GameObject("HaomingMenu");
                obj.AddComponent<HaomingMenu>();
                obj.SetActive(true);
            }
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
    [HarmonyPatch(typeof(SplashManager), nameof(SplashManager.Update))]
    class SplashLogoAnimatorPatch
    {
        public static void Prefix(SplashManager __instance)
        {
            if (TheOtherRolesPlugin.DebugMode.Value)
            {
                __instance.sceneChanger.AllowFinishLoadingScene();
                __instance.startedSceneLoad = true;
            }
        }
    }
    // [HarmonyPatch(typeof(SignInGuestOfflineChoice), nameof(SignInGuestOfflineChoice.Open))]
    // public class SignInGuestOfflineChoiceOpenPatch
    // {
    //     private static void Postfix(SignInGuestOfflineChoice __instance)
    //     {
    //         if (TheOtherRolesPlugin.DebugMode.Value) __instance?.continueOfflineButton?.OnClick?.Invoke();
    //     }
    // }
}
