using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TheOtherRoles.Modules;
using TheOtherRoles.Patches;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    [TORRPCHolder]
    public class Ninja : RoleBase<Ninja>
    {
        public static Color color = Palette.ImpostorRed;
        public static float stealthCooldown = 30f;
        public static float stealthDuration = 15f;
        public static float killPenalty { get { return CustomOptionHolder.ninjaKillPenalty.getFloat(); } }
        public static float speedBonus = 1.25f;
        public static float fadeTime = 0.5f;
        public static bool canUseVents = false;
        public static bool canBeTargeted;

        public bool penalized = false;
        public bool stealthed = false;
        public DateTime stealthedAt = DateTime.UtcNow;
        public AchievementToken<int> acTokenChallenge = null;

        static public IEnumerable<HelpSprite> GetHelpSprites()
        {
            yield return new(getButtonSprite(), "ninjaStealthHint");
        }

        public override AudioClip IntroSound { get => MetaContext.VanillaAsset.PhantomIntroSound; }

        static public IEnumerable<DocumentReplacement> GetReplacementPart() => [new("%KP%", killPenalty.ToString())];

        public static RemoteProcess<(byte playerId, bool stealthed)> Stealth = new("NinjaStealth", (message, _) =>
        {
            PlayerControl player = Helpers.playerById(message.playerId);
            setStealthed(player, message.stealthed);
        });

        public Ninja()
        {
            RoleId = roleId = RoleId.Ninja;
            penalized = false;
            stealthed = false;
            stealthedAt = DateTime.UtcNow;
            acTokenChallenge = null;
        }

        public override void ResetRole(bool isShifted)
        {
            penalized = false;
            stealthed = false;
            setOpacity(player, 1.0f);
        }

        public override void OnKill(PlayerControl target)
        {
            penalized = stealthed;
            float penalty = penalized ? killPenalty : 0f;
            if (PlayerControl.LocalPlayer == player) {
                player.SetKillTimerUnchecked(GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown + penalty);
                if (stealthed)
                {
                    acTokenChallenge.Value++;
                    if (Helpers.CurrentMonth == 11 && Helpers.isSkeld())
                        _ = new StaticAchievementToken("halloween");
                }
            }
        }

        public override void OnDeath(PlayerControl killer)
        {
            if (PlayerControl.LocalPlayer == player && killer != null && stealthed)
                _ = new StaticAchievementToken("ninja.another1");
            stealthed = false;
        }

        public override void PostInit()
        {
            if (PlayerControl.LocalPlayer != player) return;
            acTokenChallenge ??= new("ninja.challenge", 0, (val, _) => val >= 2);
        }

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.NinjaButton.png", 115f);
            return buttonSprite;
        }

        public static bool isStealthed(PlayerControl player)
        {
            if (isRole(player) && player && !player.Data.IsDead)
            {
                var n = players.First(x => x.player == player);
                return n.stealthed;
            }
            return false;
        }

        public static float stealthFade(PlayerControl player)
        {
            if (isRole(player) && fadeTime > 0f && player && !player.Data.IsDead)
            {
                var n = getRole(player);
                return Mathf.Min(1.0f, (float)(DateTime.UtcNow - n.stealthedAt).TotalSeconds / fadeTime);
            }
            return 1.0f;
        }

        public static bool isPenalized(PlayerControl player)
        {
            if (isRole(player) && player && !player.Data.IsDead)
            {
                Ninja n = getRole(player);
                return n.penalized;
            }
            return false;
        }

        public override void OnMeetingEnd(PlayerControl exiled = null)
        {
            if (player == PlayerControl.LocalPlayer)
                player.SetKillTimerUnchecked(GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown + (penalized ? killPenalty : 0f));
        }

        public static void setStealthed(PlayerControl player, bool stealthed = true)
        {
            if (isRole(player))
            {
                var n = getRole(player);
                n.stealthed = stealthed;
                n.stealthedAt = DateTime.UtcNow;
            }
        }

        public override void OnMeetingStart()
        {
            stealthed = false;
        }

        public static void setOpacity(PlayerControl player, float opacity)
        {
            var color = Color.Lerp(Palette.ClearWhite, Palette.White, opacity);
            try
            {
                // Block setting opacity if the Chameleon skill is active
                if (Chameleon.chameleon.Any(x => x.PlayerId == player.PlayerId) && Chameleon.visibility(player.PlayerId) < 1f && !isStealthed(player)) return;
                Helpers.setInvisible(player, color, opacity);
            }
            catch { }
        }

        public static void clearAndReload()
        {
            stealthCooldown = CustomOptionHolder.ninjaStealthCooldown.getFloat();
            stealthDuration = CustomOptionHolder.ninjaStealthDuration.getFloat();
            speedBonus = CustomOptionHolder.ninjaSpeedBonus.getFloat();
            fadeTime = CustomOptionHolder.ninjaFadeTime.getFloat();
            canUseVents = CustomOptionHolder.ninjaCanVent.getBool();
            canBeTargeted = CustomOptionHolder.ninjaCanBeTargeted.getBool();
            players = [];
        }

        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
        public static class PlayerPhysicsNinjaPatch
        {
            public static void Postfix(PlayerPhysics __instance)
            {
                if (__instance.AmOwner && __instance.myPlayer.CanMove && GameData.Instance && isStealthed(__instance.myPlayer))
                    __instance.body.velocity *= speedBonus + 1;

                if (isRole(__instance.myPlayer))
                {
                    var ninja = __instance.myPlayer;
                    if (ninja == null || ninja.Data.IsDead) return;

                    bool canSee = PlayerControl.LocalPlayer.CanSeeInvisible() || PlayerControl.LocalPlayer.Data.Role.IsImpostor;

                    var opacity = canSee ? 0.5f : 0.0f;

                    if (isStealthed(ninja))
                    {
                        opacity = Math.Max(opacity, 1.0f - stealthFade(ninja));
                        ninja.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", 0f);
                    }
                    else
                        opacity = Math.Max(opacity, stealthFade(ninja));

                    setOpacity(ninja, opacity);
                }
            }
        }
    }
}
