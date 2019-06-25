
#ifndef LIGHTWEIGHT_CUSTOMLIGHTING_INCLUDED
#define LIGHTWEIGHT_CUSTOMLIGHTING_INCLUDED

#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Lighting.hlsl"


inline float3 TransformViewToProjection(float3 v) {
	return mul((float3x3)UNITY_MATRIX_P, v);
}

float4 TransformOutlineToHClipScreenSpace(float3 position, float3 normal, float outlineWidth)
{
	half _OutlineScaledMaxDistance = 10;


	float4 nearUpperRight = mul(unity_CameraInvProjection, float4(1, 1, UNITY_NEAR_CLIP_VALUE, _ProjectionParams.y));
	float aspect = abs(nearUpperRight.y / nearUpperRight.x);
	float4 vertex = TransformObjectToHClip(position);
	float3 viewNormal = mul((float3x3)UNITY_MATRIX_IT_MV, normal.xyz);
	float3 clipNormal = TransformViewToProjection(viewNormal.xyz);
	float2 projectedNormal = normalize(clipNormal.xy);
	projectedNormal *= min(vertex.w, _OutlineScaledMaxDistance);
	projectedNormal.x *= aspect;
	vertex.xy += 0.01 * outlineWidth * projectedNormal.xy;

	return vertex;
}

float4 TransformOutlineToHClipWorldSpace(float3 vertex, float3 normal, half outlineWidth)
{
	float3 worldNormalLength = length(mul((float3x3)transpose(unity_WorldToObject), normal));
	float3 outlineOffset = 0.01 * outlineWidth * worldNormalLength * normal;
	return TransformObjectToHClip(vertex + outlineOffset);
}


half3 AttenuatedLightColor(InputData inputData) {
	Light mainLight = GetMainLight(inputData.shadowCoord);
	MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, half4(0, 0, 0, 0));

	half3 attenuatedLightColor = mainLight.color * (mainLight.distanceAttenuation * mainLight.shadowAttenuation);
	
#ifdef _ADDITIONAL_LIGHTS
	int pixelLightCount = GetAdditionalLightsCount();
	for (int i = 0; i < pixelLightCount; ++i)
	{
		Light light = GetAdditionalLight(i, inputData.positionWS);
		attenuatedLightColor += light.color * (light.distanceAttenuation * light.shadowAttenuation);
	}
#endif	

	
	return attenuatedLightColor;
}

half3 LightingHalfLambert(half3 lightColor, half3 lightDir, half3 normal, float coef)
{	
	half NdotL = pow((dot(normal, lightDir)) * 0.5 + 0.5, coef);
	return lightColor * NdotL;
}

half4 LightweightFragmentLambert(InputData inputData, half3 diffuse, half3 emission, half alpha)
{
	Light mainLight = GetMainLight(inputData.shadowCoord);
	MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, half4(0, 0, 0, 0));

	half3 attenuatedLightColor = mainLight.color * (mainLight.distanceAttenuation * mainLight.shadowAttenuation);
	half3 diffuseColor = inputData.bakedGI + LightingLambert(attenuatedLightColor, mainLight.direction, inputData.normalWS);
	
#ifdef _ADDITIONAL_LIGHTS
	int pixelLightCount = GetAdditionalLightsCount();
	for (int i = 0; i < pixelLightCount; ++i)
	{
		Light light = GetAdditionalLight(i, inputData.positionWS);
		half3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
		diffuseColor += LightingLambert(attenuatedLightColor, light.direction, inputData.normalWS);
	}
#endif

#ifdef _ADDITIONAL_LIGHTS_VERTEX
	diffuseColor += inputData.vertexLighting;
#endif

	half3 finalColor = diffuseColor * diffuse + emission;
	return half4(finalColor, alpha);
}

half4 LightweightFragmentHalfLambert(InputData inputData, half3 diffuse, half3 emission, half alpha, float coef)
{
	Light mainLight = GetMainLight(inputData.shadowCoord);
	MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, half4(0, 0, 0, 0));

	half3 attenuatedLightColor = mainLight.color * (mainLight.distanceAttenuation * mainLight.shadowAttenuation);
	half3 diffuseColor = inputData.bakedGI + LightingHalfLambert(attenuatedLightColor, mainLight.direction, inputData.normalWS, coef);

#ifdef _ADDITIONAL_LIGHTS
	int pixelLightCount = GetAdditionalLightsCount();
	for (int i = 0; i < pixelLightCount; ++i)
	{
		Light light = GetAdditionalLight(i, inputData.positionWS);
		half3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
		diffuseColor += LightingHalfLambert(attenuatedLightColor, light.direction, inputData.normalWS, coef);
	}
#endif


#ifdef _ADDITIONAL_LIGHTS_VERTEX
	diffuseColor += inputData.vertexLighting;
#endif
	half3 finalColor = diffuseColor * diffuse + emission;
	return half4(finalColor, alpha);
}
#endif


