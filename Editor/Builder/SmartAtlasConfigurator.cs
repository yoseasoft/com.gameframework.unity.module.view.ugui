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

namespace GameFramework.Editor.View.Ugui
{
    internal enum AtlasType
    {
        UI_Transparent,     // UI透明元素
        UI_Opaque,          // UI不透明元素
        Game_Sprite,        // 游戏精灵
        Background,         // 背景图
        Font,               // 字体图集
        Effect,            // 特效纹理
        Lightmap,          // 光照贴图
        NormalMap          // 法线贴图
    }

    internal enum QualityLevel
    {
        Low,        // 移动端低配
        Medium,     // 移动端标配
        High,       // 桌面端标配
        Ultra       // 桌面端高配
    }

    internal enum CompressionQuality
    {
        Fast = 0,   // 快速压缩
        Normal = 50, // 标准压缩
        Best = 100  // 最佳质量
    }

    /// <summary>
    /// 根据图集类型自动选择格式
    /// </summary>
    internal static class SmartAtlasConfigurator
    {
        public static TextureImporterFormat GetRecommendedFormat(AtlasType type, BuildTarget platform)
        {
            switch (platform)
            {
                case BuildTarget.iOS:
                    return GetiOSFormat(type);

                case BuildTarget.Android:
                    return GetAndroidFormat(type);

                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                case BuildTarget.StandaloneOSX:
                case BuildTarget.StandaloneLinux64:
                    return GetDesktopFormat(type);

                case BuildTarget.WebGL:
                    return GetWebGLFormat(type);

                case BuildTarget.PS4:
                case BuildTarget.PS5:
                    return GetConsoleFormat(type, platform);

                case BuildTarget.XboxOne:
                case BuildTarget.GameCoreXboxSeries:
                case BuildTarget.GameCoreXboxOne:
                    return GetXboxFormat(type, platform);

                case BuildTarget.Switch:
                    return GetSwitchFormat(type);

                default:
                    Debug.LogWarning($"平台 {platform} 未配置，使用默认RGBA32格式");
                    return TextureImporterFormat.RGBA32;
            }
        }

        /// <summary>
        /// iOS 平台格式选择
        /// </summary>
        private static TextureImporterFormat GetiOSFormat(AtlasType type)
        {
            return type switch
            {
                // 高质量UI和字体
                AtlasType.UI_Transparent => TextureImporterFormat.ASTC_4x4,
                AtlasType.Font => TextureImporterFormat.ASTC_4x4,
                AtlasType.Effect => TextureImporterFormat.ASTC_4x4,

                // 游戏精灵和普通UI
                AtlasType.Game_Sprite => TextureImporterFormat.ASTC_6x6,

                // 背景和不透明UI
                AtlasType.UI_Opaque => TextureImporterFormat.ASTC_6x6,
                AtlasType.Background => TextureImporterFormat.ASTC_8x8,
                AtlasType.Lightmap => TextureImporterFormat.ASTC_8x8,

                // 法线贴图（通常不压缩或使用专用格式）
                AtlasType.NormalMap => TextureImporterFormat.ASTC_6x6,

                _ => TextureImporterFormat.ASTC_8x8
            };
        }

        /// <summary>
        /// Android 平台格式选择
        /// </summary>
        private static TextureImporterFormat GetAndroidFormat(AtlasType type)
        {
            return type switch
            {
                // 高质量UI和字体
                AtlasType.UI_Transparent => TextureImporterFormat.ASTC_4x4,
                AtlasType.Font => TextureImporterFormat.ASTC_4x4,
                AtlasType.Effect => TextureImporterFormat.ASTC_4x4,

                // 游戏精灵和普通UI
                AtlasType.Game_Sprite => TextureImporterFormat.ASTC_6x6,

                // 背景和不透明UI
                AtlasType.UI_Opaque => TextureImporterFormat.ASTC_6x6,
                AtlasType.Background => TextureImporterFormat.ASTC_8x8,
                AtlasType.Lightmap => TextureImporterFormat.ASTC_8x8,

                // 法线贴图
                AtlasType.NormalMap => TextureImporterFormat.ASTC_6x6,

                _ => TextureImporterFormat.ASTC_8x8
            };
        }

        /// <summary>
        /// PC 桌面平台格式选择
        /// </summary>
        private static TextureImporterFormat GetDesktopFormat(AtlasType type)
        {
            return type switch
            {
                // 透明纹理使用DXT5
                AtlasType.UI_Transparent => TextureImporterFormat.DXT5,
                AtlasType.Font => TextureImporterFormat.DXT5,
                AtlasType.Effect => TextureImporterFormat.DXT5,
                AtlasType.Game_Sprite => TextureImporterFormat.DXT5,
                AtlasType.Background => TextureImporterFormat.DXT5,
                AtlasType.Lightmap => TextureImporterFormat.DXT5,

                // 不透明纹理使用DXT1（节省内存）
                AtlasType.UI_Opaque => TextureImporterFormat.DXT1,

                // 法线贴图可以使用BC5（ATI2）或BC7
                AtlasType.NormalMap => HasBC7Support() ?
                    TextureImporterFormat.BC7 : TextureImporterFormat.BC5,

                _ => TextureImporterFormat.DXT5
            };
        }

        /// <summary>
        /// WebGL 平台格式选择
        /// </summary>
        private static TextureImporterFormat GetWebGLFormat(AtlasType type)
        {
            // WebGL内存敏感，优先使用压缩格式
            bool supportsDXT = SystemInfo.SupportsTextureFormat(TextureFormat.DXT5);

            if (supportsDXT)
            {
                return type switch
                {
                    AtlasType.UI_Transparent => TextureImporterFormat.DXT5,
                    AtlasType.Font => TextureImporterFormat.DXT5,
                    AtlasType.Effect => TextureImporterFormat.DXT5,
                    AtlasType.Game_Sprite => TextureImporterFormat.DXT5,
                    AtlasType.Background => TextureImporterFormat.DXT5,
                    AtlasType.UI_Opaque => TextureImporterFormat.DXT1,
                    AtlasType.NormalMap => TextureImporterFormat.DXT5,
                    _ => TextureImporterFormat.DXT5
                };
            }
            else
            {
                // 不支持DXT时使用无压缩格式
                return type switch
                {
                    // WebGL内存紧张，对不重要纹理使用16位
                    AtlasType.Background => TextureImporterFormat.RGBA16,
                    AtlasType.UI_Opaque => TextureImporterFormat.RGB24,
                    _ => TextureImporterFormat.RGBA32
                };
            }
        }

        /// <summary>
        /// 主机平台格式选择
        /// </summary>
        private static TextureImporterFormat GetConsoleFormat(AtlasType type, BuildTarget platform)
        {
            if (platform == BuildTarget.PS4 || platform == BuildTarget.PS5)
            {
                return type switch
                {
                    // PlayStation 使用 BC3/DXT5
                    AtlasType.UI_Transparent => TextureImporterFormat.DXT5,
                    AtlasType.Font => TextureImporterFormat.DXT5,
                    AtlasType.Game_Sprite => TextureImporterFormat.DXT5,
                    AtlasType.UI_Opaque => TextureImporterFormat.DXT1,
                    AtlasType.NormalMap => TextureImporterFormat.BC5,
                    _ => TextureImporterFormat.DXT5
                };
            }

            return TextureImporterFormat.DXT5;
        }

        /// <summary>
        /// Xbox 平台格式选择
        /// </summary>
        private static TextureImporterFormat GetXboxFormat(AtlasType type, BuildTarget platform)
        {
            return type switch
            {
                // Xbox 也使用 DXT 系列
                AtlasType.UI_Transparent => TextureImporterFormat.DXT5,
                AtlasType.Font => TextureImporterFormat.DXT5,
                AtlasType.Game_Sprite => TextureImporterFormat.DXT5,
                AtlasType.UI_Opaque => TextureImporterFormat.DXT1,
                AtlasType.NormalMap => TextureImporterFormat.BC5,
                _ => TextureImporterFormat.DXT5
            };
        }

        /// <summary>
        /// Switch 平台格式选择
        /// </summary>
        private static TextureImporterFormat GetSwitchFormat(AtlasType type)
        {
            return type switch
            {
                // Switch 支持 ASTC
                AtlasType.UI_Transparent => TextureImporterFormat.ASTC_4x4,
                AtlasType.Font => TextureImporterFormat.ASTC_4x4,
                AtlasType.Game_Sprite => TextureImporterFormat.ASTC_6x6,
                AtlasType.UI_Opaque => TextureImporterFormat.ASTC_6x6,
                AtlasType.Background => TextureImporterFormat.ASTC_8x8,
                AtlasType.NormalMap => TextureImporterFormat.ASTC_6x6,
                _ => TextureImporterFormat.ASTC_8x8
            };
        }

        /// <summary>
        /// 检查是否支持BC7格式
        /// </summary>
        private static bool HasBC7Support()
        {
            // 检查显卡是否支持BC7
            return SystemInfo.SupportsTextureFormat(TextureFormat.BC7);
        }

        /// <summary>
        /// 获取平台设置，包含质量等级
        /// </summary>
        public static TextureImporterPlatformSettings GetPlatformSettings(
            BuildTarget platform,
            AtlasType type,
            QualityLevel quality = QualityLevel.Medium)
        {
            var settings = new TextureImporterPlatformSettings
            {
                name = GetPlatformName(platform),
                overridden = true,
                format = GetRecommendedFormat(type, platform),
                maxTextureSize = GetMaxTextureSize(type, quality),
                compressionQuality = (int)GetCompressionQuality(quality)
            };

            return settings;
        }

        /// <summary>
        /// 获取平台名称字符串
        /// </summary>
        private static string GetPlatformName(BuildTarget platform)
        {
            return platform switch
            {
                BuildTarget.iOS => "iPhone",
                BuildTarget.Android => "Android",
                BuildTarget.StandaloneWindows or BuildTarget.StandaloneWindows64 or
                BuildTarget.StandaloneOSX or BuildTarget.StandaloneLinux64 => "Standalone",
                BuildTarget.WebGL => "WebGL",
                BuildTarget.PS4 => "PS4",
                BuildTarget.PS5 => "PS5",
                BuildTarget.XboxOne => "XboxOne",
                BuildTarget.GameCoreXboxSeries => "GameCoreXboxSeries",
                BuildTarget.Switch => "Switch",
                _ => "Default"
            };
        }

        /// <summary>
        /// 获取最大纹理尺寸
        /// </summary>
        private static int GetMaxTextureSize(AtlasType type, QualityLevel quality)
        {
            int baseSize = type switch
            {
                AtlasType.UI_Transparent => 1024,
                AtlasType.Font => 512,
                AtlasType.Game_Sprite => 1024,
                AtlasType.Background => 2048,
                AtlasType.Lightmap => 2048,
                AtlasType.Effect => 512,
                _ => 1024
            };

            return quality switch
            {
                QualityLevel.Low => baseSize / 2,
                QualityLevel.Medium => baseSize,
                QualityLevel.High => baseSize * 2,
                QualityLevel.Ultra => baseSize * 4,
                _ => baseSize
            };
        }

        /// <summary>
        /// 获取压缩质量
        /// </summary>
        private static CompressionQuality GetCompressionQuality(QualityLevel quality)
        {
            return quality switch
            {
                QualityLevel.Low => CompressionQuality.Fast,
                QualityLevel.Medium => CompressionQuality.Normal,
                QualityLevel.High => CompressionQuality.Best,
                QualityLevel.Ultra => CompressionQuality.Best,
                _ => CompressionQuality.Normal
            };
        }

        /// <summary>
        /// 批量配置图集
        /// </summary>
        public static void ConfigureAtlasesInFolder(string folderPath, BuildTarget targetPlatform)
        {
            string[] atlasPaths = AssetDatabase.FindAssets("t:SpriteAtlas", new[] { folderPath });

            foreach (string guid in atlasPaths)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                SpriteAtlasImporter importer = AssetImporter.GetAtPath(path) as SpriteAtlasImporter;

                if (importer != null)
                {
                    // 自动检测图集类型
                    AtlasType type = DetectAtlasType(path);

                    // 获取平台设置
                    var settings = GetPlatformSettings(targetPlatform, type, QualityLevel.High);

                    // 应用设置
                    importer.SetPlatformSettings(settings);

                    Debug.Log($"已配置图集: {System.IO.Path.GetFileName(path)} -> {settings.format}");
                }
            }

            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// 根据路径自动检测图集类型
        /// </summary>
        private static AtlasType DetectAtlasType(string path)
        {
            string lowerPath = path.ToLower();

            if (lowerPath.Contains("ui") || lowerPath.Contains("interface"))
            {
                if (lowerPath.Contains("font") || lowerPath.Contains("text"))
                    return AtlasType.Font;
                if (lowerPath.Contains("icon") || lowerPath.Contains("button"))
                    return AtlasType.UI_Transparent;
                return AtlasType.UI_Opaque;
            }

            if (lowerPath.Contains("character") || lowerPath.Contains("sprite"))
                return AtlasType.Game_Sprite;

            if (lowerPath.Contains("background") || lowerPath.Contains("bg"))
                return AtlasType.Background;

            if (lowerPath.Contains("effect") || lowerPath.Contains("fx"))
                return AtlasType.Effect;

            if (lowerPath.Contains("normal") || lowerPath.Contains("bump"))
                return AtlasType.NormalMap;

            if (lowerPath.Contains("lightmap"))
                return AtlasType.Lightmap;

            return AtlasType.Game_Sprite;
        }
    }
}
