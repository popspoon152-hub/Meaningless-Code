Shader "Custom/SimpleEdgeParticles"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _ParticleColor ("Particle Color", Color) = (1, 0.5, 0, 1)
        _EdgeWidth("EdgeWidth",Float)=1.0
        _Intensity ("Intensity", Float) = 0.5
        _Speed ("Speed", Range(0, 3)) = 1
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
                float4 _MainTex_ST;
            CBUFFER_END
            float4 _MainTex_TexelSize;
            float4 _ParticleColor;
            float _Intensity;
            float _Speed;
            float _EdgeWidth;
            
            Varyings vert (Attributes i)
            {
                Varyings output;
                output.worldPos = TransformObjectToWorld(i.vertex.xyz);
                output.pos = TransformWorldToHClip(output.worldPos);
                output.uv = TRANSFORM_TEX(i.texcoord,_MainTex);
                return output;
            }
            
            half4 frag (Varyings i) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                float alpha = col.a;
                float2 pixelSize = _MainTex_TexelSize.xy * _EdgeWidth;

                half a1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(-_EdgeWidth / _ScreenParams.x, _EdgeWidth / _ScreenParams.y)).a;
                half a2 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(_EdgeWidth / _ScreenParams.x,- _EdgeWidth / _ScreenParams.y)).a;
                half a3 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(-_EdgeWidth / _ScreenParams.x,-_EdgeWidth / _ScreenParams.y)).a;
                half a4 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(_EdgeWidth / _ScreenParams.x,_EdgeWidth / _ScreenParams.y)).a;
                
                half edge = frac(saturate((a1 + a2 + a3 + a4) * 0.5 - col.a));
                
                // 粒子效果
                float time = _Time.y * _Speed;
                float noise = frac(sin(dot(i.uv + time, float2(12.9898, 78.233))) * 43758.54531 * (1-col.a));
                
                half4 particle = _ParticleColor  * edge  * _Intensity * noise ;//
                
                particle.a *= edge;
                
                return particle  + col;
            }
            ENDHLSL
        }
    }
}