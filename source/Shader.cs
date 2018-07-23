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

		public string name;

		public List<ShaderAttribute> attributes;
		public List<ShaderUniform> uniforms;
		
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

			name = shaders[0].ShaderName;
			Logger.LogInfo("Creating Shader Program " + name);
			// Load Uniforms
			///////////////////////////////////
			int uniformAmount = -1;
            GL.GetProgram(handle, GetProgramParameterName.ActiveUniforms, out uniformAmount);
			Error.checkGLError("Shader()");
			ShaderUniformManager uniManager = ShaderUniformManager.GetSingleton();
            Logger.LogInfo("\tProgram linked.\n\tUniform amount " + uniformAmount);
			uniforms = new List<ShaderUniform>(uniformAmount);

			int maxShaderNameSize = 100;
            StringBuilder shaderName = new StringBuilder(maxShaderNameSize);
            int writtenLength;
            int uniformSize;
            ActiveUniformType type;
			for (int i = 0; i < uniformAmount; i++)
			{
                GL.GetActiveUniform(this.handle, i, maxShaderNameSize, out writtenLength, out uniformSize, out type, shaderName);

				string uniformName = shaderName.ToString();
				ShaderSizeInfo info = uniManager.GetTypeSizeInfo(type);
				Logger.LogInfo("\tUniform: " + i + " name :" + uniformName + " Size: " + info.sizeElements + " Type: " + uniManager.GetDataTypeString(uniManager.UniformTypeToDataType(type)));

				ShaderUniform uniform = uniManager.CreateShaderUniform(uniformName, type, GetUniformLocation(this.handle, uniformName));
				if (uniform.IsValid())
				{
					uniforms.Add(uniform);
				}
			}

			int attributeAmount = -1;
			GL.GetProgram(handle, GetProgramParameterName.ActiveAttributes, out attributeAmount);
			ActiveAttribType attribType;
			int attrSize = -1;

			Logger.LogInfo("\tAttribute amount " + attributeAmount);

			attributes = new List<ShaderAttribute>(attributeAmount);

			for (int i = 0; i < attributeAmount; i++)
			{
				GL.GetActiveAttrib(this.handle, i, maxShaderNameSize, out writtenLength, out attrSize, out attribType, shaderName);

				string attribName = shaderName.ToString();
				int location = GetAttributeLocation(handle, attribName);
				ShaderSizeInfo info = uniManager.GetTypeSizeInfo(attribType);
				Logger.LogInfo("\tAttribute " + i + ": Name :" + attribName + " Size: " + info.sizeElements + " Location: " + location + " Type: " + uniManager.GetDataTypeString(uniManager.AttribTypeToDataType(attribType)));

				ShaderAttribute attribute = uniManager.CreateShaderAttribute(attribName, attribType, GetAttributeLocation(this.handle, attribName), info.sizeElements);
				if (attribute.IsValid())
				{
					attributes.Add(attribute);
				}	
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

		public void SetVec2Uniform(int uniformLocation , Vector2 value)
		{
			GL.Uniform2(uniformLocation, value);
			Error.checkGLError("Shader set vec2");
		}

		public void SetFloatUniform(int uniformLocation , float value)
		{
			GL.Uniform1(uniformLocation, value);
			Error.checkGLError("Shader set float");
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