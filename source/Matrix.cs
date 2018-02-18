using System;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace OpenTkConsole
{

	public class Matrix4Uniform
	{
		private readonly string name;
		private Matrix4 matrix;
		
		public Matrix4 Matrix 
		{ 
			get { return this.matrix; } set { this.matrix = value; }
		}
			
		
		public Matrix4Uniform(string name)
		{
			this.name = name;
			matrix = Matrix4.Identity;
		}
		
		public void Set(ShaderProgram program)
		{
			int i = program.GetUniformLocation(this.name);
			 
			Error.checkGLError("Matrix.GetUniformLocation of " + name);
			 
			GL.UniformMatrix4(i, false, ref this.matrix);

			Error.checkGLError("Matrix.SetValue");
        }
	}
}