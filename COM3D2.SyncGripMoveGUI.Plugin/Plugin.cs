using System;
using BepInEx;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using System.Linq;
using System.Globalization;

namespace COM3D2.SyncGripMoveGUI.Plugin
{
    [BepInPlugin("COM3D2.SyncGripMoveGUI.Plugin", "COM3D2.SyncGripMoveGUI.Plugin", "1.0.9.0")]
    public class SyncGripMoveGUI : BaseUnityPlugin
    {
        public static bool isOfficalTableVisible = false;
        public static bool isDirectModeActive = false;

        private bool previousIsDirectModeActive = false;
        private Transform leftHandAnchor;
        private Transform rightHandAnchor;
        private static PropertyInfo isVisbleProperty = null;

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

            // 获取左手和右手控制器的 Transform
            leftHandAnchor = GameMain.Instance.OvrMgr.GetVRControllerTransform(true);
            rightHandAnchor = GameMain.Instance.OvrMgr.GetVRControllerTransform(false);

            // 缓存控制器组件
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

        void Update()
        {
            isDirectModeActive = IsDirectMode();
            if (isDirectModeActive != previousIsDirectModeActive)
            {
                previousIsDirectModeActive = isDirectModeActive;
                if (isOfficalTableVisible && isDirectModeActive)
                {
                    ChangeOldGUIVisible(true);
                }
                Debug.Log("Direct Mode 状态已更改为: " + isDirectModeActive);
            }
        }

        private static void CacheReflectionData()
        {
            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                var gripMoveAssembly =
                    assemblies.FirstOrDefault(a => a.GetType("CM3D2.GripMovePlugin.Plugin.IMGUIQuad") != null);
                if (gripMoveAssembly != null)
                {
                    var imguiQuadType = gripMoveAssembly.GetType("CM3D2.GripMovePlugin.Plugin.IMGUIQuad");
                    isVisbleProperty = imguiQuadType?.GetProperty("IsVisble",
                        BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    if (isVisbleProperty == null)
                    {
                        Debug.LogError("SyncGripMoveGUIPlugin Error: IsVisble Property not found.");
                    }
                }
                else
                {
                    Debug.LogError("SyncGripMoveGUIPlugin Error: GripMovePlugin assembly not found.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("SyncGripMoveGUIPlugin: CacheReflectionData Error : " + ex);
            }
        }

        private bool IsDirectMode()
        {
            bool isDirect = false;
            if (GameMain.Instance.VRFamily == GameMain.VRFamilyType.Oculus)
            {
                if (!leftOvrController)
                {
                    leftOvrController = leftHandAnchor?.gameObject.GetComponent<OvrController>();
                }

                if (!rightOvrController)
                {
                    rightOvrController = rightHandAnchor?.gameObject.GetComponent<OvrController>();
                }

                if ((!leftOvrController || !leftOvrController.enabled) ||
                    (!rightOvrController || !rightOvrController.enabled))
                {
                    isDirect = true;
                }
            }
            else if (GameMain.Instance.VRFamily == GameMain.VRFamilyType.HTC)
            {
                if (!leftViveController)
                {
                    leftViveController = leftHandAnchor?.gameObject.GetComponent<ViveController>();
                }

                if (!rightViveController)
                {
                    rightViveController = rightHandAnchor?.gameObject.GetComponent<ViveController>();
                }

                if ((!leftViveController || !leftViveController.enabled) ||
                    (!rightViveController || !rightViveController.enabled))
                {
                    isDirect = true;
                }
            }

            return isDirect;
        }

        public static void ChangeOldGUIVisible(bool visible)
        {
            try
            {
                if (isVisbleProperty == null)
                {
                    CacheReflectionData();
                }

                isVisbleProperty?.SetValue(
                    obj: null,
                    value: visible,
                    invokeAttr: BindingFlags.Static | BindingFlags.SetProperty | BindingFlags.NonPublic |
                                BindingFlags.Public,
                    binder: null,
                    index: null,
                    culture: CultureInfo.InvariantCulture
                );
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
                SyncGripMoveGUI.isOfficalTableVisible = f_bShow;
                if (!f_bShow)
                {
                    SyncGripMoveGUI.ChangeOldGUIVisible(f_bShow);
                }
            }
        }
    }
}