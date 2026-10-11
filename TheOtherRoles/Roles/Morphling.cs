using System.Collections.Generic;
using TheOtherRoles.Modules;
using UnityEngine;
using static TheOtherRoles.Patches.PlayerControlFixedUpdatePatch;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    [TORRPCHolder]
    public class Morphling : RoleBase<Morphling> {
        public static Color color = Palette.ImpostorRed;
        private static Sprite sampleSprite;
        private static Sprite morphSprite;

        static public IEnumerable<HelpSprite> GetHelpSprites()
        {
            yield return new(getSampleSprite(), "morphlingSampleHint");
            yield return new(getMorphSprite(), "morphlingMorphHint");
        }

        public override AudioClip IntroSound { get => MetaContext.VanillaAsset.ShapeshifterIntroSound; }

        public Morphling()
        {
            RoleId = roleId = RoleId.Morphling;
            currentTarget = null;
            sampledTarget = null;
            morphTarget = null;
            morphTimer = 0f;
            acTokenChallenge = null;
        }

        public static RemoteProcess<(byte playerId, byte morphlingId)> Morph = new("MorphlingMorph", (message, _) =>
        {
            PlayerControl target = Helpers.playerById(message.playerId);
            Morphling morphling = getRole(Helpers.playerById(message.morphlingId));
            if (morphling == null || target == null) return;

            morphling.morphTarget = target;
            morphling.player.GetOutfitInfo().AddOutfit(new("Morphling", 50, true, target.GetOutfitInfo()?.GetOutfit(75)));
            morphling.morphTimer = duration;
        });

        public static float cooldown = 30f;
        public static float duration = 10f;

        public PlayerControl currentTarget;
        public PlayerControl sampledTarget;
        public PlayerControl morphTarget;
        public float morphTimer = 0f;

        public AchievementToken<(byte playerId, bool kill, bool cleared)> acTokenChallenge;

        public void resetMorph() {
            morphTarget = null;
            morphTimer = 0f;
            if (player == null) return;
            player.GetOutfitInfo().RemoveOutfit("Morphling");
        }

        public override void OnMeetingStart()
        {
            resetMorph();
        }

        public override void FixedUpdate()
        {
            morphlingSetTarget();
            morphlingUpdateTimer();
        }

        void morphlingSetTarget()
        {
            if (player != PlayerControl.LocalPlayer) return;
            currentTarget = setTarget();
            setPlayerOutline(currentTarget, color);
        }

        void morphlingUpdateTimer()
        {
            float oldMorphlingTimer = morphTimer;
            morphTimer = Mathf.Max(0f, morphTimer - Time.fixedDeltaTime);
            if ((oldMorphlingTimer > 0f && morphTimer <= 0f) || player?.Data?.IsDead == true)
                resetMorph();
        }

        public override void OnMeetingEnd(PlayerControl exiled = null)
        {
            sampledTarget = null;
            if (PlayerControl.LocalPlayer == player)
            {
                acTokenChallenge.Value.cleared |= exiled != null && acTokenChallenge.Value.kill && acTokenChallenge.Value.playerId == exiled.PlayerId;
                acTokenChallenge.Value.playerId = byte.MaxValue;
                acTokenChallenge.Value.kill = false;
            }
        }

        public override void PostInit()
        {
            if (PlayerControl.LocalPlayer != player) return;
            acTokenChallenge ??= new("morphling.challenge", (byte.MaxValue, false, false), (val, _) => val.cleared);
        }

        public override void ResetRole(bool isShifted)
        {
            resetMorph();
        }

        public override void OnKill(PlayerControl target)
        {
            if (PlayerControl.LocalPlayer == player && morphTimer > 0f && morphTarget != null)
                acTokenChallenge.Value.kill = true;
        }

        public override void OnDeath(PlayerControl killer = null)
        {
            if (killer != null && PlayerControl.LocalPlayer == player && morphTimer > 0f && morphTarget != null)
                _ = new StaticAchievementToken("morphling.another1");
            resetMorph();
        }

        public static void clearAndReload() {
            cooldown = CustomOptionHolder.morphlingCooldown.getFloat();
            duration = CustomOptionHolder.morphlingDuration.getFloat();
            players = [];
        }

        public static Sprite getSampleSprite() {
            if (sampleSprite) return sampleSprite;
            sampleSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.SampleButton.png", 115f);
            return sampleSprite;
        }

        public static Sprite getMorphSprite() {
            if (morphSprite) return morphSprite;
            morphSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.MorphButton.png", 115f);
            return morphSprite;
        }
    }
}
