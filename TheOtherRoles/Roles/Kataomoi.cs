using System;
using System.Collections.Generic;
using System.Linq;
using TheOtherRoles.MetaContext;
using TheOtherRoles.Modules;
using TheOtherRoles.Objects;
using TheOtherRoles.Patches;
using TheOtherRoles.Utilities;
using UnityEngine;
using static TheOtherRoles.Patches.PlayerControlFixedUpdatePatch;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    [TORRPCHolder]
    public class Kataomoi : RoleBase<Kataomoi>
    {
        public static Color color = Lovers.color;

        static public IEnumerable<HelpSprite> GetHelpSprites()
        {
            yield return new(getStareSprite(), "kataomoiStareHint");
            yield return new(getSearchSprite(), "kataomoiSearchHint");
            yield return new(getStalkingSprite(), "kataomoiStalkHint");
            yield return new(getLoveSprite(), "kataomoiLoveHint");
        }

        static public IEnumerable<DocumentReplacement> GetReplacementPart() => [new("%NUM%", CustomOptionHolder.kataomoiStareCount.getFloat().ToString())];

        public static float stareCooldown = 30f;
        public static float stareDuration = 3f;
        public static int stareCount = 1;
        public static int stareCountMax = 1;
        public static float stalkingCooldown = 30f;
        public static float stalkingDuration = 5f;
        public static float stalkingFadeTime = 0.5f;
        public static float searchCooldown = 30f;
        public static float searchDuration = 5f;
        public static bool isSearch = false;
        public static float stalkingTimer = 0f;
        public static float stalkingEffectTimer = 0f;
        public static bool triggerKataomoiWin = false;
        public static PlayerControl target = null;
        public static PlayerControl currentTarget = null;
        public static TMPro.TextMeshPro stareText = null;
        public static SpriteRenderer[] gaugeRenderer = new SpriteRenderer[3];
        public static Arrow arrow;
        public static float gaugeTimer = 0.0f;
        public static float baseGauge = 0f;

        static bool _isStalking = false;

        public static RemoteProcess<byte> SetTarget = RemotePrimitiveProcess.OfByte("KataomoiSetTarget", (message, _) => target = Helpers.playerById(message));

        public static RemoteProcess TriggerWin = new("KataomoiWin", (_) =>
        {
            if (!exists) return;

            triggerKataomoiWin = true;
            if (target != null)
            {
                target.Exiled();
                GameHistory.overrideDeathReasonAndKiller(target, DeadPlayer.CustomDeathReason.KataomoiStare, allPlayers.FirstOrDefault());
            }
        });

        public override GUIContext ProgressWidget { get => ProgressGUI.Holder(target != null ? ProgressGUI.OneLineText(ModTranslation.getString("roleInfoKataomoiTargetFull") + ": " + target?.Data.PlayerName?? "") : null); }

        public static RemoteProcess<byte> SetStalking = RemotePrimitiveProcess.OfByte("KataomoiSetStalking", (message, _) =>
        {
            PlayerControl player = Helpers.playerById(message);
            if (!isRole(player)) return;

            doStalking();
        });

        public Kataomoi()
        {
            RoleId = roleId = RoleId.Kataomoi;
        }

        public override void FixedUpdate()
        {
            if (player != PlayerControl.LocalPlayer || target == null) return;

            var untargetables = PlayerControl.AllPlayerControls.ToArray().Where(x => x.PlayerId != target.PlayerId).ToList();
            currentTarget = setTarget(untargetablePlayers: untargetables);
            if (currentTarget != null) setPlayerOutline(currentTarget, color);

            if (player.Data.IsDead)
            {
                if (arrow != null) UnityEngine.Object.Destroy(arrow.arrow);
                arrow = null;
                if (stareText != null && stareText.gameObject != null) UnityEngine.Object.Destroy(stareText.gameObject);
                stareText = null;
                for (int i = 0; i < gaugeRenderer.Length; ++i)
                {
                    if (gaugeRenderer[i] != null)
                    {
                        UnityEngine.Object.Destroy(gaugeRenderer[i].gameObject);
                        gaugeRenderer[i] = null;
                    }
                }
                TORMapOptions.resetPoolables();
                return;
            }

            // Update Stare Count Text
            if (stareText != null)
            {
                stareText.gameObject.SetActive(!MeetingHud.Instance);
                if (stareCount > 0)
                    stareText.text = $"{stareCount}";
                else
                    stareText.text = "";
            }

            if (target != null && TORMapOptions.playerIcons.ContainsKey(target?.PlayerId ?? byte.MaxValue))
                TORMapOptions.playerIcons[target.PlayerId].gameObject.SetActive(!MeetingHud.Instance);
            for (int i = 0; i < gaugeRenderer.Length; ++i)
                if (gaugeRenderer[i] != null)
                    gaugeRenderer[i].gameObject?.SetActive(!MeetingHud.Instance);

            // Update Arrow
            arrow ??= new Arrow(color);
            arrow.arrow.SetActive(isSearch);
            if (isSearch && target != null)
                arrow.Update(target.transform.position);
        }

        public static void generateText()
        {
            if (FastDestroyableSingleton<HudManager>.Instance != null)
            {
                stareText = UnityEngine.Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText, FastDestroyableSingleton<HudManager>.Instance.transform);
                stareText.alignment = TMPro.TextAlignmentOptions.Center;
                stareText.transform.localPosition = IntroCutsceneOnDestroyPatch.bottomLeft + new Vector3(0f, -0.35f, -62f);
                stareText.transform.localScale = Vector3.one * 0.5f;
                stareText.gameObject.SetActive(PlayerControl.LocalPlayer.isRole(RoleId.Kataomoi) && target != null);

                gaugeRenderer[0] = UnityEngine.Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.KillButton.graphic, FastDestroyableSingleton<HudManager>.Instance.transform);
                var killButton = gaugeRenderer[0].GetComponent<KillButton>();
                killButton.SetCoolDown(0.00000001f, 0.00000001f);
                killButton.SetFillUp(0.00000001f, 0.00000001f);
                killButton.SetDisabled();
                Helpers.hideGameObjects(gaugeRenderer[0].gameObject);
                var components = killButton.GetComponents<Component>();
                foreach (var c in components)
                    if (c as KillButton == null && c as SpriteRenderer == null)
                        UnityEngine.Object.Destroy(c.gameObject);

                gaugeRenderer[0].sprite = getLoveGaugeSprite(0);
                gaugeRenderer[0].color = new Color32(175, 175, 176, 255);
                gaugeRenderer[0].size = new Vector2(300f, 64f);
                gaugeRenderer[0].gameObject.SetActive(true);
                gaugeRenderer[0].transform.localPosition = new Vector3(-3.354069f + 1f, -2.429999f, -8f);
                gaugeRenderer[0].transform.localScale = Vector3.one;

                gaugeRenderer[1] = UnityEngine.Object.Instantiate(gaugeRenderer[0], FastDestroyableSingleton<HudManager>.Instance.transform);
                gaugeRenderer[1].sprite = getLoveGaugeSprite(1);
                gaugeRenderer[1].size = new Vector2(261f, 7f);
                gaugeRenderer[1].color = color;
                gaugeRenderer[1].transform.localPosition = new Vector3(-3.482069f + 1f, -2.626999f, -8.1f);
                gaugeRenderer[1].transform.localScale = Vector3.one;

                gaugeRenderer[2] = UnityEngine.Object.Instantiate(gaugeRenderer[0], FastDestroyableSingleton<HudManager>.Instance.transform);
                gaugeRenderer[2].sprite = getLoveGaugeSprite(2);
                gaugeRenderer[2].color = gaugeRenderer[0].color;
                gaugeRenderer[2].size = new Vector2(300f, 64f);
                gaugeRenderer[2].transform.localPosition = new Vector3(-3.354069f + 1f, -2.429999f, -8.2f);
                gaugeRenderer[2].transform.localScale = Vector3.one;

                gaugeTimer = 1.0f;

                for (int i = 0; i < 3; i++) if (gaugeRenderer[i] != null) gaugeRenderer[i].gameObject.SetActive(false);
            }
        }

        static Sprite stareSprite;
        public static Sprite getStareSprite()
        {
            if (stareSprite) return stareSprite;
            stareSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.KataomoiStareButton.png", 115f);
            return stareSprite;
        }

        static Sprite loveSprite;
        public static Sprite getLoveSprite()
        {
            if (loveSprite) return loveSprite;
            loveSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.KataomoiLoveButton.png", 115f);
            return loveSprite;
        }

        static Sprite searchSprite;
        public static Sprite getSearchSprite()
        {
            if (searchSprite) return searchSprite;
            searchSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.KataomoiSearchButton.png", 115f);
            return searchSprite;
        }

        static Sprite stalkingSprite;
        public static Sprite getStalkingSprite()
        {
            if (stalkingSprite) return stalkingSprite;
            stalkingSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.KataomoiStalkingButton.png", 115f);
            return stalkingSprite;
        }

        static Sprite[] loveGaugeSprites = new Sprite[3];
        public static Sprite getLoveGaugeSprite(int index)
        {
            if (index < 0 || index >= loveGaugeSprites.Length) return null;
            if (loveGaugeSprites[index]) return loveGaugeSprites[index];

            int id = 0;
            switch (index)
            {
                case 0: id = 1; break;
                case 1: id = 2; break;
                case 2: id = 11; break;
            }
            loveGaugeSprites[index] = Helpers.loadSpriteFromResources(string.Format("TheOtherRoles.Resources.KataomoiGauge_{0:d2}.png", id), 115f);
            return loveGaugeSprites[index];
        }

        public static void doStare()
        {
            baseGauge = getLoveGauge();
            gaugeTimer = 1.0f;
            stareCount = Mathf.Max(0, stareCount - 1);

            if (gaugeRenderer[2] != null && stareCount == 0)
                gaugeRenderer[2].color = color;
            if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(DestroyableSingleton<HudManager>.Instance.TaskCompleteSound, false, 0.8f);
        }

        public static void doStalking()
        {
            if (!exists) return;
            stalkingTimer = stalkingDuration;
            _isStalking = true;
        }

        public static void killKataomoi(PlayerControl player, PlayerControl killer = null)
        {
            if (!exists || player != target) return;
            foreach (var p in allPlayers)
            {
                if (p && !p.Data.IsDead)
                {
                    if (killer) p.MurderPlayer(p, MurderResultFlags.Succeeded);
                    else
                    {
                        if (PlayerControl.LocalPlayer == p && Helpers.ShowKillAnimation)
                            FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(p.Data, p.Data);
                        p.Exiled();
                    }
                    GameHistory.overrideDeathReasonAndKiller(p, DeadPlayer.CustomDeathReason.LoverSuicide);
                }
            }
        }

        public static void resetStalking()
        {
            if (!exists) return;
            _isStalking = false;
            setAlpha(1.0f);
        }

        public static bool isStalking(PlayerControl player)
        {
            if (player == null || !isRole(player)) return false;
            return _isStalking && stalkingTimer > 0;
        }

        public static bool isStalking()
        {
            return allPlayers.Any(x => isStalking(x));
        }

        public static void doSearch()
        {
            if (!exists) return;
            isSearch = true;
        }

        public static void resetSearch()
        {
            if (!exists) return;
            isSearch = false;
        }

        public static bool canLove()
        {
            return stareCount <= 0;
        }

        public static float getLoveGauge()
        {
            return 1.0f - (stareCountMax == 0 ? 0f : stareCount / (float)stareCountMax);
        }

        public static void resetAllArrow()
        {
            if (!isRole(PlayerControl.LocalPlayer)) return;
            TORMapOptions.resetPoolables();
            for (int i = 0; i < gaugeRenderer.Length; ++i)
                if (gaugeRenderer[i] != null)
                    gaugeRenderer[i].gameObject.SetActive(false);
            if (stareText != null) stareText.gameObject.SetActive(false);
        }

        public override void ResetRole(bool isShifted)
        {
            resetAllArrow();
            resetStalking();
        }

        public static void clearAndReload()
        {
            resetStalking();

            stareCooldown = CustomOptionHolder.kataomoiStareCooldown.getFloat();
            stareDuration = CustomOptionHolder.kataomoiStareDuration.getFloat();
            stareCount = stareCountMax = (int)CustomOptionHolder.kataomoiStareCount.getFloat();
            stalkingCooldown = CustomOptionHolder.kataomoiStalkingCooldown.getFloat();
            stalkingDuration = CustomOptionHolder.kataomoiStalkingDuration.getFloat();
            stalkingFadeTime = CustomOptionHolder.kataomoiStalkingFadeTime.getFloat();
            searchCooldown = CustomOptionHolder.kataomoiSearchCooldown.getFloat();
            searchDuration = CustomOptionHolder.kataomoiSearchDuration.getFloat();
            isSearch = false;
            stalkingTimer = 0f;
            stalkingEffectTimer = 0f;
            triggerKataomoiWin = false;
            target = null;
            currentTarget = null;
            if (stareText != null) UnityEngine.Object.Destroy(stareText);
            stareText = null;
            if (arrow != null && arrow.arrow != null) UnityEngine.Object.Destroy(arrow.arrow);
            for (int i = 0; i < gaugeRenderer.Length; ++i)
            {
                if (gaugeRenderer[i] != null)
                {
                    UnityEngine.Object.Destroy(gaugeRenderer[i].gameObject);
                    gaugeRenderer[i] = null;
                }
            }
            arrow = null;
            gaugeTimer = 0.0f;
            baseGauge = 0.0f;
            players = [];
        }

        public static void fixedUpdate(PlayerPhysics __instance)
        {
            var kataomoi = allPlayers.FirstOrDefault();
            if (kataomoi == null) return;
            if (kataomoi != __instance.myPlayer) return;

            if (gaugeRenderer[1] != null && gaugeTimer > 0.0f)
            {
                gaugeTimer = Mathf.Max(gaugeTimer - Time.fixedDeltaTime, 0.0f);
                float gauge = getLoveGauge();
                float nowGauge = Mathf.Lerp(baseGauge, gauge, 1.0f - gaugeTimer);
                gaugeRenderer[1].transform.localPosition = new Vector3(Mathf.Lerp(-3.470784f - 1.121919f + 1.25f, -3.470784f + 1.25f, nowGauge), -2.626999f, -8.1f);
                gaugeRenderer[1].transform.localScale = new Vector3(nowGauge, 1, 1);
            }

            if (kataomoi.Data.IsDead) return;
            if (_isStalking && stalkingTimer > 0)
            {
                kataomoi.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", 0f);
                stalkingTimer = Mathf.Max(0f, stalkingTimer - Time.fixedDeltaTime);
                if (stalkingFadeTime > 0)
                {
                    float elapsedTime = stalkingDuration - stalkingTimer;
                    float alpha = Mathf.Min(elapsedTime, stalkingFadeTime) / stalkingFadeTime;
                    alpha = Mathf.Clamp(1f - alpha, PlayerControl.LocalPlayer == kataomoi || PlayerControl.LocalPlayer.CanSeeInvisible() ? 0.5f : 0f, 1f);
                    setAlpha(alpha);
                }
                else
                    setAlpha(PlayerControl.LocalPlayer == kataomoi ? 0.5f : 0f);

                if (stalkingTimer <= 0f)
                {
                    _isStalking = false;
                    stalkingEffectTimer = stalkingFadeTime;
                }
            }
            else if (!_isStalking && stalkingEffectTimer > 0)
            {
                stalkingEffectTimer = Mathf.Max(0f, stalkingEffectTimer - Time.fixedDeltaTime);
                if (stalkingFadeTime > 0)
                {
                    float elapsedTime = stalkingFadeTime - stalkingEffectTimer;
                    float alpha = Mathf.Min(elapsedTime, stalkingFadeTime) / stalkingFadeTime;
                    alpha = Mathf.Clamp(alpha, PlayerControl.LocalPlayer == kataomoi || PlayerControl.LocalPlayer.Data.IsDead ? 0.5f : 0f, 1f);
                    setAlpha(alpha);
                }
                else
                    setAlpha(1.0f);
            }
            else
                setAlpha(1.0f);
        }

        static void setAlpha(float alpha)
        {
            var kataomoi = allPlayers.FirstOrDefault();
            if (kataomoi == null) return;
            var color = Color.Lerp(Palette.ClearWhite, Palette.White, alpha);
            try
            {
                if (Chameleon.chameleon.Any(x => x.PlayerId == kataomoi.PlayerId) && Chameleon.visibility(kataomoi.PlayerId) < 1f && !isStalking()) return;
                Helpers.setInvisible(kataomoi, color, alpha);
            }
            catch { }
        }
    }
}
