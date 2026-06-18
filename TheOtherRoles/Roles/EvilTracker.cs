using System.Collections.Generic;
using System.Linq;
using TheOtherRoles.MetaContext;
using TheOtherRoles.Objects;
using TheOtherRoles.Utilities;
using UnityEngine;
using static TheOtherRoles.Patches.PlayerControlFixedUpdatePatch;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    public class EvilTracker : RoleBase<EvilTracker>
    {
        public static Color color = Palette.ImpostorRed;

        public EvilTracker()
        {
            RoleId = roleId = RoleId.EvilTracker;
            target = null;
            futureTarget = null;
            currentTarget = null;
            arrows = [];
            impostorPositionText = [];
            targetPositionText = null;
            updateTimer = 0f;
        }

        public static float cooldown = 10f;
        public static bool resetTargetAfterMeeting = true;
        public static bool canSeeDeathFlash = true;
        public static bool canSeeTargetPosition = true;
        public static bool canSetTargetOnMeeting = true;
        public static bool canSeeTargetTasks = false;

        public float updateTimer = 0f;
        public static float arrowUpdateInterval = 0.5f;

        public PlayerControl target;
        public PlayerControl futureTarget;
        public PlayerControl currentTarget;
        public static Sprite trackerButtonSprite;
        public static Sprite arrowSprite;
        public List<Arrow> arrows = [];
        public Dictionary<string, TMPro.TMP_Text> impostorPositionText;
        public TMPro.TMP_Text targetPositionText;

        public static Sprite getEvilTrackerButtonSprite()
        {
            if (trackerButtonSprite) return trackerButtonSprite;
            trackerButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.TrackerButton.png", 115f);
            return trackerButtonSprite;
        }

        public static Sprite getArrowSprite()
        {
            if (!arrowSprite)
                arrowSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Arrow.png", 300f);
            return arrowSprite;
        }

        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason)
        {
            if (player != null && impostorPositionText != null && impostorPositionText.ContainsKey(player.Data.PlayerName))
                impostorPositionText[player.Data.PlayerName].text = "";
            if (player != null && target == player)
                targetPositionText.text = "";
        }

        public override void OnMeetingEnd(PlayerControl exiled = null)
        {
            // Reset here, else the task option would be useless
            if (resetTargetAfterMeeting) target = futureTarget;
            futureTarget = null;
        }

        public override void FixedUpdate()
        {
            if (PlayerControl.LocalPlayer != player) return;
            arrowUpdate();
            currentTarget = setTarget();
            setPlayerOutline(currentTarget, Palette.ImpostorRed);
        }

        public void arrowUpdate()
        {
            if (PlayerControl.LocalPlayer.Data.IsDead)
            {
                if (arrows?.Count > 0)
                {
                    foreach (var arrow in arrows)
                    {
                        if (arrow != null && arrow.arrow != null)
                            Object.Destroy(arrow.arrow);
                    }
                }
                if (impostorPositionText.Count > 0)
                {
                    foreach (var p in impostorPositionText.Values)
                        if (p != null) Object.Destroy(p.gameObject);
                }
                if (targetPositionText != null)
                    Object.Destroy(targetPositionText.gameObject);
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
                        Object.Destroy(arrow.arrow);
                    }
                }

                // Arrows一覧
                arrows = [];

                // インポスターの位置を示すArrowsを描画
                int count = 0;
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (p.Data.IsDead && impostorPositionText.ContainsKey(p.Data.PlayerName))
                    {
                        impostorPositionText[p.Data.PlayerName].text = "";
                        continue;
                    }
                    Arrow arrow;
                    if (p.Data.Role.IsImpostor && p != PlayerControl.LocalPlayer || p.isRole(RoleId.Spy) || Sidekick.players.Any(x => x.player == p && x.wasTeamRed)
                        || Jackal.players.Any(x => x.player == p && x.wasTeamRed))
                    {
                        arrow = new Arrow(Palette.ImpostorRed);
                        arrow.arrow.SetActive(true);
                        arrow.Update(p.transform.position);
                        arrows.Add(arrow);
                        count += 1;
                        if (!impostorPositionText.ContainsKey(p.Data.PlayerName))
                        {
                            RoomTracker roomTracker = FastDestroyableSingleton<HudManager>.Instance?.roomTracker;
                            if (roomTracker == null) return;
                            GameObject gameObject = Object.Instantiate(roomTracker.gameObject);
                            Object.DestroyImmediate(gameObject.GetComponent<RoomTracker>());
                            gameObject.transform.SetParent(FastDestroyableSingleton<HudManager>.Instance.transform);
                            gameObject.transform.localScale = Vector3.one * 1.0f;
                            TMPro.TMP_Text positionText = gameObject.GetComponent<TMPro.TMP_Text>();
                            positionText.alpha = 1.0f;
                            impostorPositionText.Add(p.Data.PlayerName, positionText);
                        }
                        impostorPositionText[p.Data.PlayerName].transform.localPosition = new Vector3(0, -2.0f + 0.25f * count, impostorPositionText[p.Data.PlayerName].transform.localPosition.z);
                        PlainShipRoom room = Helpers.getPlainShipRoom(p);
                        impostorPositionText[p.Data.PlayerName].gameObject.SetActive(true);
                        if (room != null)
                            impostorPositionText[p.Data.PlayerName].text = "<color=#FF1919FF>" + $"{p.Data.PlayerName}(" + FastDestroyableSingleton<TranslationController>.Instance.GetString(room.RoomId) + ")</color>";
                        else
                            impostorPositionText[p.Data.PlayerName].text = "";
                    }
                }

                // ターゲットの位置を示すArrowを描画
                if (target != null && !target.Data.IsDead)
                {
                    Arrow arrow = new(Palette.CrewmateBlue);
                    arrow.arrow.SetActive(true);
                    arrow.Update(target.transform.position);
                    arrows.Add(arrow);
                    if (targetPositionText == null)
                    {
                        RoomTracker roomTracker = FastDestroyableSingleton<HudManager>.Instance?.roomTracker;
                        if (roomTracker == null) return;
                        GameObject gameObject = Object.Instantiate(roomTracker.gameObject);
                        Object.DestroyImmediate(gameObject.GetComponent<RoomTracker>());
                        gameObject.transform.SetParent(FastDestroyableSingleton<HudManager>.Instance.transform);
                        gameObject.transform.localPosition = new Vector3(0, -2.0f, gameObject.transform.localPosition.z);
                        gameObject.transform.localScale = Vector3.one * 1.0f;
                        targetPositionText = gameObject.GetComponent<TMPro.TMP_Text>();
                        targetPositionText.alpha = 1.0f;
                    }
                    PlainShipRoom room = Helpers.getPlainShipRoom(target);
                    targetPositionText.gameObject.SetActive(true);
                    if (room != null)
                        targetPositionText.text = "<color=#8CFFFFFF>" + $"{target.Data.PlayerName}(" + FastDestroyableSingleton<TranslationController>.Instance.GetString(room.RoomId) + ")</color>";
                    else
                        targetPositionText.text = "";
                }
                else
                    if (targetPositionText != null)
                        targetPositionText.text = "";

                // タイマーに時間をセット
                updateTimer = arrowUpdateInterval;
            }
        }

        public void clearAllArrow()
        {
            if (arrows?.Count > 0)
            {
                foreach (var arrow in arrows)
                    if (arrow != null && arrow.arrow != null) arrow.arrow.SetActive(false);
            }
            if (impostorPositionText.Count > 0)
            {
                foreach (var p in impostorPositionText.Values)
                    if (p != null) p.gameObject.SetActive(false);
            }
            if (targetPositionText != null) targetPositionText.gameObject.SetActive(false);
        }

        public override void ResetRole(bool isShifted)
        {
            clearAllArrow();
            target = null;
            futureTarget = null;
        }

        public static void clearAndReload()
        {
            cooldown = CustomOptionHolder.evilTrackerCooldown.getFloat();
            resetTargetAfterMeeting = CustomOptionHolder.evilTrackerResetTargetAfterMeeting.getBool();
            canSeeDeathFlash = CustomOptionHolder.evilTrackerCanSeeDeathFlash.getBool();
            canSeeTargetPosition = CustomOptionHolder.evilTrackerCanSeeTargetPosition.getBool();
            canSeeTargetTasks = CustomOptionHolder.evilTrackerCanSeeTargetTask.getBool();
            canSetTargetOnMeeting = CustomOptionHolder.evilTrackerCanSetTargetOnMeeting.getBool();
            players = [];
        }
    }
}
