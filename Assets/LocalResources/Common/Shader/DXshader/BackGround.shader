Shader "Custom/BackGround"
{
    Properties
    {
        _Tex ("Albedo (RGB)", 2D) = "white" {}
        _BlockSize("BlockSize",Float)=4.0//区块大小
        _Speed("Speed",Float)=40.0//抖动速度
        _MaxRGBSplitX("MaxRGBSplitX",Float)=2.0//X最大抖动
        _MaxRGBSplitY("MaxRGBSplitY",Float)=2.0//Y最大抖动
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

            CBUFFER_START(UnityPerMaterial)
                float4 _Tex_ST;
            CBUFFER_END

            float _BlockSize;
            float _Speed;
            float _MaxRGBSplitX;
            float _MaxRGBSplitY;

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
                half4 texcol = SAMPLE_TEXTURE2D(_Tex, sampler_Tex, float2(i.uv.x,i.uv.y + _Time.x));
                half2 block = randomNoise(floor(i.uv * _BlockSize));//切分生成随机区块

                float displaceNoise = pow(block.x, 8.0) * pow(block.x, 3.0);//二次筛选
                float splitRGBNoise = pow(randomNoise(7.2341), 17.0);//分离RGB

                float offsetX = displaceNoise - splitRGBNoise * _MaxRGBSplitX;
                float offsetY = displaceNoise - splitRGBNoise * _MaxRGBSplitY;

                float noiseX = 0.05 * randomNoise(13.0);
                float noiseY = 0.05 * randomNoise(7.0);
                float2 offset = float2(offsetX * noiseX, offsetY* noiseY);

                half4 colorR = SAMPLE_TEXTURE2D(_Tex, sampler_Tex, float2(i.uv.x,i.uv.y + _Time.x));
                half4 colorG = SAMPLE_TEXTURE2D(_Tex, sampler_Tex, float2(i.uv.x,i.uv.y + _Time.x) + offset);
                half4 colorB = SAMPLE_TEXTURE2D(_Tex, sampler_Tex, float2(i.uv.x,i.uv.y + _Time.x) - offset);

                return half4(colorR.r , colorG.g, colorB.b, 0.9);
            }
            ENDHLSL
        }
    }
}
