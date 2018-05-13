using System.IO;
using System.Collections.Generic;
using OpenTK;

namespace OpenTkConsole
{
	public class SceneManager
	{
		private List<SceneFromFile> allScenes;

		public SceneManager(string sceneDir)
		{
			allScenes = new List<SceneFromFile>();
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
						SceneFromFile newScene = new SceneFromFile(fileEntry.Substring(fileEntry.LastIndexOf('\\') + 1));
						if (loadSceneByFile(fileEntry, ref newScene))
						{
							allScenes.Add(newScene);
						}
					}
				}
			}

			Directory.SetCurrentDirectory(topDir);

		}

		public SceneFromFile GetScene(string sceneFileName)
		{
			foreach (SceneFromFile s in allScenes)
			{
				if (s.ConfigFile == sceneFileName)
				{
					return s;
				}
			}

			Logger.LogError(Logger.ErrorState.Critical, "No Scene with sourceFileName " + sceneFileName + " exists");
			return null;
		}

		private bool loadSceneByFile(string sceneFile, ref SceneFromFile scene)
		{
			StreamReader sourceFile = new StreamReader(sceneFile);
			AssetManager assetManager = AssetManager.GetAssetManagerSingleton();
			scene.allModels = new List<DrawableMesh>();

			bool nameFound = false;
			bool positionFound = false;
			string modelName = null;
			Vector3 position = new Vector3(0, 0, 0);
			float scale = 1.0f;
			bool scaleFound = false;

			bool vsFound = false;
			bool fsFound = false;

			bool framesFound = true;
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

				if (line.Contains("frames"))
				{
					// Start reading camera frames
					framesFound = true;
				}

				if (line.StartsWith("f "))
				{
					// f 1,1,1 1,1,1
					string[] tokens = line.Split(space);
					if (tokens.Length == 3)
					{
						PosAndDir p = new PosAndDir();
						p.position = OBJFileReader.readVector3(tokens[1]);
						p.direction = OBJFileReader.readVector3(tokens[2]);
						cameraFrames.Add(p);
					}
				}


				if (nameFound && positionFound && scaleFound)
				{
					Logger.LogInfo("Found model settings: " + modelName + ", P: " + position.ToString() + " S: " + scale);

					int fileTypeStart = modelName.LastIndexOf('.');
					int nameLength = modelName.Length - (modelName.Length - fileTypeStart);
					string modelNameNoOBJ = modelName.Substring(0, nameLength);

					scene.allModels.Add(assetManager.GetMesh(modelNameNoOBJ, modelName, assetManager.GetMaterial(modelNameNoOBJ).materialName, scene.MainShader, position, scale));

					Logger.LogInfo("Model: " + modelName + " added to scene ");

					nameFound = false;
					positionFound = false;
					scaleFound = false;
				}

				if (vsFound && fsFound)
				{
					Logger.LogInfo("Found shader setting: VS: " + vsName + ", FS: " + fsName);

					ShaderProgram newProgram = new ShaderProgram(assetManager.GetShader(vsName), assetManager.GetShader(fsName));

					scene.MainShader = newProgram;

					Logger.LogInfo("Shader: " + vsName + " added to scene ");

					vsFound = false;
					fsFound = false;
				}


			} while (line != null);

			if (framesFound)
			{
				scene.setCameraFrames(cameraFrames);
			}

			sourceFile.Close();

			return true;
		}

	}

}