using System;
using System.Drawing;
using OpenTK;

namespace Maze
{
	public interface IState
	{
		void Load(Rectangle Window);
		void Update();
		void Draw();
		void Destroy();
	}
}

