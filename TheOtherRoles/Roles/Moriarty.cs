using System;
using System.Collections.Generic;
using TheOtherRoles.Modules;
using TheOtherRoles.Objects;
using UnityEngine;
using static TheOtherRoles.Patches.PlayerControlFixedUpdatePatch;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    [TORRPCHolder]
    public class Moriarty : RoleBase<Moriarty>
    {
        public Moriarty()
        {
            RoleId = roleId = RoleId.Moriarty;
            tmpTarget = null;
            target = null;
            currentTarget = null;
            killTarget = null;
            counter = 0;
        }

        static public IEnumerable<HelpSprite> GetHelpSprites() => [new(getBrainwashIcon(), "moriartyBrainwashHint")];

        public static RemoteProcess<(byte playerId, byte moriartyId)> Brainwash = new("MoriartyBrainwash", (message, _) =>
        {
            var p = Helpers.playerById(message.playerId);
            var player = Helpers.playerById(message.moriartyId);
            var moriarty = getRole(player);
            if (player == null || moriarty == null) return;
            moriarty.target = p;
            brainwashed.Add(p);
        });

        public static RemoteProcess<(byte targetId, byte playerId)> UpdateCounter = new("MoriartyKill", (message, _) =>
        {
            PlayerControl target = Helpers.playerById(message.targetId);
            PlayerControl player = Helpers.playerById(message.playerId);
            var moriarty = getRole(player);
            if (moriarty == null || player == null || target == null) return;
            GameHistory.overrideDeathReasonAndKiller(target, DeadPlayer.CustomDeathReason.BrainwashedKilled, player);
            if (PlayerControl.LocalPlayer == moriarty.target)
            {
                if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(target.KillSfx, false, 0.8f);
            }
            moriarty.counter += 1;
            if (target.isRole(RoleId.Sherlock)) moriarty.counter += sherlockAddition;
            hasKilled = true;
            if (numberToWin == moriarty.counter) triggerMoriartyWin = true;
        });

        public static Color color = Color.green;

        public PlayerControl tmpTarget;
        public PlayerControl target;
        public PlayerControl currentTarget;
        public PlayerControl killTarget;
        public static List<PlayerControl> brainwashed = [];

        public int counter;

        public static float brainwashTime = 2f;
        public static float brainwashCooldown = 30f;
        public static int numberToWin = 3;
        public static bool indicateKills = false;
        public static bool hasKilled = false;
        public static int sherlockAddition = 2;

        public static Sprite brainwashIcon;

        public List<Arrow> arrows = [];
        public float updateTimer = 0f;
        public static float arrowUpdateInterval = 0.5f;
        public TMPro.TMP_Text targetPositionText = null;

        public static bool triggerMoriartyWin = false;

        public static int countLovers()
        {
            int counter = 0;
            foreach (var player in allPlayers)
                if (player.isLovers()) counter += 1;
            return counter;
        }

        public static Sprite getBrainwashIcon()
        {
            if (brainwashIcon) return brainwashIcon;
            brainwashIcon = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.BrainwashButton.png", 115f);
            return brainwashIcon;
        }

        public override void OnMeetingEnd(PlayerControl exiled = null)
        {
            target = null;
        }

        public override void ResetRole(bool isShifted)
        {
            clearAllArrow();
            target = null;
            brainwashed.Clear();
        }

        public override void OnFinishShipStatusBegin()
        {
            HudManagerStartPatch.moriartyBrainwashButton.Timer = brainwashCooldown;
        }

        public override void FixedUpdate()
        {
            if (PlayerControl.LocalPlayer == player)
            {
                arrowUpdate();
                if (!PlayerControl.LocalPlayer.Data.IsDead)
                {
                    currentTarget = setTarget();
                    var untargetablePlayers = new List<PlayerControl>();
                    if (SchrodingersCat.team == SchrodingersCat.Team.Moriarty) untargetablePlayers.AddRange(SchrodingersCat.allPlayers);
                    if (target != null) killTarget = setTarget(untargetablePlayers: untargetablePlayers, targetingPlayer: target);
                    else killTarget = null;
                    setPlayerOutline(currentTarget, color);
                }
            }
        }

        public void arrowUpdate()
        {
            if (PlayerControl.LocalPlayer.Data.IsDead)
            {
                if (arrows.Count > 0)
                {
                    foreach (var arrow in arrows)
                        if (arrow != null && arrow.arrow != null) UnityEngine.Object.Destroy(arrow.arrow);
                }
                if (targetPositionText != null) UnityEngine.Object.Destroy(targetPositionText.gameObject);
                target = null;
                return;
            }

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
                arrows = [];
                // ターゲットの位置を示すArrowを描画
                if (target != null && !target.Data.IsDead)
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
                    targetPositionText.gameObject.SetActive(true);
                    int nearestPlayer = 0;
                    foreach (var p in PlayerControl.AllPlayerControls)
                    {
                        if (p != target && !p.Data.IsDead)
                        {
                            float dist = Vector2.Distance(p.transform.position, target.transform.position);
                            if (dist < 7f) nearestPlayer += 1;
                        }
                    }
                    targetPositionText.text = "<color=#8CFFFFFF>" + $"{target.Data.PlayerName}({nearestPlayer})(" + Helpers.GetRoomName(target) + ")</color>";
                }
                else {
                    if (targetPositionText != null)
                        targetPositionText.text = "";
                }

                // タイマーに時間をセット
                updateTimer = arrowUpdateInterval;
            }
        }

        public void clearAllArrow()
        {
            if (arrows.Count > 0)
            {
                foreach (var arrow in arrows)
                    if (arrow != null && arrow.arrow != null) arrow.arrow.SetActive(false);
            }
            if (targetPositionText != null) targetPositionText.gameObject.SetActive(false);
            var obj = GameObject.Find("MoriartyText(Clone)");
            if (obj != null) obj.SetActive(false);
        }

        public void generateBrainwashText()
        {
            TMPro.TMP_Text text;
            RoomTracker roomTracker = HudManager.Instance?.roomTracker;
            GameObject gameObject = UnityEngine.Object.Instantiate(roomTracker.gameObject);
            UnityEngine.Object.DestroyImmediate(gameObject.GetComponent<RoomTracker>());
            gameObject.transform.SetParent(HudManager.Instance.transform);
            gameObject.transform.localPosition = new Vector3(0, -1.3f, gameObject.transform.localPosition.z);
            gameObject.transform.localScale = Vector3.one * 3f;
            text = gameObject.GetComponent<TMPro.TMP_Text>();
            text.name = "MoriartyText(Clone)";
            PlayerControl tmpP = target;
            bool done = false;
            HudManager.Instance.StartCoroutine(Effects.Lerp(brainwashTime, new Action<float>((p) =>
            {
                if (done)
                    return;
                if (target == null || MeetingHud.Instance != null || p == 1f)
                {
                    if (text != null && text.gameObject) UnityEngine.Object.Destroy(text.gameObject);
                    if (target == tmpP) target = null;
                    done = true;
                    return;
                }
                else
                {
                    string message = (brainwashTime - p * brainwashTime).ToString("0");
                    bool even = (int)(p * brainwashTime / 0.25f) % 2 == 0; // Bool flips every 0.25 seconds
                                                                                      // string prefix = even ? "<color=#555555FF>" : "<color=#FFFFFFFF>";
                    string prefix = "<color=#555555FF>";
                    text.text = prefix + message + "</color>";
                    if (text != null) text.color = even ? Color.yellow : Color.red;
                }
            })));
        }

        public static void clearAndReload()
        {
            brainwashed = [];
            triggerMoriartyWin = false;
            hasKilled = false;
            brainwashCooldown = CustomOptionHolder.moriartyBrainwashCooldown.getFloat();
            brainwashTime = CustomOptionHolder.moriartyBrainwashTime.getFloat();
            numberToWin = (int)CustomOptionHolder.moriartyNumberToWin.getFloat();
            sherlockAddition = (int)CustomOptionHolder.moriartySherlockAddition.getFloat();
            indicateKills = CustomOptionHolder.moriartyKillIndicate.getBool();
            players = [];
        }
    }
}
