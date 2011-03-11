using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Maze
{
	
    class View : GameWindow
    {
        bool RotateCamera = false;
        DepthFirstMaze labrynth;
        Matrix4 Projection, ModelView;
		VBO2d maze2d;
		Vector3 cam;
		bool Alive;

        public View()
            : base(1200, 700, new GraphicsMode(32, 22, 0, 0), "Maze")
        {
			Alive = true;

            labrynth = new DepthFirstMaze(99,99);
            
            if (RotateCamera)
			    new Thread(new ThreadStart(SpinPlane)).Start();

			cam = new Vector3(0, 0, 8);
        }

        protected override void OnLoad(EventArgs e)
        {
            GL.ClearColor(Color.CornflowerBlue);
            base.OnLoad(e);

            GL.Viewport(0, 0, ClientRectangle.Width, ClientRectangle.Height);
            //Projection = Matrix4.CreateOrthographic(ClientRectangle.Width, ClientRectangle.Height, 1, 100);
			Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1, 1, 50);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref Projection);
			
			LoadMazeIntoVBO();

            
			
			//GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            
            GL.Viewport(0, 0, ClientRectangle.Width, ClientRectangle.Height);
            Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, ClientRectangle.Width / ClientRectangle.Height, 1, 50);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref Projection);
        }
		
		protected override void OnUnload (EventArgs e)
		{
			base.OnUnload (e);
			Alive = false;
		}
		
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
			System.Threading.Thread.Sleep(10);
			
			if (Keyboard[Key.Up])
				cam.Z--;
			if (Keyboard[Key.Down])
				cam.Z++;
			if (Keyboard[Key.Enter])
				cam.Z = 8;

            if (Keyboard[Key.Escape])
                Exit();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            ModelView = Matrix4.LookAt(cam, new Vector3(0, 0, 0), Vector3.UnitY);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref ModelView);

            DrawVBOArray();
            SwapBuffers();
        }
		
		#region 2dVBOs
		void LoadMazeIntoVBO()
		{	
			List<Vector3> VertsToDraw = new List<Vector3>();
            bool left;
			
            for (int y = 0; y < labrynth.Dimensions.Y ; y++)
            {
				left = true;
				Point last = new Point(0,0);

                for (int x = 0; x < labrynth.Dimensions.X; x++)
                {       
					if (labrynth.isWall(x,y))
					{
                        last = new Point(x, y);
						if (x > 0 && labrynth.isWall(x - 1, y) && x < labrynth.Dimensions.X - 1 && labrynth.isWall(x + 1, y))
						{
							//If in between two walls
							//Do nothing
						}
						else if (left)
                    	{
                        	VertsToDraw.Add(new Vector3(x,y,1));
                        	VertsToDraw.Add(new Vector3(x,y+1,1));
							left = false;
							if (x < labrynth.Dimensions.X - 1 && !labrynth.isWall(x + 1, y))
							{
								VertsToDraw.Add(new Vector3(x + 1, y + 1, 1));
								VertsToDraw.Add(new Vector3(x + 1, y, 1));
								left = true;
							}
                    	}
						else if (!left)
						{
							VertsToDraw.Add(new Vector3(x + 1, y + 1, 1));
							VertsToDraw.Add(new Vector3(x + 1, y, 1));
							left = true;
						}
					}
                }

				if (!left) // finish quad if not
				{
					VertsToDraw.Add(new Vector3(last.X + 1, last.Y + 1, 1));
					VertsToDraw.Add(new Vector3(last.X + 1, last.Y, 1));
				}
            }
			maze2d = new VBO2d(VertsToDraw.ToArray());
		}
		
		void DrawVBOArray()
		{
			//Reduced to two draw calls!
			GL.EnableClientState(ArrayCap.VertexArray);
			GL.PushMatrix();
            GL.Scale(labrynth.ScaleFactor);
            GL.Translate(labrynth.CenterTranslation);
			GL.Color4(Color.GreenYellow);
			GL.BindBuffer(BufferTarget.ArrayBuffer, maze2d.dataHandle);
			GL.VertexPointer(3, VertexPointerType.Float, sizeof(float) * 3, 0);
			
			GL.DrawArrays(BeginMode.Quads, 0, maze2d.Points);
			
			GL.PopMatrix();
			
			GL.DisableClientState(ArrayCap.VertexArray);
		}
			
		#endregion 2dVBOs
		
		void SpinPlane()
		{
			float speed = 0.01f;
			while (Alive)
			{
				Thread.Sleep(20);
				if (cam.X < -1)
					speed = 0.01f;
				if (cam.X > 1)
					speed = -0.01f;
				cam.X += speed;
			}
		}

        public static void Msain()
        {
            View v = new View();
            v.Run();
        }
    }
}
