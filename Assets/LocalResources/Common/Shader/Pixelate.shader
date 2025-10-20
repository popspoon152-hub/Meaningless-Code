Shader "Custom/Pixelate"
{
   Properties
    {
        _MainTex ("MaintTex", 2D) = "white" {}
        _PixelInterval("PixelInterval",Range(0.000001,1.0))=1.0
        _Indensity("Indensity",Range(0,1))=1.0
    }
    SubShader
    {
        Tags{
            "RenderType"="Opaque"
            "RenderPipeline"="UniversalRenderPipeline"
        }
        Pass{

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
            float _Indensity; 

            struct Attributes 
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct Varyings 
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };
            Varyings vert(Attributes i) 
            {
                Varyings output;
                output.worldPos = TransformObjectToWorld(i.vertex.xyz);
                output.pos = TransformWorldToHClip(output.worldPos);
                output.uv = TRANSFORM_TEX(i.texcoord,_MainTex);
                return output;
            }

            half4 frag(Varyings i):SV_Target
            {
                float2 pixelAmount = 1/_PixelInterval;
                float2 pixelAmount_Int = pixelAmount - frac(pixelAmount);//得到像素格数
                float2 pixelatedUV = floor(i.uv*pixelAmount_Int)/pixelAmount_Int;
                float2 use_uv = lerp(i.uv, pixelatedUV, _Indensity);
                half4 Pixelate = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex,use_uv);

                return Pixelate;
            }
            ENDHLSL
        }
    }
}
