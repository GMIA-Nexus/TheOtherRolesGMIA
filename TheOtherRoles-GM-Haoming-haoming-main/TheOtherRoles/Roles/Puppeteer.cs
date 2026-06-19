using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Hazel;
using TheOtherRoles.Objects;
using UnityEngine;
using static TheOtherRoles.Patches.PlayerControlFixedUpdatePatch;
using static TheOtherRoles.Patches.SubmergedPatch;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles
{
    [HarmonyPatch]
    public class Puppeteer : RoleBase<Puppeteer>
    {
        public static Color color = Palette.Purple;
        public static int counter = 0;
        public static int numKills { get { return (int)CustomOptionHolder.puppeteerNumKills.getFloat(); } }
        public static float sampleDuration { get { return CustomOptionHolder.puppeteerSampleDuration.getFloat(); } }
        public static bool canControlDummyEvenIfDead { get { return CustomOptionHolder.puppeteerCanControlDummyEvenIfDead.getBool(); } }
        public static int penaltyOnDeath { get { return (int)CustomOptionHolder.puppeteerPenaltyOnDeath.getFloat(); } }
        public static bool losesSenriganOnDeath { get { return CustomOptionHolder.puppeteerLosesSenriganOnDeath.getBool(); } }
        public static bool triggerPuppeteerWin = false;
        public static bool isActive = false;
        public static bool canSpawn = true;
        public static PlayerControl dummy = null;
        public static PlayerControl target = null;
        public static PlayerControl currentTarget = null;
        public static PlayerControl tmpTarget = null;
        public static bool stealthed = false;
        public static CustomButton sampleButton;
        public static Sprite sampleButtonSprite;
        public static CustomButton puppeteerButton;
        public static Sprite puppeteerButtonSprite;
        public static List<Arrow> arrows = new();
        public static float arrowUpdateInterval = 0.5f;
        public static float updateTimer = 0f;
        public static float posUpdateTimer = 0f;
        public static AudioClip laugh;
        public static bool soundFlag;

        public static TMPro.TMP_Text puppeteerText;

        public Puppeteer()
        {
            RoleType = roleId = RoleType.Puppeteer;
        }

        public override void OnMeetingStart()
        {
            bool isAlive = Puppeteer.allPlayers.FindAll(x => x.isAlive()).Count >= 1;
            if (soundFlag && isAlive)
            {
                SoundManager.Instance.PlaySound(laugh, false, 1f);
            }
            soundFlag = false;
            if (!isAlive && (PlayerControl.LocalPlayer.isImpostor() || PlayerControl.LocalPlayer.isRole(RoleType.Jackal) || PlayerControl.LocalPlayer.isRole(RoleType.JekyllAndHyde) || PlayerControl.LocalPlayer.isRole(RoleType.Moriarty)))
            {
                string msg = $"人形遣いのカウント数 {counter}/{numKills}";
                if (AmongUsClient.Instance.AmClient && FastDestroyableSingleton<HudManager>.Instance)
                {
                    FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, msg);
                }
            }

        }

        public override void OnMeetingEnd()
        {
            target = null;
            canSpawn = false;
            isActive = false;
            if (PlayerControl.LocalPlayer.isRole(RoleType.Puppeteer))
            {
                switchStealth(false);
            }
        }
        public override void FixedUpdate()
        {
            if (PlayerControl.LocalPlayer.isRole(RoleType.Puppeteer))
            {
                currentTarget = setTarget();
                setPlayerOutline(currentTarget, Puppeteer.color);
                arrowUpdate();
                syncDummyPos();
            }
        }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null)
        {
            counter -= penaltyOnDeath;
            setOpacity(player, 1f);
        }
        public override void OnFinishShipStatusBegin() { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static Sprite getSampleButtonSprite()
        {
            if (sampleButtonSprite) return sampleButtonSprite;
            sampleButtonSprite = ModTranslation.getImage("SampleButton.png", 115f);
            return sampleButtonSprite;
        }
        public static Sprite getPuppeteerButtonSprite()
        {
            if (puppeteerButtonSprite) return puppeteerButtonSprite;
            puppeteerButtonSprite = Helpers.loadSpriteFromAssetBundle("PuppeteerButton.png");
            return puppeteerButtonSprite;
        }
        public static void MakeButtons(HudManager hm)
        {
            sampleButton = new CustomButton(
                // OnClick
                () =>
                {
                    if (currentTarget != null)
                    {
                        tmpTarget = currentTarget;
                        sampleButton.HasEffect = true;
                        puppeteerButton.MaxTimer = 0f;
                        puppeteerButton.Timer = 0f;
                    }
                },
                // HasButton
                () => { return PlayerControl.LocalPlayer.isRole(RoleType.Puppeteer) && (PlayerControl.LocalPlayer.isAlive() || canControlDummyEvenIfDead); },
                // CouldUse
                () =>
                {
                    if (sampleButton.isEffectActive && tmpTarget != currentTarget)
                    {
                        tmpTarget = null;
                        sampleButton.Timer = 0f;
                        sampleButton.isEffectActive = false;
                    }

                    return PlayerControl.LocalPlayer.CanMove && currentTarget != null;
                },
                // OnMeetingEnds
                () =>
                {
                    sampleButton.Timer = sampleButton.MaxTimer;
                    sampleButton.isEffectActive = false;
                    target = null;
                    tmpTarget = null;
                },
                getSampleButtonSprite(),
                CustomButton.ButtonPositions.upperRowRight,
                hm,
                hm.KillButton,
                KeyCode.G,
                true,
                sampleDuration,
                // OnEffectsEnd
                () =>
                {
                    if (tmpTarget != null)
                    {
                        target = tmpTarget;
                        canSpawn = true;
                    }

                    tmpTarget = null;
                    sampleButton.Timer = sampleButton.MaxTimer;

                }
            )
            {
                buttonText = ""
            };

            puppeteerButton = new CustomButton(
                // OnClick
                () =>
                {
                    if (canSpawn)
                    {
                        spawnDummy();
                        switchStealth(true);
                    }
                    else
                    {
                        switchStealth(!stealthed);
                    }
                },
                // HasButton
                () => { return PlayerControl.LocalPlayer.isRole(RoleType.Puppeteer) && (PlayerControl.LocalPlayer.isAlive() || canControlDummyEvenIfDead) && target != null; },
                // CouldUse
                () =>
                {
                    if (puppeteerText != null)
                    {
                        puppeteerText.text = $"{counter}/{numKills}";
                    }
                    return true;
                },
                // OnMeetingEnds
                () =>
                {
                    puppeteerButton.Timer = puppeteerButton.MaxTimer;
                },
                getPuppeteerButtonSprite(),
                CustomButton.ButtonPositions.upperRowCenter,
                hm,
                hm.UseButton,
                KeyCode.F,
                false
            )
            {
                buttonText = ""
            };
            puppeteerText = GameObject.Instantiate(puppeteerButton.actionButton.cooldownTimerText, puppeteerButton.actionButton.cooldownTimerText.transform.parent);
            puppeteerText.text = "";
            puppeteerText.enableWordWrapping = false;
            puppeteerText.transform.localScale = Vector3.one * 0.5f;
            puppeteerText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

        }
        public static void SetButtonCooldowns()
        {
            sampleButton.MaxTimer = 10f;
            puppeteerButton.MaxTimer = 0f;
        }

        public static void Clear()
        {
            soundFlag = false;
            players = new List<Puppeteer>();
            if (dummy != null) GameData.Instance.RemovePlayer(dummy.PlayerId);
            dummy = null;
            stealthed = false;
            isActive = false;
            canSpawn = false;
            triggerPuppeteerWin = false;
            target = null;
            counter = 0;
            foreach (Arrow arrow in arrows)
            {
                if (arrow != null && arrow.arrow != null)
                {
                    arrow.arrow.SetActive(false);
                    UnityEngine.Object.Destroy(arrow.arrow);
                }
            }
            arrows = new List<Arrow>();
            originalZoom = 0;
            KeyboardJoystickUpdatePatch.stop = false;
        }

        public static void spawnDummy()
        {
            MessageWriter writer;
            if (dummy == null)
            {
                var playerId = (byte)GameData.Instance.GetAvailableId();
                writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SpawnDummy, Hazel.SendOption.Reliable, -1);
                writer.Write(playerId);
                writer.Write(PlayerControl.LocalPlayer.transform.position.x);
                writer.Write(PlayerControl.LocalPlayer.transform.position.y);
                writer.Write(PlayerControl.LocalPlayer.transform.position.z);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.spawnDummy(playerId, PlayerControl.LocalPlayer.transform.position);
            }
            writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.MoveDummy, Hazel.SendOption.Reliable, -1);
            writer.Write(PlayerControl.LocalPlayer.transform.position.x);
            writer.Write(PlayerControl.LocalPlayer.transform.position.y);
            writer.Write(PlayerControl.LocalPlayer.transform.position.z);
            writer.Write(true);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            // 暫定遅延実行　何故か透明化が解除されないため
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(0.25f, new Action<float>(p =>
            {
                if (p == 1)
                {
                    RPCProcedure.moveDummy(PlayerControl.LocalPlayer.transform.position);
                }
            })));
            if (target != null)
            {
                writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PuppeteerMorph, Hazel.SendOption.Reliable, -1);
                writer.Write(target.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.puppeteerMorph(target.PlayerId);
            }
            canSpawn = false;
            isActive = true;
        }

        public static float originalZoom = 0f;
        public static Vector3 originalScale = new();
        public static void senrigan(bool toggle)
        {
            // 初回呼び出し時にカメラのズーム率を保持しておく
            var hm = FastDestroyableSingleton<HudManager>.Instance;
            if (originalZoom == 0) originalZoom = Camera.main.orthographicSize;
            if (originalScale == new Vector3()) originalScale = hm.transform.localScale;
            if (!toggle)
            {
                Camera.main.orthographicSize = originalZoom;
                hm.UICamera.orthographicSize = originalZoom;
                hm.transform.localScale = originalScale;

                if (PlayerControl.LocalPlayer.isAlive())
                {
                    hm.ShadowQuad.gameObject.SetActive(true);
                }
            }
            else
            {
                Camera.main.orthographicSize = originalZoom * 3;
                hm.UICamera.orthographicSize = originalZoom * 3;
                hm.transform.localScale = originalScale * 3;
                hm.ShadowQuad.gameObject.SetActive(false);
            }
        }
        public static void switchStealth(bool flag)
        {
            if (!flag)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PuppeteerStealth, Hazel.SendOption.Reliable, -1);
                writer.Write(false);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.puppeteerStealth(false);
                var hudManager = FastDestroyableSingleton<HudManager>.Instance;
                hudManager.PlayerCam.Target = PlayerControl.LocalPlayer;
                senrigan(false);
                var player = PlayerControl.LocalPlayer;
                player.lightSource = UnityEngine.Object.Instantiate<LightSource>(player.LightPrefab);
                player.lightSource.transform.SetParent(player.transform);
                player.lightSource.transform.localPosition = player.Collider.offset;
                PlayerControl.LocalPlayer.moveable = true;
            }
            else
            {
                // 常に自身の位置から人形をスタートさせる
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.MoveDummy, Hazel.SendOption.Reliable, -1);
                writer.Write(PlayerControl.LocalPlayer.transform.position.x);
                writer.Write(PlayerControl.LocalPlayer.transform.position.y);
                writer.Write(PlayerControl.LocalPlayer.transform.position.z);
                writer.Write(true);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.moveDummy(PlayerControl.LocalPlayer.transform.position);

                writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PuppeteerStealth, Hazel.SendOption.Reliable, -1);
                writer.Write(true);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.puppeteerStealth(true);
                var hudManager = FastDestroyableSingleton<HudManager>.Instance;
                var dummy = Puppeteer.dummy;
                hudManager.PlayerCam.Target = dummy;
                if (losesSenriganOnDeath)
                {
                    bool isAlive = Puppeteer.allPlayers.FindAll(x => x.isAlive()).Count >= 1;
                    senrigan(isAlive);
                }
                else
                {
                    senrigan(true);
                }
                dummy.lightSource = UnityEngine.Object.Instantiate<LightSource>(dummy.LightPrefab);
                dummy.lightSource.transform.SetParent(dummy.transform);
                dummy.lightSource.transform.localPosition = dummy.Collider.offset;
                PlayerControl.LocalPlayer.NetTransform.Halt();
                PlayerControl.LocalPlayer.moveable = false;

            }
        }

        public static void setStealthed(bool stealthed = true)
        {
            Puppeteer.stealthed = stealthed;
            if (Puppeteer.stealthed)
            {
                KeyboardJoystickUpdatePatch.up = false;
                KeyboardJoystickUpdatePatch.down = false;
                KeyboardJoystickUpdatePatch.left = false;
                KeyboardJoystickUpdatePatch.right = false;
            }
        }

        public static void OnTargetExiled()
        {
            bool isAlive = Puppeteer.allPlayers.FindAll(x => x.isAlive()).Count >= 1;
            if (!target.isImpostor() && !target.isRole(RoleType.Jackal) && !target.isRole(RoleType.JekyllAndHyde) && !target.isRole(RoleType.Moriarty) && isAlive)
            {
                counter += 1;
            }
            if (counter >= numKills && PlayerControl.LocalPlayer.isRole(RoleType.Puppeteer))
            {
                MessageWriter winWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PuppeteerWin, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(winWriter);
                RPCProcedure.puppeteerWin();
            }
        }

        public static void OnDummyDeath(PlayerControl killer)
        {
            // クルーがダミーを殺した場合は本体が死ぬのでカウント対象外とする
            if (!killer.isCrew())
                counter += 1;
            soundFlag = true;

            // 人形遣い死亡時は空キルになるのでクールダウンにしない
            bool isAlive = Puppeteer.allPlayers.FindAll(x => x.isAlive()).Count >= 1;
            if (!isAlive)
            {
                killer.SetKillTimer(0f);
            }

            // 人形遣い専用の処理なので人形遣い以外はreturn
            if (!PlayerControl.LocalPlayer.isRole(RoleType.Puppeteer)) return;

            // 勝利条件を満たしていたら勝利
            if (counter >= numKills)
            {
                MessageWriter winWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PuppeteerWin, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(winWriter);
                RPCProcedure.puppeteerWin();
            }

            // ダミー死亡時に連動して発動するキル処理
            if (target.isAlive() && isAlive && !killer.isCrew())
            {
                MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PuppeteerKill, Hazel.SendOption.Reliable, -1);
                killWriter.Write(killer.PlayerId);
                killWriter.Write(target.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                RPCProcedure.puppeteerKill(killer.PlayerId, target.PlayerId);
            }
            else if (isAlive && killer.isCrew()) // ダミーをクルーがキルした場合は人形遣いが死亡する
            {
                MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PuppeteerKill, Hazel.SendOption.Reliable, -1);
                killWriter.Write(killer.PlayerId);
                killWriter.Write(PlayerControl.LocalPlayer.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                RPCProcedure.puppeteerKill(killer.PlayerId, PlayerControl.LocalPlayer.PlayerId);
            }


            isActive = false;
            canSpawn = false;
            target = null;
            switchStealth(false);
        }

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
                    if (arrow != null)
                    {
                        arrow.arrow.SetActive(false);
                        UnityEngine.Object.Destroy(arrow.arrow);
                    }
                }

                // Arrows一覧
                arrows = new List<Arrow>();

                // インポスターの位置を示すArrowsを描画
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (p.Data.IsDead || !p.Data.Role) continue;
                    Arrow arrow;
                    if (p.Data.Role.IsImpostor || p.isRole(RoleType.Jackal) || p.isRole(RoleType.JekyllAndHyde) || p.isRole(RoleType.Moriarty) || p == target)
                    {
                        if (p.Data.Role.IsImpostor)
                        {
                            arrow = new Arrow(Color.red);
                        }
                        else if (p.isRole(RoleType.Jackal) || (p.isRole(RoleType.SchrodingersCat) && SchrodingersCat.team == SchrodingersCat.Team.Jackal))
                        {
                            arrow = new Arrow(Jackal.color);
                        }
                        else if (p.isRole(RoleType.JekyllAndHyde))
                        {
                            arrow = new Arrow(JekyllAndHyde.color);
                        }
                        else if (p.isRole(RoleType.Moriarty))
                        {
                            arrow = new Arrow(Moriarty.color);
                        }
                        else if (p == target)
                        {
                            arrow = new Arrow(Puppeteer.color);
                        }
                        else
                        {
                            arrow = new Arrow(Color.black);
                        }
                        arrow.arrow.SetActive(true);
                        arrow.Update(p.transform.position);
                        arrows.Add(arrow);
                    }
                }

                // タイマーに時間をセット
                updateTimer = arrowUpdateInterval;
            }
        }
        static void syncDummyPos()
        {

            // 前フレームからの経過時間をマイナスする
            posUpdateTimer -= Time.fixedDeltaTime;

            // 1秒経過したらArrowを更新
            if (updateTimer <= 0.0f)
            {

                if (dummy != null)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.MoveDummy, Hazel.SendOption.Reliable, -1);
                    writer.Write(Puppeteer.dummy.transform.position.x);
                    writer.Write(Puppeteer.dummy.transform.position.y);
                    writer.Write(Puppeteer.dummy.transform.position.z);
                    writer.Write(false);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                }

                // タイマーに時間をセット
                posUpdateTimer = 1f;
            }
        }

        public static void setOpacity(PlayerControl player, float opacity)
        {
            Color color = Color.Lerp(Palette.ClearWhite, Palette.White, opacity);
            try
            {
                Helpers.setInvisible(player, color, opacity);
            }
            catch
            {
            }
        }

        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
        public static class PlayerPhysicsPatch
        {
            public static void Postfix(PlayerPhysics __instance)
            {

                if (isRole(__instance.myPlayer))
                {
                    var puppeteer = __instance.myPlayer;
                    if (puppeteer == null || puppeteer.isDead()) return;

                    bool canSee =
                        PlayerControl.LocalPlayer.isRole(RoleType.Puppeteer) ||
                        PlayerControl.LocalPlayer.isDead();

                    var opacity = canSee ? 0.1f : 0.0f;

                    if (stealthed)
                    {
                        puppeteer.cosmetics?.currentBodySprite?.BodySprite.material.SetFloat("_Outline", 0f);
                    }
                    else
                    {
                        opacity = 1.0f;
                    }

                    setOpacity(puppeteer, opacity);
                }
                else if (__instance.myPlayer == dummy)
                {
                    var dummy = __instance.myPlayer;
                    if (dummy == null || dummy.isDead()) return;

                    bool canSee =
                        PlayerControl.LocalPlayer.isRole(RoleType.Puppeteer) ||
                        PlayerControl.LocalPlayer.isDead();

                    var opacity = canSee ? 0.1f : 0.0f;

                    if (!stealthed)
                    {
                        dummy.cosmetics?.currentBodySprite?.BodySprite.material.SetFloat("_Outline", 0f);
                    }
                    else
                    {
                        opacity = 1.0f;
                    }
                    setOpacity(dummy, opacity);
                }
            }
        }

        [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
        class ExileControllerBeginPatch
        {
            public static void Prefix(ExileController __instance, [HarmonyArgument(0)] ref NetworkedPlayerInfo exiled)
            {
                if (exiled != null && exiled.Object == target)
                {
                    OnTargetExiled();
                }
            }
        }

        [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
        public static class KeyboardJoystickUpdatePatch
        {
            public static bool up = false;
            public static bool down = false;
            public static bool right = false;
            public static bool left = false;
            public static bool stop = false;
            private static IEnumerator DontMove(float n)
            {
                stop = true;
                yield return new WaitForSeconds(n);
                stop = false;
                yield break;
            }
            public static void Postfix(KeyboardJoystick __instance)
            {
                if (PlayerControl.LocalPlayer == null) return;
                if (!PlayerControl.LocalPlayer.isRole(RoleType.Puppeteer)) return;

                if (stealthed)
                {
                    if (stop) return;
                    // 梯子を使う/ドアを開ける
                    if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
                    {
                        PlainDoor[] doors;
                        if (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 4)
                        {
                            doors = FastDestroyableSingleton<AirshipStatus>.Instance.GetComponentsInChildren<PlainDoor>();
                        }
                        else if (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 2)
                        {
                            doors = FastDestroyableSingleton<PolusShipStatus>.Instance.GetComponentsInChildren<PlainDoor>();
                        }
                        else if (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 1)
                        {
                            doors = FastDestroyableSingleton<MiraShipStatus>.Instance.GetComponentsInChildren<PlainDoor>();
                        }
                        else if (SubmergedCompatibility.IsSubmerged)
                        {
                            // 遅いかも
                            doors = UnityEngine.GameObject.FindObjectsOfType<PlainDoor>();
                        }
                        else
                        {
                            doors = FastDestroyableSingleton<SkeldShipStatus>.Instance.GetComponentsInChildren<PlainDoor>();
                        }
                        PlainDoor t = null;
                        float minDistance = 9999;
                        foreach (var door in doors)
                        {
                            float distance = Vector2.Distance(door.transform.position, dummy.transform.position);
                            if (distance < 1.5f && distance < minDistance)
                            {
                                t = door;
                                minDistance = distance;
                            }
                        }
                        if (t != null)
                        {
                            var deconSystem = t.transform.parent.gameObject.GetComponent<DeconSystem>();
                            if (deconSystem != null)
                            {
                                bool flag = true;
                                if (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 2)
                                    flag = t.name.Contains("Inner");
                                else if (SubmergedCompatibility.IsSubmerged)
                                    flag = t.name.Contains("Upper");
                                var consoles = t.GetComponentsInChildren<DeconControl>();
                                DeconControl inner = null;
                                DeconControl outer = null;
                                foreach (var console in consoles)
                                {
                                    if (console.name == "InnerConsole") inner = console;
                                    if (console.name == "OuterConsole") outer = console;
                                }
                                float distOuter = Vector2.Distance(outer.transform.position, dummy.transform.position);
                                float distInner = Vector2.Distance(inner.transform.position, dummy.transform.position);
                                if (distInner < distOuter)
                                {
                                    deconSystem.OpenFromInside(flag);
                                }
                                else
                                {
                                    deconSystem.OpenDoor(flag);
                                }
                            }
                            else
                            {
                                FastDestroyableSingleton<ShipStatus>.Instance.RpcUpdateSystem(SystemTypes.Doors, (byte)(t.Id | 64));
                                t.SetDoorway(true);
                            }
                        }


                        if (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 4)
                        {
                            Ladder[] ladders = FastDestroyableSingleton<AirshipStatus>.Instance.GetComponentsInChildren<Ladder>();
                            Ladder target = null;
                            foreach (var ladder in ladders)
                            {
                                float distance = Vector2.Distance(ladder.transform.position, dummy.transform.position);
                                if (distance < 0.5f)
                                {
                                    target = ladder;
                                    break;
                                }
                            }
                            if (target != null)
                            {
                                MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PuppeteerClimbRadder, Hazel.SendOption.Reliable, -1);
                                messageWriter.Write(dummy.PlayerId);
                                messageWriter.Write(target.Id);
                                AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
                                RPCProcedure.puppeteerClimbRadder(dummy.PlayerId, target.Id);
                                return;
                            }

                            AirshipStatus shipstatus = FastDestroyableSingleton<AirshipStatus>.Instance;
                            if (shipstatus != null)
                            {
                                var consoles = shipstatus.GetComponentsInChildren<PlatformConsole>().ToList();
                                PlatformConsole leftPlatform = consoles.Find(x => x.name == "PlatformLeft");
                                PlatformConsole rightPlatform = consoles.Find(x => x.name == "PlatformRight");
                                float distanceRight = Vector2.Distance(leftPlatform.transform.position, dummy.transform.position);
                                float distanceLeft = Vector2.Distance(rightPlatform.transform.position, dummy.transform.position);
                                if (distanceRight < 0.8f || distanceLeft < 0.8f)
                                {
                                    MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PuppeteerUsePlatform, Hazel.SendOption.Reliable, -1);
                                    messageWriter.Write(dummy.PlayerId);
                                    AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
                                    RPCProcedure.puppeteerUsePlatform(dummy.PlayerId);
                                    // TODO 2回やらないと何故か乗れないので2回実行させておく
                                    FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(DontMove(1).WrapToIl2Cpp());
                                    FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(1f, new Action<float>(t =>
                                    {
                                        if (t >= 1.0f)
                                        {
                                            messageWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PuppeteerUsePlatform, Hazel.SendOption.Reliable, -1);
                                            messageWriter.Write(dummy.PlayerId);
                                            AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
                                            RPCProcedure.puppeteerUsePlatform(dummy.PlayerId);
                                        }
                                    })));
                                    return;
                                }
                            }
                        }

                        // エレベーター(サブマージド)
                        if (SubmergedCompatibility.IsSubmerged)
                        {
                            var elevators = Helpers.FindObjectsOfType(SubmarineElevatorType);
                            object elevator = null;
                            minDistance = 9999;
                            foreach (var e in elevators)
                            {
                                var pos = (e as UnityEngine.MonoBehaviour).transform.position;
                                FieldInfo lowerInnerDoorInfo = SubmarineElevatorType.GetField("LowerInnerDoor");
                                FieldInfo upperInnerDoorInfo = SubmarineElevatorType.GetField("UpperInnerDoor");
                                var lowerInnerDoor = lowerInnerDoorInfo.GetValue(e) as PlainDoor;
                                var upperInnerDoor = upperInnerDoorInfo.GetValue(e) as PlainDoor;
                                float lowerDistance = Vector2.Distance(dummy.transform.position, lowerInnerDoor.transform.position);
                                float upperDistance = Vector2.Distance(dummy.transform.position, upperInnerDoor.transform.position);
                                float distance = lowerDistance < upperDistance ? lowerDistance : upperDistance;
                                if (distance < 1.5 && distance < minDistance)
                                {
                                    minDistance = distance;
                                    elevator = e;
                                }
                            }
                            if (elevator != null)
                            {
                                var use = SubmarineElevatorType.GetMethod("Use");
                                use.Invoke(elevator, new object[0]);
                            }

                        }

                    }

                    if (Input.GetKeyDown(KeyCode.D))
                        right = true;
                    if (Input.GetKeyUp(KeyCode.D))
                        right = false;
                    if (Input.GetKeyDown(KeyCode.A))
                        left = true;
                    if (Input.GetKeyUp(KeyCode.A))
                        left = false;
                    if (Input.GetKeyDown(KeyCode.W))
                        up = true;
                    if (Input.GetKeyUp(KeyCode.W))
                        up = false;
                    if (Input.GetKeyDown(KeyCode.S))
                        down = true;
                    if (Input.GetKeyUp(KeyCode.S))
                        down = false;

                    if (Puppeteer.dummy != null && !MeetingHud.Instance)
                    {
                        Vector2 pos = Puppeteer.dummy.transform.position;
                        Vector2 offset = Vector2.zero;
                        if (up) offset += new Vector2(0f, 0.5f);
                        if (down) offset += new Vector2(0f, -0.5f);
                        if (left) offset += new Vector2(-0.5f, 0.0f);
                        if (right) offset += new Vector2(0.5f, 0.0f);
                        MessageWriter writer;
                        if (offset != Vector2.zero)
                        {
                            writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.WalkDummy, Hazel.SendOption.Reliable, -1);
                            writer.Write(offset.x);
                            writer.Write(offset.y);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                        }
                        RPCProcedure.walkDummy(offset);
                        if (!(up || down || right || left) && dummy.NetTransform.lastPosition != pos)
                        {
                            writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.MoveDummy, Hazel.SendOption.Reliable, -1);
                            writer.Write(Puppeteer.dummy.transform.position.x);
                            writer.Write(Puppeteer.dummy.transform.position.y);
                            writer.Write(Puppeteer.dummy.transform.position.z);
                            writer.Write(false);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            // RPCProcedure.moveDummy(Puppeteer.dummy.transform.position);
                        }
                    }

                }
            }
        }
    }
}
