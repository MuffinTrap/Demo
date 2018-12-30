using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace MuffinSpace
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
		NormalMap,
		IlluminationMap, // Self-illumination
		RoughnessMap,

		// Lighting system
		LightsArray,

		// For each light
		LightPosition,
		LightDirection,
		LightColor,
		AmbientStrength,
		LinearAttenuation,
		QuadraticAttenuation,

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
		Light,

		InvalidType
	}

	// This is an interface for classes that have data for shaders
	public interface IShaderDataOwner
	{
		bool SetUniform(ShaderProgram shaderProgram, int location, ShaderUniformName dataName);
	}

	public class ShaderUniformManager
	{
		public Dictionary<string, ShaderUniform> supportedUniforms;
		public Dictionary<string, ShaderAttribute> supportedAttributes;

		private static ShaderUniformManager singleton = null;
		public static ShaderUniformManager GetSingleton()
		{
			if (singleton == null)
			{
				singleton = new ShaderUniformManager();
			}
			return singleton;
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

				case ShaderUniformName.LightsArray: return "uLights";
				case ShaderUniformName.LightPosition: return "position";
				case ShaderUniformName.LightDirection: return "direction";
				case ShaderUniformName.LightColor: return "color";
				case ShaderUniformName.LinearAttenuation: return "linearAttenuation";
				case ShaderUniformName.QuadraticAttenuation: return "quadraticAttenuation";

				case ShaderUniformName.ParticleColor: return "uParticleColor";

				case ShaderUniformName.AmbientStrength: return "uAmbientStrength";
				case ShaderUniformName.DiffuseStrength: return "uDiffuseStrength";
				case ShaderUniformName.SpecularStrength: return "uSpecularStrength";
				case ShaderUniformName.SpecularPower: return "uSpecularPower";

				case ShaderUniformName.DiffuseMap: return "uDiffuseMap";
				case ShaderUniformName.NormalMap: return "uNormalMap";
				case ShaderUniformName.IlluminationMap: return "uIlluminationMap";
				case ShaderUniformName.RoughnessMap: return "uRoughnessMap";

				default: return string.Empty;
			}
		}

		private ShaderUniformManager()
		{
			supportedUniforms = new Dictionary<string, ShaderUniform>();
			supportedAttributes = new Dictionary<string, ShaderAttribute>();

			// Uniforms that shaders can have
			AddSupportedUniform(ShaderUniformName.WorldMatrix, ShaderDataType.Mat4);
			AddSupportedUniform(ShaderUniformName.ProjectionMatrix, ShaderDataType.Mat4);
			AddSupportedUniform(ShaderUniformName.ViewMatrix, ShaderDataType.Mat4);

			AddSupportedUniform(ShaderUniformName.DiffuseMap, ShaderDataType.Texture2D);
			AddSupportedUniform(ShaderUniformName.NormalMap, ShaderDataType.Texture2D);
			AddSupportedUniform(ShaderUniformName.IlluminationMap, ShaderDataType.Texture2D);
			AddSupportedUniform(ShaderUniformName.RoughnessMap, ShaderDataType.Texture2D);

			AddSupportedUniform(ShaderUniformName.LightsArray, ShaderDataType.Light);
			AddSupportedUniform(ShaderUniformName.LightPosition, ShaderDataType.Float3);
			AddSupportedUniform(ShaderUniformName.LightDirection, ShaderDataType.Float3);
			AddSupportedUniform(ShaderUniformName.LightColor, ShaderDataType.Float3);
			AddSupportedUniform(ShaderUniformName.ParticleColor, ShaderDataType.Float3);

			AddSupportedUniform(ShaderUniformName.AmbientStrength, ShaderDataType.Float);
			AddSupportedUniform(ShaderUniformName.DiffuseStrength, ShaderDataType.Float);
			AddSupportedUniform(ShaderUniformName.SpecularStrength, ShaderDataType.Float);
			AddSupportedUniform(ShaderUniformName.SpecularPower, ShaderDataType.Float);

			AddSupportedUniform(ShaderUniformName.LinearAttenuation, ShaderDataType.Float);
			AddSupportedUniform(ShaderUniformName.QuadraticAttenuation, ShaderDataType.Float);

			// Attributes that shaders can have
			AddSupportedAttribute(ShaderAttributeName.Position, ShaderDataType.Float3);
			AddSupportedAttribute(ShaderAttributeName.Normal, ShaderDataType.Float3);
			AddSupportedAttribute(ShaderAttributeName.TexCoord, ShaderDataType.Float2);
			AddSupportedAttribute(ShaderAttributeName.DiffuseColor, ShaderDataType.Float3);

		}

		private void AddSupportedUniform(ShaderUniformName name, ShaderDataType type)
		{
			supportedUniforms.Add(GetUniformName(name), new ShaderUniform(name, type));
		}

		private void AddSupportedAttribute(ShaderAttributeName name, ShaderDataType type)
		{
			supportedAttributes.Add(GetAttributeName(name), new ShaderAttribute(name, type));
		}

		public bool DoesShaderSupportUniform(ShaderProgram program, ShaderUniformName uniform)
		{
			foreach (ShaderUniform uni in program.uniforms)
			{
				if (uni.name == uniform)
				{
					return true;
				}
			}
			return false;
		}

		public bool DoesShaderUseCamera(ShaderProgram program)
		{
			return DoesShaderSupportUniform(program, ShaderUniformName.ViewMatrix)
				|| DoesShaderSupportUniform(program, ShaderUniformName.ProjectionMatrix);
		}

		public bool DoesShaderUseLights(ShaderProgram program)
		{
			return DoesShaderSupportUniform(program, ShaderUniformName.LightsArray);
		}

		public void SetArrayData(ShaderProgram shader
		, ShaderUniformName arrayName
		, ShaderUniformName dataName
		, IShaderDataOwner provider
		, int index)
		{
			foreach (ShaderUniform uni in shader.uniforms)
			{
				if (uni.name == dataName && uni.arrayName == arrayName && uni.arrayIndex == index)
				{
					bool set = provider.SetUniform(shader, uni.location, uni.name);
					if (!set)
					{
						Logger.LogError(Logger.ErrorState.Critical, "ShaderUniformManager.SetArrayData. Provider does not have "
							+ GetUniformName(arrayName) + "." + GetUniformName(dataName));
					}
					return;
				}
			}
			Logger.LogError(Logger.ErrorState.Critical, "ShaderUniformManager.SetArrayData. Shader " + shader.name + " does not use data: "
			+ GetUniformName(arrayName) + "[" + index + "]." + GetUniformName(dataName));
			foreach (ShaderUniform uni in shader.uniforms)
			{
				if (uni.arrayIndex != -1)
				{
					Logger.LogInfo(shader.name + " uses "
					+ GetUniformName(uni.arrayName) + "[" + uni.arrayIndex + "]." + GetUniformName(uni.name));
				}
			}
		}

		public bool TrySetData(ShaderProgram shader
			, ShaderUniformName dataName
			, IShaderDataOwner provider)
		{
			bool set = false;
			foreach (ShaderUniform uni in shader.uniforms)
			{
				if (uni.name == dataName)
				{
					set = provider.SetUniform(shader, uni.location, uni.name);
					if (!set)
					{
						Logger.LogError(Logger.ErrorState.Critical, "ShaderUniformManager.SetData. Provider does not have "
						+ GetUniformName(dataName));
					}
					break;
				}
			}
			return set;
		}

		public void SetData(ShaderProgram shader
			, ShaderUniformName dataName
			, IShaderDataOwner provider)
		{
			bool set = TrySetData(shader, dataName, provider);
			if (!set)
			{
				Logger.LogError(Logger.ErrorState.Unoptimal, "ShaderUniformManager.SetData. Shader " + shader.name + " does not use "
				+ GetUniformName(dataName));
			}
		}
		
		public int GetDataLocation(ShaderProgram shader
			, ShaderUniformName dataName)
		{
			foreach (ShaderUniform uni in shader.uniforms)
			{
				if (uni.name == dataName)
				{
					return uni.location;
				}
			}
			return GetInvalidDataLocation();
		}
		
		public static int GetInvalidDataLocation()
		{
			return -1;
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
					/*
					Logger.LogInfoLinePart("Created supported uniform :", System.ConsoleColor.Gray);
					Logger.LogInfoLinePart(" (", System.ConsoleColor.Gray);
					Logger.LogInfoLinePart("" + location, System.ConsoleColor.Red);
					Logger.LogInfoLinePart(")", System.ConsoleColor.Gray);
					Logger.LogInfoLinePart(" " + GetDataTypeString(uniformType), System.ConsoleColor.Cyan);
					Logger.LogInfoLinePart(" " + nameString, System.ConsoleColor.Gray);
					Logger.LogInfoLineEnd();
					*/
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
			else if (nameString.Contains("."))
			{
				// Array attribute   Array[i].variable
				string[] parts = nameString.Split('.');
				string[] arrayAndIndex = parts[0].Split('[');
				string arrayUniform = arrayAndIndex[0];
				string index = arrayAndIndex[1].Split(']')[0];
				string variableUniform = parts[1];
				if (supportedUniforms.ContainsKey(arrayUniform))
				{
					ShaderUniform supportedArray = supportedUniforms[arrayUniform];
					ShaderUniform supported = supportedUniforms[variableUniform];
					ShaderDataType uniformType = UniformTypeToDataType(type);
					if (supported.dataType == uniformType)
					{
						int arrayIndex = int.Parse(index);

						/*
						Logger.LogInfoLinePart("Created supported array uniform :", System.ConsoleColor.Gray);
						Logger.LogInfoLinePart(" (", System.ConsoleColor.Gray);
						Logger.LogInfoLinePart("" + location, System.ConsoleColor.Red);
						Logger.LogInfoLinePart(")", System.ConsoleColor.Gray);
						Logger.LogInfoLinePart(" " + GetDataTypeString(uniformType), System.ConsoleColor.Cyan);
						Logger.LogInfoLinePart(" " + arrayUniform + "[", System.ConsoleColor.Gray);
						Logger.LogInfoLinePart(" " + arrayIndex , System.ConsoleColor.Red);
						Logger.LogInfoLinePart("].", System.ConsoleColor.Gray);
						Logger.LogInfoLinePart(variableUniform, System.ConsoleColor.Gray);
						Logger.LogInfoLineEnd();
						*/

						returnValue = new ShaderUniform(supported.name, supported.dataType, location, supportedArray.name, arrayIndex);
					}
					else
					{
						Logger.LogError(Logger.ErrorState.Critical, "Shader data type mismatch with supported uniform type: "
						+ " Expected: " + GetDataTypeString(supported.dataType)
						+ " Got: " + GetDataTypeString(uniformType));
						returnValue = new ShaderUniform(ShaderUniformName.InvalidUniformName, ShaderDataType.InvalidType);
					}
				}
				else
				{
					Logger.LogError(Logger.ErrorState.Critical, "Shader uniform name contains '.' but is not supported array: "
					+ nameString);
					returnValue = new ShaderUniform(ShaderUniformName.InvalidUniformName, ShaderDataType.InvalidType);
				}
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
					/*
					Logger.LogInfoLinePart("Created attribute", System.ConsoleColor.Gray);
					Logger.LogInfoLinePart(" (", System.ConsoleColor.Gray);
					Logger.LogInfoLinePart("" + location, System.ConsoleColor.Red);
					Logger.LogInfoLinePart(") ", System.ConsoleColor.Gray);
					Logger.LogInfoLinePart(GetDataTypeString(attribType), System.ConsoleColor.Cyan);
					Logger.LogInfoLinePart(" " + nameString, System.ConsoleColor.Gray);
					Logger.LogInfoLineEnd();
					*/
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