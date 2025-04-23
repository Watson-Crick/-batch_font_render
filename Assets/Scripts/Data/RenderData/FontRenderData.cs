using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VNWGame.Game
{
    public class FontRenderData : RenderData
    {
        public GraphicsBuffer buffer_fontDatas;
        public GraphicsBuffer buffer_fontIds;
        Material mat;

        public FontRenderData(int WordMaxCount, Mesh mesh, Material material) : base(WordMaxCount, mesh, material)
        {
            mat = material;
            mpb.SetFloat("_CameraFactor", 10);
            
            rp.layer = LayerMask.NameToLayer("UI");
        }

        public void SetFontAtlasData(Vector4 data)
        {
            mpb.SetVector("_AtlasData", data);
        }

        public void SetFontTextureArray(Texture2DArray texArray)
        {
            #if UNITY_EDITOR
                mat.SetTexture("_MainTex", texArray);
            #endif
            mpb.SetTexture("_MainTex", texArray);
        }

        public void SetFontDataTex(Texture2D dataTex)
        {
            #if UNITY_EDITOR
                mat.SetTexture("_DataTex", dataTex);
            #endif
            mpb.SetTexture("_DataTex", dataTex);
            
            mpb.SetInteger("_DataTexSize", dataTex.width);
        }
    }
}


