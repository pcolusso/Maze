using System;
using OpenTK;
using OpenTK.Graphics;
namespace Maze
{
	public class Start : GameWindow
	{
		IState Game, Menu;
		
		public Start ()
			: base(1200, 700, new GraphicsMode(32, 22, 0, 0), "Maze")
		{
			Game = new GameState();
			Menu = new MenuState();
		}
		
		protected override void OnLoad (EventArgs e)
		{
			base.OnLoad (e);
			Menu.Load(ClientRectangle);
		}
		
		protected override void OnResize (EventArgs e)
		{
			base.OnResize (e);
		}
		
		protected override void OnUpdateFrame (FrameEventArgs e)
		{
			base.OnUpdateFrame (e);
			Menu.Update();
		}
		
		protected override void OnRenderFrame (FrameEventArgs e)
		{
			base.OnRenderFrame (e);
			Menu.Draw();
			SwapBuffers();
		}
		
		public static void Main()
		{
			new Start().Run();
		}
	}
}

