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
				materialManager = new MaterialManager();
			
				// Find mesh/model directory

				string modelDirName = "models";
				string modelDir = FindDirectory(modelDirName);
				if (modelDir != null)
				{
					meshManager = new MeshManager(materialManager, modelDir);
				}
				else
				{
					Logger.LogError(Logger.ErrorState.Critical, "Model directory not found");
				}

				// Find shader directory
				string shaderDirName = "shaders";
				string shaderDir = FindDirectory(shaderDirName);
				if (shaderDir != null)
				{
					shaderManager = new ShaderManager(shaderDir);
				}
				else
				{
					Logger.LogError(Logger.ErrorState.Critical, "Shader directory not found");
				}
			}
			else
			{
				Logger.LogError(Logger.ErrorState.Critical, "Data directory not found");
			}

			

		}

		public void printLoadedAssets()
		{
			materialManager.printLoadedAssets();
			meshManager.printLoadedAssets();
		}

		public MaterialManager.Material GetMaterial(string materialName)
		{
			return materialManager.GetMaterialByName(materialName);
		}

		public MeshData getMeshData(string fileName)
		{
			return meshManager.GetMeshData(fileName);
		}

		public Shader GetShader(string shaderName)
		{
			return shaderManager.GetShader(shaderName);
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