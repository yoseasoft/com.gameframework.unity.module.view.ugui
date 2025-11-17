/// <summary>
/// Game Framework
/// 
/// 创建者：Hurley
/// 创建时间：2025-11-16
/// 功能描述：
/// </summary>

namespace Game.Module.View.Ugui
{
    /// <summary>
    /// 程序集的管理模块对象类
    /// </summary>
    public static class GameModule
    {
        /// <summary>
        /// 初始化回调函数
        /// </summary>
        public static void OnInitialize()
        {
            GameEngine.GuiHandler.Instance.RegisterFormManager<UnityFormManager>();
        }

        /// <summary>
        /// 清理回调函数
        /// </summary>
        public static void OnCleanup()
        {
            GameEngine.GuiHandler.Instance.UnregisterCurrentFormManager();
        }
    }
}
