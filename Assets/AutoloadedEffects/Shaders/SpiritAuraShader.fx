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
    
    //float textureScale = radius / 1400; // 1400 is abom's ritual size
    
    float2 worldUV = screenPosition + screenSize * uv;
    float2 provUV = anchorPoint / screenSize;
    float worldDistance = distance(worldUV, anchorPoint);
    float adjustedTime = time * 0.17;
    
    // Polar coordinates
    float2 coords = uv;
    // transform to polar, relative to anchor point
    //coords.y /= screenSize.x / screenSize.y; // compensate for 
    float2 anchorCoords = (anchorPoint - screenPosition) / screenSize;
    float distanceFromCenter = distance(coords, anchorCoords);
    float2 polar = float2(atan2(coords.y - anchorCoords.y, coords.x - anchorCoords.x) / (3.141 * 0.25) + 0.5, distanceFromCenter * 1);
   
    
    // Textures
    float polarMult = 2;
    polar.y *= polarMult;
    float noiseMesh1 = tex2D(diagonalNoise, polar - adjustedTime * float2(-0.77, 0.94)).g;
    float noiseMesh2 = tex2D(diagonalNoise, polar - adjustedTime * float2(-0, 0.7)).g;
    float noiseMesh3 = tex2D(diagonalNoise, polar - adjustedTime * float2(0.93, 1.13)).g;
    float textureMesh = noiseMesh1 * 0.3 + noiseMesh2 * 0.3 + noiseMesh3 * 0.3;
    
    float opacity = 0.33;
    
        // Get the distance to the pixel from the player.
    float distToPlayer = distance(playerPosition, worldUV);
    // Fade in quickly as the player approaches the pixels
    opacity += InverseLerp(800, 500, distToPlayer) * 0.55;
    
    // Thresholds
    bool border = worldDistance > radius && opacity > 0;
    float colorMult = 1;
    if (border) 
        colorMult = InverseLerp(radius * 1.4, radius, worldDistance);
    else
    {
        colorMult = InverseLerp(radius * 0.99, radius, worldDistance);
    }
    
    opacity = clamp(opacity, 0, maxOpacity);
    
    if (colorMult == 1 && (opacity == 0 || worldDistance > radius))
        return sampleColor;
    
    float4 darkColor = float4(0.98, 0.95, 0.53, 1);; //red
    float4 midColor = float4(0.77, 0.7, 0.33, 1); //yellow
    float4 lightColor = float4(1, 1, 1, 1); //white
    
    float colorLerp = pow(colorMult, 3);
    //colorLerp = lerp(colorLerp, colorLerp * textureMesh + 0.3, 0.2);
    float4 color;
    float split = 0.9;
    //if (!border)
    //    colorLerp = split + (1 - split) * colorLerp;
    
    colorLerp = pow(colorLerp, 4);
    color = lerp(darkColor, midColor, colorLerp);
    /*
    if (colorLerp < split)
    {
        colorLerp = pow(colorLerp / split, 4);
        color = lerp(darkColor, midColor, colorLerp);
    }
    else
    {
        colorLerp = pow((colorLerp - split) / (1 - split), 7);
        color = lerp(midColor, lightColor, colorLerp);
    }
*/

    color *= pow(abs(textureMesh), 0.25);
    color *= 1.4;

    return color * colorMult * opacity;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}