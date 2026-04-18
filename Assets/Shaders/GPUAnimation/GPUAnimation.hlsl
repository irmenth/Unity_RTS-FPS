#ifndef GPUANIMATION_HLSL_INCLUDED
#define GPUANIMATION_HLSL_INCLUDED

void SampleGPUAnimation_float(
    in float vertexID,
    in float totalFrameCount,
    in float frame1,
    in float frame2,
    in float t,
    in float vertexCount,
    in TEXTURE2D_PARAM(animTex, sampler_animTex),
    out float3 outPosOS,
    out float3 outNormalOS
)
{
    float doubleCount = vertexCount * 2;

    float2 uvPos1;
    uvPos1.x = (frame1 + 0.5) / totalFrameCount;
    uvPos1.y = (2 * vertexID + 0.5) / doubleCount;
    float2 uvPos2;
    uvPos2.x = (frame2 + 0.5) / totalFrameCount;
    uvPos2.y = uvPos1.y;

    float2 uvNormal1;
    uvNormal1.x = uvPos1.x;
    uvNormal1.y = (2 * vertexID + 1 + 0.5) / doubleCount;
    float2 uvNormal2;
    uvNormal2.x = uvPos2.x;
    uvNormal2.y = uvNormal1.y;

    float4 posData1 = SAMPLE_TEXTURE2D_LOD(animTex, sampler_animTex, uvPos1, 0);
    float4 posData2 = SAMPLE_TEXTURE2D_LOD(animTex, sampler_animTex, uvPos2, 0);
    float4 posData = lerp(posData1, posData2, t);
    outPosOS = posData.xyz;

    float4 normalData1 = SAMPLE_TEXTURE2D_LOD(animTex, sampler_animTex, uvNormal1, 0);
    float4 normalData2 = SAMPLE_TEXTURE2D_LOD(animTex, sampler_animTex, uvNormal2, 0);
    float4 normalData = normalize(lerp(normalData1, normalData2, t));
    outNormalOS = normalData.xyz;
}

#endif