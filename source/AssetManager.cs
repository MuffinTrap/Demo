using System.Collections.Generic;
using System.IO;
using OpenTK;

namespace MuffinSpace
{
	public class AssetManager
	{
		private MaterialManager materialManager;
		private MeshManager meshManager;
		private ShaderManager shaderManager;

		private static AssetManager singleton;
		private static string dataDirectory;

		public static string WorkingDir { get; set; }

		public static AssetManager GetSingleton()
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
				materialManager = MaterialManager.GetSingleton();

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

		public Material GetMaterial(string materialName)
		{
			return materialManager.GetMaterialByName(materialName);
		}

		public MeshData GetMeshData(string fileName)
		{
			return meshManager.GetMeshData(fileName);
		}

		public ShaderProgram GetShaderProgram(string shaderName)
		{
			return shaderManager.GetShaderProgram(shaderName);
		}

		public DrawableMesh GetMesh(string name
			, string modelFile
			, string material
			, ShaderProgram shader
			, Vector3 position)
		{
			MeshData data = GetMeshData(modelFile);
			return CreateMesh(name, data, material, shader, position);
		}

		public DrawableMesh CreateMesh(string name
		, MeshData data
		, string material
		, ShaderProgram shader
		, Vector3 position)
		{
			List<ShaderAttributeName> attr = data.GetNeededAttributes();
			TransformComponent t = new TransformComponent(position);
			Material m =  GetMaterial(material);

			return new DrawableMesh(name, data, ShaderManager.getAttributes(attr, shader), t, m, shader);
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