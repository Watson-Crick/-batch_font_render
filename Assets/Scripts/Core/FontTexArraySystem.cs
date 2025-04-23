using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore;
using System;

namespace VNWGame.Game
{
    public class FontInfo
    {
        public bool haveCharacter;
        public TMP_Character character;
        
        public int count;
        public int arrayIndex;
        public Vector2 startUV;
    }

    public class FontTexArraySystem : MgrSingleton<FontTexArraySystem>
    {
        private FontTexArraySystem()
        {
            
        }
        public static float FontScaleBase = 512f;

        private Texture2DArray fontAtlasArray;
        // 存储每个字的使用次数, 当文字纹理被占满时,需要根据该数据来删除可移除文字
        private Dictionary<uint, FontInfo> fontTexArrayInfoDic = new Dictionary<uint, FontInfo>();
        private Queue<int> unUseFontArrayIndex = new Queue<int>();
        private Queue<uint> canRemoveFont = new Queue<uint>();
        private int rowCount, columnCount, texUnitCount;
        private bool needUpdateFont = false;
        private TMP_FontAsset tmp_Font = null;
        private const int ArrayMaxCount = 3;
        private const int Padding = 9;
        private const int SamplingPointSize = 90;
        private List<TMP_FontAsset> fontFallbackList = new List<TMP_FontAsset>();
        
        public void Init(TMP_FontAsset mainFonts, List<TMP_FontAsset> fallbackList)
        {
            tmp_Font = mainFonts;
            fontAtlasArray = new Texture2DArray(tmp_Font.atlasWidth, tmp_Font.atlasHeight, ArrayMaxCount, tmp_Font.atlasTextures[0].format, false);
            // 初始化字体纹理数组为全黑
            Color32[] cols = new Color32[tmp_Font.atlasWidth * tmp_Font.atlasHeight];
            rowCount = tmp_Font.atlasWidth / (SamplingPointSize + 2 * Padding);
            columnCount = tmp_Font.atlasHeight / (SamplingPointSize + 2 * Padding);
            texUnitCount = rowCount * columnCount;
            for (int i = 0; i < ArrayMaxCount; ++i)
            {
                fontAtlasArray.SetPixels32(cols, i, 0);
                for (int j = 0; j < texUnitCount; ++j)
                {
                    unUseFontArrayIndex.Enqueue(j + i * texUnitCount);
                }
            }

            foreach (var fontAsset in fallbackList)
            {
                fontFallbackList.Add(fontAsset);
            }
        }

        public TMP_FontAsset GetMainFontAsset()
        {
            return tmp_Font;
        }

        public Vector4 GetFontAtlasData()
        {
            return new Vector4(tmp_Font.atlasHeight, tmp_Font.atlasWidth, texUnitCount, SamplingPointSize + 2 * Padding);
        }

        private void ResetFontTex2DArray(uint word)
        {
            TMP_Character t_ch = fontTexArrayInfoDic[word].character;
            int arrayIndex = fontTexArrayInfoDic[word].arrayIndex;
            Debug.LogError((char)word + "   " + t_ch.glyph.glyphRect.x + "   " + t_ch.glyph.glyphRect.y + "   " + t_ch.glyph.glyphRect.width + "   " + t_ch.glyph.glyphRect.height);
            int x = (t_ch.glyph.glyphRect.x - Padding); // 左上角 X 坐标
            int y = (t_ch.glyph.glyphRect.y - Padding); // 左上角 Y 坐标
            int width = (t_ch.glyph.glyphRect.width + 2 * Padding); // 宽度
            int height = (t_ch.glyph.glyphRect.height + 2 * Padding); // 高度
            Graphics.CopyTexture(tmp_Font.atlasTextures[t_ch.glyph.atlasIndex], 
                0, 
                0,
                Math.Max(0, x), 
                Mathf.Max(0, y), 
                Mathf.Min(tmp_Font.atlasWidth - x, width), 
                Mathf.Min(tmp_Font.atlasHeight - x, height), 
                fontAtlasArray, 
                arrayIndex / texUnitCount, 
                0, 
                arrayIndex % texUnitCount % rowCount * (SamplingPointSize + 2 * Padding), 
                arrayIndex % texUnitCount / rowCount * (SamplingPointSize + 2 * Padding));
            needUpdateFont = true;
        }

        public void RemoveFont(uint word)
        {
            if (fontTexArrayInfoDic.ContainsKey(word))
            {
                fontTexArrayInfoDic[word].count--;
                if (fontTexArrayInfoDic[word].count <= 0)
                    canRemoveFont.Enqueue(word);
            }
        }

        private bool CanAdd(uint word)
        {
            if (!fontTexArrayInfoDic.ContainsKey(word))
            {
                uint[] temp = new uint[1];
                temp[0] = word;
                bool res = tmp_Font.TryAddCharacters(temp, true);
                res = res || tmp_Font.HasCharacter((int)word);
                if (res)
                {
                    fontTexArrayInfoDic.Add(word, new FontInfo()
                    {
                        haveCharacter = true,
                        character = tmp_Font.characterLookupTable[word],
                        count = -1
                    });
                }
                else
                {
                    for (int i = 0; i < fontFallbackList.Count; i++)
                    {
                        res = fontFallbackList[i].TryAddCharacters(temp, true);
                        res = res || fontFallbackList[i].HasCharacter((int)word);
                        if (res)
                        {
                            fontTexArrayInfoDic.Add(word, new FontInfo()
                            {
                                haveCharacter = true,
                                character = fontFallbackList[i].characterLookupTable[word],
                                count = -1
                            });
                            break;
                        }
                    }

                    if (!res)
                    {
                        fontTexArrayInfoDic.Add(word, new FontInfo()
                        {
                            haveCharacter = false,
                            character = null,
                            count = -1
                        });
                    }
                }
                if (!fontTexArrayInfoDic[word].haveCharacter)
                    Debug.LogErrorFormat("添加了该字体不能添加的文字{0}", (char)word);
            }
            return fontTexArrayInfoDic[word].haveCharacter;
        }

        public bool NeedSkip(uint word)
        {
            if (char.IsControl((char)word) || char.IsWhiteSpace((char)word))
            {
                return true;
            }
            if ((word >= 0x2000 && word <= 0x200f) || (word >= 0x2028 && word <= 0x202f) ||
                (word >= 0x2051 && word <= 0x206f))
            {
                return true;
            }

            return false;
        }

        public FontInfo GetFontInfo(uint word)
        {
            if (!CanAdd(word))
            {
                return null;
            }
            
            if (fontTexArrayInfoDic[word].count > 0)
            {
                fontTexArrayInfoDic[word].count++;
                return fontTexArrayInfoDic[word];
            }
            else
            {
                if (unUseFontArrayIndex.Count > 0)
                {
                    fontTexArrayInfoDic[word].count = 1;
                    fontTexArrayInfoDic[word].arrayIndex = unUseFontArrayIndex.Dequeue();
                    ResetFontTex2DArray(word);
                    return fontTexArrayInfoDic[word];
                }
                else
                {
                    while (canRemoveFont.Count > 0)
                    {
                        uint canRemoveFontIndex = canRemoveFont.Dequeue();
                        if (fontTexArrayInfoDic[canRemoveFontIndex].count <= 0)
                        {
                            unUseFontArrayIndex.Enqueue(fontTexArrayInfoDic[canRemoveFontIndex].arrayIndex);
                            fontTexArrayInfoDic.Remove(canRemoveFontIndex);
                        }
                    }

                    if (unUseFontArrayIndex.Count > 0)
                        return GetFontInfo(word);
                }
            }
            return null;
        }

        public int GetPadding()
        {
            return Padding;
        }

        public void Update()
        {
            if (needUpdateFont)
            {
                fontAtlasArray.Apply();
                EventManager.Invoke(GameEvent.UpdateFontTexArray, fontAtlasArray);
                needUpdateFont = false;
            }
        }

        public void Clear()
        {
            fontTexArrayInfoDic?.Clear();
            unUseFontArrayIndex?.Clear();
            canRemoveFont?.Clear();

            tmp_Font = null;
        }
    }
}
