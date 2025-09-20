using Editor;
using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VividPanels;

internal class Test
{
	[EditorEvent.Frame]
	internal static void Tick()
	{
		if ( Game.IsPlaying )
		{
			var system = VividRenderSystem.Current;
			var t = system._commands.GetType().GetMethod( "AddAction", BindingFlags.Instance | BindingFlags.NonPublic );
			system._commands.Reset();
			
			Action param = delegate
			{
				var panels = Game.ActiveScene.GetAllComponents<VividPanel>().ToList();

				panels.Sort( ( a, b ) =>
				{
					float dist0 = a.WorldPosition.DistanceSquared( Game.ActiveScene.Camera.WorldPosition );
					float dist1 = b.WorldPosition.DistanceSquared( Game.ActiveScene.Camera.WorldPosition );

					if ( dist0.AlmostEqual( dist1, 0.1f ) )
						return 0;

					if ( dist0 < dist1 )
						return 1;

					return -1;
				} );


				foreach ( var panel in panels )
				{
					var attributes = system._commands.Attributes;
					attributes.Set( "Panel", panel.Texture );

					Graphics.Attributes.SetCombo( StringToken.Literal( "D_WORLDPANEL", 3066976377u ), 1 );
					Matrix value = Matrix.CreateRotation( Rotation.From( 0f, 90f, 90f ) );
					value *= Matrix.CreateScale( 0.05f );
					value *= Matrix.CreateRotation( panel.WorldRotation );
					value *= Matrix.CreateTranslation( panel.WorldPosition );
					value *= Matrix.CreateScale( panel.WorldScale * panel.RenderScale );

					Graphics.Attributes.Set( StringToken.Literal( "WorldMat", 751663081u ), in value );
					Rect panelBounds = panel.CalculateRect();
					panelBounds.Left /= panel.RenderScale;
					panelBounds.Right /= panel.RenderScale;
					panelBounds.Top /= panel.RenderScale;
					panelBounds.Bottom /= panel.RenderScale;
					panel._rootPanel.PanelBounds = panelBounds;
					panel._rootPanel?.RenderManual();

					//attributes.SetCombo( "D_WORLDPANEL", 1 );
					//Matrix value = Matrix.CreateRotation( Rotation.From( 0f, 90f, 90f ) );
					//value *= Matrix.CreateScale( 0.05f );
					//attributes.Set( "WorldMat", value );

					//panel._rootPanel.RenderManual();

					//Graphics.Attributes.SetCombo( StringToken.Literal( "D_WORLDPANEL", 3066976377u ), 1 );
					//Matrix value = Matrix.CreateRotation( Rotation.From( 0f, 90f, 90f ) );
					//value *= Matrix.CreateScale( 0.05f );
					//Graphics.Attributes.Set( StringToken.Literal( "WorldMat", 751663081u ), in value );
					//panel._rootPanel?.RenderManual();

					//_commands.DrawRenderer

					//if ( panel.Texture.IsValid() )
					//	_commands.Draw( panel.VertexBuffer, Material.Load( "materials/vivid_panel.vmat" ), 0, panel.VertexCount );

					//var renderer = panel.GetComponent<TestPanelRenderer>();
					//if ( renderer.IsValid() )
					//{
					//	renderer._rootPanel.SceneObject.RenderingEnabled = true;
					//	_commands.DrawRenderer( renderer );
					//	//renderer._rootPanel.SceneObject.RenderingEnabled = false;
					//}
				}

			};
			t.Invoke( system._commands, new[]
			{
				param
			} );

			
		}
	}
}
