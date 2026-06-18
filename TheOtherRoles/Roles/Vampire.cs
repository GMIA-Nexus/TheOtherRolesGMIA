using System;
using System.Collections.Generic;
using System.Linq;
using TheOtherRoles.MetaContext;
using TheOtherRoles.Modules;
using TheOtherRoles.Objects;
using UnityEngine;
using static TheOtherRoles.Patches.PlayerControlFixedUpdatePatch;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    [TORRPCHolder]
    public class Vampire : RoleBase<Vampire> {
        public static Color color = Palette.ImpostorRed;

        static public IEnumerable<DocumentReplacement> GetReplacementPart()
        {
            yield return new("%TIME%", delay.ToString());
            yield return new("%SEC%", CustomOptionHolder.vampireCooldownDecrease.getFloat().ToString());
            yield return new("%GARLIC%", ModTranslation.getString($"vampireGARLIC{(canKillNearGarlics ? "Kill" : "")}Hint"));
        }

        public Vampire()
        {
            RoleId = roleId = RoleId.Vampire;
            acTokenChallenge = null;
            currentTarget = null;
            bitten = null;
            targetNearGarlic = false;
        }

        public static float delay { get { return CustomOptionHolder.vampireKillDelay.getFloat(); } }
        public static float cooldown = 30f;
        public static float cooldownDecrease = 2.5f;
        public static bool canKillNearGarlics { get { return CustomOptionHolder.vampireCanKillNearGarlics.getBool(); } }
        public static bool localPlacedGarlic = false;
        public static bool garlicsActive = true;

        public PlayerControl currentTarget;
        public PlayerControl bitten;
        public bool targetNearGarlic = false;

        public AchievementToken<(DateTime deathTime, bool cleared)> acTokenChallenge;

        public static RemoteProcess<(byte targetId, byte performReset, byte vampireId)> SetBitten = new("VampireSetBitten", (message, _) =>
        {
            var vampire = getRole(Helpers.playerById(message.vampireId));
            if (vampire == null) return;
            if (message.performReset != 0)
            {
                vampire.bitten = null;
                return;
            }

            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.PlayerId == message.targetId && !player.Data.IsDead) {
                    vampire.bitten = player;
                }
            }
        });

        public override void OnMeetingStart()
        {
            // Reset vampire bitten
            bitten = null;
            if (PlayerControl.LocalPlayer == player)
                acTokenChallenge.Value.cleared |= DateTime.UtcNow.Subtract(acTokenChallenge.Value.deathTime).TotalSeconds <= 3;
        }

        public override void PostInit()
        {
            if (PlayerControl.LocalPlayer != player) return;
            acTokenChallenge ??= new("vampire.challenge", (DateTime.UtcNow, false), (val, _) => val.cleared);
        }

        public override void FixedUpdate()
        {
            if (player != PlayerControl.LocalPlayer) return;

            PlayerControl target = null;
            if (Spy.exists)
            {
                if (Spy.impostorsCanKillAnyone) {
                    target = setTarget(false, true);
                }
                else
                {
                    var untargetables = new List<PlayerControl>();
                    untargetables.AddRange(Spy.allPlayers);
                    untargetables.AddRange(Jackal.players.Where(x => x.wasTeamRed).Select(x => x.player));
                    untargetables.AddRange(Sidekick.players.Where(x => x.wasTeamRed).Select(x => x.player));
                    target = setTarget(true, true, untargetables);
                }
            }
            else {
                var untargetables = new List<PlayerControl>();
                untargetables.AddRange(Jackal.players.Where(x => x.wasImpostor).Select(x => x.player));
                untargetables.AddRange(Sidekick.players.Where(x => x.wasImpostor).Select(x => x.player));
                target = setTarget(true, true, untargetables);
            }

            bool targetNearGarlic = false;
            if (target != null)
            {
                foreach (Garlic garlic in Garlic.garlics)
                {
                    if (Vector2.Distance(garlic.garlic.transform.position, target.transform.position) <= 1.91f)
                        targetNearGarlic = true;
                }
            }
            this.targetNearGarlic = targetNearGarlic;
            currentTarget = target;
            setPlayerOutline(currentTarget, color);
        }

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite() {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.VampireButton.png", 115f);
            return buttonSprite;
        }

        private static Sprite garlicButtonSprite;
        public static Sprite getGarlicButtonSprite() {
            if (garlicButtonSprite) return garlicButtonSprite;
            garlicButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.GarlicButton.png", 115f);
            return garlicButtonSprite;
        }

        public static void clearAndReload() {
            localPlacedGarlic = false;
            garlicsActive = CustomOptionHolder.vampireSpawnRate.getSelection() > 0;
            cooldown = CustomOptionHolder.vampireCooldown.getFloat();
            cooldownDecrease = Mathf.Min(cooldown, CustomOptionHolder.vampireCooldownDecrease.getFloat());
            players = [];
        }
    }
}
