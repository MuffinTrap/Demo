
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace OpenTkConsole
{

	static class MaterialManager
	{
		// Material
		public class Material
		{
			public string materialName;
			public string textureName;
			public int textureGLIndex;
			public float illumination;
			public Vector3 alpha;
			public Vector3 diffuse;
			public Vector3 specular;
		}

		public static List<Material> materials;
		private static string dataDir;

		public static void init(string dataDirectory)
		{
			materials = new List<Material>();
			dataDir = dataDirectory;
			loadMaterial("white.mtl");
		}

		public static Material getMaterialByName(string materialName)
		{
			foreach (Material m in materials)
			{
				if (m.materialName == materialName)
				{
					return m;
				}
			}

			return null;
		}

		public static Material loadMaterial(string materialFileName)
		{
			string fullPath = dataDir + materialFileName;
			StreamReader sourceFile = new StreamReader(fullPath);

			string line;
			string materialName = null;
			bool existingFound = false;
			Material newMaterial = null;

			char[] space = { ' ' };

			// First check if we already have this material
			do
			{
				line = sourceFile.ReadLine();
				if (line == null)
				{
					break;
				}
				else if (line.Contains("newmtl"))
				{
					materialName = line.Split(space)[1];
					break;
				}
			}
			while (line != null);


			foreach (Material m in materials)
			{
				if (m.materialName == materialName)
				{
					existingFound = true;
					newMaterial = m;
				}
			}

			if (existingFound)
			{
				sourceFile.Close();
				return newMaterial;
			}
			else
			{
				newMaterial = new Material();
				newMaterial.materialName = materialName;
			}

			// Read rest of the file

			// use . as separator instead of system default
			System.Globalization.NumberFormatInfo nfi = new System.Globalization.CultureInfo("en-US", false).NumberFormat;

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
				else if (line.Contains("map_Kd"))
				{
					// map_Kd filename.png
					newMaterial.textureName = line.Split(space)[1];
					newMaterial.textureGLIndex = loadTexture(newMaterial.textureName);
				}
				else if (line.Contains("illum"))
				{
					newMaterial.illumination = Single.Parse(line.Split(space)[1], nfi);
				}
				else if (line.Contains("Ka"))
				{
					newMaterial.alpha = OBJFileReader.readVector3(line);
				}
				else if (line.Contains("Kd"))
				{
					newMaterial.diffuse = OBJFileReader.readVector3(line);
				}
				else if (line.Contains("Ks"))
				{
					newMaterial.specular = OBJFileReader.readVector3(line);
				}
				

			} while (line != null);


			sourceFile.Close();

			materials.Add(newMaterial);

			return newMaterial;
		}

		static int loadTexture(string textureFileName)
		{
			string fullPath = dataDir + textureFileName;

			// Create bitmap manually.
			bool fromFile = false;
			Bitmap map = null;

			if (fromFile)
			{
				try
				{
					map = new Bitmap(fullPath);
				}
				catch (FileNotFoundException e)
				{
					Console.WriteLine("Load texture did not find file:" + e.Message);
					return -1;
				}
				catch (ArgumentException e)
				{
					Console.WriteLine("Load texture did not find file:" + e.Message);
					return -1;
				}

			}
			else
			{
				map = new Bitmap(4, 4);
				for (int x = 0; x < 4; x++)
				{
					for (int y = 0; y < 4; y++)
					{
						map.SetPixel(x, y, Color.White);
					}
				}
			}
			int textureId = loadTextureFromBitmap(map);
			map.Dispose();
			return textureId;

		}

		static int loadTextureFromBitmap(Bitmap map)
		{
			int texID = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, texID);
			BitmapData data = map.LockBits(new System.Drawing.Rectangle(0, 0, map.Width, map.Height),
		ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
				OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

			map.UnlockBits(data);

			Error.checkGLError("MaterialManager.loadTextureFromBitmap");

			return texID;
		}

	}
}