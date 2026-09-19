using System;
using UnityEngine;
using System.Collections.Generic;
using TheOtherRoles.Utilities;
using static TheOtherRoles.TheOtherRoles;
using System.Linq;
using TheOtherRoles.Roles;

namespace TheOtherRoles.Objects {

    public class Portal {
        public static Portal firstPortal = null;
        public static Portal secondPortal = null;
        public static bool bothPlacedAndEnabled = false;
        public static Sprite[] portalFgAnimationSprites = new Sprite[205];
        public static Sprite portalSprite;
        public static bool isTeleporting = false;
        public static float teleportDuration = 3.4166666667f;
        public string room;

        public struct tpLogEntry {
            public byte playerId;
            public string name;
            public DateTime time;
            public string startingRoom;
            public string endingRoom;
            public tpLogEntry(byte playerId, string name, DateTime time, string startingRoom, string endingRoom) {
                this.playerId = playerId;
                this.time = time;
                this.name = name;
                this.startingRoom = startingRoom;
                this.endingRoom = endingRoom;
            }
        }

        public static List<tpLogEntry> teleportedPlayers;

        public static Sprite getFgAnimationSprite(int index) {
            if (portalFgAnimationSprites == null || portalFgAnimationSprites.Length == 0) return null;
            index = Mathf.Clamp(index, 0, portalFgAnimationSprites.Length - 1);
            if (portalFgAnimationSprites[index] == null)
                portalFgAnimationSprites[index] = (Helpers.loadSpriteFromResources($"TheOtherRoles.Resources.PortalAnimation.portal_{(index):000}.png", 115f));
            return portalFgAnimationSprites[index];
        }

        public static void startTeleport(byte playerId, byte exit) {
            if (firstPortal == null || secondPortal == null) return;
            isTeleporting = true;
            
            // Generate log info
            PlayerControl playerControl = Helpers.playerById(playerId);
            bool flip = playerControl.cosmetics.currentBodySprite.BodySprite.flipX; // use the original player control here, not the morhpTarget.
            firstPortal.animationFgRenderer.flipX = flip;
            secondPortal.animationFgRenderer.flipX = flip;
            foreach (var morphling in Morphling.players) {
                if (morphling.morphTimer > 0 && morphling.player == playerControl) playerControl = morphling.morphTarget;
            }
            if (MimicA.isMorph && playerControl.isRole(RoleId.MimicA)) playerControl = MimicK.allPlayers.FirstOrDefault();
            else if (MimicK.victim != null && playerControl.isRole(RoleId.MimicK)) playerControl = MimicK.victim;

            string playerNameDisplay = Portalmaker.logOnlyHasColors ? ModTranslation.getString("portalmakerLogPlayer") + " (" + (Helpers.isLighterColor(playerControl.Data.DefaultOutfit.ColorId) ? ModTranslation.getString("detectiveLightLabel") : ModTranslation.getString("detectiveDarkLabel")) + ")" : playerControl.Data.PlayerName;

            int colorId = playerControl.Data.DefaultOutfit.ColorId;

            if (Camouflager.camouflageTimer > 0 || Helpers.MushroomSabotageActive()) {
                playerNameDisplay = ModTranslation.getString("portalmakerLogCamouflagedPlayer");
                colorId = 6;
            }
            
            if (!playerControl.Data.IsDead) {
                teleportedPlayers.Add(new tpLogEntry(playerId, playerNameDisplay, DateTime.UtcNow, Helpers.GetRoomName(playerControl),
                    exit == 2 ? secondPortal.room : findExit(playerControl.transform.position).room));
            }
            
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(teleportDuration, new Action<float>((p) => {
                if (firstPortal != null && firstPortal.animationFgRenderer != null && secondPortal != null && secondPortal.animationFgRenderer != null) {
                    if (exit == 0 || exit == 1) firstPortal.animationFgRenderer.sprite = getFgAnimationSprite((int)(p * portalFgAnimationSprites.Length));
                    if (exit == 0 || exit == 2) secondPortal.animationFgRenderer.sprite = getFgAnimationSprite((int)(p * portalFgAnimationSprites.Length));
                    playerControl.SetPlayerMaterialColors(firstPortal.animationFgRenderer);
                    playerControl.SetPlayerMaterialColors(secondPortal.animationFgRenderer);
                    if (p == 1f) {
                        firstPortal.animationFgRenderer.sprite = null;
                        secondPortal.animationFgRenderer.sprite = null;
                        isTeleporting = false;
                    }
                }
            })));
        }

        public GameObject portalFgAnimationGameObject;
        public GameObject portalGameObject;
        private SpriteRenderer animationFgRenderer;
        private SpriteRenderer portalRenderer;

        public Portal(Vector2 p) 
        {
            portalGameObject = new GameObject("Portal"){ layer = 11 };
            //Vector3 position = new Vector3(p.x, p.y, PlayerControl.LocalPlayer.transform.position.z + 1f);
            Vector3 position = new(p.x, p.y, p.y / 1000f + 0.01f);

            // Create the portal            
            portalGameObject.transform.position = position;
            portalGameObject.AddSubmergedComponent(SubmergedCompatibility.Classes.ElevatorMover);
            portalRenderer = portalGameObject.AddComponent<SpriteRenderer>();
            portalRenderer.sprite = portalSprite;

            Vector3 fgPosition = new(0, 0, -1f);
            portalFgAnimationGameObject = new GameObject("PortalAnimationFG");
            portalFgAnimationGameObject.transform.SetParent(portalGameObject.transform);
            portalFgAnimationGameObject.AddSubmergedComponent(SubmergedCompatibility.Classes.ElevatorMover);
            portalFgAnimationGameObject.transform.localPosition = fgPosition;
            animationFgRenderer = portalFgAnimationGameObject.AddComponent<SpriteRenderer>();
            animationFgRenderer.material = FastDestroyableSingleton<HatManager>.Instance.PlayerMaterial;

            // Only render the inactive portals for the Portalmaker
            bool playerIsPortalmaker = PlayerControl.LocalPlayer.isRole(RoleId.Portalmaker);
            portalGameObject.SetActive(playerIsPortalmaker);
            portalFgAnimationGameObject.SetActive(true);

            if (firstPortal == null) firstPortal = this;
            else if (secondPortal == null) {
                secondPortal = this;
            }

            var hudManager = HudManager.Instance;
            var lastRoom = hudManager && hudManager.roomTracker && hudManager.roomTracker.LastRoom
                ? HudManager.Instance.roomTracker.LastRoom.RoomId
                : SystemTypes.Outside;
            this.room = DestroyableSingleton<TranslationController>.Instance.GetString(lastRoom);
        }

        public static bool locationNearEntry(Vector2 p) {
            if (!bothPlacedAndEnabled) return false;
            float maxDist = 0.25f;

            var dist1 = Vector2.Distance(p, firstPortal.portalGameObject.transform.position);
            var dist2 = Vector2.Distance(p, secondPortal.portalGameObject.transform.position);
            if (dist1 > maxDist && dist2 > maxDist) return false;
            return true;
        }

        public static Portal findExit(Vector2 p) {
            var dist1 = Vector2.Distance(p, firstPortal.portalGameObject.transform.position);
            var dist2 = Vector2.Distance(p, secondPortal.portalGameObject.transform.position);
            return dist1 < dist2 ? secondPortal : firstPortal;
        }

        public static Vector2 findEntry(Vector2 p) {
            var dist1 = Vector2.Distance(p, firstPortal.portalGameObject.transform.position);
            var dist2 = Vector2.Distance(p, secondPortal.portalGameObject.transform.position);
            return dist1 > dist2 ? secondPortal.portalGameObject.transform.position : firstPortal.portalGameObject.transform.position;
        }

        public static void meetingEndsUpdate() {
            // checkAndEnable
            if (secondPortal != null) {
                firstPortal.portalGameObject.SetActive(true);
                secondPortal.portalGameObject.SetActive(true);
                bothPlacedAndEnabled = true;
                HudManagerStartPatch.portalmakerButtonText2.text = "2. " + secondPortal.room;
                if (PlayerControl.LocalPlayer.isRole(RoleId.Portalmaker))
                    _ = new Modules.StaticAchievementToken("portalmaker.common1");
            }

            // reset teleported players
            teleportedPlayers = new List<tpLogEntry>();

            if (PlayerControl.LocalPlayer.isRole(RoleId.Portalmaker))
            {
                if (Portalmaker.local.acTokenChallenge != null)
                    Portalmaker.local.acTokenChallenge.Value.portal = 0;
            }
        }

        private static void preloadSprites() {
            for (int i = 0; i < portalFgAnimationSprites.Length; i++) {
                getFgAnimationSprite(i);
            }
            portalSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.PortalAnimation.plattform.png", 115f);
        }

        public static void clearPortals() {
            preloadSprites();  // Force preload of sprites to avoid lag
            bothPlacedAndEnabled = false;
            firstPortal = null;
            secondPortal = null;
            isTeleporting = false;
            teleportedPlayers = new List<tpLogEntry>();
        }

    }

}
