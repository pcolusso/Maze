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
   	public struct Maze
    {
        public char[,] maze; //GASPS OF TERROR
        private int x, y;
        private float zoomx, zoomy;

        public Maze(int x, int y)
        {
            maze = new char[x, y];
            this.x = x;
            this.y = y;
            zoomx = 1.0f / x * 6.5f;
            zoomy = 1.0f / y * 6.5f;
        }

        public Point Dimensions { get { return new Point(x, y); } }
        public Vector3 ScaleFactor { get { return new Vector3(zoomx, zoomy, 0); } }
        public Vector3 CenterTranslation
		{
			get
			{
				float offsetX = 0;
				float offsetY = 0;
				if (x % 2 != 0)
					offsetX = 0.5f;
				if (y % 2 != 0)
					offsetY = 0.05f;
				return new Vector3(-x / 2 - offsetX, -y / 2 - offsetY, 1);
			}
		}
        public void SetCell(Point coord, char val)
        {
            maze[coord.Y, coord.X] = val;
        }
        public char GetCell(Point loc)
        {
            return maze[loc.Y, loc.X];
        }
    }
	
	struct VBO2d
	{
		private int bufferHandle, points;
		public VBO2d(Vector3[] verts)
		{
			points = verts.Length;
			GL.GenBuffers(1, out bufferHandle);
			GL.BindBuffer(BufferTarget.ArrayBuffer, bufferHandle);
			GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr) (points * Vector3.SizeInBytes), verts, BufferUsageHint.StaticDraw);
		}
		
		public int dataHandle { get { return bufferHandle; } }
		public int Points { get { return points; } }
	}
	
    class View : GameWindow
    {
        bool GenerateRealTime = false;
        bool RotateCamera = false;
        Maze labrynth;
        Matrix4 Projection, ModelView;
		VBO2d maze2d;
		Vector3 cam;
        int size;
		bool Alive;

        public View()
            : base(700, 700, new GraphicsMode(32, 22, 0, 0), "Maze")
        {
            size = 101;
			
			Alive = true;
            if (GenerateRealTime)
			    new Thread(new ThreadStart(GenDepthFirstMaze)).Start();
            else
                GenDepthFirstMaze();
            if (RotateCamera)
			    new Thread(new ThreadStart(SpinPlane)).Start();

			cam = new Vector3(0, 0, 8);
        }

        protected override void OnLoad(EventArgs e)
        {
            GL.ClearColor(Color.CornflowerBlue);
            base.OnLoad(e);

            GL.Viewport(0, 0, ClientRectangle.Width, ClientRectangle.Height);
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

            if (GenerateRealTime)
                DrawMazeImmediate();
            else
			    DrawVBOArray();
            SwapBuffers();
        }
        #region preds

        bool isWall(char c)
        {

            return (c == 'x');
        }
		
		bool isEnd(char c)
		{
			return (c == 'e');
		}
		
		bool isBegin(char c)
		{
			return (c == 'b');
		}

        bool isSurroundedByWalls(Point cell) //WORKING FUCK YEAH
        {
            if (isWall(labrynth.GetCell(new Point(cell.X - 1, cell.Y))) && isWall(labrynth.GetCell(new Point(cell.X + 1, cell.Y))) && isWall(labrynth.GetCell(new Point(cell.X, cell.Y - 1))) && isWall(labrynth.GetCell(new Point(cell.X, cell.Y + 1))))
                return true;
            return false;
        }
        #endregion preds

        #region Mazes
        void GenCheckerboard()
		{
            labrynth = new Maze(8,8);

            bool flip = true;
            for (int x = 0; x < labrynth.Dimensions.X; x++)
            {
                labrynth.maze[0, x] = 'x';
                labrynth.maze[labrynth.Dimensions.Y - 1, x] = 'x';
            }

            for (int y = 1; y < labrynth.Dimensions.Y - 1; y++)
            {
                labrynth.maze[y, 0] = 'x';
                labrynth.maze[y, labrynth.Dimensions.X - 1] = 'x';
                for (int x = 1; x < labrynth.Dimensions.X - 1; x++)
                {
                    if (flip)
                        labrynth.maze[y, x] = 'x';
                    else
                        labrynth.maze[y, x] = 'o';
                    flip = !flip;
                }
                flip = !flip;
            }
		}
		
		void GenDepthFirstMaze()
		{
			labrynth = new Maze(size, size);
			
			int CellsVisited = 1;
			Stack<Point> CellStack = new Stack<Point>();
			List<Point> AllCells = new List<Point>();

			for (int y = 0; y < size; y++)
				for (int x = 0; x < size; x++)
					labrynth.maze[y, x] = 'x';
			
			for (int y = 1; y < size; y += 2)
			{
				for (int x = 1; x < size; x += 2)
				{
					AllCells.Add(new Point(y, x));
					labrynth.maze[y, x] = 'o';
				}
			}
			
			int TotalCells = AllCells.Count;
			Random r = new Random();
			//Point CurrentCell = new Point(1,1);
			Point CurrentCell = AllCells[r.Next(0, AllCells.Count)];
            labrynth.SetCell(CurrentCell, 'b');
			
			while (CellsVisited < TotalCells && Alive)
			{
				
				var Neighbors = GetNeighbors(CurrentCell, size);
				
				if (Neighbors.Count > 0)
				{
					//Pick a random neighbor
					var SelectedNeighbor = Neighbors[r.Next(0, Neighbors.Count)];
                    //var SelectedNeighbor = Neighbors.OrderBy<Point, int>(x => r.Next()).Take(1);
					//Knock down wall between neighbor
					
					if (CurrentCell.Y == SelectedNeighbor.Y)
					{
                        Point Wall = new Point((CurrentCell.X + SelectedNeighbor.X) / 2, CurrentCell.Y);
                        labrynth.SetCell(Wall, 'o');
					}
					else if (CurrentCell.X == SelectedNeighbor.X)
					{
                        Point Wall = new Point(CurrentCell.X, (CurrentCell.Y + SelectedNeighbor.Y) / 2);
                        labrynth.SetCell(Wall, 'o');
					}
					
					CellStack.Push(CurrentCell);
					CurrentCell = SelectedNeighbor;
					CellsVisited++;
                    
                    if (GenerateRealTime)
                        Thread.Sleep(10);
				}
				else
				{
                    try
                    {
                        CurrentCell = CellStack.Pop();
                    }
                    catch (InvalidOperationException)
                    {
                        
                        break;
                    }
				}
                
			}
            labrynth.SetCell(CurrentCell, 'e');
		}
		
		void PrimMaze()
		{
			int size = 65;
		}
		
		
		List<Point> GetNeighbors(Point origin, int bounds)
		{
			var neighbors = new List<Point>();
            int inc = 2;
            int buffer = 2;
			
			if (origin.X > buffer)
			{
				var cell = new Point(origin.X - inc, origin.Y);
				if (isSurroundedByWalls(cell))
					neighbors.Add(cell);
			}
			if (origin.X < bounds - buffer)
			{
				var cell = new Point(origin.X + inc, origin.Y);
				if (isSurroundedByWalls(cell))
					neighbors.Add(cell);
			}
			if (origin.Y > buffer)
			{
				var cell = new Point(origin.X, origin.Y - inc);
				if (isSurroundedByWalls(cell))
					neighbors.Add(cell);
			}
			if (origin.Y < bounds - buffer)
			{
				var cell = new Point(origin.X, origin.Y + inc);
				if (isSurroundedByWalls(cell))
					neighbors.Add(cell);
			}
			return neighbors;
		}
		
		
		#endregion Mazes
		
		#region immediate
        void DrawMazeImmediate()
        {
            GL.PushMatrix();
            GL.Scale(labrynth.ScaleFactor);
            GL.Translate(labrynth.CenterTranslation);
            for (int y = 0; y < labrynth.Dimensions.Y; y++)
            {
                for (int x = 0; x < labrynth.Dimensions.X; x++)
                {
                    if (isWall(labrynth.maze[y, x]))
                        GL.Color4(Color.Black);
                    else if (isBegin(labrynth.maze[y, x]))
                        GL.Color4(Color.Green);
                    else if (isEnd(labrynth.maze[y,x]))
                        GL.Color4(Color.Purple);
                    else
                        GL.Color4(Color.White);
                    GL.Rect(x, y, x + 1, y + 1);
                }
            }
            GL.PopMatrix();
        }

        void DrawMazeImmediate2()
        {
            List<Vector3> VertsToDraw = new List<Vector3>();
            for (int y = 0; y < labrynth.Dimensions.Y; y++)
            {
                for (int x = 0; x < labrynth.Dimensions.X; x++)
                {
                    if (isWall(labrynth.maze[y, x]))
                    {
                        VertsToDraw.Add(new Vector3(x,y,0));
                        VertsToDraw.Add(new Vector3(x,y+1,0));
                        VertsToDraw.Add(new Vector3(x+1,y+1,0));
                        VertsToDraw.Add(new Vector3(x+1,y,0));
                    }
                }
            }

            GL.PushMatrix();
            GL.Scale(labrynth.ScaleFactor);
            GL.Translate(labrynth.CenterTranslation);

            //Draw the backdrop
            GL.Color4(Color.White);
            GL.Begin(BeginMode.Quads);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, labrynth.Dimensions.Y, 0);
            GL.Vertex3(labrynth.Dimensions.X, labrynth.Dimensions.Y, 0);
            GL.Vertex3(labrynth.Dimensions.X, 0, 0);
            GL.End();

            //Draw the Walls.
            GL.Color4(Color.Black);
            GL.Begin(BeginMode.Quads);
            foreach (Vector3 v in VertsToDraw)
                GL.Vertex3(v);
            GL.End();
            GL.PopMatrix();
        }
		#endregion immediate
		
		#region 2dVBOs
		void LoadMazeIntoVBO()
		{
			long time = System.DateTime.Now.Ticks;
			
			List<Vector3> VertsToDraw = new List<Vector3>();
			bool left, _top;
			
            for (int y = 0; y < labrynth.Dimensions.Y ; y++)
            {
				left = true;
				Point last;
                for (int x = 0; x < labrynth.Dimensions.X; x++)
                {       
					if (isWall(labrynth.maze[y, x]))
					{
						if (x > 0 && isWall(labrynth.maze[y, x - 1]) && x < labrynth.Dimensions.X - 1 && isWall(labrynth.maze[y, x + 1]))
						{
							//If in between two walls
							//Do nothing
						}
						else if (left)
                    	{
                        	VertsToDraw.Add(new Vector3(x,y,1));
                        	VertsToDraw.Add(new Vector3(x,y+1,1));
							left = false;
							if (x < labrynth.Dimensions.X - 1 && !isWall(labrynth.maze[y, x + 1]))
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
			
						if (isWall(labrynth.maze[y, x]))
							last = new Point(x, y);
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

        public static void Main()
        {
            View v = new View();
            v.Run();
        }
    }
}
