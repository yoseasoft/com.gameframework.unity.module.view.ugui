/// <summary>
/// Game Framework
/// 
/// 创建者：Hurley
/// 创建时间：2025-11-16
/// 功能描述：
/// </summary>

using Cysharp.Threading.Tasks;
using GameEngine;

using SystemType = System.Type;
using UnityGameObject = UnityEngine.GameObject;
using UnityTransform = UnityEngine.Transform;
using UnityRenderMode = UnityEngine.RenderMode;
using UnityCanvas = UnityEngine.Canvas;
using UnityCanvasScaler = UnityEngine.UI.CanvasScaler;
using UnityGraphicRaycaster = UnityEngine.UI.GraphicRaycaster;
using UnityEventSystem = UnityEngine.EventSystems.EventSystem;
using UnityStandaloneInputModule = UnityEngine.EventSystems.StandaloneInputModule;

namespace Game.Module.View.Ugui
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

        static UnityGameObject _dynamicCanvasObject;
        static UnityGameObject _dynamicEventSystemObject;

        static UnityTransform _dynamicCanvasTransform;
        static UnityTransform _dynamicEventSystemTransform;

        // static UnityCanvas _dynamicCanvas;
        // static UnityEventSystem _dynamicEventSystem;

        internal static string UnityGuiResourcePath
        {
            get
            {
                if (null == _unityGuiResourcePath)
                {
                    _unityGuiResourcePath = NovaEngine.Environment.GetSystemPath("UGUI_PATH");
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
            UnityGameObject targetGameObject = UnityGameObject.Find("DynamicCanvas");
            Debugger.Assert(null == targetGameObject, "The dynamic canvas object must be null.");

            targetGameObject = new UnityGameObject("DynamicCanvas");
            UnityGameObject.DontDestroyOnLoad(targetGameObject);
            UnityCanvas canvas = targetGameObject.AddComponent<UnityCanvas>();
            canvas.renderMode = UnityRenderMode.ScreenSpaceOverlay;
            UnityCanvasScaler canvasScaler = targetGameObject.AddComponent<UnityCanvasScaler>();
            canvasScaler.uiScaleMode = UnityCanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution.Set(1920, 1080);
            targetGameObject.AddComponent<UnityGraphicRaycaster>();

            _dynamicCanvasObject = targetGameObject;
            _dynamicCanvasTransform = targetGameObject.transform;

            targetGameObject = UnityGameObject.Find("DynamicEventSystem");
            Debugger.Assert(null == targetGameObject, "The dynamic event system object must be null.");

            targetGameObject = new UnityGameObject("DynamicEventSystem");
            UnityGameObject.DontDestroyOnLoad(targetGameObject);
            UnityEventSystem eventSystem = targetGameObject.AddComponent<UnityEventSystem>();
            UnityStandaloneInputModule standaloneInputModule = targetGameObject.AddComponent<UnityStandaloneInputModule>();

            _dynamicEventSystemObject = targetGameObject;
            _dynamicEventSystemTransform = targetGameObject.transform;
        }

        /// <summary>
        /// 清理UI配置信息
        /// </summary>
        private static void CleanupGuiConfig()
        {
            Debugger.Assert(_dynamicCanvasObject, "The dynamic canvas object must be non-null.");
            UnityGameObject.Destroy(_dynamicCanvasObject);
            _dynamicCanvasObject = null;
            _dynamicCanvasTransform = null;

            Debugger.Assert(_dynamicEventSystemObject, "The dynamic event system object must be non-null.");
            UnityGameObject.Destroy(_dynamicEventSystemObject);
            _dynamicEventSystemObject = null;
            _dynamicEventSystemTransform = null;
        }

        /// <summary>
        /// 窗口加载回调函数
        /// </summary>
        /// <param name="viewType">视图类型</param>
        internal static async UniTask<UnityGameObject> OnWindowLoaded(SystemType viewType)
        {
            string url = $"{UnityGuiResourcePath}{viewType.Name}/Main.prefab";

            UnityGameObject panelAssetObject = await ResourceHandler.Instance.LoadAssetAsync<UnityGameObject>(url);
            UnityGameObject panelInstantiateObject = UnityGameObject.Instantiate(panelAssetObject, _dynamicCanvasTransform);
            ResourceHandler.Instance.UnloadAsset(panelAssetObject);

            if (null == panelInstantiateObject)
            {
                Debugger.Warn("加载指定资源路径‘{%s}’下的视图类型‘{%t}’的窗口表单对象实例失败，请检查窗口资源是否存在！", url, viewType);
                return null;
            }

            //UnityGameObject.DontDestroyOnLoad(panelInstantiateObject);
            //panelInstantiateObject.transform.parent = _dynamicCanvasTransform;

            return panelInstantiateObject;
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
