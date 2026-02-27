Shader "Unlit/AttackRangeIndicator"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color",Color) = (1,1,1,1)
        _AttackRangeType ("AttackRangeType", Int) = 0
        _SectorRadius ("SectorRadius", Float) = 0
        _SectorAngle ("SectorAngle", Float) = 0
        _CircleRadius ("CircleRadius", Float) = 0
        _CircleCenter ("CircleCenter", Vector) = (0,0,0)
        _RectangleWidth ("RectangleWidth", Float) = 0
        _RectangleHeight ("RectangleHeigh", Float) = 0
        _Duration ("Duration", Float) = 0
        _Position ("Position", Vector) = (0,0,0)
        _FillProgress ("FillProgress", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
                float3 worldPos: TEXCOORD1;
                float3 localPos : TEXCOORD2;  // 添加本地坐标
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            int _AttackRangeType;
            fixed _SectorRadius;
            fixed _SectorAngle;
            fixed _CircleRadius; 
            fixed3 _CircleCenter; 
            fixed _RectangleWidth; 
            fixed _RectangleHeight; 
            fixed _Duration;
            fixed3 _Position;
            fixed4 _Color;

            fixed _FillProgress;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                
                // 直接使用模型空间的顶点坐标（已经是本地坐标）
                o.localPos = v.vertex.xyz;
                
                return o;
            }

            float SectorMask(float2 localPos, float radius, float angle)
            {
                // 1. 计算距离
                float dist = length(localPos);
                
                // 2. 判断距离
                float distOK = step(dist, radius);
                
                // 3. 计算角度（弧度）
                float angleRad = atan2(localPos.x, localPos.y);
                
                // 4. 计算扇形半角（弧度）
                float halfAngle = radians(angle * 0.5);
                
                // 5. 判断角度
                float angleOK = step(abs(angleRad), halfAngle);
                
                // 6. 两者都满足
                return distOK * angleOK;
            }

            float CircleMask(float2 localPos, float2 center, float radius)
            {
                // 1. 计算到圆心的距离
                float dist = length(localPos - center);
                
                // 2. 判断是否在范围内
                return step(dist, radius);
            }

            float RectangleMask(float2 localPos, float width, float height)
            {
                // 1. 取绝对值
                float2 absPos = abs(localPos);
                
                // 2. 判断 X 和 Z 方向
                float xOK = step(absPos.x, width * 0.5);
                float zOK = step(absPos.y, height * 0.5);
                
                // 3. 两者都满足
                return xOK * zOK;
            }

            float SweepMask(float2 localPos, float progress, float maxRange)
            {
                float dist = length(localPos);

                float nomalizeDist = dist / maxRange;

                return step(nomalizeDist,progress);
            }
            

            fixed4 frag (v2f i) : SV_Target
            {
                float4 localPos4D = mul(unity_WorldToObject, float4(i.worldPos, 1.0));
                float3 localPos3D = localPos4D.xyz;
                float2 localPos = float2(localPos3D.x, localPos3D.y);

                float shapeMask = 0;
                float maxRange = 2;
               if (_AttackRangeType == 0)
               {
                   shapeMask = SectorMask(localPos,_SectorRadius,_SectorAngle);
                   maxRange = _SectorRadius;
               }
               else if (_AttackRangeType == 1)
               {
                   shapeMask = CircleMask(localPos,_CircleCenter,_CircleRadius);
                   maxRange = _CircleRadius;
               }
               else
               {
                   shapeMask = RectangleMask(localPos,_RectangleWidth,_RectangleHeight);
                   maxRange = max(_RectangleWidth,_RectangleHeight) * 0.5;
               }
                

                if (shapeMask < 0.001)
                {
                    discard;
                };

                float sweepMask = SweepMask(localPos, _FillProgress, maxRange);

                if (sweepMask < 0.001)
                {
                    discard;
                };
               
               
                
                fixed4 col = _Color;
                col.a *= sweepMask * shapeMask;
                return col;
            }
            ENDCG
        }
    }
}
