using System.IO;
using System.Collections.Generic;

namespace MuffinSpace
{

	
	public class ShaderManager
	{
		public static List<ShaderAttribute> getAttributes(List<ShaderAttributeName> names, ShaderProgram program)
		{
			List<ShaderAttribute> list = new List<ShaderAttribute>(names.Count);
			foreach(ShaderAttributeName n in names)
			{
				list.Add(getAttribute(n, program));
			}

			return list;
		}
		
		public static ShaderAttribute getAttribute(ShaderAttributeName name, ShaderProgram shaderProgram)
		{
			foreach( ShaderAttribute attr in shaderProgram.attributes)
			{
				if (attr.name == name)
				{
					return attr;
				}
			}
			ShaderAttribute invalid = new ShaderAttribute(ShaderAttributeName.InvalidAttributeName, ShaderDataType.InvalidType);
			return invalid;
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

				if (sType == OpenTK.Graphics.OpenGL.ShaderType.FragmentShader)
				{
					Logger.LogInfoLinePart("Fragment shader ", System.ConsoleColor.Gray);
				}
				else if (sType == OpenTK.Graphics.OpenGL.ShaderType.VertexShader)
				{
					Logger.LogInfoLinePart("Vertex shader ", System.ConsoleColor.Gray);
				}
				Logger.LogInfoLinePart(fileName, System.ConsoleColor.Cyan);
				Logger.LogInfoLineEnd();
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
			Logger.LogError(Logger.ErrorState.Critical, "Shader " + shaderName + " does not exist.");
			return null;
		}
	}
	
}