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
using System.IO;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace GameFramework.Editor.View.Ugui
{
    /// <summary>
    /// 编辑器菜单和配置面板
    /// </summary>
    internal class SpriteAtlasAutoPackerEditor : EditorWindow
    {
        private static SpriteAtlasAutoPacker.PackingSettings settings;
        private Vector2 scrollPosition;
        private bool showAdvancedSettings = false;

        [MenuItem("Tools/Sprite Atlas Auto Packer/打开配置面板")]
        public static void ShowWindow()
        {
            var window = GetWindow<SpriteAtlasAutoPackerEditor>("Sprite Atlas 自动打包");
            window.minSize = new Vector2(400, 600);
            window.Show();
        }

        [MenuItem("Tools/Sprite Atlas Auto Packer/立即处理所有目录")]
        public static void ProcessAllDirectoriesNow()
        {
            if (EditorUtility.DisplayDialog("确认",
                "这将处理所有监控目录下的Sprite，生成对应的图集。确定要继续吗？",
                "确定", "取消"))
            {
                SpriteAtlasAutoPacker.ProcessAllDirectories();
                EditorUtility.DisplayDialog("完成", "所有目录处理完成", "确定");
            }
        }

        [MenuItem("Tools/Sprite Atlas Auto Packer/清理空图集")]
        public static void CleanEmptyAtlases()
        {
            var settings = LoadSettings();
            if (!Directory.Exists(settings.targetRoot))
            {
                EditorUtility.DisplayDialog("提示", "目标目录不存在", "确定");
                return;
            }

            string[] atlasFiles = Directory.GetFiles(settings.targetRoot, "*.spriteatlas", SearchOption.AllDirectories);
            int deletedCount = 0;

            foreach (string atlasFile in atlasFiles)
            {
                SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasFile);
                if (atlas != null)
                {
                    Object[] packedSprites = atlas.GetPackables();
                    if (packedSprites == null || packedSprites.Length == 0)
                    {
                        AssetDatabase.DeleteAsset(atlasFile);
                        deletedCount++;
                    }
                }
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("完成", $"已清理 {deletedCount} 个空图集", "确定");
        }

        void OnEnable()
        {
            settings = LoadSettings();
        }

        void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Sprite Atlas 自动打包配置", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // 目录配置
            DrawDirectorySettings();

            EditorGUILayout.Space(10);

            // 打包设置
            DrawPackingSettings();

            EditorGUILayout.Space(10);

            // 高级设置
            DrawAdvancedSettings();

            EditorGUILayout.Space(20);

            // 操作按钮
            DrawActionButtons();

            EditorGUILayout.EndScrollView();
        }

        private void DrawDirectorySettings()
        {
            EditorGUILayout.LabelField("目录配置", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            // 源目录
            EditorGUILayout.BeginHorizontal();
            settings.sourceRoot = EditorGUILayout.TextField("源Sprite根目录", settings.sourceRoot);
            if (GUILayout.Button("浏览", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFolderPanel("选择源目录", Application.dataPath, "");
                if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
                {
                    settings.sourceRoot = "Assets" + path.Substring(Application.dataPath.Length);
                }
            }
            EditorGUILayout.EndHorizontal();

            if (!Directory.Exists(settings.sourceRoot))
            {
                EditorGUILayout.HelpBox("源目录不存在", MessageType.Warning);
            }

            // 目标目录
            EditorGUILayout.BeginHorizontal();
            settings.targetRoot = EditorGUILayout.TextField("图集存放目录", settings.targetRoot);
            if (GUILayout.Button("浏览", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFolderPanel("选择目标目录", Application.dataPath, "");
                if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
                {
                    settings.targetRoot = "Assets" + path.Substring(Application.dataPath.Length);
                }
            }
            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                SaveSettings();
            }
        }

        private void DrawPackingSettings()
        {
            EditorGUILayout.LabelField("打包设置", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

#if UNITY_2022_1_OR_NEWER
            settings.enableTightPacking = EditorGUILayout.Toggle("紧密打包", settings.enableTightPacking);
#else
            settings.packingMode = (SpriteAtlasPackingSettings.PackingMode)EditorGUILayout.EnumPopup(
                "打包模式", settings.packingMode);
#endif
            settings.enableRotation = EditorGUILayout.Toggle("启用旋转", settings.enableRotation);
            settings.padding = EditorGUILayout.IntSlider("打包边距", settings.padding, 2, 16);
            settings.maxTextureSize = EditorGUILayout.IntSlider("最大尺寸", settings.maxTextureSize, 256, 4096);
            settings.includeAlpha = EditorGUILayout.Toggle("包含Alpha通道", settings.includeAlpha);
            settings.compressionQuality = EditorGUILayout.IntSlider("压缩质量", settings.compressionQuality, 0, 100);

            if (EditorGUI.EndChangeCheck())
            {
                SaveSettings();
            }
        }

        private void DrawAdvancedSettings()
        {
            showAdvancedSettings = EditorGUILayout.Foldout(showAdvancedSettings, "高级设置");
            if (!showAdvancedSettings) return;

            EditorGUI.indentLevel++;

            EditorGUI.BeginChangeCheck();

            settings.includeSubdirectories = EditorGUILayout.Toggle("包含子目录", settings.includeSubdirectories);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("排除目录:");
            for (int i = 0; i < settings.excludeDirectories.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                settings.excludeDirectories[i] = EditorGUILayout.TextField(settings.excludeDirectories[i]);
                if (GUILayout.Button("-", GUILayout.Width(30)))
                {
                    settings.excludeDirectories.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+ 添加排除目录", GUILayout.Width(150)))
            {
                settings.excludeDirectories.Add("");
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("排除文件名关键词:");
            for (int i = 0; i < settings.excludeKeywords.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                settings.excludeKeywords[i] = EditorGUILayout.TextField(settings.excludeKeywords[i]);
                if (GUILayout.Button("-", GUILayout.Width(30)))
                {
                    settings.excludeKeywords.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+ 添加排除关键词", GUILayout.Width(150)))
            {
                settings.excludeKeywords.Add("");
            }

            if (EditorGUI.EndChangeCheck())
            {
                SaveSettings();
            }

            EditorGUI.indentLevel--;
        }

        private void DrawActionButtons()
        {
            GUILayout.BeginVertical("Box");

            if (GUILayout.Button("保存配置", GUILayout.Height(30)))
            {
                SaveSettings();
                EditorUtility.DisplayDialog("提示", "配置已保存", "确定");
            }

            if (GUILayout.Button("立即处理所有目录", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("确认",
                    "这将处理所有监控目录下的Sprite，生成对应的图集。确定要继续吗？",
                    "确定", "取消"))
                {
                    SpriteAtlasAutoPacker.ProcessAllDirectories();
                    EditorUtility.DisplayDialog("完成", "所有目录处理完成", "确定");
                }
            }

            if (GUILayout.Button("清理空图集", GUILayout.Height(30)))
            {
                CleanEmptyAtlases();
            }

            GUILayout.EndVertical();
        }

        private static SpriteAtlasAutoPacker.PackingSettings LoadSettings()
        {
            string settingsPath = "Assets/SpriteAtlasAutoPackerSettings.asset";

            var settingsAsset = AssetDatabase.LoadAssetAtPath<SpriteAtlasAutoPacker.PackingSettings>(settingsPath);
            if (settingsAsset != null)
            {
                return settingsAsset;
            }

            // 创建默认设置
            var defaultSettings = new SpriteAtlasAutoPacker.PackingSettings();

            // 确保目录存在
            if (!Directory.Exists(defaultSettings.targetRoot))
            {
                Directory.CreateDirectory(defaultSettings.targetRoot);
            }

            // 保存默认设置
            AssetDatabase.CreateAsset(defaultSettings, settingsPath);
            AssetDatabase.SaveAssets();

            return defaultSettings;
        }

        private void SaveSettings()
        {
            string settingsPath = "Assets/SpriteAtlasAutoPackerSettings.asset";

            if (!File.Exists(settingsPath))
            {
                AssetDatabase.CreateAsset(settings, settingsPath);
            }

            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }
    }
}
