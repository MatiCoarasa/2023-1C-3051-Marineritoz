#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

uniform float4x4 World;
uniform float4x4 View;
uniform float4x4 Projection;

float Time = 0;

uniform texture ModelTexture;
sampler2D TextureSampler = sampler_state
{
    Texture = (ModelTexture);
    MagFilter = Linear;
    MinFilter = Linear;
    MipFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float2 TextureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float2 TextureCoordinate : TEXCOORD0;
};

static const float PI = 3.14159265f;

float3 GerstnerWave (float4 wave, float3 position) {
    float steepness = wave.z;
    float waveLength = wave.w;
    float2 direction = normalize(wave.xy);
	// float speed = 1.5;
	float k = 2 * PI / waveLength;
	float speed = sqrt(9.8 / k);
	float waveAmplitude = steepness / k;
	float frequency = k * (dot(direction, position.xz) - speed * Time);
    return float3(
		direction.x * (waveAmplitude * cos(frequency)),
		waveAmplitude * sin(frequency),
		direction.y * (waveAmplitude * cos(frequency))
    );
}

float4 Wave(float2 direction, float steepness, float waveLength)
{
    return float4(direction, steepness, waveLength);
}

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    // Clear the output
	VertexShaderOutput output = (VertexShaderOutput)0;
	// Model space to World space
    float4 worldPosition = mul(input.Position, World);
	float3 anchorPoint = worldPosition.xyz;
	float4 wave1 = Wave(float2(1,1),0.25,15);
	float4 wave2 = Wave(float2(1,0.6),0.25,10);
	float4 wave3 = Wave(float2(1,1.3),0.25,5);
    worldPosition.xyz += GerstnerWave(wave1, anchorPoint);
    worldPosition.xyz += GerstnerWave(wave2, anchorPoint);
    worldPosition.xyz += GerstnerWave(wave3, anchorPoint);
    // World space to View space
    float4 viewPosition = mul(worldPosition, View);	
	// View space to Projection space
    output.Position = mul(viewPosition, Projection);
    output.TextureCoordinate = input.TextureCoordinate;
    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 textureColor = tex2D(TextureSampler, input.TextureCoordinate);
    textureColor.a = 1;
    return textureColor;
}

technique OceanDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};
