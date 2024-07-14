using System;
using System.Reflection;
using BepInEx;
//using BepInEx.Logging;
//using UnityEngine;
//using UnityEngine.VR;

namespace COM3D2.SyncGripMoveGUI.Plugin
{
    [BepInPlugin("com.inorys.syncgripnovegui", "COM3D2.SyncGripMoveGUI.Plugin", "1.0.0")]
    public class SyncGripMoveGUI : BaseUnityPlugin
    {
        private OvrTablet _ovrTablet;
        private FieldInfo _visibleField;
        private MethodInfo _toggleIMGUIVisibleMethod;
        private bool _previousVisibleState;
        private bool _isInitialized = false;

        private void Awake()
        {
            Logger.LogInfo("SyncOvrTabletWithOldGUI Plugin Loaded");
        }

        private void Start()
        {
            // Find the OvrTablet object at startup
            _ovrTablet = FindObjectOfType<OvrTablet>();

            // If the object is found, get the reflection information of the m_bVisible field
            if (_ovrTablet != null)
            {
                _visibleField = typeof(OvrTablet).GetField("m_bVisible", BindingFlags.NonPublic | BindingFlags.Instance);
                if (_visibleField != null)
                {
                    _previousVisibleState = (bool)_visibleField.GetValue(_ovrTablet);
                    UpdateOldGUIVisibility(_previousVisibleState);

                    // Get the reflection information of the IMGUIQuad.ToggleIMGUIVisible method
                    Type imguiQuadType =
                        Type.GetType("CM3D2.GripMovePlugin.Plugin.IMGUIQuad, CM3D2.GripMovePlugin.Plugin");
                    if (imguiQuadType != null)
                    {
                        _toggleIMGUIVisibleMethod = imguiQuadType.GetMethod("ToggleIMGUIVisible",
                            BindingFlags.Public | BindingFlags.Static);
                        if (_toggleIMGUIVisibleMethod != null)
                        {
                            _isInitialized = true;
                        }
                        else
                        {
                            Logger.LogError("ToggleIMGUIVisible method not found in IMGUIQuad.");
                        }
                    }
                    else
                    {
                        Logger.LogError("IMGUIQuad type not found.");
                    }
                }
                else
                {
                    Logger.LogError("m_bVisible field not found in OvrTablet.");
                }
            }
            else
            {
                Logger.LogError("OvrTablet object not found in the scene.");
            }
        }

        private void Update()
        {
            // Perform the check only after initialization succeeded
            if (_isInitialized)
            {
                bool currentVisibleState = (bool)_visibleField.GetValue(_ovrTablet);
                if (currentVisibleState != _previousVisibleState)
                {
                    _previousVisibleState = currentVisibleState;
                    UpdateOldGUIVisibility(_previousVisibleState);
                }
            }
        }

        private void UpdateOldGUIVisibility(bool isVisible)
        {
            // If m_bVisible is false, hide the old GUI
            if (!isVisible)
            {
                _toggleIMGUIVisibleMethod.Invoke(null, null);
                Logger.LogInfo("Old GUI visibility toggled due to OvrTablet visibility change.");
            }
        }
    }
}