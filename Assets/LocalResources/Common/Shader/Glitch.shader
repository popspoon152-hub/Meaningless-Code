Shader "Custom/Glitch"
{
    Properties
    {
        _MainTex ("MaintTex", 2D) = "white" {}
        _Indensity("Indensity",Float)=1.0
        _TimeX("TimeX",Float)=1.0
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
            float _Indensity;
            float _TimeX;

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

            float randomNoise(float x, float y)
            {
                return frac(sin(dot(float2(x, y), float2(12.9898, 78.233))) * 43758.5453);
            }

            half4 frag(Varyings i) : SV_Target
            {
                float splitAmount = _Indensity * randomNoise(_TimeX, 2);

                half4 ColorR = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2(i.uv.x + splitAmount, i.uv.y));
                half4 ColorG = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                half4 ColorB = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2(i.uv.x - splitAmount, i.uv.y));

                return half4(ColorR.r, ColorG.g, ColorB.b, 1);
            }
            ENDHLSL
        }
    }
}
