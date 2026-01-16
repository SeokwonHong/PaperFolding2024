Shader "Custom/SpriteLit_OcclusionWhite"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _EmissionColor ("Emission Color", Color) = (0,0,0,0)
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _AlphaCutoff ("Alpha Cutoff", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags { 
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

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

           struct Varyings
            {
                float4 positionCS : SV_POSITION; // 必须带这个语义才能做屏幕坐标判断！
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
                float3 worldPos   : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);           SAMPLER(sampler_MainTex);
            TEXTURE2D(_NormalMap);         SAMPLER(sampler_NormalMap);
            float4 _Color;
            float4 _EmissionColor;
            float4 _OutlineColor;
            float _AlphaCutoff;

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
                float2 screenUV = IN.positionCS.xy / IN.positionCS.w;
                screenUV = screenUV * 0.5 + 0.5;

                float sceneDepth = SampleSceneDepth(screenUV);
                float myDepth = IN.positionCS.z / IN.positionCS.w;

                if (myDepth + 0.0005 < sceneDepth)
                {
                    // 被遮挡，返回纯白调试色
                    return float4(1, 1, 1, 1);
                }

                float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * _Color * IN.color;
                if (texColor.a < _AlphaCutoff)
                    discard;

                float3 normalTS = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, IN.uv).xyz * 2.0 - 1.0;
                float3 tangentWS = float3(1,0,0);
                float3 bitangentWS = float3(0,1,0);
                float3 normalWS = normalize(mul(normalTS, float3x3(tangentWS, bitangentWS, float3(0,0,1))));

                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);
                float NdotL = max(dot(normalWS, lightDir), 0.0);
                float3 lighting = mainLight.color * NdotL;

                lighting += SampleSH(normalWS);

                int count = GetAdditionalLightsCount();
                for (int i = 0; i < count; i++)
                {
                    Light light = GetAdditionalLight(i, IN.worldPos);
                    float3 addDir = normalize(light.direction);
                    float addDot = max(dot(normalWS, addDir), 0.0);
                    lighting += light.color * addDot * light.distanceAttenuation;
                }

                texColor.rgb = texColor.rgb * lighting + _EmissionColor.rgb * _EmissionColor.a;
                return texColor;
            }

            ENDHLSL
        }
    }
}
