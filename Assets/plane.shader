Shader "Unlit/plane"
{
    Properties
    {
        _ColorA ("ColorA",Color) = (0.9,0.9,0.9,1)
        _ColorB ("ColorB",Color) = (0.2,0.2,0.2,1)
        _LineColor ("LineColor",Color) = (0,0,0,1)
        _Tiles ("Tiles",Float) = 8
        _LinePct ("LinePct",Range(0,0.5)) = 0.06
        _UseWorld ("UseWorld",Float) = 1
    }
    SubShader
    {
        Tags {"RenderType"="Opaque" "Queue"="Geometry"}
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _ColorA,_ColorB,_LineColor;
            float _Tiles,_LinePct,_UseWorld;

            struct appdata
            {
                float4 vertex:POSITION;
                float2 uv:TEXCOORD0;
            };

            struct v2f
            {
                float4 pos:SV_POSITION;
                float2 uv:TEXCOORD0;
                float3 wpos:TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.wpos = mul(unity_ObjectToWorld,v.vertex).xyz;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i):SV_Target
            {
                float2 uv = (_UseWorld > 0.5) ? i.wpos.xz : i.uv;
                float2 uvT = uv * _Tiles;
                float2 cell = floor(uvT);
                float checker = fmod(cell.x + cell.y, 2.0);
                float3 baseCol = lerp(_ColorA.rgb, _ColorB.rgb, checker);

                float2 f = frac(uvT);
                float2 aa = fwidth(f);
                float w = _LinePct;
                float ex = 1.0 - smoothstep(w, w + aa.x, min(f.x, 1.0 - f.x));
                float ey = 1.0 - smoothstep(w, w + aa.y, min(f.y, 1.0 - f.y));
				float grid = saturate(max(ex, ey));

                return float4(lerp(baseCol, _LineColor.rgb,grid),1);
            }
            ENDCG
        }
    }
    FallBack Off
}
