// uniforms
float4x4 World;
float4x4 View;
float4x4 Projection;

float time;

float texture_scale;
float2 flow_speed;
float wave_frequency;
float wave_amplitude;
float diffuse_amount;

texture diffuse_map;
sampler2D diffuse_map_sampler = sampler_state {
	Texture		= <diffuse_map>;
	MipFilter 	= Linear;
	MinFilter 	= Linear;
	MagFilter 	= Linear;
};

texture heat_map;
sampler2D heat_map_sampler = sampler_state {
	Texture		= <heat_map>;
	MipFilter 	= Linear;
	MinFilter 	= Linear;
	MagFilter 	= Linear;
};

texture fire_map;
sampler2D fire_map_sampler = sampler_state {
	Texture		= <fire_map>;
	MipFilter 	= Linear;
	MinFilter 	= Linear;
	MagFilter 	= Linear;
};


// globals
struct Wave {
	float frequency; // Wavelength L relates to frequency w as w = 2p/L
	float amplitude; // The height from the water plane to the wave crest
	float phase;
	float2 direction; // the horizontal vector perpendicular to the wave front along which the crest travels.
};

#define WAVES 2
Wave Waves[WAVES] = {
	{ 0.0f, 0.0f, 0.50f, float2(1.0f, 0.0f) },
	{ 0.0f, 0.0f, 1.30f, float2(0.0f, 1.0f) }
};

struct VertexShaderInput
{
    float4 Position			: POSITION0;
    float2 TextureCoords	: TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position			: POSITION0;
    float2 TextureCoords	: TEXCOORD0;
    float2 HeatCoords		: TEXCOORD1;
    float2 LavaCoords		: TEXCOORD2;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	float amplitude_scaler = 1.0f;
	float ddx = 0.0f;
	float ddy = 0.0f;
	//input.Position.y = 0;
	
	for(int i = 0; i < WAVES; i++) 
	{
		Waves[i].frequency = wave_frequency * float(i + 1);
		Waves[i].amplitude = wave_amplitude * amplitude_scaler;		
		
		//calculate the wave height
    	input.Position.y += Waves[i].amplitude * sin(dot(Waves[i].direction, input.Position.xz) * Waves[i].frequency + 
    						time * Waves[i].phase);
    	amplitude_scaler *= 0.5f;
    }
    
	float time_mod = fmod(time, 10000.0);
	output.HeatCoords = input.TextureCoords * texture_scale + time_mod * flow_speed;
	output.LavaCoords = input.TextureCoords * texture_scale + time_mod * flow_speed * 0.5f;
	
    float4 world_position = mul(input.Position, World);
    float4 view_position = mul(world_position, View);
    
    output.Position = mul(view_position, Projection);
    output.TextureCoords = input.TextureCoords;
   
    return output;
}

float luminance(float4 color)
{
	return 0.3 * color.r + 0.6 * color.g + 0.1 * color.b;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{    
    float4 diffuse_color = tex2D(diffuse_map_sampler, input.LavaCoords);
    float4 heat_color = tex2D(heat_map_sampler, input.HeatCoords);
    
    float4 color = diffuse_color + diffuse_color * heat_color;
	float threshold = 0.3f;
    
    float luminance_value = luminance(color);
	if(luminance_value >= threshold)
	{
		float lookup = luminance_value - threshold;
		if(lookup > 0.98) lookup = 0.98;
		if(lookup < 0.12) lookup = 0.12;
		color = tex2D(fire_map_sampler, lookup);
	}
	else
	{
		color = diffuse_color * diffuse_amount * (sin(time * 2.0f) * 0.5f + 1.0f);
	}
	
	return float4(color.rgb, 1.0); 
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}

