using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TheOtherRoles.Modules;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    [TORRPCHolder]
    public class Teleporter : RoleBase<Teleporter>
    {
        public Teleporter()
        {
            RoleId = roleId = RoleId.Teleporter;
            teleportNumber = (int)CustomOptionHolder.teleporterTeleportNumber.getFloat();
            acTokenChallenge = null;
            SwappingMenus = false;
            target1 = null;
            target2 = null;
        }

        public static Color color = new Color32(164, 249, 255, byte.MaxValue);
        private static Sprite teleportButtonSprite;
        public static float teleportCooldown = 30f;
        public int teleportNumber = 5;
        public PlayerControl target1;
        public PlayerControl target2;
        public bool SwappingMenus;

        public AchievementToken<(byte target1, byte target2, DateTime swapTime, bool cleared)> acTokenChallenge = null;

        public static RemoteProcess<(byte target1Id, byte target2Id)> Teleport = new("TeleporterTeleport", (message, _) =>
        {
            PlayerControl target1 = Helpers.playerById(message.target1Id);
            PlayerControl target2 = Helpers.playerById(message.target2Id);

            target1.MyPhysics.ResetMoveState();
            target2.MyPhysics.ResetMoveState();
            var targetPosition = target1.GetTruePosition();
            var TempFacing = target1.cosmetics.currentBodySprite.BodySprite.flipX;
            target1.NetTransform.SnapTo(new Vector2(target2.GetTruePosition().x, target2.GetTruePosition().y + 0.3636f));
            target1.cosmetics.currentBodySprite.BodySprite.flipX = target2.cosmetics.currentBodySprite.BodySprite.flipX;
            target2.NetTransform.SnapTo(new Vector2(targetPosition.x, targetPosition.y + 0.3636f));
            target2.cosmetics.currentBodySprite.BodySprite.flipX = TempFacing;

            if (PlayerControl.LocalPlayer == target1 || PlayerControl.LocalPlayer == target2)
            {
                Helpers.showFlash(color);
                if (Minigame.Instance) Minigame.Instance.Close();
                var messageText = Helpers.CreateAndShowNotification(ModTranslation.getString("teleporterNotification"), Color.white, new Vector3(0f, 1f, -20f), spr: RoleHelpers.GetRoleIcon(RoleId.Teleporter).GetSprite());
                messageText.AdjustNotification();
            }
        });

        public override void PostInit()
        {
            if (PlayerControl.LocalPlayer != player) return;
            acTokenChallenge ??= new("teleporter.challenge", (byte.MaxValue, byte.MaxValue, DateTime.UtcNow, false), (val, _) => val.cleared);
        }

        public static Sprite getButtonSprite()
        {
            if (teleportButtonSprite) return teleportButtonSprite;
            teleportButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.TeleporterButton.png", 115f);
            return teleportButtonSprite;
        }

        public static void clearAndReload()
        {
            teleportCooldown = CustomOptionHolder.teleporterCooldown.getFloat();
            players = [];
        }

        public IEnumerator OpenSecondMenu()
        {
            try
            {
                PlayerMenu.singleton.Menu.ForceClose();
            }
            catch
            {

            }
            yield return new WaitForSeconds(0.05f);
            SwappingMenus = false;
            if (MeetingHud.Instance || PlayerControl.LocalPlayer != player) yield break;
            List<byte> transportTargets = [];
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (!player.Data.Disconnected && player != target1)
                {
                    if (!player.Data.IsDead) transportTargets.Add(player.PlayerId);
                    else
                    {
                        foreach (var body in UnityEngine.Object.FindObjectsOfType<DeadBody>())
                            if (body.ParentId == player.PlayerId) transportTargets.Add(player.PlayerId);
                    }
                }
            }
            byte[] transporttargetIDs = transportTargets.ToArray();
            var pk = new PlayerMenu((x) =>
            {
                target2 = x;
                if (target1 != null && target2 != null && !target1.Data.IsDead && !target2.Data.IsDead && target1.moveable && target2.moveable)
                {
                    _ = new StaticAchievementToken("teleporter.common1");
                    acTokenChallenge.Value.swapTime = DateTime.UtcNow;
                    acTokenChallenge.Value.target1 = target1.PlayerId;
                    acTokenChallenge.Value.target2 = target2.PlayerId;
                    Teleport.Invoke((target1.PlayerId, target2.PlayerId));
                }
                target1 = null;
                target2 = null;
                HudManagerStartPatch.teleporterTeleportButton.Timer = HudManagerStartPatch.teleporterTeleportButton.MaxTimer;
                teleportNumber--;
                SoundEffectsManager.play("teleporterTeleport");
            }, (y) =>
            {
                return transporttargetIDs.Contains(y.PlayerId);
            });
            Coroutines.Start(pk.Open(0f, true));
        }
        private static float[] PanelAreaScale = [1f, 0.95f, 0.76f];
        private static (int x, int y)[] PanelAreaSize = [(3, 5), (3, 6), (4, 6)];
        private static Vector3[] PanelAreaOffset = [new(0.0f, 0.0f, -1f), new(0.1f, 0.145f, -1f), new(-0.355f, 0.0f, -1f)];
        private static (float x, float y)[] PanelAreaMultiplier = [(1f, 1f), (1f, 0.89f), (275f * (float)Math.PI / 887f, 1f)];
        private static Vector3 ToVoteAreaPos(ShapeshifterMinigame minigame, int index, int arrangeType) => Helpers.convertPos(index, arrangeType, PanelAreaSize, new Vector3(minigame.XStart, minigame.YStart, -1f), PanelAreaOffset, new Vector3(minigame.XOffset, minigame.YOffset), PanelAreaScale, PanelAreaMultiplier);

        public class PlayerMenu
        {
            public ShapeshifterMinigame Menu;
            public Select Click;
            public Include Inclusion;
            public List<PlayerControl> Targets;
            public static PlayerMenu singleton;
            public delegate void Select(PlayerControl player);
            public delegate bool Include(PlayerControl player);

            public PlayerMenu(Select click, Include inclusion)
            {
                Click = click;
                Inclusion = inclusion;
                if (singleton != null)
                {
                    singleton.Menu.DestroyImmediate();
                    singleton = null;
                }
                singleton = this;
            }

            public IEnumerator Open(float delay, bool includeDead = false)
            {
                yield return new WaitForSecondsRealtime(delay);
                while (ExileController.Instance != null) yield return 0;
                Targets = PlayerControl.AllPlayerControls.ToArray().Where(x => Inclusion(x) && (!x.Data.IsDead || includeDead) && !x.Data.Disconnected).ToList();
                TheOtherRolesPlugin.Logger.LogMessage($"Targets {Targets.Count}");
                if (Menu == null)
                {
                    if (Camera.main == null)
                        yield break;

                    Menu = UnityEngine.Object.Instantiate(GetShapeshifterMenu(), Camera.main.transform, false);
                }

                Menu.transform.SetParent(Camera.main.transform, false);
                Menu.transform.localPosition = new(0f, 0f, -50f);
                Menu.Begin(null);
            }

            private static ShapeshifterMinigame GetShapeshifterMenu()
            {
                var rolePrefab = RoleManager.Instance.AllRoles.ToArray().First(r => r.Role == RoleTypes.Shapeshifter);
                return UnityEngine.Object.Instantiate(rolePrefab?.Cast<ShapeshifterRole>(), GameData.Instance.transform).ShapeshifterMenu;
            }

            public void Clicked(PlayerControl player)
            {
                Click(player);
                Menu.Close();
            }

            [HarmonyPatch(typeof(ShapeshifterMinigame), nameof(ShapeshifterMinigame.Begin))]
            public static class MenuPatch
            {
                public static bool Prefix(ShapeshifterMinigame __instance)
                {
                    PlayerControl.LocalPlayer.MyPhysics.ResetMoveState(false);
                    PlayerControl.LocalPlayer.NetTransform.Halt();
                    var menu = singleton;

                    if (menu == null)
                        return true;

                    __instance.potentialVictims = new();
                    var list2 = new Il2CppSystem.Collections.Generic.List<UiElement>();

                    for (var i = 0; i < menu.Targets.Count; i++)
                    {
                        int displayType = Helpers.GetDisplayType(menu.Targets.Count);
                        var player = menu.Targets[i];
                        bool isDead = player.Data.IsDead;
                        player.Data.IsDead = false;
                        var num = i % 3;
                        var num2 = i / 3;
                        var panel = UnityEngine.Object.Instantiate(__instance.PanelPrefab, __instance.transform);
                        panel.transform.localScale *= PanelAreaScale[displayType];
                        panel.transform.localPosition = ToVoteAreaPos(__instance, i, displayType);
                        panel.SetPlayer(i, player.Data, (Action)(() => menu.Clicked(player)));
                        panel.transform.FindChild("Nameplate/Highlight/ShapeshifterIcon").gameObject.SetActive(false);
                        panel.Background.gameObject.GetComponent<ButtonRolloverHandler>().OverColor = color;
                        __instance.potentialVictims.Add(panel);
                        list2.Add(panel.Button);
                        player.Data.IsDead = isDead;
                    }

                    var Phone = __instance.transform.Find("PhoneUI/Background").GetComponent<SpriteRenderer>();
                    if (Phone != null)
                    {
                        Phone.material?.SetColor(PlayerMaterial.BodyColor, color);
                        Phone.material?.SetColor(PlayerMaterial.BackColor, color - new Color(0.25f, 0.25f, 0.25f));
                    }
                    var PhoneButton = __instance.transform.Find("PhoneUI/UI_Phone_Button").GetComponent<SpriteRenderer>();
                    if (PhoneButton != null)
                    {
                        PhoneButton.material?.SetColor(PlayerMaterial.BodyColor, color);
                        PhoneButton.material?.SetColor(PlayerMaterial.BackColor, color - new Color(0.25f, 0.25f, 0.25f));
                    }

                    ControllerManager.Instance.OpenOverlayMenu(__instance.name, __instance.BackButton, __instance.DefaultButtonSelected, list2);
                    return false;
                }
            }
            [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.StartMeeting))]
            public static class StartMeeting
            {
                public static void Prefix(PlayerControl __instance)
                {
                    if (__instance == null) return;
                    try
                    {
                        singleton.Menu.Close();
                    }
                    catch { }
                }
            }
        }
    }
}
