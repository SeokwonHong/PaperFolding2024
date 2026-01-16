Shader "Custom/ShadowCasterShader" // 이름
{
    SubShader//어떤 컴퓨터 환경에서도 최적화
    {
        Tags { "RenderType"="Opaque" } //이 코드(쉐이더) 가 적용된 게임오브젝트는 불투명하다.
        Pass // 쉐이더가 렌더링을 어떻게 처리할지에 대하여
        {
            ZWrite On //카메라로 부터 떨어진 오브젝트와의 거리를 계산해, (뒤에있어) 안보이는 게임 오브젝트는 렌더링을 안하겠다 - 최적화
            CGPROGRAM // 본 코드는 C 언어의 Graphics 를 지원하는 Cg 언어로 제작되었다.
            #pragma vertex vert // 여기서 vertex는 3D 엔진에서 쓰는 점들이고, 이를 vert 가 관리하겠다.
            #pragma fragment frag //여기서 fragment 는 화면에 표시될 픽셀을 나타냄.

            struct appdata { //     대충 GPU 가 vertex 를 처리한단 뜻
                float4 vertex : POSITION;
            };

            struct v2f {
                float4 pos : SV_POSITION; //대충 fragment(화면출력 부서) 로 넘긴단 뜻
            };

            v2f vert (appdata v) { //appdata 는 vertex 데이터(위치, 색상, 텍스터 좌표)
                v2f o; //여기서 o 는 fragment 로 처리된 위치 정보를 넘기는 위치정보 ??
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                return fixed4(0, 0, 0, 1); // 색상 RABA 임으로 A 값이 1, 즉 불투명함.
            }
            ENDCG //쉐이더 코드가 끝남.
        }
    }
}
