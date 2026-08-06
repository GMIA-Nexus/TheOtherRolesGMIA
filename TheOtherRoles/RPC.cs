using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using Reactor.Utilities.Extensions;
using TheOtherRoles.CustomGameModes;
using TheOtherRoles.MetaContext;
using TheOtherRoles.Modules;
using TheOtherRoles.Objects;
using TheOtherRoles.Patches;
using TheOtherRoles.Roles;
using TheOtherRoles.Utilities;
using UnityEngine;
using static TheOtherRoles.GameHistory;
using static TheOtherRoles.HudManagerStartPatch;
using static TheOtherRoles.TheOtherRoles;
using static TheOtherRoles.TORMapOptions;

namespace TheOtherRoles
{
    public enum RoleId {
        Jester,
        Mayor,
        Portalmaker,
        Engineer,
        Sheriff,
        Deputy,
        Lighter,
        Godfather,
        Mafioso,
        Janitor,
        Detective,
        TimeMaster,
        Medic,
        Swapper,
        Seer,
        Sprinter,
        Morphling,
        Camouflager,
        Hacker,
        Tracker,
        Vampire,
        Snitch,
        Jackal,
        Sidekick,
        Eraser,
        FortuneTeller,
        Bait,
        Veteran,
        Sherlock,
        Spy,
        Trickster,
        Cleaner,
        Warlock,
        SecurityGuard,
        Arsonist,
        EvilGuesser,
        NiceGuesser,
        NiceWatcher,
        EvilWatcher, 
        BountyHunter,
        Vulture,
        Medium,
        Shifter, 
        Yasuna,
        TaskMaster,
        Teleporter,
        Jailor,
        EvilYasuna,
        Pelican,
        //Trapper,
        Lawyer, 
        //Prosecutor,
        Pursuer,
        Moriarty,
        PlagueDoctor,
        Akujo,
        Cupid,
        JekyllAndHyde,
        Fox,
        Immoralist,
        Witch,
        Assassin,
        Ninja, 
        NekoKabocha,
        Thief,
        SerialKiller,
        EvilTracker,
        MimicK,
        MimicA,
        BomberA,
        BomberB,
        EvilHacker,
        Undertaker,
        Trapper,
        Zephyr,
        Blackmailer,
        Opportunist,
        Yoyo,
        Doomsayer,
        Kataomoi,
        Collator,
        Yandere,
        Busker,
        Noisemaker,
        Archaeologist,
        SchrodingersCat,
        Madmate,
        Crewmate,
        Impostor,
        // Modifier ---
        Lover,
        //Bait, Bait is no longer a modifier
        Bloody,
        AntiTeleport,
        Tiebreaker,
        Sunglasses,
        Mini,
        Vip,
        Diseased,
        Invert,
        Multitasker,
        Radar,
        Chameleon,
        Armored,
        //Shifter
    }

    enum CustomRPC
    {
        // Main Controls

        ResetVaribles = 100,
        ShareOptions,
        ShareFilterOptions,
        ForceEnd,
        WorkaroundSetRoles,
        SetRole,
        SetModifier,
        VersionHandshake,
        UseUncheckedVent,
        UncheckedMurderPlayer,
        UncheckedCmdReportDeadBody,
        UncheckedExilePlayer,
        UncheckedSetTasks,
        DynamicMapOption,
        FinishShipStatusBegin,
        SetGameStarting,
        ShareGamemode,
        StopStart,

        // Role functionality

        ShieldedMurderAttempt = 130,
        TimeMasterRewindTime,
        ErasePlayerRoles,
        PlaceCamera,
        SealVent,
        GuesserShoot,
        SetBlanked,
        Invert,
        ThiefStealsRole,
        ShareRoom,

        // Gamemode
        SetGuesserGm,
        HuntedShield,
        HuntedRewindTime,

        // Other functionality
        ShareTimer,
        ShareGhostInfo,

        // GMIA Special functionality
        SerialKillerSuicide,
        YasunaSpecialVote,
        TaskMasterSetExTasks,
        TaskMasterUpdateExTasks,
        PlantBomb,
        ReleaseBomb,
        BomberKill,
        PlagueDoctorUpdateProgress,
        SetOddIsJekyll,
        ShareRealTasks,
        PlaceAccel,
        ActivateAccel,
        DeactivateAccel,
        PlaceDecel,
        ActivateDecel,
        UntriggerDecel,
        DeactivateDecel,
        EventKick,
        DraftModePickOrder,
        DraftModePick,
        SetLovers,
        ZephyrBlowCannon,
        ZephyrCheckCannon
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class TORRPCHolder : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Method)]
    public class CustomTORRPC : Attribute
    {

    }


    public class RPCInvoker
    {
        Action<MessageWriter> sender;
        Action localBodyProcess;
        int hash;
        public bool IsDummy { get; private set; }

        public RPCInvoker(int hash, Action<MessageWriter> sender, Action localBodyProcess)
        {
            this.hash = hash;
            this.sender = sender;
            this.localBodyProcess = localBodyProcess;
            this.IsDummy = false;
        }

        public RPCInvoker(Action localAction)
        {
            this.hash = 0;
            this.sender = null!;
            this.localBodyProcess = localAction;
            this.IsDummy = true;
        }

        public void Invoke(MessageWriter writer)
        {
            writer.Write(hash);
            sender.Invoke(writer);
            localBodyProcess.Invoke();
        }

        public void InvokeSingle()
        {
            if (IsDummy)
                localBodyProcess.Invoke();
            else
                RPCRouter.SendRpc("Invoker", hash, (writer) => sender.Invoke(writer), () => localBodyProcess.Invoke(), true);
        }

        public void InvokeLocal()
        {
            localBodyProcess.Invoke();
        }
    }

    public static class RPCRouter
    {
        public class RPCSection : IDisposable
        {
            public string Name;
            public void Dispose()
            {
                if (currentSection != this) return;

                currentSection = null;
                Debug.Log($"End Evacuating Rpcs ({Name}, Size = {evacuateds.Count})");

                CombinedRemoteProcess.CombinedRPC.Invoke([.. evacuateds]);
                evacuateds.Clear();
            }

            public RPCSection(string name = null)
            {
                Name = name ?? "Untitled";
                if (currentSection == null)
                {
                    currentSection = this;
                    Debug.Log($"Start Evacuating Rpcs ({Name})");
                }
            }
        }

        static public RPCSection CreateSection(string label = null) => new(label);

        static RPCSection currentSection = null;
        static List<RPCInvoker> evacuateds = [];
        public static void SendRpc(string name, int hash, Action<MessageWriter> sender, Action localBodyProcess, bool shouldBeReliable)
        {
            if (currentSection == null)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, 128, shouldBeReliable ? SendOption.Reliable : SendOption.None, -1);
                writer.Write(hash);
                sender.Invoke(writer);
                AmongUsClient.Instance.FinishRpcImmediately(writer);

                try
                {
                    localBodyProcess.Invoke();
                }
                catch (Exception ex)
                {
                    TheOtherRolesPlugin.Logger.LogError($"Error in RPC(Invoke: {name})" + ex.Message + ex.StackTrace);
                }
            }
            else
            {
                evacuateds.Add(new(hash, sender, localBodyProcess));
            }
        }
    }

    public class RemoteProcessBase
    {
        static public Dictionary<int, RemoteProcessBase> AllTORProcess = new();


        public int Hash { get; private set; } = -1;
        public string Name { get; private set; }

        public bool ShouldBeReliable { get; private init; } = true;


        public RemoteProcessBase(string name, bool shouldBeReliable)
        {
            Hash = name.ComputeConstantHash();
            Name = name;

            if (AllTORProcess.ContainsKey(Hash)) TheOtherRolesPlugin.Logger.LogWarning(name + " is duplicated. (" + Hash + ")");

            AllTORProcess[Hash] = this;
            ShouldBeReliable = shouldBeReliable;
        }

        static private void SwapMethodPointer(MethodInfo method0, MethodInfo method1)
        {
            unsafe
            {
                var functionPointer0 = method0.MethodHandle.Value.ToPointer();
                var functionPointer1 = method1.MethodHandle.Value.ToPointer();
                var functionShiftedPointer0 = *((int*)new IntPtr(((int*)functionPointer0 + 1)).ToPointer());
                var functionShiftedPointer1 = *((int*)new IntPtr(((int*)functionPointer1 + 1)).ToPointer());
                *((int*)new IntPtr(((int*)functionPointer0 + 1)).ToPointer()) = functionShiftedPointer1;
                *((int*)new IntPtr(((int*)functionPointer1 + 1)).ToPointer()) = functionShiftedPointer0;
            }
        }

        static Dictionary<string, RemoteProcess<object[]>> harmonyRpcMap = new();
        static private void WrapRpcMethod(Harmony harmony, MethodInfo method)
        {
            //元の静的メソッドをコピーしておく
            var copiedOriginal = harmony.Patch(method);

            //メソッド呼び出しのパラメータを取得
            var parameters = method.GetParameters();

            //RPCを定義し、登録

            List<(Action<MessageWriter, object> writer, Func<MessageReader, object> reader)> processList = new();
            if (!method.IsStatic) processList.Add(RemoteProcessAsset.GetProcess(method.DeclaringType!));
            processList.AddRange(parameters.Select(p => RemoteProcessAsset.GetProcess(p.ParameterType)));

            RemoteProcess<object[]> rpc = new(method.Name,
                (writer, args) =>
                {
                    for (int i = 0; i < processList.Count; i++) processList[i].writer(writer, args[i]);
                },
                (reader) =>
                {
                    return [.. processList.Select(p => p.reader(reader))];
                },
                (args, _) =>
                {
                    copiedOriginal.Invoke(null, args);
                }
                );
            harmonyRpcMap[method.DeclaringType!.FullName + "." + method.Name] = rpc;

            //静的メソッドをRPC呼び出しに変更
            static bool RpcPrefix(object __instance, object[] __args, MethodBase __originalMethod)
            {
                var name = __originalMethod.DeclaringType!.FullName + "." + __originalMethod.Name;
                if (!__originalMethod.IsStatic) __args = __args.Prepend(__instance!).ToArray();
                harmonyRpcMap[name].Invoke(__args);
                return false;
            }

            var prefixInfo = RpcPrefix;

            var newMethod = harmony.Patch(method, new HarmonyMethod(prefixInfo.Method));
        }

        static public void Load()
        {
            var types = Assembly.GetAssembly(typeof(RemoteProcessBase))?.GetTypes().Where((type) => type.IsDefined(typeof(TORRPCHolder))).ToList();
            if (types == null) return;

            foreach (var type in types)
            {
                RuntimeHelpers.RunClassConstructor(type.TypeHandle);
                var methods = type.GetMethods().Where(m => m.IsDefined(typeof(CustomTORRPC))).ToList();
                foreach (var method in methods) WrapRpcMethod(TheOtherRolesPlugin.Instance.Harmony, method);
            }
        }

        public virtual void Receive(MessageReader reader) { }
    }

    public static class RemoteProcessAsset
    {
        private static Dictionary<Type, (Action<MessageWriter, object>, Func<MessageReader, object>)> defaultProcessDic = new();

        static RemoteProcessAsset()
        {
            defaultProcessDic[typeof(byte)] = ((writer, obj) => writer.Write((byte)obj), (reader) => reader.ReadByte());
            defaultProcessDic[typeof(short)] = ((writer, obj) => writer.Write((short)obj), (reader) => reader.ReadInt16());
            defaultProcessDic[typeof(int)] = ((writer, obj) => writer.Write((int)obj), (reader) => reader.ReadInt32());
            defaultProcessDic[typeof(ulong)] = ((writer, obj) => writer.Write((ulong)obj), (reader) => reader.ReadUInt64());
            defaultProcessDic[typeof(float)] = ((writer, obj) => writer.Write((float)obj), (reader) => reader.ReadSingle());
            defaultProcessDic[typeof(bool)] = ((writer, obj) => writer.Write((bool)obj), (reader) => reader.ReadBoolean());
            defaultProcessDic[typeof(string)] = ((writer, obj) => writer.Write((string)obj), (reader) => reader.ReadString());
            defaultProcessDic[typeof(byte[])] = ((writer, obj) => writer.WriteBytesAndSize((byte[])obj), (reader) => reader.ReadBytesAndSize().ToArray());
            defaultProcessDic[typeof(int[])] = ((writer, obj) => { var ary = (int[])obj; writer.Write(ary.Length); for (int i = 0; i < ary.Length; i++) writer.Write(ary[i]); }, (reader) => { var ary = new int[reader.ReadInt32()]; for (int i = 0; i < ary.Length; i++) ary[i] = reader.ReadInt32(); return ary; });
            defaultProcessDic[typeof(float[])] = ((writer, obj) => { var ary = (float[])obj; writer.Write(ary.Length); for (int i = 0; i < ary.Length; i++) writer.Write(ary[i]); }, (reader) => { var ary = new float[reader.ReadInt32()]; for (int i = 0; i < ary.Length; i++) ary[i] = reader.ReadSingle(); return ary; });
            defaultProcessDic[typeof(string[])] = ((writer, obj) => { var ary = (string[])obj; writer.Write(ary.Length); for (int i = 0; i < ary.Length; i++) writer.Write(ary[i]); }, (reader) => { var ary = new string[reader.ReadInt32()]; for (int i = 0; i < ary.Length; i++) ary[i] = reader.ReadString(); return ary; });
            defaultProcessDic[typeof(Vector2)] = ((writer, obj) => { var vec = (Vector2)obj; writer.Write(vec.x); writer.Write(vec.y); }, (reader) => new Vector2(reader.ReadSingle(), reader.ReadSingle()));
            defaultProcessDic[typeof(Vector3)] = ((writer, obj) => { var vec = (Vector3)obj; writer.Write(vec.x); writer.Write(vec.y); writer.Write(vec.z); }, (reader) => new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
            defaultProcessDic[typeof(TranslatableTag)] = ((writer, obj) => writer.Write(((TranslatableTag)obj).Id), (reader) => TranslatableTag.ValueOf(reader.ReadInt32())!);
            defaultProcessDic[typeof(CommunicableTextTag)] = defaultProcessDic[typeof(TranslatableTag)];
        }

        static public (Action<MessageWriter, object>, Func<MessageReader, object>) GetProcess(Type type)
        {
            if (type.IsAssignableTo(typeof(Enum)))
                return defaultProcessDic[Enum.GetUnderlyingType(type)];
            return defaultProcessDic[type];
        }

        public static void GetMessageTreater<Parameter>(out Action<MessageWriter, Parameter> sender, out Func<MessageReader, Parameter> receiver)
        {
            Type paramType = typeof(Parameter);

            if (!typeof(Parameter).IsAssignableTo(typeof(ITuple))) throw new Exception("Can not generate sender and receiver for Non-tuple object.");

            int count = 0;


            List<(Action<MessageWriter, object>, Func<MessageReader, object>)> processList = new();
            while (true)
            {
                var field = paramType.GetField("Item" + (count + 1).ToString());
                if (field == null) break;

                processList.Add(GetProcess(field.FieldType));
                count++;
            }

            var processAry = processList.ToArray();
            var constructor = paramType.GetConstructors().FirstOrDefault(c => c.GetParameters().Length == processAry.Length);

            if (constructor == null) throw new Exception("Can not Tuple Constructor");

            sender = (writer, param) => {
                ITuple tuple = (param as ITuple)!;
                for (int i = 0; i < processAry.Length; i++) processAry[i].Item1.Invoke(writer, tuple[i]!);
            };
            receiver = (reader) => {
                return (Parameter)constructor.Invoke(processAry.Select(p => p.Item2.Invoke(reader)).ToArray());
            };
        }

    }
    public class RemoteProcess<Parameter> : RemoteProcessBase
    {
        public delegate void Process(Parameter parameter, bool isCalledByMe);

        private Action<MessageWriter, Parameter> Sender { get; set; }
        private Func<MessageReader, Parameter> Receiver { get; set; }
        private Process Body { get; set; }

        public RemoteProcess(string name, Action<MessageWriter, Parameter> sender, Func<MessageReader, Parameter> receiver, Process process, bool shouldBeReliable = true)
        : base(name, shouldBeReliable)
        {
            Sender = sender;
            Receiver = receiver;
            Body = process;
        }

        public RemoteProcess(string name, Process process, bool shouldBeReliable = true) : base(name, shouldBeReliable)
        {
            Body = process;
            RemoteProcessAsset.GetMessageTreater<Parameter>(out var sender, out var receiver);
            Sender = sender;
            Receiver = receiver;
        }


        public void Invoke(Parameter parameter)
        {
            RPCRouter.SendRpc(Name, Hash, (writer) => Sender(writer, parameter), () => Body.Invoke(parameter, true), ShouldBeReliable);
        }

        public RPCInvoker GetInvoker(Parameter parameter)
        {
            return new RPCInvoker(Hash, (writer) => Sender(writer, parameter), () => Body.Invoke(parameter, true));
        }

        public void LocalInvoke(Parameter parameter)
        {
            Body.Invoke(parameter, true);
        }

        public override void Receive(MessageReader reader)
        {
            try
            {
                Body.Invoke(Receiver.Invoke(reader), false);
            }
            catch (Exception ex)
            {
                TheOtherRolesPlugin.Logger.LogError($"Error in RPC(Received: {Name})" + ex.Message);
            }
        }
    }

    public static class RemotePrimitiveProcess
    {
        public static RemoteProcess<int> OfInteger(string name, RemoteProcess<int>.Process process) => new(name, (writer, message) => writer.Write(message), (reader) => reader.ReadInt32(), process);
        public static RemoteProcess<float> OfFloat(string name, RemoteProcess<float>.Process process) => new(name, (writer, message) => writer.Write(message), (reader) => reader.ReadSingle(), process);
        public static RemoteProcess<string> OfString(string name, RemoteProcess<string>.Process process) => new(name, (writer, message) => writer.Write(message), (reader) => reader.ReadString(), process);
        public static RemoteProcess<byte> OfByte(string name, RemoteProcess<byte>.Process process) => new(name, (writer, message) => writer.Write(message), (reader) => reader.ReadByte(), process);
        public static RemoteProcess<Vector2> OfVector2(string name, RemoteProcess<Vector2>.Process process) => new(name, (writer, message) => { writer.Write(message.x); writer.Write(message.y); }, (reader) => new(reader.ReadSingle(), reader.ReadSingle()), process);
        public static RemoteProcess<Vector3> OfVector3(string name, RemoteProcess<Vector3>.Process process) => new(name, (writer, message) => { writer.Write(message.x); writer.Write(message.y); writer.Write(message.z); }, (reader) => new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()), process);
        public static RemoteProcess<bool> OfBoolean(string name, RemoteProcess<bool>.Process process) => new(name, (writer, message) => writer.Write(message), (reader) => reader.ReadBoolean(), process);
    }

    [TORRPCHolder]
    public class CombinedRemoteProcess : RemoteProcessBase
    {
        public static CombinedRemoteProcess CombinedRPC = new();
        CombinedRemoteProcess() : base("CombinedRPC", true) { }

        public override void Receive(MessageReader reader)
        {
            int num = reader.ReadInt32();
            for (int i = 0; i < num; i++) AllTORProcess[reader.ReadInt32()].Receive(reader);
        }

        public void Invoke(params RPCInvoker[] invokers)
        {
            RPCRouter.SendRpc(Name, Hash, (writer) =>
            {
                writer.Write(invokers.Count(i => !i.IsDummy));
                foreach (var invoker in invokers)
                {
                    if (!invoker.IsDummy)
                        invoker.Invoke(writer);
                    else
                        invoker.InvokeLocal();
                }
            },
            () => { }, ShouldBeReliable);
        }
    }

    public class RemoteProcess : RemoteProcessBase
    {
        public delegate void Process(bool isCalledByMe);
        private Process Body { get; set; }
        public RemoteProcess(string name, Process process, bool shouldBeReliable = true)
        : base(name, shouldBeReliable)
        {
            Body = process;
        }

        public void Invoke()
        {
            RPCRouter.SendRpc(Name, Hash, (writer) => { }, () => Body.Invoke(true), ShouldBeReliable);
        }

        public RPCInvoker GetInvoker()
        {
            return new RPCInvoker(Hash, (writer) => { }, () => Body.Invoke(true));
        }

        public override void Receive(MessageReader reader)
        {
            try
            {
                Body.Invoke(false);
            }
            catch (Exception ex)
            {
                TheOtherRolesPlugin.Logger.LogError($"Error in RPC(Received: {Name})" + ex.Message);
            }
        }
    }

    public class DivisibleRemoteProcess<Parameter, DividedParameter> : RemoteProcessBase
    {
        public delegate void Process(DividedParameter parameter, bool isCalledByMe);

        private Func<Parameter, IEnumerator<DividedParameter>> Divider;
        private Action<MessageWriter, DividedParameter> DividedSender { get; set; }
        private Func<MessageReader, DividedParameter> Receiver { get; set; }
        private Process Body { get; set; }

        public DivisibleRemoteProcess(string name, Func<Parameter, IEnumerator<DividedParameter>> divider, Action<MessageWriter, DividedParameter> dividedSender, Func<MessageReader, DividedParameter> receiver, DivisibleRemoteProcess<Parameter, DividedParameter>.Process process)
        : base(name, true)
        {
            Divider = divider;
            DividedSender = dividedSender;
            Receiver = receiver;
            Body = process;
        }

        public DivisibleRemoteProcess(string name, Func<Parameter, IEnumerator<DividedParameter>> divider, DivisibleRemoteProcess<Parameter, DividedParameter>.Process process)
        : base(name, true)
        {
            Divider = divider;
            RemoteProcessAsset.GetMessageTreater<DividedParameter>(out var sender, out var receiver);
            DividedSender = sender;
            Receiver = receiver;
            Body = process;
        }

        public void Invoke(Parameter parameter)
        {
            void dividedSend(DividedParameter param)
            {
                RPCRouter.SendRpc(Name, Hash, (writer) => DividedSender(writer, param), () => Body.Invoke(param, true), false);
            }
            var enumerator = Divider.Invoke(parameter);
            while (enumerator.MoveNext()) dividedSend(enumerator.Current);
        }

        public void LocalInvoke(Parameter parameter)
        {
            var enumerator = Divider.Invoke(parameter);
            while (enumerator.MoveNext()) Body.Invoke(enumerator.Current, true);
        }

        public override void Receive(MessageReader reader)
        {
            try
            {
                Body.Invoke(Receiver.Invoke(reader), false);
            }
            catch (Exception ex)
            {
                TheOtherRolesPlugin.Logger.LogError($"Error in RPC(Received: {Name})" + ex.Message);
            }
        }
    }

    [TORRPCHolder]
    public static class RPCProcedure {

        // Main Controls

        public static void resetVariables() {
            Garlic.clearGarlics();
            JackInTheBox.clearJackInTheBoxes();
            AssassinTrace.clearTraces();
            Portal.clearPortals();
            Bloodytrail.resetSprites();
            MapBehaviourPatch.reset();
            MapBehaviourPatch.resetRealTasks();
            MapBehaviourPatch2.ResetIcons();
            SpawnInMinigamePatch.reset();
            MeetingOverlayHolder.clearAndReload();
            Props.clearAndReload();
            Silhouette.clearSilhouettes();
            //Trap.clearTraps();
            Trap.clearAllTraps();
            CustomNormalPlayerTask.reset();
            Shrine.reset();
            RolloverMessage.Initialize();
            VitalsStatePatch.ClearMissingPlayers();
            Antique.clearAllAntiques();
            clearAndReloadMapOptions();
            clearAndReloadRoles();
            clearGameHistory();
            setCustomButtonCooldowns();
            CustomButton.ReloadHotkeys();
            Helpers.toggleZoom(reset : true);
            GameStartManagerPatch.GameStartManagerUpdatePatch.startingTimer = 0;
            SurveillanceMinigamePatch.nightVisionOverlays = null;
            EventUtility.clearAndReload();
            RoleDraft.isRunning = false;
        }

        public static void HandleShareOptions(byte numberOfOptions, MessageReader reader) {            
            try {
                for (int i = 0; i < numberOfOptions; i++) {
                    uint optionId = reader.ReadPackedUInt32();
                    uint selection = reader.ReadPackedUInt32();
                    CustomOption option = CustomOption.options.First(option => option.id == (int)optionId);
                    option.updateSelection((int)selection, i == numberOfOptions - 1);
                }
                HelpMenu.OnUpdateOptions();
            } catch (Exception e) {
                TheOtherRolesPlugin.Logger.LogError("Error while deserializing options: " + e.Message);
            }
        }

        public static void HandleShareFilterOptions(byte numberOfOptions, MessageReader reader)
        {
            for (int i = 0; i < numberOfOptions; i++)
            {
                uint optionId = reader.ReadPackedUInt32();
                string filterText = reader.ReadString();
                var filterOpt = CustomOption.options.OfType<CustomFilterOption>().FirstOrDefault(o => o.id == (int)optionId);
                if (filterOpt == null) continue;

                filterOpt.filterSelection.Clear();
                if (!string.IsNullOrWhiteSpace(filterText))
                {
                    var names = filterText.Split(',').Where(s => !string.IsNullOrWhiteSpace(s));
                    foreach (var name in names)
                    {
                        if (byte.TryParse(name, out byte index))
                        {
                            var role = RoleInfo.allRoleInfos.FirstOrDefault(r => r.roleIndex == index);
                            if (role != null) filterOpt.filterSelection.Add(role);
                        }
                    }
                }
            }
            HelpMenu.OnUpdateOptions();
        }

        public static void forceEnd() {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (!player.Data.Role.IsImpostor)
                {
                    
                    GameData.Instance.GetPlayerById(player.PlayerId); // player.RemoveInfected(); (was removed in 2022.12.08, no idea if we ever need that part again, replaced by these 2 lines.) 
                    player.CoSetRole(RoleTypes.Crewmate, true);

                    player.MurderPlayer(player, MurderResultFlags.Succeeded);
                    player.Data.IsDead = true;
                }
            }
        }

        public static void shareGamemode(byte gm) {
            gameMode = (CustomGamemodes) gm;
            if (LobbyViewSettingsPatch.currentButtons != null)
                LobbyViewSettingsPatch.currentButtons?.ForEach(x => { if (x != null && x.gameObject != null) x?.gameObject?.Destroy(); });
            LobbyViewSettingsPatch.currentButtons?.Clear();
            LobbyViewSettingsPatch.currentButtonTypes?.Clear();
        }

        public static void stopStart(byte playerId)
        {
            if (!CustomOptionHolder.anyPlayerCanStopStart.getBool())
                return;
            SoundManager.Instance.StopSound(GameStartManager.Instance.gameStartSound);
            if (AmongUsClient.Instance.AmHost)
            {
                GameStartManager.Instance.ResetStartState();
                PlayerControl.LocalPlayer.RpcSendChat(string.Format(ModTranslation.getString("playerStopGameStartText"), Helpers.playerById(playerId).Data.PlayerName));
            }
        }

        public static void workaroundSetRoles(byte numberOfRoles, MessageReader reader)
        {
                for (int i = 0; i < numberOfRoles; i++)
                {                   
                    byte playerId = (byte) reader.ReadPackedUInt32();
                    byte roleId = (byte) reader.ReadPackedUInt32();
                    try {
                        setRole(roleId, playerId);
                    } catch (Exception e) {
                        TheOtherRolesPlugin.Logger.LogError("Error while deserializing roles: " + e.Message);
                    }
            }
            
        }

        public static void setRole(byte roleId, byte playerId) {
            PlayerControl.AllPlayerControls.GetFastEnumerator().ToArray().DoIf(
                x => x.PlayerId == playerId,
                x => x.setRole((RoleId)roleId)
            );
        }

        public static void setModifier(byte modifierId, byte playerId, byte flag) {
            PlayerControl player = Helpers.playerById(playerId); 
            switch ((RoleId)modifierId) {
                case RoleId.Bloody:
                    Bloody.bloody.Add(player);
                    break;
                case RoleId.AntiTeleport:
                    AntiTeleport.antiTeleport.Add(player);
                    break;
                case RoleId.Tiebreaker:
                    Tiebreaker.tiebreaker = player;
                    break;
                case RoleId.Sunglasses:
                    Sunglasses.sunglasses.Add(player);
                    break;
                case RoleId.Mini:
                    Mini.mini = player;
                    break;
                case RoleId.Vip:
                    Vip.vip.Add(player);
                    break;
                case RoleId.Invert:
                    Invert.invert.Add(player);
                    break;
                case RoleId.Chameleon:
                    Chameleon.chameleon.Add(player);
                    break;
                case RoleId.Armored:
                    Armored.armored = player;
                    break;
                case RoleId.Multitasker:
                    Multitasker.multitasker.Add(player);
                    break;
                case RoleId.Diseased:
                    Diseased.diseased.Add(player);
                    break;
                case RoleId.Radar:
                    Radar.radar = player;
                    break;
                case RoleId.Madmate:
                    Madmate.madmate.Add(player);
                    break;
                //case RoleId.Shifter:
                    //Shifter.shifter = player;
                    //break;
            }
        }

        public static void setLovers(byte playerId1, byte playerId2)
        {
            Lovers.addCouple(Helpers.playerById(playerId1), Helpers.playerById(playerId2));
        }

        public static void versionHandshake(int major, int minor, int build, int revision, Guid guid, int clientId, string subVer, bool isAndroid) {
            System.Version ver;
            if (revision < 0) 
                ver = new System.Version(major, minor, build);
            else 
                ver = new System.Version(major, minor, build, revision);
            GameStartManagerPatch.playerVersions[clientId] = new GameStartManagerPatch.PlayerVersion(ver, subVer, isAndroid, guid);
        }

        public static void useUncheckedVent(int ventId, byte playerId, byte isEnter) {
            PlayerControl player = Helpers.playerById(playerId);
            if (player == null) return;
            // Fill dummy MessageReader and call MyPhysics.HandleRpc as the corountines cannot be accessed
            MessageReader reader = new();
            byte[] bytes = BitConverter.GetBytes(ventId);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            reader.Buffer = bytes;
            reader.Length = bytes.Length;

            JackInTheBox.startAnimation(ventId);
            player.MyPhysics.HandleRpc(isEnter != 0 ? (byte)19 : (byte)20, reader);
        }

        public static void uncheckedMurderPlayer(byte sourceId, byte targetId, byte showAnimation) {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;
            PlayerControl source = Helpers.playerById(sourceId);
            PlayerControl target = Helpers.playerById(targetId);
            if (source != null && target != null)
            {
                if (showAnimation == 0) KillAnimationCoPerformKillPatch.hideNextAnimation = true;
                source.MurderPlayer(target, MurderResultFlags.Succeeded);
            }
        }

        public static void uncheckedCmdReportDeadBody(byte sourceId, byte targetId) {
            PlayerControl source = Helpers.playerById(sourceId);
            var t = targetId == Byte.MaxValue ? null : Helpers.playerById(targetId).Data;
            if (source != null) source.ReportDeadBody(t);
        }

        public static void uncheckedExilePlayer(byte targetId) {
            PlayerControl target = Helpers.playerById(targetId);
            if (target != null) {
                target.Exiled();
            }
        }

        public static void uncheckedSetTasks(byte playerId, byte[] taskTypeIds)
        {
            var player = Helpers.playerById(playerId);
            player.clearAllTasks();

            player.Data.SetTasks(taskTypeIds);
        }

        public static void dynamicMapOption(byte mapId) {
           GameOptionsManager.Instance.currentNormalGameOptions.MapId = mapId;
        }

        public static void finishShipStatusBegin()
        {
            HudManager.Instance.StartCoroutine(Effects.Lerp(1f, new Action<float>((p) =>
            {
                if (p == 1f)
                {
                    Role.allRoles.DoIf(x => x.player == PlayerControl.LocalPlayer, x => x.OnFinishShipStatusBegin());
                    ShipStatusPatch.commonTasks.Clear();
                    foreach (var task in PlayerControl.LocalPlayer.myTasks) {
                        if (ShipStatus.Instance.CommonTasks.Any(x => x.TaskType == task.TaskType)) {
                            ShipStatusPatch.commonTasks.Add(task.TaskType);
                            TheOtherRolesPlugin.Logger.LogMessage($"Added {task.TaskType.ToString()} for common task");
                        }
                    }

                    if (Madmate.hasTasks && Madmate.madmate.Any(x => x.PlayerId == PlayerControl.LocalPlayer.PlayerId)) {
                        PlayerControl.LocalPlayer.generateAndAssignTasks(Madmate.commonTasks, Madmate.shortTasks, Madmate.longTasks);
                    }

                    PlayerControl.AllPlayerControls.GetFastEnumerator().DoIf(x => !TORGameManager.Instance.RoleHistory.Any(history => history.PlayerId == x.PlayerId),
                        x => TORGameManager.Instance.RecordRoleHistory(x));

                    ShipStatus.Instance.AllCameras.Do
                    (
                        x =>
                        {
                            if (x.CamName == "South") x.NewName = StringNames.CamSouth;
                            else if (x.CamName == "Central") x.NewName = StringNames.CamCentral;
                            else if (x.CamName == "Northeast") x.NewName = StringNames.CamNortheast;
                            else if (x.CamName == "Northwest") x.NewName = StringNames.CamNorthwest;
                            else if (x.CamName == "Southwest") x.NewName = StringNames.CamSouthwest;
                            else if (x.CamName == "East") x.NewName = StringNames.CamEast;
                        }
                    );

                    Mini.timeOfGrowthStart = DateTime.UtcNow;
                }
            })));
        }

        public static void setGameStarting() {
            GameStartManagerPatch.GameStartManagerUpdatePatch.startingTimer = 5f;
        }

        // Role functionality

        public static RemoteProcess<(byte playerId, byte cleaningPlayerId)> CleanBody = new("CleanBody",
            (message, _) =>
            {
                if (Medium.futureDeadBodies != null && !Busker.players.Any(x => x.player && x.player.PlayerId == message.playerId && !x.pseudocideComplete)) {
                    var deadBody = Medium.futureDeadBodies.Find(x => x.Item1.player.PlayerId == message.playerId).Item1;
                    if (deadBody != null) deadBody.wasCleaned = true;
                }
                PlayerControl player = Helpers.playerById(message.playerId);
                PlayerControl cleanPlayer = Helpers.playerById(message.cleaningPlayerId);

                DeadBody[] array = UnityEngine.Object.FindObjectsOfType<DeadBody>();
                for (int i = 0; i < array.Length; i++) {
                    if (GameData.Instance.GetPlayerById(array[i].ParentId).PlayerId == message.playerId) {
                        UnityEngine.Object.Destroy(array[i].gameObject);
                    }     
                }
                TORGameManager.Instance?.GameStatistics.RecordEvent(new(GameStatistics.EventVariation.CleanBody, message.cleaningPlayerId, 1 << message.playerId)
                {
                    RelatedTag =
                    cleanPlayer.isRole(RoleId.Vulture) ? EventDetail.Eat : EventDetail.Clean
                });
                if (cleanPlayer.isRole(RoleId.Vulture)) {
                    var vulture = Vulture.getRole(cleanPlayer);
                    vulture.eatenBodies++;
                    if (vulture.eatenBodies == Vulture.vultureNumberToWin) {
                        vulture.triggerVultureWin = true;
                    }
                }

                VitalsStatePatch.AddMissingPlayer(player.Data);
            });

        public static RemoteProcess<byte> RpcRevive = RemotePrimitiveProcess.OfByte("ModRpcRevive", (message, _) =>
        {
            var player = Helpers.playerById(message);
            if (player == null) return;
            player.Revive();
            DeadBody[] array = UnityEngine.Object.FindObjectsOfType<DeadBody>();
            for (int i = 0; i < array.Length; i++)
            {
                if (GameData.Instance.GetPlayerById(array[i].ParentId).PlayerId == message)
                {
                    UnityEngine.Object.Destroy(array[i].gameObject);
                }
            }
            VitalsStatePatch.RemoveMissingPlayer(player.Data);
        });

        public static void timeMasterRewindTime(byte playerId) {
            PlayerControl player = Helpers.playerById(playerId);
            var timeMaster = TimeMaster.getRole(player);
            timeMaster.shieldActive = false; // Shield is no longer active when rewinding
            SoundEffectsManager.stop("timemasterShield");  // Shield sound stopped when rewinding
            if(PlayerControl.LocalPlayer == player) {
                resetTimeMasterButton();
                _ = new StaticAchievementToken("timeMaster.challenge");
                _ = new StaticAchievementToken("timeMaster.challenge2");
            }
            FastDestroyableSingleton<HudManager>.Instance.FullScreen.color = new Color(0f, 0.5f, 0.8f, 0.3f);
            FastDestroyableSingleton<HudManager>.Instance.FullScreen.enabled = true;
            FastDestroyableSingleton<HudManager>.Instance.FullScreen.gameObject.SetActive(true);
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(TimeMaster.rewindTime / 2, new Action<float>((p) => {
                if (p == 1f) FastDestroyableSingleton<HudManager>.Instance.FullScreen.enabled = false;
            })));

            if (!TimeMaster.exists || PlayerControl.LocalPlayer == player) return; // Time Master himself does not rewind

            TimeMaster.isRewinding = true;

            if (MapBehaviour.Instance)
                MapBehaviour.Instance.Close();
            if (Minigame.Instance)
                Minigame.Instance.ForceClose();
            PlayerControl.LocalPlayer.moveable = false;
        }

        public static void shieldedMurderAttempt(byte medicId) {
            var medic = Medic.getRole(Helpers.playerById(medicId));
            if (medic == null || medic.shielded == null) return;
            
            bool isShieldedAndShow = medic.shielded == PlayerControl.LocalPlayer && Medic.showAttemptToShielded;
            isShieldedAndShow = isShieldedAndShow && (medic.meetingAfterShielding || !Medic.showShieldAfterMeeting);  // Dont show attempt, if shield is not shown yet
            bool isMedicAndShow = medic.player == PlayerControl.LocalPlayer && Medic.showAttemptToMedic;

            if (isShieldedAndShow || isMedicAndShow || Helpers.shouldShowGhostInfo()) Helpers.showFlash(Palette.ImpostorRed, duration: 0.5f, ModTranslation.getString("medicInfo"));
        }

        public static void shifterShift(byte targetId)
        {
            PlayerControl oldShifter = Shifter.allPlayers.FirstOrDefault();
            PlayerControl player = Helpers.playerById(targetId);
            if (player == null || oldShifter == null) return;

            Shifter.futureShift = null;
            if (PlayerControl.LocalPlayer == oldShifter && Shifter.isNeutral) _ = new StaticAchievementToken("corruptedShifter.common1");

            // Suicide (exile) when impostor or impostor variants
            if (!Shifter.isNeutral && (player.Data.Role.IsImpostor || Helpers.isNeutral(player) || Madmate.madmate.Any(x => x.PlayerId == player.PlayerId) || CreatedMadmate.createdMadmate.Any(x => x.PlayerId == player.PlayerId)))
            {
                if (!oldShifter.Data.IsDead)
                {
                    oldShifter.Exiled();
                    overrideDeathReasonAndKiller(oldShifter, DeadPlayer.CustomDeathReason.Shift, player);
                }
                if (oldShifter == Lawyer.target && AmongUsClient.Instance.AmHost)
                {
                    foreach (var lawyer in Lawyer.allPlayers)
                    {
                        Lawyer.PromoteToPursuer.Invoke(lawyer.PlayerId);
                    }
                }
                if (PlayerControl.LocalPlayer == oldShifter) {
                    Shifter.niceShifterAcTokenChallenge.Value.shiftId = targetId;
                }
                return;
            }

            if (!Shifter.isNeutral)
            {
                if (PlayerControl.LocalPlayer == oldShifter)
                    _ = new StaticAchievementToken("niceShifter.common1");
            }
            else
            {
                if (PlayerControl.LocalPlayer == player && player.isRole(RoleId.Sidekick))
                    _ = new StaticAchievementToken("combination.2.corruptedShifter.sidekick.challenge");
            }

            Shifter.eraseRole(oldShifter);

            // Switch shield
            if (Shifter.shiftsMedicShield)
            {
                if (Medic.IsShielded(player)) {
                    foreach (var medic in Medic.GetMedic(player)) {
                         medic.shielded = oldShifter;
                    }
                }
                else if (Medic.IsShielded(oldShifter)){
                    foreach (var medic in Medic.GetMedic(oldShifter)) {
                         medic.shielded = player;
                    }
                }
            }

            if (Madmate.madmate.Any(x => x.PlayerId == player.PlayerId))
            {
                Madmate.madmate.Add(oldShifter);
                Madmate.madmate.Remove(player);
            }
            if (CreatedMadmate.createdMadmate.Any(x => x.PlayerId == player.PlayerId))
            {
                CreatedMadmate.createdMadmate.Add(oldShifter);
                CreatedMadmate.createdMadmate.Remove(player);
            }

            if (Shifter.shiftModifiers)
            {
                Lovers.swapLovers(oldShifter, player);
                // Switch Anti-Teleport
                if (AntiTeleport.antiTeleport.Any(x => x.PlayerId == player.PlayerId))
                {
                    AntiTeleport.antiTeleport.Add(oldShifter);
                    AntiTeleport.antiTeleport.Remove(player);
                }
                // Switch Bloody
                if (Bloody.bloody.Any(x => x.PlayerId == player.PlayerId))
                {
                    Bloody.bloody.Add(oldShifter);
                    Bloody.bloody.Remove(player);
                }
                // Switch Mini
                if (Mini.mini == player) Mini.mini = oldShifter;
                // Switch Tiebreaker
                if (Tiebreaker.tiebreaker == player) Tiebreaker.tiebreaker = oldShifter;
                // Switch Chameleon
                if (Chameleon.chameleon.Any(x => x.PlayerId == player.PlayerId))
                {
                    Chameleon.chameleon.Add(oldShifter);
                    Chameleon.chameleon.Remove(player);
                    Chameleon.removeChameleonFully(player);
                }
                // Switch Sunglasses
                if (Sunglasses.sunglasses.Any(x => x.PlayerId == player.PlayerId))
                {
                    Sunglasses.sunglasses.Add(oldShifter);
                    Sunglasses.sunglasses.Remove(player);
                }
                if (Vip.vip.Any(x => x.PlayerId == player.PlayerId))
                {
                    Vip.vip.Add(oldShifter);
                    Vip.vip.Remove(player);
                }
                if (Invert.invert.Any(x => x.PlayerId == player.PlayerId))
                {
                    Invert.invert.Add(oldShifter);
                    Invert.invert.Remove(player);
                }
                if (Multitasker.multitasker.Any(x => x.PlayerId == player.PlayerId))
                {
                    Multitasker.multitasker.Add(oldShifter);
                    Multitasker.multitasker.Remove(player);
                }
                if (Diseased.diseased.Any(x => x.PlayerId == player.PlayerId))
                {
                    Diseased.diseased.Add(oldShifter);
                    Diseased.diseased.Remove(player);
                }
                if (Armored.armored == player) Armored.armored = oldShifter;
                if (Radar.radar == player) Radar.radar = oldShifter;
            }

            if ((player.Data.Tasks == null || player.Data.Tasks?.Count == 0 || player.isRole(RoleId.Fox)) && !player.Data.IsDead && PlayerControl.LocalPlayer == player)
                player.generateNormalTasks();

            player.swapRoles(oldShifter);

            if (Lawyer.target == player) Lawyer.target = oldShifter;
            if (Kataomoi.target == player) Kataomoi.target = oldShifter;
            if (Yandere.target == player) Yandere.target = oldShifter;

            if (Shifter.isNeutral)
            {
                player.setRole(RoleId.Shifter);
                Shifter.pastShifters.Add(oldShifter.PlayerId);
                if (player.Data.Role.IsImpostor)
                {
                    player.FastSetRole(RoleTypes.Crewmate);
                    oldShifter.FastSetRole(RoleTypes.Impostor);
                }
            }

            if (!oldShifter.Data.IsDead && oldShifter.isRole(RoleId.Fox))
            {
                oldShifter.clearAllTasks();
                List<byte> taskIdList = [];
                Shrine.allShrine.ForEach(shrine => taskIdList.Add((byte)shrine.console.ConsoleId));
                taskIdList.Shuffle();
                var cpt = new CustomNormalPlayerTask("foxTaskStay", Il2CppType.Of<FoxTask>(), Fox.numTasks, [.. taskIdList], Shrine.allShrine.Find(x => x.console.ConsoleId == taskIdList.ToArray()[0]).console.Room, true);
                cpt.addTaskToPlayer(oldShifter.PlayerId);
            }

            // Set cooldowns to max for both players
            if (PlayerControl.LocalPlayer == oldShifter || PlayerControl.LocalPlayer == player)
                CustomButton.ResetAllCooldowns();

            TORGameManager.Instance?.RecordRoleHistory(oldShifter);
            TORGameManager.Instance?.RecordRoleHistory(player);
        }

        public static void plagueDoctorProgress(byte targetId, float progress)
        {
            if (PlagueDoctor.allPlayers.Count <= 0) return;
            PlagueDoctor.progress[targetId] = progress;
        }

        public static RemoteProcess<Vector2> PlaceGarlic = RemotePrimitiveProcess.OfVector2("PlaceGarlic", (message, _) => new Garlic(message));

        public static RemoteProcess<(byte playerId, string achievement)> ShareAchievement = new("ShareAchievement", (message, _) =>
        {
            PlayerControl player = Helpers.playerById(message.playerId);
            if (TORGameManager.Instance != null && player != null)
            {
                var titleShower = Helpers.playerById(message.playerId).GetTitleShower();
                TORGameManager.Instance.TitleMap[message.playerId] = titleShower.SetAchievement(message.achievement);
            }
        }, false);

        public static RemoteProcess RpcRequireHandShake = new("RequireHandShake", (isCalledByMe) =>
        {
            var ach = TORAchievementManager.MyTitle?.Id ?? "-";
            ShareAchievement.Invoke((PlayerControl.LocalPlayer.PlayerId, ach));
        }, false);

        public static RemoteProcess<(byte playerId, byte channelType, string message)> SendChatToChannel = new("SendCahtToChanel", (message, _) =>
        {
            var player = Helpers.playerById(message.playerId);
            if (player == null) return;
            var channel = (ChatCommands.ChannelType)message.channelType;
            switch (channel)
            {
                case ChatCommands.ChannelType.Default:
                    break;
                case ChatCommands.ChannelType.Impostor:
                    if (Helpers.shouldShowGhostInfo() || PlayerControl.LocalPlayer.Data.Role.IsImpostor)
                    {
                        ChatCommands.CurrentChatType = ChatCommands.ChatTypes.ImpostorChat;
                        HudManager.Instance.Chat.AddChat(player, message.message);
                    }
                    break;
                case ChatCommands.ChannelType.Lover:
                    if (player == PlayerControl.LocalPlayer.getPartner() || player == PlayerControl.LocalPlayer || Helpers.shouldShowGhostInfo())
                    {
                        ChatCommands.CurrentChatType = ChatCommands.ChatTypes.LoverChat;
                        HudManager.Instance.Chat.AddChat(player, message.message);
                    }
                    break;
                case ChatCommands.ChannelType.Jailor:
                    if (Jailor.players.Any(x => x.player == PlayerControl.LocalPlayer || x.jailTarget == PlayerControl.LocalPlayer) || Helpers.shouldShowGhostInfo())
                    {
                        ChatCommands.CurrentChatType = ChatCommands.ChatTypes.JailorChat;
                        bool jailed = Jailor.isJailed(PlayerControl.LocalPlayer.PlayerId);
                        if (jailed) HudManager.Instance.Chat.StartCoroutine(HudManager.Instance.Chat.BounceDot());
                        HudManager.Instance.Chat.AddChat(jailed ? PlayerControl.LocalPlayer : player, message.message);
                        SoundManager.Instance.PlaySound(HudManager.Instance?.Chat?.messageSound, false, 1f, null);
                    }
                    break;
            }
            ChatCommands.CurrentChatType = ChatCommands.ChatTypes.Default;
        });

        public static void shareRealTasks(MessageReader reader)
        {
            byte count = reader.ReadByte();
            for (int i = 0; i < count; i++)
            {
                byte playerId = reader.ReadByte();
                byte[] taskTmp = reader.ReadBytes(4);
                float x = BitConverter.ToSingle(taskTmp, 0);
                taskTmp = reader.ReadBytes(4);
                float y = BitConverter.ToSingle(taskTmp, 0);
                Vector2 pos = new(x, y);
                if (!MapBehaviourPatch.realTasks.ContainsKey(playerId)) MapBehaviourPatch.realTasks[playerId] = new Il2CppSystem.Collections.Generic.List<Vector2>();
                MapBehaviourPatch.realTasks[playerId].Add(pos);
            }
        }

        // Hmm... Lots of bugs huh?
        public static void fortuneTellerUsedDivine(byte fortuneTellerId, byte targetId)
        {
            PlayerControl fortuneTeller = Helpers.playerById(fortuneTellerId);
            var ftRole = FortuneTeller.getRole(fortuneTeller);
            if (ftRole == null) return;
            PlayerControl target = Helpers.playerById(targetId);
            if (target == null) return;
            if (target.Data.IsDead) return;

            if (target.isRole(RoleId.Fox) || target.isRole(RoleId.SchrodingersCat))
            {
                KillAnimationCoPerformKillPatch.hideNextAnimation = true;
                fortuneTeller.MurderPlayer(target, MurderResultFlags.Succeeded);
                overrideDeathReasonAndKiller(target, DeadPlayer.CustomDeathReason.Divined, fortuneTeller);
            }

            // インポスターの場合は占い師の位置に矢印を表示
            if (PlayerControl.LocalPlayer.Data.Role.IsImpostor)
            {
                FortuneTeller.fortuneTellerMessage(ModTranslation.getString("fortuneTellerDivinedSomeone"), 7f, Color.white);
            }
            ftRole.setDivinedFlag(true);
            ftRole.isRevealed = target.Data.Role.IsImpostor && FortuneTeller.revealOnImp;
            if (target.isRole(RoleId.Immoralist) && PlayerControl.LocalPlayer == target)
            {
                FortuneTeller.fortuneTellerMessage(ModTranslation.getString("fortuneTellerDivinedYou"), 7f, Color.white);
            }
            ftRole.divineTarget = target;

            if (PlayerControl.LocalPlayer == fortuneTeller)
            {
                if (target.isRole(RoleId.Fox)) _ = new StaticAchievementToken("fortuneTeller.another1");
                else if (target.Data.Role.IsImpostor) ftRole.acTokenImpostor.Value.divined = true;
            }
        }

        public static RemoteProcess PlaceAntique = new("PlaceAntique", (_) =>
        {
            var dictionary = new Dictionary<Vector3, SystemTypes>();
            if (Helpers.isSkeld()) dictionary = Antique.SkeldPos;
            else if (Helpers.isMira()) dictionary = Antique.MiraPos;
            else if (Helpers.isPolus()) dictionary = Antique.PolusPos;
            else if (Helpers.isAirship()) dictionary = Antique.AirsihpPos;
            else dictionary = Antique.FunglePos;

            foreach (var p in dictionary) new Antique(p.Key, p.Value);
        });

        public static void archaeologistExcavate(byte index)
        {
            if (Antique.antiques == null || Antique.antiques.Count <= index) return; 
            var antique = Antique.antiques.FirstOrDefault(x => x.id == index);
            if (antique == null) return;
            antique.isBroken = true;
            antique.spriteRenderer.sprite = Antique.getBrokenSprite();
            if (PlayerControl.LocalPlayer.isRole(RoleId.Archaeologist))
            {
                (var names, var role) = Archaeologist.getRoleInfo();
                var content = string.Format(ModTranslation.getString("archaeologistClue"), Helpers.cs(role.color, role.name), names);
                RolloverMessage rolloverMessage = RolloverMessage.Create(antique.gameObject.transform.position, true, content, 5f, 0.5f, 2f, 1f, Color.white);
                rolloverMessage.velocity = new Vector3(0f, 0.1f);
                MeetingOverlayHolder.RegisterOverlay(TORGUIContextEngine.API.VerticalHolder(GUIAlignment.Left,
                    new TORGUIText(GUIAlignment.Left, TORGUIContextEngine.API.GetAttribute(AttributeAsset.OverlayTitle), new TranslateTextComponent("archaeologistDetectInfo")),
                    new TORGUIText(GUIAlignment.Left, TORGUIContextEngine.API.GetAttribute(AttributeAsset.OverlayContent), new RawTextComponent(content)))
                    , MeetingOverlayHolder.IconsSprite[2], Archaeologist.color);
                _ = new StaticAchievementToken("archaeologist.challenge");
                if (archaeologistDetectButton.isEffectActive && antique.isDetected) archaeologistDetectButton.Timer = 0f;
            }
            antique.isDetected = false;
            if (Archaeologist.revealAntique == Archaeologist.RevealAntique.Immediately) antique.revealAntique();
        }

        public static void jackalCreatesSidekick(byte targetId, byte jackalId) {
            PlayerControl player = Helpers.playerById(targetId);
            var jackal = Jackal.getRole(Helpers.playerById(jackalId));
            if (player == null || jackal == null) return;
            //if (Lawyer.target == player && Lawyer.isProsecutor && Lawyer.lawyer != null && !Lawyer.lawyer.Data.IsDead) Lawyer.isProsecutor = false;

            if (!Jackal.canCreateSidekickFromImpostor && player.Data.Role.IsImpostor) {
                jackal.fakeSidekick = player;
            } else {
                if (player.isRole(RoleId.Immoralist) && PlayerControl.LocalPlayer == player) _ = new StaticAchievementToken("combination.2.immoralist.sidekick.challenge");
                bool wasSpy = player.isRole(RoleId.Spy);
                bool wasImpostor = player.Data.Role.IsImpostor;  // This can only be reached if impostors can be sidekicked.
                FastDestroyableSingleton<RoleManager>.Instance.SetRole(player, RoleTypes.Crewmate);
                erasePlayerRoles(player.PlayerId, true, true, false);
                player.setRole(RoleId.Sidekick);
                var sidekick = Sidekick.getRole(player);
                sidekick.jackal = jackal;
                if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId) PlayerControl.LocalPlayer.moveable = true;
                if (wasSpy || wasImpostor) sidekick.wasTeamRed = true;
                sidekick.wasSpy = wasSpy;
                sidekick.wasImpostor = wasImpostor;
                if (player == PlayerControl.LocalPlayer) {
                    SoundEffectsManager.play("jackalSidekick");
                    _ = new StaticAchievementToken("sidekick.common1");
                    if (wasImpostor) {
                        _ = new StaticAchievementToken("sidekick.common2");
                        LastImpostor.promoteToLastImpostor();
                    }
                }
                if (HandleGuesser.isGuesserGm && CustomOptionHolder.guesserGamemodeSidekickIsAlwaysGuesser.getBool() && !HandleGuesser.isGuesser(targetId))
                    setGuesserGm(targetId);
            }
            jackal.canCreateSidekick = false;
        }
        
        public static void erasePlayerRoles(byte playerId, bool ignoreModifier = true, bool isCreatedMadmate = false, bool generateTasks = true) {
            PlayerControl player = Helpers.playerById(playerId);
            if (player == null || (!player.canBeErased() && !isCreatedMadmate)) return;

            if ((player.Data.Tasks == null || player.Data.Tasks?.Count == 0 || player.isRole(RoleId.Fox)) && !player.Data.IsDead && generateTasks && PlayerControl.LocalPlayer == player)
                PlayerControl.LocalPlayer.generateNormalTasks();

            if (player.isRole(RoleId.Lawyer)) Lawyer.clearTarget();
            player.eraseAllRoles();

            // Always remove the Madmate
            if (Madmate.madmate.Any(x => x.PlayerId == player.PlayerId)) Madmate.madmate.RemoveAll(x => x.PlayerId == player.PlayerId);
            if (CreatedMadmate.createdMadmate.Any(x => x.PlayerId == player.PlayerId)) CreatedMadmate.createdMadmate.RemoveAll(x => x.PlayerId == player.PlayerId);
            if (player == LastImpostor.lastImpostor) LastImpostor.lastImpostor = null;

            // Modifier
            if (!ignoreModifier)
            {
                if (Bloody.bloody.Any(x => x.PlayerId == player.PlayerId)) Bloody.bloody.RemoveAll(x => x.PlayerId == player.PlayerId);
                if (AntiTeleport.antiTeleport.Any(x => x.PlayerId == player.PlayerId)) AntiTeleport.antiTeleport.RemoveAll(x => x.PlayerId == player.PlayerId);
                if (Sunglasses.sunglasses.Any(x => x.PlayerId == player.PlayerId)) Sunglasses.sunglasses.RemoveAll(x => x.PlayerId == player.PlayerId);
                if (player == Tiebreaker.tiebreaker) Tiebreaker.clearAndReload();
                if (player == Mini.mini) Mini.clearAndReload();
                if (Vip.vip.Any(x => x.PlayerId == player.PlayerId)) Vip.vip.RemoveAll(x => x.PlayerId == player.PlayerId);
                if (Invert.invert.Any(x => x.PlayerId == player.PlayerId)) Invert.invert.RemoveAll(x => x.PlayerId == player.PlayerId);
                if (Chameleon.chameleon.Any(x => x.PlayerId == player.PlayerId)) Chameleon.chameleon.RemoveAll(x => x.PlayerId == player.PlayerId);
                if (player == Armored.armored) Armored.clearAndReload();
                if (player == Radar.radar) Radar.clearAndReload();
                if (Multitasker.multitasker.Any(x => x.PlayerId == player.PlayerId)) Multitasker.multitasker.RemoveAll(x => x.PlayerId == player.PlayerId);
                if (Diseased.diseased.Any(x => x.PlayerId == player.PlayerId)) Diseased.diseased.RemoveAll(x => x.PlayerId == player.PlayerId);
            }
        }

        public static RemoteProcess<(int variation, int relatedTag, byte sourceId, int targetMask, float timeLag)> RecordStatistics = new("RecordStatistics", (message, _) =>
        TORGameManager.Instance?.GameStatistics.RecordEvent(new GameStatistics.Event(GameStatistics.EventVariation.ValueOf(message.variation), TORGameManager.Instance.CurrentTime + message.timeLag, message.sourceId == byte.MaxValue ? null : message.sourceId, message.targetMask, null) { RelatedTag = TranslatableTag.ValueOf(message.relatedTag) }));

        public static void foxCreatesImmoralist(byte targetId)
        {
            PlayerControl player = Helpers.playerById(targetId);
            FastDestroyableSingleton<RoleManager>.Instance.SetRole(player, RoleTypes.Crewmate);
            erasePlayerRoles(player.PlayerId, true, true, false);
            if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId) PlayerControl.LocalPlayer.moveable = true;
            player.setRole(RoleId.Immoralist);
            player.clearAllTasks();
            Fox.canCreateImmoralist = false;
        }

        static public readonly RemoteProcess<(string achievement, byte playerId)> RpcClearAchievement = new("ClearAchievement", (message, _) =>
        {
            var player = Helpers.playerById(message.playerId);
            if (player.AmOwner) new StaticAchievementToken(message.achievement);
        });

        public static void placeAccel(byte id)
        {
            new Props.AccelTrap(Props.AccelTrap.findAccelPos()[id]);
        }

        public static void placeDecel(byte id)
        {
            new Props.DecelTrap(Props.DecelTrap.findDecelPos()[id]);
        }

        public static void activateDecel(byte decelId, byte playerId)
        {
            PlayerControl player = Helpers.playerById(playerId);
            var decel = Props.DecelTrap.decels[decelId];
            decel.isTriggered = true;
            decel.activateTime = DateTime.UtcNow;
            if (Props.DecelTrap.deceled.ContainsKey(player)) Props.DecelTrap.deceled.Remove(player);
            Props.DecelTrap.deceled.Add(player, DateTime.UtcNow);
            if (PlayerControl.LocalPlayer == player) SoundEffectsManager.play("triggerDeceleration");
            decel.decelTrap.SetActive(false);
        }

        public static void untriggerDecel(byte decelId)
        {
            var decel = Props.DecelTrap.decels[decelId];
            decel.decelTrap.SetActive(true);
            decel.isTriggered = false;
        }

        public static void deactivateDecel(byte playerId)
        {
            PlayerControl player = Helpers.playerById(playerId);
            if (Props.DecelTrap.deceled.ContainsKey(player)) Props.DecelTrap.deceled.Remove(player);
            if (PlayerControl.LocalPlayer == player) SoundEffectsManager.play("untriggerDeceleration");
        }

        public static void activateAccel(byte playerId)
        {
            PlayerControl player = Helpers.playerById(playerId);
            if (Props.AccelTrap.acceled.ContainsKey(player)) Props.AccelTrap.acceled.Remove(player);
            Props.AccelTrap.acceled.Add(player, DateTime.UtcNow);
            if (PlayerControl.LocalPlayer == player) SoundEffectsManager.play("jekyllAndHydeDrug");
        }

        public static void deactivateAccel(byte playerId)
        {
            PlayerControl player = Helpers.playerById(playerId);

            if (Props.AccelTrap.acceled.ContainsKey(player)) Props.AccelTrap.acceled.Remove(player);
            if (PlayerControl.LocalPlayer == player) SoundEffectsManager.play("jekyllAndHydeDrug");
        }

        public static void serialKillerSuicide(byte serialKillerId)
        {
            PlayerControl serialKiller = Helpers.playerById(serialKillerId);
            if (serialKiller == null) return;
            serialKiller.MurderPlayer(serialKiller, MurderResultFlags.Succeeded);
            overrideDeathReasonAndKiller(serialKiller, DeadPlayer.CustomDeathReason.Suicide);
        }

        public static void updateMeeting(byte targetId)
        {
            if (MeetingHud.Instance)
            {
                foreach (PlayerVoteArea pva in MeetingHud.Instance.playerStates)
                {
                    if (pva.TargetPlayerId == targetId)
                    {
                        pva.SetDead(pva.DidReport, true);
                        pva.Overlay.gameObject.SetActive(true);
                        MeetingHudPatch.swapperCheckAndReturnSwap(MeetingHud.Instance, targetId);
                        MeetingHudPatch.yasunaCheckAndReturnSpecialVote(MeetingHud.Instance, targetId);
                    }

                    // Give players back their vote if target is shot dead
                    if (pva.VotedFor != targetId) continue;
                    pva.UnsetVote();
                    var voteAreaPlayer = Helpers.playerById(pva.TargetPlayerId);
                    if (!voteAreaPlayer.AmOwner) continue;
                    MeetingHud.Instance.ClearVote();
                }

                if (AmongUsClient.Instance.AmHost)
                    MeetingHud.Instance.CheckForEndVoting();
            }
        }

        public static void buskerPseudocide(byte targetId, bool isTrueDead, bool isLoverSuicide)
        {
            PlayerControl player = Helpers.playerById(targetId);
            var busker = Busker.getRole(player);
            if (player == null || busker == null) return;
            if (!isTrueDead)
            {
                busker.pseudocideFlag = true;
                player.gameObject.layer = LayerMask.NameToLayer("Ghost");
                if (player.AmOwner)
                {
                    if (Constants.ShouldPlaySfx())
                    {
                        SoundManager.Instance.PlaySound(player.KillSfx, false, 0.8f);
                    }
                    player.cosmetics.SetNameMask(false);
                    player.RpcSetScanner(false);
                }
                player.MyPhysics.StartCoroutine(player.KillAnimations.First().CoPerformKill(player, player));
                busker.deathPosition = player.transform.position;
                Helpers.HandleRoleFlashOnDeath(player);
            }
            else
            {
                busker.pseudocideFlag = false;
                Seer.deadBodyPositions?.Add(busker.deathPosition);
                if (Medium.futureDeadBodies != null) Medium.futureDeadBodies.Add(new Tuple<DeadPlayer, Vector3>(new DeadPlayer(player, DateTime.UtcNow, DeadPlayer.CustomDeathReason.Pseudocide, player), busker.deathPosition));
                if (!isLoverSuicide) overrideDeathReasonAndKiller(player, DeadPlayer.CustomDeathReason.Pseudocide);
                busker.pseudocideComplete = true;
                TORGameManager.Instance?.GameStatistics.RecordEvent(new(GameStatistics.EventVariation.Kill, targetId, 1 << targetId) { RelatedTag = isLoverSuicide ? EventDetail.Kill : EventDetail.Pseudocide });

                player.OnDeath(player);

                if (AmongUsClient.Instance.AmHost) FastDestroyableSingleton<RoleManager>.Instance.AssignRoleOnDeath(player, false);
            }
        }

        public static RemoteProcess BreakArmor = new("BreakArmor", (_) =>
        {
            if (Armored.armored == null || Armored.isBrokenArmor) return;
            Armored.isBrokenArmor = true;
            if (PlayerControl.LocalPlayer.Data.IsDead)
            {
                Armored.armored.ShowFailedMurder();
            }
        });

        public static void setOddIsJekyll(bool b, byte playerId)
        {
            PlayerControl player = Helpers.playerById(playerId);
            var jekyllAndHyde = JekyllAndHyde.getRole(player);
            if (player == null || jekyllAndHyde == null) return;
            jekyllAndHyde.oddIsJekyll = b;
        }

        public static void yasunaSpecialVote(byte playerid, byte targetid)
        {
            if (!MeetingHud.Instance) return;
            if (!Yasuna.isYasuna(playerid)) return;
            PlayerControl target = Helpers.playerById(targetid);
            if (target == null) return;
            Yasuna.specialVoteTargetPlayerId = targetid;
            Yasuna.remainingSpecialVotes(true);
        }

        public static void taskMasterSetExTasks(byte playerId, byte oldTaskMasterPlayerId, byte[] taskTypeIds)
        {
            PlayerControl oldTaskMasterPlayer = Helpers.playerById(oldTaskMasterPlayerId);
            if (oldTaskMasterPlayer != null)
            {
                oldTaskMasterPlayer.clearAllTasks();
                TaskMaster.oldTaskMasterPlayerId = oldTaskMasterPlayerId;
                if (PlayerControl.LocalPlayer.PlayerId == oldTaskMasterPlayerId) PlayerControl.LocalPlayer.generateNormalTasks();
            }

            if (!Helpers.playerById(playerId).isRole(RoleId.TaskMaster))
                return;
            NetworkedPlayerInfo player = GameData.Instance.GetPlayerById(playerId);
            if (player == null)
                return;

            if (taskTypeIds != null && taskTypeIds.Length > 0)
            {
                player.Object.clearAllTasks();
                player.Tasks = new Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo>(taskTypeIds.Length);
                for (int i = 0; i < taskTypeIds.Length; i++)
                {
                    player.Tasks.Add(new NetworkedPlayerInfo.TaskInfo(taskTypeIds[i], (uint)i));
                    player.Tasks[i].Id = (uint)i;
                }
                for (int i = 0; i < player.Tasks.Count; i++)
                {
                    NetworkedPlayerInfo.TaskInfo taskInfo = player.Tasks[i];
                    NormalPlayerTask normalPlayerTask = UnityEngine.Object.Instantiate(MapUtilities.CachedShipStatus.GetTaskById(taskInfo.TypeId), player.Object.transform);
                    normalPlayerTask.Id = taskInfo.Id;
                    normalPlayerTask.Owner = player.Object;
                    normalPlayerTask.Initialize();
                    player.Object.myTasks.Add(normalPlayerTask);
                }
                TaskMaster.isTaskComplete = true;
            }
            else
            {
                TaskMaster.isTaskComplete = false;
            }
        }

        public static void taskMasterUpdateExTasks(byte clearExTasks, byte allExTasks)
        {
            if (!TaskMaster.exists) return;
            TaskMaster.clearExTasks = clearExTasks;
            TaskMaster.allExTasks = allExTasks;
        }

        public static RemoteProcess<byte> ImpostorPromotesToLastImpostor = RemotePrimitiveProcess.OfByte("ImpostorPromotesToLastImpostor", (message, _) =>
        {
            PlayerControl player = Helpers.playerById(message);
            if (player == null) return;

            if (LastImpostor.lastImpostor != null && player == LastImpostor.lastImpostor) return;
            if (LastImpostor.lastImpostor != null && !LastImpostor.isOriginalGuesser) GuesserGM.clear(LastImpostor.lastImpostor.PlayerId);
            LastImpostor.clearAndReload();
            if (!HandleGuesser.isGuesser(message)) setGuesserGm(message);
            else LastImpostor.isOriginalGuesser = true;
            LastImpostor.lastImpostor = player;
            var g = GuesserGM.guessers.FindLast(x => x.guesser.PlayerId == message);
            if (g == null) return;
            g.shots = Mathf.Max(g.shots, LastImpostor.numShots);
        });

        public static RemoteProcess UnlockTaskMasterAch = new("UnlockTaskMasterAch", (_) =>
        {
            if (PlayerControl.LocalPlayer.isRole(RoleId.TaskMaster))
                new StaticAchievementToken("taskMaster.challenge");
        });

        public static void plantBomb(byte playerId)
        {
            var p = Helpers.playerById(playerId);
            if (PlayerControl.LocalPlayer.isRole(RoleId.BomberA)) BomberB.bombTarget = p;
            if (PlayerControl.LocalPlayer.isRole(RoleId.BomberB)) BomberA.bombTarget = p;
        }

        public static void releaseBomb(byte killer, byte target)
        {
            // 同時押しでダブルキルが発生するのを防止するためにBomberAで一度受け取ってから実行する
            if (PlayerControl.LocalPlayer.isRole(RoleId.BomberA))
            {
                if (BomberA.bombTarget != null && BomberB.bombTarget != null)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.BomberKill, SendOption.Reliable, -1);
                    writer.Write(killer);
                    writer.Write(target);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    bomberKill(killer, target);
                }
            }
        }

        public static void bomberKill(byte killer, byte target)
        {
            BomberA.bombTarget = null;
            BomberB.bombTarget = null;
            var k = Helpers.playerById(killer);
            var t = Helpers.playerById(target);
            if (!t.Data.IsDead)
            {
                KillAnimationCoPerformKillPatch.hideNextAnimation = true;
                k.MurderPlayer(t, MurderResultFlags.Succeeded);
                overrideDeathReasonAndKiller(t, DeadPlayer.CustomDeathReason.Bomb, k);
                if (BomberA.showEffects)
                    _ = new BombEffect(t);
            }
            bomberAPlantBombButton.Timer = bomberAPlantBombButton.MaxTimer;
            bomberBPlantBombButton.Timer = bomberBPlantBombButton.MaxTimer;
        }

        public static void placeCamera(byte[] buff, byte roomId) {
            var referenceCamera = UnityEngine.Object.FindObjectOfType<SurvCamera>(); 
            if (referenceCamera == null) return; // Mira HQ

            SecurityGuard.remainingScrews -= SecurityGuard.camPrice;
            SecurityGuard.placedCameras++;

            Vector3 position = Vector3.zero;
            position.x = BitConverter.ToSingle(buff, 0*sizeof(float));
            position.y = BitConverter.ToSingle(buff, 1*sizeof(float));

            var camera = UnityEngine.Object.Instantiate<SurvCamera>(referenceCamera);
            camera.transform.position = new Vector3(position.x, position.y, referenceCamera.transform.position.z - 1f);
            camera.CamName = $"Security Camera {SecurityGuard.placedCameras}";
            camera.Offset = new Vector3(0f, 0f, camera.Offset.z);

            camera.NewName = (SystemTypes)roomId switch
            {
                SystemTypes.Hallway => StringNames.Hallway,
                SystemTypes.Storage => StringNames.Storage,
                SystemTypes.Cafeteria => StringNames.Cafeteria,
                SystemTypes.Reactor => StringNames.Reactor,
                SystemTypes.UpperEngine => StringNames.UpperEngine,
                SystemTypes.Nav => StringNames.Nav,
                SystemTypes.Admin => StringNames.Admin,
                SystemTypes.Electrical => StringNames.Electrical,
                SystemTypes.LifeSupp => StringNames.LifeSupp,
                SystemTypes.Shields => StringNames.Shields,
                SystemTypes.MedBay => StringNames.MedBay,
                SystemTypes.Security => StringNames.Security,
                SystemTypes.Weapons => StringNames.Weapons,
                SystemTypes.LowerEngine => StringNames.LowerEngine,
                SystemTypes.Comms => StringNames.Comms,
                SystemTypes.Decontamination => StringNames.Decontamination,
                SystemTypes.Launchpad => StringNames.Launchpad,
                SystemTypes.LockerRoom => StringNames.LockerRoom,
                SystemTypes.Laboratory => StringNames.Laboratory,
                SystemTypes.Balcony => StringNames.Balcony,
                SystemTypes.Office => StringNames.Office,
                SystemTypes.Greenhouse => StringNames.Greenhouse,
                SystemTypes.Dropship => StringNames.Dropship,
                SystemTypes.Decontamination2 => StringNames.Decontamination2,
                SystemTypes.Decontamination3 => StringNames.Decontamination3,
                SystemTypes.Outside => StringNames.Outside,
                SystemTypes.Specimens => StringNames.Specimens,
                SystemTypes.BoilerRoom => StringNames.BoilerRoom,
                SystemTypes.VaultRoom => StringNames.VaultRoom,
                SystemTypes.Cockpit => StringNames.Cockpit,
                SystemTypes.Armory => StringNames.Armory,
                SystemTypes.Kitchen => StringNames.Kitchen,
                SystemTypes.ViewingDeck => StringNames.ViewingDeck,
                SystemTypes.HallOfPortraits => StringNames.HallOfPortraits,
                SystemTypes.CargoBay => StringNames.CargoBay,
                SystemTypes.Ventilation => StringNames.Ventilation,
                SystemTypes.Showers => StringNames.Showers,
                SystemTypes.Engine => StringNames.Engine,
                SystemTypes.Brig => StringNames.Brig,
                SystemTypes.MeetingRoom => StringNames.MeetingRoom,
                SystemTypes.Records => StringNames.Records,
                SystemTypes.Lounge => StringNames.Lounge,
                SystemTypes.GapRoom => StringNames.GapRoom,
                SystemTypes.MainHall => StringNames.MainHall,
                SystemTypes.Medical => StringNames.Medical,
                _ => StringNames.None,
            };

            if (GameOptionsManager.Instance.currentNormalGameOptions.MapId is 2 or 4) camera.transform.localRotation = new Quaternion(0, 0, 1, 1); // Polus and Airship 

            if (SubmergedCompatibility.IsSubmerged) {
                // remove 2d box collider of console, so that no barrier can be created. (irrelevant for now, but who knows... maybe we need it later)
                var fixConsole = camera.transform.FindChild("FixConsole");
                if (fixConsole != null) {
                    var boxCollider = fixConsole.GetComponent<BoxCollider2D>();
                    if (boxCollider != null) UnityEngine.Object.Destroy(boxCollider);
                }
            }


            if (PlayerControl.LocalPlayer.isRole(RoleId.SecurityGuard)) {
                camera.gameObject.SetActive(true);
                camera.gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.5f);
            } else {
                camera.gameObject.SetActive(false);
            }
            camerasToAdd.Add(camera);
        }

        public static void sealVent(int ventId) {
            Vent vent = MapUtilities.CachedShipStatus.AllVents.FirstOrDefault((x) => x != null && x.Id == ventId);
            if (vent == null) return;

            SecurityGuard.remainingScrews -= SecurityGuard.ventPrice;
            if (PlayerControl.LocalPlayer.isRole(RoleId.SecurityGuard)) {
                PowerTools.SpriteAnim animator = vent.GetComponent<PowerTools.SpriteAnim>(); 
                vent.EnterVentAnim = vent.ExitVentAnim = null;
                Sprite newSprite = animator == null ? SecurityGuard.getStaticVentSealedSprite() : SecurityGuard.getAnimatedVentSealedSprite();
                SpriteRenderer rend = vent.myRend;
                if (Helpers.isFungle())
                {
                    newSprite = SecurityGuard.getFungleVentSealedSprite();
                    rend = vent.transform.GetChild(3).GetComponent<SpriteRenderer>();
                    animator = vent.transform.GetChild(3).GetComponent<PowerTools.SpriteAnim>();
                }
                animator?.Stop();
                rend.sprite = newSprite;
                if (SubmergedCompatibility.IsSubmerged && vent.Id == 0) vent.myRend.sprite = SecurityGuard.getSubmergedCentralUpperSealedSprite();
                if (SubmergedCompatibility.IsSubmerged && vent.Id == 14) vent.myRend.sprite = SecurityGuard.getSubmergedCentralLowerSealedSprite();
                rend.color = new Color(1f, 1f, 1f, 0.5f);
                vent.name = "FutureSealedVent_" + vent.name;
            }

            ventsToSeal.Add(vent);
        }

        /// <summary>
        /// Shoots the dying target during the meeting
        /// </summary>
        /// <param name="killerId">PlayerId of the Guesser</param>
        /// <param name="dyingTargetId">PlayerId of the dying target (i.e. wrong guess = Guesser, right guess = target)</param>
        /// <param name="guessedTargetId">The PlayerId the dying target has guessed</param>
        /// <param name="guessedRoleId">The RoleId the Guesser has guessed (2 same RoleIds for Swapper and Shifter)</param>
        /// <param name="isSpecialRole">Whether or not this is a Nice Shifter or a Nice Swapper etc.</param>
        public static void guesserShoot(byte killerId, byte dyingTargetId, byte guessedTargetId, byte guessedRoleId, bool isSpecialRole) {
            TORGameManager.Instance?.GameStatistics.RecordEvent(new(GameStatistics.EventVariation.Kill, killerId, 1 << dyingTargetId) { RelatedTag = killerId == dyingTargetId ? EventDetail.MisGuess : EventDetail.Guessed});

            PlayerControl killer = Helpers.playerById(killerId);
            PlayerControl dyingTarget = Helpers.playerById(dyingTargetId);
            if (dyingTarget == null) return;
            if (dyingTarget.isRole(RoleId.NekoKabocha)) NekoKabocha.getRole(dyingTarget).meetingKiller = killer;

            PlayerControl guesser = Helpers.playerById(killerId);
            if (killer.isRole(RoleId.Thief) && Thief.canStealWithGuess) {
                RoleInfo roleInfo = RoleInfo.allRoleInfos.FirstOrDefault(x => (byte)x.roleId == guessedRoleId);
                if (!killer.Data.IsDead && !Thief.isFailedThiefKill(dyingTarget, guesser, roleInfo)) {
                    thiefStealsRole(dyingTarget.PlayerId, killerId);
                }
            }

            if (killer.isRole(RoleId.Doomsayer) && dyingTarget != killer) {
                if (Doomsayer.indicateGuesses && !PlayerControl.LocalPlayer.Data.IsDead)
                {
                    if (AmongUsClient.Instance.AmClient && FastDestroyableSingleton<HudManager>.Instance) {
                        FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, ModTranslation.getString("doomsayerGuessedSomeone"), false);
                    }
                }
                var doomsayer = Doomsayer.getRole(killer);
                doomsayer.counter++;
                if (doomsayer.counter >= Doomsayer.guessesToWin) doomsayer.triggerWin = true;
            }

            dyingTarget.Exiled();
            overrideDeathReasonAndKiller(dyingTarget, DeadPlayer.CustomDeathReason.Guess, guesser);

            if (PlayerControl.LocalPlayer == killer && dyingTarget != killer)
            {
                if (killer.isRole(RoleId.NiceGuesser))
                {
                    NiceGuesser.acTokenNiceGuesser.Value++;
                    _ = new StaticAchievementToken("niceGuesser.common1");
                    if (dyingTarget.isRole(RoleId.EvilGuesser))
                        _ = new StaticAchievementToken("niceGuesser.challenge2");
                }
                else if (killer.isRole(RoleId.EvilGuesser))
                {
                    EvilGuesser.acTokenEvilGuesser.Value++;
                    _ = new StaticAchievementToken("evilGuesser.common1");
                    if (dyingTarget.isRole(RoleId.NiceGuesser))
                        _ = new StaticAchievementToken("evilGuesser.challenge2");
                }
            }

            HandleGuesser.remainingShots(killerId, true);
            if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(dyingTarget.KillSfx, false, 0.8f);
            if (FastDestroyableSingleton<HudManager>.Instance != null && guesser != null)
                if (PlayerControl.LocalPlayer == dyingTarget) {
                    FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(guesser.Data, dyingTarget.Data);
                }

            if (AmongUsClient.Instance.AmClient && FastDestroyableSingleton<HudManager>.Instance)
            {
                PlayerControl guessedTarget = Helpers.playerById(guessedTargetId);
                PlayerControl sender = PlayerControl.LocalPlayer;
                RoleInfo guessedRoleInfo = RoleInfo.allRoleInfos.FirstOrDefault(x => (byte)x.roleId == guessedRoleId);
                if (isSpecialRole) {
                    if ((RoleId)guessedRoleId == RoleId.Swapper) guessedRoleInfo = RoleInfo.niceSwapper;
                    else if ((RoleId)guessedRoleId == RoleId.Shifter) guessedRoleInfo = RoleInfo.niceshifter;
                }
                string msg = "";
                if (PlayerControl.LocalPlayer.Data.IsDead && guessedTarget != null && guesser != null)
                {
                    msg = string.Format(ModTranslation.getString("guesserGuessChat"), guesser.Data.PlayerName, guessedRoleInfo?.name ?? "", guessedTarget.Data.PlayerName);
                    sender = guesser;
                }
                else if (!PlayerControl.LocalPlayer.Data.IsDead && (PlayerControl.LocalPlayer.isRole(RoleId.NiceWatcher) || PlayerControl.LocalPlayer.isRole(RoleId.EvilWatcher)) && Watcher.canSeeGuesses)
                    msg = string.Format(ModTranslation.getString("watcherGuessChat"), guessedRoleInfo?.name ?? "", guessedTarget.Data.PlayerName);

                if (!string.IsNullOrEmpty(msg))
                {
                    ChatCommands.CurrentChatType = ChatCommands.ChatTypes.GuesserMessage;
                    FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(sender, msg, false);
                }
            }
        }

        public static void setBlanked(byte playerId, byte value) {
            PlayerControl target = Helpers.playerById(playerId);
            if (target == null) return;
            Pursuer.blankedList.RemoveAll(x => x.PlayerId == playerId);
            if (value > 0) Pursuer.blankedList.Add(target);            
        }

        public static RemoteProcess<byte> SetFirstKill = RemotePrimitiveProcess.OfByte("SetFirstKill", (message, _) =>
        {
            PlayerControl target = Helpers.playerById(message);
            if (target == null) return;
            firstKillPlayer = target;
        });

        public static RemoteProcess SetTieBreak = new("SetTieBreak", (_) => Tiebreaker.isTiebreak = true);

        public static void thiefStealsRole(byte playerId, byte thiefId) {
            // Notify that the Thief cannot steal the Mimic

            PlayerControl target = Helpers.playerById(playerId);
            PlayerControl thief = Helpers.playerById(thiefId);
            if (target == null) return;
            Thief.eraseRole(thief);
            if (target.isRole(RoleId.Witch))
            {
                var witch = Witch.getRole(target);
                if (MeetingHud.Instance)
                    if (Witch.witchVoteSavesTargets)  // In a meeting, if the thief guesses the witch, all targets are saved or no target is saved.
                        witch.futureSpelled = [];
                    else  // If thief kills witch during the round, remove the thief from the list of spelled people, keep the rest
                        witch.futureSpelled.RemoveAll(x => x.PlayerId == thief.PlayerId);
            }
            var role = Role.allRoles.FirstOrDefault(x => x.player == target);
            target.swapRoles(thief);
            if (role != null && role.roleId is RoleId.Jackal or RoleId.JekyllAndHyde or RoleId.Moriarty or RoleId.Yandere or RoleId.Pelican or RoleId.SchrodingersCat) target.setRole(role.roleId);  // Keep teamed roles to the target
            if (target.Data.Role.IsImpostor) {
                RoleManager.Instance.SetRole(thief, RoleTypes.Impostor);
                FastDestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(thief.killTimer, PlayerControl.LocalPlayer.GetKillCooldown());
            }
            if (target == Lawyer.target)
                Lawyer.target = thief;
            if (thief == PlayerControl.LocalPlayer) CustomButton.ResetAllCooldowns();
            Thief.formerThief.Add(thief);  // After clearAndReload, else it would get reset...

            TORGameManager.Instance?.RecordRoleHistory(thief);
            TORGameManager.Instance?.RecordRoleHistory(target);
        }

        public static void setGuesserGm (byte playerId) {
            PlayerControl target = Helpers.playerById(playerId);
            if (target == null) return;
            new GuesserGM(target);
        }

        public static void shareTimer(float punish) {
            HideNSeek.timer -= punish;
        }

        public static void huntedShield(byte playerId) {
            if (!Hunted.timeshieldActive.Contains(playerId)) Hunted.timeshieldActive.Add(playerId);
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Hunted.shieldDuration, new Action<float>((p) => {
                if (p == 1f) Hunted.timeshieldActive.Remove(playerId);
            })));
        }

        public static void huntedRewindTime(byte playerId) {
            Hunted.timeshieldActive.Remove(playerId); // Shield is no longer active when rewinding
            SoundEffectsManager.stop("timemasterShield");  // Shield sound stopped when rewinding
            if (playerId == PlayerControl.LocalPlayer.PlayerId) {
                resetHuntedRewindButton();
            }
            FastDestroyableSingleton<HudManager>.Instance.FullScreen.color = new Color(0f, 0.5f, 0.8f, 0.3f);
            FastDestroyableSingleton<HudManager>.Instance.FullScreen.enabled = true;
            FastDestroyableSingleton<HudManager>.Instance.FullScreen.gameObject.SetActive(true);
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Hunted.shieldRewindTime, new Action<float>((p) => {
                if (p == 1f) FastDestroyableSingleton<HudManager>.Instance.FullScreen.enabled = false;
            })));

            if (!PlayerControl.LocalPlayer.Data.Role.IsImpostor) return; // only rewind hunter

            TimeMaster.isRewinding = true;

            if (MapBehaviour.Instance)
                MapBehaviour.Instance.Close();
            if (Minigame.Instance)
                Minigame.Instance.ForceClose();
            PlayerControl.LocalPlayer.moveable = false;
        }

        public enum GhostInfoTypes {
            HandcuffNoticed,
            HandcuffOver,
            AssassinMarked,
            WarlockTarget,
            MediumInfo,
            BlankUsed,
            DetectiveOrMedicInfo,
            DeathReasonAndKiller,
        }

        public static void receiveGhostInfo (byte senderId, MessageReader reader) {
            PlayerControl sender = Helpers.playerById(senderId);

            GhostInfoTypes infoType = (GhostInfoTypes)reader.ReadByte();
            switch (infoType) {
                case GhostInfoTypes.HandcuffNoticed:
                    Deputy.setHandcuffedKnows(true, senderId);
                    break;
                case GhostInfoTypes.HandcuffOver:
                    _ = Deputy.handcuffedKnows.Remove(senderId);
                    break;
                case GhostInfoTypes.AssassinMarked:
                    var assassin = Assassin.getRole(sender);
                    if (assassin != null) assassin.assassinMarked = Helpers.playerById(reader.ReadByte());
                    break;
                case GhostInfoTypes.WarlockTarget:
                    var warlock = Warlock.getRole(sender);
                    if (warlock != null) warlock.curseVictim = Helpers.playerById(reader.ReadByte());
                    break;
                case GhostInfoTypes.MediumInfo:
                    string mediumInfo = reader.ReadString();
		             if (Helpers.shouldShowGhostInfo())
                    	FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(sender, mediumInfo, false);
                    break;
                case GhostInfoTypes.DetectiveOrMedicInfo:
                    string detectiveInfo = reader.ReadString();
                    if (Helpers.shouldShowGhostInfo())
		    	        FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(sender, detectiveInfo, false);
                    break;
                case GhostInfoTypes.BlankUsed:
                    Pursuer.blankedList.Remove(sender);
                    break;
                case GhostInfoTypes.DeathReasonAndKiller:
                    overrideDeathReasonAndKiller(Helpers.playerById(reader.ReadByte()), (DeadPlayer.CustomDeathReason)reader.ReadByte(), Helpers.playerById(reader.ReadByte()));
                    break;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    class RPCHandlerPatch
    {
        static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
        {
            if (AntiCheat.RpcSafe(__instance, callId, reader)) return false;
            return true;
        }

        static public void ReceiveMessage(MessageReader reader)
        {
            int id = reader.ReadInt32();
            if (RemoteProcessBase.AllTORProcess.TryGetValue(id, out var rpc))
            {
                rpc.Receive(reader);
            }
            else
            {
                TheOtherRolesPlugin.Logger.LogError("RPC NotFound Error. id: " + id);
                throw new Exception("RPC Error Occurred. (Not found: " + id + ")");
            }
        }

        static void Postfix([HarmonyArgument(0)]byte callId, [HarmonyArgument(1)]MessageReader reader)
        {
            if (callId == 128) ReceiveMessage(reader);
            byte packetId = callId;

            switch (packetId) {

                // Main Controls

                case (byte)CustomRPC.ResetVaribles:
                    RPCProcedure.resetVariables();
                    break;
                case (byte)CustomRPC.ShareOptions:
                    RPCProcedure.HandleShareOptions(reader.ReadByte(), reader);
                    break;
                case (byte)CustomRPC.ShareFilterOptions:
                    RPCProcedure.HandleShareFilterOptions(reader.ReadByte(), reader);
                    break;
                case (byte)CustomRPC.ForceEnd:
                    RPCProcedure.forceEnd();
                    break; 
                case (byte)CustomRPC.WorkaroundSetRoles:
                    RPCProcedure.workaroundSetRoles(reader.ReadByte(), reader);
                    break;
                case (byte)CustomRPC.SetRole:
                    byte roleId = reader.ReadByte();
                    byte playerId = reader.ReadByte();
                    RPCProcedure.setRole(roleId, playerId);
                    break;
                case (byte)CustomRPC.SetModifier:
                    byte modifierId = reader.ReadByte();
                    byte pId = reader.ReadByte();
                    byte flag = reader.ReadByte();
                    RPCProcedure.setModifier(modifierId, pId, flag);
                    break;
                case (byte)CustomRPC.VersionHandshake:
                    byte major = reader.ReadByte();
                    byte minor = reader.ReadByte();
                    byte patch = reader.ReadByte();
                    float timer = reader.ReadSingle();
                    if (!AmongUsClient.Instance.AmHost && timer >= 0f) GameStartManagerPatch.timer = timer;
                    int versionOwnerId = reader.ReadPackedInt32();
                    byte revision = 0xFF;
                    Guid guid;
                    if (reader.Length - reader.Position >= 17) { // enough bytes left to read
                        revision = reader.ReadByte();
                        // GUID
                        byte[] gbytes = reader.ReadBytes(16);
                        guid = new Guid(gbytes);
                    } else {
                        guid = new Guid(new byte[16]);
                    }
                    RPCProcedure.versionHandshake(major, minor, patch, revision == 0xFF ? -1 : revision, guid, versionOwnerId, reader.ReadString(), reader.ReadBoolean());
                    break;
                case (byte)CustomRPC.UseUncheckedVent:
                    int ventId = reader.ReadPackedInt32();
                    byte ventingPlayer = reader.ReadByte();
                    byte isEnter = reader.ReadByte();
                    RPCProcedure.useUncheckedVent(ventId, ventingPlayer, isEnter);
                    break;
                case (byte)CustomRPC.UncheckedMurderPlayer:
                    byte source = reader.ReadByte();
                    byte target = reader.ReadByte();
                    byte showAnimation = reader.ReadByte();
                    RPCProcedure.uncheckedMurderPlayer(source, target, showAnimation);
                    break;
                case (byte)CustomRPC.UncheckedExilePlayer:
                    byte exileTarget = reader.ReadByte();
                    RPCProcedure.uncheckedExilePlayer(exileTarget);
                    break;
                case (byte)CustomRPC.UncheckedCmdReportDeadBody:
                    byte reportSource = reader.ReadByte();
                    byte reportTarget = reader.ReadByte();
                    RPCProcedure.uncheckedCmdReportDeadBody(reportSource, reportTarget);
                    break;
                case (byte)CustomRPC.UncheckedSetTasks:
                    RPCProcedure.uncheckedSetTasks(reader.ReadByte(), reader.ReadBytesAndSize());
                    break;
                case (byte)CustomRPC.FinishShipStatusBegin:
                    RPCProcedure.finishShipStatusBegin();
                    break;
                case (byte)CustomRPC.DynamicMapOption:
                    byte mapId = reader.ReadByte();
                    RPCProcedure.dynamicMapOption(mapId);
                    break;
                case (byte)CustomRPC.SetGameStarting:
                    RPCProcedure.setGameStarting();
                    break;
                case (byte)CustomRPC.SetLovers:
                    RPCProcedure.setLovers(reader.ReadByte(), reader.ReadByte());
                    break;

                // Role functionality
                case (byte)CustomRPC.TimeMasterRewindTime:
                    RPCProcedure.timeMasterRewindTime(reader.ReadByte());
                    break;
                case (byte)CustomRPC.ShieldedMurderAttempt:
                    RPCProcedure.shieldedMurderAttempt(reader.ReadByte());
                    break;
                case (byte)CustomRPC.ErasePlayerRoles:
                    byte eraseTarget = reader.ReadByte();
                    RPCProcedure.erasePlayerRoles(eraseTarget);
                    Eraser.alreadyErased.Add(eraseTarget);
                    break;
                case (byte)CustomRPC.SerialKillerSuicide:
                    RPCProcedure.serialKillerSuicide(reader.ReadByte());
                    break;
                case (byte)CustomRPC.PlagueDoctorUpdateProgress:
                    byte progressTarget = reader.ReadByte();
                    byte[] progressByte = reader.ReadBytes(4);
                    float progress = BitConverter.ToSingle(progressByte, 0);
                    RPCProcedure.plagueDoctorProgress(progressTarget, progress);
                    break;
                case (byte)CustomRPC.PlaceAccel:
                    RPCProcedure.placeAccel(reader.ReadByte());
                    break;
                case (byte)CustomRPC.PlaceDecel:
                    RPCProcedure.placeDecel(reader.ReadByte());
                    break;
                case (byte)CustomRPC.ActivateDecel:
                    RPCProcedure.activateDecel(reader.ReadByte(), reader.ReadByte());
                    break;
                case (byte)CustomRPC.UntriggerDecel:
                    RPCProcedure.untriggerDecel(reader.ReadByte());
                    break;
                case (byte)CustomRPC.DeactivateDecel:
                    RPCProcedure.deactivateDecel(reader.ReadByte());
                    break;
                case (byte)CustomRPC.ActivateAccel:
                    RPCProcedure.activateAccel(reader.ReadByte());
                    break;
                case (byte)CustomRPC.DeactivateAccel:
                    RPCProcedure.deactivateAccel(reader.ReadByte());
                    break;
                case (byte)CustomRPC.SetOddIsJekyll:
                    RPCProcedure.setOddIsJekyll(reader.ReadBoolean(), reader.ReadByte());
                    break;
                case (byte)CustomRPC.ZephyrBlowCannon:
                    byte blownId = reader.ReadByte();
                    byte zephyrId = reader.ReadByte();
                    var posx = reader.ReadSingle();
                    var posy = reader.ReadSingle();
                    var player = Helpers.playerById(blownId);
                    var zephyr = Helpers.playerById(zephyrId);
                    Zephyr.fireCannon(player, zephyr, new Vector2(posx, posy));
                    break;
                case (byte)CustomRPC.ZephyrCheckCannon:
                    Zephyr.checkCannon(new(reader.ReadSingle(), reader.ReadSingle()), reader.ReadByte());
                    break;
                case (byte)CustomRPC.ShareRealTasks:
                    RPCProcedure.shareRealTasks(reader);
                    break;
                case (byte)CustomRPC.YasunaSpecialVote:
                    byte id = reader.ReadByte();
                    byte targetId = reader.ReadByte();
                    RPCProcedure.yasunaSpecialVote(id, targetId);
                    break;
                case (byte)CustomRPC.TaskMasterSetExTasks:
                    playerId = reader.ReadByte();
                    byte oldTaskMasterPlayerId = reader.ReadByte();
                    byte[] taskTypeIds = reader.BytesRemaining > 0 ? reader.ReadBytes(reader.BytesRemaining) : null;
                    RPCProcedure.taskMasterSetExTasks(playerId, oldTaskMasterPlayerId, taskTypeIds);
                    break;
                case (byte)CustomRPC.TaskMasterUpdateExTasks:
                    byte clearExTasks = reader.ReadByte();
                    byte allExTasks = reader.ReadByte();
                    RPCProcedure.taskMasterUpdateExTasks(clearExTasks, allExTasks);
                    break;
                case (byte)CustomRPC.PlantBomb:
                    RPCProcedure.plantBomb(reader.ReadByte());
                    break;
                case (byte)CustomRPC.ReleaseBomb:
                    RPCProcedure.releaseBomb(reader.ReadByte(), reader.ReadByte());
                    break;
                case (byte)CustomRPC.BomberKill:
                    RPCProcedure.bomberKill(reader.ReadByte(), reader.ReadByte());
                    break;
                case (byte)CustomRPC.PlaceCamera:
                    RPCProcedure.placeCamera(reader.ReadBytesAndSize(), reader.ReadByte());
                    break;
                case (byte)CustomRPC.SealVent:
                    RPCProcedure.sealVent(reader.ReadPackedInt32());
                    break;
                case (byte)CustomRPC.GuesserShoot:
                    byte killerId = reader.ReadByte();
                    byte dyingTarget = reader.ReadByte();
                    byte guessedTarget = reader.ReadByte();
                    byte guessedRoleId = reader.ReadByte();
                    bool isSpecialRole = reader.ReadBoolean();
                    RPCProcedure.guesserShoot(killerId, dyingTarget, guessedTarget, guessedRoleId, isSpecialRole);
                    break;
                case (byte)CustomRPC.SetBlanked:
                    var pid = reader.ReadByte();
                    var blankedValue = reader.ReadByte();
                    RPCProcedure.setBlanked(pid, blankedValue);
                    break;
                case (byte)CustomRPC.ThiefStealsRole:
                    byte thiefTargetId = reader.ReadByte();
                    RPCProcedure.thiefStealsRole(thiefTargetId, reader.ReadByte());
                    break;
                case (byte)CustomRPC.DraftModePickOrder:
                    RoleDraft.receivePickOrder(reader.ReadByte(), reader);
                    break;
                case (byte)CustomRPC.DraftModePick:
                    RoleDraft.receivePick(reader.ReadByte(), reader.ReadByte(), reader.ReadBoolean(), reader.ReadBoolean());
                    break;
                case (byte)CustomRPC.ShareGamemode:
                    byte gm = reader.ReadByte();
                    RPCProcedure.shareGamemode(gm);
                    break;
                case (byte)CustomRPC.StopStart:
                    RPCProcedure.stopStart(reader.ReadByte());
                    break;

                // Game mode
                case (byte)CustomRPC.SetGuesserGm:
                    byte guesserGm = reader.ReadByte();
                    RPCProcedure.setGuesserGm(guesserGm);
                    break;
                case (byte)CustomRPC.ShareTimer:
                    float punish = reader.ReadSingle();
                    RPCProcedure.shareTimer(punish);
                    break;
                case (byte)CustomRPC.HuntedShield:
                    byte huntedPlayer = reader.ReadByte();
                    RPCProcedure.huntedShield(huntedPlayer);
                    break;
                case (byte)CustomRPC.HuntedRewindTime:
                    byte rewindPlayer = reader.ReadByte();
                    RPCProcedure.huntedRewindTime(rewindPlayer);
                    break;
                case (byte)CustomRPC.ShareGhostInfo:
                    RPCProcedure.receiveGhostInfo(reader.ReadByte(), reader);
                    break;
                case (byte)CustomRPC.EventKick:
                    byte kickSource = reader.ReadByte();
                    byte kickTarget = reader.ReadByte();
                    EventUtility.handleKick(Helpers.playerById(kickSource), Helpers.playerById(kickTarget), reader.ReadSingle());
                    break;
            }
        }
    }
} 
