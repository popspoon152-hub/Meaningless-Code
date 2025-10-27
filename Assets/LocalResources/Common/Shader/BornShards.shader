Shader "Custom/BornShards"
{
    Properties
    {
        _MainTex("人物贴图",2D) = "white"{}
        _NoiseTex("噪声贴图",2D) = "white"{}
        _ShatterSpeed("破碎速度(正值为死亡，负值为复活)",Range(-10,10)) = 5
        _NoiseThreshold("噪声强度阈值",Range(0,0.1)) = 0.05
        _PixelWidth("方格宽度",Range(0,100)) = 50
        _PixelHeight("方格高度",Range(0,100)) = 50
        _CrackOffset("缝隙偏移",Range(0,50)) = 20
        _CrackCol("缝隙颜色",Color) = (1,1,1,1)
        _StopValue("破碎停止点",Range(0,1)) = 0.2
        _Opacity("不透明度",Range(0,1)) = 1
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };


            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);
            float _ShatterSpeed;

            float _NoiseThreshold;
            float _PixelWidth;
            float _PixelHeight;
            float _CrackOffset;
            float3 _CrackCol;
            float _StopValue;
            float _Opacity;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv =v.uv;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float3 mainCol = SAMPLE_TEXTURE2D(_MainTex, sampler_NoiseTex, i.uv);;
                
                float offset = frac(- _Time.x * _ShatterSpeed);

                float2 shardUV1 = float2(round(i.uv.x * _PixelWidth) / _PixelWidth,
                                        round(i.uv.y * _PixelHeight) / _PixelHeight);

                float2 shardUV2 = float2(round(i.uv.x * (_PixelWidth -_CrackOffset)) / (_PixelWidth-_CrackOffset),
                                        round(i.uv.y * (_PixelHeight-_CrackOffset)) / (_PixelHeight-_CrackOffset));
                
                float shard1 = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, shardUV1).r;
                float shard2 = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, shardUV2).r;

                float isVisible = saturate(_StopValue + i.uv.y - offset);
                
                float mask1=step(_NoiseThreshold, isVisible * shard1);
                float mask2=1-step(_NoiseThreshold, isVisible * shard2);
                
                float3 crack=mask1 * mask2 * _CrackCol;
                float3 finalCol = mainCol * mask1 + crack;
                return float4(finalCol, mask1 );
            }
            ENDHLSL
        }
    }
}