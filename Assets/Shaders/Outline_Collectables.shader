Shader "Custom/URP_HardEdgeLineWithSurface"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _LineColor("Edge Line Color", Color) = (0,1,1,1)
        _Threshold("Edge Angle Threshold", Range(0,1)) = 0.2
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        // Pass 1: 표면 렌더링
        Pass
        {
            Name "Surface"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings o;
                float3 posWS = TransformObjectToWorld(input.positionOS.xyz);
                o.positionHCS = TransformWorldToHClip(posWS);
                return o;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return _BaseColor;
            }
            ENDHLSL
        }

        // Pass 2: 하드 엣지 라인
        Pass
        {
            Name "HardEdgeLine"
            Tags { "LightMode"="UniversalForward" }

            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma target 4.0
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 _LineColor;
            float _Threshold;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2g
            {
                float4 pos : POSITION;
                float3 normal : NORMAL;
            };

            struct g2f
            {
                float4 pos : SV_POSITION;
            };

            v2g vert(appdata v)
            {
                v2g o;
                float3 posWS = TransformObjectToWorld(v.vertex.xyz);
                o.pos = TransformWorldToHClip(posWS);
                o.normal = TransformObjectToWorldNormal(v.normal);
                return o;
            }

            [maxvertexcount(6)]
            void geom(triangle v2g input[3], inout LineStream<g2f> stream)
            {
                float3 n0 = normalize(input[0].normal);
                float3 n1 = normalize(input[1].normal);
                float3 n2 = normalize(input[2].normal);

                float threshold = 1.0 - _Threshold;

                g2f a, b;

                if (dot(n0, n1) < threshold)
                {
                    a.pos = input[0].pos;
                    b.pos = input[1].pos;
                    stream.Append(a);
                    stream.Append(b);
                }

                if (dot(n1, n2) < threshold)
                {
                    a.pos = input[1].pos;
                    b.pos = input[2].pos;
                    stream.Append(a);
                    stream.Append(b);
                }

                if (dot(n2, n0) < threshold)
                {
                    a.pos = input[2].pos;
                    b.pos = input[0].pos;
                    stream.Append(a);
                    stream.Append(b);
                }
            }

            half4 frag(g2f i) : SV_Target
            {
                return _LineColor;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
