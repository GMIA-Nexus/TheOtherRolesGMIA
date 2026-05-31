using System.Collections.Generic;
using System.Linq;
using TheOtherRoles.MetaContext;
using TheOtherRoles.Modules;
using TheOtherRoles.Objects;
using TheOtherRoles.Utilities;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    [TORRPCHolder]
    public class Yoyo : RoleBase<Yoyo>
    {
        public static Color color = Palette.ImpostorRed;

        public Yoyo()
        {
            RoleId = roleId = RoleId.Yoyo;
            markedLocation = null;
            blackout = [];
        }

        static public IEnumerable<HelpSprite> GetHelpSprites()
        {
            yield return new(getMarkButtonSprite(), "yoyoMarkHint");
            yield return new(getBlinkButtonSprite(), "yoyoBlinkHint");
        }
        public static readonly Image Illustration = new TORSpriteLoader("Assets/Sprites/YoYo.png");
        static public IEnumerable<DocumentReplacement> GetReplacementPart()
        {
            yield return new("%SEC%", blinkDuration.ToString());
            yield return new("%RAN%", blackoutRange.ToString());
            yield return new("%DUR%", blackoutDuration.ToString());
        }

        public static float blinkDuration { get { return CustomOptionHolder.yoyoBlinkDuration.getFloat(); } }
        public static float markCooldown = 0;
        public static bool markStaysOverMeeting = false;
        public float SilhouetteVisibility => silhouetteVisibility == 0 && (PlayerControl.LocalPlayer == player || PlayerControl.LocalPlayer.Data.IsDead) ? 0.1f : silhouetteVisibility;
        public static float silhouetteVisibility = 0;
        public static float blackoutRange { get { return CustomOptionHolder.yoyoBlackoutRange.getFloat(); } }
        public static float blackoutDuration { get { return CustomOptionHolder.yoyoBlackoutDuration.getFloat(); } }

        public Vector3? markedLocation = null;

        private static Sprite markButtonSprite;

        public static RemoteProcess<(Vector3 pos, byte playerId)> MarkLocation = new("YoyoMarkLocation", (message, _) =>
        {
            PlayerControl player = Helpers.playerById(message.playerId);
            var yoyo = getRole(player);
            if (player == null || yoyo == null) return;
            yoyo.markLocation(message.pos);
            new Silhouette(message.pos, player, -1, false);
        });

        public static RemoteProcess<(bool isFirstJump, Vector3 pos, byte playerId)> Blink = new("YoyoBlink", (message, _) =>
        {
            PlayerControl player = Helpers.playerById(message.playerId);
            var yoyo = getRole(player);
            TheOtherRolesPlugin.Logger.LogMessage($"blink fistjumpo: {message.isFirstJump}");
            if (player == null || yoyo == null || yoyo.markedLocation == null) return;
            var markedPos = (Vector3)yoyo.markedLocation;
            player.NetTransform.SnapTo(markedPos);

            var markedSilhouette = Silhouette.silhouettes.FirstOrDefault(s => s.gameObject.transform.position.x == markedPos.x && s.gameObject.transform.position.y == markedPos.y);
            if (markedSilhouette != null)
                markedSilhouette.permanent = false;

            // Create Silhoutte At Start Position:
            if (message.isFirstJump)
            {
                yoyo.markLocation(message.pos);
                new Silhouette(message.pos, player, blinkDuration, true);
            }
            else
            {
                new Silhouette(message.pos, player, 5, true);
                yoyo.markedLocation = null;
            }
            if (Chameleon.chameleon.Any(x => x.PlayerId == message.playerId)) // Make the Yoyo visible if chameleon!
                Chameleon.lastMoved[message.playerId] = Time.time;
        });

        public static RemoteProcess<(byte playerId, byte yoyoId)> Blackout = new("YoyoBlackout", (message, _) =>
        {
            PlayerControl player = Helpers.playerById(message.playerId);
            var yoyoPlayer = Helpers.playerById(message.yoyoId);
            var yoyo = getRole(yoyoPlayer);
            if (player == null || yoyo == null) return;
            if (yoyo.blackout.Contains(player)) return;
            yoyo.blackout.Add(player);
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(blackoutDuration + 0.5f, new System.Action<float>((p) => { if (p == 1f || MeetingHud.Instance) yoyo.blackout.Remove(player); })));
            if (PlayerControl.LocalPlayer == player)
            {
                Helpers.flashScreen(new(0, 0, 0), 0.1f, 0.4f, 1f, blackoutDuration, ModTranslation.getString("yoyoBlackoutHint"), textColor: Color.white, Helpers.RoleIcons.GetSprite(6));
            }
            else if (PlayerControl.LocalPlayer == yoyoPlayer)
                new StaticAchievementToken("yoyo.challenge2");
        });

        public void ActivateBlackout(Vector3 pos)
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (!player.Data.Role.IsImpostor && !player.Data.IsDead
                && Vector3.Distance(pos, player.transform.position) <= blackoutRange)
                {
                    Blackout.Invoke((player.PlayerId, this.player.PlayerId));
                }
            }
        }

        public List<PlayerControl> blackout = [];

        public static Sprite getMarkButtonSprite()
        {
            if (markButtonSprite) return markButtonSprite;
            markButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.YoyoMarkButtonSprite.png", 115f);
            return markButtonSprite;
        }
        private static Sprite blinkButtonSprite;

        public static Sprite getBlinkButtonSprite()
        {
            if (blinkButtonSprite) return blinkButtonSprite;
            blinkButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.YoyoBlinkButtonSprite.png", 115f);
            return blinkButtonSprite;
        }

        public void markLocation(Vector3 position)
        {
            markedLocation = position;
        }

        public override void OnMeetingEnd(PlayerControl exiled = null)
        {
            if (!markStaysOverMeeting) markedLocation = null;
            blackout = [];
        }

        public override void ResetRole(bool isShifted)
        {
            markedLocation = null;
            blackout = [];
            Silhouette.clearSilhouettes(player);
        }

        public static void clearAndReload()
        {
            markCooldown = CustomOptionHolder.yoyoMarkCooldown.getFloat();
            markStaysOverMeeting = CustomOptionHolder.yoyoMarkStaysOverMeeting.getBool();
            silhouetteVisibility = CustomOptionHolder.yoyoSilhouetteVisibility.getSelection() / 10f;
            players = [];
        }
    }
}
