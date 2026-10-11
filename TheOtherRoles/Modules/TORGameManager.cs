using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Il2CppSystem;
using TheOtherRoles.MetaContext;
using TheOtherRoles.Players;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Exception = System.Exception;
using Object = UnityEngine.Object;
using OperatingSystem = System.OperatingSystem;

namespace TheOtherRoles.Modules;

public class TORGameManager
{
    static private TORGameManager instance;
    static public TORGameManager Instance { get => instance; }
    public List<AchievementTokenBase> AllAchievementTokens = [];
    private Dictionary<byte, PlayerOutfit> allPlayerOutfits = [];
    public GameStatistics GameStatistics { get; set; } = new();
    public float CurrentTime { get; private set; }
    public RuntimeGameAsset RuntimeAsset { get; private init; }
    public List<RoleHistory> RoleHistory = [];
    public Dictionary<byte, ITORAchievement> TitleMap = [];

    public TORGameManager()
    {
        allPlayerOutfits = [];
        instance = this;
        RuntimeAsset = new();
        SendHandshakeRequest();
    }

    public IEnumerable<PlayerOutfit> AllPlayerOutfit() => allPlayerOutfits.Values;

    public void RecordRoleHistory(PlayerControl player)
    {
        var role = RoleInfo.getRoleInfoForPlayer(player, false, true).FirstOrDefault();
        RoleHistory.Add(new(player.PlayerId, role, player.hasModifier(RoleId.Madmate) || player.hasModifier(RoleId.CreatedMadmate), role.color));
    }

    public PlayerOutfit RegisterOutfit(PlayerControl player)
    {
        if (allPlayerOutfits.ContainsKey(player.PlayerId)) return allPlayerOutfits[player.PlayerId];

        var info = new PlayerOutfit(player);
        allPlayerOutfits.Add(player.PlayerId, info);
        return info;
    }

    public PlayerOutfit GetPlayerOutfit(byte playerId)
    {
        return allPlayerOutfits.TryGetValue(playerId, out var v) ? v : null;
    }

    public void Abandon()
    {
        RuntimeAsset.Abandon();
        instance = null;
    }

    public void OnUpdate()
    {
        CurrentTime += Time.deltaTime;
    }

    public void OnGameStart()
    {
        RuntimeAsset.MinimapPrefab = ShipStatus.Instance.MapPrefab;
        RuntimeAsset.MinimapPrefab.gameObject.MarkDontUnload();
        RuntimeAsset.MapScale = ShipStatus.Instance.MapScale;
    }

    void SendHandshakeRequest()
    {
        IEnumerator CoHandshake()
        {
            while (!PlayerControl.LocalPlayer) yield return Effects.Wait(0.2f);
            var localPlayer = PlayerControl.LocalPlayer;
            PlayerControl hostPlayer = null!;
            do
            {
                hostPlayer = PlayerControl.AllPlayerControls.Find((Predicate<PlayerControl>)(p => AmongUsClient.Instance.HostId == p.OwnerId));
            } while (!hostPlayer);

            TORAchievementManager.RequireShare();
        }
        TORGUIManager.Instance.StartCoroutine(CoHandshake().WrapToIl2Cpp());
    }
}

public class RuntimeGameAsset
{
    AsyncOperationHandle<GameObject> handle;
    public MapBehaviour MinimapPrefab = null!;
    public float MapScale;
    public GameObject MinimapObjPrefab => MinimapPrefab.transform.GetChild(1).gameObject;
    public void SetHandle(AsyncOperationHandle<GameObject> handle) => this.handle = handle;
    public void Abandon()
    {
        if (handle?.IsValid() ?? false) handle.Release();
    }
}

public class RoleHistory
{
    public byte PlayerId;
    public RoleInfo RoleInfo;
    public float Time;
    public bool IsMadmate;
    public Color32 Color;

    public RoleHistory(byte PlayerId, RoleInfo RoleInfo, bool IsMadmate, Color32 Color)
    {
        this.PlayerId = PlayerId;
        this.RoleInfo = RoleInfo;
        Time = TORGameManager.Instance?.CurrentTime ?? 0f;
        this.IsMadmate = IsMadmate;
        this.Color = Color;
    }
}

[HarmonyPatch(typeof(AssetReference), nameof(AssetReference.InstantiateAsync), typeof(Transform), typeof(bool))]
public static class LoadShipInstancePatch
{
    static bool Prepare()
    {
        // Do not apply on android, it is broken there.
        return !OperatingSystem.IsAndroid();
    }

    static bool Prefix(AssetReference __instance, ref AsyncOperationHandle<GameObject> __result)
    {
        foreach (var reference in AmongUsClient.Instance.ShipPrefabs)
        {
            if (!reference.AssetGUID.Equals(__instance.AssetGUID)) continue;

            __result = Addressables.InstantiateAsync(__instance.RuntimeKey, null, false, false);
            TORGameManager.Instance?.RuntimeAsset.SetHandle(__result);
            __result.Acquire();
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(AmongUsClient._CoStartGameHost_d__28), "MoveNext")]
public static class FixLoadShipPatch
{
    static bool Prepare()
    {
        // workaround for android devices
        return OperatingSystem.IsAndroid();
    }

    static bool Prefix(AmongUsClient._CoStartGameHost_d__28 __instance, ref bool __result)
    {
        if (__instance.__1__state != 0) return true;

        __instance.__1__state = -1;
        var amongUsClient = __instance.__4__this;
        if (LobbyBehaviour.Instance)
        {
            LobbyBehaviour.Instance.Despawn();
        }

        if (ShipStatus.Instance)
        {
            __instance.__2__current = null;
            __instance.__1__state = 2;
            __result = true;
            return false;
        }

        int num2 = Mathf.Clamp(GameOptionsManager.Instance.CurrentGameOptions.MapId, 0, Constants.MapNames.Length - 1);
        try
        {
            if (num2 == 0 && AprilFoolsMode.ShouldFlipSkeld())
            {
                num2 = 3;
            }
            else if (num2 == 3 && !AprilFoolsMode.ShouldFlipSkeld())
            {
                num2 = 0;
            }
        }
        catch (Exception ex)
        {
            amongUsClient.logger.Error("AmongUsClient::CoStartGame: Exception:");
            Debug.LogException(new Il2CppSystem.Exception(ex.Message), amongUsClient);
        }
        amongUsClient.ShipLoadingAsyncHandle = amongUsClient.ShipPrefabs[num2].InstantiateAsync();

        // -------- TOR --------

        TORGameManager.Instance?.RuntimeAsset.SetHandle(amongUsClient.ShipLoadingAsyncHandle);
        amongUsClient.ShipLoadingAsyncHandle.Acquire();

        // -------- TOR --------

        __instance.__2__current = amongUsClient.ShipLoadingAsyncHandle;
        __instance.__1__state = 1;
        __result = true;
        return false;
    }
}

[HarmonyPatch(typeof(AmongUsClient._CoOnPlayerChangedScene_d__42), "MoveNext")]
public static class FixLoadShipPatch2
{
    static bool Prepare()
    {
        // workaround for android devices
        return OperatingSystem.IsAndroid();
    }

    static bool Prefix(AmongUsClient._CoOnPlayerChangedScene_d__42 __instance, ref bool __result)
    {
        if (__instance.__1__state != 0) return true;

        __instance.__1__state = -1;
        __instance.client.InScene = true;
        var amongUsClient = __instance.__4__this;

        if (GameData.Instance == null)
        {
            GameData.Instance = Object.Instantiate(amongUsClient.GameDataPrefab);
        }

        GameData.Instance.RemoveDisconnectedPlayers();
        if (!amongUsClient.AmHost)
        {
            __result = false;
            return false;
        }

        if (VoteBanSystem.Instance == null)
        {
            VoteBanSystem.Instance = Object.Instantiate(amongUsClient.VoteBanPrefab);
            amongUsClient.Spawn(VoteBanSystem.Instance);
        }
        if (__instance.currentScene.Equals("Tutorial"))
        {
            GameManager.DestroyInstance();
            GameManager gameManager = GameManagerCreator.CreateGameManager(GameOptionsManager.Instance.CurrentGameOptions.GameMode);
            amongUsClient.Spawn(gameManager);
            int num2 = ((amongUsClient.TutorialMapId == 0 && AprilFoolsMode.ShouldFlipSkeld()) ? 3 : amongUsClient.TutorialMapId);
            amongUsClient.ShipLoadingAsyncHandle = amongUsClient.ShipPrefabs[num2].InstantiateAsync();

            // -------- TOR --------

            TORGameManager.Instance?.RuntimeAsset.SetHandle(amongUsClient.ShipLoadingAsyncHandle);
            amongUsClient.ShipLoadingAsyncHandle.Acquire();

            // -------- TOR --------

            __instance.__2__current = amongUsClient.ShipLoadingAsyncHandle;
            __instance.__1__state = 1;
            __result = true;
            return false;
        }
        if (__instance.currentScene.Equals("OnlineGame"))
        {
            if (__instance.client.Id != amongUsClient.ClientId)
            {
                amongUsClient.SendInitialData(__instance.client.Id);
            }
            else
            {
                if (amongUsClient.NetworkMode == NetworkModes.LocalGame)
                {
                    amongUsClient.StartCoroutine(amongUsClient.CoBroadcastManager());
                }
                GameManager.DestroyInstance();
                GameManager gameManager2 = GameManagerCreator.CreateGameManager(GameOptionsManager.Instance.CurrentGameOptions.GameMode);
                amongUsClient.Spawn(gameManager2);
            }
            __instance.__2__current = amongUsClient.CreatePlayer(__instance.client).Cast<Il2CppSystem.Object>();
            __instance.__1__state = 3;
            __result = true;
            return false;
        }

        __instance.__2__current = null;
        __instance.__1__state = -1;
        __result = false;
        return false;
    }
}
