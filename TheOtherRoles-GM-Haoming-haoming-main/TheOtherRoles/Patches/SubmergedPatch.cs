using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using TheOtherRoles;


namespace TheOtherRoles.Patches
{
    [HarmonyPatch]
    public class SubmergedPatch
    {
        public static Type SubmarineElevatorType;
        public static Type FloorHandlerType;
        public static Type SubmarineSpawnInSystemType;
        public static Type SubmarinePlayerFloorSystemType;
        public static Type SpawnInStateType;
        public static MethodInfo GetFloorHandlerMethod;
        public static MethodInfo RpcRequestChangeFloorMethod;
        public static void Patch()
        {
            var loaded = IL2CPPChainloader.Instance.Plugins.TryGetValue(SubmergedCompatibility.SUBMERGED_GUID, out PluginInfo pluginInfo);
            if (!loaded) return;
            var plugin = pluginInfo!.Instance as BasePlugin;
            var version = pluginInfo.Metadata.Version;
            var assembly = plugin!.GetType().Assembly;
            var types = AccessTools.GetTypesFromAssembly(assembly);
            SubmarineElevatorType = types.First(t => t.Name == "SubmarineElevator");
            SubmarinePlayerFloorSystemType = types.First(t => t.Name == "SubmarinePlayerFloorSystem");
            SpawnInStateType = types.First(t => t.Name == "SpawnInState");
            FloorHandlerType = types.First(t => t.Name == "FloorHandler");
            GetFloorHandlerMethod = AccessTools.Method(FloorHandlerType, "GetFloorHandler", new Type[] { typeof(PlayerControl) });
            RpcRequestChangeFloorMethod = AccessTools.Method(FloorHandlerType, "RpcRequestChangeFloor");

            // OnDestroyパッチ
            var SubmarineSelectSpawnType = types.First(t => t.Name == "SubmarineSelectSpawn");
            var SubmarineSelectSpawnOnDestroyOriginal = AccessTools.Method(SubmarineSelectSpawnType, "OnDestroy");
            var SubmarineSelectSpawnOnDestroyPostfix = SymbolExtensions.GetMethodInfo(() => SubmarineSelectSpawnOnDestroyPatch.Postfix());
            var SubmarineSelectSpawnOnDestroyPrefix = SymbolExtensions.GetMethodInfo(() => SubmarineSelectSpawnOnDestroyPatch.Prefix());

            // GetTotalPlayerAmountパッチ
            var aInt = 0;
            SubmarineSpawnInSystemType = types.First(t => t.Name == "SubmarineSpawnInSystem");
            var GetTotalPlayerAmountOriginal = AccessTools.Method(SubmarineSpawnInSystemType, "GetTotalPlayerAmount");
            var GetTotalPlayerAmountPostfix = SymbolExtensions.GetMethodInfo(() => SubmarineSpawnInSystemGetTotalPlayerAmountPatch.Postfix());
            var GetTotalPlayerAmountPrefix = SymbolExtensions.GetMethodInfo(() => SubmarineSpawnInSystemGetTotalPlayerAmountPatch.Prefix(ref aInt));
            // Detoriorateパッチ
            object aObject = null;
            float aFloat = 0f;
            var DetoriorateOriginal = AccessTools.Method(SubmarineSpawnInSystemType, "Detoriorate");
            var DetorioratePostfix = SymbolExtensions.GetMethodInfo(() => SubmarineSpawnInSystemDetorioratePatch.Postfix());
            var DetorioratePrefix = SymbolExtensions.GetMethodInfo(() => SubmarineSpawnInSystemDetorioratePatch.Prefix(aObject, aFloat));


            // パッチ適応
            var harmony = new Harmony("Submerged");
            harmony.Patch(SubmarineSelectSpawnOnDestroyOriginal, new HarmonyMethod(SubmarineSelectSpawnOnDestroyPrefix), new HarmonyMethod(SubmarineSelectSpawnOnDestroyPostfix));
            harmony.Patch(GetTotalPlayerAmountOriginal, new HarmonyMethod(GetTotalPlayerAmountPrefix), new HarmonyMethod(GetTotalPlayerAmountPostfix));
            harmony.Patch(DetoriorateOriginal, new HarmonyMethod(DetorioratePrefix), new HarmonyMethod(DetorioratePostfix));
        }

        public static void ChangePlayerFloorState(byte playerId, bool toUpper)
        {
            var SubMarinePlayerFloorSystemProperties = SubmarinePlayerFloorSystemType.GetProperties(BindingFlags.Static | BindingFlags.Public);
            var Instance = SubMarinePlayerFloorSystemProperties.First(f => f.Name == "Instance").GetValue(null);
            var ChangePlayerFloorStateMethod = SubmarinePlayerFloorSystemType.GetMethod("ChangePlayerFloorState");
            ChangePlayerFloorStateMethod.Invoke(Instance, new object[] { playerId, toUpper });
        }

        public class SubmarineSelectSpawnOnDestroyPatch
        {
            public static void Prefix() { }
            public static void Postfix()
            {
                PlayerControl.LocalPlayer.SetKillTimer(GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown);
                MapUtilities.CachedShipStatus.EmergencyCooldown = (float)GameOptionsManager.Instance.currentNormalGameOptions.EmergencyCooldown;
                ExileControllerReEnableGameplayPatch.ReEnableGameplay();
            }
        }
        public class SubmarineSpawnInSystemGetTotalPlayerAmountPatch
        {
            public static bool Prefix(ref int __result)
            {
                __result = Enumerable.Count<NetworkedPlayerInfo>(GameData.Instance.AllPlayers.ToSystemList<NetworkedPlayerInfo>(), delegate (NetworkedPlayerInfo p)
                {
                    if (p != null && !p.IsDead && !p.Disconnected && Helpers.playerById(p.PlayerId) != Puppeteer.dummy)
                    {
                        PlayerControl @object = p.Object;
                        if (@object != null)
                        {
                            return !@object.isDummy;
                        }
                    }
                    return false;
                });
                return false;
            }
            public static void Postfix() { }
        }
        public class SubmarineSpawnInSystemDetorioratePatch
        {
            public static void Postfix() { }
            public static bool Prefix(object __instance, float deltaTime)
            {
                var GetTotalPlayerAmount = AccessTools.Method(SubmarineSpawnInSystemType, "GetTotalPlayerAmount");
                var totalPlayerAmount = (GetTotalPlayerAmount.Invoke(__instance, new object[0]) as int?).Value;
                var GetReadyPlayerAmount = AccessTools.Method(SubmarineSpawnInSystemType, "GetReadyPlayerAmount");
                var ReadyPlayerAmount = (GetReadyPlayerAmount.Invoke(__instance, new object[0]) as int?).Value;
                var SubmarineSpawnInSystemFields = SubmarineSpawnInSystemType.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                var CurrentState = SubmarineSpawnInSystemFields.First(f => f.Name == "CurrentState");
                CurrentState = SubmarineSpawnInSystemType.GetField("CurrentState");
                object currentState = CurrentState.GetValue(__instance);
                Type enumUnderlyingType = System.Enum.GetUnderlyingType(SpawnInStateType);
                object state = System.Convert.ChangeType(currentState, enumUnderlyingType);

                var Timer = SubmarineSpawnInSystemFields.First(f => f.Name == "Timer");
                if ((byte)state == 1)
                {
                    var timer = MathF.Max(0f, (Timer.GetValue(__instance) as float?).Value - deltaTime);
                    Timer.SetValue(__instance, timer);
                }

                if (totalPlayerAmount == ReadyPlayerAmount)
                {
                    var Players = SubmarineSpawnInSystemFields.First(f => f.Name == "Players");
                    var SubmarineSpawnInSystemProperties = SubmarineSpawnInSystemType.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    var IsDirty = SubmarineSpawnInSystemProperties.First(f => f.Name == "IsDirty");
                    CurrentState.SetValueDirect(__makeref(__instance), (byte)state + 1);
                    //CurrentState.SetValue(__instance, Done);
                    Players.SetValue(__instance, new HashSet<byte>());
                    Timer.SetValue(__instance, 10f);
                    IsDirty.SetValue(__instance, true);
                }

                return false;

            }
        }
    }
}
