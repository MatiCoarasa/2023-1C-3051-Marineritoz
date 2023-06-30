#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 World;
float4x4 View;
float4x4 Projection;

float3 DiffuseColor;
float3 ambientColor;
float3 diffuseColor;
float3 specularColor;
float KAmbient; 
float KDiffuse; 
float KSpecular;
float shininess; 
float3 lightPosition;
float3 eyePosition;

float Time = 0;

texture baseTexture;
sampler2D textureSampler = sampler_state
{
    Texture = (baseTexture);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

texture environmentMap;
samplerCUBE environmentMapSampler = sampler_state
{
    Texture = (environmentMap);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Normal: NORMAL;
    float2 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
    float4 WorldPosition : TEXCOORD1;
    float2 TextureCoordinates : TEXCOORD0;
    float3 Normal : TEXCOORD2;    
};

static const float PI = 3.14159265f;

float3 GerstnerWave (float4 wave, float3 position, inout float3 tangent, inout float3 binormal) {
    float steepness = wave.z;
    float waveLength = wave.w;
    float2 direction = normalize(wave.xy);
	// float speed = 1.5;
	float k = 2 * PI / waveLength;
	float speed = sqrt(9.8 / k);
	float waveAmplitude = steepness / k;
	float frequency = k * (dot(direction, position.xz) - speed * Time);
	
	tangent += float3(
	    -direction.x * direction.x * (steepness * sin(frequency)),
        direction.x * (steepness * cos(frequency)),
        -direction.x * direction.y * (steepness * sin(frequency))
    );
    
	binormal += float3(
        -direction.x * direction.y * (steepness * sin(frequency)),
        direction.y * (steepness * cos(frequency)),
        -direction.y * direction.y * (steepness * sin(frequency))
	);

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
	float3 tangent = float3(1, 0, 0);
    float3 binormal = float3(0, 0, 1);
	float4 wave1 = Wave(float2(1,1),0.1, 20);
	float4 wave2 = Wave(float2(1,0.6),0.1,10);
	float4 wave3 = Wave(float2(1,1.3),0.1,5);
    float4 wave4 = Wave(float2(1,1.3),0.01,1);
    worldPosition.xyz += GerstnerWave(wave1, anchorPoint, tangent, binormal);
    worldPosition.xyz += GerstnerWave(wave2, anchorPoint, tangent, binormal);
    worldPosition.xyz += GerstnerWave(wave3, anchorPoint, tangent, binormal);
    worldPosition.xyz += GerstnerWave(wave4, anchorPoint, tangent, binormal);
    float3 normal = normalize(cross(binormal, tangent));
    
    // Final World Position of Wave
    output.WorldPosition = worldPosition;
    // WorldViewProjection
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    // Final Normal of Wave
    output.Normal = normal;
    output.TextureCoordinates = input.TextureCoordinates;

    return output;
}

VertexShaderOutput MainNormalVS(in VertexShaderInput input)
{
    // Clear the output
	VertexShaderOutput output = (VertexShaderOutput)0;
	// Model space to World space
    float4 worldPosition = mul(input.Position, World);
	float3 tangent = float3(1, 0, 0);
    float3 binormal = float3(0, 0, 1);
    float3 normal = normalize(cross(binormal, tangent));
    output.WorldPosition = worldPosition;
    // WorldViewProjection
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    // Final Normal of Wave
    output.Normal = normal;
    output.TextureCoordinates = input.TextureCoordinates;
    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    // Base vectors
    float3 lightDirection = normalize(lightPosition - input.WorldPosition.xyz);
    float3 viewDirection = normalize(eyePosition - input.WorldPosition.xyz);
    float3 halfVector = normalize(lightDirection + viewDirection);
    
	// Calculate the diffuse light
    float NdotL = saturate(dot(input.Normal, lightDirection));
    float3 diffuseLight = KDiffuse * diffuseColor * NdotL;

	// Calculate the specular light
    float NdotH = dot(input.Normal, halfVector);
    float3 specularLight = sign(NdotL) * KSpecular * specularColor * pow(saturate(NdotH), shininess);
    
    // Final calculation
    float4 finalColor = float4(saturate(ambientColor * KAmbient + diffuseLight) * DiffuseColor + specularLight, 0.6);
    return finalColor;
}

float4 EnvironmentMapWithLightPS(VertexShaderOutput input) : COLOR
{
   // Base vectors
    float3 lightDirection = normalize(lightPosition - input.WorldPosition.xyz);
    float3 viewDirection = normalize(eyePosition - input.WorldPosition.xyz);
    float3 halfVector = normalize(lightDirection + viewDirection);
    
    // Calculate the diffuse light
    float NdotL = saturate(dot(input.Normal, lightDirection));
    float3 diffuseLight = KDiffuse * diffuseColor * NdotL;

    // Calculate the specular light
    float NdotH = dot(input.Normal, halfVector);
    float3 specularLight = sign(NdotL) * KSpecular * specularColor * pow(saturate(NdotH), shininess);
            
    // Final calculation
    float4 finalColor = float4(saturate(ambientColor * KAmbient + diffuseLight) * DiffuseColor + specularLight, 1);

	//Obtener texel de CubeMap
	float3 reflection = reflect(viewDirection, input.Normal);
	float3 reflectionColor = texCUBE(environmentMapSampler, reflection).rgb;

    return float4(lerp(finalColor, reflectionColor, 0.2), 1);
}

float4 EnvironmentMapPS(VertexShaderOutput input) : COLOR
{
	//Normalizar vectores
	float3 normal = normalize(input.Normal.xyz);
    
	//Obtener texel de CubeMap
	float3 view = normalize(eyePosition.xyz - input.WorldPosition.xyz);
	float3 reflection = reflect(view, normal);
	float3 reflectionColor = texCUBE(environmentMapSampler, reflection).rgb;

    return float4(normal, 1);
}

technique OceanDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};

technique EnvironmentMapDrawing
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL EnvironmentMapWithLightPS();
    }
};
