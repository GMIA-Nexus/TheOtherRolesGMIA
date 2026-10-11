using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using TheOtherRoles.Modules;

namespace TheOtherRoles.Players;

public class OutfitCandidate(string tag, int priority, bool selfAware, NetworkedPlayerInfo.PlayerOutfit outfit)
{
    public string Tag { get; private set; } = tag;
    public int Priority { get; private set; } = priority;
    public bool SelfAware { get; private set; } = selfAware;
    public NetworkedPlayerInfo.PlayerOutfit outfit { get; private set; } = outfit;
}

public class PlayerOutfit(PlayerControl player)
{
    public PlayerControl MyControl { get; private set; } = player;
    public byte PlayerId { get; private set; } = player.PlayerId;

    private List<OutfitCandidate> outfits = [];
    public NetworkedPlayerInfo.PlayerOutfit DefaultOutfit { get; private set; } = player.Data.DefaultOutfit;
    public NetworkedPlayerInfo.PlayerOutfit CurrentOutfit => outfits.Count > 0 ? outfits[0].outfit : DefaultOutfit;

    private void UpdateOutfit()
    {
        NetworkedPlayerInfo.PlayerOutfit newOutfit = DefaultOutfit;
        if (outfits.Count > 0)
        {
            outfits.Sort((o1, o2) => o2.Priority - o1.Priority);
            newOutfit = outfits[0].outfit;
        }

        MyControl.setLook(newOutfit.PlayerName, newOutfit.ColorId, newOutfit.HatId, newOutfit.VisorId, newOutfit.SkinId, newOutfit.PetId);
    }

    public void AddOutfit(OutfitCandidate outfit)
    {
        if (!outfit.SelfAware && MyControl?.AmOwner == true) return;
        outfits.Add(outfit);
        UpdateOutfit();
    }

    public void RemoveOutfit(string tag)
    {
        outfits.RemoveAll(o => o.Tag.Equals(tag));
        UpdateOutfit();
    }

    public NetworkedPlayerInfo.PlayerOutfit GetOutfit(int maxPriority)
    {
        foreach (var outfit in outfits) if (outfit.Priority <= maxPriority) return outfit.outfit;
        return DefaultOutfit;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.OnGameStart))]
public static class OnGameStartPatch
{
    public static void Postfix(PlayerControl __instance)
    {
        var player = __instance;
        if (player.Data != null)
        {
            TORGameManager.Instance?.RegisterOutfit(player);
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MixUpOutfit))]
public static class MixUpOutfitPatch
{
    public static void Postfix(PlayerControl __instance)
    {
        var local = __instance.GetOutfitInfo();
        if (local != null)
        {
            var instance = ShipStatus.Instance.CastFast<FungleShipStatus>().specialSabotage;
            MushroomMixupSabotageSystem.CondensedOutfit condensedOutfit = instance.currentMixups[__instance.PlayerId];
            NetworkedPlayerInfo.PlayerOutfit playerOutfit = instance.ConvertToPlayerOutfit(condensedOutfit);
            local.AddOutfit(new("MushroomMixUp", 150, true, playerOutfit));
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixMixedUpOutfit))]
public static class FixMixUpOutfitPatch
{
    public static bool Prefix(PlayerControl __instance)
    {
        var local = __instance.GetOutfitInfo();
        if (local != null)
            local.RemoveOutfit("MushroomMixUp");

        __instance.CurrentOutfitType = PlayerOutfitType.Default;
        PlayerTask task = __instance.myTasks.Find((Il2CppSystem.Predicate<PlayerTask>)(x => x.TaskType == TaskTypes.MushroomMixupSabotage));
        if (task != null)
            __instance.RemoveTask(task);

        return false;
    }
}
