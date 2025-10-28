Shader "Custom/Bean"
{
    Properties
    {
        _MainTex ("MaintTex", 2D) = "white" {}
        [HDR]_Emission("Emission",Color)=(1,1,1,1)
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

            TEXTURE2D(_EmissionMask);
            SAMPLER(sampler_EmissionMask);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _EmissionMask_ST;
            CBUFFER_END
            half4 _Emission;

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
                return texcol * _Emission;
            }
            ENDHLSL
        }
    }
}

