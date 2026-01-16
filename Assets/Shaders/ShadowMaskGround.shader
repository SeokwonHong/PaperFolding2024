Shader "Custom/ShadowGround"
{
        SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 shadowCoords : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                // 对象空间 -> 裁剪空间
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);

                // 获取顶点位置并计算阴影贴图坐标
                VertexPositionInputs vertPos = GetVertexPositionInputs(IN.positionOS.xyz);
                float4 shadowCoords = GetShadowCoord(vertPos);
                OUT.shadowCoords = shadowCoords;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // 从主光源阴影图采样阴影值
                half shadow = MainLightRealtimeShadow(IN.shadowCoords);
                return half4(shadow, shadow, shadow, 1.0);
            }
            ENDHLSL
        }
    }
}