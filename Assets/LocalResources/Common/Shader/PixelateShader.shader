Shader "Custom/PixelShader"
{
    Properties
    {
        _MainTex ("MaintTex", 2D) = "white" {}
        _PixelInterval("PixelInterval",Range(0.000001,1.0))=1.0//像素化强度
        _Indensity("Indensity",Range(0,1))=1.0 //影响强度
    }
    SubShader
    {
        Tags{
            "RenderType"="Opaque"
            "RenderPipeline"="UniversalRenderPipeline"
        }
        Pass
        {

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
            CBUFFER_END
            float _PixelInterval;
            float _PixelItensity;

            struct Attributes 
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct Varyings 
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float2 uv : TEXCOORD4;
            };
            Varyings vert(Attributes i) 
            {
                Varyings output;
                output.worldPos = TransformObjectToWorld(i.vertex.xyz);
                output.pos = TransformWorldToHClip(output.worldPos);
                output.uv = TRANSFORM_TEX(i.texcoord,_MainTex);
                return output;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                float2 pixelAmount = i.uv / _PixelInterval;
                float2 pixelAmount_int = pixelAmount - frac(pixelAmount);
                pixelAmount_int *= _PixelInterval;
                float2 uv = lerp(i.uv, pixelAmount_int,_PixelItensity); 
                col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                return col;
            }
            ENDHLSL
        }
    }
}
