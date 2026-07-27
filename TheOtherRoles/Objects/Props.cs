using Hazel;
using System.Collections.Generic;
using System.Linq;
using TheOtherRoles.Utilities;
using UnityEngine;
using System;
using static TheOtherRoles.TheOtherRoles;
using HarmonyLib;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using TMPro;
using TheOtherRoles.CustomGameModes;

namespace TheOtherRoles.Objects
{
    public class Props
    {
        public static List<Vector3> skeldSpawn = new()
        {
            new Vector3(-1.1028f, 4.9466f, 0.0f), // cafeteria
            new Vector3(9.119f, 1.4038f, 0.0f), // weapons
            new Vector3(6.5369f, -3.5533f, 0.0f), // O2
            new Vector3(16.7503f, -4.9249f, 0.0f), // navigation
            new Vector3(8.9308f, -11.9944f, 0.0f), // shields
            new Vector3(4.0746f, -15.8506f, 0.0f), // communications
            new Vector3(-2.1067f, -16.1015f, 0.0f), // storage
            new Vector3(-7.0197f, -8.9111f, 0.0f), // electric
            new Vector3(-17.0391f, -9.6947f, 0.0f), // lower-engine
            new Vector3(-20.9293f, -5.3046f, 0.0f), // reactor
            new Vector3(-13.3043f, -5.3046f, 0.0f), // security
            new Vector3(-15.3514f, 1.1165f, 0.0f), // upper-engine
            new Vector3(-8.946f, -3.6638f, 0.0f) // medbay
        };

        public static List<Vector3> miraSpawn = new()
        {
            new Vector3(-4.5314f, 3.1964f, 0.0f), // launchpad
            new Vector3(15.3814f, -1.0567f, 0.0f), // medbay
            new Vector3(6.1146f, 6.3599f, 0.0f), // decontimination
            new Vector3(9.5266f, 12.3496f, 0.0f), // reactor
            new Vector3(1.7446f, 10.7656f, 0.0f), // lab
            new Vector3(17.8588f, 14.4313f, 0.0f), // communications
            new Vector3(21.0038f, 20.4957f, 0.0f), //admin
            new Vector3(14.8338f, 20.6414f, 0.0f), //greenhouse left
            new Vector3(17.8238f, 23.7692f, 0.0f), //bottom right cross
            new Vector3(25.5716f, 1.8964f, 0.0f), //balcony 11
            new Vector3(25.1819f, -1.8909f, 0.0f),
            new Vector3(19.5687f, 2.1507f, 0.0f)
        };

        public static List<Vector3> polusSpawn = new()
        {
            new Vector3(16.4563f, -6.9233f, 0.0f), // dropship
            new Vector3(5.4774f, -9.7978f, 0.0f), // electric
            new Vector3(3.3069f, -19.4683f, 0.0f), // O2
            new Vector3(9.7891f, -20.5683f, 0.0f), // death valley
            new Vector3(12.5632f, -23.3549f, 0.0f), // weapons
            new Vector3(14.8053f, -13.9657f, 0.0f), // lab
            new Vector3(20.5668f, -12.0237f, 0.0f), // upper-decontimination
            new Vector3(28.7699f, -9.7874f, 0.0f), // specimen
            new Vector3(39.1572f, -9.9311f, 0.0f), // lower-decontimination
            new Vector3(36.4155f, -21.3363f, 0.0f), // office
            new Vector3(24.0103f, -24.829f, 0.0f), //comms table 11
            new Vector3(21.9853f, -19.0738f, 0.0f)
        };

        public static List<Vector3> airshipSpawn = new()
        {
            new Vector3(-13.5475f, -12.1318f, 0.0f),
            new Vector3(1.9533f, -12.1844f, 0.0f),
            new Vector3(24.6739f, -5.5841f, 0.0f),
            new Vector3(34.1575f, -0.4562f, 0.0f),
            new Vector3(25.7596f, 7.3206f, 0.0f),
            new Vector3(11.4754f, 8.6953f, 0.0f),
            new Vector3(4.0469f, 8.7285f, 0.0f),
            new Vector3(11.1479f, 15.8621f, 0.0f),
            new Vector3(-8.7757f, 6.7163f, 0.0f),
            new Vector3(-21.1178f, -1.3707f, 0.0f),
            new Vector3(-3.5572f, -1.0386f, 0.0f)
        };

        public static List<Vector3> fungleSpawn = new()
        {
                new Vector3(-7.4664f, 8.8714f, 0.0f),
                new Vector3(21.3894f, 13.616f, 0.0f),
                new Vector3(19.6109f, 7.4753f, 0.0f),
                new Vector3(15.0743f, -16.3425f, -0.0f),
                new Vector3(-15.7243f, -7.7109f, 0.0f),
                new Vector3(-16.3124f, -1.9259f, -0.0f),
                new Vector3(-4.1911f, -10.534f, -0.0f),
                new Vector3(-18.1464f, 6.9797f, -0.0f),
                new Vector3(20.5093f, -8.3817f, 0.0f)
        };

        public static List<Vector3> miraDoorway = new()
        {
            new Vector3(7.2639f, 14.1907f, 0.0f),
            new Vector3(6.2961f, 3.7184f, 0.0f),
            new Vector3(13.5582f, 4.2025f, 0.0f),
            new Vector3(13.6618f, 0.2669f, 0.0f),
            new Vector3(-4.3283f, 0.0749f, 0.0f),
            new Vector3(13.1907f, 7.2752f, 0.0f),
            new Vector3(22.21f, 7.2627f, 0.0f),
            new Vector3(25.5939f, -1.173f, 0.0f),
            new Vector3(22.0724f, -0.9444f, 0.0f),
            new Vector3(19.5806f, 1.2253f, 0.0f),
            new Vector3(17.8747f, 15.9618f, 0.0f)
        };

        public static List<Vector3> propPos = new();

        public class AccelTrap
        {
            private static Sprite accelTrapSprite;
            public GameObject accelTrap;
            public static Dictionary<PlayerControl, DateTime> acceled = new();

            public static List<AccelTrap> accels = new();

            public static Sprite getAccelSprite()
            {
                if (accelTrapSprite) return accelTrapSprite;
                accelTrapSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.AccelerateButton.png", 300);
                return accelTrapSprite;
            }

            public AccelTrap(Vector2 p)
            {
                accelTrap = new GameObject("AccelTrap");
                Vector3 position = new(p.x, p.y, p.y / 1000f + 0.01f);
                accelTrap.transform.position = position;
                accelTrap.transform.localPosition = position;
                var accelRenderer = accelTrap.AddComponent<SpriteRenderer>();
                accelRenderer.sprite = getAccelSprite();

                accelTrap.SetActive(true);
                accels.Add(this);
            }

            public static void clearAccelTrap()
            {
                foreach (AccelTrap acce in accels)
                {
                    if (acce.accelTrap == null) continue;
                    acce.accelTrap.SetActive(false);
                    UnityEngine.Object.Destroy(acce.accelTrap);
                }
                accels = new List<AccelTrap>();
                acceled = new Dictionary<PlayerControl, DateTime>();
            }

            public static List<Vector3> findAccelPos()
            {
                if (Helpers.isSkeld()) propPos = skeldSpawn;
                else if (Helpers.isMira()) propPos = miraSpawn;
                else if (Helpers.isPolus()) propPos = polusSpawn;
                else if (Helpers.isAirship()) propPos = airshipSpawn;
                else propPos = fungleSpawn;
                return propPos;
            }

            public static void placeAccelTrap()
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    int count = 0;
                    byte[] buff = new byte[(int)CustomOptionHolder.numAccelTraps.getFloat()];
                    while (count < (int)CustomOptionHolder.numAccelTraps.getFloat())
                    {
                        bool isDuplicated = false;
                        byte id = (byte)rnd.Next(findAccelPos().Count);

                        for (int i = 0; i < buff.Length; i++)
                        {
                            if (id == buff[i])
                            {
                                isDuplicated = true;
                                break;
                            }
                        }

                        if (!isDuplicated)
                        {
                            buff[count] = id;
                            count++;
                        }
                    }
                    
                    foreach (var item in buff)
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PlaceAccel, Hazel.SendOption.Reliable);
                        writer.Write(item);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.placeAccel(item);
                    }
                }
            }
        }

        public class DecelTrap
        {
            private static Sprite decelTrapSprite;
            public GameObject decelTrap;
            public static List<DecelTrap> decels = new();
            public static List<Vector3> decelPos;
            public bool isTriggered = false;
            public DateTime activateTime = DateTime.UtcNow;

            public static Dictionary<PlayerControl, DateTime> deceled = new();

            public DecelTrap(Vector2 p)
            {
                decelTrap = new GameObject("DecelTrap");

                Vector3 position = new(p.x, p.y, p.y / 1000f + 0.01f);
                decelTrap.transform.position = position;
                decelTrap.transform.localPosition = position;
                var decelRenderer = decelTrap.AddComponent<SpriteRenderer>();
                decelRenderer.sprite = getDecelSprite();

                decelTrap.SetActive(true);
                decels.Add(this);
            }

            public static Sprite getDecelSprite()
            {
                if (decelTrapSprite) return decelTrapSprite;
                decelTrapSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.DecelerateButton.png", 300);
                return decelTrapSprite;
            }

            public static void clearDecelTrap()
            {
                foreach (DecelTrap decel in decels)
                {
                    if (decel.decelTrap == null) continue;
                    decel.isTriggered = false;
                    decel.activateTime = DateTime.UtcNow;
                    decel.decelTrap.SetActive(false);
                    UnityEngine.Object.Destroy(decel.decelTrap);
                }
                decels = new List<DecelTrap>();
                deceled = new Dictionary<PlayerControl, DateTime>();
            }

            public static List<Vector3> findDecelPos()
            {
                if (Helpers.isMira()) return miraDoorway;
                var pos = new List<Vector3>();
                foreach (var door in DestroyableSingleton<ShipStatus>.Instance.AllDoors)
                {
                    var position = door.transform.position;
                    pos.Add(new Vector3(position.x, position.y - 0.2f, position.z - 50f));
                }
                decelPos = pos;

                return pos;
            }

            public static byte getId(DecelTrap decelTrap)
            {
                for (int i = 0; i < decels.Count; i++)
                {
                    if (decels[i] == decelTrap)
                        return (byte)i;
                }
                return 0;
            }

            public static void placeDecelTrap()
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    int count = 0;
                    byte[] buff = new byte[(int)CustomOptionHolder.numDecelTraps.getFloat()];
                    while (count < (int)CustomOptionHolder.numDecelTraps.getFloat())
                    {
                        bool isDuplicated = false;
                        byte id = (byte)rnd.Next(findDecelPos().Count);

                        for (int i = 0; i < buff.Length; i++)
                        {
                            if (id == buff[i])
                            {
                                isDuplicated = true;
                                break;
                            }
                        }

                        if (!isDuplicated)
                        {
                            buff[count] = id;
                            count++;
                        }
                    }

                    foreach (var item in buff)
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PlaceDecel, Hazel.SendOption.Reliable);
                        writer.Write(item);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.placeDecel(item);
                    }
                }
            }
        }

        public static void placeProps()
        {
            if (PropHuntGM.isPropHuntGM || GameOptionsManager.Instance.currentGameOptions.GameMode == AmongUs.GameOptions.GameModes.HideNSeek) return;
            AccelTrap.placeAccelTrap();
            DecelTrap.placeDecelTrap();
        }

        public static void clearProps()
        {
            AccelTrap.clearAccelTrap();
            DecelTrap.clearDecelTrap();
        }

        public static void clearAndReload()
        {
            clearProps();
            propPos = new();
        }

        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
        public static class PlayerPhysicsPropPatch
        {
            public static void Postfix(PlayerPhysics __instance)
            {
                if (__instance.AmOwner && __instance.myPlayer.CanMove && GameData.Instance)
                {
                    if (AccelTrap.acceled.ContainsKey(__instance.myPlayer))
                        __instance.body.velocity *= CustomOptionHolder.speedAcceleration.getFloat() + 1;
                    if (DecelTrap.deceled.ContainsKey(__instance.myPlayer))
                        __instance.body.velocity *= CustomOptionHolder.speedDeceleration.getFloat() + 1;
                }
            }
        }

        [RegisterInIl2Cpp]
        public class Proptip : MonoBehaviour
        {
            public Proptip(IntPtr ptr) : base(ptr)
            {
            }

            public GameObject ProptipObj { get; set; }
            public TextMeshPro ProptipTMP { get; set; }
            public RectTransform ProptipTransform { get; set; }
            public MeshRenderer ProptipRenderer { get; set; }
            public bool Enabled { get; set; }
            public string ProptipText { get; set; }

            private void Start()
            {
                Enabled = true;

                ProptipObj = new GameObject().DontDestroy();
                ProptipObj.layer = 5;

                ProptipTMP = ProptipObj.AddComponent<TextMeshPro>();
                ProptipTMP.fontSize = 1.7f;
                ProptipTMP.alignment = TextAlignmentOptions.BottomLeft;
                ProptipTMP.overflowMode = TextOverflowModes.Overflow;
                ProptipTMP.maskable = false;

                ProptipRenderer = ProptipObj.GetComponent<MeshRenderer>();
                ProptipRenderer.sortingOrder = 1000;

                ProptipTransform = ProptipObj.GetComponent<RectTransform>();
                ProptipObj.SetActive(false);
            }

            public void OnDisable()
            {
                if (ProptipObj == null) return;
                ProptipObj.SetActive(false);
            }

            public void OnDestroy()
            {
                if (ProptipObj == null) return;
                ProptipObj.SetActive(false);
                ProptipObj.Destroy();
            }

            public void LateUpdate()
            {
                ProptipTransform.sizeDelta = ProptipTMP.GetPreferredValues(ProptipText);
                ProptipTMP.text = "<color=#EEFFB3FF>" + ProptipText + "</color>";

                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                ProptipObj.transform.position = new Vector3(mousePosition.x + (ProptipTMP.renderedWidth / 2) + 0.1f, mousePosition.y + (ProptipTMP.renderedHeight * 1.2f));
            }

            public void FixedUpdate()
            {
                ProptipObj.SetActive(false);
            }

            private void OnMouseOver()
            {
                if (!Enabled) return;
                ProptipObj.SetActive(true);
            }
        }
    }
}
