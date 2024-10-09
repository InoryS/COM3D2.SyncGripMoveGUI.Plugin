using System;
using BepInEx;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using System.Linq;
using System.Globalization;
using BepInEx.Configuration;

namespace COM3D2.SyncGripMoveGUI.Plugin
{
    [BepInPlugin("COM3D2.SyncGripMoveGUI.Plugin", "COM3D2.SyncGripMoveGUI.Plugin", "1.0.7.0")]
    public class SyncGripMoveGUI : BaseUnityPlugin
    {
        private ConfigEntry<KeyboardShortcut> toggleVisibilityShortcut;
        private bool isVisible = false;
        private PropertyInfo isVisbleProperty;
        private Type imguiQuadType;

        void Start()
        {
            if (!Environment.CommandLine.ToLower().Contains("/vr"))
            {
                Debug.Log("Not in VR mode. Shutting down COM3D2.SyncGripMoveGUI.Plugin.");
                return;
            }

            // 初始化可配置按键，默认为 "G + I"
            toggleVisibilityShortcut = Config.Bind("Hotkeys", "Toggle GripMove IMGUI Visibility",
                new KeyboardShortcut(KeyCode.I, KeyCode.G),
                "The key or key combination to toggle GripMove IMGUI visibility.");

            // 创建 Harmony 实例并应用补丁
            var harmony = new Harmony("COM3D2.SyncGripMoveGUI.Plugin");
            harmony.PatchAll();

            // 缓存类型和属性，避免每次切换时都通过反射获取
            CacheIMGUIQuadTypeAndProperty();
        }

        void Update()
        {
            // 检查是否按下配置的按键或按键组合
            if (toggleVisibilityShortcut.Value.IsDown())
            {
                ToggleVisibility();
            }
        }

        // 缓存反射相关的信息
        private void CacheIMGUIQuadTypeAndProperty()
        {
            try
            {
                var gripMoveAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetType("CM3D2.GripMovePlugin.Plugin.GripMovePlugin") != null);

                if (gripMoveAssembly != null)
                {
                    imguiQuadType = gripMoveAssembly.GetType("CM3D2.GripMovePlugin.Plugin.IMGUIQuad");
                    if (imguiQuadType != null)
                    {
                        isVisbleProperty = imguiQuadType.GetProperty("IsVisble",
                            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
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
            catch (Exception ex)
            {
                Debug.LogError("SyncGripMoveGUIPlugin Error while caching: " + ex);
            }
        }

        private void ToggleVisibility()
        {
            if (isVisbleProperty != null)
            {
                try
                {
                    isVisible = !isVisible;
                    isVisbleProperty.SetValue(null, isVisible,
                        BindingFlags.Static | BindingFlags.SetProperty | BindingFlags.NonPublic | BindingFlags.Public,
                        null, null, CultureInfo.InvariantCulture);
                    Debug.Log("Toggled IsVisble to: " + isVisible);
                }
                catch (Exception ex)
                {
                    Debug.LogError("SyncGripMoveGUIPlugin Error while toggling visibility: " + ex);
                }
            }
        }
    }

    [HarmonyPatch]
    public class OvrCameraShowUIPatch
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

                        // 获取 GripMovePlugin 的实例
                        var gripMovePluginType = gripMoveAssembly.GetType("CM3D2.GripMovePlugin.Plugin.GripMovePlugin");
                        var pluginInstances = UnityEngine.Object.FindObjectsOfType(gripMovePluginType);
                        var gripMovePluginInstance = pluginInstances.FirstOrDefault(); // 获取第一个实例

                        if (gripMovePluginInstance != null)
                        {
                            // 获取 menuTool 字段
                            var menuToolField = gripMovePluginType.GetField("menuTool",
                                BindingFlags.Instance | BindingFlags.NonPublic);
                            var menuToolInstance = menuToolField?.GetValue(gripMovePluginInstance);

                            if (menuToolInstance != null)
                            {
                                // 通过反射调用 IsDirectModeActive 方法
                                var isDirectModeActiveMethod =
                                    menuToolInstance.GetType().GetMethod("IsDirectModeActive");

                                if (isDirectModeActiveMethod != null)
                                {
                                    bool isDirectModeActive =
                                        (bool)isDirectModeActiveMethod.Invoke(menuToolInstance, null);

                                    // 只有在 f_bShow 为 true 且处于 DIRECT 模式时，才设置 IsVisble 为 true
                                    if (f_bShow && isDirectModeActive)
                                    {
                                        isVisbleProperty?.SetValue(
                                            obj: null,
                                            value: true,
                                            invokeAttr: BindingFlags.Static | BindingFlags.SetProperty |
                                                        BindingFlags.NonPublic |
                                                        BindingFlags.Public,
                                            binder: null,
                                            index: null,
                                            culture: CultureInfo.InvariantCulture
                                        );
                                    }
                                    else
                                    {
                                        isVisbleProperty?.SetValue(
                                            obj: null,
                                            value: false,
                                            invokeAttr: BindingFlags.Static | BindingFlags.SetProperty |
                                                        BindingFlags.NonPublic |
                                                        BindingFlags.Public,
                                            binder: null,
                                            index: null,
                                            culture: CultureInfo.InvariantCulture
                                        );
                                    }
                                }
                                else
                                {
                                    Debug.LogError("SyncGripMoveGUIPlugin Error: IsDirectModeActive method not found.");
                                }
                            }
                            else
                            {
                                Debug.LogError("SyncGripMoveGUIPlugin Error: menuTool instance not found.");
                            }
                        }
                        else
                        {
                            Debug.LogError("SyncGripMoveGUIPlugin Error: GripMovePlugin instance not found.");
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