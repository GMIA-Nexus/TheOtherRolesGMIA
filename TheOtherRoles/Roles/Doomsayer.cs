using System.Collections.Generic;
using System.Linq;
using TheOtherRoles.MetaContext;
using TheOtherRoles.Modules;
using TheOtherRoles.Utilities;
using UnityEngine;
using static TheOtherRoles.Patches.PlayerControlFixedUpdatePatch;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    [TORRPCHolder]
    public class Doomsayer : RoleBase<Doomsayer>
    {
        public Doomsayer()
        {
            RoleId = roleId = RoleId.Doomsayer;
            usesLeft = canObserve ? Mathf.RoundToInt(CustomOptionHolder.doomsayerNumberOfObserves.getFloat()) : 0;
            counter = 0;
            triggerWin = false;
            failedGuesses = 0;
            observed = null;
            currentTarget = null;
        }

        public static RemoteProcess<(byte playerId, byte doomsayerId)> Observe = new("DoomsayerObserve", (message, _) =>
        {
            PlayerControl player = Helpers.playerById(message.playerId);
            PlayerControl doomsayer = Helpers.playerById(message.doomsayerId);
            var doomsayerRole = getRole(doomsayer);
            if (player == null || doomsayer == null || doomsayerRole == null) return;
            doomsayerRole.observed = player;
        });

        static public IEnumerable<HelpSprite> GetHelpSprites() => [new(getButtonSprite(), "doomsayerObserveHint")];

        public static Color color = new(0f, 1f, 0.5f, 1f);

        public static int guessesToWin = 4;
        public int counter = 0;
        public static bool hasMultipleGuesses = true;
        public static float cooldown = 30f;
        public static bool indicateGuesses = true;
        public static bool canObserve = false;
        public bool triggerWin = false;
        public int failedGuesses = 0;
        public static int maxMisses = 3;
        public int usesLeft = 3;

        public override void FixedUpdate()
        {
            if (PlayerControl.LocalPlayer != player || PlayerControl.LocalPlayer.Data.IsDead || usesLeft <= 0) return;
            currentTarget = setTarget();
            setPlayerOutline(currentTarget, color);
        }

        public override void OnMeetingStart()
        {
            if (player != null && observed != null && (PlayerControl.LocalPlayer == player || Helpers.shouldShowGhostInfo()) && !player.Data.IsDead)
            {
                string msg = "";
                var list = new List<RoleInfo>();
                var info = RoleInfo.getRoleInfoForPlayer(observed, false, true).FirstOrDefault();
                if (RoleInfo.Killing.Contains(info)) {
                    msg = "doomsayerKillingInfo";
                    list = RoleInfo.Killing;
                    _ = new StaticAchievementToken("doomsayer.common2.killing");
                } else if (RoleInfo.Trick.Contains(info)) {
                    msg = "doomsayerTrickInfo";
                    list = RoleInfo.Trick;
                    _ = new StaticAchievementToken("doomsayer.common2.trick");
                } else if (RoleInfo.Detect.Contains(info)) {
                    msg = "doomsayerDetectInfo";
                    list = RoleInfo.Detect;
                    _ = new StaticAchievementToken("doomsayer.common2.detect");
                } else if (RoleInfo.Panic.Contains(info)) {
                    msg = "doomsayerPanicInfo";
                    list = RoleInfo.Panic;
                    _ = new StaticAchievementToken("doomsayer.common2.panic");
                } else if (RoleInfo.Body.Contains(info)) {
                    msg = "doomsayerBodyInfo";
                    list = RoleInfo.Body;
                    _ = new StaticAchievementToken("doomsayer.common2.body");
                } else if (RoleInfo.Team.Contains(info)) {
                    msg = "doomsayerTeamInfo";
                    list = new(RoleInfo.Team);
                    list.RemoveAll(x => x.roleId == RoleId.BomberB);
                    _ = new StaticAchievementToken("doomsayer.common2.team");
                } else if (RoleInfo.Protection.Contains(info)) {
                    msg = "doomsayerProtectionInfo";
                    list = RoleInfo.Protection;
                    _ = new StaticAchievementToken("doomsayer.common2.protection");
                } else if (RoleInfo.Outlook.Contains(info)) {
                    msg = "doomsayerOutlookInfo";
                    list = RoleInfo.Outlook;
                    _ = new StaticAchievementToken("doomsayer.common2.outlook");
                } else if (RoleInfo.Hunting.Contains(info)) {
                    msg = "doomsayerHuntingInfo";
                    list = RoleInfo.Hunting;
                    _ = new StaticAchievementToken("doomsayer.common2.hunting");
                } else if (info.roleId is RoleId.Crewmate or RoleId.Impostor) {
                    msg = "doomsayerRolelessInfo";
                    list = [RoleInfo.crewmate, RoleInfo.impostor];
                }
                if (!string.IsNullOrEmpty(msg))
                    msg = string.Format(ModTranslation.getString(msg), observed.Data.PlayerName) + "\n(" + string.Join(", ", list.Select(x => x.name)) +")";
                else
                    msg = string.Format(ModTranslation.getString("doomsayerNoneInfo"), observed.Data.PlayerName);
                    
                if (PlayerControl.LocalPlayer == player)
                    MeetingOverlayHolder.RegisterOverlay(TORGUIContextEngine.API.VerticalHolder(GUIAlignment.Left,
                    new TORGUIText(GUIAlignment.Left, TORGUIContextEngine.API.GetAttribute(AttributeAsset.OverlayTitle), new TranslateTextComponent("doomsayerInfo")),
                    new TORGUIText(GUIAlignment.Left, TORGUIContextEngine.API.GetAttribute(AttributeAsset.OverlayContent), new RawTextComponent(msg)))
                    , MeetingOverlayHolder.IconsSprite[2], color);
                FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(player, msg, false);
            }
        }

        public override void OnMeetingEnd(PlayerControl exiled = null)
        {
            observed = null;
        }

        public PlayerControl observed;
        public PlayerControl currentTarget;

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.DoomsayerButton.png", 115f);
            return buttonSprite;
        }

        public static void clearAndReload()
        {
            cooldown = CustomOptionHolder.doomsayerObserveCooldown.getFloat();
            guessesToWin = Mathf.RoundToInt(CustomOptionHolder.doomsayerGuessesToWin.getFloat());
            hasMultipleGuesses = CustomOptionHolder.doomsayerMultipleGuesses.getBool();
            canObserve = CustomOptionHolder.doomsayerCanObserve.getBool();
            maxMisses = Mathf.RoundToInt(CustomOptionHolder.doomsayerMaxMisses.getFloat());
            indicateGuesses = CustomOptionHolder.doomsayerIndicator.getBool();
            players = [];
        }
    }
}
