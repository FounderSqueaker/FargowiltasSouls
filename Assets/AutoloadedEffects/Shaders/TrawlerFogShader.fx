sampler diagonalNoise : register(s1);

float colorMult;
float time;
float maxOpacity;
float radius;

float2 screenPosition;
float2 screenSize;
float2 anchorPoint;
float2 playerPosition;

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
    float worldDistance = distance(worldUV, anchorPoint);

    float pulse = sin(time * 6) * 0.5 + 0.5;
    float pulseOpacity = lerp(0.77, 1.0, pulse);
    float pulseRadius = radius * lerp(0.95, 1.05, pulse);

    float adjustedTime = time * 0.05;

    float2 pixelatedUV = worldUV / screenSize;

    float2 noiseUV = pixelatedUV;

    float2 vec1 = float2(0.56, 1.2);
    float2 vec2 = float2(-0.3, -0.9);
    float2 vec3 = float2(0.8, 0.3);

    float noise1 = tex2D(diagonalNoise, frac(noiseUV * 2.7 + vec1 * adjustedTime)).g;
    float noise2 = tex2D(diagonalNoise, frac(noiseUV * 1.83 + vec2 * adjustedTime)).g;
    float noise3 = tex2D(diagonalNoise, frac(noiseUV * 2.44 + vec3 * adjustedTime)).g;
    float textureMesh = (noise1 + noise2 + noise3) / 3.0;

    float fogFalloff = InverseLerp(pulseRadius * 7, pulseRadius * 0.8, worldDistance);
    fogFalloff = pow(fogFalloff, 3.7); 

    float opacity = 1 * fogFalloff * pulseOpacity * (textureMesh * 3);
    opacity = clamp(opacity, 0.0, maxOpacity);

    if (opacity <= 0.01)
        return sampleColor;

    float4 darkGreen = float4(0.0, 0.2, 0.05, 1.0);
    float4 midGreen = float4(0.1, 0.5, 0.1, 1.0);
    float4 lightGreen = float4(0.6, 1.0, 0.3, 1.0);

    float colorLerp = fogFalloff;
    float4 color;

    if (colorLerp < 0.5)
        color = lerp(darkGreen, midGreen, colorLerp * 2);
    else
        color = lerp(midGreen, lightGreen, (colorLerp - 0.5) * 2);

    color *= 1.0 - pow(abs(textureMesh - 0.5), 2.0);
    
    color *= pow(abs(textureMesh), 0.25);

    return lerp(sampleColor, color, opacity);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}


