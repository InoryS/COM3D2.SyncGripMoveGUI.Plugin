using System;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.VR;
using CM3D2.GripMovePlugin.Plugin;

namespace COM3D2.SyncGripMoveGUI.Plugin
{
    [BepInPlugin("com.inorys.syncgripmovegui", "COM3D2.SyncGripMoveGUI.Plugin", "1.0.4.1")]
    public class SyncGripMoveGUI : BaseUnityPlugin
    {
        public static new ManualLogSource Logger;
        private static MethodInfo _toggleGUIVisibleMethod;
        private static bool isVRMode;

        // Subscribe to scene loaded event to update the cached scene index
        static SyncGripMoveGUI()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {
            // SceneIndex 9 is SceneTitle
            if (scene.buildIndex == 9)
            {
                ExecuteStartFunctionality();
            }
        }


        private void Awake()
        {
            Logger = base.Logger;
            isVRMode = Environment.CommandLine.ToLower().Contains("/vr");

            if (!isVRMode)
            {
                Debug.Log("Is NOT VR Mode. Shutdown COM3D2.SyncGripMoveGUI.Plugin.");
                return;
            }

            Logger.LogInfo("COM3D2.SyncGripMoveGUI.Plugin Loaded");

            //Use Harmony to hook OvrTablet's SetVisible method
            Harmony harmony = new Harmony("com.inorys.syncgripmovegui");
            harmony.PatchAll();
        }

        private static void ExecuteStartFunctionality()
        {
            if (!isVRMode) return;
            // Get reflection information of GUIQuad.switchVisibility method
            Type guiQuadType = AccessTools.TypeByName("CM3D2.GripMovePlugin.Plugin.GUIQuad");
            if (guiQuadType != null)
            {
                Logger.LogInfo("Find type CM3D2.GripMovePlugin.Plugin.GUIQuad success.");
                _toggleGUIVisibleMethod =
                    guiQuadType.GetMethod("switchVisibility", BindingFlags.NonPublic | BindingFlags.Instance);
                if (_toggleGUIVisibleMethod == null)
                {
                    Logger.LogError(
                        "CM3D2.GripMovePlugin.Plugin.GUIQuad.switchVisibility method not found in GUIQuad.");
                }
                else
                {
                    Logger.LogInfo("Find CM3D2.GripMovePlugin.Plugin.GUIQuad.switchVisibility method success.");
                }
            }
            else
            {
                Logger.LogError("CM3D2.GripMovePlugin.Plugin.GUIQuad type not found.");
            }
        }

        [HarmonyPatch(typeof(OvrTablet), "SetVisible")]
        public static class OvrTabletSetVisiblePatch
        {
            static void Postfix(OvrTablet __instance, bool f_bVisible)
            {
                if (!isVRMode) return;
                // Called after the SetVisible method executes
                if (_toggleGUIVisibleMethod != null)
                {
                    try
                    {
                        _toggleGUIVisibleMethod.Invoke(null, new object[] { f_bVisible });
                        Logger.LogInfo(
                            $"Old GUI visibility toggled due to OvrTablet visibility change. New visibility: {f_bVisible}");
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