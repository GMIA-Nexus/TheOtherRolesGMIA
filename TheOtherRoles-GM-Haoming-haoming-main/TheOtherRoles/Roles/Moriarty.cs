using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using TheOtherRoles.Objects;
using UnityEngine;
using static TheOtherRoles.Patches.PlayerControlFixedUpdatePatch;

namespace TheOtherRoles
{
    [HarmonyPatch]
    public class Moriarty : RoleBase<Moriarty>
    {
        public static Color color = Color.green;
        public static PlayerControl tmpTarget;
        public static PlayerControl target;
        public static PlayerControl currentTarget;
        public static PlayerControl killTarget;
        public static List<PlayerControl> brainwashed;
        public static CustomButton killButton;
        public static int KillDistance;
        public static int brainwashDistance { get { return CustomOptionHolder.moriartyBrainwashDistance.getSelection(); } }
        public static int killDistance { get { return CustomOptionHolder.moriartyKillDistance.getSelection(); } }
        public static CustomButton brainwashButton;
        public static float brainwashTime { get { return CustomOptionHolder.moriartyBrainwashTime.getFloat(); } }
        public static float brainwashCooldown { get { return CustomOptionHolder.moriartyBrainwashCooldown.getFloat(); } }
        public static float killCooldown = 0;
        public static int numberToWin { get { return (int)CustomOptionHolder.moriartyNumberToWin.getFloat(); } }
        public static TMPro.TMP_Text text;
        public static int counter;
        public static Sprite brainwashIcon;
        public Moriarty()
        {
            RoleType = roleId = RoleType.NoRole;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd()
        {
            brainwashed.Clear();
            target = null;
        }
        public override void FixedUpdate()
        {
            if (player == PlayerControl.LocalPlayer)
            {
                currentTarget = setTarget(killDistance: brainwashDistance);
                if (target != null)
                {
                    killTarget = setTarget(targetingPlayer: target, killDistance: killDistance);
                }
                else
                {
                    killTarget = null;
                }
                setPlayerOutline(currentTarget, Moriarty.color);
                arrowUpdate();
            }

        }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void OnFinishShipStatusBegin() { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void MakeButtons(HudManager hm)
        {
            // kill button
            killButton = new CustomButton(
                // OnClick
                () =>
                {

                    MurderAttemptResult murder = Helpers.checkMuderAttemptAndKill(PlayerControl.LocalPlayer, killTarget, showAnimation: false);
                    if (murder != MurderAttemptResult.BlankKill)
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.MoriartyKill, Hazel.SendOption.Reliable, -1);
                        writer.Write(killTarget.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.moriartyKill(killTarget.PlayerId);
                        target = null;
                        brainwashButton.Timer = brainwashButton.MaxTimer;
                    }
                },
                // HasButton
                () => { return PlayerControl.LocalPlayer.isRole(RoleType.Moriarty) && PlayerControl.LocalPlayer.isAlive(); },
                // CouldUse
                () =>
                {
                    if (text != null)
                    {
                        text.text = $"{counter}/{numberToWin}";
                    }
                    killButton.buttonText = killTarget ? killTarget.name : ModTranslation.getString("MoriartyNoneText");
                    return killTarget != null && PlayerControl.LocalPlayer.CanMove;
                },
                // OnMeetingEnds
                () =>
                {
                    killButton.Timer = killButton.MaxTimer = killCooldown;
                },
                hm.KillButton.graphic.sprite,
                CustomButton.ButtonPositions.upperRowRight,
                hm,
                hm.KillButton,
                KeyCode.Q,
                false
            )
            {
                MaxTimer = killCooldown
            };
            killButton.Timer = killButton.MaxTimer;
            text = GameObject.Instantiate(killButton.actionButton.cooldownTimerText, killButton.actionButton.cooldownTimerText.transform.parent);
            text.text = "";
            text.enableWordWrapping = false;
            text.transform.localScale = Vector3.one * 0.5f;
            text.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            brainwashButton = new CustomButton(
                // OnClick
                () =>
                {
                    if (currentTarget != null)
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetBrainwash, Hazel.SendOption.Reliable, -1);
                        writer.Write(currentTarget.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.setBrainwash(currentTarget.PlayerId);

                        // 洗脳終了までのカウントダウン
                        TMPro.TMP_Text text;
                        RoomTracker roomTracker = HudManager.Instance?.roomTracker;
                        GameObject gameObject = UnityEngine.Object.Instantiate(roomTracker.gameObject);
                        UnityEngine.Object.DestroyImmediate(gameObject.GetComponent<RoomTracker>());
                        gameObject.transform.SetParent(HudManager.Instance.transform);
                        gameObject.transform.localPosition = new Vector3(0, -1.3f, gameObject.transform.localPosition.z);
                        gameObject.transform.localScale = Vector3.one * 3f;
                        text = gameObject.GetComponent<TMPro.TMP_Text>();
                        PlayerControl tmpP = target;
                        bool done = false;
                        HudManager.Instance.StartCoroutine(Effects.Lerp(brainwashTime, new Action<float>((p) =>
                        {
                            if (done)
                            {
                                return;
                            }
                            if (target == null || MeetingHud.Instance != null || p == 1f)
                            {
                                if (text != null && text.gameObject) UnityEngine.Object.Destroy(text.gameObject);
                                if (target == tmpP) target = null;
                                done = true;
                                return;
                            }
                            else
                            {
                                string message = (brainwashTime - (p * brainwashTime)).ToString("0");
                                bool even = ((int)(p * brainwashTime / 0.25f)) % 2 == 0; // Bool flips every 0.25 seconds
                                // string prefix = even ? "<color=#555555FF>" : "<color=#FFFFFFFF>";
                                string prefix = "<color=#555555FF>";
                                text.text = prefix + message + "</color>";
                                if (text != null) text.color = even ? Color.yellow : Color.red;

                            }
                        })));
                    }
                    tmpTarget = null;
                    brainwashButton.Timer = brainwashButton.MaxTimer;
                },
                // HasButton
                () => { return PlayerControl.LocalPlayer.isRole(RoleType.Moriarty) && PlayerControl.LocalPlayer.isAlive() && target == null; },
                // CouldUse
                () =>
                {
                    brainwashButton.buttonText = currentTarget ? currentTarget.name : "None";
                    brainwashButton.Sprite = getBrainwashIcon();
                    return PlayerControl.LocalPlayer.CanMove && currentTarget != null;
                },
                // OnMeetingEnds
                () =>
                {
                    brainwashButton.Timer = brainwashButton.MaxTimer;
                },
                Moriarty.getBrainwashIcon(),
                CustomButton.ButtonPositions.upperRowLeft,
                hm,
                hm.AbilityButton,
                KeyCode.F,
                false
            )
            {
                buttonText = ModTranslation.getString("moriartyBrainwashButton"),
                MaxTimer = brainwashCooldown,
            };
        }
        public static void SetButtonCooldowns()
        {
            brainwashButton.MaxTimer = brainwashCooldown;
            killButton.MaxTimer = killCooldown;
        }

        public static void Clear()
        {
            players = new();
            brainwashed = new();
            counter = 0;
            arrows = new List<Arrow>();

        }
        public static Sprite getBrainwashIcon()
        {
            if (brainwashIcon) return brainwashIcon;
            brainwashIcon = Helpers.loadSpriteFromAssetBundle("Brainwash.png");
            return brainwashIcon;
        }
        public static int countLovers()
        {
            int counter = 0;
            foreach (var player in allPlayers)
            {
                if (player.isLovers()) counter += 1;
            }
            return counter;
        }
        public static List<Arrow> arrows = new();
        public static float updateTimer = 0f;
        public static float arrowUpdateInterval = 0.5f;
        public static TMPro.TMP_Text targetPositionText;
        public static Sprite arrowSprite;
        static void arrowUpdate()
        {

            // 前フレームからの経過時間をマイナスする
            updateTimer -= Time.fixedDeltaTime;

            // 1秒経過したらArrowを更新
            if (updateTimer <= 0.0f)
            {

                // 前回のArrowをすべて破棄する
                foreach (Arrow arrow in arrows)
                {
                    if (arrow != null && arrow.arrow != null)
                    {
                        arrow.arrow.SetActive(false);
                        UnityEngine.Object.Destroy(arrow.arrow);
                    }
                }

                // Arrows一覧
                arrows = new List<Arrow>();
                // ターゲットの位置を示すArrowを描画
                if (target != null && !target.isDead())
                {
                    Arrow arrow = new(Palette.CrewmateBlue);
                    arrow.arrow.SetActive(true);
                    arrow.Update(target.transform.position);
                    arrows.Add(arrow);
                    if (targetPositionText == null)
                    {
                        RoomTracker roomTracker = HudManager.Instance?.roomTracker;
                        if (roomTracker == null) return;
                        GameObject gameObject = UnityEngine.Object.Instantiate(roomTracker.gameObject);
                        UnityEngine.Object.DestroyImmediate(gameObject.GetComponent<RoomTracker>());
                        gameObject.transform.SetParent(HudManager.Instance.transform);
                        gameObject.transform.localPosition = new Vector3(0, -2.0f, gameObject.transform.localPosition.z);
                        gameObject.transform.localScale = Vector3.one * 1.0f;
                        targetPositionText = gameObject.GetComponent<TMPro.TMP_Text>();
                        targetPositionText.alpha = 1.0f;
                    }
                    PlainShipRoom room = Helpers.getPlainShipRoom(target);
                    targetPositionText.gameObject.SetActive(true);
                    int nearestPlayer = 0;
                    foreach (var p in PlayerControl.AllPlayerControls)
                    {
                        if (p != target)
                        {
                            float dist = Vector2.Distance(p.transform.position, target.transform.position);
                            if (dist < 7f) nearestPlayer += 1;
                        }
                    }
                    if (room != null)
                    {
                        targetPositionText.text = "<color=#8CFFFFFF>" + $"{target.name}({nearestPlayer})(" + DestroyableSingleton<TranslationController>.Instance.GetString(room.RoomId) + ")</color>";
                    }
                    else
                    {
                        targetPositionText.text = "<color=#8CFFFFFF>" + $"{target.name}({nearestPlayer})</color>";
                    }
                }
                else
                {
                    if (targetPositionText != null)
                    {
                        targetPositionText.text = "";
                    }
                }

                // タイマーに時間をセット
                updateTimer = arrowUpdateInterval;
            }
        }
        public static Sprite getArrowSprite()
        {
            if (!arrowSprite)
                arrowSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Arrow.png", 300f);
            return arrowSprite;
        }

        public static bool isAlive()
        {
            return Moriarty.allPlayers.Count(x => x.isAlive()) != 0;
        }
    }
}
