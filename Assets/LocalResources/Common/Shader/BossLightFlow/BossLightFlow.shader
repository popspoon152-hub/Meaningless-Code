Shader "Custom/BossLightFlow"
{
    Properties
    {
        _StripeCol("条纹颜色",Color) = (0,0.9,1,1)
        [HDR]_FlowCol("流光颜色",Color) = (0,1,1,1)
        _StripeTex("条纹遮罩贴图",2D) = "white"{}
        _FlowTex("流光遮罩贴图",2D) = "white"{}
        _FlowDistort("流光顺序扰动",Range(0,50)) = 10.1234
        _StripeInterval("条纹间隔",Int) = 50
        _StripeWidth("条纹宽度",Range(0,1)) = 0.3
        _FlowSpeed("流光速度",Range(0,20)) = 5
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

            float3 _StripeCol;
            float3 _FlowCol;

            TEXTURE2D(_FlowTex);
            SAMPLER(sampler_FlowTex);
            float4 _FlowTex_ST;

            TEXTURE2D(_StripeTex);
            SAMPLER(sampler_StripeTex);
            float4 _StripeTex_ST;

            float _FlowDistort;
            int _StripeInterval;
            float _StripeWidth;
            float _FlowSpeed;
            float _Opacity;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _FlowTex); // Unity 内置宏
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float newUVY = _StripeInterval * i.uv.y;
                // float stripeMask = step(_StripeWidth, frac(newUVY));
                // float bottomStripe = step(floor(newUVY), 0) * step(newUVY, 1 - _StripeWidth);
                // stripeMask += bottomStripe;

                float stripeMask = SAMPLE_TEXTURE2D(_StripeTex, sampler_StripeTex, i.uv).r;

                float stripeIndex = floor(newUVY) + round(newUVY);
                float phase = stripeIndex * _FlowDistort;

                float2 flowUV = float2(i.uv.x + _FlowSpeed * _Time.x + phase, i.uv.y);
                float flowMask = SAMPLE_TEXTURE2D(_FlowTex, sampler_FlowTex, flowUV);

                return float4(stripeMask * (_StripeCol + flowMask * _FlowCol),  _Opacity);
            }
            ENDHLSL
        }
    }
}