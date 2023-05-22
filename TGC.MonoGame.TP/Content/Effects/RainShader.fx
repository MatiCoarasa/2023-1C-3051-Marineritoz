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
float3 CameraPosition;
uniform float Time;
uniform float MaxHeight;
uniform float MinHeight;
uniform float Speed;


struct VertexShaderInput
{
    float4 LocalPosition : POSITION0;
};

struct VertexShaderOutput
{
    float4 ProjectedPosition : SV_POSITION;
    float4 Position : TEXCOORD1;
};

struct InstanceInput
{
    float3 Offset : POSITION1;
};




VertexShaderOutput MainVS(in InstanceInput instance, in VertexShaderInput input)
{
    // Clear the output
    VertexShaderOutput output = (VertexShaderOutput)0;
    // Model space to World space

    
    float4 worldPosition;
    
    worldPosition.x = input.LocalPosition.x + instance.Offset.x + CameraPosition.x;
    worldPosition.y = input.LocalPosition.y + instance.Offset.y;
    worldPosition.z = input.LocalPosition.z + instance.Offset.z + CameraPosition.z;
    worldPosition.w = 1;
    
    float4 worldPositionModified = float4(worldPosition.x, lerp(worldPosition.y, MinHeight, frac(Time * 2 * Speed + worldPosition.y)), worldPosition.zw);
    
    
    float3 normalVector = normalize(CameraPosition);
    float3 cameraRight = float3(View[0][0],View[1][0],View[2][0]);
    float3 cameraUp = float3(0, 1, 0);

    // World space to View space
    float4 viewPosition = mul(worldPositionModified, View);
	// View space to Projection space
    output.ProjectedPosition = mul(viewPosition, Projection);
    
    output.Position = input.LocalPosition;
    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    
    return float4(DiffuseColor,0.5);

}

technique BasicColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};