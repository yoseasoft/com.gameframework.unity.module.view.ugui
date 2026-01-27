/// <summary>
/// Game Framework
/// 
/// 创建者：Hurley
/// 创建时间：2025-11-16
/// 功能描述：
/// </summary>

using System;
using GameEngine;

namespace GameFramework.View.Ugui
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

        public Form CreateForm(Type viewType)
        {
            return new UnityForm(viewType);
        }
    }
}
