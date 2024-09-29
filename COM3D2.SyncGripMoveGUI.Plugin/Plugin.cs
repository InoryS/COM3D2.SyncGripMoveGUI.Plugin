using System;
using BepInEx;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using System.Linq;
using System.Globalization;

namespace COM3D2.SyncGripMoveGUI.Plugin
{
    [BepInPlugin("COM3D2.SyncGripMoveGUI.Plugin", "COM3D2.SyncGripMoveGUI.Plugin", "1.0.5.0")]
    public class SyncGripMoveGUI : BaseUnityPlugin
    {
        void Awake()
        {
            if (!Environment.CommandLine.ToLower().Contains("/vr"))
            {
                Debug.Log("Is NOT VR Mode. Shutdown COM3D2.SyncGripMoveGUI.Plugin.");
                return;
            }

            // 创建 Harmony 实例并应用补丁
            var harmony = new Harmony("COM3D2.SyncGripMoveGUI.Plugin");
            harmony.PatchAll();
        }
    }

    [HarmonyPatch]
    public class OvrCamera_ShowUI_Patch
    {
        // 指定要补丁的方法为 OvrCamera 类的 ShowUI(bool f_bShow) 方法
        static MethodBase TargetMethod()
        {
            var ovrCameraType = typeof(OvrCamera);
            return ovrCameraType.GetMethod("ShowUI",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        // 在 ShowUI 方法执行后调用
        public static void Postfix(bool f_bShow)
        {
            try
            {
                // 获取所有已加载的程序集
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();

                // 找到 GripMovePlugin 所在的程序集
                var gripMoveAssembly =
                    assemblies.FirstOrDefault(a => a.GetType("CM3D2.GripMovePlugin.Plugin.GripMovePlugin") != null);

                if (gripMoveAssembly != null)
                {
                    // 通过反射获取 IMGUIQuad 类型
                    var imguiQuadType = gripMoveAssembly.GetType("CM3D2.GripMovePlugin.Plugin.IMGUIQuad");

                    if (imguiQuadType != null)
                    {
                        // 获取 IsVisble 属性
                        var isVisbleProperty = imguiQuadType.GetProperty("IsVisble",
                            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                        if (isVisbleProperty != null)
                        {
                            // 设置 IsVisble 属性的值，使其与游戏 UI 的可见性同步
                            isVisbleProperty.SetValue(
                                obj: null,
                                value: f_bShow,
                                invokeAttr: BindingFlags.Static | BindingFlags.SetProperty | BindingFlags.NonPublic |
                                            BindingFlags.Public,
                                binder: null,
                                index: null,
                                culture: CultureInfo.InvariantCulture
                            );
                        }
                        else
                        {
                            Debug.LogError("SyncGripMoveGUIPlugin Error: IsVisble property not found.");
                        }
                    }
                    else
                    {
                        Debug.LogError("SyncGripMoveGUIPlugin Error: IMGUIQuad type not found.");
                    }
                }
                else
                {
                    Debug.LogError("SyncGripMoveGUIPlugin Error: GripMovePlugin assembly not found.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("SyncGripMoveGUIPlugin Error: " + ex);
            }
        }
    }
}