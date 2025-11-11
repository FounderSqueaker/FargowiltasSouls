sampler noise : register(s1);
float radius;
float maxOpacity;
float time;
float2 scrollVector;

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
    float2 provUV = screenPosition / screenSize;
    float worldDistance = distance(worldUV, anchorPoint);

    float opacity = 1;

    float2 pixelatedUV = worldUV / screenSize;

    float2 noiseUV = pixelatedUV - (screenPosition / screenSize) * 0.99;
    float4 textureMesh = tex2D(noise, frac(noiseUV * 1.46 + scrollVector) * 2);
    textureMesh *= 0.8;

    float4 noiseColorA = float4(0.5, 0.7, 1.0, 1) * textureMesh.r;
    float4 noiseColorB = float4(1.0, 0.4, 0.7, 1) * textureMesh.g;
    float4 noiseColorC = float4(0.3, 1.0, 0.6, 1) * textureMesh.b;
    textureMesh = (noiseColorA + noiseColorB + noiseColorC) * 0.7;

    float starNoise = tex2D(noise, frac(noiseUV * 3.1 + scrollVector * 0.4) / 2).r;
    float starMask = smoothstep(0.96, 0.995, starNoise);
    float twinkle = 0.7 + 0.3 * tex2D(noise, frac(noiseUV * 5.3 + scrollVector * 0.9)).r;
    float4 starColor = float4(1.0, 0.9, 0.7, 1) * starMask * twinkle;
    textureMesh += starColor * 1.2;

    bool border = worldDistance > radius && opacity > 0;
    float colorMult = InverseLerp(radius * 1.4, radius, worldDistance);

    float4 black = float4(0, 0, 0, 1);

    float threshold = screenSize.y * 1;
    float2 screenCenter = screenPosition + screenSize / 2;
    float screenDistance = distance(worldUV, screenCenter);

    float bloom = pow(InverseLerp(threshold * 0.36, threshold * 1.9, screenDistance), 2.5);
    float4 bloomColor = float4(0.4, 0.6, 1.0, 1) * bloom * 0.9
                  + float4(1.0, 0.3, 0.6, 1) * bloom * 0.7
                  + float4(0.2, 1.0, 0.5, 1) * bloom * 0.5;

    float flicker = tex2D(noise, frac(noiseUV * 2.7 + scrollVector * 0.3)).r;
    bloomColor.rgb *= 0.8 + 0.2 * flicker;

    if (screenDistance < threshold)
        textureMesh *= 0.15 + 0.85 * pow(screenDistance / threshold, 1.1);

    textureMesh += bloomColor;

    return (black + textureMesh) * maxOpacity * colorMult;



}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
