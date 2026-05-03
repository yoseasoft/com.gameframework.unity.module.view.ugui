/// <summary>
/// Game Framework
/// 
/// 创建者：Hurley
/// 创建时间：2025-11-16
/// 功能描述：
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;

using NovaFramework.AssetLoader;

#if GAMEFRAMEWORK_UNIVERSAL_RENDER_SUPPORTER
using UnityEngine.Rendering.Universal;
#endif

using UnityObject = UnityEngine.Object;
using UnityGameObject = UnityEngine.GameObject;
using UnityTransform = UnityEngine.Transform;
using UnityCamera = UnityEngine.Camera;
using UnityCanvas = UnityEngine.Canvas;
using UnityCanvasScaler = UnityEngine.UI.CanvasScaler;
using UnityGraphicRaycaster = UnityEngine.UI.GraphicRaycaster;
using UnityEventSystem = UnityEngine.EventSystems.EventSystem;
using UnityLayerMask = UnityEngine.LayerMask;
using UnityRenderMode = UnityEngine.RenderMode;
using UnityCameraClearFlags = UnityEngine.CameraClearFlags;

namespace GameFramework.View.Ugui
{
    /// <summary>
    /// UGUI的窗口对象辅助工具类
    /// </summary>
    public static class UnityFormHelper
    {
        /// <summary>
        /// UI资源目录
        /// </summary>
        static string _unityGuiResourcePath;

        static UnityGameObject _globalCameraObject;
        static UnityGameObject _globalEventSystemObject;

        static IDictionary<string, UnityGameObject> _canvasObjects;
        static IDictionary<string, UnityTransform> _canvasTransforms;

        internal static string UnityGuiResourcePath
        {
            get
            {
                if (null == _unityGuiResourcePath)
                {
                    _unityGuiResourcePath = NovaEngine.Environment.GetSystemPath("GUI_PATH");
                    Debugger.Assert(false == string.IsNullOrEmpty(_unityGuiResourcePath), "Invalid UGui resource path.");
                }

                return _unityGuiResourcePath;
            }
        }

        /// <summary>
        /// Unity窗口表单辅助类启动接口函数
        /// </summary>
        internal static void Startup()
        {
            _canvasObjects = new Dictionary<string, UnityGameObject>();
            _canvasTransforms = new Dictionary<string, UnityTransform>();

            InitGuiConfig();
        }

        /// <summary>
        /// Unity窗口表单辅助类关闭接口函数
        /// </summary>
        internal static void Shutdown()
        {
            CleanupGuiConfig();
        }

        /// <summary>
        /// Unity窗口表单辅助类刷新接口函数
        /// </summary>
        internal static void Update()
        {
        }

        /// <summary>
        /// 初始化UI配置信息
        /// </summary>
        private static void InitGuiConfig()
        {
            UnityGameObject cameraGameObject = UnityGameObject.Find("GlobalCamera");
            Debugger.IsNull(cameraGameObject);

            cameraGameObject = new UnityGameObject("GlobalCamera");
            UnityObject.DontDestroyOnLoad(cameraGameObject);
            UnityCamera camera = cameraGameObject.AddComponent<UnityCamera>();
#if GAMEFRAMEWORK_UNIVERSAL_RENDER_SUPPORTER
            UniversalAdditionalCameraData cameraData = camera.GetUniversalAdditionalCameraData();
            cameraData.renderType = CameraRenderType.Base;
            cameraData.renderShadows = false;
            // 禁用所有后处理
            cameraData.renderPostProcessing = false;
            cameraData.stopNaN = false;
            cameraData.dithering = false;
            cameraData.antialiasing = AntialiasingMode.None;
            // 清除 Volume 影响
            cameraData.volumeLayerMask = 0;
#endif

            camera.orthographic = true;
            camera.cullingMask = UnityLayerMask.GetMask("UI");
            camera.clearFlags = UnityCameraClearFlags.Depth;
            camera.backgroundColor = UnityEngine.Color.clear;
            camera.orthographicSize = 5f;
            camera.nearClipPlane = 0.01f;
            camera.farClipPlane = 100f;
            camera.depth = 0;  // Overlay相机深度不重要

            _globalCameraObject = cameraGameObject;

            UnityGameObject eventSystemGameObject = UnityGameObject.Find("GlobalEventSystem");
            Debugger.IsNull(eventSystemGameObject);

            eventSystemGameObject = new UnityGameObject("GlobalEventSystem");
            UnityObject.DontDestroyOnLoad(eventSystemGameObject);
            UnityEventSystem eventSystem = eventSystemGameObject.AddComponent<UnityEventSystem>();
#if ENABLE_INPUT_SYSTEM
            eventSystemGameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#elif ENABLE_LEGACY_INPUT_MANAGER
            eventSystemGameObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
#endif

            _globalEventSystemObject = eventSystemGameObject;

            CreateDefaultCanvasObjects();
        }

        /// <summary>
        /// 清理UI配置信息
        /// </summary>
        private static void CleanupGuiConfig()
        {
            DestroyAllCanvasObjects();

            _canvasObjects = null;
            _canvasTransforms = null;

            Debugger.Assert(_globalEventSystemObject, "The global event system object must be non-null.");
            UnityObject.Destroy(_globalEventSystemObject);
            _globalEventSystemObject = null;

            Debugger.Assert(_globalCameraObject, "The global camera object must be non-null.");
            UnityObject.Destroy(_globalCameraObject);
            _globalCameraObject = null;
        }

        internal static void AddGroup(string groupName, int level)
        {
            UnityGameObject targetGameObject = UnityGameObject.Find(groupName);
            Debugger.IsNull(targetGameObject);

            targetGameObject = new UnityGameObject(groupName);
            UnityObject.DontDestroyOnLoad(targetGameObject);
            targetGameObject.layer = UnityLayerMask.NameToLayer("UI");

            UnityCanvas canvas = targetGameObject.AddComponent<UnityCanvas>();
            canvas.renderMode = UnityRenderMode.ScreenSpaceCamera;
            canvas.worldCamera = _globalCameraObject.GetComponent<UnityCamera>();
            canvas.planeDistance = 100;
            canvas.sortingOrder = level * 5;
            canvas.sortingLayerName = "Default";

            UnityCanvasScaler canvasScaler = targetGameObject.AddComponent<UnityCanvasScaler>();
            canvasScaler.uiScaleMode = UnityCanvasScaler.ScaleMode.ScaleWithScreenSize;
            // canvasScaler.referenceResolution.Set(NovaEngine.Environment.designResolutionWidth, NovaEngine.Environment.designResolutionHeight);
            canvasScaler.referenceResolution = new UnityEngine.Vector2(NovaEngine.Environment.DesignResolutionWidth, NovaEngine.Environment.DesignResolutionHeight);
            canvasScaler.matchWidthOrHeight = 0.5f;
            targetGameObject.AddComponent<UnityGraphicRaycaster>();

            _canvasObjects.Add(groupName, targetGameObject);
            _canvasTransforms.Add(groupName, targetGameObject.transform);
        }

        internal static void RemoveGroup(string groupName)
        {
            if (_canvasObjects.TryGetValue(groupName, out UnityGameObject targetGameObject))
            {
                UnityObject.Destroy(targetGameObject);
                _canvasObjects.Remove(groupName);
            }

            if (_canvasTransforms.TryGetValue(groupName, out UnityTransform targetTransform))
            {
                _canvasTransforms.Remove(groupName);
            }
        }

        public static void UpdateViewCamera(UnityCamera mainCamera)
        {
#if GAMEFRAMEWORK_UNIVERSAL_RENDER_SUPPORTER
            UnityCamera camera = _globalCameraObject.GetComponent<UnityCamera>();
            UniversalAdditionalCameraData cameraData = camera.GetUniversalAdditionalCameraData();

            if (null == mainCamera)
            {
                cameraData.renderType = CameraRenderType.Base;
            }
            else
            {
                cameraData.renderType = CameraRenderType.Overlay;

                UniversalAdditionalCameraData mainCameraData = mainCamera.GetUniversalAdditionalCameraData();
                mainCameraData.cameraStack.Add(camera);
            }
#endif
        }

        /// <summary>
        /// 创建默认的Canvas对象实例
        /// </summary>
        private static void CreateDefaultCanvasObjects()
        {
            IReadOnlyList<string> groupNames = GameEngine.GuiHandler.Instance.GetAllSortingViewGroupNames();
            for (int n = 0; n < groupNames.Count; ++n)
            {
                string groupName = groupNames[n];
                int level = GameEngine.GuiHandler.Instance.GetViewGroupLevelByName(groupName);

                AddGroup(groupName, level);
            }
        }

        /// <summary>
        /// 销毁当前所有的Canvas对象实例
        /// </summary>
        private static void DestroyAllCanvasObjects()
        {
            foreach (KeyValuePair<string, UnityGameObject> kvp in _canvasObjects)
            {
                UnityObject.Destroy(kvp.Value);
            }

            _canvasObjects.Clear();
            _canvasTransforms.Clear();
        }

        /// <summary>
        /// 通过指定的分组名称获取对应的Canvas组件实例
        /// </summary>
        /// <param name="groupName">分组名称</param>
        /// <returns>返回指定名称的Canvas组件实例</returns>
        internal static UnityTransform GetGameCanvasTransformByGroupName(string groupName)
        {
            if (_canvasTransforms.TryGetValue(groupName, out UnityTransform canvasTransform))
                return canvasTransform;

            return null;
        }

        /// <summary>
        /// 窗口加载回调函数
        /// </summary>
        /// <param name="viewType">视图类型</param>
        internal static IAssetHandler OnWindowLoaded(Type viewType)
        {
            string url = $"{UnityGuiResourcePath}/{viewType.Name}/Main.prefab";

            return GameEngine.ResourceHandler.Instance.LoadAssetAsync<UnityGameObject>(url);
        }

        /// <summary>
        /// 窗口卸载回调函数
        /// </summary>
        /// <param name="form">窗口实例</param>
        internal static void OnWindowUnloaded(UnityForm form)
        {
        }
    }
}
