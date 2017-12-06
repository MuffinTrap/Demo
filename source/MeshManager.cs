using System.IO;
using System.Collections.Generic;

namespace OpenTkConsole
{
	public class MeshManager
	{
		private List<Mesh> allMeshes;

		public MeshManager(MaterialManager materialManager, string modelDir)
		{
			allMeshes = new List<Mesh>();
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
						Mesh newMesh = Mesh.CreateFromFile(fileEntry, materialManager);
						newMesh.MeshName = fileEntry.Substring(fileEntry.LastIndexOf('\\') + 1);
						allMeshes.Add(newMesh);
					}
				}
			}

			Directory.SetCurrentDirectory(topDir);

		}

		public Mesh GetMesh(string meshName)
		{
			foreach (Mesh s in allMeshes)
			{
				if (s.MeshName == meshName)
				{
					return s;
				}
			}
			return null;
		}
	}
	
}