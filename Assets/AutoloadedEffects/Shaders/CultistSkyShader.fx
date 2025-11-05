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
    float adjustedTime = time * 0.1;
    
    // Pixelate the uvs
    float2 pixelatedUV = worldUV / screenSize;
    
    float2 noiseUV = pixelatedUV - 0.5 * (anchorPoint / screenSize);
    float2 vec1 = float2(0.56, -2.2);
    float2 vec2 = float2(-0.3, -3.9);
    float2 vec3 = float2(0.8, -2.7);
    
    noiseUV *= 2;
    
    // Textures
    float noiseMesh1 = tex2D(diagonalNoise, frac(noiseUV * 1.46  + vec1 * adjustedTime)).g;
    float noiseMesh2 = tex2D(diagonalNoise, frac(noiseUV * 1.57 + vec2 * adjustedTime)).g;
    float noiseMesh3 = tex2D(diagonalNoise, frac(noiseUV * 1.57 + vec3 * adjustedTime)).g;
    float textureMesh = noiseMesh1 * 0.3 + noiseMesh2 * 0.3 + noiseMesh3 * 0.3;
    
    float opacity = 2;
    
    // Thresholds
    bool border = worldDistance < radius && opacity > 0;
    float colorMult = 1;
    float baseMult = 0.8f;
    if (worldDistance < radius * 0.45)
    {
        colorMult = baseMult;
    }
    else if (worldDistance < radius)
    {
        colorMult = baseMult + (1 - baseMult) * InverseLerp(radius * 0.45, radius, worldDistance);
    }
    else
    {
        colorMult = 1;
    }
        
    opacity = clamp(opacity, 0, maxOpacity);
    
    if (colorMult == 1 && (opacity == 0 || worldDistance > radius))
        return sampleColor;
    
    // dark blue sky color
    float4 darkColor = float4(0.01, 0.02, 0.05, 1);
    float4 midColor = float4(0.02, 0.03, 0.08, 1);
    float4 lightColor = float4(0.05, 0.07, 0.15, 1);
    
    float mult = lerp(1, textureMesh * 3.4, pow(worldDistance / radius, 2));
    colorMult *= mult;
    
    float colorLerp = pow(colorMult, 2);
    float4 color;
    float split = 0.6;
    if (colorLerp < split)
    {
        colorLerp = colorLerp / split;
        color = lerp(darkColor, midColor, colorLerp);
    }
    else
    {
        colorLerp = pow((colorLerp - split) / (1 - split), 2);
        color = lerp(midColor, lightColor, colorLerp);
    }
    //opacity /= pow(abs(textureMesh), 1.5);

    if (border)
        colorMult += 0.05;
    
  //  color += textureMesh / 5;
        
    return color * colorMult * opacity;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}