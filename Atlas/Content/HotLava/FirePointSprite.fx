float4x4 World;
float4x4 View;
float4x4 Projection;

float time;
float max_height;
float particle_circular_radius;

texture fire_map;
sampler2D fire_map_sampler = sampler_state {
	Texture		= <fire_map>;
	MipFilter 	= Linear;
	MinFilter 	= Linear;
	MagFilter 	= Linear;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Data : COLOR0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TextureCoords : TEXCOORD0;
    float Alpha : COLOR0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    //data.x = phase
    //data.y = speed
    //data.z = x phase
    float time_factor = frac(time * input.Data.y + input.Data.x);
    
	
	input.Position.y += time_factor * max_height;
	input.Position.x += sin(time * 0.2f + input.Data.z) * particle_circular_radius;
	input.Position.z += cos(time * 0.3f + input.Data.z) * particle_circular_radius;
    
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    
    output.TextureCoords = float2(0.0f, 0.0f);
    
    
    output.Alpha = sin(time_factor * 3.14159);
    
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 texture_color = tex2D(fire_map_sampler, input.TextureCoords);
	texture_color.a *= input.Alpha;
    return texture_color;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
