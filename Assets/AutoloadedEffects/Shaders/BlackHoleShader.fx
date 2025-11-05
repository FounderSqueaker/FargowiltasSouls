sampler diagonalNoise : register(s1);

float time;
float maxOpacity;
float radius;
float2 mults;

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
    
    float adjustedTime = time * 0.2;
    // Polar coordinates
    float2 coords = uv;

    // transform to polar, relative to anchor point
    //coords.y /= screenSize.x / screenSize.y; // compensate for 
    float2 anchorCoords = (anchorPoint - screenPosition) / screenSize;
    float distanceFromCenter = distance(coords, anchorCoords);
    float2 polar = float2(atan2(coords.y - anchorCoords.y, coords.x - anchorCoords.x) / (3.141 * 0.125) + 0.5, distanceFromCenter * 1);
   
    
    // Textures
    float polarMult = 1;
    polar.x *= mults.x;
    polar.y *= mults.y;
    float noiseMesh1 = tex2D(diagonalNoise, polar - adjustedTime * float2(6, -16)).g;
    float noiseMesh2 = tex2D(diagonalNoise, polar - adjustedTime * float2(7, -26)).g;
    float noiseMesh3 = tex2D(diagonalNoise, polar - adjustedTime * float2(6, -9)).g;
    float textureMesh = noiseMesh1 * 0.33 + noiseMesh2 * 0.33 + noiseMesh3 * 0.33;
    
    float worldDistance = distance(worldUV, anchorPoint);
     
    float opacity = maxOpacity;
    
    float4 black = float4(0, 0, 0, 1);
    float4 color = midColor;
    float blackMult = 0;
    float colorMult = 0;
    if (worldDistance < radius)
    {
        blackMult = 1;
        float frac = 0.99;
        if (worldDistance > radius * frac)
            blackMult -= (worldDistance - (radius * frac)) / (radius * (1 - frac));
        
    }
    if (worldDistance > radius * 0.96)
    {
        colorMult = pow(InverseLerp(radius * 3, radius * 0.96, worldDistance), 3);
        color = lerp(midColor, float4(1, 1, 1, 1), 0.66 * colorMult);
        color = lerp(color, black, 1 * pow(InverseLerp(radius, radius * 3, worldDistance), 3));
        colorMult *= 0.8;
        if (worldDistance < radius)
        {
            colorMult *= pow(InverseLerp(radius * 0.96, radius, worldDistance), 8);
        }
    }
    
    if (blackMult == 0 && colorMult == 0)
        return sampleColor;
        
    float colorOpacity = opacity / pow(abs(textureMesh), 0.5);
    
    return blackMult * black * opacity + color * colorOpacity * colorMult;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
