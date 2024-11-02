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
    [BepInPlugin("COM3D2.SyncGripMoveGUI.Plugin", "COM3D2.SyncGripMoveGUI.Plugin", "1.0.8.0")]
    public class SyncGripMoveGUI : BaseUnityPlugin
    {
        // private ConfigEntry<KeyboardShortcut> toggleVisibilityShortcut;
        // private bool isOldGUIVisible = false;
        public static bool isOfficalTableVisible = false;

        // 缓存反射获取的类型和属性，避免重复获取
        private static Assembly gripMoveAssembly;
        private static Type gripMovePluginType;
        private static Type menuToolType;
        private static Type imguiQuadType;
        private static PropertyInfo isVisbleProperty;
        private static MethodInfo isDirectModeActiveMethod;
        private static FieldInfo menuToolField;
        private static object cachedGripMovePluginInstance = null;
        private static object cachedMenuToolInstance = null;


        void Start()
        {
            if (!Environment.CommandLine.ToLower().Contains("vr"))
            {
                Debug.Log("Not in VR mode. Shutting down COM3D2.SyncGripMoveGUI.Plugin.");
                return;
            }

            // // 初始化可配置按键，默认为 "G + I"
            // toggleVisibilityShortcut = Config.Bind("Hotkeys", "Toggle GripMove IMGUI Visibility",
            //     new KeyboardShortcut(KeyCode.I, KeyCode.G),
            //     "The key or key combination to toggle GripMove IMGUI visibility.");

            var harmony = new Harmony("COM3D2.SyncGripMoveGUI.Plugin");
            harmony.PatchAll();

            CacheReflectionData();
        }

        // void Update()
        // {
        //     if (toggleVisibilityShortcut.Value.IsDown())
        //     {
        //         ToggleVisibility();
        //     }
        // }

        // 手动切换可见性的方法
        // private void ToggleVisibility()
        // {
        //     isOldGUIVisible = !isOldGUIVisible;
        //     ChangeOldGUIVisible();
        // }

        // 用于缓存反射获取的类型和属性，避免重复获取
        private static void CacheReflectionData()
        {
            try
            {
                // 获取所有已加载的程序集
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                // 找到 GripMovePlugin 所在的程序集
                gripMoveAssembly =
                    assemblies.FirstOrDefault(a => a.GetType("CM3D2.GripMovePlugin.Plugin.GripMovePlugin") != null);

                if (gripMoveAssembly != null)
                {
                    // 获取 GripMovePlugin 类型
                    gripMovePluginType = gripMoveAssembly.GetType("CM3D2.GripMovePlugin.Plugin.GripMovePlugin");
                    // 获取 MenuTool 类型
                    menuToolType = gripMoveAssembly.GetType("CM3D2.GripMovePlugin.Plugin.MenuTool");
                    // 获取 IMGUIQuad 类型
                    imguiQuadType = gripMoveAssembly.GetType("CM3D2.GripMovePlugin.Plugin.IMGUIQuad");

                    if (gripMovePluginType != null)
                    {
                        // 获取 menuTool 字段
                        menuToolField = gripMovePluginType.GetField("menuTool",
                            BindingFlags.Instance | BindingFlags.NonPublic);

                        if (menuToolType != null)
                        {
                            // 获取 IsDirectModeActive 方法
                            isDirectModeActiveMethod = menuToolType.GetMethod("IsDirectModeActive",
                                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        }
                        else
                        {
                            Debug.LogError("SyncGripMoveGUIPlugin Error: MenuTool type not found.");
                        }
                    }
                    else
                    {
                        Debug.LogError("SyncGripMoveGUIPlugin Error: GripMovePlugin type not found.");
                    }

                    if (imguiQuadType != null)
                    {
                        // 获取 IsVisble 属性
                        isVisbleProperty = imguiQuadType.GetProperty("IsVisble",
                            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    }
                    else
                    {
                        Debug.LogError("SyncGripMoveGUIPlugin Error: IMGUIQuad type not found.");
                    }


                    // 获取 GripMovePlugin 实例
                    cachedGripMovePluginInstance =
                        UnityEngine.Object.FindObjectsOfType(gripMovePluginType).FirstOrDefault();

                    if (cachedGripMovePluginInstance != null)
                    {
                        // 获取 menuTool 的实例
                        cachedMenuToolInstance = menuToolField?.GetValue(cachedGripMovePluginInstance);
                    }
                    else
                    {
                        Debug.LogError("SyncGripMoveGUIPlugin Error: GripMovePlugin Instance not found.");
                    }
                }
                else
                {
                    Debug.LogError("SyncGripMoveGUIPlugin Error: GripMovePlugin assembly not found.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("SyncGripMoveGUIPlugin Error while caching reflection data: " + ex);
            }
        }

        // 更改 OldGUI 可见性
        public static void ChangeOldGUIVisible()
        {
            try
            {
                // 检查是否已经缓存了必要的反射数据
                if (gripMoveAssembly == null || gripMovePluginType == null || menuToolType == null ||
                    imguiQuadType == null || isVisbleProperty == null || isDirectModeActiveMethod == null ||
                    menuToolField == null || cachedGripMovePluginInstance == null || cachedMenuToolInstance == null)
                {
                    CacheReflectionData();
                    // 如果缓存失败，退出
                    if (gripMoveAssembly == null || gripMovePluginType == null || menuToolType == null ||
                        imguiQuadType == null || isVisbleProperty == null || isDirectModeActiveMethod == null ||
                        menuToolField == null || cachedGripMovePluginInstance == null || cachedMenuToolInstance == null)
                    {
                        Debug.LogError("SyncGripMoveGUIPlugin Error: Failed to cache reflection data.");
                        return;
                    }
                }

                // 获取当前的 isOfficalTableVisible 值
                bool isOfficalTableVisible = SyncGripMoveGUI.isOfficalTableVisible;
                bool isDirectModeActive = false;

                // 通过反射调用 IsDirectModeActive 方法，获取当前的 isDirectModeActive 值
                isDirectModeActive = (bool)isDirectModeActiveMethod.Invoke(cachedMenuToolInstance, null);

                if (isVisbleProperty != null)
                {
                    // 设置 IsVisble 属性的值( OldGUI 是否显示)
                    isVisbleProperty?.SetValue(
                        obj: null,
                        value: isOfficalTableVisible && isDirectModeActive,
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
            catch (Exception ex)
            {
                Debug.LogError("SyncGripMoveGUIPlugin Error: " + ex);
            }
        }
    }

    // 对 OvrCamera 的 ShowUI(bool f_bShow) 方法进行补丁
    [HarmonyPatch]
    public class OvrCameraShowUIPatch
    {
        // 指定要补丁的方法
        static MethodBase TargetMethod()
        {
            var ovrCameraType = typeof(OvrCamera);
            return ovrCameraType.GetMethod("ShowUI",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        // 在 ShowUI 方法执行后调用
        public static void Postfix(bool f_bShow)
        {
            SyncGripMoveGUI.isOfficalTableVisible = f_bShow;
            SyncGripMoveGUI.ChangeOldGUIVisible();
        }
    }

    // 对 MenuTool 的 OnDirectModeEnabledChanged(bool newMode) 方法进行补丁
    [HarmonyPatch]
    public class MenuTool_OnDirectModeEnabledChanged_Patch
    {
        // 指定要补丁的方法
        static MethodBase TargetMethod()
        {
            // 获取所有已加载的程序集
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            // 找到 GripMovePlugin 所在的程序集
            var gripMoveAssembly =
                assemblies.FirstOrDefault(a => a.GetType("CM3D2.GripMovePlugin.Plugin.MenuTool") != null);

            if (gripMoveAssembly != null)
            {
                // 获取 MenuTool 类型
                var menuToolType = gripMoveAssembly.GetType("CM3D2.GripMovePlugin.Plugin.MenuTool");
                if (menuToolType != null)
                {
                    // 返回 OnDirectModeEnabledChanged 方法
                    return menuToolType.GetMethod("OnDirectModeEnabledChanged",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                }
            }

            return null;
        }

        // 在 OnDirectModeEnabledChanged 方法执行后调用
        public static void Postfix(object __instance, bool newMode)
        {
            SyncGripMoveGUI.ChangeOldGUIVisible();
        }
    }
}