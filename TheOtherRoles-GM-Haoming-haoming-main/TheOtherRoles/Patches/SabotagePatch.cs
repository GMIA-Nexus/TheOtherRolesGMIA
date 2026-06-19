using HarmonyLib;
using Hazel;

namespace TheOtherRoles.Patches
{
    [HarmonyPatch(typeof(HeliSabotageSystem), nameof(HeliSabotageSystem.UpdateSystem))]
    class HeliSabotageSystemRepairDamagePatch
    {
        static void Postfix(HeliSabotageSystem __instance, PlayerControl player, MessageReader msgReader)
        {
            HeliSabotageSystem.Tags tags = (HeliSabotageSystem.Tags)(msgReader.ReadByte() & 240);
            if (tags != HeliSabotageSystem.Tags.ActiveBit)
            {
                if (tags == HeliSabotageSystem.Tags.DamageBit)
                {
                    __instance.Countdown = CustomOptionHolder.airshipReactorDuration.getFloat();
                }
            }
        }
    }

}
