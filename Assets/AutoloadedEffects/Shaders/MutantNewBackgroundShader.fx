sampler noise : register(s1);
//sampler auroraNoise : register(s2);
float radius;
float maxOpacity;
float time;

float2 screenPosition;
float2 screenSize;
float2 anchorPoint;
float2 playerPosition;

float InverseLerp(float a, float b, float t)
{
    return saturate((t - a) / (b - a));
}

float AuroraMesh(float2 uv : TEXCOORD0) : COLOR0
{
    return 0;
    /*
    float2 worldUV = screenPosition + screenSize * uv;
    float2 provUV = anchorPoint / screenSize;
    float worldDistance = distance(worldUV, anchorPoint);
    float adjustedTime = time * 0.01;
    
    // Pixelate the uvs
    float2 pixelatedUV = worldUV / screenSize;

    pixelatedUV.x -= worldUV.x % (1 / screenSize.x);
    pixelatedUV.y -= worldUV.y % (1 / (screenSize.y / 2) * 2);
    
    float2 noiseUV = pixelatedUV - (anchorPoint / screenSize) * 0.5;
    noiseUV = (noiseUV.y, noiseUV.x);
    float2 vec1 = float2(0.56, 1.2);
    float2 vec2 = float2(-0.3, -0.9);
    float2 vec3 = float2(0.8, 0.3);
    
    // Textures
    float noiseMesh1 = tex2D(auroraNoise, frac(noiseUV * 0.46 + vec1 * adjustedTime)).g;
    float noiseMesh2 = tex2D(auroraNoise, frac(noiseUV * 0.57 + vec2 * adjustedTime)).g;
    float noiseMesh3 = tex2D(auroraNoise, frac(noiseUV * 0.57 + vec3 * adjustedTime)).g;
    float textureMesh = noiseMesh1 * 0.3 + noiseMesh2 * 0.3 + noiseMesh3 * 0.3;
    
    float auroraMult = pow(InverseLerp(radius, radius * 1.5, worldDistance), 2);
    
    return textureMesh * auroraMult;
    */
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
    float4 textureMesh = tex2D(noise, frac(noiseUV * 1.47 + float2(0.000 , 1) * time * 0.003) * 1.4);
    textureMesh *= 1.5;

    float4 noiseColorA = float4(0.5, 0.7, 1.0, 1) * textureMesh.r;
    float4 noiseColorB = float4(1.0, 0.4, 0.7, 1) * textureMesh.g;
    float4 noiseColorC = float4(0.3, 1.0, 0.6, 1) * textureMesh.b;
    textureMesh = (noiseColorA + noiseColorB + noiseColorC) * 0.7;

    float colorMult = pow(InverseLerp(radius * 1.4, radius, worldDistance), 0.5);

  

    float4 black = float4(0, 0, 0, 1);
    
    return (textureMesh) * maxOpacity * colorMult * 0.7 + AuroraMesh(uv) * maxOpacity * 1.6;
}


technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
