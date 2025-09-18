// Ideally you wouldn't need half these includes for an unlit shader
// But it's stupiod

FEATURES
{
    #include "common/features.hlsl"
}

MODES
{
    Forward();
    Depth();
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
}

struct VertexInput
{
	#include "common/vertexinput.hlsl"
};

struct PixelInput
{
	#include "common/pixelinput.hlsl"
};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( VertexInput i )
	{
		PixelInput o = ProcessVertex( i );
		// Add your vertex manipulation functions here
		return FinalizeVertex( o );
	}
}

PS
{
    #include "common/pixel.hlsl"

	CreateTexture2D(g_tPanel) < Attribute("Panel"); SrgbRead(true); >;

	RenderState( CullMode, NONE ); // TODO: Make this toggleable

	float4 MainPs( PixelInput i ) : SV_Target0
	{
		float4 col = g_tPanel.Sample(g_sTrilinearClamp, i.vTextureCoords.xy).rgba;
		return col;
	}
}
