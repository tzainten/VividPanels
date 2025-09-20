using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sandbox.VertexLayout;

namespace VividPanels;

internal class MyRootPanel : RootPanel
{
	internal CustomPanelObject SceneObject;

	public Transform Transform
	{
		get
		{
			return SceneObject.Transform;
		}
		set
		{
			SceneObject.Transform = value;
		}
	}

	public Vector3 Position
	{
		get
		{
			return Transform.Position;
		}
		set
		{
			Transform = Transform.WithPosition( in value );
		}
	}

	public Rotation Rotation
	{
		get
		{
			return Transform.Rotation;
		}
		set
		{
			Transform = Transform.WithRotation( in value );
		}
	}

	public float WorldScale
	{
		get
		{
			return Transform.UniformScale;
		}
		set
		{
			Transform = Transform.WithScale( value );
		}
	}

	public MyRootPanel( SceneWorld world )
	{
		SceneObject = new CustomPanelObject( world, this );
		SceneObject.Flags.IsOpaque = false;
		SceneObject.Flags.IsTranslucent = true;
		base.RenderedManually = true;
		base.PanelBounds = new Rect( 0, 0, 1000f, 1000f );
		base.Scale = 2f;
		//MaxInteractionDistance = 1000f;
		IsWorldPanel = true;
	}

	protected override void UpdateBounds( Rect rect )
	{
		if ( SceneObject.IsValid() )
		{
			Vector3 right = Rotation.Right;
			Vector3 down = Rotation.Down;
			Rect rect2 = base.PanelBounds * WorldScale * 0.05f;
			BBox bBox = BBox.FromPositionAndSize( right * rect2.Left + down * rect2.Top ).AddPoint( right * rect2.Left + down * rect2.Bottom ).AddPoint( right * rect2.Right + down * rect2.Top )
				.AddPoint( right * rect2.Right + down * rect2.Bottom );
			SceneObject.Bounds = bBox + Position;
		}
	}

	protected override void UpdateScale( Rect screenSize )
	{
	}
}
