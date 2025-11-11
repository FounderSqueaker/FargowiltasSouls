sampler scrollingFireNoise : register(s1);
// sampler fixedFlameNoise : register(s2);

float globalTime;
float opacity;
float scrollSpeed;

float2 screenPosition;
float2 screenSize;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
    // pixelate
    float pixelationLevel = screenSize * 0.5;
    uv = floor(uv * pixelationLevel) / pixelationLevel;
    
    // Split UVs into posterized bands along the Y axis, each with their own
    // unique horizontal offset for the CRT effect.
    float bandCount = 300;
    float posterizedV = round(uv.y * bandCount) / bandCount;
    float maxBandOffset = 0.05;
    float bandOffset = cos(posterizedV * 700) * maxBandOffset;
    uv.x += bandOffset;

    // Calculate a noise value for the upward-dissipating, semi-wavy texture.
    float noise = 0;
    float3 noisePixel = float3(1,1,1);
    float waveScrollSpeed = 1.2 * scrollSpeed;
    float riseSpeed = 0.25 * scrollSpeed;
    for (int i = 0; i < 3; i++)
    {
        float waveAngle = i * 1.57 + globalTime * waveScrollSpeed;
        float2 wave = float2(cos(waveAngle) * 0.01, 0);
        float2 upwardScroll = float2(0, globalTime * riseSpeed);
        float2 noiseUV = uv + wave + upwardScroll; // + screenPosition / 10000;
        noisePixel = tex2D(scrollingFireNoise, noiseUV).rgb;
        noise = tex2D(scrollingFireNoise, noiseUV);
    }

    // Use the whiteness of the noise map as the blend factor
    float3 noiseColor = noisePixel;
    float noiseLuminance = (noiseColor.r + noiseColor.g + noiseColor.b) / 3.0;

    float3 finalColor = lerp(sampleColor.rgb, noiseColor, noiseLuminance);
    float finalModifier = 0.05 * opacity;
    finalColor = lerp(sampleColor.rgb, finalColor, finalModifier);
    float finalA = lerp(sampleColor.a, noiseLuminance, finalModifier);
    return float4(finalColor, finalA);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}