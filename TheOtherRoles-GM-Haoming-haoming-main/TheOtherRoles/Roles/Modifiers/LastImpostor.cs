using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HarmonyLib;
using Hazel;
using TheOtherRoles.Objects;
using TheOtherRoles.Patches;
using UnityEngine;
using static TheOtherRoles.GameHistory;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles
{
    [HarmonyPatch]
    public class LastImpostor : ModifierBase<LastImpostor>
    {
        public enum DivineResults
        {
            BlackWhite,
            Team,
            Role,
        }
        public static Color color = Palette.ImpostorRed;
        public static bool isEnable { get { return CustomOptionHolder.lastImpostorEnable.getBool(); } }
        public static int killCounter = 0;
        public static int maxKillCounter { get { return (int)CustomOptionHolder.lastImpostorNumKills.getFloat(); } }
        public static int numUsed = 0;
        public static int remainingShots = 0;
        public static int selectedFunction { get { return CustomOptionHolder.lastImpostorFunctions.getSelection(); } }
        public static DivineResults divineResult { get { return (DivineResults)CustomOptionHolder.lastImpostorResults.getSelection(); } }
        public static string postfix
        {
            get
            {
                return ModTranslation.getString("lastImpostorPostfix");
            }
        }
        public static string fullName
        {
            get
            {
                return ModTranslation.getString("lastImpostor");
            }
        }

        public LastImpostor()
        {
            ModType = modId = ModifierType.LastImpostor;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target)
        {
            killCounter += 1;
        }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void OnFinishShipStatusBegin() { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static List<CustomButton> lastImpostorButtons = new();
        static Dictionary<byte, PoolablePlayer> playerIcons = new();
        public static void MakeButtons(HudManager hm)
        {
            lastImpostorButtons = new List<CustomButton>();

            Vector3 lastImpostorCalcPos(byte index)
            {
                return new Vector3(0.95f, -0.15f, -61f) + Vector3.right * index * 0.55f;
            }

            Action lastImpostorButtonOnClick(byte index)
            {
                return () =>
                {
                    if (selectedFunction == 1) return;
                    PlayerControl p = Helpers.playerById(index);
                    LastImpostor.divine(p);
                };
            }
            ;

            Func<bool> lastImpostorHasButton(byte index)
            {
                return () =>
                {
                    if (selectedFunction == 1) return false;
                    var p = PlayerControl.LocalPlayer;
                    if (!p.hasModifier(ModifierType.LastImpostor)) return false;
                    if (p.hasModifier(ModifierType.LastImpostor) && p.CanMove && p.isAlive() & p.PlayerId != index
                        && TORMapOptions.playerIcons.ContainsKey(index) && numUsed < 1 && isCounterMax())
                    {
                        return true;
                    }
                    else
                    {
                        if (playerIcons.ContainsKey(index))
                        {
                            playerIcons[index].gameObject.SetActive(false);
                            if (PlayerControl.LocalPlayer.isRole(RoleType.BountyHunter))
                                setBountyIconPos(Vector3.zero);
                        }
                        if (lastImpostorButtons.Count > index)
                        {
                            lastImpostorButtons[index].setActive(false);
                        }
                        return false;
                    }
                };
            }

            void setButtonPos(byte index)
            {
                Vector3 pos = lastImpostorCalcPos(index);
                Vector3 scale = new(0.4f, 0.8f, 1.0f);

                Vector3 iconBase = hm.UseButton.transform.localPosition;
                iconBase.x *= -1;
                if (lastImpostorButtons[index].PositionOffset != pos)
                {
                    lastImpostorButtons[index].PositionOffset = pos;
                    lastImpostorButtons[index].LocalScale = scale;
                    playerIcons[index].transform.localPosition = iconBase + pos;
                }
            }

            void setIconStatus(byte index, bool transparent)
            {
                playerIcons[index].transform.localScale = Vector3.one * 0.25f;
                playerIcons[index].gameObject.SetActive(PlayerControl.LocalPlayer.CanMove);
                playerIcons[index].setSemiTransparent(transparent);
            }

            void setBountyIconPos(Vector3 offset)
            {
                PoolablePlayer icon = TORMapOptions.playerIcons[BountyHunter.bounty.PlayerId];
                icon.transform.localPosition = Patches.IntroCutsceneOnDestroyPatch.bottomLeft + new Vector3(0f, -0.35f, -62f) + offset;
                BountyHunter.cooldownText.transform.localPosition = Patches.IntroCutsceneOnDestroyPatch.bottomLeft + new Vector3(0f, -0.35f, -63f) + offset;
            }

            Func<bool> lastImpostorCouldUse(byte index)
            {
                return () =>
                {
                    if (selectedFunction == 1) return false;

                    //　ラストインポスター以外の場合、リソースがない場合はボタンを表示しない
                    var p = Helpers.playerById(index);
                    if (!playerIcons.ContainsKey(index) ||
                        !PlayerControl.LocalPlayer.hasModifier(ModifierType.LastImpostor) ||
                        !isCounterMax())
                    {
                        return false;
                    }

                    // ボタンの位置を変更
                    setButtonPos(index);

                    // ボタンにテキストを設定
                    lastImpostorButtons[index].buttonText = PlayerControl.LocalPlayer.isAlive() ? "生存" : "死亡";

                    // アイコンの位置と透明度を変更
                    setIconStatus(index, false);

                    // Bounty Hunterの場合賞金首の位置をずらして表示する
                    if (PlayerControl.LocalPlayer.isRole(RoleType.BountyHunter))
                    {
                        Vector3 offset = new(0f, 1f, 0f);
                        setBountyIconPos(offset);
                    }

                    return PlayerControl.LocalPlayer.CanMove && numUsed < 1;
                };
            }


            for (byte i = 0; i < 15; i++)
            {
                CustomButton lastImpostorButton = new(
                    // Action OnClick
                    lastImpostorButtonOnClick(i),
                    // bool HasButton
                    lastImpostorHasButton(i),
                    // bool CouldUse
                    lastImpostorCouldUse(i),
                    // Action OnMeetingEnds
                    () => { },
                    // sprite
                    null,
                    // position
                    Vector3.zero,
                    // hudmanager
                    hm,
                    // keyboard shortcut
                    null,
                    KeyCode.None,
                    true
                )
                {
                    Timer = 0.0f,
                    MaxTimer = 0.0f
                };

                lastImpostorButtons.Add(lastImpostorButton);
            }

        }
        public static void SetButtonCooldowns() { }

        public static void Clear()
        {
            players = new List<LastImpostor>();
            killCounter = 0;
            numUsed = 0;
            remainingShots = (int)CustomOptionHolder.lastImpostorNumShots.getFloat();
            playerIcons = new Dictionary<byte, PoolablePlayer>();
        }
        public static bool isCounterMax()
        {
            if (maxKillCounter <= killCounter) return true;
            return false;
        }

        public static bool canGuess()
        {
            return remainingShots > 0 && selectedFunction == 1 && isCounterMax();
        }

        public static void promoteToLastImpostor()
        {
            if (!isEnable) return;

            var impList = new List<PlayerControl>();
            foreach (var p in PlayerControl.AllPlayerControls.ToArray())
            {
                if (p.isImpostor() && p.isAlive()) impList.Add(p);
            }
            if (impList.Count == 1)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ImpostorPromotesToLastImpostor, Hazel.SendOption.Reliable, -1);
                writer.Write(impList[0].PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.impostorPromotesToLastImpostor(impList[0].PlayerId);
            }
        }
        public static void divine(PlayerControl p)
        {
            // FortuneTeller.divine(p, resultIsCrewOrNot);
            string msgBase = "";
            string msgInfo = "";
            Color color = Color.white;

            if (divineResult == DivineResults.BlackWhite)
            {
                if (p.isCrew())
                {
                    msgBase = "divineMessageIsCrew";
                    color = Color.white;
                }
                else
                {
                    msgBase = "divineMessageIsntCrew";
                    color = Palette.ImpostorRed;
                }
            }

            else if (divineResult == DivineResults.Team)
            {
                msgBase = "divineMessageTeam";
                if (p.isCrew())
                {
                    msgInfo = ModTranslation.getString("divineCrew");
                    color = Color.white;
                }
                else if (p.isNeutral())
                {
                    msgInfo = ModTranslation.getString("divineNeutral");
                    color = Color.yellow;
                }
                else
                {
                    msgInfo = ModTranslation.getString("divineImpostor");
                    color = Palette.ImpostorRed;
                }
            }

            else if (divineResult == DivineResults.Role)
            {
                msgBase = "divineMessageRole";
                msgInfo = String.Join(" ", RoleInfo.getRoleInfoForPlayer(p).Select(x => Helpers.cs(x.color, x.name)).ToArray());
            }

            string msg = string.Format(ModTranslation.getString(msgBase), p.name, msgInfo);
            if (!string.IsNullOrWhiteSpace(msg))
            {
                FortuneTeller.fortuneTellerMessage(msg, 5f, color);
            }

            if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(FastDestroyableSingleton<HudManager>.Instance.TaskCompleteSound, false, 0.8f);
            numUsed += 1;

            // 占いを実行したことで発火される処理を他クライアントに通知
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.FortuneTellerUsedDivine, Hazel.SendOption.Reliable, -1);
            writer.Write(PlayerControl.LocalPlayer.PlayerId);
            writer.Write(p.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.fortuneTellerUsedDivine(PlayerControl.LocalPlayer.PlayerId, p.PlayerId);
            numUsed += 1;
        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
        class IntroCutsceneOnDestroyPatch
        {
            public static void Prefix(IntroCutscene __instance)
            {
                if (PlayerControl.LocalPlayer != null && FastDestroyableSingleton<HudManager>.Instance != null)
                {
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    {
                        NetworkedPlayerInfo data = p.Data;
                        PoolablePlayer player = UnityEngine.Object.Instantiate<PoolablePlayer>(__instance.PlayerPrefab, FastDestroyableSingleton<HudManager>.Instance.transform);
                        player.UpdateFromPlayerOutfit((NetworkedPlayerInfo.PlayerOutfit)p.Data.DefaultOutfit, PlayerMaterial.MaskType.ComplexUI, p.Data.IsDead, true);
                        player.SetFlipX(true);
                        player.cosmetics.currentPet?.gameObject.SetActive(false);
                        player.cosmetics.nameText.text = p.Data.DefaultOutfit.PlayerName;
                        player.gameObject.SetActive(false);
                        playerIcons[p.PlayerId] = player;
                    }
                }
            }
        }
    }
}
