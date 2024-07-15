using System;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.VR;

namespace COM3D2.SyncGripMoveGUI.Plugin
{
    [BepInPlugin("com.inorys.syncgripmovegui", "COM3D2.SyncGripMoveGUI.Plugin", "1.0.2")]
    public class SyncGripMoveGUI : BaseUnityPlugin
    {
        public static new ManualLogSource Logger;
        private static MethodInfo _toggleGUIVisibleMethod;

        private void Awake()
        {
            Logger = base.Logger;
            Logger.LogInfo("COM3D2.SyncGripMoveGUI.Plugin Loaded");

            //Use Harmony to hook OvrTablet's SetVisible method
            Harmony harmony = new Harmony("com.inorys.syncgripmovegui");
            harmony.PatchAll();
        }

        private void Start()
        {
            // Get reflection information of GUIQuad.switchVisibility method
            Type guiQuadType = Type.GetType("CM3D2.GripMovePlugin.Plugin.GUIQuad, CM3D2.GripMovePlugin.Plugin");
            if (guiQuadType != null)
            {
                _toggleGUIVisibleMethod = guiQuadType.GetMethod("switchVisibility", BindingFlags.Public | BindingFlags.Static);
                if (_toggleGUIVisibleMethod == null)
                {
                    Logger.LogError("switchVisibility method not found in GUIQuad.");
                }
            }
            else
            {
                Logger.LogError("GUIQuad type not found.");
            }
        }

        [HarmonyPatch(typeof(OvrTablet), "SetVisible")]
        public static class OvrTabletSetVisiblePatch
        {
            static void Postfix(OvrTablet __instance, bool f_bVisible)
            {
                // Called after the SetVisible method executes
                if (_toggleGUIVisibleMethod != null)
                {
                    try
                    {
                        _toggleGUIVisibleMethod.Invoke(null, new object[] { f_bVisible });
                        Logger.LogInfo($"Old GUI visibility toggled due to OvrTablet visibility change. New visibility: {f_bVisible}");
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Error invoking switchVisibility: {ex.Message}");
                    }
                }
            }
        }
    }
}
