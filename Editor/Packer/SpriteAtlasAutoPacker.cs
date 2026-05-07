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

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace GameFramework.Editor.View.Ugui
{
    /// <summary>
    /// 图集自动打包后处理器
    /// </summary>
    internal class SpriteAtlasAutoPacker : AssetPostprocessor
    {
        /// <summary>
        /// 配置设置
        /// </summary>
        [Serializable]
        public class PackingSettings : ScriptableObject
        {
            [Header("目录配置")]
            [Tooltip("源Sprite根目录，会监控此目录下的所有子目录")]
            public string sourceRoot = NovaFramework.PresetConfiguration.DefaultRawResourceRelativePath + @"/Textures/UI/";

            [Tooltip("生成的SpriteAtlas存放根目录")]
            public string targetRoot = NovaFramework.PresetConfiguration.DefaultRawResourceRelativePath + @"/SpriteAtlases/";

#if UNITY_2022_1_OR_NEWER
            [Header("图集打包设置")]
            [Tooltip("是否启用紧密打包")]
            public bool enableTightPacking = false;
#else
            [Header("图集设置")]
            [Tooltip("图集包含模式：PackedOnly(仅打包)/All(所有)")]
            public SpriteAtlasPackingSettings.PackingMode packingMode = 
                SpriteAtlasPackingSettings.PackingMode.Tight;
#endif

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

        private static PackingSettings settings = new PackingSettings();
        private static bool isProcessing = false;

        /// <summary>
        /// 导入资源时触发
        /// </summary>
        static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (isProcessing) return;

            try
            {
                isProcessing = true;

                // 检查是否有Sprite相关文件变化
                bool hasSpriteChanges = CheckForSpriteChanges(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths);
                if (!hasSpriteChanges) return;

                // 延迟一帧执行，确保所有导入完成
                EditorApplication.delayCall += () =>
                {
                    try
                    {
                        ProcessAllDirectories();
                    }
                    finally
                    {
                        isProcessing = false;
                    }
                };
            }
            catch (Exception e)
            {
                isProcessing = false;
                Debugger.Error("[SpriteAtlasAutoPacker] 处理失败: {%s}\n{%s}", e.Message, e.StackTrace);
            }
        }

        /// <summary>
        /// 检查是否有Sprite相关文件变化
        /// </summary>
        private static bool CheckForSpriteChanges(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            // 合并所有变化的资产路径
            var allPaths = new HashSet<string>(imported)
                .Union(deleted)
                .Union(moved)
                .Union(movedFrom);

            foreach (string path in allPaths)
            {
                if (IsSpriteFile(path) && IsInSourceDirectory(path))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 处理所有目录
        /// </summary>
        public static void ProcessAllDirectories()
        {
            if (!Directory.Exists(settings.sourceRoot))
            {
                Debugger.Warn("[SpriteAtlasAutoPacker] 源目录不存在: {%s}", settings.sourceRoot);
                return;
            }

            // 获取源根目录下的所有一级子目录
            string[] sourceDirectories = Directory.GetDirectories(
            settings.sourceRoot, "*", SearchOption.TopDirectoryOnly);

            int processedCount = 0;

            foreach (string sourceDir in sourceDirectories)
            {
                // 检查是否排除目录
                if (ShouldExcludeDirectory(sourceDir))
                    continue;

                if (ProcessDirectory(sourceDir))
                {
                    processedCount++;
                }
            }

            if (processedCount > 0)
            {
                AssetDatabase.Refresh();
                Debugger.Log("[SpriteAtlasAutoPacker] 已处理 {%d} 个目录的图集", processedCount);
            }
        }

        /// <summary>
        /// 处理单个目录
        /// </summary>
        private static bool ProcessDirectory(string sourceDir)
        {
            try
            {
                // 收集所有Sprite
                List<Sprite> sprites = CollectSpritesInDirectory(sourceDir);
                if (sprites.Count == 0)
                {
                    // Debugger.Log("[SpriteAtlasAutoPacker] 目录 {%s} 中没有Sprite，跳过", sourceDir);
                    return false;
                }

                // 获取目标目录
                string targetDir = GetTargetDirectory(sourceDir);
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                // 生成图集名称
                string atlasName = GetAtlasName(sourceDir);
                string atlasPath = Path.Combine(targetDir, atlasName + ".spriteatlas").Replace("\\", "/");

                // 创建或更新图集
                SpriteAtlas spriteAtlas = CreateOrUpdateSpriteAtlas(atlasPath, sprites);

                // 配置图集
                ConfigureSpriteAtlas(spriteAtlas);

                // 保存
                if (!File.Exists(atlasPath))
                {
                    AssetDatabase.CreateAsset(spriteAtlas, atlasPath);
                }

                EditorUtility.SetDirty(spriteAtlas);
                AssetDatabase.SaveAssetIfDirty(spriteAtlas);

                Debugger.Log("[SpriteAtlasAutoPacker] 已更新图集: {%s} ({%d}个Sprite)", atlasName, sprites.Count);
                return true;
            }
            catch (Exception e)
            {
                Debugger.Error("[SpriteAtlasAutoPacker] 处理目录 {%s} 失败: {%s}", sourceDir, e.Message);
                return false;
            }
        }

        /// <summary>
        /// 收集目录下所有Sprite
        /// </summary>
        private static List<Sprite> CollectSpritesInDirectory(string directory)
        {
            List<Sprite> sprites = new List<Sprite>();

            // 搜索选项
            SearchOption searchOption = settings.includeSubdirectories ?
            SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            // 获取所有支持的图片文件
            string[] imageFiles = GetSupportedImageFiles(directory, searchOption);

            foreach (string filePath in imageFiles)
            {
                // 检查排除关键词
                if (ShouldExcludeFile(filePath))
                    continue;

                // 加载Sprite
                Sprite sprite = LoadSpriteFromFile(filePath);
                if (sprite != null)
                {
                    sprites.Add(sprite);
                }
            }

            return sprites;
        }

        /// <summary>
        /// 获取支持的图片文件
        /// </summary>
        private static string[] GetSupportedImageFiles(string directory, SearchOption searchOption)
        {
            // 支持的图片格式
            string[] extensions = { "*.png", "*.jpg", "*.jpeg", "*.tga", "*.psd", "*.bmp", "*.tif", "*.tiff" };

            List<string> allFiles = new List<string>();

            foreach (string extension in extensions)
            {
                try
                {
                    string[] files = Directory.GetFiles(directory, extension, searchOption);
                    allFiles.AddRange(files);
                }
                catch (Exception e)
                {
                    Debugger.Warn("[SpriteAtlasAutoPacker] 搜索文件时出错: {%s}", e.Message);
                }
            }

            return allFiles.ToArray();
        }

        /// <summary>
        /// 从文件加载Sprite
        /// </summary>
        private static Sprite LoadSpriteFromFile(string filePath)
        {
            // 检查是否为Sprite类型
            TextureImporter importer = AssetImporter.GetAtPath(filePath) as TextureImporter;
            if (importer == null || importer.textureType != TextureImporterType.Sprite)
                return null;

            // 加载Sprite
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(filePath);
            if (sprite == null)
            {
                // 如果直接加载失败，尝试加载为Texture2D
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(filePath);
                if (texture != null)
                {
                    Debugger.Warn("[SpriteAtlasAutoPacker] 文件 {%s} 不是有效的Sprite，但它是Texture", filePath);
                }
            }

            return sprite;
        }

        /// <summary>
        /// 获取目标目录
        /// </summary>
        private static string GetTargetDirectory(string sourceDir)
        {
            // 从源目录提取相对路径
            string relativePath = sourceDir.Substring(settings.sourceRoot.Length);

            // 移除开头的斜杠
            if (relativePath.StartsWith("/"))
                relativePath = relativePath.Substring(1);

            // 组合目标路径
            string targetDir = Path.Combine(settings.targetRoot, relativePath);

            // 标准化路径
            return targetDir.Replace("\\", "/");
        }

        /// <summary>
        /// 获取图集名称
        /// </summary>
        private static string GetAtlasName(string sourceDir)
        {
            // 获取目录名
            string dirName = Path.GetFileName(sourceDir);
            if (string.IsNullOrEmpty(dirName))
                dirName = "Default";

            // 移除特殊字符
            dirName = new string(dirName.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());

            return $"{dirName}_Atlas";
        }

        /// <summary>
        /// 创建或更新SpriteAtlas
        /// </summary>
        private static SpriteAtlas CreateOrUpdateSpriteAtlas(string atlasPath, List<Sprite> sprites)
        {
            SpriteAtlas spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);

            if (null == spriteAtlas)
            {
                spriteAtlas = new SpriteAtlas();
            }

            // 清空现有内容
            spriteAtlas.Remove(new UnityEngine.Object[0]);

            // 添加所有Sprite
            if (sprites.Count > 0)
            {
                spriteAtlas.Add(sprites.ToArray());
            }

            return spriteAtlas;
        }

        /// <summary>
        /// 配置SpriteAtlas
        /// </summary>
        private static void ConfigureSpriteAtlas(SpriteAtlas spriteAtlas)
        {
            // 打包设置
            SpriteAtlasPackingSettings packingSettings = spriteAtlas.GetPackingSettings();
            packingSettings.enableRotation = settings.enableRotation;
#if UNITY_2022_1_OR_NEWER
            packingSettings.enableTightPacking = settings.enableTightPacking;
#else
            packingSettings.enableTightPacking = (settings.packingMode == SpriteAtlasPackingSettings.PackingMode.Tight);
#endif
            packingSettings.padding = settings.padding;
            spriteAtlas.SetPackingSettings(packingSettings);

            // 纹理设置
            SpriteAtlasTextureSettings textureSettings = spriteAtlas.GetTextureSettings();
            textureSettings.readable = false;
            textureSettings.generateMipMaps = false;
            textureSettings.sRGB = true;
            textureSettings.filterMode = FilterMode.Bilinear;
            spriteAtlas.SetTextureSettings(textureSettings);

#if UNITY_2022_1_OR_NEWER
            // 平台设置（Unity 2022 修正版）
            TextureImporterPlatformSettings platformSettings = spriteAtlas.GetPlatformSettings("DefaultTexturePlatform");
            if (null == platformSettings.name) // 判断是否存在该平台设置
            {
                platformSettings = new TextureImporterPlatformSettings();
                platformSettings.name = "DefaultTexturePlatform"; // 必须设置name
            }

            platformSettings.overridden = true; // 必须设为true，覆盖才会生效
            platformSettings.maxTextureSize = settings.maxTextureSize;
            platformSettings.format = settings.includeAlpha ?
                TextureImporterFormat.RGBA32 : TextureImporterFormat.RGB24;
            platformSettings.compressionQuality = settings.compressionQuality;
            platformSettings.textureCompression = TextureImporterCompression.Compressed; // 通常需要设置压缩模式
#else
            // 纹理设置
            SpriteAtlasTextureSettings textureSettings = spriteAtlas.GetTextureSettings();
            textureSettings.readable = false;
            textureSettings.generateMipMaps = false;
            textureSettings.sRGB = true;
            textureSettings.filterMode = FilterMode.Bilinear;
            spriteAtlas.SetTextureSettings(textureSettings);
        
            // 平台设置
            SpriteAtlasPlatformSettings platformSettings = spriteAtlas.GetPlatformSettings("DefaultTexturePlatform");
            if (platformSettings == null)
            {
                platformSettings = new SpriteAtlasPlatformSettings();
            }
        
            platformSettings.maxTextureSize = settings.maxTextureSize;
            platformSettings.format = settings.includeAlpha ? 
                SpriteAtlasTextureFormat.RGBA32 : SpriteAtlasTextureFormat.RGB24;
            platformSettings.compressionQuality = settings.compressionQuality;
#endif

            spriteAtlas.SetPlatformSettings(platformSettings);
        }

        /// <summary>
        /// 检查是否排除目录
        /// </summary>
        private static bool ShouldExcludeDirectory(string directoryPath)
        {
            string dirName = Path.GetFileName(directoryPath);

            // 检查排除列表
            foreach (string excludeDir in settings.excludeDirectories)
            {
                if (dirName.Contains(excludeDir))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 检查是否排除文件
        /// </summary>
        private static bool ShouldExcludeFile(string filePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);

            foreach (string keyword in settings.excludeKeywords)
            {
                if (fileName.Contains(keyword))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 检查是否为Sprite文件
        /// </summary>
        private static bool IsSpriteFile(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            return extension == ".png" || extension == ".jpg" || extension == ".jpeg" ||
                   extension == ".tga" || extension == ".psd" || extension == ".bmp" ||
                   extension == ".tif" || extension == ".tiff";
        }

        /// <summary>
        /// 检查是否在源目录下
        /// </summary>
        private static bool IsInSourceDirectory(string filePath)
        {
            string normalizedPath = filePath.Replace("\\", "/");
            string normalizedSourceRoot = settings.sourceRoot.Replace("\\", "/");

            return normalizedPath.StartsWith(normalizedSourceRoot);
        }
    }
}
