sampler diagonalNoise : register(s1);

float colorMult;
float time;
float maxOpacity;
float radius;

float2 screenPosition;
float2 screenSize;
float2 anchorPoint;
float2 playerPosition;

// This code has roots in the Providence arena shader in The Calamity Mod.
// I'm still learning the ropes of shader drawing, and like to have a reference point as a foundation.

float InverseLerp(float a, float b, float t)
{
    return saturate((t - a) / (b - a));
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
    // pixelate
    float pixelationLevel = screenSize * 0.5;
    uv = floor(uv * pixelationLevel) / pixelationLevel;
    
    float2 worldUV = screenPosition + screenSize * uv;
    float2 provUV = anchorPoint / screenSize;
    float worldDistance = distance(worldUV, anchorPoint);
    float adjustedTime = time * 0.1;

    float2 pixelatedUV = worldUV / screenSize;

    float2 noiseUV = pixelatedUV - (anchorPoint / screenSize);
    float2 vec1 = float2(0.56, 1.2);
    float2 vec2 = float2(-0.56, 1.2);

    float noiseMesh1 = tex2D(diagonalNoise, frac(noiseUV * 1.46 + vec1 * adjustedTime)).g;
    float noiseMesh2 = tex2D(diagonalNoise, frac(noiseUV * 1.57 + vec2 * adjustedTime)).g;
    float fineDetail = tex2D(diagonalNoise, frac(noiseUV * 3.2 + float2(0.3, 1.1) * adjustedTime)).g;
    float textureMesh = (noiseMesh1 + noiseMesh2) * 0.45 + fineDetail * 0.1;

    float2 polar = normalize(worldUV - anchorPoint);
    float subtleDistortion = sin(dot(polar, float2(1.0, 0.0)) * 6.0 + time * 1.5) * 0.0025;
    worldDistance += subtleDistortion;

    float distToPlayer = distance(playerPosition, worldUV);
    float opacity = 0.9f;
    opacity += InverseLerp(800, 500, distToPlayer) * 0.1;

    bool border = worldDistance < radius && opacity > 0;
    float colorMult = 1;
    if (border) 
        colorMult = InverseLerp(radius * 0.98, radius, worldDistance);
    else
        colorMult = InverseLerp(radius * 1.2, radius, worldDistance);

    opacity = clamp(opacity, 0, maxOpacity);

    if (colorMult == 1 && (opacity == 0 || worldDistance < radius))
        return sampleColor;

    float4 darkColor = float4(0.75, 0.36, 0.08, 1);
    float4 midColor = float4(0.96, 0.60, 0.09, 1);
    float4 lightColor = float4(0.98, 0.95, 0.79, 1);

    float colorLerp = pow(colorMult, 4);
    float4 color;
    float split = 0.6;
    if (colorLerp < split)
    {
        colorLerp = colorLerp / split;
        color = lerp(darkColor, midColor, colorLerp);
    }
    else
    {
        colorLerp = pow((colorLerp - split) / (1 - split), 3);
        color = lerp(midColor, lightColor, colorLerp);
    }

    float textureInfluence = lerp(0.85, 1.0, textureMesh);
    color *= textureInfluence;

    if (!border)
        colorMult += 0.02;

    return color * colorMult * opacity;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}

