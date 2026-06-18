using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TheOtherRoles.MetaContext;
using TheOtherRoles.Modules;
using TheOtherRoles.Objects;
using TheOtherRoles.Utilities;
using UnityEngine;
using static TheOtherRoles.Patches.PlayerControlFixedUpdatePatch;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    [TORRPCHolder]
    public class Fox : RoleBase<Fox>
    {
        public static Color color = new Color32(167, 87, 168, byte.MaxValue);

        public enum TaskType
        {
            Serial,
            Parallel
        }

        static public IEnumerable<HelpSprite> GetHelpSprites()
        {
            yield return new(getHideButtonSprite(), "foxHideHint");
            if (CustomOptionHolder.foxCanCreateImmoralist.getBool()) {
                yield return new(getImmoralistButtonSprite(), "foxImmoralistHint");
            }
            if (CustomOptionHolder.foxNumRepairs.getFloat() > 0) {
                yield return new(getRepairButtonSprite(), "foxRepairHint");
            }
        }

        public Fox()
        {
            RoleId = roleId = RoleId.Fox;
            stealthed = false;
            stealthedAt = DateTime.UtcNow;
        }

        public override void OnDeath(PlayerControl killer = null)
        {
            foreach (var immoralist in Immoralist.livingPlayers)
            {
                if (killer != null)
                    immoralist.MurderPlayer(immoralist, MurderResultFlags.Succeeded);
                else
                {
                    immoralist.Exiled();
                    if (PlayerControl.LocalPlayer == immoralist && Helpers.ShowKillAnimation)
                        FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(immoralist.Data, immoralist.Data);
                }
                GameHistory.overrideDeathReasonAndKiller(immoralist, DeadPlayer.CustomDeathReason.Suicide);
            }
        }

        public override void OnMeetingStart()
        {
            stealthed = false;
        }

        public static List<Arrow> arrows = [];
        public static float updateTimer = 0f;
        public static float arrowUpdateInterval = 0.5f;
        public static bool crewWinsByTasks = false;
        public static bool impostorWinsBySabotage = true;
        public static float stealthCooldown;
        public static float stealthDuration;
        public static int numTasks;
        public static float stayTime;

        public static bool stealthed = false;
        public static DateTime stealthedAt = DateTime.UtcNow;
        public static float fadeTime = 1f;

        public static int numRepair = 0;
        public static bool canCreateImmoralist;
        public static PlayerControl currentTarget;
        public static TaskType taskType;

        private static Sprite hideButtonSprite;
        private static Sprite repairButtonSprite;
        private static Sprite immoralistButtonSprite;

        public static Sprite getHideButtonSprite()
        {
            if (hideButtonSprite) return hideButtonSprite;
            hideButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.FoxHideButton.png", 115f);
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
            immoralistButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.FoxImmoralistButton.png", 115f);
            return immoralistButtonSprite;
        }

        public static float stealthFade()
        {
            if (hasAlivePlayers)
                return Mathf.Min(1.0f, (float)(DateTime.UtcNow - stealthedAt).TotalSeconds / fadeTime);
            return 1.0f;
        }

        public static RemoteProcess<bool> SetStealth = RemotePrimitiveProcess.OfBoolean("FoxSetStealth", (message, _) =>
        {
            stealthed = message;
            stealthedAt = DateTime.UtcNow;
        });

        public static RemoteProcess<byte> CreatesImmoralist = RemotePrimitiveProcess.OfByte("FoxCreatesImmoralist", (message, _) => RPCProcedure.foxCreatesImmoralist(message));

        public static void setOpacity(PlayerControl player, float opacity)
        {
            var color = Color.Lerp(Palette.ClearWhite, Palette.White, opacity);
            try
            {
                if (Chameleon.chameleon.Any(x => x.PlayerId == player.PlayerId) && Chameleon.visibility(player.PlayerId) < 1f && !stealthed) return;
                Helpers.setInvisible(player, color, opacity);
            }
            catch { }
        }

        public static bool isFoxCompletedTasks()
        {
            // 生存中の狐が1匹でもタスクを終了しているかを確認
            bool isCompleted = false;
            foreach (var fox in livingPlayers)
            {
                if (tasksComplete(fox))
                {
                    isCompleted = true;
                    break;
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
                if (task.Complete)
                    counter++;
            return counter == totalTasks;
        }

        public static void clearAllArrow()
        {
            if (arrows?.Count > 0)
            {
                foreach (var arrow in arrows)
                    if (arrow != null && arrow.arrow != null) arrow.arrow.SetActive(false);
            }
        }

        public override void FixedUpdate()
        {
            if (PlayerControl.LocalPlayer == player)
            {
                arrowUpdate();
                if (!PlayerControl.LocalPlayer.Data.IsDead)
                {
                    List<PlayerControl> untargetablePlayers = [];
                    foreach (var p in PlayerControl.AllPlayerControls.GetFastEnumerator())
                        if (p.Data.Role.IsImpostor || p.isRole(RoleId.Jackal) || p.isRole(RoleId.JekyllAndHyde) || p.isRole(RoleId.Moriarty)
                            || (p.isRole(RoleId.SchrodingersCat) && SchrodingersCat.hasTeam() && SchrodingersCat.team != SchrodingersCat.Team.Crewmate)
                            || (p.isRole(RoleId.Sidekick) && Sidekick.canKill))
                            untargetablePlayers.Add(p);
                    currentTarget = setTarget(untargetablePlayers: untargetablePlayers);
                    if (canCreateImmoralist) setPlayerOutline(currentTarget, color);
                }
            }
        }

        public static void arrowUpdate()
        {
            if (PlayerControl.LocalPlayer.Data.IsDead)
            {
                if (arrows?.Count > 0)
                {
                    foreach (var arrow in arrows)
                        if (arrow != null && arrow.arrow != null) UnityEngine.Object.Destroy(arrow.arrow);
                }
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
                    if (arrow?.arrow != null)
                    {
                        arrow.arrow.SetActive(false);
                        UnityEngine.Object.Destroy(arrow.arrow);
                    }
                }

                // Arrows一覧
                arrows = [];

                // インポスターの位置を示すArrowsを描画
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (p.Data.IsDead) continue;
                    Arrow arrow;
                    // float distance = Vector2.Distance(p.transform.position, PlayerControl.LocalPlayer.transform.position);
                    if (p.Data.Role.IsImpostor || p.isRole(RoleId.Jackal) || p.isRole(RoleId.Sheriff) || p.isRole(RoleId.JekyllAndHyde) ||
                        p.isRole(RoleId.Moriarty) || p.isRole(RoleId.Yandere) || p.isRole(RoleId.Pelican) || p.isRole(RoleId.Thief) || p.isRole(RoleId.SchrodingersCat) && SchrodingersCat.hasTeam() && SchrodingersCat.team != SchrodingersCat.Team.Crewmate ||
                        p.isRole(RoleId.Sidekick) && Sidekick.canKill)
                    {
                        if (p.Data.Role.IsImpostor)
                            arrow = new Arrow(Palette.ImpostorRed);
                        else if (p.isRole(RoleId.Jackal) || p.isRole(RoleId.Sidekick))
                            arrow = new Arrow(Jackal.color);
                        else if (p.isRole(RoleId.Sheriff))
                            arrow = new Arrow(Palette.White);
                        else if (p.isRole(RoleId.JekyllAndHyde))
                            arrow = new Arrow(JekyllAndHyde.color);
                        else if (p.isRole(RoleId.Moriarty))
                            arrow = new Arrow(Moriarty.color);
                        else if (p.isRole(RoleId.SchrodingersCat))
                            arrow = new Arrow(RoleInfo.schrodingersCat.color);
                        else if (p.isRole(RoleId.Pelican))
                            arrow = new Arrow(Pelican.color);
                        else if (p.isRole(RoleId.Yandere))
                            arrow = new Arrow(Yandere.color);
                        else
                            arrow = new Arrow(Thief.color);
                        arrow.arrow.SetActive(true);
                        arrow.Update(p.transform.position);
                        arrows.Add(arrow);
                    }
                }

                // タイマーに時間をセット
                updateTimer = arrowUpdateInterval;
            }
            else
                arrows.Do(x => x.Update());
        }

        public override void ResetRole(bool isShifted)
        {
            setOpacity(player, 1.0f);
            clearAllArrow();
            stealthed = false;
            if (!isShifted)
            {
                foreach (var immoralist in new List<PlayerControl>(Immoralist.allPlayers))
                {
                    if (PlayerControl.LocalPlayer == immoralist)
                        PlayerControl.LocalPlayer.generateNormalTasks();
                    Immoralist.eraseRole(immoralist);
                }
            }
        }

        public static void clearAndReload()
        {
            currentTarget = null;
            stealthed = false;
            stealthedAt = DateTime.UtcNow;
            crewWinsByTasks = CustomOptionHolder.foxCrewWinsByTasks.getBool();
            impostorWinsBySabotage = CustomOptionHolder.foxImpostorWinsBySabotage.getBool();
            stealthCooldown = CustomOptionHolder.foxStealthCooldown.getFloat();
            stealthDuration = CustomOptionHolder.foxStealthDuration.getFloat();
            canCreateImmoralist = CustomOptionHolder.foxCanCreateImmoralist.getBool();
            numTasks = (int)CustomOptionHolder.foxNumTasks.getFloat();
            numRepair = (int)CustomOptionHolder.foxNumRepairs.getFloat();
            stayTime = (int)CustomOptionHolder.foxStayTime.getFloat();
            taskType = (TaskType)CustomOptionHolder.foxTaskType.getSelection();
            foreach (Arrow arrow in arrows)
            {
                if (arrow?.arrow != null)
                {
                    arrow.arrow.SetActive(false);
                    UnityEngine.Object.Destroy(arrow.arrow);
                }
            }
            arrows = [];
            players = [];
        }

        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
        public static class PlayerPhysicsFoxPatch
        {
            public static void Postfix(PlayerPhysics __instance)
            {
                if (isRole(__instance.myPlayer))
                {
                    var fox = __instance.myPlayer;
                    if (fox == null || fox.Data.IsDead) return;

                    bool canSee =
                        PlayerControl.LocalPlayer == fox || PlayerControl.LocalPlayer.CanSeeInvisible() ||
                        PlayerControl.LocalPlayer.isRole(RoleId.Immoralist);

                    var opacity = canSee ? 0.5f : 0.0f;

                    if (stealthed)
                    {
                        opacity = Math.Max(opacity, 1.0f - stealthFade());
                        fox.cosmetics?.currentBodySprite?.BodySprite.material.SetFloat("_Outline", 0f);
                    }
                    else
                        opacity = Math.Max(opacity, stealthFade());

                    setOpacity(fox, opacity);
                }
            }
        }
    }
}
