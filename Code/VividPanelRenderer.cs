using Sandbox;
using System.Linq;
using VividPanels;

namespace Blocks;

[Icon("preview")]
[Category("UI")]
internal class VividPanelRenderer : Component, CameraComponent.ISceneCameraSetup
{
	public void SetupCamera( CameraComponent camera, SceneCamera sceneCamera )
	{
		sceneCamera.OnRenderOverlay += () =>
		{
			if ( !Game.IsPlaying )
				return;

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
				Graphics.Attributes.SetCombo( "D_WORLDPANEL", 1 );
				Matrix value = Matrix.CreateRotation( Rotation.From( 0f, 90f, 90f ) );
				value *= Matrix.CreateScale( Sandbox.UI.WorldPanel.ScreenToWorldScale * panel.WorldScale * panel.WorldRenderScale );
				value *= Matrix.CreateRotation( panel.WorldRotation );
				value *= Matrix.CreateTranslation( panel.WorldPosition );

				var scale = panel.WorldRenderScale;

				Graphics.Attributes.Set( "WorldMat", in value );
				Rect panelBounds = panel.CalculateRect();
				panelBounds.Left /= scale;
				panelBounds.Right /= scale;
				panelBounds.Top /= scale;
				panelBounds.Bottom /= scale;
				panel.RootPanel.PanelBounds = panelBounds;
				panel.RootPanel?.RenderManual();
			}
		};
	}
}
