using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Hazel;
using InnerNet;
using TheOtherRoles.Modules;
using TheOtherRoles.Objects;
using TheOtherRoles.Patches;
using TheOtherRoles.Utilities;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    [TORRPCHolder]
    [HarmonyPatch]
    public class Puppeteer : RoleBase<Puppeteer>
    {
        public static Color color = Palette.Purple;
        public static int counter = 0;
        public static int numKills { get { return (int)CustomOptionHolder.puppeteerNumKills.getFloat(); } }
        public static float sampleDuration { get { return CustomOptionHolder.puppeteerSampleDuration.getFloat(); } }
        public static bool canControlDummyEvenIfDead { get { return CustomOptionHolder.puppeteerCanControlDummyEvenIfDead.getBool(); } }
        public static int penaltyOnDeath { get { return (int)CustomOptionHolder.puppeteerPenaltyOnDeath.getFloat(); } }
        public static bool losesSenriganOnDeath { get { return CustomOptionHolder.puppeteerLosesSenriganOnDeath.getBool(); } }
        public static bool triggerPuppeteerWin = false;
        public static bool isActive = false;
        public static bool canSpawn = true;
        public static PlayerControl dummy = null;
        public static PlayerControl target = null;
        public static PlayerControl currentTarget = null;
        public static PlayerControl tmpTarget = null;
        public static bool stealthed = false;
        public static List<Arrow> arrows = new();
        public static float arrowUpdateInterval = 0.5f;
        public static float updateTimer = 0f;
        public static float posUpdateTimer = 0f;
        public static AudioClip laugh;
        public static bool soundFlag;
        public static float originalZoom = 0f;
        public static Vector3 originalScale = new();

        // RemoteProcess RPCs
        public static RemoteProcess<(byte playerId, float x, float y, float z)> SpawnDummyRPC = new("PuppeteerSpawnDummy", (message, _) =>
        {
            spawnDummyOnClient(message.playerId, new Vector3(message.x, message.y, message.z));
        });

        public static RemoteProcess<(float x, float y)> WalkDummy = new("PuppeteerWalkDummy", (message, _) =>
        {
            if (dummy == null) return;
            Vector2 direction = new(message.x, message.y);
            dummy.MyPhysics.body.velocity = Vector2.zero;
            if (PlayerControl.LocalPlayer.isRole(RoleId.Puppeteer))
            {
                PlayerControl.LocalPlayer.MyPhysics.body.velocity = Vector2.zero;
                KillAnimation.SetMovement(PlayerControl.LocalPlayer, false);
                KillAnimation.SetMovement(PlayerControl.LocalPlayer, true);
            }
            KillAnimation.SetMovement(dummy, false);
            KillAnimation.SetMovement(dummy, true);
            if (PlayerControl.LocalPlayer.isRole(RoleId.Puppeteer))
            {
                dummy.MyPhysics.body.interpolation = RigidbodyInterpolation2D.Interpolate;
                PlayerControl.LocalPlayer.MyPhysics.body.velocity = new Vector2(0, 0);
            }
            dummy.MyPhysics.body.velocity = direction * dummy.MyPhysics.TrueSpeed;
        });

        public static RemoteProcess<(float x, float y, float z, bool spawn)> MoveDummy = new("PuppeteerMoveDummy", (message, _) =>
        {
            if (dummy == null) return;
            Vector3 pos = new(message.x, message.y, message.z);
            if (SubmergedCompatibility.IsSubmerged && message.spawn)
            {
                bool toUpper = pos.y > -7;
                SubmergedCompatibility.ChangeFloor(toUpper);
            }
            dummy.transform.position = pos;
            dummy.NetTransform.Halt();
            dummy.Visible = true;
            dummy.moveable = true;
        });

        public static RemoteProcess<bool> Stealth = RemotePrimitiveProcess.OfBoolean("PuppeteerStealth", (message, _) =>
        {
            setStealthed(message);
        });

        public static RemoteProcess<byte> Morph = RemotePrimitiveProcess.OfByte("PuppeteerMorph", (message, _) =>
        {
            if (dummy != null)
            {
                var to = Helpers.playerById(message);
                if (to != null)
                    dummy.setLook(to.Data.PlayerName, to.Data.DefaultOutfit.ColorId, to.Data.DefaultOutfit.HatId,
                        to.Data.DefaultOutfit.VisorId, to.Data.DefaultOutfit.SkinId, to.Data.DefaultOutfit.PetId);
            }
        });

        public static RemoteProcess<byte> Win = RemotePrimitiveProcess.OfByte("PuppeteerWin", (message, _) =>
        {
            triggerPuppeteerWin = true;
        });

        public static RemoteProcess<(byte killer, byte target)> Kill = new("PuppeteerKill", (message, _) =>
        {
            var k = Helpers.playerById(message.killer);
            var t = Helpers.playerById(message.target);
            if (k != null && t != null)
            {
                KillAnimationCoPerformKillPatch.hideNextAnimation = true;
                k.MurderPlayer(t, MurderResultFlags.Succeeded);
            }
        });

        public static RemoteProcess<(byte dummyId, byte targetId)> ClimbLadder = new("PuppeteerClimbLadder", (message, _) =>
        {
            PlayerControl d = Helpers.playerById(message.dummyId);
            Ladder targetLadder = FastDestroyableSingleton<AirshipStatus>.Instance?.GetComponentsInChildren<Ladder>().ToArray().ToList().Find(x => x.Id == message.targetId);
            if (d != null && targetLadder != null)
                d.MyPhysics.ClimbLadder(targetLadder, (byte)(d.MyPhysics.lastClimbLadderSid + 1));
        });

        public static RemoteProcess<byte> UsePlatform = RemotePrimitiveProcess.OfByte("PuppeteerUsePlatform", (message, _) =>
        {
            PlayerControl d = Helpers.playerById(message);
            MovingPlatformBehaviour plat = FastDestroyableSingleton<AirshipStatus>.Instance?.GapPlatform;
            if (d != null && plat != null)
            {
                d.NetTransform.Halt();
                plat.Use(d);
            }
        });

        public Puppeteer()
        {
            RoleId = roleId = RoleId.Puppeteer;
        }

        public override void OnMeetingStart()
        {
            bool isAlive = allPlayers.FindAll(x => !x.Data.IsDead).Count >= 1;
            if (soundFlag && isAlive)
            {
                if (laugh != null)
                    SoundManager.Instance.PlaySound(laugh, false, 1f);
            }
            soundFlag = false;
            if (!isAlive && (PlayerControl.LocalPlayer.Data.Role.IsImpostor || PlayerControl.LocalPlayer.isRole(RoleId.Jackal)
                || PlayerControl.LocalPlayer.isRole(RoleId.JekyllAndHyde) || PlayerControl.LocalPlayer.isRole(RoleId.Moriarty)))
            {
                string msg = $"Puppeteer count: {counter}/{numKills}";
                if (AmongUsClient.Instance.AmClient && FastDestroyableSingleton<HudManager>.Instance)
                {
                    FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, msg);
                }
            }
        }

        public override void OnMeetingEnd(PlayerControl exiled = null)
        {
            target = null;
            canSpawn = false;
            isActive = false;
            if (PlayerControl.LocalPlayer.isRole(RoleId.Puppeteer))
            {
                switchStealth(false);
            }
        }

        public override void FixedUpdate()
        {
            if (PlayerControl.LocalPlayer.isRole(RoleId.Puppeteer))
            {
                currentTarget = PlayerControlFixedUpdatePatch.setTarget();
                PlayerControlFixedUpdatePatch.setPlayerOutline(currentTarget, color);
                arrowUpdate();
                syncDummyPos();
            }
        }

        public override void OnDeath(PlayerControl killer = null)
        {
            counter -= penaltyOnDeath;
            setOpacity(player, 1f);
        }

        public override void ResetRole(bool isShifted)
        {
            setOpacity(player, 1f);
        }

        static public IEnumerable<HelpSprite> GetHelpSprites()
        {
            // Sprites will be added once assets are available
            yield break;
        }

        public static void clearAndReload()
        {
            soundFlag = false;
            players = new List<Puppeteer>();
            if (dummy != null)
            {
                try { GameData.Instance.RemovePlayer(dummy.PlayerId); } catch { }
            }
            dummy = null;
            stealthed = false;
            isActive = false;
            canSpawn = false;
            triggerPuppeteerWin = false;
            target = null;
            counter = 0;
            foreach (Arrow arrow in arrows)
            {
                if (arrow != null && arrow.arrow != null)
                {
                    arrow.arrow.SetActive(false);
                    UnityEngine.Object.Destroy(arrow.arrow);
                }
            }
            arrows = new List<Arrow>();
            originalZoom = 0;
            KeyboardJoystickUpdatePatch.stop = false;
        }

        // ---- Dummy Creation (MCI-style) ----

        public static void spawnDummy()
        {
            if (dummy == null)
            {
                // Start MCI-style creation on the local client
                FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(SpawnDummyCoroutine().WrapToIl2Cpp());
            }
            else
            {
                // Move existing dummy to player position and make visible
                MoveDummy.Invoke((PlayerControl.LocalPlayer.transform.position.x,
                    PlayerControl.LocalPlayer.transform.position.y,
                    PlayerControl.LocalPlayer.transform.position.z, true));
            }

            // Morph to target
            if (target != null)
            {
                Morph.Invoke(target.PlayerId);
            }
            canSpawn = false;
            isActive = true;
        }

        private static IEnumerator SpawnDummyCoroutine()
        {
            // MCI-style: find available client ID
            int clientId = -1;
            for (var i = 1; i < 128; i++)
            {
                if (!AmongUsClient.Instance.allClients.ToArray().Any(x => x.Id == i)
                    && PlayerControl.LocalPlayer.OwnerId != i)
                {
                    clientId = i;
                    break;
                }
            }
            if (clientId == -1) yield break;

            var clientData = new ClientData(clientId, $"Puppet-{PlayerControl.LocalPlayer.PlayerId}", new()
            {
                Platform = Platforms.StandaloneWin10,
                PlatformName = "Bot"
            }, 1, "", "robotmodeactivate");

            AmongUsClient.Instance.GetOrCreateClient(clientData);
            yield return AmongUsClient.Instance.CreatePlayer(clientData);

            var playerControl = clientData.Character;

            // Pre-set outfit to avoid null reference errors during morph
            if (target != null)
            {
                playerControl.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId,
                    target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId,
                    target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId);
            }
            else
            {
                // Default look
                playerControl.SetColor(0);
                playerControl.SetHat(CosmeticsLayer.EMPTY_HAT_ID, 0);
                playerControl.SetVisor(CosmeticsLayer.EMPTY_VISOR_ID, 0);
                playerControl.SetSkin(CosmeticsLayer.EMPTY_SKIN_ID, 0);
                playerControl.SetPet(CosmeticsLayer.EMPTY_PET_ID, 0);
            }

            playerControl.NetTransform.enabled = true;
            playerControl.NetTransform.Halt();
            playerControl.Visible = false;
            playerControl.Data.RpcSetTasks(new byte[0]);
            playerControl.MyPhysics.ResetAnimState();
            playerControl.MyPhysics.ResetMoveState();

            dummy = playerControl;

            // Send spawn RPC to all clients for position sync
            SpawnDummyRPC.Invoke((playerControl.PlayerId,
                PlayerControl.LocalPlayer.transform.position.x,
                PlayerControl.LocalPlayer.transform.position.y,
                PlayerControl.LocalPlayer.transform.position.z));
        }

        // Fallback: called on remote clients via RPC when MCI-style creation isn't possible
        public static void spawnDummyOnClient(byte playerId, Vector3 pos)
        {
            if (dummy != null) return;

            // Try to find existing player created by host's CreatePlayer propagation
            var existingPlayer = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(x => x.PlayerId == playerId);
            if (existingPlayer != null && existingPlayer != PlayerControl.LocalPlayer)
            {
                dummy = existingPlayer;
                dummy.NetTransform.enabled = true;
                dummy.NetTransform.Halt();
                dummy.Visible = false;
                dummy.Data.RpcSetTasks(new byte[0]);
                dummy.transform.position = pos;
                return;
            }

            // Fallback: create dummy via Instantiate (original GM Haoming approach, without AddDummy)
            var playerControl = UnityEngine.Object.Instantiate(AmongUsClient.Instance.PlayerPrefab);
            playerControl.PlayerId = playerId;
            dummy = playerControl;

            // Try adding to GameData without AddDummy
            try
            {
                GameData.Instance.AddDummy(playerControl);
            }
            catch { }

            AmongUsClient.Instance.Spawn(playerControl, -2, InnerNet.SpawnFlags.IsClientCharacter);

            playerControl.transform.position = pos;
            playerControl.GetComponent<DummyBehaviour>().enabled = false;
            playerControl.NetTransform.enabled = true;
            playerControl.NetTransform.Halt();
            playerControl.Visible = false;
            playerControl.Data.RpcSetTasks(new byte[0]);
        }

        // ---- Stealth / Camera Switching ----

        public static void switchStealth(bool flag)
        {
            if (!flag)
            {
                Stealth.Invoke(false);
                var hudManager = FastDestroyableSingleton<HudManager>.Instance;
                hudManager.PlayerCam.Target = PlayerControl.LocalPlayer;
                senrigan(false);
                var player = PlayerControl.LocalPlayer;
                player.lightSource = UnityEngine.Object.Instantiate<LightSource>(player.LightPrefab);
                player.lightSource.transform.SetParent(player.transform);
                player.lightSource.transform.localPosition = player.Collider.offset;
                PlayerControl.LocalPlayer.moveable = true;
            }
            else
            {
                // Move dummy to player position first
                MoveDummy.Invoke((PlayerControl.LocalPlayer.transform.position.x,
                    PlayerControl.LocalPlayer.transform.position.y,
                    PlayerControl.LocalPlayer.transform.position.z, true));

                Stealth.Invoke(true);

                var hudManager = FastDestroyableSingleton<HudManager>.Instance;
                var d = dummy;
                if (d == null) return;

                // GM Haoming approach: just change camera target, don't reassign LocalPlayer
                hudManager.PlayerCam.Target = d;

                // Apply senrigan zoom
                if (losesSenriganOnDeath)
                {
                    bool isAlive = allPlayers.FindAll(x => !x.Data.IsDead).Count >= 1;
                    senrigan(isAlive);
                }
                else
                {
                    senrigan(true);
                }

                // Give dummy its own light source
                d.lightSource = UnityEngine.Object.Instantiate<LightSource>(d.LightPrefab);
                d.lightSource.transform.SetParent(d.transform);
                d.lightSource.transform.localPosition = d.Collider.offset;

                // Freeze puppeteer, make dummy moveable for WASD control
                PlayerControl.LocalPlayer.NetTransform.Halt();
                PlayerControl.LocalPlayer.moveable = false;

                // Make dummy visible and moveable
                d.Visible = true;
                d.moveable = true;
            }
        }

        public static void setStealthed(bool s = true)
        {
            stealthed = s;
            if (stealthed)
            {
                KeyboardJoystickUpdatePatch.up = false;
                KeyboardJoystickUpdatePatch.down = false;
                KeyboardJoystickUpdatePatch.left = false;
                KeyboardJoystickUpdatePatch.right = false;
            }
        }

        // ---- Senrigan (Camera Zoom) ----

        public static void senrigan(bool toggle)
        {
            var hm = FastDestroyableSingleton<HudManager>.Instance;
            if (originalZoom == 0) originalZoom = Camera.main.orthographicSize;
            if (originalScale == new Vector3()) originalScale = hm.transform.localScale;
            if (!toggle)
            {
                Camera.main.orthographicSize = originalZoom;
                hm.UICamera.orthographicSize = originalZoom;
                hm.transform.localScale = originalScale;

                if (!PlayerControl.LocalPlayer.Data.IsDead)
                {
                    hm.ShadowQuad.gameObject.SetActive(true);
                }
            }
            else
            {
                Camera.main.orthographicSize = originalZoom * 3;
                hm.UICamera.orthographicSize = originalZoom * 3;
                hm.transform.localScale = originalScale * 3;
                hm.ShadowQuad.gameObject.SetActive(false);
            }
        }

        // ---- Targeting / Win Condition ----

        public static void OnTargetExiled()
        {
            bool isAlive = allPlayers.FindAll(x => !x.Data.IsDead).Count >= 1;
            if (target != null && !target.Data.Role.IsImpostor && !target.isRole(RoleId.Jackal)
                && !target.isRole(RoleId.JekyllAndHyde) && !target.isRole(RoleId.Moriarty) && isAlive)
            {
                counter += 1;
            }
            if (counter >= numKills && PlayerControl.LocalPlayer.isRole(RoleId.Puppeteer))
            {
                Win.Invoke(0);
            }
        }

        public static void OnDummyDeath(PlayerControl killer)
        {
            // Crew killing dummy kills the puppeteer, so don't count
            if (!Helpers.isCrew(killer))
                counter += 1;
            soundFlag = true;

            bool isAlive = allPlayers.FindAll(x => !x.Data.IsDead).Count >= 1;
            if (!isAlive)
            {
                killer.SetKillTimer(0f);
            }

            if (!PlayerControl.LocalPlayer.isRole(RoleId.Puppeteer)) return;

            // Check win condition
            if (counter >= numKills)
            {
                Win.Invoke(0);
            }

            // Linked kill: dummy death kills target (if not crew killer)
            if (target != null && !target.Data.IsDead && isAlive && !Helpers.isCrew(killer))
            {
                Kill.Invoke((killer.PlayerId, target.PlayerId));
            }
            else if (isAlive && Helpers.isCrew(killer)) // Crew kills dummy -> puppeteer dies
            {
                Kill.Invoke((killer.PlayerId, PlayerControl.LocalPlayer.PlayerId));
            }

            isActive = false;
            canSpawn = false;
            target = null;
            switchStealth(false);
        }

        // ---- Arrow System ----

        static void arrowUpdate()
        {
            updateTimer -= Time.fixedDeltaTime;

            if (updateTimer <= 0.0f)
            {
                foreach (Arrow arrow in arrows)
                {
                    if (arrow != null)
                    {
                        arrow.arrow.SetActive(false);
                        UnityEngine.Object.Destroy(arrow.arrow);
                    }
                }

                arrows = new List<Arrow>();

                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (p.Data.IsDead || !p.Data.Role) continue;
                    Arrow arrow;
                    if (p.Data.Role.IsImpostor || p.isRole(RoleId.Jackal) || p.isRole(RoleId.JekyllAndHyde)
                        || p.isRole(RoleId.Moriarty) || p == target)
                    {
                        if (p.Data.Role.IsImpostor)
                            arrow = new Arrow(Color.red);
                        else if (p.isRole(RoleId.Jackal) || (p.isRole(RoleId.SchrodingersCat) && SchrodingersCat.team == SchrodingersCat.Team.Jackal))
                            arrow = new Arrow(Jackal.color);
                        else if (p.isRole(RoleId.JekyllAndHyde))
                            arrow = new Arrow(JekyllAndHyde.color);
                        else if (p.isRole(RoleId.Moriarty))
                            arrow = new Arrow(Moriarty.color);
                        else if (p == target)
                            arrow = new Arrow(color);
                        else
                            arrow = new Arrow(Color.black);

                        arrow.arrow.SetActive(true);
                        arrow.Update(p.transform.position);
                        arrows.Add(arrow);
                    }
                }

                updateTimer = arrowUpdateInterval;
            }
        }

        // ---- Position Sync ----

        static void syncDummyPos()
        {
            posUpdateTimer -= Time.fixedDeltaTime;

            if (posUpdateTimer <= 0.0f)
            {
                if (dummy != null)
                {
                    MoveDummy.Invoke((dummy.transform.position.x, dummy.transform.position.y,
                        dummy.transform.position.z, false));
                }
                posUpdateTimer = 1f;
            }
        }

        // ---- Opacity / Visibility ----

        public static void setOpacity(PlayerControl player, float opacity)
        {
            Color c = Color.Lerp(Palette.ClearWhite, Palette.White, opacity);
            try
            {
                Helpers.setInvisible(player, c, opacity);
            }
            catch { }
        }

        // ====== Harmony Patches ======

        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
        public static class PlayerPhysicsPatch
        {
            public static void Postfix(PlayerPhysics __instance)
            {
                if (isRole(__instance.myPlayer))
                {
                    var puppeteer = __instance.myPlayer;
                    if (puppeteer == null || puppeteer.Data.IsDead) return;

                    bool canSee = PlayerControl.LocalPlayer.isRole(RoleId.Puppeteer) || PlayerControl.LocalPlayer.Data.IsDead;
                    var opacity = canSee ? 0.1f : 0.0f;

                    if (stealthed)
                    {
                        puppeteer.cosmetics?.currentBodySprite?.BodySprite.material.SetFloat("_Outline", 0f);
                    }
                    else
                    {
                        opacity = 1.0f;
                    }

                    setOpacity(puppeteer, opacity);
                }
                else if (__instance.myPlayer == dummy)
                {
                    var d = __instance.myPlayer;
                    if (d == null || d.Data.IsDead) return;

                    bool canSee = PlayerControl.LocalPlayer.isRole(RoleId.Puppeteer) || PlayerControl.LocalPlayer.Data.IsDead;
                    var opacity = canSee ? 0.1f : 0.0f;

                    if (!stealthed)
                    {
                        d.cosmetics?.currentBodySprite?.BodySprite.material.SetFloat("_Outline", 0f);
                    }
                    else
                    {
                        opacity = 1.0f;
                    }
                    setOpacity(d, opacity);
                }
            }
        }

        [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
        class ExileControllerBeginPatch
        {
            public static void Prefix(ExileController __instance, [HarmonyArgument(0)] ref NetworkedPlayerInfo exiled)
            {
                if (exiled != null && exiled.Object == target)
                {
                    OnTargetExiled();
                }
            }
        }

        [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
        public static class KeyboardJoystickUpdatePatch
        {
            public static bool up = false;
            public static bool down = false;
            public static bool right = false;
            public static bool left = false;
            public static bool stop = false;

            private static IEnumerator DontMove(float n)
            {
                stop = true;
                yield return new WaitForSeconds(n);
                stop = false;
                yield break;
            }

            public static void Postfix(KeyboardJoystick __instance)
            {
                if (PlayerControl.LocalPlayer == null) return;
                if (!PlayerControl.LocalPlayer.isRole(RoleId.Puppeteer)) return;

                if (stealthed)
                {
                    if (stop) return;

                    // Door / ladder interaction
                    if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
                    {
                        PlainDoor[] doors;
                        if (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 4)
                            doors = FastDestroyableSingleton<AirshipStatus>.Instance.GetComponentsInChildren<PlainDoor>();
                        else if (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 2)
                            doors = FastDestroyableSingleton<PolusShipStatus>.Instance.GetComponentsInChildren<PlainDoor>();
                        else if (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 1)
                            doors = FastDestroyableSingleton<MiraShipStatus>.Instance.GetComponentsInChildren<PlainDoor>();
                        else if (SubmergedCompatibility.IsSubmerged)
                            doors = UnityEngine.GameObject.FindObjectsOfType<PlainDoor>();
                        else
                            doors = FastDestroyableSingleton<SkeldShipStatus>.Instance.GetComponentsInChildren<PlainDoor>();

                        PlainDoor t = null;
                        float minDistance = 9999;
                        foreach (var door in doors)
                        {
                            float distance = Vector2.Distance(door.transform.position, dummy.transform.position);
                            if (distance < 1.5f && distance < minDistance)
                            {
                                t = door;
                                minDistance = distance;
                            }
                        }
                        if (t != null)
                        {
                            var deconSystem = t.transform.parent.gameObject.GetComponent<DeconSystem>();
                            if (deconSystem != null)
                            {
                                bool flag = true;
                                if (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 2)
                                    flag = t.name.Contains("Inner");
                                else if (SubmergedCompatibility.IsSubmerged)
                                    flag = t.name.Contains("Upper");
                                var consoles = t.GetComponentsInChildren<DeconControl>();
                                DeconControl inner = null;
                                DeconControl outer = null;
                                foreach (var console in consoles)
                                {
                                    if (console.name == "InnerConsole") inner = console;
                                    if (console.name == "OuterConsole") outer = console;
                                }
                                float distOuter = Vector2.Distance(outer.transform.position, dummy.transform.position);
                                float distInner = Vector2.Distance(inner.transform.position, dummy.transform.position);
                                if (distInner < distOuter)
                                    deconSystem.OpenFromInside(flag);
                                else
                                    deconSystem.OpenDoor(flag);
                            }
                            else
                            {
                                FastDestroyableSingleton<ShipStatus>.Instance.RpcUpdateSystem(SystemTypes.Doors, (byte)(t.Id | 64));
                                t.SetDoorway(true);
                            }
                        }

                        // Airship ladders
                        if (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 4)
                        {
                            Ladder[] ladders = FastDestroyableSingleton<AirshipStatus>.Instance.GetComponentsInChildren<Ladder>();
                            Ladder targetLadder = null;
                            foreach (var ladder in ladders)
                            {
                                float distance = Vector2.Distance(ladder.transform.position, dummy.transform.position);
                                if (distance < 0.5f)
                                {
                                    targetLadder = ladder;
                                    break;
                                }
                            }
                            if (targetLadder != null)
                            {
                                ClimbLadder.Invoke((dummy.PlayerId, targetLadder.Id));
                                return;
                            }

                            // Airship platforms
                            AirshipStatus shipstatus = FastDestroyableSingleton<AirshipStatus>.Instance;
                            if (shipstatus != null)
                            {
                                var cons = shipstatus.GetComponentsInChildren<PlatformConsole>().ToList();
                                PlatformConsole leftPlatform = cons.Find(x => x.name == "PlatformLeft");
                                PlatformConsole rightPlatform = cons.Find(x => x.name == "PlatformRight");
                                float distanceRight = Vector2.Distance(leftPlatform.transform.position, dummy.transform.position);
                                float distanceLeft = Vector2.Distance(rightPlatform.transform.position, dummy.transform.position);
                                if (distanceRight < 0.8f || distanceLeft < 0.8f)
                                {
                                    UsePlatform.Invoke(dummy.PlayerId);
                                    FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(DontMove(1).WrapToIl2Cpp());
                                    FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(1f, new Action<float>(t2 =>
                                    {
                                        if (t2 >= 1.0f)
                                        {
                                            UsePlatform.Invoke(dummy.PlayerId);
                                        }
                                    })));
                                    return;
                                }
                            }
                        }

                        // Submerged elevators
                        if (SubmergedCompatibility.IsSubmerged)
                        {
                            try
                            {
                                var submarineElevatorType = Type.GetType("Submerged.Systems.Elevator.SubmarineElevator, Submerged");
                                if (submarineElevatorType != null)
                                {
                                    var findMethod = typeof(UnityEngine.Object).GetMethod("FindObjectsOfType", System.Type.EmptyTypes);
                                    var genericFind = findMethod.MakeGenericMethod(submarineElevatorType);
                                    var elevators = genericFind.Invoke(null, null) as IEnumerable;
                                    object elevator = null;
                                    minDistance = 9999;
                                    foreach (var e in elevators)
                                    {
                                        var pos = (e as MonoBehaviour).transform.position;
                                        var lowerInnerDoorInfo = submarineElevatorType.GetField("LowerInnerDoor");
                                        var upperInnerDoorInfo = submarineElevatorType.GetField("UpperInnerDoor");
                                        var lowerInnerDoor = lowerInnerDoorInfo.GetValue(e) as PlainDoor;
                                        var upperInnerDoor = upperInnerDoorInfo.GetValue(e) as PlainDoor;
                                        if (lowerInnerDoor == null || upperInnerDoor == null) continue;
                                        float lowerDistance = Vector2.Distance(dummy.transform.position, lowerInnerDoor.transform.position);
                                        float upperDistance = Vector2.Distance(dummy.transform.position, upperInnerDoor.transform.position);
                                        float distance = lowerDistance < upperDistance ? lowerDistance : upperDistance;
                                        if (distance < 1.5 && distance < minDistance)
                                        {
                                            minDistance = distance;
                                            elevator = e;
                                        }
                                    }
                                    if (elevator != null)
                                    {
                                        var use = submarineElevatorType.GetMethod("Use");
                                        use.Invoke(elevator, new object[0]);
                                    }
                                }
                            }
                            catch { }
                        }
                    }

                    // WASD movement for dummy
                    if (Input.GetKeyDown(KeyCode.D)) right = true;
                    if (Input.GetKeyUp(KeyCode.D)) right = false;
                    if (Input.GetKeyDown(KeyCode.A)) left = true;
                    if (Input.GetKeyUp(KeyCode.A)) left = false;
                    if (Input.GetKeyDown(KeyCode.W)) up = true;
                    if (Input.GetKeyUp(KeyCode.W)) up = false;
                    if (Input.GetKeyDown(KeyCode.S)) down = true;
                    if (Input.GetKeyUp(KeyCode.S)) down = false;

                    if (dummy != null && !MeetingHud.Instance)
                    {
                        Vector2 pos = dummy.transform.position;
                        Vector2 offset = Vector2.zero;
                        if (up) offset += new Vector2(0f, 0.5f);
                        if (down) offset += new Vector2(0f, -0.5f);
                        if (left) offset += new Vector2(-0.5f, 0.0f);
                        if (right) offset += new Vector2(0.5f, 0.0f);

                        if (offset != Vector2.zero)
                        {
                            WalkDummy.Invoke((offset.x, offset.y));
                        }
                        if (!(up || down || right || left) && dummy.NetTransform.lastPosition != pos)
                        {
                            MoveDummy.Invoke((dummy.transform.position.x, dummy.transform.position.y,
                                dummy.transform.position.z, false));
                        }
                    }
                }
            }
        }
    }
}
