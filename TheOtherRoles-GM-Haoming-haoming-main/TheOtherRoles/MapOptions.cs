using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheOtherRoles
{
    static class TORMapOptions
    {
        // Set values
        public static int maxNumberOfMeetings = 10;
        public static bool blockSkippingInEmergencyMeetings = false;
        public static bool noVoteIsSelfVote = false;
        public static bool hidePlayerNames = false;
        public static bool hideSettings = false;
        public static bool hideOutOfSightNametags = false;

        public static bool randomizeColors = false;
        public static bool allowDupeNames = false;

        public static int restrictDevices = 0;
        public static bool restrictAdmin = true;
        public static float restrictAdminTime = 600f;
        public static float restrictAdminTimeMax = 600f;
        public static bool restrictAdminText = true;
        public static bool restrictCameras = true;
        public static float restrictCamerasTime = 600f;
        public static float restrictCamerasTimeMax = 600f;
        public static bool restrictCamerasText = true;
        public static bool restrictVitals = true;
        public static float restrictVitalsTime = 600f;
        public static float restrictVitalsTimeMax = 600f;
        public static bool restrictVitalsText = true;
        public static bool disableVents = false;

        public static bool ghostsSeeRoles = true;
        public static bool ghostsSeeTasks = true;
        public static bool ghostsSeeVotes = true;
        public static bool showRoleSummary = true;
        public static bool hideNameplates = false;
        public static bool allowParallelMedBayScans = false;
        public static bool showLighterDarker = false;
        public static bool showChatNotifications = true;
        public static bool hideTaskArrows = false;
        public static bool offlineHats = false;
        public static bool hideFakeTasks = false;
        public static bool betterSabotageMap = false;
        public static bool forceNormalSabotageMap = false;
        public static bool transparentMap = false;

        // Updating values
        public static int meetingsCount = 0;
        public static List<SurvCamera> camerasToAdd = new();
        public static List<Vent> ventsToSeal = new();
        public static Dictionary<byte, PoolablePlayer> playerIcons = new();
        public static TMPro.TextMeshPro AdminTimerText = null;
        public static TMPro.TextMeshPro CamerasTimerText = null;
        public static TMPro.TextMeshPro VitalsTimerText = null;

        public static void clearAndReloadTORMapOptions()
        {
            meetingsCount = 0;
            camerasToAdd = new List<SurvCamera>();
            ventsToSeal = new List<Vent>();
            playerIcons = new Dictionary<byte, PoolablePlayer>();

            maxNumberOfMeetings = Mathf.RoundToInt(CustomOptionHolder.maxNumberOfMeetings.getSelection());
            blockSkippingInEmergencyMeetings = CustomOptionHolder.blockSkippingInEmergencyMeetings.getBool();
            noVoteIsSelfVote = CustomOptionHolder.noVoteIsSelfVote.getBool();
            hidePlayerNames = CustomOptionHolder.hidePlayerNames.getBool();

            hideOutOfSightNametags = CustomOptionHolder.hideOutOfSightNametags.getBool();

            hideSettings = CustomOptionHolder.hideSettings.getBool();

            randomizeColors = CustomOptionHolder.uselessOptions.getBool() && CustomOptionHolder.playerColorRandom.getBool();
            allowDupeNames = CustomOptionHolder.uselessOptions.getBool() && CustomOptionHolder.playerNameDupes.getBool();

            restrictDevices = CustomOptionHolder.restrictDevices.getSelection();
            restrictAdmin = CustomOptionHolder.restrictAdmin.getBool();
            restrictAdminTime = restrictAdminTimeMax = CustomOptionHolder.restrictAdminTime.getFloat();
            restrictAdminText = CustomOptionHolder.restrictAdminText.getBool();
            restrictCameras = CustomOptionHolder.restrictCameras.getBool();
            restrictCamerasTime = restrictCamerasTimeMax = CustomOptionHolder.restrictCamerasTime.getFloat();
            restrictCamerasText = CustomOptionHolder.restrictCamerasText.getBool();
            restrictVitalsText = CustomOptionHolder.restrictVitalsText.getBool();
            restrictVitals = CustomOptionHolder.restrictVitals.getBool();
            restrictVitalsTime = restrictVitalsTimeMax = CustomOptionHolder.restrictVitalsTime.getFloat();
            disableVents = CustomOptionHolder.disableVents.getBool();
            ClearTimerText();
            UpdateTimerText();

            allowParallelMedBayScans = CustomOptionHolder.allowParallelMedBayScans.getBool();
            ghostsSeeRoles = TheOtherRolesPlugin.GhostsSeeRoles.Value;
            ghostsSeeTasks = TheOtherRolesPlugin.GhostsSeeTasks.Value;
            ghostsSeeVotes = TheOtherRolesPlugin.GhostsSeeVotes.Value;
            showRoleSummary = TheOtherRolesPlugin.ShowRoleSummary.Value;
            hideNameplates = TheOtherRolesPlugin.HideNameplates.Value;
            showLighterDarker = TheOtherRolesPlugin.ShowLighterDarker.Value;
            showChatNotifications = TheOtherRolesPlugin.ShowChatNotifications.Value;
            hideTaskArrows = TheOtherRolesPlugin.HideTaskArrows.Value;
        }

        public static void resetDeviceTimes()
        {
            restrictAdminTime = restrictAdminTimeMax;
            restrictCamerasTime = restrictCamerasTimeMax;
            restrictVitalsTime = restrictVitalsTimeMax;
        }

        public static bool canUseAdmin
        {
            get
            {
                return restrictDevices == 0 || restrictAdminTime > 0f;
            }
        }

        public static bool couldUseAdmin
        {
            get
            {
                return restrictDevices == 0 || !restrictAdmin || restrictAdminTimeMax > 0f;
            }
        }

        public static bool canUseCameras
        {
            get
            {
                return restrictDevices == 0 || !restrictCameras || restrictCamerasTime > 0f;
            }
        }

        public static bool couldUseCameras
        {
            get
            {
                return restrictDevices == 0 || !restrictCameras || restrictCamerasTimeMax > 0f;
            }
        }

        public static bool canUseVitals
        {
            get
            {
                return restrictDevices == 0 || !restrictVitals || restrictVitalsTime > 0f;
            }
        }

        public static bool couldUseVitals
        {
            get
            {
                return restrictDevices == 0 || !restrictVitals || restrictVitalsTimeMax > 0f;
            }
        }
        public static void MeetingEndedUpdate()
        {
            ClearTimerText();
            UpdateTimerText();
        }

        public static void UpdateTimerText()
        {
            if (restrictDevices == 0 || (!restrictAdminText && !restrictCamerasText && !restrictVitalsText))
                return;
            if (FastDestroyableSingleton<HudManager>.Instance == null)
                return;

            // Admin
            if (restrictAdminText)
            {
                AdminTimerText = UnityEngine.Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.TaskPanel.taskText, FastDestroyableSingleton<HudManager>.Instance.transform);
                float y = -4.0f;
                if (restrictCamerasText)
                    y += 0.2f;
                if (restrictVitalsText)
                    y += 0.2f;
                AdminTimerText.transform.localPosition = new Vector3(-3.5f, y, 0);
                if (restrictAdminTime > 0)
                    AdminTimerText.text = String.Format(ModTranslation.getString("adminText"), restrictAdminTime.ToString("0.00"));
                else
                    AdminTimerText.text = ModTranslation.getString("adminRanOut");
                AdminTimerText.gameObject.SetActive(true);
            }

            // Cameras
            if (restrictCamerasText)
            {
                CamerasTimerText = UnityEngine.Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.TaskPanel.taskText, FastDestroyableSingleton<HudManager>.Instance.transform);
                float y = -4.0f;
                if (restrictVitalsText)
                    y += 0.2f;
                CamerasTimerText.transform.localPosition = new Vector3(-3.5f, y, 0);
                if (restrictCamerasTime > 0)
                    CamerasTimerText.text = String.Format(ModTranslation.getString("camerasText"), restrictCamerasTime.ToString("0.00"));
                else
                    CamerasTimerText.text = ModTranslation.getString("camerasRanOut");
                CamerasTimerText.gameObject.SetActive(true);
            }

            // Vitals
            if (restrictVitalsText)
            {
                VitalsTimerText = UnityEngine.Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.TaskPanel.taskText, FastDestroyableSingleton<HudManager>.Instance.transform);
                VitalsTimerText.transform.localPosition = new Vector3(-3.5f, -4.0f, 0);
                if (restrictVitalsTime > 0)
                    VitalsTimerText.text = String.Format(ModTranslation.getString("vitalsText"), restrictVitalsTime.ToString("0.00"));
                else
                    VitalsTimerText.text = ModTranslation.getString("vitalsRanOut");
                VitalsTimerText.gameObject.SetActive(true);
            }
        }

        private static void ClearTimerText()
        {
            if (AdminTimerText != null)
                UnityEngine.Object.Destroy(AdminTimerText);
            AdminTimerText = null;
            if (CamerasTimerText != null)
                UnityEngine.Object.Destroy(CamerasTimerText);
            CamerasTimerText = null;
            if (VitalsTimerText != null)
                UnityEngine.Object.Destroy(VitalsTimerText);
            VitalsTimerText = null;

        }
    }
}
