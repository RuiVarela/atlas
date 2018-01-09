float4x4 World;
float4x4 View;
float4x4 Projection;
//float3 ColorWhite, ColorBlack;
float3 Color1, Color2, Color3, Color4;
texture CloudTexture1, CloudTexture2;
float AnimationTime;

sampler CloudSampler1 = sampler_state
{
	texture = <CloudTexture1>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = wrap;
	AddressV = wrap;
};

sampler CloudSampler2 = sampler_state
{
	texture = <CloudTexture2>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = wrap;
	AddressV = wrap;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float2 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 TextureCoordinates : TEXCOORD0;
	float3 WorldPosition : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    output.Position = mul(worldPosition, mul(View, Projection));
    output.TextureCoordinates = input.TextureCoordinates;
    output.WorldPosition = worldPosition.xyz;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	//LAVA OLD:
	/*float3 color1 = lerp(ColorBlack, ColorWhite, tex2D(CloudSampler1, input.TextureCoordinates * 20).r);
	float3 color2 = lerp(ColorBlack, ColorWhite, tex2D(CloudSampler2, input.TextureCoordinates * 40).r);
	float3 result = lerp(color1, color2, AnimationTime);*/
    
    //LAVA NEW:
    /*float factor = tex2D(CloudSampler1, input.TextureCoordinates * 40).r * tex2D(CloudSampler2, input.TextureCoordinates * 30).r;
    factor += AnimationTime * 0.2;
    float3 result = float3(0,0,1);
    if(factor < 0.3) result = lerp( float3(0.5,0,0) , float3(0.6,0,0) , factor * (1 / 0.3));
    else if(factor < 0.6) result = lerp( float3(0.6,0,0) , float3(0.7,0.6,0) , (factor - 0.3) * (1 / 0.3));
    else result = lerp( float3(0.7,0.6,0) , float3(1,0,0) , (factor - 0.6) * (1 / 0.4));*/
    
    float factor = tex2D(CloudSampler1, input.TextureCoordinates * 16).r * tex2D(CloudSampler2, input.TextureCoordinates * 12).r;
    factor += AnimationTime * 0.2;
    float3 result;
    if(factor < 0.3) result = lerp( Color1 , Color2 , factor * (1 / 0.3));
    else if(factor < 0.6) result = lerp( Color2 , Color3 , (factor - 0.3) * (1 / 0.3));
    else result = lerp( Color3 , Color4 , (factor - 0.6) * (1 / 0.4));
    
    return float4(result, 1);
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
