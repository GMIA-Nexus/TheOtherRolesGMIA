using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using HarmonyLib;
using Hazel;
using TheOtherRoles.Roles;
using TheOtherRoles.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace TheOtherRoles.Modules {
    [HarmonyPatch]
    public static class ChatCommands {
        public enum ChatTypes
        {
            Default = 0,
            LoverChat,
            JailorChat,
            ImpostorChat,
            GuesserMessage,
        }

        public enum ChannelType
        {
            Default = 0,
            Impostor,
            Lover,
            Jailor,
        }

        public static ChatTypes CurrentChatType = ChatTypes.Default;
        public static List<ChannelType> ActiveChannels = new() { ChannelType.Default };
        public static ChannelType CurrentChannel
        {
            get;
            set
            {
                field = value;
                if (!ActiveChannels.Contains(value))
                {
                    field = ChannelType.Default;
                }
            }
        } = ChannelType.Default;


        [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
        private static class SendChatPatch {
            static bool Prefix(ChatController __instance) {
                string text = __instance.freeChatField.Text;
                bool handled = false;
                if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) {
                    if (text.ToLower().StartsWith("/kick ")) {
                        string playerName = text.Substring(6);
                        PlayerControl target = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(x => x.Data.PlayerName.Equals(playerName));
                        if (target != null && AmongUsClient.Instance != null && AmongUsClient.Instance.CanBan()) {
                            var client = AmongUsClient.Instance.GetClient(target.OwnerId);
                            if (client != null) {
                                AmongUsClient.Instance.KickPlayer(client.Id, false);
                                handled = true;
                            }
                        }
                    } else if (text.ToLower().StartsWith("/ban ")) {
                        string playerName = text.Substring(5);
                        PlayerControl target = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(x => x.Data.PlayerName.Equals(playerName));
                        if (target != null && AmongUsClient.Instance != null && AmongUsClient.Instance.CanBan()) {
                            var client = AmongUsClient.Instance.GetClient(target.OwnerId);
                            if (client != null) {
                                AmongUsClient.Instance.KickPlayer(client.Id, true);
                                handled = true;
                            }
                        }
                    }
                    else if (text.ToLower().StartsWith("/gm") && TORMapOptions.gameMode != CustomGamemodes.FreePlay)
                    {
                        string gm = text.Substring(4).ToLower();
                        CustomGamemodes gameMode = CustomGamemodes.Classic;
                        if (gm.StartsWith("guess") || gm.StartsWith("gm")) // /gm guess -> guesser
                        {
                            gameMode = CustomGamemodes.Guesser;
                        }
                        else if (gm.StartsWith("hide") || gm.StartsWith("hn")) // /gm hide -> hide N seek
                        {
                            gameMode = CustomGamemodes.HideNSeek;
                        }
                        // else its classic!

                        if (AmongUsClient.Instance.AmHost)
                        {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareGamemode, Hazel.SendOption.Reliable, -1);
                            writer.Write((byte)TORMapOptions.gameMode);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.shareGamemode((byte)gameMode);
                            RPCProcedure.shareGamemode((byte)TORMapOptions.gameMode);
                        }
                        else
                        {
                            __instance.AddChat(PlayerControl.LocalPlayer, ModTranslation.getString("switchGamemodeFailed"));
                        }
                        handled = true;
                    }
                }
                
                if (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay) {
                    if (text.ToLower().Equals("/murder")) {
                        PlayerControl.LocalPlayer.Exiled();
                        FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(PlayerControl.LocalPlayer.Data, PlayerControl.LocalPlayer.Data);
                        handled = true;
                    } else if (text.ToLower().StartsWith("/color ")) {
                        handled = true;
                        int col;
                        if (!Int32.TryParse(text.Substring(7), out col)) {
                            __instance.AddChat(PlayerControl.LocalPlayer, "Unable to parse color id\nUsage: /color {id}");
                        }
                        col = Math.Clamp(col, 0, Palette.PlayerColors.Length - 1);
                        PlayerControl.LocalPlayer.SetColor(col);
                        __instance.AddChat(PlayerControl.LocalPlayer, "Changed color succesfully");;
                    } 
                }

                if (text.ToLower().StartsWith("/tp ") && PlayerControl.LocalPlayer.Data.IsDead) {
                    string playerName = text.Substring(4).ToLower();
                    PlayerControl target = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(x => x.Data.PlayerName.ToLower().Equals(playerName));
                    if (target != null) {
                        PlayerControl.LocalPlayer.transform.position = target.transform.position;
                        handled = true;
                    }
                }

                if (!handled && CurrentChannel != ChannelType.Default)
                {
                    switch (CurrentChannel)
                    {
                        case ChannelType.Impostor:
                            {
                                RPCProcedure.SendChatToChannel.Invoke((PlayerControl.LocalPlayer.PlayerId, (byte)ChannelType.Impostor, text));
                                break;
                            }
                        case ChannelType.Jailor:
                            {
                                RPCProcedure.SendChatToChannel.Invoke((PlayerControl.LocalPlayer.PlayerId, (byte)ChannelType.Jailor, text));
                                break;
                            }
                        case ChannelType.Lover:
                            {
                                RPCProcedure.SendChatToChannel.Invoke((PlayerControl.LocalPlayer.PlayerId, (byte)ChannelType.Lover, text));
                                break;
                            }
                    }
                    __instance.freeChatField.textArea.Clear();
                    return false;
                }

                if (handled) {
                    __instance.freeChatField.Clear();
                    __instance.quickChatMenu.Clear();
                }
                return !handled;
            }
        }
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        public static class EnableChat {
            public static void Postfix(HudManager __instance) {
                if (!__instance.Chat.isActiveAndEnabled && (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay || (Lovers.enableChat && PlayerControl.LocalPlayer.isLovers())))
                    __instance.Chat.SetVisible(true);
            }
        }

        [HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetName))]
        public static class SetBubbleName { 
            public static void Postfix(ChatBubble __instance, [HarmonyArgument(0)] string playerName) {
                PlayerControl sourcePlayer = PlayerControl.AllPlayerControls.ToArray().ToList().FirstOrDefault(x => x.Data != null && x.Data.PlayerName.Equals(playerName));
                if (sourcePlayer == null) return;
                if (PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data?.Role?.IsImpostor == true && (sourcePlayer.isRole(RoleId.Spy) || Sidekick.players.Any(x => x.wasTeamRed && x.player != null && x.player == sourcePlayer) || Jackal.players.Any(x => x.wasTeamRed && x.player != null && x.player == sourcePlayer)) && __instance != null) __instance.NameText.color = Palette.ImpostorRed;
                if (MeetingHud.Instance)
                {
                    if (Jailor.players.Any(x => x.player == PlayerControl.LocalPlayer && x.jailTarget == sourcePlayer) && !sourcePlayer.Data.IsDead && !PlayerControl.LocalPlayer.Data.IsDead)
                    {
                        __instance.NameText.color = Jailor.color;
                        __instance.NameText.text = playerName + $" ({ModTranslation.getString("jailorJailed")})";
                    }
                    if ((PlayerControl.LocalPlayer == sourcePlayer || Helpers.shouldShowGhostInfo()) && Jailor.isJailed(sourcePlayer.PlayerId))
                    {
                        __instance.NameText.color = Jailor.color;
                        __instance.NameText.text = playerName + $" ({ModTranslation.getString("jailorJailed")})";
                    }
                }

                var currentType = CurrentChatType;
                CurrentChatType = ChatTypes.Default;

                switch (currentType)
                {
                    case ChatTypes.JailorChat:
                        if (MeetingHud.Instance)
                        {
                            if (Jailor.isJailed(PlayerControl.LocalPlayer.PlayerId) || Helpers.shouldShowGhostInfo())
                            {
                                __instance.NameText.color = Jailor.color;
                                __instance.NameText.text = (Helpers.shouldShowGhostInfo() ? __instance.NameText.text : "") + $" ({ModTranslation.getString("jailor")})";
                            }
                            else if (PlayerControl.LocalPlayer.isRole(RoleId.Jailor) || Helpers.shouldShowGhostInfo())
                            {
                                __instance.NameText.color = Jailor.color;
                                __instance.NameText.text = $"({ModTranslation.getString("jailor")})";
                            }
                        }
                        break;
                    case ChatTypes.Default:
                        break;
                    case ChatTypes.LoverChat:
                        __instance.NameText.color = Lovers.getColor(sourcePlayer);
                        __instance.NameText.text = $"{__instance.NameText.text} ({ModTranslation.getString("lover")})";
                        break;
                    case ChatTypes.GuesserMessage:
                        __instance.NameText.color = Color.yellow;
                        __instance.NameText.text = $"{__instance.NameText.text} ({ModTranslation.getString("guesser")})";
                        break;
                    case ChatTypes.ImpostorChat:
                        __instance.NameText.color = Palette.ImpostorRed;
                        __instance.NameText.text = $"{__instance.NameText.text} ({ModTranslation.getString("impostor")})";
                        break;
                    default:
                        break;
                }
            }
        }

        [HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
        public static class AddChat {
            public static bool Prefix(ChatController __instance, [HarmonyArgument(0)] PlayerControl sourcePlayer) {
                if (__instance != FastDestroyableSingleton<HudManager>.Instance.Chat)
                    return true;
                PlayerControl localPlayer = PlayerControl.LocalPlayer;
                if (sourcePlayer == localPlayer) return true;
                var flag = MeetingHud.Instance
                    || LobbyBehaviour.Instance
                    || (localPlayer.Data.IsDead && (RoleManager.IsGhostRole(localPlayer.Data.RoleType) || CustomGameModes.FreePlayGM.isFreePlayGM));
                if (Jailor.isJailed(sourcePlayer.PlayerId) && !localPlayer.Data.IsDead && MeetingHud.Instance && !Jailor.players.Any(x => x.jailTarget == sourcePlayer && x.player == localPlayer) && !Helpers.shouldShowGhostInfo()) return false;
                if (sourcePlayer == localPlayer.getPartner() || flag) return true;

                return flag;

            }
        }

        public static GameObject ChannelShower;
        static private GameObject TeamChatButton;
        private static MetaContext.Image TeamChatSwitch = MetaContext.SpriteLoader.FromResource("TheOtherRoles.Resources.TeamChatSwitch.png", 105f);
        [HarmonyPatch(typeof(ChatController), nameof(ChatController.Awake)), HarmonyPostfix]
        public static void ChatControllerAwake_Postfix(ChatController __instance)
        {
            __instance.freeChatField.textArea.SetText("");
            __instance.timeSinceLastMessage = 0;
            if (ChannelShower != null) return;
            ChannelShower = UnityEngine.Object.Instantiate(__instance.freeChatField.charCountText.gameObject, __instance.freeChatField.charCountText.transform.parent);
            ChannelShower.name = "Channel Shower";
            ChannelShower.transform.localPosition = new Vector3(1.95f, 0.5f, 0f);
            ChannelShower.GetComponent<RectTransform>().sizeDelta = new Vector2(5f, 0.1f);
            var tmp = ChannelShower.GetComponent<TextMeshPro>();
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.color = Color.black;
            tmp.outlineColor = Color.white;
            tmp.outlineWidth = 0.1f;
            tmp.fontSize *= 1.2f;
            tmp.SetText("");
            CurrentChannel = CurrentChannel; // Check Channel
        }

        public static void CreateTeamChatButton()
        {
            if (GameStates.IsLobby || TeamChatButton) return;

            var ChatScreenContainer = GameObject.Find("ChatScreenContainer");
            var BanMenu = ChatScreenContainer.transform.FindChild("BanMenuButton");

            TeamChatButton = UnityEngine.Object.Instantiate(BanMenu.gameObject, BanMenu.transform.parent);
            TeamChatButton.GetComponent<PassiveButton>().OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
            TeamChatButton.GetComponent<PassiveButton>().OnClick.AddListener((Action)(() => { SwitchNextChannel(); }));
            TeamChatButton.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = TeamChatSwitch.GetSprite();
            TeamChatButton.name = "FactionChat";
            var pos = BanMenu.transform.localPosition;
            TeamChatButton.transform.localPosition = new Vector3(pos.x, pos.y + 0.7f, pos.z);
        }

        [HarmonyPatch(typeof(ChatController), nameof(ChatController.Update)), HarmonyPostfix]
        public static void ChatControllerUpdate_Postfix(ChatController __instance)
        {
            if (GameStates.IsLobby) return;
            UpdateChatChannels();
            KeyboardInput(__instance);
            bool jailed = Jailor.isJailed(PlayerControl.LocalPlayer.PlayerId) && MeetingHud.Instance;
            if (TeamChatButton != null) TeamChatButton.SetActive(ActiveChannels.Count > 1 && !jailed);
            if (ChannelShower != null)
            {
                try
                {
                    var text = ModTranslation.getString($"chatChannel{Enum.GetName(CurrentChannel)}");
                    if (jailed) text = $"{ModTranslation.getString("chatChannelJailor")}";
                    ChannelShower?.GetComponent<TextMeshPro>().SetText(text);
                    ChannelShower?.SetActive(!ChannelShower.transform.parent.parent.FindChild("RateMessage (TMP)").gameObject.activeSelf);
                }
                catch { }
            }
        }

        [HarmonyPatch(typeof(ChatController), nameof(ChatController.Toggle))]
        public static class TogglePatch
        {
            public static void Postfix(ChatController __instance)
            {
                if (!__instance.IsOpenOrOpening) return;
                CreateTeamChatButton();
            }
        }

        static void SwitchNextChannel()
        {
            var channels = ActiveChannels.ToList();
            if (channels.Count == 0) { CurrentChannel = ChannelType.Default; return; }
            var currentIndex = channels.IndexOf(CurrentChannel);
            var nextIndex = (currentIndex + 1) % channels.Count;
            CurrentChannel = channels[nextIndex];
        }

        public static void KeyboardInput(ChatController __instance)
        {
            if (Jailor.players.Any(x => x.player != null && !x.player.Data.IsDead && x.jailTarget == PlayerControl.LocalPlayer) && MeetingHud.Instance) { CurrentChannel = ChannelType.Default; return; }
            if (Input.GetKeyDown(KeyCode.Mouse1)) SwitchNextChannel();
        }

        public static void UpdateChatChannels()
        {
            var channelConditions = new Dictionary<ChannelType, Func<PlayerControl, bool>>
            {
                [ChannelType.Default] = (x) => GameStates.IsLobby || MeetingHud.Instance || Helpers.shouldShowGhostInfo(),
                [ChannelType.Lover] = (x) => x.isLovers() && !x.Data.IsDead,
                [ChannelType.Jailor] = (x) => MeetingHud.Instance && !x.Data.IsDead && Jailor.players.Any(jailor => jailor.jailTarget != null && !jailor.jailTarget.Data.IsDead && jailor.player == x),
                [ChannelType.Impostor] = (x) => CustomOptionHolder.enableImpostorChat.getBool() && !x.Data.IsDead && x.Data.Role.IsImpostor && !Spy.exists
            };

            foreach (var (channelType, condition) in channelConditions)
            {
                if (condition(PlayerControl.LocalPlayer) && !ActiveChannels.Contains(channelType))
                {
                    ActiveChannels.Add(channelType);
                }
                else if (!condition(PlayerControl.LocalPlayer) && (ActiveChannels.Contains(channelType) || CurrentChannel == channelType))
                {
                    ActiveChannels.Remove(channelType);
                    if (CurrentChannel == channelType)
                    {
                        CurrentChannel = ActiveChannels.FirstOrDefault();
                    }
                }
            }
        }
    }
}
