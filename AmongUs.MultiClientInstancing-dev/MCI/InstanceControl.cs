using InnerNet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BepInEx.Unity.IL2CPP;
using MCI.Patches;
using System.Collections;
using MCI.Embedded.ReactorCoroutines;

namespace MCI;

public static class InstanceControl
{
    internal static Dictionary<int, ClientData> Clients = new();
    internal static Dictionary<byte, int> PlayerClientIDs = new();
    public static PlayerControl CurrentPlayerInPower { get; private set; }

    public static int AvailableId()
    {
        for (var i = 1; i < 128; i++)
        {
            if (!AmongUsClient.Instance.allClients.ToArray().Any(x => x.Id == i) && !Clients.ContainsKey(i) && PlayerControl.LocalPlayer.OwnerId != i)
                return i;
        }

        return -1;
    }

    public static void SwitchTo(byte playerId)
    {
        var savedPlayer = PlayerControl.LocalPlayer;
        var savedPosition = savedPlayer.transform.position;

        PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(PlayerControl.LocalPlayer.transform.position);
        PlayerControl.LocalPlayer.moveable = false;

        var light = PlayerControl.LocalPlayer.lightSource;
        var savedId = PlayerControl.LocalPlayer.PlayerId;

        //Setup new player
        var newPlayer = PlayerById(playerId);

        if (newPlayer == null)
            return;

        var newPosition = newPlayer.transform.position;

        PlayerControl.LocalPlayer = newPlayer;
        PlayerControl.LocalPlayer.lightSource = light;
        PlayerControl.LocalPlayer.moveable = true;

        AmongUsClient.Instance.ClientId = PlayerControl.LocalPlayer.OwnerId;
        AmongUsClient.Instance.HostId = PlayerControl.LocalPlayer.OwnerId;

        HudManager.Instance.SetHudActive(true);
        HudManager.Instance.ShadowQuad.gameObject.SetActive(!newPlayer.Data.IsDead);
        HudManager.Instance.KillButton.buttonLabelText.gameObject.SetActive(false);

        //hacky "fix" for twix and det

        HudManager.Instance.KillButton.transform.parent.GetComponentsInChildren<Transform>().ToList().ForEach(x =>
        {
            if (x.gameObject.name == "KillButton(Clone)")
                Object.Destroy(x.gameObject);
        });

        HudManager.Instance.KillButton.transform.GetComponentsInChildren<Transform>().ToList().ForEach(x =>
        {
            if (x.gameObject.name == "KillTimer_TMP(Clone)")
                Object.Destroy(x.gameObject);
        });

        HudManager.Instance.transform.GetComponentsInChildren<Transform>().ToList().ForEach(x =>
        {
            if (x.gameObject.name == "KillButton(Clone)")
                Object.Destroy(x.gameObject);
        });

        light.transform.SetParent(newPlayer.transform);
        light.transform.localPosition = newPlayer.Collider.offset;
        Camera.main.GetComponent<FollowerCamera>().SetTarget(newPlayer);
        newPlayer.MyPhysics.ResetMoveState(true);
        KillAnimation.SetMovement(newPlayer, true);
        newPlayer.MyPhysics.inputHandler.enabled = true;
        CurrentPlayerInPower = newPlayer;

        newPlayer.NetTransform.SnapTo(newPosition);
        savedPlayer.NetTransform.SnapTo(savedPosition);

        if (MeetingHud.Instance)
        {
            if (newPlayer.Data.IsDead)
                MeetingHud.Instance.SetForegroundForDead();
            else
                MeetingHud.Instance.SetForegroundForAlive(); //Parially works, i still need to get the darkening effect to go
        }
    }

    public static void CleanUpLoad()
    {
        if (GameData.Instance.AllPlayers.Count == 1)
        {
            Clients.Clear();
            PlayerClientIDs.Clear();
        }
    }

    public static void CreatePlayerInstance()
    {
        Coroutines.Start(_CreatePlayerInstanceEnumerator());
    }

    internal static IEnumerator _CreatePlayerInstanceEnumerator()
    {
        var sampleId = AvailableId();
        var sampleC = new ClientData(sampleId, $"Bot-{sampleId}", new()
        {
            Platform = Platforms.StandaloneWin10,
            PlatformName = "Bot"
        }, 1, "", "robotmodeactivate");

        AmongUsClient.Instance.GetOrCreateClient(sampleC);
        yield return AmongUsClient.Instance.CreatePlayer(sampleC);

        sampleC.Character.SetName(MCIPlugin.IKnowWhatImDoing ? $"Bot {{{sampleC.Character.PlayerId}:{sampleId}}}" : $"Bot {sampleC.Character.PlayerId}");
        sampleC.Character.SetSkin(HatManager.Instance.allSkins[Random.Range(0, HatManager.Instance.allSkins.Count)].ProdId, 0);
        sampleC.Character.SetNamePlate(HatManager.Instance.allNamePlates[Random.RandomRangeInt(0, HatManager.Instance.allNamePlates.Count)].ProdId);
        sampleC.Character.SetPet(HatManager.Instance.allPets[Random.RandomRangeInt(0, HatManager.Instance.allPets.Count)].ProdId);
        sampleC.Character.SetColor(Random.Range(0, Palette.PlayerColors.Length));
        sampleC.Character.SetHat("hat_NoHat", 0);
        sampleC.Character.SetVisor("visor_EmptyVisor", 0);

        Clients.Add(sampleId, sampleC);
        PlayerClientIDs.Add(sampleC.Character.PlayerId, sampleId);
        sampleC.Character.MyPhysics.ResetAnimState();
        sampleC.Character.MyPhysics.ResetMoveState();

        if (SubmergedCompatibility.Loaded)
            SubmergedCompatibility.ImpartSub(sampleC.Character);

        if (IL2CPPChainloader.Instance.Plugins.ContainsKey("me.eisbison.theotherroles"))
            sampleC.Character.GetComponent<DummyBehaviour>().enabled = true;

        yield return sampleC.Character.MyPhysics.CoSpawnPlayer(LobbyBehaviour.Instance);
        yield break;
    }

    public static void UpdateNames(string name)
    {
        foreach (var playerId in PlayerClientIDs.Keys)
        {
            if (MCIPlugin.IKnowWhatImDoing)
                PlayerById(playerId).SetName(name + $" {{{playerId}:{PlayerClientIDs[playerId]}}}");
            else
                PlayerById(playerId).SetName(name + $" {playerId}");
        }
    }

    public static PlayerControl PlayerById(byte id) => PlayerControl.AllPlayerControls.ToArray().ToList().Find(x => x.PlayerId == id);

    public static void RemovePlayer(byte id)
    {
        if (id == 0)
            return;

        var clientId = Clients.FirstOrDefault(x => x.Value.Character.PlayerId == id).Key;
        Clients.Remove(clientId, out var outputData);
        PlayerClientIDs.Remove(id);
        AmongUsClient.Instance.RemovePlayer(clientId, DisconnectReasons.Custom);
        AmongUsClient.Instance.allClients.Remove(outputData);
    }

    public static void RemoveAllPlayers()
    {
        PlayerClientIDs.Keys.ToList().ForEach(RemovePlayer);
        SwitchTo(0);
        Keyboard_Joystick.ControllingFigure = 0;
    }

    public static void SetForegroundForAlive(this MeetingHud __instance)
    {
        __instance.amDead = false;
        __instance.SkipVoteButton.gameObject.SetActive(true);
        __instance.SkipVoteButton.AmDead = false;
        __instance.Glass.gameObject.SetActive(false);
        if (CacheMeetingSprite.Cache)
            __instance.Glass.sprite = CacheMeetingSprite.Cache;
    }
}
