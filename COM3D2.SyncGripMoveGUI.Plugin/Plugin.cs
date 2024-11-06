using System;
using BepInEx;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using System.Linq;
using System.Globalization;

[assembly: AssemblyVersion("1.1.0.0")]
namespace COM3D2.SyncGripMoveGUI.Plugin
{
    [BepInPlugin("COM3D2.SyncGripMoveGUI.Plugin", "COM3D2.SyncGripMoveGUI.Plugin", "1.1.0.0")]
    public class SyncGripMoveGUI : BaseUnityPlugin
    {
        public static bool isOfficialTableVisible = false;
        public static bool isDirectModeActive = false;

        private bool previousIsDirectModeActive = false;

        private Transform leftHandAnchor;
        private Transform rightHandAnchor;
        private static PropertyInfo isVisbleProperty;

        private ViveController leftViveController;
        private ViveController rightViveController;
        private OvrController leftOvrController;
        private OvrController rightOvrController;

        void Start()
        {
            if (!Environment.CommandLine.ToLower().Contains("vr"))
            {
                Debug.Log("Not in VR mode. Shutting down COM3D2.SyncGripMoveGUI.Plugin.");
                return;
            }

            var harmony = new Harmony("COM3D2.SyncGripMoveGUI.Plugin");
            harmony.PatchAll();
            CacheReflectionData();

            leftHandAnchor = GameMain.Instance.OvrMgr.GetVRControllerTransform(true);
            rightHandAnchor = GameMain.Instance.OvrMgr.GetVRControllerTransform(false);

            InitializeControllers();
        }

        void Update()
        {
            isDirectModeActive = IsDirectMode();
            if (isDirectModeActive != previousIsDirectModeActive)
            {
                previousIsDirectModeActive = isDirectModeActive;

                if (isOfficialTableVisible && isDirectModeActive)
                {
                    ChangeOldGUIVisible(true);
                }
                else
                {
                    ChangeOldGUIVisible(false);
                }

                // Debug.Log("Direct Mode Change to " + isDirectModeActive);
            }
        }

        private void CacheReflectionData()
        {
            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                var gripMoveAssembly =
                    assemblies.FirstOrDefault(a => a.GetType("CM3D2.GripMovePlugin.Plugin.IMGUIQuad") != null);
                if (gripMoveAssembly == null)
                {
                    Debug.LogError("SyncGripMoveGUIPlugin Error: GripMovePlugin assembly not found.");
                    return;
                }

                var imguiQuadType = gripMoveAssembly.GetType("CM3D2.GripMovePlugin.Plugin.IMGUIQuad");
                isVisbleProperty = imguiQuadType?.GetProperty("IsVisble",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (isVisbleProperty == null)
                {
                    Debug.LogError("SyncGripMoveGUIPlugin Error: IMGUIQuad IsVisble Property not found.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("SyncGripMoveGUIPlugin: CacheReflectionData Error : " + ex);
            }
        }

        private void InitializeControllers()
        {
            if (GameMain.Instance.VRFamily == GameMain.VRFamilyType.Oculus)
            {
                leftOvrController = leftHandAnchor?.gameObject.GetComponent<OvrController>();
                rightOvrController = rightHandAnchor?.gameObject.GetComponent<OvrController>();
            }
            else if (GameMain.Instance.VRFamily == GameMain.VRFamilyType.HTC)
            {
                leftViveController = leftHandAnchor?.gameObject.GetComponent<ViveController>();
                rightViveController = rightHandAnchor?.gameObject.GetComponent<ViveController>();
            }
        }

        private bool IsDirectMode()
        {
            // Check if in Direct mode of GripMove
            return GameMain.Instance.VRFamily switch
            {
                GameMain.VRFamilyType.Oculus => CheckOvrControllers(),
                GameMain.VRFamilyType.HTC => CheckViveControllers(),
                _ => false
            };
        }

        private bool CheckOvrControllers()
        {
            if (!leftOvrController)
            {
                leftOvrController = leftHandAnchor?.gameObject.GetComponent<OvrController>();
            }

            if (!rightOvrController)
            {
                rightOvrController = rightHandAnchor?.gameObject.GetComponent<OvrController>();
            }

            return (!leftOvrController || !leftOvrController.enabled) ||
                   (!rightOvrController || !rightOvrController.enabled);
        }

        private bool CheckViveControllers()
        {
            if (!leftViveController)
            {
                leftViveController = leftHandAnchor?.gameObject.GetComponent<ViveController>();
            }

            if (!rightViveController)
            {
                rightViveController = rightHandAnchor?.gameObject.GetComponent<ViveController>();
            }

            return (!leftViveController || !leftViveController.enabled) ||
                   (!rightViveController || !rightViveController.enabled);
        }

        public static void ChangeOldGUIVisible(bool visible)
        {
            try
            {
                if (isVisbleProperty == null)
                {
                    // Ensure isVisible Property is up to date
                    var instance = new SyncGripMoveGUI();
                    instance.CacheReflectionData();
                }

                // This property controls whether the OldGU (IMGUI) of GripMove is visible.
                isVisbleProperty?.SetValue(null, visible,
                    BindingFlags.Static | BindingFlags.SetProperty | BindingFlags.NonPublic | BindingFlags.Public, null,
                    null, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                Debug.LogError("SyncGripMoveGUIPlugin Error: " + ex);
            }
        }

        [HarmonyPatch(typeof(OvrCamera), "ShowUI")]
        public class OvrCameraShowUIPatch
        {
            public static void Postfix(bool f_bShow)
            {
                // f_bShow Indicates whether the official tablet GUI is visible
                SyncGripMoveGUI.isOfficialTableVisible = f_bShow;
                if (!f_bShow)
                {
                    SyncGripMoveGUI.ChangeOldGUIVisible(f_bShow);
                }
            }
        }
    }
}