using System.Collections.Generic;
using System.IO;
using OpenTK;

namespace OpenTkConsole
{
	public class AssetManager
	{
		private MaterialManager materialManager;
		private MeshManager meshManager;
		private ShaderManager shaderManager;
		private SceneManager sceneManager;

		private static AssetManager singleton;
		private static string dataDirectory;

		public static string WorkingDir{ get; set; }

		public static AssetManager GetAssetManagerSingleton()
		{
			if (singleton == null)
			{
				singleton = new AssetManager(WorkingDir);
			}
			return singleton;
		}

		private AssetManager(string dataDir)
		{
			dataDirectory = dataDir;
		}

		public void LoadAll()
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

				// 
				string textureDirName = "textures";
				string textureDir = FindDirectory(textureDirName);
				if (textureDir != null)
				{
					materialManager.loadAllFromDir(textureDir);
				}

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

				// Scene manager depends on other managers
				// Find scenes directory
				string scenesDirName = "scenes";
				string sceneDir = FindDirectory(scenesDirName);
				if (sceneDir != null)
				{
					sceneManager = new SceneManager(sceneDir);
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

		public SceneFromFile GetScene(string sceneFileName)
		{
			return sceneManager.GetScene(sceneFileName);
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