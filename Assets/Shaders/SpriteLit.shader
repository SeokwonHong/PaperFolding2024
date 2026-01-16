Shader "Custom/URP_SpriteLit_Advanced"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint Color", Color) = (1,1,1,1)
        _CustomLightingDir("Custom Light Direction", Vector) = (0,0,0,0)
        _AlphaCutoff("Alpha Cutoff", Range(0,1)) = 0.5
        _NormalMap("Normal Map",2D) = "bump"{}
        _EmissionColor("Emission Color", Color) = (0,0,0,0)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // 支持主光源阴影和软阴影
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            // 支持多光源
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
                float3 worldPos   : TEXCOORD1;
            };

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_NormalMap); SAMPLER(sampler_NormalMap);
            float4 _Color;
            float4 _CustomLightingDir;
            float4 _EmissionColor;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv = IN.uv;
                OUT.color = IN.color;
                OUT.worldPos = TransformObjectToWorld(IN.positionOS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * _Color * IN.color;
                if (texColor.a < 0.01) discard;

                // 转换到世界空间
                float3 normalWS = float3(0,0,-1);
                float3 tangentWS = float3(1,0,0);
                float3 bitangentWS = float3(0,1,0);

                // 构建TBN矩阵
                float3x3 TBN = float3x3(tangentWS, bitangentWS, normalWS);

                // 从法线贴图中采样（切线空间法线）
                float3 normalTS = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, IN.uv).xyz * 2.0 - 1.0;

                // 最终扰动法线（世界空间）
                normalWS = normalize(mul(normalTS, TBN));


                Light mainLight = GetMainLight();
                float3 lightDir = mainLight.direction;
                if (length(_CustomLightingDir.xyz) > 0.001)
                    lightDir = normalize(_CustomLightingDir.xyz);

                // **阴影接收：核心一行**
                float4 worldPos = float4(IN.worldPos,1.0);
                float shadowAtten = MainLightRealtimeShadow(worldPos);

                //漫反射
                float NdotL = max(dot(normalWS, lightDir), 0.0);
                float3 lighting = NdotL * mainLight.color * shadowAtten;

                // 环境光
                lighting += SampleSH(normalWS);

                // 多光源加成
                int lightCount = GetAdditionalLightsCount();
                for (int i = 0; i < lightCount; i++)
                {
                    Light light = GetAdditionalLight(i, IN.worldPos);
                    float3 addDir = light.direction;
                    float addNdotL = max(dot(normalWS, addDir), 0.0);
                    lighting += addNdotL * light.color * light.distanceAttenuation;
                }

                // 最终颜色 = 光照颜色 + 自发光颜色
                texColor.rgb += _EmissionColor.rgb * _EmissionColor.a;

                return float4(lighting * texColor.rgb, texColor.a);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ColorMask 0
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            float4 _Color;
            float _AlphaCutoff;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv = IN.uv;
                OUT.color = IN.color;
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float alpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv).a * _Color.a * IN.color.a;
                clip(alpha - _AlphaCutoff);
                return 0;
            }
            ENDHLSL
        }
    }
}
