Shader "Game/HUDFontSDF" {
    Properties
    {
        _FaceDilate("FaceDilate", Range(-1, 1)) = 0.3
        _OutlineWidth("Outline Thickness", Range(0,1)) = 0.5
	    _OutlineSoftness("Outline Softness", Range(0,1)) = 0.2
        _ScaleRatioA("Scale Ratio A", float) = 1

        _MainTex("Font Atlas", 2DArray) = "white" {}
        _GradientScale("Gradient Scale(debug setting)", float) = 5
        _ScaleX("Scale X(debug setting)", float) = 1
        _ScaleY("Scale Y(debug setting)", float) = 1
        _PerspectiveFilter("Perspective Correction(debug setting)", Range(0, 1)) = 0.875
        _Sharpness("Sharpness(debug setting)", Range(-1,1)) = 0
        _Bias("方向偏移", Range(0, 20)) = 0
    }
    SubShader{
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent"}

        ZWrite True
        ZTest LEqual
        Cull Back
        Blend SrcAlpha OneMinusSrcAlpha
        Pass {
            Tags { "LightMode" = "ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"
            
            struct a2v
            {
                float4 vertex   : POSITION;
                float3 normal   : NORMAL;
            };

            struct v2f
            {
                float4 vertex       : SV_POSITION;
                float3 uv           : TEXCOORD0;    // Texture UV(xy), ArrayIndex(z)
                float4 param        : TEXCOORD1;
                float4 param2       : TEXCOORD2;
                float4 color        : TEXCOORD3;
                float4 outlineColor : TEXCOORD4;
            };

            uniform float _FaceDilate;
            uniform float _OutlineWidth;
            uniform float _OutlineSoftness;
            uniform float _ScaleRatioA;

            UNITY_DECLARE_TEX2DARRAY(_MainTex);
            UNITY_DECLARE_TEX2D_FLOAT(_DataTex);

            uniform uint _DataTexSize;
            uniform float _GradientScale;
            uniform float _ScaleX;
            uniform float _ScaleY;
            uniform float _PerspectiveFilter;
            uniform float _Sharpness;
            uniform float _CameraFactor;
            uniform float _Bias;
            // rouCount, columnCount, texUnitCount, atlasWidth
            float4 _AtlasData;

            float2 getUV(uint i, uint size)
            {
                float2 uv = float2(i % size, i / size);
                return (uv + 0.5) / size;
            }
            
            float4 GetOutlineColorByType(int showType)
            {
                if (showType == 0) // 己方联盟
                    return float4(1, 1, 1, 1);
                if (showType == 1) // 敌方1联盟(红色)
                    return float4(1, 0.125, 0.039, 1);
                if (showType == 2) // 敌方2联盟(蓝色)
                    return float4(0.039, 0.889, 1, 1);
                if (showType == 3) // 敌方3联盟(紫色)
                    return float4(0.776, 0.176, 1, 1);
                return float4(1, 1, 1, 1);
            }
            
            v2f vert(a2v i, uint instanceID: SV_InstanceID, uint vertexID : SV_VERTEXID) {
                v2f o;
                float2 temp = float2(vertexID > 1 ? 1 : 0, vertexID % 2);
                int j = instanceID * 3;
                float4 param = UNITY_SAMPLE_TEX2D_LOD(_DataTex, getUV(j++, _DataTexSize), 0);
                float3 offset = param.xyz;
                float arrayIndex = param.w;
                
                param = UNITY_SAMPLE_TEX2D_LOD(_DataTex, getUV(j++, _DataTexSize), 0);
                float colorType = param.x;
                float fontScale = param.y;
                float2 fontSize = param.zw;
                
                param = UNITY_SAMPLE_TEX2D_LOD(_DataTex, getUV(j++, _DataTexSize), 0);
                float3 pos = param.xyz;
                float alive = param.w;
                
                float4 outlineColor = float4(0.004, 0.008, 0.027, 1);
                float4 color = GetOutlineColorByType(colorType);
                
                float4 vert = i.vertex;
                float3 normal = UNITY_MATRIX_V[2].xyz;
                float3 right = normalize(cross(normal, float3(0, 1, 0)));
                float3 up = normalize(cross(right, normal));

                // 缩放
                vert.xy = temp * fontSize * fontScale;
                // 旋转
                vert.xyz = right * vert.x + up * vert.y + normal * vert.z;
                // 偏移
                vert.xyz += (offset.x * right + offset.y * up + offset.z * normal);
                // 恒定位置
                vert.xyz += pos + _Bias * up + normal * 0.3;
                float4 vPosition = UnityObjectToClipPos(vert);

                float2 pixelSize = vPosition.w;
                // 将投影矩阵 UNITY_MATRIX_P 乘以屏幕分辨率，得到空间中（带深度w）对应的缩放分辨率
                pixelSize /= float2(_ScaleX, _ScaleY) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));
                
                float scale = rsqrt(dot(pixelSize, pixelSize));
                scale *= _GradientScale * (_Sharpness + 1);
                if(UNITY_MATRIX_P[3][3] == 0)
                {
                    // 透视投影让文本在视线与法线处于锐角的情况下更加的柔和
                    scale = lerp(abs(scale) * (1 - _PerspectiveFilter), scale, abs(dot(UnityObjectToWorldNormal(i.normal.xyz), normalize(WorldSpaceViewDir(vert)))));
                }

                float weight = (0.125 + _FaceDilate) * 0.83333 * 0.5;

                scale /= 1 + (_OutlineSoftness * _ScaleRatioA * scale);
                float bias = (0.5 - weight * 0.3) * scale - 0.5;
                float outline = _OutlineWidth * _ScaleRatioA * 0.5 * scale;

                outlineColor.rgb *= color.a;
                outlineColor = lerp(color, outlineColor, sqrt(min(1.0, (outline * 2))));

                int atlasIndex = arrayIndex / _AtlasData.z;
                float2 rect = _AtlasData.w / float2(_AtlasData.x, _AtlasData.y);
                uint row = _AtlasData.x / _AtlasData.w;
                float2 uv = float2(arrayIndex % _AtlasData.z % row * rect.x, floor(arrayIndex % _AtlasData.z / row) * rect.y);
                // 这里看看使用另一种方式,即找到中心然后用Sampling pointsize减去glyphRect除2来处理
                uv += temp * fontSize / _AtlasData.xy;
                o.vertex = vPosition;
                o.uv = float3(uv.x, uv.y, atlasIndex);
                // o.mask = half4(vert.xy * 2 - clampedRect.xy - clampedRect.zw, 0.25 / (0.25 * half2(_MaskSoftnessX, _MaskSoftnessY) + pixelSize.xy));
                o.param = float4(scale, bias - outline, bias + outline, bias);
                o.param2 = float4(alive, 0, 0 , 0);
                o.outlineColor = outlineColor;
                o.color = color;
                return o;
            }

            float4 frag(v2f o) : SV_Target {
                clip(o.param2.x - 0.5);
                
                float d = UNITY_SAMPLE_TEX2DARRAY(_MainTex, o.uv.xyz).a * o.param.x;
                float4 c = lerp(o.outlineColor, o.color, saturate(d - o.param.z));
                c *= saturate(d - o.param.y);

                // #if UNITY_UI_CLIP_RECT
                // float2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(o.mask.xy)) * o.mask.zw);
                // c *= m.x * m.y;
                // #endif

                clip(c.a - 0.001);

                return c;
            }
            ENDCG
        }
    }
}