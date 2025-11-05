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
    
    float2 worldUV = screenPosition + screenSize * uv;
    float2 provUV = anchorPoint / screenSize;
    float worldDistance = distance(worldUV, anchorPoint);
    float adjustedTime = time * 0.17;

    float2 coords = uv;

    float2 anchorCoords = (anchorPoint - screenPosition) / screenSize;
    float2 delta = coords - anchorCoords;
    float distanceFromCenter = length(delta);
    float2 polar = float2(atan2(delta.y, delta.x) / (3.141 * 0.25) + 0.5, distanceFromCenter);

    float polarMult = 2.0;
    polar.y *= polarMult;

    float2 timeShift1 = adjustedTime * float2(-0.37, 0.94);
    float2 timeShift2 = adjustedTime * float2(0.0, 0.7);
    float2 timeShift3 = adjustedTime * float2(0.43, 1.13);
    float2 timeShift4 = adjustedTime * float2(0.15, 1.7);

    float noiseMesh1 = tex2D(diagonalNoise, polar - timeShift1).g;
    float noiseMesh2 = tex2D(diagonalNoise, polar - timeShift2).g;
    float noiseMesh3 = tex2D(diagonalNoise, polar - timeShift3).g;
    float noiseMeshDetail = tex2D(diagonalNoise, polar * 3.5 - timeShift4).g;

    float textureMesh = (noiseMesh1 + noiseMesh2 + noiseMesh3) * (1.0 / 3.0);
    textureMesh = lerp(textureMesh, noiseMeshDetail, 0.2);
    
    float flicker = 0.86 + 0.03 * sin(time * 12.0 + polar.y * 6.0 + noiseMesh3 * 10.0);
    float opacity = flicker;

    //float opacity = 0.88;

    bool border = worldDistance > radius && opacity > 0.0;
    float colorMult = border
        ? InverseLerp(radius * 1.4, radius, worldDistance)
        : InverseLerp(radius * 0.985, radius, worldDistance);

    opacity = min(opacity, maxOpacity);

    if (colorMult == 1.0 && (opacity == 0.0 || worldDistance > radius))
        return sampleColor;

    float4 darkColor = float4(0.96, 0.34, 0.04, 1.0);
    float4 midColor = float4(0.98, 0.95, 0.53, 1.0);
    float4 lightColor = float4(1.0, 1.0, 1.0, 1.0);

    float tempShift = 0.5 + 0.5 * sin(time * 2.5);
    midColor.rgb = lerp(midColor.rgb, lightColor.rgb, tempShift * 0.1);
    darkColor.rgb = lerp(darkColor.rgb, midColor.rgb, tempShift * 0.15);

    float colorLerp = pow(colorMult, 3.0);
    float4 color;
    float split = 0.9;

    if (colorLerp < split)
    {
        colorLerp = pow(colorLerp / split, 4.0);
        color = lerp(darkColor, midColor, colorLerp);
    }
    else
    {
        colorLerp = pow((colorLerp - split) / (1.0 - split), 7.0);
        color = lerp(midColor, lightColor, colorLerp);
    }

    color *= pow(abs(textureMesh), 0.25);
    color *= 1.4;

    float spark = step(0.97, noiseMeshDetail);
    color += spark * float4(1.3, 1.1, 0.6, 1.0) * (1.0 - distanceFromCenter) * 0.5;

    return color * colorMult * opacity;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
