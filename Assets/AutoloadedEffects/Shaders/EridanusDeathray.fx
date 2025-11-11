sampler noiseScrollTexture : register(s1);

float globalTime;
float3 mainColor;
matrix uWorldViewProjection;
float2 laserDirection;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float3 TextureCoordinates : TEXCOORD0;
};
struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float3 TextureCoordinates : TEXCOORD0;
};
VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    float4 pos = mul(input.Position, uWorldViewProjection);
    output.Position = pos;
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;
    return output;
}
float QuadraticBump(float x)
{
    return x * (4 - x * 4);
}

float InverseLerp(float a, float b, float t)
{
    return saturate((t - a) / (b - a));
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    input.Position.y += input.Position.x * 2;
    float2 coords = input.TextureCoordinates;
    float4 color = input.Color;
    
    coords.y = (coords.y - 0.5) / input.TextureCoordinates.z + 0.5;
    
    
    float distanceFromCenter = distance(coords.y, 0.5);
    float distanceFromEdge = distance(distanceFromCenter, 0.4);
    
    //float4 outerGlow = float4(0.12, 0.0, 0.25, 0) * 0; //saturate(pow(0.1 / distanceFromEdge, 3));
    float4 innerGlow = float4(0.1, 0.1, 0.1, 0);
    
    float4 tex1 = tex2D(noiseScrollTexture, input.Position.xy / 77 + float2(globalTime / 3, 0));
    float4 tex2 = tex2D(noiseScrollTexture, input.Position.xy / 88 + float2(globalTime / 3, globalTime / 5));
    float4 tex = (tex1 + tex2) / 2;
    innerGlow = lerp(innerGlow, float4(0, 0, 0, 1), 0) - tex / 2.8;
    float4 glow = innerGlow * 3.1;
    
    float lerper = smoothstep(1, 0, distanceFromCenter) * color.a;
    
    float opacity = 1;
    if (distanceFromCenter > 0.4)
        opacity *= InverseLerp(0.5, 0.4, distanceFromCenter);
    
    return opacity * (lerp(color, glow, lerper) * 2 + float4(0, 0, 0, 1));
}

technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}