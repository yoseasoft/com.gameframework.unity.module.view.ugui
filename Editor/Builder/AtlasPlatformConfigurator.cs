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
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace GameFramework.Editor.View.Ugui
{
    /// <summary>
    /// 配置图集平台格式
    /// </summary>
    public static class AtlasPlatformConfigurator
    {
        [MenuItem("Tools/Sprite Atlas Auto Packer/配置图集平台格式")]
        public static void ConfigureAtlasPlatforms()
        {
            // 1. 获取选中的 .spriteatlas 文件
            string[] atlasGuids = Selection.assetGUIDs;
            if (atlasGuids.Length == 0)
            {
                Debug.LogWarning("请在Project窗口选中一个 .spriteatlas 文件");
                return;
            }

            string path = AssetDatabase.GUIDToAssetPath(atlasGuids[0]);
            if (!path.EndsWith(".spriteatlas"))
            {
                Debug.LogError("选中的不是 SpriteAtlas 文件");
                return;
            }

            // 2. 关键步骤：通过路径获取 SpriteAtlasImporter
            SpriteAtlasImporter importer = AssetImporter.GetAtPath(path) as SpriteAtlasImporter;
            if (importer == null)
            {
                Debug.LogError("获取 SpriteAtlasImporter 失败");
                return;
            }

            // 3. 使用 importer 进行配置
            ConfigureAtlasForPlatforms(importer);

            // 4. 保存更改
            importer.SaveAndReimport();
            Debug.Log($"已更新图集设置: {path}");

            /**
            // 通过包装类
            var wrapper = new SpriteAtlasImporterWrapper(path);
            var settings = new TextureImporterPlatformSettings
            {
                name = "Android",
                overridden = true,
                maxTextureSize = 2048,
                format = TextureImporterFormat.ASTC_RGBA_8x8
            };
            wrapper.SetPlatformSettings(settings);
            wrapper.SaveAndReimport();
            */
        }

        private static void ConfigureAtlasForPlatforms(SpriteAtlasImporter importer)
        {
            // === iOS 配置 ===
            TextureImporterPlatformSettings iosSettings = importer.GetPlatformSettings("iPhone");
            iosSettings.overridden = true;
            iosSettings.format = TextureImporterFormat.ASTC_8x8; // 推荐格式
            iosSettings.maxTextureSize = 2048;
            iosSettings.compressionQuality = 50;
            importer.SetPlatformSettings(iosSettings);

            // === Android 配置 ===
            TextureImporterPlatformSettings androidSettings = importer.GetPlatformSettings("Android");
            androidSettings.overridden = true;
            androidSettings.format = TextureImporterFormat.ASTC_8x8;
            androidSettings.maxTextureSize = 2048;
            androidSettings.compressionQuality = 50;
            importer.SetPlatformSettings(androidSettings);

            // === PC 配置 ===
            TextureImporterPlatformSettings standaloneSettings = importer.GetPlatformSettings("Standalone");
            standaloneSettings.overridden = true;
            standaloneSettings.format = TextureImporterFormat.DXT5;
            standaloneSettings.maxTextureSize = 2048;
            standaloneSettings.compressionQuality = 50;
            importer.SetPlatformSettings(standaloneSettings);

            // === WebGL 配置 ===
            TextureImporterPlatformSettings webglSettings = importer.GetPlatformSettings("WebGL");
            webglSettings.overridden = true;
            webglSettings.format = TextureImporterFormat.DXT5; // 浏览器支持
            webglSettings.maxTextureSize = 1024; // WebGL内存敏感
            webglSettings.compressionQuality = 50;
            importer.SetPlatformSettings(webglSettings);

            // === 通用后备配置 ===
            TextureImporterPlatformSettings defaultSettings = importer.GetPlatformSettings("DefaultTexturePlatform");
            defaultSettings.overridden = true;
            defaultSettings.format = TextureImporterFormat.RGBA32; // 无压缩，开发用
            defaultSettings.maxTextureSize = 4096;
            defaultSettings.compressionQuality = 50;
            importer.SetPlatformSettings(defaultSettings);
        }
    }
}
