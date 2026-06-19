using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using TheOtherRoles;
using Il2CppInterop.Runtime.InteropTypes.Arrays;


namespace TheOtherRoles.Objects
{
    public class Shrine
    {
        public static List<Shrine> allShrine = new();
        public static int counter = 0;
        public GameObject shrine;
        public CircleCollider2D collider;
        public Console console;
        public PassiveButton button;
        public static Sprite sprite;

        public static List<Tuple<SystemTypes, Vector3>> airshipLocations = new()
        {
            new Tuple<SystemTypes, Vector3>(SystemTypes.Cockpit, new Vector3(-23.2431f, -2.83f, -0.1f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.Security, new Vector3(5.7504f, -14.4644f, -0.1f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.Medical, new Vector3(29.0891f, -6.6173f, -0.1f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.CargoBay, new Vector3(37.3017f, -3.5528f, -0.1f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.Records, new Vector3(19.8591f, 8.7203f, -0.1f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.GapRoom, new Vector3(10.6362f, 9.4588f, -0.1f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.Showers, new Vector3(25.9642f, 0.1444f, -0.1f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.MainHall, new Vector3(6.2437f, 3.0711f, -0.1f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.Engine, new Vector3(-0.6959f, -0.1619f, -0.1f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.VaultRoom, new Vector3(-10.8024f, 12.415f, -0.1f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.MeetingRoom, new Vector3(4.1004f, 15.8256f, -0.1f)),
        };
        public static List<Tuple<SystemTypes, Vector3>> miraHQLocations = new()
        {
            new Tuple<SystemTypes, Vector3>(SystemTypes.Launchpad, new Vector3(-4.4405f, 4.1234f, -0.0024f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.MedBay, new Vector3(15.43f, -0.9059f, -0.0024f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.Comms, new Vector3(15.578f, 3.2167f, -0.0024f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.LockerRoom, new Vector3(9.1727f, 5.1125f, -0.0024f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.Laboratory, new Vector3(11.4326f, 10.3038f, -0.0024f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.Reactor, new Vector3(2.5f, 13.5628f, -0.0024f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.Office, new Vector3(13.5403f, 17.2491f, -0.0024f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.Admin, new Vector3(20.4127f, 17.5f, -0.0024f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.Greenhouse, new Vector3(20.4984f, 23.2426f, -0.0024f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.Storage, new Vector3(19.5887f, 2.3912f, -0.0024f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.Cafeteria, new Vector3(28.0491f, 0.1489f, 0.0024f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.Balcony, new Vector3(18.5589f, -1.6051f -0.0024f)),
        };
        public static List<Tuple<SystemTypes, Vector3>> skeldLocations = new()
        {
            new Tuple<SystemTypes, Vector3>(SystemTypes.Cafeteria, new Vector3(-1.0944f, 5.388f, -0.1f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.Weapons, new Vector3(9.4878f, 0.3132f, -0.1f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.LifeSupp, new Vector3(5.1896f, -4.6676f, -0.1f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.Nav, new Vector3(16.8183f, -5.9425f, -0.1f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.Shields, new Vector3(9.2f, -12.1821f, -0.1f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.Comms, new Vector3(2.3031f, -15.0232f, -0.1f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.Admin, new Vector3(4.9855f, -9.6806f, -0.1f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.Electrical, new Vector3(-8.0273f, -11.0667f, -0.1f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.Security, new Vector3(-13.552f, -6.6345f, -0.1f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.Reactor, new Vector3(-21.787f, -7.9899f, -0.1f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.MedBay, new Vector3(-8.9833f, -4.1209f, -0.1f)),
        };
        public static List<Tuple<SystemTypes, Vector3>> polusLocations = new()
        {
            new Tuple<SystemTypes, Vector3>(SystemTypes.Dropship, new Vector3(16.6431f, -5.7775f, -0.1f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.Outside, new Vector3(23.9383f, -2.4089f, -0.002f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.Outside, new Vector3(4.4645f, -3.3837f, -0.0034f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.Outside, new Vector3(32.5753f, -15.6324f, -0.1f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.Outside, new Vector3(18.0687f, -25.7161f, -0.0257f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.BoilerRoom, new Vector3(2.3273f, -24.3554f, -0.1f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.Electrical, new Vector3(7.4854f, -13.079f, -0.0131f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.Storage, new Vector3(20.6816f, -12.4833f, -0.0125f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.Laboratory, new Vector3(34.9777f, -9.425f, -0.1f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.Decontamination2, new Vector3(39.1127f, -15.6093f, -0.1f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.Decontamination3, new Vector3(27.8337f, -21.293f, -0.0213f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.Admin, new Vector3(22.0768f, -25.1466f, -0.0251f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.Comms, new Vector3(12.422f, -17.2601f, -0.1f)),
        };
        public static List<Tuple<SystemTypes, Vector3>> fungleLocations = new()
        {
            new Tuple<SystemTypes, Vector3>(SystemTypes.Storage, new Vector3(1.0139f, 4.3129f, 0.0135f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.Jungle, new Vector3(-9.4981f, -13.4451f, -0.0134f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.MiningPit, new Vector3(13.0518f, 9.8709f, 0.0135f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.Lookout, new Vector3(7.733f, 4.2049f, 0.0135f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.Greenhouse, new Vector3(8.8821f, -10.03f, -0.1f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.Reactor, new Vector3(21.4631f, -7.4165f, -0.1f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.Laboratory, new Vector3(-5.075f, -9.4086f, -0.1f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.RecRoom, new Vector3(-18.7677f, -0.3175f, -0.1f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.Dropship, new Vector3(-7.5892f, 9.5704f, 0.0096f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.UpperEngine, new Vector3(21.8108f, 3.1205f, 0.0031f)),
            new Tuple<SystemTypes, Vector3>(SystemTypes.Jungle, new Vector3(-0.6718f, -5.8665f, -0.1f))
        };

        public Shrine(SystemTypes room, Vector3 pos)
        {
            shrine = new GameObject($"shrine");
            shrine.transform.position = pos;
            console = shrine.AddComponent<Console>();
            console.checkWalls = true;
            console.usableDistance = 0.7f;
            var taskList = new TaskTypes[0].ToList();
            taskList.Add(TaskTypes.None);
            console.TaskTypes = taskList.ToArray();
            console.ValidTasks = new Il2CppReferenceArray<TaskSet>(0);
            console.Image = shrine.AddComponent<SpriteRenderer>();
            console.Image.sprite = sprite;
            console.Image.material = new Material(ShipStatus.Instance.AllConsoles[0].Image.material);
            console.onlySameRoom = false;
            console.ConsoleId = 100 + counter;
            console.Room = room;
            collider = shrine.AddComponent<CircleCollider2D>();
            collider.radius = 0.4f;
            collider.isTrigger = true;
            button = shrine.AddComponent<PassiveButton>();
            button.OnMouseOut = new UnityEngine.Events.UnityEvent();
            button.OnMouseOver = new UnityEngine.Events.UnityEvent();
            button._CachedZ_k__BackingField = -0.2f;
            button.CachedZ = -0.2f;
            button.OnClick.AddListener((UnityEngine.Events.UnityAction)Use);
            var consoleList = ShipStatus.Instance.AllConsoles.ToList();
            consoleList.Add(console);
            ShipStatus.Instance.AllConsoles = new Il2CppReferenceArray<Console>(consoleList.ToArray());
            allShrine.Add(this);
            counter += 1;
        }

        void Use()
        {
            console.Use();
        }
        public static void reset()
        {
            allShrine = new();
            counter = 0;
        }

        public static void activateShrines(int mapId)
        {
            List<Tuple<SystemTypes, Vector3>> locations;
            switch (mapId)
            {
                case 0:
                    locations = skeldLocations;
                    break;
                case 1:
                    locations = miraHQLocations;
                    break;
                case 2:
                    locations = polusLocations;
                    break;
                case 4:
                    locations = airshipLocations;
                    break;
                case 5:
                    locations = fungleLocations;
                    break;
                default:
                    locations = skeldLocations;
                    break;
            }
            locations.ForEach(x => new Shrine(x.Item1, x.Item2));
        }
    }
}
