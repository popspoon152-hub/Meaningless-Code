Shader "Custom/DeadBlock"
{
    Properties
    {
        _Tex ("Albedo (RGB)", 2D) = "white" {}
        _EmissionMask ("EmissionMask", 2D) = "white" {}
        _Speed("Speed",Float)=1.0
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
		Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
            TEXTURE2D(_Tex);
            SAMPLER(sampler_Tex);
            TEXTURE2D(_EmissionMask);
            SAMPLER(sampler_EmissionMask);
            float4 _Emission;
            float _Speed;

            CBUFFER_START(UnityPerMaterial)
                float4 _Tex_ST;
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
                output.uv = TRANSFORM_TEX(i.texcoord,_Tex);
                return output;
            }

            half4 frag(Varyings i) : SV_Target
            {
                half4 texcol = SAMPLE_TEXTURE2D(_Tex, sampler_Tex, float2(i.uv.x- _Time.x * _Speed,i.uv.y ));
                half4 mask = SAMPLE_TEXTURE2D(_EmissionMask, sampler_EmissionMask, float2 ( i.uv.x - _Time.x *_Speed, i.uv.y ) );
                return texcol * mask * _Emission ;
            }
            ENDHLSL
        }
    }
}