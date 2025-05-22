Shader "Custom/HealthBarShader"
{
    Properties
    {
        _Color("Color",Color) = (1,0,0,1)
        _Health ("Health", Range(0,1)) = 1
        _LastHealth ("Last Health", Range(0,1)) = 1
        _TrailAlpha ("Trail Alpha", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

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

            float _Health;
            float _LastHealth;
            fixed4 _Color;
            float _TrailAlpha;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {

                float uvx = i.uv.x;

                float minH = min(_Health, _LastHealth);
                float maxH = max(_Health, _LastHealth);

                // 主血条区域 [0, _Health]
                float inside_health = step(uvx, _Health);

                // 拖尾区域 [_Health, _TrailEnd]
                float inside_trail = step(minH, uvx) * step(uvx, maxH);

                fixed4 barColor = _Color; //lerp(_LeftColor, _RightColor, uvx / max(_Health, 0.0001));

                fixed4 trailColor = fixed4(1,1,1,_TrailAlpha); // 白色拖尾，透明度可控

                // 综合颜色：主血条颜色或拖尾颜色，其他部分透明
                fixed4 col = inside_health * barColor + inside_trail * trailColor;

                // 保证透明度正确，超过1时裁剪
                col.a = saturate(col.a);

                return col;
            }
            ENDCG
        }
    }
}