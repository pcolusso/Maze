using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
namespace Maze
{
	public struct VBO2d
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
}

