Shader "OstatnieEcho/CinematicGrade"
{
    Properties
    {
        [Header(Exposure and Tone)]
        _Exposure ("Exposure", Range(0.1, 2.0)) = 0.58
        _Contrast ("Contrast", Range(0.5, 2.5)) = 1.25
        _Gamma ("Gamma", Range(0.3, 2.0)) = 0.95
        _BlackFloor ("Black Floor (Lift)", Range(0, 0.1)) = 0.012
        
        [Header(Color Balance)]
        _Saturation ("Saturation", Range(0, 2)) = 0.78
        _HueShift ("Global Hue Shift", Range(-0.1, 0.1)) = 0.0
        
        [Header(Three Way Color Grade)]
        _ShadowColor ("Shadow Color", Color) = (0.22, 0.18, 0.35, 1)
        _ShadowStr ("Shadow Grade Strength", Range(0, 1)) = 0.35
        _MidColor ("Midtone Color", Color) = (0.72, 0.48, 0.34, 1)
        _MidStr ("Midtone Grade Strength", Range(0, 0.6)) = 0.2
        _HighColor ("Highlight Color", Color) = (1.0, 0.78, 0.45, 1)
        _HighStr ("Highlight Grade Strength", Range(0, 1)) = 0.25
        
        [Header(Channel Curves)]
        _RedGain ("Red Gain", Range(0.5, 1.5)) = 1.05
        _GreenGain ("Green Gain", Range(0.5, 1.5)) = 0.92
        _BlueGain ("Blue Gain", Range(0.5, 1.5)) = 0.88
        _RedLift ("Red Lift (shadows)", Range(-0.1, 0.1)) = 0.02
        _GreenLift ("Green Lift (shadows)", Range(-0.1, 0.1)) = -0.01
        _BlueLift ("Blue Lift (shadows)", Range(-0.1, 0.1)) = 0.03
        
        [Header(Cool Color Handling)]
        _CoolSatPreserve ("Cool Sat Preserve", Range(0.5, 2.0)) = 1.25
        _CoolValueBoost ("Cool Value Boost", Range(0.8, 1.5)) = 1.0
        
        [Header(Vignette)]
        _VigStr ("Vignette Strength", Range(0, 1.5)) = 0.55
        _VigColor ("Vignette Color", Color) = (0.025, 0.012, 0.045, 1)
        _VigSoft ("Vignette Softness", Range(0.2, 1.5)) = 0.55
        
        [Header(CRT Effects)]
        _ScanStr ("Scanline Strength", Range(0, 0.2)) = 0.03
        _ScanDensity ("Scanline Density", Range(200, 1500)) = 550
        _ChromAb ("Chromatic Aberration", Range(0, 4)) = 0.65
        _Grain ("Film Grain", Range(0, 0.06)) = 0.008

        [Header(Comic Toon Effect)]
        _ToonStrength ("Toon Effect Strength", Range(0.0, 1.0)) = 1.0
        _ToonSteps ("Toon Shading Steps", Range(2, 20)) = 6
        _ToonSaturation ("Toon Saturation Boost", Range(0.0, 2.0)) = 1.0

        [Header(Comic Outline Effect)]
        _OutlineStrength ("Overall Outline Strength", Range(0.0, 1.0)) = 1.0
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineWidth ("Outline Width", Range(0.0, 10.0)) = 1.5
        
        [Header(Outline Edge Detection)]
        _DepthSensitivity ("Depth Edge Sensitivity", Range(0.1, 50.0)) = 10.0
        _DepthThreshold ("Depth Edge Threshold", Range(0.001, 1.0)) = 0.05
        _NormalSensitivity ("Normal Edge Sensitivity", Range(0.1, 10.0)) = 2.0
        _NormalThreshold ("Normal Edge Threshold", Range(0.01, 2.0)) = 0.5
        
        [Header(Outline Distance Fade)]
        _OutlineDistanceFade ("Distance Fade Strength", Range(0.0, 1.0)) = 1.0
        _DistanceFadeStart ("Fade Start Distance", Range(0.0, 100.0)) = 20.0
        _DistanceFadeEnd ("Fade End Distance", Range(10.0, 500.0)) = 100.0

        [Header(Effect Distance Masking)]
        _MaxEffectDistance ("Max Effect Distance", Range(10.0, 5000.0)) = 1000.0
        _EffectDistanceFade ("Effect Fade Length", Range(0.1, 500.0)) = 100.0
    }
    
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        
        Pass
        {
            Name "CinematicGradePass"
            ZTest Always ZWrite Off Cull Off
            
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
            
            CBUFFER_START(UnityPerMaterial)
                float _Exposure;
                float _Contrast;
                float _Gamma;
                float _BlackFloor;
                float _Saturation;
                float _HueShift;
                float4 _ShadowColor;
                float _ShadowStr;
                float4 _MidColor;
                float _MidStr;
                float4 _HighColor;
                float _HighStr;
                float _RedGain;
                float _GreenGain;
                float _BlueGain;
                float _RedLift;
                float _GreenLift;
                float _BlueLift;
                float _CoolSatPreserve;
                float _CoolValueBoost;
                float _VigStr;
                float4 _VigColor;
                float _VigSoft;
                float _ScanStr;
                float _ScanDensity;
                float _ChromAb;
                float _Grain;
                float _ToonStrength;
                float _ToonSteps;
                float _ToonSaturation;
                float _OutlineStrength;
                float4 _OutlineColor;
                float _OutlineWidth;
                float _DepthSensitivity;
                float _DepthThreshold;
                float _NormalSensitivity;
                float _NormalThreshold;
                float _OutlineDistanceFade;
                float _DistanceFadeStart;
                float _DistanceFadeEnd;
                float _MaxEffectDistance;
                float _EffectDistanceFade;
            CBUFFER_END
            
            float Luma(float3 c) { return dot(c, float3(0.2126, 0.7152, 0.0722)); }
            
            float3 RGBtoHSV(float3 c)
            {
                float4 K = float4(0.0, -1.0/3.0, 2.0/3.0, -1.0);
                float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
                float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
                float d = q.x - min(q.w, q.y);
                float e = 1.0e-10;
                return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
            }
            
            float3 HSVtoRGB(float3 c)
            {
                float4 K = float4(1.0, 2.0/3.0, 1.0/3.0, 3.0);
                float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
            }
            
            float Hash12(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }
            
            // ASC-CDL style color correction
            float3 ASC_CDL(float3 col, float3 slope, float3 offset, float3 power)
            {
                col = col * slope + offset;
                col = pow(max(col, 0.0001), power);
                return col;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                float2 texel = float2(1.0 / _ScreenParams.x, 1.0 / _ScreenParams.y);
                
                // ---- Chromatic Aberration ----
                float2 fromCenter = (uv - 0.5) * 2.0;
                float edgeDist = length(fromCenter) * 0.5;
                float2 caDir = fromCenter * texel * _ChromAb;
                
                float3 rawCol = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv).rgb;
                float depthCenter = LinearEyeDepth(SampleSceneDepth(uv), _ZBufferParams);
                
                float cr = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv + caDir * edgeDist).r;
                float cg = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv).g;
                float cb = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv - caDir * edgeDist).b;
                float ca = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv).a;
                float3 col = float3(cr, cg, cb);
                
                // ---- 1. Exposure ----
                col *= _Exposure;
                
                // ---- 2. Channel-specific lift/gain (ASC-CDL inspired) ----
                float3 slope = float3(_RedGain, _GreenGain, _BlueGain);
                float3 offset = float3(_RedLift, _GreenLift, _BlueLift);
                col = col * slope + offset;
                col = max(col, 0.0);
                
                // ---- 3. Gamma ----
                col = pow(max(col, 0.0001), float3(1.0/_Gamma, 1.0/_Gamma, 1.0/_Gamma));
                
                // ---- 4. Filmic S-curve contrast ----
                // Using smoothstep for a natural filmic feel
                float3 x = saturate(col);
                float3 curved = x * x * (3.0 - 2.0 * x); // smoothstep(0,1,x)
                col = lerp(x, curved, (_Contrast - 1.0) * 0.9);
                
                // ---- 5. Black floor ----
                col = col * (1.0 - _BlackFloor) + _BlackFloor;
                col = max(col, 0.0);
                
                // ---- 6. Saturation ----
                float luma = Luma(col);
                col = lerp(float3(luma, luma, luma), col, _Saturation);
                
                // ---- 7. Selective cool color handling ----
                float3 hsv = RGBtoHSV(max(col, 0.001));
                
                // Hue shift
                hsv.x = frac(hsv.x + _HueShift);
                
                // Cool detection: blues, teals, purples (hue 0.5-0.8)
                float isCool = smoothstep(0.4, 0.52, hsv.x) * (1.0 - smoothstep(0.78, 0.88, hsv.x));
                isCool = saturate(isCool);
                
                // Preserve/boost cool saturation
                hsv.y = lerp(hsv.y, min(hsv.y * _CoolSatPreserve, 1.0), isCool);
                // Optionally boost cool value
                hsv.z = lerp(hsv.z, min(hsv.z * _CoolValueBoost, 1.0), isCool);
                
                col = HSVtoRGB(hsv);
                
                // ---- 8. Three-way color grading ----
                luma = Luma(col);
                
                // Shadow zone: dark areas
                float shadowW = 1.0 - smoothstep(0.0, 0.3, luma);
                // Midtone zone
                float midW = smoothstep(0.1, 0.35, luma) * (1.0 - smoothstep(0.5, 0.75, luma));
                // Highlight zone
                float highW = smoothstep(0.5, 0.85, luma);
                
                // Apply as multiplicative tints (preserves structure)
                float3 graded = col;
                graded = lerp(graded, graded * _ShadowColor.rgb * 3.0, shadowW * _ShadowStr);
                graded = lerp(graded, graded * _MidColor.rgb * 2.2, midW * _MidStr);
                graded = lerp(graded, graded * _HighColor.rgb * 1.6, highW * _HighStr);
                col = graded;

                // ---- Comic Posterization (Toon Shading) ----
                hsv = RGBtoHSV(max(col, 0.001));
                float steppedValue = floor(hsv.z * _ToonSteps + 0.5) / _ToonSteps;
                hsv.z = lerp(hsv.z, steppedValue, _ToonStrength);
                hsv.y = lerp(hsv.y, saturate(hsv.y * _ToonSaturation), _ToonStrength);
                col = HSVtoRGB(hsv);
                
                // ---- 9. Scanlines ----
                float scan = sin(uv.y * _ScanDensity * 3.14159) * 0.5 + 0.5;
                col *= lerp(1.0, scan, _ScanStr);
                
                // ---- 10. Film grain ----
                float grain = Hash12(uv * _ScreenParams.xy + frac(_Time.y * 43.0)) * 2.0 - 1.0;
                col += grain * _Grain;
                
                // ---- Comic Edge Detection (Outline) ----
                float halfWidth = _OutlineWidth;
                float2 uv0 = uv + float2(-texel.x, -texel.y) * halfWidth;
                float2 uv1 = uv + float2( texel.x,  texel.y) * halfWidth;
                float2 uv2 = uv + float2( texel.x, -texel.y) * halfWidth;
                float2 uv3 = uv + float2(-texel.x,  texel.y) * halfWidth;
                
                float d0 = LinearEyeDepth(SampleSceneDepth(uv0), _ZBufferParams);
                float d1 = LinearEyeDepth(SampleSceneDepth(uv1), _ZBufferParams);
                float d2 = LinearEyeDepth(SampleSceneDepth(uv2), _ZBufferParams);
                float d3 = LinearEyeDepth(SampleSceneDepth(uv3), _ZBufferParams);
                
                // Depth Edge
                float depthDiff = abs(d0 - d1) + abs(d2 - d3);
                float depthEdge = saturate((depthDiff - _DepthThreshold * d0) * _DepthSensitivity);
                
                // Normal Edge
                float3 n0 = SampleSceneNormals(uv0);
                float3 n1 = SampleSceneNormals(uv1);
                float3 n2 = SampleSceneNormals(uv2);
                float3 n3 = SampleSceneNormals(uv3);
                
                float normalDiff = distance(n0, n1) + distance(n2, n3);
                float normalEdge = saturate((normalDiff - _NormalThreshold) * _NormalSensitivity);
                
                float edge = saturate(depthEdge + normalEdge) * _OutlineStrength;
                
                // Distance Fade
                float fadeFactor = saturate((d0 - _DistanceFadeStart) / max(0.1, (_DistanceFadeEnd - _DistanceFadeStart)));
                edge *= lerp(1.0, 1.0 - fadeFactor, _OutlineDistanceFade);
                
                // Apply Outline
                col = lerp(col, _OutlineColor.rgb, edge * _OutlineColor.a);
                
                // ---- 11. Vignette ----
                float2 vUV = uv - 0.5;
                float vD = length(vUV) * 2.0;
                float vig = 1.0 - smoothstep(_VigSoft, _VigSoft + 0.55, vD);
                vig = lerp(1.0, vig, _VigStr);
                col = lerp(_VigColor.rgb, col, vig);
                
                // ---- Global Effect Distance Masking ----
                float effectMask = 1.0 - saturate((depthCenter - (_MaxEffectDistance - _EffectDistanceFade)) / max(0.1, _EffectDistanceFade));
                col = lerp(rawCol, col, effectMask);
                
                return half4(saturate(col), ca);
            }
            ENDHLSL
        }
    }
}