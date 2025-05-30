Shader "Spine/Dissolve"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _DissolveTex ("Dissolve Map", 2D) = "gray" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        _EdgeColor ("Edge Color", Color) = (1, 0.5, 0, 1)
        _EdgeWidth ("Edge Width", Range(0.0, 0.2)) = 0.05
        _DissolveAmount ("Dissolve Amount", Range(0,1)) = 0.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200

        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _DissolveTex;
            fixed4 _Color;
            fixed4 _EdgeColor;
            float _DissolveAmount;
            float _EdgeWidth;

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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float noise = tex2D(_DissolveTex, i.uv).r;
                float diff = noise - _DissolveAmount;

                // 溶解
                if (diff < 0)
                    discard;

                // 边缘光
                float edge = smoothstep(0.0, _EdgeWidth, diff);
                fixed4 mainCol = tex2D(_MainTex, i.uv) * _Color;
                fixed4 finalCol = lerp(_EdgeColor, mainCol, edge);
                finalCol.a *= mainCol.a;

                return finalCol;
            }
            ENDCG
        }
    }
}
