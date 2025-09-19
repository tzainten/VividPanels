using Sandbox;
using Sandbox.Rendering;
using Sandbox.UI;
using System.Collections.Generic;

namespace VividPanels;

internal class VividRootPanel : RootPanel
{
	protected override void UpdateScale( Rect screenSize )
	{
		Scale = 2f;
	}

	protected override void UpdateBounds( Rect rect )
	{
		base.UpdateBounds( rect * Scale );
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

	[Property] internal float RenderScale { get; set; } = 1f;
	[Property] internal bool LookAtCamera { get; set; } = false;
	[Property] internal Vector2Int PanelSize { get; set; } = 512;
	[Property] internal HAlignment HorizontalAlign { get; set; } = HAlignment.Center;
	[Property] internal VAlignment VerticalAlign { get; set; } = VAlignment.Center;

	VividRootPanel _rootPanel;
	PanelComponent _source;

	Texture _texture;
	CommandList _commands;
	SceneCustomObject _renderObject;

	bool _hasRendered = false;
	int _vertexCount;
	GpuBuffer<Vertex> _vertexBuffer;

	protected override void OnEnabled()
	{
		base.OnEnabled();

		_commands = new CommandList( $"VividPanel_{GetHashCode()}" );
		Scene.Camera.AddCommandList( _commands, Stage.AfterPostProcess );
	}

	protected override void OnStart()
	{
		base.OnStart();

		_rootPanel = new VividRootPanel
		{
			RenderedManually = true,
			Scene = Scene,
			IsWorldPanel = true,
		};

		_renderObject = new( Scene.SceneWorld )
		{
			RenderOverride = OnRender
		};

		_source = GetComponent<PanelComponent>();
	}

	Vector2Int _previousPanelSize;
	Vector3 _previousPosition;
	Rotation _previousRotation;

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( WorldPosition != _previousPosition || WorldRotation != _previousRotation || PanelSize != _previousPanelSize )
		{
			_previousPosition = WorldPosition;
			_previousRotation = WorldRotation;
			_previousPanelSize = PanelSize;

			CreateVertexBuffer();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		_renderObject?.Delete();
		_renderObject = null;

		_rootPanel?.Delete();
		_rootPanel = null;
	}

	private void OnRender( SceneObject sceneObject )
	{
		if ( !_rootPanel.IsValid() )
			return;

		if ( !_rootPanel.RenderedManually )
		{
			_rootPanel.RenderedManually = true;
		}

		if (
			_source.IsValid()
			&& _source.Panel.IsValid()
			&& _source.Panel.Parent != _rootPanel
		)
		{
			_source.Panel.Parent = _rootPanel;
		}

		if ( _source.IsValid() && _source.Panel.IsValid() )
		{
			if ( _source.Panel.Parent != _rootPanel )
			{
				_source.Panel.Parent = _rootPanel;
			}
		}

		var scaledSize = (Vector2Int)(PanelSize / RenderScale);
		if ( _texture is null || _texture.Size != scaledSize )
		{
			CreateTexture( scaledSize );
			return;
		}

		Graphics.RenderTarget = RenderTarget.From( _texture );
		Graphics.Attributes.SetCombo( "D_WORLDPANEL", 0 );
		Graphics.Viewport = new Rect( 0, _rootPanel.PanelBounds.Size );
		Graphics.Clear();

		_rootPanel.RenderManual();
		_hasRendered = true;

		Graphics.RenderTarget = null;
	}

	private void CreateTexture( Vector2 size )
	{
		_hasRendered = false;
		_texture?.Dispose();
		_texture = Texture.CreateRenderTarget()
			.WithSize( size )
			.WithDynamicUsage()
			.WithUAVBinding()
			.Create();
	}

	private Rect CalculateRect()
	{
		Rect result = new Rect( 0, PanelSize );
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
		if ( VerticalAlign == VAlignment.Top )
		{
			result.Position -= new Vector2( 0f, PanelSize.y );
		}
		return result;
	}

	protected override void DrawGizmos()
	{
		using ( Gizmo.Scope( null, new Transform( 0f, Rotation.Identity, Sandbox.UI.WorldPanel.ScreenToWorldScale ) ) )
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
		var rotation = WorldRotation;
		var position = WorldPosition;

		var scale = Sandbox.UI.WorldPanel.ScreenToWorldScale;

		Rect rect = CalculateRect();
		List<Vertex> vertices =
		[
			new Vertex( position + rotation * ((Vector3)rect.TopLeft * scale), rotation.Forward, rotation.Up, new Vector4( 0, 1, 0, 0 ) ),
			new Vertex( position + rotation * ((Vector3)rect.TopRight * scale), rotation.Forward, rotation.Up, new Vector4( 1, 1, 0, 0 ) ),
			new Vertex( position + rotation * ((Vector3)rect.BottomRight * scale), rotation.Forward, rotation.Up, new Vector4( 1, 0, 0, 0 ) ),

			new Vertex( position + rotation * ((Vector3)rect.BottomRight * scale), rotation.Forward, rotation.Up, new Vector4( 1, 0, 0, 0 ) ),
			new Vertex( position + rotation * ((Vector3)rect.BottomLeft * scale), rotation.Forward, rotation.Up, new Vector4( 0, 0, 0, 0 ) ),
			new Vertex( position + rotation * ((Vector3)rect.TopLeft * scale), rotation.Forward, rotation.Up, new Vector4( 0, 1, 0, 0 ) ),
		];

		_vertexCount = vertices.Count;
		_vertexBuffer = new GpuBuffer<Vertex>( _vertexCount, GpuBuffer.UsageFlags.Vertex );
		_vertexBuffer.SetData( vertices );
	}

	protected override void OnPreRender()
	{
		base.OnPreRender();

		_commands.Reset();

		var attributes = _commands.Attributes;
		attributes.Set( "Panel", _texture );

		if ( _hasRendered )
			_commands.Draw( _vertexBuffer, Material.Load( "materials/vivid_panel.vmat" ), 0, _vertexCount );
	}

	protected override void OnDisabled()
	{
		base.OnDisabled();

		Scene.Camera.RemoveCommandList( _commands );
		_commands = null;
	}
}
