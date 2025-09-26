using Blocks;
using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;

namespace VividPanels;

public class VividPanelSystem : GameObjectSystem
{
	public VividPanelSystem( Scene scene ) : base( scene )
	{
		Listen( Stage.StartUpdate, 0, StartUpdate, "StartUpdate" );
	}

	void StartUpdate()
	{
		if ( !Game.IsPlaying || Scene.IsEditor )
			return;

		if ( !Scene.Camera.IsValid() )
			return;

		if ( !Scene.Camera.GetComponent<VividPanelRenderer>().IsValid() )
			Scene.Camera.AddComponent<VividPanelRenderer>();
	}
}

public class VividRootPanel : RootPanel
{
	internal Transform Transform;

	internal float MaxInteractionDistance;

	protected override void UpdateScale( Rect screenSize )
	{
		Scale = 2f;
	}

	protected override void UpdateBounds( Rect rect )
	{
		base.UpdateBounds( PanelBounds );
	}

	public override bool RayToLocalPosition( Ray ray, out Vector2 position, out float distance )
	{
		position = default( Vector2 );
		distance = 0f;
		Vector3? vector = new Plane( Transform.Position, Transform.Rotation.Forward ).Trace( in ray, twosided: false, MaxInteractionDistance );
		if ( !vector.HasValue )
		{
			return false;
		}
		distance = Vector3.DistanceBetween( vector.Value, ray.Position );
		if ( distance < 1f )
		{
			return false;
		}
		Vector3 vector2 = Transform.PointToLocal( vector.Value );
		Vector2 vector3 = new Vector2( vector2.y, 0f - vector2.z );
		vector3 *= 20f;
		if ( !IsInside( vector3 ) )
		{
			return false;
		}
		position = vector3;
		return true;
	}
}

[Title( "Vivid Panel" )]
[Category( "UI" )]
[Icon( "panorama_horizontal" )]
[EditorHandle( "materials/gizmo/ui.png" )]
public class VividPanel : Component
{
	public enum HAlignment
	{
		[Icon( "align_horizontal_left" )]
		Left = 1,
		[Icon( "align_horizontal_center" )]
		Center,
		[Icon( "align_horizontal_right" )]
		Right
	}

	public enum VAlignment
	{
		[Icon( "align_vertical_top" )]
		Top = 1,
		[Icon( "align_vertical_center" )]
		Center,
		[Icon( "align_vertical_bottom" )]
		Bottom
	}

	internal float WorldRenderScale = 1f;

	[Property, Change( "CreateVertexBuffer" )] public float RenderScale { get; set; } = 1f;
	[Property] public bool RenderBackFace { get; set; } = true;
	[Property, Change( "CreateVertexBuffer" )] public bool LookAtCamera { get; set; } = false;
	[Property, ShowIf( "LookAtCamera", true ), Change( "CreateVertexBuffer" )] public bool ConsistentSize { get; set; } = false;
	[Property, Change( "CreateVertexBuffer" )] public Vector2Int PanelSize { get; set; } = 512;
	[Property, Change( "CreateVertexBuffer" )] public HAlignment HorizontalAlign { get; set; } = HAlignment.Center;
	[Property, Change( "CreateVertexBuffer" )] public VAlignment VerticalAlign { get; set; } = VAlignment.Center;
	[Property, Change( "CreateVertexBuffer" )] public float InteractionRange { get; set; } = 1000f;

	public VividRootPanel RootPanel;
	PanelComponent _source;

	public Texture Texture;
	public int VertexCount;
	public GpuBuffer<Vertex> VertexBuffer;

	protected override void OnStart()
	{
		base.OnStart();

		RootPanel = new VividRootPanel
		{
			RenderedManually = true,
			Scene = Scene,
			IsWorldPanel = true,
		};

		_source = GetComponent<PanelComponent>();

		CreateVertexBuffer();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( LookAtCamera )
		{
			WorldRotation = Rotation.LookAt( Scene.Camera.WorldRotation.Backward, Vector3.Up );
		}

		if ( !RootPanel.IsValid() )
			return;

		if ( !RootPanel.RenderedManually )
		{
			RootPanel.RenderedManually = true;
		}

		if (
			_source.IsValid()
			&& _source.Panel.IsValid()
			&& _source.Panel.Parent != RootPanel
		)
		{
			_source.Panel.Parent = RootPanel;
		}

		if ( _source.IsValid() && _source.Panel.IsValid() )
		{
			if ( _source.Panel.Parent != RootPanel )
			{
				_source.Panel.Parent = RootPanel;
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		RootPanel?.Delete();
		RootPanel = null;
	}

	public Rect CalculateRect()
	{
		Rect result = new Rect( (Vector2)0.0, PanelSize );
		if ( HorizontalAlign == HAlignment.Center )
		{
			result.Position -= new Vector2( PanelSize.x * 0.5f, 0f );
		}
		if ( HorizontalAlign == HAlignment.Right )
		{
			result.Position -= new Vector2( PanelSize.x, 0f );
		}
		if ( VerticalAlign == VAlignment.Center )
		{
			result.Position -= new Vector2( 0f, PanelSize.y * 0.5f );
		}
		if ( VerticalAlign == VAlignment.Bottom )
		{
			result.Position -= new Vector2( 0f, PanelSize.y );
		}

		var camera = Scene.Camera;

		var scale = 1f;
		WorldRenderScale = RenderScale;
		if ( camera.IsValid() && Game.IsPlaying && LookAtCamera && ConsistentSize )
		{
			float depth = Vector3.Dot( WorldPosition - Scene.Camera.WorldPosition, Scene.Camera.WorldRotation.Forward );
			if ( depth > 0 )
			{
				scale = depth / 100f;

				WorldRenderScale = RenderScale * scale;
				result *= scale;
			}
		}

		return result;
	}

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

	void CreateVertexBuffer()
	{
		var rotation = WorldRotation * Rotation.From( 0f, 90f, -90f );
		var position = WorldPosition;

		var scale = Sandbox.UI.WorldPanel.ScreenToWorldScale;
		float dist = Vector3.DistanceBetween( Scene.Camera.WorldPosition, WorldPosition );

		Rect rect = CalculateRect();
		List<Vertex> vertices =
		[
			new Vertex( position + rotation * ((Vector3)rect.TopLeft * scale), rotation.Forward, rotation.Right, new Vector4( 0, 0, 0, 0 ) ),
			new Vertex( position + rotation * ((Vector3)rect.BottomLeft * scale), rotation.Forward, rotation.Right, new Vector4( 0, 1, 0, 0 ) ),
			new Vertex( position + rotation * ((Vector3)rect.BottomRight * scale), rotation.Forward, rotation.Right, new Vector4( 1, 1, 0, 0 ) ),

			new Vertex( position + rotation * ((Vector3)rect.BottomRight * scale), rotation.Forward, rotation.Right, new Vector4( 1, 1, 0, 0 ) ),
			new Vertex( position + rotation * ((Vector3)rect.TopRight * scale), rotation.Forward, rotation.Right, new Vector4( 1, 0, 0, 0 ) ),
			new Vertex( position + rotation * ((Vector3)rect.TopLeft * scale), rotation.Forward, rotation.Right, new Vector4( 0, 0, 0, 0 ) ),
		];

		VertexCount = vertices.Count;
		VertexBuffer = new GpuBuffer<Vertex>( VertexCount, GpuBuffer.UsageFlags.Vertex );
		VertexBuffer.SetData( vertices );

		RootPanel.Transform = Transform.World.WithScale( WorldScale * WorldRenderScale );
		RootPanel.MaxInteractionDistance = InteractionRange;
	}
}
