// Warning, because of a bug the code below marks UnityInput.hlsl and Lighting.hlsl as imported but only adds the required functionality
// Reason = error with redefined variables

//===============================================================================================================================================================================
// Stripped down version of:  https://github.com/Unity-Technologies/ScriptableRenderPipeline/blob/master/com.unity.render-pipelines.lightweight/ShaderLibrary/UnityInput.hlsl
//===============================================================================================================================================================================
#ifndef LIGHTWEIGHT_SHADER_VARIABLES_INCLUDED
#define LIGHTWEIGHT_SHADER_VARIABLES_INCLUDED

// Light Indices block feature
// These are set internally by the engine upon request by RendererConfiguration.
real4 unity_LightData;
real4 unity_LightIndices[2];

#endif // LIGHTWEIGHT_SHADER_VARIABLES_INCLUDED

#ifndef LIGHTWEIGHT_LIGHTING_INCLUDED
#define LIGHTWEIGHT_LIGHTING_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ImageBasedLighting.hlsl"
#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Shadows.hlsl"

///////////////////////////////////////////////////////////////////////////////
//                          Light Helpers                                    //
///////////////////////////////////////////////////////////////////////////////

// Abstraction over Light shading data.
struct Light
{
	half3   direction;
	half3   color;
	half    shadowAttenuation;
	half    distanceAttenuation;
};

int GetPerObjectLightIndex(int index)
{
	// The following code is more optimal than indexing unity_4LightIndices0.
	// Conditional moves are branch free even on mali-400
	half2 lightIndex2 = (index < 2.0h) ? unity_LightIndices[0].xy : unity_LightIndices[0].zw;
	half i_rem = (index < 2.0h) ? index : index - 2.0h;
	return (i_rem < 1.0h) ? lightIndex2.x : lightIndex2.y;
}

///////////////////////////////////////////////////////////////////////////////
//                        Attenuation Functions                               /
///////////////////////////////////////////////////////////////////////////////

// Matches Unity Vanila attenuation
// Attenuation smoothly decreases to light range.
float DistanceAttenuation(float distanceSqr, half2 distanceAttenuation)
{
	// We use a shared distance attenuation for additional directional and puctual lights
	// for directional lights attenuation will be 1
	float lightAtten = rcp(distanceSqr);

#if SHADER_HINT_NICE_QUALITY
	// Use the smoothing factor also used in the Unity lightmapper.
	half factor = distanceSqr * distanceAttenuation.x;
	half smoothFactor = saturate(1.0h - factor * factor);
	smoothFactor = smoothFactor * smoothFactor;
#else
	// We need to smoothly fade attenuation to light range. We start fading linearly at 80% of light range
	// Therefore:
	// fadeDistance = (0.8 * 0.8 * lightRangeSq)
	// smoothFactor = (lightRangeSqr - distanceSqr) / (lightRangeSqr - fadeDistance)
	// We can rewrite that to fit a MAD by doing
	// distanceSqr * (1.0 / (fadeDistanceSqr - lightRangeSqr)) + (-lightRangeSqr / (fadeDistanceSqr - lightRangeSqr)
	// distanceSqr *        distanceAttenuation.y            +             distanceAttenuation.z
	half smoothFactor = saturate(distanceSqr * distanceAttenuation.x + distanceAttenuation.y);
#endif

	return lightAtten * smoothFactor;
}

///////////////////////////////////////////////////////////////////////////////
//                      Light Abstraction                                    //
///////////////////////////////////////////////////////////////////////////////

Light GetMainLight()
{
	Light light;
	light.direction = _MainLightPosition.xyz;
	light.distanceAttenuation = unity_LightData.z;
#if defined(LIGHTMAP_ON)
	light.distanceAttenuation *= unity_ProbesOcclusion.x;
#endif
	light.shadowAttenuation = 1.0;
	light.color = _MainLightColor.rgb;

	return light;
}


Light GetMainLight(float4 shadowCoord)
{
	Light light = GetMainLight();
	light.shadowAttenuation = MainLightRealtimeShadow(shadowCoord);
	return light;
}

Light GetAdditionalLight(int i, float3 positionWS)
{
	int perObjectLightIndex = GetPerObjectLightIndex(i);

	// The following code will turn into a branching madhouse on platforms that don't support
	// dynamic indexing. Ideally we need to configure light data at a cluster of
	// objects granularity level. We will only be able to do that when scriptable culling kicks in.
	// TODO: Use StructuredBuffer on PC/Console and profile access speed on mobile that support it.
	// Abstraction over Light input constants
	float3 lightPositionWS = _AdditionalLightsPosition[perObjectLightIndex].xyz;
	half4 distanceAndSpotAttenuation = _AdditionalLightsAttenuation[perObjectLightIndex];
	half4 spotDirection = _AdditionalLightsSpotDir[perObjectLightIndex];

	float3 lightVector = lightPositionWS - positionWS;
	float distanceSqr = max(dot(lightVector, lightVector), HALF_MIN);

	half3 lightDirection = half3(lightVector * rsqrt(distanceSqr));
	half attenuation = DistanceAttenuation(distanceSqr, distanceAndSpotAttenuation.xy) * AngleAttenuation(spotDirection.xyz, lightDirection, distanceAndSpotAttenuation.zw);

	Light light;
	light.direction = lightDirection;
	light.distanceAttenuation = attenuation;
	light.shadowAttenuation = AdditionalLightRealtimeShadow(perObjectLightIndex, positionWS);
	light.color = _AdditionalLightsColor[perObjectLightIndex].rgb;

	// In case we're using light probes, we can sample the attenuation from the `unity_ProbesOcclusion`
#if defined(LIGHTMAP_ON)
	// First find the probe channel from the light.
	// Then sample `unity_ProbesOcclusion` for the baked occlusion.
	// If the light is not baked, the channel is -1, and we need to apply no occlusion.
	half4 lightOcclusionProbeInfo = _AdditionalLightsOcclusionProbes[perObjectLightIndex];

	// probeChannel is the index in 'unity_ProbesOcclusion' that holds the proper occlusion value.
	int probeChannel = lightOcclusionProbeInfo.x;

	// lightProbeContribution is set to 0 if we are indeed using a probe, otherwise set to 1.
	half lightProbeContribution = lightOcclusionProbeInfo.y;

	half probeOcclusionValue = unity_ProbesOcclusion[probeChannel];
	light.distanceAttenuation *= max(probeOcclusionValue, lightProbeContribution);
#endif

	return light;
}

int GetAdditionalLightsCount()
{
	// TODO: we need to expose in SRP api an ability for the pipeline cap the amount of lights
	// in the culling. This way we could do the loop branch with an uniform
	// This would be helpful to support baking exceeding lights in SH as well
	return min(_AdditionalLightsCount.x, unity_LightData.y);
}

#endif




void LWRPLightingFunction_float(float3 Position, out float3 direction, out float3 color, out float attenuation)
{
#ifdef LIGHTWEIGHT_LIGHTING_INCLUDED

	//Actual light data from the pipeline
	Light light = GetMainLight(GetShadowCoord(GetVertexPositionInputs(Position)));
	
	direction = light.direction;
	color = light.color;
	attenuation = light.shadowAttenuation * light.distanceAttenuation;
#else

	//Hardcoded data, used for the preview shader inside the graph
	//where light functions are not available
	direction = float3(0, 0, 0);
	color = float3(0, 0, 0);
	attenuation = 0;

#endif
}

void AdditionalLightNode_float(float3 worldPos, out float3 direction, out float3 atteColor, out float attenuation) {
//void AdditionalLightNode_float(float3 worldPos, out float3 color) {
	half3 lightColor = 0;
	half3 lightDir = 0;
	half lightAtte;
	int pixelLightCount = GetAdditionalLightsCount();
	for (int i = 0; i < pixelLightCount; ++i)
	{
		Light light = GetAdditionalLight(i, worldPos);

		lightColor += light.color * light.distanceAttenuation * light.shadowAttenuation;
		lightDir += light.direction;
		lightAtte += light.distanceAttenuation * light.shadowAttenuation;
	}

	attenuation = lightAtte;
	atteColor = lightColor;
	direction = lightDir;
}