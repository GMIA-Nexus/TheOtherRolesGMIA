using System;
using System.Collections.Generic;
using System.Linq;
using Hazel;
using TheOtherRoles.Utilities;
using UnityEngine;
using static TheOtherRoles.Patches.PlayerControlFixedUpdatePatch;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    [TORRPCHolder]
    public class PlagueDoctor : RoleBase<PlagueDoctor>
    {
        public PlagueDoctor()
        {
            RoleId = roleId = RoleId.PlagueDoctor;
            syncTimer = 0f;
        }

        public static Color color = new Color32(255, 192, 0, byte.MaxValue);

        public static Dictionary<int, PlayerControl> infected;
        public static Dictionary<int, float> progress;
        public static Dictionary<int, bool> dead;
        public static TMPro.TMP_Text statusText = null;
        public static bool triggerPlagueDoctorWin = false;
        public static float syncTimer = 0f;

        public static PlayerControl currentTarget;
        public static int numInfections = 0;
        public static bool meetingFlag = false;

        public static float infectCooldown = 10f;
        public static int maxInfectable;
        public static float infectDistance = 1f;
        public static float infectDuration = 5f;
        public static float immunityTime = 10f;
        public static bool infectKiller = true;
        public static bool canWinDead = true;

        public static Sprite plagueDoctorIcon;

        public static RemoteProcess<byte> SetInfected = RemotePrimitiveProcess.OfByte("PlagueDoctorSetInfected", (message, _) =>
        {
            var p = Helpers.playerById(message);
            if (allPlayers.Count <= 0) return;
            if (!infected.ContainsKey(message))
                infected[message] = p;
        });

        public static RemoteProcess TriggerWin = new("PlagueDoctorWin", (_) =>
        {
            triggerPlagueDoctorWin = true;
            var pd = allPlayers.FirstOrDefault();
            if (pd == null) return;
            var livingPlayers = PlayerControl.AllPlayerControls.GetFastEnumerator().ToArray().Where(p => !p.isRole(RoleId.PlagueDoctor) && !p.Data.IsDead);
            foreach (PlayerControl p in livingPlayers)
            {
                if (p.isRole(RoleId.NekoKabocha)) NekoKabocha.getRole(p).otherKiller = pd;
                if (!p.Data.IsDead) p.Exiled();
                GameHistory.overrideDeathReasonAndKiller(p, DeadPlayer.CustomDeathReason.Disease, pd);
            }
        });

        public override void OnMeetingEnd(PlayerControl exiled = null)
        {
            updateDead();

            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(immunityTime, new Action<float>((p) =>
            { // 5秒後から感染開始
                if (p == 1f)
                    meetingFlag = false;
            })));
        }

        public override void OnMeetingStart()
        {
            meetingFlag = true;
        }

        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason)
        {
            if (this.player == player && statusText != null)
            {
                UnityEngine.Object.Destroy(statusText);
                statusText = null;
            }
        }

        public void infectionUpdate()
        {
            if (MeetingHud.Instance != null)
                if (statusText != null)
                    statusText.gameObject.SetActive(false);

            if (PlayerControl.LocalPlayer == player || PlayerControl.LocalPlayer.Data.IsDead &&
                !Busker.players.Any(x => x.player == PlayerControl.LocalPlayer && x.pseudocideFlag))
            {
                if (statusText == null)
                {
                    GameObject gameObject = UnityEngine.Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance?.roomTracker.gameObject);
                    gameObject.transform.SetParent(FastDestroyableSingleton<HudManager>.Instance.transform);
                    gameObject.SetActive(true);
                    UnityEngine.Object.DestroyImmediate(gameObject.GetComponent<RoomTracker>());
                    statusText = gameObject.GetComponent<TMPro.TMP_Text>();
                    gameObject.transform.localPosition = new Vector3(-2.7f, -0.1f - PlayerControl.AllPlayerControls.ToArray().Select(x => !dead.ContainsKey(x.PlayerId)).Count() * 0.07f, gameObject.transform.localPosition.z);

                    statusText.transform.localScale = new Vector3(1f, 1f, 1f);
                    statusText.fontSize = 1.5f;
                    statusText.fontSizeMin = 1.5f;
                    statusText.fontSizeMax = 1.5f;
                    statusText.alignment = TMPro.TextAlignmentOptions.BottomLeft;
                    statusText.alpha = byte.MaxValue;
                }

                statusText.gameObject.SetActive(true);
                string text = $"[{ModTranslation.getString("plagueDoctorProgress")}]\n";

                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (p == player) continue;
                    if (dead.ContainsKey(p.PlayerId) && dead[p.PlayerId]) continue;
                    text += $"{p.Data.PlayerName}: ";
                    if (infected.ContainsKey(p.PlayerId))
                        text += Helpers.cs(Color.red, ModTranslation.getString("plagueDoctorInfectedText"));
                    else
                    {
                        // データが無い場合は作成する
                        if (!progress.ContainsKey(p.PlayerId))
                            progress[p.PlayerId] = 0f;
                        text += getProgressString(progress[p.PlayerId]);
                    }
                    text += "\n";
                }

                statusText.text = text;
            }

            if (PlayerControl.LocalPlayer == player)
            {
                if (canWinDead || !player.Data.IsDead)
                {
                    List<PlayerControl> newInfected = [];
                    syncTimer += Time.fixedDeltaTime;
                    bool canSync = syncTimer >= 0.2f;
                    if (canSync) syncTimer = 0f;

                    foreach (PlayerControl target in PlayerControl.AllPlayerControls)
                    {
                        if (target == player || target.Data.IsDead || infected.ContainsKey(target.PlayerId) || target.inVent) continue;
                        if (!progress.ContainsKey(target.PlayerId)) progress[target.PlayerId] = 0f;

                        if (!meetingFlag)
                        {
                            foreach (PlayerControl source in infected.Values.ToList())
                            {
                                if (source.Data.IsDead) continue;
                                float distance = Vector3.Distance(source.transform.position, target.transform.position);
                                bool anythingBetween = PhysicsHelpers.AnythingBetween(source.GetTruePosition(), target.GetTruePosition(), Constants.ShipAndObjectsMask, false);

                                if (distance <= infectDistance && !anythingBetween)
                                {
                                    progress[target.PlayerId] += Time.fixedDeltaTime;

                                    if (canSync)
                                    {
                                        // 他のクライアントに進行状況を通知する
                                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PlagueDoctorUpdateProgress, SendOption.Reliable, -1);
                                        writer.Write(target.PlayerId);
                                        writer.Write(progress[target.PlayerId]);
                                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                                    }

                                    // Only update a player's infection once per FixedUpdate
                                    break;
                                }
                            }
                        }

                        if (progress[target.PlayerId] > infectDuration)
                            newInfected.Add(target);
                    }

                    foreach (PlayerControl p in newInfected)
                    {
                        byte targetId = p.PlayerId;
                        SetInfected.Invoke(targetId);
                    }
                }
            }
        }

        void plagueDoctorSetTarget()
        {
            if (PlayerControl.LocalPlayer != player) return;
            if (!player.Data.IsDead && numInfections > 0)
            {
                currentTarget = setTarget(untargetablePlayers: infected.Values.ToList());
                setPlayerOutline(currentTarget, color);
            }
        }

        public override void OnDeath(PlayerControl killer = null)
        {
            if (infectKiller && killer != null && PlayerControl.LocalPlayer == player)
            {
                byte targetId = killer.PlayerId;
                SetInfected.Invoke(targetId);
            }
        }

        public override void FixedUpdate()
        {
            plagueDoctorSetTarget();
            infectionUpdate();
            checkWinStatus();
        }

        public static Sprite getSyringeIcon()
        {
            if (plagueDoctorIcon) return plagueDoctorIcon;
            plagueDoctorIcon = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.InfectButton.png", 115f);
            return plagueDoctorIcon;
        }

        public static void updateDead()
        {
            if (statusText != null) UnityEngine.Object.Destroy(statusText);
            statusText = null; // Update positions!
            foreach (var pc in PlayerControl.AllPlayerControls.GetFastEnumerator())
                dead[pc.PlayerId] = pc.Data.IsDead;
        }

        public static string getProgressString(float progress)
        {
            // Go from green -> yellow -> red based on infection progress
            Color color;
            var prog = progress / infectDuration;
            if (prog < 0.5f)
                color = Color.Lerp(Color.green, Color.yellow, prog * 2);
            else
                color = Color.Lerp(Color.yellow, Color.red, prog * 2 - 1);

            float progPercent = prog * 100;
            return Helpers.cs(color, $"{progPercent.ToString("F1")}%");
        }

        public void checkWinStatus()
        {
            if (!(canWinDead || hasAlivePlayers) || PlayerControl.LocalPlayer != player) return;

            bool winFlag = true;
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (p.Data.IsDead) continue;
                if (p == player) continue;
                if (!infected.ContainsKey(p.PlayerId))
                {
                    winFlag = false;
                    break;
                }
            }

            if (winFlag)
            {
                TriggerWin.Invoke();
            }
        }

        public override void ResetRole(bool isShifted)
        {
            if (statusText != null) UnityEngine.Object.Destroy(statusText);
            statusText = null;
        }

        public static void clearAndReload()
        {
            infectCooldown = CustomOptionHolder.plagueDoctorInfectCooldown.getFloat();
            maxInfectable = Mathf.RoundToInt(CustomOptionHolder.plagueDoctorNumInfections.getFloat());
            infectDistance = CustomOptionHolder.plagueDoctorDistance.getFloat();
            infectDuration = CustomOptionHolder.plagueDoctorDuration.getFloat();
            immunityTime = CustomOptionHolder.plagueDoctorImmunityTime.getFloat();
            infectKiller = CustomOptionHolder.plagueDoctorInfectKiller.getBool();
            canWinDead = CustomOptionHolder.plagueDoctorWinDead.getBool();
            meetingFlag = false;
            triggerPlagueDoctorWin = false;
            numInfections = maxInfectable;
            currentTarget = null;
            infected = [];
            progress = [];
            dead = [];
            if (statusText != null) UnityEngine.Object.Destroy(statusText);
            statusText = null;
            syncTimer = 0f;
            players = [];
        }
    }
}
