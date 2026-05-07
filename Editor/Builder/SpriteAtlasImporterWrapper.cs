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
using UnityEngine;

namespace GameFramework.Editor.View.Ugui
{
    /// <summary>
    /// 精灵图集导入器包装类
    /// </summary>
    public class SpriteAtlasImporterWrapper
    {
        private AssetImporter baseImporter;
        private string assetPath;

        public SpriteAtlasImporterWrapper(string path)
        {
            this.assetPath = path;
            this.baseImporter = AssetImporter.GetAtPath(path);
        }

        public void SetPlatformSettings(TextureImporterPlatformSettings settings)
        {
            // 通过 SerializedObject 间接设置
            SerializedObject serializedImporter = new SerializedObject(baseImporter);

            SerializedProperty platformSettings = serializedImporter.FindProperty("platformSettings");
            if (platformSettings != null && platformSettings.isArray)
            {
                // 查找或创建对应平台的设置
                for (int i = 0; i < platformSettings.arraySize; i++)
                {
                    SerializedProperty setting = platformSettings.GetArrayElementAtIndex(i);
                    if (setting.FindPropertyRelative("name").stringValue == settings.name)
                    {
                        ApplyPlatformSettings(setting, settings);
                        break;
                    }
                }

                serializedImporter.ApplyModifiedProperties();
            }
        }

        private void ApplyPlatformSettings(SerializedProperty setting, TextureImporterPlatformSettings newSettings)
        {
            setting.FindPropertyRelative("overridden").boolValue = newSettings.overridden;
            setting.FindPropertyRelative("maxTextureSize").intValue = newSettings.maxTextureSize;
            setting.FindPropertyRelative("format").enumValueIndex = (int) newSettings.format;
            setting.FindPropertyRelative("compressionQuality").intValue = newSettings.compressionQuality;
        }

        public void SaveAndReimport()
        {
            baseImporter.SaveAndReimport();
        }
    }
}
