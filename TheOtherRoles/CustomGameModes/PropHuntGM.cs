using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Hazel;
using TheOtherRoles.Objects;
using TheOtherRoles.Patches;
using TMPro;
using UnityEngine;

namespace TheOtherRoles.CustomGameModes
{
    public static class PropHuntGM
    {
        public static bool isPropHuntGM = false;
        public static TMP_Text timerText = null;
        public static DateTime startTime = DateTime.UtcNow;

        public static float timer = 300f;
        public static bool timerRunning = false;
        public static float blackOutTimer = 0f;

        public static int numberOfHunters = 1;
        public static float initialBlackoutTime = 10f;
        public static float killCooldownHit = 10f;
        public static float killCooldownMiss = 10f;
        public static float hunterVision = 0.5f;
        public static float propVision = 2f;
        public static bool propBecomesHunterWhenFound = false;

        public static Dictionary<byte, Tuple<string, float>> currentObject = new();
        public static Dictionary<byte, float> isCurrentlyRevealed = new();
        public static Dictionary<byte, GameObject> revealRenderer = new();
        public static Dictionary<byte, float> invisPlayers = new();
        public static Dictionary<byte, float> speedboostActive = new();

        public static GameObject currentTarget;
        private static GameObject poolablesBackground;
        private static Sprite poolablesBackgroundSprite;
        private static List<GameObject> duplicatedCollider = new();

        public static List<string> whitelistedObjects = new()
        {
            "liprop", "snowman", "onsole", "Table", "barrier", "bridge", "bigRock",
            "BushesBottom", "EmergencyButton", "telescope", "Debris", "Mushroom",
            "box", "crate", "barrel", "chair", "bench", "plant", "flower", "rock",
            "pillar", "column", "statue", "fountain", "lamp", "sign", "poster",
            "panel", "screen", "monitor", "computer", "terminal", "pipe", "vent",
            "canister", "tank", "cargo", "luggage", "bag", "satchel", "case",
            "button", "switch", "lever", "wheel", "ladder", "railing", "beam"
        };

        public static bool isHunter()
        {
            return isPropHuntGM && PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data.Role.IsImpostor;
        }

        public static bool isProp()
        {
            return isPropHuntGM && PlayerControl.LocalPlayer != null && !PlayerControl.LocalPlayer.Data.Role.IsImpostor;
        }

        public static List<PlayerControl> getHunters()
        {
            List<PlayerControl> hunters = new(PlayerControl.AllPlayerControls.ToArray());
            hunters.RemoveAll(x => !x.Data.Role.IsImpostor);
            return hunters;
        }

        public static void clearAndReload()
        {
            isPropHuntGM = TORMapOptions.gameMode == CustomGamemodes.PropHunt;
            if (timerText != null) UnityEngine.Object.Destroy(timerText);
            timerText = null;
            if (poolablesBackground != null) UnityEngine.Object.Destroy(poolablesBackground);
            poolablesBackground = null;
            foreach (var go in revealRenderer.Values) if (go != null) UnityEngine.Object.Destroy(go);
            revealRenderer = new Dictionary<byte, GameObject>();
            foreach (var go in duplicatedCollider) if (go != null) UnityEngine.Object.Destroy(go);
            duplicatedCollider = new List<GameObject>();

            numberOfHunters = Mathf.RoundToInt(CustomOptionHolder.propHuntNumberOfHunters.getFloat());
            initialBlackoutTime = CustomOptionHolder.hunterInitialBlackoutTime.getFloat();
            propBecomesHunterWhenFound = CustomOptionHolder.propBecomesHunterWhenFound.getBool();
            killCooldownMiss = CustomOptionHolder.hunterMissCooldown.getFloat();
            killCooldownHit = CustomOptionHolder.hunterHitCooldown.getFloat();
            hunterVision = CustomOptionHolder.propHunterVision.getFloat();
            propVision = CustomOptionHolder.propVision.getFloat();
            timer = CustomOptionHolder.propHuntTimer.getFloat() * 60;
            timerRunning = false;
            blackOutTimer = 0f;
            startTime = DateTime.UtcNow;
            currentObject = new Dictionary<byte, Tuple<string, float>>();
            isCurrentlyRevealed = new Dictionary<byte, float>();
            speedboostActive = new Dictionary<byte, float>();
            invisPlayers = new Dictionary<byte, float>();
            currentTarget = null;

            HunterPH.clearAndReload();
            PropPH.clearAndReload();
        }

        // ---- HUD and Update Logic ----

        public static void propTargetAndTimerDisplayUpdate()
        {
            if (!PlayerControl.LocalPlayer.Data.Role.IsImpostor)
                currentTarget = FindClosestDisguiseObject(PlayerControl.LocalPlayer.gameObject, 1f);

            if (timerText == null)
            {
                RoomTracker roomTracker = HudManager.Instance?.roomTracker;
                if (roomTracker != null)
                {
                    GameObject gameObject = UnityEngine.Object.Instantiate(roomTracker.gameObject);
                    gameObject.transform.SetParent(HudManager.Instance.transform);
                    UnityEngine.Object.DestroyImmediate(gameObject.GetComponent<RoomTracker>());
                    timerText = gameObject.GetComponent<TMP_Text>();
                    gameObject.transform.localPosition = new Vector3(0, -1.8f, gameObject.transform.localPosition.z);
                    if (AmongUs.Data.DataManager.Settings.Gameplay.StreamerMode)
                        gameObject.transform.localPosition = new Vector3(0, 2f, gameObject.transform.localPosition.z);
                }
            }
            else
            {
                if (timerRunning || blackOutTimer > 0f)
                {
                    float relevantTimer = timerRunning ? timer : blackOutTimer;
                    int minutes = (int)relevantTimer / 60;
                    int seconds = (int)relevantTimer % 60;
                    string suffix = $" {minutes:00}:{seconds:00}";
                    timerText.text = Helpers.cs(timerRunning ? Color.blue : Color.red, suffix);
                    timerText.outlineColor = Color.white;
                    timerText.outlineWidth = 0.1f;
                    timerText.color = timerRunning ? Color.blue : Color.red;
                }
            }

            if (HudManagerStartPatch.propDisguiseButton != null &&
                HudManagerStartPatch.propDisguiseButton.Timer > HudManagerStartPatch.propDisguiseButton.MaxTimer)
                HudManagerStartPatch.propDisguiseButton.Timer = HudManagerStartPatch.propDisguiseButton.MaxTimer;
        }

        public static void poolablePlayerUpdate()
        {
            if (poolablesBackground == null)
            {
                poolablesBackground = new GameObject("poolablesBackground");
                poolablesBackground.AddComponent<SpriteRenderer>();
                poolablesBackground.layer = LayerMask.NameToLayer("UI");
                if (poolablesBackgroundSprite == null)
                    poolablesBackgroundSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.poolablesBackground.jpg", 200f);
            }

            poolablesBackground.transform.SetParent(HudManager.Instance.transform);
            poolablesBackground.transform.localPosition = IntroCutsceneOnDestroyPatch.bottomLeft +
                                                          new Vector3(-1.45f, -0.05f, 0) + Vector3.right *
                                                          PlayerControl.AllPlayerControls.Count * 0.2f;
            float backgroundSizeX = PlayerControl.AllPlayerControls.Count * 0.4f + 0.2f;
            poolablesBackground.GetComponent<SpriteRenderer>().sprite = poolablesBackgroundSprite;
            if (poolablesBackground.GetComponent<SpriteRenderer>().sprite != null)
            {
                poolablesBackground.transform.localScale = new Vector3(
                    poolablesBackground.transform.localScale.x * backgroundSizeX /
                    poolablesBackground.GetComponent<SpriteRenderer>().bounds.size.x,
                    poolablesBackground.transform.localScale.y, poolablesBackground.transform.localScale.z);
            }

            foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
            {
                if (!TORMapOptions.playerIcons.ContainsKey(pc.PlayerId)) continue;
                PoolablePlayer poolablePlayer = TORMapOptions.playerIcons[pc.PlayerId];
                if (pc.Data.IsDead)
                {
                    poolablePlayer.setSemiTransparent(true);
                    poolablePlayer.cosmetics.nameText.text = Helpers.cs(Palette.DisabledGrey, pc.Data.PlayerName);
                }
                else if (pc.Data.Role.IsImpostor)
                {
                    poolablePlayer.cosmetics.nameText.text = Helpers.cs(Palette.ImpostorRed, pc.Data.PlayerName);
                    poolablePlayer.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", 2f);
                    poolablePlayer.cosmetics.currentBodySprite.BodySprite.material.SetColor("_OutlineColor", Palette.ImpostorRed);
                }
                else
                {
                    poolablePlayer.cosmetics.nameText.text = Helpers.cs(Palette.CrewmateBlue, pc.Data.PlayerName);
                }

                // update currently revealed
                if (isCurrentlyRevealed.ContainsKey(pc.PlayerId))
                {
                    if (!revealRenderer.ContainsKey(pc.PlayerId))
                    {
                        GameObject go = new GameObject($"reveal_renderer_{pc.PlayerId}");
                        go.layer = LayerMask.NameToLayer("UI");
                        go.AddComponent<SpriteRenderer>();
                        go.transform.SetParent(poolablePlayer.transform.parent, false);
                        go.SetActive(true);
                        go.transform.localPosition = poolablePlayer.transform.localPosition + new Vector3(0, 0, -50f);
                        poolablePlayer.gameObject.SetActive(false);
                        revealRenderer.Add(pc.PlayerId, go);
                    }

                    float revealTimer = isCurrentlyRevealed[pc.PlayerId] - Time.deltaTime;
                    isCurrentlyRevealed[pc.PlayerId] = revealTimer;
                    if (revealTimer > 0)
                    {
                        if (currentObject.ContainsKey(pc.PlayerId))
                        {
                            revealRenderer[pc.PlayerId].GetComponent<SpriteRenderer>().sprite =
                                pc.GetComponent<SpriteRenderer>().sprite;
                            float mag = revealRenderer[pc.PlayerId].GetComponent<SpriteRenderer>().bounds.size.magnitude;
                            if (mag > 0.001f)
                                revealRenderer[pc.PlayerId].transform.localScale *= 0.5f / mag;
                        }
                    }
                    else
                    {
                        UnityEngine.Object.Destroy(revealRenderer[pc.PlayerId].gameObject);
                        isCurrentlyRevealed.Remove(pc.PlayerId);
                        revealRenderer.Remove(pc.PlayerId);
                        poolablePlayer.gameObject.SetActive(true);
                        SoundEffectsManager.play("morphlingMorph");
                    }
                }
            }
        }

        public static void invisUpdate()
        {
            List<byte> toRemove = new();
            foreach (byte playerId in invisPlayers.Keys)
            {
                PlayerControl pc = Helpers.playerById(playerId);
                if (pc == null || pc.Data.IsDead) { toRemove.Add(playerId); continue; }
                float timeLeft = invisPlayers[playerId] - Time.deltaTime;
                invisPlayers[playerId] = timeLeft;
                if (timeLeft > 0)
                {
                    pc.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f,
                        PlayerControl.LocalPlayer.Data.IsDead || PlayerControl.LocalPlayer.PlayerId == playerId ? 0.1f : 0f);
                }
                else
                {
                    pc.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
                    toRemove.Add(playerId);
                }

                if (isCurrentlyRevealed.ContainsKey(playerId) && revealRenderer.ContainsKey(playerId))
                    revealRenderer[playerId].GetComponent<SpriteRenderer>().color = pc.GetComponent<SpriteRenderer>().color;
            }
            foreach (byte id in toRemove) invisPlayers.Remove(id);
        }

        public static void speedboostUpdate()
        {
            List<byte> toRemove = new();
            foreach (byte key in speedboostActive.Keys)
            {
                float speedboostTimer = speedboostActive[key] - Time.deltaTime;
                speedboostActive[key] = speedboostTimer;
                if (speedboostTimer < 0) toRemove.Add(key);
            }
            foreach (byte key in toRemove) speedboostActive.Remove(key);
        }

        public static void dangerMeterUpdate()
        {
            if (!HudManager.Instance || !HudManager.Instance.DangerMeter) return;
            if (HudManager.Instance.DangerMeter.gameObject.active)
            {
                float dist = 55f;
                float dist2 = 15f;
                float curr = float.MaxValue;
                try
                {
                    foreach (PlayerControl playerControl in PlayerControl.AllPlayerControls.ToArray().Where(x =>
                                 !x.Data.IsDead && (PlayerControl.LocalPlayer.Data.Role.IsImpostor
                                     ? !x.Data.Role.IsImpostor
                                     : x.Data.Role.IsImpostor)))
                    {
                        if (invisPlayers.ContainsKey(playerControl.PlayerId)) continue;
                        if (playerControl != null)
                        {
                            float sqrMagnitude = (playerControl.transform.position - PlayerControl.LocalPlayer.transform.position).sqrMagnitude;
                            if (sqrMagnitude < dist && curr > sqrMagnitude) curr = sqrMagnitude;
                        }
                    }
                }
                catch { }

                float dangerLevel1 = Mathf.Clamp01((dist - curr) / (dist - dist2));
                float dangerLevel2 = Mathf.Clamp01((dist2 - curr) / dist2);
                HudManager.Instance.DangerMeter.SetDangerValue(dangerLevel1, dangerLevel2);
            }

            HudManager.Instance.DangerMeter?.gameObject.SetActive(!PlayerControl.LocalPlayer.Data.IsDead &&
                                                                  (!PlayerControl.LocalPlayer.Data.Role.IsImpostor ||
                                                                   HudManagerStartPatch.propHuntFindButton.isEffectActive));
        }

        public static void update()
        {
            if (!isPropHuntGM)
            {
                if (GameOptionsManager.Instance.currentGameOptions.GameMode != GameModes.HideNSeek)
                    HudManager.Instance.DangerMeter?.gameObject.SetActive(false);
                return;
            }

            if (timerRunning) timer = Mathf.Clamp(timer - Time.deltaTime, 0, timer >= 0 ? timer : 0);
            else if (blackOutTimer > 0f) blackOutTimer -= Time.deltaTime;

            propTargetAndTimerDisplayUpdate();
            poolablePlayerUpdate();
            speedboostUpdate();
            invisUpdate();
            dangerMeterUpdate();
        }

        // ---- Object Disguise Logic ----

        public static void transformLayers()
        {
            PlayerControl.LocalPlayer.clearAllTasks();
            foreach (Collider2D collider in Physics2D.OverlapCircleAll(PlayerControl.LocalPlayer.transform.position, 500))
            {
                bool whiteListed = false;
                foreach (string whiteListedWord in whitelistedObjects)
                    if (collider.gameObject.name.Contains(whiteListedWord) &&
                        collider.gameObject.GetComponent<SpriteRenderer>() != null)
                        whiteListed = true;
                if (collider.GetComponent<Console>() != null || whiteListed)
                {
                    if (whiteListed)
                    {
                        GameObject newgo = GameObject.Instantiate(collider.gameObject, collider.transform.parent);
                        newgo.name = "DONTUSE";
                        duplicatedCollider.Add(newgo);
                    }
                    collider.gameObject.layer = PlayerControl.LocalPlayer.gameObject.layer;
                }
            }
        }

        public static GameObject FindClosestDisguiseObject(GameObject origin, float radius, bool verbose = false)
        {
            try
            {
                Collider2D bestCollider = null;
                float bestDist = 9999;
                foreach (Collider2D collider in Physics2D.OverlapCircleAll(origin.transform.position, radius))
                {
                    bool whiteListed = false;
                    foreach (string whiteListedWord in whitelistedObjects)
                        if (collider.gameObject?.name?.Contains(whiteListedWord) == true)
                            whiteListed = true;
                    if (collider.GetComponent<Console>() != null || whiteListed)
                    {
                        float dist = Vector2.Distance(origin.transform.position, collider.transform.position);
                        if (dist < bestDist)
                        {
                            bestCollider = collider;
                            bestDist = dist;
                        }
                    }
                }
                if (bestCollider != null) return bestCollider.gameObject;
            }
            catch (Exception e)
            {
                TheOtherRolesPlugin.Logger.LogError($"Error in find closest disguise object: {e}");
            }
            return null;
        }

        public static GameObject FindPropByNameAndPos(string propName, float posX)
        {
            GameObject[] candidates = GameObject.FindObjectsOfType<GameObject>();
            GameObject prop = null;
            foreach (GameObject candidate in candidates)
                if (candidate.name == propName && candidate.transform.position.x == posX)
                    prop = candidate;
            return prop;
        }

        // ---- Sprite Getters ----

        private static Sprite disguiseButtonSprite;
        private static Sprite unstuckButtonSprite;
        private static Sprite revealButtonSprite;
        private static Sprite invisButtonSprite;
        private static Sprite speedboostButtonSprite;
        private static Sprite findButtonSprite;

        public static Sprite getDisguiseButtonSprite()
        {
            if (disguiseButtonSprite) return disguiseButtonSprite;
            disguiseButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.DisguiseButton.png", 115f);
            return disguiseButtonSprite;
        }

        public static Sprite getUnstuckButtonSprite()
        {
            if (unstuckButtonSprite) return unstuckButtonSprite;
            unstuckButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.UnStuck.png", 115f);
            return unstuckButtonSprite;
        }

        public static Sprite getRevealButtonSprite()
        {
            if (revealButtonSprite) return revealButtonSprite;
            revealButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Reveal.png", 115f);
            return revealButtonSprite;
        }

        public static Sprite getInvisButtonSprite()
        {
            if (invisButtonSprite) return invisButtonSprite;
            invisButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.InvisButton.png", 115f);
            return invisButtonSprite;
        }

        public static Sprite getFindButtonSprite()
        {
            if (findButtonSprite) return findButtonSprite;
            findButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.FindButton.png", 115f);
            return findButtonSprite;
        }

        public static Sprite getSpeedboostButtonSprite()
        {
            if (speedboostButtonSprite) return speedboostButtonSprite;
            speedboostButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.SpeedboostButton.png", 115f);
            return speedboostButtonSprite;
        }
    }

    // ---- Hunter Abilities ----

    public static class HunterPH
    {
        public static float revealCooldown = 30f;
        public static float revealDuration = 5f;
        public static float revealPunish = 10f;
        public static float adminCooldown = 30f;
        public static float adminDuration = 10f;
        public static float findCooldown = 60f;
        public static float findDuration = 10f;

        public static void clearAndReload()
        {
            revealCooldown = CustomOptionHolder.propHuntRevealCooldown.getFloat();
            revealDuration = CustomOptionHolder.propHuntRevealDuration.getFloat();
            revealPunish = CustomOptionHolder.propHuntRevealPunish.getFloat();
            adminCooldown = CustomOptionHolder.propHuntAdminCooldown.getFloat();
            findCooldown = CustomOptionHolder.propHuntFindCooldown.getFloat();
            findDuration = CustomOptionHolder.propHuntFindDuration.getFloat();
        }
    }

    // ---- Prop Abilities ----

    public static class PropPH
    {
        public static float unstuckCooldown = 30f;
        public static float unstuckDuration = 2f;
        public static float invisCooldown = 120f;
        public static float invisDuration = 5f;
        public static float speedboostCooldown = 60f;
        public static float speedboostDuration = 5f;
        public static float speedboostRatio = 2f;
        public static bool enableInvis = true;
        public static bool enableSpeedboost = true;

        public static void clearAndReload()
        {
            unstuckCooldown = CustomOptionHolder.propHuntUnstuckCooldown.getFloat();
            unstuckDuration = CustomOptionHolder.propHuntUnstuckDuration.getFloat();
            invisCooldown = CustomOptionHolder.propHuntInvisCooldown.getFloat();
            invisDuration = CustomOptionHolder.propHuntInvisDuration.getFloat();
            speedboostCooldown = CustomOptionHolder.propHuntSpeedboostCooldown.getFloat();
            speedboostDuration = CustomOptionHolder.propHuntSpeedboostDuration.getFloat();
            speedboostRatio = CustomOptionHolder.propHuntSpeedboostSpeed.getFloat();
            enableInvis = CustomOptionHolder.propHuntInvisEnabled.getBool();
            enableSpeedboost = CustomOptionHolder.propHuntSpeedboostEnabled.getBool();
        }
    }
}
