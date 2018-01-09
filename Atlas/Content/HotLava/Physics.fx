texture temporaryMap;
sampler temporarySampler : register(s0)  = sampler_state
{
    Texture   = <temporaryMap>;
    MipFilter = None;
    MinFilter = Point;
    MagFilter = Point;
    AddressU  = Clamp;
    AddressV  = Clamp;
};

texture positionMap;
sampler positionSampler  = sampler_state
{
    Texture   = <positionMap>;
    MipFilter = None;
    MinFilter = Point;
    MagFilter = Point;
    AddressU  = Clamp;
    AddressV  = Clamp;
};

texture velocityMap;
sampler velocitySampler = sampler_state
{
    Texture   = <velocityMap>;
    MipFilter = None;
    MinFilter = Point;
    MagFilter = Point;
    AddressU  = Clamp;
    AddressV  = Clamp;
};

texture randomMap;
sampler randomSampler : register(s0) = sampler_state
{
    Texture   = <randomMap>;
    MipFilter = None;
    MinFilter = Point;
    MagFilter = Point;
    AddressU  = wrap;
    AddressV  = wrap;
};

float maxLife = 5.0f;
float elapsedTime;
float min_height = -12.0f;
float4 acceleration = float4(0.0f, -10.0f, 0.0f, 0.0f);
  
float3 generateNewPosition(float2 uv)
{
		float4 rand = tex2D(randomSampler, uv);
		return float3(rand.x * 150, min_height, rand.z * 150);
}
  
float4 UpdatePositionsPS(in float2 uv : TEXCOORD0) : COLOR
{
	float4 pos = tex2D(positionSampler, uv);
	if (pos.w >= maxLife)
	{
		// Restart time
		pos.w -= maxLife;
		// Compute new position and direction
		pos.xyz = generateNewPosition(uv);
	} 
	else
	{
		// Update particle position
		float4 velocity = tex2D(velocitySampler, uv);
		pos.xyz += elapsedTime * velocity;
		pos.w += elapsedTime;
	}
	return pos;
}



float4 UpdateVelocitiesPS(in float2 uv : TEXCOORD0) : COLOR
{
	float4 velocity = tex2D(velocitySampler, uv);
	float4 pos = tex2D(positionSampler, uv);

	if (pos.w >= maxLife)
	{
			//reset velocity
			float4 rand = tex2D(randomSampler, uv);
			float4 rand2 = tex2D(randomSampler, uv + float2(rand.y,rand.w));
			velocity.xyz = rand2.xyz * 8 + 10.0 * rand.xyz;
			velocity.y = -acceleration.y * 0.5;
	}
	else
	{

		velocity += acceleration * elapsedTime; 
	}
	return velocity;
}

float4 ResetPositionsPS(in float2 uv : TEXCOORD0) : COLOR
{
	return float4(generateNewPosition(uv), maxLife * frac (tex2D(randomSampler, 10.2484 * uv).w));
}

float4 ResetVelocitiesPS(in float2 uv : TEXCOORD0) : COLOR
{
	float4 rand = tex2D(randomSampler, uv);
	rand.y = -acceleration.y * 0.5;
	rand.w = 1.0f; 
	return rand;
}

float4 CopyTexturePS(in float2 uv : TEXCOORD0) : COLOR
{
	return tex2D(temporarySampler, uv);
}

technique ResetPositions
{
    pass P0
    {
        pixelShader  = compile ps_2_0 ResetPositionsPS();
    }
}
technique ResetVelocities
{
    pass P0
    {
        pixelShader  = compile ps_2_0 ResetVelocitiesPS();
    }
}
technique CopyTexture
{
    pass P0
    {
        pixelShader  = compile ps_2_0 CopyTexturePS();
    }
}
technique UpdatePositions
{
    pass P0
    {
        pixelShader  = compile ps_2_0 UpdatePositionsPS();
    }
}
technique UpdateVelocities
{
    pass P0
    {
        pixelShader  = compile ps_2_0 UpdateVelocitiesPS();
    }
}