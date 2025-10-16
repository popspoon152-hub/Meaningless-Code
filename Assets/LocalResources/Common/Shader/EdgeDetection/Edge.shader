Shader "PostProcessing/Edge"
{
    Properties
    {
        _MainTex ("MainTexture", 2D) = "white" {}
        _EdgeColor("EdgeColor",Color)=(1.0,1.0,1.0,1.0)
        _EdgeWidth("EdgeWidth",Float)=1.0
        _BackgroundFade("BackgroundFade",Float)=1.0
        _BackgroundColor("BackgroundColor",Color)=(1.0,1.0,1.0,1.0)

    }

    SubShader
    {
        Tags
        {
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

	        half4 _EdgeColor;
	        half4 _BackgroundColor;
	        float _EdgeWidth;
	        float _BackgroundFade;

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

	        float intensity(in float4 color)
	        {
		        return sqrt((color.x * color.x) + (color.y * color.y) + (color.z * color.z));
	        }
	
	        float sobel(float stepx, float stepy, float2 center)
	        {
                //神秘边缘检测
		        float topLeft = intensity(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, center + float2(-stepx, stepy)));
		        float bottomLeft = intensity(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, center + float2(-stepx, -stepy)));
		        float topRight = intensity(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, center + float2(stepx, stepy)));
		        float bottomRight = intensity(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, center + float2(stepx, -stepy)));

		        float Gx = -1.0 * topLeft + 1.0 * bottomRight;
		        float Gy = -1.0 * topRight + 1.0 * bottomLeft;
		        float sobelGradient = sqrt((Gx * Gx) + (Gy * Gy));
		        return sobelGradient;
	        }
	
	
	
	        half4 frag(Varyings i): SV_Target
	        {
		
		        half4 sceneColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
		
		        float sobelGradient = sobel(_EdgeWidth / _ScreenParams.x, _EdgeWidth / _ScreenParams.y, i.uv);//神秘描边长度
		        half4 backgroundColor = lerp(sceneColor, _BackgroundColor, _BackgroundFade);//神秘背景混合颜色
		        float3 edgeColor = lerp(backgroundColor.rgb, _EdgeColor.rgb, sobelGradient);//神秘描边颜色
		
		        return float4(edgeColor, 1);
	        }
            ENDHLSL
        }
    }
}
