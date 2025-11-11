sampler diagonalNoise : register(s1);

float time;
float maxOpacity;
float radius;

float2 screenPosition;
float2 screenSize;
float2 anchorPoint;
float4 darkColor = float4(0.8, 0.05, 0.0, 1.0);
float4 midColor = float4(1.0, 0.25, 0.0, 1.0);
float4 brightColor = float4(1.0, 1.0, 1.0, 1.0);

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

    float adjustedTime = time * 0.2;

    float2 pixelatedUV = worldUV / screenSize;

    float2 noiseUV = pixelatedUV;

    float2 vec1 = float2(0.56, 1.2);
    float2 vec2 = float2(-0.3, -0.9);
    float2 vec3 = float2(0.8, 0.3);

    float noise1 = tex2D(diagonalNoise, frac(noiseUV * 1.73 + vec1 * adjustedTime)).g;
    float noise2 = tex2D(diagonalNoise, frac(noiseUV * 2.17 + vec2 * adjustedTime)).g;
    float noise3 = tex2D(diagonalNoise, frac(noiseUV * 2.73 + vec3 * adjustedTime)).g;
    float textureMesh = (noise1 + noise2 + noise3) / 3.0;

    float radiusFalloff = InverseLerp(radius * 1.4, 0.0, worldDistance);
    radiusFalloff = pow(radiusFalloff, 1.5);

    float opacity = radiusFalloff * textureMesh * 5;
    opacity *= maxOpacity;

    if (opacity <= 0.01)
        return sampleColor;

    float colorLerp = textureMesh;
    float4 color;

    if (colorLerp < 0.5)
        color = lerp(darkColor, midColor, colorLerp * 2);
    else
        color = lerp(midColor, brightColor, (colorLerp - 0.5) * 2);

    color = lerp(color, brightColor, pow(radiusFalloff * 0.75, 3));

    color *= 1.2 - pow(abs(textureMesh - 0.5), 2.0);
    color *= pow(abs(textureMesh), 0.2);

    return lerp(sampleColor, color, opacity);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}


