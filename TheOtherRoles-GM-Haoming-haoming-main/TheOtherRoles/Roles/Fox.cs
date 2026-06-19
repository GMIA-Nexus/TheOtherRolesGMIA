using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using TheOtherRoles.Objects;
using TheOtherRoles.Patches;
using UnityEngine;
using static TheOtherRoles.GameHistory;
using static TheOtherRoles.Patches.PlayerControlFixedUpdatePatch;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles
{
    [HarmonyPatch]
    public class Fox : RoleBase<Fox>
    {
        public enum TaskType
        {
            Serial,
            Parallel
        }
        public static Color color = new Color32(167, 87, 168, byte.MaxValue);
        private static CustomButton foxButton;
        private static CustomButton foxRepairButton;
        private static CustomButton foxImmoralistButton;
        public static List<Arrow> arrows = new();
        public static float updateTimer = 0f;

        public static float arrowUpdateInterval = 0.5f;
        public static bool crewWinsByTasks { get { return CustomOptionHolder.foxCrewWinsByTasks.getBool(); } }
        public static bool impostorWinsBySabotage { get { return CustomOptionHolder.foxImpostorWinsBySabotage.getBool(); } }
        public static float stealthCooldown { get { return CustomOptionHolder.foxStealthCooldown.getFloat(); } }
        public static float stealthDuration { get { return CustomOptionHolder.foxStealthDuration.getFloat(); } }
        public static int numTasks { get { return (int)CustomOptionHolder.foxNumTasks.getFloat(); } }
        public static float stayTime { get { return (int)CustomOptionHolder.foxStayTime.getFloat(); } }
        public static TaskType taskType { get { return (TaskType)CustomOptionHolder.foxTaskType.getSelection(); } }


        public bool stealthed = false;
        public DateTime stealthedAt = DateTime.UtcNow;
        public static float fadeTime = 1f;

        public static int numRepair = 0;

        public static bool canCreateImmoralist { get { return CustomOptionHolder.foxCanCreateImmoralist.getBool(); } }
        public static PlayerControl currentTarget;
        public static PlayerControl immoralist;
        public static List<byte> exiledFox = new();



        public Fox()
        {
            stealthed = false;
            stealthedAt = DateTime.UtcNow;
            RoleType = roleId = RoleType.Fox;
            immoralist = null;
            currentTarget = null;
            exiledFox = new List<byte>();
        }

        public override void OnMeetingStart()
        {
            stealthed = false;
            foxButton.isEffectActive = false;
            foxButton.Timer = foxButton.MaxTimer = stealthCooldown;
        }

        public override void OnMeetingEnd() { }
        public override void OnKill(PlayerControl target) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
        public override void OnDeath(PlayerControl killer = null)
        {
            exiledFox.Add(player.PlayerId);
            player.clearAllTasks();
            if (!Fox.isFoxAlive())
            {
                foreach (var immoralist in Immoralist.allPlayers)
                {
                    if (immoralist.isAlive())
                    {
                        if (killer == null)
                        {
                            immoralist.Exiled();
                        }
                        else
                        {
                            immoralist.MurderPlayer(immoralist);
                        }
                        finalStatuses[immoralist.PlayerId] = FinalStatus.Suicide;
                    }
                }
            }
        }
        public override void OnFinishShipStatusBegin()
        {
            // PlayerControl.LocalPlayer.clearAllTasks();
            // local.assignTasks();
        }

        public override void FixedUpdate()
        {
            if (player == PlayerControl.LocalPlayer)
            {
                arrowUpdate();
                if (player.isAlive())
                {
                    List<PlayerControl> untargetablePlayers = new();
                    foreach (var p in PlayerControl.AllPlayerControls.ToArray())
                    {
                        if (p.isImpostor() || p.isRole(RoleType.Jackal) || p.isRole(RoleType.Sheriff))
                        {
                            untargetablePlayers.Add(p);
                        }
                    }
                    currentTarget = setTarget(untargetablePlayers: untargetablePlayers);
                    setPlayerOutline(currentTarget, Fox.color);
                }
            }
        }

        public static void Clear()
        {
            players = new List<Fox>();
            foreach (Arrow arrow in arrows)
            {
                if (arrow?.arrow != null)
                {
                    arrow.arrow.SetActive(false);
                    UnityEngine.Object.Destroy(arrow.arrow);
                }
            }
            arrows = new List<Arrow>();
            Immoralist.Clear();
        }

        private static Sprite hideButtonSprite;
        private static Sprite repairButtonSprite;
        private static Sprite immoralistButtonSprite;

        public static Sprite getHideButtonSprite()
        {
            if (hideButtonSprite) return hideButtonSprite;
            hideButtonSprite = Helpers.loadSpriteFromAssetBundle("FoxHideButton.png");
            return hideButtonSprite;
        }

        public static Sprite getRepairButtonSprite()
        {
            if (repairButtonSprite) return repairButtonSprite;
            repairButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.RepairButton.png", 115f);
            return repairButtonSprite;
        }

        public static Sprite getImmoralistButtonSprite()
        {
            if (immoralistButtonSprite) return immoralistButtonSprite;
            immoralistButtonSprite = Helpers.loadSpriteFromAssetBundle("FoxImmoralistButton.png");
            return immoralistButtonSprite;
        }

        public static float stealthFade(PlayerControl player)
        {
            if (isRole(player) && fadeTime > 0f && player.isAlive())
            {
                Fox n = players.First(x => x.player == player);
                return Mathf.Min(1.0f, (float)(DateTime.UtcNow - n.stealthedAt).TotalSeconds / fadeTime);
            }
            return 1.0f;
        }

        public static bool isStealthed(PlayerControl player)
        {
            if (isRole(player) && player.isAlive())
            {
                Fox n = players.First(x => x.player == player);
                return n.stealthed;
            }
            return false;
        }

        public static void setStealthed(PlayerControl player, bool stealthed = true)
        {
            if (isRole(player))
            {
                Fox n = players.First(x => x.player == player);
                n.stealthed = stealthed;
                n.stealthedAt = DateTime.UtcNow;
            }
        }


        public static void MakeButtons(HudManager hm)
        {
            // Fox stealth
            foxButton = new CustomButton(
                () =>
                {
                    if (foxButton.isEffectActive)
                    {
                        foxButton.Timer = 0;
                        return;
                    }

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.FoxStealth, Hazel.SendOption.Reliable, -1);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    writer.Write(true);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.foxStealth(PlayerControl.LocalPlayer.PlayerId, true);
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleType.Fox) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (foxButton.isEffectActive)
                    {
                        foxButton.buttonText = ModTranslation.getString("FoxUnstealthText");
                    }
                    else
                    {
                        foxButton.buttonText = ModTranslation.getString("FoxStealthText");
                    }
                    return PlayerControl.LocalPlayer.CanMove;
                },
                () =>
                {
                    foxButton.Timer = foxButton.MaxTimer = Fox.stealthCooldown;
                },
                Fox.getHideButtonSprite(),
                CustomButton.ButtonPositions.upperRowRight,
                hm,
                hm.AbilityButton,
                KeyCode.F,
                true,
                Fox.stealthDuration,
                () =>
                {
                    foxButton.Timer = foxButton.MaxTimer = Fox.stealthCooldown;
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.FoxStealth, Hazel.SendOption.Reliable, -1);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    writer.Write(false);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.foxStealth(PlayerControl.LocalPlayer.PlayerId, false);
                }
            );
            foxButton.Timer = foxButton.MaxTimer = Fox.stealthCooldown;
            foxButton.buttonText = ModTranslation.getString("FoxStealthText");
            foxButton.effectCancellable = true;

            foxRepairButton = new CustomButton(
                () =>
                {
                    bool sabotageActive = false;
                    foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
                        if (task.TaskType is TaskTypes.FixLights or TaskTypes.RestoreOxy or TaskTypes.ResetReactor or TaskTypes.ResetSeismic or TaskTypes.FixComms or TaskTypes.StopCharles)
                            sabotageActive = true;
                    if (!sabotageActive) return;

                    foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
                    {
                        if (task.TaskType == TaskTypes.FixLights)
                        {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.EngineerFixLights, Hazel.SendOption.Reliable, -1);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.engineerFixLights();
                        }
                        else if (task.TaskType == TaskTypes.RestoreOxy)
                        {
                            MapUtilities.CachedShipStatus.RpcUpdateSystem(SystemTypes.LifeSupp, 0 | 64);
                            MapUtilities.CachedShipStatus.RpcUpdateSystem(SystemTypes.LifeSupp, 1 | 64);
                        }
                        else if (task.TaskType == TaskTypes.ResetReactor)
                        {
                            MapUtilities.CachedShipStatus.RpcUpdateSystem(SystemTypes.Reactor, 16);
                        }
                        else if (task.TaskType == TaskTypes.ResetSeismic)
                        {
                            MapUtilities.CachedShipStatus.RpcUpdateSystem(SystemTypes.Laboratory, 16);
                        }
                        else if (task.TaskType == TaskTypes.FixComms)
                        {
                            MapUtilities.CachedShipStatus.RpcUpdateSystem(SystemTypes.Comms, 16 | 0);
                            MapUtilities.CachedShipStatus.RpcUpdateSystem(SystemTypes.Comms, 16 | 1);
                        }
                        else if (task.TaskType == TaskTypes.StopCharles)
                        {
                            MapUtilities.CachedShipStatus.RpcUpdateSystem(SystemTypes.Reactor, 0 | 16);
                            MapUtilities.CachedShipStatus.RpcUpdateSystem(SystemTypes.Reactor, 1 | 16);
                        }
                    }
                    numRepair -= 1;
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleType.Fox) && PlayerControl.LocalPlayer.isAlive() && numRepair > 0; },
                () =>
                {
                    bool sabotageActive = false;
                    foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
                        if (task.TaskType is TaskTypes.FixLights or TaskTypes.RestoreOxy or TaskTypes.ResetReactor or TaskTypes.ResetSeismic or TaskTypes.FixComms or TaskTypes.StopCharles)
                            sabotageActive = true;
                    return sabotageActive && numRepair > 0 && PlayerControl.LocalPlayer.CanMove;
                },
                () => { foxRepairButton.Timer = foxRepairButton.MaxTimer = 0f; },
                Fox.getRepairButtonSprite(),
                CustomButton.ButtonPositions.upperRowLeft,
                hm,
                hm.AbilityButton,
                KeyCode.G
            );
            foxRepairButton.Timer = foxRepairButton.MaxTimer = 0f;
            foxRepairButton.buttonText = ModTranslation.getString("FoxRepairText"); ;

            foxImmoralistButton = new CustomButton(
                () =>
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.FoxCreatesImmoralist, Hazel.SendOption.Reliable, -1);
                    writer.Write(currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.foxCreatesImmoralist(currentTarget.PlayerId);
                },
                () => { return !Immoralist.exists && canCreateImmoralist && PlayerControl.LocalPlayer.isRole(RoleType.Fox) && PlayerControl.LocalPlayer.isAlive(); },
                () => { return canCreateImmoralist && Fox.currentTarget != null && PlayerControl.LocalPlayer.CanMove; },
                () => { foxImmoralistButton.Timer = foxImmoralistButton.MaxTimer = 20f; },
                getImmoralistButtonSprite(),
                CustomButton.ButtonPositions.upperRowCenter,
                hm,
                hm.AbilityButton,
                KeyCode.I
            );
            foxImmoralistButton.Timer = foxImmoralistButton.MaxTimer = 20f;
            foxImmoralistButton.buttonText = ModTranslation.getString("FoxImmoralistText");
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
                    if (arrow?.arrow != null)
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
                    if (p.isDead()) continue;
                    Arrow arrow;
                    // float distance = Vector2.Distance(p.transform.position, PlayerControl.LocalPlayer.transform.position);
                    if (p.Data.Role.IsImpostor || p.isRole(RoleType.Jackal) || p.isRole(RoleType.Sheriff) || p.isRole(RoleType.JekyllAndHyde) || p.isRole(RoleType.Moriarty))
                    {
                        if (p.Data.Role.IsImpostor)
                        {
                            arrow = new Arrow(Palette.ImpostorRed);
                        }
                        else if (p.isRole(RoleType.Jackal))
                        {
                            arrow = new Arrow(Jackal.color);
                        }
                        else if (p.isRole(RoleType.Sheriff))
                        {
                            arrow = new Arrow(Palette.White);
                        }
                        else if (p.isRole(RoleType.JekyllAndHyde))
                        {
                            arrow = new Arrow(JekyllAndHyde.color);
                        }
                        else if (p.isRole(RoleType.Moriarty))
                        {
                            arrow = new Arrow(Moriarty.color);
                        }
                        else
                        {
                            arrow = new Arrow(Palette.Black);
                        }
                        arrow.arrow.SetActive(true);
                        arrow.Update(p.transform.position);
                        arrows.Add(arrow);
                    }
                }

                // タイマーに時間をセット
                updateTimer = arrowUpdateInterval;
            }
            else
            {
                arrows.Do(x => x.Update());
            }
        }

        public static bool isFoxAlive()
        {
            bool isAlive = false;
            foreach (var fox in Fox.allPlayers)
            {
                if (fox.isAlive() && !exiledFox.Contains(fox.PlayerId))
                {
                    isAlive = true;
                }
            }
            return isAlive;
        }

        public static bool isFoxCompletedTasks()
        {
            // 生存中の狐が1匹でもタスクを終了しているかを確認
            bool isCompleted = false;
            foreach (var fox in allPlayers)
            {
                if (fox.isAlive())
                {
                    if (tasksComplete(fox))
                    {
                        isCompleted = true;
                        break;
                    }
                }
            }
            return isCompleted;
        }

        private static bool tasksComplete(PlayerControl p)
        {
            int counter = 0;
            int totalTasks = 1;
            if (totalTasks == 0) return true;
            foreach (var task in p.Data.Tasks)
            {
                if (task.Complete)
                {
                    counter++;
                }
            }
            return counter == totalTasks;
        }

        public void assignTasks()
        {
            // player.generateAndAssignTasks(numCommonTasks, numShortTasks, numLongTasks);
        }

        // 透明化
        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
        public static class PlayerPhysicsFoxPatch
        {
            public static void Postfix(PlayerPhysics __instance)
            {
                if (isRole(__instance.myPlayer))
                {
                    var fox = __instance.myPlayer;
                    if (fox == null || fox.isDead()) return;

                    bool canSee =
                        PlayerControl.LocalPlayer.isRole(RoleType.Fox) ||
                        PlayerControl.LocalPlayer.isRole(RoleType.Immoralist) ||
                        PlayerControl.LocalPlayer.isDead() ||
                        (PlayerControl.LocalPlayer.isRole(RoleType.Lighter) && Lighter.isLightActive(PlayerControl.LocalPlayer));

                    var opacity = canSee ? 0.1f : 0.0f;

                    if (isStealthed(fox))
                    {
                        opacity = Math.Max(opacity, 1.0f - stealthFade(fox));
                        fox.cosmetics?.currentBodySprite?.BodySprite.material.SetFloat("_Outline", 0f);
                    }
                    else
                        opacity = Math.Max(opacity, stealthFade(fox));

                    Color color = Color.Lerp(Palette.ClearWhite, Palette.White, opacity);
                    try
                    {
                        Helpers.setInvisible(fox, color, opacity);
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}
