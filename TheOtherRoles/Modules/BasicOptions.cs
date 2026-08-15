using System;
using System.Collections.Generic;
using System.IO;
using AmongUs.GameOptions;
using BepInEx.Configuration;

namespace TheOtherRoles
{
    public static class BasicOptions
    {
        public static void Save(Dictionary<int, string> optionTable, StreamWriter sw)
        {
            var optionData = GetOptions();
            if (optionData == null)
            {
                TheOtherRolesPlugin.Logger.LogWarning("BasicOptions.Save: no normal game options available, skipping vanilla options.");
                return;
            }

            // Generic options
            Save(mapId.id, optionData.MapId, optionTable, sw);
            Save(playerSpeedMod.id, optionData.PlayerSpeedMod, optionTable, sw);
            Save(crewLightMod.id, optionData.CrewLightMod, optionTable, sw);
            Save(impostorLightMod.id, optionData.ImpostorLightMod, optionTable, sw);
            Save(killCooldown.id, optionData.KillCooldown, optionTable, sw);
            Save(numCommonTasks.id, optionData.NumCommonTasks, optionTable, sw);
            Save(numLongTasks.id, optionData.NumLongTasks, optionTable, sw);
            Save(numShortTasks.id, optionData.NumShortTasks, optionTable, sw);
            Save(numEmergencyMeetings.id, optionData.NumEmergencyMeetings, optionTable, sw);
            Save(emergencyCooldown.id, optionData.EmergencyCooldown, optionTable, sw);
            Save(numImpostors.id, optionData.NumImpostors, optionTable, sw);
            Save(ghostsDoTasks.id, optionData.GhostsDoTasks, optionTable, sw);
            Save(killDistance.id, optionData.KillDistance, optionTable, sw);
            Save(discussionTime.id, optionData.DiscussionTime, optionTable, sw);
            Save(votingTime.id, optionData.VotingTime, optionTable, sw);
            Save(confirmImpostor.id, optionData.ConfirmImpostor, optionTable, sw);
            Save(visualTasks.id, optionData.VisualTasks, optionTable, sw);
            Save(anonymousVotes.id, optionData.AnonymousVotes, optionTable, sw);
            Save(taskBarMode.id, (byte)optionData.TaskBarMode, optionTable, sw);
            Save(isDefaults.id, optionData.IsDefaults, optionTable, sw);
        }

        // 取当前原版选项（currentNormalGameOptions 在部分场景（主菜单）可能为 null，做回退；都拿不到就跳过原版设置）
        static NormalGameOptionsV10 GetOptions()
        {
            if (GameOptionsManager.Instance == null) return null;
            var o = GameOptionsManager.Instance.currentNormalGameOptions;
            if (o == null) o = GameOptionsManager.Instance.normalGameHostOptions;
            return o;
        }

        public static void Load(Dictionary<int, string> optionTable)
        {
            if (optionTable == null) return;

            var optionData = GetOptions();
            if (optionData == null) return;

            // Generic options
            if (optionTable.TryGetValue(mapId.id, out string s) && byte.TryParse(s, out var mapId_))
                optionData.MapId = mapId_;
            if (optionTable.TryGetValue(playerSpeedMod.id, out s) && float.TryParse(s, out var playerSpeedMod_))
                optionData.PlayerSpeedMod = playerSpeedMod_;
            if (optionTable.TryGetValue(crewLightMod.id, out s) && float.TryParse(s, out var crewLightMod_))
                optionData.CrewLightMod = crewLightMod_;
            if (optionTable.TryGetValue(impostorLightMod.id, out s) && float.TryParse(s, out var impostorLightMod_))
                optionData.ImpostorLightMod = impostorLightMod_;
            if (optionTable.TryGetValue(killCooldown.id, out s) && float.TryParse(s, out var killCooldown_))
                optionData.KillCooldown = killCooldown_;
            if (optionTable.TryGetValue(numCommonTasks.id, out s) && int.TryParse(s, out var numCommonTasks_))
                optionData.NumCommonTasks = numCommonTasks_;
            if (optionTable.TryGetValue(numLongTasks.id, out s) && int.TryParse(s, out var numLongTasks_))
                optionData.NumLongTasks = numLongTasks_;
            if (optionTable.TryGetValue(numShortTasks.id, out s) && int.TryParse(s, out var numShortTasks_))
                optionData.NumShortTasks = numShortTasks_;
            if (optionTable.TryGetValue(numEmergencyMeetings.id, out s) && int.TryParse(s, out var numEmergencyMeetings_))
                optionData.NumEmergencyMeetings = numEmergencyMeetings_;
            if (optionTable.TryGetValue(emergencyCooldown.id, out s) && int.TryParse(s, out var emergencyCooldown_))
                optionData.EmergencyCooldown = emergencyCooldown_;
            if (optionTable.TryGetValue(numImpostors.id, out s) && int.TryParse(s, out var numImpostors_))
                optionData.NumImpostors = numImpostors_;
            if (optionTable.TryGetValue(ghostsDoTasks.id, out s) && bool.TryParse(s, out var ghostsDoTasks_))
                optionData.GhostsDoTasks = ghostsDoTasks_;
            if (optionTable.TryGetValue(killDistance.id, out s) && int.TryParse(s, out var killDistance_))
                optionData.KillDistance = killDistance_;
            if (optionTable.TryGetValue(discussionTime.id, out s) && int.TryParse(s, out var discussionTime_))
                optionData.DiscussionTime = discussionTime_;
            if (optionTable.TryGetValue(votingTime.id, out s) && int.TryParse(s, out var votingTime_))
                optionData.VotingTime = votingTime_;
            if (optionTable.TryGetValue(confirmImpostor.id, out s) && bool.TryParse(s, out var confirmImpostor_))
                optionData.ConfirmImpostor = confirmImpostor_;
            if (optionTable.TryGetValue(visualTasks.id, out s) && bool.TryParse(s, out var visualTasks_))
                optionData.VisualTasks = visualTasks_;
            if (optionTable.TryGetValue(anonymousVotes.id, out s) && bool.TryParse(s, out var anonymousVotes_))
                optionData.AnonymousVotes = anonymousVotes_;
            if (optionTable.TryGetValue(taskBarMode.id, out s) && Enum.TryParse<TaskBarMode>(s, out var taskBarMode_))
                optionData.TaskBarMode = (AmongUs.GameOptions.TaskBarMode)taskBarMode_;
            if (optionTable.TryGetValue(isDefaults.id, out s) && bool.TryParse(s, out var isDefaults_))
                optionData.IsDefaults = isDefaults_;
        }

        public static void Inherit(string section, Dictionary<ConfigDefinition, string> orphanedEntries, StreamWriter sw)
        {
            // Generic options
            if (mapId.Load(section, orphanedEntries, out byte byteValue))
                sw.WriteLine(string.Format("{0},{1}", mapId.id, byteValue));
            if (playerSpeedMod.Load(section, orphanedEntries, out float floatValue))
                sw.WriteLine(string.Format("{0},{1}", playerSpeedMod.id, floatValue));
            if (crewLightMod.Load(section, orphanedEntries, out floatValue))
                sw.WriteLine(string.Format("{0},{1}", crewLightMod.id, floatValue));
            if (impostorLightMod.Load(section, orphanedEntries, out floatValue))
                sw.WriteLine(string.Format("{0},{1}", impostorLightMod.id, floatValue));
            if (killCooldown.Load(section, orphanedEntries, out floatValue))
                sw.WriteLine(string.Format("{0},{1}", killCooldown.id, floatValue));
            if (numCommonTasks.Load(section, orphanedEntries, out int intValue))
                sw.WriteLine(string.Format("{0},{1}", numCommonTasks.id, intValue));
            if (numLongTasks.Load(section, orphanedEntries, out intValue))
                sw.WriteLine(string.Format("{0},{1}", numLongTasks.id, intValue));
            if (numShortTasks.Load(section, orphanedEntries, out intValue))
                sw.WriteLine(string.Format("{0},{1}", numShortTasks.id, intValue));
            if (numEmergencyMeetings.Load(section, orphanedEntries, out intValue))
                sw.WriteLine(string.Format("{0},{1}", numEmergencyMeetings.id, intValue));
            if (emergencyCooldown.Load(section, orphanedEntries, out intValue))
                sw.WriteLine(string.Format("{0},{1}", emergencyCooldown.id, intValue));
            if (numImpostors.Load(section, orphanedEntries, out intValue))
                sw.WriteLine(string.Format("{0},{1}", numImpostors.id, intValue));
            if (ghostsDoTasks.Load(section, orphanedEntries, out bool boolValue))
                sw.WriteLine(string.Format("{0},{1}", ghostsDoTasks.id, boolValue));
            if (killDistance.Load(section, orphanedEntries, out intValue))
                sw.WriteLine(string.Format("{0},{1}", killDistance.id, intValue));
            if (discussionTime.Load(section, orphanedEntries, out intValue))
                sw.WriteLine(string.Format("{0},{1}", discussionTime.id, intValue));
            if (votingTime.Load(section, orphanedEntries, out intValue))
                sw.WriteLine(string.Format("{0},{1}", votingTime.id, intValue));
            if (confirmImpostor.Load(section, orphanedEntries, out boolValue))
                sw.WriteLine(string.Format("{0},{1}", confirmImpostor.id, boolValue));
            if (visualTasks.Load(section, orphanedEntries, out boolValue))
                sw.WriteLine(string.Format("{0},{1}", visualTasks.id, boolValue));
            if (anonymousVotes.Load(section, orphanedEntries, out boolValue))
                sw.WriteLine(string.Format("{0},{1}", anonymousVotes.id, boolValue));
            if (taskBarMode.Load(section, orphanedEntries, out TaskBarMode taskBarModeValue))
                sw.WriteLine(string.Format("{0},{1}", taskBarMode.id, taskBarModeValue));
            if (isDefaults.Load(section, orphanedEntries, out boolValue))
                sw.WriteLine(string.Format("{0},{1}", isDefaults.id, boolValue));
        }

        public static void Remove(string section, Dictionary<ConfigDefinition, string> orphanedEntries, bool isSave = false)
        {
            // Generic options
            mapId.Remove(section, orphanedEntries);
            playerSpeedMod.Remove(section, orphanedEntries);
            crewLightMod.Remove(section, orphanedEntries);
            impostorLightMod.Remove(section, orphanedEntries);
            killCooldown.Remove(section, orphanedEntries);
            numCommonTasks.Remove(section, orphanedEntries);
            numLongTasks.Remove(section, orphanedEntries);
            numShortTasks.Remove(section, orphanedEntries);
            numEmergencyMeetings.Remove(section, orphanedEntries);
            emergencyCooldown.Remove(section, orphanedEntries);
            numImpostors.Remove(section, orphanedEntries);
            ghostsDoTasks.Remove(section, orphanedEntries);
            killDistance.Remove(section, orphanedEntries);
            discussionTime.Remove(section, orphanedEntries);
            votingTime.Remove(section, orphanedEntries);
            confirmImpostor.Remove(section, orphanedEntries);
            visualTasks.Remove(section, orphanedEntries);
            anonymousVotes.Remove(section, orphanedEntries);
            taskBarMode.Remove(section, orphanedEntries);
            isDefaults.Remove(section, orphanedEntries);

            if (isSave)
                TheOtherRolesPlugin.Instance.Config.Save();
        }

        public static void Init()
        {
            defaultData = new NormalGameOptionsV10(null);

            // Generic options : 890000000-
            //keywords = new Option<InnerNet.GameKeywords>(890000000, defaultData.Keywords);
            //maxPlayers = new Option<int>(890000001, defaultData.MaxPlayers);
            mapId = new Option<byte>(890000002, defaultData.MapId);
            playerSpeedMod = new Option<float>(890000003, defaultData.PlayerSpeedMod);
            crewLightMod = new Option<float>(890000004, defaultData.CrewLightMod);
            impostorLightMod = new Option<float>(890000005, defaultData.ImpostorLightMod);
            killCooldown = new Option<float>(890000006, defaultData.KillCooldown);
            numCommonTasks = new Option<int>(890000007, defaultData.NumCommonTasks);
            numLongTasks = new Option<int>(890000008, defaultData.NumLongTasks);
            numShortTasks = new Option<int>(890000009, defaultData.NumShortTasks);
            numEmergencyMeetings = new Option<int>(890000010, defaultData.NumEmergencyMeetings);
            emergencyCooldown = new Option<int>(890000011, defaultData.EmergencyCooldown);
            numImpostors = new Option<int>(890000012, defaultData.NumImpostors);
            ghostsDoTasks = new Option<bool>(890000013, defaultData.GhostsDoTasks);
            killDistance = new Option<int>(890000014, defaultData.KillDistance);
            discussionTime = new Option<int>(890000015, defaultData.DiscussionTime);
            votingTime = new Option<int>(890000016, defaultData.VotingTime);
            confirmImpostor = new Option<bool>(890000017, defaultData.ConfirmImpostor);
            visualTasks = new Option<bool>(890000018, defaultData.VisualTasks);
            anonymousVotes = new Option<bool>(890000019, defaultData.AnonymousVotes);
            taskBarMode = new Option<TaskBarMode>(890000020, (TaskBarMode)defaultData.TaskBarMode);
            isDefaults = new Option<bool>(890000021, defaultData.IsDefaults);
        }

        static void Save(int id, bool value, Dictionary<int, string> optionTable, StreamWriter sw)
        {
            var v = value;
            if (optionTable.TryGetValue(id, out string s))
                bool.TryParse(s, out v);
            else
                optionTable[id] = value.ToString();
            sw.WriteLine(string.Format("{0},{1}", id, v));
        }

        static void Save(int id, byte value, Dictionary<int, string> optionTable, StreamWriter sw)
        {
            var v = value;
            if (optionTable.TryGetValue(id, out string s))
                byte.TryParse(s, out v);
            else
                optionTable[id] = value.ToString();
            sw.WriteLine(string.Format("{0},{1}", id, v));
        }

        static void Save(int id, int value, Dictionary<int, string> optionTable, StreamWriter sw)
        {
            var v = value;
            if (optionTable.TryGetValue(id, out string s))
                int.TryParse(s, out v);
            else
                optionTable[id] = value.ToString();
            sw.WriteLine(string.Format("{0},{1}", id, v));
        }

        static void Save(int id, float value, Dictionary<int, string> optionTable, StreamWriter sw)
        {
            var v = value;
            if (optionTable.TryGetValue(id, out string s))
                float.TryParse(s, out v);
            else
                optionTable[id] = value.ToString();
            sw.WriteLine(string.Format("{0},{1}", id, v));
        }

        static void Save(int id, TaskBarMode value, Dictionary<int, string> optionTable, StreamWriter sw)
        {
            var v = value;
            if (optionTable.TryGetValue(id, out string s))
                Enum.TryParse(s, out v);
            else
                optionTable[id] = value.ToString();
            sw.WriteLine(string.Format("{0},{1}", id, v));
        }

        class Option<T>
        {
            public int id { get; private set; }
            public T defaultData { get; private set; }

            public Option(int id, T defaultData)
            {
                this.id = id;
                this.defaultData = defaultData;
            }

            public void Save(string section, Dictionary<ConfigDefinition, string> orphanedEntries, T value)
            {
                var configDefinition = new ConfigDefinition(section, id.ToString());
                if (!orphanedEntries.ContainsKey(configDefinition))
                    orphanedEntries.Add(configDefinition, value.ToString());
                else
                    orphanedEntries[configDefinition] = value.ToString();
            }

            public bool Load(string section, Dictionary<ConfigDefinition, string> orphanedEntries, out int outValue)
            {
                outValue = 0;
                var configDefinition = new ConfigDefinition(section, id.ToString());
                return orphanedEntries.TryGetValue(configDefinition, out string value) && int.TryParse(value, out outValue);
            }

            public bool Load(string section, Dictionary<ConfigDefinition, string> orphanedEntries, out float outValue)
            {
                outValue = 0;
                var configDefinition = new ConfigDefinition(section, id.ToString());
                return orphanedEntries.TryGetValue(configDefinition, out string value) && float.TryParse(value, out outValue);
            }

            public bool Load(string section, Dictionary<ConfigDefinition, string> orphanedEntries, out byte outValue)
            {
                outValue = 0;
                var configDefinition = new ConfigDefinition(section, id.ToString());
                return orphanedEntries.TryGetValue(configDefinition, out string value) && byte.TryParse(value, out outValue);
            }

            public bool Load(string section, Dictionary<ConfigDefinition, string> orphanedEntries, out bool outValue)
            {
                outValue = false;
                var configDefinition = new ConfigDefinition(section, id.ToString());
                return orphanedEntries.TryGetValue(configDefinition, out string value) && bool.TryParse(value, out outValue);
            }

            public bool Load(string section, Dictionary<ConfigDefinition, string> orphanedEntries, out TaskBarMode outValue)
            {
                outValue = TaskBarMode.Normal;
                var configDefinition = new ConfigDefinition(section, id.ToString());
                return orphanedEntries.TryGetValue(configDefinition, out string value) && TaskBarMode.TryParse(value, out outValue);
            }

            public bool Remove(string section, Dictionary<ConfigDefinition, string> orphanedEntries)
            {
                var configDefinition = new ConfigDefinition(section, id.ToString());
                if (orphanedEntries.ContainsKey(configDefinition))
                {
                    orphanedEntries.Remove(configDefinition);
                    return true;
                }
                return false;
            }
        }

        // Generic options
        //static Option<InnerNet.GameKeywords> keywords;
        //static Option<int> maxPlayers;
        static Option<byte> mapId;
        static Option<float> playerSpeedMod;
        static Option<float> crewLightMod;
        static Option<float> impostorLightMod;
        static Option<float> killCooldown;
        static Option<int> numCommonTasks;
        static Option<int> numLongTasks;
        static Option<int> numShortTasks;
        static Option<int> numEmergencyMeetings;
        static Option<int> emergencyCooldown;
        static Option<int> numImpostors;
        static Option<bool> ghostsDoTasks;
        static Option<int> killDistance;
        static Option<int> discussionTime;
        static Option<int> votingTime;
        static Option<bool> confirmImpostor;
        static Option<bool> visualTasks;
        static Option<bool> anonymousVotes;
        static Option<TaskBarMode> taskBarMode;
        static Option<bool> isDefaults;

        static NormalGameOptionsV10 defaultData = null;
    }
}
