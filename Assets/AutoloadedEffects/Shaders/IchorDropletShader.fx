sampler diagonalNoise : register(s1);

float time;
float maxOpacity;
float radius;

float2 screenPosition;
float2 screenSize;
float2 anchorPoint;
float4 midColor;
float2 direction;

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

    float2 toUV = worldUV - anchorPoint;
    float stretchFactor = 1.6;
    float2 stretchDir = normalize(direction);
    float2 orthoDir = float2(-stretchDir.y, stretchDir.x);
    float stretchedX = dot(toUV, stretchDir) / stretchFactor;
    float stretchedY = dot(toUV, orthoDir);
    float worldDistance = length(float2(stretchedX, stretchedY));

    float opacity = maxOpacity;

    float adjustedTime = time * 0.2;

    float2 pixelatedUV = worldUV / screenSize;

    float2 noiseUV = pixelatedUV;

    float2 vec1 = float2(0.56, 1.2);
    float2 vec2 = float2(-0.3, -0.9);
    float2 vec3 = float2(0.8, 0.3);

    bool border = worldDistance < radius && opacity > 0;
    float colorMult = 1;
    colorMult = InverseLerp(radius * 3, radius * 0.9, worldDistance);
    colorMult = pow(colorMult, 5);
    if (colorMult > 0.88)
        colorMult = 0.88;

    if (colorMult == 1 && (opacity == 0 || worldDistance > radius))
        return sampleColor;

    return midColor * colorMult * 1 * opacity;

}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}