/// <summary>
/// Game Framework
/// 
/// 创建者：Hurley
/// 创建时间：2025-11-16
/// 功能描述：
/// </summary>

using GameEngine;

using SystemType = System.Type;
using UnityObject = UnityEngine.Object;
using UnityGameObject = UnityEngine.GameObject;
using UnityTransform = UnityEngine.Transform;

using UniTask = Cysharp.Threading.Tasks.UniTask;

namespace Game.Module.View.Ugui
{
    /// <summary>
    /// 基于UGUI封装的窗口对象类
    /// </summary>
    public sealed class UnityForm : Form
    {
        /// <summary>
        /// 视图对象的游戏节点对象实例
        /// </summary>
        private UnityGameObject _gameObject;
        /// <summary>
        /// 视图对象的游戏节点转换组件实例
        /// </summary>
        private UnityTransform _gameTransform;

        /// <summary>
        /// 资源对象
        /// </summary>
        private UnityGameObject _assetObject;

        /// <summary>
        /// 窗口根节点对象实例
        /// </summary>
        public override object Root => _gameObject;

        internal UnityForm(SystemType viewType) : base(viewType)
        {
        }

        ~UnityForm()
        { }

        /// <summary>
        /// 窗口实例的加载接口函数
        /// </summary>
        protected override sealed async UniTask Load()
        {
            UnityGameObject assetObject = await UnityFormHelper.OnWindowLoaded(_viewType);

            UnityGameObject instantiateObject = UnityObject.Instantiate(assetObject, UnityFormHelper.DynamicCanvasTransform);
            // ResourceHandler.Instance.UnloadAsset(assetObject);

            if (null == instantiateObject)
            {
                Debugger.Warn("加载指定视图类型‘{%t}’的窗口表单对象实例失败，请检查窗口资源是否存在！", _viewType);
                return;
            }

            //UnityObject.DontDestroyOnLoad(instantiateObject);
            //instantiateObject.transform.parent = UnityFormHelper.DynamicCanvasTransform;

            if (null != instantiateObject)
            {
                _isLoaded = true;

                _gameObject = instantiateObject;
                _gameTransform = instantiateObject.transform;

                _assetObject = assetObject;

                // 编辑器下显示名字
                if (NovaEngine.Environment.IsDevelopmentState())
                {
                    _gameObject.name = $"{_viewType.Name}";
                }
            }
        }

        /// <summary>
        /// 窗口实例的卸载接口函数
        /// </summary>
        protected override sealed void Unload()
        {
            UnityObject.Destroy(_gameObject);
            _gameObject = null;
            _gameTransform = null;

            ResourceHandler.Instance.UnloadAsset(_assetObject);
            _assetObject = null;

            _isLoaded = false;
        }

        /// <summary>
        /// 窗口实例的显示接口函数
        /// </summary>
        protected override sealed void Show()
        {
            //_window.ShowContentPane();
        }

        /// <summary>
        /// 窗口实例的隐藏接口函数
        /// </summary>
        protected override sealed void Hide()
        {
            Debugger.Throw<System.NotImplementedException>();
        }
    }
}
