using System;
using System.Drawing;
using System.Collections.Generic;
using OpenTK;

namespace Maze
{
	public class PrimMaze
	{	
		private char[,] maze;
        int width, height;

        public PrimMaze(int x, int y)
        {
            maze = new char[x, y];
            width = x;
            height = y;

            Generate();
        }

        //Properties
        #region properties
        public Point Dimensions{ get { return new Point(width, height); } }

        public Vector3 ScaleFactor { get { return new Vector3(1.0f / width * 6.5f, 1.0f / height * 6.5f, 0); } }

        public Vector3 CenterTranslation
        {
            get
            {
                float xoffset = 0, yoffset = 0;
                if (width % 2 != 0)
                    xoffset = 0.5f;
                if (height % 2 != 0)
                    yoffset = 0.5f;
                return new Vector3((-width / 2) - xoffset , (-height / 2) - yoffset, 0);
            }
        }
        #endregion properties
        //Getter/Setter Methods
        #region accessors
        public char GetCell(Point coord) //These simplify the adressing of maze elements, both internally & externally, as the maze is in the YxX format
        {
            return maze[coord.Y, coord.X];
        }

        public char GetCell(int x, int y)
        {
            return maze[y, x];
        }

        public void SetCell(Point coord, char val)
        {
            maze[coord.Y, coord.X] = val;
        }

        public void SetCell(int x, int y, char val)
        {
            maze[y, x] = val;
        }
        #endregion accessors
        //Condition Checkers
        #region condcheck
        public bool isWall(char c)
        {
            return (c == 'x');
        }

        public bool isWall(int x, int y)
        {
            return isWall(maze[y, x]);
        }

	 	public bool isSurroundedByWalls(Point cell) //WORKING FUCK YEAH
        {
            if (isWall(GetCell(cell.X - 1, cell.Y)) && isWall(GetCell(cell.X + 1, cell.Y)) && isWall(GetCell(cell.X, cell.Y - 1)) && isWall(GetCell(cell.X, cell.Y + 1)))
                return true;
            return false;
        }
        #endregion condcheck
        //Generation Code
        #region gencode
        void Generate()
		{
            List<Point> AllCells = new List<Point>();
			List<Point> WallList = new List<Point>();
			List<Point> CellsPartOfMaze = new List<Point>();

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    SetCell(x, y, 'x');

            for (int y = 1; y < height; y += 2)
            {
                for (int x = 1; x < width; x += 2)
                {
                    AllCells.Add(new Point(y, x));
                    SetCell(x, y, 'o');
                }
            }

            Random r = new Random();
			
            Point CurrentCell = new Point(1,1);
			CellsPartOfMaze.Add(CurrentCell);
			
			WallList.AddRange(GetWalls(CurrentCell));
			
            while (WallList.Count > 0)
			{
               	var RandomWall = WallList[r.Next(0, WallList.Count)];
				
            }
        }
		
		List<Point> GetWalls(Point origin)
		{
			List<Point> WallList = new List<Point>();
			
			if (origin.X > 2)
				WallList.Add(new Point(origin.X - 1, origin.Y));
			if (origin.Y > 2)
				WallList.Add(new Point(origin.X, origin.Y - 1));
			if (origin.Y < width - 2)
				WallList.Add(new Point(origin.X + 1, origin.Y));
			if (origin.X < height - 2)
				WallList.Add(new Point(origin.X, origin.Y + 1));
			
			return WallList;
	
		}
		
		public List<Point> GetNeighbors(Point origin, int bounds)
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
        #endregion gencode
    }
}

