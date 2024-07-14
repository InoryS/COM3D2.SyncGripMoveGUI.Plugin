using System;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.VR;

namespace COM3D2.SyncGripMoveGUI.Plugin
{
    [BepInPlugin("com.inorys.syncgripnovegui", "COM3D2.SyncGripMoveGUI.Plugin", "1.0.1")]
    public class SyncGripMoveGUI : BaseUnityPlugin
    {
        public static ManualLogSource Logger;
        private void Awake()
        {
            Logger.LogInfo("SyncOvrTabletWithOldGUI Plugin Loaded");

            // Use Harmony to hook OvrTablet's SetVisible method
            Harmony harmony = new Harmony("com.inorys.syncgripnovegui");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(OvrTablet), "SetVisible")]
        public static class OvrTabletSetVisiblePatch
        {
            static MethodInfo _toggleIMGUIVisibleMethod;

            static OvrTabletSetVisiblePatch()
            {
                // Get the reflection information of the IMGUIQuad.ToggleIMGUIVisible method
                Type imguiQuadType = Type.GetType("CM3D2.GripMovePlugin.Plugin.IMGUIQuad, CM3D2.GripMovePlugin.Plugin");
                if (imguiQuadType != null)
                {
                    _toggleIMGUIVisibleMethod =
                        imguiQuadType.GetMethod("ToggleIMGUIVisible", BindingFlags.Public | BindingFlags.Static);
                    if (_toggleIMGUIVisibleMethod == null)
                    {
                        Logger.LogError("ToggleIMGUIVisible method not found in IMGUIQuad.");
                    }
                }
                else
                {
                    Logger.LogError("IMGUIQuad type not found.");
                }
            }

            static void Postfix(OvrTablet __instance, bool f_bVisible)
            {
                // Called after the SetVisible method is executed
                if (_toggleIMGUIVisibleMethod != null && !f_bVisible)
                {
                    _toggleIMGUIVisibleMethod.Invoke(null, null);
                    Logger.LogInfo("Old GUI visibility toggled due to OvrTablet visibility change.");
                }
            }
        }
    }
}