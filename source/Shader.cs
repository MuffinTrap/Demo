using System;
using System.Text;
using System.Text.RegularExpressions;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace MuffinSpace
{

	public class Shader
	{
		private readonly int handle;
		public string ShaderName { get; set; }
		
		public int Handle { get { return handle; } }
		
		public Shader(string name, ShaderType type, string code)
		{
			handle = GL.CreateShader(type);
			
			GL.ShaderSource(handle, code);
			GL.CompileShader(handle);

            // Check if compile worked
            int successValue = 0;
            GL.GetShader(handle, ShaderParameter.CompileStatus, out successValue);
            if (successValue == 0)
            {
				string glErrorString = GL.GetShaderInfoLog(handle);
				Logger.LogError(Logger.ErrorState.Limited, "Shader compile failed: " + glErrorString);
				// Parse error line
				int lineStart = glErrorString.IndexOf("(");
				int lineEnd = glErrorString.IndexOf(")", lineStart);
				int number = int.Parse(glErrorString.Substring(lineStart + 1, lineEnd - lineStart - 1));

				// Files start at 1, but opengl at 0. 
				number -= 1;

				string[] fileLines = Regex.Split(code, "\r\n");
				for (int l = 0; l < fileLines.Length; l++)
				{
					if (l < number - 10 || l > number + 10)
					{
						continue;
					}
					ConsoleColor c = ConsoleColor.Gray;
					if (l == number)
					{
						c = ConsoleColor.Red;
					}
					Logger.LogInfoLinePart("" + l + " ", c);
					Logger.LogInfoLinePart(fileLines[l], c);
					Logger.LogInfoLineEnd();
				}
            }
			ShaderName = name;
		}
	}
	
	public class ShaderProgram
	{
		private readonly int handle;

		public string name;

		public List<ShaderAttribute> attributes;
		public List<ShaderUniform> uniforms;
		
		public ShaderProgram(string nameParam, params Shader[] shaders)
		{
			handle = GL.CreateProgram();
			
			foreach (var shader in shaders)
			{
				GL.AttachShader(handle, shader.Handle);
			}
			
			GL.LinkProgram(handle);

			// Check link errors
			int successValue = 0;
            GL.GetProgram(handle, GetProgramParameterName.LinkStatus, out successValue);
            if (successValue == 0)
            {
				Logger.LogError(Logger.ErrorState.Limited, "Shader link failed: " + GL.GetProgramInfoLog(handle));
            }

			name = nameParam;
			Logger.LogInfoLinePart("Creating Shader Program: ", ConsoleColor.Gray);
			Logger.LogInfoLinePart(name, ConsoleColor.Cyan);
			Logger.LogInfoLineEnd();

			// Load Uniforms
			///////////////////////////////////
			int uniformAmount = -1;
            GL.GetProgram(handle, GetProgramParameterName.ActiveUniforms, out uniformAmount);
			Error.checkGLError("Shader()");
			ShaderUniformManager uniManager = ShaderUniformManager.GetSingleton();

			Logger.LogInfo("Program linked.");

			uniforms = new List<ShaderUniform>(uniformAmount);

			int maxShaderNameSize = 100;
            StringBuilder shaderName = new StringBuilder(maxShaderNameSize);
            int writtenLength;
            int uniformSize;
            ActiveUniformType type;

			Logger.LogInfo("Uniforms (" + uniformAmount + ") >>>");
			for (int i = 0; i < uniformAmount; i++)
			{
                GL.GetActiveUniform(this.handle, i, maxShaderNameSize, out writtenLength, out uniformSize, out type, shaderName);

				string uniformName = shaderName.ToString();
				ShaderSizeInfo info = uniManager.GetTypeSizeInfo(type);

				ShaderUniform uniform = uniManager.CreateShaderUniform(uniformName, type, GetUniformLocation(this.handle, uniformName));

				Logger.LogInfoLinePart(" " + i, ConsoleColor.White);
				Logger.LogInfoLinePart("\t" + uniManager.GetDataTypeString(uniManager.UniformTypeToDataType(type)), ConsoleColor.Cyan);
				Logger.LogInfoLinePart("\t" + uniformName + " (" + info.sizeElements + ")", ConsoleColor.White);
				if (uniform.IsValid())
				{
					Logger.LogInfoLinePart("\t\t [", ConsoleColor.Gray);
					Logger.LogInfoLinePart("OK", ConsoleColor.Green);
					Logger.LogInfoLinePart("]", ConsoleColor.Gray);
					uniforms.Add(uniform);
				}
				Logger.LogInfoLineEnd();
			}

			int attributeAmount = -1;
			GL.GetProgram(handle, GetProgramParameterName.ActiveAttributes, out attributeAmount);
			ActiveAttribType attribType;
			int attrSize = -1;

			Logger.LogInfo("Attributes (" + attributeAmount + ") >>>");

			attributes = new List<ShaderAttribute>(attributeAmount);

			for (int i = 0; i < attributeAmount; i++)
			{
				GL.GetActiveAttrib(this.handle, i, maxShaderNameSize, out writtenLength, out attrSize, out attribType, shaderName);

				string attribName = shaderName.ToString();
				int location = GetAttributeLocation(handle, attribName);
				ShaderSizeInfo info = uniManager.GetTypeSizeInfo(attribType);
				ShaderAttribute attribute = uniManager.CreateShaderAttribute(attribName, attribType, GetAttributeLocation(this.handle, attribName), info.sizeElements);

				Logger.LogInfoLinePart(" " + i, ConsoleColor.White);
				Logger.LogInfoLinePart("\t" + uniManager.GetDataTypeString(uniManager.AttribTypeToDataType(attribType)), ConsoleColor.Cyan);
				Logger.LogInfoLinePart("\t" + attribName + " (" + info.sizeElements + ")", ConsoleColor.White);
				Logger.LogInfoLinePart("\t " + location, ConsoleColor.Red);
				if (attribute.IsValid())
				{
					Logger.LogInfoLinePart("\t\t [", ConsoleColor.Gray);
					Logger.LogInfoLinePart("OK", ConsoleColor.Green);
					Logger.LogInfoLinePart("]", ConsoleColor.Gray);
					attributes.Add(attribute);
				}
				Logger.LogInfoLineEnd();
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

		public void SetColorUniform(int uniformLocation , Vector4 color)
		{
			GL.Uniform4(uniformLocation, color);
			Error.checkGLError("Shader set vec4 color");
		}

		public void SetVec3Uniform(int uniformLocation , Vector3 value)
		{
			GL.Uniform3(uniformLocation, value);
			Error.checkGLError("Shader set vec3");
		}

		public Vector3 GetVec3Uniform(int uniformLocation)
		{
			float[] outVec = new float[] { -1, -1, -1 };
			GL.GetUniform(handle, uniformLocation, outVec);
			return new Vector3(outVec[0], outVec[1], outVec[2]);
		}

		public void SetVec2Uniform(int uniformLocation , Vector2 value)
		{
			GL.Uniform2(uniformLocation, value);
			Error.checkGLError("Shader set vec2");
		}

		public void SetFloatUniform(ShaderUniformName name, float value)
		{
			foreach(ShaderUniform u in uniforms)
			{
				if (u.name == name)
				{
					SetFloatUniform(u.location, value);
					break;
				}
			}
		}

		public void SetFloatUniform(int uniformLocation , float value)
		{
			GL.Uniform1(uniformLocation, value);
			Error.checkGLError("Shader set float");
		}

		public float GetFloatUniform(int uniformLocation)
		{
			float outFloat = -1;
			GL.GetUniform(handle, uniformLocation, out outFloat);
			return outFloat;
		}

		public void SetIntUniform(int uniformLocation , int value)
		{
			GL.Uniform1(uniformLocation, value);
			Error.checkGLError("Shader set int");
		}

		public void SetMatrix4Uniform(int uniformLocation, ref Matrix4 value)
		{
			GL.UniformMatrix4(uniformLocation, false, ref value);
			Error.checkGLError("Shader set matrix4");
		}

		public void SetSamplerUniform(int uniformLocation, int textureUnit)
		{
			GL.Uniform1(uniformLocation, textureUnit);
			Error.checkGLError("Shader set sampler");
		}

		public int GetCustomUniformLocation(string name)
		{
			return GetUniformLocation(handle, name);
		}

		public int GetUniformLocation(ShaderUniformName name)
		{
			foreach(ShaderUniform a in uniforms)
			{
				if (a.name == name)
				{
					return a.location;
				}
			}
			ShaderUniformManager uniMan = ShaderUniformManager.GetSingleton();
			Logger.LogError(Logger.ErrorState.Limited, "Uniform " + uniMan.GetUniformName(name) + " location not found");
			return -1;
		}

		public int GetAttributeLocation(ShaderAttributeName name)
		{
			foreach (ShaderAttribute a in attributes)
			{
				if (a.name == name)
				{
					return a.location;
				}
			}
			ShaderUniformManager uniMan = ShaderUniformManager.GetSingleton();
			Logger.LogError(Logger.ErrorState.Limited, "Attribute " + uniMan.GetAttributeName(name) + " location not found");
			return -1;
		}
	}
}