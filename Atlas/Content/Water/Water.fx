/*
 * http://http.developer.nvidia.com/GPUGems/gpugems_ch01.html
**/

// uniforms
float4x4 World;
float4x4 View;
float4x4 ViewInverted;
float4x4 Projection;

float time;

float texture_scale;
float bump_height;
float2 flow_speed;
float wave_frequency;
float wave_amplitude;

float4 deep_water_color;
float4 surface_water_color;
float4 reflection_color;
float water_amount;
float reflection_amount;
float fresnel_bias;
float fresnel_power;

texture cube_map;
samplerCUBE cube_map_sampler = sampler_state {
	Texture		= <cube_map>;
	MipFilter 	= Linear;
	MinFilter 	= Linear;
	MagFilter 	= Linear;
};

texture normal_map;
sampler2D normal_map_sampler = sampler_state {
	Texture		= <normal_map>;
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

#define WAVES 3
Wave Waves[WAVES] = {
	{ 0.0f, 0.0f, 0.50f, float2(1.0f, -1.0f) },
	{ 0.0f, 0.0f, 1.30f, float2(0.0f, 0.5f) },
	{ 0.0f, 0.0f, 0.25f, float2(0.2f, 0.3f) },
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
    float3 TangentBasis[3]	: TEXCOORD1;
    float3 ViewVector		: TEXCOORD4;
    float2 Bump				: TEXCOORD5;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	float amplitude_scaler = 1.0f;
	float ddx = 0.0f;
	float ddy = 0.0f;
	//input.Position.y = 0;
	
	for(int i = 0; i < 3; i++) 
	{
		Waves[i].frequency = wave_frequency * float(i + 1);
		Waves[i].amplitude = wave_amplitude * amplitude_scaler;		
		
		//calculate the wave height
    	input.Position.y += Waves[i].amplitude * sin(dot(Waves[i].direction, input.Position.xz) * Waves[i].frequency + 
    						time * Waves[i].phase);
    						
    	//calculate the differential
    	float differential = Waves[i].amplitude * Waves[i].frequency * cos(dot(Waves[i].direction, input.Position.xz) * Waves[i].frequency + 
    						 time * Waves[i].phase);
    	
    	ddx += differential * Waves[i].direction.x;
    	ddy += differential * Waves[i].direction.y;
    	amplitude_scaler *= 0.5f;
    }
    
    float3 binormal	= float3(1.0f, ddx, 0.0f);
    float3 tangent	= float3(0.0f, ddy, 1.0f);
    float3 normal	= float3(-ddx,   1, -ddy);
    
    //convert the tangent frame to world space
    output.TangentBasis[0] = mul(bump_height * normalize(tangent), World);
	output.TangentBasis[1] = mul(bump_height * normalize(binormal), World);
	output.TangentBasis[2] = mul(normalize(normal), World);
	
	float time_mod = fmod(time, 100.0f);
	output.Bump = input.TextureCoords * texture_scale * 1.0f + time_mod * flow_speed * 1.0f;
	
    float4 world_position = mul(input.Position, World);
    float4 view_position = mul(world_position, View);
    
    output.Position = mul(view_position, Projection);
    output.ViewVector = ViewInverted[3].xyz - world_position;
    output.TextureCoords = input.TextureCoords * texture_scale;
   
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{    
    // tangent to world
    float3x3 tangent_matrix;
    tangent_matrix[0] = input.TangentBasis[0];
    tangent_matrix[1] = input.TangentBasis[1];
    tangent_matrix[2] = input.TangentBasis[2];

    float3 normal = tex2D(normal_map_sampler, input.Bump) * 2.0f - 1.0f;    
    float3 world_normal = normalize(mul(normal, tangent_matrix));
    
    input.ViewVector = normalize(input.ViewVector);
    float view_dot_normal = 1.0 - max(dot(input.ViewVector, world_normal), 0.0f);
    
    float3 refected_view_vector = reflect(-input.ViewVector, world_normal);
    float4 reflection = texCUBE(cube_map_sampler, refected_view_vector);
  
    //float4 reflection = tex2D(cube_map_sampler, input.TextureCoords);
	float fresnel = fresnel_bias + (1.0f - fresnel_bias) * pow(view_dot_normal, fresnel_power);

    float4 water_color = lerp(deep_water_color, surface_water_color, view_dot_normal);
	
	return water_color * water_amount + 
	       float4(reflection.rgb, 1.0) * reflection_color * reflection_amount * fresnel;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}

