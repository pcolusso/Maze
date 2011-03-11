using System;
using OpenTK;
using OpenTK.Graphics;
namespace Maze
{
	public class Start : GameWindow
	{
		IState Game;
		
		public Start ()
			: base(1200, 700, new GraphicsMode(32, 22, 0, 0), "Maze")
		{
			Game = new GameState();
		}
		
		protected override void OnLoad (EventArgs e)
		{
			base.OnLoad (e);
			Game.Load(ClientRectangle);
		}
		
		protected override void OnResize (EventArgs e)
		{
			base.OnResize (e);
		}
		
		protected override void OnUpdateFrame (FrameEventArgs e)
		{
			base.OnUpdateFrame (e);
			Game.Update();
		}
		
		protected override void OnRenderFrame (FrameEventArgs e)
		{
			base.OnRenderFrame (e);
			Game.Draw();
			SwapBuffers();
		}
		
		public static void Main()
		{
			new Start().Run();
		}
	}
}

