float4x4 World;
float4x4 View;
float4x4 Projection;

texture sprite_map;
sampler2D sprite_sampler = sampler_state {
	Texture		= <sprite_map>;
	MipFilter 	= None;
	MinFilter 	= None;
	MagFilter 	= None;
};

texture positionMap;
sampler positionSampler = sampler_state
{
    Texture   = <positionMap>;
    MipFilter = None;
    MinFilter = Point;
    MagFilter = Point;
    AddressU  = Clamp;
    AddressV  = Clamp;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color	: COLOR0;
};

struct VertexShaderOutput
{
   float4 Position : POSITION0;
    
   #ifdef XBOX
   float4 TextureCoords : SPRITETEXCOORD;
   #else
   float2 TextureCoords : TEXCOORD0;
   #endif
   
   float Size		:PSIZE0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;   
    
    float4 position = tex2Dlod(positionSampler, float4(input.Position.x, input.Position.z, 0, 0));
    position.w = 1.0f;
     
    float4 worldPosition = mul(position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    
    
    output.TextureCoords = float2(0.0f, 0.0f);
    // 2.0f = particle_size, 600 screen_height
    output.Size = 2.0f * Projection._m11 / output.Position.w * 600 / 2;
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 texture_color = tex2D(sprite_sampler, input.TextureCoords);
	
	//texture_color.a = 0.5f;
    return texture_color;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
