// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Ocean Toolkit/Ocean Shader"
{
    Properties
    {
        ot_NormalMap0 ("Normal Map 0", 2D) = "blue" {}
        ot_NormalMap1 ("Normal Map 1", 2D) = "blue" {}
        ot_FoamMap ("Foam Map", 2D) = "white" {}

        ot_AbsorptionCoeffs ("Absorption Coeffs", Vector) = (3.0, 20.0, 50.0, 1.0)
        ot_DetailFalloffStart ("Detail Falloff Start", float) = 60.0
        ot_DetailFalloffDistance ("Detail Falloff Distance", float) = 40.0
        ot_DetailFalloffNormalGoal ("Detail Falloff Normal Goal", float) = 0.2
        ot_AlphaFalloff ("Alpha Falloff", float) = 1.0
        ot_FoamFalloff ("Foam Falloff", float) = 1.5
        ot_FoamStrength ("Foam Strength", float) = 1.2
        ot_FoamAmbient ("Foam Ambient", float) = 0.3
        ot_ReflStrength ("Reflection Strength", float) = 0.7
        ot_RefrStrength ("Refraction Strength", float) = 1.0
        ot_RefrColor ("Refraction Color", Color) = (1.0, 0.0, 0.0, 1.0)
        ot_RefrNormalOffset ("Refraction Normal Offset", float) = 0.05
        ot_RefrNormalOffsetRamp ("Refraction Normal Offset Ramp", float) = 2.0
        ot_FresnelPow ("Fresnel Pow", float) = 4.0
        ot_SunColor ("Sun Color", Color) = (1.0, 0.95, 0.6)
        ot_SunPow ("Sun Power", float) = 100.0
        ot_DeepWaterColor ("Deep Water Color", Color) = (0.045, 0.15, 0.3, 1.0)
        ot_DeepWaterAmbientBoost ("Deep Water Ambient Boost", float) = 0.3
        ot_DeepWaterIntensityZenith ("Deep Water Intensity Zenith", float) = 1.0
        ot_DeepWaterIntensityHorizon ("Deep Water Intensity Horizon", float) = 0.4
        ot_DeepWaterIntensityDark ("Deep Water Intensity Dark", float) = 0.1
        ot_ClipExtents ("Clip Extents", Vector) = (50.0, 50.0, 0.0, 0.0)
		ot_SsrSampleCount ("SSR Sample Count", float) = 16.0
		ot_SsrStride("SSR Stride", float) = 8.0
		ot_SsrWorldDistance ("SSR World Distance", float) = 50.0
		ot_SsrZThickness("SSR Z Thickness", float) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent-100"
            "ForceNoShadowCasting"="True"
            "IgnoreProjector"="True"
        }

        GrabPass { "_Refraction" }

        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Back

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma shader_feature OT_REFL_OFF OT_REFL_SKY_ONLY OT_REFL_SSR
            #pragma shader_feature OT_REFR_OFF OT_REFR_COLOR OT_REFR_NORMAL_OFFSET
			#pragma shader_feature OT_CLIP_OFF OT_CLIP_ON
            #include "CommonOceanToolkit.cginc"

            // Currently set by script, not material
            uniform float3  ot_LightDir;
            uniform float   ot_DeepWaterScalar;

            uniform float4x4    ot_Proj;
            uniform float4x4    ot_InvView;
            uniform float4      ot_QkCorner0; // view.xyz1 / proj.w
            uniform float4      ot_QkCorner1;
            uniform float4      ot_QkCorner2;
            uniform float4      ot_QkCorner3;

            // Currently set by material
            uniform sampler2D   _Refraction;
            uniform float4      _Refraction_TexelSize;

            uniform sampler2D ot_NormalMap0;
            uniform sampler2D ot_NormalMap1;
            uniform sampler2D ot_FoamMap;

            uniform float4 ot_NormalMap0_ST;
            uniform float4 ot_NormalMap1_ST;
            uniform float4 ot_FoamMap_ST;

            uniform float4  ot_AbsorptionCoeffs;
            uniform float   ot_DetailFalloffStart;
            uniform float   ot_DetailFalloffDistance;
            uniform float   ot_DetailFalloffNormalGoal;
            uniform float   ot_AlphaFalloff;
            uniform float   ot_FoamFalloff;
            uniform float   ot_FoamStrength;
            uniform float   ot_FoamAmbient;

            uniform float   ot_ReflStrength;
            uniform float   ot_RefrStrength;
            uniform float4  ot_RefrColor;
            uniform float   ot_RefrNormalOffset;
            uniform float   ot_RefrNormalOffsetRamp;
            uniform float   ot_FresnelPow;
            uniform float3  ot_SunColor;
            uniform float   ot_SunPow;
            uniform float3  ot_DeepWaterColor;
            uniform float   ot_DeepWaterAmbientBoost;
            uniform float   ot_DeepWaterIntensityZenith;
            uniform float   ot_DeepWaterIntensityHorizon;
            uniform float   ot_DeepWaterIntensityDark;
            uniform float2  ot_ClipExtents;
			uniform float	ot_SsrSampleCount;
			uniform float	ot_SsrStride;
			uniform float	ot_SsrWorldDistance;
			uniform float	ot_SsrZThickness;

            struct VertOutput
            {
                float4 position : SV_POSITION;
                float4 screenPos : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                float4 worldNormal : TEXCOORD2;
				#if defined(OT_CLIP_ON)
				float4 localPos : TEXCOORD3;
				#endif
            };

            VertOutput vert(appdata_base input)
            {
                VertOutput output;

                float4 left = lerp(ot_QkCorner0, ot_QkCorner3, input.vertex.y);
                float4 right = lerp(ot_QkCorner1, ot_QkCorner2, input.vertex.y);
                float4 viewVertex = lerp(left, right, input.vertex.x);
                viewVertex *= 1.0 / viewVertex.w;

                float4 worldVertex = mul(ot_InvView, viewVertex);

				// Lookup height
				float height;
				float3 worldNormal;
				waveHeight(worldVertex.xyz, height, worldNormal);

				float detailFalloff = saturate((distance(worldVertex.xyz, _WorldSpaceCameraPos) - ot_DetailFalloffStart) / ot_DetailFalloffDistance);
				worldVertex.y = ot_OceanHeight + height * (1.0 - detailFalloff);

				float4 projVertex = mul(UNITY_MATRIX_VP, worldVertex);

                output.position = projVertex;
                output.screenPos = ComputeScreenPos(projVertex);
                output.worldPos = worldVertex;
                output.worldNormal = float4(worldNormal, 1.0);

				#if defined(OT_CLIP_ON)
				output.localPos = mul(unity_WorldToObject, worldVertex);
				#endif

                return output;
            }

            inline float3 calcReflDir(float3 viewDir, float3 normal)
            {
                float3 reflDir = reflect(viewDir, normal);
				reflDir.y = abs(reflDir.y);
                return reflDir;
            }

            float4 frag(VertOutput input) : SV_Target
            {
                #if defined(OT_CLIP_ON)
                clip(ot_ClipExtents.xy - abs(input.localPos.xz));
                #endif

                float screenZ = input.screenPos.w;
				float3 normal = input.worldNormal.xyz;
				// DEBUG: return float4(normal.x * 0.5 + 0.5, 0.0, normal.z * 0.5 + 0.5, 1.0);

                // Get fine normal from normal maps
                float2 normalUv0 = TRANSFORM_TEX(input.worldPos.xz, ot_NormalMap0);
                float3 fineNormal0 = UnpackNormal(tex2D(ot_NormalMap0, normalUv0)).xzy;
                float2 normalUv1 = TRANSFORM_TEX(input.worldPos.xz, ot_NormalMap1);
                float3 fineNormal1 = UnpackNormal(tex2D(ot_NormalMap1, normalUv1)).xzy;
				float3 fineNormal = fineNormal0 + fineNormal1;

				// Fade normal towards the horizon
				float detailFalloff = saturate((screenZ - ot_DetailFalloffStart) / ot_DetailFalloffDistance);
				fineNormal = normalize(lerp(fineNormal, float3(0.0, 2.0, 0.0), saturate(detailFalloff - ot_DetailFalloffNormalGoal)));
				normal = normalize(lerp(normal, float3(0.0, 1.0, 0.0), detailFalloff));

                // Transform fine normal to world space
                float3 tangent = cross(normal, float3(0.0, 0.0, 1.0));
                float3 bitangent = cross(tangent, normal);
                normal = tangent * fineNormal.x + normal * fineNormal.y + bitangent * fineNormal.z;
				// DEBUG: return float4(normal.xzy * 0.5 + 0.5, 1.0);

                float3 viewDir = normalize(input.worldPos - _WorldSpaceCameraPos.xyz);
                float3 reflDir = calcReflDir(viewDir, normal);
                float viewDotNormal = saturate(-dot(viewDir, normal));

                // ---
                // Sun
                // ---
				float3 sun = pow(saturate(dot(reflDir, ot_LightDir)), ot_SunPow) * ot_SunColor;

                // ----------
                // Reflection
                // ----------
                float3 refl = float3(0.0, 1.0, 1.0);

                #if defined(OT_REFL_SKY_ONLY)
				refl = sampleSky(reflDir);
                #endif

                #if defined(OT_REFL_SSR)
                float2 hitCoord;
                float hitZ;

                if (raytrace2d(input.worldPos.xyz, reflDir, ot_SsrWorldDistance, UNITY_MATRIX_V, UNITY_MATRIX_P, ot_SsrSampleCount, ot_SsrStride, ot_SsrZThickness, hitCoord, hitZ))
                {
                    #if defined(UNITY_UV_STARTS_AT_TOP)
                    if (_Refraction_TexelSize.y < 0.0 && _ProjectionParams.x >= 0.0)
                    {
                        hitCoord.y = 1.0 - hitCoord.y;
                    }
                    #endif

                    refl = tex2Dlod(_Refraction, float4(hitCoord, 0.0, 0.0)).xyz;
                    sun *= 0.0;
                }
                else
                {
					refl = sampleSky(reflDir);
                }
                #endif

                refl *= ot_ReflStrength;

                // ----------
                // Refraction
                // ----------
                float3 refr = float3(0.0, 1.0, 1.0);

                float2 uv = input.screenPos.xy / input.screenPos.w;
                float depthBelowSurface = sampleDepth(uv) * _ProjectionParams.z - screenZ;
                float refrDepthBelowSurface = depthBelowSurface;

                #if defined(OT_REFR_COLOR)
                refr = ot_RefrColor.xyz;
                #endif

                #if defined(OT_REFR_NORMAL_OFFSET)
                // Sample refraction first using offset proportional to the center reference depth. This makes the
                // surface transition "inside" objects smooth.
                float2 refrUv = uv + normal.xz * ot_RefrNormalOffset * saturate(depthBelowSurface / ot_RefrNormalOffsetRamp);
                refrDepthBelowSurface = sampleDepth(refrUv) * _ProjectionParams.z - screenZ;

                // Now, sample refraction using offset proportional to the refracted depth. This makes the
                // surface transition "outside" objects smooth.
                refrUv = uv + normal.xz * ot_RefrNormalOffset * saturate(refrDepthBelowSurface / ot_RefrNormalOffsetRamp);
                refrDepthBelowSurface = sampleDepth(refrUv) * _ProjectionParams.z - screenZ;

                // This procedure removes artifacts close to the surface, the downside is that we
                // need to sample the depth twice for refraction.

                // Is the refracted sample on geometry above the surface?
                if (refrDepthBelowSurface < 0.0)
                {
                    refrUv = uv;
                    refrDepthBelowSurface = depthBelowSurface;
                }

                #if defined(UNITY_UV_STARTS_AT_TOP)
                if (_Refraction_TexelSize.y < 0.0 && _ProjectionParams.x >= 0.0)
                {
                    refrUv.y = 1.0 - refrUv.y;
                }
                #endif

                refr = tex2D(_Refraction, refrUv).xyz;
                #endif

                // Absorb light relative to depth
                float3 deepWaterColor = ot_DeepWaterColor * ot_DeepWaterScalar * (1.0 + pow(viewDotNormal, 10.0) * ot_DeepWaterAmbientBoost);

                refr = lerp(refr, deepWaterColor, saturate(refrDepthBelowSurface.xxx / ot_AbsorptionCoeffs.xyz));
                refr *= ot_RefrStrength;

                // ----
                // Foam
                // ----

                // Depth-based
                // Wave-tops, pre-computed depth in the future?
                float foamShade = saturate(ot_FoamAmbient + dot(normal, ot_LightDir));
                float foamMask = 1.0 - pow(saturate(refrDepthBelowSurface / ot_FoamFalloff), 4.0);
                float2 foamUv = TRANSFORM_TEX(input.worldPos.xz, ot_FoamMap);
                float foam = foamMask * ot_FoamStrength * foamShade * tex2D(ot_FoamMap, foamUv).w;

                // ------------------
                // Combine everything
                // ------------------
                float fresnel = 0.0;

                #if defined(OT_REFL_OFF)
                fresnel = 0.0;
                #else
                #if defined(OT_REFR_OFF)
                fresnel = 1.0;
                #else
                fresnel = pow(1.0 - max(viewDotNormal, 0.0), ot_FresnelPow);
                #endif
                #endif

                float3 color = (1.0 - foam) * (fresnel * refl + (1.0 - fresnel) * refr + sun) + foam.xxx;
                float alpha = saturate(depthBelowSurface / ot_AlphaFalloff);
                return float4(color, alpha);
            }
            ENDCG
        }
    }

    CustomEditor "OceanToolkit.OceanShaderEditor"
}