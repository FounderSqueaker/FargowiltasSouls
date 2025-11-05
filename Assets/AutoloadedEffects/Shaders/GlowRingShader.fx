sampler diagonalNoise : register(s1);

float time;
float maxOpacity;
float radius;

float2 screenPosition;
float2 screenSize;
float2 anchorPoint;
float4 midColor;

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
    worldUV.x -= worldUV.x % 2;
    worldUV.y -= worldUV.y % 2;
    float worldDistance = distance(worldUV, anchorPoint);
    
    float opacity = maxOpacity;
    
    // Thresholds
    bool border = worldDistance < radius && opacity > 0;
    float colorMult = 1;
    colorMult = InverseLerp(radius, 0, worldDistance);
    if (colorMult > 0.88)
        colorMult = 0.88;
    colorMult = pow(colorMult, 0.75);
    
    if (colorMult == 1 && (opacity == 0 || worldDistance > radius))
        return sampleColor;
        
    return midColor * colorMult * 1.6 * opacity;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
