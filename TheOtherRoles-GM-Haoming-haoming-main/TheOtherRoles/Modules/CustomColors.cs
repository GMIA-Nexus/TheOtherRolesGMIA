using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
#if Steam
using AmongUs.Data;
using AmongUs.Data.Legacy;
#endif

namespace TheOtherRoles.Modules
{
    public class CustomColors
    {
        protected static Dictionary<int, string> ColorStrings = new();
        public static List<int> lighterColors = new() { 3, 4, 5, 7, 10, 11, 13, 14, 17 };
        public static uint pickableColors = (uint)Palette.ColorNames.Length;

        /* version 1
        private static readonly List<int> ORDER = new List<int>() { 7, 17, 5, 33, 4,
                                                                    30, 0, 19, 27, 3,
                                                                    13, 25, 18, 15, 23,
                                                                    8, 32, 1, 21, 31,
                                                                    10, 34, 12, 14, 28,
                                                                    22, 29, 11, 26, 2,
                                                                    20, 24, 9, 16, 6 }; */
#pragma warning disable format

        private static readonly List<int> ORDER = new()
        {
            7, 14, 5, 33, 4,
            30, 0, 19, 27, 3,
            17, 25, 18, 13, 23,
            8, 32, 1, 21, 31,
            10, 34, 15, 28, 22,
            29, 11, 2, 26, 16,
            20, 24, 9, 12, 6
        };

#pragma warning restore format

        public static void Load()
        {
            List<StringNames> longlist = Enumerable.ToList<StringNames>(Palette.ColorNames);
            List<Color32> colorlist = Enumerable.ToList<Color32>(Palette.PlayerColors);
            List<Color32> shadowlist = Enumerable.ToList<Color32>(Palette.ShadowColors);

            List<CustomColor> colors = new();

            /* Custom Colors */
            colors.Add(new CustomColor
            {
                longname = "colorSalmon",
                color = new Color32(239, 191, 192, byte.MaxValue), // color = new Color32(0xD8, 0x82, 0x83, byte.MaxValue),
                shadow = new Color32(182, 119, 114, byte.MaxValue), // shadow = new Color32(0xA5, 0x63, 0x65, byte.MaxValue),
                isLighterColor = true
            });

            colors.Add(new CustomColor
            {
                longname = "colorBordeaux",
                color = new Color32(109, 7, 26, byte.MaxValue),
                shadow = new Color32(54, 2, 11, byte.MaxValue),
                isLighterColor = false
            });

            colors.Add(new CustomColor
            {
                longname = "colorOlive",
                color = new Color32(154, 140, 61, byte.MaxValue),
                shadow = new Color32(104, 95, 40, byte.MaxValue),
                isLighterColor = false
            });

            colors.Add(new CustomColor
            {
                longname = "colorTurquoise",
                color = new Color32(22, 132, 176, byte.MaxValue),
                shadow = new Color32(15, 89, 117, byte.MaxValue),
                isLighterColor = false
            });

            colors.Add(new CustomColor
            {
                longname = "colorMint",
                color = new Color32(111, 192, 156, byte.MaxValue),
                shadow = new Color32(65, 148, 111, byte.MaxValue),
                isLighterColor = true
            });

            colors.Add(new CustomColor
            {
                longname = "colorLavender",
                color = new Color32(173, 126, 201, byte.MaxValue),
                shadow = new Color32(131, 58, 203, byte.MaxValue),
                isLighterColor = true
            });

            colors.Add(new CustomColor
            {
                longname = "colorNougat",
                color = new Color32(160, 101, 56, byte.MaxValue),
                shadow = new Color32(115, 15, 78, byte.MaxValue),
                isLighterColor = false
            });

            colors.Add(new CustomColor
            {
                longname = "colorPeach",
                color = new Color32(255, 164, 119, byte.MaxValue),
                shadow = new Color32(238, 128, 100, byte.MaxValue),
                isLighterColor = true
            });

            colors.Add(new CustomColor
            {
                longname = "colorWasabi",
                color = new Color32(112, 143, 46, byte.MaxValue),
                shadow = new Color32(72, 92, 29, byte.MaxValue),
                isLighterColor = false
            });

            colors.Add(new CustomColor
            {
                longname = "colorHotPink",
                color = new Color32(255, 51, 102, byte.MaxValue),
                shadow = new Color32(232, 0, 58, byte.MaxValue),
                isLighterColor = true
            });

            colors.Add(new CustomColor
            {
                longname = "colorPetrol",
                color = new Color32(0, 99, 105, byte.MaxValue),
                shadow = new Color32(0, 61, 54, byte.MaxValue),
                isLighterColor = false
            });

            colors.Add(new CustomColor
            {
                longname = "colorLemon",
                color = new Color32(0xDB, 0xFD, 0x2F, byte.MaxValue),
                shadow = new Color32(0x74, 0xE5, 0x10, byte.MaxValue),
                isLighterColor = true
            });

            colors.Add(new CustomColor
            {
                longname = "colorSignalOrange",
                color = new Color32(0xF7, 0x44, 0x17, byte.MaxValue),
                shadow = new Color32(0x9B, 0x2E, 0x0F, byte.MaxValue),
                isLighterColor = true
            });

            colors.Add(new CustomColor
            {
                longname = "colorTeal",
                color = new Color32(0x25, 0xB8, 0xBF, byte.MaxValue),
                shadow = new Color32(0x12, 0x89, 0x86, byte.MaxValue),
                isLighterColor = false
            });

            colors.Add(new CustomColor
            {
                longname = "colorBlurple",
                color = new Color32(0x59, 0x3C, 0xD6, byte.MaxValue),
                shadow = new Color32(0x29, 0x17, 0x96, byte.MaxValue),
                isLighterColor = false
            });

            colors.Add(new CustomColor
            {
                longname = "colorSunrise",
                color = new Color32(0xFF, 0xCA, 0x19, byte.MaxValue),
                shadow = new Color32(0xDB, 0x44, 0x42, byte.MaxValue),
                isLighterColor = true
            });

            colors.Add(new CustomColor
            {
                longname = "colorIce",
                color = new Color32(0xA8, 0xDF, 0xFF, byte.MaxValue),
                shadow = new Color32(0x59, 0x9F, 0xC8, byte.MaxValue),
                isLighterColor = true
            });

            pickableColors += (uint)colors.Count; // Colors to show in Tab
            /** Hidden Colors **/

            /** Add Colors **/
            int id = 50000;
            foreach (CustomColor cc in colors)
            {
                longlist.Add((StringNames)id);
                CustomColors.ColorStrings[id++] = cc.longname;
                colorlist.Add(cc.color);
                shadowlist.Add(cc.shadow);
                if (cc.isLighterColor)
                    lighterColors.Add(colorlist.Count - 1);
            }

            Palette.ColorNames = longlist.ToArray();
            Palette.PlayerColors = colorlist.ToArray();
            Palette.ShadowColors = shadowlist.ToArray();
        }

        protected internal struct CustomColor
        {
            public string longname;
            public Color32 color;
            public Color32 shadow;
            public bool isLighterColor;
        }

        [HarmonyPatch]
        public static class CustomColorPatches
        {
            [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new[] {
                typeof(StringNames),
                typeof(Il2CppReferenceArray<Il2CppSystem.Object>)
            })]
            private class ColorStringPatch
            {
                public static bool Prefix(ref string __result, [HarmonyArgument(0)] StringNames name)
                {
                    if ((int)name >= 50000)
                    {
                        string text = CustomColors.ColorStrings[(int)name];
                        if (text != null)
                        {
                            __result = ModTranslation.getString(text) + " (MOD)";
                            return false;
                        }
                    }
                    return true;
                }
            }

            [HarmonyPatch(typeof(ChatNotification), nameof(ChatNotification.SetUp))]
            private class ChatNotificationColorsPatch
            {
                public static bool Prefix(ChatNotification __instance, PlayerControl sender, string text)
                {
                    if (ShipStatus.Instance && !TORMapOptions.showChatNotifications)
                    {
                        return false;
                    }
                    __instance.timeOnScreen = 5f;
                    __instance.gameObject.SetActive(true);
                    __instance.SetCosmetics(sender.Data);
                    string str;
                    Color color;
                    try
                    {
                        str = ColorUtility.ToHtmlStringRGB(Palette.TextColors[__instance.player.ColorId]);
                        color = Palette.TextOutlineColors[__instance.player.ColorId];
                    }
                    catch
                    {
                        Color32 c = Palette.PlayerColors[__instance.player.ColorId];
                        str = ColorUtility.ToHtmlStringRGB(c);

                        color = c.r + c.g + c.b > 180 ? Palette.Black : Palette.White;
                        TheOtherRolesPlugin.Logger.LogMessage($"{c.r}, {c.g}, {c.b}");
                    }
                    __instance.playerColorText.text = __instance.player.ColorBlindName;
                    __instance.playerNameText.text = "<color=#" + str + ">" + (string.IsNullOrEmpty(sender.Data.PlayerName) ? "..." : sender.Data.PlayerName);
                    __instance.playerNameText.outlineColor = color;
                    __instance.chatText.text = text;
                    return false;
                }
            }

            [HarmonyPatch(typeof(PlayerTab), nameof(PlayerTab.OnEnable))]
            private static class PlayerTabEnablePatch
            {
                public static void Postfix(PlayerTab __instance)
                { // Replace instead
                    Il2CppArrayBase<ColorChip> chips = __instance.ColorChips.ToArray();

                    int cols = 5; // TODO: Design an algorithm to dynamically position chips to optimally fill space
                    for (int i = 0; i < ORDER.Count; i++)
                    {
                        int pos = ORDER[i];
                        if (pos < 0 || pos > chips.Length)
                            continue;
                        ColorChip chip = chips[pos];
                        int row = i / cols, col = i % cols; // Dynamically do the positioning
                        chip.transform.localPosition = new Vector3(-0.975f + (col * 0.485f), 1.475f - (row * 0.49f), chip.transform.localPosition.z);
                        chip.transform.localScale *= 0.78f;
                    }
                    for (int j = ORDER.Count; j < chips.Length; j++)
                    { // If number isn't in order, hide it
                        ColorChip chip = chips[j];
                        chip.transform.localScale *= 0f;
                        chip.enabled = false;
                        chip.Button.enabled = false;
                        chip.Button.OnClick.RemoveAllListeners();
                    }
                }
            }

#if Steam
            [HarmonyPatch(typeof(LegacySaveManager), nameof(LegacySaveManager.LoadPlayerPrefs))]
            private static class LoadPlayerPrefsPatch
            { // Fix Potential issues with broken colors
                private static bool needsPatch = false;
                public static void Prefix([HarmonyArgument(0)] bool overrideLoad)
                {
                    if (!LegacySaveManager.loaded || overrideLoad)
                        needsPatch = true;
                }
                public static void Postfix()
                {
                    if (!needsPatch) return;
                    LegacySaveManager.colorConfig %= CustomColors.pickableColors;
                    needsPatch = false;
                }
            }
#endif
            [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckColor))]
            private static class PlayerControlCheckColorPatch
            {
                private static bool isTaken(PlayerControl player, uint color)
                {
                    foreach (NetworkedPlayerInfo p in GameData.Instance.AllPlayers)
                        if (!p.Disconnected && p.PlayerId != player.PlayerId && p.DefaultOutfit.ColorId == color)
                            return true;
                    return false;
                }
                public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte bodyColor)
                { // Fix incorrect color assignment
                    uint color = (uint)bodyColor;
                    if (isTaken(__instance, color) || color >= Palette.PlayerColors.Length)
                    {
                        int num = 0;
                        while (num++ < 50 && (color >= CustomColors.pickableColors || isTaken(__instance, color)))
                        {
                            color = (color + 1) % CustomColors.pickableColors;
                        }
                    }
                    __instance.RpcSetColor((byte)color);
                    return false;
                }
            }
        }
    }
}
