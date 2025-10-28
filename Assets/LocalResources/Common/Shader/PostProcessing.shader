Shader "PostProcessing"
{
    Properties
    {
        _MainTex ("MainTexture", 2D) = "white" {}
        //_EdgeColor("EdgeColor",Color)=(1.0,1.0,1.0,1.0)
        //_EdgeWidth("EdgeWidth",Float)=1.0
        //_BackgroundFade("BackgroundFade",Float)=1.0
        //_BackgroundColor("BackgroundColor",Color)=(1.0,1.0,1.0,1.0)

    }
           
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "RenderPipeline"="UniversalRenderPipeline"
        }
        Cull Off
        ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {

            Name "EdgeDetection"

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


        Pass
        {
            Name"Nosie"
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
            float _Speed;
            float _Strength;

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
                output.uv = TRANSFORM_TEX(i.texcoord,_MainTex);
                return output;
            }

            half4 frag(Varyings i) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                float time = _Time.y * _Speed;
                float noise = max(frac(sin(dot(i.uv + time, float2(12.9898, 78.233))) * 43758.54531),_Strength);
                return col*noise;
            }
            ENDHLSL
        }

        Pass
        {
            Name"LineBlock"
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

	        float _Frequency ;
	        float _TimeX ;
	        float _Offset ;
	        float _LinesWidth ;
	        float _Alpha ;
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
                float2 uv : TEXCOORD1;
            };

            Varyings vert(Attributes i) 
            {
                Varyings output;
                output.worldPos = TransformObjectToWorld(i.vertex.xyz);
                output.pos = TransformWorldToHClip(output.worldPos);
                output.uv = TRANSFORM_TEX(i.texcoord,_MainTex);
                return output;
            }
	
	        float randomNoise(float2 c)
	        {
		        return frac(sin(dot(c.xy, float2(12.9898, 78.233))) * 43758.5453);
	        }
	
	        float trunc(float x, float num_levels)
	        {
		        return floor(x * num_levels) / num_levels;
	        }
	
	
	        float4 frag(Varyings i): SV_Target
	        {
		        float2 uv = i.uv;
		
		        //随机强度梯度线条生成
		        float truncTime = trunc(randomNoise(_TimeX), 2.0)*_Time.x;
		        float uv_trunc = randomNoise(trunc(uv.yy, float2(8, 8)) + 100.0 * truncTime);
                float uv_randomTrunc = 6.0 * trunc(_TimeX, 24.0 * uv_trunc);
                float blockLine_random = 0.5 * randomNoise(trunc(uv.yy + uv_randomTrunc, float2(8 * _LinesWidth, 8 * _LinesWidth)));
                blockLine_random += 0.5 * randomNoise(trunc(uv.yy + uv_randomTrunc, float2(7, 7)));
                blockLine_random = blockLine_random * 2.0 - 1.0;   
                blockLine_random = sign(blockLine_random) * saturate((abs(blockLine_random) - _Amount) / (0.4));

		        half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                
		        return color*(1-blockLine_random);//dot(color, half3(0.3, 0.59, 0.11))
	        }
            ENDHLSL
        }

        Pass
        {
            Name"BlockGlitch"
            
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
                output.uv = TRANSFORM_TEX(i.texcoord,_MainTex);
                return output;
            }

            //抖动
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
                half2 block = randomNoise(floor(i.uv * _BlockSize));//切分生成随机区块

                float displaceNoise = pow(block.x, 8.0) * pow(block.x, 3.0);//二次筛选
                float splitRGBNoise = pow(randomNoise(7.2341), 17.0);//分离RGB

                float offsetX = displaceNoise - splitRGBNoise * _MaxRGBSplitX;
                float offsetY = displaceNoise - splitRGBNoise * _MaxRGBSplitY;

                float noiseX = 0.05 * randomNoise(13.0);
                float noiseY = 0.05 * randomNoise(7.0);
                float2 offset = float2(offsetX * noiseX, offsetY* noiseY);

                half4 colorR = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + offset);
                half4 colorG = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv );
                half4 colorB = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv - offset);

                return half4(colorR.r , colorG.g, colorB.b, (colorR.a + colorG.a + colorB.a)*0.28);
            }
            ENDHLSL
        }

        Pass
        {
            Name "Vignette"
            
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
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            half3 _Color;
            float _Intensity;
            float2 _Center;
            float _CenterRadius;
            float _SmoothnessRadius;
            float _BrightnessThreshold;
            float _SmoothnessThreshold;
            uint _Rounded;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                half brightness = 0.2125 * mainTex.x + 0.7154 * mainTex.y + 0.0721 * mainTex.b;

                float roundValue = _ScreenParams.y / _ScreenParams.x;
                float2 roundedUV = float2(uv.x * roundValue, uv.y) * _Rounded + uv * (1.0 - _Rounded);
                half uvRadius = length(_Center - roundedUV);
                //half vignetteValue = smoothstep(_CenterRadius, _CenterRadius + _SmoothnessRadius, uvRadius) * step(brightness, _BrightnessThreshold) * _Intensity;
                half vignetteValue = smoothstep(_CenterRadius, _CenterRadius + _SmoothnessRadius, uvRadius)
                                        * (1.0 - smoothstep(_BrightnessThreshold - _SmoothnessThreshold, _BrightnessThreshold, brightness)) * _Intensity;

                float2 mappingCoordinate = uv - _Center;
                float mappingDistance = saturate(length(mappingCoordinate));
                float theta = atan2(mappingCoordinate.y, mappingCoordinate.x);
                float thetaValue01 = theta - _Time.y * 0.1;
                thetaValue01 = thetaValue01 + step(thetaValue01, -PI) * 2 * PI;
                float thetaValue02 = theta + _Time.y * 0.25;
                thetaValue02 = thetaValue02 - step(PI, thetaValue02) * 2 * PI;
                float thetaValue03 = theta + _Time.y * 0.4;
                thetaValue03 = thetaValue03 - step(PI, thetaValue03) * 2 * PI;
                float num01 = 2.0f, num02 = 4.0f, num03 = 6.0f;
                float mappingValue01 = abs(0.5 - frac(abs(thetaValue01) * (1.0 / PI) * num01)) * 2.0;
                float mappingValue02 = abs(0.5 - frac(abs(thetaValue02) * (1.0 / PI) * num02)) * 2.0;
                float mappingValue03 = abs(0.5 - frac(abs(thetaValue03) * (1.0 / PI) * num03)) * 2.0;
                vignetteValue *= (mappingValue01 + mappingValue02 + mappingValue03) * (1.0 - mappingDistance);

                half3 finalColor = vignetteValue * _Color + (1.0 - vignetteValue) * mainTex.rgb;
                
                return half4(finalColor, mainTex.a);
            }
            ENDHLSL
        }

        Pass
        {
            Name "Pixelate"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
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
                float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                return col;
            }
            ENDHLSL
        }
        //续写你的后处理shader(统一定义_MainTex
    }
}
