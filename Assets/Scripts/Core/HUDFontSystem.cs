using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.TextCore;
using System;
using System.IO;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace VNWGame.Game
{
    public class HUDFontData
    {
        public char word;
        public Vector3 offset;
        public Vector2 fontSize;
        public int colorType;
        public int arrayIndex;
        public int alive;
        public float fontScale;
        public long shipID;
        public static int PixelCount = 3;
    }
    public class HUDFontSystem : MgrSingleton<HUDFontSystem>
    {
        private HUDFontSystem()
        {
            
        }
        /// <summary>
        /// key->strList, 目前显示的strList
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, List<HUDFontData>> dataDic = new Dictionary<int, List<HUDFontData>>();
        FontRenderData fontRenderData = null;
        
        private const int dataTexSize = 1024;
        private Texture2D dataTex;
        private const int WordMaxCount = 87381;
        private int renderCount = 0;
        private bool needUpdate = false;

        public void Init(Material material)
        {
            renderCount = 0;
            
            Material mat = material;
            InitFontRenderData(mat);
            CreateTex();

            EventManager.AddListenerWithAParam(GameEvent.UpdateFontTexArray, ResetFontTextureArray);
        }
        
        void CreateTex()
        {
            if (dataTex == null)
            {
                dataTex = new Texture2D(dataTexSize, dataTexSize, TextureFormat.RGBAFloat, false);
                dataTex.wrapMode = TextureWrapMode.Clamp;
                dataTex.filterMode = FilterMode.Point;
                dataTex.name = "HUDFontTex";
                fontRenderData.SetFontDataTex(dataTex);
            }
        }

        void InitFontRenderData(Material mat)
        {
            if (mat != null)
            {
                fontRenderData = new FontRenderData(WordMaxCount, MeshCreater.GeneratorQuadMesh(false, true), mat);
                fontRenderData.SetFontAtlasData(FontTexArraySystem.Instance.GetFontAtlasData());
            }
            else
                Debug.LogError("没有文字材质");
        }

        public void SetFontData()
        {
            if (!needUpdate)
                return;
            var rawData = dataTex.GetRawTextureData<float>();
            
            int index = 0;
            int wordCount = 0;
            int pos = 0;
            foreach (var data in dataDic)
            {
                wordCount += data.Value.Count;
                if (data.Value.Count == 0)
                    continue;
                
                for (int i = 0; i < data.Value.Count; ++i)
                {
                    rawData[index++] = data.Value[i].offset.x;
                    rawData[index++] = data.Value[i].offset.y;
                    rawData[index++] = data.Value[i].offset.z;
                    rawData[index++] = data.Value[i].arrayIndex;
                        
                    rawData[index++] = data.Value[i].colorType;
                    rawData[index++] = data.Value[i].fontScale;
                    rawData[index++] = data.Value[i].fontSize.x;
                    rawData[index++] = data.Value[i].fontSize.y;
                    
                    rawData[index++] = pos % 5 * 2;
                    rawData[index++] = pos / 5;
                    rawData[index++] = 0;
                    rawData[index++] = data.Value[i].alive;
                }

                pos++;
            }
            if (wordCount != renderCount)
            {
                Debug.LogError($"hudFont dataDic数量{wordCount}与renderCount{renderCount}不符");
                renderCount = wordCount;
            }
            if (index > 0)
                dataTex.Apply(false, false);
            needUpdate = false;
        }

        public void RenderFont()
        {
            UnityEngine.Profiling.Profiler.BeginSample("FontRenderPrepare");
            SetFontData();
            fontRenderData.Render(renderCount);
            UnityEngine.Profiling.Profiler.EndSample();
        }

        void ResetFontTextureArray(object tex)
        {
            fontRenderData.SetFontTextureArray(tex as Texture2DArray);
        }

        // 添加检测移到上层,能调用该函数说明一定可以添加
        public void ShowFont(int id, float scale, string content, float maxLength = 10)
        {
            // 每行所拥有的文字列表
            List<HUDFontData> strList = new List<HUDFontData>();
            if (dataDic.ContainsKey(id))
            {
                renderCount -= dataDic[id].Count;
                dataDic[id].Clear();
                dataDic.Remove(id);
            }
            dataDic.Add(id, strList);

            float fontscale = scale / FontTexArraySystem.FontScaleBase;

            Vector2 strSize = Vector2.zero;
            for (int i = 0; i < content.Length; i++)
            {
                uint word = content[i];
                if (Char.IsHighSurrogate(content[i]))
                {
                    word = (uint)Char.ConvertToUtf32(content[i], content[++i]);
                }
                
                FontTexArraySystem.Instance.NeedSkip(word);
                FontInfo info = FontTexArraySystem.Instance.GetFontInfo(word);
                if (info == null)
                {
                    word = '\u25a1';
                    info = FontTexArraySystem.Instance.GetFontInfo(word);
                    if (info == null)
                        continue;
                }
                
                GlyphMetrics glm = info.character.glyph.metrics;
                if (strSize.x + glm.width * fontscale > maxLength)
                    break;
                
                strList.Add(new HUDFontData());
                ++renderCount;
                
                HandleFontData(glm, info, strList[strList.Count - 1], fontscale, 0, ref strSize);
                strList[strList.Count - 1].word = (char)word;
                strList[strList.Count - 1].shipID = id;
                strList[strList.Count - 1].alive = 1;
            }
            
            needUpdate = true;
        }

        void HandleFontData(GlyphMetrics glm, FontInfo info, HUDFontData data, float fontScale, int colorType, ref Vector2 strSize)
        {
            // fontSize负责uv和网格大小,这两是一致的
            data.fontSize.x = glm.width == 0 ? 24.70313f : glm.width + 2 * FontTexArraySystem.Instance.GetPadding();
            data.fontSize.y = glm.height + 2 * FontTexArraySystem.Instance.GetPadding();
            // 通过colorType直接确定颜色, arrayIndex是文字所在纹理数组的索引, fontScale是字号在shader中具体计算文字的缩放等信息,offset是文字起点
            data.colorType = colorType;
            data.arrayIndex = info.arrayIndex;
            data.fontScale = fontScale;
            strSize.x += glm.horizontalBearingX * fontScale;
            data.offset = new Vector3(strSize.x, (glm.horizontalBearingY - glm.height) * fontScale, 0);
            strSize.x += (glm.horizontalAdvance - glm.horizontalBearingX) * fontScale;
            strSize.y = Math.Max(glm.height * fontScale, strSize.y);
        }

        public void SaveTexture()
        {
            Utils.SaveTexture(dataTex);
        }
        
        public override void Destroy()
        {
            base.Destroy();
            
            fontRenderData?.Clear();
            fontRenderData = null;
            
            dataDic?.Clear();

            EventManager.RemoveListenerWithAParam(GameEvent.UpdateFontTexArray, ResetFontTextureArray);
        }
    }
}
