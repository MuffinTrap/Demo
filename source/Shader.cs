using System;
using System.IO;
using System.Text;
using System.Collections;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace OpenTkConsole
{

	public class Shader
	{
		private readonly int handle;
		public string ShaderName { get; set; }
		
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
				Logger.LogError(Logger.ErrorState.Limited, "Shader compile failed: " + GL.GetShaderInfoLog(handle));
            }
		}
		
		static public Shader CreateFromFile(ShaderType type, string filename)
		{
			try
			{
				StreamReader sourceFile = new StreamReader(filename);

				string sourceCode = sourceFile.ReadToEnd();

				sourceFile.Close();

				return new Shader(type, sourceCode);
			}
			catch (Exception e)
			{
				Logger.LogError(Logger.ErrorState.Limited, "Shader CreateFromFile exception when opening file " + filename + " Error: " + e.Message);
				return null;
			}
		}
	}
	
	public class ShaderProgram
	{
		private readonly int handle;

		public List<ShaderAttribute> attributes;
		public List<ShaderAttribute> uniforms;
		
		static public int defaultPositionLocation = 0;
		
		private ShaderAttribute typeToAttributes(ActiveUniformType type)
		{
			int bt = MeshData.BytesPerFloat;
			ShaderAttribute result;

			switch(type)
			{
				case ActiveUniformType.Float:
					result = new ShaderAttribute("Float", 0, bt, 1);
					break;
				case ActiveUniformType.FloatVec2:
					result = new ShaderAttribute("FloatVec2", 0, bt * 2, 2);
					break;
				case ActiveUniformType.FloatVec3:
					result = new ShaderAttribute("FloatVec3", 0, bt * 3, 3);
					break;
				case ActiveUniformType.FloatVec4:
					result = new ShaderAttribute("FloatVec4", 0, bt * 4, 4);
					break;
				case ActiveUniformType.FloatMat4:
					result = new ShaderAttribute("FloatMat4", 0, bt * 16, 16);
					break;

				default:
					result = new ShaderAttribute("Default", 0, 0, 0);
					break;
			}

			return result;
		}

		private ShaderAttribute typeToAttributes(ActiveAttribType type)
		{
			int bt = MeshData.BytesPerFloat;
			ShaderAttribute result;

			switch (type)
			{
				case ActiveAttribType.Float:
					result = new ShaderAttribute("Float", 0, bt, 1);
					break;
				case ActiveAttribType.FloatVec2:
					result = new ShaderAttribute("FloatVec2", 0, bt * 2, 2);
					break;
				case ActiveAttribType.FloatVec3:
					result = new ShaderAttribute("FloatVec3", 0, bt * 3, 3);
					break;
				case ActiveAttribType.FloatVec4:
					result = new ShaderAttribute("FloatVec4", 0, bt * 4, 4);
					break;
				case ActiveAttribType.FloatMat4:
					result = new ShaderAttribute("FloatMat4", 0, bt * 16, 16);
					break;

				default:
					result = new ShaderAttribute("Default", 0, 0, 0);
					break;
			}

			return result;
		}

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
				Logger.LogError(Logger.ErrorState.Limited, "Shader link failed: " + GL.GetProgramInfoLog(handle));
            }

			int uniformAmount = -1;
            GL.GetProgram(handle, GetProgramParameterName.ActiveUniforms, out uniformAmount);
			
			Error.checkGLError("Shader()");

            Logger.LogInfo("Program linked. Uniform amount " + uniformAmount);

			uniforms = new List<ShaderAttribute>(uniformAmount);

			int maxShaderNameSize = 100;
            StringBuilder shaderName = new StringBuilder(maxShaderNameSize);
            int writtenLength;
            int uniformSize;
            ActiveUniformType type;
			for (int i = 0; i < uniformAmount; i++)
			{
                GL.GetActiveUniform(this.handle, i, maxShaderNameSize, out writtenLength, out uniformSize, out type, shaderName);

				string uniformName = shaderName.ToString();
				ShaderAttribute info = typeToAttributes(type);
				Logger.LogInfo("Uniform: " + i + " name :" + uniformName + " Size: " + info.sizeElements + " Type: " + info.name);

				uniforms.Add(new ShaderAttribute(shaderName.ToString(), GetUniformLocation(handle, uniformName), info.sizeBytes, info.sizeElements));
			}

			int attributeAmount = -1;
			GL.GetProgram(handle, GetProgramParameterName.ActiveAttributes, out attributeAmount);
			ActiveAttribType attribType;
			int attrSize = -1;

			Logger.LogInfo("Attribute amount " + attributeAmount);

			attributes = new List<ShaderAttribute>(attributeAmount);

			for (int i = 0; i < attributeAmount; i++)
			{
				GL.GetActiveAttrib(this.handle, i, maxShaderNameSize, out writtenLength, out attrSize, out attribType, shaderName);

				string attribName = shaderName.ToString();
				int location = GetAttributeLocation(handle, attribName);
				ShaderAttribute info = typeToAttributes(attribType);
				Logger.LogInfo("Attribute " + i + ": Name :" + attribName + " Size: " + info.sizeElements + " Location: " + location + " Type: " + info.name);

				attributes.Add(new ShaderAttribute(attribName, location, info.sizeBytes, info.sizeElements));
			}
			
            foreach (var shader in shaders)
			{
				GL.DeleteShader(shader.Handle);
			}
		}
		
		public void Use()
		{
			GL.UseProgram(handle);
		}
		
		private static int GetAttributeLocation(int handle, string name)
		{
			/*
			if (!inUse)
			{
				Logger.LogError(Logger.ErrorState.Limited, "Program not in use! Cannot get attribute location");
			}
			*/
			
			if (!GL.IsProgram(handle))
			{
				Logger.LogError(Logger.ErrorState.Limited, ("Shader " + name + " is not a program"));
			}
			int location = GL.GetAttribLocation(handle, name);
			if (location == -1)
			{
				Logger.LogInfo("Attribute " + name + " not found");
			}
			return location;
		}
		
		private static int GetUniformLocation(int handle, string name)
		{
			/*
			if (!inUse)
			{
				Logger.LogError(Logger.ErrorState.Limited, "Program " + name + " not in use! cannot get uniform location");
			}
			*/
		
			if (!GL.IsProgram(handle))
			{
				Logger.LogError(Logger.ErrorState.Limited, ("Shader " + name + " is not a program"));
			}
			
			int location = GL.GetUniformLocation(handle, name);
			if (location == -1)
			{
				Logger.LogError(Logger.ErrorState.Limited, "Uniform " + name + " not found");
			}
			return location;
			
		}

		public void setSamplerUniform(string samplerName, int location)
		{
			int uniformLocation = GetUniformLocation(samplerName);
			GL.Uniform1(uniformLocation, location);
		}

		public int GetUniformLocation(string name)
		{
			foreach(ShaderAttribute a in uniforms)
			{
				if (a.name == name)
				{
					return a.index;
				}
			}
			Logger.LogError(Logger.ErrorState.Limited, "Uniform " + name + " not found");
			return -1;
		}

		public int GetAttributeLocation(string name)
		{
			foreach (ShaderAttribute a in attributes)
			{
				if (a.name == name)
				{
					return a.index;
				}
			}
			Logger.LogError(Logger.ErrorState.Limited, "Attribute " + name + " not found");
			return -1;
		}
	}
}