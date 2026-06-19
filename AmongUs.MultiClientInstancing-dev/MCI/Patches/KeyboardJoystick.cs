using HarmonyLib;
using UnityEngine;

namespace MCI.Patches;

[HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
public static class Keyboard_Joystick
{
    public static int ControllingFigure;

    public static void Postfix()
    {
        if (!MCIPlugin.Enabled)
            return;

        if (Input.GetKeyDown(KeyCode.F5))
        {
            CreatePlayer();
        }

        if (Input.GetKeyDown(KeyCode.F9))
        {
            Switch(true);
        }

        if (Input.GetKeyDown(KeyCode.F10))
        {
            Switch(false);
        }

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.F6))
        {
            IKnowWhatImDoingToggle();
        }
        else if (Input.GetKeyDown(KeyCode.F6))
            MCIPlugin.Persistence = !MCIPlugin.Persistence;

        if (Input.GetKeyDown(KeyCode.F11))
            InstanceControl.RemoveAllPlayers();
    }
    
    internal static void IKnowWhatImDoingToggle()
    {
        MCIPlugin.IKnowWhatImDoing = !MCIPlugin.IKnowWhatImDoing;
        InstanceControl.UpdateNames(MCIPlugin.RobotName);
    }

    internal static void CreatePlayer()
    {
        ControllingFigure = PlayerControl.LocalPlayer.PlayerId;

        if (PlayerControl.AllPlayerControls.Count == 15 && !Input.GetKeyDown(KeyCode.LeftShift))
            return; //press f6 and f5 to bypass limit

        InstanceControl.CleanUpLoad();
        InstanceControl.CreatePlayerInstance();
    }

    internal static void Switch(bool increment)
    {
        if (LobbyBehaviour.Instance)
            return;

        Cycle(increment);
        InstanceControl.SwitchTo((byte)ControllingFigure);
    }

    private static void Cycle(bool increment)
    {
        if (increment)
            ControllingFigure++;
        else
            ControllingFigure--;

        if (ControllingFigure < 0)
            ControllingFigure = InstanceControl.Clients.Count - 1;
        else if (ControllingFigure > InstanceControl.Clients.Count)
            ControllingFigure = 0;
    }
}
