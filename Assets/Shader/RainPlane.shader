/*
 * 雨天地面Shader - RainPlane
 * 功能：实现雨滴涟漪、水坑流动、反射效果的统一地面渲染
 * 作者：[周惠新]
 * 用途：RTS/RPG项目的雨天天气系统
 */

Shader "Unlit/RainPlane"
{
    Properties
    {
        // ===== 基础纹理设置 =====
        [Header(Base)]
        _MainTex ("Texture", 2D) = "White"{}  // 地面基础纹理
        _Color ("Color",Color) = (1,1,1,1)    // 地面颜色（预留，当前未使用）
        
        // ===== 涟漪效果参数 =====
        // 涟漪通过雨滴碰撞触发，从中心向外扩散的同心圆波纹
        [Space(15)]
        [Header(Ripple Settings)]
        // 注意：_RippleDataArray不在Properties中声明，在CGPROGRAM中声明
        // 这是因为Shader Properties不支持数组类型
        _RippleCount ("Active Ripple Count", Int) = 0          // 当前活跃的涟漪数量（C#端动态设置）
        _RippleSpeed ("Ripple Speed", Float) = 5.0            // 涟漪扩散速度（单位/秒）
        _RippleStrength ("Ripple Strength", Float) = 0.3      // 涟漪强度（影响振幅）
        _RippleWidth ("Ripple Width", Float) = 0.5            // 涟漪波长（环与环之间的距离）
        _RippleLifetime ("Ripple Lifetime", Float) = 2.0      // 涟漪生命周期（秒）
        
        // ===== 水坑效果参数 =====
        // 水坑用于标识积水区域，影响反射和湿度
        [Space(15)]
        [Header(Puddle Settings)]
        _PuddleNoise ("Puddle Noise", 2D) = "gray" {}         // 噪声贴图（用于流动扰动）
        _PuddleMask ("Puddle Mask", 2D) = "white" {}          // 水坑遮罩（定义哪些区域有水）
        _PuddleSize ("积水块大小", Range(1, 50)) = 20
        _PuddleLiftTime("Puddle Lifetime",Float) = 7          //生命周期
        _PuddleThreshold("Puddle Threshold", Range(0,1)) = 0.5 //积水面积
        
        // ===== 流动效果参数 =====
        // 通过噪声图UV滚动模拟水面流动感
        [Space(15)]
        [Header(Flow Settings)]
        _FlowSpeed ("Flow Speed", Vector) = (0.1, 0.05, 0, 0) // 流动速度（xy=主方向，zw未使用）
        _FlowScale1 ("Flow Scale 1", Float) = 1.0             // 第一层噪声缩放（大尺度流动）
        _FlowScale2 ("Flow Scale 2", Float) = 3.0             // 第二层噪声缩放（细节流动）
        _FlowStrength ("Flow Strength", Float) = 0.05         // 流动扰动强度（UV偏移量）
        
        // ===== 反射效果参数 =====
        // 使用CubeMap实现天空和环境反射
        [Space(15)]
        [Header(Reflection Settings)]
        _ReflectionCube ("Reflection Cubemap", Cube) = "" {}      // 反射贴图（天空盒或环境）
        _ReflectionStrength ("Reflection Strength", Range(0,1)) = 0.8  // 反射强度（湿润区域的反射程度）
    }
    SubShader
    {
        // 渲染类型：不透明（地面通常不需要透明）
        Tags { "RenderType"="Opaque" }
        LOD 100  // 细节层次：100为低复杂度

        Pass
        {
            CGPROGRAM
            // 指定顶点着色器和片段着色器函数
            #pragma vertex vert
            #pragma fragment frag

            // 引入Unity内置函数库（包含矩阵变换、时间等）
            #include "UnityCG.cginc"
            
            // ===== 变量声明区域 =====
            // 注意：这里声明的变量必须与Properties中的名称完全一致（除了数组）
            
            // 基础纹理
            sampler2D _MainTex;      // 地面纹理采样器
            float4 _MainTex_ST;      // 纹理的Tiling(xy)和Offset(zw)，Unity自动提供
            
            // ===== 涟漪相关变量 =====
            // 涟漪数据数组：每个元素是Vector4(x, y, z, time)
            // xyz = 涟漪中心的世界坐标
            // w = 涟漪生成的时间戳（Time.time）
            float4 _RippleDataArray[128];  // 最多支持128个同时存在的涟漪
            float _RippleStrengthArray[128]; //每个涟漪的强度
            int _RippleCount;              // 当前活跃涟漪数量（由C#脚本每帧更新）
            float _RippleSpeed;            // 涟漪扩散速度
            float _RippleStrength;         // 涟漪振幅强度
            float _RippleWidth;            // 涟漪波长
            float _RippleLifetime;         // 涟漪存活时间
            
            // ===== 水坑相关变量 =====
            sampler2D _PuddleNoise;   // 噪声贴图（用于流动扰动）
            sampler2D _PuddleMask;    // 水坑遮罩贴图
            float _PuddleSize;
            fixed _PuddleLiftTime;
            fixed _PuddleThreshold;
            
            // ===== 流动相关变量 =====
            fixed4 _FlowSpeed;   // 流动方向和速度
            fixed _FlowScale1;   // 第一层噪声缩放
            fixed _FlowScale2;   // 第二层噪声缩放
            fixed _FlowStrength; // 流动扰动强度
            
            // ===== 反射相关变量 =====
            samplerCUBE _ReflectionCube;  // CubeMap反射贴图采样器
            fixed _ReflectionStrength;    // 反射强度

            // ===== 顶点着色器输入结构体 =====
            // 从Mesh获取的顶点数据
            struct appdata
            {
                float4 vertex : POSITION;      // 顶点位置（模型空间）
                float3 normal : NORMAL;        // 顶点法线（模型空间）
                float2 uv : TEXCOORD0;         // UV坐标（纹理坐标）
            };

            // ===== 顶点着色器到片段着色器的数据传递结构体 =====
            // v2f = vertex to fragment
            struct v2f
            {
                float2 uv : TEXCOORD0;              // UV坐标（传递给片段着色器用于纹理采样）
                float3 worldpos : TEXCOORD1;        // 世界空间位置（用于计算涟漪距离）
                float3 worldNormal : TEXCOORD3;     // 世界空间法线（用于反射计算）
                float4 vertex : SV_POSITION;        // 裁剪空间位置（用于GPU光栅化）
            };

            // ===== 顶点着色器 =====
            // 作用：将顶点从模型空间转换到裁剪空间，并计算传递给片段着色器的数据
            // 每个顶点执行一次
            v2f vert (appdata v)
            {
                v2f o;  // 输出结构体
                
                // 1. 转换顶点到裁剪空间（用于GPU渲染管线）
                // UnityObjectToClipPos = MVP矩阵变换（Model * View * Projection）
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                // 2. 计算世界空间位置（用于涟漪距离计算）
                // unity_ObjectToWorld是Unity提供的模型到世界的变换矩阵
                o.worldpos = mul(unity_ObjectToWorld, v.vertex).xyz;
                
                // 3. 转换法线到世界空间（用于反射和光照）
                // UnityObjectToWorldNormal考虑了非均匀缩放
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                
                // 4. 应用纹理的Tiling和Offset
                // TRANSFORM_TEX宏展开为：uv * _MainTex_ST.xy + _MainTex_ST.zw
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                return o;
            }

            // ===== 片段着色器 =====
            // 作用：计算每个像素的最终颜色
            // 每个像素执行一次（这是GPU渲染的核心部分）
            fixed4 frag (v2f i) : SV_Target
            {
                // ========== 第一阶段：涟漪计算 ==========
                
                // rippleValue：累积的涟漪高度/强度值
                // 正值表示波峰，负值表示波谷
                float rippleValue = 0;
                
                // rippleNormal：累积的法线偏移（xz平面）
                // 用于后续的反射扰动（当前仅计算，未应用）
                float2 rippleNormal = float2(0,0);

                // 遍历所有活跃的涟漪
                // 注意：这是一个动态循环，_RippleCount由C#脚本每帧更新
                for (int idx = 0; idx < _RippleCount; idx++)
                {
                    // 1. 提取当前涟漪的数据
                    float3 rippleCenter = _RippleDataArray[idx].xyz;  // 涟漪中心的世界坐标
                    // 2. 计算当前像素到涟漪中心的距离（只在xz平面，忽略高度）
                    // 为什么只用xz？因为涟漪是在地面水平扩散的
                    float2 offset = i.worldpos.xz - rippleCenter.xz;  // 偏移向量（2D）
                    float distance = length(offset);                  // 距离（标量）
                    

                    // 3. 计算涟漪的"年龄"（存在时间）
                    // _Time.y是Unity提供的当前时间（秒）
                    float brithTime = _RippleDataArray[idx].w;       // 涟漪生成时刻（秒）
                    float age = _Time.y - brithTime;

                    float energyFactor = _RippleStrengthArray[idx];
                    float rippleSpeed = _RippleSpeed * energyFactor;
                    float rippleStrength = _RippleStrength * energyFactor;
                    float rippleDuration = energyFactor * _RippleLifetime;
                    float rippleSpreadDistance = rippleSpeed * rippleDuration;
                    float ringDistance = age * rippleSpeed;
                    //float distanceToRing = distance - ringDistance;
                    float distanceToRing = abs(distance - ringDistance);

                    
                    
                    

                    // 如果波前已经超出最大距离，跳过整个涟漪
                    if (age > rippleDuration)
                        continue;

                    
                    float phase = saturate(distanceToRing / (_RippleWidth * 2));
                    float wave = sin(phase * UNITY_HALF_PI);

                    // if (distanceToRing < 0 || distanceToRing > _RippleWidth * 2)
                    // {
                    //     wave = 0;
                    // }else
                    // {
                    //     phase = saturate(distanceToRing / (_RippleWidth * 2));
                    //     wave = sin(phase * UNITY_HALF_PI);
                    // }

                    if (distanceToRing > _RippleWidth)
                        wave = 0;
                    else
                        wave = cos(distanceToRing / _RippleWidth * UNITY_HALF_PI);
                    

                    // 7. 计算衰减系数（让涟漪逐渐消失）
                    
                    // 7a. 时间衰减：随着年龄增长，强度降低
                    // 1.0 → 0.0 线性衰减
                    float timeAttenuation = 1.0 - (age / rippleDuration);
                    // 平方衰减：让衰减曲线更自然（慢→快→慢）
                    timeAttenuation = timeAttenuation * timeAttenuation;

                    // 7b. 距离衰减：离中心越远越弱
                    float maxDistance = rippleSpreadDistance;  // 涟漪的最大传播距离
                    float distanceAttenuation = 1.0 - saturate(distance / maxDistance);

                    // 7c. 综合衰减：两种衰减相乘
                    float attenuation = timeAttenuation * distanceAttenuation;

                    // 8. 计算最终波形值并累加
                    float currentWave = wave * attenuation * rippleStrength;
                    rippleValue += currentWave;  // 多个涟漪会叠加产生干涉效果

                    // 9. 计算法线偏移（用于后续反射扰动）
                    if (distance > 0.001)  // 避免在中心点除零
                    {
                        // 计算径向方向（从中心指向当前像素）
                        float2 direction = normalize(offset);
                        
                        // 使用余弦计算法线强度（cos是sin的导数）
                        // 法线偏移应该垂直于波峰方向
                        float normalStrength = wave * attenuation * rippleStrength;
                        
                        // 累加法线偏移
                        rippleNormal += direction * normalStrength;
                    }
                }


                float2 puddleUV = i.worldpos.xz / _PuddleSize;
                float puddleNoise = tex2D(_PuddleNoise,puddleUV).r;

                float timeFactor = sin(_Time.y / _PuddleLiftTime);
                float threshold = _PuddleThreshold + timeFactor * 0.2;

                float puddleMask = smoothstep(threshold + 0.05, threshold - 0.05, puddleNoise);

                //return float4(puddleMask, puddleMask, puddleMask, 1);

                float2 flowUV1 = i.worldpos.xz * _FlowScale1;
                flowUV1 += _FlowSpeed.xy * _Time.y;
                float2 flow1 = tex2D(_PuddleNoise,flowUV1).rg;
                flow1 = (flow1 - 0.5) * 2.0;

                float2 flowUV2 = i.worldpos.xz * _FlowScale2;
                flowUV2 += _FlowSpeed.xy * _Time.y * 1.5;
                float2 flow2 = tex2D(_PuddleNoise,flowUV2).rg;
                flow2 = (flow2 - 0.5) * 2.0;

                float2 combinedFlow = (flow1 * 0.6 + flow2 * 0.4) * _FlowStrength;
                float2 waterDistortion = combinedFlow * puddleMask;

                float puddleDepth = saturate((threshold - puddleNoise) / 0.15);
                puddleDepth = puddleDepth * puddleDepth;
                //puddleDepth *= puddleMask;
                
                

                // ========== 第二阶段：纹理采样和颜色混合 ==========
                
                // 1. 采样地面基础纹理
                // tex2D：2D纹理采样函数
                fixed4 baseColor = tex2D(_MainTex, i.uv);

                float wetnessFactor = lerp(1.0, 0.65 - puddleDepth * 0.15, puddleMask);
                fixed4 wetGroundColor = baseColor * wetnessFactor;

                fixed4 waterColor = fixed4(0.3, 0.45, 0.55, 1);
                fixed4 waterTint = lerp(wetGroundColor, waterColor, puddleMask * (0.4 + puddleDepth * 0.3));

                float3 viewDir = normalize(i.worldpos - _WorldSpaceCameraPos);
                float3 reflectDir = reflect(viewDir, float3(0,1,0));

                float fresnel = pow(1.0 - saturate(dot(viewDir, float3(0,-1,0))), 3);

                float specular = fresnel * puddleMask * 0.3;
                

                // 2. 将涟漪值转换为颜色（调试可视化）
                // fixed4(0.3, 0.5, 1.0, 1)：淡蓝色（模拟水的颜色）
                // saturate(rippleValue * 5)：放大5倍后限制在0-1范围
                //   - rippleValue * 5：放大涟漪效果，方便观察
                //   - saturate()：确保颜色不会过曝
                fixed4 rippleColor = fixed4(0.3, 0.5, 1.0, 1) * saturate(abs(rippleValue) * 5);
                rippleColor *= puddleMask * 0.2 * (0.5 + puddleDepth);

                float flowHighlight = saturate(dot(waterDistortion, float2(1,1)) * 3.0);
                flowHighlight *= puddleMask * 0.2;
                
                // 3. 混合基础颜色和涟漪颜色
                // 使用加法混合：让涟漪叠加在地面上
                // 注意：这是临时的可视化方案，后续应该改为更真实的混合方式
                // 例如：基于湿度的lerp，或者通过法线扰动影响反射
                fixed4 finalColor = waterTint + rippleColor + flowHighlight + specular;
                
                // ========== 第三阶段：返回最终颜色 ==========
                // SV_Target：系统值目标，表示输出到渲染目标（通常是屏幕）
                return finalColor;
            }
            ENDCG
        }
    }
}
