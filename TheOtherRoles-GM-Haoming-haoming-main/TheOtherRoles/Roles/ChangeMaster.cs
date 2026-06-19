#if URUSEN
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hazel;
using HarmonyLib;
using TheOtherRoles.Objects;
using static TheOtherRoles.TheOtherRoles;
using static TheOtherRoles.Patches.PlayerControlFixedUpdatePatch;
using UnityEngine;
using BepInEx.IL2CPP.Utils.Collections;

namespace TheOtherRoles
{
    [HarmonyPatch]
    public class ChangeMaster : RoleBase<ChangeMaster>
    {
        public class cosmetic{
            public string hat;
            public int colorId;
            public string name;
            public AudioClip clip;
            public cosmetic(string h, string n, int c, AudioClip cl)
            {
                this.hat = h;
                this.colorId = c;
                this.name = n;
                this.clip = cl;
            }
        }
        public static List<cosmetic> cosmetics;
        public static void createCosmetics()
        {
            cosmetics = new()
            {
                new cosmetic("hat_卯ノ花しうね_卯ノ花しうね", "卯ノ花しうね", 7, siune),
                new cosmetic("hat_ポン酢野郎_卯ノ花しうね", "ポン酢野郎", 2, ponzu),
                new cosmetic("hat_yuta14_卯ノ花しうね", "yuta14", 1, yuta14),
                new cosmetic("hat_みさとらん_卯ノ花しうね", "みさとらん", 13, misatoran),
                new cosmetic("hat_ぼんじゅうる_卯ノ花しうね", "ぼんじゅうる", 8, bonjuru),
                new cosmetic("hat_けいすけ_卯ノ花しうね", "けいすけ", 11, keisuke),
            };
        }
        public static int currentCosmetic = 0;
        public static cosmetic getCosmetic()
        {
            return cosmetics[currentCosmetic];
        }
        public static GameObject targetAudioObject;
        public static GameObject changeMasterAudioObject;
        public static TMPro.TMP_Text numChangeMasterText;
        private static CustomButton changeMasterButton;
        public static float maxDistance {get {return CustomOptionHolder.changeMasterMaxDistance.getFloat();}}
        public static float minDistance {get {return CustomOptionHolder.changeMasterMinDistance.getFloat();}}

        public static Color color = Palette.White;

        public static float numChangeMaster { get { return CustomOptionHolder.changeMasterNum.getFloat(); } }
        public static bool changeTargetAfterMeeting {get { return CustomOptionHolder.changeMasterChangeTargetAfterMeeting.getBool();}}
        public static float cooldown = 0f;

        public PlayerControl currentTarget;

        public static List<PlayerControl> changeMasters = new List<PlayerControl>();

        public bool lightActive = false;

        public static AudioClip siune;
        public static AudioClip bonjuru;
        public static AudioClip keisuke;
        public static AudioClip misatoran;
        public static AudioClip ponzu;
        public static AudioClip yuta14;
        public static Sprite change;

        public ChangeMaster()
        {
            RoleType = roleId = RoleType.ChangeMaster;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd()
        {
            if (PlayerControl.LocalPlayer.isRole(RoleType.ChangeMaster) && changeTargetAfterMeeting)
            {
                setCurrentCosmetic();
            }
            changeMasters = new();
        }
        public override void FixedUpdate()
        {
            currentTarget = setTarget(untargetablePlayers: changeMasters);
            setPlayerOutline(currentTarget, ChangeMaster.color);
        }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void OnFinishShipStatusBegin() { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void MakeButtons(HudManager hm)
        {
            Logger.info("MakeButtons");
            changeMasterButton = new CustomButton(
                () =>
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetChangeMaster, Hazel.SendOption.Reliable, -1);
                    writer.Write(local.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.setChangeMaster(local.currentTarget.PlayerId);
                    writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PlayChangeMasterVoice, Hazel.SendOption.Reliable, -1);
                    writer.Write(local.currentTarget.PlayerId);
                    writer.Write(local.player.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.playChangeMasterVoice(local.currentTarget.PlayerId, local.player.PlayerId);
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleType.ChangeMaster) && !PlayerControl.LocalPlayer.Data.IsDead && numChangeMaster > changeMasters.Count(); },
                () =>
                {
                    if (numChangeMasterText != null)
                    {
                        if (numChangeMaster > changeMasters.Count())
                            numChangeMasterText.text = String.Format(ModTranslation.getString("sheriffShots"), numChangeMaster - changeMasters.Count());
                        else
                            numChangeMasterText.text = "";
                    }
                    return local.currentTarget && PlayerControl.LocalPlayer.CanMove;
                },
                () => { changeMasterButton.Timer = changeMasterButton.MaxTimer; },
                ChangeMaster.getButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                hm,
                hm.UseButton,
                KeyCode.F
            )
            { buttonText = "Change!" };
            numChangeMasterText = GameObject.Instantiate(changeMasterButton.actionButton.cooldownTimerText, changeMasterButton.actionButton.cooldownTimerText.transform.parent);
            numChangeMasterText.text = "";
            numChangeMasterText.enableWordWrapping = false;
            numChangeMasterText.transform.localScale = Vector3.one * 0.5f;
            numChangeMasterText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
        }

        public static void SetButtonCooldowns()
        {
            changeMasterButton.MaxTimer = cooldown;
        }

        public static void Clear()
        {
            players = new List<ChangeMaster>();
            createCosmetics();
            if (AmongUsClient.Instance.AmHost)
            {
                setCurrentCosmetic();
            }
            changeMasters = new();
        }

        public static Sprite buttonSprite;
        public static Sprite getButtonSprite()
        {
            return change;
        }

        public static void playVoice(byte targetPlayerId, byte changeMasterId)
        {
            HudManager.Instance.StartCoroutine(CoPlayVoice(targetPlayerId, changeMasterId).WrapToIl2Cpp());
        }

        public static IEnumerator CoPlayVoice(byte targetPlayerId, byte changeMasterId)
        {
            var targetClip = getCosmetic().clip;
            var target = Helpers.playerById(targetPlayerId);
            var changeMaster = Helpers.playerById(changeMasterId);

            var targetAudioObject= new GameObject("targetAudioSource");
            targetAudioObject.transform.position = target.transform.position;
            AudioSource targetAudioSource = targetAudioObject.gameObject.GetComponent<AudioSource>();
            if (targetAudioSource == null)
            {
                targetAudioSource = targetAudioObject.gameObject.AddComponent<AudioSource>();
            }
            targetAudioSource.priority = 0;
            targetAudioSource.spatialBlend = 1;
            targetAudioSource.clip = targetClip;
            targetAudioSource.loop = false;
            targetAudioSource.playOnAwake = false;
            targetAudioSource.maxDistance = maxDistance;
            targetAudioSource.minDistance = minDistance;
            targetAudioSource.rolloffMode = AudioRolloffMode.Linear;
            targetAudioSource.PlayOneShot(targetClip);
            yield break;
        }
        public static void setCurrentCosmetic()
        {
            byte val = Convert.ToByte(rnd.Next(0, cosmetics.Count));
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetCosmetic, Hazel.SendOption.Reliable, -1);
            writer.Write(val);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.setCosmetic(val);
        }
    }
}
#endif
