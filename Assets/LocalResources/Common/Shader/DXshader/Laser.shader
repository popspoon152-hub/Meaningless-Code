Shader "Custom/Laser"
{
    Properties
    {
        _MainTex ("MaintTex", 2D) = "white" {}
        _LaserMask("LaserMask",2D)="white"{}
        _LaskTex("LaskTex",2D)="white"{}
    }
    SubShader
    {
        Tags{
            "RenderType"="Opaque"
            "RenderPipeline"="UniversalRenderPipeline"
        }
        Cull Off
        ZWrite Off
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

            TEXTURE2D(_LaskMask);
            SAMPLER(sampler_LaskMask);

            TEXTURE2D(_LaskTex);
            SAMPLER(sampler_LaskTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _LaserMask_ST;
                float4 _LaserTex_ST;
            CBUFFER_END

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

            half4 frag(Varyings i) : SV_Target
            {
                half4 texcol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                half4 laskcol = SAMPLE_TEXTURE2D(_LaskTex, sampler_LaskTex, i.uv + _Time.y * 0.5);
                half4 laskmask = SAMPLE_TEXTURE2D(_LaskMask, sampler_LaskMask, i.uv + _Time.x);
                return texcol * (laskcol + 1 - laskmask*2);
            }
            ENDHLSL
        }
    }
}

