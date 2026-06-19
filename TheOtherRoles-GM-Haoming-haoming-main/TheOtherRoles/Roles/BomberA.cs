using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using TheOtherRoles.Objects;
using TheOtherRoles.Patches;
using UnityEngine;
using static TheOtherRoles.GameHistory;
using static TheOtherRoles.Patches.PlayerControlFixedUpdatePatch;

namespace TheOtherRoles
{
    [HarmonyPatch]
    public class BomberA : RoleBase<BomberA>
    {
        public static Color color = Palette.ImpostorRed;

        public static CustomButton bomberButton;
        public static CustomButton releaseButton;

        public static PlayerControl bombTarget;
        public static PlayerControl currentTarget;
        public static PlayerControl tmpTarget;
        public static TMPro.TextMeshPro targetText;
        public static TMPro.TextMeshPro partnerTargetText;
        public static Dictionary<byte, PoolablePlayer> playerIcons = new();
        public static float duration { get { return CustomOptionHolder.bomberDuration.getFloat(); } }
        public static float cooldown { get { return CustomOptionHolder.bomberCooldown.getFloat(); } }
        public static bool countAsOne { get { return CustomOptionHolder.bomberCountAsOne.getBool(); } }
        public static bool showEffects { get { return CustomOptionHolder.bomberShowEffects.getBool(); } }
        public static bool ifOneDiesBothDie { get { return CustomOptionHolder.bomberIfOneDiesBothDie.getBool(); } }
        public static bool hasOneVote { get { return CustomOptionHolder.bomberHasOneVote.getBool(); } }
        public static bool alwaysShowArrow { get { return CustomOptionHolder.bomberAlwaysShowArrow.getBool(); } }
        public static Sprite bomberButtonSprite;
        public static Sprite releaseButtonSprite;
        public static float updateTimer = 0f;
        public static List<Arrow> arrows = new();
        public static float arrowUpdateInterval = 0.5f;

        public BomberA()
        {
            RoleType = roleId = RoleType.BomberA;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd()
        {
            bombTarget = null;
            BombEffect.clearBombEffects();
        }
        public override void FixedUpdate()
        {
            if (player == PlayerControl.LocalPlayer)
            {
                currentTarget = setTarget();
                setPlayerOutline(currentTarget, BomberA.color);
                arrowUpdate();

                foreach (PoolablePlayer pp in TORMapOptions.playerIcons.Values) pp.gameObject.SetActive(false);
                foreach (PoolablePlayer pp in playerIcons.Values) pp.gameObject.SetActive(false);
                if (player.isAlive() && BomberB.isAlive())
                {
                    if (bombTarget != null && TORMapOptions.playerIcons.ContainsKey(bombTarget.PlayerId) && TORMapOptions.playerIcons[bombTarget.PlayerId].gameObject != null)
                    {
                        var icon = TORMapOptions.playerIcons[bombTarget.PlayerId];
                        icon.gameObject.SetActive(true);
                        icon.transform.localPosition = Patches.IntroCutsceneOnDestroyPatch.bottomLeft + new Vector3(0f, -0.35f, -62f);
                        icon.transform.localScale = Vector3.one * 0.4f;
                        if (targetText == null)
                        {
                            targetText = GameObject.Instantiate(icon.cosmetics.nameText, icon.cosmetics.nameText.transform.parent);
                            targetText.enableWordWrapping = false;
                            targetText.transform.localScale = Vector3.one * 1.5f;
                            targetText.transform.localPosition += new Vector3(0f, 1.7f, 0);
                        }
                        targetText.text = ModTranslation.getString("bomberTarget");
                        targetText.gameObject.SetActive(true);
                        targetText.transform.parent = icon.gameObject.transform;
                    }
                    // 相方の設置したターゲットを表示する
                    if (BomberB.bombTarget != null && playerIcons.ContainsKey(BomberB.bombTarget.PlayerId) && playerIcons[BomberB.bombTarget.PlayerId].gameObject != null)
                    {
                        var icon = playerIcons[BomberB.bombTarget.PlayerId];
                        icon.gameObject.SetActive(true);
                        icon.transform.localPosition = Patches.IntroCutsceneOnDestroyPatch.bottomLeft + new Vector3(0f, -0.35f, -62f);
                        icon.transform.localScale = Vector3.one * 0.4f;
                        if (partnerTargetText == null)
                        {
                            partnerTargetText = GameObject.Instantiate(icon.cosmetics.nameText, icon.cosmetics.nameText.transform.parent);
                            partnerTargetText.enableWordWrapping = false;
                            partnerTargetText.transform.localScale = Vector3.one * 1.5f;
                            partnerTargetText.transform.localPosition += new Vector3(0f, 1.7f, 0);
                        }
                        partnerTargetText.text = ModTranslation.getString("bomberPartnerTarget");
                        partnerTargetText.gameObject.SetActive(true);
                        partnerTargetText.transform.parent = icon.gameObject.transform;
                    }
                }
            }
        }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null)
        {
            if (ifOneDiesBothDie)
            {
                var partner = BomberB.players.FirstOrDefault().player;
                if (!partner.Data.IsDead)
                {
                    if (killer != null)
                    {
                        partner.MurderPlayer(partner);
                    }
                    else
                    {
                        partner.Exiled();
                    }

                    finalStatuses[partner.PlayerId] = FinalStatus.Suicide;
                }
            }
        }
        public override void OnFinishShipStatusBegin() { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void MakeButtons(HudManager hm)
        {

            // Bomber button
            bomberButton = new CustomButton(
                // OnClick
                () =>
                {
                    if (currentTarget != null)
                    {
                        tmpTarget = currentTarget;
                        bomberButton.HasEffect = true;
                    }
                },
                // HasButton
                () => { return PlayerControl.LocalPlayer.isRole(RoleType.BomberA) && PlayerControl.LocalPlayer.isAlive() && BomberB.isAlive(); },
                // CouldUse
                () =>
                {
                    if (bomberButton.isEffectActive && tmpTarget != currentTarget)
                    {
                        tmpTarget = null;
                        bomberButton.Timer = 0f;
                        bomberButton.isEffectActive = false;
                    }

                    return PlayerControl.LocalPlayer.CanMove && currentTarget != null;
                },
                // OnMeetingEnds
                () =>
                {
                    bomberButton.Timer = bomberButton.MaxTimer;
                    bomberButton.isEffectActive = false;
                    tmpTarget = null;
                },
                getBomberButtonSprite(),
                CustomButton.ButtonPositions.upperRowCenter,
                hm,
                hm.KillButton,
                KeyCode.F,
                true,
                duration,
                // OnEffectsEnd
                () =>
                {
                    if ((tmpTarget.hasModifier(ModifierType.Mini) && !Mini.isGrownUp(tmpTarget)) || (BomberB.bombTarget != null && tmpTarget == BomberB.bombTarget))
                    {
                        bomberButton.Timer = 0f;
                    }
                    else
                    {
                        if (tmpTarget != null)
                        {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PlantBomb, SendOption.Reliable);
                            writer.Write(tmpTarget.PlayerId);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            bombTarget = tmpTarget;
                        }
                        tmpTarget = null;
                        bomberButton.Timer = bomberButton.MaxTimer;
                    }
                }
            )
            {
                buttonText = ModTranslation.getString("bomberPlantBomb")
            };
            // Bomber button
            releaseButton = new CustomButton(
                // OnClick
                () =>
                {
                    var bomberB = BomberB.allPlayers.FirstOrDefault();
                    float distance = Vector2.Distance(PlayerControl.LocalPlayer.transform.localPosition, bomberB.transform.localPosition);

                    if (PlayerControl.LocalPlayer.CanMove && BomberA.bombTarget != null && BomberB.bombTarget != null && bomberB.isAlive() && distance < 1)
                    {
                        var target = bombTarget;
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ReleaseBomb, Hazel.SendOption.Reliable, -1);
                        writer.Write(PlayerControl.LocalPlayer.PlayerId);
                        writer.Write(target.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.releaseBomb(PlayerControl.LocalPlayer.PlayerId, target.PlayerId);
                    }
                },
                // HasButton
                () => { return PlayerControl.LocalPlayer.isRole(RoleType.BomberA) && PlayerControl.LocalPlayer.isAlive() && BomberB.isAlive(); },
                // CouldUse
                () =>
                {
                    var bomberB = BomberB.allPlayers.FirstOrDefault();
                    float distance = Vector2.Distance(PlayerControl.LocalPlayer.transform.localPosition, bomberB.transform.localPosition);

                    return PlayerControl.LocalPlayer.CanMove && BomberA.bombTarget != null && BomberB.bombTarget != null && bomberB.isAlive() && distance < 1;
                },
                // OnMeetingEnds
                () =>
                {
                    releaseButton.Timer = releaseButton.MaxTimer;
                },
                getReleaseButtonSprite(),
                CustomButton.ButtonPositions.lowerRowCenter,
                hm,
                hm.KillButton,
                KeyCode.F,
                false
            )
            {
                buttonText = ModTranslation.getString("bomberDetonate")
            };

        }
        public static void SetButtonCooldowns()
        {
            bomberButton.MaxTimer = cooldown;
            bomberButton.EffectDuration = duration;
            releaseButton.MaxTimer = 0f;
        }

        public static void Clear()
        {
            bombTarget = null;
            currentTarget = null;
            tmpTarget = null;
            arrows = new List<Arrow>();
            players = new List<BomberA>();
            playerIcons = new Dictionary<byte, PoolablePlayer>();
            targetText = null;
            partnerTargetText = null;
        }
        public static bool isAlive()
        {
            foreach (var bomber in players)
            {
                if (!(bomber.player.Data.IsDead || bomber.player.Data.Disconnected))
                    return true;
            }
            return false;
        }
        public static Sprite getBomberButtonSprite()
        {
            if (bomberButtonSprite) return bomberButtonSprite;
            bomberButtonSprite = Helpers.loadSpriteFromAssetBundle("PlantBombButton.png");
            return bomberButtonSprite;
        }
        public static Sprite getReleaseButtonSprite()
        {
            if (releaseButtonSprite) return releaseButtonSprite;
            releaseButtonSprite = Helpers.loadSpriteFromAssetBundle("ReleaseButton.png");
            return releaseButtonSprite;
        }
        static void arrowUpdate()
        {
            if ((BomberA.bombTarget == null || BomberB.bombTarget == null) && !alwaysShowArrow) return;

            // 前フレームからの経過時間をマイナスする
            updateTimer -= Time.fixedDeltaTime;

            // 1秒経過したらArrowを更新
            if (updateTimer <= 0.0f)
            {

                // 前回のArrowをすべて破棄する
                foreach (Arrow arrow in arrows)
                {
                    if (arrow != null)
                    {
                        arrow.arrow.SetActive(false);
                        UnityEngine.Object.Destroy(arrow.arrow);
                    }
                }

                // Arrows一覧
                arrows = new List<Arrow>();

                // 相方の位置を示すArrowsを描画
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (p.Data.IsDead) continue;
                    if (p.isRole(RoleType.BomberB))
                    {
                        Arrow arrow;
                        arrow = new Arrow(Color.red);
                        arrow.arrow.SetActive(true);
                        arrow.Update(p.transform.position);
                        arrows.Add(arrow);
                    }
                }

                // タイマーに時間をセット
                updateTimer = arrowUpdateInterval;
            }
        }
        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
        class IntroCutsceneOnDestroyPatch
        {
            public static void Prefix(IntroCutscene __instance)
            {
                if (PlayerControl.LocalPlayer != null && FastDestroyableSingleton<HudManager>.Instance != null)
                {
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    {
                        NetworkedPlayerInfo data = p.Data;
                        PoolablePlayer player = UnityEngine.Object.Instantiate<PoolablePlayer>(__instance.PlayerPrefab, FastDestroyableSingleton<HudManager>.Instance.transform);
                        player.UpdateFromPlayerOutfit((NetworkedPlayerInfo.PlayerOutfit)p.Data.DefaultOutfit, PlayerMaterial.MaskType.ComplexUI, p.Data.IsDead, true);
                        player.SetFlipX(true);
                        player.cosmetics.currentPet?.gameObject.SetActive(false);
                        player.cosmetics.nameText.text = p.Data.DefaultOutfit.PlayerName;
                        player.gameObject.SetActive(false);
                        playerIcons[p.PlayerId] = player;
                    }
                }
            }
        }
    }
}
