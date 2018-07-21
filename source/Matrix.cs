using System;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace OpenTkConsole
{

	public class Matrix4Uniform
	{
		private readonly ShaderUniformName name;
		private Matrix4 matrix;
		
		public Matrix4 Matrix 
		{ 
			get { return this.matrix; } set { this.matrix = value; }
		}
			
		
		public Matrix4Uniform(ShaderUniformName nameParam)
		{
			name = nameParam;
			matrix = Matrix4.Identity;
		}

		public void SetToShader(ShaderProgram program, int location)
		{
			program.SetMatrix4Uniform(location, ref matrix);
			Error.checkGLError("Matrix4Uniform.SetToShader " + program.name + " Matrix: "+ name + " to " + location);
		}
		
	}
}