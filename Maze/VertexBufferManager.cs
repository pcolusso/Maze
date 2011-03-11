using System;
using System.Drawing;
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
	public struct Component
	{
		public byte R, G, B, A;
		public Vector3 vertex;
        public static int SizeInBytes = 16;
	}
	
    int dataHandle, maxSize;
    List<Component> ElementList = new List<Component>();

    public DynamicColorVertexBuffer(Vector3[] Verts, Color[] Colors, int maxSize)
    {
        GL.GenBuffers(1, out dataHandle);
        this.maxSize = maxSize;
        AddElements(Verts, Colors);
        UpdateBuffers();
    }

    public bool AddElements(Vector3[] Verts, Color[] Colors)
    {
        if (Verts.Length != Colors.Length)
            throw new Exception("Was not given enough data to update VBO");
        if (ElementList.Count + Verts.Length > maxSize)
            throw new Exception("VBO Size Exceeded.");

        for (int i = 0; i < Verts.Length; i++)
        {
            Component element = new Component();
            element.R = Colors[i].R;
            element.G = Colors[i].G;
            element.B = Colors[i].B;
            element.A = Colors[i].A;
            element.vertex = Verts[i];
            ElementList.Add(element);
        }

        return true;
    }

    public void UpdateBuffers()
	{
        //Discard old VBO, use new one.
        GL.BindBuffer(BufferTarget.ArrayBuffer, dataHandle);
        GL.BufferData<Component>(BufferTarget.ArrayBuffer, (IntPtr) (Component.SizeInBytes * maxSize), ElementList.ToArray(), BufferUsageHint.DynamicDraw);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

        //PrintLocalContents();
        //PrintVBOContents();
	}

    public void PrintVBOContents()
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, dataHandle);
        IntPtr VideoMemoryPtr = GL.MapBuffer(BufferTarget.ArrayBuffer, BufferAccess.ReadOnly);
        unsafe
        {
            ushort* VideoMemory = (ushort*)VideoMemoryPtr.ToPointer();
            for (int i = 0; i < maxSize; i++)
            {
                Console.WriteLine("@ " + i + ": " + VideoMemory[i]);
            }
        }
        GL.UnmapBuffer(BufferTarget.ArrayBuffer);
    }

    public void PrintLocalContents()
    {
        foreach (Component c in ElementList)
            Console.WriteLine(c.ToString());
    }

    public void Draw()
    {
        GL.EnableClientState(ArrayCap.VertexArray);
        GL.EnableClientState(ArrayCap.ColorArray);

        GL.BindBuffer(BufferTarget.ArrayBuffer, dataHandle);
        GL.ColorPointer(4, ColorPointerType.UnsignedByte, Component.SizeInBytes, 0);
        GL.VertexPointer(3, VertexPointerType.Float, Vector3.SizeInBytes, (IntPtr)(sizeof(byte) * 4));
        GL.PointSize(20);
        GL.DrawArrays(BeginMode.Points, 0, ElementList.Count);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

        GL.DisableClientState(ArrayCap.VertexArray);
        GL.DisableClientState(ArrayCap.ColorArray);
    }
}
