/// <summary>
/// Game Framework
/// 
/// 创建者：Hurley
/// 创建时间：2025-11-16
/// 功能描述：
/// </summary>

using SystemType = System.Type;

using UnityGameObject = UnityEngine.GameObject;
using UnityTransform = UnityEngine.Transform;
using UniTask = Cysharp.Threading.Tasks.UniTask;

using GameEngine;

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
            UnityGameObject panel = await UnityFormHelper.OnWindowLoaded(_viewType);

            if (null != panel)
            {
                _isLoaded = true;

                _gameObject = panel;
                _gameTransform = panel.transform;

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
            UnityGameObject.Destroy(_gameObject);
            _gameObject = null;
            _gameTransform = null;

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
