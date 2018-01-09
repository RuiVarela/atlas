float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 NormalMatrix;
float3 LightPosition, EyePosition;
texture Diffuse, Specular, Normal;
float SpecularPower;
float Modulation;

sampler DiffuseSampler = sampler_state
{
	texture = <Diffuse>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = wrap;
	AddressV = wrap;
};

sampler SpecularSampler = sampler_state
{
	texture = <Specular>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = wrap;
	AddressV = wrap;
};

sampler NormalSampler = sampler_state
{
	texture = <Normal>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = wrap;
	AddressV = wrap;
};

struct VertexShaderInput
{
    float4 position : POSITION0;
	float2 textureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 position : POSITION0;
	float2 textureCoordinates : TEXCOORD0;
	float3 toEye : TEXCOORD1; //in world-space
	float3 toLight : TEXCOORD2; //in world-space
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
	output.textureCoordinates = input.textureCoordinates;
	float4 positionWS = mul(input.position, World);
    output.position = mul(positionWS, mul(View, Projection));
    output.toEye = normalize(EyePosition - positionWS.xyz);
    output.toLight = normalize(LightPosition - positionWS.xyz);
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float3 N = tex2D(NormalSampler, input.textureCoordinates).xzy;
	N = (N * 2) - 1;
	N = mul(float4(N, 0.0), NormalMatrix).xyz;
	N = normalize(N);
	float3 E = normalize(input.toEye);
	float3 toLight = normalize(input.toLight);
	float3 halfV = normalize(toLight + E);
	float specular = dot(N, halfV) * tex2D(SpecularSampler, input.textureCoordinates).r;
	specular = pow(specular, SpecularPower);
	float diffuse = max(0, dot(N, toLight));
	
	float3 final = tex2D(DiffuseSampler, input.textureCoordinates).rgb * diffuse + specular;
	final *= Modulation;
	return float4(final, 1.0);
	
	/*float temp = diffuse;
    return float4(temp, temp, temp, 1.0);*/
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_1_1 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
