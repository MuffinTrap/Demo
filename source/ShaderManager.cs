using System.IO;
using System.Collections.Generic;

namespace OpenTkConsole
{
	public struct ShaderAttribute
	{
		public ShaderAttribute(string nameP, int indexP, int sizeBytesP, int sizeElementsP)
		{
			name = nameP;
			index = indexP;
			sizeBytes = sizeBytesP;
			sizeElements = sizeElementsP;
		}

		public int index;
		public string name;
		public int sizeBytes;
		public int sizeElements;

		public static string getPositionAttributeName()
		{
			return "vPosition";
		}

		public static string getNormalAttributeName()
		{
			return "vNormal";
		}

		public static string getTexCoordAttributeName()
		{
			return "vTexCoord";
		}
	}

	
	public class ShaderManager
	{
		public static List<ShaderAttribute> getDefaultAttributes(ShaderProgram shaderProgram)
		{
			List<ShaderAttribute> attributes = new List<ShaderAttribute>();

			attributes.Add(new ShaderAttribute(ShaderAttribute.getPositionAttributeName()
				, shaderProgram.GetAttributeLocation(ShaderAttribute.getPositionAttributeName())
				, MeshData.getPositionSizeBytes()
				, MeshData.getElementsInPosition()));

			attributes.Add(new ShaderAttribute(ShaderAttribute.getTexCoordAttributeName()
				, shaderProgram.GetAttributeLocation(ShaderAttribute.getTexCoordAttributeName())
				, MeshData.getTexCoordSizeBytes()
				, MeshData.getElementsInTexCoord()));

			attributes.Add(new ShaderAttribute(ShaderAttribute.getNormalAttributeName()
			, shaderProgram.GetAttributeLocation(ShaderAttribute.getNormalAttributeName())
			, MeshData.getNormalSizeBytes()
			, MeshData.getElementsInNormal()));

			return attributes;

		}

		private List<Shader> allShaders;

		public ShaderManager(string shaderDir)
		{
			allShaders = new List<Shader>();
			// Load all shaders

			string[] files = Directory.GetFiles(shaderDir);

			string vertexType = ".vs";
			string fragmentType = ".fs";
			foreach (string shaderFile in files)
			{
				// Check shader type

				// Create shader
				OpenTK.Graphics.OpenGL.ShaderType sType = OpenTK.Graphics.OpenGL.ShaderType.VertexShader;
				if (shaderFile.Contains(vertexType))
				{
					sType = OpenTK.Graphics.OpenGL.ShaderType.VertexShader;
				}
				else if (shaderFile.Contains(fragmentType))
				{
					sType = OpenTK.Graphics.OpenGL.ShaderType.FragmentShader;
				}

				string fileName = shaderFile.Substring(shaderFile.LastIndexOf('\\') + 1);
				Shader newShader = Shader.CreateFromFile(sType, shaderFile);
				newShader.ShaderName = fileName;
				allShaders.Add(newShader);
			}

		}

		public Shader GetShader(string shaderName)
		{
			foreach (Shader s in allShaders)
			{
				if (s.ShaderName == shaderName)
				{
					return s;
				}
			}
			return null;
		}
	}
	
}