using System.IO;
using System.Collections.Generic;

namespace MuffinSpace
{
	public class ShaderManager
	{
		public static List<ShaderAttribute> getAttributes(List<ShaderAttributeName> names, ShaderProgram program)
		{
			List<ShaderAttribute> list = new List<ShaderAttribute>(names.Count);
			foreach (ShaderAttributeName n in names)
			{
				list.Add(getAttribute(n, program));
			}

			return list;
		}

		public static ShaderAttribute getAttribute(ShaderAttributeName name, ShaderProgram shaderProgram)
		{
			foreach (ShaderAttribute attr in shaderProgram.attributes)
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
		private List<ShaderProgram> allPrograms;

		public ShaderManager(string shaderDir)
		{
			allShaders = new List<Shader>();
			allPrograms = new List<ShaderProgram>();

			Dictionary<string, string> shaderSources = new Dictionary<string, string>();
			// Load all shaders

			string[] files = Directory.GetFiles(shaderDir);

			string sourceType = ".ss";

			foreach (string shaderFile in files)
			{
				if (shaderFile.Contains(sourceType))
				{
					string fileName = GetFilenameFromPath(shaderFile);
					shaderSources.Add(fileName, GetSourceFromFile(shaderFile));
					Logger.LogInfo("Saved shader source " + fileName);
				}
			}

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
				else
				{
					continue;
				}

				
				string shaderName = GetFilenameFromPath(shaderFile);
				string shaderSource = GetSourceFromFile(shaderFile);

				if (sType == OpenTK.Graphics.OpenGL.ShaderType.FragmentShader)
				{
					Logger.LogInfoLinePart("Fragment shader ", System.ConsoleColor.Gray);
				}
				else if (sType == OpenTK.Graphics.OpenGL.ShaderType.VertexShader)
				{
					Logger.LogInfoLinePart("Vertex shader ", System.ConsoleColor.Gray);
				}
				Logger.LogInfoLinePart(shaderName, System.ConsoleColor.Cyan);
				Logger.LogInfoLineEnd();

				AddIncludesAndDefines(ref shaderSource, ref shaderSources);

				Shader newShader = new Shader(shaderName, sType, shaderSource);
				allShaders.Add(newShader);
			}
		}

		private string GetFilenameFromPath(string path)
		{
			char pathSeparator = '\\';
			#if (MUFFIN_PLATFORM_LINUX)
				pathSeparator = '/';
			#elif (MUFFIN_PLATFORM_WINDOWS)
				// Nop, \ is the separator
			#endif
				
			string fileName = path.Substring(path.LastIndexOf(pathSeparator) + 1);
			return fileName;
		}

		static public string GetSourceFromFile(string filename)
		{
			try
			{
				StreamReader sourceFile = new StreamReader(filename);

				string sourceCode = sourceFile.ReadToEnd();

				sourceFile.Close();

				return sourceCode;
			}
			catch (System.Exception e)
			{

				Logger.LogError(Logger.ErrorState.Limited, "Shader CreateFromFile exception when opening file " + filename + " Error: " + e.Message);
				return null;
			}
		}

		private void AddIncludesAndDefines(ref string shaderSource, ref Dictionary<string, string> shaderSources)
		{
			string includeString = "#include";
			if (shaderSource.Contains(includeString))
			{
				int includeStart = shaderSource.IndexOf(includeString);
				int includeEnd = shaderSource.IndexOf(';', includeStart); // Removes trailing ;
				if (includeEnd <= includeStart)
				{
					Logger.LogError(Logger.ErrorState.Critical, "Failed parsing include from shader source");
					return;
				}
				string includeRow = shaderSource.Substring(includeStart, includeEnd - includeStart);
				string[] parts = includeRow.Split(' ');
				string includeFilename = parts[1];
				if (shaderSources.ContainsKey(includeFilename))
				{
					Logger.LogInfo("Found #include at " + includeStart + " : '" + includeRow + "' and found matching source file");
					shaderSource = shaderSource.Remove(includeStart, includeEnd - includeStart);
					shaderSource = shaderSource.Insert(includeStart, shaderSources[includeFilename]);
				}
			}
		}

		private Shader GetShader(string shaderName)
		{
			foreach (Shader s in allShaders)
			{
				if (s.ShaderName == shaderName)
				{
					return s;
				}
			}
			Logger.LogError(Logger.ErrorState.Critical, "Shader " + shaderName + " does not exist.");
			foreach (Shader s in allShaders)
			{
				Logger.LogInfo(s.ShaderName);
			}
			return null;
		}
		
		public ShaderProgram GetShaderProgram(string shaderName)
		{
			foreach (ShaderProgram s in allPrograms)
			{
				if (s.name == shaderName)
				{
					return s;
				}
			}

			string vertexName = shaderName + ".vs";
			string fragmentName = shaderName + ".fs";
			ShaderProgram nProg = new ShaderProgram(shaderName, GetShader(vertexName), GetShader(fragmentName));
			allPrograms.Add(nProg);
			return nProg;
		}
	}
	
}