using TheOtherRoles.Modules;
using UnityEngine;
using static TheOtherRoles.Patches.PlayerControlFixedUpdatePatch;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    [TORRPCHolder]
    public class Noisemaker : RoleBase<Noisemaker>
    {
        public static Color32 color = new(160, 131, 187, byte.MaxValue);
        public static PlayerControl noisemaker;
        public PlayerControl currentTarget = null;
        public PlayerControl target = null;

        public Noisemaker()
        {
            RoleId = roleId = RoleId.Noisemaker;
            currentTarget = null;
            target = null;
            numSound = Mathf.RoundToInt(CustomOptionHolder.noisemakerSoundNumber.getFloat());
        }

        public override AudioClip IntroSound { get => MetaContext.VanillaAsset.NoisemakerIntroSound; }

        public static RemoteProcess<(byte playerId, byte noisemakerId)> SetSounded = new("NoisemakerSetSounded", (message, _) =>
        {
            PlayerControl player = Helpers.playerById(message.playerId);
            PlayerControl noisem = Helpers.playerById(message.noisemakerId);
            var noisemaker = getRole(noisem);
            if (noisemaker == null || noisem == null) return;
            noisemaker.target = player;
        });

        public override void FixedUpdate()
        {
            if (PlayerControl.LocalPlayer != player) return;
            currentTarget = setTarget();
            if (target == null && numSound > 0)
                setPlayerOutline(currentTarget, color);
        }

        public enum SoundTarget
        {
            Noisemaker,
            Crewmates,
            Everyone
        }

        public static float cooldown;
        public static float duration;
        public int numSound = 0;
        public static SoundTarget soundTarget;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.NoisemakerButton.png", 115f);
            return buttonSprite;
        }

        public static void clearAndReload()
        {
            cooldown = CustomOptionHolder.noisemakerCooldown.getFloat();
            duration = CustomOptionHolder.noisemakerSoundDuration.getFloat();
            soundTarget = (SoundTarget)CustomOptionHolder.noisemakerSoundTarget.getSelection();
            players = [];
        }
    }
}
