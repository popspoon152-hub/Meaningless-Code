Shader "Custom/Ground"
{
    Properties
    {
        _MainTex ("MainTex", 2D) = "white" {}
        [Toggle]_Is2rd("Is2rd",Int)=1
        _Indensity("Indensity",Float)=1.0
        _BlockSize("BlockSize",Float)=4.0//区块大小
        _Speed("Speed",Float)=40.0//抖动速度
        _MaxRGBSplitX("MaxRGBSplitX",Float)=2.0//X最大抖动
        _MaxRGBSplitY("MaxRGBSplitY",Float)=2.0//Y最大抖动
    }
    SubShader{
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
            #pragma shader_feature_local _IS2RD_ON
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float _Indensity;
            float _Saturation;
            half4 _Emission;
            float _BlockSize;
            float _Speed;
            float _MaxRGBSplitX;
            float _MaxRGBSplitY;

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
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
                output.uv = TRANSFORM_TEX(i.texcoord,_MainTex);
                return output;
            }

            inline float randomNoise(float2 seed)
            {
                return frac(sin(dot(seed * floor(_Time.y * _Speed), float2(17.13, 3.71))) * 43758.5453123);
            }

            inline float randomNoise(float seed)
            {
                return randomNoise(float2(seed, 1.0));
            }

            half4 frag(Varyings i) : SV_Target
            {
                half3 texcol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv).rgb;
                half a = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv).a;

                half2 block = randomNoise(floor(i.uv * _BlockSize));//切分生成随机区块

                float displaceNoise = pow(block.x, 8.0) * pow(block.x, 3.0);//二次筛选
                float splitRGBNoise = pow(randomNoise(7.2341), 17.0);//分离RGB

                float offsetX = displaceNoise - splitRGBNoise * _MaxRGBSplitX;
                float offsetY = displaceNoise - splitRGBNoise * _MaxRGBSplitY;

                float noiseX = 0.05 * randomNoise(13.0);
                float noiseY = 0.05 * randomNoise(7.0);
                float2 offset = float2(offsetX * noiseX, offsetY* noiseY);

                half4 colorR = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv - offset);
                half4 colorG = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv+ offset);
                half4 colorB = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv );

                half gray1 = (1-dot(colorR, half3(0.3, 0.59, 0.11))) * _Indensity;
                half gray2 = (1-dot(colorG, half3(0.3, 0.59, 0.11))) * _Indensity;
                half gray3 = (1-dot(colorB, half3(0.3, 0.59, 0.11)))* _Indensity;
                #if defined(_IS2RD_ON)
                return half4(gray1,gray2,gray3,a);
                #endif

                return half4(texcol,a);
            }
            ENDHLSL
        }

    }
}
