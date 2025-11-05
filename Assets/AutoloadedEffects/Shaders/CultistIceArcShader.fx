sampler diagonalNoise : register(s1);

float colorMult;
float time;
float maxOpacity;
float radius;

float2 screenPosition;
float2 screenSize;
float2 anchorPoint;
float2 nearbyPosition;
float arcWidth;

float4 darkColor;
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
    //float textureScale = radius / 1400; // 1400 is abom's ritual size
    
    float2 worldUV = screenPosition + screenSize * uv;
    float2 provUV = anchorPoint / screenSize;
    float worldDistance = distance(worldUV, anchorPoint);
    float adjustedTime = time * 0.2;
    
    float2 direction = worldUV - anchorPoint;
    float angleDiff = abs(acos(dot(direction, nearbyPosition) / (length(direction) * length(nearbyPosition))));
    
    // Polar coordinates
    float2 coords = uv;
    // transform to polar, relative to anchor point
    //coords.y /= screenSize.x / screenSize.y; // compensate for 
    float2 anchorCoords = (anchorPoint - screenPosition) / screenSize;
    float distanceFromCenter = distance(coords, anchorCoords);
    float2 polar = float2(atan2(coords.y - anchorCoords.y, coords.x - anchorCoords.x) / (3.141 * 0.125) + 0.5, distanceFromCenter * 1);
   
    
    // Textures
    float polarMult = 4;
    polar.x *= 0.5;
    polar.y *= polarMult;
    float noiseMesh1 = tex2D(diagonalNoise, polar - adjustedTime * float2(-0.47, 3)).g;
    float noiseMesh2 = tex2D(diagonalNoise, polar - adjustedTime * float2(-1.39, 2)).g;
    float noiseMesh3 = tex2D(diagonalNoise, polar - adjustedTime * float2(0.93, 2.7)).g;
    float textureMesh = noiseMesh1 * 0.33 + noiseMesh2 * 0.33 + noiseMesh3 * 0.33;
    
    //float distToPlayer = distance(nearbyPosition, worldUV);
    float opacity = maxOpacity;
    //opacity += InverseLerp(500, 200, distToPlayer) * maxOpacity;
    
    
    // Thresholds
    bool border = worldDistance > radius && opacity > 0;
    float colorMult = 1;
    if (border) 
        colorMult = pow(InverseLerp(radius * 34, radius, worldDistance), 0.5);
    else
    {
        colorMult = InverseLerp(radius * 0.4, radius, worldDistance);
    }
    colorMult *= InverseLerp(arcWidth, arcWidth * 0.5f, angleDiff);
    if (colorMult > 0.86)
        colorMult = 0.86;
        
    opacity = clamp(opacity, 0, maxOpacity);
    
    if (colorMult == 1 && (opacity == 0 || worldDistance > radius))
        return sampleColor;
    
    
    float colorLerp = pow(colorMult, 4);
    float4 color;
    float split = 0.6;
    color = lerp(darkColor, midColor, colorLerp);
    opacity /= pow(abs(textureMesh), 0.5);
        
    return color * colorMult * opacity;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
