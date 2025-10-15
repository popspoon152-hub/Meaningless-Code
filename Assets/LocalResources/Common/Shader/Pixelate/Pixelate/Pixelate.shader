Shader "Custom/Pixelate"
{
    Properties
    {
//        _Columns("列数",Int) = 10
//        _Rows("行数",Int) = 10
        _Interval("像素格数",Int) = 10
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BlitTexture);
            SAMPLER(sampler_BlitTexture);
            // float _Columns;
            // float _Rows;
            float _Interval;


            struct appdata
            {
                uint vertexID : SV_VertexID;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 position : SV_POSITION;
            };


            v2f vert(appdata v)
            {
                v2f o;
                float2 pos;
                if (v.vertexID == 0) pos = float2(-1, -1);
                else if (v.vertexID == 1) pos = float2(3, -1);
                else pos = float2(-1, 3);
                o.position = float4(pos, 0, 1);
                // o.uv = pos * 0.5 + 0.5;
                float2 uv = pos * 0.5 + 0.5;
                o.uv = float2(uv.x, 1.0 - uv.y); // 翻转 y 轴 UV
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                // float2 uv = float2(round(i.uv.x * _Columns) / _Columns, round(i.uv.y * _Rows) / _Rows);
                float screenRatio = _ScreenParams.x / _ScreenParams.y;
                float2 uv = float2(round(i.uv.x * _Interval * screenRatio) / (_Interval * screenRatio),round(i.uv.y * _Interval) / _Interval);
                float4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, uv);
                return col;
            }
            ENDHLSL
        }
    }
}