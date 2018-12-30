using System.IO;
using System.Collections.Generic;
using OpenTK;

namespace MuffinSpace
{
	public class SceneManager
	{
		private List<Scene> allScenes;

		public SceneManager(string sceneDir)
		{
			allScenes = new List<Scene>();
			// Load all meshes and their materials

			string topDir = Directory.GetCurrentDirectory();

			string[] directories = Directory.GetDirectories(sceneDir);
			foreach (string dir in directories)
			{
				Directory.SetCurrentDirectory(dir);
				string[] files = Directory.GetFiles(dir);
				foreach (string fileEntry in files)
				{
					if (fileEntry.EndsWith(".sce"))
					{
						string fileName = fileEntry.Substring(fileEntry.LastIndexOf('\\') + 1);
						Scene newScene = new Scene();
						if (loadSceneByFile(fileEntry, ref newScene))
						{
							allScenes.Add(newScene);
						}
					}
				}
			}

			Directory.SetCurrentDirectory(topDir);

		}

		public Scene GetScene(string sceneFileName)
		{
			foreach (Scene s in allScenes)
			{
				if (s.name == sceneFileName)
				{
					return s;
				}
			}

			Logger.LogError(Logger.ErrorState.Critical, "No Scene with sourceFileName " + sceneFileName + " exists");
			return null;
		}

		private bool loadSceneByFile(string sceneFile, ref Scene scene)
		{
			StreamReader sourceFile = new StreamReader(sceneFile);
			AssetManager assetManager = AssetManager.GetSingleton();

			bool nameFound = false;
			bool positionFound = false;
			string modelName = null;
			Vector3 position = new Vector3(0, 0, 0);
			float scale = 1.0f;
			bool scaleFound = false;

			bool vsFound = false;
			bool fsFound = false;
			ShaderProgram modelShader = null;

			List<PosAndDir> cameraFrames = new List<PosAndDir>();

			string vsName = null;
			string fsName = null;

			char[] space = { ' ' };
			string line;
			do
			{
				line = sourceFile.ReadLine();
				if (line == null)
				{
					break;
				}

				if (line.Contains("#"))
				{
					// comment
				}

				if (line.Contains("scene"))
				{
					string[] sceneLines = line.Split(space);
					string sceneName = sceneLines[1];
					Logger.LogInfo("Loading scene " + sceneName + " from file.");
				}

				if (line.Contains("model"))
				{
					string[] modelLines = line.Split(space);
					modelName = modelLines[1];
					nameFound = true;
				}

				if (line.Contains("position"))
				{
					position = OBJFileReader.readVector3(line);
					positionFound = true;
				}

				if (line.Contains("scale"))
				{
					scale = OBJFileReader.readFloat(line);
					scaleFound = true;
				}

				if (line.Contains("shadervs"))
				{
					string[] tokens = line.Split(space);
					vsFound = true;
					vsName = tokens[1];
				}

				if (line.Contains("shaderfs"))
				{
					string[] tokens = line.Split(space);
					fsFound = true;
					fsName = tokens[1];
				}

				if (nameFound && positionFound && scaleFound)
				{
					Logger.LogInfo("Found model settings: " + modelName + ", P: " + position.ToString() + " S: " + scale);

					int fileTypeStart = modelName.LastIndexOf('.');
					int nameLength = modelName.Length - (modelName.Length - fileTypeStart);
					string modelNameNoOBJ = modelName.Substring(0, nameLength);

					scene.AddDrawable(assetManager.GetMesh(modelNameNoOBJ, modelName, assetManager.GetMaterial(modelNameNoOBJ).materialName, modelShader, position, scale));

					Logger.LogInfo("Model: " + modelName + " added to scene ");

					nameFound = false;
					positionFound = false;
					scaleFound = false;
				}

				if (vsFound && fsFound)
				{
					Logger.LogInfo("Found shader setting: VS: " + vsName + ", FS: " + fsName);

					ShaderProgram newProgram = assetManager.GetShaderProgram(vsName, fsName);

					Logger.LogInfo("Shader: " + vsName + " added to scene ");

					vsFound = false;
					fsFound = false;
				}


			} while (line != null);

			sourceFile.Close();

			return true;
		}

	}

}