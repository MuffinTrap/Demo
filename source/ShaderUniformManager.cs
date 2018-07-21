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

		DiffuseColor,

		CustomAttribute, // For custom attributes starting with uCustom
		InvalidAttributeName
	}

	public enum ShaderUniformName
	{
		WorldMatrix,
		ProjectionMatrix,
		ViewMatrix,

		DiffuseColor,
		ParticleColor,
		DiffuseMap,

		// Lighting system
		LightDirection,
		LightColor,
		AmbientStrength,

		// Material
		DiffuseStrength,
		SpecularStrength,
		SpecularPower,

		CustomUniform, // For custom uniforms starting with uCustom
		InvalidUniformName
	}

	public enum ShaderDataType
	{
		Float,
		Float2,
		Float3,
		Float4,

		Mat4,

		Texture2D,

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

			// Uniforms that shaders can have
			AddSupportedUniform(ShaderUniformName.WorldMatrix, ShaderDataType.Mat4);
			AddSupportedUniform(ShaderUniformName.ProjectionMatrix, ShaderDataType.Mat4);
			AddSupportedUniform(ShaderUniformName.ViewMatrix, ShaderDataType.Mat4);

			AddSupportedUniform(ShaderUniformName.DiffuseMap, ShaderDataType.Texture2D);

			AddSupportedUniform(ShaderUniformName.LightDirection, ShaderDataType.Float3);
			AddSupportedUniform(ShaderUniformName.LightColor, ShaderDataType.Float3);
			AddSupportedUniform(ShaderUniformName.ParticleColor, ShaderDataType.Float3);

			AddSupportedUniform(ShaderUniformName.AmbientStrength, ShaderDataType.Float);
			AddSupportedUniform(ShaderUniformName.DiffuseStrength, ShaderDataType.Float);
			AddSupportedUniform(ShaderUniformName.SpecularStrength, ShaderDataType.Float);
			AddSupportedUniform(ShaderUniformName.SpecularPower, ShaderDataType.Float);

			// Attributes that shaders can have
			AddSupportedAttribute(ShaderAttributeName.Position, ShaderDataType.Float3);
			AddSupportedAttribute(ShaderAttributeName.Normal, ShaderDataType.Float3);
			AddSupportedAttribute(ShaderAttributeName.TexCoord, ShaderDataType.Float2);
			AddSupportedAttribute(ShaderAttributeName.DiffuseColor, ShaderDataType.Float3);

			uniformDataOwners = new Dictionary<ShaderUniformName, IShaderDataOwner>();
		}

		private void AddSupportedUniform(ShaderUniformName name, ShaderDataType type)
		{
			supportedUniforms.Add(GetUniformName(name), new ShaderUniform(name, type));
		}

		private void AddSupportedAttribute(ShaderAttributeName name, ShaderDataType type)
		{
			supportedAttributes.Add(GetAttributeName(name), new ShaderAttribute(name, type));
		}

		public static ShaderUniformManager GetSingleton()
		{
			if (singleton == null)
			{
				singleton = new ShaderUniformManager();
			}
			return singleton;
		}

		public void SetData(ShaderProgram shader, ShaderUniformName dataName)
		{
			foreach (ShaderUniform uni in shader.uniforms)
			{
				if (uni.name == dataName)
				{
					IShaderDataOwner owner;
					if (uniformDataOwners.TryGetValue(uni.name, out owner))
					{ 
						owner.SetUniform(shader, uni.location, uni.name);
						return;
					}
					break;
				}
			}
			Logger.LogError(Logger.ErrorState.Critical, "ShaderUniformManager.SetData. No registered owner for "
			+ GetUniformName(dataName));
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
					continue;
				}
				else
				{
					Logger.LogError(Logger.ErrorState.Limited, "Shader " + shader.name + " activation was not complete. Uniform "
					+ GetUniformName(uni.name) + " not found.");
				}
			}
		}

		public void RegisterDataOwner(IShaderDataOwner owner, ShaderUniformName uniform)
		{
			// Remove old first if exists
			if (uniformDataOwners.ContainsKey(uniform))
			{
				if (uniformDataOwners[uniform] != owner)
				{
					uniformDataOwners.Remove(uniform);
				}
				else
				{
					// Already the owner
					return;
				}
			}
			// Register owner
			uniformDataOwners.Add(uniform, owner);
		}

		public ShaderUniform CreateShaderUniform(string nameString, ActiveUniformType type, int location)
		{
			ShaderUniform returnValue;
			if (supportedUniforms.ContainsKey(nameString))
			{
				ShaderUniform supported = supportedUniforms[nameString];
				ShaderDataType uniformType = UniformTypeToDataType(type);
				if (supported.dataType == uniformType) 
				{
					Logger.LogInfo("\tCreated supported uniform : " + nameString + " of type " + GetDataTypeString(uniformType)
					+ " at " + location);
					returnValue = new ShaderUniform(supported.name, supported.dataType, location);
				}
				else
				{
					Logger.LogError(Logger.ErrorState.Critical, "Shader data type mismatch with supported uniform type: "
					+ " Expected: " + GetDataTypeString(supported.dataType)
					+ " Got: " + GetDataTypeString(uniformType));
					returnValue = new ShaderUniform(ShaderUniformName.InvalidUniformName, ShaderDataType.InvalidType);
				}
			}
			else if (nameString.StartsWith("uCustom"))
			{
				// Custom attribute is ok
				returnValue = new ShaderUniform(ShaderUniformName.CustomUniform, UniformTypeToDataType(type), location);
			}
			else
			{
				Logger.LogError(Logger.ErrorState.Critical, "Unsupported shader uniform: " + nameString + ", of type:" + GetDataTypeString(UniformTypeToDataType(type)));
				returnValue = new ShaderUniform(ShaderUniformName.InvalidUniformName, ShaderDataType.InvalidType);
			}

			return returnValue;
		}

		public ShaderAttribute CreateShaderAttribute(string nameString, ActiveAttribType type, int location, int sizeElements)
		{
			ShaderAttribute returnValue;
			if (supportedAttributes.ContainsKey(nameString))
			{
				ShaderAttribute supported = supportedAttributes[nameString];
				ShaderDataType attribType = AttribTypeToDataType(type);
				if (supported.dataType == attribType)
				{
					Logger.LogInfo("\tCreated supported attribute : " + nameString + " of type " + GetDataTypeString(attribType)
					+ " at " + location);
					returnValue = new ShaderAttribute(supported.name, supported.dataType, location, sizeElements);
				}
				else
				{
					Logger.LogError(Logger.ErrorState.Critical, "Shader data type mismatch with supported uniform type: "
					+ " Expected: " + GetDataTypeString(supported.dataType)
					+ " Got: " + GetDataTypeString(attribType));
					returnValue = new ShaderAttribute(ShaderAttributeName.InvalidAttributeName, ShaderDataType.InvalidType);
				}
			}
			else if (nameString.StartsWith("uCustom"))
			{
				// Custom attribute is ok
				returnValue = new ShaderAttribute(ShaderAttributeName.CustomAttribute, AttribTypeToDataType(type), location, sizeElements);
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
				case ShaderAttributeName.DiffuseColor: return "aDiffuseColor";
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
				
				case ShaderUniformName.LightDirection: return "uLightDirection";
				case ShaderUniformName.LightColor: return "uLightColor";
				case ShaderUniformName.ParticleColor: return "uParticleColor";

				case ShaderUniformName.AmbientStrength: return "uAmbientStrength";
				case ShaderUniformName.DiffuseStrength: return "uDiffuseStrength";
				case ShaderUniformName.SpecularStrength: return "uSpecularStrength";
				case ShaderUniformName.SpecularPower: return "uSpecularPower";

				case ShaderUniformName.DiffuseMap: return "uDiffuseMap";

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
				case ActiveUniformType.Sampler2D:
					return ShaderDataType.Texture2D;

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
				case ShaderDataType.Texture2D:
					return "Texture2D";

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