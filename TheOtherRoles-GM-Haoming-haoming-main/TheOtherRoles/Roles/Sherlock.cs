using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using TheOtherRoles.Objects;
using UnityEngine;
using static TheOtherRoles.Patches.PlayerControlFixedUpdatePatch;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles
{
    [HarmonyPatch]
    public class Sherlock : RoleBase<Sherlock>
    {
        private static CustomButton sherlockInvestigateButton;
        public static TMPro.TMP_Text numInvestigateText;
        private static CustomButton sherlockWatchButton;
        public static TMPro.TMP_Text numKillTimerText;
        public static int killTimerCounter;
        public static int numTasks { get { return (int)CustomOptionHolder.sherlockRechargeTasksNumber.getFloat(); } }
        public static int cooldown { get { return (int)CustomOptionHolder.sherlockCooldown.getFloat(); } }
        public static float investigateDistance { get { return CustomOptionHolder.sherlockInvestigateDistance.getFloat(); } }
        public static int numUsed;
        public static List<Tuple<byte, Tuple<byte, Vector3>>> killLog;

        public static Color color = new Color32(248, 205, 70, byte.MaxValue);
        private static Sprite watchIcon;
        private static Sprite investigateIcon;



        public int numInvestigate = 0;
        public PlayerControl currentTarget;

        public Sherlock()
        {
            RoleType = roleId = RoleType.Sherlock;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd()
        {
            killTimerCounter = 0;
        }

        public override void FixedUpdate() { }

        public override void OnKill(PlayerControl target) { }
        public static void recordKillLog(PlayerControl killer, PlayerControl target)
        {
            killLog.Add(Tuple.Create(killer.PlayerId, Tuple.Create(target.PlayerId, target.transform.position + Vector3.zero)));
        }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void OnFinishShipStatusBegin() { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void MakeButtons(HudManager hm)
        {

            // Sherlock Investigate
            sherlockInvestigateButton = new CustomButton(
                () =>
                {
                    string message = "";
                    foreach (var item in killLog)
                    {
                        float distance = Vector3.Distance(item.Item2.Item2, PlayerControl.LocalPlayer.transform.position);
                        if (distance < investigateDistance)
                        {
                            PlayerControl killer = Helpers.getPlayerById(item.Item1);
                            PlayerControl target = Helpers.getPlayerById(item.Item2.Item1);
                            string killerTeam = RoleInfo.GetRolesString(killer, true);

                            // if(killer.isImpostor())
                            // {
                            //     killerTeam = ModTranslation.getString("sherlockImpostor");
                            // }
                            // else if(killer.isRole(RoleType.Moriarty))
                            // {
                            //     killerTeam = ModTranslation.getString("moriarty");
                            // }
                            // else if(killer.isRole(RoleType.Jackal))
                            // {
                            //     killerTeam = ModTranslation.getString("jackal");
                            // }
                            // else if(killer.isNeutral())
                            // {
                            //     killerTeam = ModTranslation.getString("sherlockNeutral");
                            // }
                            // else
                            // {
                            //     killerTeam = ModTranslation.getString("sherlockCrewmate");
                            // }
                            message += String.Format(ModTranslation.getString("sherlockInvestigateMessage1"), target.name, killerTeam);
                        }
                    }
                    if (message == "")
                    {
                        message = ModTranslation.getString("sherlockInvestigateMessage2");
                    }
                    investigateMessage(message, 5f, Color.white);
                    numUsed += 1;
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleType.Sherlock) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (numInvestigateText != null)
                    {
                        numInvestigateText.text = $"{numUsed}/{getNumInvestigate()}";
                    }

                    return PlayerControl.LocalPlayer.CanMove && numUsed < getNumInvestigate();
                },
                () => { sherlockInvestigateButton.Timer = sherlockInvestigateButton.MaxTimer; },
                getInvestigateIcon(),
                CustomButton.ButtonPositions.lowerRowRight,
                hm,
                hm.KillButton,
                KeyCode.Q
            )
            { buttonText = ModTranslation.getString("InvestigateText") };

            numInvestigateText = GameObject.Instantiate(sherlockInvestigateButton.actionButton.cooldownTimerText, sherlockInvestigateButton.actionButton.cooldownTimerText.transform.parent);
            numInvestigateText.text = "";
            numInvestigateText.enableWordWrapping = false;
            numInvestigateText.transform.localScale = Vector3.one * 0.5f;
            numInvestigateText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            // Sherlock Watch
            sherlockWatchButton = new CustomButton(
                () => { },
                () => { return PlayerControl.LocalPlayer.isRole(RoleType.Sherlock) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (numKillTimerText != null)
                    {
                        numKillTimerText.text = $"{killTimerCounter}";
                    }
                    if (sherlockWatchButton.Timer <= 0)
                    {
                        killTimerCounter += 1;
                        sherlockWatchButton.Timer = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown;
                    }

                    return PlayerControl.LocalPlayer.CanMove && numUsed < getNumInvestigate();
                },
                () => { sherlockWatchButton.Timer = sherlockWatchButton.MaxTimer; },
                getWatchIcon(),
                CustomButton.ButtonPositions.upperRowRight,
                hm,
                hm.KillButton,
                KeyCode.Q
            )
            { buttonText = "" };

            numKillTimerText = GameObject.Instantiate(sherlockWatchButton.actionButton.cooldownTimerText, sherlockWatchButton.actionButton.cooldownTimerText.transform.parent);
            numKillTimerText.text = "";
            numKillTimerText.enableWordWrapping = false;
            numKillTimerText.transform.localScale = Vector3.one * 0.5f;
            numKillTimerText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
        }

        public static void SetButtonCooldowns()
        {
            sherlockInvestigateButton.MaxTimer = Sherlock.cooldown;
            sherlockWatchButton.MaxTimer = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown;
        }

        public static void Clear()
        {
            players = new List<Sherlock>();
            numUsed = 0;
            killLog = new();
        }
        public static Sprite getInvestigateIcon()
        {
            if (investigateIcon) return investigateIcon;
            investigateIcon = Helpers.loadSpriteFromAssetBundle("SherlockInvestigate.png");
            return investigateIcon;
        }
        public static Sprite getWatchIcon()
        {
            if (watchIcon) return watchIcon;
            watchIcon = Helpers.loadSpriteFromAssetBundle("SherlockWatch.png");
            return watchIcon;
        }


        public static int getNumInvestigate()

        {

            var p = players.Where(p => p.player == PlayerControl.LocalPlayer).FirstOrDefault();

            int counter = p.player.Data.Tasks.ToArray().Where(t => t.Complete).Count();

            return (int)Math.Floor((float)counter / numTasks);

        }

        private static TMPro.TMP_Text text;

        public static void investigateMessage(string message, float duration, Color color)

        {

            RoomTracker roomTracker = HudManager.Instance?.roomTracker;

            if (roomTracker != null)

            {
                GameObject gameObject = UnityEngine.Object.Instantiate(roomTracker.gameObject);

                gameObject.transform.SetParent(HudManager.Instance.transform);
                UnityEngine.Object.DestroyImmediate(gameObject.GetComponent<RoomTracker>());

                // Use local position to place it in the player's view instead of the world location
                gameObject.transform.localPosition = new Vector3(0, -1.8f, gameObject.transform.localPosition.z);
                gameObject.transform.localScale *= 1.5f;

                text = gameObject.GetComponent<TMPro.TMP_Text>();
                text.text = message;
                text.color = color;

                HudManager.Instance.StartCoroutine(Effects.Lerp(duration, new Action<float>((p) =>
                {
                    if (p == 1f && text != null && text.gameObject != null)
                    {
                        UnityEngine.Object.Destroy(text.gameObject);
                    }
                })));
            }
        }
    }
}
