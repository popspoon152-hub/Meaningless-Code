Shader "Custom/PlayerShader"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        [Toggle]_IsBleeding("玩家是否处于扣血状态",Int)=1.0
        _ParticleColor ("粒子颜色", Color) = (1, 0.5, 0, 1)
        _EdgeWidth("描边宽度",Float)=1.0
        _Intensity ("粒子密度", Float) = 0.5
        _ParticleSpeed ("粒子闪烁速度", Range(0, 3)) = 1
        [Toggle]_IsAttcked("玩家是否被攻击",Int)=1
        _GlitchSpeed("受击抖动速度",Float)=1.0
        _GlitchStrength("受击抖动强度",Float)=1.0
        _BlockSize("抖动色块大小",Float)=1.0
        _BlockSpeed("色块抖动速度",Float)=1.0
        [HDR] _Emission ("自发光", Color) = (0.0, 0.0, 0.0, 0.0)
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }


        ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local _ISBLEEDING_ON
            #pragma shader_feature_local _ISATTCKED_ON
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
            struct Attributes
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };
            
            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float4 pos : SV_POSITION;
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
                half4 _MainTex_ST;
            CBUFFER_END

            half4 _MainTex_TexelSize;
            half4 _ParticleColor;
            half4 _Emission;

            float _Intensity;
            float _ParticleSpeed;
            float _EdgeWidth;
            float _GlitchSpeed;
            float _GlitchStrength;
            float _BlockSize;
            float _BlockSpeed;
            
            Varyings vert (Attributes i)
            {
                Varyings output;

                float4 offset = float4(0.0, 0.0 ,0.0 ,0.0);
                
                #if defined(_ISBLEEDING_ON) 
                float time = _Time.y * _GlitchSpeed ;
                offset.x =frac(sin(dot(i.texcoord + time, float2(12.9898, 78.233))) * 43758.54531)*_GlitchStrength-_GlitchStrength/2;
                #endif

                output.worldPos = TransformObjectToWorld(i.vertex.xyz + offset);
                output.pos = TransformWorldToHClip(output.worldPos);
                output.uv = TRANSFORM_TEX(i.texcoord,_MainTex);
                return output;
            }

            inline float randomNoise(float2 seed)
            {
                return frac(sin(dot(seed * floor(_Time.y * _BlockSpeed), float2(17.13, 3.71))) * 43758.5453123);
            }

            inline float randomNoise(float seed)
            {
                return randomNoise(float2(seed, 1.0));
            }
            
            half4 frag (Varyings i) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                float alpha = col.a;

                half4 particle = (0,0,0,0);
                half displaceNoise = 1;
                _Emission = (0.0,0.0,0.0,0.0);

                //角色失血时边缘附着的粒子效果
                #if defined(_ISBLEEDING_ON) 
                float2 pixelSize = _MainTex_TexelSize.xy * _EdgeWidth;
                half a1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(-_EdgeWidth / _ScreenParams.x, _EdgeWidth / _ScreenParams.y)).a;
                half a2 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(_EdgeWidth / _ScreenParams.x,- _EdgeWidth / _ScreenParams.y)).a;
                half a3 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(-_EdgeWidth / _ScreenParams.x,-_EdgeWidth / _ScreenParams.y)).a;
                half a4 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(_EdgeWidth / _ScreenParams.x,_EdgeWidth / _ScreenParams.y)).a;
                
                half edge = frac(saturate((a1 + a2 + a3 + a4) * 0.5 - col.a));
                float time = _Time.y * _ParticleSpeed;
                float noise = frac(sin(dot(i.uv + time, float2(12.9898, 78.233))) * 43758.54531 * (1-col.a));
                particle = _ParticleColor  * edge  * _Intensity * noise ;
                particle.a *= edge;
                half2 block = randomNoise(floor(i.uv * _BlockSize));
                displaceNoise = max(pow(block.x, 1)*2,0.8);
                _Emission = (1.0,1.0,1.0,1.0);
                _Emission *= displaceNoise;
                #else
                return col;
                #endif

                //角色受击时区块抖动
                #if defined(_ISATTCKED_ON)
                return particle + col + _Emission * col.a;
                #endif

                return particle + col * displaceNoise * _Emission;
            }
            ENDHLSL
        }
    }
}
