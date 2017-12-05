using System.IO;

namespace OpenTkConsole
{
	public class AssetManager
	{

		private MaterialManager materialManager;
		private MeshManager meshManager;
		private ShaderManager shaderManager;

		public AssetManager(string dataDirectory)
		{
			// Find data directory and go to it
			bool dataDirFound = false;
			{
				string dir = FindDirectory(dataDirectory);
				if (dir != null)
				{ 
					dataDirFound = true;
					Directory.SetCurrentDirectory(dir);
				}
			}

			if (dataDirFound)
			{
				// Find material directory.
				string materialDirName = "materials";
				string materialDir = FindDirectory(materialDirName);
				if (materialDir != null)
				{ 
					materialManager = new MaterialManager(materialDir);
				}

				// Find mesh/model directory

				string modelDirName = "models";
				string modelDir = FindDirectory(modelDirName);
				if (modelDir != null)
				{
					meshManager = new MeshManager(materialManager, modelDir);
				}

				// Find shader directory
				string shaderDirName = "shaders";
				string shaderDir = FindDirectory(shaderDirName);
				if (shaderDir != null)
				{
					shaderManager = new ShaderManager(shaderDir);
				}
			}
			
		}

		public Shader GetShader(string shaderName)
		{
			return shaderManager.GetShader(shaderName);
		}

		public Mesh GetMesh(string meshName)
		{
			return meshManager.GetMesh(meshName);
		}

		private string FindDirectory(string directoryName)
		{
			string[] allDirs = Directory.GetDirectories(Directory.GetCurrentDirectory());
			foreach (string dir in allDirs)
			{
				if (dir.Contains(directoryName))
				{
					return dir;
				}
			}

			return null;
		}
	}
}