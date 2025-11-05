float radius;
float maxOpacity;


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
    float2 provUV = anchorPoint / screenSize;
    float worldDistance = distance(worldUV, anchorPoint);
    
    float opacity = 1;
    
    // Define the border and mix the inferno for a smooth transition
    bool border = worldDistance < radius && opacity > 0;
    float colorMult = 1;
    if (border) 
        colorMult = 0;
    else
    {
        colorMult = InverseLerp(radius * 1.4, radius, worldDistance);
    }
    
    float blackOpacity = 0;
    float4 black = float4(0, 0, 0, 1);
    if (worldDistance < radius)
        blackOpacity = 1;
    
    blackOpacity = clamp(blackOpacity, 0, maxOpacity);
    
    return black * (1 - colorMult) * blackOpacity;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
