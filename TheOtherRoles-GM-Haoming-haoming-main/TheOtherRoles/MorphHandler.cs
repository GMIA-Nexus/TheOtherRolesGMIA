using System.Diagnostics;
using UnityEngine;

namespace TheOtherRoles;

public static class MorphHandler
{
    public static void morphToPlayer(this PlayerControl pc, PlayerControl target)
    {
        setOutfit(pc, target.Data.DefaultOutfit, target.Visible);
    }

    public static void setOutfit(this PlayerControl pc, NetworkedPlayerInfo.PlayerOutfit outfit, bool visible = true)
    {
        StackFrame stack1 = new(1);
        StackFrame stack2 = new(2);
        pc.Data.Outfits[PlayerOutfitType.Shapeshifted] = outfit;
        pc.CurrentOutfitType = PlayerOutfitType.Shapeshifted;

        pc.RawSetName(outfit.PlayerName);
        pc.RawSetHat(outfit.HatId, outfit.ColorId);
        pc.RawSetVisor(outfit.VisorId, outfit.ColorId);
        pc.RawSetColor(outfit.ColorId);
        Helpers.setSkinWithAnim(pc.MyPhysics, outfit.SkinId);

        if (pc.cosmetics.currentPet) Object.Destroy(pc.cosmetics.currentPet.gameObject);
        if (!pc.Data.IsDead)
            //pc.cosmetics.currentPet = Object.Instantiate(FastDestroyableSingleton<HatManager>.Instance
            //    .GetPetById(outfit.PetId));
            //pc.cosmetics.currentPet.transform.position = pc.transform.position;
            //pc.cosmetics.currentPet.Source = pc;
            //pc.cosmetics.currentPet.Visible = visible;
            //pc.SetPlayerMaterialColors(pc.cosmetics.currentPet.rend);
            pc.RawSetPet(outfit.PetId, outfit.ColorId);
    }

    public static void resetMorph(this PlayerControl pc)
    {
        morphToPlayer(pc, pc);
        Munou.reMorph(pc.PlayerId);
        pc.CurrentOutfitType = PlayerOutfitType.Default;
    }
}
