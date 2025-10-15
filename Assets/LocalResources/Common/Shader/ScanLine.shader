Shader "Custom/ScanLine"
{
    Properties
    {
        _MainTex ("MaintTex", 2D) = "white" {}
        _Frequency("Frequency",Float)=0
        _Threshold("Threshold",Float)=0
        _Amount("Amount",Range(0,1.0))=0.5
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
            float _Frequency;
            float _Threshold;
            float _Amount;

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
                half strength = 0;
		        #if USING_FREQUENCY_INFINITE
			        strength = 1;
		        #else
			    strength = 0.5 + 0.5 * cos(_Time.y * _Frequency);
		        #endif
		
		
		        float jitter = randomNoise(i.uv.y, _Time.x) * 2 - 1;
		        jitter *= step(_Threshold, abs(jitter)) * _Amount * strength;
		
		        half4 sceneColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, frac(i.uv + float2(jitter, 0)));
		
		        return sceneColor;
            }

            ENDHLSL
        }
    }
}
