using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TheOtherRoles.Modules;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    [TORRPCHolder]
    public class Sprinter : RoleBase<Sprinter>
    {
        public static Color color = new Color32(128, 128, 255, byte.MaxValue);

        public Sprinter()
        {
            RoleId = roleId = RoleId.Sprinter;
            sprintAt = DateTime.UtcNow;
            sprinting = false;
            acTokenMove = null;
        }

        public static float sprintCooldown = 30f;
        public static float sprintDuration { get { return CustomOptionHolder.sprinterDuration.getFloat(); } }
        public static float fadeTime = 0.5f;
        public static float speedBonus = 0.25f;

        static public IEnumerable<DocumentReplacement> GetReplacementPart() => [new("%SEC%", sprintDuration.ToString())];

        public bool sprinting = false;
        public DateTime sprintAt = DateTime.UtcNow;
        public AchievementToken<(Vector3 pos, bool cleared)> acTokenMove = null;

        public override void PostInit()
        {
            if (PlayerControl.LocalPlayer != player) return;
            if (sprintDuration <= 15f)
                acTokenMove ??= new("sprinter.common2", (Vector3.zero, false), (val, _) => val.cleared);
        }

        public static RemoteProcess<(byte playerId, bool sprinting)> Sprint = new("SprinterSprint", (message, _) =>
        {
            PlayerControl player = Helpers.playerById(message.playerId);
            setSprinting(player, message.sprinting);
        });

        public override void ResetRole(bool isShifted)
        {
            sprinting = false;
            setOpacity(player, 1.0f);
        }

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.SprintButton.png", 115f);
            return buttonSprite;
        }

        public override void OnMeetingStart()
        {
            sprinting = false;
        }

        public static float sprintFade(PlayerControl player)
        {
            if (isRole(player) && fadeTime > 0 && !player.Data.IsDead)
            {
                var sprinter = getRole(player);
                return Mathf.Min(1.0f, (float)(DateTime.UtcNow - sprinter.sprintAt).TotalSeconds / fadeTime);
            }
            return 1.0f;
        }

        public static void setSprinting(PlayerControl player, bool sprinting = true)
        {
            if (isRole(player))
            {
                var sprinter = getRole(player);
                sprinter.sprinting = sprinting;
                sprinter.sprintAt = DateTime.UtcNow;
            }
        }

        public static bool isSprinting(PlayerControl player)
        {
            if (isRole(player) && !player.Data.IsDead)
            {
                var sprinter = getRole(player);
                return sprinter.sprinting;
            }
            return false;
        }

        public override void OnDeath(PlayerControl killer = null)
        {
            sprinting = false;
        }

        public static void setOpacity(PlayerControl player, float opacity)
        {
            var color = Color.Lerp(Palette.ClearWhite, Palette.White, opacity);
            try
            {
                if (player.hasModifier(RoleId.Chameleon) && Chameleon.visibility(player.PlayerId) < 1f && !isSprinting(player)) return;
                Helpers.setInvisible(player, color, opacity);
            }
            catch { }
        }

        public static void clearAndReload()
        {
            sprintCooldown = CustomOptionHolder.sprinterCooldown.getFloat();
            fadeTime = CustomOptionHolder.sprinterFadeTime.getFloat();
            speedBonus = CustomOptionHolder.sprinterSpeedBonus.getFloat();
            players = [];
        }

        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
        public static class PlayerPhysicsSprinterPatch
        {
            public static void Postfix(PlayerPhysics __instance)
            {
                if (isRole(__instance.myPlayer))
                {
                    if (GameData.Instance && isSprinting(__instance.myPlayer) && __instance.AmOwner && __instance.myPlayer.CanMove) __instance.body.velocity *= 1 + speedBonus;
                    var sprinter = __instance.myPlayer;
                    if (sprinter == null || sprinter.Data.IsDead) return;

                    bool canSee = PlayerControl.LocalPlayer.CanSeeInvisible() ||
                        PlayerControl.LocalPlayer == sprinter;

                    var opacity = canSee ? 0.5f : 0.0f;

                    if (isSprinting(sprinter))
                    {
                        opacity = Math.Max(opacity, 1.0f - sprintFade(sprinter));
                        sprinter.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", 0f);
                    }
                    else
                        opacity = Math.Max(opacity, sprintFade(sprinter));

                    setOpacity(sprinter, opacity);
                }
            }
        }
    }
}
