float4x4 World;
float4x4 View;
float4x4 Projection;

float time;

float4 low_color;
float4 high_color;
float height_scale;
float base_y;

struct VertexShaderInput
{
    float4 Position			: POSITION0;
};

struct VertexShaderOutput
{
    float4 Position			: POSITION0;
    float Height			: TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
	output.Height = input.Position.y;
	
	input.Position.y += base_y;

    float4 world_position = mul(input.Position, World);
    float4 view_position = mul(world_position, View);
    output.Position = mul(view_position, Projection);


    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{   

	float variation = sin(time * 0.8f) * 0.3f; 
	float height = clamp((input.Height / height_scale) + variation, 0.0f, 1.0f);
	float4 color = lerp(low_color, high_color, height);
	
	color.a = 1.0f;
	return color;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}

