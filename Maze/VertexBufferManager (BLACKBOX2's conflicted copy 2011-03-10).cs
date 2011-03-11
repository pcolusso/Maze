using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;

public class VertexBufferManager
{
	public VertexBufferManager()
	{
	}
}

public class VertexOnly : VertexBufferManager
{
    int bufferHandle, points;

    public VertexOnly(Vector3[] Verticies)
    {
        try
        {
            GL.GenBuffers(1, out bufferHandle);
        }
        catch (InvalidOperationException)
        {
            throw new Exception("Could not create VBO, have you initialsied the OGL context first?");
        }

        points = Verticies.Length;
        GL.BindBuffer(BufferTarget.ArrayBuffer, bufferHandle);
        GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr) (points * Vector3.SizeInBytes), Verticies, BufferUsageHint.StaticDraw);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    }

    public void Draw(BeginMode mode)
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, bufferHandle);
        GL.DrawArrays(mode, 0, points);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    }
}

public class DynamicColorVertexBuffer : VertexBufferManager
{
    int dataHandle, maxSize;
    List<Vector3> VertexArray = new List<Vector3>();

    public DynamicColorVertexBuffer(Vector3[] Verticies, int maxSize)
    {
        this.maxSize = maxSize;
        VertexArray.AddRange(Verticies);

        GL.GenBuffers(1, out dataHandle);
        GL.BindBuffer(BufferTarget.ArrayBuffer, dataHandle);
        GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(Vector3.SizeInBytes * maxSize), Verticies, BufferUsageHint.DynamicDraw);
    }

    public bool AddVerticiesToArray(Vector3
}
