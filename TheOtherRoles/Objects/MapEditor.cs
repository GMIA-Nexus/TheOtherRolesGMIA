using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TheOtherRoles.CustomGameModes;
using TheOtherRoles.MetaContext;
using TheOtherRoles.Modules;
using UnityEngine;

namespace TheOtherRoles.Objects
{
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
    class OptimizeMapPatch
    {
        public static void Postfix(ShipStatus __instance)
        {
            PolusOptimize.OptimizePolus();
            AirshipOptimize.OptimizeAirship();
        }
    }

    public class PolusOptimize
    {
        private static Vent CreateVent(SystemTypes room, string ventName, Vector3 position)
        {
            var referenceVent = ShipStatus.Instance.AllVents[0];
            Vent vent = UnityEngine.Object.Instantiate(referenceVent, ShipStatus.Instance.FastRooms[room].transform);
            vent.transform.position = position;
            vent.Left = null;
            vent.Right = null;
            vent.Center = null;
            vent.Id = ShipStatus.Instance.AllVents.Select(x => x.Id).Max() + 1; // Make sure we have a unique id

            var allVentsList = ShipStatus.Instance.AllVents.ToList();
            allVentsList.Add(vent);
            ShipStatus.Instance.AllVents = allVentsList.ToArray();

            vent.gameObject.SetActive(true);
            vent.name = ventName;
            vent.gameObject.name = ventName;
            var console = vent.GetComponent<VentCleaningConsole>();
            console.Room = room;
            console.ConsoleId = ShipStatus.Instance.AllVents.Length;

            var allConsolesList = ShipStatus.Instance.AllConsoles.ToList();
            allConsolesList.Add(console);
            ShipStatus.Instance.AllConsoles = allConsolesList.ToArray();

            return vent;
        }

        public static void OptimizePolus()
        {
            AddAdditionalVents();
            moveVital();
        }

        public static void AddAdditionalVents()
        {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started ||
                PropHuntGM.isPropHuntGM || GameOptionsManager.Instance.currentGameOptions.GameMode == AmongUs.GameOptions.GameModes.HideNSeek) return;

            // Polus Vents
            if (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 2 && CustomOptionHolder.additionalVents.getBool())
            {
                var vent1 = CreateVent(SystemTypes.Specimens, "SpecimenVent", new(36.54f, -21.77f, 1f));
                var vent2 = CreateVent(SystemTypes.Dropship, "DropshipVent", new(16.64f, -2.46f, 1f));
                var vent3 = CreateVent(SystemTypes.Admin, "OfficeVentNew", new(26.67f, -17.54f, 1f));
                vent1.Left = vent3;
                vent2.Center = vent3;
                vent3.Right = vent1;
                vent3.Left = vent2;
            }
        }

        public static void moveVital()
        {
            if (PropHuntGM.isPropHuntGM || GameOptionsManager.Instance.currentGameOptions.GameMode == AmongUs.GameOptions.GameModes.HideNSeek) return;
            if (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 2 && CustomOptionHolder.specimenVital.getBool())
            {
                var panel = GameObject.Find("panel_vitals");
                if (panel != null)
                {
                    var transform = panel.GetComponent<Transform>();
                    transform.SetPositionAndRotation(new(35.39f, -22.10f, 1.0f), transform.rotation);
                }
            }
        }
    }

    public class AirshipOptimize
    {
        static private SpriteLoader customMeetingSideSprite = SpriteLoader.FromResource("TheOtherRoles.Resources.ladder_bg.png", 100f);
        static private SpriteLoader customMeetingLadderSprite = SpriteLoader.FromResource("TheOtherRoles.Resources.ladder.png", 100f);

        public static void OptimizeAirship()
        {
            addLadder(GameOptionsManager.Instance.currentNormalGameOptions.MapId);
            optimizeMap(GameOptionsManager.Instance.currentNormalGameOptions.MapId);
        }

        public static void optimizeMap(int mapId)
        {
            if (!CustomOptionHolder.airshipOptimize.getBool()) return;
            if (mapId == 4)
            {
                var obj = ShipStatus.Instance.FastRooms[SystemTypes.GapRoom].gameObject;
                //昇降機右に影を追加
                OneWayShadows oneWayShadow = obj.transform.FindChild("Shadow").FindChild("LedgeShadow").GetComponent<OneWayShadows>();
                oneWayShadow.enabled = false;
                if (PlayerControl.LocalPlayer.Data.Role.IsImpostor) oneWayShadow.gameObject.SetActive(false);

                SpriteRenderer renderer;

                GameObject fance = new("ModFance")
                {
                    layer = LayerMask.NameToLayer("Ship")
                };
                fance.transform.SetParent(obj.transform);
                fance.transform.localPosition = new Vector3(4.2f, 0.15f, 0.5f);
                fance.transform.localScale = new Vector3(1f, 1f, 1f);
                fance.SetActive(true);
                var Collider = fance.AddComponent<EdgeCollider2D>();
                Collider.points = new Vector2[] { new(1.5f, -0.2f), new(-1.5f, -0.2f), new(-1.5f, 1.5f) };
                Collider.enabled = true;
                renderer = fance.AddComponent<SpriteRenderer>();
                renderer.sprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.AirshipFence.png", 100f);

                var panel = obj.transform.FindChild("panel_data");
                panel.localPosition = new Vector3(4.52f, -3.95f, 0.1f);
            }
        }

        public static void addLadder(int mapId)
        {
            if (mapId == 4 && CustomOptionHolder.airshipLadder.getBool())
            {
                Transform meetingRoom = ShipStatus.Instance.FastRooms[SystemTypes.MeetingRoom].transform;
                Transform gapRoom = ShipStatus.Instance.FastRooms[SystemTypes.GapRoom].transform;

                float diffX = (meetingRoom.position.x - gapRoom.transform.position.x) / 0.7f;
                float[] shadowX = [0f, 0f];

                //画像を更新する
                GameObject customRendererObj = new("meeting_custom");
                customRendererObj.transform.SetParent(meetingRoom);
                customRendererObj.transform.localPosition = new Vector3(9.58f, -2.86f, 4.8f);
                customRendererObj.transform.localScale = new Vector3(1f, 1f, 1f);
                customRendererObj.AddComponent<SpriteRenderer>().sprite = customMeetingSideSprite.GetSprite(); ;
                customRendererObj.layer = LayerMask.NameToLayer("Ship");

                //はしごを生成
                GameObject originalLadderObj = meetingRoom.FindChild("ladder_meeting").gameObject;
                GameObject ladderObj = UnityEngine.Object.Instantiate(meetingRoom.FindChild("ladder_meeting").gameObject, meetingRoom);
                ladderObj.name = "ladder_meeting_custom";
                ladderObj.transform.position += new Vector3(10.9f, 0);
                ladderObj.GetComponent<SpriteRenderer>().sprite = customMeetingLadderSprite.GetSprite();
                ladderObj.GetComponentsInChildren<Ladder>().Do(l => { l.Id = (byte)(l.Id + 8); ShipStatus.Instance.Ladders = new Il2CppReferenceArray<Ladder>(ShipStatus.Instance.Ladders.Append(l).ToArray()); });

                //MeetingRoomの当たり判定に手を加える
                var collider = meetingRoom.FindChild("Walls").GetComponents<EdgeCollider2D>().Where((c) => c.pointCount == 43).FirstOrDefault();
                Il2CppSystem.Collections.Generic.List<Vector2> colliderPosList = new();
                int index = 0;
                float tempX = 0f;
                float tempY = 0f;

                foreach (var p in collider!.points)
                {
                    if (index != 30) colliderPosList.Add(p);
                    if (index == 29) tempX = p.x;
                    if (index == 30)
                    {
                        tempX = (tempX + p.x) / 2f;
                        colliderPosList.Add(new Vector2(tempX, p.y));
                        colliderPosList.Add(new Vector2(tempX, -1.8067f));
                        colliderPosList.Add(new Vector2(p.x, -1.8067f));
                    }
                    index++;
                }
                collider.SetPoints(colliderPosList);

                //MeetingRoomの影に手を加える
                collider = meetingRoom.FindChild("Shadows").GetComponents<EdgeCollider2D>().Where((c) => c.pointCount == 46).FirstOrDefault();

                colliderPosList = new Il2CppSystem.Collections.Generic.List<Vector2>();
                index = 0;
                while (index <= 40)
                {
                    colliderPosList.Add(collider!.points[index]);
                    index++;
                }

                shadowX[0] = collider!.points[41].x;
                shadowX[1] = tempX = (collider.points[40].x + collider.points[41].x) / 2f;
                tempY = (collider.points[40].y + collider.points[41].y) / 2f;
                colliderPosList.Add(new Vector2(tempX, tempY));
                colliderPosList.Add(new Vector2(tempX, tempY - 2.56f));
                var newCollider = meetingRoom.FindChild("Shadows").gameObject.AddComponent<EdgeCollider2D>();
                newCollider.SetPoints(colliderPosList);

                colliderPosList = new Il2CppSystem.Collections.Generic.List<Vector2>();
                index = 41;
                while (index <= 45)
                {
                    if (index == 41) colliderPosList.Add(collider.points[41] - new Vector2(0, 2.56f));
                    colliderPosList.Add(collider.points[index]);
                    index++;
                }
                tempX = collider.points[41].x;
                collider.SetPoints(colliderPosList);

                //GapRoomの影に手を加える
                collider = gapRoom.FindChild("Shadow").GetComponents<EdgeCollider2D>().Where(x => Math.Abs(x.points[0].x + 6.2984f) < 0.1).FirstOrDefault();
                colliderPosList = new Il2CppSystem.Collections.Generic.List<Vector2>();
                index = 0;
                while (index <= 1)
                {
                    colliderPosList.Add(collider!.points[index]);
                    index++;
                }
                colliderPosList.Add(new Vector2(shadowX[0] + diffX, collider!.points[1].y));
                newCollider = gapRoom.FindChild("Shadow").gameObject.AddComponent<EdgeCollider2D>();
                newCollider.SetPoints(colliderPosList);
                colliderPosList = new Il2CppSystem.Collections.Generic.List<Vector2>();
                index = 2;
                colliderPosList.Add(new Vector2(shadowX[1] + diffX, collider.points[1].y));
                while (index <= 4)
                {
                    colliderPosList.Add(collider.points[index]);
                    index++;
                }
                collider.SetPoints(colliderPosList);

                AirshipStatus airship = ShipStatus.Instance.Cast<AirshipStatus>();
                airship.Ladders = new Il2CppReferenceArray<Ladder>(airship.GetComponentsInChildren<Ladder>());

                ladderObj.transform.GetChild(2).gameObject.SetActive(false);
                ladderObj.transform.GetChild(3).gameObject.SetActive(false);
            }
        }
    }

    public static class FungleAdditionalElectrical
    {
        public static void CreateElectrical()
        {
            if (!Helpers.isFungle() || !CustomOptionHolder.fungleElectrical.getBool())
                return;

            FungleShipStatus fungleShipStatus = ShipStatus.Instance.CastFast<FungleShipStatus>();
            SwitchSystem system = new();
            fungleShipStatus.Systems[SystemTypes.Electrical] = system.TryCast<ISystemType>();
            fungleShipStatus.Systems[SystemTypes.Sabotage].TryCast<SabotageSystemType>().specials.Add(system.TryCast<IActivatable>());
            List<PlayerTask> Tasks = ShipStatus.Instance.SpecialTasks.ToList();
            Tasks.Add(VanillaAsset.MapAsset[4].SpecialTasks.FirstOrDefault(x => x.TaskType == TaskTypes.FixLights));
            ShipStatus.Instance.SpecialTasks = new(Tasks.ToArray());

            Console console1 = GameObject.Instantiate(VanillaAsset.MapAsset[4].transform.FindChild("Storage/task_lightssabotage (cargo)"), fungleShipStatus.transform).GetComponent<Console>();
            console1.transform.localPosition = new(-16.2f, 7.67f, 0);
            console1.ConsoleId = 0;

            Console console2 = GameObject.Instantiate(VanillaAsset.MapAsset[0].transform.FindChild("Electrical/Ground/electric_frontset/SwitchConsole"), fungleShipStatus.transform).GetComponent<Console>();
            console2.transform.localPosition = new(-5.7f, -7.7f, -1.008f);
            console2.ConsoleId = 1;

            Console console3 = GameObject.Instantiate(console1, fungleShipStatus.transform);
            console3.transform.localPosition = new(21.48f, 4.27f, 0f);
            console3.ConsoleId = 2;
            //Agartha.MapLoader.Airship.transform.FindChild("Storage/task_lightssabotage (cargo)"), fungleShipStatus.transform).GetComponent<Console>();
            List<Console> Consoles = ShipStatus.Instance.AllConsoles.ToList();
            Consoles.Add(console1);
            Consoles.Add(console2);
            Consoles.Add(console3);
            ShipStatus.Instance.AllConsoles = Consoles.ToArray();
        }

        [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.Awake))]
        class MapBehaviourAwakePatch
        {
            public static void Postfix(MapBehaviour __instance)
            {
                if (!Helpers.isFungle() || !CustomOptionHolder.fungleElectrical.getBool())
                    return;

                MapRoom mapRoom = GameObject.Instantiate(VanillaAsset.MapAsset[4].MapPrefab.infectedOverlay.rooms.FirstOrDefault(x => x.room == SystemTypes.Electrical), __instance.infectedOverlay.transform);
                mapRoom.Parent = __instance.infectedOverlay;
                mapRoom.transform.localPosition = new(-0.83f, -1.8f, -1f);
                var buttons = __instance.infectedOverlay.allButtons.ToList();
                buttons.Add(mapRoom.GetComponentInChildren<ButtonBehavior>());
                __instance.infectedOverlay.allButtons = buttons.ToArray();
                var buttons2 = __instance.infectedOverlay.rooms.ToList();
                buttons2.Add(mapRoom);
                __instance.infectedOverlay.rooms = buttons2.ToArray();
            }
        }
    }

    public static class MapLoader
    {
        public static IEnumerator LoadMaps()
        {
            while (AmongUsClient.Instance == null)
                yield return null;

            EastAsianFontChanger.LoadFont();

            for (int i = 0; i < VanillaAsset.MapAsset.Length; i++)
            {
                if (i == 3) continue;
                var handle = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>(AmongUsClient.Instance.ShipPrefabs[i].RuntimeKey);
                yield return handle;
                VanillaAsset.MapAsset[i] = handle.Result.GetComponent<ShipStatus>();
            }

            EastAsianFontChanger.ReflectFallBackFont();
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.Awake))]
    public static class AmongUsClientAwakePatch
    {
        private static bool Loaded;

        public static void Prefix(AmongUsClient __instance)
        {
            if (Loaded)
                return;
            Loaded = true;

            Modules.CustomHats.CustomHatManager.LoadHats();
            VanillaAsset.LoadAssetsOnTitle();
            HelpMenu.Load();

            __instance.StartCoroutine(BepInEx.Unity.IL2CPP.Utils.Collections.CollectionExtensions.WrapToIl2Cpp(MapLoader.LoadMaps()));
        }
    }
}
