using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace OpenTkConsole
{

	// Inside the renderer, the uniforms are handles using the enum names.
	// When shader is compiled the string names are checked that they 
	// are recognized by the renderer.

	// Shader asks this manager if a name is supported and that the uniform
	// is of correct type

	// This manager also handles setting the values to the shader
	// using the uniform locations

	public class ShaderUniformLocation
	{
		public int location;
	}

	public enum ShaderAttributeName
	{ 
		Position,
		TexCoord,
		Normal,

		InvalidAttributeName
	}

	public enum ShaderUniformName
	{
		WorldMatrix,
		ProjectionMatrix,
		ViewMatrix,

		DiffuseColor,
		DiffuseTexture,

		InvalidUniformName
	}

	public enum ShaderDataType
	{
		Float,
		Float2,
		Float3,
		Float4,

		Mat4,

		InvalidType
	}

	// This is an interface for classes that have data for shaders
	// They need to register to uniform manager that then 
	// asks them to submit the data shader asks for it.
	public interface IShaderDataOwner
	{
		void ActivateForDrawing();
		void SetUniform(ShaderProgram shaderProgram, int location, ShaderUniformName dataName);
	}

	public class ShaderUniformManager
	{
		private Dictionary<ShaderUniformName, IShaderDataOwner> uniformDataOwners;
		public Dictionary<string, ShaderUniform> supportedUniforms;
		public Dictionary<string, ShaderAttribute> supportedAttributes;
		private static ShaderUniformManager singleton = null;


		private ShaderUniformManager()
		{
			supportedUniforms = new Dictionary<string, ShaderUniform>();
			supportedAttributes = new Dictionary<string, ShaderAttribute>();

			supportedUniforms.Add(GetUniformName(ShaderUniformName.WorldMatrix), new ShaderUniform(ShaderUniformName.WorldMatrix, ShaderDataType.Mat4));
			supportedUniforms.Add(GetUniformName(ShaderUniformName.ProjectionMatrix), new ShaderUniform(ShaderUniformName.ProjectionMatrix, ShaderDataType.Mat4));
			supportedUniforms.Add(GetUniformName(ShaderUniformName.ViewMatrix), new ShaderUniform(ShaderUniformName.ViewMatrix, ShaderDataType.Mat4));

			supportedAttributes.Add(GetAttributeName(ShaderAttributeName.Position), new ShaderAttribute(ShaderAttributeName.Position, ShaderDataType.Float3));
			supportedAttributes.Add(GetAttributeName(ShaderAttributeName.Normal), new ShaderAttribute(ShaderAttributeName.Normal, ShaderDataType.Float3));
			supportedAttributes.Add(GetAttributeName(ShaderAttributeName.TexCoord), new ShaderAttribute(ShaderAttributeName.TexCoord, ShaderDataType.Float2));

			uniformDataOwners = new Dictionary<ShaderUniformName, IShaderDataOwner>();
		}

		public static ShaderUniformManager GetSingleton()
		{
			if (singleton == null)
			{
				singleton = new ShaderUniformManager();
			}
			return singleton;
		}

		public void ActivateShader(ShaderProgram shader)
		{
			shader.Use();

			foreach (ShaderUniform uni in shader.uniforms)
			{
				IShaderDataOwner owner;
				if (uniformDataOwners.TryGetValue(uni.name, out owner))
				{ 
					owner.SetUniform(shader, uni.location, uni.name);
				}
			}
		}

		public void RegisterDataOwner(IShaderDataOwner owner, ShaderUniformName uniform)
		{
			if (uniformDataOwners.ContainsKey(uniform))
			{
				if (uniformDataOwners[uniform] != owner)
				{
					uniformDataOwners.Remove(uniform);
					uniformDataOwners.Add(uniform, owner);
				}
			}
		}

		public ShaderUniform CreateShaderUniform(string nameString, ActiveUniformType type, int location)
		{
			ShaderUniform returnValue;
			if (supportedUniforms.ContainsKey(nameString))
			{
				ShaderUniform supported = supportedUniforms[nameString];
				returnValue = new ShaderUniform(supported.name, supported.dataType, location);
			}
			else
			{
				Logger.LogError(Logger.ErrorState.Critical, "Unsupported shader uniform: " + nameString + ", of type:" + GetDataTypeString(UniformTypeToDataType(type)));
				returnValue = new ShaderUniform(ShaderUniformName.InvalidUniformName, ShaderDataType.InvalidType);
			}

			return returnValue;
		}

		public ShaderAttribute CreateShaderAttribute(string nameString, ActiveAttribType type, int location)
		{
			ShaderAttribute returnValue;
			if (supportedAttributes.ContainsKey(nameString))
			{
				ShaderAttribute supported = supportedAttributes[nameString];
				returnValue = new ShaderAttribute(supported.name, supported.dataType, location);
			}
			else
			{
				Logger.LogError(Logger.ErrorState.Critical, "Unsupported shader attribute: " + nameString + ", of type:" + GetDataTypeString(AttribTypeToDataType(type)));
				returnValue = new ShaderAttribute(ShaderAttributeName.InvalidAttributeName, ShaderDataType.InvalidType);
			}

			return returnValue;
		}

		public string GetAttributeName(ShaderAttributeName name)
		{
			switch (name)
			{
				case ShaderAttributeName.Position: return "aPosition";
				case ShaderAttributeName.Normal: return "aNormal";
				case ShaderAttributeName.TexCoord: return "aTexCoord";
				default: return string.Empty;
			}
		}
		public string GetUniformName(ShaderUniformName name)
		{
			switch (name)
			{
				case ShaderUniformName.ViewMatrix: return "uViewMatrix";
				case ShaderUniformName.ProjectionMatrix: return "uProjectionMatrix";
				case ShaderUniformName.WorldMatrix: return "uWorldMatrix";

				case ShaderUniformName.DiffuseTexture: return "uDiffuseTexture";

				default: return string.Empty;
			}
		}


		public ShaderDataType UniformTypeToDataType(ActiveUniformType type)
		{
			switch (type)
			{
				case ActiveUniformType.Float:
					return ShaderDataType.Float;
				case ActiveUniformType.FloatVec2:
					return ShaderDataType.Float2;
				case ActiveUniformType.FloatVec3:
					return ShaderDataType.Float3;
				case ActiveUniformType.FloatVec4:
					return ShaderDataType.Float4;
				case ActiveUniformType.FloatMat4:
					return ShaderDataType.Mat4;

				default:
					Logger.LogError(Logger.ErrorState.Critical, "Unsupported shader data type");
					return ShaderDataType.InvalidType;
			}
		}

		public ShaderDataType AttribTypeToDataType(ActiveAttribType type)
		{
			switch (type)
			{
				case ActiveAttribType.Float:
					return ShaderDataType.Float;
				case ActiveAttribType.FloatVec2:
					return ShaderDataType.Float2;
				case ActiveAttribType.FloatVec3:
					return ShaderDataType.Float3;
				case ActiveAttribType.FloatVec4:
					return ShaderDataType.Float4;
				case ActiveAttribType.FloatMat4:
					return ShaderDataType.Mat4;

				default:
					Logger.LogError(Logger.ErrorState.Critical, "Unsupported shader data type");
					return ShaderDataType.InvalidType;
			}
		}

		public ShaderSizeInfo GetTypeSizeInfo(ActiveUniformType type)
		{
			return GetTypeSizeInfo(UniformTypeToDataType(type));
		}
		public ShaderSizeInfo GetTypeSizeInfo(ActiveAttribType type)
		{
			return GetTypeSizeInfo(AttribTypeToDataType(type));
		}

		public ShaderSizeInfo GetTypeSizeInfo(ShaderDataType type)
		{
			int bt = MeshData.BytesPerFloat;
			ShaderSizeInfo result;

			switch (type)
			{
				case ShaderDataType.Float:
					result = new ShaderSizeInfo(bt, 1);
					break;
				case ShaderDataType.Float2:
					result = new ShaderSizeInfo(bt * 2, 2);
					break;
				case ShaderDataType.Float3:
					result = new ShaderSizeInfo(bt * 3, 3);
					break;
				case ShaderDataType.Float4:
					result = new ShaderSizeInfo(bt * 4, 4);
					break;
				case ShaderDataType.Mat4:
					result = new ShaderSizeInfo(bt * 16, 16);
					break;

				default:
					result = new ShaderSizeInfo(0, 0);
					break;
			}

			return result;
		}

		public string GetDataTypeString(ShaderDataType type)
		{
			switch(type)
			{
				case ShaderDataType.Float:
					return "Float";
				case ShaderDataType.Float2:
					return "Float2";
				case ShaderDataType.Float3:
					return "Float3";
				case ShaderDataType.Float4:
					return "Float4";
				case ShaderDataType.Mat4:
					return "Mat4";

				case ShaderDataType.InvalidType:
					return "Invalid";

				default:
					return "(No name for this data type)";
			}
		}



		public static int getAttributeSizeBytes(ShaderAttributeName name)
		{
			switch (name)
			{
				case ShaderAttributeName.Position: return MeshData.getPositionSizeBytes();
				case ShaderAttributeName.Normal: return MeshData.getNormalSizeBytes();
				case ShaderAttributeName.TexCoord: return MeshData.getTexCoordSizeBytes();
				default: return 0;
			}
		}

		public static int getAttributeSizeElements(ShaderAttributeName name)
		{
			switch (name)
			{
				case ShaderAttributeName.Position: return MeshData.getElementsInPosition();
				case ShaderAttributeName.Normal: return MeshData.getElementsInNormal();
				case ShaderAttributeName.TexCoord: return MeshData.getElementsInTexCoord();
				default: return 0;
			}
		}

	}
}