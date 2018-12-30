using System.IO;
using System.Collections.Generic;

namespace MuffinSpace
{
	public class MeshManager
	{
		private List<MeshData> allMeshes;

		public MeshManager(MaterialManager materialManager, string modelDir)
		{
			allMeshes = new List<MeshData>();
			// Load all meshes and their materials

			string topDir = Directory.GetCurrentDirectory();

			string[] directories = Directory.GetDirectories(modelDir);
			foreach(string dir in directories)
			{
				Directory.SetCurrentDirectory(dir);
				string[] files = Directory.GetFiles(dir);
				foreach(string fileEntry in files)
				{
					if (fileEntry.EndsWith(".obj"))
					{
						MeshData newMesh = MeshDataGenerator.CreateFromFile(fileEntry, materialManager);
						newMesh.sourceFileName = fileEntry.Substring(fileEntry.LastIndexOf('\\') + 1);
						allMeshes.Add(newMesh);
					}
				}
			}

			Directory.SetCurrentDirectory(topDir);
		}

		public MeshData GetMeshData(string sourceFileName)
		{
			foreach (MeshData s in allMeshes)
			{
				if (s.sourceFileName == sourceFileName)
				{
					return s;
				}
			}

			Logger.LogError(Logger.ErrorState.Critical, "No mesh with sourceFileName " + sourceFileName + " exists");
			return null;
		}

		public void printLoadedAssets()
		{
			foreach (MeshData m in allMeshes)
			{
				Logger.LogInfoLinePart("Loaded mesh ", System.ConsoleColor.Gray);
				Logger.LogInfoLinePart(m.sourceFileName, System.ConsoleColor.Cyan);
				Logger.LogInfoLinePart(" (" + m.VertexAmount + "v)", System.ConsoleColor.Gray);
				Logger.LogInfoLineEnd();
			}
		}
	}
	
}