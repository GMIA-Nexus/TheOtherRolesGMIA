using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using TheOtherRoles.MetaContext;
using TheOtherRoles.Modules;
using TheOtherRoles.Objects;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    public class Zephyr : RoleBase<Zephyr>
    {
        public Zephyr()
        {
            RoleId = roleId = RoleId.Zephyr;
            numUses = Mathf.RoundToInt(CustomOptionHolder.zephyrNumberOfCannons.getFloat());
        }

        static public IEnumerable<HelpSprite> GetHelpSprites() => [new(getButtonSprite(), "zephyrCannonHint")];

        static public IEnumerable<DocumentReplacement> GetReplacementPart() => [new("%OPT%", leaveEvidence ? ModTranslation.getString("zephyrOPTHint") : "")];

        private static IDividedSpriteLoader smokeSprite = DividedSpriteLoader.FromResource("TheOtherRoles.Resources.ZephyrSmoke.png", 150f, 4, 1);
        static private Image spriteBloodPuddle = SpriteLoader.FromResource("TheOtherRoles.Resources.BloodPuddle.png", 130f);

        public static float cooldown;
        public int numUses = 3;
        public static float cannonRange;
        public static float cannonAttenuation;
        public static bool leaveEvidence { get { return CustomOptionHolder.zephyrLeaveEvidence.getBool(); } }
        public static bool triggerBothCooldown;

        public static Color color = Palette.ImpostorRed;

        private static Vector2 CalcPowerVector(Vector2 impactPos, Vector2 playerPos, float maxPower, float reductionFactor = 1f)
        {
            var dir = playerPos - impactPos;
            float mag = Mathf.Max(0f, maxPower - dir.magnitude * reductionFactor);
            return dir.normalized * mag;
        }

        public override void OnKill(PlayerControl target)
        {
            if (PlayerControl.LocalPlayer == player && HudManagerStartPatch.zephyrButton != null && triggerBothCooldown)
                HudManagerStartPatch.zephyrButton.Timer = HudManagerStartPatch.zephyrButton.MaxTimer;
        }

        public static Vector2 SuggestMoveToPos(Vector2 playerPos, Vector2 maxVector)
        {
            var currentData = MapData.GetCurrentMapData();
            bool CanWarpTo(Vector2 pos) => currentData.CheckMapArea(pos, 0.25f);

            int length = Mathf.Max((int)(maxVector.magnitude * 4), 100);
            Vector2[] pos = new Vector2[length];
            for (int i = 0; i < length; i++) pos[i] = playerPos + maxVector * (i + 1) / length;

            for (int i = 0; i < length; i++)
            {
                var p = pos[pos.Length - 1 - i];
                if (CanWarpTo(p)) return p;
            }
            return playerPos; //すべての場所が移動不可なら元の位置から動かない
        }

        public static void fireCannon(PlayerControl player, PlayerControl zephyr, Vector2 to)
        {
            if (player == null) return;
            if (player.hasModifier(RoleId.Mini) && !Mini.isGrownUp()) return;
            TORGameManager.Instance.GameStatistics.RecordEvent(new(GameStatistics.EventVariation.Kill, zephyr.PlayerId, 1 << player.PlayerId) { RelatedTag = EventDetail.Blown });

            TORGUIManager.Instance.StartCoroutine(CoPlayJumpAnimation(player, player?.transform.position ?? Vector2.zero, to, onLand : () =>
            {
                if (player.isRole(RoleId.NekoKabocha)) NekoKabocha.getRole(player).otherKiller = zephyr;
                player.Exiled();
                GameHistory.overrideDeathReasonAndKiller(player, DeadPlayer.CustomDeathReason.Blown, zephyr);
                if (leaveEvidence)
                {
                    var bloodRenderer = Helpers.CreateObject<SpriteRenderer>("ZephyrBlood", null, to.AsVector3(-10f).AsWorldPos(true));
                    bloodRenderer.sprite = spriteBloodPuddle.GetSprite();
                    bloodRenderer.color = player.Data.Color;
                    bloodRenderer.transform.localScale = new(0.45f, 0.45f, 1f);
                }
                if (player.isRole(RoleId.Bait)) Bait.getRole(player).OnDeath(zephyr);
            }).WrapToIl2Cpp());

            if (PlayerControl.LocalPlayer == zephyr) _ = new StaticAchievementToken("zephyr.challenge");
        }

        public static void RpcFireCannon(PlayerControl player, PlayerControl zephyr, Vector2 to)
        {
            if (player == null || zephyr == null) return;
            var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ZephyrBlowCannon, Hazel.SendOption.Reliable);
            writer.Write(player.PlayerId);
            writer.Write(zephyr.PlayerId);
            writer.Write(to.x);
            writer.Write(to.y);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            fireCannon(player, zephyr, to);
        }

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.ZephyrButton.png", 115f);
            return buttonSprite;
        }

        public static void checkCannon(Vector2 pos, byte zypherId)
        {
            var zephyr = Helpers.playerById(zypherId);
            IEnumerator CoShowSmoke()
            {
                var smoke = Helpers.CreateObject<SpriteRenderer>("SmokeRenderer", null, pos.AsVector3(-1f), LayerMask.NameToLayer("Objects"));
                smoke.sprite = smokeSprite.GetSprite(0);

                for (int i = 0; i < 4; i++)
                {
                    smoke.sprite = smokeSprite.GetSprite(i);
                    smoke.color = new(1f, 1f, 1f, 1f - i * 0.15f);
                    yield return Effects.Wait(0.12f);
                }
                UnityEngine.Object.Destroy(smoke.gameObject);
            }
            TORGUIManager.Instance.StartCoroutine(CoShowSmoke().WrapToIl2Cpp());

            var myPlayer = PlayerControl.LocalPlayer;
            if (myPlayer.Data.IsDead || !myPlayer.CanMove || myPlayer.PlayerId == zypherId) return;

            var powerVec = CalcPowerVector(pos, myPlayer.transform.position, cannonRange, cannonAttenuation);
            if (powerVec.magnitude < 0.5f) return; //たいして移動しない場合は何もしない。(計算の量を減らすための早期リターン)
            var moveTo = SuggestMoveToPos(myPlayer.GetTruePosition(), powerVec) - (UnityEngine.Vector2)(myPlayer.GetTruePosition() - (Vector2)myPlayer.transform.position);
            if ((moveTo - (UnityEngine.Vector2)myPlayer.transform.position).magnitude < 0.5f) return; //たいして移動しない場合は何もしない。

            //ミニゲームを開いている場合は閉じてから考える
            if (Minigame.Instance)
            {
                if (Minigame.Instance.MyNormTask)
                Minigame.Instance.ForceClose();
            }
            if (myPlayer.CanMove)
            {
                RpcFireCannon(myPlayer, zephyr, moveTo);
            }
        }

        public void RpcCheckCannon(Vector2 pos)
        {
            numUses--;
            var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ZephyrCheckCannon, Hazel.SendOption.Reliable);
            writer.Write(pos.x);
            writer.Write(pos.y);
            writer.Write(player.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            checkCannon(pos, player.PlayerId);
        }

        public static IEnumerator CoPlayJumpAnimation(PlayerControl player, Vector2 from, Vector2 to, float animOffset = 1.82f, float speedMul = 3f, Action onLand = null)
        {
            if (player == null) yield break;
            player.moveable = false;
            bool isLeft = to.x < from.x;
            player.MyPhysics.FlipX = isLeft;
            player.MyPhysics.Animations.Animator.Play(player.MyPhysics.Animations.group.SpawnAnim, 0f);
            player.MyPhysics.Animations.Animator.SetTime(animOffset);
            var skinAnim = player.cosmetics.skin.skin.SpawnLeftAnim && isLeft ? player.cosmetics.skin.skin.SpawnLeftAnim : player.cosmetics.skin.skin.SpawnAnim;
            player.cosmetics.skin.animator.Play(skinAnim, 0f);
            player.cosmetics.skin.animator.SetTime(animOffset);

            //Spawnアニメーションの最終位置はずれがあるので、アニメーションに合わせてずれを補正
            var animTo = to - new Vector2(isLeft ? -0.3f : 0.3f, -0.24f);

            //移動を滑らかにする
            player.NetTransform.SetPaused(true);
            player.NetTransform.ClearPositionQueues();

            //壁を無視して飛ぶ

            player.Collider.enabled = false;

            IEnumerator CoAnimMovement()
            {
                yield return player.MyPhysics.WalkPlayerTo(animTo, 0.01f, speedMul, true);

                player.MyPhysics.Animations.Animator.SetSpeed(1f);
                player.cosmetics.skin.animator.SetSpeed(1f);
                while (player.MyPhysics.Animations.IsPlayingSpawnAnimation() && player.MyPhysics.Animations.Animator.Playing) yield return null;
            }

            yield return new ParallelCoroutine(new StackfullCoroutine(CoAnimMovement()), ManagedCoroutineHelper.Continue(() => !MeetingHud.Instance)).WaitAndProcessTillSomeoneFinished();
            player.MyPhysics.Animations.Animator.SetSpeed(1f);
            player.cosmetics.skin.animator.SetSpeed(1f);
            player.Collider.enabled = true;

            //Spawnアニメーションのずれを補正
            player.transform.position = to;

            onLand?.Invoke();

            player.MyPhysics.Animations.PlayIdleAnimation();
            player.cosmetics.AnimateSkinIdle();

            player.NetTransform.SetPaused(false);
            player.moveable = true;

            yield break;
        }

        public static void clearAndReload()
        {
            cooldown = CustomOptionHolder.zephyrCooldown.getFloat();
            cannonRange = CustomOptionHolder.zephyrCannonRange.getFloat();
            cannonAttenuation = CustomOptionHolder.zephyrCannonAttenuation.getFloat();
            triggerBothCooldown = CustomOptionHolder.zephyrTriggerBothCooldown.getBool();
        }
    }
}
