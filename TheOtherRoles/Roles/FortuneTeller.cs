using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using TheOtherRoles.MetaContext;
using TheOtherRoles.Modules;
using TheOtherRoles.Objects;
using TheOtherRoles.Utilities;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    [TORRPCHolder]
    public class FortuneTeller : RoleBase<FortuneTeller>
    {
        public PlayerControl divineTarget;
        public static Color color = new Color32(175, 198, 241, byte.MaxValue);

        public FortuneTeller()
        {
            RoleId = roleId = RoleId.FortuneTeller;
            numUsed = 0;
            isRevealed = false;
            pageIndex = 1;
            divinedFlag = false;
            divineTarget = null;
            acTokenImpostor = null;
            playerStatus = [];
            progress = [];
        }

        public enum DivineResults
        {
            BlackWhite,
            Team,
            Role,
        }

        public static int numTasks;
        public static DivineResults divineResult;
        public static float duration;
        public static float distance;
        public static bool revealOnImp;

        public static bool meetingFlag = false;

        public Dictionary<byte, float> progress = [];
        public Dictionary<byte, bool> playerStatus = [];
        public bool divinedFlag = false;
        public bool isRevealed = false;
        public int numUsed = 0;

        private static Sprite leftButtonSprite;
        private static Sprite rightButtonSprite;

        public static List<Arrow> arrows = [];
        public static float updateTimer = 0f;

        public int pageIndex = 1;

        public AchievementToken<(bool divined, bool cleared)> acTokenImpostor = null;

        public override void PostInit()
        {
            if (PlayerControl.LocalPlayer != player) return;
            acTokenImpostor ??= new("fortuneTeller.challenge", (false, false), (val, _) => val.cleared);
        }

        public override void ResetRole(bool isShifted)
        {
            TORMapOptions.resetPoolables();
            resetArrow();
        }

        public override GUIContext ProgressContext { get => ProgressGUI.Holder(divineTarget != null ? ProgressGUI.OneLineText(ModTranslation.getString("roleInfoFortuneTellerTargetFull") + ": " + (divineTarget?.Data.PlayerName ?? "")) : null); }

        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason)
        {
            if (player == this.player) resetArrow();
        }

        public override void FixedUpdate()
        {
            fortuneTellerUpdate();
            impostorArrowUpdate();
        }

        public override void OnMeetingEnd(PlayerControl exiled = null)
        {
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(5.0f, new Action<float>((p) =>
            {
                if (p == 1f)
                    meetingFlag = false;
            })));

            foreach (var p in PlayerControl.AllPlayerControls)
                playerStatus[p.PlayerId] = !p.Data.IsDead;

            if (PlayerControl.LocalPlayer == player)
                acTokenImpostor.Value.cleared |= exiled != null && acTokenImpostor.Value.divined && divineTarget != null
                    && divineTarget.PlayerId == exiled.PlayerId;
        }

        public static RemoteProcess<(byte fortuneTellerId, byte targetId)> UseDivine = new("FortuneTellerUseDivine", (message, _) => RPCProcedure.fortuneTellerUsedDivine(message.fortuneTellerId, message.targetId));

        private void fortuneTellerUpdate()
        {
            if (player == PlayerControl.LocalPlayer && !meetingFlag)
            {
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (!progress.ContainsKey(p.PlayerId)) progress[p.PlayerId] = 0f;
                    if (p.Data.IsDead) continue;
                    var fortuneTeller = PlayerControl.LocalPlayer;
                    float distance = Vector3.Distance(p.transform.position, fortuneTeller.transform.position);
                    // 障害物判定
                    bool anythingBetween = PhysicsHelpers.AnythingBetween(p.GetTruePosition(), fortuneTeller.GetTruePosition(), Constants.ShipAndObjectsMask, false);
                    if (!anythingBetween && distance <= FortuneTeller.distance && progress[p.PlayerId] < duration) progress[p.PlayerId] += Time.fixedDeltaTime;
                }
            }
        }

        public static void resetArrow() {
            foreach (Arrow arrow in arrows)
            {
                if (arrow?.arrow != null)
                {
                    arrow.arrow.SetActive(false);
                    UnityEngine.Object.Destroy(arrow.arrow);
                }
            }
        }

        public void impostorArrowUpdate()
        {
            if (!PlayerControl.LocalPlayer.Data.Role.IsImpostor && arrows != null && arrows.Count > 0) {
                resetArrow();
                return;
            }
            if (PlayerControl.LocalPlayer.Data.Role.IsImpostor)
            {
                // 前フレームからの経過時間をマイナスする
                updateTimer -= Time.fixedDeltaTime;

                // 1秒経過したらArrowを更新
                if (updateTimer <= 0.0f)
                {
                    resetArrow();
                    // Arrow一覧
                    arrows = [];

                    foreach (var p in players)
                    {
                        if (p == null || p.player == null) continue;
                        if (p.player.Data.IsDead) continue;
                        if (!p.isRevealed) continue;

                        Arrow arrow = new(color);
                        arrow.arrow.SetActive(true);
                        arrow.Update(p.player.transform.position);
                        arrows.Add(arrow);
                    }

                    // タイマーに時間をセット
                    updateTimer = 1f;
                }
                else
                    arrows.Do(x => x.Update());
            }
        }

        public static Sprite getLeftButtonSprite()
        {
            if (leftButtonSprite) return leftButtonSprite;
            leftButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.FortuneTellerButtonLeft.png", 130f);
            return leftButtonSprite;
        }

        public static Sprite getRightButtonSprite()
        {
            if (rightButtonSprite) return rightButtonSprite;
            rightButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.FortuneTellerButtonRight.png", 130f);
            return rightButtonSprite;
        }

        public static bool isCompletedNumTasks(PlayerControl p)
        {
            var (tasksCompleted, tasksTotal) = TasksHandler.taskInfo(p.Data);
            return tasksCompleted >= numTasks;
        }

        public override void OnMeetingStart()
        {
            meetingFlag = true;
        }

        public void setDivinedFlag(bool flag)
        {
            divinedFlag = flag;
        }

        public bool canDivine(byte index)
        {
            bool status = true;
            if (playerStatus.ContainsKey(index))
                status = playerStatus[index];
            return (progress.ContainsKey(index) && progress[index] >= duration) || !status;
        }

        public static void fortuneTellerMessage(string message, float duration, Color color)
        {
            var messageText = Helpers.CreateAndShowNotification(message, color, new Vector3(0f, 1f, -20f), spr: Illustrations.GetRoleIcon(RoleId.FortuneTeller).GetSprite());
            messageText.alphaTimer = duration;
            messageText.AdjustNotification();
        }

        public void divine(PlayerControl p)
        {
            if (p == null) return;
            string msg = "";
            Color color = Color.white;

            if (divineResult == DivineResults.BlackWhite)
            {
                if (!Helpers.isNeutral(p) && !p.Data.Role.IsImpostor)
                {
                    msg = string.Format(ModTranslation.getString("divineMessageIsCrew"), p.Data.PlayerName);
                    color = Color.white;
                }
                else
                {
                    msg = string.Format(ModTranslation.getString("divineMessageIsntCrew"), p.Data.PlayerName);
                    color = Palette.ImpostorRed;
                }
            }

            else if (divineResult == DivineResults.Team)
            {
                if (!Helpers.isNeutral(p) && !p.Data.Role.IsImpostor)
                {
                    msg = string.Format(ModTranslation.getString("divineMessageTeamCrew"), p.Data.PlayerName);
                    color = Color.white;
                }
                else if (Helpers.isNeutral(p))
                {
                    msg = string.Format(ModTranslation.getString("divineMessageTeamNeutral"), p.Data.PlayerName);
                    color = Color.yellow;
                }
                else
                {
                    msg = string.Format(ModTranslation.getString("divineMessageTeamImp"), p.Data.PlayerName);
                    color = Palette.ImpostorRed;
                }
            }

            else if (divineResult == DivineResults.Role)
                msg = $"{p.Data.PlayerName} was The {string.Join(" ", RoleInfo.getRoleInfoForPlayer(p, false, true).Select(x => Helpers.cs(x.color, x.name)))}";

            if (p.Data.Role.IsImpostor && revealOnImp)
                msg += "<br>" + ModTranslation.getString("fortuneTellerRevealNotification");

            if (!string.IsNullOrWhiteSpace(msg))
                fortuneTellerMessage(msg, 7f, color);

            if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(DestroyableSingleton<HudManager>.Instance.TaskCompleteSound, false, 0.8f);
            numUsed += 1;

            // 占いを実行したことで発火される処理を他クライアントに通知
            UseDivine.Invoke((PlayerControl.LocalPlayer.PlayerId, p.PlayerId));
        }

        public static void clearAndReload()
        {
            meetingFlag = true;
            duration = CustomOptionHolder.fortuneTellerDuration.getFloat();
            if (arrows != null)
                foreach (Arrow arrow in arrows)
                    if (arrow?.arrow != null)
                        UnityEngine.Object.Destroy(arrow.arrow);
            arrows = [];
            numTasks = (int)CustomOptionHolder.fortuneTellerNumTasks.getFloat();
            distance = CustomOptionHolder.fortuneTellerDistance.getFloat();
            divineResult = (DivineResults)CustomOptionHolder.fortuneTellerResults.getSelection();
            revealOnImp = CustomOptionHolder.fortuneTellerRevealOnImpDivine.getBool();
            players = [];
        }

#if WINDOWS
        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
#else
        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin))]
#endif
        class IntroCutsceneOnDestroyPatch
        {
            public static void Prefix(IntroCutscene __instance)
            {
                FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(16.2f, new Action<float>((p) =>
                {
                    if (p == 1f)
                        meetingFlag = false;
                })));
            }
        }
    }
}
