using System;
using System.IO;
using System.Text;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace OpenTkConsole
{

	public class Shader
	{
		private readonly int handle;
		
		public int Handle { get { return this.handle; } }
		
		public Shader(ShaderType type, string code)
		{
			this.handle = GL.CreateShader(type);
			
			GL.ShaderSource(this.handle, code);
			GL.CompileShader(this.handle);

            // Check if compile worked
            int successValue;
            GL.GetShader(handle, ShaderParameter.CompileStatus, out successValue);
            if (successValue == 0)
            {
                Console.WriteLine("Shader compile failed: " + GL.GetShaderInfoLog(handle));
            }
		}
		
		static public Shader CreateFromFile(ShaderType type, string filename)
		{
			StreamReader sourceFile = new StreamReader(filename);

            string sourceCode = sourceFile.ReadToEnd();

            sourceFile.Close();
			
			return new Shader(type, sourceCode);
			
		}
	}
	
	public class ShaderProgram
	{
		private readonly int handle;
		private bool inUse;
		
		static public int defaultPositionLocation = 0;
		
		public ShaderProgram(params Shader[] shaders)
		{
			this.handle = GL.CreateProgram();
			
			foreach (var shader in shaders)
			{
				GL.AttachShader(this.handle, shader.Handle);
			}
			
			GL.LinkProgram(this.handle);

            // Check link errors
            int successValue;
            GL.GetProgram(handle, GetProgramParameterName.LinkStatus, out successValue);
            if (successValue == 0)
            {
                Console.WriteLine("Shader link failed: " + GL.GetProgramInfoLog(handle));
            }

            int uniformAmount;
            GL.GetProgram(handle, GetProgramParameterName.ActiveUniforms, out uniformAmount);
			
			Error.checkGLError("Shader()");

            Console.WriteLine("Program linked. Uniform amount " + uniformAmount);

            StringBuilder shaderName = new StringBuilder();
            int writtenLength;
            int uniformSize;
            ActiveUniformType type;
			for (int i = 0; i < uniformAmount; i++)
			{
                GL.GetActiveUniform(this.handle, i, 100, out writtenLength, out uniformSize, out type, shaderName);
                Console.WriteLine("Uniform: " + i + " name :" + shaderName.ToString());
			}
			
            foreach (var shader in shaders)
			{
				GL.DeleteShader(shader.Handle);
			}
		}
		
		public void Use()
		{
			
			GL.UseProgram(this.handle);
			inUse = true;
		}
		
		public int GetAttributeLocation(string name)
		{
			if (!inUse)
			{
				Console.WriteLine("Program not in use! Cannot get attribute location");
			}
			
			if (!GL.IsProgram(this.handle))
			{
				Console.WriteLine("Not a program");
			}
			int location = GL.GetAttribLocation(this.handle, name);
			if (location == -1)
			{
				Console.WriteLine("Attribute " + name + " not found");
			}
			return location;
		}
		
		public int GetUniformLocation(string name)
		{
			if (!inUse)
			{
				Console.WriteLine("Program not in use! cannot get uniform location");
			}
		
			if (!GL.IsProgram(this.handle))
			{
				Console.WriteLine("Not a program");
			}
			
			int location = GL.GetUniformLocation(this.handle, name);
			if (location == -1)
			{
				Console.WriteLine("Uniform " + name + " not found");
			}
			return location;
			
		}
	}
}