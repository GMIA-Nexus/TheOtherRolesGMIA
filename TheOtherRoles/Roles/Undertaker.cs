using System.Collections.Generic;
using System.Linq;
using Hazel;
using TheOtherRoles.Modules;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    [TORRPCHolder]
    public class Undertaker : RoleBase<Undertaker>
    {
        public static Color color = Palette.ImpostorRed;

        public Undertaker()
        {
            RoleId = roleId = RoleId.Undertaker;
        }

        static public IEnumerable<HelpSprite> GetHelpSprites()
        {
            yield return new HelpSprite(getDragButtonSprite(), "undertakerDragHint");
            yield return new HelpSprite(getDropButtonSprite(), "undertakerDropHint");
        }

        public static DeadBody DraggedBody;
        public static DeadBody TargetBody;
        public static bool CanDropBody;

        public static float speedDecrease = 0f;
        public static bool canVentWhileDragging = true;
        public static bool connectVent = true;

        public static Sprite dragButtonSprite;
        public static Sprite dropButtonSprite;

        public static RemoteProcess<Vector3> DropBody = RemotePrimitiveProcess.OfVector3("UndertakerDropBody", (message, _) =>
        {
            if (!DraggedBody) return;
            DraggedBody.transform.position = message;
            DraggedBody = null;
            TargetBody = null;
        });

        public override void FixedUpdate()
        {
            undertakerSetTarget();
            undertakerUpdate();
            undertakerCanDropTarget();
        }

        public override void OnDeath(PlayerControl killer = null)
        {
            if (PlayerControl.LocalPlayer == player && DraggedBody != null && killer != null)
                _ = new StaticAchievementToken("undertaker.another1");
        }

        void undertakerCanDropTarget()
        {
            if (player != PlayerControl.LocalPlayer) return;

            CanDropBody = false;

            if (DraggedBody == null) return;

            if (DraggedBody.enabled && Vector2.Distance(player.GetTruePosition(), DraggedBody.TruePosition) <= player.MaxReportDistance && !PhysicsHelpers.AnythingBetween(PlayerControl.LocalPlayer.GetTruePosition(), DraggedBody.TruePosition, Constants.ShipAndObjectsMask, false))                 CanDropBody = true;
        }

        void undertakerSetTarget()
        {
            if (player != PlayerControl.LocalPlayer) return;
            if (TargetBody != null)
                Helpers.SetDeadBodyOutline(TargetBody, null);

            if (DraggedBody == null)
            {
                TargetBody = Helpers.setDeadTarget(2f / 3f);
                Helpers.SetDeadBodyOutline(TargetBody, color);
            }
        }

        void undertakerUpdate()
        {
            var bodyComponent = DraggedBody;

            if (player == null || bodyComponent == null) return;

            var targetPos = player.transform.position;
            targetPos.z = targetPos.y / 1000f;

            if (player.inVent)
            {
                bodyComponent.transform.position = targetPos;
            }
            else
            {
                bodyComponent.transform.position = Vector3.Lerp(bodyComponent.transform.position, targetPos, 5f * Time.deltaTime);
            }

            if (player.Data.IsDead) {
                if (player.AmOwner)
                    DropBody.Invoke(bodyComponent.transform.position);
                return;
            }

            if (!player.AmOwner) return;

            Helpers.SetDeadBodyOutline(bodyComponent, Color.green);
        }

        public static RemoteProcess<byte> DragBody = RemotePrimitiveProcess.OfByte("UndertakerDragBody", (message, _) =>
        {
            if (allPlayers.Count == 0) return;
            var body = Object.FindObjectsOfType<DeadBody>().FirstOrDefault(b => b.ParentId == message);
            if (body == null) return;
            DraggedBody = body;
        });

        public static Sprite getDragButtonSprite()
        {
            if (dragButtonSprite) return dragButtonSprite;
            dragButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.DragButton.png", 115f);
            return dragButtonSprite;
        }

        public static Sprite getDropButtonSprite()
        {
            if (dropButtonSprite) return dropButtonSprite;
            dropButtonSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.DropButton.png", 115f);
            return dropButtonSprite;
        }

        public override void PostInit()
        {
            if (PlayerControl.LocalPlayer == player) EditVentInfo(true);
        }

        public override void OnFinishShipStatusBegin()
        {
            if (PlayerControl.LocalPlayer == player) EditVentInfo(true);
        }

        public override void ResetRole(bool isShifted)
        {
            if (PlayerControl.LocalPlayer == player)
            {
                EditVentInfo(false);
                DropBody.Invoke(DraggedBody?.transform.position ?? Vector3.zero);
            }
        }

        private static Vent GetVent(string name)
        {
            return ShipStatus.Instance.AllVents.FirstOrDefault(v => v.name == name);
        }

        private static void EditVentInfo(bool activate)
        {
            if (!connectVent) return;
            switch (GameOptionsManager.Instance.CurrentGameOptions.MapId)
            {
                case 0:
                    //Skeld
                    GetVent("NavVentNorth")!.Right = activate ? GetVent("NavVentSouth") : null;
                    GetVent("NavVentSouth")!.Right = activate ? GetVent("NavVentNorth") : null;

                    GetVent("ShieldsVent")!.Left = activate ? GetVent("WeaponsVent") : null;
                    GetVent("WeaponsVent")!.Center = activate ? GetVent("ShieldsVent") : null;

                    GetVent("ReactorVent")!.Left = activate ? GetVent("UpperReactorVent") : null;
                    GetVent("UpperReactorVent")!.Left = activate ? GetVent("ReactorVent") : null;

                    GetVent("SecurityVent")!.Center = activate ? GetVent("ReactorVent") : null;
                    GetVent("ReactorVent")!.Center = activate ? GetVent("SecurityVent") : null;

                    GetVent("REngineVent")!.Center = activate ? GetVent("LEngineVent") : null;
                    GetVent("LEngineVent")!.Center = activate ? GetVent("REngineVent") : null;

                    GetVent("AdminVent")!.Center = activate ? GetVent("MedVent") : null;
                    GetVent("MedVent")!.Center = activate ? GetVent("AdminVent") : null;

                    GetVent("CafeVent")!.Center = activate ? GetVent("WeaponsVent") : null;
                    GetVent("WeaponsVent")!.Center = activate ? GetVent("CafeVent") : null;

                    break;
                case 2:
                    //Polus
                    GetVent("CommsVent")!.Center = activate ? GetVent("ElecFenceVent") : null;
                    GetVent("ElecFenceVent")!.Center = activate ? GetVent("CommsVent") : null;

                    GetVent("ElectricalVent")!.Center = activate ? GetVent("ElectricBuildingVent") : null;
                    GetVent("ElectricBuildingVent")!.Center = activate ? GetVent("ElectricalVent") : null;

                    GetVent("ScienceBuildingVent")!.Right = activate ? GetVent("BathroomVent") : null;
                    GetVent("BathroomVent")!.Center = activate ? GetVent("ScienceBuildingVent") : null;

                    GetVent("SouthVent")!.Center = activate ? GetVent("OfficeVent") : null;
                    GetVent("OfficeVent")!.Center = activate ? GetVent("SouthVent") : null;

                    if (GetVent("SpecimenVent") != null)
                    {
                        GetVent("AdminVent")!.Center = activate ? GetVent("SpecimenVent") : null;

                        if (activate)
                        {
                            GetVent("SpecimenVent")!.Left = GetVent("AdminVent");
                            GetVent("SpecimenVent").Center = GetVent("OfficeVentNew");
                            GetVent("SpecimenVent")!.Right = GetVent("SubBathroomVent");
                        }
                        else
                        {
                            GetVent("SpecimenVent")!.Left = GetVent("OfficeVentNew");
                            GetVent("SpecimenVent")!.Center = null;
                            GetVent("SpecimenVent")!.Right = null;
                        }

                        GetVent("OfficeVentNew").Center = activate ? GetVent("OfficeVent") : null;
                        GetVent("SubBathroomVent")!.Center = activate ? GetVent("SpecimenVent") : null;

                        GetVent("ScienceBuildingVent").Center = activate ? GetVent("DropshipVent") : null;
                        GetVent("ElectricBuildingVent").Right = activate ? GetVent("DropshipVent") : null;

                        GetVent("DropshipVent").Left = activate ? GetVent("ElectricBuildingVent") : null;
                        GetVent("DropshipVent").Right = activate ? GetVent("ScienceBuildingVent") : null;
                    }

                    break;
                case 4:
                    //Airship
                    GetVent("VaultVent")!.Right = activate ? GetVent("GaproomVent1") : null;
                    GetVent("GaproomVent1")!.Center = activate ? GetVent("VaultVent") : null;

                    GetVent("EjectionVent")!.Right = activate ? GetVent("KitchenVent") : null;
                    GetVent("KitchenVent")!.Center = activate ? GetVent("EjectionVent") : null;

                    GetVent("HallwayVent1")!.Center = activate ? GetVent("HallwayVent2") : null;
                    GetVent("HallwayVent2")!.Center = activate ? GetVent("HallwayVent1") : null;

                    GetVent("GaproomVent2")!.Center = activate ? GetVent("RecordsVent") : null;
                    GetVent("RecordsVent")!.Center = activate ? GetVent("GaproomVent2") : null;

                    break;
                case 5:
                    //Fungle
                    GetVent("NorthWestJungleVent")!.Center = activate ? GetVent("SouthWestJungleVent") : null;
                    GetVent("SouthWestJungleVent")!.Center = activate ? GetVent("NorthWestJungleVent") : null;

                    GetVent("NorthEastJungleVent")!.Center = activate ? GetVent("SouthEastJungleVent") : null;
                    GetVent("SouthEastJungleVent")!.Center = activate ? GetVent("NorthEastJungleVent") : null;

                    GetVent("StorageVent")!.Center = activate ? GetVent("CommunicationsVent") : null;
                    GetVent("CommunicationsVent")!.Center = activate ? GetVent("StorageVent") : null;

                    break;
            }
        }

        public static void clearAndReload()
        {
            DraggedBody = null;
            TargetBody = null;

            speedDecrease = CustomOptionHolder.undertakerSpeedDecrease.getFloat();
            canVentWhileDragging = CustomOptionHolder.undertakerCanVentWhileDragging.getBool();
            connectVent = CustomOptionHolder.undertakerConnectsVent.getBool();
            players = [];
        }
    }
}
