using HarmonyLib;
using TheOtherRoles.Objects;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Hazel;

namespace TheOtherRoles
{
    [HarmonyPatch]
    public class SoulPlayer
    {
        public static Color color = Palette.CrewmateBlue;
        private static CustomButton senriganButton;
        private static bool enableSenrigan { get { return CustomOptionHolder.enableSenrigan.getBool(); } }
        public static bool toggle = false;
        public static Sprite senriganIcon;
        public static Dictionary<byte, float> killTimers;
        public static float updateInterval = 2f;
        public static float timer = 0;
        public static TMPro.TMP_Text statusText = null;
        public static void senrigan()
        {
            var hm = FastDestroyableSingleton<HudManager>.Instance;
            if (toggle)
            {
                Camera.main.orthographicSize /= 6f;
                hm.UICamera.orthographicSize /= 6f;
                hm.transform.localScale /= 6f;
            }
            else
            {
                Camera.main.orthographicSize *= 6f;
                hm.UICamera.orthographicSize *= 6f;
                hm.transform.localScale *= 6f;
            }
            toggle = !toggle;
        }

        public static void FixedUpdate()
        {
            if (CustomOptionHolder.deadImpostorCanSeeKillColdown.getBool() && PlayerControl.LocalPlayer.isImpostor() && PlayerControl.LocalPlayer.isAlive())
            {
                timer += Time.fixedDeltaTime;
                if (timer >= updateInterval)
                {
                    timer = 0;
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SyncKillTimer, Hazel.SendOption.Reliable, -1);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    writer.Write(PlayerControl.LocalPlayer.killTimer);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.syncKillTimer(PlayerControl.LocalPlayer.PlayerId, PlayerControl.LocalPlayer.killTimer);
                }
            }
            UpdateStatusText();
        }
        public static void MakeButtons(HudManager hm)
        {
            senriganButton = new CustomButton(
                () =>
                {/*ボタンが押されたとき*/
                    senrigan();
                },
                () => {/*ボタンが有効になる条件*/ return enableSenrigan && PlayerControl.LocalPlayer.isDead() && !PlayerControl.LocalPlayer.isRole(RoleType.Puppeteer); },
                () => {/*ボタンが使える条件*/ return PlayerControl.LocalPlayer.isDead(); },
                () => {/*ミーティング終了時*/ },
                getSenriganIcon(),
                CustomButton.ButtonPositions.upperRowLeft,
                hm,
                hm.AbilityButton,
                KeyCode.F
            )
            {
                MaxTimer = 0f,
                Timer = 0f,
                buttonText = ModTranslation.getString("")
            };
        }
        public static Sprite getSenriganIcon()
        {
            if (senriganIcon) return senriganIcon;
            senriganIcon = Helpers.loadSpriteFromAssetBundle("Senrigan.png");
            return senriganIcon;
        }
        public static void SetButtonCooldowns()
        {
            senriganButton.Timer = senriganButton.MaxTimer = 0f;

        }



        public static void Clear()
        {
            toggle = false;
            if (statusText)
            {
                UnityEngine.GameObject.Destroy(statusText);
            }
            statusText = null;
            killTimers = new();
        }
        public static void UpdateStatusText()
        {
            if (CustomOptionHolder.deadImpostorCanSeeKillColdown.getBool())
            {
                if (MeetingHud.Instance != null)
                {
                    if (statusText != null)
                    {
                        statusText.gameObject.SetActive(false);
                    }
                    return;
                }

                if (PlayerControl.LocalPlayer.isDead() && PlayerControl.LocalPlayer.isImpostor())
                {
                    if (statusText == null)
                    {
                        GameObject gameObject = UnityEngine.Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance?.roomTracker.gameObject);
                        gameObject.transform.SetParent(FastDestroyableSingleton<HudManager>.Instance.transform);
                        gameObject.SetActive(true);
                        UnityEngine.Object.DestroyImmediate(gameObject.GetComponent<RoomTracker>());
                        statusText = gameObject.GetComponent<TMPro.TMP_Text>();
                        gameObject.transform.localPosition = new Vector3(-2.7f, -0.1f, gameObject.transform.localPosition.z);

                        statusText.transform.localScale = new Vector3(1f, 1f, 1f);
                        statusText.fontSize = 1.5f;
                        statusText.fontSizeMin = 1.5f;
                        statusText.fontSizeMax = 1.5f;
                        statusText.alignment = TMPro.TextAlignmentOptions.BottomLeft;
                    }

                    statusText.gameObject.SetActive(true);
                    string text = "KillTimer\n";
                    killTimers.Keys.ToList().ForEach(key =>
                    {
                        if (key == PlayerControl.LocalPlayer.PlayerId) return;
                        PlayerControl p = Helpers.playerById(key);
                        if (p.isDead()) return;
                        if (killTimers[key] > 0)
                        {
                            killTimers[key] -= Time.fixedDeltaTime;
                        }
                        else
                        {
                            killTimers[key] = 0;
                        }
                        text += $"{p.name}: {killTimers[key]:F2}s";
                        text += "\n";

                    });
                    statusText.text = text;
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.StartMeeting))]
        class StartMeetingPatch
        {
            public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)] NetworkedPlayerInfo meetingTarget)
            {
                if (PlayerControl.LocalPlayer.Data.IsDead)
                {
                    if (toggle)
                    {
                        senrigan();
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Minigame), nameof(Minigame.Begin))]
        class MinigameBeginPatch
        {
            static void Prefix(Minigame __instance)
            {
                if (PlayerControl.LocalPlayer.isDead())
                {
                    if (toggle)
                    {
                        senrigan();
                    }
                }
            }
        }
    }
}
