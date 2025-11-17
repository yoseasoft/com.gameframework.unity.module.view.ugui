/// <summary>
/// Game Framework
/// 
/// 创建者：Hurley
/// 创建时间：2025-11-16
/// 功能描述：
/// </summary>

using GameEngine;

using SystemType = System.Type;

namespace Game.Module.View.Ugui
{
    public class UnityFormManager : IFormManager
    {
        public void Startup()
        {
            UnityFormHelper.Startup();
        }

        public void Shutdown()
        {
            UnityFormHelper.Shutdown();
        }

        public void Update()
        {
            UnityFormHelper.Update();
        }

        public Form CreateForm(SystemType viewType)
        {
            return new UnityForm(viewType);
        }
    }
}
