
HEADER
{
	Description = "";
}

FEATURES
{
	#include "common/features.hlsl"
}

MODES
{
	Forward();
	Depth();
	ToolsShadingComplexity( "tools_shading_complexity.shader" );
}

COMMON
{
	#ifndef S_ALPHA_TEST
	#define S_ALPHA_TEST 0
	#endif
	#ifndef S_TRANSLUCENT
	#define S_TRANSLUCENT 1
	#endif
	
	#include "common/shared.hlsl"
	#include "procedural.hlsl"

	#define S_UV2 1

	float4 g_vViewport;                   // xy = viewport min, zw = viewport size
	float4x4 g_matTransform;              // transform for the panel
	float4x4 LayerMat;                    // layer matrix
	float4x4 g_matWorldPanel;             // world panel transform
}

struct VertexInput
{
	#include "common/vertexinput.hlsl"
	float4 vColor : COLOR0 < Semantic( Color ); >;
};

struct PixelInput
{
	#include "common/pixelinput.hlsl"
	float3 vPositionOs : TEXCOORD14;
	float3 vNormalOs : TEXCOORD15;
	float4 vTangentUOs_flTangentVSign : TANGENT	< Semantic( TangentU_SignV ); >;
	float4 vColor : COLOR0;
	float4 vTintColor : COLOR1;
	#if ( PROGRAM == VFX_PROGRAM_PS )
		bool vFrontFacing : SV_IsFrontFace;
	#endif
};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( VertexInput v )
	{
		PixelInput i;



		float3 vPositionSs = v.vPositionOs.xyz; // treat incoming vertex as SS quad pos

		#ifdef D_WORLDPANEL
			float4 vMatrix = mul(LayerMat, mul(g_matTransform, float4(vPositionSs.xyz, 1)));
			vPositionSs.xyz = vMatrix.xyz / vMatrix.w;

			float3x4 matObjectToWorld = GetTransformMatrix(v.nInstanceTransformID);
			matObjectToWorld = mul(matObjectToWorld, g_matWorldPanel);

			float4 vPositionWs = mul(matObjectToWorld, float4(vPositionSs, 1));
			i.vPositionWs = vPositionWs.xyz;
			i.vPositionPs = Position3WsToPs(vPositionWs.xyz);

			i.vTextureCoords.xy = vPositionSs.xy / g_vViewport.zw;
		#else
// Transform from panel-space -> clip-space
		float4 vMatrix = mul(LayerMat, mul(g_matTransform, float4(vPositionSs.xy, 0, 1)));
		vPositionSs.xy = vMatrix.xy / vMatrix.w;

		float4 positionPs;
		positionPs.xy = 2.0 * (vPositionSs.xy - g_vViewport.xy) / (g_vViewport.zw) - float2(1.0, 1.0);
		positionPs.y *= -1.0;
		positionPs.z = 1.0;
		positionPs.w = 1.0;
		i.vPositionPs = positionPs;

		// Pass through UVs (normalized against viewport)
		i.vTextureCoords.xy = vPositionSs.xy / g_vViewport.zw;

		// Copy other data you need
		i.vColor = v.vColor;
		i.vTintColor = GetExtraPerInstanceShaderData(v.nInstanceTransformID).vTint;
		#endif

		

		return i;
	}
}

PS
{
	#include "common/pixel.hlsl"

	CreateTexture2D(g_tPanel) < Attribute("Panel"); SrgbRead(true); >;

	RenderState( CullMode, NONE ); // TODO: Make this toggleable

	RenderState( BlendEnable, true );
	RenderState( SrcBlend, SRC_ALPHA );
	RenderState( DstBlend, INV_SRC_ALPHA );

	float4 MainPs( PixelInput i ) : SV_Target0
	{
		float4 col = g_tPanel.Sample(g_sAnsio, i.vTextureCoords.xy).rgba;

		return col;
	}
}