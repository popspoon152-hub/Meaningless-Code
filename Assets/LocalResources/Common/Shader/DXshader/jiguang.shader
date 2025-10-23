Shader "Custom/jiguang"
{
    Properties
    {
        _BaseColor ("BaseColor", 2D) = "white" {}
        _EmissionMask ("EmissionMask", 2D) = "white" {}
        _Speed("Speed",Float) = 1.0
        [HDR]_Emission("Emission",Color)=(1.0,1.0,1.0,1.0)
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "RenderPipeline"="UniversalRenderPipeline"
            "IgnoreProjector" = "true"
        }
        Cull Off
        ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local _ISOVER_ON
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
            TEXTURE2D(_BaseColor);
            TEXTURE2D(_EmissionMask);
            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_BaseColor);
            SAMPLER(sampler_EmissionMask);
            SAMPLER(sampler_NoiseTex);
            float _Speed;
            float _DissolveThreshold;
            float _EdgeColor;
            float _EdgeWidth;
            half4 _Emission;

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor_ST;
                float4 _EmissionMask_ST;
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
                output.worldPos = TransformObjectToWorld(i.vertex);
                output.pos = TransformWorldToHClip(output.worldPos);
                output.uv = TRANSFORM_TEX(i.texcoord,_BaseColor);
                return output;
            }

            half4 frag(Varyings i) : SV_Target
            {
                half4 texcol = SAMPLE_TEXTURE2D(_BaseColor, sampler_BaseColor, float2 ( i.uv.x + _Time.x * _Speed , i.uv.y ));
                half4 mask = SAMPLE_TEXTURE2D(_EmissionMask, sampler_EmissionMask, float2 ( i.uv.x + _Time.x * _Speed , i.uv.y ) );
                return texcol * _Emission * mask ;
            }
            ENDHLSL
        }
    }
}

