using Sandbox;
using Sandbox.Rendering;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VividPanels;

internal class TestPanelRenderer : Renderer
{
	internal MyRootPanel _rootPanel;

	PanelComponent _source;

	protected override void OnStart()
	{
		base.OnStart();

		_rootPanel = new MyRootPanel( Scene.SceneWorld );

		_source = GetComponent<PanelComponent>();
	}

	[Property] internal float RenderScale { get; set; } = 1f;
	[Property] internal Vector2 PanelSize { get; set; } = 512f;

	protected override void DrawGizmos()
	{
		using ( Gizmo.Scope( null, new Transform( 0f, Rotation.From( 0f, 90f, -90f ), Sandbox.UI.WorldPanel.ScreenToWorldScale ) ) )
		{
			Rect rect = CalculateRect();
			Gizmo.Draw.Line( (Vector3)rect.TopLeft, (Vector3)rect.TopRight );
			Gizmo.Draw.Line( (Vector3)rect.TopLeft, (Vector3)rect.BottomLeft );
			Gizmo.Draw.Line( (Vector3)rect.TopRight, (Vector3)rect.BottomRight );
			Gizmo.Draw.Line( (Vector3)rect.BottomLeft, (Vector3)rect.BottomRight );
			Gizmo.Draw.Color = Color.Cyan.WithAlpha( 0.2f );
			Gizmo.Draw.SolidTriangle( new Triangle( rect.TopLeft, rect.TopRight, rect.BottomRight ) );
			Gizmo.Draw.SolidTriangle( new Triangle( rect.BottomRight, rect.BottomLeft, rect.TopLeft ) );
		}
	}

	private Rect CalculateRect()
	{
		Rect result = new Rect( (Vector2)0.0, PanelSize );
		//if ( HorizontalAlign == HAlignment.Center )
		//{
		//	result.Position -= new Vector2( PanelSize.x * 0.5f, 0f );
		//}
		//if ( HorizontalAlign == HAlignment.Right )
		//{
		//	result.Position -= new Vector2( PanelSize.x, 0f );
		//}
		//if ( VerticalAlign == VAlignment.Center )
		//{
		//	result.Position -= new Vector2( 0f, PanelSize.y * 0.5f );
		//}
		//if ( VerticalAlign == VAlignment.Bottom )
		//{
		//	result.Position -= new Vector2( 0f, PanelSize.y );
		//}
		return result;
	}

	protected override void OnPreRender()
	{
		base.OnPreRender();

		if ( _rootPanel.IsValid() )
		{
			if ( _source.IsValid() )
				_source.Panel.Parent = _rootPanel;

			Rotation rotation = base.WorldRotation;
			Vector3 worldScale = base.WorldScale;
			//if ( LookAtCamera && base.Scene.Camera != null )
			//{
			//	rotation = Rotation.LookAt( base.Scene.Camera.WorldPosition - base.WorldPosition, base.Scene.Camera.WorldRotation.Up );
			//}
			_rootPanel.Transform = base.WorldTransform.WithRotation( in rotation ).WithScale( worldScale * RenderScale );
			Rect panelBounds = CalculateRect();
			panelBounds.Left /= RenderScale;
			panelBounds.Right /= RenderScale;
			panelBounds.Top /= RenderScale;
			panelBounds.Bottom /= RenderScale;
			_rootPanel.PanelBounds = panelBounds;
		}
	}

	//protected override void OnUpdate()
	//{
	//	base.OnUpdate();

	//	if ( _source.IsValid() )
	//		_source.Panel.Parent = _rootPanel;

	//	SceneObject.Transform = WorldTransform;
	//}
}
