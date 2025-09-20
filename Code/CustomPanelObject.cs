using Sandbox;
using Sandbox.Internal;
using Sandbox.UI;
using System;

namespace VividPanels;

[Description( "Renders a panel in a scene world. You are probably looking for <a>WorldPanel</a>." )]
internal sealed class CustomPanelObject : SceneCustomObject
{
	public const float ScreenToWorldScale = 0.05f;

	public RootPanel Panel { get; private set; }

	public CustomPanelObject( SceneWorld world, RootPanel Panel )
		: base( world )
	{
		this.Panel = Panel;
	}

	public override void RenderSceneObject()
	{
		Graphics.Attributes.SetCombo( StringToken.Literal( "D_WORLDPANEL", 3066976377u ), 1 );
		Matrix value = Matrix.CreateRotation( Rotation.From( 0f, 90f, 90f ) );
		value *= Matrix.CreateScale( 0.05f );
		Graphics.Attributes.Set( StringToken.Literal( "WorldMat", 751663081u ), in value );
		Panel?.RenderManual();
	}
}
