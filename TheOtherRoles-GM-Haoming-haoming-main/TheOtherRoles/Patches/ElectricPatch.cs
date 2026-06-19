using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hazel;
using UnityEngine;
using static TheOtherRoles.GameHistory;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Patches
{
    [Harmony]
    public class ElectricPatch
    {
        public static bool isOntask()
        {
            return Camera.main.gameObject.GetComponentInChildren<SwitchMinigame>() != null;
        }
    }
}
