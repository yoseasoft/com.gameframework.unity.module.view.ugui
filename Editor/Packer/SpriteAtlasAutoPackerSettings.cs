/// -------------------------------------------------------------------------------
/// GameFramework Editor By UnityEngine
///
/// Copyright (C) 2026, Hurley, Independent Studio.
///
/// Permission is hereby granted, free of charge, to any person obtaining a copy
/// of this software and associated documentation files (the "Software"), to deal
/// in the Software without restriction, including without limitation the rights
/// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
/// copies of the Software, and to permit persons to whom the Software is
/// furnished to do so, subject to the following conditions:
///
/// The above copyright notice and this permission notice shall be included in
/// all copies or substantial portions of the Software.
///
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
/// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
/// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
/// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
/// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
/// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
/// THE SOFTWARE.
/// -------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.Editor.View.Ugui
{
    [CreateAssetMenu(fileName = "SpriteAtlasAutoPackerSettings", menuName = "Tools/Sprite Atlas Auto Packer/Settings")]
    internal class SpriteAtlasAutoPackerSettings : ScriptableObject
    {
        [Header("目录配置")]
        [Tooltip("源Sprite根目录，会监控此目录下的所有子目录")]
        public string sourceRoot = NovaFramework.PresetConfiguration.DefaultRawResourceRelativePath + @"/Textures/UI/";

        [Tooltip("生成的SpriteAtlas存放根目录")]
        public string targetRoot = NovaFramework.PresetConfiguration.DefaultRawResourceRelativePath + @"/SpriteAtlases/";

        [Header("图集打包设置")]
        [Tooltip("是否启用紧密打包")]
        public bool enableTightPacking = false;

        [Tooltip("是否启用旋转")]
        public bool enableRotation = false;

        [Tooltip("打包边距")]
        public int padding = 4;

        [Tooltip("图集最大尺寸")]
        public int maxTextureSize = 2048;

        [Tooltip("是否包含Alpha通道")]
        public bool includeAlpha = true;

        [Tooltip("压缩质量 (0-100)")]
        [Range(0, 100)]
        public int compressionQuality = 50;

        [Header("过滤器设置")]
        [Tooltip("是否包含子目录")]
        public bool includeSubdirectories = true;

        [Tooltip("要排除的目录名（包含即跳过）")]
        public List<string> excludeDirectories = new List<string>() { "_Ignore", "Editor" };

        [Tooltip("要排除的文件名关键词（包含即跳过）")]
        public List<string> excludeKeywords = new List<string>() { "@2x", "@3x", "_temp" };
    }
}
