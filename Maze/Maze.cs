using System;
using System.Drawing;
using System.Collections.Generic;
using OpenTK;

namespace Maze
{

	
	public class Maze
	{	
		private char[,] maze;
		
		
	 	public bool isSurroundedByWalls(Point cell, Maze labrynth) //WORKING FUCK YEAH
        {
            if (isWall(labrynth.GetCell(new Point(cell.X - 1, cell.Y))) && isWall(labrynth.GetCell(new Point(cell.X + 1, cell.Y))) && isWall(labrynth.GetCell(new Point(cell.X, cell.Y - 1))) && isWall(labrynth.GetCell(new Point(cell.X, cell.Y + 1))))
                return true;
            return false;
        }
		
		public static List<Point> GetNeighbors(Point origin, int bounds, Maze labrynth)
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
	}
}

